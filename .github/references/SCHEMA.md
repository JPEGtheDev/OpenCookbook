# Recipe Data Schema

Canonical data schema for OpenCookbook recipe files. All recipes are YAML (`.yaml`).
This schema defines the structure used for rendering, validation, and data export.

---

## Top-Level: `Recipe`

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string | Yes | Recipe display name |
| `version` | string | Yes | Version string, **quoted** (e.g. `"1.0"`) |
| `author` | string | Yes | `First Last \| Handle` |
| `description` | string | Yes | 1–3 sentence overview |
| `status` | `stable` \| `beta` \| `draft` | Yes | Recipe readiness level |
| `ingredients` | IngredientGroup[] | Yes | At least one group with at least one item |
| `utensils` | UtensilGroup[] | No | Equipment lists |
| `instructions` | Section[] | Yes* | *Empty `[]` allowed only when `status: draft` |
| `related` | RelatedRecipe[] | No | Cross-links to other recipe files |
| `notes` | string[] | No | Open questions — only for `beta` or `draft` |

---

## `IngredientGroup`

| Field | Type | Required | Description |
|---|---|---|---|
| `heading` | string \| null | Yes | `null` for default group; descriptive string for sub-groups |
| `items` | Ingredient[] | Yes | All ingredients in this group |

---

## `Ingredient`

| Field | Type | Required | Description |
|---|---|---|---|
| `quantity` | number | Yes | Amount in grams, ml, or count |
| `unit` | string | Yes | `g`, `ml`, or count word (`cloves`, `sprigs`, `whole`) |
| `name` | string | Yes | Ingredient name |
| `volume_alt` | string | Conditional | **Required** when `quantity < 10` and `unit = g`. Tsp/tbsp fallback. |
| `note` | string | No | Substitution tips or ingredient-level context |

---

## `UtensilGroup`

| Field | Type | Required | Description |
|---|---|---|---|
| `heading` | string \| null | Yes | `null` for default group; string for sub-groups |
| `items` | string[] | Yes | List of utensil names |

---

## `Section`

| Field | Type | Required | Description |
|---|---|---|---|
| `heading` | string \| null | Yes | `null` for main steps; descriptive string for named sections |
| `type` | `sequence` \| `branch` | Yes | `sequence` = always runs; `branch` = one of several options |
| `branch_group` | string | Conditional | **Required** when `type = branch`. Groups mutually exclusive paths. |
| `optional` | boolean | No | `true` for optional sections (Serving, Freezing) |
| `steps` | Step[] | Yes | All steps in this section |

---

## `Step`

| Field | Type | Required | Description |
|---|---|---|---|
| `text` | string | Yes | The instruction text |
| `notes` | string[] | No | Tips, clarifications, or warnings for this step |

---

## `RelatedRecipe`

| Field | Type | Required | Description |
|---|---|---|---|
| `label` | string | Yes | Display text for the link |
| `path` | string | Yes | Relative file path to the `.yaml` file |

---

## Example: Kebab Meatballs

```yaml
name: Kebab Meatballs
version: "1.1"
author: Jonathan Petz | JPEGtheDev
description: >
  A continuation of the Kebab Meat recipe to make the meat into meatballs
  that you can make in bulk to eat now or freeze later.
status: beta

ingredients:
  - heading: null
    items:
      - quantity: 907
        unit: g
        name: Ground Beef
        note: "Or 1x Kebab Meat Recipe"
      - quantity: 40
        unit: g
        name: Panko Bread Crumbs

utensils:
  - heading: null
    items:
      - Mixing Bowl
      - Baking Sheet

instructions:
  - heading: null
    type: sequence
    steps:
      - text: "Preheat oven to 204°C (400°F)"
      - text: "Mix breadcrumbs into Kebab Meat"
      - text: "Form into 24 equal-sized meatballs"
      - text: "Space meatballs evenly on baking sheet"
      - text: "Bake for 18-20 minutes until internal temperature reaches 71°C (160°F)"

  - heading: Freezing
    type: sequence
    optional: true
    steps:
      - text: "After meatballs have cooled, place uncovered tray in freezer"
        notes:
          - "Ensure meatballs do not touch each other"
      - text: "Freeze for 1-2 hours until pieces no longer stick together"
      - text: "Transfer to a sealed freezer bag or vacuum-sealed bag"
        notes:
          - "Stores up to 3 months in a standard freezer bag"

related:
  - label: Kebab Meat Recipe
    path: ./Kebab_Meat.yaml
```

---

## Rendering Output Targets

| Target | Format | Notes |
|---|---|---|
| Web page | HTML + CSS | Two-column layout; tab-based branch switching |
| PDF | Print-ready document | Single column; page break rules apply |
| Structured data export | JSON | Direct serialization from YAML |
