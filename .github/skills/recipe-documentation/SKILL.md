---
name: recipe-documentation
description: How to format and structure a recipe YAML file in the OpenCookbook repository. Use this skill when writing, editing, or reviewing any recipe file. Covers every field, unit rule, and file convention.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.2"
---

# Recipe Documentation

This skill tells you the **exact format** for recipe files. Follow every rule below.

---

## When to Use This Skill

Use this when you are:
- Writing a new recipe file
- Editing an existing recipe file
- Checking if a recipe follows the correct format
- Converting recipe data between formats

---

## Required File Format

All recipes are YAML files with the `.yaml` extension.

**NEVER** create a recipe as a Markdown (`.md`) file.
**NEVER** create a recipe as JSON, TOML, or any other format.

---

## Required Top-Level Fields

Every recipe file **must** have ALL of these fields:

| Field | Type | Example | Notes |
|---|---|---|---|
| `name` | string | `Chicken Shawarma` | Descriptive name of the dish |
| `version` | string | `"1.0"` | **Must be quoted** — `"1.0"` not `1.0` |
| `author` | string | `Jonathan Petz \| JPEGtheDev` | Format: `Full Name \| Handle` |
| `description` | string | `A spiced buttermilk...` | 1–3 sentences. Use `>` for multiline. |
| `status` | string | `beta` | One of: `stable`, `beta`, `draft` |
| `ingredients` | list | (see below) | At least one group with at least one item |
| `instructions` | list | (see below) | At least one section (unless `status: draft`) |

## Optional Top-Level Fields

| Field | Type | When to Use |
|---|---|---|
| `utensils` | list | Equipment needed for the recipe |
| `related` | list | Cross-links to other recipe files |
| `notes` | list of strings | Open questions — **only for `beta` or `draft`** |

**NEVER** include a `notes` field on a `stable` recipe. Remove it before promoting.

---

## Ingredients

### Structure

```yaml
ingredients:
  - heading: null        # null = default/main group
    items:
      - quantity: 907
        unit: g
        name: Ground Beef
  - heading: "Optional Toppings"    # string = named sub-group
    items:
      - quantity: 50
        unit: g
        name: Shredded Cheese
```

### Unit Rules

**ALWAYS use these units:**

| Unit | When to Use |
|---|---|
| `g` (grams) | All solid ingredients — meat, vegetables, spices, flour, etc. |
| `ml` (milliliters) | Liquids where volume is standard — buttermilk, lemon juice, etc. |
| Count words (`cloves`, `sprigs`, `whole`) | Items counted, not weighed |

**NEVER use these units:**

| ❌ Banned Unit | Use Instead |
|---|---|
| `lbs.` or `lb` | Convert to `g` |
| `oz.` or `oz` | Convert to `g` |
| `cups` or `cup` | Convert to `g` or `ml` |
| `tsp.` or `tbsp.` | Convert to `g` (use `volume_alt` for the fallback) |

### Whole-Unit Ingredients

Some ingredients are bought and used as distinct whole units. Follow the separability rule:

| Type | Definition | Example | How to Write |
|---|---|---|---|
| **Separable** | Can be stored after partial use | Yellow onion, lemon, bell pepper, garlic head | `unit: g` + `weight_alt: "1 whole"` |
| **Non-separable** | Container unusable after opening for partial use | Egg, canned tomatoes | `unit: g` + `weight_alt: "1 whole"` |

**Separable whole ingredients** (onion, lemon, garlic clove, etc.):
- Use `unit: g` as the primary quantity. Grams scale correctly and enable nutrition calculation.
- Add `weight_alt: "1 whole"` (or `"1 clove"`, `"1 wedge"`, etc.) as the whole-unit display hint. It scales in **1/4 increments** — `1/4 whole`, `1/2 whole`, `1`, `1 1/4 whole`, `1 1/2 whole` — so cooks without a scale know how many to grab at any yield.

✅ **CORRECT — separable:**
```yaml
- quantity: 110
  unit: g
  name: Yellow Onion
  nutrition_id: "..."
  weight_alt: "1 whole"
```

