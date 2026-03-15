using System.Text.Json.Serialization;

namespace OpenCookbook.Domain.Entities;

public class NutritionEntry
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = [];

    [JsonPropertyName("per_100g")]
    public NutrientInfo Per100g { get; set; } = new();

    [JsonPropertyName("fdc_id")]
    public int? FdcId { get; set; }
}
