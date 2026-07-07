namespace MisRecetas.Models;
public class RecipeDatabase
{
    public List<Recipe> Recipes { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Ingredient> Ingredients { get; set; } = new();
}