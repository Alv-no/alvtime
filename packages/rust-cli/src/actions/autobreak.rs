use crate::actions::add_event;
use crate::events::Event;
use crate::models;
use crate::store::EventStore;
use chrono::{DateTime, Duration, Local, NaiveDate};

const BREAK_START_HOUR: u32 = 11;
const BREAK_START_MINUTE: u32 = 0;
const BREAK_DURATION_MINUTES: i64 = 30;

pub fn autobreak(
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
) -> Option<String> {
    let now = Local::now();
    let today = now.date_naive();

    let (last_date, already_processed) = event_store.get_autobreak();
    if already_processed && last_date == today {
        return None;
    }

    let (break_start, break_end) = break_window(today);

    if now < break_end {
        return None;
    }

    if break_already_recorded(tasks, break_start, break_end, now) {
        event_store.save_autobreak(&today, true);
        return None;
    }

    // Insert break at 11:00
    let break_event = Event::BreakStarted {
        start_time: break_start,
        is_generated: true,
    };

    add_event(tasks, history, event_store, break_event, "");
    let break_event_end = Event::Stopped {
        end_time: break_end,
        is_generated: true,
    };
    add_event(tasks, history, event_store, break_event_end, "");

    event_store.save_autobreak(&today, true);

    Some(format!(
        "Autobreak recorded ({}-{})..",
        break_start.format("%H:%M"),
        break_end.format("%H:%M")
    ))
}

fn break_window(today: NaiveDate) -> (DateTime<Local>, DateTime<Local>) {
    let start = today
        .and_hms_opt(BREAK_START_HOUR, BREAK_START_MINUTE, 0)
        .expect("valid break start time")
        .and_local_timezone(Local)
        .single()
        .expect("break start time should not be ambiguous");
    let end = start + Duration::minutes(BREAK_DURATION_MINUTES);
    (start, end)
}

fn break_already_recorded(
    tasks: &[models::Task],
    break_start: DateTime<Local>,
    break_end: DateTime<Local>,
    now: DateTime<Local>,
) -> bool {
    tasks
        .iter()
        .filter(|task| task.is_break)
        .filter(|task| task.start_time.date_naive() == break_start.date_naive())
        .any(|task| {
            let task_end = task.end_time.unwrap_or(now);
            overlaps(task.start_time, task_end, break_start, break_end)
        })
}

