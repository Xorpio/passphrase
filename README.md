# Passphrase Generator

A cryptographically secure passphrase generator built with Blazor WebAssembly and .NET 10.

[**View website**](https://xorpio.github.io/passphrase/)

## About

Generates strong, memorable passphrases using the [Diceware](https://theworld.com/~reinhold/diceware.html) method — rolling a virtual die five times to look up a word from a 7776-word list. Supports English and Dutch word lists.

### Features

- **Cryptographically secure** randomness via `System.Security.Cryptography.RandomNumberGenerator`
- **Two languages** — English and Dutch (7776 words each)
- **Configurable word count** — 3 to 10 words
- **Optional transformations** — spaces, capitalisation, numbers, and special characters
- **Runs entirely in the browser** — no server, no telemetry

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run locally

```bash
dotnet run --project src/PassphraseGenerator
```

Navigate to `https://localhost:5001/`.

### Run tests

```bash
dotnet test
```

The solution contains two test projects:

| Project | Type | Description |
|---------|------|-------------|
| `src/PassphraseGenerator.Tests` | xUnit | Unit tests for services and logic |
| `tests/PassphraseGenerator.Tests` | bUnit + xUnit | Component tests for Razor components |

### Build for production

```bash
dotnet publish src/PassphraseGenerator -c Release -o publish -p:BlazorPathBase=/passphrase/
```

## Project structure

```
src/
  PassphraseGenerator/          # Blazor WASM app
  PassphraseGenerator.Core/     # Shared core library
  PassphraseGenerator.Tests/    # Unit tests
tests/
  PassphraseGenerator.Tests/    # Component tests (bUnit)
```

## CI / CD

GitHub Actions workflows:

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `ci.yml` | Push / PR to any branch | Build, test, and report coverage |
| `deploy.yml` | Push to `main` | Build, gate on tests, deploy to GitHub Pages |
