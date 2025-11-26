use crate::actions::utils::{insert_and_resolve_overlaps, parse_time};
use crate::events::Event;
use crate::external_models::TaskDto;
use crate::store::EventStore;
use crate::{config, models, projector, view};
use chrono::{Datelike, Local, NaiveDate};
use inquire::{Select, Text};

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

    let date_options: Vec<String> = dates
        .iter()
        .map(|d| d.format("%Y-%m-%d").to_string())
        .collect();

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
            let end_str = p
                .end_time
                .map(|t| t.format("%H:%M").to_string())
                .unwrap_or("...".to_string());
            println!(
                "{}. [{}-{}] {}",
                i + 1,
                p.start_time.format("%H:%M"),
                end_str,
                p.name
            );
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
                // Pick Task - Filter only Favorites
                let favorite_tasks: Vec<&TaskDto> = external_tasks
                    .iter()
                    .filter(|t| app_config.favorite_tasks.contains(&t.id))
                    .collect();

                let task_options: Vec<String> =
                    favorite_tasks.iter().map(|t| t.to_string()).collect();

                // Also add "Break"
                let mut combined_options = vec!["Break".to_string()];
                combined_options.extend(task_options);

                let task_choice =
                    match Select::new("Select Task/Break (Favorites only):", combined_options)
                        .prompt()
                    {
                        Ok(t) => t,
                        Err(_) => continue,
                    };

                let (name, id, is_break, project_name, rate) = if task_choice == "Break" {
                    ("Break".to_string(), -1, true, "".to_string(), 0.0)
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
                        t.compensation_rate,
                    )
                };

                // Ask Start Time
                let start_str = match Text::new("Start Time (HH:MM):").prompt() {
                    Ok(s) => s,
                    Err(_) => continue,
                };
                let start_time = match parse_time(selected_date, &start_str) {
                    Some(t) => t,
                    None => {
                        println!("Invalid time format.");
                        continue;
                    }
                };

                // Ask End Time
                let end_str = match Text::new("End Time (HH:MM):").prompt() {
                    Ok(s) => s,
                    Err(_) => continue,
                };
                let end_time = match parse_time(selected_date, &end_str) {
                    Some(t) => t,
                    None => {
                        println!("Invalid time format.");
                        continue;
                    }
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
                    comment: None,
                    is_break,
                    is_generated: false,
                };

                insert_and_resolve_overlaps(&mut day_tasks, new_p);
            }
            "Edit Entry Time" => {
                if day_tasks.is_empty() {
                    continue;
                }
                let indices: Vec<String> = (1..=day_tasks.len()).map(|i| i.to_string()).collect();
                let idx_res = Select::new("Select entry number:", indices).prompt();
                if let Ok(idx_str) = idx_res {
                    let idx = idx_str.parse::<usize>().unwrap() - 1;
                    // Remove temporarily to resolve overlaps against others
                    let mut p = day_tasks.remove(idx);

                    let s_default = p.start_time.format("%H:%M").to_string();
                    let e_default = p
                        .end_time
                        .map(|t| t.format("%H:%M").to_string())
                        .unwrap_or_default();

                    let start_res = Text::new("Start Time:").with_default(&s_default).prompt();
                    let end_res = Text::new("End Time:").with_default(&e_default).prompt();

                    if let (Ok(s), Ok(e)) = (start_res, end_res) {
                        let start = match parse_time(selected_date, s.trim()) {
                            Some(t) => t,
                            None => {
                                println!("Invalid start time format.");
                                day_tasks.insert(idx, p);
                                continue;
                            }
                        };

                        let end = {
                            let trimmed = e.trim();
                            if trimmed.is_empty() {
                                None
                            } else if let Some(parsed) = parse_time(selected_date, trimmed) {
                                Some(parsed)
                            } else {
                                println!("Invalid end time format.");
                                day_tasks.insert(idx, p);
                                continue;
                            }
                        };

                        if let Some(end_time) = end {
                            if end_time <= start {
                                println!("Error: End time must be after start time.");
                                day_tasks.insert(idx, p);
                                continue;
                            }
                        }

                        p.start_time = start;
                        p.end_time = end;
                        insert_and_resolve_overlaps(&mut day_tasks, p);
                    } else {
                        day_tasks.insert(idx, p); // Restore on cancel
                    }
                }
            }
            "Edit Entry Type" => {
                if day_tasks.is_empty() {
                    continue;
                }

                let indices: Vec<String> = (1..=day_tasks.len()).map(|i| i.to_string()).collect();

                let idx = match Select::new("Select entry number:", indices).prompt() {
                    Ok(choice) => choice.parse::<usize>().unwrap() - 1,
                    Err(_) => continue,
                };

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

                if let Some(entry) = day_tasks.get_mut(idx) {
                    let selected_label = type_choice.as_str();

                    if selected_label == "Break" {
                        entry.id = -1;
                        entry.name = "Break".to_string();
                        entry.project_name.clear();
                        entry.rate = 0.0;
                        entry.is_break = true;
                    } else if let Some(new_task) = favorite_tasks
                        .iter()
                        .find(|t| t.to_string() == selected_label)
                    {
                        entry.id = new_task.id;
                        entry.name = new_task.name.clone();
                        entry.project_name = new_task.project.name.clone();
                        entry.rate = new_task.compensation_rate;
                        entry.is_break = false;
                        entry.comment = None;
                        entry.is_generated = false;
                    }
                }
            }
            "Delete Entry" => {
                if day_tasks.is_empty() {
                    continue;
                }
                let indices: Vec<String> = (1..=day_tasks.len()).map(|i| i.to_string()).collect();
                let idx_res = Select::new("Select entry number:", indices).prompt();
                if let Ok(idx_str) = idx_res {
                    let idx = idx_str.parse::<usize>().unwrap() - 1;
                    let removed = day_tasks.remove(idx);

                    // 1. If deleting a break, extend previous task to fill gap
                    if removed.is_break && idx > 0 {
                        if let Some(prev_task) = day_tasks.get_mut(idx - 1) {
                            prev_task.end_time = removed.end_time;
                        }
                    }
                    // 2. If deleting a task and it has tasks on both sides, insert a break
                    else if !removed.is_break && idx > 0 && idx < day_tasks.len() {
                        if let Some(end_time) = removed.end_time {
                            let break_task = models::Task {
                                id: -1,
                                name: "Break".to_string(),
                                project_name: "".to_string(),
                                rate: 0.0,
                                start_time: removed.start_time,
                                end_time: Some(end_time),
                                comment: None,
                                is_break: true,
                                is_generated: false,
                            };
                            day_tasks.insert(idx, break_task);
                        }
                    }
                }
            }
            "Save & Finish" => {
                if selected_date < today {
                    if let Some(open_task) = day_tasks.iter().find(|task| task.end_time.is_none()) {
                        eprintln!(
                            "Cannot save: the entry '{}' has no end time. \
                             Finish all tasks when editing past days.",
                            open_task.name
                        );
                        continue;
                    }
                }

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
                        new_events.push(Event::BreakStarted {
                            start_time: p.start_time,
                            is_generated: false,
                        });
                    } else {
                        new_events.push(Event::TaskStarted {
                            id: p.id,
                            name: p.name,
                            project_name: p.project_name,
                            rate: p.rate,
                            start_time: p.start_time,
                            is_generated: false,
                        });
                    }
                    if let Some(end) = p.end_time {
                        new_events.push(Event::Stopped {
                            end_time: end,
                            is_generated: false,
                        });
                        last_end_time = Some(end);
                    } else {
                        last_end_time = None;
                    }
                }

                let revised_event = Event::DayRevised {
                    date: selected_date,
                    events: new_events,
                };

                // Persist
                event_store.persist(&revised_event);

                // If we edited today, we should update the in-memory state passed to this function
                if selected_date == Local::now().date_naive() {
                    history.push(revised_event);
                    *tasks = projector::restore_state(history);
                }

                return "Day updated.".to_string();
            }
            "Cancel" => return "Cancelled.".to_string(),
            _ => {}
        }
    }
}

