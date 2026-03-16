using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

internal sealed class FakeRecipeRepository : IRecipeRepository
{
    private readonly Dictionary<string, Recipe> _recipes;

    public FakeRecipeRepository(Dictionary<string, Recipe> recipes)
    {
        _recipes = recipes;
    }

    public Task<IReadOnlyList<RecipeIndex>> GetRecipeIndexAsync()
    {
        return Task.FromResult<IReadOnlyList<RecipeIndex>>([]);
    }

    public Task<Recipe> GetRecipeAsync(string path)
    {
        if (_recipes.TryGetValue(path, out var recipe))
            return Task.FromResult(recipe);

        throw new KeyNotFoundException($"Recipe not found: {path}");
    }
}
