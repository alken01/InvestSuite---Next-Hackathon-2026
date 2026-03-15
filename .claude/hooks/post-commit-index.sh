#!/bin/bash
# Post-commit hook: auto-update codebase-map.md after every commit
# Amends the commit to include the updated map

INPUT=$(cat)

# Only run on git commit commands
if ! echo "$INPUT" | grep -q '"git commit'; then
  exit 0
fi

CWD=$(echo "$INPUT" | sed -n 's/.*"cwd"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p')
if [ -z "$CWD" ]; then
  CWD="."
fi

MAP_FILE="${CWD}/docs/codebase-map.md"

# Skip if no codebase map exists
if [ ! -f "$MAP_FILE" ]; then
  exit 0
fi

# Get the last-indexed commit
BASELINE=$(grep -oP '(?<=last-indexed: )\w+' "$MAP_FILE" 2>/dev/null || grep -o 'last-indexed: [a-f0-9]*' "$MAP_FILE" 2>/dev/null | awk '{print $2}')
if [ -z "$BASELINE" ]; then
  exit 0
fi

# Get current HEAD (the commit that just happened)
NEW_HEAD=$(cd "$CWD" && git rev-parse --short HEAD 2>/dev/null)
if [ -z "$NEW_HEAD" ] || [ "$NEW_HEAD" = "$BASELINE" ]; then
  exit 0
fi

# Update the baseline stamp in the map
TODAY=$(date +%Y-%m-%d)
BRANCH=$(cd "$CWD" && git rev-parse --abbrev-ref HEAD 2>/dev/null)
sed -i "s/<!-- last-indexed: .* -->/<!-- last-indexed: ${NEW_HEAD} on ${BRANCH} at ${TODAY} -->/" "$MAP_FILE"

# Amend the commit to include the updated map
cd "$CWD" && git add docs/codebase-map.md && git commit --amend --no-edit 2>/dev/null

exit 0
