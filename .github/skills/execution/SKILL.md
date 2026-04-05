---
name: execution
description: Autonomous execution protocol for implementing tasks. Governs planning, iteration, subagent use, verification, and self-correction. Use when executing any non-trivial implementation work — recipe edits, code changes, CI/CD updates, or multi-step tasks.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
  category: execution
  project: OpenCookbook
---

# Execution

## How This Skill is Invoked

This skill is **mandatory for all implementation work**. `copilot-instructions.md` requires it for any task involving file changes, multi-step work, or structural decisions. Read it before starting.

---

## Core Principles

These govern every decision during execution:

- **Keep It Simple:** Prefer the straightforward approach. Only introduce complexity when it earns its place.
- **Surgical Precision:** Touch only what the task requires. A smaller diff is almost always a better diff.
- **Drive to Completion:** Act on what you know. Resolve blockers yourself. Don't wait for permission to fix obvious problems.
- **Never Guess:** In a recipe repo, wrong data is worse than no data. If you don't know a value, ask.

---

## Phase 1: Scope and Sequence

**Establish a clear plan before making changes** whenever the task involves 3+ steps or any structural judgment.

### Decision Table

| Situation | Response |
|---|---|
| Single-file, obvious fix | Implement immediately — no ceremony needed |
| Changes spanning multiple files | Outline the sequence first |
| Structural or format decisions | Specify the approach before touching files |
| Ambiguous requirements | Research first (subagents are cheap) |
| User story with acceptance criteria | Decompose criteria into ordered checkpoints |

### Building the Plan

1. **Create a todo list** (`manage_todo_list`) with concrete, verifiable items
2. **Specify expected changes** up front — which files, what kind of edit
3. **Bake in proof steps** — plan how you will verify each change (validation checklist, build, etc.)
4. **Sanity-check coverage** — does the plan address every requirement?
5. **Abandon broken plans early** — if progress stalls, scrap the approach and redesign

---

## Phase 2: Disciplined Iteration

### The Work Loop

For every planned item:

```
1. Flag it as in-progress
2. Make the change
3. Prove it works (validate, build, inspect diff)
4. Flag it as done
5. Commit when you reach a logical boundary
6. Advance to the next item
```

**A task is not done until you can demonstrate it works.** That means:
- File is valid YAML (for recipes)
- Validation checklist passes (for recipe edits)
- Build succeeds (for code changes)
- Tests pass (if applicable)
- You would confidently submit this for review

### Communicating Progress

- Summarize at the milestone level, not the keystroke level
- State what changed, what was validated, and what comes next
- When all items are complete, give a brief accounting of files touched, tests run, and notable decisions

### Commit Rhythm

- One commit per logical unit — not per file, not per session
- Every commit must be valid on its own (no half-finished states)
- Follow conventional commit format (see `recipe-versioning` skill)
- Never lump unrelated changes together

---

## Phase 3: Delegating to Subagents

Preserve your context window by offloading research and exploration.

### Good Candidates for Delegation

| Work | Delegate? |
|---|---|
| Exploring recipe patterns across the repo | Yes |
| Scanning for broken cross-references | Yes |
| Reading multiple skill files for context | Yes |
| Making a single targeted edit | No — do this inline |
| Investigating CI failures or build errors | Yes |

### Delegation Guidelines

- **One clear objective per subagent** — focused tasks yield better results
- **State the return format** — tell it exactly what information you need back
- **Accept the findings** unless they conflict with verified facts
- Subagents gather information; you apply it

---

## Phase 4: Prove It Before You Ship It

**Completion requires evidence, not just intent.**

### For Recipe Changes

- [ ] **Valid YAML:** No syntax errors
- [ ] **Validation checklist:** Run the full recipe-validation skill checklist
- [ ] **Cross-references valid:** All `path` values in `related[]` point to real files
- [ ] **Diff inspected:** `git diff` — read every hunk for accidental changes

### For Code Changes (Visualizer)

- [ ] **Builds:** `dotnet build`
- [ ] **Tests green:** `dotnet test`
- [ ] **Diff inspected:** `git diff` — no unintended changes

### For CI/Workflow Changes

- [ ] **Valid YAML syntax** — no indentation errors
- [ ] **Correct action versions** — pinned to specific versions
- [ ] **Diff inspected:** only intended changes present

### Universal

- [ ] **No trusted silence:** Empty tool output gets a second verification method
- [ ] **Review-ready:** You would not hesitate to open a PR with this

---

## Phase 5: Pursue Clarity

After something works but before you commit, pause: **"Is there a cleaner way to express this?"**

### Apply When

- A working solution feels convoluted
- Hindsight reveals a simpler path
- Nearby content could be simplified without expanding scope

### Skip When

- The fix is already obvious and direct
- The change is purely mechanical
- Improvement would require unrelated restructuring

---

## Phase 6: Resolve Errors Autonomously

When you encounter a failing build, broken YAML, validation error, or CI failure: **resolve it without prompting.**

### Approach

1. **Parse the failure** — read the actual error, not just the summary
2. **Reproduce locally** — run the same command that failed
3. **Trace to the root** — don't mask symptoms with patches
4. **Implement the fix** — choose the correct solution, not the fastest one
5. **Confirm resolution** — prove the error no longer occurs
6. **Scan for siblings** — check whether the same pattern exists elsewhere

### Prohibited Behaviors

- Asking whether you should fix an obvious error — of course you should
- Describing a problem without working toward a solution
- Presenting a menu of options instead of executing the best one
- Pushing diagnosis work back to the user
- Settling for a workaround when a proper fix is reachable

---

## Phase 7: Continuous Skill Refinement

**Correct course in real time, not just at session end.**

After any mistake or user correction during a session:

1. **Name the failure mode** — what exactly went wrong?
2. **Search existing skills** — is this already documented?
3. **If it's new, update the relevant skill now** — don't defer to the end-of-session review
4. **Write a concrete prevention rule** — vague advice doesn't prevent recurrence

This complements the mandatory end-of-session self-evaluation. The difference: this happens the moment you learn something, not after the work is finished.

---

## Prohibited Patterns

These behaviors are explicitly disallowed:

1. **Waiting for a green light** — if the task is defined, begin executing
2. **Surfacing problems without fixes** — always pair a diagnosis with a resolution
3. **Declaring done without proof** — every completion claim needs supporting evidence
4. **Doubling down on a stuck approach** — halt, rethink, restart
5. **Gold-plating simple work** — match effort to actual complexity
6. **Winging complex work** — multi-step tasks get a written plan
7. **Walking past defects** — if you spot an issue while working, address or log it
8. **Believing empty output** — always cross-check with a second method
9. **Guessing data values** — in a recipe repo, wrong quantities are dangerous. Ask.

---

## Quick Reference

```
Task arrives
    ↓
Trivial (1-2 steps)?
    Yes → Implement, verify, commit
    No  ↓
Plan: build todo list with verifiable items
    ↓
Per item:
    Start → Implement → Prove → Complete → Commit
    ↓
    Stuck? → Stop → Re-plan → Resume
    ↓
All done?
    ↓
Final gate:
    Valid ✓ | Validated ✓ | Diff clean ✓ | Review-ready ✓
    ↓
Self-evaluate (per self-evaluation skill)
    ↓
Ship it
```
