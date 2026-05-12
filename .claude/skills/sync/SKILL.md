---
name: sync
description: "Refresh the workspace and pull latest dev across all submodules. Skips submodules currently on a feature branch so in-flight work is not disrupted."
---

# /sync — Daily Workspace Sync

When the user runs `/sync`, refresh the workspace and bring every submodule up to date with its `dev` branch — **without yanking the user off any active feature branch.**

## Step 1: Pull the workspace itself

From the workspace root:

```bash
git pull
```

This refreshes workspace-level files (CLAUDE.md, shared skills, .gitmodules, recorded submodule pointers).

If `git pull` reports a conflict or refuses to fast-forward, stop and tell the user — do not try to merge automatically.

## Step 2: Inspect each submodule's current branch

For each submodule, check the current branch before touching it:

```bash
git submodule foreach 'echo "$name: $(git rev-parse --abbrev-ref HEAD)"'
```

You'll get output like:

```
Entering 'members'
members: dev
Entering 'Admin_system'
Admin_system: claude/COMY1-123-some-feature
```

## Step 3: Update only the submodules on `dev`

For each submodule whose current branch is `dev` (or detached HEAD), check out `dev` and pull. **Skip any submodule on a feature branch** — those represent in-flight work and must not be yanked.

Run this single command, which fast-forwards `dev` only where safe:

```bash
git submodule foreach '
  current=$(git rev-parse --abbrev-ref HEAD)
  if [ "$current" = "dev" ]; then
    git pull --ff-only origin dev || echo "⚠️  $name: dev could not fast-forward (diverged?)"
  elif [ "$current" = "HEAD" ]; then
    git checkout dev && git pull --ff-only origin dev
  else
    echo "⏭️  $name: on feature branch ($current) — skipped"
  fi
'
```

## Step 4: Report results

Summarize for the user:

- **Updated:** which submodules were brought to latest `dev`
- **Skipped (feature branch):** which submodules were left alone, and the branch name — so the user can decide if they want to merge dev in manually
- **Warnings:** any submodule where `dev` had diverged and couldn't fast-forward

Example summary:

```
Sync complete.

✅ Updated to latest dev: cme_db, Onboarding, community-proj, pricelist_tools, notifications/utils
⏭️  Skipped (on feature branch):
   - members → claude/COMY1-123-deep-linking
   - Admin_system → claude/COMY1-123-deep-linking
⚠️  No warnings.

To pull dev into your feature branches:
   cd members && git pull origin dev
   cd Admin_system && git pull origin dev
```

## When NOT to use /sync

- **Mid-merge or mid-rebase** in any submodule — finish or abort first
- **Uncommitted changes** in a submodule — commit, stash, or discard first (sync will fail noisily but won't lose work)

## Why not `git submodule update --remote --merge`?

That command forces a merge of each submodule's tracked branch into wherever HEAD currently sits. When you're on a feature branch, it merges `dev` into your feature branch — often producing conflicts and leaving the submodule in a half-merged state. `/sync` avoids this by checking the branch first and only acting where safe.
