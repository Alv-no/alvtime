use crate::models::Task;
use chrono::{Datelike, Local, NaiveDate, Weekday};
use std::collections::{BTreeMap, HashSet};

// ANSI Color Constants
mod colors {
    // Background colors for compensation rates (matching timebank)
    pub const BG_RATE_0_5: &str = "\x1b[42m";      // Green BG (Volunteer 0.5)
    pub const BG_RATE_1_0: &str = "\x1b[48;5;93m"; // Purple BG (Internal 1.0)
    pub const BG_RATE_1_5: &str = "\x1b[45m";      // Magenta BG (Billable 1.5)
    pub const BG_RATE_2_0: &str = "\x1b[43m";      // Yellow BG (Billable Mandatory 2.0)

    // Status colors
    pub const BG_ACTIVE: &str = "\x1b[42m";        // Green BG for active tasks
    pub const BG_LOCAL_ONLY: &str = "\x1b[48;5;208;30m"; // Orange BG, Black text for local/manual tasks
    pub const BG_HOLIDAY: &str = "\x1b[101m";      // Bright Red BG
    pub const BG_WEEKEND: &str = "\x1b[41m";       // Red BG

    // Text colors
    pub const FG_BREAK_ACTIVE: &str = "\x1b[31m";  // Red text for active break
    pub const FG_BREAK_INACTIVE: &str = "\x1b[90m"; // Grey text for inactive break
    pub const FG_HOLIDAY: &str = "\x1b[91m";       // Bright red text
    pub const FG_WEEKEND: &str = "\x1b[31m";       // Dark red text
    pub const FG_DIMMED: &str = "\x1b[90m";        // Dimmed text
    pub const FG_BOLD: &str = "\x1b[1m";           // Bold text
    pub const FG_OVERTIME: &str = "\x1b[33m";      // Yellow/orange text
    pub const FG_HOURS_WORKED: &str = "\x1b[32m";  // Green text
    pub const FG_HOURS_OVERTIME: &str = "\x1b[33m"; // Yellow text

    pub const RESET: &str = "\x1b[0m";
}

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
            format!("{}{}{}", colors::BG_LOCAL_ONLY, day_str, colors::RESET)
        } else if is_holiday {
            format!("{}{}{}", colors::FG_HOLIDAY, day_str, colors::RESET)
        } else if is_weekend {
            format!("{}{}{}", colors::FG_WEEKEND, day_str, colors::RESET)
        } else if minutes > 0 {
            format!("{}{}{}", colors::FG_BOLD, day_str, colors::RESET)
        } else {
            format!("{}{}{}", colors::FG_DIMMED, day_str, colors::RESET)
        };

        let colored_hours = if minutes > 0 {
            let content = format!("{:<6}", hours_str);
            if is_holiday || is_weekend || minutes > 450 {
                format!("{}{}{}", colors::FG_HOURS_OVERTIME, content, colors::RESET)
            } else {
                format!("{}{}{}", colors::FG_HOURS_WORKED, content, colors::RESET)
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
            format!("{} {} (Local) {}", colors::BG_LOCAL_ONLY, date_str, colors::RESET)
        } else if is_holiday {
            format!("{} {} (Holiday) {}", colors::BG_HOLIDAY, date_str, colors::RESET)
        } else if is_weekend {
            format!("{} {} {}", colors::BG_WEEKEND, date_str, colors::RESET)
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

fn get_rate_color(rate: f64) -> Option<&'static str> {
    if (rate - 0.5).abs() < 0.001 {
        Some(colors::BG_RATE_0_5)
    } else if (rate - 1.0).abs() < 0.001 {
        Some(colors::BG_RATE_1_0)
    } else if (rate - 1.5).abs() < 0.001 {
        Some(colors::BG_RATE_1_5)
    } else if (rate - 2.0).abs() < 0.001 {
        Some(colors::BG_RATE_2_0)
    } else {
        None
    }
}

fn format_rate_label(rate: f64) -> String {
    if (rate - 0.5).abs() < 0.001 {
        " (0.5)".to_string()
    } else if (rate - 1.0).abs() < 0.001 {
        " (1.0)".to_string()
    } else if (rate - 1.5).abs() < 0.001 {
        " (1.5)".to_string()
    } else if (rate - 2.0).abs() < 0.001 {
        " (2.0)".to_string()
    } else {
        String::new()
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

        let bg_style = if !p.is_break {
            get_rate_color(p.rate)
        } else {
            None
        };

        if !p.is_break {
            label.push_str(&format_rate_label(p.rate));
        }

        let info_text = if p.is_break {
            p.name.clone()
        } else {
            format!("{} | {}", p.name, p.customer_name)
        };

        let label_len = label.len();
        let min_width = std::cmp::max(label_len, info_text.len()) + 2;
        // Scale: 1 char per 15 minutes, but cap it to avoid overflow on huge durations
        let calc_width = (minutes / 15) as usize;
        // Cap width to something reasonable for terminal (e.g., 200 chars)
        let width = std::cmp::min(std::cmp::max(std::cmp::max(min_width, calc_width), 8), 200);

        let padding = width.saturating_sub(label_len + 2);
        let close_char = if p.end_time.is_none() { ">" } else { "]" };

        let bar_content = if p.is_break {
            // Safe subtraction for w
            let w = width.saturating_sub(2);
            let inner = format!("{:-<w$}", label, w = w);
            if p.end_time.is_none() {
                format!("[{}{}{}{}", colors::FG_BREAK_ACTIVE, inner, colors::RESET, close_char)
            } else {
                format!("[{}{}{}{}", colors::FG_BREAK_INACTIVE, inner, colors::RESET, close_char)
            }
        } else if let Some(style) = bg_style {
            let inner = format!("{}{}", label, " ".repeat(padding));
            format!("[{}{}{}{}", style, inner, colors::RESET, close_char)
        } else if p.end_time.is_none() {
            let inner = format!("{}{}", label, " ".repeat(padding));
            format!("[{}{}{}{}", colors::BG_ACTIVE, inner, colors::RESET, close_char)
        } else {
            format!("[{}{}{}", label, " ".repeat(padding), close_char)
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
        println!("{}Overtime: {}h {}m{}", colors::FG_OVERTIME, ot / 60, ot % 60, colors::RESET);
    }
}