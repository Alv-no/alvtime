use inquire::MultiSelect;
use rustyline::Editor;
use crate::config;
use crate::external_models::TaskDto;
use crate::input_helper::InputHelper;

pub fn handle_favorites(
    parts: &[&str],
    app_config: &mut config::Config,
    tasks: &[TaskDto],
    rl: &mut Editor<InputHelper, rustyline::history::DefaultHistory>,
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
            let options: Vec<String> = tasks.iter()
                .map(|t| t.to_string())
                .collect();

            if options.is_empty() {
                return "No tasks available to add.".to_string();
            }

            let mut default_indices = Vec::new();
            for (i, t) in tasks.iter().enumerate() {
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
                        if let Some(task) = tasks.iter().find(|t| t.to_string() == sel) {
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
