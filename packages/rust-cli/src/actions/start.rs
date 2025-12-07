use std::collections::HashMap;
use chrono::Local;
use inquire::Select;
use crate::events::Event;
use crate::{config, models, projector};
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
    let mut customer_name = String::new();
    let mut rate = 0.0;
    let now = Local::now();

    if parts.len() < 2 {
        // If we have a running task, stop it first
        if let Some(last_task) = tasks.last() {
            if last_task.end_time.is_none() {
                let stop_event = Event::Stopped {
                    end_time: now,
                };
                add_event(tasks, history, event_store, stop_event, "Stopped previous task.");
            }
        }

        if app_config.favorite_tasks.is_empty() {
            return "No favorites found. Use 'favorites add' to search and add tasks.".to_string();
        }

        // Calculate usage stats for the last year
        let mut task_seconds: HashMap<i32, i64> = HashMap::new();
        let today = now.date_naive();
        let start_date = today - chrono::Duration::days(365);

        let mut d = start_date;
        while d <= today {
            let events = event_store.events_for_day(d);
            let day_tasks = projector::restore_state(&events);
            for t in day_tasks {
                if !t.is_break {
                    *task_seconds.entry(t.id).or_default() += t.duration().num_seconds();
                }
            }
            d += chrono::Duration::days(1);
        }

        let mut favorite_tasks: Vec<&TaskDto> = external_tasks.iter()
            .filter(|t| app_config.favorite_tasks.contains(&t.id))
            .collect();

        // Sort by usage descending
        favorite_tasks.sort_by(|a, b| {
            let sec_a = task_seconds.get(&a.id).copied().unwrap_or(0);
            let sec_b = task_seconds.get(&b.id).copied().unwrap_or(0);
            sec_b.cmp(&sec_a)
        });

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
                    customer_name = task.project.customer.name.clone();
                    rate = task.compensation_rate;
                }
            },
            Err(_) => return "Cancelled.".to_string(),
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
            customer_name = task.project.customer.name.clone();
            rate = task.compensation_rate ;
        } else {
            let matches: Vec<&TaskDto> = external_tasks.iter()
                .filter(|t| app_config.favorite_tasks.contains(&t.id) && t.name.to_lowercase().contains(&search.to_lowercase()))
                .collect();

            if matches.len() == 1 {
                name = matches[0].name.clone();
                id = matches[0].id;
                project_name = matches[0].project.name.clone();
                customer_name = matches[0].project.customer.name.clone();
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
                            customer_name = task.project.customer.name.clone();
                            rate = task.compensation_rate ;
                        }
                    },
                    Err(_) => return "Cancelled.".to_string(),
                }
            }
        }
    }

    // If there is a gap after the last task, fill it with a break
    if let Some(last_task) = tasks.last() {
        if let Some(end_time) = last_task.end_time {
            if end_time < now && end_time.date_naive() == now.date_naive() {
                let break_event = Event::BreakStarted {
                    start_time: end_time,
                };
                event_store.persist(&break_event);
                history.push(break_event);

                let stop_event = Event::Stopped {
                    end_time: now,
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
        customer_name,
        rate,
        start_time: now,
    };
    add_event(tasks, history, event_store, event, &format!("Started task at {}", now.format("%H:%M")))
}
