# Seller draft items: implementation contract (v3)

This file is the source of truth for the parallel implementers. The plan is `plans/seller-draft-variants-plan.md`
(the v3 section at the top) and the review is `plans/seller-draft-variants-review.md`.

## Worktrees (branch `claude/seller-draft-items` in each repo; never touch the shared checkouts)
- Admin_system: `C:\Users\User\Documents\keilot\.wt\admin-drafts`
- community-proj: `C:\Users\User\Documents\keilot\.wt\cp-drafts`
- cme_db: `C:\Users\User\Documents\keilot\.wt\cme_db-drafts`
- members: `C:\Users\User\Documents\keilot\.wt\members-drafts`

`node_modules` in each worktree is a **junction** to the shared checkout's. Never delete it or run `npm install`
there. Implementers do not commit; the lead session commits.

## Statuses
`default_statuses_with_draft`: -1 DRAFT, 0 ACTIVE, 1 NOT_ACTIVE (paused), 2 DELETED. Statuses are always looked up by
name in SQL (`WHERE status = 'DRAFT'`), never by a hard-coded id.

## The invariant
`ACTIVE ⇒ the item has a valid main image`. A valid main image means `main_image_url` is a UUID with a
`files_suppliers` row for **this supplier** (check the column names: `uuid`, `supplier_id`). The DB backstop is
`chk_products_active_requires_image` / `chk_restaurants_active_requires_image`:
`CHECK (status_id <> 0 OR (main_image_url IS NOT NULL AND btrim(main_image_url) <> ''))`. It isn't applied on dev
yet. The app must enforce the rule by itself.

## Admin_system HTTP API (`/api/products/...` and `/api/restaurants/...`, same router)

**Items in responses** (list, GET one, create, update) keep every existing field and add:
- `status: 'draft' | 'active' | 'paused'` (`is_active` is unchanged, true only for active);
- `has_image: boolean` (a valid main image, as defined above).

**`GET /supplier`** list response also gets `draft_count: number`: every DRAFT item of the supplier in that vertical,
not paginated.

**`POST /supplier`** (create): the body is unchanged. A valid image → ACTIVE plus notification #58, as today. No valid
image → **DRAFT** and **no** #58. The response stays `{ product, fulfillment_missing }`.

**`PUT /supplier/:id`** (update):
- New optional multipart field `publish` (`"true"`).
- DELETED or unknown id → 404.
- On a DRAFT:
  - with `publish=true` and a valid main image (new upload or kept) → ACTIVE in the same write, plus #58;
  - with `publish=true` and no image → 409 `IMAGE_REQUIRED`;
  - otherwise it stays DRAFT.
- On ACTIVE / NOT_ACTIVE: the result must have a valid main image, otherwise 409 `IMAGE_REQUIRED` and nothing is
  written. The status is unchanged.
- The gallery is rebuilt from the kept `extra_image_urls[i]` (UUID extracted like on create) plus the new
  `extra_images` files, in order (fixes B3).
- The response adds `fulfillment_missing` (boolean, meaningful when it was just published) and `published: boolean`.

**`PUT /supplier/:id/status`** `{ status: 'active' | 'paused' }`:
- DELETED or unknown id → 404.
- A DRAFT + `paused` → 409 `DRAFT_CANNOT_PAUSE`.
- `active` without a valid main image (draft or paused) → 409 `IMAGE_REQUIRED`.
- A DRAFT + `active` with an image = **publish**: ACTIVE plus #58 (in try/catch; a failure is logged and never
  fails the request).
- The UPDATE is atomic: `WHERE id AND supplier_id AND status_id <> DELETED AND (target <> ACTIVE OR image present)`,
  then check `rowCount`.
- Response `{ message, status, published, fulfillment_missing }`.

**Error shape:** `AppError(message, 409, err?, { code })` → the JSON body carries `code` at the TOP level (errorHandler spreads `details` into the body on a 4xx), e.g. `{ status, message, code: "IMAGE_REQUIRED" }`. Codes:
- `IMAGE_REQUIRED`
- `DRAFT_CANNOT_PAUSE`
- `PRODUCT_NOT_AVAILABLE` (400, job editor)

A Postgres error with code 23514 on one of the two constraint names maps to `IMAGE_REQUIRED` on every product write
(`isActiveImageViolation(err)`).

**Job editor** (`jobsService` editJob / COMY1-292, around `jobsService.ts:1811`): every existing `product_id` on the
edited order must be ACTIVE or NOT_ACTIVE **and** owned by the order's supplier. Otherwise 400
`PRODUCT_NOT_AVAILABLE` (one query over all ids). Hidden one-off `new_product` lines keep working.

**Delete user** (`userProfileService.ts` ~602-629): set NOT_ACTIVE only `WHERE status_id = ACTIVE` (S3).

## Exports the agent tools rely on (Admin_system, implemented by the API worker)
- `createProductHandler(caller, body, files, vertical, opts?: { notify?: boolean })`: `notify` defaults to true.
  With `notify:false` it never sends #58.
- `productService.notifySupplierAddedItems(supplierId, vertical)`: already exists. Keep it exported and callable
  once after a batch.
- `updateProductHandler(caller, productId, body, files, vertical)`: honours `body.publish === 'true' || true`.
  Returns `{ product, published, fulfillment_missing }`, or whatever it returns today plus those fields. **Read the
  final signature in the file.**
