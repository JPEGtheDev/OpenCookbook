#!/usr/bin/env python3
"""
Validate that all recipe ingredients with nutrition_id match the nutrition database.
This script is intended to run as a pre-commit hook.

Checks performed:
  1. All nutrition_id entries exist in the database
  2. Ingredient names match database entries exactly
  3. No duplicate IDs in the nutrition database
  4. No duplicate names in the nutrition database
  5. Every ingredient with nutrition_id references valid entries

Exit codes:
  0: All validations passed
  1: Validation errors found (details printed to stderr)
  2: Script error (file not found, invalid JSON, etc.)

Prerequisites:
  - PyYAML: Install with `pip install pyyaml==6.0.2`
"""

import json
import subprocess
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
    return {entry["id"]: entry["name"] for entry in entries}, entries


def validate_nutrition_db(db_entries):
    """
    Validate the nutrition database itself for internal consistency.
    
    Returns:
      (is_valid, error_messages) tuple
    """
    errors = []
    
    # Check for duplicate IDs
    ids = [entry["id"] for entry in db_entries]
    id_set = set(ids)
    if len(ids) != len(id_set):
        from collections import Counter
        duplicates = [id for id, count in Counter(ids).items() if count > 1]
        for dup_id in duplicates:
            names = [e["name"] for e in db_entries if e["id"] == dup_id]
            errors.append(
                f"nutrition-db.json: Duplicate ID '{dup_id}' found in entries: {names}"
            )
    
    # Check for duplicate names
    names = [entry["name"] for entry in db_entries]
    name_set = set(names)
    if len(names) != len(name_set):
        from collections import Counter
        duplicates = [name for name, count in Counter(names).items() if count > 1]
        for dup_name in duplicates:
            ids = [e["id"] for e in db_entries if e["name"] == dup_name]
            errors.append(
                f"nutrition-db.json: Duplicate name '{dup_name}' found in IDs: {ids}"
            )
    
    # Check that all entries have complete nutrition data (per_100g with at least calories)
    for entry in db_entries:
        nutrition = entry.get("per_100g", {})
        if not isinstance(nutrition, dict) or "calories_kcal" not in nutrition:
            errors.append(
                f"nutrition-db.json: Entry '{entry['name']}' ({entry['id']}) "
                f"missing calories_kcal in per_100g nutrition data"
            )
    
    return len(errors) == 0, errors


def load_recipes(repo_root):
    """Load all recipe YAML files from Recipes/ and Recipes/Beta/."""
    recipes_dir = repo_root / "Recipes"
    recipes = []
    
    if recipes_dir.exists():
        for yaml_file in recipes_dir.rglob("*.yaml"):
            try:
                with open(yaml_file) as f:
                    recipe = yaml.safe_load(f)
                rel_path = yaml_file.relative_to(repo_root)
                recipes.append((rel_path, recipe))
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
    
    for rel_path, recipe in recipes:
        if not recipe or "ingredients" not in recipe:
            continue
        
        recipe_name = recipe.get("name", rel_path.name)
        
        for group in recipe.get("ingredients", []):
            for item_idx, item in enumerate(group.get("items", [])):
                ingredient_name = item.get("name", f"[Unknown at index {item_idx}]")
                nutrition_id = item.get("nutrition_id")
                
                if not nutrition_id:
                    # No nutrition_id: that's fine (some ingredients might not have data)
                    continue
                
                # Check that nutrition_id exists in DB
                if nutrition_id not in nutrition_db:
                    errors.append(
                        f"{rel_path} ({recipe_name}), "
                        f"ingredient '{ingredient_name}': "
                        f"nutrition_id '{nutrition_id}' not found in nutrition database"
                    )
                    continue
                
                # Check that ingredient name matches DB name
                db_name = nutrition_db[nutrition_id]
                if ingredient_name != db_name:
                    errors.append(
                        f"{rel_path} ({recipe_name}), "
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
        nutrition_db, db_entries = load_nutrition_db(repo_root)
    except (FileNotFoundError, json.JSONDecodeError) as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return 2
    
    # Validate the nutrition DB itself
    db_valid, db_errors = validate_nutrition_db(db_entries)
    if not db_valid:
        print("❌ Nutrition database validation FAILED:", file=sys.stderr)
        for error in db_errors:
            print(f"  • {error}", file=sys.stderr)
        print(
            "\nℹ️  Fix duplicate IDs and names in docs/data/nutrition-db.json",
            file=sys.stderr
        )
        return 1
    
    try:
        recipes = load_recipes(repo_root)
    except Exception as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return 2
    
    if recipes is None:
        return 2
    
    # Validate recipes against DB
    recipes_valid, recipe_errors = validate_recipes(recipes, nutrition_db)
    
    if not recipes_valid:
        print("❌ Recipe validation FAILED:", file=sys.stderr)
        for error in recipe_errors:
            print(f"  • {error}", file=sys.stderr)
        print(
            "\nℹ️  Ensure ingredient names match exactly with the nutrition database.",
            file=sys.stderr
        )
        return 1
    
    print(f"✓ All validations passed ({len(recipes)} recipes, {len(db_entries)} DB entries)", file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main())
