# Alvtime CLI client

This is the Alvtime CLI client which lets you register your work in Alvtime without
leaving your precious terminal.

## Installation

Prerequisites:

- python 3.13+
- uv or pipx

both of which should be availble in a package manager near you

```
uv tool install git+https://github.com/Alv-no/alvtime.git#subdirectory=packages/cli
```

or with pipx:

```
pipx install git+https://github.com/Alv-no/alvtime.git#subdirectory=packages/cli
```

## Configure personal access token

Obtain a personal access token (PAT) for Alvtime at https://classic.alvtime.no/#/tokens and
add it to Alvtime CLI:

```
❯ alvtime config pat set
Create a personal access token at https://classic.alvtime.no/#/tokens.
Personal access token:
```

## Usage

### Aliases

To easily create time entries, without having to remember long names or seemingly
random IDs, Alvtime CLI lets you create aliases for the tasks so they can be
referred to using a easily remembered name.

List and filter available tasks by running:

```
alvtime tasks list <search string>
```

Then add the desired task as an alias by running:

```
alvtime alias add <alias name> <task id>
```

*Example*

```
❯ alvtime tasks list "Annen interntid"
[12] Alv Interntid 50% Annen interntid (50.0%)

❯ alvtime alias add alv-intern 12
```

Now, the alias `alv-intern` has been registered.

### Registrations

Work is primarily registered with Alvtime CLI using the `start` and `stop` commands:

```
❯ alvtime start --help
Usage: alvtime start [OPTIONS] ALIAS [COMMENT]

  Starts an activity

Options:
  --at DATETIME
```

and

```
❯ alvtime stop --help
Usage: alvtime stop [OPTIONS]

  Stops current activity

Options:
  --at DATETIME
  --comment TEXT
```

### Pull and sync

Use `pull` to fetch entries from Alvtime to your local system.

```
Usage: alvtime pull [OPTIONS]

  Pulls time entries from the server, merging it with local storage

Options:
  --from DATE
  --to DATE
```

Then use `sync` to update Alvtime

```
❯ alvtime sync --help
Usage: alvtime sync [OPTIONS]

  Synchronizes time entries with the server, pulling first, then pushing.

Options:
  --from DATE
  --to DATE
```

Note: The `sync` commands automatically performs a pull before pushing to
Alvtime.

### Auto-sync

Alvtime CLI has an `auto-sync` feature, which automatically perfomrs a
`sync` after any command that changes a time entry. To enable it, run:

```
❯ alvtime config auto-sync on
auto-sync enabled
```

## Shell completion

To enable shell completion, which you should, add the following to your shell's
config:

For `zsh` add this to `~/.zshrc`:

```
eval "$(_ALVTIME_COMPLETE=zsh_source alvtime)"
```

For `bash` add this to `~/.bashrc`:

```
eval "$(_ALVTIME_COMPLETE=bash_source alvtime)"
```

For other shells, please provide a PR 🤷‍♂️
