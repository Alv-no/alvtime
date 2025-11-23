use crate::config;

pub fn handle_config(parts: &[&str], app_config: &mut config::Config) -> String {
    if parts.len() < 2 {
        return "Usage: config set-token <value>".to_string();
    }
    match parts[1] {
        "set-token" => {
            if parts.len() < 3 {
                "Please provide a token value.".to_string()
            } else {
                let token = parts[2].to_string();
                app_config.personal_token = Some(token);
                app_config.save();
                "Personal token saved.".to_string()
            }
        }
        "autobreak" => {
            if parts.len() < 3 {
                format!(
                    "Autobreak is currently {}.",
                    if app_config.autobreak { "on" } else { "off" }
                )
            } else {
                match parts[2] {
                    "on" | "true" | "enable" => {
                        app_config.autobreak = true;
                        app_config.save();
                        "Autobreak enabled (11:00 - 11:30).".to_string()
                    }
                    "off" | "false" | "disable" => {
                        app_config.autobreak = false;
                        app_config.save();
                        "Autobreak disabled.".to_string()
                    }
                    _ => "Usage: config autobreak <on|off>".to_string()
                }
            }
        }
        _ => "Unknown config command.".to_string(),
    }
}