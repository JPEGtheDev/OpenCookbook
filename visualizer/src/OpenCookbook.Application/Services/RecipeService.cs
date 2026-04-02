using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

public class RecipeService
{
    private readonly IRecipeRepository _repository;
    private readonly DietaryInferenceService _dietaryInference;
    private IReadOnlyList<RecipeIndex>? _indexCache;

    public RecipeService(IRecipeRepository repository, DietaryInferenceService dietaryInference)
    {
        _repository = repository;
        _dietaryInference = dietaryInference;
    }

    public async Task<IReadOnlyList<RecipeIndex>> GetAllRecipesAsync()
    {
        _indexCache ??= await _repository.GetRecipeIndexAsync();
        return _indexCache;
    }

    public async Task<IReadOnlyList<RecipeIndex>> SearchRecipesAsync(string query)
    {
        var all = await GetAllRecipesAsync();

        if (string.IsNullOrWhiteSpace(query))
            return all;

        var term = query.Trim();
        return all
            .Where(r =>
            {
                var tags = r.Tags ?? [];
                var ingredients = r.Ingredients ?? [];

                return r.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                       tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                       ingredients.Any(i => i.Contains(term, StringComparison.OrdinalIgnoreCase));
            })
            .OrderBy(r => NameRelevanceScore(r.Name, term))
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Applies structured filters on top of the optional free-text search.
    /// </summary>
    public async Task<IReadOnlyList<RecipeIndex>> FilterRecipesAsync(RecipeFilter filter)
    {
        // Start from text search results (or all recipes when query is empty).
        var candidates = await SearchRecipesAsync(filter.SearchQuery);

        // ── ingredient filter (AND: every selected ingredient must be present) ──
        if (filter.SelectedIngredients.Count > 0)
        {
            candidates = candidates
                .Where(r =>
                {
                    var ingredients = r.Ingredients ?? [];
                    return filter.SelectedIngredients.All(sel =>
                        ingredients.Any(i => i.Equals(sel, StringComparison.OrdinalIgnoreCase)));
                })
                .ToList();
        }

        // ── tag filter (OR: at least one selected tag must be present) ──
        if (filter.SelectedTags.Count > 0)
        {
            candidates = candidates
                .Where(r =>
                {
                    var tags = r.Tags ?? [];
                    return filter.SelectedTags.Any(sel =>
                        tags.Any(t => t.Equals(sel, StringComparison.OrdinalIgnoreCase)));
                })
                .ToList();
        }

        // ── dietary filter (inference-based) ──
        bool anyDietary = filter.FilterDairyFree || filter.FilterVegetarian || filter.FilterVegan;
        if (anyDietary)
        {
            candidates = candidates
                .Where(r =>
                {
                    var profile = _dietaryInference.Infer(r.Ingredients ?? []);

                    if (filter.FilterDairyFree && profile.IsDairyFree != true)
                        return false;
                    if (filter.FilterVegetarian && profile.IsVegetarian != true)
                        return false;
                    if (filter.FilterVegan && profile.IsVegan != true)
                        return false;

                    return true;
                })
                .ToList();
        }

        return candidates;
    }

    /// <summary>Returns all unique tags from the recipe index, sorted alphabetically.</summary>
    public async Task<IReadOnlyList<string>> GetAvailableTagsAsync()
    {
        var all = await GetAllRecipesAsync();
        return all
            .SelectMany(r => r.Tags ?? [])
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Returns all unique ingredient names from the recipe index, sorted alphabetically.</summary>
    public async Task<IReadOnlyList<string>> GetAvailableIngredientsAsync()
    {
        var all = await GetAllRecipesAsync();
        return all
            .SelectMany(r => r.Ingredients ?? [])
            .Select(i => i.Trim())
            .Where(i => i.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(i => i, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int NameRelevanceScore(string name, string term)
    {
        if (name.Equals(term, StringComparison.OrdinalIgnoreCase))
            return 0;
        if (name.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            return 1;
        if (name.Contains(term, StringComparison.OrdinalIgnoreCase))
            return 2;
        return 3;
    }

    public Task<Recipe> GetRecipeByPathAsync(string path)
    {
        return _repository.GetRecipeAsync(path);
    }
}
