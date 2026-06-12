---
name: implement
description: "Implement a Jira ticket end-to-end: fetch ticket, plan, code, test, verify, fix, PR"
---

# /implement — Full Ticket Implementation

When the user runs `/implement TICKET-KEY` (e.g. `/implement COMY1-106`), follow this pipeline:

## Step 1: Fetch the Jira Ticket

Use the Atlassian MCP to get the ticket details:
- Call `mcp__atlassian__getJiraIssue` with cloudId `communiteme.atlassian.net` and the ticket key
- Extract: summary, description, acceptance criteria, linked Figma URLs
- If the ticket has a Figma link, use `mcp__figma__get_design_context` to get the design

## Step 2: Route to Project

Determine which project(s) this ticket involves based on the ticket description and requirements.
A ticket can involve **multiple projects** (e.g. a new feature needs frontend in `members/` + API in `Admin_system/` + notifications in `notifications/`).

Available projects:
- `members/` — Community members platform. Next.js 16, React 19, Prisma, Tailwind v4, Radix UI, TanStack Query, Server Actions, next-intl (Hebrew/RTL)
- `Admin_system/` — Supplier/member/community management API. Express 4, TypeScript, raw PostgreSQL (no ORM), JWT cookie auth, MinIO file storage, Jest
- `Onboarding/` — User onboarding flows. Monorepo: Vite/React 18 client (TanStack Router, Zustand, MUI 7, Tailwind 3) + Express server (Passport OAuth, Prisma, SendGrid/Brevo)
- `community-proj/` — Community management SPA. React 18 (JavaScript, NOT TypeScript), CRA, MUI 6, react-router-dom 6, Formik+Yup, Axios, i18next, SCSS
- `notifications/` — Real-time notification microservice. Express 5, TypeScript 5.9, Prisma 7, PostgreSQL, Socket.IO 4, AJV validation, JWT auth

The project directory is at: `C:/Users/User/Documents/keilot/workspace/{project}/`

**For multi-project tickets:**
- Read each project's SKILL.md separately
- Plan tasks per project (e.g. "API endpoint in Admin_system" + "UI component in members")
- Spawn agents with the correct project directory for each task
- Coordinate contracts between projects (e.g. API response shape must match frontend types)
- Create separate git branches and PRs per project that has changes

## Step 3: Load Memory & Learn from Past Runs

Before planning, read the lessons file:
- `C:/Users/User/Documents/keilot/workspace/.claude/memory/implement_lessons.md`

This file contains lessons from previous runs — patterns that worked, mistakes to avoid,
conventions discovered. Apply these lessons to your plan.

## Step 4: Explore & Plan (YOU are the PM)

Before writing any code, explore the codebase:
1. Read the project's SKILL.md: `{project}/.claude/skills/*/SKILL.md`
2. Use Glob to find existing files related to the ticket (components, actions, types, translations)
3. Use Grep to find related patterns (imports, hooks, utilities)
4. Read 2-3 key existing files to understand conventions

Then create a plan with:
- **EXISTING**: What already exists and can be reused
- **CREATE**: What needs to be created
- **MODIFY**: What needs to be modified
- **ORDER**: Dependencies between tasks
- **ACCEPTANCE CRITERIA**: How to verify each task is done (derived from ticket + Figma + your analysis)

Save the plan to `{project}/.hivemind/PLAN.md`.

**DO NOT ask the user for approval.** You are autonomous — execute the plan immediately.
Only ask the user if you have a genuine question that blocks progress (e.g. ambiguous requirements,
conflicting designs, missing credentials). The user is NOT a babysitter.

## Step 5: Implement (Spawn Agent Team)

Create an agent team to implement the plan. Teammates work in parallel on independent tasks
and coordinate through shared task lists.

Tell Claude to create a team with teammates for each independent work stream. For example:
"Create a team with 3 teammates: one for the server action and types, one for the UI components,
and one for translations. Each should work in C:/Users/User/Documents/keilot/workspace/members/"

**Critical rules for teammate instructions:**
- Tell each teammate the EXACT project directory to work in
- List existing files it should read first
- List conventions from SKILL.md it must follow
- Tell it what other teammates are building (so it knows what to expect)
- Tell it NOT to run git commands
- Tell it to write a handoff summary to .hivemind/HANDOFF_{task}.md when done

