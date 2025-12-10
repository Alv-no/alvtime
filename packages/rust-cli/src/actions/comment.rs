use crate::actions::utils::get_all_tasks;
use crate::events::Event;
use crate::store::EventStore;
use crate::view;
use chrono::NaiveDate;
use inquire::{Select, Editor};
use std::collections::HashMap;

/// Comment editor for a single day.
/// Shows tasks for that day (excluding breaks), lets user add/remove a single comment per task.
pub fn handle_comments(
    holidays: &std::collections::HashSet<NaiveDate>,
    event_store: &EventStore,
) -> String {
    let all_tasks = get_all_tasks(event_store);

    // 1. Select day using interactive month view
    let selected_dates = match view::interactive_view(
        &*all_tasks,
        holidays,
        &event_store.get_unsynced_dates(),
        false,
    ) {
        Ok(d) => d,
        Err(e) => return format!("Interactive view failed: {}", e),
    };

    let date = match selected_dates.first() {
        Some(d) => *d,
        None => return "Cancelled. No day selected.".to_string(),
    };

    // 2. Load tasks for that day
    let day_events = event_store.events_for_day(date);
    let tasks = crate::projector::restore_state(&day_events);

    // Filter out breaks and deduplicate by task.id
    let mut unique_tasks: HashMap<i32, _> = HashMap::new();
    for t in tasks.into_iter().filter(|t| !t.is_break) {
        unique_tasks.entry(t.id).or_insert(t);
    }

    if unique_tasks.is_empty() {
        return "No tasks (excluding breaks) for selected day.".to_string();
    }

    let tasks: Vec<_> = unique_tasks.into_values().collect();

    loop {
        println!("\n--- Comments for {} ---", date);
        for (i, t) in tasks.iter().enumerate() {
            let comment_display = match &t.comment {
                Some(c) => {
                    // Truncate long comments for display
                    if c.len() > 50 {
                        format!(" [{}...]", &c[..47])
                    } else {
                        format!(" [{}]", c)
                    }
                }
                None => String::new(),
            };
            println!("{}. {} ({}){}", i + 1, t.name, t.project_name, comment_display);
        }

        let idx_opts: Vec<String> = (1..=tasks.len()).map(|i| i.to_string()).collect();

        let entry_idx = match Select::new("Select task:", idx_opts).prompt() {
            Ok(s) => s.parse::<usize>().unwrap() - 1,
            Err(_) => return "Cancelled.".to_string(),
        };

        let task = &tasks[entry_idx];

        // Display current comment if exists
        if let Some(current_comment) = &task.comment {
            println!("\nCurrent comment:");
            println!("{}", current_comment);
        } else {
            println!("\nNo comment currently set.");
        }

        let op = match Select::new(
            "Action:",
            vec!["Add/Replace Comment", "Remove Comment", "Cancel"],
        )
            .prompt()
        {
            Ok(o) => o,
            Err(_) => continue,
        };

        match op {
            "Add/Replace Comment" => {
                let mut editor = Editor::new("Enter the comment");

                // Pre-fill editor with existing comment if present
                if let Some(existing) = &task.comment {
                    editor = editor.with_predefined_text(existing);
                }

                let comment = match editor.prompt() {
                    Ok(c) => c,
                    Err(_) => continue,
                };

                event_store.persist(&Event::CommentAdded {
                    date,
                    task_id: task.id,
                    comment,
                });

                return "Comment added/updated.".to_string();
            }
            "Remove Comment" => {
                event_store.persist(&Event::CommentRemoved {
                    date,
                    task_id: task.id,
                });
                return "Comment removed.".to_string();
            }
            _ => return "Cancelled.".to_string(),
        }
    }
}