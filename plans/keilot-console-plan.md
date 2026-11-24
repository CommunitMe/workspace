# Keilot Console — replacing KA (Appsmith) with one flexible admin app

Status (2026-11-24): **P0 built, not deployed.**
- Repo `CommunitMe/console` (submodule `console/`).
- PRs: cme_db#121, Admin_system#424, members#369.
- Migration applied on DEV; first owner = profile 10001 (dev).
- Changes from the original plan, based on what already exists:
  - Sign-in reuses the normal Keilot login cookie (Onboarding Google/phone) — no separate OAuth client.
  - Writes go through `Admin_system /api/staff` with the staff member's own cookie, not a service key.
- Still open for P0: dev deploy (host/domain, CI, `CONSOLE_DB_URL` secret). "Return to Console after login" = Onboarding#203 (needs the `VITE_CONSOLE_URL` variable per env).

Status (2026-11-24, later): **P1 built, verified on dev, not merged.** console#1 + Admin_system#425 (stacked on #424).
Communities, Suppliers, Feedback, Rev-Share (+ read-only config) pages; actions: delete account (owner),
reset view indications (support), remove supplier from a community (ops). Left for later: editing Rev-Share config.

## 1. What we have today (findings)

**KA = Appsmith v1.94 at ka-staging.keilot.com.** 9 apps in one workspace:
- `Keilot Admin - Dev / - Staging / - Prod`: three hand-maintained copies of the same app, one per environment.
- `Keilot AI Agents - Dev / - Prod`.
- `Keilot Catalog Review - Dev`, plus `(1)`, `(2)` and `(3)` duplicates.

**The main admin is one page with an "Admin Options" dropdown** of about 15 actions:
- Search user by name / phone
- Delete profile view indications, delete supplier role, delete profile
- 3 feedback lists: supplier missing pricelist, missing service, leaving community
- Communities stats, community data, suppliers stats, supplier data
- RevShare views (supplier / community / general) + RevShare config

**Why it hurts:**
1. **Rigid UI.** It is a drag-and-drop DSL. Every new screen is manual widget work, and AI agents can't edit it well. We generate an import JSON and re-import it through Chrome, which wipes other sessions' tabs.
2. **No git.** The apps are not git-connected and the server can't reach GitHub. Nothing gets PR review or rollback.
3. **3–4 copies per app.** The schema name (`"1_0_0_DEV"`) is hardcoded in every query, so each environment is a separate app that drifts.
4. **Raw SQL writes straight to the DB.** `delete_profile` is a third, hand-written copy of delete-user logic. It is out of sync with the real implementations (`Admin_system/src/services/userProfileService.ts`, `members/.../delete-user-account.handler.ts`). It hard-deletes rows instead of anonymizing, and on Dev one subquery reads `"1_0_0_STAGING".suppliers` (a bug).
5. **No staff identity, no RBAC, no audit log.** `handled_by` / `reviewed_by` store an Appsmith email at best.
6. **Read-mostly.** It can't run real operations, such as adding a pricelist, messaging a user or refunding, because those need business logic that lives in `Admin_system`, not in SQL.

**What exists that we can reuse:**
- `Admin_system` already has the services for pricelists (`/api/pricing`), products, orders/jobs, hatavot, community AI, notifications and delete-account. It is the community-manager/supplier backend, so these are real, tested business operations.
- `notifications/utils` has a service-to-service API protected by `INTERNAL_SERVICE_KEY`.
- The admin-agent infrastructure (AI SDK, tools, Confirm cards, traces, cost dashboard) is already built.
- Roles table has `0 KEILOT_ADMIN`, but nothing enforces it. Community type `KEILOT_ADMINS` exists.
- Feature flags (`features`, `community_features`, `profile_features`) and the AI views and settings tables.

**Gaps:**
- No staff concept.
- No refunds table: failed releases only log "MANUAL REFUND NEEDED".
- No audit log.
- No way to message a user from the back-office.

## 2. Options considered (web research, Nov 2026)

| Option | Verdict |
|---|---|
| Upgrade Appsmith + Git sync | Fixes versioning, not rigidity. Apps are still DSL JSON, which AI edits poorly. Audit log and granular RBAC need Business ($15/user/mo). |
| Retool / ToolJet / Budibase | Same drag-drop problem. Retool self-host is Enterprise-only. ToolJet Git is $199/builder/mo. |
| Directus / NocoDB on top of Postgres | Good DB browsers, but a second permission system and none of our business logic. Licenses changed in 2026. |
| AI-native builders (Superblocks Clark, Retool AppGen) | Paid and vendor-locked. Claude Code on a plain repo already gives us this. |
| react-admin / AdminJS | react-admin's RBAC and audit are paid EE and it is MUI-based. AdminJS looks stale. |
| **Code-first Next.js admin (shadcn/ui + TanStack Table, optionally Refine core)** | **Recommended.** It lives in git with PRs and CI, and Claude Code builds a new screen in minutes. Same stack as `members/`. shadcn got first-class RTL in Jan 2026. No per-seat cost. |

## 3. Recommendation: "Keilot Console"

A new Next.js 16 app (its own repo and submodule, `console/`). Its job is to be the one place where Keilot staff see and operate everything.

