#!/usr/bin/env bash
# copy-recipes.sh — Copy recipe YAML files to the site output directory.
# Usage: ./scripts/copy-recipes.sh <output_dir>
#   output_dir: path to the wwwroot output (e.g. visualizer/output/wwwroot)
set -euo pipefail

OUTPUT_DIR="${1:?Usage: copy-recipes.sh <output_dir>}"
RECIPES_DIR="$OUTPUT_DIR/recipes"

mkdir -p "$RECIPES_DIR"
find Recipes -name "*.yaml" -print0 | while IFS= read -r -d '' file; do
  relpath="${file#Recipes/}"
  dir=$(dirname "$relpath")
  if [ "$dir" != "." ]; then
    mkdir -p "$RECIPES_DIR/$dir"
  fi
  cp "$file" "$RECIPES_DIR/$relpath"
done
