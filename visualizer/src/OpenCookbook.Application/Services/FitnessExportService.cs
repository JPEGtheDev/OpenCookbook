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
        var servings = recipe.ServingSize?.Quantity ?? recipe.Yields?.Quantity ?? 1;
        sb.AppendLine($"Servings: {servings}");
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
                        var resolvedPath = ResolveSubRecipePath(basePath, item.DocLink);
                        var subRecipe = await _recipeRepository.GetRecipeAsync(resolvedPath);
                        var subBasePath = GetDirectoryFromPath(resolvedPath);
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

    private static string ResolveSubRecipePath(string? basePath, string docLink)
    {
        var linkPath = docLink.StartsWith("./", StringComparison.Ordinal) ? docLink[2..] : docLink;
        var combined = string.IsNullOrEmpty(basePath) ? linkPath : $"{basePath}/{linkPath}";

        var parts = combined.Split('/');
        var normalized = new List<string>();
        foreach (var part in parts)
        {
            if (part == "..")
            {
                // Extra ".." beyond the root are silently dropped — traversal above root is not possible
                if (normalized.Count > 0)
                    normalized.RemoveAt(normalized.Count - 1);
            }
            else if (part != "." && part.Length > 0)
            {
                normalized.Add(part);
            }
        }
        return string.Join("/", normalized);
    }

    private static string? GetDirectoryFromPath(string path)
    {
        var lastSlash = path.LastIndexOf('/');
        return lastSlash > 0 ? path[..lastSlash] : null;
    }
}
