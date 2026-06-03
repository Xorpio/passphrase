using Microsoft.Extensions.Logging;
using PassphraseGenerator.Services.Models;

namespace PassphraseGenerator.Services;

public class WordService : IWordService
{
    private readonly IRandomService _random;
    private readonly IWordlistService _wordlist;
    private readonly ILogger<WordService> _logger;

    public WordService(IRandomService random, IWordlistService wordlist, ILogger<WordService> logger)
    {
        _random = random;
        _wordlist = wordlist;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetSentenceAsync(SentenceSpec spec)
    {
        var words = new List<string>(spec.WordCount);

        for (int i = 0; i < spec.WordCount; i++)
        {
            var rolls = await _random.GetDiceRollsAsync(5);
            var key = string.Concat(rolls);
            var word = await _wordlist.GetWordAsync(spec.Language, key);

            if (word is null)
            {
                _logger.LogWarning("No word found for key ''{Key}'' in language ''{Language}''. Using key as fallback.", key, spec.Language);
                word = key;
            }

            words.Add(word);
        }

        return words.AsReadOnly();
    }
}
