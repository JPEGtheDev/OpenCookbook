using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Interfaces;

public interface IRecipeRepository
{
    Task<IReadOnlyList<RecipeIndex>> GetRecipeIndexAsync();
    Task<Recipe> GetRecipeAsync(string path);
}
