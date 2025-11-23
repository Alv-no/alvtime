use crate::external_models::TaskDto;
use rustyline::completion::Completer;
use rustyline::{Context, Helper, Highlighter, Hinter, Result, Validator};

#[derive(Helper, Highlighter, Validator, Hinter)]
pub struct InputHelper {
    pub commands: Vec<String>,
    pub tasks: Vec<TaskDto>,
    pub favorites: Vec<i32>,
}

impl Completer for InputHelper {
    type Candidate = String;

    fn complete(
        &self,
        line: &str,
        pos: usize,
        _ctx: &Context<'_>,
    ) -> Result<(usize, Vec<Self::Candidate>)> {
        let (start, word) = if let Some(idx) = line[..pos].rfind(char::is_whitespace) {
            (idx + 1, &line[idx + 1..pos])
        } else {
            (0, &line[..pos])
        };

        let mut matches = Vec::new();

        if start == 0 {
            // Start of line: suggest main commands
            for cmd in &self.commands {
                if cmd.starts_with(word) {
                    matches.push(cmd.clone());
                }
            }
        } else {
            // Sub-command context: check the first word
            let line_prefix = &line[..start];
            let parts: Vec<&str> = line_prefix.split_whitespace().collect();
            let first_word = parts.first().copied().unwrap_or("");

            match first_word {
                "start" => {
                    // Autocomplete only from favorites or simple exact matches
                    for fav_id in &self.favorites {
                        if let Some(task) = self.tasks.iter().find(|t| t.id == *fav_id) {
                            if task.name.to_lowercase().contains(&word.to_lowercase()) {
                                matches.push(task.name.clone());
                            }
                        }
                    }
                }
                "config" => {
                    if parts.len() >= 2 && parts[1] == "autobreak" {
                        let options = ["on", "off"];
                        for opt in &options {
                            if opt.starts_with(word) {
                                matches.push(opt.to_string());
                            }
                        }
                    } else {
                        if "set-token".starts_with(word) {
                            matches.push("set-token".to_string());
                        }
                        if "autobreak".starts_with(word) {
                            matches.push("autobreak".to_string());
                        }
                    }
                }
                "view" => {
                    let options = ["week", "month", "year"];
                    for opt in &options {
                        if opt.starts_with(word) {
                            matches.push(opt.to_string());
                        }
                    }
                }
                "favorites" => {
                    let options = ["add", "remove"];
                    for opt in &options {
                        if opt.starts_with(word) {
                            matches.push(opt.to_string());
                        }
                    }
                }
                _ => {}
            }
        }

        Ok((start, matches))
    }
}