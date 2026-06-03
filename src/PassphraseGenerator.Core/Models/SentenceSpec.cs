namespace PassphraseGenerator.Core.Models;

/// <summary>Specification for generating a Diceware word sentence.</summary>
public record SentenceSpec(string Language = "NL", int Words = 6);
