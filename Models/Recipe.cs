using MisRecetas.Models;

public class Recipe
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string? SourceUrl { get; set; }
    public List<string> CategoryIds { get; set; } = new();
    public List<RecipeIngredient> Ingredients { get; set; } = new(); // antes: IngredientIds
    public string Steps { get; set; } = "";
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}