using System.Security.Cryptography;
using PassphraseGenerator.Core.Abstractions;

namespace PassphraseGenerator.Core.Services;

/// <summary>Cryptographically-secure RNG backed by <see cref="RandomNumberGenerator.GetInt32"/>.</summary>
public sealed class RandomService : IRandomService
{
    public int GetInt(int minInclusive, int maxExclusive)
        => RandomNumberGenerator.GetInt32(minInclusive, maxExclusive);

    public Task<int> GetDiceRollAsync()
        => Task.FromResult(GetInt(1, 7));

    public Task<IReadOnlyList<int>> GetDiceRollsAsync(int count)
    {
        var rolls = new List<int>(count);
        for (var i = 0; i < count; i++)
            rolls.Add(GetInt(1, 7));
        return Task.FromResult<IReadOnlyList<int>>(rolls);
    }
}