**Non-separable ingredients** (egg, canned item):
- Use `unit: g` with the gram weight of a **single standard unit** (e.g., 56g for 1 large egg).
- Add `weight_alt` using `pcs` (pieces) as the unit together with the count for the recipe. This is an integer-only unit — the scaler always rounds to the nearest whole number (never shows "1 1/2 pcs"). The ingredient **name** carries the size/type (`Large Eggs`, `Medium Eggs`).
- **Why not `"whole"`?** `"whole"` rounds in 1/4 steps (correct for onions). Non-separable items need integer rounding — once an egg is cracked the shell is gone.
- **Why not `note`?** A `note` field is static and does **not** scale — at 2× it would still say "approximately 6 large eggs", which is wrong. Use `weight_alt` instead.
- **Why grams and not a direct count?** Once you crack an egg, the shell is gone — you cannot store the remainder. Grams let the recipe scale to any multiplier and the cook reads the `weight_alt` count to know how many to open.

✅ **CORRECT — non-separable (6 large eggs for full recipe):**
```yaml
- quantity: 300
  unit: g
  name: Large Eggs
  weight_alt: "6 pcs"
```

At 2×: displays `(≈ 12 pcs)`. At 0.5×: `(≈ 3 pcs)`. At 0.25×: `(≈ 2 pcs)` — rounds up from 1.5, never fractional.

### The volume_alt Rule

**If a spice ingredient has `quantity` less than 10 AND `unit` is `g`, you MUST add a `volume_alt` field.**

This is NOT optional. Many kitchen scales cannot read below 1g accurately.
Look up the conversion in [SPICE_CONVERSIONS.md](../../references/SPICE_CONVERSIONS.md).

✅ **CORRECT:**
```yaml
- quantity: 3
  unit: g
  name: Black Pepper
  volume_alt: "3/4 tsp."
```

❌ **WRONG — missing volume_alt:**
```yaml
- quantity: 3
  unit: g
  name: Black Pepper
```

❌ **WRONG — tsp as primary unit:**
```yaml
- quantity: 0.75
  unit: tsp.
  name: Black Pepper
```

### Ingredient Notes

Use the optional `note` field for substitution tips or context:

```yaml
- quantity: 907
  unit: g
  name: 88/12 Ground Beef
  note: "For a fattier result, use 80/20 or 70/30."
```

Use the optional `doc_link` field when an ingredient is itself another recipe in the repository:

```yaml
- quantity: 1
  unit: whole
  name: Kebab Meat Recipe (full batch)
  doc_link: ./Kebab_Meat.yaml
```

**RULE:** Any ingredient that refers to another recipe in this repository **must** include a `doc_link` field with the relative path to that recipe's `.yaml` file. Use the same relative-path conventions as the `related` field (`./`, `../`, `../<Folder>/File.yaml`). Whenever you add, change, or remove `doc_link` rules here, you **must also** update the Cross-References checklist item in `.github/skills/recipe-validation/SKILL.md` so the validation pass explicitly enforces this requirement.

---

## Instructions

### Structure

```yaml
instructions:
  - heading: null           # null = main steps
    type: sequence          # ALWAYS runs in order
    steps:
      - text: "Step one"
      - text: "Step two"
        notes:
          - "A tip or clarification"
```

> **Explicitness rule:** Write every step as if directing a complete novice or a robot. Do not assume the reader will fill in logical gaps. Opening containers, measuring, transferring between bowls, and any preparation actions must be stated explicitly.

### type Field

Every instruction section MUST have a `type` field:

| `type` | Meaning | When to Use |
|---|---|---|
| `sequence` | Steps always run in order | Default for most sections |
| `branch` | One of several options — cook picks one | For variations (Grilled vs. Baked) |

### Branching Paths

When a recipe has multiple cooking methods, use `type: branch` with a `branch_group`:

```yaml
  - heading: Grilled
    type: branch
    branch_group: cooking-method
    steps:
      - text: "Preheat grill to 218°C (425°F)"

  - heading: Baked
    type: branch
    branch_group: cooking-method
    steps:
      - text: "Preheat oven to 218°C (425°F)"
```

