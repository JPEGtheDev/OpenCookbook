namespace OpenCookbook.Domain.Entities;

public class IngredientNutrition
{
    public string IngredientName { get; set; } = string.Empty;
    public double QuantityG { get; set; }
    public bool IsMatch { get; set; }
    public NutrientInfo? Nutrients { get; set; }
}
