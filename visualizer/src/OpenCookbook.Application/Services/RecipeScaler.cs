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
        if (multiplier <= 0)
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be positive.");

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
        if (newQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Locked quantity must be positive.");

        if (groupIndex < 0 || groupIndex >= groups.Count)
            throw new ArgumentOutOfRangeException(nameof(groupIndex), "Group index is out of range.");

        var group = groups[groupIndex];
        if (itemIndex < 0 || itemIndex >= group.Items.Count)
            throw new ArgumentOutOfRangeException(nameof(itemIndex), "Item index is out of range.");

        var originalQuantity = group.Items[itemIndex].Quantity;
        if (originalQuantity == 0)
            throw new InvalidOperationException("Cannot lock an ingredient with zero quantity.");

        var multiplier = newQuantity / originalQuantity;
        var scaled = ScaleByMultiplier(groups, multiplier);

        return (multiplier, scaled);
    }
}