#[cfg(test)]
mod edit_tests {
    use super::*;
    use crate::actions::utils::{insert_and_resolve_overlaps, parse_time};
    use crate::events::Event;
    use crate::external_models::{CustomerDto, TaskDto};
    use crate::{config, models, projector};
    use chrono::{Local, NaiveDate, TimeZone, Timelike};

    // Helper function to create a test date
    fn test_date() -> NaiveDate {
        NaiveDate::from_ymd_opt(2024, 6, 15).unwrap()
    }

    // Helper function to create test datetime
    fn test_datetime(hour: u32, minute: u32) -> chrono::DateTime<Local> {
        Local
            .from_local_datetime(&test_date().and_hms_opt(hour, minute, 0).unwrap())
            .unwrap()
    }

    // Helper to create a test task
    fn create_test_task(
        id: i32,
        name: &str,
        start_hour: u32,
        start_min: u32,
        end_hour: u32,
        end_min: u32,
        is_break: bool,
    ) -> models::Task {
        models::Task {
            id,
            name: name.to_string(),
            project_name: "Test Project".to_string(),
            rate: 100.0,
            start_time: test_datetime(start_hour, start_min),
            end_time: Some(test_datetime(end_hour, end_min)),
            comment: None,
            is_break,
            is_generated: false,
        }
    }

