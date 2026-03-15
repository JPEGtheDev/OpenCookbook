---
name: ci-workflows
description: How to read, write, and extend GitHub Actions workflows in OpenCookbook. Covers the existing deploy-pages workflow, conventions for new workflows, and patterns for dotnet builds, PR checks, and preview deployments.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
---

# CI Workflows

## When to Use This Skill

Use this when:
- Writing a new GitHub Actions workflow file
- Editing an existing workflow
- Debugging a workflow failure
- Implementing one of the planned CI features (issues #4–#7)

---

## Repo CI Overview

| File | Trigger | Purpose |
|---|---|---|
| `.github/workflows/deploy-pages.yml` | Push to `master`, `workflow_dispatch` | Build Blazor WASM, generate recipe index, deploy to GitHub Pages |
| `.github/workflows/pr-build.yml` | `pull_request` targeting `master` | Build/test, upload artifact, post download instructions comment on PR |
| `.github/workflows/pr-preview.yml` | `pull_request_target` closed targeting `master` | Update PR comment when PR is closed/merged |

---

## Project Structure (Relevant to CI)

```
OpenCookbook/
├── Recipes/              # YAML recipe files — copied to WASM output at build time
├── visualizer/           # Blazor WASM project root
│   ├── src/
│   │   └── OpenCookbook.Web/
│   │       └── OpenCookbook.Web.csproj
│   └── (test projects)
└── .github/
    └── workflows/
```

**All `dotnet` commands must use `working-directory: visualizer`.**

---

## Shared Scripts

Both `deploy-pages.yml` and `pr-build.yml` call these scripts instead of duplicating inline shell:

| Script | Purpose |
|---|---|
| `scripts/copy-recipes.sh <output_dir>` | Copies `Recipes/**/*.yaml` into `<output_dir>/recipes/` preserving folder structure |
| `scripts/generate-recipe-index.py <recipes_dir>` | Generates `recipe-index.json` from YAML metadata (name, path, status, description) |
| `scripts/prepare-spa.sh <output_dir>` | Adds `.nojekyll` and copies `index.html` → `404.html` for SPA routing |
| `scripts/serve.py [port]` | Bundled in artifact. Python SPA server for local preview (handles `.wasm` MIME type, falls back unknown paths to `index.html`) |

All scripts use `set -euo pipefail`. Never add inline versions of these steps — call the scripts.

---

## Existing Workflow: `deploy-pages.yml`

### What it does

1. `dotnet restore` — restores NuGet packages
2. `dotnet test` — runs all tests (no-restore, normal verbosity)
3. `dotnet publish` — publishes the Blazor WASM app to `visualizer/output/`
4. Shell script — copies all `Recipes/**/*.yaml` files into `visualizer/output/wwwroot/recipes/`, preserving folder structure
5. Python script — generates `recipe-index.json` from the copied YAML files (name, path, status, description)
6. Adds `.nojekyll` and copies `index.html` → `404.html` for SPA routing
7. Uploads `visualizer/output/wwwroot` as a Pages artifact
8. Deploys to GitHub Pages via `actions/deploy-pages@v4`

### Permissions

```yaml
permissions:
  contents: read
  pages: write
  id-token: write
```

### Concurrency

```yaml
concurrency:
  group: pages
  cancel-in-progress: false
```

`cancel-in-progress: false` ensures a deploy already in progress is not cancelled. Use this on all deploy workflows.

---

## Conventions for New Workflows

### Naming

- File: `kebab-case.yml` (e.g. `pr-build.yml`, `pr-preview.yml`)
- Workflow `name:` field: human-readable sentence case (e.g. `Build PR`, `Deploy PR Preview`)
- Job names: `build`, `test`, `deploy`, `cleanup` — lowercase, single word where possible

### Trigger branch

Always use `master` (the repo's default branch). Never use `main`.

All changes go through branches and pull requests — never push directly to `master`.

```yaml
on:
  push:
    branches: [master]
  pull_request:
    branches: [master]
```

### .NET version

Always pin to `10.0.x`:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: 10.0.x
```

### Working directory

All `dotnet` commands run from the `visualizer/` subdirectory:

```yaml
- name: Restore
  working-directory: visualizer
  run: dotnet restore
```

### Action versions (pinned)

| Action | Version |
|---|---|
| `actions/checkout` | `@v4` |
| `actions/setup-dotnet` | `@v4` |
| `actions/upload-pages-artifact` | `@v3` |
| `actions/deploy-pages` | `@v4` |
| `actions/upload-artifact` | `@v4` |
| `actions/download-artifact` | `@v4` |
| `actions/github-script` | `@v7` |

Do not use `@latest` or unpinned versions.

### Permissions

Only declare permissions your workflow actually needs. Start from this minimum and add only what is required:

```yaml
permissions:
  contents: read
```

Common additions:
- `pull-requests: write` — to post comments on PRs
- `pages: write` + `id-token: write` — to deploy to GitHub Pages
- `deployments: write` — to create deployment environments

### Concurrency

All workflows that deploy or post comments must have a concurrency group to prevent races:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true   # ok for build/test jobs
```

For deploy jobs, use `cancel-in-progress: false`.

---

## Planned Workflows (Issues #4–#7)

### #4 — `pr-build.yml` — dotnet build check on every PR ✅ Implemented

See `.github/workflows/pr-build.yml`. Triggers on `pull_request` targeting `master`. Runs full build/test pipeline, uploads the built site as an artifact, and posts a PR comment with download instructions.

**Artifact-based PR preview approach:** The built site (including `scripts/serve.py`) is uploaded as a GitHub Actions artifact named `pr-preview-site`. The PR comment posts both a `gh` CLI command and a `curl` one-liner so reviewers can download and serve the site locally with `python3 serve.py`. This avoids overwriting the production GitHub Pages site.

**Key points:**
- Permissions: `contents: read` + `pull-requests: write` only. No `pages: write` or `id-token: write` needed.
- The `upload-artifact` step outputs `artifact-id` — capture this with `id: upload` and pass it to the comment script via `env:`.
- `serve.py` handles `.wasm` MIME type and SPA route fallback. It is copied into the artifact output dir before upload so it travels with the build.

### #5 — Website package versioning

Version is embedded in the `.csproj` or via a `Directory.Build.props` file. The CI workflow reads the version and can tag builds. No separate workflow file needed — this is a project-level change.

### #6 — `fix(website): stale site on returning visits`

Implemented in the Blazor project itself (service worker update strategy or cache headers), not in the workflow. No new workflow file needed unless the fix requires a build-time step.

### #7 — `pr-preview.yml` — PR preview cleanup ✅ Implemented

See `.github/workflows/pr-preview.yml`. Uses `pull_request_target` (types: `closed`) so it fires when a PR is merged or closed. Updates the `## 📦 PR Preview Built` bot comment to indicate the artifact is no longer available.

**Note:** Uses `pull_request_target` not `pull_request` — this is required to get write permissions on the `GITHUB_TOKEN` when the PR is from a fork. The job only makes GitHub API calls, no code checkout.

---

## Dotnet Command Reference

| Purpose | Command |
|---|---|
| Restore packages | `dotnet restore` |
| Build (no restore) | `dotnet build --no-restore` |
| Run tests | `dotnet test --no-restore --verbosity normal` |
| Publish release | `dotnet publish <project>.csproj -c Release -o <output-dir>` |

Always pass `--no-restore` to `build`/`test`/`publish` when a prior restore step exists in the same job.

---

## Debugging Workflow Failures

| Symptom | Likely cause |
|---|---|
| Workflow never triggers on push | Branch name mismatch — check `branches:` trigger matches `master` |
| `dotnet` command not found | Missing `setup-dotnet` step |
| `working-directory` error | Path is wrong — must be `visualizer`, not `visualizer/src` |
| Pages deploy fails with permission error | Missing `pages: write` or `id-token: write` permission |
| Recipe index missing entries | YAML copy step ran before `dotnet publish` created output dir |
| PR comment not posted | Missing `pull-requests: write` permission |
