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

                return tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                       ingredients.Any(i => i.Contains(term, StringComparison.OrdinalIgnoreCase));
            })
            .ToList();
    }

    public Task<Recipe> GetRecipeByPathAsync(string path)
    {
        return _repository.GetRecipeAsync(path);
    }
}
