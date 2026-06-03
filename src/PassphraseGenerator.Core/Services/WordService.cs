using PassphraseGenerator.Core.Abstractions;
using PassphraseGenerator.Core.Models;

namespace PassphraseGenerator.Core.Services;

/// <summary>Generates a Diceware word sentence using <see cref="IRandomService"/> and <see cref="IWordlistService"/>.</summary>
public sealed class WordService : IWordService
{
    private readonly IRandomService _random;
    private readonly IWordlistService _wordlist;

    public WordService(IRandomService random, IWordlistService wordlist)
    {
        _random = random;
        _wordlist = wordlist;
    }

    public async Task<IReadOnlyList<string>> GetSentenceAsync(SentenceSpec spec)
    {
        var words = new List<string>(spec.Words);

        for (var i = 0; i < spec.Words; i++)
        {
            var key = await GenerateDicewareKeyAsync();
            var word = await _wordlist.GetWordAsync(spec.Language, key)
                       ?? key; // fallback: use the key itself if lookup fails
            words.Add(word);
        }

        return words;
    }

    /// <summary>Generates a 5-digit Diceware key by rolling a d6 five times.</summary>
    internal async Task<string> GenerateDicewareKeyAsync()
    {
        var rolls = await _random.GetDiceRollsAsync(5);
        return string.Concat(rolls.Select(r => r.ToString()));
    }
}
