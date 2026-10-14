---
name: delete-user-sync
description: "Keep the delete-user (delete account) API in sync with the database. Use this skill WHENEVER a change adds or alters a table, column, or relation that holds data about a user — a new table with profile_id / supplier_id / created_by / *_profile_id, a new FK to profiles or suppliers, a new personal-data column on profiles or suppliers (name, phone, email, address, image, description, card/token…), a new per-user log, cache or saved-card table, or a new lookup status the deletion relies on. Also use it when touching cme_db migrations, the Prisma schema, or either delete-user implementation. Even if the ticket never mentions account deletion, if it stores something about a user, run this skill before calling the work done."
---

# delete-user-sync — keep account deletion complete

Account deletion must remove or anonymize **everything** a user leaves behind. Every new
table or column that stores user data silently breaks that promise unless the delete flow
is updated in the same change. This skill makes that update part of the work.

## Where the delete-user API lives (update BOTH — they must stay identical in behavior)

| Caller | Implementation |
|---|---|
| Admins app (`community-proj`, endpoint `deleteAccount: "profile"`) → `DELETE /api/profile` | `Admin_system/src/services/userProfileService.ts` → `deleteUserAccount()` (raw SQL, one transaction) |
| Members app (`DeleteAccountDialog`) | `members/server/actions/profile/delete-user-account.action.ts` → `deleteUserAccountAction` (Prisma `$transaction([...])`) |

The open-orders guard runs first in both (`getUserOpenOrders` in Admin_system,
`assertNoOpenOrders` in members). If a change adds a new order type or a new "open"
status, update those guards too.

`community-proj` only calls the Admin_system endpoint — it never needs a change unless the
response/error contract changes (409 = open orders, 404 = profile not found).

## When this skill applies

Run it if the change does **any** of:
- adds a table with a column pointing at a user: `profile_id`, `supplier_id`, `creator_id`,
  `created_by`, `*_profile_id`, `recipient_profile_id`, `member_profile_id`, … (with or
  without a formal FK — several existing ones have none)
- adds a column to `profiles` or `suppliers`
- adds stored files, saved cards/tokens, logs, usage counters, or caches keyed by a user
- renames/drops a table or column the delete flow already touches (the delete will crash)
- adds a lookup status the delete flow writes (e.g. `NOT_ACTIVE`, `DELETED`, `PENDING`)

## Step 1 — Audit what references a user (dev DB)

Run against the dev schema (Admin_system `.env` has the connection; `search_path` = `DB_SCHEMA`):

```sql
-- Every FK into profiles / suppliers, with its ON DELETE action (a=no action, c=cascade)
SELECT cl.relname AS tbl, a.attname AS col, ft.relname AS ref, con.confdeltype AS ondel
FROM pg_constraint con
JOIN pg_class cl ON cl.oid = con.conrelid
JOIN pg_class ft ON ft.oid = con.confrelid
JOIN pg_namespace n ON n.oid = cl.relnamespace
JOIN pg_attribute a ON a.attrelid = con.conrelid AND a.attnum = ANY(con.conkey)
WHERE con.contype = 'f' AND n.nspname = current_schema()
  AND ft.relname IN ('profiles', 'suppliers')
ORDER BY ref, tbl;

-- User-ish columns WITHOUT an FK (easy to miss)
SELECT c.table_name, c.column_name
FROM information_schema.columns c
JOIN information_schema.tables t USING (table_schema, table_name)
WHERE t.table_type = 'BASE TABLE' AND c.table_schema = current_schema()
  AND c.column_name ~ '(profile|supplier_id|creator_id|created_by|owner|phone|email)'
  AND c.table_name !~ '_20[0-9]{2}$';   -- skip partitions
```

Diff the result against the statements in both implementations. Every table/column in the
audit must be either handled or listed under "Deliberately kept" below.

## Step 2 — Decide: delete, anonymize, or keep

