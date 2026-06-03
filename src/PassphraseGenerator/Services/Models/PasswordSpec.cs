namespace PassphraseGenerator.Services.Models;

/// <summary>Specifies which transformations to apply when generating the final password.</summary>
public record PasswordSpec(
    bool UseSpace = true,
    bool UseCapital = true,
    bool UseNumbers = false,
    bool UseSpecialCharacters = true);
