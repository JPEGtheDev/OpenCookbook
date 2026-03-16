namespace OpenCookbook.Domain.Entities;

public class Ingredient
{
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? VolumeAlt { get; set; }
    public string? Note { get; set; }
    public Guid? NutritionId { get; set; }
    public string? DocLink { get; set; }
}
