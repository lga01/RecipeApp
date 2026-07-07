using Microsoft.JSInterop;
using MisRecetas.Services;
using System.Text.Json;

public class AuthTokenService : IAuthTokenService
{
    private const string TokenKey = "gh_token";
    private const string ConfigKey = "gh_config";

    private readonly IJSRuntime _js;

    public AuthTokenService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<string?> GetTokenAsync()
        => await _js.InvokeAsync<string?>("localStorageInterop.getItem", TokenKey);

    public async Task SetTokenAsync(string token)
        => await _js.InvokeVoidAsync("localStorageInterop.setItem", TokenKey, token);

    public async Task<GitHubConfig?> GetConfigAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorageInterop.getItem", ConfigKey);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<GitHubConfig>(json);
    }

    public async Task SetConfigAsync(GitHubConfig config)
    {
        var json = JsonSerializer.Serialize(config);
        await _js.InvokeVoidAsync("localStorageInterop.setItem", ConfigKey, json);
    }

    public async Task ClearAsync()
    {
        await _js.InvokeVoidAsync("localStorageInterop.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorageInterop.removeItem", ConfigKey);
    }
}

