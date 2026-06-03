# Hermione — History

## Project Context
- **Project**: Passphrase Generator (Blazor WASM rewrite)
- **Tech**: .NET 10, Blazor WebAssembly, GitHub Actions, GitHub Pages
- **Goal**: Modern rewrite of Angular 7 tool with improved security (crypto RNG), feature parity, automated CI/CD
- **User**: Niek de Gooijer
- **Created**: 2026-06-03

## Team
- **Dumbledore** — Lead, architecture, code review
- **Luna** — Frontend (Blazor components, UI)
- **Hermione** (you) — Backend (services, business logic)
- **Moody** — Tester (unit/E2E tests, quality)
- **Dobby** — DevOps (CI/CD, GitHub Pages)
- **Scribe** — Session logger
- **Ralph** — Work monitor

## Core Services to Build
1. **RandomService** — Cryptographically secure RNG (System.Security.Cryptography)
2. **WordlistService** — Load/cache Diceware word lists (EN/NL JSON files)
3. **WordService** — Diceware key (5-digit roll) → word lookup
4. **PasswordService** — Apply transformations (capitalize, uppercase, spacing, etc.)
5. **optional RandomOrgService** — Hardware RNG integration

## Key Constraints
- Static site, no backend
- All logic runs in browser via WASM
- Security focus: crypto-secure RNG (not Math.random())

## Learnings

### 2026-06-03 — Phase 1 Backend Infrastructure (ARCH-01 + SVC-01–04)

**What was built:**
- `.NET 10 Blazor WASM` project at `src/PassphraseGenerator/`
- `xUnit` test project at `src/PassphraseGenerator.Tests/`
- Solution file at `src/PassphraseGenerator.sln`
- 4 services + interfaces under `src/PassphraseGenerator/Services/`
- Model records in `src/PassphraseGenerator/Services/Models/`
- Word lists in `src/PassphraseGenerator/wwwroot/data/`
- `.nojekyll` in `src/PassphraseGenerator/wwwroot/`
- `PassphraseForm` component scaffold created by sub-agent

**Architecture decisions:**
- Services registered with `AddScoped` (not `AddSingleton`) because in Blazor WASM the root DI container IS the scope — AddSingleton and AddScoped are effectively equivalent, but scoped avoids DI lifetime validation errors when HttpClient (itself scoped) is injected into WordlistService.
- Model records (`SentenceSpec`, `PasswordSpec`) placed in `Services/Models/` sub-namespace to keep them separate from interface/implementation files. Interfaces reference models via explicit `using` import.
- `LocalRandomService` uses `RandomNumberGenerator.GetInt32()` — never `Math.Random()` or `System.Random`.
- Word lists are lazy-loaded and cached in `Dictionary<string, Dictionary<string,string>>` in `WordlistService`.
- Diceware key = `string.Concat(rolls)` where each roll is 1–6, giving a 5-digit string key like `"23456"`.

**Key file paths:**
- `src/PassphraseGenerator/Services/IRandomService.cs` — interface
- `src/PassphraseGenerator/Services/LocalRandomService.cs` — CSPRNG impl
- `src/PassphraseGenerator/Services/IWordlistService.cs` — interface
- `src/PassphraseGenerator/Services/WordlistService.cs` — lazy-loading, HttpClient, caching
- `src/PassphraseGenerator/Services/IWordService.cs` — interface
- `src/PassphraseGenerator/Services/WordService.cs` — Diceware logic
- `src/PassphraseGenerator/Services/IPasswordService.cs` — interface
- `src/PassphraseGenerator/Services/PasswordService.cs` — transformations
- `src/PassphraseGenerator/Services/Models/SentenceSpec.cs` — record
- `src/PassphraseGenerator/Services/Models/PasswordSpec.cs` — record
- `src/PassphraseGenerator/Program.cs` — DI registration
- `src/PassphraseGenerator/wwwroot/data/diceware.wordlist.json` — English word list
- `src/PassphraseGenerator/wwwroot/data/DicewareDutch.json` — Dutch word list

**Pitfall encountered:**
- Sub-agent created duplicate model files in both `Services/` and `Services/Models/`, causing CS0104 ambiguous reference errors. Fixed by deleting the bare `Services/SentenceSpec.cs` and `Services/PasswordSpec.cs`, keeping the `Models/` versions and adding `using PassphraseGenerator.Services.Models;` to the interfaces.

**Build status:** ✅ `dotnet build` → 0 errors, 0 warnings
