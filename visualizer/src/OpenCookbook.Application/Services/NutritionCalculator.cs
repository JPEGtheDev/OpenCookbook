using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

/// <summary>
/// Calculates recipe nutrition by matching ingredients against the nutrition database.
/// Ingredients must have a <see cref="Ingredient.NutritionId"/> set to be matched.
/// Entries are cached after the first load to avoid redundant fetches within the same scope.
/// Registered as scoped — not thread-safe.
/// </summary>
public class NutritionCalculator
{
    private readonly INutritionRepository _nutritionRepository;
    private IReadOnlyList<NutritionEntry>? _cachedEntries;
    private Dictionary<Guid, NutritionEntry>? _cachedIdLookup;

    public NutritionCalculator(INutritionRepository nutritionRepository)
    {
        _nutritionRepository = nutritionRepository;
    }

    public async Task<RecipeNutrition> CalculateAsync(Recipe recipe, int servings = 1)
    {
        _cachedEntries ??= await _nutritionRepository.GetAllEntriesAsync();
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
                    : null;
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

        if (recipe.Yields is { Quantity: > 0 })
        {
            result.YieldsQuantity = recipe.Yields.Quantity;
            result.YieldsUnit = recipe.Yields.Unit;
            result.PerUnitNutrients = new NutrientInfo
            {
                CaloriesKcal = Math.Round(totalCalories / recipe.Yields.Quantity, 1),
                ProteinG = Math.Round(totalProtein / recipe.Yields.Quantity, 1),
                FatG = Math.Round(totalFat / recipe.Yields.Quantity, 1),
                CarbsG = Math.Round(totalCarbs / recipe.Yields.Quantity, 1)
            };

            if (recipe.ServingSize is { Quantity: > 0 })
            {
                result.ServingSizeQuantity = recipe.ServingSize.Quantity;
                result.ServingSizeUnit = recipe.ServingSize.Unit;
                result.PerServingNutrients = new NutrientInfo
                {
                    CaloriesKcal = Math.Round(totalCalories / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1),
                    ProteinG = Math.Round(totalProtein / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1),
                    FatG = Math.Round(totalFat / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1),
                    CarbsG = Math.Round(totalCarbs / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1)
                };
            }
        }
        else if (servings > 0)
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
}
