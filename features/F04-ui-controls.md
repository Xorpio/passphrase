# F04 — UI Controls & Layout

## Summary

The application presents a single-page form with controls for configuring the passphrase, displaying the generated word sentence, and showing the final password output. The UI is responsive and works on desktop and mobile.

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F04-1 | user | see a clear, minimal form | I am not overwhelmed and can focus on the passphrase |
| US-F04-2 | user | increase/decrease the word count with +/- buttons | I don't have to type a number |
| US-F04-3 | user | click a refresh icon to regenerate the sentence | I can quickly try new combinations |
| US-F04-4 | user | copy the generated password to my clipboard with one click | I don't have to manually select and copy the text |
| US-F04-5 | user | see the passphrase displayed as a readable text field | I can read and verify it before copying |
| US-F04-6 | user | use the app on mobile without horizontal scrolling | The layout is responsive |

---

## Functional Requirements

### Navigation / Chrome

| ID | Requirement |
|---|---|
| FR-F04-1 | The application SHALL display a top navigation bar with the app title ("Passphrase"). |
| FR-F04-2 | The app SHALL consist of a single page/route (no sub-navigation required). |

### Language Selector

| ID | Requirement |
|---|---|
| FR-F04-3 | A `<select>` dropdown SHALL allow the user to choose a word list language (see F03). |
| FR-F04-4 | The label "Taal" (Language) SHALL be displayed next to the selector. |

### Options Checkboxes

| ID | Requirement |
|---|---|
| FR-F04-5 | Four checkboxes SHALL control the transformation options: Use Space, Use Capital, Use Numbers, Use Special Characters (see F02). |
| FR-F04-6 | Checkboxes SHALL be labelled in the UI language (Dutch labels match original: "Spatie gebruiken", "Hoofdletter gebruiken", "Getallen gebruiken", "Leestekens gebruiken"). |

### Word Count Stepper

| ID | Requirement |
|---|---|
| FR-F04-7 | A stepper control (◀ count ▶) SHALL adjust the number of words from 3 to 10. |
| FR-F04-8 | The decrement button SHALL be disabled when word count equals the minimum (3). |
| FR-F04-9 | The increment button SHALL be disabled when word count equals the maximum (10). |

### Sentence Display

| ID | Requirement |
|---|---|
| FR-F04-10 | A read-only text input SHALL display the currently generated word sentence (space-separated words before password transformations). |
| FR-F04-11 | A refresh icon button next to the sentence field SHALL trigger regeneration of the word sentence. |

### Generate Button

| ID | Requirement |
|---|---|
| FR-F04-12 | A "Genereer wachtwoord" (Generate password) button SHALL apply all active transformations to the current sentence and display the result. |

### Password Output

| ID | Requirement |
|---|---|
| FR-F04-13 | A read-only text input SHALL display the generated password. |
| FR-F04-14 | A copy-to-clipboard icon button SHALL copy the password value to the system clipboard and provide brief visual feedback (e.g., icon changes to a checkmark for 2 seconds). |

---

## UI Component Library

The rewrite SHALL use **Bootstrap 5** for layout and form components. An optional Blazor component library (e.g. [Blazorise](https://blazorise.com/) or [MudBlazor](https://mudblazor.com/)) may be used if it does not add unnecessary complexity.

Icons SHALL use a modern icon library compatible with Blazor (e.g. **Bootstrap Icons** or **Font Awesome 6**).

---

## Responsive Design

- Layout SHALL use Bootstrap's 12-column grid.
- Labels and inputs SHALL stack vertically on viewports narrower than `md` (< 768 px).
- All interactive elements SHALL meet WCAG 2.1 AA touch target size (≥ 44 × 44 px).

---

## Implementation Notes (Blazor WASM / .NET 10)

- All state lives in `PassphraseForm.razor` (mirroring the single Angular component).
- Two-way binding via `@bind-Value` for checkboxes and the language selector.
- Clipboard API: `await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", password)`.
- Icons via `<span class="bi bi-arrow-clockwise">` (Bootstrap Icons bundled with app).
