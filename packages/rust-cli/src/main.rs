mod actions;
mod alvtime;
mod config;
mod events;
mod external_models;
mod input_helper;
mod models;
mod projector;
mod store;
mod view;

use crate::actions::add_event;
use crate::actions::autobreak::autobreak;
use crate::actions::config::handle_config;
use crate::actions::edit::handle_edit;
use crate::actions::favorites::handle_favorites;
use crate::actions::push::handle_push;
use crate::actions::start::handle_start;
use crate::actions::sync::handle_sync;
use crate::actions::timebank::handle_timebank;
use crate::config::Config;
use crate::external_models::TaskDto;
use crate::models::Task;
use chrono::{Datelike, Local, NaiveDate};
use clap::{CommandFactory, Parser, Subcommand, ValueEnum};
use clap_complete::{Shell, generate};
use events::Event;
use input_helper::InputHelper;
use rustyline::Editor;
use rustyline::error::ReadlineError;
use rustyline::history::DefaultHistory;
use std::collections::HashSet;
use std::io;
use store::EventStore;
use view::ViewMode;
use crate::actions::utils::get_all_tasks;

#[derive(Parser)]
#[command(name = "atime")]
#[command(about = "Alvtime CLI", long_about = None)]
struct Cli {
    /// Start interactive shell mode
    #[arg(short, long)]
    shell: bool,

    #[command(subcommand)]
    command: Option<Commands>,
}

#[derive(Subcommand, Clone)]
enum Commands {
    /// Start tracking a task
    Start {
        /// Task name (optional)
        name: Option<String>,
    },
    /// Take a break
    Break,
    /// Stop tracking
    Stop,
    /// Sync entries from server
    Sync,
    /// Push local changes to server
    Push,
    /// View the timeline
    View {
        #[arg(value_enum)]
        mode: Option<ViewModeArg>,
    },
    /// Start interactive monthly calendar view
    InteractiveView,
    /// Edit events (interactive)
    Edit,
    /// Undo last action
    Undo,
    /// Redo last action
    Redo,
    /// Manage favorites
    Favorites {
        #[command(subcommand)]
        action: Option<FavoritesAction>,
    },
    /// Manage Timebank
    Timebank,

    /// Manage configuration
    Config {
        #[command(subcommand)]
        action: Option<ConfigAction>,
    },
    /// Generate shell completions
    ///
    /// Example for zsh: source <(atime completions zsh)
    #[command(long_about = "Generate shell completions to stdout.

To install completions for zsh, you can add this to your ~/.zshrc:

    source <(atime completions zsh)

Or write it to a file in your $fpath:

    atime completions zsh > /usr/local/share/zsh/site-functions/_atime")]
    Completions {
        /// The shell to generate the completions for
        #[arg(value_enum)]
        shell: Shell,
    },
    /// Exit the shell
    Quit,
}

#[derive(Clone, ValueEnum)]
enum ViewModeArg {
    Day,
    #[clap(alias = "W")]
    Week,
    #[clap(alias = "M")]
    Month,
    #[clap(alias = "Y")]
    Year,
}

impl From<ViewModeArg> for ViewMode {
    fn from(arg: ViewModeArg) -> Self {
        match arg {
            ViewModeArg::Day => ViewMode::Day,
            ViewModeArg::Week => ViewMode::Week,
            ViewModeArg::Month => ViewMode::Month,
            ViewModeArg::Year => ViewMode::Year,
        }
    }
}

#[derive(Subcommand, Clone)]
enum FavoritesAction {
    Add,
    Remove,
    List,
}

#[derive(Subcommand, Clone)]
enum ConfigAction {
    SetToken { token: String },
    Autobreak { value: String },
}

struct AppContext<'a> {
    app_config: &'a mut Config,
    event_store: &'a EventStore,
    client: &'a alvtime::AlvtimeClient,
    external_tasks: &'a mut Vec<TaskDto>,
    holidays: &'a HashSet<NaiveDate>,
    today_history: &'a mut Vec<Event>,
    today_tasks: &'a mut Vec<Task>,
    view_mode: &'a mut ViewMode,
    editor: &'a mut Editor<InputHelper, DefaultHistory>,
}

