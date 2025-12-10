use crate::models::Task;
use chrono::{Datelike, Duration, Local, NaiveDate, Weekday};
use crossterm::{
    cursor::{Hide, MoveTo, Show},
    event::{self, Event, KeyCode, KeyEventKind},
    execute,
    terminal::{
        Clear, ClearType, EnterAlternateScreen, LeaveAlternateScreen, disable_raw_mode,
        enable_raw_mode,
    },
};
use std::collections::{BTreeMap, HashSet};
use std::io::{self, Result as IOResult, Write};

mod colors {
    pub const BG_RATE_0_5: &str = "\x1b[42m";
    pub const BG_RATE_1_0: &str = "\x1b[48;5;93m";
    pub const BG_RATE_1_5: &str = "\x1b[45m";
    pub const BG_RATE_2_0: &str = "\x1b[43m";

    pub const BG_ACTIVE: &str = "\x1b[42m";
    pub const BG_LOCAL_ONLY: &str = "\x1b[48;5;208;30m";
    pub const BG_HOLIDAY: &str = "\x1b[101m";
    pub const BG_WEEKEND: &str = "\x1b[41m";

    pub const FG_BREAK_ACTIVE: &str = "\x1b[31m";
    pub const FG_BREAK_INACTIVE: &str = "\x1b[90m";
    pub const FG_HOLIDAY: &str = "\x1b[91m";
    pub const FG_WEEKEND: &str = "\x1b[31m";
    pub const FG_DIMMED: &str = "\x1b[90m";
    pub const FG_BOLD: &str = "\x1b[1m";
    pub const FG_OVERTIME: &str = "\x1b[33m";
    pub const FG_HOURS_WORKED: &str = "\x1b[32m";
    pub const FG_HOURS_OVERTIME: &str = "\x1b[33m";
    pub const BG_CURRENT_SELECT: &str = "\x1b[44m\x1b[37m";
    pub const BG_MARKED_SELECT: &str = "\x1b[48;5;220;30m";

    pub const FG_COMMENT_HEADER: &str = "\x1b[1;37m"; // Bold White
    pub const FG_COMMENT_TEXT: &str = "\x1b[37m";     // White
    pub const FG_PROJECT: &str = "\x1b[36m";          // Cyan
    pub const RESET: &str = "\x1b[0m";
}

pub enum ViewMode {
    Day,
    Week,
    Month,
    Year,
}

pub fn draw_timeline(
    projects: &[Task],
    mode: &ViewMode,
    holidays: &HashSet<NaiveDate>,
    unsynced_dates: &HashSet<NaiveDate>,
) -> IOResult<()> {
    match mode {
        ViewMode::Month => {
            let now = Local::now();
            draw_month_calendar(
                now.year(),
                now.month(),
                projects,
                holidays,
                Some(now.date_naive()),
                &HashSet::new(),
                unsynced_dates
            )?;
            draw_linear_timeline(projects, &ViewMode::Day, holidays,
                                 unsynced_dates)?;
        }
        ViewMode::Year => {
            draw_year_calendar(projects, holidays,
                               unsynced_dates)?;
            draw_linear_timeline(projects, &ViewMode::Day, holidays,
                                 unsynced_dates)?;
        }
        _ => draw_linear_timeline(projects, mode, holidays,
                                  unsynced_dates)?,
    }
    Ok(())
}

fn draw_year_calendar(projects: &[Task], holidays: &HashSet<NaiveDate>, unsynced_dates: &HashSet<NaiveDate>,) -> IOResult<()> {
    let now = Local::now();
    let year = now.year();
    for month in 1..=12 {
        draw_month_calendar(
            year,
            month,
            projects,
            holidays,
            Some(now.date_naive()),
            &HashSet::new(),
            unsynced_dates
        )?;
    }
    Ok(())
}

