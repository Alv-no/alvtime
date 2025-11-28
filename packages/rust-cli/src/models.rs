use chrono::{DateTime, Duration, Local};

#[derive(Debug, Clone)]
pub struct Task {
    pub id: i32,
    pub project_name: String,
    pub customer_name: String,
    pub rate: f64,
    pub name: String,
    pub start_time: DateTime<Local>,
    pub end_time: Option<DateTime<Local>>,
    pub comment: Option<String>,
    pub is_break: bool,
    pub is_generated: bool,
}

impl Task {
    pub fn duration(&self) -> Duration {
        let end = self.end_time.unwrap_or_else(Local::now);
        end - self.start_time
    }
}