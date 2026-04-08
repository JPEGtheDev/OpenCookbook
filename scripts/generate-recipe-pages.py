#!/usr/bin/env python3
"""generate-recipe-pages.py — Generate static HTML recipe pages with Schema.org JSON-LD.

For each recipe YAML file this script produces:

  recipe/{slug}/index.html — canonical page (200 OK from GitHub Pages).
    Contains Schema.org Recipe JSON-LD in the <head> and otherwise serves the
    full Blazor SPA so browsers get the interactive UI at the canonical URL.

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
import re
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


def _inject_json_ld_into_spa(spa_html, recipe_name, schema):
    """Return a copy of the Blazor SPA index.html with JSON-LD and a recipe title injected."""
    json_ld = json.dumps(schema, indent=2, ensure_ascii=False)
    json_ld = re.sub(r"</", r"<\\/", json_ld)
    safe_name = html.escape(recipe_name)

    json_ld_tag = f'  <script type="application/ld+json">\n{json_ld}\n  </script>\n'

    # Replace the generic <title> with the recipe name
    result, title_replacements = re.subn(
        r"<title>[^<]*</title>",
        f"<title>{safe_name} — OpenCookbook</title>",
        spa_html,
        count=1,
    )
    if title_replacements != 1:
        raise ValueError(
            "Failed to inject recipe title into SPA template: expected exactly one "
            "simple <title>...</title> element."
        )

    # Inject the JSON-LD script immediately before the closing </head> tag.
    # Match case-insensitively so harmless HTML formatting/casing differences
    # do not silently produce canonical pages without Schema.org data.
    head_close_match = re.search(r"</head\s*>", result, flags=re.IGNORECASE)
    if head_close_match is None:
        raise ValueError("Unable to inject JSON-LD: closing </head> tag not found in SPA HTML.")

    result = (
        result[: head_close_match.start()]
        + json_ld_tag
        + result[head_close_match.start():]
    )
    return result


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

    # Read the Blazor SPA entrypoint once — used for every canonical recipe page.
    spa_index_path = os.path.join(output_dir, "index.html")
    with open(spa_index_path, encoding="utf-8") as f:
        spa_html = f.read()

    count = 0
    for filepath in sorted(glob.glob(os.path.join(recipes_dir, "**/*.yaml"), recursive=True)):
        with open(filepath, encoding="utf-8") as f:
            data = yaml.safe_load(f)

        rel_path = os.path.relpath(filepath, recipes_dir).replace("\\", "/")

        # Slug: recipe path without .yaml extension
        page_slug = rel_path[:-5] if rel_path.endswith(".yaml") else rel_path

        # Canonical URL for this recipe — uses trailing slash so the URL resolves to
        # the directory-served index.html without an extra redirect on static hosts.
        canonical_url = f"{base_url}/recipe/{quote(page_slug, safe='/')}/"

        recipe_name = data.get("name", page_slug)
        schema = _build_schema(data, canonical_url)

        # Canonical page: recipe/{slug}/index.html — copy of the Blazor SPA with
        # JSON-LD injected in the <head>.  GitHub Pages serves this as a 200 OK so
        # crawlers see JSON-LD and browsers get the interactive UI.
        canonical_html = _inject_json_ld_into_spa(spa_html, recipe_name, schema)
        canonical_out = os.path.join(output_dir, "recipe", page_slug, "index.html")
        os.makedirs(os.path.dirname(canonical_out), exist_ok=True)
        with open(canonical_out, "w", encoding="utf-8") as f:
            f.write(canonical_html)
        print(f"Generated: recipe/{page_slug}/index.html")

        count += 1

    print(f"Done. {count} canonical recipe page(s) generated.")


if __name__ == "__main__":
    main()
