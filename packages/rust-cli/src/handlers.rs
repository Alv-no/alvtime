use crate::events::Event;
use crate::external_models::TaskDto;
use crate::input_helper::InputHelper;
use crate::store::EventStore;
use crate::{alvtime, config, external_models, models, projector, view};
use chrono::{Datelike, Local, NaiveDate};
use inquire::{MultiSelect, Select, Text};
use rustyline::Editor;
use std::collections::{HashMap, HashSet};

pub fn add_event(
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    event: Event,
    success_msg: &str,
) -> String {
    event_store.persist(&event);
    history.push(event);
    *tasks = projector::restore_state(history);
    success_msg.to_string()
}

pub fn process_autobreak(
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
) -> Option<String> {
    let now = Local::now();
    let today_break_start = now
        .date_naive()
        .and_hms_opt(11, 0, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();
    let today_break_end = now
        .date_naive()
        .and_hms_opt(11, 30, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();

    let mut feedback = None;
    let mut break_just_started = false;

    // Phase 1: Start break
    if let Some(last_task) = tasks.last() {
        if last_task.end_time.is_none() && !last_task.is_break {
            if last_task.start_time < today_break_start && now >= today_break_start {
                let event = Event::BreakStarted {
                    start_time: today_break_start,
                    is_generated: true,
                };
                add_event(tasks, history, event_store, event, "");
                feedback = Some("Autobreak started at 11:00.".to_string());
                break_just_started = true;
            }
        }
    }

    // Phase 2: End break
    if let Some(last_task) = tasks.last() {
        if last_task.end_time.is_none() && last_task.is_break {
            if last_task.start_time == today_break_start && now >= today_break_end {
                // Find the last non-break task to resume
                let prev_task = tasks
                    .iter()
                    .rev()
                    .find(|p| !p.is_break);

                if let Some(prev) = prev_task {
                    // Capture values before mutable borrow in add_event
                    let prev_id = prev.id;
                    let prev_name = prev.name.clone();

                    let event = Event::TaskStarted {
                        id: prev_id,
                        name: prev_name.clone(),
                        project_name: prev.project_name.clone() ,
                        rate:prev.rate,
                        start_time: today_break_end,
                        is_generated: true,
                    };
                    add_event(tasks, history, event_store, event, "");

                    if break_just_started {
                        feedback = Some(format!("Autobreak recorded (11:00-11:30). Resumed '{}'.", prev_name));
                    } else {
                        feedback = Some(format!("Autobreak ended. Resumed '{}' at 11:30.", prev_name));
                    }
                }
            }
        }
    }
    feedback
}

pub fn handle_start(
    parts: &[&str],
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    app_config: &config::Config,
    external_tasks: &[TaskDto],
) -> String {
    let mut name = String::new();
    let mut id = 0;
    let mut project_name = String::new();
    let mut rate = 0.0;

    if parts.len() < 2 {
        // Resume logic or dropdown
        let last_real_task = tasks.iter().rev().find(|p| !p.is_break);

        if last_real_task.is_none() || (tasks.last().is_some() && !tasks.last().unwrap().is_break && tasks.last().unwrap().end_time.is_some()) {
            // No active task or last was stopped.
            if app_config.favorite_tasks.is_empty() {
                return "No favorites found. Use 'favorites add' to search and add tasks.".to_string();
            }

            let favorite_tasks: Vec<&TaskDto> = external_tasks.iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id))
                .collect();

            let options: Vec<String> = favorite_tasks.iter().map(|t| t.to_string()).collect();

            let selection = Select::new("Select task from favorites:", options)
                .with_page_size(10)
                .prompt();

            match selection {
                Ok(selected_str) => {
                    if let Some(task) = favorite_tasks.iter().find(|t| t.to_string() == selected_str) {
                        name = task.name.clone();
                        id = task.id;
                        project_name = task.project.name.clone();
                        rate = task.compensation_rate;
                    }
                },
                Err(_) => return "Cancelled.".to_string(),
            }
        } else if let Some(prev) = last_real_task {
            name = prev.name.clone();
            id = prev.id;
            project_name = prev.project_name.clone();
            rate = prev.rate;
        } else {
            return "No previous task to resume.".to_string();
        }
    } else {
        let search = parts[1..].join(" ");

        // Find task by name in favorites first
        let fav_match = external_tasks.iter()
            .find(|t| app_config.favorite_tasks.contains(&t.id) && t.name == search);

        if let Some(task) = fav_match {
            name = task.name.clone();
            id = task.id;
            project_name = task.project.name.clone();
            rate = task.compensation_rate ;
        } else {
            let matches: Vec<&TaskDto> = external_tasks.iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id) && t.name.to_lowercase().contains(&search.to_lowercase()))
                .collect();

            if matches.len() == 1 {
                name = matches[0].name.clone();
                id = matches[0].id;
                project_name = matches[0].project.name.clone();
                rate = matches[0].compensation_rate ;
            } else if matches.is_empty() {
                return "Task not found in favorites. Use 'favorites add' to find it.".to_string();
            } else {
                let options: Vec<String> = matches.iter().map(|t| t.to_string()).collect();
                let selection = Select::new("Multiple favorites match. Select one:", options).prompt();
                match selection {
                    Ok(sel) => {
                        if let Some(task) = matches.iter().find(|t| t.to_string() == sel) {
                            name = task.name.clone();
                            id = task.id;
                            project_name = task.project.name.clone();
                            rate = task.compensation_rate ;
                        }
                    },
                    Err(_) => return "Cancelled.".to_string(),
                }
            }
        }
    }

    let now = Local::now();

    // If there is a gap after the last task, fill it with a break
    if let Some(last_task) = tasks.last() {
        if let Some(end_time) = last_task.end_time {
            if end_time < now && end_time.date_naive() == now.date_naive() {
                let break_event = Event::BreakStarted {
                    start_time: end_time,
                    is_generated: true,
                };
                event_store.persist(&break_event);
                history.push(break_event);

                let stop_event = Event::Stopped {
                    end_time: now,
                    is_generated: true,
                };
                event_store.persist(&stop_event);
                history.push(stop_event);
            }
        }
    }

    let event = Event::TaskStarted {
        id,
        name: name.clone(),
        project_name,
        rate,
        start_time: now,
        is_generated: false,
    };
    add_event(tasks, history, event_store, event, &format!("Started task at {}", now.format("%H:%M")))
}

