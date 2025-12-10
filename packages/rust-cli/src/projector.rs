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
    let last_clear_index = effective_events
        .iter()
        .rposition(|e| matches!(e, Event::LocallyCleared { .. }));

    // 3. Filter the stream: Keep only events that occurred AFTER the last clear.
    let starting_index = last_clear_index.map(|i| i + 1).unwrap_or(0);
    let final_effective_events = &effective_events[starting_index..];

    // 4. Process the final filtered and ordered events
    for event in final_effective_events {
        process_event(&mut projects, event);
    }

    // 5. Sort by start time
    projects.sort_by_key(|p| p.start_time);

    // 6. Join Adjacent Tasks (Cleanup fragmentation)
    join_adjacent_tasks(&mut projects);

    // 7. Remove Zero-Duration Tasks (and clean up artifacts)
    projects.retain(|task| {
        match task.end_time {
            Some(end) => {
                let duration = end.signed_duration_since(task.start_time);
                // Keep if duration is positive and significant (e.g., >= 1 min)
                duration >= Duration::minutes(1)
            }
            None => true,
        }
    });

    // 8. Remove breaks at the start/end of the day
    remove_edge_breaks(&mut projects);

    // 9. Handle Running Tasks in Past/Future (Auto-Close)
    // If a task is still running, but it's not from "Today", we must close it.
    let now = Local::now();
    let today = now.date_naive();

    // Calculate how much work has already been done today (excluding breaks and running tasks)
    let total_worked_duration: Duration = projects
        .iter()
        .filter(|t| !t.is_break && t.end_time.is_some())
        .map(|t| t.duration())
        .sum();

    let target_work_hours = Duration::minutes(450); // 7.5 hours
    let min_duration = Duration::minutes(15);

    for task in &mut projects {
        if task.end_time.is_none() {
            let task_date = task.start_time.date_naive();

            // If the task is not from today (it is past or future relative to system time)
            if task_date != today {

                let needed_to_reach_target = if total_worked_duration < target_work_hours {
                    target_work_hours - total_worked_duration
                } else {
                    Duration::zero()
                };

                let duration_to_apply = if needed_to_reach_target > min_duration {
                    needed_to_reach_target
                } else {
                    min_duration
                };

                let candidate_end = task.start_time + duration_to_apply;

                // Ensure we don't spill over into the next day just to satisfy the 7.5h rule.
                // If the calculated end time is tomorrow, clamp it to 23:59:59 or just use 15m.
                if candidate_end.date_naive() == task_date {
                    task.end_time = Some(candidate_end);
                } else {
                    // Fallback: If adding the needed time pushes us to tomorrow,
                    // just close it short (15m) to keep the record clean.
                    task.end_time = Some(task.start_time + min_duration);
                }
            }
        }
    }

    projects
}

