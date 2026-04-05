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
- Version bump within same minor version (1.0 → 1.1)

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
  - Ingredient "Pasta Water" uses ml unit but has no weight_alt for consistency
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

The review agent is invoked via:
1. **Manual trigger**: `copilot exec recipe-review <recipe-file-path>`
2. **PR hook**: Runs automatically on PR review
3. **Pre-commit**: Runs locally before pushing (optional)

Output is posted as a PR comment or returned to stdout with exit code:
- `0`: Pass
- `1`: Fail (needs revision)
- `2`: Error (agent failure)
