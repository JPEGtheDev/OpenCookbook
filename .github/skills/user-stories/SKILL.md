---
name: user-stories
description: Workflow for writing well-formed user stories using the INVEST framework with Given/When/Then acceptance criteria. Use when defining features, tasks, or improvements for the OpenCookbook project.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
---

# User Stories (INVEST Framework)

## When to Use This Skill

Use this when:
- Defining a new feature for the recipe renderer, website, or tooling
- Breaking down a vague request into actionable development tasks
- Writing GitHub issues that describe behavior from a user's perspective
- Planning work before committing to an implementation approach

---

## The INVEST Framework

Every story must pass all six checks. If it fails any one, revise before writing it.

| Letter | Criterion | What it means | Common failure |
|---|---|---|---|
| **I** | Independent | The story has no hard dependency on another incomplete story. Dependencies on already-completed work are acceptable. | "This needs story X done first" |
| **N** | Negotiable | The story describes *what*, not *how*. Implementation details are left to the developer | Story specifies which library or file to use |
| **V** | Valuable | Delivering this story moves the product forward for a real user or stakeholder | Story is purely internal scaffolding with no visible output |
| **E** | Estimable | The team (or agent) can give a rough size estimate | Story is too vague to size, or too large to fit in one effort |
| **S** | Small | The story fits comfortably in a single work session or sprint | Story requires more than ~3 days of uninterrupted work |
| **T** | Testable | Acceptance criteria exist and can be verified pass/fail | Story says "it should feel fast" or uses unmeasurable language |

---

## Story Format

All stories follow this structure:

```
As a <user type>,
I want <goal or capability>,
so that <reason or benefit>.
```

### User Types in OpenCookbook

Use these specific personas — do not use generic "user":

| Persona | Who they are |
|---|---|
| `recipe author` | JPEGtheDev or any contributor creating/editing recipe YAML files |
| `home cook` | Someone browsing and following a recipe from the rendered output |
| `developer` | Someone building a renderer, tool, or integration against the recipe data |
| `AI assistant` | An agent reading and generating recipe files programmatically |

---

## Acceptance Criteria Format

Every story must have at least one acceptance criterion. Use Given/When/Then (Gherkin-style):

```
Given <initial context or precondition>,
When <action is taken>,
Then <expected, verifiable outcome>.
```

Multiple criteria are allowed. Each must be independently testable.

**Rules for acceptance criteria:**
- Use concrete, observable outcomes. ❌ "it works correctly" → ✅ "the page shows the ingredient list"
- Do not describe implementation. ❌ "the Vue component renders" → ✅ "the ingredient list is visible"
- Cover the happy path first, then edge cases
- At least one criterion must cover the failure/error case if the feature can fail

---

## Story Size Reference

| Size | Effort | Example |
|---|---|---|
| XS | < 1 hour | Add a missing `volume_alt` field to one ingredient |
| S | ~half day | Add a new field to the recipe YAML schema |
| M | 1–2 days | Build a single page/component in the renderer |
| L | 3–5 days | Build a full feature (search, filtering, print view) |
| XL | > 5 days | **Split this story** — it is too large |

If a story is XL, do not write it. Split it into M or smaller stories first.

---

## Information to Gather Before Writing

Do not write a story until you have answers to all of these:

1. **Who is the user?** Which persona (from the table above)?
2. **What do they want to do?** What is the goal or task?
3. **Why do they want it?** What is the value — what problem does it solve?
4. **How will we know it worked?** At least one concrete, verifiable outcome.
5. **Are there edge cases?** Empty states, missing data, error conditions.
6. **Does this depend on another story?** If yes, it may not be Independent — consider splitting.

**If any answer is missing: ask. Do not guess.**

---

## Writing Process

### Step 1 — Draft the story

Write the As a / I want / So that sentence. Check it against INVEST:
- Is it independent of in-progress work?
- Does it describe *what*, not *how*?
- Is there a real user who benefits?
- Can you give it a size (XS–L)?
- Does it fit in a single working session?
- Can you write at least one testable acceptance criterion?

### Step 2 — Write acceptance criteria

Write at least:
- 1 happy-path criterion
- 1 edge case or failure criterion (if applicable)

### Step 3 — Size the story

Assign XS / S / M / L. If L, consider splitting. If XL, split before proceeding.

### Step 4 — Link related stories or context

If the story relates to a recipe, link the `.yaml` file.
If it relates to another story or issue, reference it.

---

## Output Format

When writing a story for a GitHub issue, use this template:

```markdown
## User Story

As a <persona>,
I want <goal>,
so that <benefit>.

## Acceptance Criteria

- [ ] Given <context>, when <action>, then <outcome>.
- [ ] Given <context>, when <action>, then <outcome>.

## Size

<XS / S / M / L>

## Notes

<Any relevant context, links, or constraints. Omit section if empty.>
```

---

## INVEST Validation Checklist

Run this before finalizing any story.

- [ ] **I** — Story has no hard dependency on another incomplete story
- [ ] **N** — Story describes what the user needs, not how to build it
- [ ] **V** — At least one real persona benefits from this story
- [ ] **E** — Story has been assigned a size (XS / S / M / L)
- [ ] **S** — Story is L or smaller (not XL); if XL, it must be split first
- [ ] **T** — At least one acceptance criterion is written in Given/When/Then format
- [ ] Acceptance criteria use concrete, observable outcomes (no vague language)
- [ ] At least one edge case or failure criterion is included (where applicable)
- [ ] User persona is one of the four defined personas (not generic "user")
