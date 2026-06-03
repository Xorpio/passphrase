using PassphraseGenerator.Services;

namespace PassphraseGenerator.Tests.Services;

/// <summary>
/// Unit tests for LocalRandomService — validates crypto-secure randomness behaviour.
/// </summary>
public class RandomServiceTests
{
    private readonly LocalRandomService _sut = new();

    // ------- GetDiceRollAsync -------

    [Fact]
    public async Task GetDiceRollAsync_ReturnsValueInRange1To6()
    {
        var roll = await _sut.GetDiceRollAsync();
        Assert.InRange(roll, 1, 6);
    }

    [Fact]
    public async Task GetDiceRollAsync_Multiple_NeverOutsideRange()
    {
        for (int i = 0; i < 200; i++)
        {
            var roll = await _sut.GetDiceRollAsync();
            Assert.InRange(roll, 1, 6);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(100)]
    public async Task GetDiceRollAsync_OverManyCallsAllValuesStayInBounds(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            var roll = await _sut.GetDiceRollAsync();
            Assert.True(roll >= 1 && roll <= 6, $"Roll {roll} out of [1,6]");
        }
    }

    // ------- GetDiceRollsAsync -------

    [Fact]
    public async Task GetDiceRollsAsync_Returns5ValuesWhenAsked()
    {
        var rolls = await _sut.GetDiceRollsAsync(5);
        Assert.Equal(5, rolls.Count);
    }

    [Fact]
    public async Task GetDiceRollsAsync_AllValuesInRange1To6()
    {
        var rolls = await _sut.GetDiceRollsAsync(5);
        Assert.All(rolls, r => Assert.InRange(r, 1, 6));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task GetDiceRollsAsync_ReturnsCorrectCount(int count)
    {
        var rolls = await _sut.GetDiceRollsAsync(count);
        Assert.Equal(count, rolls.Count);
    }

    // ------- GetIntAsync -------

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 7)]
    [InlineData(0, 2)]
    public async Task GetIntAsync_ReturnsValueInRequestedRange(int min, int max)
    {
        for (int i = 0; i < 100; i++)
        {
            var value = await _sut.GetIntAsync(min, max);
            Assert.InRange(value, min, max - 1);
        }
    }

    // ------- Crypto distribution test (chi-square) -------

    /// <summary>
    /// Collect 1 200 dice rolls (200 per face) and perform a chi-square
    /// goodness-of-fit test. A fair die has expected count = 200 per bucket.
    /// Chi-square at 5 df, p=0.001 critical value ≈ 20.5. We reject
    /// deterministic patterns but accept genuine randomness.
    /// </summary>
    [Fact]
    public async Task GetDiceRollAsync_Distribution_IsApproximatelyUniform()
    {
        const int totalRolls = 1200;
        const int faces = 6;
        const double expected = totalRolls / (double)faces;      // 200
        const double criticalValue = 20.5;                       // chi-square 5 df p=0.001

        var counts = new int[faces + 1]; // index 1..6
        for (int i = 0; i < totalRolls; i++)
        {
            var roll = await _sut.GetDiceRollAsync();
            counts[roll]++;
        }

        double chiSquare = 0;
        for (int face = 1; face <= faces; face++)
        {
            double diff = counts[face] - expected;
            chiSquare += (diff * diff) / expected;
        }

        Assert.True(chiSquare < criticalValue,
            $"Chi-square {chiSquare:F2} exceeds critical value {criticalValue}. " +
            $"Distribution: {string.Join(", ", counts.Skip(1).Select((c, i) => $"{i + 1}:{c}"))}");
    }

    /// <summary>
    /// Verifies GetDiceRollsAsync produces all 6 faces over 600 rolls
    /// (sanity check that no face is systematically missing).
    /// </summary>
    [Fact]
    public async Task GetDiceRollsAsync_600Rolls_AllFacesPresent()
    {
        var rolls = await _sut.GetDiceRollsAsync(600);
        var distinct = rolls.Distinct().ToHashSet();
        for (int face = 1; face <= 6; face++)
            Assert.Contains(face, distinct);
    }
}
