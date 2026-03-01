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
| Writing a recipe file without asking the user first | Read the recipe-creation skill. You must gather info through conversation first |
| Skipping `version`, `author`, or `status` fields | Every recipe needs ALL 5 required fields — check the list below |
| Putting `notes` on a `stable` recipe | `notes` is only for `beta` or `draft`. Remove it before promoting |
| Using `1.0` instead of `"1.0"` for version | The version field MUST be a quoted string: `"1.0"` |

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

If you don't know which skill to use, read **recipe-documentation** — it has the format rules.

---

## Before You Start Any Task

Do these steps **in this exact order**. Do not skip any. These are not suggestions.

1. **Read lessons FIRST.** Open [lessons.md](lessons.md) and read the ENTIRE file, every entry. If a past mistake applies to your current task, follow the prevention rule. If you skip this step, you will repeat a known mistake.
2. **Read the skill.** Open the correct skill file from the table above and read the ENTIRE file. Do not skim. Do not summarize. Read every line. The skill contains rules you must follow.
3. **Plan before building.** If the task has 3+ steps or involves any structural decision, write out a numbered step-by-step plan BEFORE doing anything. Show the plan. If something goes wrong mid-plan, STOP immediately and write a new plan. Do not push forward on a broken approach.
4. **Ask if unsure.** If any detail is ambiguous, missing, or could be interpreted two ways, STOP and ask the user before proceeding. Do not guess. Do not assume. Do not fill in blanks with plausible values.

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
| The user corrects you | Update [lessons.md](lessons.md) with what went wrong, a prevention rule, and a rule ID. Do this immediately. |
| A lesson in lessons.md applies to what you're doing | Follow the prevention rule. It exists because the mistake already happened once. |
| You need a spice gram-to-volume conversion | Look it up in [SPICE_CONVERSIONS.md](skills/references/SPICE_CONVERSIONS.md). Do not guess. |
| A validation gap or new error pattern is found | Add a rule to [RULES.md](skills/references/RULES.md) and a checklist item to the validation skill. |
| You're unsure about anything | Ask the user. Do not guess. Do not assume. |
