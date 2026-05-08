# Keilot / CommunitMe Repo Map

Last scanned: 2026-05-04
Atlassian site: `communiteme.atlassian.net`

This is a cached snapshot of the workspace, designed to fit comfortably in context. The workspace is a git-submodule monorepo on GitHub; each user clones it locally to a path of their choosing. Use this map to evaluate scope quickly. When the map isn't enough, grep the relevant repo in the user's local clone — don't make scope decisions blind.

## Big picture

CommunitMe is a multi-tenant community platform with three primary user types:

- **Members** — end users of a community (consumers).
- **Suppliers** — vendors who sell into a community.
- **Community managers / admins** — operators who run a community day-to-day.

The platform is split into separate apps per user type, plus shared services. Many features touch more than one app — a change "for community managers" often ripples into the supplier flow, the member-facing UI, and notifications.

Hosted environments (from Onboarding's config):
- `https://platform-staging.keilot.com` — members platform
- `https://admin-staging.keilot.com/` — admin system

## Repos

### `members/` — member-facing platform
- **Stack:** Next.js 16, React 19, TypeScript, Prisma, Tailwind v4, next-intl
- **Port:** 3000 (3100 in some configs)
- **Jira prefix:** `COMY1-*`
- **Languages:** Hebrew (RTL primary), English, plus other locales
- **What it owns:** everything an end-member sees and does — browsing the community, placing orders, managing their account, member-side feedback. The "consumer app".
- **Conventions a PM should know:**
  - Uses `next-intl` (NOT i18next). Translation changes here are different from `community-proj` or `Onboarding`.
  - RTL-first; Hebrew is gender-inclusive copy. Any UI change has design implications for RTL.
  - Kebab-case filenames.
  - Has its own development skill (`commy-development`) — devs follow strict Figma-to-code conventions.
- **Touches:** `Admin_system` (server APIs), `notifications/` (push/email/in-app), `cme_db` (schema).

### `community-proj/` — community manager app
- **Stack:** React 18 (JavaScript, NOT TypeScript), Create React App, MUI 6, i18next, Formik
- **Port:** 3001
- **What it owns:** the operator-facing app. Community managers use this for day-to-day operations: moderation, supplier oversight, member oversight, community settings, payouts.
- **Conventions a PM should know:**
  - Plain JavaScript, not TypeScript — different style than `members/` or `Admin_system/`.
  - Uses `i18next` (different from members' `next-intl`).
  - MUI 6 components; visual style differs from members.
- **Touches:** `Admin_system` (server APIs), often `notifications/`, sometimes `cme_db` (schema for new fields).

### `Admin_system/` — backend admin/API service
- **Stack:** Express 4, TypeScript, raw PostgreSQL via `pg` (no ORM), JWT-cookie auth, Jest
- **Port:** 4000
- **Jira prefix:** `COMY-ADMIN-*`
- **What it owns:** the central server-side API for admin/community-manager operations. Auth (JWT in httpOnly cookies), admin endpoints, raw SQL queries against the shared `cme_db` schema.
- **Conventions a PM should know:**
  - Raw SQL — no Prisma here. Schema changes in `cme_db` need matching query updates.
  - Most cross-app features that need a server endpoint go through here.
- **Touches:** `cme_db` (schema), and is consumed by `community-proj` and indirectly by `members`.

### `Onboarding/` — sign-up & onboarding flow
- **Stack:** Vite/React 18 + Express, TypeScript, Passport.js (Google/Facebook OAuth), Prisma, npm workspaces monorepo (client / server / shared)
- **Ports:** client 5173, server 3000
- **What it owns:** the entire pre-platform experience. Sign-up, OAuth, OTP verification, password reset, invite handling, supplier onboarding form (3-step), community creation, role selection, lobby. After onboarding finishes, redirects users to either the admin system (suppliers) or members platform (members).
- **Conventions a PM should know:**
  - Multi-tenant via DB schema isolation (`DB_SCHEMA` env var).
  - Invite flow is intricate: invite link → validation → account → invite-complete → redirect. Touching invites usually touches all three Onboarding packages (client, server, shared).
  - Has shared Zod schemas for auth — changes to login/register/OTP/password-reset validation should go here so client and server stay in sync.
  - Languages: en, he (RTL), ru.
- **Touches:** `Admin_system`, `members/` (via post-onboarding redirect), email providers (SendGrid/Brevo), MinIO (file storage).

### `notifications/` — notifications service
- **Stack:** Express 5, TypeScript, Prisma 7, Socket.IO 4
- **What it owns:** notification routing and delivery — push, in-app (real-time via Socket.IO), email. Tracks delivery state.
- **Conventions a PM should know:**
  - Any feature that sends a user a message (alert, confirmation, reminder, status update) likely touches this service.
  - Real-time channels mean this isn't just send-and-forget — UI consumers subscribe to socket events.
- **Touches:** consumed by `members/`, `community-proj/`, `Admin_system/`. Reads from `cme_db`.

### `cme_db/` — shared PostgreSQL schema
- **Stack:** PostgreSQL schema, Docker Compose for local dev, ~125+ table definitions, functions, scripts
- **What it owns:** the source-of-truth database schema for the whole platform. Tables cover communities, community settings, members, suppliers, orders, feedback, files, banks, languages, currencies, districts, categories, and many more.
- **Conventions a PM should know:**
  - Any change to data model starts here. After this, every consumer with a Prisma client (`members`, `Onboarding`, `notifications`) needs `prisma db pull` + migration. `Admin_system` needs raw-SQL updates.
  - Schemas are environment-isolated (e.g. `1_0_0_DEV`).
  - A new field on an existing entity is rarely a one-repo change — almost always touches at least 2-3 services.

### `pricelist_tools/` — pricelist utilities
- **Status:** repo not yet created on GitHub as of 2026-05-04. Skip when scoping until it exists.

## Cross-repo scoping heuristics

Use these as starting hypotheses, then confirm by reading code:

- **"For community managers"** → `community-proj/` (UI) + `Admin_system/` (API). Often `notifications/`. If touching new data, `cme_db/`.
- **"For members"** → `members/` (UI + server actions + Prisma) + likely `notifications/` + likely `cme_db/`.
- **"For suppliers"** → suppliers are managed both during onboarding (`Onboarding/`) and afterwards via the admin/community-manager surfaces (`Admin_system/`, `community-proj/`). Confirm where the supplier-facing surface for *this specific feature* lives.
- **Login / signup / OTP / password reset / invite flow** → `Onboarding/` (all three packages: client, server, shared).
- **Auth / session / JWT changes** → `Admin_system/` and `Onboarding/server`.
- **Data model change (new field, new table, new relation)** → `cme_db/` first, then every service that reads/writes it.
- **New notification (push / in-app / email)** → `notifications/` + the originating app's UI to trigger it.
- **Translation / copy change** → identify the app first; `members/` uses `next-intl`, `community-proj/` uses `i18next`, `Onboarding/` uses its own loader (`/public/langs/data.json`). They are not interchangeable.

## Flagging conventions for the ticket

When you list affected repos in a ticket, use these confidence labels honestly:

- **confirmed** — verified by reading code in the user's local workspace.
- **suspected** — the repo map says it should be involved, but you didn't open the code.
- **needs dev confirmation** — there's a technical question only a developer can answer (will likely require a migration / contract change / etc.). This is for things devs will naturally check during planning, NOT for things you needed to know to write the spec. If you needed to know it, it goes in `⚠ Open questions` instead.
