# F06 — CI/CD Pipeline & GitHub Pages Deployment

## Summary

The project SHALL be built, tested, and deployed automatically on every push to `main` using GitHub Actions. The compiled Blazor WASM output is published to GitHub Pages, replicating (and significantly improving on) the original project's manual deployment approach.

---

## Goals

- Zero-touch deployment: merge to `main` → live site updated within ~3 minutes.
- Tests run on every pull request to prevent regressions.
- Build artefacts are never committed to the repository (no `gh-pages` branch with compiled output).

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F06-1 | developer | have tests run automatically on every PR | regressions are caught before merge |
| US-F06-2 | developer | have the app deployed automatically on merge to `main` | I never need to run a deploy command manually |
| US-F06-3 | developer | see build and deploy status badges in the README | I know at a glance whether the pipeline is healthy |

---

## Workflow: `ci.yml` (Pull Request Checks)

Triggered on: `push` to any branch, `pull_request` targeting `main`.

```
Steps:
1. actions/checkout
2. actions/setup-dotnet  (version: 10.x)
3. dotnet restore
4. dotnet build --no-restore -c Release
5. dotnet test --no-build -c Release (bUnit unit tests)
```

---

## Workflow: `deploy.yml` (Deploy to GitHub Pages)

Triggered on: `push` to `main` only.

```
Steps:
1.  actions/checkout
2.  actions/setup-dotnet  (version: 10.x)
3.  dotnet restore
4.  dotnet build --no-restore -c Release
5.  dotnet test --no-build -c Release          ← gate: deploy only if tests pass
6.  dotnet publish -c Release -o publish/
7.  Copy publish/wwwroot → staging dir
8.  Add .nojekyll file                          ← required for Blazor WASM on GitHub Pages
9.  Rewrite <base href> to repo sub-path        ← required for GitHub Pages project sites
10. actions/upload-pages-artifact
11. actions/deploy-pages
```

### GitHub Pages Settings

| Setting | Value |
|---|---|
| Source | GitHub Actions (not a branch) |
| Environment | `github-pages` (auto-created by `deploy-pages` action) |
| Custom domain | Optional (configured via `CNAME` file in `wwwroot/`) |

---

## Functional Requirements

| ID | Requirement |
|---|---|
| FR-F06-1 | A `ci.yml` workflow SHALL run `dotnet build` and `dotnet test` on every push and pull request. |
| FR-F06-2 | A `deploy.yml` workflow SHALL publish the Blazor WASM output to GitHub Pages on every push to `main`, **only after tests pass**. |
| FR-F06-3 | The deployment SHALL NOT require any secrets beyond the default `GITHUB_TOKEN` (using `actions/deploy-pages`). |
| FR-F06-4 | A `.nojekyll` file SHALL be included in the published output to prevent GitHub Pages from processing Blazor's `_framework/` directory with Jekyll. |
| FR-F06-5 | The `<base href>` in `index.html` SHALL be dynamically set during publish to match the GitHub Pages sub-path (e.g. `/passphrase/`). |
| FR-F06-6 | The README SHALL include build-status and deployment-status badges. |

---

## Base Href Configuration for GitHub Pages

Blazor WASM requires the correct `<base href>` to resolve relative URLs. For a project site at `https://username.github.io/passphrase/`, the base href must be `/passphrase/`.

This is handled in `deploy.yml` with a `sed` (or PowerShell) step that rewrites `<base href="/" />` to `<base href="/passphrase/" />` in the published `index.html`.

Alternatively, use the `--pathbase` publish argument:

```yaml
- name: Publish
  run: dotnet publish -c Release -o publish/ /p:BlazorPathBase=/passphrase/
```

---

## Dependency Automation

| Tool | Configuration |
|---|---|
| **Renovate Bot** | `renovate.json` — auto-raise PRs for NuGet, GitHub Actions, and npm package updates |
| **Dependabot** | `.github/dependabot.yml` (alternative to Renovate for GitHub-native integration) |

---

## Secrets Required

| Secret | Source | Used for |
|---|---|---|
| `GITHUB_TOKEN` | Auto-provisioned | GitHub Pages deployment via `actions/deploy-pages` |

No additional secrets are required.