pub fn handle_favorites(
    parts: &[&str],
    app_config: &mut config::Config,
    tasks: &[TaskDto],
    rl: &mut Editor<InputHelper, rustyline::history::DefaultHistory>,
) -> String {
    if parts.len() < 2 {
        let mut output = String::from("Favorites:\n");
        for id in &app_config.favorite_tasks {
            if let Some(task) = tasks.iter().find(|t| t.id == *id) {
                output.push_str(&format!(" - {}\n", task));
            } else {
                output.push_str(&format!(" - Unknown ID: {}\n", id));
            }
        }
        return output;
    }

    match parts[1] {
        "add" => {
            let options: Vec<String> = tasks.iter()
                .map(|t| t.to_string())
                .collect();

            if options.is_empty() {
                return "No tasks available to add.".to_string();
            }

            let mut default_indices = Vec::new();
            for (i, t) in tasks.iter().enumerate() {
                if app_config.favorite_tasks.contains(&t.id) {
                    default_indices.push(i);
                }
            }

            let ans = MultiSelect::new("Select task to add (Type to filter):", options.clone())
                .with_page_size(15)
                .with_default(&default_indices)
                .prompt();

            match ans {
                Ok(selections) => {
                    let mut new_favorites = Vec::new();
                    for sel in selections {
                        if let Some(task) = tasks.iter().find(|t| t.to_string() == sel) {
                            new_favorites.push(task.id);
                        }
                    }

                    let count = new_favorites.len();
                    app_config.favorite_tasks = new_favorites;
                    app_config.save();
                    if let Some(h) = rl.helper_mut() {
                        h.favorites = app_config.favorite_tasks.clone();
                    }
                    format!("Favorites updated. Total: {}", count)
                },
                Err(_) => "Selection cancelled.".to_string(),
            }
        },
        "remove" => {
            if app_config.favorite_tasks.is_empty() {
                return "No favorites to remove.".to_string();
            }

            let current_favorites: Vec<String> = tasks.iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id))
                .map(|t| t.to_string())
                .collect();

            let ans = MultiSelect::new("Select favorites to remove:", current_favorites)
                .prompt();

            match ans {
                Ok(selections) => {
                    app_config.favorite_tasks.retain(|id| {
                        let task_str = tasks.iter().find(|t| t.id == *id).map(|t| t.to_string()).unwrap_or_default();
                        !selections.contains(&task_str)
                    });
                    app_config.save();
                    if let Some(h) = rl.helper_mut() {
                        h.favorites = app_config.favorite_tasks.clone();
                    }
                    "Favorites removed.".to_string()
                },
                Err(_) => "Cancelled.".to_string(),
            }
        },
        _ => "Unknown command.".to_string()
    }
}

