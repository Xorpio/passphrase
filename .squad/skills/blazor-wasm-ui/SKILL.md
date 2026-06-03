---
name: "blazor-wasm-ui"
description: "Patterns for building accessible, Bootstrap-5 Blazor WASM components with JS interop"
domain: "frontend, blazor, accessibility"
confidence: "high"
source: "earned"
---

## Context
Building Blazor WASM SPA components with Bootstrap 5, Bootstrap Icons, and JavaScript interop for features like clipboard access. Applies when creating Razor components with code-behind, form bindings, and accessibility requirements.

## Patterns

### Razor variable naming in @foreach
Never name loop variables `code`, `page`, `section`, or any other Razor directive keyword.
```razor
@foreach (var langCode in _languages)  // ✅ Not `var code`
{
    <option value="@langCode.Key">@langCode.Value</option>
}
```

### Partial class code-behind
Use `ComponentName.razor.cs` with `public partial class ComponentName : ComponentBase`.
Inject services with `[Inject]` attribute.

### Language change detection
```csharp
private string _selectedLanguage = "nl";
private string _previousLanguage = "nl";

protected override async Task OnParametersSetAsync()
{
    if (_selectedLanguage != _previousLanguage)
    {
        _previousLanguage = _selectedLanguage;
        await RegenerateSentence();
    }
}
```

### Accessible stepper control
```razor
<div role="group" aria-labelledby="wordCountLabel">
    <button class="btn stepper-btn" disabled="@(_count <= Min)" aria-label="Minder">
        <i class="bi bi-chevron-left" aria-hidden="true"></i>
    </button>
    <span aria-live="polite" aria-atomic="true" role="status">@_count</span>
    <button class="btn stepper-btn" disabled="@(_count >= Max)" aria-label="Meer">
        <i class="bi bi-chevron-right" aria-hidden="true"></i>
    </button>
</div>
```

### Copy-to-clipboard with fallback
`wwwroot/js/interop.js`:
```javascript
window.clipboardInterop = {
    copyText: async function (text) {
        try {
            if (navigator.clipboard?.writeText) {
                await navigator.clipboard.writeText(text);
                return true;
            }
            // execCommand fallback
            ...
        } catch { return false; }
    }
};
```
Call from C#: `await JS.InvokeAsync<bool>("clipboardInterop.copyText", _password)`

### 2-second visual feedback
```csharp
_copiedResetCts?.Cancel();
_copiedResetCts = new CancellationTokenSource();
var token = _copiedResetCts.Token;
await Task.Delay(2000, token);
if (!token.IsCancellationRequested) { _copied = false; StateHasChanged(); }
```

### Bootstrap Icons usage
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
<!-- usage: -->
<i class="bi bi-clipboard" aria-hidden="true"></i>
```

### WCAG touch target for custom buttons
```css
.stepper-btn {
    min-width: 44px;
    min-height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
}
```

## Examples
- `src/PassphraseGenerator/Components/PassphraseForm.razor` — full form component
- `src/PassphraseGenerator/Components/PassphraseForm.razor.cs` — code-behind
- `src/PassphraseGenerator/wwwroot/js/interop.js` — clipboard JS

## Anti-Patterns
- Do NOT name `@foreach` variables after Razor directives (`code`, `page`, `functions`, `inherits`, etc.)
- Do NOT use `@bind-Value` on plain `<input>` checkboxes; use `@bind` directly
- Do NOT call `StateHasChanged()` before async work without `await Task.Yield()` — use loading flags instead
- Do NOT block on async calls (`GetAwaiter().GetResult()`) in components — propagate async properly
