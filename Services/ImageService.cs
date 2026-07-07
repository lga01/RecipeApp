using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace MisRecetas.Services;

public class ImageService
{
    private readonly IJSRuntime _js;
    private readonly IGitHubStorageService _storage;

    public ImageService(IJSRuntime js, IGitHubStorageService storage)
    {
        _js = js;
        _storage = storage;
    }

    /// Sube una foto elegida por el usuario, comprimida, y devuelve la URL pública.
    public async Task<string> UploadRecipeImageAsync(IBrowserFile file, string recipeId)
    {
        using var ms = new MemoryStream();
        await file.OpenReadStream(maxAllowedSize: 15_000_000).CopyToAsync(ms); // hasta 15MB de entrada
        var originalBase64 = Convert.ToBase64String(ms.ToArray());

        var compressedBase64 = await _js.InvokeAsync<string>(
            "imageInterop.resizeAndCompress", originalBase64, 1200, 0.75);

        var bytes = Convert.FromBase64String(compressedBase64);
        var path = $"images/{recipeId}.jpg";

        await _storage.UploadBinaryFileAsync(path, bytes, $"Subir imagen receta {recipeId}");
        return await _storage.GetRawUrlAsync(path);
    }

    public async Task<string?> TryGetYouTubeThumbnailAsync(string videoUrl)
        => await _js.InvokeAsync<string?>("imageInterop.getYouTubeThumbnail", videoUrl);

    public async Task<string?> TryGetTikTokThumbnailAsync(string videoUrl)
        => await _js.InvokeAsync<string?>("imageInterop.getTikTokThumbnail", videoUrl);
}