# CommunitMe — Workspace

This repo is the development workspace for all CommunitMe projects.
Each project lives as a git submodule with its own GitHub remote.

Clone everything at once:

```bash
git clone --recurse-submodules https://github.com/CommunitMe/workspace.git commy
```

---

## Projects

| Directory | Description | Stack | Port |
|---|---|---|---|
| `members/` | Community members platform | Next.js 16, React 19, Prisma, Tailwind v4, next-intl (Hebrew/RTL) | 3000 |
| `Admin_system/` | Management API | Express 4, TypeScript, raw PostgreSQL, JWT cookies, Jest | 4000 |
| `Onboarding/` | Onboarding flows | Vite/React 18 + Express, Passport OAuth, Prisma | 5173 |
| `community-proj/` | Community management SPA | React 18 (JS), CRA, MUI 6, i18next, Formik | 3001 |
| `notifications/` | Notification microservice | Express 5, TypeScript, Prisma 7, Socket.IO 4 | — |
| `cme_db/` | Database schema & migrations | PostgreSQL, Docker | — |
| `pricelist_tools/` | Pricelist utilities | — | — |

---

## Local Setup

### Prerequisites

| Tool | Version | Install |
|---|---|---|
| Node.js | 20+ | https://nodejs.org |
| Docker | any | https://docker.com |
| gh CLI | any | `winget install GitHub.cli` |
| Claude Code | latest | `npm install -g @anthropic-ai/claude-code` |

### Step-by-step

**1. Clone the workspace**

```bash
git clone --recurse-submodules https://github.com/CommunitMe/workspace.git commy
cd commy
```

If you already cloned without `--recurse-submodules`:

```bash
git submodule update --init --recursive
```

**2. Install dependencies**

From the workspace root, one command installs everything:

```bash
npm install
```

This installs the root runner (`concurrently`) and then cascades into every project (`members`, `Admin_system`, `community-proj`, `Onboarding`, `notifications/utils`) via the `postinstall` hook.

If you only want one project, you can still install it directly:

```bash
npm run install:members      # or :admin, :community, :onboarding, :notifications
```

**3. Set up environment variables**

Download `.env` files from Google Drive (ask a team member for access):
https://drive.google.com/drive/folders/1hkM4YgEO3ANzmgdsiXzxTboRK8-XTZuV

Place each file in its project directory:

```
members/.env                          (Next.js also accepts .env.local)
Admin_system/.env
Onboarding/packages/client/.env
Onboarding/packages/server/.env
community-proj/.env
```

The `.env` files from Drive point to a shared remote dev database hosted by the team. **You don't need to run a local Postgres** for normal day-to-day development — the apps connect straight to the shared DB over the network.

**4. (Optional) Start a local database**

Skip this step unless you specifically need an isolated local Postgres (heavy migration testing, working offline, etc.). It requires Docker Desktop (`brew install --cask docker`).

```bash
npm run db:up      # docker compose up -d in cme_db/
npm run db:down    # stop it
```

If you do start a local DB, you'll also need to change the `DATABASE_URL` in your `.env` files to point at `localhost:5432`.

**5. (Optional) Run database migrations**

Only needed if you started a local database in step 4. If you're using the shared remote DB, schemas are already migrated.

```bash
cd members    && npx prisma migrate dev && cd ..
cd Onboarding && npx prisma migrate dev && cd ..
```

**6. Start dev servers**

Run everything in one terminal (color-coded output, Ctrl-C kills all):

```bash
npm run dev
```

| Project | Port |
|---|---|
| members | http://localhost:3100 |
| Admin_system | http://localhost:4000 |
| community-proj | http://localhost:3001 |
| Onboarding (server + client) | http://localhost:5173 |

Only need one project? Use a focused script:

```bash
npm run dev:members        # or :admin, :community, :onboarding, :notifications
```

---

## Submodule Workflow

### Daily sync

Run this at the start of a session to get the latest workspace + every submodule on `dev`:

```bash
git pull                                              # workspace itself (skills, CLAUDE.md, etc.)
git submodule foreach 'git checkout dev && git pull'  # each submodule → latest dev
```

Or use the slash command in Claude: `/sync`.

**Caveat:** if you're mid-feature on a branch inside a submodule (e.g. `claude/COMY1-xxx-foo`), don't run the `foreach` blindly — it will switch you off your feature branch back to `dev`. Either skip that submodule, or merge `dev` into your feature branch manually:

```bash
cd <submodule>
git pull origin dev   # merge latest dev into your feature branch
```

**Avoid** `git submodule update --remote --merge` — it forces a merge of each submodule's tracked branch into wherever it currently sits, which often produces conflicts when feature branches are checked out.

### After committing inside a project

```bash
# Inside the project repo:
git add <files> && git commit -m "feat: ..." && git push

# Back in workspace root — bump the submodule pointer:
git add members    # whichever project changed
git commit -m "chore: update members submodule"
git push
```

### Add a new project

```bash
git submodule add https://github.com/CommunitMe/new-project.git new-project
git commit -m "chore: add new-project submodule"
```

---

## AI Development (Claude)

Always open Claude from the workspace root (`commy/`), not from inside a project.
This gives Claude context across all projects at once.

```bash
cd commy && claude
```

Shared skills live in `.claude/skills/`. Project-specific skills live inside each project's `.claude/skills/` directory. See `CLAUDE.md` for the full AI workflow.
