namespace OpenCookbook.Domain.Entities;

public class IngredientGroup
{
    public string? Heading { get; set; }
    public List<Ingredient> Items { get; set; } = [];
}
