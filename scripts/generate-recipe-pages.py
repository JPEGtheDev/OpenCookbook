#!/usr/bin/env python3
"""generate-recipe-pages.py — Generate static HTML recipe share pages with Schema.org JSON-LD.

Each generated page allows external apps (e.g. Lose It!) to import a recipe by URL.
The page contains machine-readable Schema.org Recipe markup and redirects browsers to
the full Blazor recipe view.

Usage:
    python3 scripts/generate-recipe-pages.py <recipes_dir> <output_dir> <base_url>

  recipes_dir: path to recipes inside wwwroot output (e.g. visualizer/output/wwwroot/recipes)
  output_dir:  path to wwwroot root (e.g. visualizer/output/wwwroot)
  base_url:    public base URL without trailing slash (e.g. https://jpegthedev.github.io/OpenCookbook)
"""

import glob
import html
import json
import os
import sys
from urllib.parse import quote

import yaml


def _format_ingredient(item):
    """Return a human-readable ingredient string for Schema.org recipeIngredient."""
    qty = item.get("quantity", "")
    unit = item.get("unit", "")
    name = (item.get("name") or "").strip()
    volume_alt = (item.get("volume_alt") or "").strip()

    if volume_alt and name:
        return f"{volume_alt} {name}"

    qty_str = str(int(qty)) if isinstance(qty, float) and qty == int(qty) else str(qty)
    parts = [p for p in [qty_str, unit, name] if p]
    return " ".join(parts)


def _build_schema(data, page_url):
    """Build a Schema.org Recipe JSON-LD dict from parsed YAML data."""
    schema = {
        "@context": "https://schema.org",
        "@type": "Recipe",
        "name": data.get("name", ""),
        "description": (data.get("description", "") or "").strip(),
        "author": {
            "@type": "Person",
            "name": data.get("author", ""),
        },
        "url": page_url,
    }

    yields = data.get("yields")
    if yields:
        schema["recipeYield"] = f"{yields.get('quantity', '')} {yields.get('unit', '')}".strip()

    tags = data.get("tags")
    if tags:
        schema["keywords"] = ", ".join(tags)

    ingredients = []
    for group in (data.get("ingredients") or []):
        for item in (group.get("items") or []):
            if not item.get("doc_link"):
                ingredients.append(_format_ingredient(item))
    schema["recipeIngredient"] = ingredients

    instructions = []
    for section in (data.get("instructions") or []):
        heading = section.get("heading")
        steps = [
            {"@type": "HowToStep", "text": s.get("text", "").strip()}
            for s in (section.get("steps") or [])
            if s.get("text", "").strip()
        ]
        if not steps:
            continue
        if heading:
            instructions.append({
                "@type": "HowToSection",
                "name": heading,
                "itemListElement": steps,
            })
        else:
            instructions.extend(steps)
    schema["recipeInstructions"] = instructions

    return schema


def _generate_html(recipe_name, schema, app_url):
    json_ld = json.dumps(schema, indent=2, ensure_ascii=False)
    # Prevent the JSON-LD from accidentally closing the surrounding <script> tag.
    # Replace all occurrences of "</" (case-insensitive in HTML) with the JSON escape.
    import re
    json_ld = re.sub(r"</", r"<\\/", json_ld)
    safe_name = html.escape(recipe_name)
    safe_app_url = html.escape(app_url)
    return f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>{safe_name} — OpenCookbook</title>
  <meta http-equiv="refresh" content="0; url={safe_app_url}" />
  <script type="application/ld+json">
{json_ld}
  </script>
</head>
<body>
  <p>Redirecting to <a href="{safe_app_url}">{safe_name}</a>&#8230;</p>
</body>
</html>
"""


def main():
    if len(sys.argv) < 4:
        print(
            "Usage: generate-recipe-pages.py <recipes_dir> <output_dir> <base_url>",
            file=sys.stderr,
        )
        sys.exit(1)

    recipes_dir = sys.argv[1]
    output_dir = sys.argv[2]
    base_url = sys.argv[3].rstrip("/")

    count = 0
    for filepath in sorted(glob.glob(os.path.join(recipes_dir, "**/*.yaml"), recursive=True)):
        with open(filepath, encoding="utf-8") as f:
            data = yaml.safe_load(f)

        rel_path = os.path.relpath(filepath, recipes_dir).replace("\\", "/")

        # Slug: recipe path without .yaml extension, used as the share/ subdirectory
        page_slug = rel_path[:-5] if rel_path.endswith(".yaml") else rel_path

        # URL of the static share page (what Lose It! scrapes)
        page_url = f"{base_url}/share/{page_slug}/"

        # URL of the Blazor recipe view (where the meta refresh sends humans)
        # Encode the full path including slashes so it matches the Blazor @page "/recipe/{Path}"
        app_url = f"{base_url}/recipe/{quote(rel_path, safe='')}"

        recipe_name = data.get("name", page_slug)
        schema = _build_schema(data, page_url)
        html = _generate_html(recipe_name, schema, app_url)

        out_path = os.path.join(output_dir, "share", page_slug, "index.html")
        os.makedirs(os.path.dirname(out_path), exist_ok=True)
        with open(out_path, "w", encoding="utf-8") as f:
            f.write(html)

        count += 1
        print(f"Generated: share/{page_slug}/index.html")

    print(f"Done. {count} share page(s) generated.")


if __name__ == "__main__":
    main()
