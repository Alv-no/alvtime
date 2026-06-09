#!/bin/bash
set -e

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command')

# Match git write subcommands anywhere in the command, regardless of
# path prefix (/usr/bin/git), env wrappers, subshells, or pipes.
GIT_WRITE="(^|[;&| \"'\`[:space:]])([^[:space:]]*/)?git[[:space:]]+(commit|push|merge|rebase|reset|add|stash|tag|branch[[:space:]]+-[Dd])"

if [[ "$COMMAND" =~ $GIT_WRITE ]]; then
  echo "Blocked: git write operations are not allowed by project policy." >&2
  exit 2
fi

exit 0