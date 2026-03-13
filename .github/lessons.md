# Lessons Learned

> **YOU MUST READ EVERY ENTRY BELOW BEFORE STARTING ANY TASK.**
> Do not skim. Do not skip. If an entry applies to your current task, follow the prevention rule.
> These lessons exist because the mistake already happened. Do not repeat it.

Every entry is a mistake that happened or a non-obvious behavior that was discovered.
Each lesson includes a prevention rule so the same mistake does not happen again.

---

## How to Add a Lesson

After ANY correction from the user, add an entry here. Format:

```
### YYYY-MM-DD — Short title

**What happened:** What went wrong or what was discovered.
**Prevention rule:** A specific, actionable rule to follow in the future.
**Rule ID:** R-number from RULES.md (if applicable), or "None".
```

Add new entries at the **TOP** of the log (newest first).

---

## Log

### 2026-03-12 — Never use sed or terminal commands to edit files

**What happened:** Used `sed -i` to bulk-update reference links across multiple files. The user loses visibility and control over each individual change when edits happen via terminal commands.
**Prevention rule:** Always use the built-in file editing tools (`replace_string_in_file` or `multi_replace_string_in_file`) for any file modification. Never use `sed`, `awk`, `perl -i`, or any terminal-based text replacement. Using `grep` for **reading/searching** is fine — the restriction is on **write operations** only. The only exception is if the user explicitly asks for a terminal command.
**Rule ID:** None.

### 2026-03-12 — Do not stop when the user skips a tool call

**What happened:** The user skipped a terminal command and the assistant stopped working instead of asking why.
**Prevention rule:** When the user skips a tool call, ask them why using the `ask_questions` tool before deciding next steps. Do not assume the task is cancelled. The user may want a different approach or may have skipped by accident.
**Rule ID:** None.

### 2026-03-12 — PR titles must use conventional commits format

**What happened:** A PR was created with a freeform title ("Add doc_link to sub-recipe ingredients; enforce self-evaluation; fix binder description") instead of the conventional commits format.
**Prevention rule:** All PR titles must use conventional commits format: `<type>(<scope>): <description>`. Common types: `feat` (new feature/field), `fix` (bug/error correction), `docs` (documentation only). Scope is optional but use `recipes` for recipe file changes. Example: `feat(recipes): add doc_link to sub-recipe ingredients`.
**Rule ID:** None.

### 2026-03-12 — Sub-recipe ingredients must include a doc_link field

**What happened:** The "Kebab Meat Recipe (full batch)" ingredient in `Recipes/Grilling/Kebab_Meatballs.yaml`
had no `doc_link` field pointing to the Kebab_Meat.yaml recipe file, even though it is
explicitly a reference to another recipe in the repository.
**Prevention rule:** Any time an ingredient's `name` refers to another recipe in this
repository, immediately add a `doc_link` field with the relative path to that recipe's
`.yaml` file. Do not rely on the `related` section alone — the link must be on the
ingredient itself so a renderer can surface it inline.
**Rule ID:** None.

### 2026-03-12 — Must run validation checklist after editing any recipe file

**What happened:** After editing four recipe YAML files (renaming, ingredient changes, reordering,
description fixes), the assistant declared the work done without running the validation checklist
from the recipe-validation skill on any of the modified recipes.
**Prevention rule:** After editing ANY recipe YAML file, immediately run the full validation
checklist from the recipe-validation skill on every file you changed before declaring done.
Do not skip this step even for small edits. This is required regardless of how minor the change
appears.
**Rule ID:** None.

### 2026-03-11 — Present options before implementing a technology choice

**What happened:** The user asked for a plan for an HTML visualizer and said
their preference was Blazor, but they could be persuaded on another SPA. The
assistant went straight to implementing Blazor without presenting alternative
options (e.g., React, Svelte, plain static-site generators) for the user to
evaluate.
**Prevention rule:** When the user asks for a plan or says "I can be persuaded,"
always present at least 2–3 technology options with trade-offs before
implementing. Let the user choose. Do not assume the stated preference is the
final decision.
**Rule ID:** None.

### 2026-03-05 — Always show scaling math and confirm with the user

**What happened:** A panko quantity of "65 for scaling to 18" was misinterpreted
as 65 g total for 18 servings, when it actually meant 65 g for a 5-serving test
batch. This caused panko and egg quantities to be drastically under-scaled
(65 g / 6 eggs instead of 234 g / 8 eggs).
**Prevention rule:** When scaling ingredient quantities from a test batch to a
full recipe, always show the per-serving calculation and the final scaled amount,
then ask the user to confirm before writing. Do not assume which number is the
base and which is the target.
**Rule ID:** None.

### 2026-02-28 — Instructions must leave no room for assumptions

**What happened:** We discussed the peanut-butter-and-jelly thought experiment to
highlight that implicit steps cause failures when instructions are interpreted
literally. Even though no mistake occurred in the repository, the risk remains
whenever recipes are written with gaps.
**Prevention rule:** When collecting or writing recipe instructions, require absolute
clarity. Every action a novice would need must be included explicitly. Assume a
machine (or a new cook) will follow the text exactly—open jars, preheat ovens,
measure ingredients, handle equipment, etc.  Do not rely on context or common
sense.
**Rule ID:** R3.7.

### 2026-02-28 — Must self-evaluate after editing instructions/skills

**What happened:** After rewriting copilot-instructions.md and all skill files, the assistant
declared the work done without running any verification pass — violating the very rules
it had just written ("Verify Before Done", "Run the validation checklist").
**Prevention rule:** After editing ANY file in `.github/` (instructions, skills, lessons, references),
re-read the file you edited and check it against the rules in copilot-instructions.md. If you wrote
rules, follow them yourself before declaring done.
**Rule ID:** None.

### 2026-02-28 — Imperial-to-metric conversion requires author confirmation

**What happened:** Converting "2 lbs" to 907g was technically correct, but the author
might prefer a round number like 900g or 1000g.
**Prevention rule:** When converting imperial to metric, always show the user the
converted value and ask them to confirm before writing it into the recipe.
**Rule ID:** None.

### 2026-02-28 — Watch for merged words from copy-paste

**What happened:** Recipe text contained "Fahrenheittested yet" instead of
"Fahrenheit tested yet" — two words merged from a copy-paste artifact.
**Prevention rule:** After pasting any text into a recipe, scan the entire text
for merged/concatenated words before saving.
**Rule ID:** R6.1.

### 2026-02-28 — volume_alt is required, not optional, for spices under 10g

**What happened:** A spice ingredient with `quantity < 10` and `unit: g` was
written without a `volume_alt` field.
**Prevention rule:** Every time you write a spice ingredient under 10g, add
`volume_alt` immediately. Do not write the ingredient and plan to add it later.
Look up the value in [SPICE_CONVERSIONS.md](references/SPICE_CONVERSIONS.md).
**Rule ID:** R2.5.

### 2026-02-28 — Migrated recipes must update all internal paths

**What happened:** A recipe was migrated from `.md` to `.yaml` but cross-references
in the `related` field still pointed to `.md` files.
**Prevention rule:** When changing a recipe's file extension or location, search the
entire repo for references to the old path and update them all.
**Rule ID:** R5.4.
