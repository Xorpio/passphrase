# Moody — History

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
- **Moody** (you) — Tester (unit/E2E tests, quality)
- **Dobby** — DevOps (CI/CD, GitHub Pages)
- **Scribe** — Session logger
- **Ralph** — Work monitor

## Testing Strategy
- **Unit tests**: RandomService, WordlistService, WordService, PasswordService
- **E2E tests**: PassphraseForm.razor user interactions (generate → copy)
- **Crypto validation**: Verify RNG security (output distribution, no patterns)
- **Coverage target**: >80%
- **Test framework**: xUnit or MSTest (TBD by team)
- **Integration**: Tests run in GitHub Actions (Dobby coordinates)

## Key Security Tests
- Randomness is not predictable
- Output distribution is uniform
- No weak or default RNG used

## Learnings

### Phase 1 — TEST-01 + TEST-02 (2026-06-03)

**Test framework decisions:**
- xUnit for unit tests, bUnit 1.37.7 for component tests, Moq 4.x for mocking
- Test project uses `Microsoft.NET.Sdk.Razor` SDK (required for bUnit to resolve Razor components)
- Must add `_Imports.razor` to test project for bUnit namespace imports

**Service contract discoveries (inconsistencies fixed as Tester):**
- `IPasswordService` was defined with `Task<string> GeneratePasswordAsync` but component called sync `GeneratePassword`. Fixed to sync — WASM single-thread makes `.GetAwaiter().GetResult()` safe inside sync context.
- `IWordlistService` interface was missing `SupportedLanguages` property used by the component; added it.
- Two duplicate model files existed (`Services/SentenceSpec.cs` + `Services/Models/SentenceSpec.cs`); consolidated to `Models/` only.
- `PasswordService` originally used `RandomNumberGenerator.GetInt32` directly (bypassing `IRandomService` mock). Fixed to use injected `_random.GetIntAsync` so tests can drive deterministic index selection.

**Mocking strategies:**
- `HttpMessageHandler.SendAsync` mocked via Moq's `Protected()` API for `WordlistService` HTTP tests
- Setup multiple URL matchers for different wordlist URLs (EN vs NL)
- For `IRandomService` mocking: cycle through `int[]` array with modulo index
- bUnit: register all services via `Services.AddSingleton(mock.Object)` BEFORE calling `RenderComponent<T>()`

**Crypto chi-square approach:**
- 1200 rolls, expected 200/face, critical value 20.5 (chi-square 5df p=0.001)
- Tests that CSPRNG is uniform without being a security proof

**Coverage result:**
- Line: 83.88%, Branch: 82.85%, Method: 92.1% — exceeds 80% target

**bUnit API notes:**
- Use `.HasAttribute("disabled")` not `.IsDisabled()` (no such extension method)
- Click Generate button before Copy button test — password is only populated after explicit Generate click
- `await cut.InvokeAsync(() => Task.CompletedTask)` flushes pending async state
