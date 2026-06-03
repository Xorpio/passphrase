namespace PassphraseGenerator.Services;

/// <summary>Loads and caches Diceware word lists by language.</summary>
public interface IWordlistService
{
    /// <summary>Language code to display name mapping for all supported languages.</summary>
    IReadOnlyDictionary<string, string> SupportedLanguages { get; }

    /// <summary>Returns the word for the given key in the given language, or null if not found.</summary>
    Task<string?> GetWordAsync(string language, string key);
}
