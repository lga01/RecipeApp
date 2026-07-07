
namespace MisRecetas.Services;
public interface IAuthTokenService
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task<GitHubConfig?> GetConfigAsync();
    Task SetConfigAsync(GitHubConfig config);
    Task ClearAsync();
}
public class GitHubConfig
{
    public string Owner { get; set; } = "";
    public string Repo { get; set; } = "";
    public string FilePath { get; set; } = "data/recipes.json";
    public string Branch { get; set; } = "main";
}