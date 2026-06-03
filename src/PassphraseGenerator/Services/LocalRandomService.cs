using System.Security.Cryptography;

namespace PassphraseGenerator.Services;

public class LocalRandomService : IRandomService
{
    public Task<int> GetDiceRollAsync()
    {
        return Task.FromResult(RandomNumberGenerator.GetInt32(1, 7));
    }

    public Task<List<int>> GetDiceRollsAsync(int count)
    {
        var rolls = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            rolls.Add(RandomNumberGenerator.GetInt32(1, 7));
        }
        return Task.FromResult(rolls);
    }

    public Task<int> GetIntAsync(int minInclusive, int maxExclusive)
    {
        return Task.FromResult(RandomNumberGenerator.GetInt32(minInclusive, maxExclusive));
    }
}
