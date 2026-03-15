namespace OpenCookbook.Domain.Entities;

public class RecipeNutrition
{
    public NutrientInfo TotalNutrients { get; set; } = new();
    public NutrientInfo? PerServingNutrients { get; set; }
    public int Servings { get; set; }
    public List<IngredientNutrition> Ingredients { get; set; } = [];
    public List<string> MissingIngredients { get; set; } = [];
    public bool IsComplete => MissingIngredients.Count == 0;
}