### Architecture
```
Staff browser ──► Console (Next.js, Google login @keilot.com, one deploy per env)
                    │  reads: read-only DB role → views (fast dashboards, search, SQL lab)
                    │  writes: server-side only ──► Admin_system  /api/staff/*
                    │                                 (INTERNAL_SERVICE_KEY + X-Staff-Id)
                    │                                 RBAC check → existing services → staff_audit_log
                    └─ Ops Agent chat ──► the same /api/staff tools (UI parity)
```
- **Writes only through `Admin_system /api/staff/*`.** It reuses the real pricing, delete-user, orders and notification services, so the console can never drift from the product the way the Appsmith SQL did.
- **Reads straight from Postgres with a read-only DB user**, through views. Dashboards and new list screens stay cheap to build; this is where the flexibility comes from.
- **One codebase, deployed to dev / staging / prod** like the other apps. The schema comes from `DB_SCHEMA`, so there are no copies. Add an environment badge and a red banner on prod.
- **Staff identity:** new `staff_members` table (email, name, role: `owner` | `ops` | `support` | `viewer`, active). Google sign-in restricted to the company domain, plus 2FA.
- **Audit:** new append-only `staff_audit_log` table (staff_id, action, entity, entity_id, reason, before/after JSON, request_id, via `ui` | `agent`). It is written in the same transaction as the write. Rows reference target profiles, so they go into the delete-user flow (delete-user-sync skill). Anonymize the target rather than dropping audit history.
- **Danger rules:** typed confirmation for deletes; a dry-run count for bulk actions; a required reason on every write; later, a 2-person approval for refunds above a threshold.

### Modules (what admins can do)
1. **Users 360.** Search by name, phone or email. A profile card with communities and roles, orders, feedback, agent conversations, features and terms.
   - Actions: delete user through the real API, remove supplier role, toggle feature flags, resend invite.
   - Later: read-only "view as user".
2. **Communities.** List and stats, type, settings, managers, features, RevShare config.
3. **Suppliers & pricelists.** Supplier data and stats, subscriptions, KYC status.
   - **Add or edit a pricelist on the supplier's behalf**, using the existing `/api/pricing` services, including from a photo or Excel file through the existing menu-photo reader.
4. **Catalog.** The AI catalog review queue (port), category tree, merges.
5. **Orders & money.** Orders with status history and RevShare views.
   - A **refunds queue**: a new `refund_requests` table instead of the "MANUAL REFUND NEEDED" log line, plus a "mark refunded" action.
6. **Inbox.** One queue for unanswered agent questions, help requests and feedback (missing pricelist, missing service, supplier leaving).
   - **Reply to the user** from here, then mark handled. Answered items can feed the agent's knowledge.
7. **Messaging.** Send a message to one user or a segment (a community's suppliers, members without orders, and so on) by push, in-app notification or email (Brevo).
   - Preview → recipient count → confirm. Every send is logged.
8. **AI Agents.** Port the usage and cost dashboard, model prices, budget alerts, kill switches, per-community agent flags.
9. **System.** Feature flags, terms versions, staff management, audit log viewer.
10. **SQL lab / saved views** (flexibility). Owners write read-only SQL (or ask the agent to), see a table or chart, and save it as a pinned widget on the home page. New questions no longer need a code change.

### Ops Agent (yes, worth it — in phase 4)
A chat panel in the console, built on the same agent infrastructure as the member and admin agents.
- **Tools = the same `/api/staff` endpoints the UI uses** (our UI-parity rule).
  - Reads run immediately.
  - Writes show a Confirm card with the exact effect and are audited with `via = agent` and the staff member who confirmed.
- **Examples:**
  - "Which suppliers in community 12 have no pricelist? Message them a reminder."
  - "Build a pricelist for supplier X from this photo."
  - "Draft replies to today's unanswered questions."
  - "Why did order 4512 fail?"
  - "Show agent cost per community this month as a chart" (writes SQL lab queries and can pin them).
- **Guardrails:** no free-form SQL writes; bulk sends capped and confirmed; every turn traced into the existing AI cost views.

## 4. Phases

| Phase | Scope | Retires | Rough size |
|---|---|---|---|
| **P0 Foundations** | Repo and submodule, Next.js + shadcn RTL (Hebrew/English), Google login, `staff_members` + `staff_audit_log` migrations, `Admin_system /api/staff` skeleton with service key + RBAC middleware + audit helper, read-only DB user, dev deploy. Read-only Users 360. | — | ~1 week |
| **P1 Replace KA main page** | User search and delete (via real API), supplier role removal, feedback lists, community and supplier stats and data, RevShare views and config. Staging + prod deploy. | `Keilot Admin - Dev/Staging/Prod` | ~1–1.5 weeks |
| **P2 Port the AI pages** | AI Agents dashboard (all tabs incl. budget and unanswered questions), Catalog review, flags and kill switches. | Appsmith entirely; shut down ka-staging | ~1 week |
| **P3 Operations** | Pricelist on behalf (incl. photo/Excel), Inbox + reply, Messaging (single + segment), refunds queue, SQL lab. | Manual scripts and DB edits | ~2–3 weeks |
| **P4 Ops Agent** | Chat panel, tools over `/api/staff`, Confirm cards, eval set like the other agents. | — | ~1–2 weeks |
| Later | View-as-user, 2-person approvals, scheduled reports, read replica if prod load grows. | | |

Every phase follows the normal ticket workflow: worktree, PR, tests, browser QA. Run Appsmith and the Console side by side until each screen is verified, then delete the Appsmith app.

## 5. Decisions needed from you
1. **Who logs in:** only Keilot staff (Google @keilot.com), or also outside helpers (then phone OTP via Onboarding)?
2. **Roles:** is `owner / ops / support / viewer` enough? Who should be able to delete users or send bulk messages?
3. **Domain:** e.g. `console.keilot.com` (+ `console-dev`, `console-staging`), IP-restricted or open with 2FA?
4. **Messaging channels in P3:** push + in-app + email only, or WhatsApp/SMS too (needs a provider)?
5. **Start order:** P0→P1 as above, or Operations (pricelist + messaging) first because that's where the value is?
