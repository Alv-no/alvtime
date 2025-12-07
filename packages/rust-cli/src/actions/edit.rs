use crate::actions::utils::{get_all_tasks, parse_time};
use crate::events::Event;
use crate::external_models::TaskDto;
use crate::store::EventStore;
use crate::{config, models, projector, view};
use chrono::{Local, NaiveDate};
use inquire::{Select, Text};
use std::collections::HashSet;

// Helper function to create a default 7.5 hour task from a TaskDto
fn create_default_day_task(date: NaiveDate, task_dto: &TaskDto) -> models::Task {
    let start_time = date
        .and_hms_opt(8, 0, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();
    let end_time = date
        .and_hms_opt(15, 30, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap(); // 7.5 hours later

    models::Task {
        id: task_dto.id,
        name: task_dto.name.clone(),
        project_name: task_dto.project.name.clone(),
        customer_name: task_dto.project.customer.name.clone(),
        rate: task_dto.compensation_rate,
        start_time,
        end_time: Some(end_time),
        is_break: false,
    }
}

pub fn handle_edit(
    holidays: &HashSet<NaiveDate>,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
    app_config: &config::Config,
) -> String {
    // 1. Initial Action Selection
    let initial_options = vec!["Edit Single Day", "Bulk Edit/Clear Days", "Cancel"];

    let initial_action = match Select::new("Select editing mode:", initial_options).prompt() {
        Ok(a) => a,
        Err(_) => return "Cancelled.".to_string(),
    };

    // Get the tasks needed for the interactive month view
    let all_tasks = get_all_tasks(event_store);

    match initial_action {
        "Edit Single Day" => {
            // 1. Select Day using the interactive view
            let selected_dates = match view::interactive_view(&all_tasks, &holidays, &event_store.get_unsynced_dates(),false) {
                Ok(d) => d,
                Err(e) => return format!("Interactive view failed: {}", e),
            };

            let selected_date = match selected_dates.first() {
                Some(d) => *d,
                None => return "Cancelled. No day selected.".to_string(),
            };

            handle_single_day_edit(selected_date, event_store, external_tasks, app_config)
        }
        "Bulk Edit/Clear Days" => handle_bulk_day_edit(
            holidays,
            event_store,
            external_tasks,
            app_config,
            &all_tasks,
        ),
        _ => "Cancelled.".to_string(),
    }
}

fn handle_single_day_edit(
    selected_date: NaiveDate,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
    app_config: &config::Config,
) -> String {
    let today = Local::now().date_naive();
    let initial_day_events = event_store.events_for_day(selected_date);

    // Vector to store newly created or compensating events
    let mut new_events: Vec<Event> = Vec::new();

    // Helper closure to get the currently projected state
    let get_current_tasks = |new_events: &[Event]| -> Vec<models::Task> {
        let mut combined_events = initial_day_events.clone();
        combined_events.extend_from_slice(new_events);
        // Using the projector to derive the stable, overlapping-resolved state
        projector::restore_state(&combined_events)
    };

    // 3. Edit Loop
    loop {
        // Project the current state (initial + new_events) for display
        let mut current_day_tasks = get_current_tasks(&new_events);
        current_day_tasks.sort_by_key(|p| p.start_time);

        let (closed_tasks, open_tasks): (Vec<models::Task>, Vec<models::Task>) =
            current_day_tasks
                .into_iter()
                .partition(|t| t.end_time.is_some());

        let current_day_tasks = closed_tasks;

        println!("\n--- Editing {} ---", selected_date);
        if current_day_tasks.is_empty() {
            println!("(No closed tasks available for edit)");
        }

        // Show graphical timeline for the projected state (including open tasks for context)
        // Rebuild refs using the original, unfiltered list for rendering
        let all_tasks_for_view = {
            let mut combined = current_day_tasks.clone();
            combined.extend(open_tasks.clone());
            combined.sort_by_key(|p| p.start_time);
            combined
        };

        if !all_tasks_for_view.is_empty() {
            let refs: Vec<&models::Task> = all_tasks_for_view.iter().collect();
            if let Err(e) = view::render_day(&refs) {
                eprintln!("Error rendering day: {}", e);
            }
        }

        // Print only the CLOSED tasks available for editing, plus the open ones with a warning
        for (i, p) in current_day_tasks.iter().enumerate() {
            let end_str = p
                .end_time
                .map(|t| t.format("%H:%M").to_string())
                .unwrap_or("...".to_string());
            println!(
                "{}. [{}-{}] {} ({})",
                i + 1,
                p.start_time.format("%H:%M"),
                end_str,
                p.name,
                p.project_name
            );
        }

        if !open_tasks.is_empty() {
            println!("--- Active Tasks (Cannot be edited) ---");
            for p in open_tasks.iter() {
                println!(
                    "   [{}->] {} ({})",
                    p.start_time.format("%H:%M"),
                    p.name,
                    p.project_name
                );
            }
        }

        println!("-----------------------");

        let options = vec![
            "Add Entry",
            "Edit Entry Time",
            "Edit Entry Type",
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
                let favorite_tasks: Vec<&TaskDto> = external_tasks
                    .iter()
                    .filter(|t| app_config.favorite_tasks.contains(&t.id))
                    .collect();

                let task_options: Vec<String> =
                    favorite_tasks.iter().map(|t| t.to_string()).collect();
                let mut combined_options = vec!["Break".to_string()];
                combined_options.extend(task_options);

                let task_choice =
                    match Select::new("Select Task/Break (Favorites only):", combined_options)
                        .prompt()
                    {
                        Ok(t) => t,
                        Err(_) => continue,
                    };

                let (name, id, is_break, project_name, customer_name, rate) =
                    if task_choice == "Break" {
                        (
                            "Break".to_string(),
                            -1,
                            true,
                            "".to_string(),
                            "".to_string(),
                            0.0,
                        )
                    } else {
                        let t = favorite_tasks
                            .iter()
                            .find(|t| t.to_string() == task_choice)
                            .unwrap();
                        (
                            t.name.clone(),
                            t.id,
                            false,
                            t.project.name.clone(),
                            t.project.customer.name.clone(),
                            t.compensation_rate,
                        )
                    };

                let start_str = match Text::new("Start Time (HH:MM):").prompt() {
                    Ok(s) => s,
                    Err(_) => continue,
                };
                let start_time = match parse_time(selected_date, &start_str) {
                    Some(t) => t,
                    None => {
                        println!("Invalid start time format.");
                        continue;
                    }
                };

                let end_str = match Text::new("End Time (HH:MM):").prompt() {
                    Ok(s) => s,
                    Err(_) => continue,
                };
                let end_time = match parse_time(selected_date, &end_str) {
                    Some(t) => t,
                    None => {
                        println!("Invalid end time format.");
                        continue;
                    }
                };

                if end_time <= start_time {
                    println!("Error: End time must be after start time.");
                    continue;
                }

                if is_break {
                    new_events.push(Event::BreakStarted {
                        start_time,
                    });
                } else {
                    new_events.push(Event::TaskStarted {
                        id,
                        name,
                        project_name,
                        customer_name,
                        rate,
                        start_time,
                    });
                }

                new_events.push(Event::Stopped {
                    end_time,
                });
            }
            "Edit Entry Time" => {
                if current_day_tasks.is_empty() {
                    println!("No closed tasks available to edit.");
                    continue;
                }
                let indices: Vec<String> = (1..=current_day_tasks.len())
                    .map(|i| i.to_string())
                    .collect();
                let idx_res = Select::new("Select entry number:", indices).prompt();

                if let Ok(idx_str) = idx_res {
                    let idx = idx_str.parse::<usize>().unwrap() - 1;

                    let original_task = match current_day_tasks.get(idx) {
                        Some(t) => t.clone(),
                        None => continue,
                    };

                    // Guardrail against accidental open task editing - redundant due to filtering, but safe.
                    if original_task.end_time.is_none() {
                        println!("Error: Cannot edit an active (unclosed) task.");
                        continue;
                    }

                    let s_default = original_task.start_time.format("%H:%M").to_string();
                    let e_default = original_task
                        .end_time
                        .map(|t| t.format("%H:%M").to_string())
                        .unwrap_or_default();

                    let start_res = Text::new("New Start Time:")
                        .with_default(&s_default)
                        .prompt();
                    let end_res = Text::new("New End Time:").with_default(&e_default).prompt();

                    if let (Ok(s), Ok(e)) = (start_res, end_res) {
                        let new_start = match parse_time(selected_date, s.trim()) {
                            Some(t) => t,
                            None => {
                                println!("Invalid start time format.");
                                continue;
                            }
                        };

                        let new_end = {
                            let trimmed = e.trim();
                            if trimmed.is_empty() {
                                // Prevent saving as an open task during edit time operation
                                println!("Error: End time is mandatory when editing a closed entry.");
                                continue;
                            } else if let Some(parsed) = parse_time(selected_date, trimmed) {
                                Some(parsed)
                            } else {
                                println!("Invalid end time format.");
                                continue;
                            }
                        };

                        if let Some(end_time) = new_end {
                            if end_time <= new_start {
                                println!("Error: End time must be after start time.");
                                continue;
                            }
                        }

                        // Overwrite with the New Task/Break using new times
                        if original_task.is_break {
                            new_events.push(Event::BreakStarted {
                                start_time: new_start,
                            });
                        } else {
                            new_events.push(Event::TaskStarted {
                                id: original_task.id,
                                name: original_task.name,
                                project_name: original_task.project_name,
                                customer_name: original_task.customer_name,
                                rate: original_task.rate,
                                start_time: new_start,
                            });
                        }
                        if let Some(end) = new_end {
                            new_events.push(Event::Stopped {
                                end_time: end,
                            });
                        }
                    }
                }
            }
            "Edit Entry Type" => {
                if current_day_tasks.is_empty() {
                    println!("No closed tasks available to edit.");
                    continue;
                }

                let indices: Vec<String> = (1..=current_day_tasks.len())
                    .map(|i| i.to_string())
                    .collect();

                let idx = match Select::new("Select entry number:", indices).prompt() {
                    Ok(choice) => choice.parse::<usize>().unwrap() - 1,
                    Err(_) => continue,
                };

                let original_task = match current_day_tasks.get(idx) {
                    Some(t) => t.clone(),
                    None => continue,
                };

                // Guardrail against accidental open task editing - redundant due to filtering, but safe.
                if original_task.end_time.is_none() {
                    println!("Error: Cannot edit an active (unclosed) task.");
                    continue;
                }

                let favorite_tasks: Vec<&TaskDto> = external_tasks
                    .iter()
                    .filter(|t| app_config.favorite_tasks.contains(&t.id))
                    .collect();

                let mut type_options = vec!["Break".to_string()];
                type_options.extend(favorite_tasks.iter().map(|t| t.to_string()));

                let type_choice = match Select::new("Select new task type:", type_options).prompt()
                {
                    Ok(choice) => choice,
                    Err(_) => continue,
                };

                let selected_label = type_choice.as_str();
                let (new_id, new_name, new_is_break, new_project_name, new_customer_name, new_rate) =
                    if selected_label == "Break" {
                        (
                            -1,
                            "Break".to_string(),
                            true,
                            "".to_string(),
                            "".to_string(),
                            0.0,
                        )
                    } else if let Some(new_task_dto) = favorite_tasks
                        .iter()
                        .find(|t| t.to_string() == selected_label)
                    {
                        (
                            new_task_dto.id,
                            new_task_dto.name.clone(),
                            false,
                            new_task_dto.project.name.clone(),
                            new_task_dto.project.customer.name.clone(),
                            new_task_dto.compensation_rate,
                        )
                    } else {
                        continue;
                    };

                // Overwrite with the New Task/Break using the ORIGINAL start/end times.
                if new_is_break {
                    new_events.push(Event::BreakStarted {
                        start_time: original_task.start_time,
                    });
                } else {
                    new_events.push(Event::TaskStarted {
                        id: new_id,
                        name: new_name,
                        project_name: new_project_name,
                        customer_name: new_customer_name,
                        rate: new_rate,
                        start_time: original_task.start_time,
                    });
                }

                if let Some(end) = original_task.end_time {
                    new_events.push(Event::Stopped {
                        end_time: end,
                    });
                }
            }
            "Delete Entry" => {
                if current_day_tasks.is_empty() {
                    println!("No closed tasks available to delete.");
                    continue;
                }
                let indices: Vec<String> = (1..=current_day_tasks.len())
                    .map(|i| i.to_string())
                    .collect();
                let idx_res = Select::new("Select entry number:", indices).prompt();

                if let Ok(idx_str) = idx_res {
                    let idx = idx_str.parse::<usize>().unwrap() - 1;

                    let removed = match current_day_tasks.get(idx) {
                        Some(t) => t.clone(),
                        None => continue,
                    };

                    if removed.end_time.is_none() {
                        println!("Error: Cannot delete an active (unclosed) task.");
                        continue;
                    }

                    // To delete the task, we replace its duration with a Break.
                    new_events.push(Event::BreakStarted {
                        start_time: removed.start_time,
                    });
                    new_events.push(Event::Stopped {
                        end_time: removed.end_time.unwrap(), // Safe due to filtering/guardrail
                    });
                }
            }
            "Save & Finish" => {
                let final_tasks = get_current_tasks(&new_events);

                if selected_date < today {
                    if final_tasks
                        .iter()
                        .find(|task| task.end_time.is_none())
                        .is_some()
                    {
                        eprintln!(
                            "Cannot save: an entry has no end time. Finish all tasks when editing past days."
                        );
                        continue;
                    }
                }

                if new_events.is_empty() {
                    return "No changes to save.".to_string();
                }

                for event in new_events.drain(..) {
                    // Drain to move events out of the vector
                    event_store.persist(&event);
                }

                return "Day updated. Changes saved as appended events.".to_string();
            }
            "Cancel" => return "Cancelled.".to_string(),
            _ => {}
        }
    }
}fn handle_bulk_day_edit(
    holidays: &HashSet<NaiveDate>,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
    app_config: &config::Config,
    all_tasks: &[models::Task],
) -> String {
    println!("\n--- Bulk Edit Mode ---");
    let days_to_edit = match view::interactive_view(all_tasks, &holidays, &event_store.get_unsynced_dates(),true) {
        Ok(d) => d,
        Err(e) => return format!("Interactive view failed: {}", e),
    };

    if days_to_edit.is_empty() {
        return "Cancelled. No days selected.".to_string();
    }

    let bulk_options = vec![
        "Add Full Day Task (7.5h)",
        "Clear All Tasks on Selected Days",
        "Cancel",
    ];
    let bulk_action = match Select::new(
        &format!("Action for {} days:", days_to_edit.len()),
        bulk_options,
    )
    .prompt()
    {
        Ok(a) => a,
        Err(_) => return "Cancelled.".to_string(),
    };

    let today = Local::now().date_naive();
    let mut days_processed = 0;

    match bulk_action {
        "Add Full Day Task (7.5h)" => {
            // Select the Task to apply (Favorites only)
            let favorite_tasks: Vec<&TaskDto> = external_tasks
                .iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id))
                .collect();

            let task_options: Vec<String> = favorite_tasks.iter().map(|t| t.to_string()).collect();

            let task_choice =
                match Select::new("Select Task (7.5h) to apply:", task_options).prompt() {
                    Ok(t) => t,
                    Err(_) => return "Cancelled.".to_string(),
                };

            let selected_task_dto = favorite_tasks
                .iter()
                .find(|t| t.to_string() == task_choice)
                .unwrap();

            for date in days_to_edit {
                // 1. Pre-check for active task
                if date == today {
                    let day_events = event_store.events_for_day(date);
                    let day_tasks = projector::restore_state(&day_events);
                    if day_tasks.iter().any(|t| t.end_time.is_none()) {
                        println!("\rSkipping {} (has an active task).", date);
                        continue;
                    }
                }

                // 2. Determine necessary events
                let full_day_task = create_default_day_task(date, selected_task_dto);
                let mut events_to_persist = Vec::new();

                // To overwrite the day's existing history without DayRevised,
                // we must generate a new event sequence based on the new task,
                // and assume that these new events supersede any previous events
                // covering the same time range when restored by the projector.

                // The safest way to "clear" the day first without DayRevised is
                // to explicitly push a compensating Break covering the entire day (8:00 to 15:30),
                // and then immediately overwrite it with the intended task.
                // Since this gets messy, we'll just push the task events and rely on
                // the projector's conflict resolution logic (which typically prioritizes later events).

                // Start of the new task
                events_to_persist.push(Event::TaskStarted {
                    id: full_day_task.id,
                    name: full_day_task.name,
                    project_name: full_day_task.project_name,
                    customer_name: full_day_task.customer_name,
                    rate: full_day_task.rate,
                    start_time: full_day_task.start_time,
                });

                // End of the new task
                if let Some(end_time) = full_day_task.end_time {
                    events_to_persist.push(Event::Stopped {
                        end_time,
                    });
                }

                // 3. Persist individual events
                for event in events_to_persist {
                    event_store.persist(&event);
                }

                days_processed += 1;
            }

            if days_processed == 0 {
                "No days were updated due to active tasks or cancellation.".to_string()
            } else {
                format!(
                    "Successfully added 7.5h task to {} days. (Saved as events)",
                    days_processed
                )
            }
        }
        "Clear All Tasks on Selected Days" => {
            for date in days_to_edit {
                // 1. Pre-check for active task
                if date == today {
                    let day_events = event_store.events_for_day(date);
                    let day_tasks = projector::restore_state(&day_events);
                    if day_tasks.iter().any(|t| t.end_time.is_none()) {
                        println!("\rSkipping {} (has an active task).", date);
                        continue;
                    }
                }

                // We'll calculate the existing tasks and generate a Break over their entire duration.
                let day_events = event_store.events_for_day(date);
                let existing_tasks = projector::restore_state(&day_events);

                if existing_tasks.is_empty() {
                    // Nothing to clear on this day
                    continue;
                }

                event_store.persist(&Event::LocallyCleared {
                    date,
                });

                days_processed += 1;
            }

            if days_processed == 0 {
                "No days were cleared due to active tasks or cancellation.".to_string()
            } else {
                format!(
                    "Successfully cleared tasks from {} days. (Saved as events)",
                    days_processed
                )
            }
        }
        _ => "Cancelled.".to_string(),
    }
}
