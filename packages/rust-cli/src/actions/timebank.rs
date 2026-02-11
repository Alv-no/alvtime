use crate::alvtime;
use crate::external_models::AvailableHoursEntryDto;
use std::collections::HashMap;
use std::fmt::Write;

/// Render available hours by compensation rate as ASCII bars with vertical connectors
/// to an "effective hours" bar (sum of hours * rate) at the bottom.
/// Uses ANSI color codes with background colors matching the timeline view.
pub fn handle_timebank(client: &alvtime::AlvtimeClient) -> String {
    // Layout constants
    const LABEL_W: usize = 38;
    const BAR_W: usize = 40;
    const EFFECTIVE_BAR_W: usize = 80; // Full width for effective bar

    // ASCII bar characters (background colored spaces)
    const FILL: &str = " ";
    const EMPTY: char = ' ';

    // ANSI color codes matching timeline view
    const COLOR_0_5: &str = "\x1b[42m"; // Green BG (Volunteer 0.5)
    const COLOR_1_0: &str = "\x1b[48;5;93m"; // Purple BG (Internal 1.0)
    const COLOR_1_5: &str = "\x1b[45m"; // Magenta BG (Billable 1.5)
    const COLOR_2_0: &str = "\x1b[43m"; // Yellow BG (Billable Mandatory 2.0)
    const COLOR_RESET: &str = "\x1b[0m";

    // Helper: normalize f64 to 1 decimal place as a stable map key
    fn key_for_rate(rate: f64) -> String {
        format!("{rate:.1}")
    }

    // Sum entries by 1-decimal compensation rate string key (stable)
    fn sum_by_compensation_rate(entries: &[AvailableHoursEntryDto]) -> HashMap<String, f64> {
        let mut acc = HashMap::new();
        for e in entries {
            *acc.entry(key_for_rate(e.compensation_rate)).or_default() += e.hours;
        }
        acc
    }

    // Helper: build a fixed-width colored ASCII bar
    fn bar(width: usize, value: f64, scale_max: f64, color: &str) -> String {
        let blocks = if scale_max > 0.0 {
            ((value / scale_max) * width as f64).round()
        } else {
            0.0
        }
        .clamp(0.0, width as f64) as usize;

        let mut s = String::new();
        if blocks > 0 {
            s.push_str(color);
            s.push_str(&FILL.repeat(blocks));
            s.push_str(COLOR_RESET);
        }
        s.push_str(
            &std::iter::repeat(EMPTY)
                .take(width.saturating_sub(blocks))
                .collect::<String>(),
        );
        s
    }

    // Helper: build the weighted/effective bar with mixed colors and embedded labels
    fn weighted_bar(
        width: usize,
        categories: &[(f64, f64, &str, &str)], // (hours, rate, label, color)
        scale_max: f64,
    ) -> String {
        let total_effective: f64 = categories.iter().map(|(h, r, _, _)| h * r).sum();

        if total_effective == 0.0 || scale_max == 0.0 {
            return std::iter::repeat(EMPTY).take(width).collect::<String>();
        }

        let total_blocks = ((total_effective / scale_max) * width as f64)
            .round()
            .clamp(0.0, width as f64) as usize;

        // First pass: calculate segment positions and prepare labels
        struct Segment {
            start: usize,
            width: usize,
            label: String,
            color: String,
        }

        let mut segments = Vec::new();
        let mut blocks_used = 0;

        for (hours, rate, _, color) in categories {
            let effective = hours * rate;
            if effective <= 0.0 {
                continue;
            }

            let proportion = effective / total_effective;
            let blocks_for_this = ((proportion * total_blocks as f64).round() as usize)
                .min(total_blocks - blocks_used);

            if blocks_for_this > 0 {
                let label_text = format!("{:.1}h", effective);
                segments.push(Segment {
                    start: blocks_used,
                    width: blocks_for_this,
                    label: label_text,
                    color: color.to_string(),
                });
                blocks_used += blocks_for_this;
            }
        }

        // Build the bar with embedded labels
        let mut result = vec![EMPTY; width];

        for seg in &segments {
            // Try to fit label in the segment
            let label_chars: Vec<char> = seg.label.chars().collect();
            let label_len = label_chars.len();

            if seg.width >= label_len {
                let label_start = seg.start + 2;

                for (i, ch) in label_chars.iter().enumerate() {
                    if label_start + i < seg.start + seg.width {
                        result[label_start + i] = *ch;
                    }
                }
            }
        }

        // Build final string with colors
        let mut output = String::new();
        let mut current_seg_idx = 0;

        for (pos, ch) in result.iter().enumerate() {
            // Find which segment we're in
            while current_seg_idx < segments.len()
                && pos >= segments[current_seg_idx].start + segments[current_seg_idx].width
            {
                current_seg_idx += 1;
            }

            if current_seg_idx < segments.len()
                && pos >= segments[current_seg_idx].start
                && pos < segments[current_seg_idx].start + segments[current_seg_idx].width
            {
                // We're inside a segment
                if pos == segments[current_seg_idx].start || output.ends_with(COLOR_RESET) {
                    output.push_str(&segments[current_seg_idx].color);
                }
                output.push(*ch);
            } else {
                // Between segments or after all segments
                if !output.is_empty() && !output.ends_with(COLOR_RESET) {
                    output.push_str(COLOR_RESET);
                }
                output.push(*ch);
            }
        }

        if !output.ends_with(COLOR_RESET) {
            output.push_str(COLOR_RESET);
        }

        output
    }

    let available = match client.get_available_hours() {
        Ok(v) => v,
        Err(e) => return format!("error: failed to fetch available hours: {e}"),
    };

    let by_rate = sum_by_compensation_rate(&available.entries);

    // We render these four categories in a fixed, readable order.
    struct Category {
        rate: f64,
        key: &'static str,
        label: &'static str,
        color: &'static str,
    }

    let categories = [
        Category {
            rate: 0.5,
            key: "0.5",
            label: "Volunteer (0.5)",
            color: COLOR_0_5,
        },
        Category {
            rate: 1.0,
            key: "1.0",
            label: "Internal (Mandatory) (1.0)",
            color: COLOR_1_0,
        },
        Category {
            rate: 1.5,
            key: "1.5",
            label: "Billable (1.5)",
            color: COLOR_1_5,
        },
        Category {
            rate: 2.0,
            key: "2.0",
            label: "Billable (Mandatory) (2.0)",
            color: COLOR_2_0,
        },
    ];

    // Gather values
    let mut rows: Vec<(f64, f64, &str, &str)> = Vec::with_capacity(categories.len());
    for c in &categories {
        let hours = by_rate.get(c.key).copied().unwrap_or(0.0);
        rows.push((hours, c.rate, c.label, c.color));
    }

    let total_hours: f64 = rows.iter().map(|(h, _, _, _)| *h).sum();
    let effective_total: f64 = rows.iter().map(|(h, r, _, _)| h * r).sum();

    // Common scaling so bars are comparable; include effective_total in the scale
    let max_hours: f64 = rows
        .iter()
        .map(|(h, _, _, _)| *h)
        .fold(0.0, |acc, h| acc.max(h));
    let max_hours = max_hours.max(effective_total).max(1.0);

    let mut out = String::new();

    // Header line
    writeln!(out, "{:<LABEL_W$} {:>6.2}h", "Total available", total_hours).ok();

    // Rows for each category
    for (hours, _, label, color) in &rows {
        let left_bar = bar(BAR_W, *hours, max_hours, color);
        writeln!(out, "{:<LABEL_W$} {:>6.2}h  {}", label, hours, left_bar).ok();
    }

    // Add spacing and vertical connectors
    const CONNECTOR_HEIGHT: usize = 1;
    let indent = LABEL_W + 9; // align with where bars start

    for _ in 0..CONNECTOR_HEIGHT {
        writeln!(out, "{:indent$}|", "").ok();
    }

    // Effective hours bar with full width and embedded labels
    let eff_bar = weighted_bar(EFFECTIVE_BAR_W, &rows, max_hours);

    writeln!(out, "{:indent$}v", "").ok();
    writeln!(
        out,
        "{:<LABEL_W$} {:>6.2}h  {}",
        "Effective hours (weighted)", effective_total, eff_bar
    )
    .ok();

    out
}
