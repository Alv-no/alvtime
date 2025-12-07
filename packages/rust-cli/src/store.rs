use std::collections::{HashMap, HashSet};
use chrono::NaiveDate;
use crate::events::Event;
use sled::Db;
use crate::external_models::TaskDto;

pub struct EventStore {
    db: Db,
}

impl EventStore {
    pub fn new(path: &str) -> Self {
        // Creates a folder named "tracker_db" in your project root
        let db = sled::open(path).expect("Failed to open database");
        EventStore { db }
    }

    pub fn persist(&self, event: &Event) {
        self.persist_batch(std::slice::from_ref(event));
    }

    pub fn persist_batch(&self, events: &[Event]) {
        let dates_to_unsync = self.persist_events_data(events);

        // Use the batch function to mark all collected dates as UNSYNCED
        self.set_days_synced_batch(&dates_to_unsync, false);
    }

    /// Persists a batch of events received from the server and marks the affected dates as SYNCHRONIZED.
    pub fn persist_synced_batch(&self, events: &[Event]) {
        let dates_to_sync = self.persist_events_data(events);

        // Use the batch function to mark all collected dates as SYNCHRONIZED
        self.set_days_synced_batch(&dates_to_sync, true);
    }

    /// Sets the sync status for a single day.
    /// Note: This is now just a wrapper for the batch function for convenience.
    pub fn set_day_synced(&self, date: &NaiveDate, is_synced: bool) {
        self.set_days_synced_batch(std::slice::from_ref(date), is_synced);
    }

    /// Batch version: Sets the sync status for multiple days at once.
    /// - `is_synced = true`: Dates are removed from the unsynced tree (Implicitly Synced).
    /// - `is_synced = false`: Dates are inserted into the unsynced tree (Explicitly Unsynced).
    pub fn set_days_synced_batch(&self, dates: &[NaiveDate], is_synced: bool) {
        if dates.is_empty() {
            return;
        }

        let unsynced_tree = self.db.open_tree("unsynced_dates").unwrap();

        let mut batch = sled::Batch::default();

        for date in dates {
            let date_str = date.format("%Y-%m-%d").to_string();
            let key = date_str.as_bytes();

            if is_synced {
                // To sync: remove the key.
                batch.remove(key);
            } else {
                // To mark as unsynced: insert the key (value is empty slice).
                batch.insert(key, &[]);
            }
        }

        // Apply all operations in a single atomic database call
        unsynced_tree.apply_batch(batch).unwrap();
        unsynced_tree.flush().unwrap();
    }

    fn persist_events_data(&self, events: &[Event]) -> Vec<NaiveDate> {
        if events.is_empty() {
            return Vec::new();
        }

        let dates_idx = self.db.open_tree("dates_index").unwrap();
        let mut trees: HashMap<String, sled::Tree> = HashMap::new();
        let mut affected_dates: HashSet<NaiveDate> = HashSet::new();

        for event in events {
            // 1. Get next ID
            let id = self.db.update_and_fetch("id_sequence", |val| {
                let current = val.map(|v| {
                    let mut bytes = [0u8; 8];
                    bytes.copy_from_slice(v);
                    u64::from_be_bytes(bytes)
                }).unwrap_or(0);

                Some((current + 1).to_be_bytes())
            }).unwrap().unwrap();

            let key = id.as_ref();

            let date = event.date();
            let date_str = date.format("%Y-%m-%d").to_string();

            affected_dates.insert(date);

            // 2. Update dates index
            dates_idx.insert(&date_str, &[]).unwrap();

            // 3. Persist event data
            let tree_key = format!("events_{}", date_str);
            let tree = trees.entry(tree_key.clone())
                .or_insert_with(|| self.db.open_tree(&tree_key).unwrap());

            let value = serde_json::to_vec(event).unwrap();
            tree.insert(key, value).unwrap();
        }

        dates_idx.flush().unwrap();
        for tree in trees.values() {
            tree.flush().unwrap();
        }

        affected_dates.drain().collect()
    }

    pub fn events_for_day(&self, date: NaiveDate) -> Vec<Event> {
        let tree_key = format!("events_{}", date.format("%Y-%m-%d"));
        match self.db.open_tree(tree_key) {
            Ok(tree) => tree
                .iter()
                .filter_map(|res| {
                    let (_, value) = res.ok()?;
                    serde_json::from_slice(&value).ok()
                })
                .collect(),
            Err(_) => Vec::new(),
        }
    }

