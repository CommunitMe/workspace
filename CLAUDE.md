# CommunitMe (Commy) — AI Development Workspace

This is the workspace root for all CommunitMe projects.
Each project lives as a git submodule with its own GitHub remote.
**Always open Claude from this root directory** — never from inside a project.

For human setup instructions see [README.md](README.md).

---

## Projects

| Directory | Stack |
|---|---|
| `members/` | Next.js 16, React 19, Prisma, Tailwind v4, next-intl (Hebrew/RTL) |
| `Admin_system/` | Express 4, TypeScript, raw PostgreSQL, JWT cookies, Jest |
| `Onboarding/` | Vite/React 18 + Express, Passport OAuth, Prisma |
| `community-proj/` | React 18 (JavaScript, NOT TypeScript), CRA, MUI 6, i18next, Formik |
| `notifications/` | Express 5, TypeScript, Prisma 7, Socket.IO 4 |
| `cme_db/` | PostgreSQL schema & migrations |
| `pricelist_tools/` | Pricelist utilities |

---

## Claude Skills Architecture

```
commy/
  .claude/skills/
    implement/              ← Full ticket implementation workflow (cross-project)

  members/.claude/skills/
    members-development/    ← Next.js + RTL + next-intl conventions

  Admin_system/.claude/skills/
    admin-development/      ← Express + raw SQL conventions

  community-proj/.claude/skills/
    community-development/  ← CRA + MUI + JavaScript conventions

  Onboarding/.claude/skills/
    onboarding-development/ ← Vite + Passport + Prisma conventions

  notifications/.claude/skills/
    notif-development/      ← Express 5 + Socket.IO conventions
```

**Rule:** Always read the project's `SKILL.md` before writing any code.

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

## Critical Rules

- **ALWAYS read SKILL.md** before coding in any project
- **ALWAYS check existing files** before creating new ones — never recreate
- `members/` — use `next-intl` (NOT i18next), handle RTL, kebab-case filenames
- `community-proj/` — JavaScript NOT TypeScript
- Tickets can span multiple projects — coordinate API contracts between them
- Commit inside the project repo first, then bump the pointer in the workspace
- Never force-push submodule branches — coordinate with the team
- Run `upgrader` agent after every implementation run to save lessons
