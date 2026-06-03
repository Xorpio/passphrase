# F05 — Security & Privacy

## Summary

The original application used `Math.random()` for all random number generation. This is a pseudo-random number generator (PRNG) that is **not cryptographically secure** and is therefore unsuitable for generating security-critical passphrases. The rewrite MUST replace all randomness with a cryptographically secure source.

---

## User Stories

| ID | As a … | I want to … | So that … |
|---|---|---|---|
| US-F05-1 | user | trust that my passphrase is truly random | it cannot be predicted or reproduced by an attacker |
| US-F05-2 | user | generate passphrases entirely in my browser | no passphrase data is ever sent to a server |

---

## Functional Requirements

### RNG Source Priority

The application supports two RNG sources, selected automatically based on user configuration:

1. **random.org API** (opt-in, see F08) — true hardware randomness via atmospheric noise; requires a user-supplied API key stored in `localStorage`.
2. **`RandomNumberGenerator` / CSPRNG** (default) — OS-level entropy, always available, no account required.

When random.org is configured but unavailable, the system silently falls back to the CSPRNG and notifies the user (see F08 FR-F08-11 through FR-F08-13).

---

## Cryptographically Secure RNG (Default)

| ID | Requirement |
|---|---|
| FR-F05-1 | ALL random number generation SHALL use `System.Security.Cryptography.RandomNumberGenerator` (or `Random.Shared` which is CSPRNG-seeded in .NET 6+). |
| FR-F05-2 | The `RandomService` SHALL expose `int GetInt(int minInclusive, int maxExclusive)` backed by `RandomNumberGenerator.GetInt32`. |
| FR-F05-3 | Dice-roll simulation (keys 1–6) SHALL use `RandomNumberGenerator.GetInt32(1, 7)` to ensure uniform distribution with no modulo bias. |
| FR-F05-4 | Random index selection for capital/number/special transformations SHALL use the same secure RNG. |

### Privacy

| ID | Requirement |
|---|---|
| FR-F05-5 | The application SHALL have NO backend server component; all logic runs client-side in the browser (Blazor WASM). |
| FR-F05-6 | No generated passphrase or word list query SHALL be sent over the network after the initial asset load. |
| FR-F05-7 | The application SHALL NOT use cookies, local storage, or session storage to persist generated passphrases. |
| FR-F05-8 | The application SHALL be served over HTTPS (enforced by GitHub Pages). |

### Content Security

| ID | Requirement |
|---|---|
| FR-F05-9 | The word list JSON files are static, trusted assets bundled with the application. They SHALL NOT be fetched from a third-party CDN at runtime. |

---

## Why `Math.random()` Is Insufficient

`Math.random()` in V8 / SpiderMonkey is a deterministic algorithm seeded with a small internal state. Academic research has demonstrated that observing a sequence of `Math.random()` outputs is sufficient to predict future values. For a security tool like a passphrase generator, only a CSPRNG with OS-level entropy is acceptable.

**Replacement in .NET:**

```csharp
// Secure — uses OS entropy (CryptGenRandom / getrandom)
int diceRoll = RandomNumberGenerator.GetInt32(1, 7); // 1..6 inclusive

// Also acceptable in .NET 6+ (ThreadLocal CSPRNG)
int index = Random.Shared.Next(0, wordList.Count);
```

---

## Threat Model (out of scope for v1.0)

The following threats are acknowledged but explicitly out of scope:

- Side-channel attacks on the browser process.
- Clipboard interception after copy-to-clipboard.
- Compromised browser extensions.
