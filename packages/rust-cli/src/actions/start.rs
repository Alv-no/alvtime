use chrono::Local;
use inquire::Select;
use crate::events::Event;
use crate::{config, models};
use crate::actions::add_event;
use crate::external_models::TaskDto;
use crate::store::EventStore;

pub fn handle_start(
    parts: &[&str],
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    app_config: &config::Config,
    external_tasks: &[TaskDto],
) -> String {
    let mut name = String::new();
    let mut id = 0;
    let mut project_name = String::new();
    let mut rate = 0.0;

    if parts.len() < 2 {
        // Resume logic or dropdown
        let last_real_task = tasks.iter().rev().find(|p| !p.is_break);

        if last_real_task.is_none() || (tasks.last().is_some() && !tasks.last().unwrap().is_break && tasks.last().unwrap().end_time.is_some()) {
            // No active task or last was stopped.
            if app_config.favorite_tasks.is_empty() {
                return "No favorites found. Use 'favorites add' to search and add tasks.".to_string();
            }

            let favorite_tasks: Vec<&TaskDto> = external_tasks.iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id))
                .collect();

            let options: Vec<String> = favorite_tasks.iter().map(|t| t.to_string()).collect();

            let selection = Select::new("Select task from favorites:", options)
                .with_page_size(10)
                .prompt();

            match selection {
                Ok(selected_str) => {
                    if let Some(task) = favorite_tasks.iter().find(|t| t.to_string() == selected_str) {
                        name = task.name.clone();
                        id = task.id;
                        project_name = task.project.name.clone();
                        rate = task.compensation_rate;
                    }
                },
                Err(_) => return "Cancelled.".to_string(),
            }
        } else if let Some(prev) = last_real_task {
            name = prev.name.clone();
            id = prev.id;
            project_name = prev.project_name.clone();
            rate = prev.rate;
        } else {
            return "No previous task to resume.".to_string();
        }
    } else {
        let search = parts[1..].join(" ");

        // Find task by name in favorites first
        let fav_match = external_tasks.iter()
            .find(|t| app_config.favorite_tasks.contains(&t.id) && t.name == search);

        if let Some(task) = fav_match {
            name = task.name.clone();
            id = task.id;
            project_name = task.project.name.clone();
            rate = task.compensation_rate ;
        } else {
            let matches: Vec<&TaskDto> = external_tasks.iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id) && t.name.to_lowercase().contains(&search.to_lowercase()))
                .collect();

            if matches.len() == 1 {
                name = matches[0].name.clone();
                id = matches[0].id;
                project_name = matches[0].project.name.clone();
                rate = matches[0].compensation_rate ;
            } else if matches.is_empty() {
                return "Task not found in favorites. Use 'favorites add' to find it.".to_string();
            } else {
                let options: Vec<String> = matches.iter().map(|t| t.to_string()).collect();
                let selection = Select::new("Multiple favorites match. Select one:", options).prompt();
                match selection {
                    Ok(sel) => {
                        if let Some(task) = matches.iter().find(|t| t.to_string() == sel) {
                            name = task.name.clone();
                            id = task.id;
                            project_name = task.project.name.clone();
                            rate = task.compensation_rate ;
                        }
                    },
                    Err(_) => return "Cancelled.".to_string(),
                }
            }
        }
    }

    let now = Local::now();

    // If there is a gap after the last task, fill it with a break
    if let Some(last_task) = tasks.last() {
        if let Some(end_time) = last_task.end_time {
            if end_time < now && end_time.date_naive() == now.date_naive() {
                let break_event = Event::BreakStarted {
                    start_time: end_time,
                    is_generated: true,
                };
                event_store.persist(&break_event);
                history.push(break_event);

                let stop_event = Event::Stopped {
                    end_time: now,
                    is_generated: true,
                };
                event_store.persist(&stop_event);
                history.push(stop_event);
            }
        }
    }

    let event = Event::TaskStarted {
        id,
        name: name.clone(),
        project_name,
        rate,
        start_time: now,
        is_generated: false,
    };
    add_event(tasks, history, event_store, event, &format!("Started task at {}", now.format("%H:%M")))
}
