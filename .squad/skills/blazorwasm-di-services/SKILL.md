---
name: "blazorwasm-di-services"
description: "How to correctly register and structure injectable services in a Blazor WASM project, avoiding DI lifetime conflicts with HttpClient"
domain: "di, blazor, dotnet"
confidence: "high"
source: "earned"
---

## Context

Blazor WebAssembly runs in a single browser process with one ambient DI scope that spans the entire app lifetime. This has implications for service lifetime registration and dependency chains involving `HttpClient`.

## Patterns

### 1. Use `AddScoped` (not `AddSingleton`) for all app services

In Blazor WASM, `AddScoped` and `AddSingleton` are functionally equivalent (one scope = app lifetime). However, `AddSingleton` services **cannot** depend on `AddScoped` services (like `HttpClient`) without triggering DI validation errors. Use `AddScoped` universally to avoid this.

```csharp
// Program.cs — correct pattern
builder.Services.AddScoped<IRandomService, LocalRandomService>();
builder.Services.AddScoped<IWordlistService, WordlistService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
```

### 2. Separate model records into a `Models/` sub-namespace

Put data-only records (`SentenceSpec`, `PasswordSpec`) in `Services/Models/` so they can be imported independently. Prevents CS0104 ambiguous reference errors when a component imports both `Services` and `Services.Models`.

```csharp
// Services/Models/SentenceSpec.cs
namespace PassphraseGenerator.Services.Models;
public record SentenceSpec(int WordCount = 6, string Language = "en");

// Services/IWordService.cs
using PassphraseGenerator.Services.Models;
namespace PassphraseGenerator.Services;
public interface IWordService
{
    Task<IReadOnlyList<string>> GetSentenceAsync(SentenceSpec spec);
}
```

### 3. Lazy-load HTTP resources with in-memory caching

Fetch JSON assets on first request per cache key; subsequent calls return from the dictionary.

```csharp
private readonly Dictionary<string, Dictionary<string, string>> _cache = new();

private async Task<Dictionary<string, string>?> GetOrLoadAsync(string key, string path)
{
    if (_cache.TryGetValue(key, out var cached)) return cached;
    var json = await _http.GetStringAsync(path);
    var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    if (result is not null) _cache[key] = result;
    return result;
}
```

### 4. Crypto-secure RNG — always use `RandomNumberGenerator.GetInt32`

```csharp
// Dice roll [1,6]
int roll = RandomNumberGenerator.GetInt32(1, 7);

// Random index [0, count)
int idx = RandomNumberGenerator.GetInt32(0, count);
```

Never use `System.Random` or `Math.random()` in security-critical paths.

## Examples

- `src/PassphraseGenerator/Services/LocalRandomService.cs` — CSPRNG implementation
- `src/PassphraseGenerator/Services/WordlistService.cs` — lazy-load + cache pattern
- `src/PassphraseGenerator/Program.cs` — DI registration

## Anti-Patterns

- ❌ `builder.Services.AddSingleton<IWordlistService, WordlistService>()` — fails if WordlistService depends on HttpClient (scoped)
- ❌ `new Random()` or `Math.random()` for passphrase randomness
- ❌ Defining model records directly in `Services/` namespace when service interfaces also live there — causes ambiguous reference errors in consumers
