# CommunitMe (Commy) — AI Development Workspace

This is the **workspace root** for all CommunitMe projects.
Each project lives as a git submodule with its own GitHub remote.
Opening Claude from this directory gives it context across all projects.

---

## Quick Start — New Developer Setup

```bash
# 1. Clone workspace (gets all projects in one shot)
git clone --recurse-submodules https://github.com/CommunitMe/workspace.git commy
cd commy

# 2. If you already cloned without --recurse-submodules:
git submodule update --init --recursive

# 3. Install dependencies for each project you'll work on:
cd members        && npm install && cd ..
cd Admin_system   && npm install && cd ..
cd community-proj && npm install && cd ..
cd Onboarding     && npm install && cd ..

# 4. Set up environment variables (copy the example files):
cp members/.env.example        members/.env.local
cp Admin_system/.env.example   Admin_system/.env
cp Onboarding/.env.example     Onboarding/.env

# 5. Start the database (PostgreSQL via Docker):
cd cme_db && docker-compose up -d && cd ..

# 6. Run Prisma migrations:
cd members      && npx prisma migrate dev && cd ..
cd Onboarding   && npx prisma migrate dev && cd ..

# 7. Start dev servers:
cd members        && npm run dev   # → http://localhost:3000
cd Admin_system   && npm run dev   # → http://localhost:4000
cd community-proj && npm start     # → http://localhost:3001
cd Onboarding     && npm run dev   # → http://localhost:5173
```

### Prerequisites

| Tool | Version | Install |
|---|---|---|
| Node.js | 20+ | https://nodejs.org |
| Docker | any | https://docker.com |
| gh CLI | any | `winget install GitHub.cli` |
| Claude Code | latest | `npm install -g @anthropic-ai/claude-code` |

---

## Projects

| Directory | Description | Stack |
|---|---|---|
| `members/` | Community members platform | Next.js 16, React 19, Prisma, Tailwind v4, next-intl (Hebrew/RTL) |
| `Admin_system/` | Management API | Express 4, TypeScript, raw PostgreSQL, JWT cookies, Jest |
| `Onboarding/` | Onboarding flows | Vite/React 18 + Express, Passport OAuth, Prisma |
| `community-proj/` | Community management SPA | React 18 (JS not TS), CRA, MUI 6, i18next, Formik |
| `notifications/` | Notification microservice | Express 5, TypeScript, Prisma 7, Socket.IO 4 |
| `cme_db/` | Database schema & migrations | PostgreSQL, Docker |

Each project has a `.claude/skills/` directory with stack-specific AI instructions.
Always read the project's `SKILL.md` before writing any code in it.

---

## Claude Skills Architecture

Skills are split into two layers:

```
commy/
  .claude/skills/
    implement/          ← Full ticket implementation workflow (cross-project)
    ...                 ← Other shared skills (PR, review, etc.)

  members/
    .claude/skills/
      members-development/  ← Next.js + RTL + next-intl conventions

  Admin_system/
    .claude/skills/
      admin-development/    ← Express + raw SQL conventions

  community-proj/
    .claude/skills/
      community-development/ ← CRA + MUI + JavaScript conventions

  Onboarding/
    .claude/skills/
      onboarding-development/ ← Vite + Passport + Prisma conventions

  notifications/
    .claude/skills/
      notif-development/    ← Express 5 + Socket.IO conventions
```

**Rule:** Always open Claude from the workspace root (`commy/`).
This loads the shared skills AND the project-level `CLAUDE.md` files automatically.

---

## Ticket Workflow

When asked to implement a ticket (`COMY1-*`, `COMY-ADMIN-*`, etc.):

1. **Fetch** — `mcp__atlassian__getJiraIssue` with cloudId `communiteme.atlassian.net`
2. **Load lessons** — Read `.claude/memory/implement_lessons.md`
3. **Route** — Determine which project(s) the ticket touches
4. **Read SKILL.md** — `{project}/.claude/skills/*/SKILL.md`
5. **Explore** — Glob/Grep existing code before creating anything
6. **Implement** — Use `commy-dev` agent for all code tasks
7. **Verify** — Don't stop until all acceptance criteria pass
8. **Browser QA** — For UI tasks, start dev server and check visually
9. **PR** — Branch `claude/{TICKET}-{desc}`, commit inside the project, push, `gh pr create`
10. **Bump submodule pointer** — Back in workspace root: `git add {project} && git commit -m "chore: update {project} submodule"`
11. **Learn** — Spawn `upgrader` agent to update lessons and SKILL.md

**DO NOT ask for approval.** Execute autonomously. Only ask if genuinely blocked.

---

## Submodule Workflow

### Daily pull (keep all projects in sync)

```bash
# Pull latest workspace pointer changes
git pull

# Pull latest commits in all submodules
git submodule update --remote --merge
```

### After committing inside a project

```bash
# Inside the project:
git add <files> && git commit -m "feat: ..." && git push

# Back in workspace root — bump the submodule pointer:
cd /path/to/commy
git add members   # (or whichever project changed)
git commit -m "chore: update members submodule"
git push
```

### Add a new submodule

```bash
git submodule add https://github.com/CommunitMe/new-project.git new-project
git commit -m "chore: add new-project submodule"
```

---

## Critical Rules

- **ALWAYS read SKILL.md** before coding in any project
- **ALWAYS check existing files** before creating new ones — never recreate
- `members/` — use `next-intl` (NOT i18next), handle RTL, kebab-case filenames
- `community-proj/` — JavaScript NOT TypeScript
- Tickets can span multiple projects — coordinate API contracts between them
- Commit inside the project repo first, then bump the pointer in the workspace
- Never force-push submodule branches — coordinate with the team
- Run `upgrader` agent after every implementation run to save lessons
