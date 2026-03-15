---
name: update-index
description: Scan the codebase and update docs/codebase-map.md with any new or removed files. Tracks the last indexed commit for incremental updates.
---

Update the codebase map at `docs/codebase-map.md` by scanning changes since the last indexed commit.

## Concept

The codebase map tracks the last commit it was indexed against via a metadata footer:
```
<!-- last-indexed: {commit-hash} on {branch} at {date} -->
```
On each run, diff from that commit to the current HEAD to find what changed.

## Steps

### 1. Determine baseline

- Read `docs/codebase-map.md` and find the `<!-- last-indexed: ... -->` footer.
- Extract the commit hash — this is the **baseline**.
- If not found (first run), do a full scan.

### 2. Find changes

```bash
git diff --name-only {baseline}..HEAD
```

From the changed files, determine which sections of the map need updating:
- Backend source/tests
- Frontend components/hooks/lib
- Docs
- Prompts
- Skills/commands

### 3. Scan affected areas

For each area with changes, verify what files currently exist and update the map tree accordingly.

### 4. Update the map

- Add new files/directories
- Remove deleted entries
- Update descriptions if file purpose changed
- Keep alphabetical order within sections

### 5. Stamp the new baseline

Update the metadata footer:
```
<!-- last-indexed: {HEAD hash} on {branch} at {YYYY-MM-DD} -->
```

### 6. Report

Print a summary:
```
Index update: {baseline}..{new_head} ({N} commits)
Added: file1, file2
Removed: file3
Updated sections: Backend, Frontend
New baseline: {new_head} (YYYY-MM-DD)
```

$ARGUMENTS
