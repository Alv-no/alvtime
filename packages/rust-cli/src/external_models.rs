use std::fmt;
use serde::{Deserialize, Serialize};

#[derive(Debug, Deserialize, Serialize, Clone)]
pub struct TaskDto {
    pub id: i32,
    pub name: String,
    pub locked: bool,
    #[serde(rename = "compensationRate")]
    pub compensation_rate: f64,
    pub project: ProjectDto,
}

impl fmt::Display for TaskDto {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "{} | {} | {} | {:.2}",
            self.name, self.project.name, self.project.customer.name, self.compensation_rate
        )
    }
}

#[derive(Debug, Deserialize, Serialize, Clone)]
pub struct ProjectDto {
    pub name: String,
    pub customer: CustomerDto,
}

#[derive(Debug, Deserialize, Serialize, Clone)]
pub struct CustomerDto {
    pub name: String,
}

#[derive(Debug, Deserialize, Serialize, Clone)]
pub struct TimeEntryDto {
    #[serde(rename = "taskId")]
    pub task_id: i32,
    pub date: String, // ISO Format YYYY-MM-DD
    pub value: f64,   // Hours
    pub comment: Option<String>,
    pub id: u64,
}

#[derive(Debug, Deserialize, Serialize, Clone)]
pub struct PayoutDto {
    pub date: String,
    pub hours: f64,
}

#[derive(Debug, Deserialize, Serialize, Clone)]
pub struct AvailableHoursDto {
    #[serde(rename = "availableHoursBeforeCompensation")]
    pub available_hours_before_compensation: f64,
    #[serde(rename = "availableHoursAfterCompensation")]
    pub available_hours_after_compensation: f64,
    #[serde(default)]
    pub entries: Vec<AvailableHoursEntryDto>,
}

#[derive(Debug, Deserialize, Serialize, Clone)]
pub struct AvailableHoursEntryDto {
    pub date: String,
    pub hours: f64,
    #[serde(rename = "compensationRate")]
    pub compensation_rate: f64,
    #[serde(rename = "type")]
    pub entry_type: i32,
    pub active: bool,
}