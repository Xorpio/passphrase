---
name: "blazor-wasm-testing"
description: "Configure xUnit + bUnit test project for Blazor WASM with correct SDK and mocking patterns"
domain: "testing"
confidence: "high"
source: "earned (PassphraseGenerator Phase 1, Moody, 2026-06-03)"
---

## Context

Testing Blazor WASM apps with bUnit requires specific project configuration and mocking patterns that differ from standard .NET test projects.

## Patterns

### Test project SDK
Use `Microsoft.NET.Sdk.Razor` in the test `.csproj`, NOT `Microsoft.NET.Sdk`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
```

### Required packages
```xml
<PackageReference Include="bunit" Version="1.37.7" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
<PackageReference Include="coverlet.msbuild" Version="10.0.1" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
```

### _Imports.razor in test project
```razor
@using Microsoft.AspNetCore.Components
@using Bunit
@using YourApp.Services
@using YourApp.Components
@using Moq
```

### Service registration order (CRITICAL)
Register ALL services **before** calling `RenderComponent<T>()`:
```csharp
Services.AddSingleton(wordServiceMock.Object);
Services.AddSingleton(passwordServiceMock.Object);
var cut = RenderComponent<MyComponent>(); // AFTER all service registrations
```

### bUnit API — disabled button check
```csharp
// CORRECT
Assert.True(element.HasAttribute("disabled"));
// WRONG — method does not exist
Assert.True(element.IsDisabled());
```

### Mocking HttpMessageHandler for WordlistService
```csharp
var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
handlerMock.Protected()
    .Setup<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
    .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });
var http = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
```

### Deterministic IRandomService mock
```csharp
int idx = 0;
var mockRandom = new Mock<IRandomService>();
mockRandom.Setup(r => r.GetIntAsync(It.IsAny<int>(), It.IsAny<int>()))
    .ReturnsAsync(() => sequence[idx++ % sequence.Length]);
```

### Flushing async state in bUnit
```csharp
await cut.InvokeAsync(() => Task.CompletedTask); // flushes OnInitializedAsync etc.
```

### Component that requires prior action before copy button
Ensure password is populated before testing clipboard:
```csharp
cut.Find("button.btn-primary").Click();        // Generate
await cut.InvokeAsync(() => Task.CompletedTask);
cut.Find("button[aria-label*='copy']").Click(); // Copy (now enabled)
```

## Anti-Patterns

- Using `Microsoft.NET.Sdk` (not Razor) — bUnit fails to resolve Razor components
- Registering services AFTER `RenderComponent<T>()` — services are already injected
- Service implementations that call `RandomNumberGenerator.GetInt32` directly — bypasses mock, tests become non-deterministic
- Testing sync `GeneratePassword` without verifying that `IRandomService` is injected (not hardcoded RNG)