**Team strategy:**
- Independent tasks → separate teammates working in parallel
- Dependent tasks → use task dependencies so they unblock automatically
- Each teammate owns different files to prevent conflicts
- 3-5 teammates is the sweet spot

## Step 6: PM Verification Loop (CRITICAL)

After all developer tasks complete, YOU (the PM) verify the implementation against the plan.
This is a loop — keep going until everything passes:

```
WHILE not all acceptance criteria pass:
  1. Read every file that was created/modified
  2. Check each acceptance criterion from the plan:
     - [ ] Feature works as described
     - [ ] Follows project conventions (from SKILL.md)
     - [ ] No orphaned files or imports
     - [ ] Translations complete (Hebrew + English for members)
     - [ ] Types are correct and consistent
     - [ ] No hardcoded strings (use translations)
     - [ ] RTL-safe (for members project)
  3. If ANY criterion fails:
     - List the specific failures
     - Spawn a fix agent to correct them
     - After fix agent completes, loop back to step 1
  4. If ALL criteria pass:
     - Write VERDICT: PASS to {project}/.hivemind/PM_REVIEW.md
     - Exit the loop
```

**Max 3 fix rounds.** If still failing after 3 rounds, report remaining issues to the user.

### Backend verification — DB QA script pattern (members/)

Unit tests with mocked Prisma prove the shape of the queries. They do **not** prove the change works against the real schema, real transaction semantics, or real row-level constraints. For backend changes that touch `prisma.$transaction`, `is_default`-style mutual-exclusion flags, dedup keys, or any logic where "works in unit tests" isn't enough, **drive the production code against the dev DB with a self-cleaning script**:

```
snapshot the targeted rows for the test profile
exercise the production code (inline-copy the action handler if needed
  to skip the next-safe-action wrapper)
assert expected DB state after each step
delete any rows the script created
restore the snapshot exactly
print PASS/FAIL count
```

Hard rule: **the script must restore the user's data exactly**. Snapshot at start, diff at end, fail the run if final state ≠ initial state.

The auto-mode classifier will block the script's first execution because it's a write to real user data. **Do not work around it** — call `AskUserQuestion` with the exact list of rows you'll touch + the cleanup guarantee, get an explicit yes, then run. This pattern verified all 4 backend findings on COMY1-287 in one ~10s run (9/9 PASS) against the real `member_saved_cards` table without leaving any residue. Reference implementation lived at `members/scripts/qa-backend-e2e.ts` during PR #189 (deleted post-merge).

## Step 7: Build & Test

Run the project's build and tests:
- `members/`: `cd {project} && npx tsc --noEmit` (type check)
- `Admin_system/`: `cd {project} && npx tsc`
- `notifications/`: `cd {project} && npx tsc`
- `community-proj/`: `cd {project} && npx tsc`
- `Onboarding/`: `cd {project} && npx tsc`

If build fails, fix the errors (spawn fix agent if needed), then re-verify (back to Step 6).

### Pre-existing noise in `members/scripts/`
The `members/scripts/` directory contains untracked ad-hoc QA scripts (e.g. `kard-qa-check-order.ts`) that use BigInt literals and don't compile against the project's `tsconfig.json` target. `npx tsc --noEmit` will emit `TS2737: BigInt literals are not available when targeting lower than ES2020` errors from there even on a clean workspace. **Filter them out** with `npx tsc --noEmit 2>&1 | grep -vE "^scripts/"` before deciding the type check failed.

### Stale `.next/dev/types` after deleting a route
After deleting a route handler or page (including the temp QA pages in `src/app/[locale]/dev/...`), Next.js's incremental type cache at `.next/dev/types/validator.ts` keeps a stale `import` to the deleted module and emits `TS2307: Cannot find module ...page.js`. The cache is regenerated on the next `npm run dev`, but if you're tsc-ing after the page has been deleted, **delete `members/.next/dev/types` first** to avoid a false-positive failure. `.next/` is gitignored so it doesn't affect the PR.