fn overlaps(
    start_a: DateTime<Local>,
    end_a: DateTime<Local>,
    start_b: DateTime<Local>,
    end_b: DateTime<Local>,
) -> bool {
    start_a < end_b && end_a > start_b
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::models::Task;
    use crate::view::render_day;
    use chrono::Timelike;
    use tempfile::TempDir;

    fn setup_test_store() -> (EventStore, TempDir) {
        let temp_dir = TempDir::new().unwrap();
        let store = EventStore::new(temp_dir.path().to_str().unwrap());
        (store, temp_dir)
    }

    fn ts(hour: u32, minute: u32) -> DateTime<Local> {
        let today = Local::now().date_naive();
        today
            .and_hms_opt(hour, minute, 0)
            .unwrap()
            .and_local_timezone(Local)
            .single()
            .unwrap()
    }

    #[test]
    fn test_autobreak_with_early_manual_break() {
        let (event_store, _temp_dir) = setup_test_store();
        let mut tasks = Vec::new();
        let mut history = Vec::new();

        // Task started at 9:00
        let task1 = models::Task {
            id: 1,
            name: "Morning work".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 1.3,
            start_time: ts(9, 0),
            end_time: Some(ts(10, 0)),
            is_break: false,
            is_generated: false,
        };
        history.extend(task1.to_events());
        tasks.push(task1);

        // Manual break at 10:00-10:30
        let manual_break = models::Task {
            id: 2,
            name: "Break".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 1.0,
            start_time: ts(10, 0),
            end_time: Some(ts(10, 30)),
            is_break: true,
            is_generated: false,
        };
        history.extend(manual_break.to_events());
        tasks.push(manual_break);

        // Resumed work at 10:30
        let task2 = models::Task {
            id: 3,
            name: "More work".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 1.0,
            start_time: ts(10, 30),
            end_time: Some(ts(12, 0)),
            is_break: false,
            is_generated: false,
        };

        history.extend(task2.to_events());
        tasks.push(task2);

        // Call autobreak (simulating it being called after 11:30)
        let feedback = autobreak(&mut tasks, &mut history, &event_store);

        // Should insert autobreak from 11:00-11:30
        assert!(feedback.is_some());
        assert!(feedback.unwrap().contains("Autobreak recorded"));

        // Should have 5 tasks now: original 3 + autobreak + resumed task
        assert_eq!(tasks.len(), 5);

        // Check autobreak was inserted
        let autobreak_task = tasks.iter().find(|t| {
            t.is_break && t.start_time.time().hour() == 11 && t.start_time.time().minute() == 0
        });
        assert!(autobreak_task.is_some());

        // Check task was resumed at 11:30
        let resumed_task = tasks.iter().find(|t| {
            !t.is_break && t.start_time.time().hour() == 11 && t.start_time.time().minute() == 30
        });
        let day_projects_refs: Vec<&Task> = tasks.iter().collect();
        render_day(&day_projects_refs).unwrap();

        assert!(resumed_task.is_some());
        assert_eq!(resumed_task.unwrap().name, "More work");
    }

    #[test]
    fn test_autobreak_already_processed_today() {
        let (event_store, _temp_dir) = setup_test_store();
        let mut tasks = Vec::new();
        let mut history = Vec::new();

        let today = Local::now().date_naive();

        // Mark autobreak as already processed today
        event_store.save_autobreak(&today, true);

        // Should return None since already processed
        let feedback = autobreak(&mut tasks, &mut history, &event_store);
        assert!(feedback.is_none());
        assert_eq!(tasks.len(), 0);
    }

    #[test]
    fn test_autobreak_with_task_overlapping_break_period() {
        let (event_store, _temp_dir) = setup_test_store();
        let mut tasks = Vec::new();
        let mut history = Vec::new();

        // Task that spans across the autobreak period
        let long_task = models::Task {
            id: 1,
            name: "Long task".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 1.0,
            start_time: ts(10, 0),
            end_time: Some(ts(12, 0)),
            is_break: false,
            is_generated: false,
        };
        tasks.push(long_task);

        // Should insert autobreak because the task is work, not a break
        let feedback = autobreak(&mut tasks, &mut history, &event_store);
        assert!(feedback.is_some());
        assert!(
            tasks
                .iter()
                .any(|t| t.is_break && t.start_time.time().hour() == 11)
        );
    }
    #[test]
    fn test_autobreak_skips_when_break_already_present() {
        let (event_store, _temp_dir) = setup_test_store();
        let mut tasks = Vec::new();
        let mut history = Vec::new();

        let morning_task = models::Task {
            id: 1,
            name: "Work".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 1.0,
            start_time: ts(9, 0),
            end_time: Some(ts(10, 45)),
            is_break: false,
            is_generated: false,
        };
        tasks.push(morning_task);

        let break_task = models::Task {
            id: 2,
            name: "Lunch".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 0.0,
            start_time: ts(11, 0),
            end_time: Some(ts(11, 30)),
            is_break: true,
            is_generated: false,
        };
        tasks.push(break_task);

        let feedback = autobreak(&mut tasks, &mut history, &event_store);
        assert!(feedback.is_none());

        let (stored_date, processed) = event_store.get_autobreak();
        assert_eq!(stored_date, Local::now().date_naive());
        assert!(processed);
        assert_eq!(tasks.len(), 2);
    }

    #[test]
    fn test_autobreak_skips_when_break_already_present_and_open_task_present() {
        let (event_store, _temp_dir) = setup_test_store();
        let mut tasks = Vec::new();
        let mut history = Vec::new();

        let morning_task = models::Task {
            id: 1,
            name: "Work".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 1.0,
            start_time: ts(9, 0),
            end_time: Some(ts(10, 45)),
            is_break: false,
            is_generated: false,
        };
        tasks.push(morning_task);

        let break_task = models::Task {
            id: 2,
            name: "Lunch".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 0.0,
            start_time: ts(11, 0),
            end_time: Some(ts(11, 30)),
            is_break: true,
            is_generated: false,
        };
        tasks.push(break_task);
        let after_lunsh_task = models::Task {
            id: 2,
            name: "Lunch".to_string(),
            project_name: "test".to_string(),
            customer_name: "test".to_string(),
            rate: 0.0,
            start_time: ts(11, 30),
            end_time: None,
            is_break: true,
            is_generated: false,
        };
        tasks.push(after_lunsh_task);

        let feedback = autobreak(&mut tasks, &mut history, &event_store);
        assert!(feedback.is_none());

        let (stored_date, processed) = event_store.get_autobreak();
        assert_eq!(stored_date, Local::now().date_naive());
        assert!(processed);
        assert_eq!(tasks.len(), 3);
    }
}
