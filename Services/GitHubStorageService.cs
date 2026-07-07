using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MisRecetas.Services;

public class GitHubStorageService : IGitHubStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthTokenService _authTokenService;

    public GitHubStorageService(HttpClient httpClient, IAuthTokenService authTokenService)
    {
        _httpClient = httpClient;
        _authTokenService = authTokenService;
    }

    private async Task PrepareRequestAsync()
    {
        var token = await _authTokenService.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("No hay token de GitHub configurado.");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MisRecetasApp");

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    public async Task<(string content, string sha)?> GetFileAsync()
    {
        await PrepareRequestAsync();
        var config = await _authTokenService.GetConfigAsync()
            ?? throw new InvalidOperationException("No hay configuración de repositorio.");

        // Añadimos un timestamp para evitar que el navegador devuelva una respuesta cacheada
        var cacheBuster = DateTimeOffset.UtcNow.Ticks;
        var url = $"https://api.github.com/repos/{config.Owner}/{config.Repo}/contents/{config.FilePath}?ref={config.Branch}&_={cacheBuster}";

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<GitHubContentResponse>();
        var decodedBytes = Convert.FromBase64String(json!.Content.Replace("\n", ""));
        var content = Encoding.UTF8.GetString(decodedBytes);

        return (content, json.Sha);
    }

    public async Task SaveFileAsync(string content, string? sha, string commitMessage)
    {
        await PrepareRequestAsync();
        var config = await _authTokenService.GetConfigAsync()
            ?? throw new InvalidOperationException("No hay configuración de repositorio.");

        var url = $"https://api.github.com/repos/{config.Owner}/{config.Repo}/contents/{config.FilePath}";

        var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

        var body = new GitHubUpdateRequest
        {
            Message = commitMessage,
            Content = base64Content,
            Sha = sha, // null si el archivo es nuevo
            Branch = config.Branch
        };

        var response = await _httpClient.PutAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
    }
    public async Task UploadBinaryFileAsync(string path, byte[] content, string commitMessage)
    {
        await PrepareRequestAsync();
        var config = await _authTokenService.GetConfigAsync()
            ?? throw new InvalidOperationException("No hay configuración de repositorio.");

        // Comprobamos si ya existe un archivo en esa ruta, para obtener su sha
        string? sha = null;
        var checkUrl = $"https://api.github.com/repos/{config.Owner}/{config.Repo}/contents/{path}?ref={config.Branch}";
        var checkResponse = await _httpClient.GetAsync(checkUrl);
        if (checkResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            checkResponse.EnsureSuccessStatusCode();
            var existing = await checkResponse.Content.ReadFromJsonAsync<GitHubContentResponse>();
            sha = existing?.Sha;
        }

        var url = $"https://api.github.com/repos/{config.Owner}/{config.Repo}/contents/{path}";
        var body = new GitHubUpdateRequest
        {
            Message = commitMessage,
            Content = Convert.ToBase64String(content),
            Sha = sha,
            Branch = config.Branch
        };

        var response = await _httpClient.PutAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetRawUrlAsync(string path)
    {
        var config = await _authTokenService.GetConfigAsync()
            ?? throw new InvalidOperationException("No hay configuración de repositorio.");

        return $"https://raw.githubusercontent.com/{config.Owner}/{config.Repo}/{config.Branch}/{path}";
    }
    private class GitHubContentResponse
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        [JsonPropertyName("sha")]
        public string Sha { get; set; } = "";
    }

    private class GitHubUpdateRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        [JsonPropertyName("sha")]
        public string? Sha { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; } = "main";
    }
}