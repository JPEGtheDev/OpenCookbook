#!/usr/bin/env bash
# prepare-spa.sh — Add .nojekyll and copy index.html to 404.html for SPA routing.
# Usage: ./scripts/prepare-spa.sh <output_dir>
#   output_dir: path to the wwwroot output (e.g. visualizer/output/wwwroot)
set -euo pipefail

OUTPUT_DIR="${1:?Usage: prepare-spa.sh <output_dir>}"

touch "$OUTPUT_DIR/.nojekyll"
cp "$OUTPUT_DIR/index.html" "$OUTPUT_DIR/404.html"
