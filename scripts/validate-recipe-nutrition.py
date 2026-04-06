#!/usr/bin/env python3
"""
Validate that all recipe ingredients with nutrition_id match the nutrition database.
This script is intended to run as a pre-commit hook.

Exit codes:
  0: All validations passed
  1: Validation errors found (details printed to stderr)
  2: Script error (file not found, invalid JSON, etc.)
"""

import json
import sys
import yaml
from pathlib import Path


def find_repo_root():
    """Find the repository root by looking for .git directory."""
    current = Path.cwd()
    while current != current.parent:
        if (current / ".git").exists():
            return current
        current = current.parent
    raise FileNotFoundError("Could not find repository root (.git directory)")


def load_nutrition_db(repo_root):
    """Load the nutrition database from docs/data/nutrition-db.json."""
    db_path = repo_root / "docs" / "data" / "nutrition-db.json"
    if not db_path.exists():
        raise FileNotFoundError(f"Nutrition database not found at {db_path}")
    
    with open(db_path) as f:
        entries = json.load(f)
    
    # Create lookup: id -> name
    return {entry["id"]: entry["name"] for entry in entries}


def load_recipes(repo_root):
    """Load all recipe YAML files from Recipes/ and Recipes/Beta/."""
    recipes_dir = repo_root / "Recipes"
    recipes = []
    
    if recipes_dir.exists():
        for yaml_file in recipes_dir.rglob("*.yaml"):
            try:
                with open(yaml_file) as f:
                    recipe = yaml.safe_load(f)
                recipes.append((yaml_file, recipe))
            except Exception as e:
                print(f"ERROR: Failed to load {yaml_file}: {e}", file=sys.stderr)
                return None
    
    return recipes


def validate_recipes(recipes, nutrition_db):
    """
    Validate all recipes against the nutrition database.
    
    Returns:
      (is_valid, error_messages) tuple
    """
    errors = []
    
    for yaml_file, recipe in recipes:
        if not recipe or "ingredients" not in recipe:
            continue
        
        recipe_name = recipe.get("name", yaml_file.name)
        
        for group_idx, group in enumerate(recipe.get("ingredients", [])):
            for item_idx, item in enumerate(group.get("items", [])):
                ingredient_name = item.get("name", f"[Unknown at index {item_idx}]")
                nutrition_id = item.get("nutrition_id")
                
                if not nutrition_id:
                    # No nutrition_id: that's fine (some ingredients might not have data)
                    continue
                
                # Check that nutrition_id exists in DB
                if nutrition_id not in nutrition_db:
                    errors.append(
                        f"{yaml_file.name} ({recipe_name}), "
                        f"ingredient '{ingredient_name}': "
                        f"nutrition_id '{nutrition_id}' not found in nutrition database"
                    )
                    continue
                
                # Check that ingredient name matches DB name
                db_name = nutrition_db[nutrition_id]
                if ingredient_name != db_name:
                    errors.append(
                        f"{yaml_file.name} ({recipe_name}), "
                        f"ingredient '{ingredient_name}': "
                        f"name does not match nutrition DB entry '{db_name}' "
                        f"(nutrition_id: {nutrition_id})"
                    )
    
    return len(errors) == 0, errors


def main():
    """Main entry point for the validation script."""
    try:
        repo_root = find_repo_root()
    except FileNotFoundError as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return 2
    
    try:
        nutrition_db = load_nutrition_db(repo_root)
    except (FileNotFoundError, json.JSONDecodeError) as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return 2
    
    try:
        recipes = load_recipes(repo_root)
    except Exception as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return 2
    
    if recipes is None:
        return 2
    
    is_valid, errors = validate_recipes(recipes, nutrition_db)
    
    if not is_valid:
        print("❌ Recipe validation FAILED:", file=sys.stderr)
        for error in errors:
            print(f"  • {error}", file=sys.stderr)
        print(
            "\nℹ️  Ensure ingredient names match exactly with the nutrition database.",
            file=sys.stderr
        )
        return 1
    
    print(f"✓ Recipe validation passed ({len(recipes)} recipes checked)", file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main())
