namespace PassphraseGenerator.Services.Models;

/// <summary>Specifies the parameters for generating a Diceware word sentence.</summary>
public record SentenceSpec(int WordCount = 6, string Language = "nl");
