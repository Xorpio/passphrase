using PassphraseGenerator.Core.Models;

namespace PassphraseGenerator.Core.Abstractions;

/// <summary>Applies character-level transformations to a word list to produce a password.</summary>
public interface IPasswordService
{
    /// <summary>Joins and transforms <paramref name="words"/> according to <paramref name="spec"/>.</summary>
    string GeneratePassword(PasswordSpec spec, IReadOnlyList<string> words);
}