- `updateProductStatusHandler(...)`: as above.
- Item objects carry `status` and `has_image`.
- `getProductById(id, supplierId, vertical, { excludeDeleted })`: GET-one, update, delete, status and publish all
  exclude DELETED.

## Agent tools (`Admin_system/src/services/agent`)
- **`products_create`:**
  - `main_photo` is optional; `""` means **no photo**, i.e. a draft.
  - There's no newest-photo default for products; `resolvePhotos` gets a `defaultToNewest` flag, used only by
    `business_set_image`.
  - The card and receipt say "draft" and "members won't see it until a photo is added".
- **New `products_create_many`** (confirm tier, `uiParity` `same`, endpoints `POST /products/supplier` and
  `POST /restaurants/supplier`):
  - Schema `{ catalog, items[2..20]: { name, price, category, description?, tags?, photo? }, allow_duplicates?: string[] }`.
    `photo` is an explicit ref; the same ref on two items → `duplicateItem`.
  - **Dedup** by exact `normalize(name)` equality, against every non-deleted item of that catalog (paginate or use
    one SQL query) and within the batch. A pending `products_create` card with the same name also counts.
  - Invalid items and duplicates are **skipped into the `productsSkipped` note**, shown as "name: reason". If no item
    is valid, the card is refused with the first reason.
  - The card: title `productsCreateMany {count}`; rows with label = name, detail = category (+ photo file name),
    `from: null`, `to` = price, `flag` = "draft – no photo" for items without a photo; the `notified` field; and an
    attachments strip.
  - `replaceKey` = vertical. The handler loops `createProductHandler(..., { notify:false })` per item, re-checks
    dedup right before each, collects the outcomes, and sends one `notifySupplierAddedItems` if any item went live.
  - The receipt is `productsCreatedMany`, an ICU plural: "Added X of N: K live, D drafts (…names…); not added: …".
    It uses `select.fulfillment` like `productCreated`.
  - Chain limit 3 in `config.ts`.
- **New `products_publish`** (confirm tier, **no Undo**): `{ product, catalog?, photo? }`.
  - The item is resolved by name or id through the route's search, not by page 1 only.
  - It must be a DRAFT, or paused with no image (S4).
  - With a photo → `updateProductHandler` with `main_image` + `publish=true`.
  - Without a photo → it needs `has_image`, then `updateProductStatusHandler` active. Without an image it is refused
    with `draftNeedsPhoto` plus a handoff button to the products page.
  - The receipt uses `select.fulfillment`.
- **`products_set_status`:** unchanged shape (active⇄paused, Undo). The `productDraft` refusal text now points to
  publishing ("send a photo to publish it").
- **`products_update`:** allowed on drafts. The main-photo rule applies only to active and paused items.
- **`products_list`:** adds `status` and `has_photo`.
- **Prompt:** replace "Only what the supplier asked, one job / community / product per card. Never several at once"
  with: "One card per change, except the batch tools (`products_create_many`, and the `_many` tools), which put every
  item the supplier named in their own message on one card." Also update the `products_create` photo line, add a
  short drafts explanation, and add "never use a menu or pricelist photo as a dish photo; never invent photos". Use
  gender-neutral Hebrew.
- **`normalize()`:** copy it from PR #381's `src/services/catalogSkill/text.ts`
  (`gh pr diff 381 --repo CommunitMe/Admin_system` or `git show origin/claude/agent-catalog-skill:src/services/catalogSkill/text.ts`)
  into `src/services/agent/text.ts`, with a comment saying it's copied pending #381.

## community-proj (`src/Containers/ManagePricelists/Content/ProductsContent/*`, JavaScript)
- **`ProductModal`:**
  - create: no image → primary "Save as draft" plus a hint;
  - editing a draft: "Publish" (primary, disabled until there's an image; sends `publish=true`) and "Save draft";
  - editing active or paused: the image stays required;
  - a draft with gallery images but no main image: "Use as main photo" on a gallery image.
- **`ProductsTable`:**
  - a Draft chip;
  - an "Add a photo to publish" CTA for drafts and for paused items without an image;
  - the kebab menu for a draft has no pause/unpause and has "Add photo and publish".
- **`index.js`:**
  - uses `status`;
  - errors switch on `error.response.data.code` (`IMAGE_REQUIRED`, `DRAFT_CANNOT_PAUSE`) and show a
    translated snackbar (no more `console.error` only);
  - publish shows `FulfillmentReminderDialog` when `fulfillment_missing`;
  - a `draft_count` header line with a filter.
- **`ViewJobModal/ModalContent/EditOrder/index.js`:** the product picker hides `status === 'draft'` items.
- **Text fixes:** `image_too_large` 5MB → 15MB, and the delete success toast.
- **Translations:** `public/locales/{en,he}/supplier-products.json`, gender-neutral Hebrew: טיוטה, שמירה כטיוטה,
  פרסום, הוספת תמונה לפרסום.

## Tests
Admin_system has no jest. Tests use `node:test` + `assert/strict` through ts-node:
- `npm run test:agent` runs `tests/agent/unit/*.test.ts`;
- `npm run test:ai` type-checks `tests/` first.

New API unit tests go in `tests/agent/unit/` (or a new `tests/catalog/unit/` with its own npm script). Fake
`dbUtil` the way the existing tests fake their stores. Type-check with `npx tsc --noEmit -p .`.
