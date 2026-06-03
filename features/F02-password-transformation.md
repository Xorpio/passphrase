# F02 — Password Transformations

## Summary

After generating the base passphrase (a list of Diceware words), the user can opt-in to optional character-level transformations that increase complexity without sacrificing memorability. Each transformation is independently toggled via a checkbox.

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F02-1 | user | use spaces between words | my passphrase is easier to type and read |
| US-F02-2 | user | capitalise a random character | my passphrase meets "must contain uppercase" policies |
| US-F02-3 | user | replace a random character with a digit (0–9) | my passphrase meets "must contain number" policies |
| US-F02-4 | user | replace a random character with a special character | my passphrase meets "must contain special character" policies |
| US-F02-5 | user | combine any subset of the above options | I can match the exact policy of any system |

---

## Functional Requirements

| ID | Requirement |
|---|---|
| FR-F02-1 | **Use Space** — when enabled, words SHALL be joined with a single space `' '`; when disabled, words SHALL be concatenated with no separator. Default: **enabled**. |
| FR-F02-2 | **Use Capital** — when enabled, a randomly selected character in a randomly selected word SHALL be replaced with its uppercase equivalent. Randomness is driven by the secure RNG (see F05). |
| FR-F02-3 | **Use Numbers** — when enabled, a randomly selected character in a randomly selected word SHALL be replaced with a random digit (0–9). |
| FR-F02-4 | **Use Special Characters** — when enabled, a randomly selected character in a randomly selected word SHALL be replaced with a randomly selected special character from the predefined set below. |
| FR-F02-5 | Each transformation SHALL target an independently random word and character position, so that multiple active transformations do not always modify the same word. |
| FR-F02-6 | The password output SHALL reflect all active transformations applied to the same word list that is displayed in the sentence field. |

---

## Special Character Set

The following characters are available for the "Use Special Characters" transformation (matching the original implementation):

```
! @ # $ % ^ & * ( ) _ - + = ` ~ < > , . / ? : ; " ' { } [ ] \ |
```

---

## Default State

| Option | Default |
|---|---|
| Use Space | ✅ enabled |
| Use Capital | ✅ enabled |
| Use Numbers | ❌ disabled |
| Use Special Characters | ✅ enabled |

---

## Implementation Notes (Blazor WASM / .NET 10)

- `PasswordSpec` record:
  ```csharp
  public record PasswordSpec(
      bool UseSpace = true,
      bool UseCapital = true,
      bool UseNumbers = false,
      bool UseSpecialCharacters = true
  );
  ```
- `PasswordService.GeneratePassword(PasswordSpec spec, IReadOnlyList<string> words)` returns `string`.
- Transformations are applied in order: Capital → Number → Special → Join.
- `string` manipulation uses `string.Create` or `StringBuilder` for efficient single-character replacement.
- The same `RandomService` used in F01 handles all random index selection.
