mod actions; // Added 'actions' mod declaration for completeness, even if empty
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
use crate::actions::utils::get_all_tasks;
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
use crate::actions::comment::handle_comments;

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
    /// Add a comment to one of the tasks
    Comment,
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

    if let Some(Commands::Completions { shell }) = cli.command.clone() {
        let mut cmd = Cli::command();
        let bin_name = cmd.get_name().to_string();
        generate(shell, &mut cmd, bin_name, &mut io::stdout());
        return;
    }

    let mut app_config = Config::load();
    let event_store: EventStore;
    match EventStore::new(&app_config.storage_path) {
        Ok(store) => {
            event_store = store;
        }
        Err(_) => {
            // Graceful (and whimsical) failure if the database remains locked after the retry loop
            eprintln!("\n\
\x1b[33m\x1b[1m+-------------------------------------------------------------+\n\
| A Solemn Decree from the Timeless Archive! 📜✨             |\n\
+-------------------------------------------------------------+\x1b[0m\n\
\n\
The \x1b[1mGrand Ledger of Time\x1b[0m (your database) is currently under the careful guard \n\
of a diligent \x1b[1mTimekeeping Elf\x1b[0m. This Elf is deep in concentration, meticulously \n\
recording recent events. \n\
\n\
The Ledger is held fast by a \x1b[1mMagical Lock of Exclusivity\x1b[0m to ensure the records \n\
remain pure and free from overlapping scribbles.\n\
\n\
Our little 'atime' Elf attempted to gain access for a patient, one-second vigil, \n\
but the lock held firm. The \x1b[1mElder Elf\x1b[0m (the other running instance) holds the key.\n\
\n\
\x1b[33m\x1b[1mTo proceed, the Elder Elf must first complete its work and respectfully close the \n\
Grand Ledger.\x1b[0m Please wait for the other instance to finish its duty, then try again.\n\
");
            // Exit the application with a non-zero status code
            std::process::exit(1);
        }
    }

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
            if let Err(e) = view::draw_timeline(
                &tasks,
                ctx.view_mode,
                ctx.holidays,
                &ctx.event_store.get_unsynced_dates(),
            ) {
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

            // --- START: Required logic for implicit Stop ---
            // Check if a task is already running (end_time is None)
            let is_running = ctx.today_tasks.iter().any(|t| t.end_time.is_none());
            let mut stop_feedback = String::new();

            if is_running {
                // If running, implicitly issue a Stop event
                stop_feedback = add_event(
                    ctx.today_tasks,
                    ctx.today_history,
                    ctx.event_store,
                    Event::Stopped {
                        end_time: Local::now(),
                    },
                    "\x1b[36m\x1b[1mThe Clockwork Cog pauses.\x1b[0m Your previous chapter is automatically closed.",
                );
            }
            // --- END: Required logic for implicit Stop ---

            let result = handle_start(
                &parts,
                ctx.today_tasks,
                ctx.today_history,
                ctx.event_store,
                ctx.app_config,
                ctx.external_tasks,
            );

            let start_feedback = if result.starts_with("Started task") {
                format!("\x1b[32m\x1b[1mThe Clockwork Cog has begun to turn!\x1b[0m Your 'upon a \x1b[1mTIME\x1b[0m' tale of labor has been noted.")
            } else {
                format!("\x1b[31m\x1b[1mThe Scroll of Beginnings is confused:\x1b[0m {}", result)
            };

            // Combine implicit stop feedback with start feedback
            if is_running {
                format!("{}\n{}", stop_feedback, start_feedback)
            } else {
                start_feedback
            }
        }
        Commands::Break => {
            add_event(
                ctx.today_tasks,
                ctx.today_history,
                ctx.event_store,
                Event::BreakStarted {
                    start_time: Local::now(),
                },
                "\x1b[36m\x1b[1mThe Timekeeping Elf has declared a recess.\x1b[0m Rest well, for the Grand Ledger awaits your return.",
            )
        }
        Commands::Stop => {
            // --- START: Required logic to ensure a task is running ---
            let is_running = ctx.today_tasks.iter().any(|t| t.end_time.is_none());

            if !is_running {
                return "\x1b[31m\x1b[1mThe Elder Elf is confused:\x1b[0m No task is currently running to stop. The Grand Ledger is at rest.".to_string();
            }
            // --- END: Required logic to ensure a task is running ---

            add_event(
                ctx.today_tasks,
                ctx.today_history,
                ctx.event_store,
                Event::Stopped {
                    end_time: Local::now(),
                },
                "\x1b[36m\x1b[1mThe quill has been set down.\x1b[0m Your current chapter is closed in the Grand Ledger.",
            )
        }
        Commands::Sync => {
            let result = handle_sync(
                ctx.client,
                ctx.today_tasks,
                ctx.today_history,
                ctx.event_store,
                ctx.external_tasks,
            );
            if result.starts_with("Synced") {
                format!("\x1b[33m\x1b[1mThe Whisperwind Post arrives!\x1b[0m The day's events are harmonized with the Elder Elves' Master Scroll.")
            } else {
                format!("\x1b[31m\x1b[1mA Snag in the Temporal Threads:\x1b[0m {}", result)
            }
        }
        Commands::Push => {
            // Process each day individually instead of loading all days at once
            let all_dates = ctx.event_store.get_unsynced_dates();
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

                // Filter out success messages, only push errors/warnings to feedback
                if !day_result.starts_with("Pushed") {
                    overall_feedback.push(format!("\x1b[31mDate {}:\x1b[0m {}", date, day_result));
                }
            }

            // Refresh current day view context
            let today = Local::now().date_naive();
            *ctx.today_history = ctx.event_store.events_for_day(today);
            *ctx.today_tasks = projector::restore_state(ctx.today_history);

            if overall_feedback.is_empty() {
                "\x1b[32m\x1b[1mThe Carrier Pigeons have flown!\x1b[0m All local scripts are now etched into the Mountain Archives.".to_string()
            } else {
                overall_feedback.insert(0, "\x1b[31m\x1b[1mObstacles on the Path to the Archives:\x1b[0m".to_string());
                overall_feedback.join("\n")
            }
        }
        Commands::View { mode } => {
            if let Some(m) = mode {
                *ctx.view_mode = m.into();
            }
            "\x1b[33m\x1b[1mThe Temporal Scope is Adjusted!\x1b[0m Now viewing the Grand Ledger through a new lens.".to_string()
        }
        Commands::Edit => {
            // We can't capture the internal feedback of handle_edit directly, but we can set a thematic success message
            let result = handle_edit(
                ctx.holidays,
                ctx.event_store,
                ctx.external_tasks,
                ctx.app_config,
            );
            if result.starts_with("Error") {
                format!("\x1b[31m\x1b[1mA Glitch in the Loom of Time:\x1b[0m {}", result)
            } else {
                // handle_edit is interactive and should print its own success/error messages, so this is just a confirmation of initiation
                "\x1b[33m\x1b[1mOpening the Scroll of Revisions.\x1b[0m The Elder Elf is ready to mend the time fabric.".to_string()
            }
        }
        Commands::Comment => {
            let result = handle_comments(
                ctx.holidays,
                ctx.event_store,
            );

            if result.starts_with("Error") {
                format!(
                    "\x1b[31m\x1b[1mThe Ink Sprite refuses your annotation:\x1b[0m {}",
                    result
                )
            } else if result.contains("updated") {
                "\x1b[33m\x1b[1mA fresh note is etched into the margin.\x1b[0m The Ledger now whispers your thoughts."
                    .to_string()
            } else if result.contains("removed") {
                "\x1b[33m\x1b[1mThe note fades from the parchment.\x1b[0m Silence returns to that moment in time."
                    .to_string()
            } else if result.contains("No tasks") {
                "\x1b[31m\x1b[1mThe Quill is idle:\x1b[0m No tasks on this day to annotate."
                    .to_string()
            } else {
                "\x1b[34m\x1b[1mThe Annotation Scroll opens.\x1b[0m Choose wisely what words you bind to time."
                    .to_string()
            }
        }
        Commands::InteractiveView => {
            match view::interactive_view(
                &*get_all_tasks(ctx.event_store),
                ctx.holidays,
                &ctx.event_store.get_unsynced_dates(),
                false,
            ) {
                Ok(_) => "".to_string(), // Interactive view clears screen, no need for feedback here.
                Err(e) => format!("\x1b[31m\x1b[1mThe Map of the Months is torn:\x1b[0m {}", e),
            }
        }
        Commands::Undo => add_event(
            ctx.today_tasks,
            ctx.today_history,
            ctx.event_store,
            Event::Undo { time: Local::now() },
            "\x1b[35m\x1b[1mThe Hourglass is Reversed!\x1b[0m The last mark is scrubbed from the Grand Ledger.",
        ),
        Commands::Redo => add_event(
            ctx.today_tasks,
            ctx.today_history,
            ctx.event_store,
            Event::Redo { time: Local::now() },
            "\x1b[35m\x1b[1mThe Sands are Restored!\x1b[0m The forgotten mark has been brought back to the Grand Ledger.",
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
            let result = handle_favorites(
                &parts,
                ctx.app_config,
                ctx.external_tasks,
                ctx.editor,
                ctx.event_store,
                ctx.client,
            );
            // Favorites handling is interactive or has complex feedback, just wrap the result
            if result.starts_with("Error") {
                format!("\x1b[31m\x1b[1mThe Scroll of Favors is Tangled:\x1b[0m {}", result)
            } else {
                format!("\x1b[33m\x1b[1mThe Scroll of Favors Updated:\x1b[0m {}", result)
            }
        }
        Commands::Timebank => {
            let result = handle_timebank(ctx.client);
            if result.starts_with("Error") {
                format!("\x1b[31m\x1b[1mThe Timebank Golem is uncooperative:\x1b[0m {}", result)
            } else {
                format!("\x1b[34m\x1b[1mThe Treasury's Count:\x1b[0m\n{}", result)
            }
        }
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
            let result = handle_config(&parts, ctx.app_config);

            if result.starts_with("Token saved") {
                // Special case, needs to trigger a break and reload
                result
            } else if result.starts_with("Error") {
                format!("\x1b[31m\x1b[1mThe Edict of Settings is Invalid:\x1b[0m {}", result)
            } else {
                format!("\x1b[33m\x1b[1mThe Edict of Settings has been updated:\x1b[0m {}", result)
            }
        }
        Commands::Completions { shell } => {
            let mut cmd = Cli::command();
            let bin_name = cmd.get_name().to_string();
            generate(shell, &mut cmd, bin_name, &mut io::stdout());
            "\x1b[32m\x1b[1mThe Word-Weaver's Spell is Cast!\x1b[0m Completion scrolls sent to the shell's feet.".to_string()
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
        "\x1b[34m\x1b[1mWelcome, Time Traveler!\x1b[0m Type 'help' to consult the Lorekeeper. The 'Auto-Nap' charm is {}.",
        if ctx.app_config.autobreak {
            "\x1b[32mACTIVE\x1b[34m\x1b[1m"
        } else {
            "\x1b[31mDORMANT\x1b[34m\x1b[1m"
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
                // Autobreak is a BreakStarted event, use the thematic message
                feedback = fb.replace("Break started.", "\x1b[36m\x1b[1mThe Auto-Nap Charm Activated!\x1b[0m The Timekeeping Elf insisted you take a pause.");
            }
        }

        print!("\x1b[2J\x1b[1;1H"); // Clear screen

        // Build tasks for view
        let tasks_to_view = get_tasks_for_view(ctx.view_mode, ctx.event_store, ctx.today_tasks);
        view::draw_timeline(
            &tasks_to_view,
            ctx.view_mode,
            ctx.holidays,
            &ctx.event_store.get_unsynced_dates(),
        )
            .unwrap();

        if !feedback.is_empty() {
            println!("\n{}", feedback);
            feedback.clear();
        }

        // Custom, themed prompt
        let readline = ctx.editor.readline("\x1b[32mThe Lorekeeper awaits your command:\x1b[0m ");
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
                                println!("\x1b[35m\x1b[1mThe Grand Ledger is carefully closed.\x1b[0m Farewell for now!");
                                break;
                            }

                            // Execute command and get themed feedback
                            feedback = execute_command(command, ctx);

                            // Check for the special case of 'Token saved' which requires a reload/exit
                            if feedback.starts_with("Token saved") {
                                println!("\x1b[33m\x1b[1mThe Elder Elf received the new Seal of Authority!\x1b[0m Please restart to acknowledge the change.");
                                break;
                            }
                        }
                    }
                    Err(e) => {
                        if e.kind() == clap::error::ErrorKind::DisplayHelp {
                            // Clap help is fine, just use the error string
                            feedback = e.to_string();
                        } else {
                            // Translate general parsing errors
                            feedback = format!("\x1b[31m\x1b[1mThe Word-Weaver did not understand the spell:\x1b[0m {}", e);
                        }
                    }
                }
            }
            Err(ReadlineError::Interrupted) | Err(ReadlineError::Eof) => {
                println!("\x1b[35m\x1b[1mThe Grand Ledger is carefully closed.\x1b[0m Farewell for now!");
                break;
            }
            Err(err) => {
                println!("\x1b[31m\x1b[1mA Glitch in the Crystal Ball:\x1b[0m {:?}", err);
                break;
            }
        }
    }
}