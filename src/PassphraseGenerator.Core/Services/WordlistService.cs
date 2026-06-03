using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PassphraseGenerator.Core.Abstractions;

namespace PassphraseGenerator.Core.Services;

/// <summary>Loads and caches Diceware word list JSON files from wwwroot/data/.</summary>
public sealed class WordlistService : IWordlistService
{
    private static readonly Dictionary<string, string> AssetPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["NL"] = "data/DicewareDutch.json",
        ["EN"] = "data/diceware.wordlist.json",
    };

    private readonly Dictionary<string, Dictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly HttpClient _http;
    private readonly ILogger<WordlistService> _logger;

    public WordlistService(HttpClient http, ILogger<WordlistService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public IReadOnlyList<string> SupportedLanguages => AssetPaths.Keys.ToList();

    public async Task<Dictionary<string, string>> GetListAsync(string language)
    {
        var lang = language.ToUpperInvariant();

        if (_cache.TryGetValue(lang, out var cached))
            return cached;

        if (!AssetPaths.TryGetValue(lang, out var path))
        {
            _logger.LogWarning("Unknown language '{Language}', falling back to EN.", language);
            lang = "EN";
            path = AssetPaths["EN"];
        }

        // Already cached under the resolved key after fallback
        if (_cache.TryGetValue(lang, out cached))
            return cached;

        var dict = await _http.GetFromJsonAsync<Dictionary<string, string>>(path)
                   ?? throw new InvalidOperationException($"Word list for '{lang}' returned null.");

        _cache[lang] = dict;
        return dict;
    }

    public async Task<string?> GetWordAsync(string language, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must not be null or empty.", nameof(key));

        var list = await GetListAsync(language);
        list.TryGetValue(key, out var word);
        return word;
    }
}
