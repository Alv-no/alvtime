mod alvtime;
mod config;
mod events;
mod external_models;
mod input_helper;
mod models;
mod projector;
mod store;
mod utils;
mod view;
mod handlers;

use chrono::{Datelike, Local, NaiveDate};
use events::Event;
use input_helper::InputHelper;
use rustyline::error::ReadlineError;
use rustyline::Editor;
use store::EventStore;
use view::ViewMode;
use handlers::{add_event, handle_config, handle_edit, handle_favorites, handle_push, handle_start, handle_sync, process_autobreak, get_help_text};

fn main() {
    let mut app_config = config::Config::load();
    let event_store = EventStore::new(&app_config.storage_path);

    // Setup Client
    let client = alvtime::AlvtimeClient::new(
        app_config.api_url.clone(),
        app_config.personal_token.clone(),
    );

    // Fetch tasks
    let external_tasks = client.list_tasks().unwrap_or_else(|e| {
        eprintln!("Warning: Failed to fetch tasks from API: {}", e);
        Vec::new()
    })
        ;
    // Check and fetch holidays
    let current_year = Local::now().year();
    if !event_store.has_cached_holidays(current_year) && app_config.personal_token.is_some() {
        match client.list_bank_holidays(current_year, current_year) {
            Ok(holidays) => {
                event_store.save_holidays(current_year, &holidays);
                println!("Updated holidays for {}.", current_year);
            }
            Err(e) => eprintln!("Failed to fetch holidays: {}", e),
        }
    }

    // Load holidays into memory for the view
    let holidays = event_store.get_holidays();

    // 1. Load History (Current Day)
    let today = Local::now().date_naive();
    let mut today_history = event_store.events_for_day(today);

    // 2. Rebuild State (Current Day)
    let mut today_tasks = projector::restore_state(&today_history);

    let mut feedback = format!(
        "Type 'help' for commands. Autobreak is {}.",
        if app_config.autobreak { "ON" } else { "OFF" }
    );

    // We need to map the favorites (which are IDs) to strings for the helper if needed,
    // but InputHelper was updated to take Vec<i32> for favorites.
    // Ensure InputHelper definition in input_helper.rs matches this usage.
    let helper = InputHelper {
        commands: vec![
            "start".to_string(),
            "break".to_string(),
            "stop".to_string(),
            "view".to_string(),
            "sync".to_string(),
            "push".to_string(),
            "undo".to_string(),
            "redo".to_string(),
            "help".to_string(),
            "config".to_string(),
            "favorites".to_string(),
            "edit".to_string(),
            "quit".to_string(),
        ],
        tasks: external_tasks.clone(),
        favorites: app_config.favorite_tasks.clone(),
    };

    let mut rl = Editor::new().expect("Failed to initialize readline");
    rl.set_helper(Some(helper));

    let mut current_mode = ViewMode::Month;

    loop {
        // Handle day change logic? (Simplistic approach: if date changed, reset today vars)
        let now = Local::now();
        if now.date_naive() != today {
            // Ideally restart main or update refs, but for now we assume single-session-day usage or restart
        }

        // --- Autobreak Logic ---
        if app_config.autobreak {
            if let Some(fb) = process_autobreak(&mut today_tasks, &mut today_history, &event_store) {
                feedback = fb;
            }
        }

        print!("\x1b[2J\x1b[1;1H"); // Clear screen

        // Build tasks for view
        let tasks_to_view = match current_mode {
            ViewMode::Day => today_tasks.clone(),
            ViewMode::Week => {
                let mut all_tasks = Vec::new();
                let start_of_week = now.date_naive() - chrono::Duration::days(now.weekday().num_days_from_monday() as i64);
                for i in 0..7 {
                    let d = start_of_week + chrono::Duration::days(i);
                    let events = event_store.events_for_day(d);
                    all_tasks.extend(projector::restore_state(&events));
                }
                all_tasks
            },
            ViewMode::Month => {
                let mut all_tasks = Vec::new();
                let start_of_month = NaiveDate::from_ymd_opt(now.year(), now.month(), 1).unwrap();
                let mut d = start_of_month;
                while d.month() == now.month() {
                    let events = event_store.events_for_day(d);
                    all_tasks.extend(projector::restore_state(&events));
                    d += chrono::Duration::days(1);
                }
                all_tasks
            },
            ViewMode::Year => {
                let mut all_tasks = Vec::new();
                let start_of_year = NaiveDate::from_ymd_opt(now.year(), 1, 1).unwrap();
                let mut d = start_of_year;
                while d.year() == now.year() {
                    let events = event_store.events_for_day(d);
                    all_tasks.extend(projector::restore_state(&events));
                    d += chrono::Duration::days(1);
                }
                all_tasks
            }
        };

        view::draw_timeline(&tasks_to_view, &current_mode, &holidays);

        if !feedback.is_empty() {
            println!("\n{}", feedback);
            feedback.clear();
        }

        let readline = rl.readline("> ");
        match readline {
            Ok(line) => {
                let input = line.trim();
                if input.is_empty() {
                    continue;
                }

                let _ = rl.add_history_entry(input);
                let parts: Vec<&str> = input.split_whitespace().collect();

                match parts[0] {
                    "start" => {
                        feedback = handle_start(
                            &parts,
                            &mut today_tasks,
                            &mut today_history,
                            &event_store,
                            &app_config,
                            &external_tasks,
                        );
                    }
                    "favorites" => {
                        feedback = handle_favorites(
                            &parts,
                            &mut app_config,
                            &external_tasks,
                            &mut rl,
                        );
                    }
                    "break" => {
                        feedback = add_event(&mut today_tasks, &mut today_history, &event_store, Event::BreakStarted { start_time: Local::now(), is_generated: false }, "Break started.");
                    }
                    "stop" => {
                        let now = Local::now();
                        let rounded_end = utils::round_ceil_quarter(now);
                        feedback = add_event(&mut today_tasks, &mut today_history, &event_store, Event::Stopped { end_time: rounded_end, is_generated: false }, "Stopped.");
                    }
                    "view" => {
                        current_mode = if parts.len() > 1 {
                            match parts[1] {
                                "week" | "w" => ViewMode::Week,
                                "month" | "m" => ViewMode::Month,
                                "year" | "y" => ViewMode::Year,
                                _ => ViewMode::Day,
                            }
                        } else {
                            ViewMode::Month
                        };
                    }
                    "undo" => {
                        feedback = add_event(&mut today_tasks, &mut today_history, &event_store, Event::Undo { time: Local::now() }, "Undone.");
                    }
                    "redo" => {
                        feedback = add_event(&mut today_tasks, &mut today_history, &event_store, Event::Redo { time: Local::now() }, "Redone.");
                    }
                    "sync" => {
                        feedback = handle_sync(
                            &client,
                            &mut today_tasks,
                            &mut today_history,
                            &event_store,
                            &external_tasks,
                        );
                    }
                    "push" => {
                        feedback = handle_push(
                            &client,
                            &mut today_tasks,
                            &mut today_history,
                            &event_store,
                            &external_tasks,
                        );
                    }
                    "edit" => {
                        feedback = handle_edit(
                            &mut today_tasks,
                            &mut today_history,
                            &event_store,
                            &external_tasks,
                            &app_config,
                        );
                    }
                    "config" => {
                        feedback = handle_config(&parts, &mut app_config);
                        // If token was set, break inner loop to re-initialize client in outer loop
                        if parts.len() >= 2 && parts[1] == "set-token" {
                            println!("Token saved. Reloading session...");
                            break;
                        }
                    }
                    "help" => {
                        feedback = get_help_text();
                    }
                    "quit" => break,
                    _ => {
                        feedback = "Unknown command. Type 'help' for list of commands.".to_string()
                    }
                }
            }
            Err(ReadlineError::Interrupted) => {
                println!("Exited");
                break;
            }
            Err(ReadlineError::Eof) => {
                println!("Exited");
                break;
            }
            Err(err) => {
                println!("Error: {:?}", err);
                break;
            }
        }
    }
}