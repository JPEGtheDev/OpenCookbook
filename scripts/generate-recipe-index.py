#!/usr/bin/env python3
"""generate-recipe-index.py — Build recipe-index.json from YAML recipe files.

Usage: python3 scripts/generate-recipe-index.py <recipes_dir>
  recipes_dir: path to the recipes folder inside wwwroot output
               (e.g. visualizer/output/wwwroot/recipes)
"""
import json, yaml, os, glob, sys

recipes_dir = sys.argv[1] if len(sys.argv) > 1 else None
if not recipes_dir:
    print("Usage: generate-recipe-index.py <recipes_dir>", file=sys.stderr)
    sys.exit(1)

entries = []
for filepath in sorted(glob.glob(os.path.join(recipes_dir, "**/*.yaml"), recursive=True)):
    with open(filepath) as f:
        data = yaml.safe_load(f)
    rel_path = os.path.relpath(filepath, recipes_dir)
    entries.append({
        "name": data.get("name", ""),
        "path": rel_path,
        "status": data.get("status", ""),
        "description": (data.get("description", "") or "").strip()
    })

with open(os.path.join(recipes_dir, "recipe-index.json"), "w") as f:
    json.dump(entries, f, indent=2)
