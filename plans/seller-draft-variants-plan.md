# Seller draft items without an image: problem, market research, solution and plan (v3)

Status: **approved by the Fable review (APPROVE WITH CHANGES, `plans/seller-draft-variants-review.md`). Implementing.**

## v3: review changes accepted (these override v2 below where they differ)
- **R1.** `editJob` (`jobsService.ts` ~1811) rejects a `product_id` that isn't ACTIVE/NOT_ACTIVE **and** owned by the
  order's supplier (400 `PRODUCT_NOT_AVAILABLE`). The community-proj EditOrder picker hides drafts. Jest tests.
- **R2.** "Has an image" = a `files_suppliers` file of **this supplier** (resolved in the service). An unknown UUID
  counts as no image: create → DRAFT; editing a live item → 409 `IMAGE_REQUIRED`. The CHECK stays as the backstop.
- **R3.** One error contract:
  - `AppError(msg, 409, err, { code })` with `IMAGE_REQUIRED`, `DRAFT_CANNOT_PAUSE`, `PRODUCT_NOT_AVAILABLE`;
  - `isActiveImageViolation(err)` checks code 23514 **and** the constraint name, applied on every product write;
  - an atomic status UPDATE that checks `rowCount`;
  - community-proj switches on `data.details?.code`.
- **R4.** **No `createProductsBatch`.** `products_create_many` loops `createProductHandler` per item: dedup is
  re-checked before each, the receipt reads "X of N", `createProduct({ notify:false })` is used, and one #58 is sent
  after the loop. `uiParity` = `same` (`POST /{products,restaurants}/supplier`). Validation stays all-or-nothing at
  Confirm (prepare re-runs).
- **R5.** No silent default photo for products: `main_photo: ""` = no photo = draft. `defaultToNewest` stays only for
  `business_set_image`.
- **R6.** `products_create_many` gets `replaceKey: vertical`. A pending `products_create` card with the same normalized
  name counts as a duplicate.
- **R7.** The migration is schema-qualified. Order: DROP IF EXISTS → demote UPDATE → ADD (validated). Constraint names
  are `chk_products_active_requires_image` / `chk_restaurants_active_requires_image`. On staging and prod, a non-zero
  demotion count is a **decision for the user**, not a side effect.
- **R8.** Dev rollout order: the members fixture fix is merged → a note in the shared folder → then the dev
  migration.
- **R9.** `getProductById({ excludeDeleted })` is used by GET-one, update, delete, status and publish, so a DELETED id
  → 404.
- **R10.** B3 is in scope: on update, the gallery is rebuilt from the kept `extra_image_urls` plus new files, in
  order. The UI offers "use as main photo" for a draft that has gallery images but no main image.
- **R11.** Publish responses carry `fulfillment_missing`. The UI shows `FulfillmentReminderDialog`, and the agent
  receipts use `select.fulfillment`.
- **R12.** #381 is still open and CONFLICTING:
  - Branch from `dev`, and copy `normalize()` into `src/services/agent/text.ts`.
  - #381 also touches `catalogHandlers.ts` and `render.ts`.
  - The prompt sentence is agreed via the shared note: "One card per change, except the batch tools
    (`products_create_many`, and the `_many` tools), which put every item the supplier named in their own message on
    one card."
- **Suggestions taken:**
  - S1: exact `normalize` equality, plus `allow_duplicates`;
  - S2: notes show "name: reason";
  - S3: delete-user sets NOT_ACTIVE only on ACTIVE rows (both repos);
  - S4: paused items without an image get the same CTA;
  - S5: #58 in try/catch;
  - S6: `draft_count` from the server;
  - S7: `products_publish` resolves the item through `search`;
  - S8: the injection eval goes through the untrusted path;
  - S10: receipts name the drafts.
- **Deferred:**
  - staging/prod read-only counts and the AppSmith-writers question (needed before the staging/prod migration, not
    for the dev build);
  - B6 and B10 (follow-up ticket).