**RULE:** Sections with the same `branch_group` are mutually exclusive. The cook picks ONE.
**RULE:** Each branch must be complete on its own. Do not assume steps from another branch.

### Optional Sections

Serving and Freezing sections use `optional: true`:

```yaml
  - heading: Freezing
    type: sequence
    optional: true
    steps:
      - text: "Flash freeze on a baking sheet for 2 hours"
```

### Temperature Format

**ALWAYS** write temperatures as `°C (°F)`.

✅ `218°C (425°F)`
✅ `74°C (165°F)`
❌ `425°F`
❌ `218°C`
❌ `425 degrees`

### Food Safety Temperatures

**ALWAYS** state the minimum safe internal temperature for meat:

| Meat Type | Minimum Temperature |
|---|---|
| Poultry (chicken, turkey) | 74°C (165°F) |
| Ground meat (beef, pork, lamb) | 71°C (160°F) |
| Whole cuts (beef, pork, lamb) | 63°C (145°F) |

---

## Cross-Linking

Use the `related` field to link to other recipe files:

```yaml
related:
  - label: Kebab Meat Recipe
    path: ./Kebab_Meat.yaml
```

**RULES:**
- Use `./` for same-directory files
- Use `../` for parent-directory files
- Use `../<Folder>/File.yaml` for sibling-directory files
- **ALWAYS** use `.yaml` extension, never `.md`
- **ALWAYS** use relative paths, never absolute paths

---

## File Placement

| Recipe Status | Folder | Meaning |
|---|---|---|
| `stable` | `Recipes/` | Tested and finalized |
| `beta` | `Recipes/Beta/` | Works but needs refinement |
| `draft` | `Recipes/Beta/` | Incomplete — missing steps or unvalidated quantities |
| Any (grouped) | `Recipes/<Topic>/` | Two or more related recipes (e.g. `Brisket/`) |

**Filename:** `Title_Case_With_Underscores.yaml`

✅ `Chicken_Shawarma.yaml`
❌ `chicken-shawarma.yaml`
❌ `chicken_shawarma.md`

---

## References

- [STRUCTURED_FORMAT.md](../../references/STRUCTURED_FORMAT.md) — Full YAML schema with complete examples
- [SPICE_CONVERSIONS.md](../../references/SPICE_CONVERSIONS.md) — Gram-to-volume conversion table for all spices

---

## Cut Styles Reference

When a recipe says to cut an ingredient, use the correct term and include an approximate size so there is no ambiguity.

| Cut Style | Description | Approximate Size |
|---|---|---|
| Rough chop | Irregular, uneven pieces | ~2–3 cm (¾–1 in) |
| Chopped | Roughly uniform pieces | ~2 cm (¾ in) |
| Diced | Uniform cubes | ~1 cm (⅜ in) |
| Small dice | Smaller uniform cubes | ~6 mm (¼ in) |
| Julienned | Thin matchstick strips | ~3 mm × 3 mm × 5 cm |
| Chiffonade | Thin ribbons (leafy herbs/greens) | ~2–3 mm wide |
| Minced | Very finely cut | ~2 mm (⅛ in) |
| Brunoise | Tiny uniform cubes | ~3 mm (⅛ in) |
| Sliced | Flat cuts across the ingredient | Specify thickness |

**Rule:** Always pair the cut style with an approximate metric size in the step text so the reader doesn't have to guess.

---

## Quick Self-Check (Run After Every Edit)

Before saving any recipe file, answer every question below. If any answer is "no", fix it.

1. Is the file extension `.yaml`? (Not `.md`, not `.json`)
2. Are ALL 5 required fields present? (`name`, `version`, `author`, `description`, `status`)
3. Is `version` a quoted string? (`"1.0"` not `1.0`)
4. Are all units `g`, `ml`, or count words? (No cups, lbs, oz, tsp, tbsp)
5. Does every spice under 10g have a `volume_alt` field?
6. Are all temperatures written as `°C (°F)`?
7. Does every instruction section have a `type` field?
8. Is the filename `Title_Case_With_Underscores.yaml`?
