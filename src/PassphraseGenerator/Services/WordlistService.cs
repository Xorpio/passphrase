using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PassphraseGenerator.Services;

public class WordlistService : IWordlistService
{
    private readonly HttpClient _http;
    private readonly ILogger<WordlistService> _logger;
    private readonly Dictionary<string, Dictionary<string, string>> _cache = new();

    private static readonly Dictionary<string, string> LanguageFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        { "en", "data/diceware.wordlist.json" },
        { "nl", "data/DicewareDutch.json" }
    };

    public IReadOnlyDictionary<string, string> SupportedLanguages { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["nl"] = "Nederlands",
            ["en"] = "English"
        };

    public WordlistService(HttpClient http, ILogger<WordlistService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<string?> GetWordAsync(string language, string key)
    {
        var resolvedLanguage = LanguageFiles.ContainsKey(language) ? language : "en";
        if (resolvedLanguage != language)
        {
            _logger.LogWarning("Unknown language '{Language}', falling back to 'en'.", language);
        }

        var wordlist = await GetOrLoadWordlistAsync(resolvedLanguage);
        if (wordlist is null) return null;

        return wordlist.TryGetValue(key, out var word) ? word : null;
    }

    private async Task<Dictionary<string, string>?> GetOrLoadWordlistAsync(string language)
    {
        if (_cache.TryGetValue(language, out var cached))
            return cached;

        if (!LanguageFiles.TryGetValue(language, out var filePath))
            return null;

        try
        {
            var json = await _http.GetStringAsync(filePath);
            var wordlist = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (wordlist is not null)
            {
                _cache[language] = wordlist;
                _logger.LogInformation("Loaded {Count} words for language '{Language}'.", wordlist.Count, language);
            }
            return wordlist;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to load word list for language '{Language}' from '{FilePath}'.", language, filePath);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize word list for language '{Language}'.", language);
            return null;
        }
    }
}