fn main() {
    let cli = Cli::parse();

    let mut app_config = config::Config::load();
    let event_store = EventStore::new(&app_config.storage_path);

    // Setup Client
    let client = alvtime::AlvtimeClient::new(
        app_config.api_url.clone(),
        app_config.personal_token.clone(),
    );

    // Fetch tasks (Check cache first)
    let mut external_tasks = event_store.get_cached_tasks();
    if external_tasks.is_empty() {
        if let Ok(tasks) = client.list_tasks() {
            event_store.save_tasks(&tasks);
            external_tasks = tasks;
        } else {
            eprintln!("Warning: Failed to fetch tasks from API and cache is empty.");
        }
    }

    // Check and fetch holidays
    let current_year = Local::now().year();
    if !event_store.has_cached_holidays(current_year) && app_config.personal_token.is_some() {
        match client.list_bank_holidays(current_year, current_year) {
            Ok(holidays) => {
                event_store.save_holidays(current_year, &holidays);
                println!("Updated holidays for {}.", current_year);
            }
            Err(e) => eprintln!("Failed to fetch holidays: {}", e),
        }
    }

    // Load holidays into memory for the view
    let holidays = event_store.get_holidays();

    // 1. Load History (Current Day)
    let today = Local::now().date_naive();
    let mut today_history = event_store.events_for_day(today);

    // 2. Rebuild State (Current Day)
    let mut today_tasks = projector::restore_state(&today_history);

    // 3. Setup Editor (Shared between Shell and CLI modes)
    let helper = create_input_helper(&external_tasks, &app_config);
    let mut editor = Editor::new().expect("Failed to init readline");
    editor.set_helper(Some(helper));

    let mut view_mode = ViewMode::Day;

    // 4. Create Context
    let mut ctx = AppContext {
        app_config: &mut app_config,
        event_store: &event_store,
        client: &client,
        external_tasks: &mut external_tasks,
        holidays: &holidays,
        today_history: &mut today_history,
        today_tasks: &mut today_tasks,
        view_mode: &mut view_mode,
        editor: &mut editor,
    };

    if cli.shell {
        run_shell(&mut ctx);
    } else if let Some(command) = cli.command {
        // Apply autobreak before executing command
        if ctx.app_config.autobreak {
            if let Some(autobreak_feedback) =
                autobreak(ctx.today_tasks, ctx.today_history, ctx.event_store)
            {
                println!("{}", autobreak_feedback);
            }
        }

        let feedback = execute_command(command.clone(), &mut ctx);

        if matches!(command, Commands::InteractiveView) {
            println!("{}", feedback);
        } else if let Commands::View { .. } = command {
            let tasks = get_tasks_for_view(ctx.view_mode, ctx.event_store, ctx.today_tasks);
            if let Err(e) = view::draw_timeline(&tasks, ctx.view_mode, ctx.holidays) {
                eprintln!("Error drawing timeline: {}", e);
            }
        } else {
            println!("{}", feedback);
        }
    } else {
        Cli::command().print_help().unwrap();
    }
}

fn create_input_helper(tasks: &[TaskDto], config: &Config) -> InputHelper {
    InputHelper {
        commands: vec![
            "start".to_string(),
            "break".to_string(),
            "stop".to_string(),
            "view".to_string(),
            "sync".to_string(),
            "push".to_string(),
            "undo".to_string(),
            "redo".to_string(),
            "help".to_string(),
            "config".to_string(),
            "favorites".to_string(),
            "edit".to_string(),
            "timebank".to_string(),
            "interactive-view".to_string(),
            "quit".to_string(),
        ],
        tasks: tasks.to_vec(),
        favorites: config.favorite_tasks.clone(),
    }
}

