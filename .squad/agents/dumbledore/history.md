# Dumbledore — History

## Project Context
- **Project**: Passphrase Generator (Blazor WASM rewrite)
- **Tech**: .NET 10, Blazor WebAssembly, GitHub Actions, GitHub Pages
- **Goal**: Modern rewrite of Angular 7 tool with improved security (crypto RNG), feature parity, automated CI/CD
- **User**: Niek de Gooijer
- **Created**: 2026-06-03

## Team
- **Dumbledore** (you) — Lead, architecture, code review
- **Luna** — Frontend (Blazor components, UI)
- **Hermione** — Backend (services, business logic)
- **Moody** — Tester (unit/E2E tests, quality)
- **Dobby** — DevOps (CI/CD, GitHub Pages)
- **Scribe** — Session logger
- **Ralph** — Work monitor

## Learnings

### 2026-06-03 — Architecture decomposition

**Key architectural decisions made:**
- Two-project .NET solution: `src/PassphraseGenerator` (Blazor WASM) + `tests/PassphraseGenerator.Tests`
- UI: Bootstrap 5 + Bootstrap Icons only — no MudBlazor/Blazorise (PRD-specified, smallest footprint)
- Randomness: `RandomNumberGenerator.GetInt32` always; Math.random/PRNG explicitly prohibited
- Service DI: `IRandomService` → `RandomServiceRouter` singleton; WordlistService singleton (cache owner); WordService + PasswordService scoped
- Records defined up-front: `SentenceSpec(Language="NL", Words=6)`, `PasswordSpec(UseSpace, UseCapital, UseNumbers, UseSpecialCharacters)`
- Testing: xUnit + Moq (unit), bUnit (component), Playwright (E2E)
- CI/CD: `ci.yml` (all branches) + `deploy.yml` (main only) + `e2e.yml` (Phase 2 post-deploy)
- GitHub Pages: `<base href>` rewritten via `/p:BlazorPathBase=/passphrase/`; `.nojekyll` required
- F08 (random.org) deferred to Phase 2 — it is opt-in and adds complexity; MVP is CSPRNG + Diceware core

**Key file paths:**
- PRD: `PRD.md`
- Feature specs: `features/F01–F08.md`
- Architecture decision: `.squad/decisions/inbox/dumbledore-architecture.md`

**Phasing:**
- Phase 1 MVP: F01 F02 F03 F04 F05 F06 F07 (core)
- Phase 2: F08 random.org, Playwright E2E, Renovate/Dependabot
- Phase 3: Accessibility audit, additional languages, CSP

**User preferences observed:**
- Niek uses Squad CLI conventions (decisions.md, inbox/, history.md, wisdom.md patterns)
- Project is a tech demo for .NET 10 Blazor WASM — showing C# in the browser replacing Angular
