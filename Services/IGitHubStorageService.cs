namespace MisRecetas.Services;

public interface IGitHubStorageService
{
    Task<(string content, string sha)?> GetFileAsync();
    Task SaveFileAsync(string content, string? sha, string commitMessage);

    // Nuevos, para imágenes (o cualquier archivo binario en el futuro)
    Task UploadBinaryFileAsync(string path, byte[] content, string commitMessage);
    Task<string> GetRawUrlAsync(string path);
}