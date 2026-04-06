# Lessons Learned

This file is an **append-only log** of mistakes and discoveries.

When the user corrects you, add an entry here **and** absorb the prevention rule into the
correct skill file or into copilot-instructions Working Principles. The lesson log is a
record; the skills and instructions are where rules live and get enforced.

---

## How to Add a Lesson

1. Add an entry at the **top** of the Log section (newest first).
2. Absorb the prevention rule into the appropriate skill or copilot-instructions.
3. If the lesson doesn't fit any existing skill, add it to the Working Principles table in copilot-instructions.

```
### YYYY-MM-DD — Short title

**What happened:** What went wrong or what was discovered.
**Absorbed into:** File or section where the rule now lives.
```

---

## Log

### 2026-04-06 — Check git branch before committing; create branch as part of startup

**What happened:** Made commit f80015b directly on `main` branch, violating git workflow rules. Did not verify current branch before committing after pulling. Then repeated the error by checking out main and committing lessons directly without creating a feature branch first.
**Absorbed into:** copilot-instructions → Before You Start (step 1 now includes `git branch` verification as first action after pull); Working Principles (new rules added).

### 2026-04-06 — Use git built-ins for path resolution in pre-commit hooks, never bash tricks

**What happened:** Pre-commit hook used `dirname ${BASH_SOURCE[0]}` for path resolution. When installed as `.git/hooks/pre-commit` via symlink, resolved to `.git/hooks/` directory instead of repo root, and hook couldn't find validation script.
**Absorbed into:** copilot-instructions → Working Principles (new rule: use `git rev-parse --show-toplevel`).

### 2026-04-06 — Always verify data dependencies before deletion; use grep to check

**What happened:** Attempted to remove duplicate Yellow Onion from nutrition DB without checking which ID was referenced by recipes; almost deleted the wrong entry that recipes actually use.
**Absorbed into:** copilot-instructions → Working Principles (new rule: verify with grep before deleting data).

### 2026-04-01 — Imperial units must be checked in ALL text fields, not just ingredient units

**What happened:** PR review flagged imperial units (`1 lb`, `6 oz`) in description, step text, and top-level notes. Validation only checked the ingredients section units.
**Absorbed into:** recipe-validation skill → Consistency section; new checklist item added.

### 2026-04-01 — nutrition_id references must have DB entries in the same PR

**What happened:** Recipe introduced new ingredients (Fresh Garlic, Crushed Tomatoes, Vodka) with `nutrition_id` fields pointing to entries that didn't exist in `docs/data/nutrition-db.json`. PR reviewer caught the gap.
**Absorbed into:** recipe-validation skill → Consistency section; new checklist item added.

### 2026-03-28 — Updating docs/data/nutrition-db.json requires a build to sync wwwroot

**What happened:** PR #69 added new nutrition DB entries to `docs/data/nutrition-db.json` but did not rebuild, so the committed `wwwroot/data/nutrition-db.json` was left at the old entry count. The incremental `CopyNutritionDb` target (using `Inputs`/`Outputs`) skipped the copy in CI because both files had the same checkout timestamp. The deployed app showed 3 ingredients as missing nutrition data.
**Absorbed into:** recipe-validation skill → Consistency section; validation checklist item updated to clarify the wwwroot copy is build-generated (not manually tracked).

### 2026-03-18 — Always pluralize units in rendered quantity strings

**What happened:** The "Makes 24 Serving" yields display showed a singular unit regardless of quantity. The `Pluralize` helper existed in `NutritionPanel` but was not applied to the yields line in `IngredientList`.
**Absorbed into:** recipe-rendering skill → Ingredient / Yield Display rules; see "Unit Pluralization" section.

### 2026-03-15 — Include self‑evaluation section at end of every response

**What happened:** I responded without a consistent self‑evaluation format and the user pointed out the missing behavior.
**Absorbed into:** copilot-instructions → Working Principles ("You finish a response to the user → Add a self‑evaluation section...").

### 2026-03-14 — Verify state before assuming blockers

**What happened:** Assumed PR review conversations were still unresolved without checking.
**Absorbed into:** copilot-instructions → Working Principles ("You previously reported a blocker → re-verify").

### 2026-03-14 — Always check out main and pull before starting work

**What happened:** Work was started on a branch without first returning to `main` and pulling.
**Absorbed into:** copilot-instructions → Before You Start (steps 1–2).

### 2026-03-12 — Never use sed or terminal commands to edit files

**What happened:** Used `sed -i` for bulk edits; user lost visibility over changes.
**Absorbed into:** copilot-instructions → Working Principles ("You need to edit a file → use built-in tools").

### 2026-03-12 — Do not stop when the user skips a tool call

**What happened:** User skipped a command and assistant stopped instead of asking why.
**Absorbed into:** copilot-instructions → Working Principles ("The user skips a tool call → ask why mid-flight").

### 2026-03-12 — PR titles must use conventional commits format

**What happened:** PR created with freeform title instead of `<type>(<scope>): <description>`.
**Absorbed into:** pull-request skill → PR Title Check section.

### 2026-03-12 — Sub-recipe ingredients must include a doc_link field

**What happened:** Sub-recipe ingredient had no `doc_link` pointing to the referenced recipe.
**Absorbed into:** recipe-validation skill → Cross-References checklist.

### 2026-03-12 — Must run validation checklist after editing any recipe file

**What happened:** Edits were declared done without running validation.
**Absorbed into:** copilot-instructions → Working Principles ("You finish editing a recipe → run validation").

### 2026-03-11 — Present options before implementing a technology choice

**What happened:** Went straight to Blazor without presenting alternatives when user was open to options.
**Absorbed into:** copilot-instructions → Working Principles ("User is open to options → present 2–3 with trade-offs").

### 2026-03-05 — Always show scaling math and confirm with the user

**What happened:** Panko quantity misinterpreted due to ambiguous base/target in scaling.
**Absorbed into:** recipe-creation skill → Step 1 (scaling confirmation rule) and copilot-instructions → Working Principles.

### 2026-02-28 — Instructions must leave no room for assumptions

**What happened:** PB&J thought experiment highlighted that implicit steps fail under literal interpretation.
**Absorbed into:** recipe-documentation skill → R3.7 and recipe-validation skill → Instructions checklist.

### 2026-02-28 — Must self-evaluate after editing instructions/skills

**What happened:** Rules were written but not self-verified before declaring done.
**Absorbed into:** copilot-instructions → Working Principles ("You finish editing any .github/ file → re-read and verify").

### 2026-02-28 — Imperial-to-metric conversion requires author confirmation

**What happened:** 2 lbs → 907g was correct but author might prefer a round number.
**Absorbed into:** recipe-creation skill → Step 1 (imperial conversion confirmation) and copilot-instructions → Working Principles.

### 2026-02-28 — Watch for merged words from copy-paste

**What happened:** "Fahrenheittested yet" — two words merged from paste artifact.
**Absorbed into:** recipe-validation skill → Text Quality checklist ("scan all pasted text").

### 2026-02-28 — volume_alt is required, not optional, for spices under 10g

**What happened:** Spice under 10g written without `volume_alt`.
**Absorbed into:** copilot-instructions → The 5 Rules (rule 3) and recipe-validation skill → Ingredients checklist.

### 2026-02-28 — Migrated recipes must update all internal paths

**What happened:** Recipe migrated from `.md` to `.yaml` but `related` paths still pointed to `.md`.
**Absorbed into:** recipe-validation skill → Cross-References checklist ("If moved/renamed, update all references").