### RTL bidi traps (members/)
Three real traps caught on COMY1-287. Single source of truth lives in `members/.claude/skills/commy-development/references/rtl-i18n.md` under "RTL Bidi Traps and Workarounds" — read it before any work involving latin-script content inside Hebrew RTL. Headline rule: **`dir="ltr"` on a `<div>` is silently overridden** by `globals.css:198`; use `<span dir="ltr">` instead. SVGs with `<text>` need `direction="ltr"` as an attribute on the `<svg>`. Quantity+unit cells (`"1 item"`, `"25 meter"`) need the same span wrapper. Verify by measuring `Range.getBoundingClientRect()` left positions — computed `direction` alone can lie.

### Nested radix Dialog trap (members/)
`vaul`'s `Drawer` wraps a radix `Dialog`. If you render it INSIDE another radix `Dialog` (like the Kard modal), the two focus traps fight and click events on the inner trigger get swallowed silently. **Render the nested drawer as a sibling of the outer Dialog, not a descendant.** Symptom: a help icon button with an `onClick` handler that fires `setOpen(true)` and yet the drawer never opens, no errors logged. Full pattern in `members/.claude/skills/commy-development/references/kard.md` §6.

### Order modal step deep links (members/)
The home page order modal uses `?orderId=…&step=N&pricelist=…`. **`step=10` is post-payment feedback ("AllDone"), not "view order".** For viewing a `WAITING_FOR_PAYMENT` order, use `step=8`. Authoritative `status_id → step` mapping is in `src/components/MyOrders/my-orders-tab.tsx` `STATUS_ID_TO_STEP`. Derive from there, never hardcode `step=10` thinking it means "show details".

### Canceled vs completed share the same step (members/)
`STATUS_NAME_TO_STEP` collapses `CLOSED` (50), `CANCELED_BY_MEMBER` (100), and `CANCELED_BY_SUPPLIER` (101) all onto step 10 = `Step11OrderCompleted`. Any per-step UI string in `MembersOrderModalContent.tsx` (sidebar title, header label, CTA copy) must branch on a separate `isOrderCanceled = orderDetails?.status === "CANCELED_BY_MEMBER" || === "CANCELED_BY_SUPPLIER"` derivation — **never on the existing `orderCancelled` memo, which is offer-based** ("all suppliers declined while searching") and means something different. The two flags can both be true or both be false; don't conflate them. Fixed in COMY1-278 / PR #194.

### MinIO presigned uploads & browser reachability (members/)
The order-attachment upload flow at `src/components/Modals/MembersOrderModal/MembersOrderModalContent.tsx` → `uploadAttachments` does browser → MinIO PUT via a presigned URL built from `MINIO_ENDPOINT` (see `server/lib/config/minio.ts`). If that endpoint is an **internal hostname** (docker name / private IP) or `MINIO_USE_SSL=false` behind an https app, **the browser silently blocks the PUT** (DNS failure or mixed-content) and the only client-visible signal is the generic `steps.uploadError` toast (`"אירעה שגיאה בהעלאת הקבצים. אנא נסו שוב"`). Before chasing a code bug for "upload fails for large videos but images work" (where in practice images sometimes squeak through caches), **first check the staging `.env` for a public HTTPS `MINIO_ENDPOINT`** — it's almost always env, not code. The signed-URL/confirm route pair (`src/app/api/orders/[orderId]/files/{signed-url,confirm}/route.ts`) is structurally fine; reverting to server-proxied upload contradicts the deliberate move in commit `f87390e` and reintroduces Next.js body-size/Turbopack issues.

## Step 7b: Browser QA (for frontend tasks)

If the ticket involves UI changes (components, pages, styling), do a visual check.

### Setup — launch.json (workspace root, NOT submodule root)

The preview tool reads `C:/Users/User/Documents/keilot/workspace/.claude/launch.json`. It runs from the workspace root, so `npm run dev` would invoke the workspace's concurrent-all-projects script. Use `--prefix <project>` instead:

```json
{
  "version": "0.0.1",
  "configurations": [
    {
      "name": "members-dev",
      "runtimeExecutable": "npm",
      "runtimeArgs": ["--prefix", "members", "run", "dev"],
      "port": 3100
    }
  ]
}
```

Default ports per project:
- `members/` — **3100** (hardcoded `-p 3100` in package.json dev script)
- `community-proj/` — 3000
- `Onboarding/` — 5173

If port 3100 is already taken by a stale node process (common — user may have a leftover dev server), the preview tool refuses to start. Identify with `Get-Process -Id <pid>` and ask the user before killing.

