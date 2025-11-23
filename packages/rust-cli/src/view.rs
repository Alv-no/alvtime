use crate::models::Task;
use chrono::{Datelike, Local, NaiveDate, Weekday};
use std::collections::{BTreeMap, HashSet};

pub enum ViewMode {
    Day,
    Week,
    Month,
    Year,
}

pub fn draw_timeline(projects: &[Task], mode: &ViewMode, holidays: &HashSet<NaiveDate>) {
    match mode {
        ViewMode::Month => {
            let now = Local::now();
            draw_month_calendar(now.year(), now.month(), projects, holidays);
            draw_linear_timeline(projects, &ViewMode::Day, holidays);
        }
        ViewMode::Year => {
            draw_year_calendar(projects, holidays);
            draw_linear_timeline(projects, &ViewMode::Day, holidays);
        }
        _ => draw_linear_timeline(projects, mode, holidays),
    }
}
fn draw_year_calendar(projects: &[Task], holidays: &HashSet<NaiveDate>) {
    let now = Local::now();
    let year = now.year();
    for month in 1..=12 {
        draw_month_calendar(year, month, projects, holidays);
    }
}
fn draw_month_calendar(year: i32, month: u32, projects: &[Task], holidays: &HashSet<NaiveDate>) {
    let first_day = NaiveDate::from_ymd_opt(year, month, 1).unwrap();
    let month_name = first_day.format("%B");

    // Calculate totals per day
    let mut daily_minutes: BTreeMap<NaiveDate, i64> = BTreeMap::new();
    let mut manual_dates: HashSet<NaiveDate> = HashSet::new();

    for p in projects {
        let date = p.start_time.date_naive();
        if date.year() == year && date.month() == month && !p.is_break {
            *daily_minutes.entry(date).or_default() += p.duration().num_minutes();

            if !p.is_generated {
                manual_dates.insert(date);
            }
        }
    }

    println!("\n{} {}", month_name, year);
    // Headers: Wk + 7 Days (11 chars each)
    println!(" Wk  Mon        Tue        Wed        Thu        Fri        Sat        Sun       ");

    // Find weekday of 1st day. Mon=0 .. Sun=6
    let start_weekday = first_day.weekday().num_days_from_monday();

    let mut current_line = String::new();

    // Start line with Week Number
    let week_num = first_day.iso_week().week();
    current_line.push_str(&format!(" {:2} ", week_num));

    // Print initial padding
    for _ in 0..start_weekday {
        current_line.push_str("           ");
    }

    let days_in_month = get_days_in_month(year, month);

    for day in 1..=days_in_month {
        let date = NaiveDate::from_ymd_opt(year, month, day).unwrap();
        let weekday = date.weekday();
        let is_weekend = weekday == Weekday::Sat || weekday == Weekday::Sun;
        let is_holiday = holidays.contains(&date);
        let has_manual = manual_dates.contains(&date);

        let minutes = daily_minutes.get(&date).copied().unwrap_or(0);
        let hours_str = if minutes > 0 {
            format!("{:.2}h", minutes as f64 / 60.0)
        } else {
            "".to_string()
        };

        // Cell format: " 01 7.5h   "
        let day_str = format!("{:02}", day);

        let colored_day = if has_manual {
            // Orange background for local-only tasks
            format!("\x1b[48;5;208;30m{}\x1b[0m", day_str)
        } else if is_holiday {
            // Bright Red Text for Holidays
            format!("\x1b[91m{}\x1b[0m", day_str)
        } else if is_weekend {
            // Dark Red/Grey Text for Weekends
            format!("\x1b[31m{}\x1b[0m", day_str)
        } else if minutes > 0 {
            // Bold/White for active days
            format!("\x1b[1m{}\x1b[0m", day_str)
        } else {
            // Dimmed for empty days
            format!("\x1b[90m{}\x1b[0m", day_str)
        };

        let colored_hours = if minutes > 0 {
            let content = format!("{:<6}", hours_str);
            // Orange if holiday, weekend (red day), or overtime
            if is_holiday || is_weekend || minutes > 450 {
                format!("\x1b[33m{}\x1b[0m", content)
            } else {
                format!("\x1b[32m{}\x1b[0m", content)
            }
        } else {
            format!("{:<6}", "")
        };

        current_line.push_str(&format!(" {} {} ", colored_day, colored_hours));

        if weekday == Weekday::Sun {
            println!("{}", current_line);
            current_line.clear();

            // If there are more days, start next line with next week number
            if day < days_in_month {
                let next_day = NaiveDate::from_ymd_opt(year, month, day + 1).unwrap();
                let next_week = next_day.iso_week().week();
                current_line.push_str(&format!(" {:2} ", next_week));
            }
        }
    }
    if !current_line.is_empty() {
        println!("{}", current_line);
    }
    println!();
}
fn draw_linear_timeline(projects: &[Task], mode: &ViewMode, holidays: &HashSet<NaiveDate>) {
    let now = Local::now();
    let today = now.date_naive();

    // Group projects by date
    let mut grouped: BTreeMap<NaiveDate, Vec<&Task>> = BTreeMap::new();

    for p in projects {
        let p_date = p.start_time.date_naive();
        let include = match mode {
            ViewMode::Day => p_date == today,
            ViewMode::Week => {
                p.start_time.iso_week().week() == now.iso_week().week()
                    && p.start_time.year() == now.year()
            }
            // Month handled separately, but fallthrough safety for grouped logic
            ViewMode::Month => {
                p.start_time.month() == now.month() && p.start_time.year() == now.year()
            }
            _ => false,
        };

        if include {
            grouped.entry(p_date).or_default().push(p);
        }
    }

    if grouped.is_empty() {
        println!("No activity recorded for this period.");
        return;
    }

    for (date, day_projects) in grouped {
        let weekday = date.weekday();
        let is_weekend = weekday == Weekday::Sat || weekday == Weekday::Sun;
        let is_holiday = holidays.contains(&date);
        let has_manual = day_projects.iter().any(|p| !p.is_break && !p.is_generated);

        let date_str = date.format("%Y-%m-%d (%A)").to_string();

        // Colored backgrounds for the Date header line
        let header = if has_manual {
            // Orange Background, Black Text for local tasks
            format!("\x1b[48;5;208;30m {} (Local) \x1b[0m", date_str)
        } else if is_holiday {
            // Bright Red Background, Black Text
            format!("\x1b[101;30m {} (Holiday) \x1b[0m", date_str)
        } else if is_weekend {
            // Red Background, White Text
            format!("\x1b[41;37m {} \x1b[0m", date_str)
        } else {
            format!("Date: {}", date_str)
        };

        println!("\n{}", header);
        render_day(&day_projects);
    }
}

