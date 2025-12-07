use crate::events::Event;
use crate::models::Task;
use chrono::{Duration, Local, TimeZone};

// Only used on single days
pub fn restore_state(events: &[Event]) -> Vec<Task> {
    let mut projects = Vec::new();

    // 1. Determine Effective Events (Handle Undo/Redo)
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
            _ => {
                effective_events.push(event);
                redo_stack.clear();
            }
        }
    }

    // 2. Identify the last effective Event::LocallyCleared.
    let last_clear_index = effective_events.iter()
        .rposition(|e| matches!(e, Event::LocallyCleared { .. }));

    // 3. Filter the stream: Keep only events that occurred AFTER the last clear.
    let starting_index = last_clear_index.map(|i| i + 1).unwrap_or(0);
    let final_effective_events = &effective_events[starting_index..];


    // 4. Process the final filtered and ordered events
    for event in final_effective_events {
        process_event(&mut projects, event);
    }

    remove_edge_breaks(&mut projects);

    projects.sort_by_key(|p| p.start_time);
    projects
}

fn process_event(projects: &mut Vec<Task>, event: &Event) {
    match event {
        Event::TaskStarted {
            id,
            name,
            project_name,
            customer_name,
            rate,
            start_time,
        } => {
            resolve_start_overlaps(projects, *start_time);
            close_last_project(projects, *start_time); // Simplifed call

            projects.push(Task {
                id: *id,
                project_name: project_name.clone(),
                customer_name: customer_name.to_string(),
                rate: *rate,
                name: name.clone(),
                start_time: *start_time,
                end_time: None,
                is_break: false,
            });
        }
        Event::BreakStarted { start_time } => {
            // Deleted logic for pushing to paused_tasks based on is_generated

            resolve_start_overlaps(projects, *start_time);
            close_last_project(projects, *start_time); // Simplifed call

            projects.push(Task {
                id: -1,
                project_name: String::new(),
                customer_name: String::new(),
                rate: 0.0,
                name: "Break".to_string(),
                start_time: *start_time,
                end_time: None,
                is_break: true,
            });
        }
        Event::Stopped { end_time, .. } => {
            close_last_project(projects, *end_time);

            if let Some(killer_start) = projects.last().map(|task| task.start_time) {
                resolve_stop_overlaps(projects, killer_start, *end_time);
            }

            // Deleted logic for resuming paused task
        }
        Event::Reopen { start_time } => {
            let target_task = projects
                .iter()
                .rev()
                .find(|t| !t.is_break && t.end_time.is_some())
                .cloned();

            if let Some(task) = target_task {
                resolve_start_overlaps(projects, *start_time);
                close_last_project(projects, *start_time); // Simplifed call

                projects.push(Task {
                    id: task.id,
                    project_name: task.project_name,
                    customer_name: task.customer_name,
                    rate: task.rate,
                    name: task.name,
                    start_time: *start_time,
                    end_time: None,
                    is_break: false,
                });
            }
        }
        Event::Undo { .. } | Event::Redo { .. } => {}
        Event::CommentAdded { .. } => {}
        Event::LocallyCleared { .. } => {}
    }
}

// Deleted close_last_project_with_break_cleanup
// Deleted running_generated_break
// Deleted resume_paused_task_if_break_finished

fn resolve_stop_overlaps(projects: &mut Vec<Task>, killer_start: chrono::DateTime<Local>, killer_end: chrono::DateTime<Local>) {
    let len = projects.len();
    if len == 0 { return; }

    let last_index = len - 1;

    let mut i = 0;
    projects.retain(|task| {
        let keep_index = i;
        i += 1;

        if keep_index == last_index { return true; }

        let t_start = task.start_time;

        if t_start >= killer_start && t_start < killer_end {
            if let Some(t_end) = task.end_time {
                if t_end <= killer_end {
                    return false;
                }
            }
        }
        true
    });
    let current_len = projects.len();

    for (index, task) in projects.iter_mut().enumerate() {
        if index == current_len - 1 { continue; }

        if task.start_time >= killer_start && task.start_time < killer_end {
            task.start_time = killer_end;
        }
    }
}

