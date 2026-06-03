# F01 — Diceware Passphrase Generation

## Summary

Generate a human-readable passphrase by simulating dice rolls and looking up the resulting keys in a Diceware word list.

---

## Background

The **Diceware** method maps a 5-digit key (each digit 1–6, simulating one roll of a six-sided die) to a word in a pre-defined list. Rolling 5 dice gives 6⁵ = 7 776 possible words. Repeating the process for each word in the passphrase builds a phrase that is both memorable and statistically strong.

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F01-1 | user | click a "Generate" button | I receive a new passphrase immediately |
| US-F01-2 | user | configure the number of words (default: 6) | I can balance memorability vs. strength |
| US-F01-3 | user | regenerate the word list independently from the password | I can browse passphrases before committing |

---

## Functional Requirements

| ID | Requirement |
|---|---|
| FR-F01-1 | The system SHALL generate a passphrase by performing `N` independent Diceware lookups, where `N` is configured by the user (range 3–10, default 6). |
| FR-F01-2 | Each Diceware key SHALL be formed by simulating 5 independent rolls of a fair six-sided die (values 1–6). |
| FR-F01-3 | The word list SHALL be loaded once per language selection and cached in memory for subsequent generations. |
| FR-F01-4 | The generated word list (before password transformations) SHALL be displayed separately so the user can see the source words. |
| FR-F01-5 | Generating a new sentence SHALL replace the displayed words and the derived password. |

---

## Non-Functional Requirements

| ID | Requirement |
|---|---|
| NFR-F01-1 | Word-list loading SHALL be done asynchronously so the UI remains responsive. |
| NFR-F01-2 | The word list JSON file SHALL be embedded in `wwwroot/data/` and served as a static asset — no server call beyond the initial load. |
| NFR-F01-3 | Generation of a passphrase (after word list is cached) SHALL complete in < 50 ms. |

---

## Word List Format

```json
{
  "11111": "a",
  "11112": "a&p",
  ...
  "66666": "zulu"
}
```

The JSON object is keyed by the 5-digit dice roll string. Both the English (`diceware.wordlist.json`) and Dutch (`DicewareDutch.json`) lists use this same format and are sourced directly from the original project assets.

---

## Implementation Notes (Blazor WASM / .NET 10)

- `RandomService` generates the 5-digit key using `RandomNumberGenerator` (see F05).
- `WordlistService` uses `HttpClient` (injected into Blazor WASM) to fetch the JSON asset and deserialises it to `Dictionary<string, string>`.
- `WordService.GetSentenceAsync(SentenceSpec)` returns `IReadOnlyList<string>`.
- Word count stepper control is covered in F04.