| Kind of data | Action | Examples |
|---|---|---|
| Personal data / credentials / per-user logs | **DELETE** rows | `banks_accounts`, `member_saved_cards`, `grow_saved_cards`, `feedbacks`, `community_ai_traces`, `community_ai_usage`, `onboarding_profiles` (matched by email/phone) |
| Supplier configuration (pricing, discounts, fulfilment, districts, files) | **DELETE** rows | `supplier_*_config`, `*_discounts_*`, `files_suppliers` |
| Rows other records still reference (orders point at them) | **ANONYMIZE / deactivate**, never delete | `profiles`, `suppliers` (status `DELETED`), `products` / `restaurants` (status `NOT_ACTIVE`) |
| Order & payment records | **KEEP**, but strip secrets | `grow_payment_authorizations` (null `card_token`, `card_token_iv`) |
| Financial ledger / revenue shares | **KEEP** untouched | `transactions`, `user_balance`, `rs_*` |

New column on `profiles` or `suppliers` holding anything a person typed or that identifies
them → add it to the anonymizing `UPDATE` in **both** files (set `NULL`, or a placeholder if
the column is `NOT NULL`).

Ordering rules:
- Children before parents (`invitations_links` before `invitations`).
- Anything matched by the user's email/phone runs **before** the final `UPDATE profiles`
  wipes them.
- Collect MinIO file keys **before** the transaction, remove objects only **after** commit.
- Tables already `ON DELETE CASCADE` from something we delete (e.g. `notifications_views`
  from `notifications_recipients`) don't need their own statement — note it in a comment.

Resolve lookup ids by name (`(SELECT id FROM … WHERE status = '…')` in SQL, the generated
`*_IDS` constants in members) — never a numeric literal.

If a table is not in the members Prisma schema yet, **add the model — don't fall back to
raw SQL.** The schema is hand-maintained, so never run a plain `prisma db pull` (it wipes
relations that have no detectable FK). Instead: `npm run prisma:pull` in `members/` (it
introspects into `prisma/_pulled.scratch.prisma`), copy only the new `model` block(s) into
`prisma/schema.prisma`, add the matching back-relation fields on `profiles` / `suppliers` /
`communities`, delete the scratch file, then `npx prisma format && npx prisma validate &&
npx prisma generate`. Check `git diff -w prisma/schema.prisma` shows only additions.

### Deliberately kept (do not "fix" these)
`transactions`, `user_balance`, `rs_deal_revenue_shares`, `rs_*_share_totals`,
`rs_deal_fee_calculations`, `rs_supplier_deal_counters`, order tables
(`service_orders`, `products_orders`, `restaurants_orders`, approved `service_order_offers`),
`grow_payment_authorizations` rows, `supplier_subscriptions` / `supplier_subscription_history`
/ `supplier_scores` (rev-share inputs, no personal data), `grow_kyc_*` (payment-provider
merchant record), `community_ai_announcements` / `community_ai_answers` (community content).
If you think one of these should change, raise it with the user instead of changing it.

## Step 3 — Verify against the dev DB without persisting anything

Never run a real delete on dev data. Prove every statement executes, then roll back:

- **Admin_system**: a throw-away ts-node script that wraps `pool.connect()` so the client's
  `query('COMMIT')` becomes `ROLLBACK`, zeroes `order_count` rows so the open-orders guard
  passes, and stubs `FileService.prototype.removeStoredObject`. Call
  `deleteUserAccount(<supplier profile id>)` and check each statement logs OK.
- **members**: a throw-away vitest file that mocks `createAction` (return the handler),
  `assertNoOpenOrders` and `fileService`, and wraps `prisma.$transaction` to append
  ``prisma.$executeRaw`SELECT 1/0` `` — the batch must fail with *division by zero* (meaning
  every real statement succeeded) and the profile must be unchanged afterwards.
- Pick a profile that is a supplier **and** a member and has rows in the new table.
- Delete both harness files before committing; `npx tsc --noEmit` in both projects.

## Step 4 — Report

In the PR / summary, list which new tables/columns were added to the delete flow, which
were classified as "keep" and why, and the dry-run result.
