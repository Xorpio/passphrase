# F03 — Multi-Language Word List Support

## Summary

The application supports multiple human languages for the Diceware word list. The user selects a language from a dropdown; the matching word list is fetched and cached, and subsequent generations use that list.

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F03-1 | user | select **Dutch (NL)** as the word list language | I can generate passphrases I can actually pronounce and remember |
| US-F03-2 | user | select **English (EN)** as the word list language | I can use the international standard Diceware list |
| US-F03-3 | user | switch language at any time | A new sentence is generated using the newly selected language |

---

## Functional Requirements

| ID | Requirement |
|---|---|
| FR-F03-1 | The language selector SHALL offer at minimum: **Dutch (NL)** and **English (EN)**. |
| FR-F03-2 | Default language SHALL be **Dutch (NL)** (matching the original). |
| FR-F03-3 | Switching language SHALL trigger a new sentence generation automatically. |
| FR-F03-4 | Each language's word list SHALL be cached independently; switching back to a previously loaded language SHALL NOT trigger a new HTTP request. |
| FR-F03-5 | If a requested language's asset cannot be loaded, the system SHALL fall back to English and display a non-blocking warning. |

---

## Supported Languages (v1.0)

| Code | Label | Asset file |
|---|---|---|
| `NL` | Nederlands | `wwwroot/data/DicewareDutch.json` |
| `EN` | English | `wwwroot/data/diceware.wordlist.json` |

---

## Extensibility

The architecture SHALL support adding new languages by:
1. Adding a new wordlist JSON file to `wwwroot/data/`.
2. Adding an entry to a configuration dictionary in `WordlistService` — no code changes elsewhere.

---

## Implementation Notes (Blazor WASM / .NET 10)

- `SentenceSpec` record:
  ```csharp
  public record SentenceSpec(string Language = "NL", int Words = 6);
  ```
- `WordlistService` maintains a `Dictionary<string, Dictionary<string, string>>` cache keyed by language code.
- Language options for the dropdown are driven by a static list in the service, keeping the component free of configuration data.
