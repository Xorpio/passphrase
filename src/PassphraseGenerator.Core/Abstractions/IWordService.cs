using PassphraseGenerator.Core.Models;

namespace PassphraseGenerator.Core.Abstractions;

/// <summary>Generates Diceware word sentences.</summary>
public interface IWordService
{
    /// <summary>Returns a list of Diceware words as specified by <paramref name="spec"/>.</summary>
    Task<IReadOnlyList<string>> GetSentenceAsync(SentenceSpec spec);
}