pub fn handle_config(parts: &[&str], app_config: &mut config::Config) -> String {
    if parts.len() < 2 {
        return "Usage: config set-token <value>".to_string();
    }
    match parts[1] {
        "set-token" => {
            if parts.len() < 3 {
                "Please provide a token value.".to_string()
            } else {
                let token = parts[2].to_string();
                app_config.personal_token = Some(token);
                app_config.save();
                "Personal token saved.".to_string()
            }
        }
        "autobreak" => {
            if parts.len() < 3 {
                format!(
                    "Autobreak is currently {}.",
                    if app_config.autobreak { "on" } else { "off" }
                )
            } else {
                match parts[2] {
                    "on" | "true" | "enable" => {
                        app_config.autobreak = true;
                        app_config.save();
                        "Autobreak enabled (11:00 - 11:30).".to_string()
                    }
                    "off" | "false" | "disable" => {
                        app_config.autobreak = false;
                        app_config.save();
                        "Autobreak disabled.".to_string()
                    }
                    _ => "Usage: config autobreak <on|off>".to_string()
                }
            }
        }
        _ => "Unknown config command.".to_string(),
    }
}

pub fn get_help_text() -> String {
    r#"Commands:
        start [name]                - Start tracking a task (from favorites)
        break                       - Take a break
        stop                        - Stop tracking
        sync                        - Sync entries from server (current year)
        push                        - push all local changes to server
        view [w/m/y]                - Show the timeline (Day/Week/Month/Year)
        edit                        - Edit events for a selectable day
        undo                        - Undo the last entry
        redo                        - Redo the last entry
        favorites [add/remove]      - Manage favorite tasks for quick access
        config set-token <token>    - Set personal token
        config autobreak <on|off>   - Enable/disable 11:00-11:30 break
        help                        - List commands
        quit                        - Exit"#.to_string()
}

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

