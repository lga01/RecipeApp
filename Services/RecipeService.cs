using System.Text.Json;
using MisRecetas.Models;
using MisRecetas.Services;
using MisRecetas.Utils;
public class RecipeService
{
    private readonly IGitHubStorageService _storage;
    private RecipeDatabase _db = new();
    private string? _currentSha;
    private bool _loaded = false;

    public RecipeService(IGitHubStorageService storage)
    {
        _storage = storage;
    }

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;

        var result = await _storage.GetFileAsync();
        if (result is null)
        {
            _db = new RecipeDatabase();
            _currentSha = null;
        }
        else
        {
            _db = JsonSerializer.Deserialize<RecipeDatabase>(result.Value.content) ?? new RecipeDatabase();
            _currentSha = result.Value.sha;
        }

        _loaded = true;
    }

    public List<Recipe> GetAll() => _db.Recipes;
    public List<Category> GetAllCategories() => _db.Categories;
    public List<Ingredient> GetAllIngredients() => _db.Ingredients;

    public string GetOrCreateCategoryId(string rawName)
    {
        var normalized = TextNormalizer.Normalize(rawName);
        var existing = _db.Categories.FirstOrDefault(c => c.NormalizedName == normalized);
        if (existing != null) return existing.Id;

        var newCategory = new Category { Name = rawName.Trim(), NormalizedName = normalized };
        _db.Categories.Add(newCategory);
        return newCategory.Id;
    }

    public string GetOrCreateIngredientId(string rawName)
    {
        var normalized = TextNormalizer.Normalize(rawName);
        var existing = _db.Ingredients.FirstOrDefault(i => i.NormalizedName == normalized);
        if (existing != null) return existing.Id;

        var newIngredient = new Ingredient { Name = rawName.Trim(), NormalizedName = normalized };
        _db.Ingredients.Add(newIngredient);
        return newIngredient.Id;
    }

    public async Task AddOrUpdateRecipeAsync(Recipe recipe)
    {
        var existing = _db.Recipes.FirstOrDefault(r => r.Id == recipe.Id);
        if (existing != null)
            _db.Recipes.Remove(existing);

        _db.Recipes.Add(recipe);

        await PersistAsync($"Guardar receta: {recipe.Title}");
    }

    public async Task DeleteRecipeAsync(string recipeId)
    {
        _db.Recipes.RemoveAll(r => r.Id == recipeId);
        await PersistAsync($"Eliminar receta {recipeId}");
    }

    private async Task PersistAsync(string commitMessage)
    {
        // Releemos el sha más reciente justo antes de guardar, por si ha cambiado
        // desde la última vez (edición manual, u otro dispositivo guardando antes)
        var latest = await _storage.GetFileAsync();
        _currentSha = latest?.sha;

        var json = JsonSerializer.Serialize(_db, new JsonSerializerOptions { WriteIndented = true });
        await _storage.SaveFileAsync(json, _currentSha, commitMessage);

        // Volvemos a leer para quedarnos con el sha posterior al guardado
        var result = await _storage.GetFileAsync();
        _currentSha = result?.sha;
    }
}