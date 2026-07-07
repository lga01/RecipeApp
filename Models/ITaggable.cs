namespace MisRecetas.Models;

public interface ITaggable
{
    string Id { get; set; }
    string Name { get; set; }
    string NormalizedName { get; set; }
}