fn draw_month_calendar(
    year: i32,
    month: u32,
    projects: &[Task],
    holidays: &HashSet<NaiveDate>,
    highlight: Option<NaiveDate>,
    marked: &HashSet<NaiveDate>,
    unsynced_dates: &HashSet<NaiveDate>,
) -> IOResult<()> {
    let first_day = NaiveDate::from_ymd_opt(year, month, 1).unwrap();
    let month_name = first_day.format("%B");
    let mut stdout = io::stdout();

    let mut daily_minutes: BTreeMap<NaiveDate, i64> = BTreeMap::new();

    for p in projects {
        let date = p.start_time.date_naive();
        if date.year() == year && date.month() == month && !p.is_break {
            *daily_minutes.entry(date).or_default() += p.duration().num_minutes();
        }
    }

    writeln!(&mut stdout, "\r\n{} {}", month_name, year)?;
    writeln!(
        &mut stdout,
        "\r Wk  Mon        Tue        Wed        Thu        Fri        Sat        Sun       "
    )?;

    let start_weekday = first_day.weekday().num_days_from_monday();

    let mut current_line = String::new();

    let week_num = first_day.iso_week().week();
    current_line.push_str(&format!(" {:2} ", week_num));

    for _ in 0..start_weekday {
        current_line.push_str("           ");
    }

    let days_in_month = get_days_in_month(year, month);

    for day in 1..=days_in_month {
        let date = NaiveDate::from_ymd_opt(year, month, day).unwrap();
        let weekday = date.weekday();
        let is_weekend = weekday == Weekday::Sat || weekday == Weekday::Sun;
        let is_holiday = holidays.contains(&date);
        let is_selected = highlight == Some(date);
        let is_marked = marked.contains(&date);
        let is_unsynced = unsynced_dates.contains(&date);

        let minutes = daily_minutes.get(&date).copied().unwrap_or(0);
        let hours_str = if minutes > 0 {
            format!("{:.2}h", minutes as f64 / 60.0)
        } else {
            "".to_string()
        };

        let day_str = format!("{:02}", day);

        let mut day_styles = Vec::new();

        if is_selected {
            day_styles.push(colors::BG_CURRENT_SELECT);
        } else if is_marked || is_unsynced {
            day_styles.push(colors::BG_MARKED_SELECT);
        } else if is_holiday {
            day_styles.push(colors::FG_HOLIDAY);
        } else if is_weekend {
            day_styles.push(colors::FG_WEEKEND);
        } else if minutes > 0 {
            day_styles.push(colors::FG_BOLD);
        } else {
            day_styles.push(colors::FG_DIMMED);
        };

        let colored_day = format!("{}{}{}", day_styles.concat(), day_str, colors::RESET);

        let colored_hours = if minutes > 0 {
            let content = format!("{:<6}", hours_str);

            let hour_styles = if is_holiday || is_weekend || minutes > 450 {
                colors::FG_HOURS_OVERTIME
            } else {
                colors::FG_HOURS_WORKED
            };

            format!("{}{}{}", hour_styles, content, colors::RESET)
        } else {
            let empty_content = format!("{:<6}", "");
            "".to_string() + &empty_content
        };

        let cell_content = if is_selected || is_marked || is_unsynced {
            format!(
                " {} {} {}",
                colored_day,
                colors::RESET.to_string() + &colored_hours,
                colors::RESET
            )
        } else {
            format!(" {} {} ", colored_day, colored_hours)
        };

        current_line.push_str(&cell_content);

        if weekday == Weekday::Sun {
            writeln!(&mut stdout, "\r{}", current_line)?;
            current_line.clear();

            if day < days_in_month {
                let next_day = NaiveDate::from_ymd_opt(year, month, day + 1).unwrap();
                let next_week = next_day.iso_week().week();
                current_line.push_str(&format!(" {:2} ", next_week));
            }
        }
    }
    if !current_line.is_empty() {
        while current_line.len() < 80 {
            current_line.push(' ');
        }
        writeln!(&mut stdout, "\r{}", current_line)?;
    }
    writeln!(&mut stdout, "\r")?;

    Ok(())
}