fn join_adjacent_tasks(projects: &mut Vec<Task>) {
    if projects.is_empty() {
        return;
    }

    let incoming_tasks = std::mem::take(projects);
    let mut merged: Vec<Task> = Vec::with_capacity(incoming_tasks.len());

    for task in incoming_tasks {
        match merged.last_mut() {
            Some(last) => {
                let is_same_entity = last.id == task.id
                    && last.is_break == task.is_break
                    && last.project_name == task.project_name;

                let is_contiguous = last.end_time == Some(task.start_time);

                if is_same_entity && is_contiguous {
                    last.end_time = task.end_time;
                } else {
                    merged.push(task);
                }
            }
            None => merged.push(task),
        }
    }

    *projects = merged;
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

            // Ensure only one task is running at a time.
            // If there is a running task, close it at the start time of this new task.
            close_last_project(projects, *start_time);

            projects.push(Task {
                id: *id,
                project_name: project_name.clone(),
                customer_name: customer_name.to_string(),
                rate: *rate,
                name: name.clone(),
                comment: None,
                start_time: *start_time,
                end_time: None,
                is_break: false,
            });
        }
        Event::BreakStarted { start_time } => {
            resolve_start_overlaps(projects, *start_time);
            close_last_project(projects, *start_time);

            projects.push(Task {
                id: -1,
                project_name: String::new(),
                customer_name: String::new(),
                rate: 0.0,
                name: "Break".to_string(),
                comment: None,
                start_time: *start_time,
                end_time: None,
                is_break: true,
            });
        }
        Event::Stopped { end_time, .. } => {
            // FIX: Identify the running task by searching for end_time == None.
            // The projects vector might be unordered due to split tails being appended.
            let running_task_idx = projects.iter().rposition(|t| t.end_time.is_none());

            // If no task is running, this Stop event is redundant. Ignore it.
            if running_task_idx.is_none() {
                return;
            }

            // Capture metadata before we mutate or close
            let was_break = running_task_idx
                .map(|i| projects[i].is_break)
                .unwrap_or(false);

            let killer_start = running_task_idx.map(|i| projects[i].start_time);

            close_last_project(projects, *end_time);

            // Clean up overlaps based on the task we just identified
            if let Some(start) = killer_start {
                resolve_stop_overlaps(projects, start, *end_time);
            }

            // --- Implicit Auto-Reopen after Break Stops ---
            if was_break {
                // Find the ID of the last non-break task.
                let target_id = projects
                    .iter()
                    .rev()
                    .find(|t| !t.is_break)
                    .map(|t| t.id);

                if let (Some(id), Some(resume_time)) =
                    (target_id, projects.last().and_then(|t| t.end_time))
                {
                    // Find metadata of the task to resume
                    let task_to_reopen = projects
                        .iter()
                        .filter(|t| t.id == id && !t.is_break)
                        .last()
                        .cloned();

                    if let Some(task) = task_to_reopen {
                        // Ensure we don't create zero-gap duplicates
                        let last_segment_of_id = projects
                            .iter()
                            .rev()
                            .find(|t| t.id == id && !t.is_break);

                        if last_segment_of_id.map(|t| t.end_time) != Some(Some(resume_time)) {
                            projects.push(Task {
                                id: task.id,
                                project_name: task.project_name,
                                customer_name: task.customer_name,
                                rate: task.rate,
                                name: task.name,
                                comment: None,
                                start_time: resume_time,
                                end_time: None, // Starts running
                                is_break: false,
                            });
                        }
                    }
                }
            }
        }
        Event::Undo { .. } => {}
        Event::Redo { .. } => {}
        Event::CommentAdded { task_id, comment, .. } => {
            for task in projects.iter_mut() {
                if task.id == *task_id {
                    task.comment = Some(comment.clone());
                }
            }
        }
        Event::CommentRemoved { task_id, .. } => {
            for task in projects.iter_mut() {
                if task.id == *task_id {
                    task.comment = None;
                }
            }
        }
        Event::LocallyCleared { .. } => {}
    }
}

/// Resolves overlaps triggered by Stopping a task.
/// It deletes tasks fully contained within [killer_start, killer_end).
/// It truncates tasks that overlap partially.
fn resolve_stop_overlaps(
    projects: &mut Vec<Task>,
    killer_start: chrono::DateTime<Local>,
    killer_end: chrono::DateTime<Local>,
) {
    if projects.is_empty() {
        return;
    }

    // 1. Remove tasks fully swallowed by the stop range.
    projects.retain(|task| {
        // Protect the task that caused the stop (Self)
        let is_killer = task.start_time == killer_start && task.end_time == Some(killer_end);
        if is_killer {
            return true;
        }

        let t_start = task.start_time;
        // If task starts inside the killer range
        if t_start >= killer_start && t_start < killer_end {
            // And ends (or would end) inside the killer range
            if let Some(t_end) = task.end_time {
                if t_end <= killer_end {
                    return false; // DELETE
                }
            }
        }
        true
    });

    // 2. Adjust start times for tasks that start inside but end outside
    for task in projects.iter_mut() {
        // Protect the killer task
        if task.start_time == killer_start && task.end_time == Some(killer_end) {
            continue;
        }

        if task.start_time >= killer_start && task.start_time < killer_end {
            task.start_time = killer_end;
        }
    }
}