fn resolve_start_overlaps(projects: &mut Vec<Task>, new_start: chrono::DateTime<Local>) {
    let mut tails = Vec::new();

    for task in projects.iter_mut() {
        if task.end_time.is_none() { continue; }

        let t_start = task.start_time;
        let t_end = task.end_time.unwrap();

        if t_start < new_start && t_end > new_start {
            let mut tail = task.clone();
            tail.start_time = new_start;
            tail.end_time = Some(t_end);
            tails.push(tail);

            task.end_time = Some(new_start);
        }
    }

    projects.extend(tails);
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
    if time < start_time {
        return;
    }
    let end_time = time;

    if start_time.date_naive() == end_time.date_naive() {
        projects[last_idx].end_time = Some(end_time);
    } else {
        let name = projects[last_idx].name.clone();
        let project_name = projects[last_idx].project_name.clone();
        let customer_name = projects[last_idx].customer_name.clone();
        let rate = projects[last_idx].rate.clone();
        let id = projects[last_idx].id;
        let is_break = projects[last_idx].is_break;
        // is_generated removed

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
                is_break,
                // is_generated removed
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
            is_break,
            // is_generated removed
        });
    }
}

fn remove_edge_breaks(projects: &mut Vec<Task>) {
    while projects.first().map(|t| t.is_break).unwrap_or(false) {
        projects.remove(0);
    }

    while projects.last().map(|t| t.is_break).unwrap_or(false) {
        projects.pop();
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::view::render_day;
    use chrono::{DateTime, Duration, NaiveDateTime, TimeZone};

    // --- Helper for creating Local DateTime from string ---
    fn dt(s: &str) -> DateTime<Local> {
        let naive = NaiveDateTime::parse_from_str(s, "%Y-%m-%d %H:%M:%S")
            .expect("Failed to parse date string");
        Local.from_local_datetime(&naive).unwrap()
    }

    #[test]
    fn test_basic_start_and_stop() {
        let start = dt("2023-10-27 09:00:00");
        let end = dt("2023-10-27 10:00:00");

        let events = vec![
            Event::TaskStarted {
                id: 1,
                name: "Coding".to_string(),
                project_name: "Rust".to_string(),
                customer_name: "OpenSrc".to_string(),
                rate: 100.0,
                start_time: start,
            },
            Event::Stopped { end_time: end },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        let task = &tasks[0];
        assert_eq!(task.name, "Coding");
        assert_eq!(task.start_time, start);
        assert_eq!(task.end_time, Some(end));
        assert_eq!(task.duration(), Duration::hours(1));
    }

    #[test]
    fn test_task_switching_closes_previous() {
        let t1_start = dt("2023-10-27 09:00:00");
        let t2_start = dt("2023-10-27 10:00:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Task 1".into(), project_name: "A".into(),
                customer_name: "C".into(), rate: 50.0, start_time: t1_start
            },
            Event::TaskStarted {
                id: 2, name: "Task 2".into(), project_name: "A".into(),
                customer_name: "C".into(), rate: 50.0, start_time: t2_start
            },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[0].end_time, Some(t2_start));
        assert_eq!(tasks[1].name, "Task 2");
        assert!(tasks[1].end_time.is_none());
    }

    #[test]
    fn test_break_logic() {
        let start = dt("2023-10-27 09:00:00");
        let break_start = dt("2023-10-27 12:00:00");
        let break_end = dt("2023-10-27 12:30:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Work".into(), project_name: "A".into(), customer_name: "C".into(),
                rate: 50.0, start_time: start
            },
            Event::BreakStarted { start_time: break_start },
            Event::Stopped { end_time: break_end },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);

        assert_eq!(tasks[0].name, "Work");
        assert_eq!(tasks[0].end_time, Some(break_start));

        assert_eq!(tasks[1].name, "Break");
        assert!(tasks[1].is_break);
        assert_eq!(tasks[1].start_time, break_start);
        assert_eq!(tasks[1].end_time, Some(break_end));
    }

    #[test]
    fn test_reopen_logic() {
        let start = dt("2023-10-27 09:00:00");
        let stop = dt("2023-10-27 10:00:00");
        let reopen = dt("2023-10-27 10:30:00");

        let events = vec![
            Event::TaskStarted {
                id: 10, name: "Design".into(), project_name: "Art".into(), customer_name: "X".into(),
                rate: 80.0, start_time: start
            },
            Event::Stopped { end_time: stop },
            Event::Reopen { start_time: reopen },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);

        assert_eq!(tasks[0].end_time, Some(stop));

        assert_eq!(tasks[1].id, 10);
        assert_eq!(tasks[1].name, "Design");
        assert_eq!(tasks[1].start_time, reopen);
        assert!(tasks[1].end_time.is_none());
    }

    #[test]
    fn test_midnight_splitting() {
        let start = dt("2023-10-27 23:00:00");
        let end = dt("2023-10-28 01:00:00");
        let midnight = dt("2023-10-28 00:00:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Late Night".into(), project_name: "P".into(), customer_name: "C".into(),
                rate: 50.0, start_time: start
            },
            Event::Stopped { end_time: end },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2, "Should split into two tasks across midnight");

        assert_eq!(tasks[0].start_time, start);
        assert_eq!(tasks[0].end_time, Some(midnight));

        assert_eq!(tasks[1].start_time, midnight);
        assert_eq!(tasks[1].end_time, Some(end));
    }

    #[test]
    fn test_undo_restores_state() {
        let start = dt("2023-10-27 09:00:00");
        let stop = dt("2023-10-27 10:00:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Undo Me".into(), project_name: "P".into(), customer_name: "C".into(),
                rate: 50.0, start_time: start
            },
            Event::Stopped { end_time: stop },
            Event::Undo { time: dt("2023-10-27 10:00:05") },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].name, "Undo Me");
        assert!(tasks[0].end_time.is_none());
    }

    #[test]
    fn test_redo_restores_undone_action() {
        let start = dt("2023-10-27 09:00:00");
        let stop = dt("2023-10-27 10:00:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Redo Me".into(), project_name: "P".into(), customer_name: "C".into(),
                rate: 50.0, start_time: start
            },
            Event::Stopped { end_time: stop },
            Event::Undo { time: dt("2023-10-27 10:00:05") },
            Event::Redo { time: dt("2023-10-27 10:00:10") },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].end_time, Some(stop));
    }

    #[test]
    fn test_overlap_punch_hole() {
        let t_start = dt("2023-10-27 09:00:00");
        let t_end = dt("2023-10-27 12:00:00");
        let b_start = dt("2023-10-27 10:00:00");
        let b_end = dt("2023-10-27 11:00:00");

        let events = vec![
            Event::TaskStarted { id: 1, name: "A".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: t_start },
            Event::Stopped { end_time: t_end },
            Event::TaskStarted { id: 2, name: "B".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: b_start },
            Event::Stopped { end_time: b_end },
        ];

        let mut tasks = restore_state(&events);
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 3);

        assert_eq!(tasks[0].name, "A");
        assert_eq!(tasks[0].start_time, t_start);
        assert_eq!(tasks[0].end_time, Some(b_start));

        assert_eq!(tasks[1].name, "B");
        assert_eq!(tasks[1].start_time, b_start);
        assert_eq!(tasks[1].end_time, Some(b_end));

        assert_eq!(tasks[2].name, "A");
        assert_eq!(tasks[2].start_time, b_end);
        assert_eq!(tasks[2].end_time, Some(t_end));
    }

    #[test]
    fn test_overlap_replace_fully_overlapped() {
        let a_start = dt("2023-10-27 10:00:00");
        let a_end = dt("2023-10-27 10:30:00");
        let b_start = dt("2023-10-27 09:00:00");
        let b_end = dt("2023-10-27 11:00:00");

        let events = vec![
            Event::TaskStarted { id: 1, name: "A".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: a_start },
            Event::Stopped { end_time: a_end },
            Event::TaskStarted { id: 2, name: "B".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: b_start },
            Event::Stopped { end_time: b_end },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].name, "B");
        assert_eq!(tasks[0].start_time, b_start);
        assert_eq!(tasks[0].end_time, Some(b_end));
    }

    #[test]
    fn test_overlap_change_start_of_partly_overlapped() {
        let a_start = dt("2023-10-27 09:00:00");
        let a_end = dt("2023-10-27 10:00:00");
        let b_start = dt("2023-10-27 08:30:00");
        let b_end = dt("2023-10-27 09:30:00");

        let events = vec![
            Event::TaskStarted { id: 1, name: "A".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: a_start },
            Event::Stopped { end_time: a_end },
            Event::TaskStarted { id: 2, name: "B".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: b_start },
            Event::Stopped { end_time: b_end },
        ];

        let mut tasks = restore_state(&events);
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 2);

        assert_eq!(tasks[0].name, "B");
        assert_eq!(tasks[0].start_time, b_start);
        assert_eq!(tasks[0].end_time, Some(b_end));

        assert_eq!(tasks[1].name, "A");
        assert_eq!(tasks[1].start_time, b_end);
        assert_eq!(tasks[1].end_time, Some(a_end));
    }

    #[test]
    fn test_overlap_insert_break_into_task() {
        let t_start_a = dt("2023-10-27 09:00:00");
        let t_end_a = dt("2023-10-27 10:00:00");
        let t_start_b = dt("2023-10-27 10:00:00");
        let t_end_b = dt("2023-10-27 10:30:00");
        let t_start_c = dt("2023-10-27 10:30:00");
        let t_end_c = dt("2023-10-27 12:00:00");
        let t_start_d = dt("2023-10-27 11:00:00");
        let t_end_d = dt("2023-10-27 11:30:00");

        let events = vec![
            Event::TaskStarted { id: 1, name: "Task A".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_a },
            Event::Stopped { end_time: t_end_a },

            Event::BreakStarted { start_time: t_start_b },
            Event::Stopped { end_time: t_end_b },

            Event::TaskStarted { id: 3, name: "Task C".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_c },
            Event::Stopped { end_time: t_end_c },

            Event::BreakStarted { start_time: t_start_d },
            Event::Stopped { end_time: t_end_d },
        ];

        let mut tasks = restore_state(&events);
        let day_projects_refs: Vec<&Task> = tasks.iter().collect();
        render_day(&day_projects_refs).unwrap();
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 5, "There should be 5 tasks: A, B, C1, D, C2");

        assert_eq!(tasks[0].name, "Task A");
        assert_eq!(tasks[0].start_time, t_start_a);
        assert_eq!(tasks[0].end_time, Some(t_end_a));

        assert_eq!(tasks[1].name, "Break");
        assert!(tasks[1].is_break);
        assert_eq!(tasks[1].start_time, t_start_b);
        assert_eq!(tasks[1].end_time, Some(t_end_b));

        assert_eq!(tasks[2].name, "Task C");
        assert_eq!(tasks[2].start_time, t_start_c);
        assert_eq!(tasks[2].end_time, Some(t_start_d));

        assert_eq!(tasks[3].name, "Break");
        assert_eq!(tasks[3].start_time, t_start_d);
        assert_eq!(tasks[3].end_time, Some(t_end_d));

        assert_eq!(tasks[4].name, "Task C");
        assert_eq!(tasks[4].start_time, t_end_d);
        assert_eq!(tasks[4].end_time, Some(t_end_c));

    }

    #[test]
    fn test_overlap_insert_break_into_task_open_ended() {
        let t_start_a = dt("2023-10-27 09:00:00");
        let t_end_a = dt("2023-10-27 10:00:00");
        let t_start_b = dt("2023-10-27 10:00:00");
        let t_end_b = dt("2023-10-27 10:30:00");
        let t_start_c = dt("2023-10-27 10:30:00");
        let t_start_d = dt("2023-10-27 11:00:00");
        let t_end_d = dt("2023-10-27 11:30:00");

        let events = vec![
            Event::TaskStarted { id: 1, name: "Task A".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_a },
            Event::Stopped { end_time: t_end_a },

            Event::BreakStarted { start_time: t_start_b },
            Event::Stopped { end_time: t_end_b },

            Event::TaskStarted { id: 3, name: "Task C".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_c },

            Event::BreakStarted { start_time: t_start_d },
            Event::Stopped { end_time: t_end_d },
        ];

        let mut tasks = restore_state(&events);
        let day_projects_refs: Vec<&Task> = tasks.iter().collect();
        render_day(&day_projects_refs).unwrap();
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 5, "There should be 5 tasks: A, B, C1, D, C2");

        assert_eq!(tasks[0].name, "Task A");
        assert_eq!(tasks[0].start_time, t_start_a);
        assert_eq!(tasks[0].end_time, Some(t_end_a));

        assert_eq!(tasks[1].name, "Break");
        assert!(tasks[1].is_break);
        assert_eq!(tasks[1].start_time, t_start_b);
        assert_eq!(tasks[1].end_time, Some(t_end_b));

        assert_eq!(tasks[2].name, "Task C");
        assert_eq!(tasks[2].start_time, t_start_c);
        assert_eq!(tasks[2].end_time, Some(t_start_d));

        assert_eq!(tasks[3].name, "Break");
        assert_eq!(tasks[3].start_time, t_start_d);
        assert_eq!(tasks[3].end_time, Some(t_end_d));

        assert_eq!(tasks[4].name, "Task C");
        assert_eq!(tasks[4].start_time, t_end_d);
        assert_eq!(tasks[4].end_time, None);

    }
}