    // Helper to create test config with favorites
    fn create_test_config(favorite_ids: Vec<i32>) -> config::Config {
        config::Config {
            favorite_tasks: favorite_ids,
            ..Default::default()
        }
    }

    // Helper to create test external tasks
    fn create_test_external_tasks() -> Vec<TaskDto> {
        vec![
            TaskDto {
                id: 1,
                name: "Task A".to_string(),
                project: crate::external_models::ProjectDto {
                    name: "Project Alpha".to_string(),
                    customer: CustomerDto {
                        name: "Test Customer".to_string(),
                    },
                },
                compensation_rate: 1.0,
                locked: false,
            },
            TaskDto {
                id: 2,
                name: "Task B".to_string(),
                project: crate::external_models::ProjectDto {
                    name: "Project Beta".to_string(),
                    customer: CustomerDto {
                        name: "Test Customer".to_string(),
                    },
                },
                compensation_rate: 1.5,
                locked: false,
            },
            TaskDto {
                id: 3,
                name: "Task C".to_string(),
                project: crate::external_models::ProjectDto {
                    name: "Project Gamma".to_string(),
                    customer: CustomerDto {
                        name: "Test Customer".to_string(),
                    },
                },
                compensation_rate: 2.0,
                locked: false,
            },
        ]
    }

