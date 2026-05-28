---
name: frontend-design
description: Universal frontend & design-system guardrails for the CommunitMe / Keilot apps (Members, Admins/Community-manager/Supplier, Onboarding). Use this skill **whenever any UI work is being planned or implemented** — building a screen, adding a component, fixing a layout, writing copy, working on a Figma-to-code task, or running task-creator's design/UX phase. Even when the user doesn't explicitly say "frontend" or "design", invoke this skill if the work involves any visible surface or user-facing behavior. The skill enforces the cardinal rule: **don't invent new design — match the existing system** (colors, spacing, fonts, icons, components, copy patterns), routes to the right project's existing development skill (`commy-development` for members, `onboarding-development` for Onboarding, `community-proj` patterns for Admins), and surfaces design/UX questions back to the requester rather than guessing. Use this skill alongside `task-creator` during planning to make sure design considerations are part of the spec.
---

# Frontend & Design

The job: every time a UI surface is being built or planned, this skill ensures the result matches the **existing design language** of whichever app it lives in (Members, Admins, Onboarding) and respects universal frontend architecture principles. The bar is *consistency over cleverness* — if a pattern already exists in the app, use it; if it doesn't, ask, don't invent.

This skill is the **universal layer**. It does not replace the project-specific dev skills (`commy-development` for Members, `onboarding-development` for Onboarding) — those go deeper on per-project conventions. This skill routes to them and adds cross-project guardrails.

## When this skill applies

Trigger this skill when any of these are happening:

- A new screen, page, modal, drawer, or component is being built.
- An existing UI surface is being changed (layout, copy, behavior, state, validation).
- A design (Figma, screenshot, mockup) is being translated to code.
- A bug fix touches anything visible.
- `task-creator` has reached the UX/design batch or the technical-impact batch and the affected repo is a frontend project.
- The user mentions screens, components, layout, styling, RTL/Hebrew, mobile, accessibility, or design tokens.

If you're in doubt whether the work is "frontend enough" to invoke this — invoke it.

## Two modes

### Planning mode (during task-creator)