### Workflow

1. **Start the server**: `preview_start name="members-dev"`. Confirm port from the returned `serverId` + `port`.
2. **Navigate**: `preview_eval` with `window.location.href = 'http://localhost:3100/he/...'`. Wait ~4-5s for Next.js to compile new routes.
3. **For `members/`, get past the auth gate.** All non-public paths redirect unauthenticated users to the Onboarding `/welcome` page (port 5173). If your route needs to be reachable without login, temporarily add it to `publicPaths` in `members/src/middleware/types/index.ts`:
   ```ts
   export const publicPaths = ["/api/health", "/api/auth", "/api/files/order", "/he/dev", "/en/dev"];
   ```
   Revert the change before committing.
4. **Test in BOTH locales**: `/he/...` (RTL) and `/en/...` (LTR). The `<html dir>` is set by the `[locale]/layout.tsx` from `rtl-detect`.
5. **Verify**:
   - `preview_inspect` for computed CSS values — STRONGEST evidence for layout/position/color fixes. Use for RTL checks: read `left` and `right` computed styles directly.
   - `preview_snapshot` for the a11y tree — confirms text, roles, accessible names.
   - `preview_console_logs level=error` for runtime errors.
   - `preview_screenshot` LAST. **Screenshots time out** in members/ dev mode when SocketProvider reconnect loops + heavy components (qrcode.react, modals) stall the renderer. If it times out, the inspect+snapshot data is sufficient evidence — don't waste cycles retrying.
6. **Stop the server**: `preview_stop`.

### Real Chrome > headless preview — when to switch (members/)

Headless preview rendering is **necessary but not sufficient** for any change involving Hebrew RTL or latin-script content in `/he`. Three classes of bugs **only surfaced in the user's real Chrome session at `localhost:3100/he`** during COMY1-287:

| Bug class | Headless said | Real Chrome showed | Why |
|---|---|---|---|
| `dir="ltr"` on `<div>` (F7) | DOM attribute correct | digits flipped on screen | computed style `direction: rtl` from `globals.css !important` |
| SVG `<text>` clipped to "ay" (F6/F13) | viewBox attributes correct | half the logo missing | SVG text inherits RTL → `getBBox().x` goes negative |
| Nested-Dialog click swallow (R2) | `onClick` handler attached | drawer never opened | two radix focus traps fight, no error logged |

**Rule:** for `members/` UI work in Hebrew, finish in real Chrome at `/he`. Drive it via the `Claude_in_Chrome` MCP if the user is logged in (`list_connected_browsers` → `select_browser` → use `find` + `hover`/`computer`). Headless preview is fine for English LTR and for the structural shape, but it WILL miss bidi traps. When the user has a tab on `localhost:3100/he` already authenticated, that's the strongest verification environment available.

### PR body as living artifact (members/)

Don't fill the PR body once at branch-push time and forget it. Treat the test plan as a checklist that ticks as evidence accumulates:

1. **Initial push**: leave all test-plan boxes `[ ]` unchecked, just list them.
2. **After each verification step**: `gh pr edit <num> --body-file ...` to tick the boxes you proved, inline the evidence (computed values, screenshot reference IDs, log lines, DB query results).
3. **When the user finds a bug post-push**: re-open the relevant boxes with a `_round 2: untickable, see commit <sha>_` note, push the fix as a new commit, then re-tick.
4. **At review-ready**: every box should either be `[x] verified live` or `[ ] covered by unit test only, see <test-file>`. Be honest about which is which — reviewers trust a PR that distinguishes them.

Auto-mode safety net mindset: when the classifier blocks a destructive action (DB write, `publicPaths` bypass, hook skip, force-push), **don't try to work around it.** Call `AskUserQuestion` with the exact action + reason + cleanup plan. The block is doing its job. Working around it with creative tool chains is how you lose user data.

### Gated screens (modals reached only through deep flows)

For screens that only mount inside a modal triggered by upstream state (e.g. Kard screens behind a QR scan + supplier fetch), create a **temporary test page** that imports the components directly with mocked props:

```tsx
// src/app/[locale]/dev/<feature>-test/page.tsx — DELETE before commit
"use client";
import { TheComponent } from "@/components/...";
const mockProps = { /* satisfy the component's types */ };
export default function TestPage() { return <TheComponent {...mockProps} />; }
```

