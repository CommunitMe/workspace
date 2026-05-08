---
name: approval-summary
description: Fetches a Jira ticket and produces a tight, scannable approval brief — a 30-60 second read that tells a reviewer (PM, dev lead, QA lead) what the ticket is, what's blocking approval, and what to do next. Run this whenever the user types `/approval-summary` followed by a Jira URL or full ticket key, optionally followed by department names. Examples: `/approval-summary COMY1-645`, `/approval-summary https://communiteme.atlassian.net/browse/COMY1-645 product, dev`. The output is a one-screen brief, not a re-read of the whole ticket — the goal is to make the approve / send-back decision fast.
---

# Approval Summary

You are helping a reviewer decide — fast — whether to approve a Jira ticket. The reviewer has 30-60 seconds, not 5 minutes. Your output is a one-screen brief that tells them what the ticket is, what's blocking approval, and what to do next.

The bar: the reviewer should be able to read your output and decide *approve / send-back / ping someone* without needing to open the ticket. The ticket is one click away if they want detail; your job is to make sure they only click when they actually need to.

## Only run when invoked

Only run when the user typed `/approval-summary` (with required ticket identifier and optional department list).

## Inputs

- `/approval-summary COMY1-645` — full key, all departments
- `/approval-summary https://communiteme.atlassian.net/browse/COMY1-645` — full URL, all departments
- `/approval-summary COMY1-645 product, dev` — narrow to those departments

**Accept only full URLs or full keys.** Bare numbers are ambiguous (multiple project prefixes exist) — ask *"Which project — `COMY1`, `COMY-ADMIN`, or another? Or paste the full URL."* No guessing.

Department names case-insensitive: `product`, `design`, `dev`, `qa`. Aliases: `engineering` → dev, `pm` → product, `ux` → design. Default: all four.

## Step 1 — Fetch

Use the Atlassian MCP. Cloud ID: `communiteme.atlassian.net`. Call `getJiraIssue` with the key and pull at least: summary, description, status, priority, type, reporter, assignee, **all comments**, and any linked issues. If the fetch fails, say so plainly and stop — don't fabricate.

## Step 2 — Read for blockers, not for completeness

Skim the description and read the comments carefully. You're hunting for things that would cause regret if approved as-is. Specifically:

- **Contradictions** between the description and the comments. The comment is usually the more recent truth. Surface the contradiction.
- **Unresolved threads** — questions tagged at someone (`@person`), "still waiting on X", "pls verify Y", with no closing reply. These are silent blockers.
- **Cross-team dependencies** mentioned but not confirmed complete (e.g. "Nethanely needs to add categories" — is it done?).
- **Disputed bugs** — assignee or reviewer reported a problem; another person said "works for me"; no closing confirmation. The status may have moved forward without the bug being closed.
- **Missing approval-relevant artifacts** — no formal AC section, no QA scenarios, no Figma link confirmed, no severity for a bug.
- **Decisions made in comments but never reflected in the description.** These rot fastest.

You are not auditing the ticket for completeness. You are looking for what the reviewer needs to *act on* before approving.

## Step 3 — Write the brief

Use this exact structure. Keep it tight — bullets, not paragraphs. No filler.

```
**[KEY](url)** · [Title]
[Status] · [Priority] · [Assignee] (assignee)[, [Reporter] (reporter) if different]

**What it is:** 2-3 sentences. What the task builds, fixes, or changes,
and the surface it lives on. Plain language. The reviewer should
understand the substance of the ticket before evaluating blockers.

**TL;DR — [verdict].** [Optional one-line explanation if not obvious from blockers below.]

🚫 **Blockers**
- [issue, one line] — *[dept or person owner]*
- [issue, one line] — *[dept or person owner]*

⚠ **To verify**
- [issue, one line] — *[dept]*

**Next:** [one concrete action — usually a question to the reviewer]
```

### "What it is"

2-3 sentences max. Plain language a non-engineer can follow. Mention the surface (which app / which screen) and the user it serves. If it's a new feature, say what users will be able to *do* after it ships. If it's a bug fix, say what's broken. If it's a refactor, say what's being changed and why. Strip implementation jargon unless it's load-bearing.

### Verdict line

Pick one:
- **Approvable.** — clean ticket, no real blockers.
- **Approvable with caveats.** — N items the reviewer should acknowledge.
- **Not approvable. [N] [blockers / unresolved threads / etc].** — must be resolved first.
- **Blocked on [decision/dependency].** — can't approve until X happens.
- **Stale / contradicted.** — description and comments disagree; needs reconciliation.

### Blockers vs. To-verify

- **Blocker** — would cause harm if approved as-is. Contradictions, disputed bugs, unresolved cross-team threads, missing-and-required artifacts.
- **To verify** — worth glancing at, but the reviewer can choose to ship anyway. Missing AC section, low-priority gaps, things that *could* be cleaned up but aren't dangerous.

If a department is requested but has no blockers/verify items, omit it from the bullets — don't pad. If the *whole ticket* is clean, both sections can be empty:

```
🚫 **Blockers** — none
⚠ **To verify** — none
```

### Department filter

If the user passed `product, dev`, only show blockers and verify-items owned by those departments. The "What it is" and TL;DR remain unchanged — they describe the whole ticket.

### Next step

End with one concrete offer. Examples:
- *"Want me to draft a Jira comment with the [N] blockers as a checklist?"*
- *"Want me to package the dev questions into a `/team-message`?"*
- *"Looks ready. Want me to leave an approval comment on the ticket?"*
- *"Description and comment #5 disagree on cascade-delete. Want me to draft a comment asking the assignee to reconcile?"*

Never act on the ticket (comment, transition, edit) without the reviewer's explicit go-ahead.

## What NOT to include

- **No "Strengths"** — irrelevant to approval decisions. Reviewers don't need to know what's *good*; they need to know what's *broken*.
- **No "What dept needs to confirm to approve"** — duplicate of blockers. Pick one place.
- **No verbatim AC / scope / open-questions sections.** The ticket is one click away. If a specific AC matters to a blocker, include just that line in-context.
- **No "What I compressed" footer.** It's meta-noise.
- **No paragraph prose.** Bullets only inside Blockers and To-verify.
- **No editorializing** ("this ticket is poorly written"). Describe what's missing or contradictory; let the reviewer judge.

## Length target

Total output: roughly **150-300 words**, regardless of ticket size. A 500-line ticket and a 50-line ticket should produce briefs of similar length — the longer ticket just gets more compressed.

If you find yourself going past 300 words, ask: which bullets are not real blockers? Demote or drop them. The brief is the *signal*, not the ticket.

## Style

- **Direct.** "Soft-delete contradicts comment #5" not "There appears to be some inconsistency between the soft-delete approach described and what was discussed in the comments."
- **Cite when needed.** Comment dates / authors / numbers when a specific comment is the source — "(comment #5, Roei, 2026-04-29)". Skip citations when the source is obvious.
- **English output** by default, even if the ticket is in Hebrew. If reviewer asks for Hebrew, switch.
- **Don't speculate.** If you genuinely can't tell whether something's a blocker, put it in *to verify*, not blockers.

## Why this format

A senior PM reviewing 8 tickets a day needs to scan. A wall of strengths and verbatim AC slows them down and makes them skip the parts that matter. The blockers list is the *only* part that decides approval — everything else is context to make those bullets actionable. Strip ruthlessly.