pub fn handle_sync(
    client: &alvtime::AlvtimeClient,
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
) -> String {
    let current_year = Local::now().year();
    println!("Fetching time entries for {}...", current_year);

    let mut entries = Vec::new();
    use std::io::Write;

    for month in 1..=12 {
        let start_date = NaiveDate::from_ymd_opt(current_year, month, 1).unwrap();
        let next_month_start = if month == 12 {
            NaiveDate::from_ymd_opt(current_year + 1, 1, 1).unwrap()
        } else {
            NaiveDate::from_ymd_opt(current_year, month + 1, 1).unwrap()
        };
        let end_date = next_month_start.pred_opt().unwrap();

        print!(".");
        std::io::stdout().flush().ok();

        match client.list_time_entries(start_date, end_date) {
            Ok(mut e) => entries.append(&mut e),
            Err(e) => return format!("\nFailed to fetch entries: {}", e),
        };
    }
    println!(" Done.");

    // 1. Group server entries by Date
    let mut entries_by_date: HashMap<NaiveDate, Vec<&external_models::TimeEntryDto>> =
        HashMap::new();
    for entry in &entries {
        if let Ok(date) = NaiveDate::parse_from_str(&entry.date, "%Y-%m-%d") {
            entries_by_date.entry(date).or_default().push(entry);
        }
    }

    // 2. Identify which dates already have LOCAL events (not generated by sync)
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
                Event::Undo { time } => (time.date_naive(), false), // Undo is manual
                Event::Redo { time } => (time.date_naive(), false), // Redo is manual
                Event::DayRevised { date, .. } => (*date, false),   // Manual revision
            };
            if !is_generated {
                Some(date)
            } else {
                None
            }
        })
        .collect();

    let mut synced_days = 0;

    // 3. Handle Local Changes (Sync Only)
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
                "Discard Local (Sync from Server)",
                "Skip Day (Keep Local)",
            ];

            match Select::new(&msg, options).prompt() {
                Ok(choice) => match choice {
                    "Discard Local (Sync from Server)" => {
                        // Generate events from server data
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
                        // Skip
                    }
                },
                Err(_) => {}
            }
        }
    }

    let mut added_count = 0;

    // 4. Process new dates
    for (date, day_entries) in entries_by_date {
        if local_dates.contains(&date) {
            continue; // Skip syncing for days that already have local data (or were handled above)
        }

        let total_hours: f64 = day_entries.iter().map(|e| e.value).sum();
        if total_hours == 0.0 {
            continue;
        }

        // Explicitly delete existing events for this day from the store
        event_store.delete_events_for_day(date);

        // If the date being synced is the current day (today), clear the in-memory history
        // so we can rebuild it with the fresh events from the server.
        if date == Local::now().date_naive() {
            history.clear();
        }

        let new_events = generate_events_from_server_entries(date, &day_entries, external_tasks);
        for evt in new_events {
            event_store.persist(&evt);
            // Only push to history if it is for the current day
            if evt.date() == Local::now().date_naive() {
                history.push(evt);
            }
        }
        added_count += 1;
    }

    if added_count > 0 || synced_days > 0 {
        // Restore state after bulk insert (re-projects history into tasks)
        *tasks = projector::restore_state(history);
        format!(
            "Synced. Pulled {} days, Overwrote Local {} days.",
            added_count, synced_days
        )
    } else {
        "No incoming changes.".to_string()
    }
}

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

