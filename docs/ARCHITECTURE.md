# OpenCookbook Visualizer — Architecture Plan

## Overview

A static HTML visualizer for OpenCookbook recipe YAML files, hosted on GitHub Pages.
Built with **Blazor WebAssembly** following **Clean Architecture** principles.

---

## Technology Choice: Blazor WebAssembly

### Why Blazor WASM?

| Consideration | Decision |
|---|---|
| **GitHub Pages compatibility** | ✅ Blazor WASM compiles to static files (HTML, CSS, JS, WASM). No server required. |
| **Author preference** | ✅ .NET / Blazor was the preferred choice. |
| **Clean Architecture** | ✅ .NET ecosystem has mature patterns for layered architecture with clear dependency rules. |
| **YAML parsing** | ✅ YamlDotNet works in WASM — recipes are parsed client-side. |
| **Type safety** | ✅ C# provides strong typing for the recipe data model from SCHEMA.md. |
| **Testability** | ✅ xUnit + bUnit provide excellent unit and component testing. |

### Trade-offs

| Concern | Mitigation |
|---|---|
| ~5–10 MB initial download (WASM runtime) | Acceptable for a recipe site; cached after first visit. Blazor's trimming and AOT options reduce size. |
| Slower first load vs. vanilla JS | Pre-rendered loading screen. Subsequent navigations are instant. |
| SEO limitations (client-rendered) | Not a priority for a personal recipe site. Can add prerendering later if needed. |

---

## Clean Architecture

Dependencies flow **inward** — outer layers depend on inner layers, never the reverse.

```
┌──────────────────────────────────────────────┐
│              OpenCookbook.Web                 │  ← Blazor WASM (Presentation)
│         Components, Pages, DI Setup          │
├──────────────────────────────────────────────┤
│         OpenCookbook.Infrastructure          │  ← YAML parsing, HTTP loading
│     YamlRecipeParser, HttpRecipeRepository   │
├──────────────────────────────────────────────┤
│          OpenCookbook.Application            │  ← Use cases, interfaces
│       IRecipeRepository, RecipeService       │
├──────────────────────────────────────────────┤
│            OpenCookbook.Domain               │  ← Core entities (no dependencies)
│    Recipe, Ingredient, Section, Step, ...    │
└──────────────────────────────────────────────┘
```

### Layer Rules

| Layer | May Reference | Must NOT Reference |
|---|---|---|
| **Domain** | Nothing | Application, Infrastructure, Web |
| **Application** | Domain | Infrastructure, Web |
| **Infrastructure** | Domain, Application | Web |
| **Web** | Domain, Application, Infrastructure | — |

---

## Project Structure

```
visualizer/
├── src/
│   ├── OpenCookbook.Domain/              # Core entities — zero dependencies
│   │   └── Entities/
│   │       ├── Recipe.cs
│   │       ├── IngredientGroup.cs
│   │       ├── Ingredient.cs
│   │       ├── UtensilGroup.cs
│   │       ├── Section.cs
│   │       ├── Step.cs
│   │       ├── RelatedRecipe.cs
│   │       └── RecipeStatus.cs
│   │
│   ├── OpenCookbook.Application/         # Interfaces + services
│   │   ├── Interfaces/
│   │   │   ├── IRecipeRepository.cs
│   │   │   └── IRecipeParser.cs
│   │   └── Services/
│   │       └── RecipeService.cs
│   │
│   ├── OpenCookbook.Infrastructure/      # External concerns
│   │   ├── Parsing/
│   │   │   └── YamlRecipeParser.cs
│   │   └── Repositories/
│   │       └── HttpRecipeRepository.cs
│   │
│   └── OpenCookbook.Web/                 # Blazor WASM app
│       ├── wwwroot/
│       │   ├── index.html
│       │   ├── css/
│       │   └── recipes/                  # YAML files copied here at build
│       ├── Components/
│       │   ├── RecipeCard.razor
│       │   ├── IngredientList.razor
│       │   ├── InstructionList.razor
│       │   └── StatusBadge.razor
│       ├── Pages/
│       │   ├── Home.razor
│       │   └── RecipeDetail.razor
│       └── Program.cs
│
├── tests/
│   ├── OpenCookbook.Domain.Tests/
│   └── OpenCookbook.Infrastructure.Tests/
│
└── OpenCookbook.sln
```

---

## GitHub Pages Deployment

### Strategy

1. **GitHub Actions** builds the Blazor WASM app on push to `main`.
2. The published output (static files) is deployed to GitHub Pages.
3. A `.nojekyll` file prevents Jekyll processing.
4. A `404.html` duplicates `index.html` for SPA client-side routing.

### Workflow Summary

```
Push to main → dotnet publish → Copy recipes to wwwroot → Deploy to GitHub Pages
```

### Key Configuration

- **Base path**: Set to repository name (`/OpenCookbook/`) for GitHub Pages project sites.
- **Recipe loading**: The app fetches YAML files via HTTP from `wwwroot/recipes/`.
- **Recipe index**: A `recipe-index.json` is generated at build time listing all available recipes.

---

## Data Flow

```
1. User navigates to site
2. Blazor WASM loads in browser
3. App fetches recipe-index.json (list of all recipes)
4. User selects a recipe
5. App fetches the .yaml file via HTTP
6. YamlRecipeParser deserializes YAML → Recipe entity
7. RecipeService returns the Recipe to the UI
8. Blazor components render the recipe using the layout spec
```

---

## Rendering Rules (from recipe-rendering skill)

The visualizer follows the layout defined in the recipe-rendering skill:

- **Two-column layout**: Ingredients (1/3 left) + Instructions (2/3 right)
- **Branch sections** render as **tabs** (mutually exclusive by `branch_group`)
- **Status badges**: Stable (green), Beta (amber), Draft (red)
- **Ingredient display**: `quantity unit Name (≈ volume_alt)` when `volume_alt` present
- **Optional sections** (Serving, Freezing) render in a footer area
- **Notes** only display for non-stable recipes as a callout box

---

## Future Enhancements

| Feature | Priority | Notes |
|---|---|---|
| PDF export | Medium | Use browser print styles or a PDF library |
| Recipe search/filter | Low | Client-side filtering of recipe index |
| Shopping list mode | Low | Checkbox ingredients with local storage |
| Dark mode | Low | CSS custom properties |
| PWA support | Low | Service worker for offline recipe access |
