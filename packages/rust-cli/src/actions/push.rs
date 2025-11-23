use std::collections::{HashMap, HashSet};
use chrono::{Datelike, Local, NaiveDate};
use inquire::Select;
use crate::{alvtime, external_models, models, projector};
use crate::actions::utils::generate_events_from_server_entries;
use crate::events::Event;
use crate::external_models::TaskDto;
use crate::store::EventStore;

pub fn handle_push(
    client: &alvtime::AlvtimeClient,
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
) -> String {
    // 1. Identify which dates already have LOCAL events
    let local_dates: HashSet<NaiveDate> = history
        .iter()
        .filter_map(|e| {
            let (date, is_generated) = match e {
                Event::TaskStarted {
                    start_time,
                    is_generated,
                    ..
                } => (start_time.date_naive(), *is_generated),
                Event::BreakStarted {
                    start_time,
                    is_generated,
                } => (start_time.date_naive(), *is_generated),
                Event::Stopped {
                    end_time,
                    is_generated,
                } => (end_time.date_naive(), *is_generated),
                Event::Undo { time } => (time.date_naive(), false),
                Event::Redo { time } => (time.date_naive(), false),
                Event::DayRevised { date, .. } => (*date, false),
            };
            if !is_generated {
                Some(date)
            } else {
                None
            }
        })
        .collect();

    if local_dates.is_empty() {
        return "No local changes to push.".to_string();
    }

    let current_year = Local::now().year();
    let start_date = NaiveDate::from_ymd_opt(current_year, 1, 1).unwrap();
    let end_date = Local::now().date_naive();

    println!("Fetching server state for checks ({} to {})...", start_date, end_date);

    let entries = match client.list_time_entries(start_date, end_date) {
        Ok(e) => e,
        Err(e) => return format!("Failed to fetch entries: {}", e),
    };

    let mut entries_by_date: HashMap<NaiveDate, Vec<&external_models::TimeEntryDto>> =
        HashMap::new();
    for entry in &entries {
        if let Ok(date) = NaiveDate::parse_from_str(&entry.date, "%Y-%m-%d") {
            entries_by_date.entry(date).or_default().push(entry);
        }
    }

    let mut pushed_days = 0;
    let mut synced_days = 0;
    let mut entries_to_push: Vec<external_models::TimeEntryDto> = Vec::new();

    for date in &local_dates {
        let server_entries = entries_by_date.get(date);
        let has_server_entries = server_entries.map(|e| !e.is_empty()).unwrap_or(false);

        let day_tasks: Vec<&models::Task> = tasks
            .iter()
            .filter(|t| t.start_time.date_naive() == *date && !t.is_break)
            .collect();

        if day_tasks.is_empty() && !has_server_entries {
            continue;
        }

        let mut should_push = true;

        if has_server_entries {
            let server_hours: f64 = server_entries.unwrap().iter().map(|e| e.value).sum();
            let local_hours: f64 = day_tasks
                .iter()
                .map(|t| t.duration().num_minutes() as f64 / 60.0)
                .sum();

            let msg = format!(
                "Conflict on {}: Local {:.1}h vs Server {:.1}h. What do you want to do?",
                date, local_hours, server_hours
            );

            let options = vec![
                "Overwrite Server (Push Local)",
                "Discard Local (Sync from Server)",
                "Skip Day",
            ];

            match Select::new(&msg, options).prompt() {
                Ok(choice) => match choice {
                    "Overwrite Server (Push Local)" => {
                        should_push = true;
                    }
                    "Discard Local (Sync from Server)" => {
                        should_push = false;
                        let new_events = generate_events_from_server_entries(
                            *date,
                            server_entries.unwrap(),
                            external_tasks,
                        );
                        let revised_event = Event::DayRevised {
                            date: *date,
                            events: new_events,
                        };
                        event_store.persist(&revised_event);
                        history.push(revised_event);
                        synced_days += 1;
                    }
                    _ => {
                        should_push = false;
                    }
                },
                Err(_) => {
                    should_push = false;
                }
            }
        }

        if should_push {
            let mut sums: HashMap<i32, f64> = HashMap::new();
            for t in day_tasks {
                let hours = t.duration().num_minutes() as f64 / 60.0;
                *sums.entry(t.id).or_default() += hours;
            }

            let empty_vec = Vec::new();
            let server_entries_for_day = server_entries.unwrap_or(&empty_vec);

            for (task_id, hours) in sums {
                let existing_id = server_entries_for_day
                    .iter()
                    .find(|e| e.task_id == task_id)
                    .map(|e| e.id)
                    .unwrap_or(0);

                entries_to_push.push(external_models::TimeEntryDto {
                    id: existing_id,
                    task_id,
                    date: date.format("%Y-%m-%d").to_string(),
                    value: hours,
                    comment: None,
                });
            }
            pushed_days += 1;
        }
    }

    if !entries_to_push.is_empty() {
        println!(
            "Pushing {} entries for {} days...",
            entries_to_push.len(),
            pushed_days
        );
        if let Err(e) = client.upsert_time_entries(&entries_to_push) {
            return format!("Error pushing local entries: {}", e);
        }

        let mut pushed_dates: HashSet<NaiveDate> = HashSet::new();
        for entry in &entries_to_push {
            if let Ok(d) = NaiveDate::parse_from_str(&entry.date, "%Y-%m-%d") {
                pushed_dates.insert(d);
            }
        }

        for date in pushed_dates {
            match client.list_time_entries(date, date) {
                Ok(server_entries) => {
                    let refs: Vec<&external_models::TimeEntryDto> =
                        server_entries.iter().collect();
                    let new_events =
                        generate_events_from_server_entries(date, &refs, external_tasks);
                    let revised_event = Event::DayRevised {
                        date,
                        events: new_events,
                    };
                    event_store.persist(&revised_event);
                    history.push(revised_event);
                }
                Err(e) => eprintln!("Failed to refetch day {}: {}", date, e),
            }
        }
    }

    if pushed_days > 0 || synced_days > 0 {
        *tasks = projector::restore_state(history);
        format!(
            "Push complete. Pushed {} days, Synced {} days.",
            pushed_days, synced_days
        )
    } else {
        "No changes pushed.".to_string()
    }
}
