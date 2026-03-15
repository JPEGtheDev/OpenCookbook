namespace OpenCookbook.Domain.Entities;

public class NutritionEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = [];
    public NutrientInfo Per100g { get; set; } = new();
}
