using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

/// <summary>
/// Calculates recipe nutrition by matching ingredients against the nutrition database.
/// Entries are cached after the first load to avoid redundant fetches within the same scope.
/// Registered as scoped — not thread-safe.
/// </summary>
public class NutritionCalculator
{
    private readonly INutritionRepository _nutritionRepository;
    private IReadOnlyList<NutritionEntry>? _cachedEntries;
    private Dictionary<string, NutritionEntry>? _cachedLookup;
    private Dictionary<Guid, NutritionEntry>? _cachedIdLookup;

    public NutritionCalculator(INutritionRepository nutritionRepository)
    {
        _nutritionRepository = nutritionRepository;
    }

    public async Task<RecipeNutrition> CalculateAsync(Recipe recipe, int servings = 1)
    {
        _cachedEntries ??= await _nutritionRepository.GetAllEntriesAsync();
        _cachedLookup ??= BuildLookup(_cachedEntries);
        _cachedIdLookup ??= BuildIdLookup(_cachedEntries);

        var result = new RecipeNutrition { Servings = servings };
        var totalCalories = 0.0;
        var totalProtein = 0.0;
        var totalFat = 0.0;
        var totalCarbs = 0.0;

        foreach (var group in recipe.Ingredients)
        {
            foreach (var ingredient in group.Items)
            {
                if (!IsGramUnit(ingredient.Unit))
                {
                    result.MissingIngredients.Add(ingredient.Name);
                    result.Ingredients.Add(new IngredientNutrition
                    {
                        IngredientName = ingredient.Name,
                        QuantityG = 0,
                        IsMatch = false
                    });
                    continue;
                }

                var entry = ingredient.NutritionId.HasValue
                    ? FindEntryById(_cachedIdLookup, ingredient.NutritionId.Value)
                        ?? FindEntry(_cachedLookup, ingredient.Name)
                    : FindEntry(_cachedLookup, ingredient.Name);
                if (entry is null)
                {
                    result.MissingIngredients.Add(ingredient.Name);
                    result.Ingredients.Add(new IngredientNutrition
                    {
                        IngredientName = ingredient.Name,
                        QuantityG = ingredient.Quantity,
                        IsMatch = false
                    });
                    continue;
                }

                var factor = ingredient.Quantity / 100.0;
                var nutrients = new NutrientInfo
                {
                    CaloriesKcal = Math.Round(entry.Per100g.CaloriesKcal * factor, 1),
                    ProteinG = Math.Round(entry.Per100g.ProteinG * factor, 1),
                    FatG = Math.Round(entry.Per100g.FatG * factor, 1),
                    CarbsG = Math.Round(entry.Per100g.CarbsG * factor, 1)
                };

                totalCalories += nutrients.CaloriesKcal;
                totalProtein += nutrients.ProteinG;
                totalFat += nutrients.FatG;
                totalCarbs += nutrients.CarbsG;

                result.Ingredients.Add(new IngredientNutrition
                {
                    IngredientName = ingredient.Name,
                    QuantityG = ingredient.Quantity,
                    IsMatch = true,
                    Nutrients = nutrients
                });
            }
        }

        result.TotalNutrients = new NutrientInfo
        {
            CaloriesKcal = Math.Round(totalCalories, 1),
            ProteinG = Math.Round(totalProtein, 1),
            FatG = Math.Round(totalFat, 1),
            CarbsG = Math.Round(totalCarbs, 1)
        };

        if (servings > 0)
        {
            result.PerServingNutrients = new NutrientInfo
            {
                CaloriesKcal = Math.Round(totalCalories / servings, 1),
                ProteinG = Math.Round(totalProtein / servings, 1),
                FatG = Math.Round(totalFat / servings, 1),
                CarbsG = Math.Round(totalCarbs / servings, 1)
            };
        }

        return result;
    }

    private static bool IsGramUnit(string unit)
    {
        return unit.Equals("g", StringComparison.OrdinalIgnoreCase)
            || unit.Equals("ml", StringComparison.OrdinalIgnoreCase);
    }

    internal static Dictionary<string, NutritionEntry> BuildLookup(IReadOnlyList<NutritionEntry> entries)
    {
        var lookup = new Dictionary<string, NutritionEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            lookup.TryAdd(NormalizeKey(entry.Name), entry);

            foreach (var alias in entry.Aliases)
            {
                lookup.TryAdd(NormalizeKey(alias), entry);
            }
        }

        return lookup;
    }

    internal static Dictionary<Guid, NutritionEntry> BuildIdLookup(IReadOnlyList<NutritionEntry> entries)
    {
        var lookup = new Dictionary<Guid, NutritionEntry>();

        foreach (var entry in entries)
        {
            lookup.TryAdd(entry.Id, entry);
        }

        return lookup;
    }

    internal static NutritionEntry? FindEntryById(
        Dictionary<Guid, NutritionEntry> idLookup, Guid nutritionId)
    {
        return idLookup.TryGetValue(nutritionId, out var entry) ? entry : null;
    }

    internal static NutritionEntry? FindEntry(
        Dictionary<string, NutritionEntry> lookup, string ingredientName)
    {
        var normalized = NormalizeKey(ingredientName);

        if (lookup.TryGetValue(normalized, out var exact))
            return exact;

        // Try stripping parenthetical qualifiers, e.g. "Fine Sea Salt (for the water)" → "Fine Sea Salt"
        var parenIndex = normalized.IndexOf('(');
        if (parenIndex > 0)
        {
            var withoutParen = normalized[..parenIndex].Trim();
            if (lookup.TryGetValue(withoutParen, out var stripped))
                return stripped;
        }

        return null;
    }

    private static string NormalizeKey(string name)
    {
        return name.Trim().ToLowerInvariant();
    }
}
