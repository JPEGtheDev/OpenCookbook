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
        return _repository.GetRecipeAsync(path);
    }
}