After QA: `rm -rf src/app/[locale]/dev`, revert the `publicPaths` edit. Verify with `git status` that only the intended fix files remain modified.

## Step 8: Create PR

After PM verification passes:

### Branch hygiene first (submodule realities)

Each project is a git submodule and may already be on a feature branch from prior work with **unrelated dirty files** (e.g. sibling submodule pointer drift, `package-lock.json` from an unrelated install). Check before branching:

```bash
cd {project} && git status --short && git branch --show-current
```

If sitting on a feature branch or there are unrelated dirty files:
1. Stash ONLY your fix files by pathspec: `git stash push -m "{TICKET}-changes" -- <file1> <file2> ...`
2. Branch from the right base (see below): `git checkout origin/<base> -b claude/{TICKET-KEY}-{short-description}`
3. Pop the stash: `git stash pop` — auto-merge usually clean since you're moving to a fresh tip
4. Verify only your fix files appear modified: `git diff --stat <files>`

### Base branch per project

- `members/` — **`dev`** (NOT master/main)
- Other projects: check `git symbolic-ref refs/remotes/origin/HEAD` to confirm the default
- The workspace root uses `master`, but that's separate from submodules

### Commit + push

1. Stage specific files (NOT `git add .` — your working tree may have unrelated dirt): `git add <file1> <file2> ...`
2. Commit: `git commit -m "[TICKET-KEY] description"`
3. Push: `git push -u origin {branch}`

### Create PR via gh CLI

`gh` is NOT on the default Git Bash PATH on Windows — use PowerShell with the full path:

```powershell
& "C:\Program Files\GitHub CLI\gh.exe" pr create --base dev --head <branch> `
  --title "[TICKET-KEY] description" --body-file "$env:TEMP\pr-body.md"
```

Pass the body via `--body-file` with a here-string written to `$env:TEMP\pr-body.md`. Single-quoted here-strings (`@'...'@`) avoid PowerShell variable expansion; double-up apostrophes inside.

### Jira transition

`mcp__atlassian__transitionJiraIssue` may be **blocked by auto-mode** as an "External System Write". Don't retry on denial — leave it for the user. Even when allowed: tickets in "To Do" typically don't have a direct "In Review" transition — only `To Do → In Progress` first, then onward from there.

### Workspace pointer (submodule pin)

**Do NOT bump the workspace-root submodule pointer for an unmerged PR branch.** That would pin the workspace at a commit that doesn't exist on `dev` yet. Standard flow: bump after the PR merges, ideally in a batch with other merged submodules.

## Step 9: Learn & Memorize

After EVERY run, capture lessons inline. There is no `upgrader` agent on this machine — do it yourself:

1. If you discovered a non-obvious convention, gotcha, or workflow nuance during this run, edit this SKILL.md directly.
2. If the lesson is project-specific (members/ build flag, Admin_system/ SQL pattern, etc.), put it in that project's SKILL.md under `{project}/.claude/skills/*/SKILL.md`.
3. Skip the lessons step for trivial runs (one-line typo fixes, etc.) — only capture what would have saved you time if you'd known it going in.

Examples of lessons worth capturing:
- A port, path, or env var that differs from what this SKILL.md says
- An auth gate or middleware that blocks a workflow with a non-obvious workaround
- A tool that fails consistently in this codebase and what to use instead
- A branch base, package manager, or CI quirk specific to one project

## Important Rules

- NEVER create files outside the target project directory (except temporary `[locale]/dev/` test pages, which MUST be deleted before commit)
- ALWAYS read existing files before creating new ones
- ALWAYS follow the project's SKILL.md conventions
- For `members/`: always handle RTL (Hebrew) and LTR (English), use next-intl NOT i18next
- Don't spawn an agent team for tiny tickets (≤4 file edits, low complexity). The overhead outweighs the benefit — do it inline.
- The PM verification loop is NON-NEGOTIABLE — never skip it
- For UI changes, prefer `preview_inspect` (computed CSS) and `preview_snapshot` (a11y tree) over screenshots — they're faster, more reliable, and give exact values rather than visual approximations
- Save lessons inline (Step 9) — don't rely on agents that don't exist
