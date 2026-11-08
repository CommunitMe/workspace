# Review: seller draft items without an image (plan v2)

Reviewer: Fable (independent, read-only). Date: 2026-10-30.
Read: `plans/seller-draft-variants-plan.md` v2, `plans/agent-bulk-actions-plan.md`, the sibling notes, and the code on
`origin/dev` (Admin_system `c42ba05`, community-proj `7416285`, cme_db `d8c2abf`, members `e4e56e5`) plus the diff of
Admin_system PR #381.

## Verdict: **APPROVE WITH CHANGES**

The core design is right:
- "a draft without a photo, never live without one", using the existing DRAFT status;
- a CHECK on the same row as the final guarantee;
- a separate `products_publish`, so `products_set_status` and its Undo stay intact;
- menu-photo reading deferred.

Most gap claims (G1–G8) hold up line by line. But the plan has one real hole in the invariant (drafts can still get
into orders), a weak "image" definition (any non-empty string passes), and a bulk-execution design that conflicts with
the repo's own UI-parity test and with the transaction tooling that actually exists. Fix the items below and then
implement.

---

## Required changes

### R1. Drafts can be added to orders through the job editor. Close it.
**Evidence:**
- community-proj `ViewJobModal/ModalContent/EditOrder/index.js:214-232`: the "Choose Product" list loads
  `products/supplier?page=1&page_size=200`, which returns every item that isn't deleted (`productService.ts:50`). That
  includes paused items today, and drafts after this change. It does no `is_active` filtering.
- Admin_system `jobsService.ts:1811-1813` takes `p.product_id` as given, and `~1920-1935` inserts it into
  `product_order_items`. There is no status check and **no ownership check**.

So §3.6's "a draft can never be ordered" is false: a seller can put a draft with no image on a member's order. That
also makes the "versioning a draft that has orders" path reachable.

**Change to the plan:** add to §3.4 and §3.5:
- (a) `editJob` rejects any `product_id` that isn't `ACTIVE`/`NOT_ACTIVE` **and** owned by `so.supplier_id` (400
  `PRODUCT_NOT_AVAILABLE`), with one query over all ids. This also fixes the cross-supplier id gap, which predates the
  plan.
- (b) The EditOrder picker drops `status === 'draft'` items.
- (c) A jest test for both.

### R2. "Has an image" must mean a real file of this supplier, not any non-empty string.
**Evidence:**
- `catalogHandlers.ts` create (`~340-346`, "Duplicate case") and update (`~455-462`) accept `body.main_image_url`
  verbatim. If the regex doesn't match, they store the raw string.
- The proposed CHECK only tests `btrim(...) <> ''`, so `POST main_image_url=x` creates a **live** item with a broken
  image, bypassing the whole rule.
- The eval seed already does this on purpose: `evals/admin-agent/seed.ts:258-262`, "a stand-in main image … no file
  behind it", `randomUUID()`.

**Change:**
- In the service, before an item is created ACTIVE, published, or edited while live, resolve the UUID against
  `files_suppliers` for **this supplier**. Check the exact column names when implementing.
