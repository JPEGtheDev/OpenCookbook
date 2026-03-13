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

> ⚠️ **Known issue:** `deploy-pages.yml` currently triggers on `branches: [main]` but the repo's default branch is `master`. This means the automatic push trigger never fires — only `workflow_dispatch` works. This should be corrected to `branches: [master]` when the workflow is next touched.

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

Always use `master`. Never use `main`.

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

### #4 — `pr-build.yml` — dotnet build check on every PR

Triggers on `pull_request` targeting `master`. Runs `dotnet restore` + `dotnet build` + `dotnet test`. Must be a required status check before merge.

```yaml
on:
  pull_request:
    branches: [master]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x
      - name: Restore
        working-directory: visualizer
        run: dotnet restore
      - name: Build
        working-directory: visualizer
        run: dotnet build --no-restore
      - name: Test
        working-directory: visualizer
        run: dotnet test --no-restore --verbosity normal
```

### #5 — Website package versioning

Version is embedded in the `.csproj` or via a `Directory.Build.props` file. The CI workflow reads the version and can tag builds. No separate workflow file needed — this is a project-level change.

### #6 — `fix(website): stale site on returning visits`

Implemented in the Blazor project itself (service worker update strategy or cache headers), not in the workflow. No new workflow file needed unless the fix requires a build-time step.

### #7 — `pr-preview.yml` — PR preview deploy + cleanup

**Depends on #4 being implemented first.** Only deploy if the build job passes.

Two triggers:
1. `pull_request` (types: `opened`, `synchronize`, `reopened`) → build + deploy preview
2. `pull_request` (types: `closed`) → clean up preview environment

Preview environment name: `pr-${{ github.event.pull_request.number }}`

Post the preview URL as a PR comment using `gh` CLI or the GitHub REST API.

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
