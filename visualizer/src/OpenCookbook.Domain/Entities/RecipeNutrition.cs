namespace OpenCookbook.Domain.Entities;

public class RecipeNutrition
{
    public NutrientInfo TotalNutrients { get; set; } = new();
    public NutrientInfo? PerUnitNutrients { get; set; }
    public NutrientInfo? PerServingNutrients { get; set; }
    public int Servings { get; set; }
    public int? YieldsQuantity { get; set; }
    public string? YieldsUnit { get; set; }
    public int? ServingSizeQuantity { get; set; }
    public string? ServingSizeUnit { get; set; }
    public List<IngredientNutrition> Ingredients { get; set; } = [];
    public List<string> MissingIngredients { get; set; } = [];
    public bool IsComplete => MissingIngredients.Count == 0;
}
