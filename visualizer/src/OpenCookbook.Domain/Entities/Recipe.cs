namespace OpenCookbook.Domain.Entities;

public class Recipe
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;
    public List<IngredientGroup> Ingredients { get; set; } = [];
    public List<UtensilGroup>? Utensils { get; set; }
    public List<Section> Instructions { get; set; } = [];
    public List<RelatedRecipe>? Related { get; set; }
    public List<string>? Notes { get; set; }
    public RecipeYield? Yields { get; set; }
    public RecipeServingSize? ServingSize { get; set; }
}
