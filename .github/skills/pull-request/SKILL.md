---
name: pull-request
description: How to fill in the PULL_REQUEST_TEMPLATE.md when creating or updating a PR in OpenCookbook. Covers the PR Title Check, description, type of change, checklist generation, and related issues.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
---

# Pull Request Descriptions

## When to Use This Skill

Use this when:
- Creating a new pull request
- Updating an existing PR description
- Reviewing whether a PR description is complete

---

## The Template Scaffold

The file `.github/PULL_REQUEST_TEMPLATE.md` provides the sections. You must fill in every section. Do not leave any section with only its comment placeholder.

---

## Section-by-Section Rules

### ⚠️ PR Title Check

State explicitly whether the PR title follows `<type>(<scope>): <description>` format.

**Write one of these:**
- Yes — `feat(recipes): add Kiev_Cutlet beta recipe`
- No — current title is "Update recipes" → should be `fix(recipes): correct volume_alt on Brisket_Rub`

**Valid types for this repo:**

| Type | When to use |
|---|---|
| `feat` | New recipe, new field, new feature, new skill |
| `fix` | Correcting an error in a recipe, workflow, or code |
| `docs` | Skill files, README, lessons, RULES.md only — no recipe YAML changes |
| `style` | Formatting only, no logic change |
| `chore` | Maintenance — dependency updates, repo config |
| `ci` | GitHub Actions workflow changes |
| `refactor` | Code restructure with no behavior change |

**Valid scopes for this repo:**

| Scope | When to use |
|---|---|
| `recipes` | Any `.yaml` recipe file change |
| `skills` | Any skill file change |
| `ci` | Any `.github/workflows/` change |
| `website` | Any `visualizer/` change |
| `docs` | README, RULES.md, SCHEMA.md, lessons.md |

A PR touching multiple scopes uses the primary scope. If truly mixed, omit the scope.

---

### Description

2–4 sentences covering:
1. What changed
2. Why it changed
3. Any notable decisions or trade-offs

Do not write a bullet list here. Write prose.

---

### Type of Change

Tick every checkbox that applies. More than one is allowed.

- New recipe or recipe update → tick when any `.yaml` recipe file is new or changed
- Bug fix → tick when correcting an error (wrong quantity, bad step, broken workflow)
- New feature → tick when adding new capability to the site, CI, or recipe format
- Documentation / skills update → tick when only `.md` skill/reference files changed
- CI / workflow change → tick when `.github/workflows/` files changed
- Other → tick and describe if none of the above fit

---

### Checklist

**Do not copy a fixed list.** Generate only the items that apply to what this PR actually touches.

#### Recipe checklist items (use when `.yaml` recipe files changed)

Pull items from the recipe-validation skill that are relevant to the specific recipe(s) changed. At minimum:

- [ ] Ran full validation checklist from recipe-validation skill on every changed recipe
- [ ] All weights in `g` or `ml` — no imperial units
- [ ] Spices under 10g have `volume_alt`
- [ ] All temperatures are dual-format: `°C (°F)`
- [ ] `name`, `version`, `author`, `description`, `status` all present
- [ ] `notes` absent on any `stable` recipe
- [ ] Filename: `Title_Case_With_Underscores.yaml`
- [ ] File in correct folder for its status
- [ ] All instruction steps are explicit — no assumed knowledge (R3.7)
- [ ] Version bumped per recipe-versioning skill (if this merges to main)

Add sub-recipe `doc_link` item only if this PR touches a recipe that references another recipe.

#### Website checklist items (use when `visualizer/` files changed)

- [ ] `dotnet build` passes locally
- [ ] `dotnet test` passes locally

#### CI checklist items (use when `.github/workflows/` files changed)

- [ ] Workflow YAML is syntactically valid
- [ ] Trigger branches match the repo's default branch (`main`)
- [ ] Permissions are scoped to minimum required
- [ ] Concurrency group is set if the workflow deploys or mutates shared state
- [ ] New workflow follows naming and structure conventions in the ci-workflows skill

#### Skills / docs checklist items (use when skill files changed)

- [ ] Skill frontmatter `description` is a single-line string (no `>` multiline)
- [ ] Skill is listed in the skills table in `copilot-instructions.md`
- [ ] No duplication with existing skills — detail lives in exactly one place

---

### Related Issues

List every issue this PR resolves using `Closes #N` — one per line. GitHub will auto-close them on merge.

If the PR is partial work on an issue (not fully resolving it), use `Related to #N` instead.

---

## Validation Before Submitting

- [ ] No section is empty or contains only its comment placeholder
- [ ] PR title passes the `<type>(<scope>): <description>` check
- [ ] Checklist contains only items relevant to what actually changed
- [ ] Every closed issue is linked with `Closes #N`