    pub fn delete_events_for_days(&self, dates: &[NaiveDate]) {
        let mut trees = Vec::with_capacity(dates.len());
        for date in dates {
            let tree_key = format!("events_{}", date.format("%Y-%m-%d"));
            if let Ok(tree) = self.db.open_tree(tree_key) {
                tree.clear().unwrap();
                trees.push(tree);
            }
        }
        for tree in trees {
            tree.flush().unwrap();
        }
    }

    pub fn has_cached_holidays(&self, year: i32) -> bool {
        let meta = self.db.open_tree("meta").unwrap();
        let key = format!("holidays_{}", year);
        meta.contains_key(key).unwrap_or(false)
    }

    pub fn save_holidays(&self, year: i32, holidays: &[NaiveDate]) {
        let tree = self.db.open_tree("holidays").unwrap();
        for h in holidays {
            let k = h.format("%Y-%m-%d").to_string();
            let _ = tree.insert(k, &[]);
        }
        // Mark this year as cached so we don't fetch again
        let meta = self.db.open_tree("meta").unwrap();
        let _ = meta.insert(format!("holidays_{}", year), &[]);
    }

    pub fn get_holidays(&self) -> HashSet<NaiveDate> {
        let tree = self.db.open_tree("holidays").unwrap();
        let mut set = HashSet::new();
        for item in tree.iter() {
            if let Ok((k, _)) = item {
                if let Ok(s) = std::str::from_utf8(&k) {
                    if let Ok(d) = NaiveDate::parse_from_str(s, "%Y-%m-%d") {
                        set.insert(d);
                    }
                }
            }
        }
        set
    }

    pub fn save_tasks(&self, tasks: &[TaskDto]) {
        let tree = self.db.open_tree("tasks_cache").unwrap();
        tree.clear().unwrap();
        for task in tasks {
            let key = task.id.to_be_bytes();
            let value = serde_json::to_vec(task).unwrap();
            tree.insert(key, value).unwrap();
        }
        tree.flush().unwrap();
    }

    pub fn get_cached_tasks(&self) -> Vec<TaskDto> {
        let tree = self.db.open_tree("tasks_cache").unwrap();
        let mut tasks = Vec::new();
        for item in tree.iter() {
            if let Ok((_, value)) = item {
                if let Ok(task) = serde_json::from_slice::<TaskDto>(&value) {
                    tasks.push(task);
                }
            }
        }
        tasks
    }

    pub fn get_all_dates_with_events(&self) -> Vec<NaiveDate> {
        let dates_idx = self.db.open_tree("dates_index").unwrap();
        dates_idx
            .iter()
            .filter_map(|res| {
                let (k, _) = res.ok()?;
                let date_str = std::str::from_utf8(&k).ok()?;
                NaiveDate::parse_from_str(date_str, "%Y-%m-%d").ok()
            })
            .collect()
    }

    pub fn save_autobreak(&self, date: &NaiveDate, flag: bool) {
        let tree = self.db.open_tree("properties").unwrap();
        let value = format!("{}|{}", date.format("%Y-%m-%d"), flag);
        tree.insert("autobreak", value.as_bytes()).unwrap();
        tree.flush().unwrap();
    }

    pub fn get_autobreak(&self) -> (NaiveDate, bool) {
        let tree = self.db.open_tree("properties").unwrap();

        if let Some(raw) = tree.get("autobreak").unwrap() {
            if let Ok(s) = String::from_utf8(raw.to_vec()) {
                let parts: Vec<&str> = s.split('|').collect();
                if parts.len() == 2 {
                    if let Ok(date) = NaiveDate::parse_from_str(parts[0], "%Y-%m-%d") {
                        let flag = parts[1].parse::<bool>().unwrap_or(false);
                        return (date, flag);
                    }
                }
            }
        }

        // default: today + false
        let today = chrono::Local::now().naive_local().date();
        (today, false)
    }


    /// Retrieves all dates from the dedicated 'unsynced_dates' tree.
    /// This is highly performant as it only iterates over the necessary keys.
    pub fn get_unsynced_dates(&self) -> HashSet<NaiveDate> {
        let unsynced_tree = self.db.open_tree("unsynced_dates").unwrap();

        unsynced_tree
            .iter()
            .filter_map(|res| {
                // Only the key (k) is needed
                let (k, _) = res.ok()?;

                let date_str = std::str::from_utf8(&k).ok()?;
                NaiveDate::parse_from_str(date_str, "%Y-%m-%d").ok()
            })
            .collect()
    }
}