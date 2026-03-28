using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Interfaces;

public interface IRecipeRepository
{
    Task<IReadOnlyList<RecipeIndex>> GetRecipeIndexAsync();
    Task<Recipe> GetRecipeAsync(string path);

    /// <summary>
    /// Fetches and parses a recipe from an absolute HTTPS URL.
    /// </summary>
    /// <param name="absoluteUrl">A fully-qualified HTTPS URL pointing to a <c>.yaml</c> recipe file.</param>
    /// <exception cref="ArgumentException">Thrown when the URL is not valid (not HTTPS or missing .yaml extension).</exception>
    Task<Recipe> GetRecipeFromUrlAsync(string absoluteUrl);
}
