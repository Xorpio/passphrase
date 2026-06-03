namespace PassphraseGenerator.Services;

/// <summary>Cryptographically-secure random number service.</summary>
public interface IRandomService
{
    /// <summary>Simulates a single six-sided dice roll, returning a value in [1, 6].</summary>
    Task<int> GetDiceRollAsync();

    /// <summary>Returns <paramref name="count"/> independent dice rolls, each in [1, 6].</summary>
    Task<List<int>> GetDiceRollsAsync(int count);

    /// <summary>Returns a cryptographically-secure random integer in [minInclusive, maxExclusive).</summary>
    Task<int> GetIntAsync(int minInclusive, int maxExclusive);
}
