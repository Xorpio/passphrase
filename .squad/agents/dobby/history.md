# Dobby — History

## Project Context
- **Project**: Passphrase Generator (Blazor WASM rewrite)
- **Tech**: .NET 10, Blazor WebAssembly, GitHub Actions, GitHub Pages
- **Goal**: Modern rewrite of Angular 7 tool with improved security (crypto RNG), feature parity, automated CI/CD
- **User**: Niek de Gooijer
- **Created**: 2026-06-03

## Team
- **Dumbledore** — Lead, architecture, code review
- **Luna** — Frontend (Blazor components, UI)
- **Hermione** — Backend (services, business logic)
- **Moody** — Tester (unit/E2E tests, quality)
- **Dobby** (you) — DevOps (CI/CD, GitHub Pages)
- **Scribe** — Session logger
- **Ralph** — Work monitor

## CI/CD Pipeline Tasks
1. **Build**: dotnet build (.NET 10 Blazor WASM)
2. **Publish**: dotnet publish for WASM
3. **Test**: Run xUnit/MSTest tests
4. **Deploy**: Push to GitHub Pages

## Key Deployment Details
- **Target**: GitHub Pages (static hosting)
- **Artifacts**: Blazor WASM app output (wwwroot/ directory)
- **Secrets**: optional random.org API key (if used)
- **Dependency updates**: Renovate or Dependabot
- **Test command**: Coordinate with Moody

## Non-Goal
- Docker, Kubernetes, or complex infrastructure

## Learnings

### 2026-06-03 — GitHub Actions CI/CD Pipeline Setup

**CI/CD Workflows Created:**

1. **ci.yml** (Build & Test on all branches)
   - Triggers: `push` to any branch, `pull_request` to main
   - Uses `actions/setup-dotnet@v4` for .NET 10.x setup
   - Key steps: restore → build Release → test with coverage collection
   - Coverage enforcement: Fails if <80% (using XPlat Code Coverage in trx format)
   - Test artifacts uploaded to Actions summary for inspection
   - No deployment from CI pipeline (test-only)

2. **deploy.yml** (Publish & Deploy on main branch only)
   - Triggers: `push` to main only (gated release)
   - Same test gate: deployment blocked if any test fails
   - **BlazorPathBase**: Set to `/passphrase/` for GitHub Pages subpath via MSBuild property
   - Adds `.nojekyll` file to published wwwroot to prevent Jekyll processing
   - Uses `actions/upload-pages-artifact@v3` and `actions/deploy-pages@v4`
   - Permissions: pages:write, id-token:write for Pages environment
   - Concurrency control on `pages` group (prevents concurrent deployments)

**Key Technical Decisions:**

- **Coverage reporting**: Parse XPlat Code Coverage XML output; fail build if <80%
- **BlazorPathBase vs sed rewrite**: Chose MSBuild property approach (cleaner, no string replacement fragility)
- **.nojekyll requirement**: Prevents GitHub's Jekyll from trying to process `_framework/` directory (breaks Blazor WASM routing)
- **Test format**: TRX (Test Results XML) for structured test reporting in Actions UI
- **GitHub Pages environment**: Named environment `github-pages` with URL output from deploy-pages action

**.gitignore additions:**
- Standard .NET: `bin/`, `obj/`, `publish/`, `TestResults/`
- IDE: `.vs/`, `.vscode/`, `*.user`
- Coverage: `*.coverage`, `*.coveragexml`
- Test results: `*.trx`

**Secrets Management:**
- No custom secrets required (using auto-provisioned `GITHUB_TOKEN`)
- random.org API key (if used) deferred to Phase 2
- Environment setup uses GitHub's built-in Pages authentication (no PAT needed)

**Coordination with team:**
- Workflows assume project structure: `src/PassphraseGenerator` (Blazor) + `tests/PassphraseGenerator.Tests` (per Dumbledore architecture)
- Test command: `dotnet test --configuration Release --no-build` (will use xUnit per team consensus)
- Coverage threshold 80% aligns with Moody's test discipline charter

**Next steps (Phase 2):**
- Renovate or Dependabot for dependency updates
- E2E workflow (e2e.yml) with Playwright post-deploy
- Optional random.org secret handling in GitHub Secrets

