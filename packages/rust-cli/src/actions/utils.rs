use chrono::{Local, NaiveDate};
use crate::{external_models, models};
use crate::events::Event;
use crate::external_models::TaskDto;

pub fn insert_and_resolve_overlaps(
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

pub fn parse_time(date: NaiveDate, time_str: &str) -> Option<chrono::DateTime<Local>> {
    let parts: Vec<&str> = time_str.trim().split(':').collect();
    if parts.len() != 2 {
        return None;
    }

    let hour = parts[0].parse::<u32>().ok()?;
    let min = parts[1].parse::<u32>().ok()?;

    let naive = date.and_hms_opt(hour, min, 0)?;
    Some(naive.and_local_timezone(Local).unwrap())
}


pub fn generate_events_from_server_entries(
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
