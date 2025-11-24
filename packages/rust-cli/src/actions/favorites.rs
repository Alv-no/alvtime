use inquire::MultiSelect;
use rustyline::Editor;
use std::collections::HashMap;
use chrono::Local;
use crate::config;
use crate::external_models::TaskDto;
use crate::input_helper::InputHelper;
use crate::store::EventStore;
use crate::projector;

pub fn handle_favorites(
    parts: &[&str],
    app_config: &mut config::Config,
    tasks: &[TaskDto],
    rl: &mut Editor<InputHelper, rustyline::history::DefaultHistory>,
    store: &EventStore,
) -> String {
    if parts.len() < 2 {
        let mut output = String::from("Favorites:\n");
        for id in &app_config.favorite_tasks {
            if let Some(task) = tasks.iter().find(|t| t.id == *id) {
                output.push_str(&format!(" - {}\n", task));
            } else {
                output.push_str(&format!(" - Unknown ID: {}\n", id));
            }
        }
        return output;
    }

    match parts[1] {
        "add" => {
            // Calculate usage stats for the last year
            let mut task_seconds: HashMap<i32, i64> = HashMap::new();
            let today = Local::now().date_naive();
            let start_date = today - chrono::Duration::days(365);

            let mut d = start_date;
            while d <= today {
                let events = store.events_for_day(d);
                let day_tasks = projector::restore_state(&events);
                for t in day_tasks {
                    if !t.is_break {
                        *task_seconds.entry(t.id).or_default() += t.duration().num_seconds();
                    }
                }
                d += chrono::Duration::days(1);
            }

            let mut sorted_tasks = tasks.to_vec();
            sorted_tasks.sort_by(|a, b| {
                let sec_a = task_seconds.get(&a.id).copied().unwrap_or(0);
                let sec_b = task_seconds.get(&b.id).copied().unwrap_or(0);
                sec_b.cmp(&sec_a)
            });

            let options: Vec<String> = sorted_tasks.iter()
                .map(|t| t.to_string())
                .collect();

            if options.is_empty() {
                return "No tasks available to add.".to_string();
            }

            let mut default_indices = Vec::new();
            for (i, t) in sorted_tasks.iter().enumerate() {
                if app_config.favorite_tasks.contains(&t.id) {
                    default_indices.push(i);
                }
            }

            let ans = MultiSelect::new("Select task to add (Type to filter):", options.clone())
                .with_page_size(15)
                .with_default(&default_indices)
                .prompt();

            match ans {
                Ok(selections) => {
                    let mut new_favorites = Vec::new();
                    for sel in selections {
                        if let Some(task) = sorted_tasks.iter().find(|t| t.to_string() == sel) {
                            new_favorites.push(task.id);
                        }
                    }

                    let count = new_favorites.len();
                    app_config.favorite_tasks = new_favorites;
                    app_config.save();
                    if let Some(h) = rl.helper_mut() {
                        h.favorites = app_config.favorite_tasks.clone();
                    }
                    format!("Favorites updated. Total: {}", count)
                },
                Err(_) => "Selection cancelled.".to_string(),
            }
        },
        "remove" => {
            if app_config.favorite_tasks.is_empty() {
                return "No favorites to remove.".to_string();
            }

            let current_favorites: Vec<String> = tasks.iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id))
                .map(|t| t.to_string())
                .collect();

            let ans = MultiSelect::new("Select favorites to remove:", current_favorites)
                .prompt();

            match ans {
                Ok(selections) => {
                    app_config.favorite_tasks.retain(|id| {
                        let task_str = tasks.iter().find(|t| t.id == *id).map(|t| t.to_string()).unwrap_or_default();
                        !selections.contains(&task_str)
                    });
                    app_config.save();
                    if let Some(h) = rl.helper_mut() {
                        h.favorites = app_config.favorite_tasks.clone();
                    }
                    "Favorites removed.".to_string()
                },
                Err(_) => "Cancelled.".to_string(),
            }
        },
        _ => "Unknown command.".to_string()
    }
}
