using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Services;

public static class RecipeScaler
{
    /// <summary>
    /// Scale all ingredient quantities by a multiplier.
    /// Returns a new list of groups with scaled quantities; original groups are not modified.
    /// volume_alt values are kept as-is (reference only).
    /// </summary>
    public static List<IngredientGroup> ScaleByMultiplier(
        List<IngredientGroup> groups, double multiplier)
    {
        if (!double.IsFinite(multiplier) || multiplier <= 0)
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be a finite positive number.");

        return groups.Select(g => new IngredientGroup
        {
            Heading = g.Heading,
            Items = g.Items.Select(i => new Ingredient
            {
                Quantity = i.Quantity * multiplier,
                Unit = i.Unit,
                Name = i.Name,
                VolumeAlt = i.VolumeAlt,
                Note = i.Note,
                NutritionId = i.NutritionId,
                DocLink = i.DocLink,
            }).ToList()
        }).ToList();
    }

    /// <summary>
    /// Calculate the multiplier needed to set a specific ingredient to a new quantity,
    /// then scale all ingredient quantities proportionally.
    /// Returns the computed multiplier and the new scaled groups.
    /// </summary>
    public static (double Multiplier, List<IngredientGroup> ScaledGroups) ScaleByLockedIngredient(
        List<IngredientGroup> groups,
        int groupIndex,
        int itemIndex,
        double newQuantity)
    {
        if (!double.IsFinite(newQuantity) || newQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Locked quantity must be a finite positive number.");

        if (groupIndex < 0 || groupIndex >= groups.Count)
            throw new ArgumentOutOfRangeException(nameof(groupIndex), "Group index is out of range.");

        var group = groups[groupIndex];
        if (itemIndex < 0 || itemIndex >= group.Items.Count)
            throw new ArgumentOutOfRangeException(nameof(itemIndex), "Item index is out of range.");

        var originalQuantity = group.Items[itemIndex].Quantity;
        if (originalQuantity == 0)
            throw new InvalidOperationException("Cannot lock an ingredient with zero quantity.");

        var multiplier = newQuantity / originalQuantity;
        if (!double.IsFinite(multiplier))
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Computed multiplier is not finite.");

        var scaled = ScaleByMultiplier(groups, multiplier);

        return (multiplier, scaled);
    }

    /// <summary>
    /// Scale a <see cref="NutrientInfo"/> by the given multiplier. Returns a new instance.
    /// </summary>
    public static NutrientInfo ScaleNutrients(NutrientInfo nutrients, double multiplier)
    {
        if (!double.IsFinite(multiplier) || multiplier <= 0)
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be a finite positive number.");

        return new NutrientInfo
        {
            CaloriesKcal = Math.Round(nutrients.CaloriesKcal * multiplier, 1),
            ProteinG = Math.Round(nutrients.ProteinG * multiplier, 1),
            FatG = Math.Round(nutrients.FatG * multiplier, 1),
            CarbsG = Math.Round(nutrients.CarbsG * multiplier, 1)
        };
    }
}
