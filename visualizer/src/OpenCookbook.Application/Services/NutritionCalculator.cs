using OpenCookbook.Application.Interfaces;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

/// <summary>
/// Calculates recipe nutrition by matching ingredients against the nutrition database.
/// Ingredients must have a <see cref="Ingredient.NutritionId"/> set to be matched.
/// Ingredients with a <see cref="Ingredient.DocLink"/> are resolved as sub-recipes and
/// their nutrition is scaled by the ingredient quantity and added to the parent totals.
/// Entries are cached after the first load to avoid redundant fetches within the same scope.
/// Registered as scoped — not thread-safe.
/// </summary>
public class NutritionCalculator
{
    private readonly INutritionRepository _nutritionRepository;
    private readonly IRecipeRepository? _recipeRepository;
    private IReadOnlyList<NutritionEntry>? _cachedEntries;
    private Dictionary<Guid, NutritionEntry>? _cachedIdLookup;

    public NutritionCalculator(INutritionRepository nutritionRepository, IRecipeRepository? recipeRepository = null)
    {
        _nutritionRepository = nutritionRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<RecipeNutrition> CalculateAsync(Recipe recipe, int servings = 1, string? basePath = null)
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
                // Sub-recipe reference: resolve via doc_link before unit check
                if (ingredient.DocLink is not null)
                {
                    if (_recipeRepository is not null)
                    {
                        try
                        {
                            var resolvedPath = DocLinkResolver.ResolvePath(basePath, ingredient.DocLink);
                            var subRecipe = await _recipeRepository.GetRecipeAsync(resolvedPath);
                            var subBasePath = DocLinkResolver.GetDirectory(resolvedPath);
                            var subNutrition = await CalculateAsync(subRecipe, basePath: subBasePath);

                            var scale = ingredient.Quantity;
                            var scaledNutrients = new NutrientInfo
                            {
                                CaloriesKcal = Math.Round(subNutrition.TotalNutrients.CaloriesKcal * scale, 1),
                                ProteinG = Math.Round(subNutrition.TotalNutrients.ProteinG * scale, 1),
                                FatG = Math.Round(subNutrition.TotalNutrients.FatG * scale, 1),
                                CarbsG = Math.Round(subNutrition.TotalNutrients.CarbsG * scale, 1)
                            };

                            totalCalories += scaledNutrients.CaloriesKcal;
                            totalProtein += scaledNutrients.ProteinG;
                            totalFat += scaledNutrients.FatG;
                            totalCarbs += scaledNutrients.CarbsG;

                            // Propagate any missing items from the sub-recipe so the parent
                            // accurately reflects whether the full nutrition is known.
                            foreach (var missing in subNutrition.MissingIngredients)
                                result.MissingIngredients.Add($"{ingredient.Name} → {missing}");

                            result.Ingredients.Add(new IngredientNutrition
                            {
                                IngredientName = ingredient.Name,
                                QuantityG = 0,
                                IsMatch = subNutrition.IsComplete,
                                Nutrients = scaledNutrients
                            });
                        }
                        catch (Exception ex) when (ex is ArgumentException
                                                       or KeyNotFoundException
                                                       or HttpRequestException
                                                       or InvalidOperationException)
                        {
                            result.MissingIngredients.Add(ingredient.Name);
                            result.Ingredients.Add(new IngredientNutrition
                            {
                                IngredientName = ingredient.Name,
                                QuantityG = 0,
                                IsMatch = false
                            });
                        }
                    }
                    else
                    {
                        result.MissingIngredients.Add(ingredient.Name);
                        result.Ingredients.Add(new IngredientNutrition
                        {
                            IngredientName = ingredient.Name,
                            QuantityG = 0,
                            IsMatch = false
                        });
                    }
                    continue;
                }

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

                // Only multiply when the serving-size unit matches the yields unit
                // (e.g. 4 meatballs from 24 meatballs yields a meaningful per-serving value).
                // When units differ (e.g. 120 g from 14 servings), the serving_size is
                // informational — use PerUnitNutrients directly to avoid cross-unit multiplication.
                if (string.Equals(recipe.ServingSize.Unit, recipe.Yields.Unit, StringComparison.OrdinalIgnoreCase))
                {
                    result.PerServingNutrients = new NutrientInfo
                    {
                        CaloriesKcal = Math.Round(totalCalories / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1),
                        ProteinG = Math.Round(totalProtein / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1),
                        FatG = Math.Round(totalFat / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1),
                        CarbsG = Math.Round(totalCarbs / recipe.Yields.Quantity * recipe.ServingSize.Quantity, 1)
                    };
                }
                else
                {
                    result.PerServingNutrients = result.PerUnitNutrients;
                }
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

    /// <summary>
    /// Kept for backward compatibility with existing tests that call this method directly.
    /// Delegates to <see cref="DocLinkResolver.ResolvePath"/>.
    /// </summary>
    internal static string ResolveSubRecipePath(string? basePath, string docLink)
        => DocLinkResolver.ResolvePath(basePath, docLink);

    /// <summary>
    /// Kept for backward compatibility with existing tests that call this method directly.
    /// Delegates to <see cref="DocLinkResolver.GetDirectory"/>.
    /// </summary>
    internal static string? GetDirectoryFromPath(string path)
        => DocLinkResolver.GetDirectory(path);
}
