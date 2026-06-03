using PassphraseGenerator.Core.Abstractions;
using PassphraseGenerator.Core.Models;

namespace PassphraseGenerator.Core.Services;

/// <summary>
/// Applies character-level transformations to a word list to produce a final password.
/// Transformation order: Capital → Numbers → Special → Join.
/// </summary>
public sealed class PasswordService : IPasswordService
{
    private static readonly char[] SpecialChars =
        "!@#$%^&*()_-+=`~<>,./?:;\"'{}[]\\|".ToCharArray();

    private readonly IRandomService _random;

    public PasswordService(IRandomService random)
    {
        _random = random;
    }

    public string GeneratePassword(PasswordSpec spec, IReadOnlyList<string> words)
    {
        // Work on mutable copies so transformations don't affect the displayed sentence
        var mutable = words.Select(w => w.ToCharArray()).ToList();

        if (spec.UseCapital)
            ApplyCapital(mutable);

        if (spec.UseNumbers)
            ApplyReplacement(mutable, () => _random.GetInt(0, 10).ToString()[0]);

        if (spec.UseSpecialCharacters)
            ApplyReplacement(mutable, () => SpecialChars[_random.GetInt(0, SpecialChars.Length)]);

        var separator = spec.UseSpace ? " " : string.Empty;
        return string.Join(separator, mutable.Select(chars => new string(chars)));
    }

    private void ApplyCapital(List<char[]> words)
    {
        var wordIdx = _random.GetInt(0, words.Count);
        var charIdx = _random.GetInt(0, words[wordIdx].Length);
        words[wordIdx][charIdx] = char.ToUpperInvariant(words[wordIdx][charIdx]);
    }

    private void ApplyReplacement(List<char[]> words, Func<char> charFactory)
    {
        var wordIdx = _random.GetInt(0, words.Count);
        var charIdx = _random.GetInt(0, words[wordIdx].Length);
        words[wordIdx][charIdx] = charFactory();
    }
}
