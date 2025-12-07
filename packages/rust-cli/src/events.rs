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
    },
    BreakStarted {
        start_time: DateTime<Local>,
    },
    Reopen {
        start_time: DateTime<Local>,
    },
    Stopped {
        end_time: DateTime<Local>,
    },
    Undo {
        time: DateTime<Local>,
    },
    Redo {
        time: DateTime<Local>,
    },
    CommentAdded {
        date: NaiveDate,
        task_id: i32,
        comment: String,
    },
    LocallyCleared {
        date: NaiveDate,
    }
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
            Event::CommentAdded { date,.. } => {*date}
            Event::LocallyCleared { date,.. } => {*date}
        }
    }
}