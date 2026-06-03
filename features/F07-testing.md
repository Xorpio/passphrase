# F07 — Testing Strategy

## Summary

The rewrite introduces a proper test suite to validate both the core logic (unit tests) and the full user flow (end-to-end tests). This replaces the largely empty Karma/Jasmine stubs in the original project.

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F07-1 | developer | have unit tests for all services | I can refactor safely |
| US-F07-2 | developer | have component tests for the UI form | I know the UI renders and responds correctly |
| US-F07-3 | developer | have end-to-end tests for the happy path | I know the full app works in a real browser |

---

## Unit Tests (bUnit + xUnit)

### Framework

| Library | Role |
|---|---|
| **xUnit** | Test runner and assertion library |
| **bUnit** | Blazor component testing library (renders components in-memory) |
| **Moq** | Mocking framework for service dependencies |

### Test Cases

#### `RandomService`

| Test | Expected |
|---|---|
| `GetInt` returns value in range `[min, max)` | Always true over 1 000 calls |
| `GetDiceRoll` always returns 1–6 | Always true |
| `GenerateKey` returns 5-character string of digits 1–6 | Format `\d{5}` |

#### `WordlistService`

| Test | Expected |
|---|---|
| `GetListAsync("EN")` returns dictionary with > 7 000 entries | True |
| `GetListAsync("NL")` returns dictionary with > 7 000 entries | True |
| Second call for same language uses cache (no HTTP call) | `HttpClient` called exactly once |
| Unknown language falls back to EN | Returns EN list + logs warning |

#### `WordService`

| Test | Expected |
|---|---|
| `GetSentenceAsync` with `Words=6` returns list of 6 strings | Count == 6 |
| Each word is a non-empty string | All non-null/empty |

#### `PasswordService`

| Test | Expected |
|---|---|
| `UseSpace=true` joins with space | Output contains spaces |
| `UseSpace=false` joins without separator | Output contains no spaces |
| `UseCapital=true` introduces at least one uppercase char | `output.Any(char.IsUpper)` |
| `UseNumbers=true` introduces at least one digit | `output.Any(char.IsDigit)` |
| `UseSpecialCharacters=true` introduces at least one special char | Character in charlist |
| All options disabled → words concatenated unchanged | Output == words joined |

#### `PassphraseForm` Component (bUnit)

| Test | Expected |
|---|---|
| Component renders without exception | No exceptions |
| Language dropdown has options for NL and EN | 2 `<option>` elements |
| Clicking "Genereer wachtwoord" with mocked services populates password field | Non-empty value |
| Word count stepper decrement is disabled at minimum (3) | `disabled` attribute present |
| Word count stepper increment is disabled at maximum (10) | `disabled` attribute present |
| Copy button triggers JS interop call | `JSRuntime` invoked with clipboard API |

#### `RandomServiceRouter` (F08)

| Test | Expected |
|---|---|
| No API key → routes to `LocalRandomService` | `LocalRandomService.GetDiceRollsAsync` called |
| API key set → routes to `RandomOrgService` | `RandomOrgService.GetDiceRollsAsync` called |
| `RandomOrgService` throws → fallback to local, `OnFallback` raised | Local result returned, event fired |
| `advisoryDelay` > 0 → `Task.Delay` called with correct ms | Verified via mock clock |

#### `RandomOrgService` (F08)

| Test | Expected |
|---|---|
| Successful response returns correct integer list | Data array matches |
| Batches `5 × wordCount` integers in a single request | Exactly one HTTP call |
| Invalid API key (error response) throws `RandomOrgException` | Exception type correct |
| Network failure throws `RandomOrgException` | Exception type correct |

---

## End-to-End Tests (Playwright)

### Framework

| Library | Role |
|---|---|
| **Microsoft.Playwright** | Browser automation (Chromium, Firefox, WebKit) |
| **xUnit** | Test host |

### Test Scenarios

| Scenario | Steps | Assertion |
|---|---|---|
| Happy path — generate passphrase | Load app → click "Genereer" | Password field is non-empty |
| Language switch | Select EN → click regenerate icon | Sentence updates |
| Word count change | Click ▶ three times → click "Genereer" | Sentence has more words |
| All options enabled | Check all boxes → generate | Password contains upper, digit, special |
| Copy button | Generate → click copy | Clipboard contains the password text |
| random.org opt-in | Enter valid API key in settings → generate | Badge "Using random.org" visible; password generated |
| random.org fallback | Enter invalid key → generate | Toast shown; password still generated via CSPRNG |

---

## Coverage Target

| Layer | Target coverage |
|---|---|
| Services (`RandomService`, `WordService`, `PasswordService`, `WordlistService`) | ≥ 90% line coverage |
| Component (`PassphraseForm`) | ≥ 70% branch coverage |
| End-to-end | 100% of user stories smoke-tested |

---

## CI Integration

- Unit tests and component tests run in the `ci.yml` workflow on every push/PR (see F06).
- End-to-end tests are optional in CI (may require a running dev server step); included as a separate `e2e.yml` workflow triggered on `main` post-deploy.
