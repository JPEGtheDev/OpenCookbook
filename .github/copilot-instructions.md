# OpenCookbook — Instructions for AI Assistants

You are helping with an open-source recipe repository. Every recipe is a YAML file.
The author is Jonathan Petz (JPEGtheDev). The license is CC0 (public domain).

---

## The 5 Rules (Never Break These)

1. **Recipes are YAML files (`.yaml`).** Never create a Markdown (`.md`) recipe file.
2. **All weights are in grams (`g`).** No pounds, cups, or ounces. Liquids may use milliliters (`ml`).
3. **Spices under 10g need a `volume_alt` field.** This is a tsp/tbsp fallback for cooks without a precise scale.
4. **Temperatures are always dual-format.** Write `218°C (425°F)`. Never write only Celsius or only Fahrenheit.
5. **Never guess quantities or steps.** If something is unknown, ask the user. Do not invent ingredient amounts, cooking times, or temperatures.

---

## Common Mistakes — DO NOT DO THESE

| ❌ Mistake | ✅ What to Do Instead |
|---|---|
| Creating a `.md` recipe file | Always use `.yaml` |
| Using cups, pounds, ounces, tsp, tbsp as the unit | Convert to grams (`g`) or milliliters (`ml`) |
| Writing a spice under 10g without `volume_alt` | Add `volume_alt` on the same ingredient — look it up in SPICE_CONVERSIONS.md |
| Writing `425°F` or `218°C` alone | Write `218°C (425°F)` — always both |
| Making up a quantity the user didn't provide | Ask the user. Say "I don't have this information" |
| Writing a vague or implicit instruction step | Every step must be explicit enough for a novice or robot — no assumed knowledge (R3.7) |
| Writing a recipe file without asking the user first | Read the recipe-creation skill. You must gather info through conversation first |
| Skipping `version`, `author`, or `status` fields | Every recipe needs ALL 5 required fields — check the list below |
| Putting `notes` on a `stable` recipe | `notes` is only for `beta` or `draft`. Remove it before promoting |

---

## What a Recipe File Looks Like

Every recipe YAML file has this shape:

```yaml
name: Recipe Name
version: "1.0"
author: Jonathan Petz | JPEGtheDev
description: >
  Short description of the dish. 1-3 sentences.
status: beta              # stable, beta, or draft

ingredients:
  - heading: null          # null = main group
    items:
      - quantity: 907
        unit: g
        name: Ground Beef
      - quantity: 3
        unit: g
        name: Black Pepper
        volume_alt: "3/4 tsp."   # REQUIRED because 3 < 10

utensils:                  # optional section
  - heading: null
    items:
      - Mixing Bowl
      - Baking Sheet

instructions:
  - heading: null
    type: sequence         # sequence = always runs in order
    steps:
      - text: "Preheat oven to 204°C (400°F)"
      - text: "Mix all ingredients in a bowl"

  - heading: Serving       # optional sections
    type: sequence
    optional: true
    steps:
      - text: "Serve warm with pita bread"

related:                   # optional — links to other recipes
  - label: Kebab Meat Recipe
    path: ./Kebab_Meat.yaml
```

---

## Where Recipe Files Go

| Folder | When to Use | Status |
|---|---|---|
| `Recipes/` | Tested, finalized recipes | `stable` |
| `Recipes/Beta/` | Recipes still being tested or incomplete | `beta` or `draft` |
| `Recipes/<Topic>/` | Group of 2+ related recipes (e.g. `Brisket/`) | Any |

**Filename format:** `Title_Case_With_Underscores.yaml`

- ✅ Good: `Chicken_Shawarma.yaml`, `Guajillo_Brisket_Rub.yaml`
- ❌ Bad: `chicken-shawarma.yaml`, `ChickenShawarma.yaml`, `Chicken Shawarma.yaml`

---

## Skills — Read Before Working

This repository has detailed skill files. **Read the right skill BEFORE starting work.**

