---
name: "dotnet-github-actions-cicd"
description: "Set up GitHub Actions CI/CD pipelines for .NET Blazor WASM projects with build, test, coverage enforcement, and GitHub Pages deployment"
domain: "devops, ci-cd, github-actions, dotnet, blazor"
confidence: "high"
source: "implemented (Dobby, 2026-06-03)"
tools:
  - name: "github-actions"
    description: "Workflow orchestration and deployment"
    when: "Building, testing, and deploying .NET projects"
---

## Context

When setting up CI/CD for a .NET project (especially Blazor WASM apps) that deploys to GitHub Pages, you need:
1. Automated build and test on all branches (PR gate)
2. Automated publish and deploy on main branch only
3. Test coverage enforcement (fail if <80%)
4. Proper GitHub Pages configuration (`.nojekyll`, correct base href)
5. No hardcoded secrets (use auto-provisioned GITHUB_TOKEN)

This skill covers the complete end-to-end setup.

## Patterns

### Pattern 1: CI Workflow (All Branches)

**File**: `.github/workflows/ci.yml`

**Triggers**: `push` (all branches), `pull_request` (targeting main)

**Key steps**:
1. Checkout code
2. Setup .NET via `actions/setup-dotnet@v4` with version `{dotnet-version}`
3. `dotnet restore`
4. `dotnet build --configuration Release --no-restore`
5. `dotnet test --configuration Release --no-build --logger "trx" --collect:"XPlat Code Coverage"`
6. Parse XPlat Code Coverage XML; fail if coverage <{threshold}%
7. Upload test results as artifact for inspection

**Critical details**:
- Use `--no-restore` flag on build (already restored)
- Use `--no-build` flag on test (already built)
- Collect XPlat Code Coverage for structured reporting
- Use TRX format for GitHub Actions UI integration
- Threshold enforcement must fail the job (exit 1) on coverage drop

### Pattern 2: Deploy Workflow (Main Branch Only)

**File**: `.github/workflows/deploy.yml`

**Triggers**: `push` to main branch only (no PRs)

**Key steps**:
1. Checkout code
2. Setup .NET
3. Build (same as CI)
4. **Run tests as gate** (deployment blocked if any test fails)
5. Publish with BlazorPathBase:
   ```yaml
   dotnet publish {project-path} -c Release -o publish \
     -p:BlazorPathBase={github-pages-subpath}/
   ```
6. Add `.nojekyll` file: `touch publish/wwwroot/.nojekyll`
7. Upload artifact: `actions/upload-pages-artifact@v3` (path: `publish/wwwroot/`)
8. Deploy: `actions/deploy-pages@v4`

**Permissions required**:
```yaml
permissions:
  contents: read
  pages: write
  id-token: write
```

**Environment setup**:
```yaml
environment:
  name: github-pages
  url: ${{ steps.deployment.outputs.page_url }}
```

**Concurrency control** (prevents overlapping deployments):
```yaml
concurrency:
  group: pages
  cancel-in-progress: false
```

### Pattern 3: Coverage Enforcement

**Approach**: Parse XPlat Code Coverage XML, extract line-rate, calculate percentage, fail if <threshold.

**Example logic**:
```bash
COVERAGE=$(find . -name "coverage.cobertura.xml" | head -1)
COVERAGE_PCT=$(grep -oP 'line-rate="\K[^"]+' "$COVERAGE" | head -1)
COVERAGE_INT=$(echo "$COVERAGE_PCT * 100" | bc | cut -d'.' -f1)
if [ "$COVERAGE_INT" -lt 80 ]; then
  echo "❌ Coverage ${COVERAGE_INT}% below 80% threshold."
  exit 1
fi
```

**Why this works**:
- XPlat Code Coverage generates `coverage.cobertura.xml` (standard Cobertura format)
- Line-rate is decimal (0.75 = 75%)
- Fail the job with `exit 1` to block merge/deployment

### Pattern 4: BlazorPathBase for GitHub Pages

**Decision**: Use MSBuild property (not post-publish string replacement).

**Implementation**:
```bash
dotnet publish src/MyApp -c Release -o publish \
  -p:BlazorPathBase=/myapp/
```

