# Alvtime Rust CLI

A terminal-based CLI tool for interacting with Alvtime, written in Rust. This CLI allows you to manage your time entries, view your schedule, and sync with the Alvtime API directly from your terminal.

## Features

*   **Interactive Shell**: Built with `rustyline` for a readline-style experience with command history.
*   **Time Tracking**: Start, stop, and insert breaks easily.
*   **Task Management**: Fetch tasks from Alvtime and manage favorites.
*   **Offline Support**: Uses `sled` for local event sourcing storage, allowing you to work offline and sync later.
*   **Visualization**: View your timeline in Day, Week, Month, or Year modes.
*   **Undo/Redo**: Event-based architecture supports undoing and redoing actions locally.
*   **API Integration**: Sync and push your registered hours to the Alvtime backend.


![](atime.gif)

## Installation

### Prerequisites

*   [Rust & Cargo](https://www.rust-lang.org/tools/install) (Edition 2024)

### Building from Source

1.  Navigate to the `packages/rust-cli` directory.
2.  Use the provided `Makefile` to build and install:

```shell
# Build and install to /usr/local/bin
make install DESTDIR=/usr/local
```

Alternatively, you can build it manually with Cargo:
```shell
cargo build --release
```

## Configuration

Before using the CLI, you need to configure your personal access token from Alvtime.

1.  Start the CLI:
    ```sh
    atime
    ```
2.  Set your personal token:
    ```
    > config set-token <YOUR_ALVTIME_TOKEN>
    ```
2.  Set up favorite tasks:
    ```
    > favorties add
    ```

## Usage

Once inside the `atime` shell, the following commands are available:

### Time Management
*   `start `: Start registering time on a specific task from favorites.
*   `break`: Start a break.
*   `stop`: Stop the current activity.
*   `edit`: Edit entries (interactive mode).

### View & Navigation
*   `view [day|week|month|year]`: Switch the timeline view mode.
    *   Shortcuts: `v d`, `v w`, `v m`, `v y`

### Synchronization
*   `sync`: Fetch external tasks and reconcile local state with the server, pulls the full year.
*   `push`: Push your local registered hours to Alvtime.

### Other
*   `undo`: Undo the last local action.
*   `redo`: Redo the last undone action.
*   `favorites`: Manage favorite tasks for quicker access.
*   `config`: View or modify configuration.
*   `help`: Show the help text.
*   `quit` (or `Ctrl+D`): Exit the application.

## License

Internal Alvtime project.

