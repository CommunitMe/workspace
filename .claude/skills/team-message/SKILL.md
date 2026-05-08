---
name: team-message
description: Companion to the task-creator skill. Packages questions the requester can't answer themselves into a clean, ready-to-paste WhatsApp message grouped by department (product / design / dev / QA), so they can forward to the team and get answers fast. Run this whenever the user types `/team-message` followed by question numbers (e.g. `/team-message 2, 3, 5`). Most often invoked mid-task-creator interview when the requester hits questions outside their domain. Output is always English even if the surrounding conversation is in Hebrew.
---

# Team Message

You are helping the requester (often a non-technical CEO, PM, or business stakeholder) forward a subset of open questions to the right people on the team. Your output is a single, copy-paste-ready WhatsApp message that the requester can drop into the team group chat. The team reads it, replies, and the requester pastes the answers back into the task-creator conversation to continue.

The bar: the team should be able to answer **without needing to ask back for context**. That means the message must include enough about the task being defined for an answer to be self-contained.

## Only run when invoked

Only run when the user typed `/team-message` (with optional question numbers as args). Don't trigger on adjacent phrases.

## Inputs

The user's invocation looks like one of:
- `/team-message 2, 3, 5` — pick specific questions
- `/team-message 2-4, 7` — ranges OK
- `/team-message all` — every open question in the most recent batch
- `/team-message` (no args) — ask which questions they want included

Question numbers refer to the **most recent numbered batch of questions** in the conversation, unless the requester clarifies otherwise (e.g. "from the last batch but one").

## Step 1 — Find the questions

Look back through the conversation for the most recent numbered list of questions Claude asked the requester. If you can't find a recent batch, ask: *"Which questions do you want me to package up? Paste them in or point me at the message that has them."*

If the args reference numbers that don't exist (e.g. user said `/team-message 7` but the last batch only had 5 questions), tell them and ask which questions they meant.

## Step 2 — Identify each question's owner department

Each question goes to one of: **product**, **design**, **dev**, **QA**, or **business / leadership**.

Many questions in task-creator output already carry an explicit owner label like `— *dev*` or `— *requester / product*`. **Trust those labels** when present. Don't second-guess.

If a question has no label, infer the department from the content:
- API contracts, data models, migrations, performance, infra, "how is X stored", "does service Y support Z", deploy correlation → **dev**
- User behavior, scope decisions, KPIs, what should happen in edge case X, business policy → **product**
- Visual treatment, layout, screens, mockups, UX flows, accessibility → **design**
- Test coverage, regression risk, environments to verify, severity classification of a bug behavior → **QA**
- Strategic priority, budget, partnership, legal, brand → **business / leadership**

If a single question genuinely needs two departments to coordinate (e.g. "should this be feature-flagged per community?" — product needs to decide rollout, dev needs to confirm feasibility), put it under the **primary** department and note the cross-functional ask in the question.

If you genuinely can't tell, ask the requester: *"Question N — should this go to dev or product?"*

## Step 3 — Compose the message

Output a single WhatsApp-ready message. Use this structure:

```
👋 Need your help on a task spec I'm writing up — questions below, grouped by team.

**Context:** [1-2 sentence summary of what the task is. Use the `Original request` if a ticket draft exists, otherwise summarize from the conversation.]

**For [Department]:**
1. [Question, verbatim where useful]
2. [Question]

**For [Department]:**
1. [Question]

—
Reply in this thread or DM me and I'll fold the answers into the spec. Thanks 🙏
```

Notes on style:
- Keep it warm, brief, and respectful of the team's time. They're being asked for help, not interrogated.
- Restate the task context at the top so the team doesn't have to ask "what is this about?".
- Renumber within each department block — `1, 2, 3` per dept rather than carrying the original `2, 3, 5` numbers, which only make sense to the requester. Keep the original numbers in a separate mapping so you can match answers back later (see Step 5).
- WhatsApp supports `*bold*` not `**bold**`. Use single asterisks if you want bold to render. Plain text is also fine — many teams prefer it.
- Light emoji is fine if it matches the team's culture; default to one or two (👋 🙏). Skip emoji entirely if the requester signals they prefer plain text.
- Do NOT include internal repo names or technical jargon the team won't recognize unless the question itself requires it.
- Output is in **English** even if the surrounding conversation is in Hebrew, matching the task-creator convention. If the team uses Hebrew internally, the requester can translate before pasting — ask if they want the English version or a translated version.

## Step 4 — Show it to the requester for sign-off

Before declaring it done, present the message and ask: *"Here's the message. Want any changes — tone, more/less context, different department for any question — or are we good to send?"*

If they ask for changes, apply them and re-show.

## Step 5 — Track the question mapping

Keep an internal note (just in your reply to the requester) of the original-question → department-block-position mapping, like:

> Mapping for when answers come back:
> - Original Q2 → "For dev" #1
> - Original Q3 → "For product" #1
> - Original Q5 → "For dev" #2

This helps the requester paste back answers in a structured way, e.g. *"dev #1: yes, it's an enqueued worker"*.

End with a clear next step: *"Send this to the team. When you have any answers — even partial — paste them back here and tell me which questions they answer. I'll fold them into the spec."*

## Edge cases

- **No questions in the conversation yet:** ask the requester to paste the questions they want sent.
- **The task-creator skill hasn't been run yet:** that's OK — this skill works standalone for any open-question situation. The requester just needs to provide the questions.
- **The requester asks for a translation to Hebrew:** translate the message but warn that if any technical terms (e.g. component names, status codes) are best kept in English for clarity, you'll keep those.
- **The requester says "actually skip question N":** drop it and re-show the message.
- **A question is so unclear that the team won't be able to answer it without back-and-forth:** flag it back to the requester before sending — *"Q3 as written might trigger more questions back from the team. Want me to rewrite it as: ___?"* This saves a round-trip.

## Why this skill exists

The slowest part of task-spec'ing isn't writing the spec — it's the latency of bouncing technical questions through a non-technical requester to the dev team and back. This skill compresses that loop: requester forwards a clean message → team answers in their normal channel → requester pastes back → spec moves forward. Without it, the requester has to hand-translate "what does PENDING → CONFIRMED mean for service orders?" into something a team chat can act on, which they'll often skip or do badly.
