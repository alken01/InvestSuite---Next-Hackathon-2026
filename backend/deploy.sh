#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Check for .env
if [ ! -f .env ]; then
  if [ -f .env.example ]; then
    cp .env.example .env
    echo "Created .env from .env.example — set your CLAUDE_API_KEY before running again."
    exit 1
  else
    echo "No .env file found."
    exit 1
  fi
fi

echo "Building image..."
docker compose build --pull

echo "Starting / updating container..."
docker compose up -d --remove-orphans

echo "Done. Container status:"
docker compose ps

echo ""
echo "Tail logs with:  docker compose logs -f"
