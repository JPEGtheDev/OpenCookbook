using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

/// <summary>
/// Applies user-selected ingredient alternates to a composed recipe, producing a
/// new recipe instance with the selected alternates swapped into the ingredient list.
/// The original recipe is not mutated.
/// </summary>
public static class AlternateSelector
{
    /// <summary>
    /// A key identifying a specific ingredient within a recipe's ingredient list.
    /// </summary>
    public readonly record struct IngredientKey(int GroupIndex, int ItemIndex);

    /// <summary>
    /// Creates a copy of <paramref name="recipe"/> with the specified alternates applied.
    /// Each entry in <paramref name="selections"/> maps an ingredient position to the
    /// zero-based index of its selected alternate. Ingredients without a matching selection
    /// (or with a selection of -1) retain their default values.
    /// </summary>
    public static Recipe ApplySelections(
        Recipe recipe,
        IReadOnlyDictionary<IngredientKey, int> selections)
    {
        if (selections.Count == 0)
            return recipe;

        var newGroups = new List<IngredientGroup>(recipe.Ingredients.Count);

        for (var gi = 0; gi < recipe.Ingredients.Count; gi++)
        {
            var group = recipe.Ingredients[gi];
            var newItems = new List<Ingredient>(group.Items.Count);

            for (var ii = 0; ii < group.Items.Count; ii++)
            {
                var item = group.Items[ii];
                var key = new IngredientKey(gi, ii);

                if (selections.TryGetValue(key, out var altIndex)
                    && altIndex >= 0
                    && item.Alternates is { Count: > 0 }
                    && altIndex < item.Alternates.Count)
                {
                    var alt = item.Alternates[altIndex];
                    newItems.Add(new Ingredient
                    {
                        Quantity = alt.Quantity ?? item.Quantity,
                        Unit = alt.Unit ?? item.Unit,
                        Name = alt.Name,
                        VolumeAlt = alt.VolumeAlt ?? item.VolumeAlt,
                        WeightAlt = alt.WeightAlt ?? item.WeightAlt,
                        Note = alt.Note ?? item.Note,
                        NutritionId = alt.NutritionId ?? item.NutritionId,
                        DocLink = item.DocLink,
                        Alternates = item.Alternates,
                    });
                }
                else
                {
                    newItems.Add(item);
                }
            }

            newGroups.Add(new IngredientGroup
            {
                Heading = group.Heading,
                Items = newItems
            });
        }

        return new Recipe
        {
            Name = recipe.Name,
            Version = recipe.Version,
            Author = recipe.Author,
            Description = recipe.Description,
            Status = recipe.Status,
            Ingredients = newGroups,
            Utensils = recipe.Utensils,
            Instructions = recipe.Instructions,
            Related = recipe.Related,
            Notes = recipe.Notes,
            Yields = recipe.Yields,
            ServingSize = recipe.ServingSize,
        };
    }
}
