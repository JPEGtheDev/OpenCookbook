namespace OpenCookbook.Application.Models;

public class RecipeFilter
{
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>All selected ingredients must be present (AND behavior).</summary>
    public List<string> SelectedIngredients { get; set; } = [];

    /// <summary>Any selected tag must be present (OR behavior).</summary>
    public List<string> SelectedTags { get; set; } = [];

    public bool FilterDairyFree { get; set; }
    public bool FilterVegetarian { get; set; }
    public bool FilterVegan { get; set; }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(SearchQuery) &&
        SelectedIngredients.Count == 0 &&
        SelectedTags.Count == 0 &&
        !FilterDairyFree &&
        !FilterVegetarian &&
        !FilterVegan;
}
