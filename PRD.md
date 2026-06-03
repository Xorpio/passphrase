# PRD — Passphrase Generator (Blazor WASM Rewrite)

> **Version:** 1.0  
> **Date:** 2026-05-28  
> **Status:** Draft  
> **Original project:** https://github.com/xorpio/passphrase  
> **Tech target:** .NET 10 · Blazor WebAssembly · GitHub Actions → GitHub Pages

---

## Overview

This document describes the requirements for a complete rewrite of the *Passphrase Generator* tool using modern .NET technology. The application runs entirely in the browser as a Blazor WebAssembly SPA with no backend server, uses the Diceware method for human-readable password generation, and is published automatically to GitHub Pages via GitHub Actions.

---

## Document Index

| File | Description |
|---|---|
| [features/F01-diceware-generation.md](features/F01-diceware-generation.md) | Core passphrase generation using Diceware word lists |
| [features/F02-password-transformation.md](features/F02-password-transformation.md) | Optional character transformations on the generated passphrase |
| [features/F03-language-support.md](features/F03-language-support.md) | Multi-language word list support (EN / NL) |
| [features/F04-ui-controls.md](features/F04-ui-controls.md) | UI controls, layout, and copy-to-clipboard |
| [features/F05-security.md](features/F05-security.md) | Cryptographically secure randomness and privacy |
| [features/F06-cicd-github-pages.md](features/F06-cicd-github-pages.md) | GitHub Actions CI/CD pipeline and GitHub Pages deployment |
| [features/F07-testing.md](features/F07-testing.md) | Unit and end-to-end test strategy |
| [features/F08-randomorg-integration.md](features/F08-randomorg-integration.md) | Optional random.org API integration for true hardware randomness |

---

## Goals

1. **Feature parity** with the original Angular 7 application.
2. **Improved security** — replace `Math.random()` with cryptographically secure RNG.
3. **Modern stack** — .NET 10 + Blazor WASM, enabling C# across the full application.
4. **Automated CI/CD** — build, test, and deploy on every push via GitHub Actions.
5. **Maintainability** — strong typing, unit tests, and dependency update automation (Renovate / Dependabot).

---

## Non-Goals

- No server-side component; the app must work as a fully static site.
- No user accounts or persisted data.
- No mobile-native app.

---

## High-Level Architecture

```
Browser
└── Blazor WASM (.NET 10)
    ├── Pages/
    │   └── Home.razor           ← single page (matches original)
    ├── Components/
    │   └── PassphraseForm.razor ← main form component
    ├── Services/
    │   ├── RandomService        ← crypto-secure RNG
    │   ├── WordlistService      ← lazy-loads JSON word lists
    │   ├── WordService          ← Diceware key → word lookup
    │   └── PasswordService      ← applies transformations
    └── wwwroot/
        └── data/
            ├── diceware.wordlist.json   (English)
            └── DicewareDutch.json       (Dutch)
```

---

## Stakeholders

| Role | Interest |
|---|---|
| Developer | Tech-demo showcasing .NET 10 Blazor WASM for front-end apps |
| End user | Quick, memorable password generation without a server |
