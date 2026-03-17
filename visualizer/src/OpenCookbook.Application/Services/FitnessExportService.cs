using System.Globalization;
using System.Text;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

public class FitnessExportService
{
    /// <summary>
    /// Generates a plain-text ingredient export in "quantity unit name" order,
    /// compatible with fitness tracking apps that list quantity before ingredient name.
    /// </summary>
    public string GenerateQtyFirstExport(Recipe recipe)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Name: {recipe.Name}");
        sb.AppendLine();
        sb.AppendLine("Ingredients:");

        foreach (var group in recipe.Ingredients)
        {
            if (group.Heading is not null)
                sb.AppendLine($"# {group.Heading}");

            foreach (var item in group.Items)
                sb.AppendLine($"{FormatQuantity(item.Quantity)} {item.Unit} {item.Name}");
        }

        sb.AppendLine();
        var servings = recipe.ServingSize?.Quantity ?? recipe.Yields?.Quantity ?? 1;
        sb.Append($"Number of Servings: {servings}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a plain-text ingredient export in "name, quantity unit" order,
    /// compatible with fitness tracking apps that list the ingredient name first.
    /// </summary>
    public string GenerateNameFirstExport(Recipe recipe)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Recipe: {recipe.Name}");
        var servings = recipe.ServingSize?.Quantity ?? recipe.Yields?.Quantity ?? 1;
        sb.AppendLine($"Servings: {servings}");
        sb.AppendLine();
        sb.AppendLine("Ingredients:");

        foreach (var group in recipe.Ingredients)
        {
            if (group.Heading is not null)
                sb.AppendLine($"# {group.Heading}");

            foreach (var item in group.Items)
                sb.AppendLine($"{item.Name}, {FormatQuantity(item.Quantity)} {item.Unit}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatQuantity(double qty)
        => qty == Math.Floor(qty)
            ? qty.ToString("F0", CultureInfo.InvariantCulture)
            : qty.ToString("0.###", CultureInfo.InvariantCulture);
}