fn resolve_start_overlaps(projects: &mut Vec<Task>, new_start: chrono::DateTime<Local>) {
    let mut tails = Vec::new();

    for task in projects.iter_mut() {
        // Ignore currently running tasks (end_time is None) -> handled by close_last_project
        if task.end_time.is_none() {
            continue;
        }

        let t_start = task.start_time;
        let t_end = task.end_time.unwrap();

        // If the new task starts in the middle of an existing finished task
        if t_start < new_start && t_end > new_start {
            // Create the tail (the part after the new start)
            let mut tail = task.clone();
            tail.start_time = new_start;
            tail.end_time = Some(t_end);
            tails.push(tail);

            // Truncate the current task
            task.end_time = Some(new_start);
        }
    }

    projects.extend(tails);
}

fn close_last_project(projects: &mut Vec<Task>, time: chrono::DateTime<Local>) {
    let running_task_idx = projects.iter().rposition(|t| t.end_time.is_none());

    let last_idx = match running_task_idx {
        Some(idx) => idx,
        None => return,
    };

    let start_time = projects[last_idx].start_time;
    let end_time = time;

    // Sanity check: prevent closing before start
    if end_time < start_time {
        return;
    }

    if start_time.date_naive() == end_time.date_naive() {
        projects[last_idx].end_time = Some(end_time);
    } else {
        // Handle multi-day split
        let name = projects[last_idx].name.clone();
        let project_name = projects[last_idx].project_name.clone();
        let customer_name = projects[last_idx].customer_name.clone();
        let rate = projects[last_idx].rate;
        let id = projects[last_idx].id;
        let is_break = projects[last_idx].is_break;

        let next_day_midnight = (start_time.date_naive() + Duration::days(1))
            .and_hms_opt(0, 0, 0)
            .unwrap();
        let next_day_local = Local.from_local_datetime(&next_day_midnight).unwrap();

        projects[last_idx].end_time = Some(next_day_local);

        let mut current_start = next_day_local;
        while current_start.date_naive() < end_time.date_naive() {
            let next_day = (current_start.date_naive() + Duration::days(1))
                .and_hms_opt(0, 0, 0)
                .unwrap();
            let next_day_local = Local.from_local_datetime(&next_day).unwrap();

            projects.push(Task {
                id,
                project_name: project_name.clone(),
                customer_name: customer_name.clone(),
                rate,
                name: name.clone(),
                comment: None,
                start_time: current_start,
                end_time: Some(next_day_local),
                is_break,
            });
            current_start = next_day_local;
        }

        projects.push(Task {
            id,
            project_name,
            customer_name,
            rate,
            name,
            comment: None,
            start_time: current_start,
            end_time: Some(end_time),
            is_break,
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
    use chrono::{Local, NaiveDate};

    // Helper to make times readable
    fn dt(hour: u32, min: u32) -> chrono::DateTime<Local> {
        Local::now()
            .date_naive()
            .and_hms_opt(hour, min, 0)
            .unwrap()
            .and_local_timezone(Local)
            .unwrap()
    }

    // Helper to make times on specific dates readable (for past/future testing)
    fn dt_date(date: NaiveDate, hour: u32, min: u32) -> chrono::DateTime<Local> {
        date.and_hms_opt(hour, min, 0)
            .unwrap()
            .and_local_timezone(Local)
            .unwrap()
    }

    fn task_start(id: i32, hour: u32, min: u32) -> Event {
        Event::TaskStarted {
            id,
            name: format!("Task {}", id),
            project_name: "Proj".to_string(),
            customer_name: "Cust".to_string(),
            rate: 100.0,
            start_time: dt(hour, min),
        }
    }

    fn task_start_date(id: i32, date: NaiveDate, hour: u32, min: u32) -> Event {
        Event::TaskStarted {
            id,
            name: format!("Task {}", id),
            project_name: "Proj".to_string(),
            customer_name: "Cust".to_string(),
            rate: 100.0,
            start_time: dt_date(date, hour, min),
        }
    }

    #[test]
    fn test_simple_day_flow() {
        let events = vec![
            task_start(1, 9, 0),
            Event::Stopped { end_time: dt(10, 0)}, // Explicit Stop
            task_start(2, 10, 0),
            Event::Stopped { end_time: dt(11, 0) },
        ];

        let tasks = restore_state(&events);
        assert_eq!(tasks.len(), 2);
        assert_eq!(tasks[0].start_time, dt(9, 0));
        assert_eq!(tasks[0].end_time, Some(dt(10, 0)));
        assert_eq!(tasks[1].start_time, dt(10, 0));
    }

    #[test]
    fn test_multiple_stops_ignored() {
        // User hits Stop multiple times.
        // 1. Task A starts 09:00
        // 2. Stop 10:00 (Closes A)
        // 3. Stop 11:00 (Should be ignored, no task running)
        // 4. Stop 12:00 (Should be ignored)

        let events = vec![
            task_start(1, 9, 0),
            Event::Stopped { end_time: dt(10, 0) },
            Event::Stopped { end_time: dt(11, 0) },
            Event::Stopped { end_time: dt(12, 0) },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].id, 1);
        assert_eq!(tasks[0].end_time, Some(dt(10, 0)));
        // If 11:00 or 12:00 were processed, the end time might have been overwritten
        // or errors thrown.
    }

    #[test]
    fn test_multiple_starts_auto_close() {
        // User switches tasks without stopping explicitly.
        // 1. Start A (09:00)
        // 2. Start B (10:00) -> Should close A at 10:00
        // 3. Start C (11:00) -> Should close B at 11:00

        let events = vec![
            task_start(1, 9, 0),
            task_start(2, 10, 0),
            task_start(3, 11, 0),
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 3);

        // Task A
        assert_eq!(tasks[0].id, 1);
        assert_eq!(tasks[0].start_time, dt(9, 0));
        assert_eq!(tasks[0].end_time, Some(dt(10, 0)));

        // Task B
        assert_eq!(tasks[1].id, 2);
        assert_eq!(tasks[1].start_time, dt(10, 0));
        assert_eq!(tasks[1].end_time, Some(dt(11, 0)));

        // Task C (Running)
        assert_eq!(tasks[2].id, 3);
        assert_eq!(tasks[2].start_time, dt(11, 0));
        assert_eq!(tasks[2].end_time, None);
    }

    #[test]
    fn test_past_running_task_auto_close_standard() {
        // Scenario: Viewing "Yesterday's" history.
        // A task was started at 09:00 and never stopped.
        // It should implicitly close at 09:00 + 7.5h = 16:30.

        let yesterday = Local::now().date_naive() - Duration::days(1);

        let events = vec![
            task_start_date(1, yesterday, 9, 0)
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].start_time, dt_date(yesterday, 9, 0));

        // Expect 7.5 hours duration
        let expected_end = dt_date(yesterday, 9, 0) + Duration::minutes(450); // 16:30
        assert_eq!(tasks[0].end_time, Some(expected_end));
    }

    #[test]
    fn test_past_running_task_auto_close_late() {
        // Scenario: Viewing "Yesterday's" history.
        // A task was started very late (23:00) and never stopped.
        // 7.5 hours would push it to tomorrow (Today).
        // Fallback logic should close it after 15 minutes to keep it in the day.

        let yesterday = Local::now().date_naive() - Duration::days(1);

        let events = vec![
            task_start_date(1, yesterday, 23, 0)
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].start_time, dt_date(yesterday, 23, 0));

        // Expect 15 minutes duration fallback
        let expected_end = dt_date(yesterday, 23, 15);
        assert_eq!(tasks[0].end_time, Some(expected_end));
    }

    #[test]
    fn test_break_sandwich_auto_resume() {
        // Scenario: Work -> Break -> (Auto Resume) -> Stop
        // User report: "Closing the whole task removed previous one up to break"
        let events = vec![
            task_start(1, 9, 0),
            Event::BreakStarted { start_time: dt(12, 0) }, // Implicitly stops Task 1
            Event::Stopped { end_time: dt(12, 30) }, // Stops break, Auto-resumes Task 1
            Event::Stopped { end_time: dt(17, 0)}, // Stops the Resumed Task 1
        ];

        let tasks = restore_state(&events);

        // Expected:
        // 1. Task 1: 09:00 - 12:00
        // 2. Break:  12:00 - 12:30
        // 3. Task 1: 12:30 - 17:00

        assert_eq!(tasks.len(), 3, "Should have Morning, Break, Afternoon");

        assert_eq!(tasks[0].name, "Task 1");
        assert_eq!(tasks[0].end_time, Some(dt(12, 0)));

        assert!(tasks[1].is_break);
        assert_eq!(tasks[1].start_time, dt(12, 0));
        assert_eq!(tasks[1].end_time, Some(dt(12, 30)));

        assert_eq!(tasks[2].name, "Task 1");
        assert_eq!(tasks[2].start_time, dt(12, 30));
        assert_eq!(tasks[2].end_time, Some(dt(17, 0)));
    }

    #[test]
    fn test_break_sandwich_merged() {
        // If sorting and joining works correctly, adjacent tasks might merge
        // but here the break interrupts them, so they shouldn't merge.
        let events = vec![
            task_start(1, 9, 0),
            Event::BreakStarted { start_time: dt(12, 0) },
            Event::Stopped { end_time: dt(12, 30) },
            Event::Stopped { end_time: dt(17, 0)},
        ];

        let tasks = restore_state(&events);
        assert_eq!(tasks.len(), 3); // Break prevents merging
    }

    #[test]
    fn test_overlap_deletion_logic() {
        // Scenario:
        // 1. Task A runs 10:00 - 12:00
        // 2. User mistakenly enters a stop for A at 14:00 (overlapping next task?)
        // Let's try:
        // Task A: 10:00 - 12:00
        // Task B: 12:00 - 13:00 (Running)
        // Event Stopped at 14:00 (Closes B at 14:00).
        // Should NOT delete A.

        let events = vec![
            task_start(1, 10, 0),
            task_start(2, 12, 0), // Implicitly closes A at 12:00
            Event::Stopped { end_time: dt(14, 0)},
        ];

        let tasks = restore_state(&events);

        // Task A: 10-12
        // Task B: 12-14
        assert_eq!(tasks.len(), 2);
        assert_eq!(tasks[0].end_time, Some(dt(12, 0)));
        assert_eq!(tasks[1].start_time, dt(12, 0));
        assert_eq!(tasks[1].end_time, Some(dt(14, 0)));
    }

    #[test]
    fn test_aggressive_overlap_bug() {
        // This attempts to reproduce the bug where "previous one was removed".
        // This happens if killer_start is calculated incorrectly.

        // 1. Start Task A (09:00)
        // 2. Start Break (12:00)
        // 3. Stop Break (12:30) -> Auto Resume Task A
        // 4. Stop Task A (17:00)

        // If logic is wrong, step 4 might delete the Break or Morning Task A.

        let mut projects = Vec::new();

        // Manually stepping through process_event to ensure internal state is correct
        process_event(&mut projects, &task_start(1, 9, 0)); // Task A Running
        process_event(&mut projects, &Event::BreakStarted { start_time: dt(12, 0) }); // Task A closed, Break Running
        process_event(&mut projects, &Event::Stopped { end_time: dt(12, 30) }); // Break closed, Task A Resumed

        // At this point:
        // A: 9:00-12:00
        // B: 12:00-12:30
        // A: 12:30-Running
        assert_eq!(projects.len(), 3);
        assert_eq!(projects[2].start_time, dt(12, 30));

        // The Stop Event
        process_event(&mut projects, &Event::Stopped { end_time: dt(17, 0), });

        // killer_start should be 12:30.
        // It should NOT affect A(9-12) or B(12-12:30) because they end <= 12:30.

        assert_eq!(projects.len(), 3);
        assert_eq!(projects[0].end_time, Some(dt(12, 0))); // Morning safe
        assert_eq!(projects[1].end_time, Some(dt(12, 30))); // Break safe
        assert_eq!(projects[2].end_time, Some(dt(17, 0))); // Afternoon closed
    }

    #[test]
    fn test_fragmented_inserts() {
        // User inserts an event in the past that causes overlaps
        // Task A: 10-11
        // Insert Task B: 10:30 (Running)
        // Stop Task B: 11:30

        let events = vec![
            task_start(1, 10, 0),
            Event::Stopped { end_time: dt(11, 0),},
            // Now start something in the middle
            task_start(2, 10, 30),
            Event::Stopped { end_time: dt(11, 30), },
        ];

        let tasks = restore_state(&events);

        // 1. Task A starts 10:00.
        // 2. Task B starts 10:30.
        //    TaskStarted logic calls resolve_start_overlaps(10:30).
        //    Task A (10-11) is split?
        //    resolve_start_overlaps splits IF new start is strictly inside.
        //    10:30 is inside 10:00-11:00.
        //    Task A becomes 10:00-10:30.
        //    Tail A' becomes 10:30-11:00.
        //    Task B is added 10:30-Running.

        // 3. Stop B at 11:30.
        //    Closes B at 11:30.
        //    resolve_stop_overlaps(killer_start=10:30, end=11:30).
        //    Tail A' (10:30-11:00) is fully inside killer range.
        //    Tail A' should be DELETED.

        // Expected Result:
        // Task A: 10:00 - 10:30
        // Task B: 10:30 - 11:30

        assert_eq!(tasks.len(), 2);
        assert_eq!(tasks[0].id, 1);
        assert_eq!(tasks[0].end_time, Some(dt(10, 30)));
        assert_eq!(tasks[1].id, 2);
        assert_eq!(tasks[1].start_time, dt(10, 30));
        assert_eq!(tasks[1].end_time, Some(dt(11, 30)));
    }
    #[test]
    fn test_undo_redo_flow() {
        // Scenario:
        // 1. Start Task A
        // 2. Start Task B (Oops, mistake)
        // 3. Undo (Removes Start B)
        // 4. Stop (Should apply to Task A)
        let events = vec![
            task_start(1, 9, 0),
            task_start(2, 9, 30), // Mistake
            Event::Undo { time: dt(9, 31) }, // Undoes Task 2 Start
            Event::Stopped { end_time: dt(10, 0) }, // Should close Task 1
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].id, 1);
        assert_eq!(tasks[0].end_time, Some(dt(10, 0)));
    }

    #[test]
    fn test_undo_redo_complex() {
        // Scenario: Start A -> Undo -> Redo -> Stop
        let events = vec![
            task_start(1, 9, 0),
            Event::Undo { time: dt(9, 1) }, // Stack: [Start A]
            Event::Redo { time: dt(9, 2) }, // Stack empty, Event applied
            Event::Stopped { end_time: dt(10, 0) },
        ];

        let tasks = restore_state(&events);
        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].id, 1);
    }

    #[test]
    fn test_noise_filtering() {
        // Scenario: Tasks shorter than 1 minute should be discarded.
        let events = vec![
            task_start(1, 9, 0),
            Event::Stopped { end_time: dt(9, 0) + Duration::seconds(30) }, // 30s duration
            task_start(2, 10, 0),
            Event::Stopped { end_time: dt(10, 0) + Duration::minutes(5) }, // 5m duration
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1, "Short task should be filtered out");
        assert_eq!(tasks[0].id, 2);
    }

    #[test]
    fn test_local_clear_state() {
        // Scenario: Events exist, then Clear happens, then new events.
        // Old events should be ignored.
        let events = vec![
            task_start(1, 8, 0),
            Event::Stopped { end_time: dt(9, 0) },
            Event::LocallyCleared { date: NaiveDate::MIN }, // RESET
            task_start(2, 10, 0),
            Event::Stopped { end_time: dt(11, 0) },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1, "Events before clear should be ignored");
        assert_eq!(tasks[0].id, 2);
        assert_eq!(tasks[0].start_time, dt(10, 0));
    }

    #[test]
    fn test_the_eclipse_swallowing_tasks() {
        // Scenario: "Eclipse"
        // 1. Task A exists (10:00 - 11:00).
        // 2. User later enters a massive task B (09:00 - 12:00) that completely swallows A.

        let events = vec![
            task_start(1, 10, 0),
            Event::Stopped { end_time: dt(11, 0) },
            // Insert B start BEFORE A
            task_start(2, 9, 0),
            // Stop B AFTER A
            Event::Stopped { end_time: dt(12, 0) },
        ];

        let tasks = restore_state(&events);

        // Logic check:
        // 1. A created (10-11).
        // 2. B starts (09:00). B is pushed to list. (A is not touched by resolve_start because A starts > 09:00).
        // 3. B stops (12:00). resolve_stop_overlaps(killer_start=09:00, killer_end=12:00).
        //    A (10-11) is strictly inside [09-12). A should be deleted.

        assert_eq!(tasks.len(), 1, "Task A should be swallowed by Task B");
        assert_eq!(tasks[0].id, 2);
        assert_eq!(tasks[0].start_time, dt(9, 0));
        assert_eq!(tasks[0].end_time, Some(dt(12, 0)));
    }
    #[test]
    fn test_regression_displaced_tail_overlap() {
        // Scenario: A "tail" of a split task ends up at the end of the vector.
        // Then we stop a different task.
        // 1. Start Task A (06:00 - 10:00).
        // 2. Start Task B (12:00 Running).
        // 3. Insert Task C in the past (08:00 - 09:00).
        //    - This splits A.
        //    - A becomes 06:00-08:00.
        //    - Tail A' (09:00-10:00) is appended to vector end.
        // 4. Stop Task B at 13:00.
        //    - Previously, code would think Tail A' (starts 09:00) was the killer.
        //    - It would delete anything inside 09:00 - 13:00.
        //    - It would mangle B.

        let events = vec![
            task_start(1, 6, 0),
            Event::Stopped { end_time: dt(10, 0) },
            task_start(2, 12, 0), // Task 2 Running
            // Insert Task 3 in the middle of Task 1
            task_start(3, 8, 0),
            Event::Stopped { end_time: dt(9, 0) },
            // Stop Task 2
            Event::Stopped { end_time: dt(13, 0) },
        ];

        let tasks = restore_state(&events);

        // Expected:
        // Task 1: 06:00 - 08:00
        // Task 3: 08:00 - 09:00
        // Task 1 (Tail): 09:00 - 10:00 -> This was at risk of deletion
        // Task 2: 12:00 - 13:00

        let t1_tail = tasks
            .iter()
            .find(|t| t.id == 1 && t.start_time == dt(9, 0));
        assert!(t1_tail.is_some(), "The tail of Task 1 should survive");

        let t2 = tasks.iter().find(|t| t.id == 2);
        assert!(t2.is_some());
        assert_eq!(t2.unwrap().end_time, Some(dt(13, 0)));
    }

    #[test]
    fn test_past_fill_full_7_5_hours() {
        // Only one task running from 09:00. Total work = 0.
        // Should fill full 7.5 hours.
        let yesterday = Local::now().date_naive() - Duration::days(1);
        let events = vec![task_start_date(1, yesterday, 9, 0)];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        let expected_end = dt_date(yesterday, 9, 0) + Duration::minutes(450); // 16:30
        assert_eq!(tasks[0].end_time, Some(expected_end));
    }

    #[test]
    fn test_past_fill_partial_hours() {
        // Task A: 09:00 - 15:00 (6 hours)
        // Task B: 15:00 - Running
        // Already have 6 hours. Target is 7.5. Need 1.5 hours more.
        let yesterday = Local::now().date_naive() - Duration::days(1);
        let events = vec![
            task_start_date(1, yesterday, 9, 0),
            Event::Stopped { end_time: dt_date(yesterday, 15, 0) },
            task_start_date(2, yesterday, 15, 0)
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);
        // Task B should close after 1.5 hours (15:00 + 1:30 = 16:30)
        let expected_end = dt_date(yesterday, 16, 30);
        assert_eq!(tasks[1].end_time, Some(expected_end));
    }

    #[test]
    fn test_past_fill_already_over_target() {
        // Task A: 08:00 - 16:00 (8 hours)
        // Task B: 16:00 - Running
        // Already > 7.5 hours.
        // Task B should default to 15 minutes.
        let yesterday = Local::now().date_naive() - Duration::days(1);
        let events = vec![
            task_start_date(1, yesterday, 8, 0),
            Event::Stopped { end_time: dt_date(yesterday, 16, 0) },
            task_start_date(2, yesterday, 16, 0)
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);
        // Task B closes at 16:15
        let expected_end = dt_date(yesterday, 16, 15);
        assert_eq!(tasks[1].end_time, Some(expected_end));
    }

    #[test]
    fn test_past_spillover_protection() {
        // Started very late (23:00). Needs 7.5 hours ideally.
        // But 23:00 + 7.5h spills into next day.
        // Should fallback to 15m.
        let yesterday = Local::now().date_naive() - Duration::days(1);
        let events = vec![task_start_date(1, yesterday, 23, 0)];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        let expected_end = dt_date(yesterday, 23, 15);
        assert_eq!(tasks[0].end_time, Some(expected_end));
    }

    #[test]
    fn test_comment_override() {
        // Multiple comments on same task - last one wins
        let events = vec![
            task_start(1, 9, 0),
            Event::Stopped { end_time: dt(10, 0) },
            Event::CommentAdded {
                task_id: 1,
                comment: "First comment".to_string(),
                date: dt(10, 1).date_naive(),
            },
            Event::CommentAdded {
                task_id: 1,
                comment: "Updated comment".to_string(),
                date: dt(10, 2).date_naive(),
            },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].comment, Some("Updated comment".to_string()));
    }

    #[test]
    fn test_comment_removed() {
        let events = vec![
            task_start(1, 9, 0),
            Event::Stopped { end_time: dt(10, 0) },
            Event::CommentAdded {
                task_id: 1,
                comment: "Temporary note".to_string(),
                date: dt(10, 1).date_naive(),
            },
            Event::CommentRemoved {
                task_id: 1,
                date: dt(10, 2).date_naive(),
            },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].comment, None);
    }

    #[test]
    fn test_comment_only_affects_matching_id() {
        let events = vec![
            task_start(1, 9, 0),
            Event::Stopped { end_time: dt(10, 0) },
            task_start(2, 10, 0),
            Event::Stopped { end_time: dt(11, 0) },
            Event::CommentAdded {
                task_id: 1,
                comment: "Only for task 1".to_string(),
                date: dt(11, 1).date_naive(),
            },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 2);
        assert_eq!(tasks[0].comment, Some("Only for task 1".to_string()));
        assert_eq!(tasks[1].comment, None);
    }

    #[test]
    fn test_comment_with_undo() {
        // Comment is added, then undone
        let events = vec![
            task_start(1, 9, 0),
            Event::Stopped { end_time: dt(10, 0) },
            Event::CommentAdded {
                task_id: 1,
                comment: "Should be undone".to_string(),
                date: dt(10, 1).date_naive(),
            },
            Event::Undo { time: dt(10, 2) },
        ];

        let tasks = restore_state(&events);

        assert_eq!(tasks.len(), 1);
        assert_eq!(tasks[0].comment, None, "Comment should be undone");
    }

    #[test]
    fn test_comment_after_split() {
        // Add comment after task has been split by another task
        let events = vec![
            task_start(1, 10, 0),
            Event::Stopped { end_time: dt(12, 0) },
            // Insert Task 2 in the middle, which splits Task 1
            task_start(2, 11, 0),
            Event::Stopped { end_time: dt(11, 30) },
            // Add comment to task 1 after the split
            Event::CommentAdded {
                task_id: 1,
                comment: "Split task comment".to_string(),
                date: dt(12, 1).date_naive(),
            },
        ];

        let tasks = restore_state(&events);

        let task1_segments: Vec<_> = tasks.iter().filter(|t| t.id == 1).collect();

        // Task 1 should be split into 10:00-11:00 and 11:30-12:00
        assert_eq!(task1_segments.len(), 2);

        for segment in task1_segments {
            assert_eq!(
                segment.comment,
                Some("Split task comment".to_string()),
                "Both segments should have the comment"
            );
        }
    }
}