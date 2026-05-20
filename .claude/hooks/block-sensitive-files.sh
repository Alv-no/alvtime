#!/bin/bash
set -e
  
INPUT=$(cat)
TOOL=$(echo "$INPUT" | jq -r '.tool_name')

SENSITIVE_PATTERNS=(
  '\.env'
  'credentials'
  'secrets'
  '\.pem$'
  '\.key$'
  '\.keyfile$'
  '\.pfx$'
  '\.p12$'
  'keystore'
  'truststore'
  'password'
  '\.npmrc'
  '\.pypirc'
  'id_rsa'
  'id_ed25519'
  'token'
) 

# File extensions that are safe infrastructure-as-code definitions,
# not actual secret material (e.g. Terraform files named secrets.tf).
SAFE_EXTENSIONS=('\.tf$' '\.tfvars$')

is_safe_infra_file() {
  local text="$1"
  for pattern in "${SAFE_EXTENSIONS[@]}"; do
    if [[ "$text" =~ $pattern ]]; then
      return 0
    fi
  done
  return 1
}

has_sensitive_ref() {
  local text="$1"
  for pattern in "${SENSITIVE_PATTERNS[@]}"; do
    if [[ "$text" =~ $pattern ]]; then
      return 0
    fi
  done
  return 1
}

if [[ "$TOOL" == "Bash" ]]; then
  COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command')
  
  # Block any command that references a sensitive file pattern,
  # unless every matching word is a safe infra file (e.g. secrets.tf).
  if has_sensitive_ref "$COMMAND"; then
    BLOCKED=false
    for word in $COMMAND; do
      if has_sensitive_ref "$word" && ! is_safe_infra_file "$word"; then
        BLOCKED=true
        break
      fi
    done
    if $BLOCKED; then
      echo "Blocked: access to sensitive files is not allowed by project policy." >&2
      exit 2
    fi
  fi
else
  FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')
  if [ -n "$FILE_PATH" ] && has_sensitive_ref "$FILE_PATH" && ! is_safe_infra_file "$FILE_PATH"; then
    echo "Blocked: access to sensitive files is not allowed by project policy." >&2
    exit 2
  fi
fi

exit 0