fn execute_command(command: Commands, ctx: &mut AppContext) -> String {
    match command {
        Commands::Start { name } => {
            let mut parts = vec!["start"];
            if let Some(n) = &name {
                parts.push(n);
            }
            handle_start(
                &parts,
                ctx.today_tasks,
                ctx.today_history,
                ctx.event_store,
                ctx.app_config,
                ctx.external_tasks,
            )
        }
        Commands::Break => add_event(
            ctx.today_tasks,
            ctx.today_history,
            ctx.event_store,
            Event::BreakStarted {
                start_time: Local::now(),
                is_generated: false,
            },
            "Break started.",
        ),
        Commands::Stop => add_event(
            ctx.today_tasks,
            ctx.today_history,
            ctx.event_store,
            Event::Stopped {
                end_time: Local::now(),
                is_generated: false,
            },
            "Stopped.",
        ),
        Commands::Sync => handle_sync(
            ctx.client,
            ctx.today_tasks,
            ctx.today_history,
            ctx.event_store,
            ctx.external_tasks,
        ),
        Commands::Push => {
            // Process each day individually instead of loading all days at once
            let all_dates = ctx.event_store.get_all_dates_with_events();
            let mut overall_feedback = Vec::new();

            for date in all_dates {
                let mut day_history = ctx.event_store.events_for_day(date);
                let mut day_tasks = projector::restore_state(&day_history);

                // Push this specific day
                let day_result = handle_push(
                    ctx.client,
                    &mut day_tasks,
                    &mut day_history,
                    ctx.event_store,
                    ctx.external_tasks,
                );

                if !day_result.is_empty() {
                    overall_feedback.push(format!("{}: {}", date, day_result));
                }
            }

            // Refresh current day view context
            let today = Local::now().date_naive();
            *ctx.today_history = ctx.event_store.events_for_day(today);
            *ctx.today_tasks = projector::restore_state(ctx.today_history);

            if overall_feedback.is_empty() {
                "All days pushed successfully.".to_string()
            } else {
                overall_feedback.join("\n")
            }
        }
        Commands::View { mode } => {
            if let Some(m) = mode {
                *ctx.view_mode = m.into();
            }
            String::new()
        }
        Commands::Edit => handle_edit(
            ctx.holidays,
            ctx.event_store,
            ctx.external_tasks,
            ctx.app_config,
        ),
        Commands::InteractiveView => {
            match view::interactive_view(&*get_all_tasks(ctx.event_store), ctx.holidays, false) {
                Ok(_) => "".to_string(),
                Err(e) => format!("Error in interactive month view: {}", e),
            }
        }
        Commands::Undo => add_event(
            ctx.today_tasks,
            ctx.today_history,
            ctx.event_store,
            Event::Undo { time: Local::now() },
            "Undone.",
        ),
        Commands::Redo => add_event(
            ctx.today_tasks,
            ctx.today_history,
            ctx.event_store,
            Event::Redo { time: Local::now() },
            "Redone.",
        ),
        Commands::Favorites { action } => {
            let mut parts = vec!["favorites"];
            let binding_add = "add".to_string();
            let binding_remove = "remove".to_string();
            match action {
                Some(FavoritesAction::Add) => parts.push(&binding_add),
                Some(FavoritesAction::Remove) => parts.push(&binding_remove),
                _ => {}
            }
            handle_favorites(
                &parts,
                ctx.app_config,
                ctx.external_tasks,
                ctx.editor,
                ctx.event_store,
                ctx.client,
            )
        }
        Commands::Timebank => handle_timebank(ctx.client),
        Commands::Config { action } => {
            let mut parts = vec!["config"];
            let binding_token = "set-token".to_string();
            let binding_autobreak = "autobreak".to_string();
            let token_val;
            let auto_val;

            match action {
                Some(ConfigAction::SetToken { token }) => {
                    parts.push(&binding_token);
                    token_val = token;
                    parts.push(&token_val);
                }
                Some(ConfigAction::Autobreak { value }) => {
                    parts.push(&binding_autobreak);
                    auto_val = value;
                    parts.push(&auto_val);
                }
                None => {}
            }
            handle_config(&parts, ctx.app_config)
        }
        Commands::Completions { shell } => {
            let mut cmd = Cli::command();
            let bin_name = cmd.get_name().to_string();
            generate(shell, &mut cmd, bin_name, &mut io::stdout());
            String::new()
        }
        Commands::Quit => "Exiting...".to_string(),
    }
}


