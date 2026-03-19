using System.Globalization;
using System.Text;
using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

public class FitnessExportService
{
    private readonly IRecipeRepository? _recipeRepository;

    public FitnessExportService(IRecipeRepository? recipeRepository = null)
    {
        _recipeRepository = recipeRepository;
    }

    /// <summary>
    /// Generates a flat plain-text ingredient list suitable for pasting into a
    /// fitness tracking app's recipe importer. Ingredients linked to sub-recipes
    /// via <see cref="Ingredient.DocLink"/> are expanded inline and scaled by the
    /// parent ingredient's quantity.
    /// </summary>
    public async Task<string> GenerateExportAsync(Recipe recipe, string? basePath = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Name: {recipe.Name}");

        if (recipe.Yields is { Quantity: > 0 })
        {
            sb.AppendLine($"Yield: {recipe.Yields.Quantity} {PluralizeUnit(recipe.Yields.Unit, recipe.Yields.Quantity)}");
            if (recipe.ServingSize is { Quantity: > 0 })
                sb.AppendLine($"Serving Size: {recipe.ServingSize.Quantity} {PluralizeUnit(recipe.ServingSize.Unit, recipe.ServingSize.Quantity)}");
        }
        else if (recipe.ServingSize is { Quantity: > 0 })
        {
            sb.AppendLine($"Serving Size: {recipe.ServingSize.Quantity} {PluralizeUnit(recipe.ServingSize.Unit, recipe.ServingSize.Quantity)}");
        }
        else
        {
            sb.AppendLine("Servings: 1");
        }

        sb.AppendLine();
        sb.AppendLine("Ingredients:");

        await AppendIngredients(sb, recipe, basePath, scale: 1.0);

        return sb.ToString().TrimEnd();
    }

    private async Task AppendIngredients(StringBuilder sb, Recipe recipe, string? basePath, double scale)
    {
        foreach (var group in recipe.Ingredients)
        {
            if (group.Heading is not null)
                sb.AppendLine($"# {group.Heading}");

            foreach (var item in group.Items)
            {
                if (item.DocLink is not null && _recipeRepository is not null)
                {
                    try
                    {
                        var resolvedPath = DocLinkResolver.ResolvePath(basePath, item.DocLink);
                        var subRecipe = await _recipeRepository.GetRecipeAsync(resolvedPath);
                        var subBasePath = DocLinkResolver.GetDirectory(resolvedPath);
                        await AppendIngredients(sb, subRecipe, subBasePath, scale * item.Quantity);
                    }
                    catch (Exception ex) when (ex is ArgumentException
                                                   or KeyNotFoundException
                                                   or HttpRequestException
                                                   or InvalidOperationException)
                    {
                        // If sub-recipe resolution fails, fall back to listing the reference as-is
                        sb.AppendLine($"{FormatQuantity(item.Quantity * scale)} {item.Unit} {item.Name}");
                    }
                }
                else
                {
                    sb.AppendLine($"{FormatQuantity(item.Quantity * scale)} {item.Unit} {item.Name}");
                }
            }
        }
    }

    private static string FormatQuantity(double qty)
        => qty == Math.Floor(qty)
            ? qty.ToString("F0", CultureInfo.InvariantCulture)
            : qty.ToString("0.###", CultureInfo.InvariantCulture);

    private static readonly HashSet<string> NonPluralizableUnits =
        new(StringComparer.OrdinalIgnoreCase) { "g", "ml", "kg", "l", "oz", "lb", "lbs" };

    private static string PluralizeUnit(string? unit, int quantity)
    {
        if (string.IsNullOrEmpty(unit)) return string.Empty;
        if (quantity == 1) return unit;
        if (NonPluralizableUnits.Contains(unit)) return unit;
        return unit + "s";
    }

    private static string ResolveSubRecipePath(string? basePath, string docLink)
        => DocLinkResolver.ResolvePath(basePath, docLink);

    private static string? GetDirectoryFromPath(string path)
        => DocLinkResolver.GetDirectory(path);
}
