use chrono::{DateTime, Local, NaiveDate};
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Debug, Clone)]
pub enum Event {
    TaskStarted {
        id: i32,
        name: String,
        project_name: String,
        customer_name: String,
        rate: f64,
        start_time: DateTime<Local>,
        #[serde(default)]
        is_generated: bool,
    },
    BreakStarted {
        start_time: DateTime<Local>,
        #[serde(default)]
        is_generated: bool,
    },
    Reopen {
        start_time: DateTime<Local>,
        #[serde(default)]
        is_generated: bool,
    },
    Stopped {
        end_time: DateTime<Local>,
        #[serde(default)]
        is_generated: bool,
    },
    Undo {
        time: DateTime<Local>,
    },
    Redo {
        time: DateTime<Local>,
    },
    DayRevised {
        date: NaiveDate,
        events: Vec<Event>,
    },
}

impl Event {
    pub fn date(&self) -> NaiveDate {
        match self {
            Event::TaskStarted { start_time, .. } => start_time.date_naive(),
            Event::BreakStarted { start_time, .. } => start_time.date_naive(),
            Event::Stopped { end_time, .. } => end_time.date_naive(),
            Event::Reopen { start_time, .. } => start_time.date_naive(),
            Event::Undo { time } => time.date_naive(),
            Event::Redo { time } => time.date_naive(),
            Event::DayRevised { date, .. } => *date,
        }
    }
}