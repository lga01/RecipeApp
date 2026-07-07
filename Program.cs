using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MisRecetas;
using MisRecetas.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IAuthTokenService, AuthTokenService>();
builder.Services.AddScoped<IGitHubStorageService, GitHubStorageService>();
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<ImageService>();
await builder.Build().RunAsync();