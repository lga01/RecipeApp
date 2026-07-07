namespace MisRecetas.Models;
public class Category : ITaggable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";           // "Postres" (como se muestra)
    public string NormalizedName { get; set; } = ""; // "postres" (para comparar sin duplicados)
}