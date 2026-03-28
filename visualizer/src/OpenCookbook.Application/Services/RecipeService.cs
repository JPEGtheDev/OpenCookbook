using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

public class RecipeService
{
    private readonly IRecipeRepository _repository;
    private IReadOnlyList<RecipeIndex>? _indexCache;

    public RecipeService(IRecipeRepository repository)
    {
        _repository = repository;
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
        if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            return _repository.GetRecipeFromUrlAsync(path);

        return _repository.GetRecipeAsync(path);
    }

    /// <summary>
    /// Tries to extract a local recipe path from a full app URL.
    /// Returns <see langword="true"/> when <paramref name="input"/> is a recipe URL
    /// rooted at <paramref name="baseUrl"/> (e.g.
    /// <c>https://jpegthedev.github.io/OpenCookbook/recipe/Chicken.yaml</c>).
    /// </summary>
    public static bool TryExtractLocalPath(string input, string baseUrl, out string localPath)
    {
        localPath = string.Empty;

        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(baseUrl))
            return false;

        var recipeBase = baseUrl.TrimEnd('/') + "/recipe/";
        if (!input.StartsWith(recipeBase, StringComparison.OrdinalIgnoreCase))
            return false;

        localPath = Uri.UnescapeDataString(input[recipeBase.Length..]);
        return !string.IsNullOrEmpty(localPath);
    }
}
