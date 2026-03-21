#!/usr/bin/env python3
"""generate-recipe-index.py — Build recipe-index.json from YAML recipe files.

Usage: python3 scripts/generate-recipe-index.py <recipes_dir>
  recipes_dir: path to the recipes folder inside wwwroot output
               (e.g. visualizer/output/wwwroot/recipes)
"""
import glob
import json
import os
import sys

import yaml

recipes_dir = sys.argv[1] if len(sys.argv) > 1 else None
if not recipes_dir:
    print("Usage: generate-recipe-index.py <recipes_dir>", file=sys.stderr)
    sys.exit(1)


def extract_ingredient_names(data):
    names = []
    for group in (data.get("ingredients") or []):
        for item in (group.get("items") or []):
            name = (item.get("name") or "").strip()
            if name:
                names.append(name)
    return names


entries = []
for filepath in sorted(glob.glob(os.path.join(recipes_dir, "**/*.yaml"), recursive=True)):
    with open(filepath) as f:
        data = yaml.safe_load(f)
    rel_path = os.path.relpath(filepath, recipes_dir)
    entries.append({
        "name": data.get("name", ""),
        "path": rel_path,
        "status": data.get("status", ""),
        "description": (data.get("description", "") or "").strip(),
        "tags": data.get("tags") or [],
        "ingredients": extract_ingredient_names(data)
    })

with open(os.path.join(recipes_dir, "recipe-index.json"), "w") as f:
    json.dump(entries, f, indent=2)
