# Recipe Review Rules

**Purpose:** Define how an independent recipe review agent validates recipes without bias or author influence.

## Review Agent Authority

The Recipe Review Agent operates **independently** with:
- No author communication during review
- No pre-approval or veto power from author
- Clear, objective checklist-based evaluation
- Output: Pass/Fail with detailed findings
- Escalation: Blocked recipes go to QA or maintainer queue

## Review Dispatch Rules

### When to Trigger Review
A recipe review is **mandatory** when:
1. **New recipe** created (before merge to `main`)
2. **Major version bump** (e.g., 1.0 → 2.0) on an existing recipe
3. **Status change**: `draft` → `beta` → `stable`
4. **Significant edits**: Ingredients changed, instructions rewritten, yields adjusted by >20%

### When to Skip Review
- Minor typo fixes (no logic/yield changes)
- Metadata updates (tags, notes only)
- Patch-only version bump within same minor version (1.0.0 → 1.0.1)

## Review Checklist

**NOTE:** Validation rules are defined in `.github/skills/recipe-validation/SKILL.md`. The review agent uses that checklist as the source of truth.

## Review Agent Output

### Pass
✅ Recipe is valid and ready for merge.

### Fail
❌ Recipe has issues. Provide:
1. **Blocking issues** (must fix): e.g., missing required field, invalid units
2. **Minor issues** (should fix): e.g., typo, unclear step
3. **Suggestions** (nice-to-have): e.g., add a related recipe link

## Example Review Output

```
Recipe: Shrimp_Scampi.yaml
Status: ❌ NEEDS REVISION

Blocking Issues:
  - Ingredient "Pasta Water" uses cups as the unit; convert to g or ml per the validation checklist
  - Step 5 refers to "timer" without clarifying how many minutes total

Minor Issues:
  - Typo: "al dente" appears once as "al dente" (consistent, no change needed)
  - Step 3: Add salt amount reference (quantity in grams for accuracy)

Suggestions:
  - Consider adding "serving size" field to calculate servings per batch
  - Link to pairing wine selection

Reviewer: RecipeReviewAgent | Timestamp: 2026-04-05T22:28:00Z
```

## Dispatch Mechanism

There is currently **no repository-managed workflow, script, or pre-commit configuration** that dispatches recipe review automatically.

Until an implementation is added, treat recipe review as a **manual maintainer or agent process** performed before merge when the trigger conditions above apply.

Possible future integration points include:
1. **Manual CLI entry point**: `copilot exec recipe-review <recipe-file-path>`
2. **PR automation**: A workflow that runs review during pull request validation
3. **Pre-commit integration**: A local hook that runs review before pushing (optional)

If and when one of these mechanisms is implemented, this section should link to the corresponding workflow, script, or configuration location in the repository.

Expected output conventions for a review tool or agent run:
- Exit code `0`: Pass
- Exit code `1`: Fail (needs revision)
- Exit code `2`: Error (agent failure)
