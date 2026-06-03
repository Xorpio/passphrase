namespace PassphraseGenerator.Core.Models;

/// <summary>Specification for applying transformations to a word list to produce a password.</summary>
public record PasswordSpec(
    bool UseSpace = true,
    bool UseCapital = true,
    bool UseNumbers = false,
    bool UseSpecialCharacters = true);
