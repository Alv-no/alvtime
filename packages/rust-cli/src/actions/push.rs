use crate::actions::utils::{generate_events_from_server_entries, round_duration_to_quarter_hour};
use crate::alvtime::AlvtimeClient;
use crate::events::Event;
use crate::external_models::{TaskDto, TimeEntryDto};
use crate::models::Task;
use crate::projector::restore_state;
use crate::store::EventStore;
use crate::view::render_day;
use inquire::Select;
use std::collections::{HashMap, HashSet};

pub fn handle_push(
    client: &AlvtimeClient,
    tasks: &mut Vec<Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    external_tasks: &[TaskDto],
) -> String {
    // 1. Extract the date from the tasks/history
    let date = if let Some(task) = tasks.first() {
        task.start_time.date_naive()
    } else if let Some(event) = history.first() {
        event.date()
    } else {
        return "No events to push.".to_string();
    };

    let entries = match client.list_time_entries(date, date) {
        Ok(e) => e,
        Err(e) => return format!("Failed to fetch entries for {}: {}", date, e),
    };

    let day_tasks: Vec<&Task> = tasks.iter().filter(|t| !t.is_break).collect();

    let has_server_entries = !entries.is_empty();

    if day_tasks.is_empty() && !has_server_entries {
        return String::new();
    }

    let mut should_push = true;

    // Conflict Check and Resolution
    if has_server_entries {
        let server_hours: f64 = entries.iter().map(|e| e.value).sum();

        let mut local_sums: HashMap<i32, i64> = HashMap::new();
        for t in &day_tasks {
            *local_sums.entry(t.id).or_default() += t.duration().num_minutes();
        }
        let local_hours: f64 = local_sums
            .values()
            .map(|m| round_duration_to_quarter_hour(*m))
            .sum();

        // 5a. Generate Server's current Task state
        let new_events_server = generate_events_from_server_entries(
            date,
            &entries.iter().collect::<Vec<_>>(),
            external_tasks,
        );
        let server_tasks = restore_state(&new_events_server);

        // 5b. Generate Local's PUSHED TimeEntryDto state (Apply rounding/aggregation)
        let mut local_time_entries_to_compare: Vec<TimeEntryDto> = Vec::new();
        for (task_id, minutes) in &local_sums {
            local_time_entries_to_compare.push(TimeEntryDto {
                // ID is irrelevant for comparison and should be ignored/normalized
                id: 0,
                task_id: *task_id,
                date: date.format("%Y-%m-%d").to_string(),
                value: round_duration_to_quarter_hour(*minutes), // Apply rounding
                comment: None,
            });
        }

        // 5c. Compare Local Pushed State with Server State
        // Normalize server entries (reset ID and sort) for comparison
        let mut normalized_server_entries: Vec<TimeEntryDto> = entries
            .iter()
            .map(|e| TimeEntryDto {
                id: 0, // Reset ID
                task_id: e.task_id,
                date: e.date.clone(),
                value: e.value,
                comment: None,
            })
            .collect();

        // Normalize local entries (sort)
        let mut normalized_local_entries = local_time_entries_to_compare.clone();

        // Sorting both vectors by task_id ensures reliable comparison,
        // ignoring order differences from server response vs local HashMap iteration.
        normalized_server_entries.sort_by_key(|e| e.task_id);
        normalized_local_entries.sort_by_key(|e| e.task_id);

        let are_identical_after_rounding = normalized_local_entries == normalized_server_entries;

        if are_identical_after_rounding {
           event_store.set_day_synced(&date, true);
            return format!(
                "Local changes for {} match server entries after rounding. Marked local history as synced.",
                date
            );
        }
        // Continue with conflict resolution (only if states are different)

        // Then, convert the aggregated/rounded entries back into tasks for the snapshot.
        let new_events_local_pushed = generate_events_from_server_entries(
            date,
            &local_time_entries_to_compare.iter().collect::<Vec<_>>(),
            external_tasks,
        );
        let local_tasks_for_snapshot = restore_state(&new_events_local_pushed);

        // Use the newly generated local snapshot for the comparison view
        render_snapshots(
            &local_tasks_for_snapshot.iter().collect::<Vec<_>>(),
            &server_tasks,
        );

        let msg = format!(
            "Conflict on {}: Local {:.1}h vs Server {:.1}h. What do you want to do?",
            date, local_hours, server_hours
        );

        let options = vec![
            "Overwrite Server (Push Local)",
            "Discard Local (Sync from Server)",
            "Skip Day",
        ];

        let choice = if cfg!(test) {
            "Overwrite Server (Push Local)"
        } else {
            match Select::new(&msg, options).prompt() {
                Ok(c) => c,
                Err(_) => {
                    return format!("Cancelled push for {}.", date);
                }
            }
        };

        match choice {
            "Overwrite Server (Push Local)" => {
                should_push = true;
            }
            "Discard Local (Sync from Server)" => {
                let new_events = generate_events_from_server_entries(
                    date,
                    &entries.iter().collect::<Vec<_>>(),
                    external_tasks,
                );
                event_store.persist_synced_batch(&new_events);
                history.extend_from_slice(&new_events);
                *tasks = restore_state(history);
                return format!("Synced {} from server.", date);
            }
            _ => {
                return format!("Skipped {}.", date);
            }
        }
    }

    // 6. Push Logic
    if should_push {
        // Calculate local task sums (in minutes)
        let mut local_sums: HashMap<i32, i64> = HashMap::new();
        for t in &day_tasks {
            *local_sums.entry(t.id).or_default() += t.duration().num_minutes();
        }

        let local_task_ids: HashSet<i32> = local_sums.keys().copied().collect();
        let mut entries_to_push: Vec<TimeEntryDto> = Vec::new();

        // Remove server entries that don't exist locally (value: 0.0)
        for server_entry in &entries {
            if !local_task_ids.contains(&server_entry.task_id) {
                entries_to_push.push(TimeEntryDto {
                    id: server_entry.id,
                    task_id: server_entry.task_id,
                    date: date.format("%Y-%m-%d").to_string(),
                    value: 0.0,
                    comment: None,
                });
            }
        }

        // Add/update local entries
        for (task_id, minutes) in local_sums {
            let existing_id = entries
                .iter()
                .find(|e| e.task_id == task_id)
                .map(|e| e.id)
                .unwrap_or(0);

            entries_to_push.push(TimeEntryDto {
                id: existing_id,
                task_id,
                date: date.format("%Y-%m-%d").to_string(),
                value: round_duration_to_quarter_hour(minutes),
                comment: None,
            });
        }

        if !entries_to_push.is_empty() {
            if let Err(e) = client.upsert_time_entries(&entries_to_push) {
                return format!("Error pushing {}: {}", date, e);
            }

            event_store.set_day_synced(&date, true);
        }

        format!("Pushed {}.", date)
    } else {
        String::new()
    }
}

// Provided, but modified to take a slice of references for local tasks
fn render_snapshots(local: &[&Task], server: &[Task]) {
    println!("\n--- 💾 Server Snapshot ---");
    let server_refs: Vec<&Task> = server.iter().collect();
    render_day(&server_refs).unwrap();

    println!("\n--- 🖥️ Local Snapshot ---");
    render_day(local).unwrap();
    println!("--------------------------");
}
