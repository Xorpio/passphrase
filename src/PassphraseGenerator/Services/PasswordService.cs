using Microsoft.Extensions.Logging;
using PassphraseGenerator.Services.Models;

namespace PassphraseGenerator.Services;

/// <summary>
/// Applies character-level transformations to a Diceware word list.
/// Transformation order: Capital -> Number -> Special -> Join.
/// </summary>
public class PasswordService : IPasswordService
{
    private static readonly char[] SpecialChars = "!@#$%^&*()_-+=`~<>,./?:;\"'{}[]\\|".ToCharArray();

    private readonly IRandomService _random;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(IRandomService random, ILogger<PasswordService> logger)
    {
        _random = random;
        _logger = logger;
    }

    public string GeneratePassword(PasswordSpec spec, IReadOnlyList<string> words)
    {
        if (words.Count == 0)
            return string.Empty;

        var mutableWords = words.Select(w => w.ToCharArray()).ToList();

        if (spec.UseCapital)
            ApplyTransform(mutableWords, c => char.ToUpperInvariant(c));

        if (spec.UseNumbers)
            ApplyTransform(mutableWords, _ => (char)('0' + GetInt(0, 10)));

        if (spec.UseSpecialCharacters)
            ApplyTransform(mutableWords, _ => SpecialChars[GetInt(0, SpecialChars.Length)]);

        var separator = spec.UseSpace ? " " : string.Empty;
        return string.Join(separator, mutableWords.Select(chars => new string(chars)));
    }

    private void ApplyTransform(List<char[]> words, Func<char, char> transform)
    {
        var wordIdx = GetInt(0, words.Count);
        var word = words[wordIdx];
        if (word.Length == 0) return;
        var charIdx = GetInt(0, word.Length);
        word[charIdx] = transform(word[charIdx]);
    }

    private int GetInt(int min, int max) => _random.GetIntAsync(min, max).GetAwaiter().GetResult();
}

