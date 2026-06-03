namespace PassphraseGenerator.Core.Abstractions;

/// <summary>Cryptographically-secure random number service (wraps RandomNumberGenerator).</summary>
public interface IRandomService
{
    /// <summary>Returns a cryptographically-secure random integer in [minInclusive, maxExclusive).</summary>
    int GetInt(int minInclusive, int maxExclusive);

    /// <summary>Simulates a single six-sided dice roll, returning a value in [1, 6].</summary>
    Task<int> GetDiceRollAsync();

    /// <summary>Returns <paramref name="count"/> independent dice rolls, each in [1, 6].</summary>
    Task<IReadOnlyList<int>> GetDiceRollsAsync(int count);
}
