Create a git commit with an auto-updated codebase map.

## Steps

### 1. Update the codebase index

Run the `/update-index` skill to scan for any new, moved, or deleted files since the last indexed commit. This updates `docs/codebase-map.md` with the current file tree and stamps the new baseline commit.

### 2. Update memory (if needed)

If this session involved significant decisions, new patterns, or debugging insights, update `MEMORY.md` in the auto-memory directory before committing. Skip for routine changes.

### 3. Stage and commit

1. Run `git status` to see all changes (staged + unstaged + untracked)
2. Run `git diff` to review what changed
3. Run `git log --oneline -5` to match the repo's commit message style
4. Stage all relevant files — including the updated `docs/codebase-map.md`
5. Draft a concise commit message (1-2 sentences, focus on "why" not "what")
6. Commit the changes
7. Run `git status` to verify success

## Rules

- Do NOT push unless explicitly asked
- Do NOT commit files with secrets (.env, credentials)
- Do NOT use `git add -A` — stage specific files by name
- Use a HEREDOC for the commit message to preserve formatting
- Do NOT add "Co-Authored-By" or any AI attribution to commit messages
- If a pre-commit hook fails, fix the issue and create a NEW commit (don't amend)

$ARGUMENTS
