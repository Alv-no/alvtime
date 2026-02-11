use chrono::{DateTime, Duration, Local};
use crate::events::Event;

#[derive(Debug, Clone)]
pub struct Task {
    pub id: i32,
    pub project_name: String,
    pub customer_name: String,
    pub rate: f64,
    pub name: String,
    pub comment: Option<String>,
    pub start_time: DateTime<Local>,
    pub end_time: Option<DateTime<Local>>,
    pub is_break: bool,
}

impl Task {
    pub fn duration(&self) -> Duration {
        let end = self.end_time.unwrap_or_else(Local::now);
        end - self.start_time
    }

    pub fn to_events(&self) -> Vec<Event> {
        let mut events = Vec::new();

        // 1. Add the starting event
        if self.is_break {
            events.push(Event::BreakStarted {
                start_time: self.start_time,
            });
        } else {
            events.push(Event::TaskStarted {
                id: self.id,
                name: self.name.clone(),
                project_name: self.project_name.clone(),
                customer_name: self.customer_name.clone(),
                rate: self.rate,
                start_time: self.start_time,
            });
        }

        // 2. Add the stopped event if an end time exists
        if let Some(end_time) = self.end_time {
            events.push(Event::Stopped {
                end_time,
            });
        }

        events
    }
}