use chrono::{Datelike, Local, NaiveDate};
use inquire::{Select, Text};
use crate::events::Event;
use crate::external_models::TaskDto;
use crate::{config, models, projector, view};
use crate::actions::utils::{insert_and_resolve_overlaps, parse_time};
use crate::store::EventStore;

pub fn handle_edit(
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
    app_config: &config::Config,
) -> String {
    // 1. Select Day
    let now = Local::now();
    let current_year = now.year();
    let today = now.date_naive();

    let mut dates = Vec::new();
    let mut d = NaiveDate::from_ymd_opt(current_year, 1, 1).unwrap();

    while d.year() == current_year {
        dates.push(d);
        d += chrono::Duration::days(1);
    }

    dates.reverse(); // Newest first

    let date_options: Vec<String> = dates.iter().map(|d| d.format("%Y-%m-%d").to_string()).collect();

    let default_idx = dates.iter().position(|d| *d == today).unwrap_or(0);

    let selected_date_str = match Select::new("Select day to edit:", date_options)
        .with_starting_cursor(default_idx)
        .prompt()
    {
        Ok(s) => s,
        Err(_) => return "Cancelled.".to_string(),
    };

    let selected_date = NaiveDate::parse_from_str(&selected_date_str, "%Y-%m-%d").unwrap();

    // 2. Get tasks for that day
    // We need to load events for that day to reconstruct the tasks
    // Note: We are not using the passed 'tasks' or 'history' here as they likely represent "today"
    // We will load fresh data for the edit session.
    let day_events = event_store.events_for_day(selected_date);
    let mut day_tasks = projector::restore_state(&day_events);

    // 3. Edit Loop
    loop {
        // sort for display
        day_tasks.sort_by_key(|p| p.start_time);

        println!("\n--- Editing {} ---", selected_date);
        if day_tasks.is_empty() {
            println!("(No tasks)");
        }

        // Show graphical timeline
        if !day_tasks.is_empty() {
            let refs: Vec<&models::Task> = day_tasks.iter().collect();
            view::render_day(&refs);
        }

        if day_tasks.is_empty() {
            println!("(No tasks)");
        }

        for (i, p) in day_tasks.iter().enumerate() {
            let end_str = p.end_time.map(|t| t.format("%H:%M").to_string()).unwrap_or("...".to_string());
            println!("{}. [{}-{}] {}", i + 1, p.start_time.format("%H:%M"), end_str, p.name);
        }
        println!("-----------------------");

        let options = vec![
            "Add Entry",
            "Edit Entry Time",
            "Delete Entry",
            "Save & Finish",
            "Cancel",
        ];

        let action = match Select::new("Action:", options).prompt() {
            Ok(a) => a,
            Err(_) => return "Cancelled.".to_string(),
        };

        match action {
            "Add Entry" => {
                // Pick Task - Filter only Favorites
                let favorite_tasks: Vec<&TaskDto> = external_tasks.iter()
                    .filter(|t| app_config.favorite_tasks.contains(&t.id))
                    .collect();

                let task_options: Vec<String> = favorite_tasks.iter().map(|t| t.to_string()).collect();

                // Also add "Break"
                let mut combined_options = vec!["Break".to_string()];
                combined_options.extend(task_options);

                let task_choice = match Select::new("Select Task/Break (Favorites only):", combined_options).prompt() {
                    Ok(t) => t,
                    Err(_) => continue,
                };

                let (name, id, is_break, project_name, rate) = if task_choice == "Break" {
                    ("Break".to_string(), -1, true, "".to_string(), 0.0)
                } else {
                    let t = favorite_tasks.iter().find(|t| t.to_string() == task_choice).unwrap();
                    (t.name.clone(), t.id, false, t.project.name.clone(), t.compensation_rate)
                };

                // Ask Start Time
                let start_str = match Text::new("Start Time (HH:MM):").prompt() {
                    Ok(s) => s,
                    Err(_) => continue,
                };
                let start_time = match parse_time(selected_date, &start_str) {
                    Some(t) => t,
                    None => { println!("Invalid time format."); continue; }
                };

                // Ask End Time
                let end_str = match Text::new("End Time (HH:MM):").prompt() {
                    Ok(s) => s,
                    Err(_) => continue,
                };
                let end_time = match parse_time(selected_date, &end_str) {
                    Some(t) => t,
                    None => { println!("Invalid time format."); continue; }
                };

                if end_time <= start_time {
                    println!("Error: End time must be after start time.");
                    continue;
                }

                let new_p = models::Task {
                    id,
                    name,
                    project_name,
                    rate,
                    start_time,
                    end_time: Some(end_time),
                    is_break,
                    is_generated: false,
                };

                insert_and_resolve_overlaps(&mut day_tasks, new_p);
            },
            "Edit Entry Time" => {
                if day_tasks.is_empty() { continue; }
                let indices: Vec<String> = (1..=day_tasks.len()).map(|i| i.to_string()).collect();
                let idx_res = Select::new("Select entry number:", indices).prompt();
                if let Ok(idx_str) = idx_res {
                    let idx = idx_str.parse::<usize>().unwrap() - 1;
                    // Remove temporarily to resolve overlaps against others
                    let mut p = day_tasks.remove(idx);

                    let s_default = p.start_time.format("%H:%M").to_string();
                    let e_default = p.end_time.map(|t| t.format("%H:%M").to_string()).unwrap_or_default();

                    let start_res = Text::new("Start Time:").with_default(&s_default).prompt();
                    let end_res = Text::new("End Time:").with_default(&e_default).prompt();

                    if let (Ok(s), Ok(e)) = (start_res, end_res) {
                        let st = parse_time(selected_date, &s);
                        let et = parse_time(selected_date, &e);

                        match (st, et) {
                            (Some(start), Some(end)) => {
                                if end <= start {
                                    println!("Error: End time must be after start time.");
                                    day_tasks.insert(idx, p); // Restore original
                                } else {
                                    p.start_time = start;
                                    p.end_time = Some(end);
                                    insert_and_resolve_overlaps(&mut day_tasks, p);
                                }
                            }
                            _ => {
                                println!("Invalid time format.");
                                day_tasks.insert(idx, p); // Restore original
                            }
                        }
                    } else {
                        day_tasks.insert(idx, p); // Restore on cancel
                    }
                }
            },
            "Delete Entry" => {
                if day_tasks.is_empty() { continue; }
                let indices: Vec<String> = (1..=day_tasks.len()).map(|i| i.to_string()).collect();
                let idx_res = Select::new("Select entry number:", indices).prompt();
                if let Ok(idx_str) = idx_res {
                    let idx = idx_str.parse::<usize>().unwrap() - 1;
                    day_tasks.remove(idx);
                }
            },
            "Save & Finish" => {
                // Rebuild events for this day
                day_tasks.sort_by_key(|p| p.start_time);
                let mut new_events = Vec::new();
                let mut last_end_time: Option<chrono::DateTime<Local>> = None;

                for p in day_tasks {
                    if let Some(last_end) = last_end_time {
                        if p.start_time > last_end {
                            new_events.push(Event::BreakStarted {
                                start_time: last_end,
                                is_generated: true,
                            });
                            new_events.push(Event::Stopped {
                                end_time: p.start_time,
                                is_generated: true,
                            });
                        }
                    }

                    if p.is_break {
                        new_events.push(Event::BreakStarted { start_time: p.start_time, is_generated: false });
                    } else {
                        new_events.push(Event::TaskStarted {
                            id: p.id,
                            name: p.name,
                            project_name: p.project_name,
                            rate: p.rate,
                            start_time: p.start_time,
                            is_generated: false
                        });
                    }
                    if let Some(end) = p.end_time {
                        new_events.push(Event::Stopped { end_time: end, is_generated: false });
                        last_end_time = Some(end);
                    } else {
                        last_end_time = None;
                    }
                }

                let revised_event = Event::DayRevised { date: selected_date, events: new_events };

                // Persist
                event_store.persist(&revised_event);

                // If we edited today, we should update the in-memory state passed to this function
                if selected_date == Local::now().date_naive() {
                    history.push(revised_event);
                    *tasks = projector::restore_state(history);
                }

                return "Day updated.".to_string();
            },
            "Cancel" => return "Cancelled.".to_string(),
            _ => {},
        }
    }
}