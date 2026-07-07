namespace MisRecetas.Models;

public class RecipeIngredient
{
    public string IngredientId { get; set; } = "";
    public string? Quantity { get; set; } // texto libre: "200g", "2 cucharadas", "1", etc.
}