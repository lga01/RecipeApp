namespace MisRecetas.Models;
public class Ingredient : ITaggable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";
}