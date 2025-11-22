use std::collections::HashSet;
use chrono::NaiveDate;
use crate::events::Event;
use sled::Db;

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
        // Generate a unique, monotonic ID for the key
        // 'id_sequence' is a special key used to track the counter
        let id = self.db.update_and_fetch("id_sequence", |val| {
            let current = val.map(|v| {
                let mut bytes = [0u8; 8];
                bytes.copy_from_slice(v);
                u64::from_be_bytes(bytes)
            }).unwrap_or(0);

            Some((current + 1).to_be_bytes())
        }).unwrap().unwrap();

        let key = id.as_ref(); // id is IVec, can serve as key directly

        let date = event.date();
        let date_str = date.format("%Y-%m-%d").to_string();

        // 1. Update the index of existing days
        let dates_idx = self.db.open_tree("dates_index").unwrap();
        // We only need the key (date), value can be empty
        dates_idx.insert(&date_str, &[]).unwrap();

        // 2. Insert the event into the partition (tree) for that day
        let tree_key = format!("events_{}", date_str);
        let tree = self.db.open_tree(tree_key).unwrap();

        // Serialize event to JSON and save
        let value = serde_json::to_vec(event).unwrap();
        tree.insert(key, value).unwrap();

        // Ensure it's written to disk immediately
        tree.flush().unwrap();
        dates_idx.flush().unwrap();
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

    pub fn delete_events_for_day(&self, date: NaiveDate) {
        let tree_key = format!("events_{}", date.format("%Y-%m-%d"));
        if let Ok(tree) = self.db.open_tree(tree_key) {
            tree.clear().unwrap();
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
}