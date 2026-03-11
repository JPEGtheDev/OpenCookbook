using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

public class RecipeService
{
    private readonly IRecipeRepository _repository;

    public RecipeService(IRecipeRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<RecipeIndex>> GetAllRecipesAsync()
    {
        return _repository.GetRecipeIndexAsync();
    }

    public Task<Recipe> GetRecipeByPathAsync(string path)
    {
        return _repository.GetRecipeAsync(path);
    }
}