**Why not sed/PowerShell replacement**:
- Fragile (brittle string matching on HTML)
- Requires platform-specific tools (sed doesn't work on Windows without Git Bash)
- MSBuild property is Blazor's intended mechanism

**GitHub Pages subpath**: Always `/reponame/` (if project site) or `/` (if user/org site).

### Pattern 5: .gitignore for .NET + Workflows

**Additions to repo .gitignore**:
```
# .NET build artifacts
bin/
obj/
publish/

# Test results
TestResults/
*.trx
*.coverage
*.coveragexml

# IDE
.vs/
.vscode/
*.user
```

**Don't commit**:
- Build output (`bin/`, `obj/`, `publish/`)
- Test results (`.trx`, coverage XML)
- User-specific IDE settings (`*.user`)
- Node modules (if any npm dependencies)

## Examples

### Minimal Blazor WASM Project Structure

```
src/
├── MyApp/
│   ├── MyApp.csproj
│   ├── Pages/
│   ├── Components/
│   ├── wwwroot/
│   └── Program.cs
tests/
├── MyApp.Tests/
│   ├── MyApp.Tests.csproj
│   └── Services/
└── MyApp.E2E/
    ├── MyApp.E2E.csproj
    └── Fixtures/
```

### Sample ci.yml (Minimal)

```yaml
name: CI
on:
  push:
    branches: ['**']
  pull_request:
    branches: [main]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - run: dotnet restore
      - run: dotnet build -c Release --no-restore
      - run: dotnet test -c Release --no-build --logger "trx" --collect:"XPlat Code Coverage"
```

### Sample deploy.yml (Minimal)

```yaml
name: Deploy
on:
  push:
    branches: [main]

permissions:
  pages: write
  id-token: write

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - run: dotnet restore
      - run: dotnet build -c Release --no-restore
      - run: dotnet test -c Release --no-build
      - run: dotnet publish src/MyApp -c Release -o publish -p:BlazorPathBase=/myapp/
      - run: touch publish/wwwroot/.nojekyll
      - uses: actions/upload-pages-artifact@v3
        with:
          path: 'publish/wwwroot/'
      - uses: actions/deploy-pages@v4
        id: deployment
```

## Anti-Patterns

### ❌ Don't Hardcode Base Href in index.html

```html
<!-- Bad: hardcoded, not configurable -->
<base href="/myapp/" />
```

Instead, use BlazorPathBase or sed rewrite during publish.

### ❌ Don't Forget .nojekyll

```bash
# Bad: missing .nojekyll on GitHub Pages
dotnet publish ... -o publish/
actions/upload-pages-artifact@v3:
  path: 'publish/wwwroot/'  # ← Blazor fails due to Jekyll processing
```

Without `.nojekyll`, GitHub's Jekyll will try to process `_framework/` and break Blazor routing.

### ❌ Don't Deploy Without Test Gate

```yaml
# Bad: deploy runs even if tests fail
- run: dotnet publish ...
- run: actions/upload-pages-artifact
# ← Tests may have failed silently above
```

Always run tests *before* publish and ensure non-zero exit on failure.

### ❌ Don't Skip Coverage Check

```bash
# Bad: CI passes without coverage enforcement
dotnet test ...
echo "Tests passed!"
# ← No coverage check; coverage drift undetected
```

Enforce minimum coverage or coverage regression will accumulate.

### ❌ Don't Use sed for Cross-Platform

```bash
# Bad: assumes sed (Unix-only)
sed -i 's|<base href="/" />|<base href="/myapp/" />|' ...
```

Use MSBuild property or PowerShell (available on all platforms).

### ❌ Don't Store Secrets in Code

```yaml
# Bad: API key hardcoded
- run: dotnet publish ... -p:RandomOrgApiKey="sk-..."
```

Use GitHub Secrets: `${{ secrets.RANDOM_ORG_API_KEY }}`

## Integration Checklist

- [ ] `.github/workflows/ci.yml` created (build + test on all branches)
- [ ] `.github/workflows/deploy.yml` created (deploy on main only)
- [ ] Coverage enforcement: XPlat Code Coverage + ≥80% threshold
- [ ] `.nojekyll` added to published wwwroot (GitHub Pages requirement)
- [ ] BlazorPathBase set correctly for GitHub Pages subpath
- [ ] `.gitignore` updated (bin/, obj/, TestResults/, etc.)
- [ ] GitHub Pages enabled in repo settings (Source: GitHub Actions)
- [ ] Manual one-time: Enable GitHub Pages in repo settings
- [ ] Verify: Push to main → site live within ~3 minutes

## References

- GitHub Actions Setup .NET: https://github.com/actions/setup-dotnet
- GitHub Pages Deploy Action: https://github.com/actions/deploy-pages
- Blazor WASM Hosting on GitHub Pages: https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly#github-pages
- XPlat Code Coverage: https://github.com/coverlet-coverage/coverlet