Research date: 2026-10-30.
Sources:
- the code on `origin/dev` of every submodule, read line by line;
- the dev DB (`1_0_0_DEV`), read-only (staging and prod were not read: they need the user's OK);
- web research on 13 platforms.

Siblings: agent-bulk-actions, agent-catalog-skill (done, PR #94) and community-chat (done, PR #380).

---

## 1. The problem

### 1.1 What the seller needs
A restaurant or shop wants to put its menu or catalog into Keilot fast, often by telling the AI "add these 8 dishes".
At that moment it usually **has the names and prices but not the photos**.

Today the app forces a photo for every item before it can be saved:
- the community-proj form requires a main image;
- the agent's `products_create` throws `mainPhotoRequired`.

So the seller either stops, or uploads a random image to get past the form. The market data says photos really do
matter to buyers:
- Uber Eats: merchants with ≥70% of items photographed earn about **$330 a week more**;
- DoorDash: **+44% monthly sales** with item photos;
- Grubhub: up to **+30%** item sales.

So we don't want imageless items in front of members either.

**The need:** "let me save it now and finish it later", with a hard rule that **nothing reaches members without a
photo**.

### 1.2 What the code does today (verified on origin/dev)
A dish or shop item is one row in `products` (store) or `restaurants` (menu). The two tables are identical and one
code path serves both. Sellers manage them in **community-proj** (`/manage-pricelists`, Products / Food tabs) through
**Admin_system** `/api/{products|restaurants}/supplier`. The seller-side AI is the **Admin_system agent**
(`Admin_system/src/services/agent`), used by community-proj's chat. `members` only shows and sells items.

The DRAFT status **already exists** (`default_statuses_with_draft`: -1 DRAFT, 0 ACTIVE, 1 NOT_ACTIVE = paused,
2 DELETED), but no item has ever been a draft. The "photo is required" rule lives **only in the UI and the agent**.
The server and DB don't enforce it, and several paths break it:

| # | Gap | Evidence |
|---|---|---|
| G1 | The server creates **ACTIVE items with no image**: it checks only name and price; the status is hard-coded ACTIVE; the DB default is ACTIVE. | `catalogHandlers.ts:326`, `productService.ts:208,214`, `tables/products.sql` |
| G2 | **An edit can wipe the image of a live item**: `main_image_url = @mainImageUrl` with `|| null`. The current UI and agent always resend the URL, so only a direct API call hits it. | `productService.ts:285,353` |
| G3 | **Unpause has no gate**: `PUT …/status {active}` sets ACTIVE on any row the seller owns, **including DELETED rows** (`getProductById` doesn't exclude them). A hidden one-off order line (no image, COMY1-292) or an old superseded version can be brought back to the storefront. | `productService.ts:439-462,504`, `jobsService.ts:1801-1809` |
| G4 | **The UI knows only `is_active`**: every non-active item shows "Paused" and offers "Unpause". A draft would look paused, and "Unpause" would publish it without a photo (G3). | `ProductsTable.js:195-205,258-261`, `productService.ts:100,555` |
| G5 | **Status errors are silent** in the UI (`console.error` only), so an "image required" refusal would show nothing. | `ProductsContent/index.js:266-268` |
| G6 | The community-manager notification #58 ("new items in the store") fires **only on create**. A draft published later would never announce itself. | `productService.ts:227-229` |
| G7 | The members agent eval fixture inserts **ACTIVE items with no image**. A DB rule would break it, so it must be fixed in the same change. | `members/evals/member-agent/fixture.s3.ts:53-57` |
| G8 | **The agent can't add several items at once**: one item per card, the prompt rule "one product per card" (`prompt.ts:27`) and a cap of 10 writes per conversation. "Add 30 dishes" is impossible. | `products.write.tools.ts`, `config.ts:58` |

Smaller issues found on the way (follow-ups, not in scope unless the user wants them):
- **B3** editing extra images loses the ones kept (`catalogHandlers.ts:485-493`);
- **B6** dead 409 `PRODUCT_HAS_ORDERS` path;
- **B10** catalog image files are never cleaned up, and duplicates share file UUIDs;
- "supplier has a store" `EXISTS` checks ignore status;
- the delete toast says "Delete Product";
- the `image_too_large` text says 5 MB while the limit is 15 MB.

### 1.3 What the data says (dev DB, read-only)
- 77 items in all (64 products, 13 restaurants) from 8 suppliers. **0 drafts.** **Every non-deleted item has a main
  image**, and every image UUID resolves to a `files_suppliers` row.
  - The 9 items without an image are all DELETED products: hidden one-off order lines, or their leftovers.
  - None has an image only in the gallery, and there are no empty-string images.
- **A hard DB rule fits dev today with no data fix.** Staging and prod must be counted before the migration.
- Sellers do enter items in runs: 16, 12 and 5 consecutive ids by one supplier. The tables have **no timestamps**,
  so we can't measure it better.
- Duplicate names exist: one supplier has "צלחת" ×10 (QA junk), another "נעלי בית" ×2. Dedup on bulk create is worth
  having.
- Drafts are used today only by service pricelists (`supplier_services_pricing_data.status = -1`, 29 rows, created
  by Onboarding). Their UX shows "Waiting for pricing" / "מחכה לתמחור", and any save publishes them.
- Agent history: 6 `products_create` calls (4 executed, 2 failed on a 15 MB image), all with a photo, 0 refusals for
  a missing photo. Chat photo attachments are used mostly for pricelists (21) and product creation (4).
- The eval seed was writing to dev **during** the scan: the test suppliers' items come and go. Note this for QA.

## 2. How other platforms handle it

| Platform | Can an item be saved without a photo? | Can it go live without a photo? | What happens to a live item that loses its image | Bulk creation |
|---|---|---|---|---|
| **eBay** | Yes, as a **draft** | **No** (≥1 photo, ≥500 px) | — | Bulk tool / CSV |
| **Amazon** | Yes (Incomplete) | **No**: main image required | **Search-suppressed**: kept and hidden, with a "Fix" queue | Flat-file upload |
| **Google Merchant Center** | Yes (in the feed) | **No**: `image_link` required | **Disapproved** until fixed | Feed |
| **Etsy** | Yes, as a draft | **No** (≥1 photo) | — | CSV (third-party tools) |
| **Meta Shops** | — | **No**; placeholder images are flagged | Rejected | Catalog feed |
| **Shopify** | Yes (Draft / Active / Archived / Unlisted) | Yes | — | CSV; AI backgrounds only |
| **WooCommerce** | Yes (draft / pending / private / publish) | Yes, shows a grey **placeholder** | — | CSV |
| **Wolt** (incl. Israel) | Yes | Yes. **AI-generated food photos are banned**; "Improve with AI" edits real photos | Enable / disable toggle (used for stock) | Google Sheets import, batch photo upload |
| **Uber Eats** | Yes | Yes; photos are reviewed 3–5 days | Available / Sold out | Menu Maker; AI descriptions approved by the merchant; AI photo enhancement |
| **DoorDash** | Yes | Yes | Active / Out of stock / Hidden | Menu import; AI descriptions and photo enhancement |
| **Toast** | Yes | Yes ("not required to go live") | Visibility per channel | — |
| **Square** | Yes | Yes | Items vs menus | **Upload a menu PDF/photo → AI builds the menu** |

(10bis/Cibus, Deliveroo, Just Eat and Lightspeed: no usable public docs found.)

**Takeaways for Keilot:**
1. **"Draft without a photo, can't publish without one" is exactly eBay's and Etsy's model**, and Amazon and Google
   enforce the same rule more strictly. Food apps are looser but push photo coverage hard, because it's the lever with
   real numbers behind it. Our rule puts us with the strict marketplaces, which suits a small community market where
   each item is seen by neighbours.
2. **Hide; don't show a placeholder.** WooCommerce's grey box is the counter-example, and Meta flags placeholders.
3. **"Is the item ready" is separate from "is it available now".** Shopify, Toast and Wolt separate the two. We
   already have that split: DRAFT is "not ready", and ACTIVE vs NOT_ACTIVE (paused) is "on or off".
4. **A clear "why it isn't live, and how to fix it"** (Amazon's Fix queue) beats a silent hidden state. A draft
   should say "add a photo to publish" and offer it as one tap.
5. **Bulk creation is standard** (Square from a menu photo, Wolt from Sheets). Our agent already reads pricelist
   photos, so "add these dishes" (from text or a menu photo) should become **one card with many drafts**.
6. **Don't let AI invent dish photos.** Wolt bans it, and Uber's AI "re-plating" drew criticism. We only take real
   photos from the seller.
7. **The seller approves what AI writes** (Uber's AI descriptions need the merchant's OK). The agent's Confirm card
   already does this.

## 3. The solution

### 3.1 Lifecycle
```
                 ┌─────────── Publish (needs a main image) ───────────┐
create, no image ─► DRAFT ──────────────────────────────────────────► ACTIVE ◄──► NOT_ACTIVE (paused)
create with image ─────────────────────────────────────────────────► ACTIVE
                                                       any (owner delete) ──► DELETED (terminal)
```
- **Rule (the invariant):** `status = ACTIVE ⇒ main_image_url is set`. It is enforced in four places:
  1. the **DB** (a CHECK constraint, the final guarantee);
  2. the **Admin_system service** (409 `IMAGE_REQUIRED`, with a clear message);
  3. the **agent's `prepare`** (a refusal plus a button);
  4. the **UI** (it never offers Publish without an image).
- **The seller decides when to publish.** A draft with an image still needs the seller's "Publish" tap, so it never
  goes live by surprise.
- **DRAFT can't be paused or unpaused.** Its only way forward is Publish, and it can be deleted.
- **A live or paused item can't lose its main image.** To take it down, the seller pauses it.
- **DELETED is final** for the status API. That closes G3: you can't bring back hidden order lines or old versions.
- **Visibility:** a draft is seen only by its owner, in Manage pricelists and in the agent's `products_list`. It is
  already invisible everywhere else: the members storefront, search, cart and checkout, the members agent, the
  community-manager supplier view, dashboards, community AI and the join-community "has a catalog" check all read
  ACTIVE only (§3.6).
- **Notification #58** fires on create when the item is ACTIVE, and on a draft's first publish. It doesn't fire for a
  draft.

### 3.2 Options considered for enforcement

| Option | Pros | Cons | Verdict |
|---|---|---|---|
| A. UI only (today) | — | Already broken by G1–G3 | ✗ |
| B. App guard in the Admin_system service | Clear errors | Any other writer (scripts, the members fixture, future code) bypasses it | needed, not enough |
| **C. B + a DB CHECK on the same row** `CHECK (status_id <> 0 OR main_image_url IS NOT NULL AND btrim(main_image_url) <> '')` | The final guarantee, cheap, no trigger; the main image is on the same row; dev has 0 violators | Gallery images don't count (by design: the UI already requires the main image) | **✓ recommended** |
| D. A trigger that also counts gallery images | Allows "gallery only" | Two-table trigger; create inserts the row before the gallery (no transaction), so it needs deferral | ✗ not worth it |
| E. A new `is_draft` column | — | A second source of truth next to the existing DRAFT status | ✗ |

### 3.3 DB (cme_db)
`migrations/{dev,staging,prod}/2026-10-30_items_active_requires_image.sql`. It follows `cme_db`'s db-migrations
skill: three copies that differ only in the schema name, `BEGIN; … COMMIT;`, re-runnable, and a "why" header. The
precedent is `2026-10-08_order_items_available_quantity.sql`. The same block runs for `products` and `restaurants`:

```sql
ALTER TABLE products DROP CONSTRAINT IF EXISTS chk_products_active_requires_image;
ALTER TABLE products ADD  CONSTRAINT chk_products_active_requires_image
  CHECK (status_id <> 0 OR (main_image_url IS NOT NULL AND btrim(main_image_url) <> '')) NOT VALID;
UPDATE products SET status_id = -1      -- live items without an image become drafts (0 rows on dev)
 WHERE status_id = 0 AND (main_image_url IS NULL OR btrim(main_image_url) = '');
ALTER TABLE products VALIDATE CONSTRAINT chk_products_active_requires_image;
```

- Update `tables/products.sql` and `restaurants.sql` to match.
- **Before running it on staging or prod**, do a read-only count of the rows the UPDATE would demote. That needs
  the user's OK, because this session may not read prod. Demoted rows are listed in the PR.
- No new table and no user columns, so the delete-user API is unaffected. I'll confirm with the delete-user-sync
  skill.

### 3.4 Admin_system API (`productService.ts`, `catalogHandlers.ts`, `productController.ts`)
1. **Create (G1):** status = `hidden ? DELETED : (image ? ACTIVE : DRAFT)`. #58 only when ACTIVE. The response adds
   `status: 'draft' | 'active' | 'paused'` (and keeps `is_active`).
2. **Reads (G4):** `getSupplierProducts` and `getProductById` return `status`. `getProductById` gets an option to
   exclude DELETED, used by the status and update paths.
3. **Status (G3):** `PUT …/status` takes `'active' | 'paused'` as today.
   - A DELETED row → 404.
   - A draft + `paused` → 409 `DRAFT_CANNOT_PAUSE`.
   - `active` without an image → 409 `IMAGE_REQUIRED`.
   - A draft + `active` with an image = **publish**, which sends #58.
   - The UPDATE has `AND status_id <> 2`. A CHECK violation (`23514`) maps to `IMAGE_REQUIRED`, so a race returns
     409, not 500.
4. **Update (G2):** if the row is ACTIVE or paused and the result would have no main image → 409 `IMAGE_REQUIRED`.
   - A new optional `publish=true` on a draft that has an image (new or kept) sets ACTIVE in the same statement.
   - It works in **both** the in-place path and the versioning path (the versioning path copies `old.status_id`, so
     it must use the new status).
   - A DELETED id → 404.
5. **Bulk create (G8):** a new `createProductsBatch(supplierId, vertical, items[])`.
   - Images are saved first (`saveCatalogImage`); then one `databaseUtil` transaction (`useTransaction`, as in
     `databaseUtil.ts:186`) inserts every item with the same SQL as create, plus tags and gallery.
   - One #58 for the ACTIVE ones.
   - It has no HTTP route: only the agent uses it.
6. **Error codes** are stable strings (`IMAGE_REQUIRED`, `DRAFT_CANNOT_PAUSE`), so the UI and the agent translate
   them.
7. **Tests (jest):** every transition in the table above; create without an image → DRAFT; the 23514 mapping;
   publish on update in both paths; a DELETED row can't be revived.

### 3.5 community-proj seller UI (JavaScript, MUI, i18next; `ManagePricelists/Content/ProductsContent/*`)
- **`ProductModal.js`:**
  - The image stops being mandatory **on create and for drafts**. With no image the primary button reads **"Save
    as draft"**, and a line under the image box says "Without a photo the item is saved as a draft and members won't
    see it. Add a photo to publish."
  - Editing a draft: "Publish" (primary; disabled until there's an image) plus "Save draft".
  - Editing a live or paused item: the image stays required, as today.
- **`ProductsTable.js`:**
  - A **"Draft" chip** (a new tone, next to "Paused"; theme tokens, following the `KycStatusChip` pattern).
  - A clickable **"Add a photo to publish"** action that opens the edit modal on the image field.
  - The kebab menu for a draft: "Add photo and publish", Edit, Duplicate, Delete, and no Pause/Unpause.
- **`ProductsContent/index.js`:**
  - Uses `status`.
  - Shows a translated snackbar on 409 `IMAGE_REQUIRED` / `DRAFT_CANNOT_PAUSE`, fixing the silent error (G5).
  - Optionally a small header line, "3 drafts are waiting for photos", with a filter. That's Amazon's "Fix" queue
    idea, kept small.
- **Translations:** `public/locales/{he,en}/supplier-products.json`, in gender-neutral Hebrew ("טיוטה", "הוספת תמונה
  לפרסום", "שמירה כטיוטה", "פרסום"). The agent already says "טיוטה" (`adminAgent.json` `productDraft`).
- The component registry gets an entry. The PR carries "Translation review needed" and "Design review needed" (no
  Figma: a new chip tone and CTA).

### 3.6 Who must not see drafts (audit; all verified)
- **Already ACTIVE-only, so no change:**
  - members: `list-stores.handler.ts:200,323,376`, `get-store-catalog.handler.ts:264`,
    `create-store-order.handler.ts:133` (checkout rejects non-ACTIVE), `get-product-order-details.handler.ts:317`,
    and the members agent via those handlers;
  - Admin_system: `supplierService.ts:613,626,746,759,1888,2543`, `supplierProfileService.ts:1595,1600,1828` (the CM
    view), `supplierDashboardService.ts:67`, `pricingService.ts:2433`, `supplierCategoriesUtil.ts:86`,
    `communityAi/context.ts:141`;
  - community-proj's join-community check (`is_active` products).
- **Must change:** the seller list and get, create, status and update (§3.4); the seller table (§3.5); the agent
  (§3.7); the members eval fixture (G7).
- **Unchanged, and fine:**
  - past orders, jobs and rev-share read ordered items by id (a draft can never be ordered);
  - delete-account sets everything NOT_ACTIVE, which the CHECK allows;
  - Onboarding, notifications and pricelist_tools never touch items.
- **Pre-existing and cosmetic (follow-up):** the `EXISTS (… products …)` "supplier has a store" checks ignore status.

### 3.7 Agent (Admin_system agent)
Facts that shape the design, from `origin/dev`:
- The card already renders any number of `rows` with no truncation (`AgentActionCard.js:73-111`), but only **one
  thumbnail strip** for the whole card.
- **Photo caps:** 20 attachments per conversation (`attachments.ts:18`) and 10 per message (`runTurn.ts:88`).
- **`resolvePhotos` with `""` picks the newest unused photo, even a menu or pricelist photo**
  (`products.write.tools.ts:88`). `used_by` is set only after Confirm.
- The 10-writes cap counts a batch as 1.

The changes:
1. **`products_create`:**
   - The photo becomes optional. Without one, the card is titled "New draft" and notes that members won't see it
     until a photo is added. The receipt says draft or live.
   - The `""` default skips photos already read as a menu or pricelist (`analysis` set). That fixes the wrong-photo
     risk.
   - `uiParity` stays `same`, because the form allows drafts too.
2. **New `products_create_many`** (confirm tier):
   - `{ catalog, items: [{ name, price, category, description?, tags?, photo? }] }`, with 2–20 items.
   - `photo` is an **explicit ref per item, with no default**. The same ref twice → `duplicateItem`.
   - `prepare` runs the per-item checks of `products_create`, at proposal and again at Confirm.
   - **Dedup** with the catalog skill's `normalize()` (Hebrew-safe: final letters, niqqud, geresh;
     `catalogSkill/text.ts:25`) against every non-deleted item of that catalog (paginated) and within the batch.
   - **Card, in the same shape as the catalog skill's in-flight `catalogCreateMany`:**
     - title `productsCreateMany {count}`;
     - one row per item: label = name, detail = category (and the photo's file name if there is one), `from: null`,
       `to` = price, `flag` = "draft – no photo" or nothing;
     - the field `notified`;
     - notes `productsSkipped` ("Not added": duplicates and invalid items, each with its reason);
     - the thumbnail strip of the photos used.
   - Invalid items are **skipped into the note, not fatal**, as the catalog skill does. The card is refused only if
     no item is valid.
   - **Execution is one transaction** (`createProductsBatch`), with the duplicates re-checked inside it, so it's all
     or nothing.
   - Receipt `productsCreatedMany`: "Added 8: 2 live, 6 drafts (add photos to publish)", as an ICU plural.
   - It counts as one write, with a chain limit of 3 per conversation.
   - `uiParity` = `chat-only`.
3. **New `products_publish`** (confirm tier, no Undo) for a draft: `{ product, photo? }`.
   - With a photo, it sets the image and publishes in one card. Without a photo it needs the draft to already have
     one; otherwise it refuses with a button to the products page.
   - **`products_set_status` stays active⇄paused only.** Its `productDraft` refusal now points to
     `products_publish`. That avoids a broken Undo: today's Undo can only go back to active or paused, never to
     draft.
4. **`products_update`:** allowed on drafts. The photo rule (`:288`) applies only to live and paused items.
5. **`products_list`:** adds `status` (draft / live / paused) and `has_photo`, so the agent can say "3 dishes are
   waiting for photos".
6. **Prompt:**
   - Replace the "one … product per card" rule with an exception for `products_create_many`.
   - Change the `products_create` line ("needs a main photo") to describe drafts.
   - Real photos only: never pick a menu photo as a dish photo, and never invent or generate one.
   - Gender-neutral Hebrew.
   - I'll coordinate the wording with the siblings, because they edit the same lines.
7. **Translations, `uiParity.ts`, `tools/index.ts`, unit tests** (s3 and writes suites; `writes.test.ts:217` stays
   true because `products_publish` has no Undo).
8. **Evals:**
   - flip `product_no_photo_en` (`scenarios.s3.ts:232`): now it must create a **draft**;
   - extend `s3State` with restaurants and draft status;
   - new journeys:
     - "add these 8 dishes" (he), no photos → 8 drafts;
     - 2 dish photos → 2 live + 6 drafts;
     - a duplicate of "Eco cleaning kit" → noted, not created;
     - publish without a photo → refused, then a photo → published;
     - an injection in a dish description.
   - Dish photos are drawn with sharp in `photos/generate.cjs`.

   They run in community 43 (supplier 10084, profile 10101) at k=1, coordinated with any sibling eval run.

**Not in v1 (proposed as phase 2): "read my menu photo → drafts".** `pricelists_read_photo` already reads names and
prices from a photo, but its output is shaped for service pricelists, has no categories, and caches on the single
`analysis` column. A `products_read_menu` tool feeding `products_create_many` is the Square-style feature. It's worth
doing next, once v1 lands.

## 4. Open questions (my recommendation in bold)
1. **Auto-publish when a photo is added to a draft?** **No: an explicit "Publish" button, primary once there's a
   photo.** The seller stays in control, and it's still one tap.
2. **Allow "save as draft" for an item that has a photo** (e.g. to prepare a seasonal menu)? **Not in v1.** A draft
   means "missing a photo", which keeps the badge meaningful. It's easy to add later, since the status already
   exists.
3. **Bulk create size:** **20 per card.** Photos stay optional per item: the chat caps (10 per message, 20 per
   conversation) make "20 dishes, each with a photo" impossible in one chat anyway. Most bulk items will be drafts,
   and the seller adds photos later in Manage pricelists or one by one in chat. **Keep the caps as they are.**
4. **Bulk validity:** **invalid items and duplicates are left out and listed on the card** (as the catalog skill
   does). **Confirm then creates the rest in one transaction, all or nothing.**
5. **Menu-photo reading (Square-style):** **phase 2**, not v1.
6. **Fix the small bugs found on the way** (B3 lost extra images, B6 dead 409, toast text, 5 MB text)? **Fix the two
   text bugs here (two lines). Put B3, B6 and B10 in a follow-up ticket.**
7. **Read staging and prod counts** before the migration: **yes, read-only aggregate counts only.** This needs your
   explicit OK.

## 5. Coordination
- **Bulk actions (members agent):** its `bulk:` framework lives in **members**, and its plan excludes create tools.
  The seller tools live in **Admin_system**, so there's no shared code. We match its rules instead: one card, a
  per-item server `prepare` at proposal and at Confirm, item ids never trusted from the model, exclusions shown with
  a reason.
- **Catalog skill** (Admin_system #381, CONFLICTING; cme_db #94; members #325; community-proj #405; all open):
  - It edits the same `prompt.ts` lines (:23, :46-58), `config.ts` chain limits, `uiParity.ts`, `tools/index.ts` and
    `adminAgent.json`.
  - Its **unpushed** work adds the multi-item `catalogCreateMany` card and a `catalogSkipped` note.
  - **Dependency: I'll branch Admin_system from #381's tip once it's pushed (or rebase onto it), import its
    `normalize()`, and copy its card and note shape.** If #381 isn't ready when I start, I'll branch from dev, copy
    `normalize()` into a shared `agent/text.ts` and rebase later.
- **Bulk actions** (plan v2 only, no code yet):
  - It plans to change `define.ts`, `toolSet.ts`, `actions.ts` and `AgentActionCard.js` (checkboxes, "Show all N",
    "Confirm N").
  - **I won't touch those files.** The rows I add are plain `rows[]`, so its future card UI picks them up for free.
  - It lists `products_set_status` as a candidate it may wrap. With `products_publish` separate, `products_set_status`
    keeps its current shape, so there's nothing to break.
  - Shared conventions: a batch counts as 1 write; a `duplicateItem` key; a "can't be included" note with reasons.
- **Community chat / square** (Admin_system #380, merged): only delete-user code, no overlap.

## 6. Delivery plan
1. **cme_db** (its own worktree): migration plus table files. Apply on dev.
2. In parallel, in worktrees cut from `origin/dev`:
   - **Admin_system**: API (§3.4) + agent (§3.7), one PR;
   - **community-proj**: UI (§3.5), one PR;
   - **members**: the fixture fix (G7), one tiny PR.

   Separate subagents can do these, since the files don't overlap.
3. Review with `/code-review` on each PR.
4. **QA:**
   - Browser, as seller 10101 in community 43: create without a photo → Draft chip; Publish is unavailable and the
     API refuses `active` with 409; add a photo → Publish → live; confirm the members storefront never showed the
     draft.
   - The agent: the same journey, plus "add these 8 dishes".
   - Coordinate with any session running evals on dev, because the eval seed rewrites the test items.
5. **Deploy order: DB → Admin_system → community-proj.** The old UI still requires an image, so nothing breaks in
   between.
6. A PR per submodule, the root pointer bump, then the upgrader agent with the lessons.