Inject design-system thinking into the spec **before** code is written. Add UX/design questions to the relevant batch in the interview. See [Planning checklist](#planning-checklist).

### Execution mode (during implementation)

Apply the cardinal rules and patterns while writing code. Read the project-specific skill first, then this skill's universal principles. See [Execution checklist](#execution-checklist).

## Project routing — read first

Three apps, three design systems. Identify which app you're in before doing anything else.

| App / repo | Stack | Project skill | Design anchor file |
|---|---|---|---|
| `members/` (member-facing platform) | Next.js 16, Tailwind v4, shadcn/ui | `commy-development` | `members/src/app/globals.css` (`@theme inline` block) + `members/src/components/ui/` |
| `community-proj/` (Admins — community manager + supplier) | React 18 (JS), MUI 6, i18next | *(none yet — defer to design anchors)* | `community-proj/src/theme.js` + `community-proj/src/Components/` |
| `Onboarding/` | Vite/React 18 (TS), MUI 7 + Tailwind + Radix | `onboarding-development` | `Onboarding/packages/client/src/theme.ts` + `Onboarding/packages/client/src/components/` |

Read [`references/project-design-anchors.md`](references/project-design-anchors.md) for the detailed snapshot — token names, components folders, naming conventions, fonts.

**Cross-project rule:** these apps look slightly different on purpose (members is consumer-facing, admins is operator-facing, onboarding is pre-platform). Don't try to make them match each other. A button in members is not the same as a button in admins. **Match the app you're in.**

When uncertain about a project-specific convention, defer to that project's skill or its design anchor file.

### Cross-project pattern transfer — what's allowed

There are four kinds of "borrow from another app", and only one of them is allowed by default:

| Kind | Example | Rule |
|---|---|---|
| **Visual** — colors, spacing, fonts, radius | Use `members`' primary blue in `community-proj` | **Not allowed.** Each app's visual language is deliberate. Match what's already in the target app. There is no cross-app visual consistency requirement — a "Save" button in `community-proj` is not supposed to look like the "Save" button in `members`. |
| **UX idea** — how a problem is solved (infinite scroll behavior, empty-state pattern, multi-step form flow) | `members` solves long lists with infinite scroll; reuse the same UX shape in `community-proj` | **Allowed as a fallback only.** First, look for a matching pattern inside the target app. If you find one, use it. Only if the target app has nothing analogous, you may reference another app's UX idea — but reimplement it in the target app's stack and idioms. Do **not** import the visual styling along with the idea. Note the source in the PR: *"Modeled after `members/src/components/ListPage.tsx`; rewritten in MUI."* |
| **Code** — copy a component file from one app to another | Copy `AddressMapInput.tsx` from `members` to `community-proj` | **Not allowed.** The stacks differ (Tailwind vs MUI, TS vs JS, Next vs CRA). Copy-pasted code becomes a maintenance trap. Reimplement in the target stack instead. |
| **Shared package** — extract code into a workspace package | Move shared types into a new `packages/shared` like Onboarding has | **Project-level decision — never made alone.** Only Onboarding currently has `packages/shared`. The other repos don't have a shared workspace at all. Do not propose or implement this without explicit human approval — it's a build/CI/infrastructure change. |

**Default order of search when looking for a pattern:**

1. Existing screens in the **same app** (the right answer 95% of the time).
2. The project's own design anchor file (`references/project-design-anchors.md`).
3. *Only if 1 and 2 turn up nothing:* another app's UX idea, reimplemented in the target stack. Cite the source in the PR.

If even that doesn't yield a clear answer, this is genuinely a new pattern — flag it in the PR's `⚠ Design review needed` block.

## The cardinal rule — don't invent design

The goal is consistency, not creativity. If you find yourself picking a color, a spacing value, a font size, a border radius, a shadow, an animation, or a component shape from imagination, **stop**. Find an existing precedent in the app and match it.

In order of authority:

1. **Figma (when present)** wins on visual decisions — colors, layout, spacing, sizes, copy. If Figma differs from existing components, follow Figma but flag the divergence to the requester (it may be intentional, it may be a designer mistake).
2. **Existing similar screens** in the same app win when there's no Figma. Find the closest analog and match it — same paddings, same typography hierarchy, same button placement, same loading pattern.
3. **The app's design tokens** (colors, spacing, fonts, radius) win on individual values. Never hardcode a hex code, a pixel value, or a font family that isn't a token.
4. **Universal principles** (below) apply across all of the above.

If none of these cover a decision, that's a real gap — surface it to the requester or design (see [When you genuinely can't infer](#when-you-genuinely-cant-infer)).

## Figma workflow

When a ticket has a Figma link:

1. **Try the Figma MCP first** if one is configured in the user's environment. It can fetch nodes, frames, and tokens directly. If you find one, prefer it — it's higher fidelity than screenshots.
2. **Otherwise, ask the user to share screenshots** of the relevant frames. Tell them which frames you need ("the empty state, the populated state, the error state, the mobile view"). Don't guess from the URL alone.
3. **Read the Figma against the existing system.** Same tokens? Same components? If not, that's either an intentional design decision (use the new value) or a designer oversight (existing wins). Surface the discrepancy in your output: *"Figma uses `#3B82F6` for the button but the system primary is `#2563EB` — confirm with design, or treat as a Figma mistake?"*
4. **Pixel-perfect is the bar** unless you find a genuine system conflict. If Figma says 16px gap, ship 16px gap (which should map to a token).

## UX — not just design

Visual fidelity is half the job. UX is the other half. While building or planning a UI surface, hold these UX principles in mind. They're as important as picking the right color.

- **Forgiving paths.** Destructive actions need a confirmation. Mistakes need an undo or a clear recovery. A user clicking "Delete" should be able to think *"oh wait, no"* and back out.
- **Progressive disclosure.** Don't dump every option on the first screen. Show what's needed; reveal more on demand.
- **Cognitive load.** Forms with 12 fields should be split or grouped. Tables with 9 columns need sensible defaults. Decisions should be obvious — if a user has to think hard about which button to press, the labels are wrong.
- **Feedback within 100ms.** Every action — click, submit, drag — needs visible response immediately. Loading states are not optional; absence of feedback feels broken.
- **Empty states are first-class.** "No data" is a UX surface. Tell the user what to do next ("No products yet — Add your first product"), don't just show a blank screen.
- **Error states recover the user.** A failed submit should preserve their input, explain what went wrong in plain language, and offer a retry. Never lose user data.
- **Discoverability over hidden gestures.** Don't rely on the user knowing to swipe / right-click / hover — visible affordances win.
- **Consistency = trust.** The same action in two places should look and behave the same way. If "Delete" is red on one screen and gray on another, that's a bug.
- **Microcopy matters.** "Are you sure?" is bad. "Delete this product? Members with active orders will still see it in their order history." is good — tells the user what will and won't happen.
- **Mobile is not just a smaller desktop.** Tap targets need ≥ 44px. Hover doesn't exist. Modals are heavier on mobile — consider full-screen sheets instead.

When a UX decision is non-trivial and you don't have explicit guidance, **flag it as a question to product/design** rather than guessing. UX choices accumulate — a series of small "I'll just pick this one" decisions adds up to an inconsistent app.

## Universal principles (apply in every project)

These hold regardless of stack — Tailwind, MUI, anything else.

### Reuse before create
Before building anything, scan the project's existing components folder. Look for the closest analog. If a similar component exists:
- **Use it as-is** if it fits.
- **Extend it** (add a variant, a prop) if it almost fits.
- **Create new** only if reuse and extension genuinely don't work — and if you're about to do this, *flag it* (see [When to pause](#when-to-pause-vs-proceed)). Adding a new primitive is a system-level decision.

### Tokens, not hardcoded values
Colors, spacing, typography, radius, shadows — read from the theme/tokens file. Never `color: #abc`, `padding: 13px`, or `fontFamily: "Arial"` in component code. If the value you need isn't in the tokens, that's a design question, not a coding workaround.

### i18n, not hardcoded strings

Every user-facing string goes through the project's i18n system. Never put English or Hebrew text directly in JSX.

**Per-project file shapes** (these differ — don't mix them up):

| Project | System | File shape |
|---|---|---|
| `members/` | `next-intl` | One folder per locale (`translations/en/`, `translations/he/`), many namespace files inside (`common.json`, `header.json`, …). Same file name appears in every locale folder. |
| `community-proj/` | `i18next` | Same pattern: `public/locales/en/`, `public/locales/he/`, namespace files inside. |
| `Onboarding/` | Custom loader | A **single** `public/langs/data.json` file. Every leaf key contains all locales nested: `{ "en": "…", "he": "…", "ru": "…" }`. Don't add separate locale files. |

**Locales required:**
- `members/` and `community-proj/`: **English + Hebrew**.
- `Onboarding/`: **English + Hebrew + Russian**.

**The workflow for any new string:**

1. **Pick (or create) the right namespace file.** Reuse an existing namespace if the string belongs there. New namespace = new file in every locale folder (members / community-proj) or new top-level section in `data.json` (Onboarding). Match the existing key naming style of the file.
2. **Add the key to *every* locale.** No missing keys — the apps fall back ugly when a locale is missing. For `members/` and `community-proj/`, add to both `en/` and `he/`. For `Onboarding/`, add `en`, `he`, and `ru` inside the same key in `data.json`.
3. **Write each language natively.** No placeholders, no leaving English in a Hebrew slot.
   - **English:** native.
   - **Hebrew:** follow the gender-neutral rule (see below). Write it natively — don't punt to TODO.
   - **Russian** (Onboarding only): the team has no native Russian speaker. Claude writes it. Be careful and idiomatic — the goal is text a Russian speaker would not flag as machine-translated.
4. **Flag the new strings in the PR.** All strings get human review at PR time (see the [Translation review block](#the-pr-translation-review-block) below).

**Hebrew gender-neutral rule.**
Use male plural ("ברוכים הבאים", not "ברוך הבא") or neutral phrasing ("אותך", "בשבילך"). Never gender-specific singular unless the surface is deliberately addressing one gender (rare). Write naturally — Hebrew that reads as machine-translated English is a fail even when grammatically correct.

**No `TODO` markers in the JSON values.** Past versions of this skill suggested sentinels like `[TODO_HE]` in the value. Don't. The PR review block is the only tracking — once merged, every string is considered final unless flagged separately.

### The PR translation review block

When a PR adds or changes user-facing strings, include a clearly-marked review block in the PR description:

```markdown
## ⚠ Translation review needed

Please verify the Hebrew (and Russian, where applicable) reads naturally and is gender-neutral.

| Key | EN | HE | RU |
|---|---|---|---|
| `kard.welcomeTitle` | Welcome to your community | ברוכים הבאים לקהילה שלכם | — |
| `kard.welcomeBody` | Start by exploring nearby suppliers | התחילו בגילוי ספקים באזור | — |

(Onboarding example with Russian:)

| Key | EN | HE | RU |
|---|---|---|---|
| `welcome.subtitle` | Choose your role | בחרו את התפקיד שלכם | Выберите вашу роль |

cc @Shoshana-Chaya @yehonatanYifrach
```

The block must include:
- Every new or changed key.
- All locales side-by-side so reviewers can spot bad translations at a glance.
- A mention of `@Shoshana-Chaya` and `@yehonatanYifrach`.

If the PR also has a `⚠ Design review needed` block, keep them as **two separate blocks** — different reviewers may focus on different concerns, and merging them makes both harder to scan.

**No external tooling.** All translation work is direct edits to the JSON files in the repo. There's no Lokalise / Crowdin / spreadsheet — the PR is the workflow.

### RTL/LTR symmetry
Every layout must work in both directions. Hebrew is RTL; English is LTR. Specifically:
- Use logical properties (`margin-inline-start`, `padding-inline-end`) over directional ones (`margin-left`, `padding-right`) where the framework supports it.
- For MUI projects, the `direction` prop and `stylis-plugin-rtl` handle most cases automatically — but verify on visual elements (icons, arrows, alignment).
- For Tailwind, use `rtl:` and `ltr:` modifiers when the layout has handed-ness.
- Icons that have direction (arrows, chevrons) should mirror in RTL.
- Numbers and dates can be tricky — Hebrew text right-aligns but numeric content often stays LTR within RTL flow. Match what's already in the app.

### Mobile + desktop responsive
Default assumption: every UI must work on mobile *and* desktop. Use the project's breakpoints (defined in the theme/tokens file). If a feature is desktop-only or mobile-only, that's an explicit product decision — flag it.

### Empty, loading, error states are required
Every list, table, form, and async surface needs all four:
- **Idle** — the normal state.
- **Loading** — a skeleton or spinner consistent with the project's pattern.
- **Empty** — purposeful, with a next-step CTA where possible.
- **Error** — recoverable, preserves user input.

If a Figma doesn't include these, it's incomplete. Ask, then match the app's existing pattern for similar surfaces.

### Accessibility — minimum bar
The minimum:
- Semantic HTML (`<button>`, `<nav>`, `<main>`, real headings).
- Keyboard navigation works (Tab, Enter, Esc — modals close on Esc).
- Focus is visible and reasonable.
- Form inputs have labels.
- Images have `alt`.

The team uses an accessibility plugin that catches additional issues, so the floor here is "don't write code that fails the basics." Don't over-invest in ARIA gymnastics unless the basics are solved first.

### No new dependencies without justification
Don't `npm install` a new package to solve a problem the existing stack can solve. If a new dep is genuinely needed, flag it — the bundle and the maintenance surface are real costs.

### Animation and motion — use library defaults, don't invent

There is **no formal motion system** across these apps. No duration tokens, no easing tokens, no installed motion library (no framer-motion, react-spring, gsap). The animations users see today come entirely from library defaults — Tailwind / shadcn in `members/`, MUI in `community-proj/` and `Onboarding/`.

The rule:

- **Use library defaults.** Tailwind's `transition-*` / `duration-*` / `ease-*` utilities and shadcn's built-in component animations in `members/`. MUI's built-in `Transition` / `Fade` / `Slide` / default Dialog / Menu / Drawer animations in the others.
- **Do not write custom timings or easings inline.** No `transition: 0.4s ease-in-out` ad hoc, no `style={{ transition: '...' }}`, no custom `@keyframes` — these accumulate into inconsistency fast.
- **Do not install a motion library.** Same dependency rule as elsewhere.

**This is the one exception to the "proceed and flag in PR" pattern.** If a task genuinely requires animation that the library defaults can't provide — a custom keyframe sequence, a non-default duration tier, a coordinated multi-element transition — **stop and ask in chat**. Do not pick a value and flag it in the PR. Motion is a system-level decision the team has not made yet, and ad-hoc choices would drift the system in a way that's hard to claw back.

The exact phrasing when this happens:

> "This task needs an animation that the library defaults don't cover (e.g., [specific need]). The team hasn't defined a motion system, so I can't pick durations/easings on my own. Please decide: [options]. I'll wait."

## Planning checklist

When `task-creator` is in the design/UX batch or the tech-impact batch for a frontend task, surface these questions to the requester. Don't ask all of them — ask the ones the request hasn't already answered.

- **Surface:** new screen, or change to an existing one? If new, where in the IA does it live?
- **Mockups:** Figma link? If yes, are all states present (idle / loading / empty / error / mobile)? If no, who's producing them?
- **Reuse vs. new component:** is there an existing component close to what's needed? (Worth a look before promising to "build a new X".)
- **Mobile:** required, or desktop-only? Default to required.
- **RTL/LTR:** any direction-sensitive elements (icons, alignment, mixed numeric+text content)?
- **Locales:** Hebrew + English at minimum. Other locales for this surface?
- **Empty state:** what does the user see when there's no data, and what do we want them to do?
- **Error states:** what failure modes does this surface have, and what does recovery look like?
- **Copy:** is microcopy provided, or does this need product/design to write it?
- **Accessibility:** any specific a11y requirements beyond the floor (screen reader flow, keyboard shortcuts)?
- **Design system delta:** is anything in the design new to the system (a new color, a new component, a new token)? If yes, this is a system-level decision and needs explicit approval.

## Execution checklist

Before you submit / open a PR:

- [ ] Read the project-specific skill (`commy-development`, `onboarding-development`, etc.) and followed its conventions.
- [ ] Ran the discovery checklist (list folder → grep keywords → read closest matches) before creating any new component.
- [ ] Re-used existing components where possible. New components and non-trivial extensions are flagged in the PR's `⚠ Design review needed` block with `@Shoshana-Chaya @yehonatanYifrach` mentions.
- [ ] If a component was created, deleted, or meaningfully changed, the `references/component-registry.md` file was updated in the same PR (once that file exists).
- [ ] All values come from tokens (colors, spacing, fonts, radius). No hardcoded hex codes or px values.
- [ ] All user-facing strings go through i18n. No raw English/Hebrew in JSX.
- [ ] Every new key is present in every required locale (en + he for `members`/`community-proj`; en + he + ru for `Onboarding`). No missing locales, no TODO sentinels in values.
- [ ] Hebrew copy is gender-neutral (male plural or neutral phrasing) and reads naturally.
- [ ] If strings were added or changed, the PR includes a `⚠ Translation review needed` block listing every key with all locales side-by-side and `@Shoshana-Chaya @yehonatanYifrach` mentioned.
- [ ] Layout works in both LTR and RTL. Handed elements (icons, alignment) verified.
- [ ] Layout works on mobile and desktop at the project's breakpoints.
- [ ] Empty, loading, and error states implemented.
- [ ] Keyboard navigation works (Tab, Enter, Esc).
- [ ] Focus states are visible.
- [ ] Form inputs have labels; images have `alt`.
- [ ] No new dependencies added without flagging.
- [ ] Visually inspected the result (see below).

## Visual verification

When code-reading isn't enough — when you're uncertain whether the spacing, color, RTL behavior, or interaction actually feels right — **run the dev server and look**.

Each project has its own dev command (see the project skill for specifics: typically `npm run dev`). Yes, this adds 30-60 seconds. It's worth it. The model has historically shipped UI that *compiled* but looked wrong — running the app catches that.

Specifically run the app when:
- You've added or changed a layout.
- You're translating Figma to code and need to compare visually.
- You're working with RTL behavior.
- You're working on a responsive change.
- You created or modified a component primitive.

Do **not** skip this for "small" UI changes — small UI changes are often the ones that look subtly wrong.

## Mock data — for prototyping and FE-first work

Sometimes the frontend ships before the backend exists. The two main cases:

- **Frontend-first tickets** (`task-creator` FE-first mode) — the deliverable is a working interactive demo with mock data, reviewed by PM + design + FE dev before backend work begins.
- **Prototyping during normal development** — the backend isn't ready yet but you want to start the frontend to unblock design review or to lock down the UX shape.

In both cases, the mock-data approach matters because it determines how cleanly the frontend swaps to real data later. A sloppy mock layer becomes a real refactor when the backend lands.

### Default approach

Lean on these defaults unless the project clearly does something else. Match what's already in the project if there's an existing pattern.

- **Small surfaces** (a single component, a copy edit, a one-screen change): inline hardcoded mock values in the component are fine. No separate file. Don't over-engineer.
- **Lists / typed entities / anything that will eventually fetch from an API**: a typed mock-data file at the project's `__mocks__` / `mocks` / `fixtures` location (match the project's existing convention; default to `src/__mocks__/<feature>/data.ts`), consumed via a **mock hook that mimics the eventual real-API shape**. Example:
  ```ts
  // src/__mocks__/suppliers/data.ts
  export const mockSuppliers: Supplier[] = [ /* ... */ ];

  // src/hooks/useSuppliers.ts — mock impl
  export function useSuppliers() {
    return { data: mockSuppliers, isLoading: false, error: null };
  }
  ```
  Why this shape: when the backend lands, only the hook's implementation changes — swap to a real React Query / SWR / `fetch` call returning the same shape. Component code does not change. This is the whole point.
- **Forms that "submit"** in the demo: simulate success by updating local state. Show success / empty / error states using the same mock layer (e.g., a `submitMock` that returns a resolved Promise, or a rejected one if you want to demo the error state).
- **Multi-step flows**: drive step state from local state or URL params. Persist nothing to a backend.
- **Async-feeling realism**: where the user would normally wait on the backend (initial list load, submit, search), add a *small* simulated delay (200–500ms) so loading states are testable. Don't add delays just to feel "more real" — only where loading UI matters for the demo.

### What to flag in the PR

When mock data is in play, the PR description must include:

- **Mock layer location** — where the mock data and hooks live, so the future "swap to real backend" PR knows what to change.
- **Expected real-API shape** — the data shape the mock matches, so backend can build to it.
- **What's mocked vs. what's real** — be explicit. e.g., *"List fetch is mocked; auth and routing are real."*

This is what makes the eventual backend integration cheap rather than expensive.

### Naming the approach in a FE-first ticket

`task-creator` in FE-first mode includes a **Mock-data approach** line in the FE-First scope block. Fill that line with the specific approach you've picked for *this* feature — not the abstract defaults from this section. Example: *"Mock list at `src/__mocks__/suppliers/data.ts` with 12 sample suppliers; `useSuppliers()` hook returning `{ data, isLoading: false, error: null }`; submit simulated with 300ms delay; error state demoed via a query-param toggle."*

The frontend dev reviews and amends this at graduation sign-off — that's their call. The skill's job is to propose something specific, not to ask the requester (who shouldn't be deciding mock-layer architecture).

## Component creation — the decision flow

Three actions, three rules. This applies to every UI work session.

| Action | Rule |
|---|---|
| **Use** an existing component as-is | Always proceed. No flag needed. |
| **Extend** an existing component (additive only) | Proceed. Note the extension in the PR description. |
| **Create** a new component | Proceed with best judgment, then flag prominently in the PR. |

"Additive only" means: a new prop, a new variant, a new optional behavior. Anything that **changes** an existing prop's behavior, alters layout/spacing of existing usages, or removes/renames an export is **not** additive — treat it as creating new.

### Mandatory discovery before creating

Before creating any new component, complete all three steps:

1. **List the project's components folder.** Read every name. Locations: `members/src/components/ui/` and `members/src/components/`, `community-proj/src/Components/`, `Onboarding/packages/client/src/components/` and `Onboarding/packages/client/src/components/ui/`.
2. **Grep for functional keywords.** If you need a table, search `table`, `grid`, `list`. If a menu, search `menu`, `dropdown`, `select`. If a card, search `card`, `tile`, `item`. This catches components that don't follow the naming convention.
3. **Read the source of the 1–2 closest matches.** Confirm they genuinely don't fit — names can mislead.

Only after all three steps return no match, create.

### Project-specific note: `community-proj/`

Creating a new `Custom*` wrapper around an MUI component is the established pattern in `community-proj/`. It's daily work — proceed without flagging, as long as the discovery checklist above passed (don't recreate `CustomTable` if it exists). Flag in the PR only when the new wrapper introduces visually novel behavior or a new token.

### The PR flag

Whenever Claude makes any of the decisions listed in [What requires the flag](#what-requires-the-flag), the PR description must include a clearly-marked review block:

```markdown
## ⚠ Design review needed

**What I did:** Created a new primitive `IconButton` because the existing `Button` couldn't render an icon-only state without breaking padding.

**Alternative considered:** Extend `Button` with an `iconOnly` prop. I chose to create new because the spacing logic diverges enough that branching inside `Button` would be messier.

**Please review:** @Shoshana-Chaya @yehonatanYifrach
```

The block must include:
- *What* was created or decided.
- *Why* this choice over the obvious alternative.
- A mention of `@Shoshana-Chaya` and `@yehonatanYifrach` (Erez may also approve, but is not mentioned by default).

If multiple decisions were made in one PR, list them all in one block — don't bury them in the diff.

### What requires the flag

Surface a `⚠ Design review needed` block in the PR for any of these:

- A **new component** was created (any file added to a `components/` or `Components/` tree, including new `Custom*` wrappers in `community-proj/` *only* when they introduce visually novel behavior or a new token).
- A **non-trivial extension** of an existing component (anything beyond an additive prop or variant).
- A **new design token** — color, spacing, font, radius, shadow.
- The codebase has **two disagreeing patterns** for the same problem (e.g., some forms validate inline, others on submit) and Claude picked one. List both patterns and the rationale.
- A **composite that introduces a visually new approach** not seen in the app (e.g., a "Stepper" pattern when the app has no stepper).
- A **new dependency** was added.
- A **mobile breakpoint behavior** had to be inferred without a Figma reference.
- An **animation or transition** not already in the system was added.

Default test: *would a designer or PM be surprised by what I built?* If yes, flag it.

Approvers (any one is sufficient): **Yonatan**, **Shoshana-Haya**, or **Erez**.

### Don't pause execution

This skill used to say "pause and ask" in several places. That has been replaced. **Do not pause mid-task to ask design questions.** Make the best judgment call, complete the task, and flag the decision in the PR. Reviewers will catch and correct anything wrong before merge.

There are two explicit exceptions where Claude *must* pause and ask in chat (not flag in PR):

1. **Animation/motion that needs custom values.** See [Animation and motion](#animation-and-motion--use-library-defaults-dont-invent). The team has not defined a motion system, so any custom timing/easing is a system-level decision that must be made before the PR is opened.
2. **The request is fundamentally ambiguous.** E.g., "I literally cannot tell which of two completely different screens is being asked for." Ask before making the wrong thing.

Everything else: proceed and flag.

### Component registry

The full inventory of components across all three projects lives in [`references/component-registry.md`](references/component-registry.md). Each component has a one-line description; legacy components are listed under a "do-not-reference" section per project.

**Read the registry first** when running the discovery checklist. The registry is the canonical fast scan; only fall back to listing the folder directly if you suspect drift (e.g., the registry's "last scanned" date is stale relative to recent commits).

**When Claude creates, deletes, or meaningfully changes a component, it must update the registry in the same PR.** The execution checklist enforces this. The post-implementation `upgrader` agent verifies it.

What counts as "meaningfully change":
- Added or removed exports.
- Changed the component's purpose.
- Marked or unmarked as legacy.
- Non-trivial API change (new required prop, renamed prop, behavior shift).

Cosmetic changes don't require a registry update.

## TBD — for the dev team to fill in

This skill has open questions that should be answered when Yonatan / Shoshana-Haya pair on it. Until then, the model should follow the conservative defaults written above, and *flag* the question when it hits one of these areas.

- **"Don't follow this" patterns beyond components.** The component registry now marks legacy *components*, but there may be folder-level or pattern-level legacy zones (entire screens / API patterns / older state-management approaches) worth flagging. Until catalogued, flag in the PR if a piece of code looks legacy and you're unsure whether to follow it.
The model should **not** invent answers in these areas. When stuck, ask the requester or flag it explicitly.

## Why this skill exists

Without this skill, every UI task becomes a chance for the model to invent something — pick a color, a spacing, a layout — that drifts the system. Three apps × dozens of contributors × many small decisions = visual entropy. The skill makes consistency the default and creativity the explicit exception, which is the right tradeoff for a small team shipping fast.

Match the system. Ask when you can't. Don't invent.
