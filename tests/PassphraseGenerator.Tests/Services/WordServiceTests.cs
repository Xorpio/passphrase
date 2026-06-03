using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PassphraseGenerator.Services;
using PassphraseGenerator.Services.Models;
using PassphraseGenerator.Tests.TestFixtures;

namespace PassphraseGenerator.Tests.Services;

/// <summary>
/// Unit tests for WordService — Diceware key generation and word lookup.
/// </summary>
public class WordServiceTests
{
    // -------- Helpers --------

    /// <summary>
    /// Creates a mock IRandomService that cycles through the provided roll sequences.
    /// Each call to GetDiceRollsAsync returns the next batch of 5 rolls.
    /// </summary>
    private static Mock<IRandomService> CreateDeterministicRandom(params int[][] rollBatches)
    {
        int callIndex = 0;
        var mockRandom = new Mock<IRandomService>();
        mockRandom
            .Setup(r => r.GetDiceRollsAsync(5))
            .ReturnsAsync(() =>
            {
                var rolls = rollBatches[callIndex % rollBatches.Length];
                callIndex++;
                return rolls.ToList();
            });
        return mockRandom;
    }

    /// <summary>Creates a mock IWordlistService backed by the test English wordlist.</summary>
    private static Mock<IWordlistService> CreateWordlistMock(
        Dictionary<string, string>? list = null)
    {
        list ??= TestWordlist.English;
        var mock = new Mock<IWordlistService>();
        mock.Setup(w => w.GetWordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string lang, string key) =>
                list.TryGetValue(key, out var word) ? word : null);
        mock.Setup(w => w.SupportedLanguages)
            .Returns(new Dictionary<string, string> { ["en"] = "English", ["nl"] = "Nederlands" });
        return mock;
    }

    // -------- Word count --------

    [Fact]
    public async Task GetSentenceAsync_Returns3Words_WhenSpecRequires3()
    {
        var rollBatches = Enumerable.Range(0, 3)
            .Select(_ => new[] { 1, 1, 1, 1, 1 }).ToArray();
        var sut = new WordService(
            CreateDeterministicRandom(rollBatches).Object,
            CreateWordlistMock().Object,
            NullLogger<WordService>.Instance);

        var words = await sut.GetSentenceAsync(new SentenceSpec(3, "en"));
        Assert.Equal(3, words.Count);
    }

    [Fact]
    public async Task GetSentenceAsync_Returns6Words_Default()
    {
        var rollBatches = Enumerable.Range(0, 6)
            .Select(_ => new[] { 1, 1, 1, 1, 1 }).ToArray();
        var sut = new WordService(
            CreateDeterministicRandom(rollBatches).Object,
            CreateWordlistMock().Object,
            NullLogger<WordService>.Instance);

        var words = await sut.GetSentenceAsync(new SentenceSpec());
        Assert.Equal(6, words.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public async Task GetSentenceAsync_ReturnsCorrectCount_ForEdgeCases(int count)
    {
        var rollBatches = Enumerable.Range(0, count)
            .Select(_ => new[] { 1, 1, 1, 1, 1 }).ToArray();
        var sut = new WordService(
            CreateDeterministicRandom(rollBatches).Object,
            CreateWordlistMock().Object,
            NullLogger<WordService>.Instance);

        var words = await sut.GetSentenceAsync(new SentenceSpec(count, "en"));
        Assert.Equal(count, words.Count);
    }

    // -------- Diceware key construction --------

    [Fact]
    public async Task GetSentenceAsync_DicewareKey_1_1_1_1_1_LookupsCorrectWord()
    {
        // Rolls [1,1,1,1,1] → key "11111" → "aardvark" in TestWordlist.English
        var mockRandom = CreateDeterministicRandom(new[] { 1, 1, 1, 1, 1 });
        var sut = new WordService(
            mockRandom.Object,
            CreateWordlistMock().Object,
            NullLogger<WordService>.Instance);

        var words = await sut.GetSentenceAsync(new SentenceSpec(1, "en"));
        Assert.Equal("aardvark", words[0]);
    }

    [Fact]
    public async Task GetSentenceAsync_DicewareKey_FormattedFromRolls()
    {
        // Rolls [1,1,1,3,4] → key "11134" → "accord" in TestWordlist.English
        var mockRandom = CreateDeterministicRandom(new[] { 1, 1, 1, 3, 4 });
        var sut = new WordService(
            mockRandom.Object,
            CreateWordlistMock().Object,
            NullLogger<WordService>.Instance);

        var words = await sut.GetSentenceAsync(new SentenceSpec(1, "en"));
        Assert.Equal("accord", words[0]);
    }

    // -------- All words non-empty --------

    [Fact]
    public async Task GetSentenceAsync_AllWordsAreNonEmpty()
    {
        var rollBatches = Enumerable.Range(0, 5)
            .Select(_ => new[] { 1, 1, 1, 1, 1 }).ToArray();
        var sut = new WordService(
            CreateDeterministicRandom(rollBatches).Object,
            CreateWordlistMock().Object,
            NullLogger<WordService>.Instance);

        var words = await sut.GetSentenceAsync(new SentenceSpec(5, "en"));
        Assert.All(words, w => Assert.False(string.IsNullOrEmpty(w)));
    }

    // -------- Key-not-found fallback --------

    [Fact]
    public async Task GetSentenceAsync_WhenWordNotFound_UsesDicewareKeyAsFallback()
    {
        // Empty wordlist → every lookup returns null → fallback to key string
        var emptyMock = new Mock<IWordlistService>();
        emptyMock.Setup(w => w.GetWordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string? )null);

        var sut = new WordService(
            CreateDeterministicRandom(new[] { 1, 1, 1, 1, 1 }).Object,
            emptyMock.Object,
            NullLogger<WordService>.Instance);

        var words = await sut.GetSentenceAsync(new SentenceSpec(1, "en"));
        Assert.Equal("11111", words[0]);
    }

    // -------- Language forwarding --------

    [Fact]
    public async Task GetSentenceAsync_ForwardsLanguageToWordlistService()
    {
        var wordlistMock = CreateWordlistMock(TestWordlist.Dutch);
        var sut = new WordService(
            CreateDeterministicRandom(new[] { 1, 1, 1, 1, 1 }).Object,
            wordlistMock.Object,
            NullLogger<WordService>.Instance);

        await sut.GetSentenceAsync(new SentenceSpec(1, "nl"));

        wordlistMock.Verify(w => w.GetWordAsync("nl", It.IsAny<string>()), Times.Once);
    }
}
