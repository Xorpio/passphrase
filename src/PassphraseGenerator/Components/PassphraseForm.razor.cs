using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PassphraseGenerator.Services;
using PassphraseGenerator.Services.Models;

namespace PassphraseGenerator.Components;

public partial class PassphraseForm : ComponentBase
{
    [Inject] private IWordService WordService { get; set; } = default!;
    [Inject] private IPasswordService PasswordService { get; set; } = default!;
    [Inject] private IWordlistService WordlistService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private const int MinWords = 3;
    private const int MaxWords = 10;
    private const int DefaultWords = 5;

    // --- State ---
    private string _selectedLanguage = "nl";
    private string _previousLanguage = "nl";
    private int _wordCount = DefaultWords;

    private bool _useSpace = true;
    private bool _useCapital = true;
    private bool _useNumbers = false;
    private bool _useSpecialCharacters = true;

    private IReadOnlyList<string> _generatedWords = Array.Empty<string>();
    private string _sentenceDisplay = string.Empty;
    private string _generatedPassword = string.Empty;

    private bool _isLoading = false;
    private string? _error = null;
    private bool _copied = false;
    private CancellationTokenSource? _copiedResetCts;

    private IReadOnlyDictionary<string, string> _languages = new Dictionary<string, string>();

    // --- Lifecycle ---
    protected override async Task OnInitializedAsync()
    {
        _languages = WordlistService.SupportedLanguages;
        await RegenerateSentence();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_selectedLanguage != _previousLanguage)
        {
            _previousLanguage = _selectedLanguage;
            await RegenerateSentence();
        }
    }

    // --- Actions ---
    private void IncrementWordCount()
    {
        if (_wordCount < MaxWords) _wordCount++;
    }

    private void DecrementWordCount()
    {
        if (_wordCount > MinWords) _wordCount--;
    }

    private async Task RegenerateSentence()
    {
        _isLoading = true;
        _error = null;
        StateHasChanged();

        try
        {
            var spec = new SentenceSpec(_wordCount, _selectedLanguage);
            _generatedWords = await WordService.GetSentenceAsync(spec);
            _sentenceDisplay = string.Join(" ", _generatedWords);
            // Re-apply password transformations with fresh words
            if (!string.IsNullOrEmpty(_generatedPassword))
                GeneratePasswordInternal();
        }
        catch (Exception ex)
        {
            _error = $"Fout bij het laden van de woordenlijst: {ex.Message}";
            _generatedWords = Array.Empty<string>();
            _sentenceDisplay = string.Empty;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void GeneratePassword()
    {
        if (_generatedWords.Count == 0) return;
        GeneratePasswordInternal();
    }

    private void GeneratePasswordInternal()
    {
        var spec = new PasswordSpec(_useSpace, _useCapital, _useNumbers, _useSpecialCharacters);
        _generatedPassword = PasswordService.GeneratePassword(spec, _generatedWords);
    }

    private async Task CopyToClipboard()
    {
        if (string.IsNullOrEmpty(_generatedPassword)) return;

        try
        {
            var success = await JS.InvokeAsync<bool>("clipboardInterop.copyText", _generatedPassword);
            if (success)
            {
                _copied = true;
                StateHasChanged();

                _copiedResetCts?.Cancel();
                _copiedResetCts = new CancellationTokenSource();
                var token = _copiedResetCts.Token;

                await Task.Delay(2000, token);
                if (!token.IsCancellationRequested)
                {
                    _copied = false;
                    StateHasChanged();
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Normal cancellation — ignore
        }
        catch (Exception ex)
        {
            _error = $"Kopiëren mislukt: {ex.Message}";
        }
    }
}
