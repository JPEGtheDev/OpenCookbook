---
name: recipe-versioning
description: Versioning rules for OpenCookbook recipes using conventional commits. Defines when and how to bump the version field in a recipe YAML file, and how to format commit messages for recipe changes.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
---

# Recipe Versioning

## When to Use This Skill

Use this when:
- You are about to commit a change to any recipe file
- You need to decide what version number a recipe should have after an edit
- You are writing a commit message for a recipe change

---

## The Core Rule

**Only bump a recipe's `version` field when the change will be merged to `main`.**

- During local edits or drafting, do NOT change the version.
- When working on a branch that will be merged via PR, include the version bump in the branch commits — it will land on `main` when the PR merges.

---

## Version Format

All recipe versions are **two-part quoted strings**: `"Major.Minor"`

```yaml
version: "1.0"   ✅
version: "1.2"   ✅
version: 1.0     ❌  (must be quoted)
version: "1.0.1" ❌  (no patch segment)
```

- **Major** — the left number. Increments on breaking changes.
- **Minor** — the right number. Increments on all other changes.
- Minor resets to `0` when Major is bumped.

---

## Conventional Commit Format

All commits touching recipe files must use the conventional commit format:

```
<type>(<scope>): <short description>

[optional body]

[optional footer — for BREAKING CHANGE only]
```

- **`<type>`** — one of the types listed below
- **`<scope>`** — the recipe filename without extension, e.g. `Kiev_Cutlet`
- **`<short description>`** — imperative, lowercase, no period

### Examples

```
fix(Chicken_Shawarma): correct caraway seed volume_alt
feat(Kiev_Cutlet): add breading section and cooking instructions
docs(Kebab_Meat): clarify onion squeeze technique
fix(Perfect_Mashed_Potatoes): specify salt amount in water step

feat(Kebab_Meat): reformulate spice ratios for stronger flavor

BREAKING CHANGE: quantities have changed significantly from v1.x;
existing batches mixed at the old ratios will taste different.
```

---

## Commit Types and Version Bumps

| Commit Type | What it Means for a Recipe | Version Bump |
|---|---|---|
| `fix` | Corrects an error — typo, wrong unit, missing `volume_alt`, incorrect temperature | Minor (`1.0` → `1.1`) |
| `feat` | Adds something new — new section, new ingredient, new variation/branch | Minor (`1.0` → `1.1`) |
| `docs` | Improves clarity without changing the recipe — rewording steps, adding notes | Minor (`1.0` → `1.1`) |
| `style` | Formatting only — whitespace, YAML indentation, no content change | **No bump** |
| `chore` | Admin/meta — status promotion, file move, validation fixes that don't change recipe content | **No bump** (but update `status` if promoting) |

> **BREAKING CHANGE** — appended as a footer on any type — always causes a **Major bump** (`1.x` → `2.0`).

---

## What Counts as a BREAKING CHANGE?

A breaking change in a recipe means **a cook following the old version would get a meaningfully different result** using the new version.

| Scenario | Breaking? |
|---|---|
| Changed a core ingredient or its quantity significantly | ✅ Yes |
| Changed cooking temperature or time significantly | ✅ Yes |
| Replaced a spice with a different one | ✅ Yes |
| Added an entirely new required ingredient | ✅ Yes |
| Removed an ingredient | ✅ Yes |
| Added an optional section (Serving, Freezing) | ❌ No — `feat` |
| Clarified a step without changing the outcome | ❌ No — `docs` |
| Fixed a typo or missing field | ❌ No — `fix` |
| Added `volume_alt` to an existing spice | ❌ No — `fix` |
| Promoted from `beta` to `stable` | ❌ No — `chore` |

---

## Step-by-Step: How to Version a Recipe Change

1. **Make your edits** to the recipe YAML file.
2. **Determine the commit type** using the table above.
3. **Determine if it is a BREAKING CHANGE** using the table above.
4. **Calculate the new version:**
   - `style` or `chore` → keep current version unchanged
   - `fix`, `feat`, or `docs` → increment Minor by 1
   - Any `BREAKING CHANGE` → increment Major by 1, reset Minor to 0
5. **Update the `version` field** in the recipe YAML before committing.
6. **Write the commit message** using the conventional commit format.

### Quick Version Calculator

| Current | Type | New Version |
|---|---|---|
| `"1.0"` | `fix` / `feat` / `docs` | `"1.1"` |
| `"1.4"` | `fix` / `feat` / `docs` | `"1.5"` |
| `"1.4"` | `BREAKING CHANGE` | `"2.0"` |
| `"2.3"` | `BREAKING CHANGE` | `"3.0"` |
| `"1.0"` | `style` / `chore` | `"1.0"` (no change) |

---

## Multiple Recipes in One Commit

If a single commit touches more than one recipe file:
- Use a generic scope or omit the scope: `fix: correct volume_alt on multiple spices`
- Bump the version in **each affected recipe file** individually according to the change made to that recipe.

---

## Validation

Before committing any recipe change, confirm:

- [ ] `version` field is updated (unless `style` or `chore`)
- [ ] `version` is a quoted two-part string (`"Major.Minor"`)
- [ ] Commit message uses the conventional commit format
- [ ] `BREAKING CHANGE` footer is present if the change is breaking
- [ ] `status` field is correct for the current state of the recipe
