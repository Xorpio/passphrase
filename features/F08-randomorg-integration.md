# F08 — random.org API Integration (Opt-in True Randomness)

## Summary

As a client-side opt-in, users may supply their own [random.org](https://random.org) API key. When a key is configured, all dice rolls are sourced from random.org's true hardware random number generator (atmospheric noise) instead of the local CSPRNG. When no key is configured, the application falls back to `RandomNumberGenerator` (see F05).

**This feature is entirely opt-in and client-side.** The API key is never sent to any developer-controlled server.

---

## Background

random.org generates true random numbers from atmospheric noise and exposes them via a JSON-RPC 4 API. For dice rolls, the `generateIntegers` method is ideal: it generates `n` integers in `[min, max]` with replacement — exactly the semantics of rolling dice.

API endpoint: `https://api.random.org/json-rpc/4/invoke`

**Example request** — 5 dice rolls for one Diceware word:
```json
{
  "jsonrpc": "2.0",
  "method": "generateIntegers",
  "params": {
    "apiKey": "<user-supplied-key>",
    "n": 5,
    "min": 1,
    "max": 6,
    "replacement": true
  },
  "id": 1
}
```

**Example response:**
```json
{
  "jsonrpc": "2.0",
  "result": {
    "random": { "data": [1, 5, 4, 6, 6] },
    "bitsUsed": 16,
    "bitsLeft": 199984,
    "requestsLeft": 9999,
    "advisoryDelay": 0
  },
  "id": 1
}
```

To generate a passphrase of `N` words, a **single API call** requests `5 × N` integers (e.g. 30 integers for 6 words), minimising round trips and quota usage.

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F08-1 | user | optionally enter my random.org API key | I can use true hardware randomness for my passphrases |
| US-F08-2 | user | leave the key blank | the app works perfectly with the built-in CSPRNG (no account required) |
| US-F08-3 | user | see my remaining quota after generation | I know how many bits/requests are left on my key |
| US-F08-4 | user | be notified when the API call fails | I understand why randomness fell back to CSPRNG |
| US-F08-5 | user | have my key remembered across sessions | I don't have to re-enter it every time |

---

## Functional Requirements

### API Key Input

| ID | Requirement |
|---|---|
| FR-F08-1 | A collapsible/expandable "Settings" section SHALL contain an API key input field labelled "random.org API key". |
| FR-F08-2 | The field type SHALL be `password` to prevent shoulder-surfing, with a show/hide toggle. |
| FR-F08-3 | The API key SHALL be persisted in **browser `localStorage`** under the key `randomorg_api_key`, so it survives page refreshes. |
| FR-F08-4 | A "Clear key" button SHALL remove the key from `localStorage` and revert to CSPRNG mode. |
| FR-F08-5 | When a key is present, a visible indicator (e.g. a badge "Using random.org") SHALL appear near the generate controls. |

### Request Strategy

| ID | Requirement |
|---|---|
| FR-F08-6 | When a key is configured, ALL dice rolls for a single generation cycle SHALL be batched into a **single** `generateIntegers` API request with `n = 5 × wordCount`. |
| FR-F08-7 | The request SHALL use `replacement: true` and `min: 1, max: 6`. |
| FR-F08-8 | The application SHALL respect the `advisoryDelay` value from the response, delaying any subsequent request by at least that many milliseconds. |

### Quota Display

| ID | Requirement |
|---|---|
| FR-F08-9 | After a successful API call, the response `bitsLeft` and `requestsLeft` SHALL be displayed near the settings panel. |
| FR-F08-10 | If `requestsLeft` drops below a configurable threshold (default: 10), a warning SHALL be displayed. |

### Fallback Behaviour

| ID | Requirement |
|---|---|
| FR-F08-11 | If the random.org API call fails for any reason (network error, invalid key, quota exhausted), the system SHALL fall back to the local CSPRNG automatically. |
| FR-F08-12 | A non-blocking toast/alert SHALL inform the user of the fallback with the error reason. |
| FR-F08-13 | A failed API call SHALL NOT block passphrase generation — the fallback MUST be transparent. |

### Privacy / Security

| ID | Requirement |
|---|---|
| FR-F08-14 | The API key SHALL only ever be sent to `https://api.random.org` — never to any other endpoint. |
| FR-F08-15 | The key SHALL be stored in `localStorage` only; it SHALL NOT appear in the application URL, query string, or any analytics. |
| FR-F08-16 | A privacy notice SHALL be displayed near the API key field, stating: *"Your key is stored locally in your browser and is only sent directly to api.random.org."* |

---

## Service Design

```csharp
public interface IRandomService
{
    Task<int> GetDiceRollAsync();
    Task<IReadOnlyList<int>> GetDiceRollsAsync(int count);
}

// Local implementation (CSPRNG)
public class LocalRandomService : IRandomService { ... }

// random.org implementation
public class RandomOrgService : IRandomService
{
    // Batches all rolls into one HTTP request
    // Respects advisoryDelay
    // Throws RandomOrgException on API error → caller catches and falls back
}

// Wrapper that routes to one or the other
public class RandomServiceRouter : IRandomService
{
    // Uses RandomOrgService if ApiKey is set; otherwise LocalRandomService
    // Catches RandomOrgException, falls back, raises OnFallback event
}
```

The `RandomServiceRouter` is registered as the `IRandomService` singleton in DI; components never reference the implementations directly.

---

## UI Mockup (Wireframe Description)

```
┌─────────────────────────────────────────────┐
│ ⚙ Settings                           [▲ collapse] │
├─────────────────────────────────────────────┤
│ random.org API key                          │
│ [••••••••••••••••••••••••••] [👁] [✕ Clear] │
│                                             │
│ ℹ Your key is stored locally in your        │
│   browser and only sent to api.random.org.  │
│                                             │
│ Quota: 199,984 bits · 9,999 requests left   │
└─────────────────────────────────────────────┘

[🔀 Using random.org]  ← badge shown when key is active
```

---

## API Key Acquisition

Users can obtain a free API key at [https://api.random.org/api-keys](https://api.random.org/api-keys). The free tier provides 1,000,000 bits/day and 1,000 requests/day — sufficient for thousands of passphrases.

---

## Implementation Notes (Blazor WASM / .NET 10)

- `HttpClient` is pre-configured in Blazor WASM and can call `https://api.random.org` directly (CORS is supported by random.org for browser clients).
- `localStorage` access via JS interop: `await JSRuntime.InvokeAsync<string>("localStorage.getItem", "randomorg_api_key")`.
- `advisoryDelay` compliance: use `Task.Delay(advisoryDelay)` before the next call.
- JSON-RPC request/response can be modelled with simple C# `record` types and `System.Text.Json`.