| If you need to... | Read this file |
|---|---|
| Create a new recipe from scratch | [recipe-creation](skills/recipe-creation/SKILL.md) |
| Edit, format, or understand recipe structure | [recipe-documentation](skills/recipe-documentation/SKILL.md) |
| Check a recipe for errors or validate it | [recipe-validation](skills/recipe-validation/SKILL.md) |
| Build a renderer for recipes (web or PDF) | [recipe-rendering](skills/recipe-rendering/SKILL.md) |
| Version a recipe or write a commit message | [recipe-versioning](skills/recipe-versioning/SKILL.md) |
| Write a user story using the INVEST framework | [user-stories](skills/user-stories/SKILL.md) |
| Fill in a PR description or title | [pull-request](skills/pull-request/SKILL.md) |
| Write or edit a GitHub Actions workflow | [ci-workflows](skills/ci-workflows/SKILL.md) |

If you don't know which skill to use, read **recipe-documentation** — it has the format rules.

---

## Before You Start Any Task

Do these steps **in this exact order**. Do not skip any. These are not suggestions.

1. **Check out `main` and pull.** Run `git checkout main && git pull origin main` before starting any new task. This ensures you are working from the latest state of the repo.
2. **Re-read changed files.** After pulling, re-read any copilot-instructions, lessons, or skill files that may have been updated. Do not rely on previously cached content.
3. **Read the skill.** Open the correct skill file from the table above and read the ENTIRE file. Do not skim. Do not summarize. Read every line. The skill contains rules you must follow.
4. **Plan before building.** If the task has 3+ steps or involves any structural decision, write out a numbered step-by-step plan BEFORE doing anything. Show the plan. If something goes wrong mid-plan, STOP immediately and write a new plan. Do not push forward on a broken approach.
5. **Ask if unsure.** If any detail is ambiguous, missing, or could be interpreted two ways, STOP and ask the user before proceeding. Do not guess. Do not assume. Do not fill in blanks with plausible values.

---

## Git Workflow

**Never commit directly to `main`.** All changes go through a branch and pull request.

1. **Create a branch** from `main` — use a descriptive name: `<type>/<short-description>` (e.g. `docs/kiev-cutlet-breading-notes`, `feat/search-page`, `fix/deploy-trigger`).
2. **Commit to the branch** using conventional commit messages.
3. **Push the branch** to `origin`.
4. **Create a pull request** targeting `main`. Fill in the PR using the [pull-request skill](skills/pull-request/SKILL.md).
5. **Address any review comments** — push fixes to the same branch.
6. **Merge the PR** once all checks pass and conversations are resolved.

---

## Working Principles

These are if-then rules. Follow them literally.

| If this happens... | Then do this... |
|---|---|
| Task has 3+ steps or a structural decision | Write a numbered plan BEFORE doing anything. Show it to the user. |
| Your plan stops working or hits an error | STOP. Write a new plan. Do not continue the old plan. |
| You find a fixable error in a recipe | Fix it immediately. Do not just list it. |
| You find an error you can't fix (need author's input) | Flag it with `[ASK]` and explain what info you need. |
| You finish editing a recipe | Run the validation checklist from the recipe-validation skill. Do not skip this. |
| You finish editing any `.github/` file | Re-read the file and verify it against the rules in this document. Check if any lessons in [lessons.md](lessons.md) have not been absorbed into skills yet — if so, absorb them now. |
| The user corrects you | Log a short entry in [lessons.md](lessons.md), then immediately absorb the rule into the correct skill file or this Working Principles table. Do not leave rules only in lessons. |
| You start a new task from a todo list | Re-read the relevant skill file(s) before beginning. Do not rely on cached content from earlier in the session. |
| You need a spice gram-to-volume conversion | Look it up in [SPICE_CONVERSIONS.md](references/SPICE_CONVERSIONS.md). Do not guess. |
| A validation gap or new error pattern is found | Add a rule to [RULES.md](references/RULES.md) and a checklist item to the validation skill. |
| You need to edit a file | Use built-in editing tools (`replace_string_in_file` / `multi_replace_string_in_file`). Never use `sed`, `awk`, or terminal writes. `grep` for reading is fine. |
| The user skips a tool call | Ask why immediately using `ask_questions` with multi-select options. Do not stop, do not start a new session — ask mid-flight and continue. |
| You previously reported a blocker | Re-verify the blocker state before repeating it. Do not assume it still exists. |
| The user asks for a plan and says they're open to options | Present 2–3 options with trade-offs before implementing. Let the user choose. |
| You convert imperial to metric | Show the converted value and ask the user to confirm before writing. |
| You scale quantities from a test batch | Show per-serving math and final amount, then ask the user to confirm. Do not assume base vs. target. |
| You're unsure about anything | Ask the user. Do not guess. Do not assume. |