fn get_days_in_month(year: i32, month: u32) -> u32 {
    if month == 12 {
        31
    } else {
        let next_month = NaiveDate::from_ymd_opt(year, month + 1, 1).unwrap();
        let this_month = NaiveDate::from_ymd_opt(year, month, 1).unwrap();
        (next_month - this_month).num_days() as u32
    }
}

pub fn render_day(projects: &[&Task]) {
    if projects.is_empty() {
        println!("No activity recorded.");
        return;
    }

    let mut line_names = String::new();
    let mut line_top = String::new();
    let mut line_bars = String::new();
    let mut line_bot = String::new();
    let mut line_times = String::new();

    let mut total_minutes = 0;

    for p in projects {
        let duration = p.duration();
        let minutes = std::cmp::max(0, duration.num_minutes()); // Ensure non-negative

        if !p.is_break {
            total_minutes += minutes;
        }

        // Visual calculation
        let mut label = if minutes >= 60 {
            format!("{}h {}m", minutes / 60, minutes % 60)
        } else {
            format!("{}m", minutes)
        };

        let mut bg_style = None;
        let rate = p.rate;
        if (rate - 0.5).abs() < 0.001 {
            bg_style = Some("\x1b[42;102m"); // Green BG, White FG
            label.push_str(" (0.5)");
        } else if (rate - 1.0).abs() < 0.001 {
            bg_style = Some("\x1b[48;5;93;102m"); // Purple BG, White FG
            label.push_str(" (1.0)");
        } else if (rate - 1.5).abs() < 0.001 {
            bg_style = Some("\x1b[45;102m"); // Magenta BG, White FG
            label.push_str(" (1.5)");
        }

        let info_text = if p.is_break {
            p.name.clone()
        } else {
            format!("{} | {}", p.name, p.project_name)
        };

        let label_len = label.len();
        let min_width = std::cmp::max(label_len, info_text.len()) + 2;
        // Scale: 1 char per 15 minutes, but cap it to avoid overflow on huge durations
        let calc_width = (minutes / 15) as usize;
        // Cap width to something reasonable for terminal (e.g., 200 chars)
        let width = std::cmp::min(std::cmp::max(std::cmp::max(min_width, calc_width), 8), 200);

        let padding = width.saturating_sub(label_len + 2);
        let bar_text = if p.is_break {
            // Safe subtraction for w
            let w = width.saturating_sub(2);
            format!("[{:-<w$}]", label, w = w)
        } else {
            format!("[{}{}]", label, " ".repeat(padding))
        };

        let bar_content = if p.is_break {
            if p.end_time.is_none() {
                // Active break in red
                format!("\x1b[31m{}\x1b[0m", bar_text)
            } else {
                // Inactive break in light grey (using bright black)
                format!("\x1b[90m{}\x1b[0m", bar_text)
            }
        } else if let Some(style) = bg_style {
            format!("{}{}\x1b[0m", style, bar_text)
        } else if p.end_time.is_none() {
            // Highlight current running project in green
            format!("\x1b[32m{}\x1b[0m", bar_text)
        } else {
            bar_text
        };

        let start_str = p.start_time.format("%H:%M").to_string();

        line_names.push_str(&format!("  {:<w$}", info_text, w = width - 2));
        line_top.push_str(&format!("/{:<w$}", " ", w = width - 1));
        line_bars.push_str(&bar_content);
        line_bot.push_str(&format!("\\{:<w$}", " ", w = width - 1));
        line_times.push_str(&format!(" {:<w$}", start_str, w = width - 1));
    }

    if let Some(last) = projects.last() {
        if let Some(end_time) = last.end_time {
            line_bot.push_str("\\");
            line_times.push_str(&end_time.format("%H:%M").to_string());
        }
    }

    println!("\n{}", line_names);
    println!("{}", line_top);
    println!("{}", line_bars);
    println!("{}", line_bot);
    println!("{}\n", line_times);

    let hours = total_minutes / 60;
    let mins = total_minutes % 60;
    println!("Total time: {}h {}m", hours, mins);

    if total_minutes > 450 {
        // 7h 30m = 450m
        let ot = total_minutes - 450;
        println!("\x1b[33mOvertime: {}h {}m\x1b[0m", ot / 60, ot % 60);
    }
}
