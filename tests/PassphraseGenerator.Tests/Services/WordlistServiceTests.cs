using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using PassphraseGenerator.Services;
using PassphraseGenerator.Tests.TestFixtures;

namespace PassphraseGenerator.Tests.Services;

/// <summary>
/// Unit tests for WordlistService — caching, language fallback, and HTTP error handling.
/// </summary>
public class WordlistServiceTests
{
    // -------- Helpers --------

    private static (WordlistService service, Mock<HttpMessageHandler> handlerMock) CreateService(
        Dictionary<string, string>? wordlist = null)
    {
        wordlist ??= TestWordlist.English;
        var json = TestWordlist.ToJson(wordlist);

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var http = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var service = new WordlistService(http, NullLogger<WordlistService>.Instance);
        return (service, handlerMock);
    }

    private static (WordlistService service, Mock<HttpMessageHandler> handlerMock) CreateServiceWithMultipleWordlists()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Return EN wordlist for the EN asset path
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.PathAndQuery.Contains("diceware.wordlist.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestWordlist.ToJson(TestWordlist.English))
            });

        // Return NL wordlist for the NL asset path
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.PathAndQuery.Contains("DicewareDutch.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestWordlist.ToJson(TestWordlist.Dutch))
            });

        var http = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var service = new WordlistService(http, NullLogger<WordlistService>.Instance);
        return (service, handlerMock);
    }

    private static (WordlistService service, Mock<HttpMessageHandler> handlerMock) CreateServiceWithHttpError()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated network error"));

        var http = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
        var service = new WordlistService(http, NullLogger<WordlistService>.Instance);
        return (service, handlerMock);
    }

    // -------- SupportedLanguages --------

    [Fact]
    public void SupportedLanguages_ContainsNlAndEn()
    {
        var (service, _) = CreateService();
        Assert.Contains("nl", service.SupportedLanguages.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("en", service.SupportedLanguages.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void SupportedLanguages_HasDisplayNames()
    {
        var (service, _) = CreateService();
        Assert.NotEmpty(service.SupportedLanguages["nl"]);
        Assert.NotEmpty(service.SupportedLanguages["en"]);
    }

    // -------- GetWordAsync — happy path --------

    [Fact]
    public async Task GetWordAsync_ReturnsCorrectWordForKnownKey()
    {
        var (service, _) = CreateService(TestWordlist.English);
        var word = await service.GetWordAsync("en", "11111");
        Assert.Equal("aardvark", word);
    }

    [Fact]
    public async Task GetWordAsync_ReturnsNullForUnknownKey()
    {
        var (service, _) = CreateService();
        var word = await service.GetWordAsync("en", "99999");
        Assert.Null(word);
    }

    [Fact]
    public async Task GetWordAsync_NlLanguage_ReturnsNlWord()
    {
        var (service, _) = CreateServiceWithMultipleWordlists();
        var word = await service.GetWordAsync("nl", "11111");
        Assert.Equal("aarde", word);
    }

    // -------- Caching --------

    [Fact]
    public async Task GetWordAsync_SameLanguageTwice_OnlyMakesOneHttpRequest()
    {
        var (service, handlerMock) = CreateService();

        await service.GetWordAsync("en", "11111");
        await service.GetWordAsync("en", "11112");   // second call should hit cache

        handlerMock
            .Protected()
            .Verify("SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetWordAsync_DifferentLanguages_MakesSeparateHttpRequests()
    {
        var (service, handlerMock) = CreateServiceWithMultipleWordlists();

        await service.GetWordAsync("en", "11111");
        await service.GetWordAsync("nl", "11111");

        handlerMock
            .Protected()
            .Verify("SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    // -------- Language fallback --------

    [Fact]
    public async Task GetWordAsync_UnknownLanguage_FallsBackToEnglish()
    {
        var (service, _) = CreateService(TestWordlist.English);
        // "xx" is not a valid language; should fall back to "en"
        var word = await service.GetWordAsync("xx", "11111");
        Assert.Equal("aardvark", word);   // matches English wordlist entry
    }

    // -------- Network error handling --------

    [Fact]
    public async Task GetWordAsync_NetworkError_ReturnsNull_DoesNotThrow()
    {
        var (service, _) = CreateServiceWithHttpError();
        var word = await service.GetWordAsync("en", "11111");
        Assert.Null(word);   // gracefully returns null instead of throwing
    }
}
