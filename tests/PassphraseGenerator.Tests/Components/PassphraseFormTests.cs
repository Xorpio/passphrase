using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using PassphraseGenerator.Components;
using PassphraseGenerator.Services;
using PassphraseGenerator.Services.Models;

namespace PassphraseGenerator.Tests.Components;

/// <summary>
/// bUnit component tests for PassphraseForm.razor.
/// All service dependencies are mocked — no real HTTP calls.
/// </summary>
public class PassphraseFormTests : TestContext
{
    // -------- Helpers --------

    private static readonly IReadOnlyList<string> SampleWords = ["alpha", "bravo", "charlie", "delta", "echo"];
    private const string SamplePassword = "Alpha bravo charlie delta echo";

    private Mock<IWordService> _wordServiceMock = new();
    private Mock<IPasswordService> _passwordServiceMock = new();
    private Mock<IWordlistService> _wordlistServiceMock = new();

    private void SetupDefaultMocks(
        IReadOnlyList<string>? words = null,
        string? password = null)
    {
        words ??= SampleWords;
        password ??= SamplePassword;

        _wordServiceMock = new Mock<IWordService>();
        _wordServiceMock
            .Setup(s => s.GetSentenceAsync(It.IsAny<SentenceSpec>()))
            .ReturnsAsync(words);

        _passwordServiceMock = new Mock<IPasswordService>();
        _passwordServiceMock
            .Setup(s => s.GeneratePassword(It.IsAny<PasswordSpec>(), It.IsAny<IReadOnlyList<string>>()))
            .Returns(password);

        _wordlistServiceMock = new Mock<IWordlistService>();
        _wordlistServiceMock
            .Setup(s => s.SupportedLanguages)
            .Returns(new Dictionary<string, string> { ["nl"] = "Nederlands", ["en"] = "English" });

        Services.AddSingleton(_wordServiceMock.Object);
        Services.AddSingleton(_passwordServiceMock.Object);
        Services.AddSingleton(_wordlistServiceMock.Object);
    }

    private IRenderedComponent<PassphraseForm> RenderForm()
    {
        SetupDefaultMocks();
        return RenderComponent<PassphraseForm>();
    }

    // -------- Render / structure --------

    [Fact]
    public void PassphraseForm_RendersWithoutException()
    {
        SetupDefaultMocks();
        var exception = Record.Exception(() => RenderComponent<PassphraseForm>());
        Assert.Null(exception);
    }

    [Fact]
    public void PassphraseForm_LanguageSelector_IsPresent()
    {
        var cut = RenderForm();
        var select = cut.Find("select#languageSelect");
        Assert.NotNull(select);
    }

    [Fact]
    public void PassphraseForm_LanguageSelector_HasNlAndEnOptions()
    {
        var cut = RenderForm();
        var options = cut.FindAll("select#languageSelect option");
        var values = options.Select(o => o.GetAttribute("value")?.ToLower()).ToList();
        Assert.Contains("nl", values);
        Assert.Contains("en", values);
    }

    [Fact]
    public void PassphraseForm_DecrementButton_IsPresent()
    {
        var cut = RenderForm();
        var decrementBtn = cut.Find(".stepper-btn[aria-label='Minder woorden']");
        Assert.NotNull(decrementBtn);
    }

    [Fact]
    public void PassphraseForm_IncrementButton_IsPresent()
    {
        var cut = RenderForm();
        var incrementBtn = cut.Find(".stepper-btn[aria-label='Meer woorden']");
        Assert.NotNull(incrementBtn);
    }

    [Fact]
    public void PassphraseForm_GenerateButton_IsPresent()
    {
        var cut = RenderForm();
        var btn = cut.Find("button.btn-primary");
        Assert.NotNull(btn);
    }

    [Fact]
    public void PassphraseForm_SentenceField_IsPresent()
    {
        var cut = RenderForm();
        var input = cut.Find("input[aria-label='Gegenereerde zinwoorden']");
        Assert.NotNull(input);
    }

    [Fact]
    public void PassphraseForm_PasswordField_IsPresent()
    {
        var cut = RenderForm();
        var input = cut.Find("input[aria-label='Gegenereerd wachtwoord']");
        Assert.NotNull(input);
    }

    [Fact]
    public void PassphraseForm_CheckboxesPresent()
    {
        var cut = RenderForm();
        var checkboxes = cut.FindAll("input[type='checkbox']");
        Assert.Equal(4, checkboxes.Count); // Space, Capital, Numbers, Special
    }

    // -------- Word count stepper --------

    [Fact]
    public void PassphraseForm_DecrementDisabled_AtMinimum()
    {
        SetupDefaultMocks();
        var cut = RenderComponent<PassphraseForm>();

        // Click decrement to reach minimum (start is 5, min is 3)
        var decBtn = cut.Find(".stepper-btn[aria-label='Minder woorden']");
        cut.Find(".stepper-btn[aria-label='Minder woorden']").Click();
        cut.Find(".stepper-btn[aria-label='Minder woorden']").Click(); // should now be at 3

        // Now at 3, decrement should be disabled
        decBtn = cut.Find(".stepper-btn[aria-label='Minder woorden']");
        Assert.True(decBtn.HasAttribute("disabled"), "Decrement button should be disabled at minimum word count");
    }