fn draw_linear_timeline(
    projects: &[Task],
    mode: &ViewMode,
    holidays: &HashSet<NaiveDate>,
    unsynced_dates: &HashSet<NaiveDate>,
) -> IOResult<()> {
    let now = Local::now();
    let today = now.date_naive();
    let mut stdout = io::stdout();

    let mut grouped: BTreeMap<NaiveDate, Vec<&Task>> = BTreeMap::new();

    for p in projects {
        let p_date = p.start_time.date_naive();
        let include = match mode {
            ViewMode::Day => p_date == today,
            ViewMode::Week => {
                p.start_time.iso_week().week() == now.iso_week().week()
                    && p.start_time.year() == now.year()
            }
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
        writeln!(&mut stdout, "\rNo activity recorded for this period.")?;
        return Ok(());
    }

    for (date, day_projects) in grouped {
        let weekday = date.weekday();
        let is_weekend = weekday == Weekday::Sat || weekday == Weekday::Sun;
        let is_holiday = holidays.contains(&date);
        let is_unsynced = unsynced_dates.contains(&date);

        let date_str = date.format("%Y-%m-%d (%A)").to_string();

        let header = if is_unsynced {
            format!(
                "{} {} (Unsynced) {}",
                colors::BG_LOCAL_ONLY,
                date_str,
                colors::RESET
            )
        } else if is_holiday {
            format!(
                "{} {} (Holiday) {}",
                colors::BG_HOLIDAY,
                date_str,
                colors::RESET
            )
        } else if is_weekend {
            format!("{} {} {}", colors::BG_WEEKEND, date_str, colors::RESET)
        } else {
            format!("Date: {}", date_str)
        };

        writeln!(&mut stdout, "\r\n{}", header)?;
        render_day(&day_projects)?;
    }
    Ok(())
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

pub fn render_day(projects: &[&Task]) -> IOResult<()> {
    let mut buffer = String::new();

    if projects.is_empty() {
        buffer.push_str("\rNo activity recorded.\n");
        return io::stdout().write_all(buffer.as_bytes());
    }

    let mut line_names = String::new();
    let mut line_top = String::new();
    let mut line_bars = String::new();
    let mut line_bot = String::new();
    let mut line_times = String::new();

    let mut total_minutes = 0;

    // We use this to track which IDs we have already rendered comments for
    // to ensure we only print the comment once per task ID.
    let mut processed_comment_ids: HashSet<i32> = HashSet::new();
    let mut comments_buffer = String::new();

    for p in projects {
        // --- Existing Timeline Logic ---
        let duration = p.duration();
        let minutes = std::cmp::max(0, duration.num_minutes());

        if !p.is_break {
            total_minutes += minutes;
        }

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
        let calc_width = (minutes / 15) as usize;
        let width = std::cmp::min(std::cmp::max(std::cmp::max(min_width, calc_width), 8), 200);

        let padding = width.saturating_sub(label_len + 2);
        let close_char = if p.end_time.is_none() { ">" } else { "]" };

        let bar_content = if p.is_break {
            let w = width.saturating_sub(2);
            let inner = format!("{:-<w$}", label, w = w);
            if p.end_time.is_none() {
                format!(
                    "[{}{}{}{}",
                    colors::FG_BREAK_ACTIVE,
                    inner,
                    colors::RESET,
                    close_char
                )
            } else {
                format!(
                    "[{}{}{}{}",
                    colors::FG_BREAK_INACTIVE,
                    inner,
                    colors::RESET,
                    close_char
                )
            }
        } else if let Some(style) = bg_style {
            let inner = format!("{}{}", label, " ".repeat(padding));
            format!("[{}{}{}{}", style, inner, colors::RESET, close_char)
        } else if p.end_time.is_none() {
            let inner = format!("{}{}", label, " ".repeat(padding));
            format!(
                "[{}{}{}{}",
                colors::BG_ACTIVE,
                inner,
                colors::RESET,
                close_char
            )
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

    // --- Render Timeline ---
    buffer.push_str(&format!("\r\n{}\n", line_names));
    buffer.push_str(&format!("\r{}\n", line_top));
    buffer.push_str(&format!("\r{}\n", line_bars));
    buffer.push_str(&format!("\r{}\n", line_bot));
    buffer.push_str(&format!("\r{}\n\n", line_times));

    let hours = total_minutes / 60;
    let mins = total_minutes % 60;
    buffer.push_str(&format!("\rTotal time: {}h {}m\n", hours, mins));

    if total_minutes > 450 {
        let ot = total_minutes - 450;
        buffer.push_str(&format!(
            "\r{}Overtime: {}h {}m{}\n",
            colors::FG_OVERTIME,
            ot / 60,
            ot % 60,
            colors::RESET
        ));
    }

    // --- Generate Comment Section ---
    let mut has_comments = false;
    for p in projects {
        if processed_comment_ids.contains(&p.id) {
            continue;
        }
        processed_comment_ids.insert(p.id);

        if let Some(comment) = &p.comment {
            if !comment.trim().is_empty() {
                // Initialize header only once
                if !has_comments {
                    comments_buffer.push_str(&format!(
                        "\r\n{}Notes & Comments:{}\n",
                        colors::FG_BOLD,
                        colors::RESET
                    ));
                    has_comments = true;
                }

                // Header line: Task Name | Project
                let task_header = format!(
                    " • {}{}{} | {}{}{} ({})",
                    colors::FG_COMMENT_HEADER,
                    p.name,
                    colors::RESET,
                    colors::FG_PROJECT,
                    p.project_name,
                    colors::RESET,
                    p.customer_name
                );

                // Comment body with indentation
                let comment_body = format!(
                    "    {}{}{}",
                    colors::FG_COMMENT_TEXT,
                    comment.trim().replace('\n', "\n    "), // Handle multi-line comments nicely
                    colors::RESET
                );

                comments_buffer.push_str(&format!("\r{}\n\r{}\n", task_header, comment_body));
            }
        }
    }

    // Append comments to the main buffer
    buffer.push_str(&comments_buffer);

    io::stdout().write_all(buffer.as_bytes())
}

pub fn interactive_view(
    projects: &[Task],
    holidays: &HashSet<NaiveDate>,
    unsynced_dates: &HashSet<NaiveDate>,
    allow_selection: bool,
) -> IOResult<Vec<NaiveDate>> {
    let _guard = TerminalGuard::new()?;
    let mut stdout = io::stdout();
    let mut selected = Local::now().date_naive();
    let mut marked_dates: HashSet<NaiveDate> = HashSet::new();

    let instruction = if allow_selection {
        "Interactive month view — ←/→ day | ↑/↓ week | PgUp/PgDn month | **SPACE to mark/unmark** | **ENTER to confirm marked dates** | Q/Esc to clear & exit"
    } else {
        "Interactive month view — ←/→ day | ↑/↓ week | PgUp/PgDn month | **ENTER to select current day** | Q/Esc to clear & exit"
    };

    loop {
        execute!(stdout, MoveTo(0, 0), Clear(ClearType::All))?;

        write!(stdout, "{}\n\r", instruction)?;

        let legend_selection = if allow_selection {
            format!(
                " | {}Marked{}{}",
                colors::BG_MARKED_SELECT,
                "    ",
                colors::RESET
            )
        } else {
            String::new()
        };

        writeln!(
            stdout,
            "Legend: {}Current{}{} {} | {}Local/Manual{}{} | {}Holiday{}{}",
            colors::BG_CURRENT_SELECT,
            "    ",
            colors::RESET,
            legend_selection,
            colors::BG_LOCAL_ONLY,
            "    ",
            colors::RESET,
            colors::BG_HOLIDAY,
            "    ",
            colors::RESET
        )?;
        writeln!(
            stdout,
            "\r--------------------------------------------------------------------------------\n\r"
        )?;

        draw_month_calendar(
            selected.year(),
            selected.month(),
            projects,
            holidays,
            Some(selected),
            &marked_dates,
            unsynced_dates
        )?;

        let day_entries: Vec<&Task> = projects
            .iter()
            .filter(|t| t.start_time.date_naive() == selected)
            .collect();

        let marked_info = if allow_selection {
            format!(
                " - {}Marked: {} Dates{}",
                colors::FG_BOLD,
                marked_dates.len(),
                colors::RESET
            )
        } else {
            String::new()
        };

        writeln!(
            stdout,
            "\r\nSelected day: {} ({} entries){}\n",
            selected.format("%Y-%m-%d (%A)"),
            day_entries.len(),
            marked_info,
        )?;
        render_day(&day_entries)?;
        stdout.flush().ok();

        match event::read()? {
            Event::Key(key) if key.kind == KeyEventKind::Press => match key.code {
                // 1. Confirmation & Exit (ENTER)
                KeyCode::Enter => {
                    let mut result: Vec<NaiveDate> = if allow_selection {
                        // Return all marked dates
                        marked_dates.into_iter().collect()
                    } else {
                        // Return only the currently selected day
                        vec![selected]
                    };
                    result.sort();
                    return Ok(result);
                }

                // 2. Clear Selection & Exit (ESC/Q)
                KeyCode::Esc | KeyCode::Char('q') | KeyCode::Char('Q') => {
                    return Ok(Vec::new());
                }

                // 3. Multi-Select/Mark (Conditional)
                KeyCode::Char(' ') if allow_selection => {
                    if marked_dates.contains(&selected) {
                        marked_dates.remove(&selected);
                    } else {
                        marked_dates.insert(selected);
                    }
                }

                // 4. Navigation
                KeyCode::Left => selected = shift_days(selected, -1),
                KeyCode::Right => selected = shift_days(selected, 1),
                KeyCode::Up => selected = shift_days(selected, -7),
                KeyCode::Down => selected = shift_days(selected, 7),
                KeyCode::PageUp => {
                    selected = shift_months(selected, -1);
                    if selected.month()
                        != (Local::now().date_naive().month() as i32 + -1).rem_euclid(12) as u32
                    {
                        selected = NaiveDate::from_ymd_opt(selected.year(), selected.month(), 1)
                            .unwrap_or(selected);
                    }
                }
                KeyCode::PageDown => {
                    selected = shift_months(selected, 1);
                    if selected.month()
                        != (Local::now().date_naive().month() as i32 + 1).rem_euclid(12) as u32
                    {
                        selected = NaiveDate::from_ymd_opt(selected.year(), selected.month(), 1)
                            .unwrap_or(selected);
                    }
                }
                KeyCode::Home => {
                    selected = selected.with_day(1).unwrap_or(selected);
                }
                KeyCode::End => {
                    let last = get_days_in_month(selected.year(), selected.month());
                    selected = selected.with_day(last).unwrap_or(selected);
                }
                _ => {}
            },
            _ => {}
        }
    }
}

struct TerminalGuard;

impl TerminalGuard {
    fn new() -> IOResult<Self> {
        enable_raw_mode()?;
        let mut stdout = io::stdout();
        execute!(stdout, EnterAlternateScreen, Hide)?;
        Ok(Self)
    }
}

impl Drop for TerminalGuard {
    fn drop(&mut self) {
        let _ = disable_raw_mode();
        let mut stdout = io::stdout();
        let _ = execute!(stdout, Show, LeaveAlternateScreen, Clear(ClearType::All));
    }
}

fn shift_days(date: NaiveDate, delta: i64) -> NaiveDate {
    date.checked_add_signed(Duration::days(delta))
        .unwrap_or(date)
}

fn shift_months(date: NaiveDate, delta: i32) -> NaiveDate {
    let mut year = date.year();
    let mut month = date.month() as i32 + delta;
    while month < 1 {
        month += 12;
        year -= 1;
    }
    while month > 12 {
        month -= 12;
        year += 1;
    }
    let last_day = get_days_in_month(year, month as u32);
    let day = date.day().min(last_day);
    NaiveDate::from_ymd_opt(year, month as u32, day).unwrap_or(date)
}
