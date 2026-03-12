# OpenCookbook Structured Recipe Format

This document defines the YAML schema used to represent a recipe as structured data.
Rendering (web, PDF) is out of scope here — this schema defines the data layer only.

**YAML (`.yaml`) is the primary format and source of truth for all migrated recipes.**
Markdown (`.md`) files are the authoring format for recipes that have not yet been migrated.

> **Migration status:** All recipes have been migrated to YAML. There are no remaining `.md` recipe files.

---

## Why YAML?

| Property | Why It Matters |
|---|---|
| Human-readable | Authors can read and edit YAML directly without tooling |
| Multiline strings | Recipe descriptions and notes survive cleanly |
| Nested structures | Handles ingredient groups, instruction sections, and branching paths naturally |
| Widely supported | Parseable in every major language without custom tooling |
| Less ceremony than JSON | No quoting every key, no trailing comma issues |

---

## Branching Paths

Recipes often have multiple valid cooking methods (Grilled vs. Baked, Fancy vs. Regular).
These are represented as instruction `sections` with a `type` field:

| `type` | Meaning |
|---|---|
| `sequence` | Always executed, in order |
| `branch` | One of several options in the same `branch_group` — the cook picks one |

Sections sharing the same `branch_group` string are mutually exclusive alternatives.
Sections without a `branch_group` (or with `type: sequence`) are always run.

---

## Schema

### Top-Level Recipe Object

```yaml
name: string                  # Recipe display name
version: string               # "1.0", "1.1", etc.
author: string                # "First Last | Handle"
description: string           # Multi-sentence overview
status: stable | beta | draft # stable = tested/finalized; beta = usable but being refined; draft = incomplete
ingredients:
  - (IngredientGroup)
utensils:                     # Optional
  - (UtensilGroup)
instructions:
  - (Section)
related:                      # Optional — linked recipes
  - (RelatedRecipe)
```

---

### IngredientGroup

```yaml
ingredients:
  - heading: null             # null = default group; string = sub-group label
    items:
      - (Ingredient)
  - heading: "Fancy Additions"
    items:
      - (Ingredient)
```

---

### Ingredient

Spice quantities under 10g must include `volume_alt` as a fallback for cooks without a precise scale.

```yaml
- quantity: 300               # Always in grams (or ml for liquids)
  unit: g                     # g | ml (base unit only)
  name: Heavy Whipping Cream

- quantity: 3                 # grams
  unit: g
  name: Black Pepper
  volume_alt: "3/4 tsp."      # Required when quantity < 10g

- quantity: 2                 # count-based ingredient
  unit: cloves
  name: Garlic

- quantity: 907               # ingredient with a contextual note
  unit: g
  name: 88/12 Ground Beef
  note: "For a fattier result, use 80/20 or 70/30."

- quantity: 1                 # ingredient that is itself another recipe
  unit: whole
  name: Kebab Meat Recipe (full batch)
  doc_link: ./Kebab_Meat.yaml  # relative path to the linked recipe file
```

**Unit rules:**
- `g` — all solid ingredients
- `ml` — liquids where volumetric measure is standard
- `cloves`, `sprigs`, `whole` — count-based ingredients with no weight
- No pounds, cups, ounces, or teaspoons as a primary unit

**Optional fields:**
- `volume_alt` — required when `quantity < 10` and `unit = g`; volumetric fallback for cooks without a precise scale
- `note` — optional; ingredient-level context (substitution suggestions, fat ratio guidance, etc.)
- `doc_link` — optional in general; **REQUIRED** when this ingredient is itself another recipe in the repo; relative path to the linked recipe file (e.g. `./Kebab_Meat.yaml`)

---

### UtensilGroup

```yaml
utensils:
  - heading: null
    items:
      - Large Pot
      - Strainer / Colander
  - heading: "Fancy Additions"
    items:
      - Sauce Pan
      - Fine Mesh Strainer
```

---

### Section

```yaml
instructions:
  # Top-level steps run always, in order
  - heading: null
    type: sequence
    steps:
      - (Step)

  # Branching paths — cook picks one from the same branch_group
  - heading: Grilled
    type: branch
    branch_group: cooking-method
    steps:
      - (Step)

  - heading: Baked
    type: branch
    branch_group: cooking-method
    steps:
      - (Step)

  # Always-run sections after the branch
  - heading: Serving
    type: sequence
    steps:
      - (Step)

  - heading: Freezing
    type: sequence
    optional: true
    steps:
      - (Step)
```

---

### Step

```yaml
steps:
  - text: "Preheat oven to 218°C (425°F)"
    notes: []

  - text: "Place chicken skin side down and bake for 8 minutes"
    notes:
      - "If using a convection oven, reduce temperature by 14°C (25°F)"
      - "Do not open the oven door during the first 8 minutes"
```

---

### RelatedRecipe

