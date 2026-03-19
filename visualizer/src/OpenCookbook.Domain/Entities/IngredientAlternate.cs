namespace OpenCookbook.Domain.Entities;

public class IngredientAlternate
{
    public string Name { get; set; } = string.Empty;
    public Guid? NutritionId { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? VolumeAlt { get; set; }
    public string? WeightAlt { get; set; }
    public string? Note { get; set; }
}