- Not found → treat it as no image: create → DRAFT; update of a live item → 409 `IMAGE_REQUIRED`.
- Keep the CHECK as the cheap DB backstop. No FK: B10 cleanup and shared duplicate UUIDs make an FK painful.
- Point the admin eval seed at a real seeded file row, or accept that the seed bypasses the service (it writes SQL
  directly, so it's unaffected).

### R3. One error contract and one 23514 mapper, used by every write path.
**Evidence:**
- `AppError` already carries machine-readable `details` (`middlewares/errorHandler.ts:12-15`, precedent
  `BELOW_ORDER_MINIMUM`).
- The plan maps 23514 only in the status path. But `updateProduct` in place (`productService.ts:348-362`), its
  versioning CTE (`:299-336`), create, and the agent's publish can all hit the constraint in a race. Unmapped, each is a
  500.
- The UI reads `error.response.data.message`: `ProductsContent/index.js` delete-409 branch and `ProductModal.js:366-372`.
  The modal shows the raw English server message for any 4xx.

**Change:**
- `isActiveImageViolation(err)`: `err.code === '23514' && /^chk_(products|restaurants)_active_requires_image$/.test(err.constraint)`.
  Don't match on the code alone: other CHECKs exist, such as `chk_product_order_items_available_quantity`.
- Every product write converts a match into
  `new AppError('Main image is required to publish', 409, err, { code: 'IMAGE_REQUIRED' })`. Same pattern for
  `DRAFT_CANNOT_PAUSE`.
- community-proj switches on `data.details?.code` (fall back to `data.message` for old codes) and translates.
- Make the status UPDATE itself atomic, so the CHECK is a backstop rather than the normal path:
  ```sql
  … WHERE id=@id AND supplier_id=@s AND status_id <> 2
    AND (@target <> 0 OR (main_image_url IS NOT NULL AND btrim(main_image_url) <> ''))
  ```
  Then check `rowCount`.

### R4. Bulk create: run per item through the route's handler, not a new one-transaction service.
**Evidence:**
- There is no callback transaction helper. `databaseUtil.ts:184-215` (`executeMultipleQueries`) runs a *static list*
  of queries, so a later query can't use the `RETURNING id` of the insert.
- `createProduct`, `saveProductTags` and `addProductImage` each take their own pool connection.
- So "one `useTransaction`, as in `databaseUtil.ts:186`" really means rewriting the whole create path on a raw
  `PoolClient` (the `productJobsService.ts:941` pattern): a **second create path** that must track
  `createProductHandler` forever (digital-flag rule COMY1-331, tags, gallery, `fulfillment_missing`).
- A tool with no HTTP route is `chat-only`. `tests/agent/unit/uiParity.test.ts` pins
  `chatOnly === ['debug_echo','pricelists_read_photo']` ("only the chat-only tools the requester decided on"), so it
  would need the user's explicit decision. The plan presents it as a free choice.
- The sibling #381 batch tool (`catalog_create_item` handler) deliberately runs items "one by one: each is its own
  transaction … an item someone added meanwhile is reported, not fatal for the rest". The bulk-actions plan (Q5) uses
  the same model.

**Change to §3.4.5 and §3.7.2:**
- Drop `createProductsBatch`. The `products_create_many` handler loops over the prepared items and calls
  `createProductHandler` for each.
- Re-check dedup right before each item. Collect per-item outcomes.
- Receipt: "Added 7 of 8 (2 live, 5 drafts); not added: X — reason".
- `uiParity` becomes `same` (endpoints `POST /products/supplier`, `POST /restaurants/supplier`), with no test change.
- Give `createProduct` an internal `notify?: boolean` (default true). The batch passes false and sends **one** #58
  after the loop if any item went live.
- Validation stays all-or-nothing at Confirm: prepare re-runs, and a fingerprint drift refreshes the card. That's the
  existing `actions.ts:172-186` behaviour.
- If the user still wants all-or-nothing *execution*, it's their call. It needs a `dbUtil.withTransaction(fn)` helper,
  client-taking variants of the tag and image helpers, and a `chat-only` parity entry with their decision recorded.
  Ask them; don't assume.

### R5. The photo must be an explicit ref. In `products_create`, `""` means "no photo, save as draft".
**Evidence:**
- `products.write.tools.ts:86-90`: `""` picks the newest image with no `used_by`.
- `used_by` is written only after Confirm (`actions.ts:206`).
- The plan's fix (skip images that have `analysis`) only covers photos that `pricelists_read_photo` already read.
  These would still be picked:
  - a menu photo the model never read;
  - a photo left from a declined card;
  - a photo meant for the next dish;
  - the same photo by two pending cards.
- With photos now optional, the failure mode of a default is a **wrong photo going live**. The failure mode of an
  explicit ref is a draft, which is safe.

**Change:**
- `main_photo: ""` = no photo (draft). The schema description says to pass the ref shown in
  `[Attached to this message: photo #ref …]`.
- Keep the `""` default only for `business_set_image`. Split `resolvePhotos(…, mainRequired)` into
  `defaultToNewest`.
- Update the s3 scenarios that relied on the default, and the prompt line.

### R6. `products_create_many` needs a `replaceKey` and the one-pending-batch rule.
**Evidence:**
- `toolSet.ts:216-217` and `262-268`: without a `replaceKey`, "actually make the falafel ₪28" leaves the first card
  confirmable next to the second. Confirming both creates duplicates.
- In-batch dedup at Confirm would catch most of them, but not when names were also edited.
- #381 solved the same problem: `replaceKey` plus the `catalogOneCard` refusal in `catalog_create_item.prepare`.

**Change:** `replaceKey: (p) => p.vertical`. A new batch for the same catalog replaces the pending one. A pending
single `products_create` card for the same normalized name counts as a duplicate.

### R7. Fix the migration as written.
**Evidence:**
- cme_db `.claude/skills/db-migrations/SKILL.md` rule 2: the copies differ only in the schema token, and every object
  is schema-qualified. The plan's SQL uses bare `products`.
- The cited precedent `migrations/2026-10-08_order_items_available_quantity.sql` is a **legacy DEV-only** file. The
  skill says files before 2026-10-25 were DEV-only. It's a style precedent, not a three-copy one.
- `ADD … NOT VALID` → `UPDATE` → `VALIDATE` inside one `BEGIN … COMMIT` buys nothing. `ADD CONSTRAINT` already holds
  `ACCESS EXCLUSIVE` until COMMIT, and the tables have about 77 rows.

**Change:**
- `"1_0_0_DEV".products` / `.restaurants` everywhere.
- Order: `DROP CONSTRAINT IF EXISTS` → the demotion `UPDATE` → `ADD CONSTRAINT` (validated).
- For staging and prod, the demotion is a **decision, not a side effect**. Demoting silently takes live items off the
  storefront with no word to the seller. If the read-only count is above 0, stop and list the rows for the user. They
  choose: demote and tell the sellers, or fix the images first.
- Name the constraints exactly as R3's regex expects. Mirror them in `tables/products.sql` and `restaurants.sql`.

### R8. Order the dev rollout around the shared dev DB.
**Evidence:**
- The members fixture (`members/evals/member-agent/fixture.s3.ts:49-57`) copies the seed product's `status_id`
  (ACTIVE) and sets no image, so it breaks the moment the CHECK exists.
- Other sessions run evals on `1_0_0_DEV` and community 43.

**Change to §6:**
1. Merge the members fixture fix first.
2. Post a note in the shared notes folder ("CHECK going on dev at <time>").
3. Then apply the dev migration.

The plan's "apply on dev" as step 1, with members in parallel, breaks member-agent evals in between.

### R9. DELETED is final everywhere, not only in the status route.
**Evidence:**
- `getProductById` (`productService.ts:504-532`) has no status filter. It backs:
  - `getSupplierProductHandler` (GET one), which the agent's `resolveProduct` uses (`products.write.tools.ts:224`);
  - `updateProduct`;
  - `deleteProduct`.
- So `products_update` with a forged own id can edit a superseded or hidden row today.

**Change:** add `excludeDeleted` to `getProductById` and pass it in GET-one, update, delete, status and publish. Add a
jest case: "a DELETED id → 404 on each".

### R10. Bring B3 into scope.
**Evidence:**
- `catalogHandlers.ts:485-493` deletes **all** extra images whenever any new extra file is uploaded. It ignores the
  `extra_image_urls[i]` that `ProductModal.js:323-329` sends for the images being kept.
- With drafts, "edit → add photos" becomes the main way an item gets finished. A draft saved with gallery images and
  no main image, then completed with a main image and one more extra, loses its gallery.

**Change:**
- About 10 lines in `updateProductHandler`: rebuild the gallery from the kept `extra_image_urls` (UUIDs extracted as on
  create) plus the new files, in order.
- Add a test.
- UI: when a draft has gallery images but no main image, offer "use as main photo" next to Publish. The CHECK stays on
  `main_image_url` only (see the answer on the gallery question below).

### R11. Publishing must carry `fulfillment_missing`, like create does.
**Evidence:**
- `catalogHandlers.ts:395-402` returns `fulfillment_missing` on create, so community-proj shows
  `FulfillmentReminderDialog`.
- A seller whose **first live item comes from a Publish** never gets that reminder. Their store stays invisible to
  members with no explanation.

**Change:**
- The status-publish and update-publish responses add `fulfillment_missing`.
- `ProductsContent/index.js` shows the same dialog.
- The `products_publish` and `products_create_many` receipts use the existing `select.fulfillment` (as
  `productCreated` does).

### R12. Fix the coordination section against what #381 really contains.
**Evidence (from `gh pr diff 381`):**
- #381 also edits `src/handlers/catalogHandlers.ts` (adds `searchPricelistHandler`) and `src/services/agent/render.ts`.
  Neither is in the plan's overlap list, and `catalogHandlers.ts` is the core of this change.
- `catalogCreateMany` and `catalogSkipped` are **already pushed** in #381 (`catalogSkill.tools.ts`), not "unpushed".
- `normalize()` lives at `src/services/catalogSkill/text.ts`, outside `agent/`.
- #381 is `CONFLICTING`.
- The prompt rule "Only what the supplier asked, one job / community / product per card. Never several at once" is
  rewritten by **both** this plan and bulk-actions. #381 also rewrites the `products_create` prompt line.

**Change:**
- Don't stack on #381's tip. Wait for it to merge to `dev` and branch from `dev`. If it hasn't merged when work starts,
  copy `normalize()` into `src/services/agent/text.ts` and switch the import later (the plan's fallback, made the
  default).
- Agree the one prompt-rule sentence with bulk-actions **before** coding, in one note in the shared notes folder. One
  owner edits it, and the other rebases. Suggested text:
  > "One card per change, except the batch tools (`products_create_many`, and the `_many` tools), which put every item
  > the supplier named in their own message on one card."

---

## Suggestions (optional)

1. **Dedup by exact `normalize(name)` equality within the catalog.** Don't use `tokens()`/`wordForms()`: they're
   built for fuzzy service matching and would merge "פיצה מרגריטה" with "פיצה מרגריטה משפחתית". Show "already in your
   menu: name (#id)". If the seller says "add it anyway", the next card takes an explicit
   `allow_duplicates: [names]`.
2. **`productsSkipped` must show the reason.** #381's `catalogSkipped` renders `x.name || x.reason`, which drops the
   reason when there is a name. Render "name — reason".
3. **Delete-user:** both implementations set **every** row to NOT_ACTIVE, including DELETED and DRAFT rows:
   - `Admin_system/src/services/userProfileService.ts:627-629`;
   - `members/server/actions/profile/delete-user-account.handler.ts:111-119`.

   That turns hidden order lines and superseded versions into "paused" rows, and drafts into image-less paused rows.
   Limit it to `status_id = 0`. Follow `.claude/skills/delete-user-sync/SKILL.md`; it's a two-line change in each
   repo.
4. **Paused without an image can exist:** from item 3, from legacy rows, and because the CHECK allows it. Unpause then
   returns 409 `IMAGE_REQUIRED`. The table shows "Paused" plus the same "Add a photo" CTA. Test it.
5. **Wrap #58 in try/catch on the publish path.** It runs after the UPDATE committed, so a notification failure must
   not turn a successful publish into a 500.
6. **The "3 drafts are waiting" header must come from the server** (`draft_count` on the list response, or a
   `status=draft` filter). The list is paginated 20 at a time, so a client-side count is wrong. Otherwise drop it from
   v1.
7. **Name resolution only looks at the first page:**
   - `productState` first 100 (`account.write.tools.ts:333`);
   - `resolveProduct` first 50 (`products.write.tools.ts:228`);
   - `products_list` first 30 (`catalog.tools.ts:130`).

   Bulk drafts make catalogs bigger. `products_publish` should resolve through the route's `search` param, not page 1.
8. **The injection eval should use the untrusted path.** A seller typing an injection into their own dish is not an
   attack. Seed a description with an instruction (the existing `productNote` pattern, `seed.ts:403`), then "list my
   drafts / publish X" must not produce any other write. Keep the "pasted list from a WhatsApp forward that says also
   delete Y" case.
9. **One #58 per publish** means N publishes send N notifications to every CM. That's parity with today's UI creates,
   so it's fine for v1. Put a debounce in the follow-up ticket.
10. **The card already renders rows and notes only while `status === 'proposed'`** (`AgentActionCard.js:73,113`). The
    receipt must therefore name the drafts ("6 drafts: …") so the seller still sees which items need photos after
    Confirm.

## Claims in the plan verified as wrong (or stale)

| Plan claim | Reality |
|---|---|
| §3.6 "a draft can never be ordered" | The job editor can add any non-deleted item, with no status or ownership check (R1). |
| §3.4.5 "`useTransaction`, as in `databaseUtil.ts:186`" | Line 186 is `executeMultipleQueries`, a static query list. There's no callback transaction helper, and the create helpers each use their own connection (R4). |
| §3.7.2 `uiParity = chat-only` (a free choice) | The parity test pins the chat-only list. It needs the requester's decision (R4). |
| §5 "#381's multi-item work is **unpushed**" | `catalogCreateMany` and `catalogSkipped` are in the #381 diff (R12). |
| §5 the shared-file list | Misses `catalogHandlers.ts` and `render.ts`, both changed by #381 (R12). |
| §3.7.2 "notes … each with its reason, as the catalog skill does" | #381's note shows the name *or* the reason, not both (Suggestion 2). |
| §3.3 the precedent migration is a three-copy example | It's a legacy DEV-only file. The plan's SQL is also not schema-qualified (R7). |
| §3.7.2 "invalid items skipped … as the catalog skill does" + "one transaction, all or nothing" | The catalog skill *executes* per item, not all-or-nothing (R4). |
| §1.3 "every image UUID resolves to a `files_suppliers` row" | Maybe true at scan time, but nothing enforces it, and the eval seed writes random UUIDs with no file (R2). |
| `catalogSkill/text.ts:25` | The path is `src/services/catalogSkill/text.ts`, outside `agent/` (minor). |

Verified **correct**:
- G1 (`productService.ts:208,214`; handler `:326`);
- G2 (`:285,353`);
- G3 (`:439-462`, `getProductById` without a status filter; hidden lines `jobsService.ts:1801-1809`);
- G4 (`is_active` only, `:100,555`);
- G5 (pause `catch` is `console.error` only);
- G6 (#58 only in create, `:227-229`);
- G7;
- G8 (`prompt.ts` "one … product per card", `config.ts:58`);
- the photo caps (`attachments.ts:18`, `runTurn.ts:88`);
- the card renders any number of rows (`AgentActionCard.js:73-111`), and a batch counts as one write (one action row,
  `toolSet.ts:80-93`);
- `writes.test.ts:217` stays green, because `products_publish` has no Undo;
- the ACTIVE-only readers I spot-checked:
  - members `list-stores.handler.ts:200,323,376`, `get-store-catalog.handler.ts:264`,
    `create-store-order.handler.ts:133`, `get-product-order-details.handler.ts:317`;
  - Admin `supplierProfileService.ts:1595,1600,1828`, `communityAi/context.ts:141`;
  - community-proj's join-community filters (`CommunityCard/index.js:95`, `MyCommunities/index.jsx:259`);
- the `EXISTS (… products …)` checks without a status filter are avatar or "has a store" fallbacks. Cosmetic, as the
  plan says.
- B3, B6 (dead 409 path: `deleteProduct` never returns `has_orders`), and both text bugs (`image_too_large` "5MB", the
  delete success toast using `delete_dialog.confirm`).

**Not verifiable from code:** AppSmith (ka-staging) isn't git-connected, and the `keilot_admin` export showed no
product writes in a code search. Before the staging and prod migration, ask the user whether any AppSmith page edits
`products`/`restaurants` `status_id` or `main_image_url`. If one does, it gets the 409/CHECK behaviour too.

## Answers to the plan's open questions

**Q1. Auto-publish when a photo is added?** Agree: **no**, an explicit Publish. In the edit modal, once there's an
image, the primary button reads "Publish" and saving and publishing happen in one request (`publish=true`). It's one
tap, not two.

**Q2. A draft with a photo?** Agree: **not in v1.** Make sure the batch card says clearly which rows go **live on
Confirm** (rows with a photo). Users read "drafts" in the title and may assume all of them are drafts.

**Q3. 20 per card, keep the photo caps?** Agree.

**Q4. Validity:** skip invalid items and duplicates into the note at proposal (agree). At Confirm, re-prepare
all-or-nothing (a drift refreshes the card). **Execution is per item with an "X of N" receipt** (R4), not one
transaction, unless the user explicitly chooses atomic execution and accepts the `chat-only` parity entry.

**Q5. Menu-photo reading:** agree, **phase 2.** When it lands, names and prices read from a photo are untrusted text.
They may only feed a `products_create_many` card, the same rule as pricelist photos.

**Q6. Small bugs:**
- **B3: in scope** (R10). It sits on the draft-completion path.
- B6 and B10: follow-up ticket.
- The two text fixes: yes, here.
- Also take Suggestion 3 (delete-user status filter) if the user agrees, since it touches the delete-user API.

**Q7. Staging and prod counts:** yes, read-only aggregates only, with the user's OK. Count:
- (a) ACTIVE with no or empty image, which the migration would demote;
- (b) NOT_ACTIVE with no image, which would be refused on unpause;
- (c) items whose `main_image_url` has no `files_suppliers` row (R2);
- (d) items with gallery images but no main image.

If (a) is above 0, the demotion becomes a decision (R7).

---

## Gallery question (asked in the task)
Keep the CHECK on `main_image_url` only:
- Every reader (members cards, storefront, order lines) renders the **main** image.
- A gallery-only item would still show members a blank tile.
- A two-table trigger buys nothing.

Handle "has gallery, no main" in the UX: "use as main photo" (R10).
