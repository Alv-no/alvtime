use crate::events::Event;
use crate::models::Task;
use chrono::{Duration, Local, TimeZone};

pub fn restore_state(events: &[Event]) -> Vec<Task> {
    let mut projects = Vec::new();
    let mut paused_tasks = Vec::new();

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
            _ => {
                effective_events.push(event);
                redo_stack.clear();
            }
        }
    }

    for event in effective_events {
        process_event(&mut projects, &mut paused_tasks, event);
    }

    projects.sort_by_key(|p| p.start_time);
    projects
}

fn process_event(projects: &mut Vec<Task>, paused_tasks: &mut Vec<Task>, event: &Event) {
    match event {
        Event::TaskStarted {
            id,
            name,
            project_name,
            customer_name,
            rate,
            start_time,
            is_generated,
        } => {
            resolve_start_overlaps(projects, *start_time);
            close_last_project_with_break_cleanup(projects, paused_tasks, *start_time);

            projects.push(Task {
                id: *id,
                project_name: project_name.clone(),
                customer_name: customer_name.to_string(),
                rate: *rate,
                name: name.clone(),
                start_time: *start_time,
                end_time: None,
                comment: None,
                is_break: false,
                is_generated: *is_generated,
            });
        }
        Event::BreakStarted { start_time, is_generated } => {
            if *is_generated {
                if let Some(task) = projects
                    .last()
                    .filter(|t| t.end_time.is_none() && !t.is_break)
                    .cloned()
                {
                    paused_tasks.push(task);
                }
            }

            resolve_start_overlaps(projects, *start_time);
            close_last_project_with_break_cleanup(projects, paused_tasks, *start_time);

            projects.push(Task {
                id: -1,
                project_name: String::new(),
                customer_name: String::new(),
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

            if let Some(killer_start) = projects.last().map(|task| task.start_time) {
                resolve_stop_overlaps(projects, killer_start, *end_time);
            }

            resume_paused_task_if_break_finished(projects, paused_tasks, *end_time);
        }
        Event::Reopen { start_time, is_generated } => {
            let target_task = projects
                .iter()
                .rev()
                .find(|t| !t.is_break && t.end_time.is_some())
                .cloned();

            if let Some(task) = target_task {
                resolve_start_overlaps(projects, *start_time);
                close_last_project_with_break_cleanup(projects, paused_tasks, *start_time);

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
            projects.retain(|p| p.start_time.date_naive() != *date);
            for sub_event in events {
                process_event(projects, paused_tasks, sub_event);
            }
        }
        Event::Undo { .. } | Event::Redo { .. } => {}
    }
}

fn close_last_project_with_break_cleanup(
    projects: &mut Vec<Task>,
    paused_tasks: &mut Vec<Task>,
    time: chrono::DateTime<Local>,
) {
    let break_was_running = running_generated_break(projects);
    close_last_project(projects, time);
    if break_was_running && !running_generated_break(projects) {
        paused_tasks.pop();
    }
}

fn running_generated_break(projects: &[Task]) -> bool {
    projects
        .last()
        .map(|task| task.is_break && task.is_generated && task.end_time.is_none())
        .unwrap_or(false)
}

fn resume_paused_task_if_break_finished(
    projects: &mut Vec<Task>,
    paused_tasks: &mut Vec<Task>,
    resume_time: chrono::DateTime<Local>,
) {
    let should_resume = projects
        .last()
        .map(|task| task.is_break && task.is_generated)
        .unwrap_or(false);

    if should_resume {
        if let Some(mut task) = paused_tasks.pop() {
            task.start_time = resume_time;
            task.end_time = None;
            projects.push(task);
        }
    }
}

fn resolve_stop_overlaps(projects: &mut Vec<Task>, killer_start: chrono::DateTime<Local>, killer_end: chrono::DateTime<Local>) {
    // We need to iterate carefully because we might remove items.
    // We skip the last item because that's the "Killer" task (the one we just stopped).
    let len = projects.len();
    if len == 0 { return; }

    let last_index = len - 1;

    // Use retain to filter out fully overlapped tasks
    let mut i = 0;
    projects.retain(|task| {
        let keep_index = i;
        i += 1;

        // Don't kill yourself (the last task)
        if keep_index == last_index { return true; }

        let t_start = task.start_time;

        // 1. Fully Overlapped: Task is inside the killer zone -> Remove it
        // Note: We check if it starts >= killer_start.
        // Logic: if it starts inside the zone, and ends inside (or even outside, but we prioritize the new task),
        // we might need to verify behavior.
        // Prompt: "replace the fully overlapped"
        if t_start >= killer_start && t_start < killer_end {
            if let Some(t_end) = task.end_time {
                if t_end <= killer_end {
                    return false; // DELETE
                }
            }
        }
        true
    });
    let current_len = projects.len();
    // Now handle "Partly Overlapped at the end" -> Push start time forward
    for (index, task) in projects.iter_mut().enumerate() {
        if index == current_len - 1 { continue; } // Skip self

        // If task starts inside the killer zone (but wasn't fully removed above,
        // meaning it must extend beyond killer_end), move its start.
        if task.start_time >= killer_start && task.start_time < killer_end {
            task.start_time = killer_end;
        }
    }
}

fn resolve_start_overlaps(projects: &mut Vec<Task>, new_start: chrono::DateTime<Local>) {
    let mut tails = Vec::new();

    for task in projects.iter_mut() {
        // Skip open tasks (they are handled by close_last_project)
        if task.end_time.is_none() { continue; }

        let t_start = task.start_time;
        let t_end = task.end_time.unwrap();

        // If the task strictly spans across the new start time
        if t_start < new_start && t_end > new_start {
            // Create a "tail" segment that starts where the new task starts
            let mut tail = task.clone();
            tail.start_time = new_start;
            tail.end_time = Some(t_end);
            tails.push(tail);

            // Cut the current task at the new start
            task.end_time = Some(new_start);
        }
    }

    // Add the displaced tails back to the list
    // (They will likely be modified again by resolve_stop_overlaps when the new task stops)
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

#[cfg(test)]
mod tests {
    use super::*;
    use crate::view::render_day;
    // Imports Event, Task, restore_state, etc.
        use chrono::{DateTime, Duration, NaiveDate, NaiveDateTime, TimeZone};

    // --- Helper for creating Local DateTime from string ---
    fn dt(s: &str) -> DateTime<Local> {
        let naive = NaiveDateTime::parse_from_str(s, "%Y-%m-%d %H:%M:%S")
            .expect("Failed to parse date string");
        Local.from_local_datetime(&naive).unwrap()
    }

    // --- Helper for creating NaiveDate ---
    fn nd(s: &str) -> NaiveDate {
        NaiveDate::parse_from_str(s, "%Y-%m-%d").unwrap()
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
                is_generated: false,
            },
            Event::Stopped { end_time: end, is_generated: false },
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
                customer_name: "C".into(), rate: 50.0, start_time: t1_start, is_generated: false
            },
            // Starting Task 2 should implicitly stop Task 1
            Event::TaskStarted {
                id: 2, name: "Task 2".into(), project_name: "A".into(),
                customer_name: "C".into(), rate: 50.0, start_time: t2_start, is_generated: false
            },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);
        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[0].end_time, Some(t2_start)); // Closed by next start
        assert_eq!(tasks[1].name, "Task 2");
        assert!(tasks[1].end_time.is_none()); // Currently running
    }

    #[test]
    fn test_break_logic() {
        let start = dt("2023-10-27 09:00:00");
        let break_start = dt("2023-10-27 12:00:00");
        let break_end = dt("2023-10-27 12:30:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Work".into(), project_name: "A".into(), customer_name: "C".into(),
                rate: 50.0, start_time: start, is_generated: false
            },
            Event::BreakStarted { start_time: break_start, is_generated: false },
            Event::Stopped { end_time: break_end, is_generated: false },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);

        // Task 1 stopped at break start
        assert_eq!(tasks[0].name, "Work");
        assert_eq!(tasks[0].end_time, Some(break_start));

        // Break task
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
                rate: 80.0, start_time: start, is_generated: false
            },
            Event::Stopped { end_time: stop, is_generated: false },
            Event::Reopen { start_time: reopen, is_generated: false },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);

        // Original session
        assert_eq!(tasks[0].end_time, Some(stop));

        // Reopened session (clones details from ID 10)
        assert_eq!(tasks[1].id, 10);
        assert_eq!(tasks[1].name, "Design");
        assert_eq!(tasks[1].start_time, reopen);
        assert!(tasks[1].end_time.is_none());
    }

    #[test]
    fn test_midnight_splitting() {
        // Task starts at 11 PM and ends at 1 AM the next day
        let start = dt("2023-10-27 23:00:00");
        let end = dt("2023-10-28 01:00:00");
        let midnight = dt("2023-10-28 00:00:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Late Night".into(), project_name: "P".into(), customer_name: "C".into(),
                rate: 50.0, start_time: start, is_generated: false
            },
            Event::Stopped { end_time: end, is_generated: false },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2, "Should split into two tasks across midnight");

        // Segment 1: 23:00 -> 00:00
        assert_eq!(tasks[0].start_time, start);
        assert_eq!(tasks[0].end_time, Some(midnight));

        // Segment 2: 00:00 -> 01:00
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
                rate: 50.0, start_time: start, is_generated: false
            },
            Event::Stopped { end_time: stop, is_generated: false },
            // Undo the Stop event
            Event::Undo { time: dt("2023-10-27 10:00:05") },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].name, "Undo Me");
        // Task should be running (end_time removed)
        assert!(tasks[0].end_time.is_none());
    }

    #[test]
    fn test_redo_restores_undone_action() {
        let start = dt("2023-10-27 09:00:00");
        let stop = dt("2023-10-27 10:00:00");

        let events = vec![
            Event::TaskStarted {
                id: 1, name: "Redo Me".into(), project_name: "P".into(), customer_name: "C".into(),
                rate: 50.0, start_time: start, is_generated: false
            },
            Event::Stopped { end_time: stop, is_generated: false },
            Event::Undo { time: dt("2023-10-27 10:00:05") }, // Reverts Stop
            Event::Redo { time: dt("2023-10-27 10:00:10") }, // Re-applies Stop
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].end_time, Some(stop));
    }

    #[test]
    fn test_day_revised_overwrites_history() {
        let day_target = nd("2023-10-27");
        let day_other = nd("2023-10-28");

        let t1_start = dt("2023-10-27 09:00:00");
        let t2_start = dt("2023-10-28 09:00:00"); // Different day

        let events = vec![
            // 1. Initial history for Day 27
            Event::TaskStarted {
                id: 1, name: "Old Task".into(), project_name: "P".into(), customer_name: "C".into(),
                rate: 50.0, start_time: t1_start, is_generated: false
            },
            Event::Stopped { end_time: dt("2023-10-27 10:00:00"), is_generated: false },

            // 2. History for Day 28 (Should not be touched)
            Event::TaskStarted {
                id: 2, name: "Keep Me".into(), project_name: "P".into(), customer_name: "C".into(),
                rate: 50.0, start_time: t2_start, is_generated: false
            },

            // 3. Revision for Day 27 (Replaces "Old Task" with "New Task")
            Event::DayRevised {
                date: day_target,
                events: vec![
                    Event::TaskStarted {
                        id: 3, name: "New Task".into(), project_name: "P".into(), customer_name: "C".into(),
                        rate: 50.0, start_time: dt("2023-10-27 12:00:00"), is_generated: false
                    },
                    Event::Stopped { end_time: dt("2023-10-27 13:00:00"), is_generated: false }
                ]
            }
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);

        // Sort acts on start time.
        // 1. New Task (12:00 Day 27)
        // 2. Keep Me (09:00 Day 28)

        let t_day27 = tasks.iter().find(|t| t.start_time.date_naive() == day_target).unwrap();
        assert_eq!(t_day27.name, "New Task");
        assert_eq!(t_day27.start_time, dt("2023-10-27 12:00:00"));

        let t_day28 = tasks.iter().find(|t| t.start_time.date_naive() == day_other).unwrap();
        assert_eq!(t_day28.name, "Keep Me");
    }

    #[test]
    fn test_overlap_punch_hole() {
        // Existing: A [09:00 -------- 12:00]
        // Insert:   B      [10:00 - 11:00]
        // Result:   A [09-10], B [10-11], A [11-12]

        let t_start = dt("2023-10-27 09:00:00");
        let t_end = dt("2023-10-27 12:00:00");
        let b_start = dt("2023-10-27 10:00:00");
        let b_end = dt("2023-10-27 11:00:00");

        let events = vec![
            // 1. Create Task A
            Event::TaskStarted { id: 1, name: "A".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: t_start, is_generated: false },
            Event::Stopped { end_time: t_end, is_generated: false },
            // 2. Insert Task B in the middle
            Event::TaskStarted { id: 2, name: "B".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: b_start, is_generated: false },
            Event::Stopped { end_time: b_end, is_generated: false },
        ];

        let mut tasks = restore_state(&events);

        // Sort to ensure chronological check (A1, B, A2)
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 3);

        // A Part 1
        assert_eq!(tasks[0].name, "A");
        assert_eq!(tasks[0].start_time, t_start);
        assert_eq!(tasks[0].end_time, Some(b_start));

        // B
        assert_eq!(tasks[1].name, "B");
        assert_eq!(tasks[1].start_time, b_start);
        assert_eq!(tasks[1].end_time, Some(b_end));

        // A Part 2 (The Tail)
        assert_eq!(tasks[2].name, "A");
        assert_eq!(tasks[2].start_time, b_end); // Start moved to end of B
        assert_eq!(tasks[2].end_time, Some(t_end));
    }

    #[test]
    fn test_overlap_replace_fully_overlapped() {
        // Existing: A [10:00 - 10:30]
        // Insert:   B [09:00 -------- 11:00]
        // Result:   B [09:00 - 11:00] (A is gone)

        let a_start = dt("2023-10-27 10:00:00");
        let a_end = dt("2023-10-27 10:30:00");
        let b_start = dt("2023-10-27 09:00:00");
        let b_end = dt("2023-10-27 11:00:00");

        let events = vec![
            Event::TaskStarted { id: 1, name: "A".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: a_start, is_generated: false },
            Event::Stopped { end_time: a_end, is_generated: false },
            // Insert B covering A
            Event::TaskStarted { id: 2, name: "B".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: b_start, is_generated: false },
            Event::Stopped { end_time: b_end, is_generated: false },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].name, "B");
        assert_eq!(tasks[0].start_time, b_start);
        assert_eq!(tasks[0].end_time, Some(b_end));
    }

    #[test]
    fn test_overlap_change_start_of_partly_overlapped() {
        // Existing: A [09:00 - 10:00]
        // Insert:   B [08:30 - 09:30]
        // Result:   B [08:30 - 09:30], A [09:30 - 10:00]

        let a_start = dt("2023-10-27 09:00:00");
        let a_end = dt("2023-10-27 10:00:00");
        let b_start = dt("2023-10-27 08:30:00");
        let b_end = dt("2023-10-27 09:30:00");

        let events = vec![
            Event::TaskStarted { id: 1, name: "A".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: a_start, is_generated: false },
            Event::Stopped { end_time: a_end, is_generated: false },
            // Insert B overlapping start of A
            Event::TaskStarted { id: 2, name: "B".into(), project_name: "".into(), customer_name: "".into(), rate: 0.0, start_time: b_start, is_generated: false },
            Event::Stopped { end_time: b_end, is_generated: false },
        ];

        let mut tasks = restore_state(&events);
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 2);

        // B comes first
        assert_eq!(tasks[0].name, "B");
        assert_eq!(tasks[0].start_time, b_start);
        assert_eq!(tasks[0].end_time, Some(b_end));

        // A is trimmed
        assert_eq!(tasks[1].name, "A");
        assert_eq!(tasks[1].start_time, b_end); // Start moved to 09:30
        assert_eq!(tasks[1].end_time, Some(a_end));
    }

    #[test]
    fn test_overlap_insert_break_into_task() {
        // Scenario:
        // 1. Task A [09:00 - 10:00]
        // 2. Break B [10:00 - 10:30]
        // 3. Task C [10:30 - 12:00]
        // 4. Insert Break D [11:00 - 11:30] into Task C.
        //
        // Result should be 4 closed tasks:
        // A [09:00 - 10:00]
        // B [10:00 - 10:30]
        // C1 [10:30 - 11:00]
        // D [11:00 - 11:30]
        // C2 [11:30 - 12:00]

        let t_start_a = dt("2023-10-27 09:00:00");
        let t_end_a = dt("2023-10-27 10:00:00");
        let t_start_b = dt("2023-10-27 10:00:00"); // Manual break start
        let t_end_b = dt("2023-10-27 10:30:00"); // Manual break end
        let t_start_c = dt("2023-10-27 10:30:00");
        let t_end_c = dt("2023-10-27 12:00:00");
        let t_start_d = dt("2023-10-27 11:00:00"); // Inserted break start
        let t_end_d = dt("2023-10-27 11:30:00"); // Inserted break end

        let events = vec![
            // 1. Task A
            Event::TaskStarted { id: 1, name: "Task A".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_a, is_generated: false },
            Event::Stopped { end_time: t_end_a, is_generated: false },

            // 2. Break B
            Event::BreakStarted { start_time: t_start_b, is_generated: false },
            Event::Stopped { end_time: t_end_b, is_generated: false },

            // 3. Task C
            Event::TaskStarted { id: 3, name: "Task C".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_c, is_generated: false },
            Event::Stopped { end_time: t_end_c, is_generated: false },

            // --- Inserted Break D (simulates the autobreak logic) ---
            // This break will "punch a hole" in Task C
            Event::BreakStarted { start_time: t_start_d, is_generated: true }, // Use is_generated: true for better context
            Event::Stopped { end_time: t_end_d, is_generated: true },
        ];

        let mut tasks = restore_state(&events);
        let day_projects_refs: Vec<&Task> = tasks.iter().collect();
        render_day(&day_projects_refs);
        // Ensure chronological order
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 5, "There should be 5 tasks: A, B, C1, D, C2");

        // 1. Task A (09:00-10:00) - Untouched
        assert_eq!(tasks[0].name, "Task A");
        assert_eq!(tasks[0].start_time, t_start_a);
        assert_eq!(tasks[0].end_time, Some(t_end_a));

        // 2. Break B (10:00-10:30) - Untouched
        assert_eq!(tasks[1].name, "Break");
        assert!(tasks[1].is_break);
        assert_eq!(tasks[1].start_time, t_start_b);
        assert_eq!(tasks[1].end_time, Some(t_end_b));

        // 3. Task C Part 1 (10:30-11:00) - Cut short by resolve_start_overlaps
        assert_eq!(tasks[2].name, "Task C");
        assert_eq!(tasks[2].start_time, t_start_c);
        assert_eq!(tasks[2].end_time, Some(t_start_d)); // Ends at 11:00

        // 4. Break D (11:00-11:30) - The inserted break
        assert_eq!(tasks[3].name, "Break");
        assert!(tasks[3].is_generated);
        assert_eq!(tasks[3].start_time, t_start_d);
        assert_eq!(tasks[3].end_time, Some(t_end_d));

        // 5. Task C Part 2 (11:30-12:00) - The tail created by resolve_start_overlaps
        assert_eq!(tasks[4].name, "Task C");
        assert_eq!(tasks[4].start_time, t_end_d); // Starts at 11:30
        assert_eq!(tasks[4].end_time, Some(t_end_c));

    }

    #[test]
    fn test_overlap_insert_break_into_task_open_ended() {
        // Scenario:
        // 1. Task A [09:00 - 10:00]
        // 2. Break B [10:00 - 10:30]
        // 3. Task C [10:30 - 12:00]
        // 4. Insert Break D [11:00 - 11:30] into Task C.
        //
        // Result should be 4 closed tasks:
        // A [09:00 - 10:00]
        // B [10:00 - 10:30]
        // C1 [10:30 - 11:00]
        // D [11:00 - 11:30]
        // C2 [11:30 - 12:00]

        let t_start_a = dt("2023-10-27 09:00:00");
        let t_end_a = dt("2023-10-27 10:00:00");
        let t_start_b = dt("2023-10-27 10:00:00"); // Manual break start
        let t_end_b = dt("2023-10-27 10:30:00"); // Manual break end
        let t_start_c = dt("2023-10-27 10:30:00");
        let t_start_d = dt("2023-10-27 11:00:00"); // Inserted break start
        let t_end_d = dt("2023-10-27 11:30:00"); // Inserted break end

        let events = vec![
            // 1. Task A
            Event::TaskStarted { id: 1, name: "Task A".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_a, is_generated: false },
            Event::Stopped { end_time: t_end_a, is_generated: false },

            // 2. Break B
            Event::BreakStarted { start_time: t_start_b, is_generated: false },
            Event::Stopped { end_time: t_end_b, is_generated: false },

            // 3. Task C
            Event::TaskStarted { id: 3, name: "Task C".into(), project_name: "P".into(), customer_name: "C".into(), rate: 10.0, start_time: t_start_c, is_generated: false },

            // --- Inserted Break D (simulates the autobreak logic) ---
            // This break will "punch a hole" in Task C
            Event::BreakStarted { start_time: t_start_d, is_generated: true }, // Use is_generated: true for better context
            Event::Stopped { end_time: t_end_d, is_generated: true },
        ];

        let mut tasks = restore_state(&events);
        let day_projects_refs: Vec<&Task> = tasks.iter().collect();
        render_day(&day_projects_refs);
        // Ensure chronological order
        tasks.sort_by_key(|t| t.start_time);

        assert_eq!(tasks.len(), 5, "There should be 5 tasks: A, B, C1, D, C2");

        // 1. Task A (09:00-10:00) - Untouched
        assert_eq!(tasks[0].name, "Task A");
        assert_eq!(tasks[0].start_time, t_start_a);
        assert_eq!(tasks[0].end_time, Some(t_end_a));

        // 2. Break B (10:00-10:30) - Untouched
        assert_eq!(tasks[1].name, "Break");
        assert!(tasks[1].is_break);
        assert_eq!(tasks[1].start_time, t_start_b);
        assert_eq!(tasks[1].end_time, Some(t_end_b));

        // 3. Task C Part 1 (10:30-11:00) - Cut short by resolve_start_overlaps
        assert_eq!(tasks[2].name, "Task C");
        assert_eq!(tasks[2].start_time, t_start_c);
        assert_eq!(tasks[2].end_time, Some(t_start_d)); // Ends at 11:00

        // 4. Break D (11:00-11:30) - The inserted break
        assert_eq!(tasks[3].name, "Break");
        assert!(tasks[3].is_generated);
        assert_eq!(tasks[3].start_time, t_start_d);
        assert_eq!(tasks[3].end_time, Some(t_end_d));

        // 5. Task C Part 2 (11:30>) - The tail created by resolve_start_overlaps
        assert_eq!(tasks[4].name, "Task C");
        assert_eq!(tasks[4].start_time, t_end_d); // Starts at 11:30
        assert_eq!(tasks[4].end_time, None);

    }
}