    #[test]
    fn test_insert_and_resolve_overlaps_no_overlap() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(2, "Task 2", 11, 0, 12, 0, false),
        ];

        let new_task = create_test_task(3, "Task 3", 10, 0, 11, 0, false);
        insert_and_resolve_overlaps(&mut tasks, new_task);

        assert_eq!(tasks.len(), 3);
        tasks.sort_by_key(|t| t.start_time);
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[1].name, "Task 3");
        assert_eq!(tasks[2].name, "Task 2");
    }

    #[test]
    fn test_insert_and_resolve_overlaps_complete_overlap() {
        let mut tasks = vec![create_test_task(1, "Task 1", 9, 0, 12, 0, false)];

        // New task completely overlaps Task 1
        let new_task = create_test_task(2, "Task 2", 10, 0, 11, 0, false);
        insert_and_resolve_overlaps(&mut tasks, new_task);

        assert_eq!(tasks.len(), 3);
        tasks.sort_by_key(|t| t.start_time);

        // Task 1 should be split
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[0].start_time.time().hour(), 9);
        assert_eq!(tasks[0].end_time.unwrap().time().hour(), 10);

        assert_eq!(tasks[1].name, "Task 2");
        assert_eq!(tasks[1].start_time.time().hour(), 10);
        assert_eq!(tasks[1].end_time.unwrap().time().hour(), 11);

        assert_eq!(tasks[2].name, "Task 1");
        assert_eq!(tasks[2].start_time.time().hour(), 11);
        assert_eq!(tasks[2].end_time.unwrap().time().hour(), 12);
    }

    #[test]
    fn test_insert_and_resolve_overlaps_partial_overlap_start() {
        let mut tasks = vec![create_test_task(1, "Task 1", 10, 0, 12, 0, false)];

        // New task overlaps start of Task 1
        let new_task = create_test_task(2, "Task 2", 9, 0, 11, 0, false);
        insert_and_resolve_overlaps(&mut tasks, new_task);

        assert_eq!(tasks.len(), 2);
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks[0].name, "Task 2");
        assert_eq!(tasks[0].start_time.time().hour(), 9);
        assert_eq!(tasks[0].end_time.unwrap().time().hour(), 11);

        // Task 1 should be trimmed
        assert_eq!(tasks[1].name, "Task 1");
        assert_eq!(tasks[1].start_time.time().hour(), 11);
        assert_eq!(tasks[1].end_time.unwrap().time().hour(), 12);
    }

    #[test]
    fn test_insert_and_resolve_overlaps_partial_overlap_end() {
        let mut tasks = vec![create_test_task(1, "Task 1", 9, 0, 11, 0, false)];

        // New task overlaps end of Task 1
        let new_task = create_test_task(2, "Task 2", 10, 0, 12, 0, false);
        insert_and_resolve_overlaps(&mut tasks, new_task);

        assert_eq!(tasks.len(), 2);
        tasks.sort_by_key(|t| t.start_time);

        // Task 1 should be trimmed
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[0].start_time.time().hour(), 9);
        assert_eq!(tasks[0].end_time.unwrap().time().hour(), 10);

        assert_eq!(tasks[1].name, "Task 2");
        assert_eq!(tasks[1].start_time.time().hour(), 10);
        assert_eq!(tasks[1].end_time.unwrap().time().hour(), 12);
    }

    #[test]
    fn test_insert_and_resolve_overlaps_multiple_overlaps() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(2, "Task 2", 10, 0, 11, 0, false),
            create_test_task(3, "Task 3", 11, 0, 12, 0, false),
        ];

        // New task overlaps all three
        let new_task = create_test_task(4, "Task 4", 9, 30, 11, 30, false);
        insert_and_resolve_overlaps(&mut tasks, new_task);

        tasks.sort_by_key(|t| t.start_time);

        // Task 1 should be trimmed
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(
            tasks[0].end_time.unwrap().time(),
            chrono::NaiveTime::from_hms_opt(9, 30, 0).unwrap()
        );

        // Task 4 should be inserted
        let task_4 = tasks.iter().find(|t| t.name == "Task 4").unwrap();
        assert_eq!(
            task_4.start_time.time(),
            chrono::NaiveTime::from_hms_opt(9, 30, 0).unwrap()
        );
        assert_eq!(
            task_4.end_time.unwrap().time(),
            chrono::NaiveTime::from_hms_opt(11, 30, 0).unwrap()
        );

        // Task 3 should be trimmed
        let task_3 = tasks.iter().find(|t| t.name == "Task 3").unwrap();
        assert_eq!(
            task_3.start_time.time(),
            chrono::NaiveTime::from_hms_opt(11, 30, 0).unwrap()
        );
    }

    #[test]
    fn test_insert_and_resolve_overlaps_with_breaks() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(-1, "Break", 10, 0, 10, 30, true),
            create_test_task(2, "Task 2", 10, 30, 12, 0, false),
        ];

        // New task overlaps break and part of Task 2
        let new_task = create_test_task(3, "Task 3", 10, 0, 11, 0, false);
        insert_and_resolve_overlaps(&mut tasks, new_task);

        tasks.sort_by_key(|t| t.start_time);

        // Break should be removed (completely overlapped)
        assert!(!tasks.iter().any(|t| t.is_break));

        // Task 3 should be inserted
        assert!(tasks.iter().any(|t| t.name == "Task 3"));

        // Task 2 should be trimmed
        let task_2 = tasks.iter().find(|t| t.name == "Task 2").unwrap();
        assert_eq!(task_2.start_time.time().hour(), 11);
    }

    #[test]
    fn test_parse_time_valid_formats() {
        let date = test_date();

        // Test HH:MM format
        let result = parse_time(date, "09:30");
        assert!(result.is_some());
        let dt = result.unwrap();
        assert_eq!(dt.time().hour(), 9);
        assert_eq!(dt.time().minute(), 30);

        // Test single digit hour
        let result = parse_time(date, "9:30");
        assert!(result.is_some());
        let dt = result.unwrap();
        assert_eq!(dt.time().hour(), 9);

        // Test midnight
        let result = parse_time(date, "00:00");
        assert!(result.is_some());
        let dt = result.unwrap();
        assert_eq!(dt.time().hour(), 0);

        // Test end of day
        let result = parse_time(date, "23:59");
        assert!(result.is_some());
        let dt = result.unwrap();
        assert_eq!(dt.time().hour(), 23);
        assert_eq!(dt.time().minute(), 59);
    }

    #[test]
    fn test_parse_time_invalid_formats() {
        let date = test_date();

        // Invalid format
        assert!(parse_time(date, "9:30am").is_none());
        assert!(parse_time(date, "25:00").is_none());
        assert!(parse_time(date, "12:60").is_none());
        assert!(parse_time(date, "abc").is_none());
        assert!(parse_time(date, "").is_none());
        assert!(parse_time(date, "12").is_none());
    }

    #[test]
    fn test_day_revised_event_generation() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(-1, "Break", 10, 0, 10, 30, true),
            create_test_task(2, "Task 2", 10, 30, 12, 0, false),
        ];

        tasks.sort_by_key(|p| p.start_time);
        let mut new_events = Vec::new();
        let mut last_end_time: Option<chrono::DateTime<Local>> = None;

        for p in tasks {
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
                new_events.push(Event::BreakStarted {
                    start_time: p.start_time,
                    is_generated: false,
                });
            } else {
                new_events.push(Event::TaskStarted {
                    id: p.id,
                    name: p.name,
                    project_name: p.project_name,
                    rate: p.rate,
                    start_time: p.start_time,
                    is_generated: false,
                });
            }

            if let Some(end) = p.end_time {
                new_events.push(Event::Stopped {
                    end_time: end,
                    is_generated: false,
                });
                last_end_time = Some(end);
            }
        }

        // Should have: TaskStarted, Stopped, BreakStarted, Stopped, TaskStarted, Stopped
        assert_eq!(new_events.len(), 6);

        // Verify event sequence
        assert!(matches!(new_events[0], Event::TaskStarted { .. }));
        assert!(matches!(new_events[1], Event::Stopped { .. }));
        assert!(matches!(new_events[2], Event::BreakStarted { .. }));
        assert!(matches!(new_events[3], Event::Stopped { .. }));
        assert!(matches!(new_events[4], Event::TaskStarted { .. }));
        assert!(matches!(new_events[5], Event::Stopped { .. }));
    }

    #[test]
    fn test_day_revised_event_with_gaps() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            // Gap from 10:00 to 11:00
            create_test_task(2, "Task 2", 11, 0, 12, 0, false),
        ];

        tasks.sort_by_key(|p| p.start_time);
        let mut new_events = Vec::new();
        let mut last_end_time: Option<chrono::DateTime<Local>> = None;

        for p in tasks {
            if let Some(last_end) = last_end_time {
                if p.start_time > last_end {
                    // Gap detected - insert generated break
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

            new_events.push(Event::TaskStarted {
                id: p.id,
                name: p.name.clone(),
                project_name: p.project_name.clone(),
                rate: p.rate,
                start_time: p.start_time,
                is_generated: false,
            });

            if let Some(end) = p.end_time {
                new_events.push(Event::Stopped {
                    end_time: end,
                    is_generated: false,
                });
                last_end_time = Some(end);
            }
        }

        // Should have: TaskStarted, Stopped, BreakStarted (generated), Stopped (generated), TaskStarted, Stopped
        assert_eq!(new_events.len(), 6);

        // Verify the generated break
        if let Event::BreakStarted { is_generated, .. } = new_events[2] {
            assert!(is_generated);
        } else {
            panic!("Expected generated BreakStarted event");
        }
    }

    #[test]
    fn test_delete_break_extends_previous_task() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(-1, "Break", 10, 0, 10, 30, true),
            create_test_task(2, "Task 2", 10, 30, 12, 0, false),
        ];

        // Simulate deleting the break at index 1
        let idx = 1;
        let removed = tasks.remove(idx);

        if removed.is_break && idx > 0 {
            if let Some(prev_task) = tasks.get_mut(idx - 1) {
                prev_task.end_time = removed.end_time;
            }
        }

        // Task 1 should now extend to 10:30
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(
            tasks[0].end_time.unwrap().time(),
            chrono::NaiveTime::from_hms_opt(10, 30, 0).unwrap()
        );
        assert_eq!(tasks.len(), 2);
    }

    #[test]
    fn test_delete_task_inserts_break_when_between_tasks() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(2, "Task 2", 10, 0, 11, 0, false),
            create_test_task(3, "Task 3", 11, 0, 12, 0, false),
        ];

        // Simulate deleting Task 2 at index 1
        let idx = 1;
        let removed = tasks.remove(idx);

        // Should insert break between Task 1 and Task 3
        if !removed.is_break && idx > 0 && idx < tasks.len() {
            if let Some(end_time) = removed.end_time {
                let break_task = models::Task {
                    id: -1,
                    name: "Break".to_string(),
                    project_name: "".to_string(),
                    rate: 0.0,
                    start_time: removed.start_time,
                    end_time: Some(end_time),
                    comment: None,
                    is_break: true,
                    is_generated: false,
                };
                tasks.insert(idx, break_task);
            }
        }

        assert_eq!(tasks.len(), 3);
        assert_eq!(tasks[1].is_break, true);
        assert_eq!(tasks[1].start_time.time().hour(), 10);
        assert_eq!(tasks[1].end_time.unwrap().time().hour(), 11);
    }

    #[test]
    fn test_delete_first_task_no_break_insertion() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(2, "Task 2", 10, 0, 11, 0, false),
        ];

        let idx = 0;
        let removed = tasks.remove(idx);

        // No break should be inserted since idx is 0
        if !removed.is_break && idx > 0 && idx < tasks.len() {
            // This block shouldn't execute
            panic!("Should not insert break for first task");
        }

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].name, "Task 2");
    }

    #[test]
    fn test_delete_last_task_no_break_insertion() {
        let mut tasks = vec![
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(2, "Task 2", 10, 0, 11, 0, false),
        ];

        let idx = 1;
        let removed = tasks.remove(idx);

        // No break should be inserted since we're at the end
        if !removed.is_break && idx > 0 && idx < tasks.len() {
            // This block shouldn't execute
            panic!("Should not insert break for last task");
        }

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].name, "Task 1");
    }

    #[test]
    fn test_favorite_tasks_filtering() {
        let all_tasks = create_test_external_tasks();
        let config = create_test_config(vec![1, 3]); // Only Task A and Task C are favorites

        let favorite_tasks: Vec<&TaskDto> = all_tasks
            .iter()
            .filter(|t| config.favorite_tasks.contains(&t.id))
            .collect();

        assert_eq!(favorite_tasks.len(), 2);
        assert!(favorite_tasks.iter().any(|t| t.id == 1));
        assert!(favorite_tasks.iter().any(|t| t.id == 3));
        assert!(!favorite_tasks.iter().any(|t| t.id == 2));
    }

    #[test]
    fn test_task_sorting_by_start_time() {
        let mut tasks = vec![
            create_test_task(3, "Task 3", 11, 0, 12, 0, false),
            create_test_task(1, "Task 1", 9, 0, 10, 0, false),
            create_test_task(2, "Task 2", 10, 0, 11, 0, false),
        ];

        tasks.sort_by_key(|p| p.start_time);

        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[1].name, "Task 2");
        assert_eq!(tasks[2].name, "Task 3");
    }

    #[test]
    fn test_projector_restore_state_from_events() {
        let events = vec![
            Event::TaskStarted {
                id: 1,
                name: "Task 1".to_string(),
                project_name: "Project A".to_string(),
                rate: 100.0,
                start_time: test_datetime(9, 0),
                is_generated: false,
            },
            Event::Stopped {
                end_time: test_datetime(10, 0),
                is_generated: false,
            },
            Event::BreakStarted {
                start_time: test_datetime(10, 0),
                is_generated: false,
            },
            Event::Stopped {
                end_time: test_datetime(10, 30),
                is_generated: false,
            },
        ];

        let tasks = projector::restore_state(&events);

        assert_eq!(tasks.len(), 2);
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[0].start_time.time().hour(), 9);
        assert_eq!(tasks[0].end_time.unwrap().time().hour(), 10);
        assert_eq!(tasks[1].is_break, true);
    }

    #[test]
    fn test_empty_task_list_handling() {
        let tasks: Vec<models::Task> = vec![];

        // Should handle empty list gracefully
        assert_eq!(tasks.len(), 0);
        assert!(tasks.is_empty());
    }

    #[test]
    fn test_task_with_no_end_time() {
        let task = models::Task {
            id: 1,
            name: "Ongoing Task".to_string(),
            project_name: "Project A".to_string(),
            rate: 100.0,
            start_time: test_datetime(9, 0),
            end_time: None,
            comment: None,
            is_break: false,
            is_generated: false,
        };

        assert!(task.end_time.is_none());
        assert_eq!(task.name, "Ongoing Task");
    }

    #[test]
    fn test_date_selection_range() {
        let current_year = 2024;
        let mut dates = Vec::new();
        let mut d = NaiveDate::from_ymd_opt(current_year, 1, 1).unwrap();

        while d.year() == current_year {
            dates.push(d);
            d += chrono::Duration::days(1);
        }

        // Should have 366 days for 2024 (leap year)
        assert_eq!(dates.len(), 366);
        assert_eq!(dates[0], NaiveDate::from_ymd_opt(2024, 1, 1).unwrap());
        assert_eq!(dates[365], NaiveDate::from_ymd_opt(2024, 12, 31).unwrap());
    }

    #[test]
    fn test_date_selection_today_default() {
        let now = Local::now();
        let today = now.date_naive();
        let current_year = now.year();

        let mut dates = Vec::new();
        let mut d = NaiveDate::from_ymd_opt(current_year, 1, 1).unwrap();

        while d.year() == current_year {
            dates.push(d);
            d += chrono::Duration::days(1);
        }

        dates.reverse(); // Newest first

        let default_idx = dates.iter().position(|d| *d == today);
        assert!(default_idx.is_some());
    }

    #[test]
    fn test_task_dto_display_format() {
        let task = TaskDto {
            id: 1,
            name: "Test Task".to_string(),
            project: crate::external_models::ProjectDto {
                name: "Test Project".to_string(),
                customer: CustomerDto {
                    name: "Test Customer".to_string(),
                },
            },
            compensation_rate: 1.5,
            locked: false,
        };

        let display = task.to_string();
        assert!(display.contains("Test Task"));
        assert!(display.contains("Test Project"));
    }

    #[test]
    fn test_break_task_creation() {
        let break_task = models::Task {
            id: -1,
            name: "Break".to_string(),
            project_name: "".to_string(),
            rate: 0.0,
            start_time: test_datetime(10, 0),
            end_time: Some(test_datetime(10, 30)),
            comment: None,
            is_break: true,
            is_generated: false,
        };

        assert_eq!(break_task.id, -1);
        assert_eq!(break_task.name, "Break");
        assert!(break_task.is_break);
        assert_eq!(break_task.rate, 0.0);
        assert_eq!(break_task.project_name, "");
    }

    #[test]
    fn test_overlap_resolution_preserves_task_data() {
        let mut tasks = vec![models::Task {
            id: 1,
            name: "Important Task".to_string(),
            project_name: "Critical Project".to_string(),
            rate: 250.0,
            start_time: test_datetime(9, 0),
            end_time: Some(test_datetime(12, 0)),
            comment: None,
            is_break: false,
            is_generated: false,
        }];

        let new_task = create_test_task(2, "New Task", 10, 0, 11, 0, false);
        insert_and_resolve_overlaps(&mut tasks, new_task);

        // Original task should be split but preserve its data
        let original_tasks: Vec<_> = tasks.iter().filter(|t| t.id == 1).collect();
        assert_eq!(original_tasks.len(), 2); // Split into two parts

        for task in original_tasks {
            assert_eq!(task.name, "Important Task");
            assert_eq!(task.project_name, "Critical Project");
            assert_eq!(task.rate, 250.0);
        }
    }
}
