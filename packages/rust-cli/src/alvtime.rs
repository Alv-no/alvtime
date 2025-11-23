use crate::external_models::*;
use chrono::NaiveDate;
use reqwest::blocking::Client;
use reqwest::header::{HeaderMap, HeaderValue, AUTHORIZATION};
use reqwest::StatusCode;
use serde_json::Value;

pub struct AlvtimeClient {
    base_url: String,
    token: Option<String>,
    client: Client,
}

impl AlvtimeClient {
    pub fn new(base_url: String, token: Option<String>) -> Self {
        Self {
            base_url,
            token,
            client: Client::new(),
        }
    }

    fn get_headers(&self) -> HeaderMap {
        let mut headers = HeaderMap::new();
        if let Some(token) = &self.token {
            if let Ok(val) = HeaderValue::from_str(&format!("Bearer {}", token)) {
                headers.insert(AUTHORIZATION, val);
            }
        }
        headers
    }

    pub fn ping(&self) -> Result<Value, reqwest::Error> {
        let url = format!("{}/api/ping", self.base_url);
        let response = self.client.get(&url).send()?;
        response.json()
    }

    pub fn list_tasks(&self) -> Result<Vec<TaskDto>, reqwest::Error> {
        let url = format!("{}/api/user/Tasks", self.base_url);
        let response = self.client.get(&url).headers(self.get_headers()).send()?;
        response.error_for_status()?.json()
    }

    pub fn list_time_entries(
        &self,
        from: NaiveDate,
        to: NaiveDate,
    ) -> Result<Vec<TimeEntryDto>, Box<dyn std::error::Error>> {
        let url = format!("{}/api/user/TimeEntries", self.base_url);
        let params = [
            ("fromDateInclusive", from.to_string()),
            ("toDateInclusive", to.to_string()),
        ];
        let response = self
            .client
            .get(&url)
            .headers(self.get_headers())
            .query(&params)
            .send()?;

        let response = response.error_for_status()?;
        let text = response.text()?;

        match serde_json::from_str(&text) {
            Ok(entries) => Ok(entries),
            Err(e) => {
                println!("Error parsing JSON from {}: {}", url, e);
                println!("Response body: {}", text);
                Err(Box::new(e))
            }
        }
    }

    pub fn upsert_time_entries(
        &self,
        entries: &[TimeEntryDto],
    ) -> Result<Vec<TimeEntryDto>, reqwest::Error> {
        let url = format!("{}/api/user/TimeEntries", self.base_url);
        let response = self
            .client
            .post(&url)
            .headers(self.get_headers())
            .json(entries)
            .send()?;
        response.error_for_status()?.json()
    }

    pub fn list_bank_holidays(
        &self,
        from_year: i32,
        to_year: i32,
    ) -> Result<Vec<NaiveDate>, reqwest::Error> {
        let url = format!("{}/api/Holidays/Years", self.base_url);
        let params = [
            ("fromYearInclusive", from_year.to_string()),
            ("toYearInclusive", to_year.to_string()),
        ];
        let response = self
            .client
            .get(&url)
            .headers(self.get_headers())
            .query(&params)
            .send()?;
        response.error_for_status()?.json()
    }

    pub fn get_available_hours(&self) -> Result<AvailableHoursDto, reqwest::Error> {
        let url = format!("{}/api/user/AvailableHours", self.base_url);
        let response = self.client.get(&url).headers(self.get_headers()).send()?;
        response.error_for_status()?.json()
    }

    pub fn upsert_payout(&self, payout: &PayoutDto) -> Result<Option<PayoutDto>, reqwest::Error> {
        let url = format!("{}/api/user/Payouts", self.base_url);
        let response = self
            .client
            .post(&url)
            .headers(self.get_headers())
            .json(payout)
            .send()?;

        let response = response.error_for_status()?;

        // Handle 204 No Content or empty body
        if response.status() == StatusCode::NO_CONTENT
            || response.content_length().unwrap_or(0) == 0
        {
            return Ok(None);
        }

        response.json().map(Some)
    }
}