# Project Design Anchors

Last scanned: 2026-05-06

A snapshot of where the design system lives in each frontend project. Use this to find tokens and components fast. The actual code in the repos is the source of truth — this file just points you at it.

## `members/` — member-facing platform

- **Stack:** Next.js 16, React 19, Tailwind v4, shadcn/ui-style primitives
- **Project skill:** `commy-development` — read it first for full conventions (RTL, i18n, gender-inclusive Hebrew, Figma-to-code workflow, testing, mobile+desktop)
- **Design tokens:** `members/src/app/globals.css` inside the `@theme inline { ... }` block. Includes:
  - Semantic palette: `--color-background`, `--color-foreground`, `--color-primary`, `--color-secondary`, `--color-muted`, `--color-destructive`, `--color-accent`, `--color-card`, `--color-popover`, `--color-border`, `--color-ring`, `--color-input`, `--color-sidebar*`, `--color-chart-{1..5}`
  - Brand palette: `--color-ghost-white`, `--color-light-gray`, `--color-silver-gray`, `--color-lavendar-grey`, `--color-lavendar-mist`, `--color-cool-gray`, `--color-cool-gray-2`, `--color-graphite`, `--color-light-green`, `--color-dark-green`, `--color-soft-green`, `--color-dark-teal`, `--color-mint-gray`, `--color-charcoal-blue`, `--color-coral-red`, `--color-verdigris-green`, `--color-ocean-breeze-blue`, `--color-sunset-orange`, `--color-pale-cyan`, `--color-dusty-teal`, `--color-soft-violet-gray`, `--color-mint-white`, `--color-grape-gray`, `--color-golden-coral`, `--color-deep-royal-purple`, `--color-pale-orchid`, `--color-paper-white`, `--color-white-smoke`, `--color-medium-orchid`, `--color-blush-lavender`
  - Screen tokens: `--color-kard-yellow`, `--color-kard-footer`, `--color-kard-bg`, `--shadow-kard-card`, `--shadow-kard-input`
  - Radius scale: `--radius-sm` (6px), `--radius-md` (8px), `--radius-lg` (10px = `--radius`), `--radius-xl` (14px)
  - Fonts: `--font-sans` (Poppins), `--font-mono` (Geist Mono), `--font-hebrew` (Noto Sans Hebrew)
- **Primitive components:** `members/src/components/ui/` — accordion, avatar, badge, button, card, carousel, checkbox, dialog, drawer, dropdown-menu, form, input, label, progress, scroll-area, select, separator, sheet, skeleton, sonner, switch, tabs, textarea, toggle, toggle-group, tooltip
- **Composite components:** `members/src/components/` — AddressMapInput, AvatarStack, CommercialToasts, CustomDatePicker, CustomTooltip, Footer, Header, ImageUploader, Input, Kard, Modals, MyOrders, MyProfile, Notifications, PhoneNumberInput, SingleServiceAccordian
- **Icons:** `members/src/icons-lib/` (custom icon library with theming and sizing)
- **i18n:** `next-intl`, locale files under `members/translations/`
- **File naming:** kebab-case
- **RTL:** primary direction is RTL (Hebrew); LTR for English
- **Dev server:** `cd members && npm run dev` → port 3000

## `community-proj/` — Admins (Community manager + Supplier)

- **Stack:** React 18 (JavaScript, NOT TypeScript), Create React App, MUI 6, i18next, Formik
- **Project skill:** *none yet* — defer to this anchor file and the existing components for conventions
- **Theme & tokens:** `community-proj/src/theme.js` (MUI theme) + `community-proj/src/ThemeWrapper.js` (DirectionProvider for RTL/LTR + emotion CacheProvider)
- **Components:** `community-proj/src/Components/` (PascalCase folders). Lots of `Custom*` wrappers around MUI:
  - Layout: AppLoader, Layouts/, Footer
  - Inputs/forms: InputField, CustomDatePicker, CustomCheckboxLabel, CustomDropdowns, CustomSwitch, MultiLevelDropDown, MultiSelectDropdown
  - Display: CMTable, CustomTable, CommunityCard, MemberCard, MyCommunityCard, InfoCard, AvatarStack, HoverList
  - Navigation/menus: CustomMenu, CustomTabs, CustomFilterTabs, Dropdown, InviteMenuBtn
  - Buttons: GradientButton (and direct MUI `Button` components)
  - Modals: Modals/ (multiple), CustomTooltip, CustomTooltips, CustomAccordion
  - Visual: LinearProgressBar, ImageUploader, AddressMapInput
  - Domain-specific: JobModalsManager, CommunitySuppliersHeaderFilters
- **i18n:** `i18next` (not `next-intl`), config at `community-proj/src/i18n.js`
- **File naming:** PascalCase folders, mixed inside
- **JS, not TS:** type annotations and `.tsx` files do not belong here
- **RTL:** `DirectionProvider` flips direction; verify visual handed-ness manually
- **Dev server:** `cd community-proj && npm start` → port 3001

## `Onboarding/`

- **Stack:** Vite + React 18 (TypeScript), MUI 7 + Tailwind CSS + Radix UI, npm workspaces monorepo (`packages/client`, `packages/server`, `packages/shared`)
- **Project skill:** `onboarding-development`
- **Theme:** `Onboarding/packages/client/src/theme.ts` — MUI `createTheme` with custom breakpoints (`sm: 650`, `md: 1279`, `lg: 1350`, `xl: 1536`), Public Sans font, RTL via `stylis-plugin-rtl` and `prefixer`
- **Components:** `Onboarding/packages/client/src/components/`
- **Routes:** TanStack Router, file-based in `Onboarding/packages/client/src/routes/` — auto-generates `routeTree.gen.ts` (never edit manually)
- **State:** Zustand (`useUserStore`, `useInviteStore`, `useLangStore`)
- **Forms:** React Hook Form + Zod (shared schemas in `packages/shared`)
- **Path aliases:** `@/*` → `./src/*`, `@ob/shared` → `./src/shared`
- **i18n:** custom loader at init in `config.ts` → `window.langs_data`, `window.categories_data`. Translation files at `/public/langs/data.json`. Hook: `const { t, lang } = useTranslate('section')` then `t('key')`.
  - Languages: en (LTR), he (RTL), ru (LTR)
  - Hebrew RTL via `stylis-plugin-rtl` + MUI `direction`
- **Dev server:** `cd Onboarding && npm run dev` → client on 5173, server on 3000

## Shared rules across all three projects

- **Hebrew gender-neutral copy:** male plural ("ברוכים הבאים", not "ברוך הבא") or neutral phrasing ("אותך", "בשבילך"). Never gender-specific singular unless deliberately addressing one gender.
- **Mobile + desktop responsive** by default; verify both at the project's breakpoints.
- **No hardcoded values** for colors / spacing / fonts / radius / shadows. Read from theme/tokens.
- **No raw user-facing strings** in JSX. Always go through the i18n system.
- **Empty, loading, error states** are required for any list/form/async surface.
- **Accessibility minimum bar:** semantic HTML, keyboard nav, focus visible, labels, alt text. The team has an a11y plugin that catches deeper issues.

## When you can't tell which project you're in

If a request is ambiguous about which app it targets ("add a 'remind me later' button" — to which app?), ask. The three apps are distinct; the right answer in one is wrong in another.
