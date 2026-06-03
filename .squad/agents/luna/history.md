# Luna — History

## Project Context
- **Project**: Passphrase Generator (Blazor WASM rewrite)
- **Tech**: .NET 10, Blazor WebAssembly, GitHub Actions, GitHub Pages
- **Goal**: Modern rewrite of Angular 7 tool with improved security (crypto RNG), feature parity, automated CI/CD
- **User**: Niek de Gooijer
- **Created**: 2026-06-03

## Team
- **Dumbledore** — Lead, architecture, code review
- **Luna** (you) — Frontend (Blazor components, UI)
- **Hermione** — Backend (services, business logic)
- **Moody** — Tester (unit/E2E tests, quality)
- **Dobby** — DevOps (CI/CD, GitHub Pages)
- **Scribe** — Session logger
- **Ralph** — Work monitor

## Key UI Requirements
- Single-page app (Home.razor)
- Main form component (PassphraseForm.razor)
- Display generated words
- Copy-to-clipboard button
- Transformation toggles (capitalize, uppercase, spacing)
- Language selector (EN / NL)
- Clean, accessible UI

## Learnings

### 2026-06-03 — Phase 1 UI (UI-01 to UI-04)

**Project structure:**
- Blazor WASM project at `src/PassphraseGenerator/`
- Core services library at `src/PassphraseGenerator.Core/`
- Local service interfaces in `src/PassphraseGenerator/Services/` (use lowercase language codes "nl"/"en")
- Models at `src/PassphraseGenerator/Services/Models/` (`SentenceSpec(int WordCount, string Language)`, `PasswordSpec`)

**Bootstrap & icons:**
- Bootstrap 5 is bundled locally via `wwwroot/lib/bootstrap` (from Blazor template)
- Bootstrap Icons loaded via CDN `bootstrap-icons@1.11.3` in `index.html`
- All icon usage: `<i class="bi bi-{name}" aria-hidden="true"></i>` pattern

**Accessibility patterns:**
- `aria-live="polite" aria-atomic="true" role="status"` on word count stepper display
- `aria-readonly="true"` on read-only text inputs
- `<fieldset>` + `<legend>` for checkbox transformation group
- `role="group" aria-labelledby="..."` for stepper button group
- Touch targets ≥ 44×44px via `.stepper-btn` CSS class

**Blazor WASM gotchas:**
- Variable named `code` inside a Razor `@foreach` triggers the `@code` directive parser — rename to `langCode` or similar
- Clean build (`dotnet clean && dotnet build`) sometimes needed when switching namespace references
- `@bind` on `<select>` works correctly with `_selectedLanguage` state; language change detection uses `OnParametersSetAsync` comparing `_selectedLanguage != _previousLanguage`
- Two-way checkbox binding: `@bind="_useSpace"` works directly on `<input type="checkbox">`

**Copy-to-clipboard:**
- JS interop via `window.clipboardInterop.copyText(text)` in `wwwroot/js/interop.js`
- Fallback using `document.execCommand('copy')` for blocked contexts
- Visual feedback: button changes to green with checkmark for 2 seconds via `CancellationTokenSource`

**Dutch UX labels (from F04 spec):**
- "Spatie gebruiken", "Hoofdletter gebruiken", "Getallen gebruiken", "Leestekens gebruiken"
- "Genereer wachtwoord" (generate button)
- "Taal" (language label)
- "Aantal woorden" (word count label)

**Service pre-existing bug found:**
- `WordlistService.cs` in the Blazor project had its class body duplicated (written twice). Fixed by rewriting cleanly. `SupportedLanguages` property must be before constructor in class body to avoid truncation issues.