fn get_tasks_for_view(mode: &ViewMode, store: &EventStore, today_tasks: &[Task]) -> Vec<Task> {
    let now = Local::now();
    match mode {
        ViewMode::Day => today_tasks.to_vec(),
        ViewMode::Week => {
            let mut all_tasks = Vec::new();
            let start_of_week = now.date_naive()
                - chrono::Duration::days(now.weekday().num_days_from_monday() as i64);
            for i in 0..7 {
                let d = start_of_week + chrono::Duration::days(i);
                let events = store.events_for_day(d);
                all_tasks.extend(projector::restore_state(&events));
            }
            all_tasks
        }
        ViewMode::Month => {
            let mut all_tasks = Vec::new();
            let start_of_month = NaiveDate::from_ymd_opt(now.year(), now.month(), 1).unwrap();
            let mut d = start_of_month;
            while d.month() == now.month() {
                let events = store.events_for_day(d);
                all_tasks.extend(projector::restore_state(&events));
                d += chrono::Duration::days(1);
            }
            all_tasks
        }
        ViewMode::Year => {
            let mut all_tasks = Vec::new();
            let start_of_year = NaiveDate::from_ymd_opt(now.year(), 1, 1).unwrap();
            let mut d = start_of_year;
            while d.year() == now.year() {
                let events = store.events_for_day(d);
                all_tasks.extend(projector::restore_state(&events));
                d += chrono::Duration::days(1);
            }
            all_tasks
        }
    }
}

fn run_shell(ctx: &mut AppContext) {
    let mut feedback = format!(
        "Type 'help' for commands. Autobreak is {}.",
        if ctx.app_config.autobreak {
            "ON"
        } else {
            "OFF"
        }
    );

    // Default mode for shell
    *ctx.view_mode = ViewMode::Month;
    let today = Local::now().date_naive();

    loop {
        // Check day change
        let now = Local::now();
        if now.date_naive() != today {
            // In a real persistent app, we'd need to reload today_history here
        }

        if ctx.app_config.autobreak {
            if let Some(fb) = autobreak(ctx.today_tasks, ctx.today_history, ctx.event_store) {
                feedback = fb;
            }
        }

        print!("\x1b[2J\x1b[1;1H"); // Clear screen

        // Build tasks for view
        let tasks_to_view = get_tasks_for_view(ctx.view_mode, ctx.event_store, ctx.today_tasks);
        view::draw_timeline(&tasks_to_view, ctx.view_mode, ctx.holidays).unwrap();

        if !feedback.is_empty() {
            println!("\n{}", feedback);
            feedback.clear();
        }

        let readline = ctx.editor.readline("> ");
        match readline {
            Ok(line) => {
                let input = line.trim();
                if input.is_empty() {
                    continue;
                }

                let _ = ctx.editor.add_history_entry(input);

                let parts = input.split_whitespace();
                let mut args = vec!["atime"];
                args.extend(parts);

                match Cli::try_parse_from(args) {
                    Ok(cli) => {
                        if let Some(command) = cli.command {
                            if matches!(command, Commands::Quit) {
                                break;
                            }

                            // Pass the existing context re-borrowed
                            feedback = execute_command(command, ctx);

                            if feedback.starts_with("Token saved") {
                                println!("Token saved. Reloading session...");
                                break;
                            }
                        }
                    }
                    Err(e) => {
                        if e.kind() == clap::error::ErrorKind::DisplayHelp {
                            feedback = e.to_string();
                        } else {
                            feedback = format!("Error: {}", e);
                        }
                    }
                }
            }
            Err(ReadlineError::Interrupted) | Err(ReadlineError::Eof) => {
                println!("Exited");
                break;
            }
            Err(err) => {
                println!("Error: {:?}", err);
                break;
            }
        }
    }
}
