use chrono::Local;
use crate::actions::add_event;
use crate::events::Event;
use crate::models;
use crate::store::EventStore;

pub fn autobreak(
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
                        customer_name: prev.customer_name.clone(),
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
