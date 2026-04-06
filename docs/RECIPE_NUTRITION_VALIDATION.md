# Recipe Nutrition Validation

This document explains how to set up and use the recipe nutrition validation system.

## Why This Exists

When recipes are created with `nutrition_id` fields, it's critical that:
1. The ingredient **name matches exactly** what's in the nutrition database
2. All `nutrition_id` values reference valid entries
3. Ingredients don't have extra descriptions in the name (e.g., "Salt (for pasta water)" should be "Fine Sea Salt")

If names don't match, nutrition data won't display in the web app even though IDs are present.

## Prerequisites

- **Python 3.6+** and **PyYAML**: Install with:
  ```bash
  pip install pyyaml==6.0.2
  ```

## Setup: Install the Pre-Commit Hook

Run this **once** after cloning the repository:

```bash
ln -s ../../scripts/pre-commit-validate.sh .git/hooks/pre-commit
```

This creates a symbolic link so the hook runs automatically before each commit.

## Using the Validation

### Automatic (Pre-Commit)
When you try to commit, the hook will run and block the commit if validation fails:

```bash
$ git commit -m "Add new recipe"
❌ Recipe validation FAILED:
  • Recipes/Beta/MyRecipe.yaml (My Recipe), ingredient 'Salt': 
    name does not match nutrition DB entry 'Fine Sea Salt' (nutrition_id: 5c00ccf3...)
```

### Manual
Run the validation script directly anytime:

```bash
python3 scripts/validate-recipe-nutrition.py
```

## Fixing Errors

If you see a mismatch error, update the ingredient name in your recipe to match the database entry **exactly**.

**Before (incorrect):**
```yaml
- quantity: 50
  unit: g
  name: Salt                          # ❌ Wrong name
  nutrition_id: "5c00ccf3-0bd6..."
```

**After (correct):**
```yaml
- quantity: 50
  unit: g
  name: Fine Sea Salt                 # ✓ Matches DB entry
  nutrition_id: "5c00ccf3-0bd6..."
```

Or remove the `nutrition_id` if you don't want nutrition tracking for that ingredient.

## Finding the Correct Name

Check what names are available in the nutrition database:

```bash
python3 << 'EOF'
import json
with open('docs/data/nutrition-db.json') as f:
    for entry in json.load(f):
        if 'salt' in entry['name'].lower():
            print(f"{entry['name']}: {entry['id']}")
EOF
```

## Bypassing the Hook (Not Recommended)

If you absolutely need to skip the hook for a single commit:

```bash
git commit --no-verify -m "message"
```

**Note:** This is only recommended for emergency fixes. The validation exists for a reason!

## Disabling the Hook

If you want to temporarily disable the hook:

```bash
rm .git/hooks/pre-commit
```

To re-enable it later:

```bash
ln -s ../../scripts/pre-commit-validate.sh .git/hooks/pre-commit
```

