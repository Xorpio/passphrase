using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PassphraseGenerator;
using PassphraseGenerator.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddLogging();

// Register services
builder.Services.AddScoped<IRandomService, LocalRandomService>();
builder.Services.AddScoped<IWordlistService, WordlistService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

await builder.Build().RunAsync();
