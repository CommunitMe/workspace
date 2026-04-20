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

The project directory is at: `C:/Users/97254/Documents/commy/{project}/`

**For multi-project tickets:**
- Read each project's SKILL.md separately
- Plan tasks per project (e.g. "API endpoint in Admin_system" + "UI component in members")
- Spawn agents with the correct project directory for each task
- Coordinate contracts between projects (e.g. API response shape must match frontend types)
- Create separate git branches and PRs per project that has changes

## Step 3: Load Memory & Learn from Past Runs

Before planning, read the lessons file:
- `C:/Users/97254/Documents/commy/.claude/memory/implement_lessons.md`

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
and one for translations. Each should work in C:/Users/97254/Documents/commy/members/"

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

## Step 7: Build & Test

Run the project's build and tests:
- `members/`: `cd {project} && npx tsc --noEmit` (type check)
- `Admin_system/`: `cd {project} && npx tsc`
- `notifications/`: `cd {project} && npx tsc`
- `community-proj/`: `cd {project} && npx tsc`
- `Onboarding/`: `cd {project} && npx tsc`

If build fails, fix the errors (spawn fix agent if needed), then re-verify (back to Step 6).

## Step 7b: Browser QA (for frontend tasks)

If the ticket involves UI changes (components, pages, styling), do a visual check:

1. **Start the dev server** using Claude Preview (`preview_start`) or directly:
   - `members/`: `npm run dev` (port 3000 or 3100)
   - `community-proj/`: `npm start` (port 3000)
   - `Onboarding/`: `npm run dev` (port 5173)

2. **Navigate to the relevant page** and take screenshots:
   - Test in Hebrew (`/he`) AND English (`/en`) for members project
   - Check RTL layout looks correct in Hebrew
   - Check the feature renders without errors

3. **Check browser console** for errors:
   - No React errors or warnings
   - No 404s for missing assets
   - No uncaught exceptions

4. **Verify the feature works:**
   - Does it match the Figma design (if provided)?
   - Do buttons/links work?
   - Does the loading state show correctly?
   - Does the empty state show correctly?

5. If issues found → spawn fix agent → re-check (back to Step 6)

6. **Stop the dev server** when done.

## Step 8: Create PR

After PM verification passes:
1. Create a git branch: `claude/{TICKET-KEY}-{short-description}`
2. Stage changes: `git add` the specific files (NOT `git add .`)
3. Commit with message: `[TICKET-KEY] description`
4. Push: `git push -u origin {branch}`
5. Create PR: `gh pr create --title "[TICKET-KEY] description" --body "summary"`
6. Transition Jira ticket to "In Review" using `mcp__atlassian__transitionJiraIssue`

## Step 9: Learn & Memorize (Upgrader Agent)

After EVERY run (success or failure), spawn the upgrader agent:

```
Agent(
  subagent_type="upgrader",
  description="Upgrade lessons and skills from this run",
  prompt="Analyze the completed run for [TICKET-KEY] in [project].
    Read .hivemind/HANDOFF_*.md and PM_REVIEW.md.
    Update implement_lessons.md with what worked, what failed, and lessons.
    Update SKILL.md if you discovered new conventions."
)
```

The upgrader will:
- Read all handoff files from this run
- Update `C:/Users/97254/Documents/commy/.claude/memory/implement_lessons.md`
- Update the project's SKILL.md if new conventions were discovered
- Make the next run smarter

## OMCC Integration

When available, use OMCC execution modes for better performance:
- **`ralph:`** prefix for persistent verify/fix loops (recommended for most tickets)
- **`ultrapilot:`** prefix for maximum parallelism on large tickets
- Use `commy-dev` agent for all implementation tasks (knows all 5 projects)
- Use `upgrader` agent for post-run learning

## Important Rules

- NEVER create files outside the target project directory
- ALWAYS read existing files before creating new ones
- ALWAYS follow the project's SKILL.md conventions
- For `members/`: always handle RTL (Hebrew) and LTR (English), use next-intl NOT i18next
- Keep agents focused — one agent per logical unit of work
- The PM verification loop is NON-NEGOTIABLE — never skip it
- Always save lessons, even from failed runs — failure is the best teacher
- Always run the upgrader agent at the end — learning is mandatory
