# Component Registry

Last scanned: 2026-05-08

A one-line description of every UI component across the three frontend projects, plus markers for components that are legacy and should not be referenced in new code.

## How to use this file

**Before creating any new component**, do the [discovery checklist](../SKILL.md#mandatory-discovery-before-creating):

1. Scan this registry for an existing component with a matching purpose.
2. Grep the codebase for functional keywords (`table`, `menu`, `modal`, etc.) to catch components that don't follow naming conventions or are missing from this list.
3. Read the source of the 1–2 closest matches to confirm they don't fit.

If a component exists, **use or extend it**. Only create new if the discovery turned up nothing.

## How to keep this file updated

**Whenever you create, delete, or meaningfully change a component, update this file in the same PR.** This is enforced by the execution checklist in `SKILL.md` and verified by the post-implementation `upgrader` agent.

What counts as "meaningfully change":
- Added or removed exports.
- Changed the component's purpose (e.g., `Modal` becomes `Drawer`).
- Marked or unmarked as legacy.
- A non-trivial API change (new required prop, renamed prop, behavior shift).

Cosmetic changes (refactors, typo fixes, internal helpers) don't require a registry update.

---

## `members/` — Member-facing platform

Stack: Next.js 16, React 19, Tailwind v4, shadcn/ui-style primitives. Project skill: `commy-development`.

### Primitives — `src/components/ui/`

- **`accordion.tsx`** — Accordion primitive. Root with Item, Trigger, Content subcomponents.
- **`avatar.tsx`** — Avatar primitive. Image, Fallback subcomponents; default size 8x8.
- **`badge.tsx`** — Badge primitive. Variants: default | secondary | destructive | outline.
- **`button.tsx`** — Button primitive. Variants: default | destructive | outline | secondary | ghost | link. Sizes: xs | sm | md | lg | default | icon.
- **`card.tsx`** — Card container primitive. Header, Footer, Title, Description subcomponents.
- **`carousel.tsx`** — Carousel primitive. Embla-based with prev/next controls and API context.
- **`checkbox.tsx`** — Checkbox primitive. Radix-based with check icon indicator.
- **`dialog.tsx`** — Dialog primitive. Root, Trigger, Portal, Close, Content subcomponents.
- **`drawer.tsx`** — Drawer primitive. Vaul-based with custom container support.
- **`dropdown-menu.tsx`** — Dropdown menu primitive. Radix-based with check/chevron indicators.
- **`form.tsx`** — React Hook Form integration. FormProvider, Field, Item, Label, Control, Message.
- **`input.tsx`** — Text input with start/end adornments, wrapper styling, icon click handler.
- **`label.tsx`** — Label primitive. Radix-based with disabled state support.
- **`progress.tsx`** — Progress bar primitive. Radix-based with RTL locale support (Hebrew).
- **`scroll-area.tsx`** — Scroll area primitive. Radix-based with custom scrollbar styling.
- **`select.tsx`** — Select primitive. Root, Group, Trigger, Content, Item subcomponents.
- **`separator.tsx`** — Separator primitive. Horizontal/vertical orientation support.
- **`sheet.tsx`** — Sheet primitive. Side panel using Radix Dialog with close button.
- **`skeleton.tsx`** — Skeleton loader. Pulse animation for loading states.
- **`sonner.tsx`** — Sonner toaster integration. Theme-aware with CSS var customization.
- **`switch.tsx`** — Toggle switch primitive. Radix-based with checked/unchecked states.
- **`tabs.tsx`** — Tabs primitive. Variants: default | outline | pills with size control.
- **`textarea.tsx`** — Textarea primitive. Focus/disabled states, resize-none layout.
- **`toggle-group.tsx`** — Toggle group primitive. Radix-based with shared variant context.
- **`toggle.tsx`** — Toggle button primitive. Variants: default | outline. Sizes: sm | default | lg.
- **`tooltip.tsx`** — Tooltip primitive. Radix-based with provider and delayed trigger.

### Composites — `src/components/`

- **`AddressMapInput/`** — Google Maps autocomplete. Address parsing, place ID, city/street/neighborhood extraction.
- **`AvatarStack.tsx`** — Stacked avatar display. Shows up to N avatars with count badge, community manager trophy.
- **`CommercialToasts/CustomToast.tsx`** — Toast with avatar, progress bar, action link. Dismissible after duration.
- **`CustomDatePicker/`** — Date picker with custom header. Month/year selects, locale support, max date prop.
- **`CustomTooltip/`** — Custom tooltip wrapper. Dark mode, content className, manual open control.
- **`Footer.tsx`** — App footer. Links: terms, privacy, copyright text with locale support.
- **`Header/`** — App header. Logo, community dropdown, language switch, notifications, profile menu (plus internal helpers).
- **`ImageUploader/`** — File upload with preview. Size/type validation, remove button, error handling, initial/temp state.
- **`Input/NumericInput.tsx`** — Numeric-only input. Allows digits and delimiters (.,), filters duplicates.
- **`Kard/KardButton.tsx`** — Wallet payment button. Opens Kard modal for payment flow (plus KardModal, KardCardLayout, KardCreditCardVisual).
- **`Modals/MembersOrderModal/`** — Order modal dialog. URL-driven state (plus nested service-detail / payment-info components).
- **`MyProfile/`** — Profile drawer. Tabs: profile (enabled), orders/card/coins (disabled). Fetches user profile data.
- **`Notifications/`** — Notifications list. Socket-integrated, dynamic routing (plus NotificationsTrigger, NotificationItem, ContactInfoPopover).
- **`PhoneNumberInput/`** — Phone input with country prefix. React-phone-input-2, country data, form integration.
- **`SingleServiceAccordian/`** — Service accordion item. Price, currency, unit display. Locale-aware (Hebrew RTL).

### Legacy / do-not-reference

*None detected.*

---

## `community-proj/` — Admins (Community Manager + Supplier)

Stack: React 18 (JavaScript), Create React App, MUI 6, i18next, Formik. No project skill yet — defer to this registry and existing components.

### Components — `src/Components/`

- **`AddressMapInput/`** — Address picker with Google Maps autocomplete and place ID lookup. Used in profile and location forms.
- **`AppLoader/`** — Full-screen circular progress loader with custom branding colors.
- **`CommunityCard/`** — Community display card with location, manager info, tags, and join/cancel actions.
- **`CommunitySuppliersHeaderFilters/`** — Header filters for supplier lists: search, category/location dropdowns, and mobile filter drawer.
- **`CustomAccordion/`** — MUI Accordion wrapper with summary actions, avatar support, and detail rows.
- **`CustomCheckboxLabel/`** — Custom-styled checkbox with label, custom icons, and disabled state.
- **`CustomDatePicker/`** — React DatePicker with custom year/month selectors and max date validation.
- **`CustomDropdowns/MultiSelectDropdown/`** — Multi-select dropdown with checkbox list, no max option selection.
- **`CustomDropdowns/SingleSelectDropdown/`** — Single-select dropdown with optional avatars and helper text.
- **`CustomFilterTabs/`** — Scrollable tab filter component with underline indicator and soon badges.
- **`CustomMenu/`** — Menu button component with icon and dynamic menu items.
- **`CustomSwitch/`** — iOS-style toggle switch with custom colors and disabled state.
- **`CustomTable/`** — **Canonical generic table** for `community-proj/`. Prop-driven (`columns` + `data`), 184 lines, sticky headers, horizontal scroll detection, RTL-friendly. Use this for new tables. Used by `MyCommunityCard`, `MyMembersCard`, `MySuppliersCard`, `MyJobsCard`.
- **`CustomTabs/`** — Tabbed content switcher with customizable styles and layout options.
- **`CustomTooltip/`** — Tooltip with optional Swiper carousel for more info, mobile-aware display.
- **`CustomTooltips/CustomTooltipBox/`** — Info icon with toggle tooltip showing multiple styled sections.
- **`Dropdown/`** — Basic Select dropdown with hover menu behavior and option rendering.
- **`GradientButton/`** — Button with gradient background and navigation integration.
- **`HoverList/`** — Popover list that appears on hover with custom styling.
- **`ImageUploader/`** — File uploader for images (JPEG/PNG/WebP) with preview, validation, and remove button.
- **`InfoCard/`** — Small info card with icon, heading, subheading, and responsive sizing.
- **`InputField/`** — MUI TextField wrapper with phone/tel number validation and end adornment support.
- **`InviteMenuBtn/`** — Invite button with dropdown menu for invite options (copy, paste actions).
- **`JobModalsManager/`** — Custom hook managing multiple job-related modal states (view, decline, cancel).
- **`LinearProgressBar/`** — Linear progress bar with optional percentage text display.
- **`MemberCard/`** — Clickable member card displaying heading, subheading, active state.
- **`MultiLevelDropDown/`** — Multi-level dropdown with sub-menu support for hierarchical options.
- **`MultiSelectDropdown/`** — Multi-select with checkbox, search, and MenuItem rendering.
- **`MyCommunityCard/`** — Expandable community card with table/accordion toggle, sort controls, and member list.
- **`MyMembersCard/`** — Member list with table/accordion toggle, sort, actions menu, and skeleton loading.
- **`MyProfile/`** — Full profile drawer with profile edit, order history, filter tabs, account deletion.
- **`MySuppliersCard/`** — Supplier list with table/accordion toggle, sort, status, tooltip, and action menu.
- **`MySuppliersTable/`** — Supplier table with sort labels, avatars, and action menu (not drawer-based).
- **`NotificaitonItem/`** — Notification item with formatted date, avatar, action buttons (note: typo in folder name).
- **`PhoneNumberInput/`** — Phone input with country flag selector and custom styling.
- **`RequestServiceAccordian/`** — Service request accordion with pricing table and disabled state handling.
- **`SearchField/`** — Search input with icon, autofill disabled, placeholder support.
- **`SingleSelectAutocomplete/`** — Autocomplete dropdown with search, custom styling, error handling.
- **`SingleServiceAccordian/`** — Service accordion with summary content and detail expansion.
- **`Skeletons/SkeletonAccordion/`** — Skeleton loader for accordion with configurable row count.
- **`Skeletons/SkeletonTable/`** — Skeleton loader for table with configurable columns and rows.
- **`SnackBar/`** — Context-based snackbar with custom severity, duration, and show/hide control.
- **`SupplierCards/`** — Supplier profile card with image, name, location, description, and action button.
- **`SupplierTable/`** — Supplier table with sort labels, avatars, menu, and sticky headers.
- **`Typography/`** — Custom typography component with type variants (heading, label, bold, etc.).
- **`UserDropDown/`** — User profile dropdown menu button with avatar and responsive sizing.

### Modals — `src/Components/Modals/`

- **`AddLinkModal/`** — Dialog for adding online meeting link to job (built on AlertModal).
- **`AlertModal/`** — Generic alert/confirmation dialog with title, body, and action button.
- **`BankDetailsMissingModal/`** — AlertModal variant notifying user to add bank details for payments.
- **`CancelOfferModel/`** — Dialog for canceling supplier/offer with custom heading, description, loader.
- **`DeleteAccountDialog/`** — Account deletion confirmation with warning, checkbox, and order check.
- **`DeleteDeactivatePricelistDialog/`** — Pricelist deletion dialog with active order count and confirmation.
- **`GenericSwiperDialog/`** — Two-column dialog with Swiper carousel, header, footer actions, tooltips.
- **`InviteModal/`** — Success modal showing invite details (email, phone, WhatsApp) with copy actions.
- **`JoinCommunityModel/`** — Modal for joining community with discount input, terms checkbox, validation.
- **`MaximumCapacityReachedModal/`** — Alert modal informing user community has reached max capacity.
- **`NeedPricelistModal/`** — Alert modal requesting pricelist creation before proceeding.
- **`NewPricelistCreatedModal/`** — Success modal confirming pricelist creation.
- **`SupplierJoinOffersModal/`** — Multi-section modal for supplier community offers: left/right sections, actions.
- **`TermsDialog/`** — Terms & conditions dialog with Markdown rendering and optional accept/reject actions.

### Legacy / do-not-reference

- **`CMTable/`** — Monolithic 1,559-line table hardcoded for community-management (Requests / Invites / Members). Contains dead/commented-out code, inline hardcoded English strings (no i18n), no RTL handling. Still imported by `Containers/CommunityManagement/` so it ships in production, but **do not model new code after this**. For new tables, use `CustomTable/` (the canonical generic table). When `CommunityManagement/` is touched substantially, migrate it to `CustomTable/`.
- **`NewCMTable/`** — Unused. Appears to be an abandoned attempt to replace `CMTable/`. Not imported anywhere. Do not reference in new code; consider deleting in a cleanup PR.

---

## `Onboarding/` — Pre-platform onboarding flow

Stack: Vite + React 18 (TS), MUI 7 + Tailwind + Radix, npm workspaces monorepo (`packages/client`, `packages/server`, `packages/shared`). Project skill: `onboarding-development`.

### Primitives — `packages/client/src/components/ui/`

- **`AddressInput.tsx`** — Google Maps autocomplete address input with district mapping and coordinates.
- **`button.tsx`** — Polymorphic button primitive (button or Link with loading state & spinner).
- **`CardsInput.tsx`** — Radio-button-styled card selector with label, icon, and description.
- **`CheckboxInput.tsx`** — Accessible custom checkbox with error state.
- **`customEnable.tsx`** — Accessibility toolbar toggle button (ESC shortcut).
- **`CustomDatePicker.tsx`** — Date picker with month/year selects and keyboard navigation via Radix + react-datepicker.
- **`dropdown-input.tsx`** — Styled dropdown with icon support via react-dropdown library.
- **`form.tsx`** — Composable form context (shadcn pattern) with FormField, FormItem, FormMessage.
- **`Icon.tsx`** — Icon renderer from static SVG/PNG files in `/assets/icons/`.
- **`input-otp.tsx`** — OTP input slots (Radix-based input-otp) with caret animation.
- **`input.tsx`** — Text/email/password/date input with password visibility toggle & date picker.
- **`label.tsx`** — Label component (Radix + CVA variants).
- **`Loader.tsx`** — Inline loader with spinner and "Loading..." text.
- **`multi-select-autocomplete.tsx`** — MUI Autocomplete with checkbox, multi-select, chip display.
- **`multi-select-dropdown.tsx`** — MUI Menu-based multi-select with groups, images, chips.
- **`PhoneInput.tsx`** — Country code dropdown + phone number input with validation.
- **`SelectLang.tsx`** — Language picker dropdown with flags (uses global `window.langs`).
- **`support.tsx`** — WhatsApp support link button.
- **`TagsInput.tsx`** — Tags input with autocomplete suggestions and keyboard navigation.

### Composites — `packages/client/src/components/`

- **`BgPageWarper.tsx`** — Full-page wrapper with background, header, nav, TOS info, back/cancel buttons.
- **`changedBg.tsx`** — Background image + overlay text + actions (lang, support, accessibility).
- **`commyInvitationCard.tsx`** — Card showing "You've been invited to [community name]".
- **`devHeader.tsx`** — Empty dev header placeholder.
- **`FullScreenLoader.tsx`** — Zustand-driven full-screen loader overlay toggle.
- **`socials.tsx`** — Google OAuth button with loading spinner.
- **`spinner.tsx`** — Spinning icon (FaSpinner from react-icons).

### Forms — `packages/client/src/components/forms/`

#### Root-level helpers

- **`devtools.tsx`** — Dev buttons to validate/reset forms (validation & state testing).
- **`DynamicForm.tsx`** — Generic form renderer supporting text, email, password, checkbox, dropdown, address, tags, date, phone fields.
- **`OtpFrom.tsx`** — Reusable OTP form with resend countdown, validation, API mutation. (Note: typo in filename — should be `OtpForm.tsx`.)
- **`phoneOtpForm.tsx`** — Phone + OTP input form with country code, validation, error handling.

#### `signup/`

- **`addPhone.tsx`** — Wrapper for phone OTP entry during signup.
- **`basic_details.tsx`** — First name, last name, email, birthday (all required).
- **`commy-tos.tsx`** — Terms of Service acceptance form for community.
- **`location.tsx`** — Address input with optional postal code, district, coordinates.
- **`otpFormWrapper.tsx`** — Wrapper for OTP verification form during signup.
- **`password-creation.tsx`** — Password + repeat password with validation (letters, numbers, special chars).
- **`user-creation.tsx`** — Phone + OTP form (initial signup entry point).

#### `signin/`

- **`signin.tsx`** — Phone/email + password input form with optional OTP fallback.

#### `reset_password/`

- **`ChangePasswordForm.tsx`** — New password + repeat password with OTP token.
- **`SendOtpForm.tsx`** — Email input to trigger password reset OTP.
- **`VerifyOtpForm.tsx`** — OTP verification for password reset flow.

#### `create_community/`

- **`CountryForm.tsx`** — Country, language, currency dropdowns (Israel/Hebrew/Shekel hardcoded).
- **`LocationForm.tsx`** — Address + location radius selector (none/country/state/city/neighborhood/street/building).
- **`NameForm.tsx`** — Community name input (required).
- **`TagsForm.tsx`** — Multi-select tags from API-fetched predefined list.

#### `add_supplier/`

- **`BasicDetails.tsx`** — Business name, type, business number, validation checkbox, categories.
- **`Location.tsx`** — Address, postal code (optional with validation), district, coordinates.
- **`TosSupplier.tsx`** — Terms of Service acceptance form for suppliers.

### Legacy / do-not-reference

- **`forms/create_commyOLD/CommyCreation.tsx`** — Old community-creation form (name + type only). Replaced by `forms/create_community/`.
- **`forms/signin/OneTimePasswordOLD.tsx`** — Old OTP-only signin flow. Replaced by `phoneOtpForm.tsx` + `signin.tsx`.

---

## Maintenance notes

- **`members/` Header is currently a folder with internal helpers** — list only the entry component here, not every internal piece.
- **`community-proj/` has a typo'd folder name** (`NotificaitonItem/`). Don't fix without coordination — many imports may reference it.
- **`Onboarding/` has typo'd file `OtpFrom.tsx`** — same caution.
- This file is a snapshot. The actual code is the source of truth. If you spot drift, open a PR that updates this file alongside the underlying change.
