using PassphraseGenerator.Services.Models;

namespace PassphraseGenerator.Services;

public interface IWordService
{
    Task<IReadOnlyList<string>> GetSentenceAsync(SentenceSpec spec);
}
