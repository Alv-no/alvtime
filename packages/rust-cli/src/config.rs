use serde::{Deserialize, Serialize};
use std::fs;
use std::path::{Path, PathBuf};

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct Config {
    pub storage_path: String,
    pub personal_token: Option<String>,
    #[serde(default = "default_api_url")]
    pub api_url: String,
    #[serde(default)]
    pub autobreak: bool,
    #[serde(default)]
    pub favorite_tasks: Vec<i32>,
}

fn default_api_url() -> String {
    "https://api.alvtime.no".to_string()
}

fn get_app_dir() -> PathBuf {
    let home = std::env::var("HOME")
        .or_else(|_| std::env::var("USERPROFILE"))
        .expect("Could not find home directory");
    Path::new(&home).join(".time")
}

impl Default for Config {
    fn default() -> Self {
        let storage_path = get_app_dir().join("tracker_db");
        Self {
            storage_path: storage_path.to_string_lossy().into_owned(),
            personal_token: None,
            api_url: default_api_url(),
            autobreak: false,
            favorite_tasks: Vec::new(),
        }
    }
}

impl Config {
    pub fn load() -> Self {
        let app_dir = get_app_dir();
        if !app_dir.exists() {
            fs::create_dir_all(&app_dir).expect("Failed to create app directory");
        }

        let config_path = app_dir.join("config.json");

        if config_path.exists() {
            let content = fs::read_to_string(&config_path).unwrap_or_default();
            serde_json::from_str(&content).unwrap_or_else(|_| Self::default())
        } else {
            let config = Self::default();
            config.save();
            config
        }
    }

    pub fn save(&self) {
        let app_dir = get_app_dir();
        if !app_dir.exists() {
            fs::create_dir_all(&app_dir).expect("Failed to create app directory");
        }

        let config_path = app_dir.join("config.json");
        let content = serde_json::to_string_pretty(self).expect("Failed to serialize config");
        fs::write(config_path, content).expect("Failed to write config file");
    }
}