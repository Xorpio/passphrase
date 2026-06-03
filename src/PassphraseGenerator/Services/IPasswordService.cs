using PassphraseGenerator.Services.Models;

namespace PassphraseGenerator.Services;

/// <summary>Applies character-level transformations to a word list to produce a password.</summary>
public interface IPasswordService
{
    /// <summary>Joins and transforms words according to spec. Synchronous.</summary>
    string GeneratePassword(PasswordSpec spec, IReadOnlyList<string> words);
}
