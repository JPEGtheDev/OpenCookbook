---
name: recipe-creation
description: Step-by-step workflow for creating a new recipe from scratch. Use when the user wants to add a new recipe to the repository. Always starts with a conversation to gather information — never write a recipe without asking first.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
---

# Recipe Creation

## When to Use This Skill

Use this when:
- Creating a brand new recipe file
- Converting a personal recipe into the OpenCookbook format
- The user says "add a recipe" or "write a recipe"

**Before you start:** Read [recipe-documentation](../recipe-documentation/SKILL.md) for the format rules.

---

## THE MOST IMPORTANT RULE

**Do NOT start writing a recipe file until you have gathered information through conversation.**

Recipes are personal. You cannot guess quantities, cooking methods, or serving sizes. ASK FIRST.

---

## Step 1: Gather Information

Ask the user these questions. Do not skip any unless the answer is already known:

1. What is the dish?
2. What are the ingredients and their quantities?
3. What are the step-by-step cooking instructions?  
   - **Be extremely granular**; include every action a novice would need (open jars, measure, preheat, etc.).
   Machines will interpret text literally. If a detail is not written, it is not done.
4. How many servings / what yield?
5. Has this recipe been tested? How many times?
6. Are there variation paths (e.g. grilled vs. baked, spicy vs. mild)?
7. What equipment or utensils are needed?
8. Are there related recipes that should be linked?
9. Any food safety notes (meat temperatures, allergy info)?
10. Any storage or freezing instructions?

**If the user gives quantities in pounds, cups, or ounces:**
- Convert to grams before writing the file
- Tell the user the converted amount and ask them to confirm

⚠️ **STOP — DO NOT PROCEED TO STEP 2.**
You need enough information to write the recipe. If you are missing ingredients, quantities, or cooking steps, go back and ask. Do not fill in gaps with guesses.

---

## Step 2: Determine Status and Placement

Use this decision table:

| Situation | Status | Folder |
|---|---|---|
| Recipe is fully tested, quantities are confirmed | `stable` | `Recipes/` |
| Recipe works but needs more testing | `beta` | `Recipes/Beta/` |
| Recipe is incomplete (missing instructions or uncertain quantities) | `draft` | `Recipes/Beta/` |
| Recipe is part of a set (e.g. rub + binder) | Any | `Recipes/<Topic>/` |

**When in doubt, use `beta`.** It is easier to promote than demote.

---

## Step 3: Plan the File Structure

Before writing YAML, answer these questions:

1. What instruction sections will this recipe have? (prep, cook, serve, freeze)
2. Are there branching paths? → If yes, what is the `branch_group` name?
3. Which spice ingredients are under 10g? → These need `volume_alt`.
4. Does this relate to existing recipes? → These need `related` entries.

Write out this plan and show it to the user. If you skip this step, you will make structural mistakes.

⚠️ **STOP — REVIEW YOUR PLAN BEFORE WRITING ANY YAML.**
Does the plan match what the user described? If not, revise it. If you are unsure, ask the user to confirm.

---

## Step 4: Write the YAML File

Use this template as your starting point:

```yaml
name: [Recipe Name]
version: "1.0"
author: Jonathan Petz | JPEGtheDev
description: >
  [1-3 sentence description]
status: [stable/beta/draft]

ingredients:
  - heading: null
    items:
      - quantity: [number]
        unit: [g/ml/cloves/etc.]
        name: [Ingredient Name]

utensils:
  - heading: null
    items:
      - [Utensil Name]

instructions:
  - heading: null
    type: sequence
    steps:
      - text: "[Step text]"
```

**Rules to follow while writing:**
- Every spice under 10g → add `volume_alt` (look up in [SPICE_CONVERSIONS.md](../../references/SPICE_CONVERSIONS.md))
- Every temperature → write as `°C (°F)`
- Every meat recipe → include safe internal temperature
- Every branch → set `type: branch` and `branch_group`
- Every optional section (Serving, Freezing) → set `optional: true`

---

## Step 5: Choose the Filename

**Format:** `Title_Case_With_Underscores.yaml`

- Use a descriptive name: `Guajillo_Brisket_Rub.yaml` not `Rub.yaml`
- Underscores between words, no spaces, no hyphens
- `.yaml` extension (never `.md`)

---

## Step 6: Add Cross-Links

If this recipe relates to another:

```yaml
related:
  - label: Kebab Meat Recipe
    path: ./Kebab_Meat.yaml
```

Also update the other recipe's `related` field to link back.

---

## Step 7: Validate

**Do NOT consider the recipe done until you check EVERY item below.**

Go through this checklist one item at a time. If any item fails, fix it now. Do not say "I'll fix it later."

- [ ] All 5 required fields present? (`name`, `version`, `author`, `description`, `status`)
- [ ] `version` is quoted? (`"1.0"` not `1.0`)
- [ ] All quantities in grams or ml?
- [ ] All spices under 10g have `volume_alt`?
- [ ] All temperatures are dual-format `°C (°F)`?
- [ ] Meat recipes state minimum safe internal temperature?
- [ ] File is in the correct folder for its status?
- [ ] Filename is `Title_Case_With_Underscores.yaml`?
- [ ] All `related` paths point to files that actually exist?

If any item fails, fix it now. Do not leave it for later.

For a thorough validation, use the [recipe-validation skill](../recipe-validation/SKILL.md).

---

## If the Recipe is Incomplete

Sometimes you won't have all the information. That's okay:

1. Set `status: draft`
2. Place in `Recipes/Beta/`
3. Set `instructions: []` (empty list) if no cooking instructions exist yet
4. Add a `notes` field listing what is still needed:

```yaml
status: draft
instructions: []
notes:
  - "Cooking instructions needed — smoke, bake, or sous vide?"
  - "Spice quantities may need halving"
```
