---
name: recipe-rendering
description: Data model and layout specification for rendering OpenCookbook recipes on a webpage or as a PDF. Defines how YAML recipe data maps to visual output. Language-agnostic - covers data structure and layout intent, not implementation.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
---

# Recipe Rendering

## When to Use This Skill

Use this when:
- Building a web or print renderer for OpenCookbook recipes
- Defining how recipe YAML data maps to visual output
- Designing a recipe viewer or recipe card layout

This skill defines **data and layout only**. It is language-agnostic.
See [SCHEMA.md](../../references/SCHEMA.md) for the full data schema.

---

## Data Model

Recipe YAML files are the source of truth. A renderer reads the YAML and maps fields to visual output.

### Field Mapping (YAML → Rendered Output)

| YAML Field | Rendered As |
|---|---|
| `name` | Page title / H1 heading |
| `author` | Author byline |
| `version` | Version label |
| `description` | Overview paragraph below the title |
| `status` | Status badge (see below) |
| `ingredients` | Grouped ingredient list (left column on web) |
| `utensils` | Equipment list (collapsible on web) |
| `instructions` | Numbered step list (right column on web) |
| `related` | Links section at the bottom |
| `notes` | Beta/Draft callout box (only rendered for non-stable) |

### Branch Rendering

When `instructions` contains sections with `type: branch`:
- Render branches as **tabs** (web) or **separate labeled blocks** (PDF)
- Sections with the same `branch_group` are alternatives — only one is active at a time
- `type: sequence` sections always render (they are not tabs)

### Status Badge

| Status | Badge |
|---|---|
| `stable` | No badge, or subtle green indicator |
| `beta` | Amber/yellow badge: "Beta — In Progress" |
| `draft` | Red badge: "Draft — Incomplete" |

---

## Web Layout

### Page Structure (top to bottom)

```
[Header]
  Title (large)
  Author | Version | Status badge

[Description]
  1-3 sentence paragraph

[Two-Column Layout]
  Left column (1/3 width):
    Ingredients (grouped by heading)
    Utensils (grouped, collapsible)
  Right column (2/3 width):
    Instructions (by section)
    Branch sections as tabs

[Footer Sections] (full width)
  Serving (if present)
  Freezing (if present)
  Related Recipes (if present)
  Notes (only if status is beta or draft — styled as a callout)
```

### Ingredient Display

- Group heading as a sub-label above the group
- Each ingredient: `quantity unit` in a fixed-width span, `name` in normal weight
- If `volume_alt` exists, show it in parentheses: `3g Black Pepper (≈ 3/4 tsp.)`
- Optional: checkbox prefix for shopping list mode

### Instruction Display

- Steps are numbered
- `notes` array items render as small callouts below the step
- Branch sections render as labeled tabs (one active at a time)

---

## PDF Layout

### Page Structure

```
[Header]
  Title, Author, Version, Date

[Description]

[Ingredients]
  Each group as its own block
  Two-column list if group has 8+ items

[Utensils]  (single list, omit if empty)

[Instructions]
  Each section as a labeled block
  Notes as indented italic text below the step

[Footer Sections]
  Serving, Freezing, Notes — as labeled blocks

[Page Footer]
  "OpenCookbook — CC0 1.0 Universal"
```

### Typography

- Title: large serif/display font, left-aligned
- Section headings: bold, slightly larger than body
- Ingredient quantities: monospace or tabular figures
- Notes: italic, smaller, indented
- Beta callout: bordered box, amber tint

### Print Rules

- Do not break a page in the middle of an instruction section
- Ingredients must not be orphaned from the first instruction
- Cross-reference links print as the file path in parentheses

---

## References

See [SCHEMA.md](../../references/SCHEMA.md) for the full data schema definition.
