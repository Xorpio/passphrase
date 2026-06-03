namespace PassphraseGenerator.Core.Abstractions;

/// <summary>Loads and caches Diceware word lists per language.</summary>
public interface IWordlistService
{
    /// <summary>Returns the full word list dictionary for <paramref name="language"/>.
    /// Falls back to EN and logs a warning for unknown languages.</summary>
    Task<Dictionary<string, string>> GetListAsync(string language);

    /// <summary>Returns the word for <paramref name="key"/> in <paramref name="language"/>'s list,
    /// or null if the key does not exist.</summary>
    Task<string?> GetWordAsync(string language, string key);

    /// <summary>Returns the supported language codes.</summary>
    IReadOnlyList<string> SupportedLanguages { get; }
}