```yaml
related:
  - label: Kebab Meat Recipe
    path: ./Kebab_Meat.yaml
```

---

## Full Example: Chicken Shawarma

```yaml
name: Grilled / Baked Chicken Shawarma Meat
version: "1.1"
author: Jonathan Petz | JPEGtheDev
description: >
  While not real Chicken Shawarma, it comes close to the real deal and is
  practical when cooking for a large number of people. Marinated in a spiced
  buttermilk brine and cooked on the grill or in the oven.
status: beta

ingredients:
  - heading: null
    items:
      - quantity: 1300
        unit: g
        name: Boneless Skinless Chicken Thighs
      - quantity: 2
        unit: g
        name: Black Pepper
        volume_alt: "1/2 tsp."
      - quantity: 1
        unit: g
        name: Garlic Powder
        volume_alt: "1/4 tsp."
      - quantity: 2
        unit: g
        name: Onion Powder
        volume_alt: "3/4 tsp."
      - quantity: 1
        unit: g
        name: Cumin
        volume_alt: "1/2 tsp."
      - quantity: 2
        unit: g
        name: Paprika
        volume_alt: "1 tsp."
      - quantity: 0.3
        unit: g
        name: Ground Coriander
        volume_alt: "1/8 tsp."
      - quantity: 5
        unit: g
        name: Fine Sea Salt
        volume_alt: "3/4 tsp."
      - quantity: 1
        unit: g
        name: Cayenne Pepper
        volume_alt: "1/2 tsp."
      - quantity: 0.5
        unit: g
        name: Red Pepper Flakes
        volume_alt: "1/4 tsp."
      - quantity: 55
        unit: ml
        name: Lemon Juice
      - quantity: 250
        unit: ml
        name: Buttermilk

utensils:
  - heading: null
    items:
      - Tongs (Grilled)
      - Mixing bowl
      - Temperature Probe / Thermometer
      - Baking sheet (Baked)
      - Wire rack (Baked)

instructions:
  - heading: Preparing the Chicken
    type: sequence
    steps:
      - text: Combine all spices in your bowl
      - text: Add half the buttermilk and all the lemon juice, stir to combine
      - text: Add your chicken thighs and mix
      - text: Add remaining buttermilk to cover the chicken and mix again
      - text: Cover and refrigerate for 8 to 72 hours
        notes:
          - The longer you marinate, the better the flavor. The more past-expiration-date your buttermilk is, the tangier it will taste.
          - Do not use buttermilk that is moldy, off-color, or no longer resembles buttermilk. Use your own judgement if it is past the expiration date.

  - heading: Grilled
    type: branch
    branch_group: cooking-method
    steps:
      - text: Preheat grill to 218°C (425°F) and ensure grates are fully heated
      - text: Place chicken skin side down and cook for 8 minutes
      - text: Flip and cook for another 8 minutes
      - text: Check internal temperature — must reach 74°C (165°F). If not, cook in 3-minute increments until it does.

  - heading: Baked
    type: branch
    branch_group: cooking-method
    steps:
      - text: Preheat oven to 218°C (425°F)
      - text: Place chicken skin side down on a wire rack over a baking sheet and bake for 8 minutes
      - text: Flip and bake for another 8 minutes
      - text: Check internal temperature — must reach 74°C (165°F). If not, bake in 3-minute increments until it does.

  - heading: Serving
    type: sequence
    steps:
      - text: Let chicken rest for 8 minutes before slicing
        notes:
          - Resting allows the juices to redistribute and stay in the meat when cut.
      - text: Slice into cubes and serve

  - heading: Freezing
    type: sequence
    optional: true
    steps:
      - text: Flash freeze individual cubes on a baking sheet, ensuring pieces do not touch
      - text: Freeze for approximately 3 hours or until pieces no longer stick together
      - text: Transfer to a sealed freezer bag or vacuum-sealed bag
        notes:
          - Stores up to 3 months in a standard freezer bag; longer in a vacuum-sealed bag.
```

---

## Relationship Between Markdown and YAML

YAML is the canonical format for migrated recipes. Markdown is the authoring format
for unmigrated stable recipes only. There is no Markdown companion file for any recipe
that has a YAML file.

The `branch_group` concept in YAML maps directly to sibling `###` sub-sections that
represent mutually exclusive cooking methods in the source material. If two sections
are alternatives (pick one), they share a `branch_group` string.

---

## File Naming

YAML files use the same `Title_Case_With_Underscores` convention as the Markdown files they replaced:

```
Recipes/
  Perfect_Mashed_Potatoes.yaml
Recipes/Beta/
  Chicken_Shawarma.yaml
  Kebab_Meat.yaml
  Kebab_Meatballs.yaml
  Chicken_Wings.yaml
Recipes/Brisket/
  Guajillo_Brisket_Rub.yaml
  Guajillo_Brisket_Binder.yaml
```

There is never both a `.md` and `.yaml` file for the same recipe.