fn generate_events_from_server_entries(
    date: NaiveDate,
    day_entries: &[&external_models::TimeEntryDto],
    external_tasks: &[TaskDto],
) -> Vec<Event> {
    let mut events = Vec::new();

    let day_start = date
        .and_hms_opt(0, 0, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();

    let day_end = date
        .and_hms_opt(23, 45, 00)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();

    let lunch_start = date
        .and_hms_opt(11, 0, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();

    let lunch_end = date
        .and_hms_opt(11, 30, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();

    let lunch_dur_minutes: i64 = (lunch_end - lunch_start).num_minutes();

    // Calculate total work minutes
    let total_work_minutes: i64 = day_entries
        .iter()
        .map(|e| (e.value * 60.0).round() as i64)
        .sum();

    if total_work_minutes <= 0 {
        return events;
    }

    // Assume lunch will be inserted for projection
    let projected_span_minutes = total_work_minutes + lunch_dur_minutes;

    // Default start
    let default_start = date
        .and_hms_opt(8, 0, 0)
        .unwrap()
        .and_local_timezone(Local)
        .unwrap();

    // Project end with default start
    let projected_end = default_start + chrono::Duration::minutes(projected_span_minutes);

    let mut cursor = default_start;

    // If projected end overflows, push start earlier
    if projected_end > day_end {
        let excess_minutes = (projected_end - day_end).num_minutes();
        cursor = default_start - chrono::Duration::minutes(excess_minutes);
        if cursor < day_start {
            cursor = day_start;
        }
    }

    let mut lunch_inserted = false;

    // Sort entries by task_id for consistent order
    let mut sorted_entries: Vec<&external_models::TimeEntryDto> = day_entries.to_vec();
    sorted_entries.sort_by_key(|e| e.task_id);

    for entry in sorted_entries {
        let found_task = external_tasks.iter().find(|t| t.id == entry.task_id);

        let task_name = found_task
            .map(|t| t.name.clone())
            .unwrap_or_else(|| format!("Task {}", entry.task_id));

        let project_name = found_task
            .map(|t| t.project.name.clone())
            .unwrap_or_default();

        let rate = found_task.map(|t| t.compensation_rate).unwrap_or(0.0);

        let mut remaining_minutes = (entry.value * 60.0).round() as i64;

        while remaining_minutes > 0 {
            let time_left_in_day = (day_end - cursor).num_minutes();
            if time_left_in_day <= 0 {
                // Cannot add more; truncate remaining
                break;
            }

            // Attempt to insert lunch if conditions met
            if !lunch_inserted
                && cursor < lunch_start
                && cursor + chrono::Duration::minutes(remaining_minutes) > lunch_start
            {
                // Add work before lunch
                let mins_to_lunch = (lunch_start - cursor).num_minutes();
                if mins_to_lunch > 0 {
                    let pre_lunch_end = cursor + chrono::Duration::minutes(mins_to_lunch);
                    events.push(Event::TaskStarted {
                        id: entry.task_id,
                        name: task_name.clone(),
                        project_name: project_name.clone(),
                        rate,
                        start_time: cursor,
                        is_generated: true,
                    });
                    events.push(Event::Stopped {
                        end_time: pre_lunch_end,
                        is_generated: true,
                    });
                    remaining_minutes -= mins_to_lunch;
                }

                // Insert lunch break
                events.push(Event::BreakStarted {
                    start_time: lunch_start,
                    is_generated: true,
                });
                events.push(Event::Stopped {
                    end_time: lunch_end,
                    is_generated: true,
                });
                cursor = lunch_end;
                lunch_inserted = true;
                continue;
            }

            // Add task segment
            let mins_to_add = remaining_minutes.min(time_left_in_day);
            let segment_end = cursor + chrono::Duration::minutes(mins_to_add);
            events.push(Event::TaskStarted {
                id: entry.task_id,
                name: task_name.clone(),
                project_name: project_name.clone(),
                rate,
                start_time: cursor,
                is_generated: true,
            });
            events.push(Event::Stopped {
                end_time: segment_end,
                is_generated: true,
            });
            cursor = segment_end;
            remaining_minutes -= mins_to_add;
        }
    }

    events
}

fn insert_and_resolve_overlaps(
    tasks: &mut Vec<models::Task>,
    new_entry: models::Task,
) {
    let mut result = Vec::new();
    let n_start = new_entry.start_time;
    // If end time is missing (running task), treat it as 'now' for overlap logic
    let n_end = new_entry.end_time.unwrap_or_else(Local::now);

    // We use drain to move out all existing tasks, filter/modify them, and put back into result
    for p in tasks.drain(..) {
        let p_start = p.start_time;
        let p_end = p.end_time.unwrap_or_else(Local::now);

        // Case 0: No overlap
        // Existing: |---|
        // New:            |---|
        // OR
        // New:      |---|
        // Existing:       |---|
        if p_end <= n_start || p_start >= n_end {
            result.push(p);
            continue;
        }

        // Overlap detected. The new entry "wins". We keep parts of 'p' that are outside 'new'.

        // 1. Keep part of p strictly before n
        if p_start < n_start {
            let mut left = p.clone();
            left.end_time = Some(n_start);
            result.push(left);
        }

        // 2. Keep part of p strictly after n
        if p_end > n_end {
            let mut right = p.clone();
            right.start_time = n_end;
            // right.end_time remains as p.end_time
            result.push(right);
        }

        // If p is fully inside n, both checks fail and p is dropped.
    }

    result.push(new_entry);
    result.sort_by_key(|p| p.start_time);
    *tasks = result;
}

fn parse_time(date: NaiveDate, time_str: &str) -> Option<chrono::DateTime<Local>> {
    let parts: Vec<&str> = time_str.trim().split(':').collect();
    if parts.len() != 2 {
        return None;
    }

    let hour = parts[0].parse::<u32>().ok()?;
    let min = parts[1].parse::<u32>().ok()?;

    let naive = date.and_hms_opt(hour, min, 0)?;
    Some(naive.and_local_timezone(Local).unwrap())
}