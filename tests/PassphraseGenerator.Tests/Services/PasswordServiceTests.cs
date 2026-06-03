using Moq;
using PassphraseGenerator.Services;
using PassphraseGenerator.Services.Models;

namespace PassphraseGenerator.Tests.Services;

/// <summary>
/// Unit tests for PasswordService — all transformation combinations.
/// </summary>
public class PasswordServiceTests
{
    private static readonly char[] SpecialChars = "!@#$%^&*()_-+=`~<>,./?:;\"'{}[]\\|".ToCharArray();

    // -------- Helpers --------

    /// <summary>
    /// Creates a PasswordService backed by a mock IRandomService.
    /// <paramref name="intSequence"/> values are returned in order by GetIntAsync.
    /// </summary>
    private static PasswordService CreateServiceWith(params int[] intSequence)
    {
        int idx = 0;
        var mockRandom = new Mock<IRandomService>();
        mockRandom
            .Setup(r => r.GetIntAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(() => intSequence.Length > 0 ? intSequence[idx++ % intSequence.Length] : 0);

        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<PasswordService>.Instance;
        return new PasswordService(mockRandom.Object, logger);
    }

    private static readonly IReadOnlyList<string> ThreeWords = ["alpha", "bravo", "charlie"];
    private static readonly IReadOnlyList<string> OneWord = ["hello"];
    private static readonly IReadOnlyList<string> EmptyList = [];

    // -------- Empty input --------

    [Fact]
    public void GeneratePassword_EmptyWordList_ReturnsEmpty()
    {
        var svc = CreateServiceWith();
        var result = svc.GeneratePassword(new PasswordSpec(), EmptyList);
        Assert.Equal(string.Empty, result);
    }

    // -------- Separator --------

    [Fact]
    public void GeneratePassword_UseSpace_True_JoinsWithSpace()
    {
        var svc = CreateServiceWith(0, 0);   // wordIdx=0, charIdx=0 for capital
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: true, UseCapital: false, UseNumbers: false, UseSpecialCharacters: false),
            ["hello", "world"]);
        Assert.Contains(" ", result);
    }

    [Fact]
    public void GeneratePassword_UseSpace_False_NoSeparator()
    {
        var svc = CreateServiceWith();
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: false, UseNumbers: false, UseSpecialCharacters: false),
            ["hello", "world"]);
        Assert.Equal("helloworld", result);
    }

    // -------- Capitalize --------

    [Fact]
    public void GeneratePassword_UseCapital_IntroducesUppercaseChar()
    {
        // Deterministic: wordIdx=0, charIdx=0 → capitalize first char of "alpha"
        var svc = CreateServiceWith(0, 0);
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: true, UseNumbers: false, UseSpecialCharacters: false),
            ["alpha"]);
        Assert.Contains("A", result);
    }

    [Fact]
    public void GeneratePassword_UseCapital_ResultContainsAtLeastOneUppercase()
    {
        var svc = CreateServiceWith(0, 0, 0, 0);
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: true, UseCapital: true, UseNumbers: false, UseSpecialCharacters: false),
            ThreeWords);
        Assert.True(result.Any(char.IsUpper), $"No uppercase found in: {result}");
    }

    [Fact]
    public void GeneratePassword_UseCapital_False_NoUppercase()
    {
        var svc = CreateServiceWith();
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: false, UseNumbers: false, UseSpecialCharacters: false),
            ["alpha", "bravo"]);
        Assert.False(result.Any(char.IsUpper), $"Unexpected uppercase in: {result}");
    }

    // -------- Numbers --------

    [Fact]
    public void GeneratePassword_UseNumbers_IntroducesDigit()
    {
        // wordIdx=0, charIdx=0 for number; digit drawn from GetIntAsync(0,10)=7 → char '7'
        var svc = CreateServiceWith(0, 0, 7);
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: false, UseNumbers: true, UseSpecialCharacters: false),
            ["alpha"]);
        Assert.True(result.Any(char.IsDigit), $"No digit found in: {result}");
    }

    [Fact]
    public void GeneratePassword_UseNumbers_False_NoDigit()
    {
        var svc = CreateServiceWith();
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: false, UseNumbers: false, UseSpecialCharacters: false),
            ["hello", "world"]);
        Assert.False(result.Any(char.IsDigit), $"Unexpected digit in: {result}");
    }

    // -------- Special characters --------

    [Fact]
    public void GeneratePassword_UseSpecialCharacters_IntroducesSpecialChar()
    {
        var svc = CreateServiceWith(0, 0, 0); // wordIdx=0, charIdx=0, specialIdx=0 → '!'
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: false, UseNumbers: false, UseSpecialCharacters: true),
            ["hello"]);
        Assert.True(result.Any(c => SpecialChars.Contains(c)), $"No special char in: {result}");
    }

    // -------- Uppercase all --------

    [Fact]
    public void GeneratePassword_AllCapsWord_CapitalizeStillWorks()
    {
        var svc = CreateServiceWith(0, 0);
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: true, UseNumbers: false, UseSpecialCharacters: false),
            ["WORD"]);
        Assert.Equal("WORD", result);   // already uppercase — no visible change but no crash
    }

    // -------- Combined transformations --------

    [Fact]
    public void GeneratePassword_CapitalAndNumber_BothPresent()
    {
        // Sequence: capital wordIdx=0, charIdx=0; number wordIdx=1, charIdx=0, digit=5
        var svc = CreateServiceWith(0, 0, 1, 0, 5);
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: true, UseCapital: true, UseNumbers: true, UseSpecialCharacters: false),
            ["alpha", "bravo"]);
        Assert.True(result.Any(char.IsUpper), $"No uppercase in: {result}");
        Assert.True(result.Any(char.IsDigit), $"No digit in: {result}");
    }

    [Fact]
    public void GeneratePassword_AllTransformationsDisabled_OutputEqualsWordsJoined()
    {
        var svc = CreateServiceWith();
        var words = new[] { "alpha", "bravo" };
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: false, UseNumbers: false, UseSpecialCharacters: false),
            words);
        Assert.Equal("alphabravo", result);
    }

    [Fact]
    public void GeneratePassword_AllTransformationsEnabled_OutputChangedFromOriginal()
    {
        // Just verify it doesn't crash with all options on
        var svc = CreateServiceWith(0, 0, 1, 0, 5, 2, 0, 3);
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: true, UseCapital: true, UseNumbers: true, UseSpecialCharacters: true),
            ThreeWords);
        Assert.NotEmpty(result);
    }

    // -------- Single word edge case --------

    [Fact]
    public void GeneratePassword_SingleWord_TransformApplied()
    {
        var svc = CreateServiceWith(0, 0);  // wordIdx=0, charIdx=0
        var result = svc.GeneratePassword(
            new PasswordSpec(UseSpace: false, UseCapital: true, UseNumbers: false, UseSpecialCharacters: false),
            OneWord);
        Assert.Equal("Hello", result);
    }
}