    [Fact]
    public void PassphraseForm_IncrementDisabled_AtMaximum()
    {
        SetupDefaultMocks();
        var cut = RenderComponent<PassphraseForm>();

        // Click increment to reach maximum (start is 5, max is 10, need 5 clicks)
        for (int i = 0; i < 5; i++)
            cut.Find(".stepper-btn[aria-label='Meer woorden']").Click();

        var incBtn = cut.Find(".stepper-btn[aria-label='Meer woorden']");
        Assert.True(incBtn.HasAttribute("disabled"), "Increment button should be disabled at maximum word count");
    }

    [Fact]
    public void PassphraseForm_DecrementEnabled_AboveMinimum()
    {
        var cut = RenderForm();
        var decBtn = cut.Find(".stepper-btn[aria-label='Minder woorden']");
        Assert.False(decBtn.HasAttribute("disabled"), "Decrement button should be enabled above minimum");
    }

    [Fact]
    public void PassphraseForm_IncrementEnabled_BelowMaximum()
    {
        var cut = RenderForm();
        var incBtn = cut.Find(".stepper-btn[aria-label='Meer woorden']");
        Assert.False(incBtn.HasAttribute("disabled"), "Increment button should be enabled below maximum");
    }

    // -------- Generate --------

    [Fact]
    public async Task PassphraseForm_OnInitialized_SentenceFieldPopulated()
    {
        SetupDefaultMocks();
        var cut = RenderComponent<PassphraseForm>();
        await cut.InvokeAsync(() => Task.CompletedTask); // flush async work

        var sentenceInput = cut.Find("input[aria-label='Gegenereerde zinwoorden']");
        Assert.NotEmpty(sentenceInput.GetAttribute("value") ?? "");
    }

    [Fact]
    public void PassphraseForm_GenerateButton_Click_InvokesWordService()
    {
        SetupDefaultMocks();
        var cut = RenderComponent<PassphraseForm>();
        cut.Find("button.btn-primary").Click();

        _wordServiceMock.Verify(s => s.GetSentenceAsync(It.IsAny<SentenceSpec>()),
            Times.AtLeastOnce, "WordService should be called when Generate is clicked");
    }

    [Fact]
    public void PassphraseForm_GenerateButton_Click_InvokesPasswordService()
    {
        SetupDefaultMocks();
        var cut = RenderComponent<PassphraseForm>();
        cut.Find("button.btn-primary").Click();

        _passwordServiceMock.Verify(s => s.GeneratePassword(
            It.IsAny<PasswordSpec>(), It.IsAny<IReadOnlyList<string>>()),
            Times.AtLeastOnce, "PasswordService.GeneratePassword should be called");
    }

    // -------- Language selector triggers regeneration --------

    [Fact]
    public void PassphraseForm_LanguageChange_TriggersNewGeneration()
    {
        SetupDefaultMocks();
        var cut = RenderComponent<PassphraseForm>();

        // Change language selector to "en"
        cut.Find("select#languageSelect").Change("en");

        // After language change, OnParametersSetAsync → RegenerateSentence
        _wordServiceMock.Verify(s => s.GetSentenceAsync(It.IsAny<SentenceSpec>()),
            Times.AtLeastOnce);
    }

    // -------- Copy button JS interop --------

    [Fact]
    public async Task PassphraseForm_CopyButton_InvokesJsInterop()
    {
        SetupDefaultMocks();

        // Mock JS runtime — must be registered before rendering
        var jsMock = new Mock<IJSRuntime>();
        jsMock.Setup(j => j.InvokeAsync<bool>(
                "clipboardInterop.copyText",
                It.IsAny<object[]>()))
            .ReturnsAsync(true);
        Services.AddSingleton(jsMock.Object);

        var cut = RenderComponent<PassphraseForm>();
        await cut.InvokeAsync(() => Task.CompletedTask); // wait for OnInitializedAsync

        // Click Generate first so the password field is populated
        cut.Find("button.btn-primary").Click();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Now the copy button should be enabled — click it
        cut.Find("button[aria-label*='Kopieer']").Click();
        await cut.InvokeAsync(() => Task.CompletedTask);

        jsMock.Verify(j => j.InvokeAsync<bool>(
            "clipboardInterop.copyText",
            It.IsAny<object[]>()), Times.Once);
    }

    // -------- Error display --------

    [Fact]
    public async Task PassphraseForm_WhenWordServiceThrows_ErrorMessageDisplayed()
    {
        _wordServiceMock = new Mock<IWordService>();
        _wordServiceMock
            .Setup(s => s.GetSentenceAsync(It.IsAny<SentenceSpec>()))
            .ThrowsAsync(new HttpRequestException("Simulated failure"));
        _passwordServiceMock = new Mock<IPasswordService>();
        _wordlistServiceMock = new Mock<IWordlistService>();
        _wordlistServiceMock
            .Setup(s => s.SupportedLanguages)
            .Returns(new Dictionary<string, string> { ["nl"] = "Nederlands" });

        Services.AddSingleton(_wordServiceMock.Object);
        Services.AddSingleton(_passwordServiceMock.Object);
        Services.AddSingleton(_wordlistServiceMock.Object);

        var cut = RenderComponent<PassphraseForm>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        var alert = cut.Find(".alert-warning");
        Assert.NotNull(alert);
    }
}
