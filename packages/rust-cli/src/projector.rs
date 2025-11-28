use chrono::{Duration, Local, TimeZone};
use crate::events::Event;
use crate::models::Task;

pub fn restore_state(events: &[Event]) -> Vec<Task> {
    let mut projects = Vec::new();

    // Replay logic for Undo/Redo
    let mut effective_events = Vec::new();
    let mut redo_stack = Vec::new();

    for event in events {
        match event {
            Event::Undo { .. } => {
                if let Some(ev) = effective_events.pop() {
                    redo_stack.push(ev);
                }
            }
            Event::Redo { .. } => {
                if let Some(ev) = redo_stack.pop() {
                    effective_events.push(ev);
                }
            }
            // Regular events (Start, Break, Stop, DayRevised)
            _ => {
                effective_events.push(event);
                // If a new action happens, the redo stack is invalidated
                redo_stack.clear();
            }
        }
    }

    for event in effective_events {
        process_event(&mut projects, event);
    }

    // Ensure chronological order, especially after DayRevised edits on past days
    projects.sort_by_key(|p| p.start_time);

    projects
}

fn process_event(projects: &mut Vec<Task>, event: &Event) {
    match event {
        Event::TaskStarted { id, name, project_name, customer_name, rate, start_time, is_generated } => {
            // Check if the last task is the same as the one being started
            if let Some(last) = projects.last_mut() {
                if last.name == *name && last.project_name == *project_name && !last.is_break {
                    last.end_time = None;
                    return;
                }
            }

            // Logic: Starting a new project stops the previous one
            close_last_project(projects, *start_time);

            projects.push(Task {
                id: *id,
                project_name: project_name.clone(),
                customer_name: customer_name.to_string(),
                rate: rate.clone(),
                name: name.clone(),
                start_time: *start_time,
                end_time: None,
                comment: None,
                is_break: false,
                is_generated: *is_generated,
            });
        }
        Event::BreakStarted { start_time, is_generated } => {
            close_last_project(projects, *start_time);

            projects.push(Task {
                id: -1,
                project_name: "".to_string(),
                customer_name: "".to_string(),
                rate: 0.0,
                name: "Break".to_string(),
                start_time: *start_time,
                end_time: None,
                comment: None,
                is_break: true,
                is_generated: *is_generated,
            });
        }
        Event::Stopped { end_time, .. } => {
            close_last_project(projects, *end_time);
        }
        Event::Reopen { start_time, is_generated } => {
            // Find the latest closed task (not break) to reopen
            let target_task = projects.iter().rev().find(|t| !t.is_break && t.end_time.is_some()).cloned();

            if let Some(task) = target_task {
                // Check if we can just "open the old task" if it's the immediate predecessor
                if let Some(last) = projects.last_mut() {
                    if last.name == task.name && last.project_name == task.project_name && !last.is_break {
                        last.end_time = None;
                        return;
                    }
                }

                close_last_project(projects, *start_time);

                projects.push(Task {
                    id: task.id,
                    project_name: task.project_name,
                    customer_name: task.customer_name,
                    rate: task.rate,
                    name: task.name,
                    start_time: *start_time,
                    end_time: None,
                    comment: None,
                    is_break: false,
                    is_generated: *is_generated,
                });
            }
        }
        Event::DayRevised { date, events } => {
            // Remove all projects that fall on this date
            projects.retain(|p| p.start_time.date_naive() != *date);

            // Replay the revised events
            // Note: These events are raw (Start, Stop), so we process them recursively.
            // We don't need Undo/Redo logic here as DayRevised contains the final desired sequence.
            for sub_event in events {
                process_event(projects, sub_event);
            }
        }
        // Undo and Redo are structural events, processed above
        Event::Undo { .. } | Event::Redo { .. } => {}
    }
}

fn close_last_project(projects: &mut Vec<Task>, time: chrono::DateTime<chrono::Local>) {
    let last_idx = if projects.is_empty() {
        return;
    } else {
        projects.len() - 1
    };

    if projects[last_idx].end_time.is_some() {
        return;
    }

    let start_time = projects[last_idx].start_time;
    let end_time = time;

    if start_time.date_naive() == end_time.date_naive() {
        projects[last_idx].end_time = Some(end_time);
    } else {
        // Split the project across midnight boundaries
        let name = projects[last_idx].name.clone();
        let project_name = projects[last_idx].project_name.clone();
        let customer_name = projects[last_idx].customer_name.clone();
        let rate = projects[last_idx].rate.clone();
        let id = projects[last_idx].id;
        let is_break = projects[last_idx].is_break;
        let is_generated = projects[last_idx].is_generated;

        // 1. Close the current project at the end of the day (start of next day)
        let next_day_midnight = (start_time.date_naive() + Duration::days(1))
            .and_hms_opt(0, 0, 0)
            .unwrap();
        let next_day_local = Local.from_local_datetime(&next_day_midnight).unwrap();

        projects[last_idx].end_time = Some(next_day_local);

        // 2. Create intermediate full-day entries if the span covers whole days
        let mut current_start = next_day_local;
        while current_start.date_naive() < end_time.date_naive() {
            let next_day = (current_start.date_naive() + Duration::days(1))
                .and_hms_opt(0, 0, 0)
                .unwrap();
            let next_day_local = Local.from_local_datetime(&next_day).unwrap();

            projects.push(Task {
                id,
                project_name: project_name.clone(),
                customer_name: customer_name.to_string(),
                rate,
                name: name.clone(),
                start_time: current_start,
                end_time: Some(next_day_local),
                comment: None,
                is_break,
                is_generated,
            });
            current_start = next_day_local;
        }

        // 3. Create the final segment on the end day
        projects.push(Task {
            id,
            project_name,
            customer_name,
            rate,
            name,
            start_time: current_start,
            end_time: Some(end_time),
            comment: None,
            is_break,
            is_generated,
        });
    }
}