use std::collections::{HashMap, HashSet};
use inquire::Select;
use crate::{alvtime, external_models, models};
use crate::actions::utils::{generate_events_from_server_entries, round_duration_to_quarter_hour};
use crate::events::Event;
use crate::external_models::TaskDto;
use crate::projector::restore_state;
use crate::store::EventStore;

pub fn handle_push(
    client: &alvtime::AlvtimeClient,
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
) -> String {
    // Extract the date from the tasks/history (single day only)
    let date = if let Some(task) = tasks.first() {
        task.start_time.date_naive()
    } else if let Some(event) = history.first() {
        match event {
            Event::TaskStarted { start_time, .. } => start_time.date_naive(),
            Event::BreakStarted { start_time, .. } => start_time.date_naive(),
            Event::Stopped { end_time, .. } => end_time.date_naive(),
            Event::Reopen { start_time, .. } => start_time.date_naive(),
            Event::Undo { time } => time.date_naive(),
            Event::Redo { time } => time.date_naive(),
            Event::DayRevised { date, .. } => *date,
        }
    } else {
        return "No events to push.".to_string();
    };

    // Check if there are any local (non-generated) events
    let has_local_events = history.iter().any(|e| {
        let is_generated = match e {
            Event::TaskStarted { is_generated, .. } => *is_generated,
            Event::BreakStarted { is_generated, .. } => *is_generated,
            Event::Stopped { is_generated, .. } => *is_generated,
            Event::Reopen { is_generated, .. } => *is_generated,
            Event::Undo { .. } | Event::Redo { .. } | Event::DayRevised { .. } => false,
        };
        !is_generated
    });

    if !has_local_events {
        return String::new(); // No local changes for this day
    }

    // Fetch server entries for this specific day
    let entries = match client.list_time_entries(date, date) {
        Ok(e) => e,
        Err(e) => return format!("Failed to fetch entries for {}: {}", date, e),
    };

    let day_tasks: Vec<&models::Task> = tasks
        .iter()
        .filter(|t| !t.is_break)
        .collect();

    let has_server_entries = !entries.is_empty();

    if day_tasks.is_empty() && !has_server_entries {
        return String::new();
    }

    let mut should_push = true;

    // Check for conflicts
    if has_server_entries {
        let server_hours: f64 = entries.iter().map(|e| e.value).sum();

        let mut local_sums: HashMap<i32, i64> = HashMap::new();
        for t in &day_tasks {
            *local_sums.entry(t.id).or_default() += t.duration().num_minutes();
        }
        let local_hours: f64 = local_sums.values().map(|m| round_duration_to_quarter_hour(*m)).sum();

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
                    let new_events = generate_events_from_server_entries(
                        date,
                        &entries.iter().collect::<Vec<_>>(),
                        external_tasks,
                    );
                    let revised_event = Event::DayRevised {
                        date,
                        events: new_events,
                    };
                    event_store.persist(&revised_event);
                    history.push(revised_event);
                    *tasks = restore_state(history);
                    return format!("Synced {} from server.", date);
                }
                _ => {
                    return format!("Skipped {}.", date);
                }
            },
            Err(_) => {
                return format!("Cancelled push for {}.", date);
            }
        }
    }

    if should_push {
        // Calculate local task sums
        let mut local_sums: HashMap<i32, i64> = HashMap::new();
        for t in &day_tasks {
            *local_sums.entry(t.id).or_default() += t.duration().num_minutes();
        }

        let local_task_ids: HashSet<i32> = local_sums.keys().copied().collect();
        let mut entries_to_push: Vec<external_models::TimeEntryDto> = Vec::new();

        // First, remove server entries that don't exist locally (0 hours)
        for server_entry in &entries {
            if !local_task_ids.contains(&server_entry.task_id) {
                entries_to_push.push(external_models::TimeEntryDto {
                    id: server_entry.id,
                    task_id: server_entry.task_id,
                    date: date.format("%Y-%m-%d").to_string(),
                    value: 0.0,
                    comment: None,
                });
            }
        }

        // Then add/update local entries
        for (task_id, minutes) in local_sums {
            let existing_id = entries
                .iter()
                .find(|e| e.task_id == task_id)
                .map(|e| e.id)
                .unwrap_or(0);

            entries_to_push.push(external_models::TimeEntryDto {
                id: existing_id,
                task_id,
                date: date.format("%Y-%m-%d").to_string(),
                value: round_duration_to_quarter_hour(minutes),
                comment: None,
            });
        }

        if !entries_to_push.is_empty() {
            if let Err(e) = client.upsert_time_entries(&entries_to_push) {
                return format!("Error pushing {}: {}", date, e);
            }

            // Mark all events as generated (synced with server)
            let mut new_events = Vec::new();
            for task in tasks.iter() {
                if task.is_break {
                    new_events.push(Event::BreakStarted {
                        start_time: task.start_time,
                        is_generated: true,
                    });
                } else {
                    new_events.push(Event::TaskStarted {
                        id: task.id,
                        name: task.name.clone(),
                        project_name: task.project_name.clone(),
                        customer_name: task.customer_name.clone(),
                        rate: task.rate,
                        start_time: task.start_time,
                        is_generated: true,
                    });
                }

                if let Some(end_time) = task.end_time {
                    new_events.push(Event::Stopped {
                        end_time,
                        is_generated: true,
                    });
                }
            }

            let revised_event = Event::DayRevised {
                date,
                events: new_events,
            };
            event_store.persist(&revised_event);
            history.push(revised_event);
            *tasks = restore_state(history);
        }

        format!("Pushed {}.", date)
    } else {
        String::new()
    }
}