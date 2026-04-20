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

```bash
cd members        && npm install && cd ..
cd Admin_system   && npm install && cd ..
cd community-proj && npm install && cd ..
cd Onboarding     && npm install && cd ..
```

**3. Set up environment variables**

Download `.env` files from Google Drive (ask a team member for access):
https://drive.google.com/drive/folders/1hkM4YgEO3ANzmgdsiXzxTboRK8-XTZuV

Place each file in its project directory:

```
members/.env.local
Admin_system/.env
Onboarding/.env
community-proj/.env
```

**4. Start the database**

```bash
cd cme_db && docker-compose up -d && cd ..
```

**5. Run database migrations**

```bash
cd members    && npx prisma migrate dev && cd ..
cd Onboarding && npx prisma migrate dev && cd ..
```

**6. Start dev servers**

Open a terminal per project:

```bash
cd members        && npm run dev    # → http://localhost:3000
cd Admin_system   && npm run dev    # → http://localhost:4000
cd community-proj && npm start      # → http://localhost:3001
cd Onboarding     && npm run dev    # → http://localhost:5173
```

---

## Submodule Workflow

### Daily sync

```bash
git pull
git submodule update --remote --merge
```

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
