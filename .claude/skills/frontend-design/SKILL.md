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

When uncertain about a project-specific convention, defer to that project's skill or its design anchor file. Don't import patterns across projects unless the user explicitly asks.

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
Every user-facing string goes through the project's i18n system (`next-intl` for members, `i18next` for community-proj, the Onboarding loader for Onboarding). Never put English or Hebrew text directly in JSX. Add the key to all locale files.

For new strings:
- Add the English first.
- For Hebrew: follow the **gender-neutral rule**. Use male plural ("ברוכים הבאים" not "ברוך הבא") or neutral phrasing ("אותך", "בשבילך"). If you're not confident the Hebrew is gender-neutral and natural, leave a `// TODO: HE translation review` and ask the user.
- For other locales (ru, etc.) — add the English value as a placeholder if no translator is available, with a TODO marker.

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
- [ ] Re-used existing components where possible. Did not create new primitives unless flagged and approved.
- [ ] All values come from tokens (colors, spacing, fonts, radius). No hardcoded hex codes or px values.
- [ ] All user-facing strings go through i18n. No raw English/Hebrew in JSX.
- [ ] Hebrew copy is gender-neutral (male plural or neutral phrasing).
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

## When patterns disagree across screens

If half the screens in the app do X and half do Y (e.g., some forms have inline validation, some have submit-time validation), do not pick one and proceed. **This is a design/product decision and needs to be made deliberately.**

Surface it as a question:
> "I see two patterns for this in the codebase — [pattern A on screens X, Y] and [pattern B on screens Z, W]. Which is the intended direction? This affects UX consistency."

Add it to the ticket's `⚠ Open questions blocking execution` if you're in planning mode. Pause and ask if you're in execution mode and the choice is non-trivial.

## When you genuinely can't infer

For *small* changes (adjust a copy string, add a checkbox to an existing form, fix a spacing bug): if you can confidently pattern-match from existing screens, **proceed and flag your inference in the PR description**: *"No Figma; I matched the spacing and font from the [adjacent screen]. Design please verify."*

For *large* changes (new screen, new component primitive, new visual surface): **pause and ask**. Do not invent a design.

The line between small and large isn't bright, but the test is: *would a designer be surprised by what I built?* If yes, it's large. If no, it's small.

## When to pause vs proceed

| Situation | Action |
|---|---|
| Existing component fits → use it | Proceed |
| Existing component almost fits, small extension | Proceed, note the extension in PR |
| New variant of an existing component | Pause, ask product/design |
| New component primitive (e.g., new "Stepper") | Pause, ask product/design |
| New design token (color, spacing, font) | Pause, ask design — this is a system decision |
| Existing patterns disagree | Pause, ask — see [When patterns disagree](#when-patterns-disagree-across-screens) |
| Small copy or behavior change in an existing surface | Proceed |
| Mobile breakpoint behavior unclear from existing screens | Pause, ask |
| Animation / transition not in the system | Pause, ask — animation is part of the system |

## TBD — for the dev team to fill in

This skill has open questions that should be answered when Yonatan / Shoshana-Haya pair on it. Until then, the model should follow the conservative defaults written above, and *flag* the question when it hits one of these areas.

- **Component creation threshold.** When can the model create a new primitive vs. extend vs. ask? What's the formal threshold? Is there a registry of approved-to-create vs. ask-first?
- **"Don't follow this" patterns.** Are there parts of the codebase that are legacy / known-not-the-pattern-we-want? Folders, components, or files to route around?
- **Translation workflow.** New strings — add to all locale files with English placeholder + TODO? Use a translation queue/tool? Mark with a key prefix? What's the process for getting Hebrew + other locales translated?
- **Cross-project pattern transfer.** If a great pattern exists in one app, can it be ported to another (different stack), or are projects strictly siloed?
- **Animation / motion.** Is there a motion system (durations, easings, allowed transitions)? Or is it ad hoc per component?

The model should **not** invent answers in these areas. When stuck, ask the requester or flag it explicitly.

## Why this skill exists

Without this skill, every UI task becomes a chance for the model to invent something — pick a color, a spacing, a layout — that drifts the system. Three apps × dozens of contributors × many small decisions = visual entropy. The skill makes consistency the default and creativity the explicit exception, which is the right tradeoff for a small team shipping fast.

Match the system. Ask when you can't. Don't invent.
