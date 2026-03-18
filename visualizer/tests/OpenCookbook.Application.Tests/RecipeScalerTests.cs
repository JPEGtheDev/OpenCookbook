using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class RecipeScalerTests
{
    private static List<IngredientGroup> CreateSampleGroups()
    {
        return
        [
            new IngredientGroup
            {
                Heading = null,
                Items =
                [
                    new Ingredient { Quantity = 500, Unit = "g", Name = "Chicken Thighs" },
                    new Ingredient { Quantity = 200, Unit = "ml", Name = "Yogurt" },
                    new Ingredient { Quantity = 5, Unit = "g", Name = "Cumin", VolumeAlt = "1 tsp." },
                    new Ingredient { Quantity = 3, Unit = "g", Name = "Black Pepper", VolumeAlt = "3/4 tsp." },
                ]
            },
            new IngredientGroup
            {
                Heading = "Garnish",
                Items =
                [
                    new Ingredient { Quantity = 30, Unit = "g", Name = "Fresh Parsley" },
                ]
            }
        ];
    }

    // ── ScaleByMultiplier ──────────────────────────────

    [Fact]
    public void ScaleByMultiplier_AtOneX_ReturnsOriginalQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var scaled = RecipeScaler.ScaleByMultiplier(groups, 1.0);

        // Assert
        Assert.Equal(500, scaled[0].Items[0].Quantity);
        Assert.Equal(200, scaled[0].Items[1].Quantity);
        Assert.Equal(30, scaled[1].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByMultiplier_AtTwoX_DoublesAllQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var scaled = RecipeScaler.ScaleByMultiplier(groups, 2.0);

        // Assert
        Assert.Equal(1000, scaled[0].Items[0].Quantity);
        Assert.Equal(400, scaled[0].Items[1].Quantity);
        Assert.Equal(10, scaled[0].Items[2].Quantity);
        Assert.Equal(6, scaled[0].Items[3].Quantity);
        Assert.Equal(60, scaled[1].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByMultiplier_AtHalfX_HalvesAllQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var scaled = RecipeScaler.ScaleByMultiplier(groups, 0.5);

        // Assert
        Assert.Equal(250, scaled[0].Items[0].Quantity);
        Assert.Equal(100, scaled[0].Items[1].Quantity);
        Assert.Equal(15, scaled[1].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByMultiplier_PreservesVolumeAlt()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var scaled = RecipeScaler.ScaleByMultiplier(groups, 3.0);

        // Assert — volume_alt should NOT be scaled, stays as reference
        Assert.Equal("1 tsp.", scaled[0].Items[2].VolumeAlt);
        Assert.Equal("3/4 tsp.", scaled[0].Items[3].VolumeAlt);
    }

    [Fact]
    public void ScaleByMultiplier_PreservesGroupHeadings()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var scaled = RecipeScaler.ScaleByMultiplier(groups, 2.0);

        // Assert
        Assert.Null(scaled[0].Heading);
        Assert.Equal("Garnish", scaled[1].Heading);
    }

    [Fact]
    public void ScaleByMultiplier_PreservesIngredientMetadata()
    {
        // Arrange
        var groups = CreateSampleGroups();
        groups[0].Items[0].Note = "boneless";
        groups[0].Items[0].NutritionId = Guid.NewGuid();
        groups[0].Items[0].DocLink = "./sub_recipe.yaml";

        // Act
        var scaled = RecipeScaler.ScaleByMultiplier(groups, 2.0);

        // Assert
        Assert.Equal("boneless", scaled[0].Items[0].Note);
        Assert.Equal(groups[0].Items[0].NutritionId, scaled[0].Items[0].NutritionId);
        Assert.Equal("./sub_recipe.yaml", scaled[0].Items[0].DocLink);
    }

    [Fact]
    public void ScaleByMultiplier_DoesNotMutateOriginal()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        _ = RecipeScaler.ScaleByMultiplier(groups, 3.0);

        // Assert — original is unchanged
        Assert.Equal(500, groups[0].Items[0].Quantity);
        Assert.Equal(200, groups[0].Items[1].Quantity);
    }

    [Fact]
    public void ScaleByMultiplier_ZeroMultiplier_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByMultiplier(groups, 0));
    }

    [Fact]
    public void ScaleByMultiplier_NegativeMultiplier_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByMultiplier(groups, -1));
    }

    [Fact]
    public void ScaleByMultiplier_NaNMultiplier_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByMultiplier(groups, double.NaN));
    }

    [Fact]
    public void ScaleByMultiplier_InfinityMultiplier_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByMultiplier(groups, double.PositiveInfinity));
    }

    [Fact]
    public void ScaleByMultiplier_EmptyGroups_ReturnsEmptyList()
    {
        // Arrange
        var groups = new List<IngredientGroup>();

        // Act
        var scaled = RecipeScaler.ScaleByMultiplier(groups, 2.0);

        // Assert
        Assert.Empty(scaled);
    }

    // ── ScaleByLockedIngredient ────────────────────────

    [Fact]
    public void ScaleByLockedIngredient_LocksChickenAt1000g_DoublesEverything()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act — lock chicken (group 0, item 0) at 1000g (originally 500g → 2× multiplier)
        var (multiplier, scaled) = RecipeScaler.ScaleByLockedIngredient(groups, 0, 0, 1000);

        // Assert
        Assert.Equal(2.0, multiplier, precision: 10);
        Assert.Equal(1000, scaled[0].Items[0].Quantity);
        Assert.Equal(400, scaled[0].Items[1].Quantity);
        Assert.Equal(10, scaled[0].Items[2].Quantity);
        Assert.Equal(60, scaled[1].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByLockedIngredient_LocksYogurtAt450g_ScalesProportionally()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act — lock yogurt (group 0, item 1) at 450ml (originally 200ml → 2.25× multiplier)
        var (multiplier, scaled) = RecipeScaler.ScaleByLockedIngredient(groups, 0, 1, 450);

        // Assert
        Assert.Equal(2.25, multiplier, precision: 10);
        Assert.Equal(1125, scaled[0].Items[0].Quantity);
        Assert.Equal(450, scaled[0].Items[1].Quantity);
    }

    [Fact]
    public void ScaleByLockedIngredient_LocksGarnishIngredient_ScalesAll()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act — lock parsley in garnish group (group 1, item 0) at 15g (originally 30g → 0.5×)
        var (multiplier, scaled) = RecipeScaler.ScaleByLockedIngredient(groups, 1, 0, 15);

        // Assert
        Assert.Equal(0.5, multiplier, precision: 10);
        Assert.Equal(250, scaled[0].Items[0].Quantity);
        Assert.Equal(15, scaled[1].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByLockedIngredient_PreservesVolumeAlt()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var (_, scaled) = RecipeScaler.ScaleByLockedIngredient(groups, 0, 0, 1000);

        // Assert
        Assert.Equal("1 tsp.", scaled[0].Items[2].VolumeAlt);
    }

    [Fact]
    public void ScaleByLockedIngredient_InvalidGroupIndex_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByLockedIngredient(groups, 5, 0, 100));
    }

    [Fact]
    public void ScaleByLockedIngredient_InvalidItemIndex_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByLockedIngredient(groups, 0, 99, 100));
    }

    [Fact]
    public void ScaleByLockedIngredient_ZeroQuantity_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByLockedIngredient(groups, 0, 0, 0));
    }

    [Fact]
    public void ScaleByLockedIngredient_ZeroOriginalQuantity_Throws()
    {
        // Arrange
        var groups = new List<IngredientGroup>
        {
            new()
            {
                Items = [new Ingredient { Quantity = 0, Unit = "g", Name = "Water" }]
            }
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            RecipeScaler.ScaleByLockedIngredient(groups, 0, 0, 100));
    }

    [Fact]
    public void ScaleByLockedIngredient_NaNNewQuantity_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByLockedIngredient(groups, 0, 0, double.NaN));
    }

    [Fact]
    public void ScaleByLockedIngredient_InfinityNewQuantity_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByLockedIngredient(groups, 0, 0, double.PositiveInfinity));
    }

    // ── ScaleByTargetYield ─────────────────────────────

    [Fact]
    public void ScaleByTargetYield_HalvesYield_HalvesAllQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups(); // 500g chicken, 200ml yogurt, etc.

        // Act — original yield 8, target 4 → 0.5× multiplier
        var (multiplier, scaled) = RecipeScaler.ScaleByTargetYield(groups, 8, 4);

        // Assert
        Assert.Equal(0.5, multiplier, precision: 10);
        Assert.Equal(250, scaled[0].Items[0].Quantity);
        Assert.Equal(100, scaled[0].Items[1].Quantity);
    }

    [Fact]
    public void ScaleByTargetYield_DoublesYield_DoublesAllQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act — original yield 8, target 16 → 2× multiplier
        var (multiplier, scaled) = RecipeScaler.ScaleByTargetYield(groups, 8, 16);

        // Assert
        Assert.Equal(2.0, multiplier, precision: 10);
        Assert.Equal(1000, scaled[0].Items[0].Quantity);
        Assert.Equal(400, scaled[0].Items[1].Quantity);
        Assert.Equal(60, scaled[1].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByTargetYield_SameAsOriginal_ReturnsMultiplierOne()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act — target equals original → multiplier = 1.0
        var (multiplier, scaled) = RecipeScaler.ScaleByTargetYield(groups, 8, 8);

        // Assert
        Assert.Equal(1.0, multiplier, precision: 10);
        Assert.Equal(500, scaled[0].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByTargetYield_FractionalTarget_ScalesCorrectly()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act — original yield 4, target 10 → 2.5× multiplier
        var (multiplier, scaled) = RecipeScaler.ScaleByTargetYield(groups, 4, 10);

        // Assert
        Assert.Equal(2.5, multiplier, precision: 10);
        Assert.Equal(1250, scaled[0].Items[0].Quantity);
    }

    [Fact]
    public void ScaleByTargetYield_PreservesVolumeAlt()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var (_, scaled) = RecipeScaler.ScaleByTargetYield(groups, 8, 16);

        // Assert — volume_alt should NOT be scaled
        Assert.Equal("1 tsp.", scaled[0].Items[2].VolumeAlt);
        Assert.Equal("3/4 tsp.", scaled[0].Items[3].VolumeAlt);
    }

    [Fact]
    public void ScaleByTargetYield_DoesNotMutateOriginal()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        _ = RecipeScaler.ScaleByTargetYield(groups, 8, 16);

        // Assert — original is unchanged
        Assert.Equal(500, groups[0].Items[0].Quantity);
        Assert.Equal(200, groups[0].Items[1].Quantity);
    }

    [Fact]
    public void ScaleByTargetYield_ZeroOriginalYield_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByTargetYield(groups, 0, 8));
    }

    [Fact]
    public void ScaleByTargetYield_NegativeOriginalYield_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByTargetYield(groups, -1, 8));
    }

    [Fact]
    public void ScaleByTargetYield_NaNOriginalYield_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByTargetYield(groups, double.NaN, 8));
    }

    [Fact]
    public void ScaleByTargetYield_InfinityOriginalYield_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByTargetYield(groups, double.PositiveInfinity, 8));
    }

    [Fact]
    public void ScaleByTargetYield_ZeroTargetYield_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByTargetYield(groups, 8, 0));
    }

    [Fact]
    public void ScaleByTargetYield_NaNTargetYield_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByTargetYield(groups, 8, double.NaN));
    }

    [Fact]
    public void ScaleByTargetYield_InfinityTargetYield_Throws()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleByTargetYield(groups, 8, double.PositiveInfinity));
    }

    // ── ScaleNutrients ─────────────────────────────────

    [Fact]
    public void ScaleNutrients_AtTwoX_DoublesAllValues()
    {
        // Arrange
        var nutrients = new NutrientInfo
        {
            CaloriesKcal = 500,
            ProteinG = 25,
            FatG = 10,
            CarbsG = 60
        };

        // Act
        var scaled = RecipeScaler.ScaleNutrients(nutrients, 2.0);

        // Assert
        Assert.Equal(1000, scaled.CaloriesKcal);
        Assert.Equal(50, scaled.ProteinG);
        Assert.Equal(20, scaled.FatG);
        Assert.Equal(120, scaled.CarbsG);
    }

    [Fact]
    public void ScaleNutrients_AtHalfX_HalvesAllValues()
    {
        // Arrange
        var nutrients = new NutrientInfo
        {
            CaloriesKcal = 500,
            ProteinG = 25,
            FatG = 10,
            CarbsG = 60
        };

        // Act
        var scaled = RecipeScaler.ScaleNutrients(nutrients, 0.5);

        // Assert
        Assert.Equal(250, scaled.CaloriesKcal);
        Assert.Equal(12.5, scaled.ProteinG);
        Assert.Equal(5, scaled.FatG);
        Assert.Equal(30, scaled.CarbsG);
    }

    [Fact]
    public void ScaleNutrients_DoesNotMutateOriginal()
    {
        // Arrange
        var nutrients = new NutrientInfo { CaloriesKcal = 500 };

        // Act
        _ = RecipeScaler.ScaleNutrients(nutrients, 3.0);

        // Assert
        Assert.Equal(500, nutrients.CaloriesKcal);
    }

    [Fact]
    public void ScaleNutrients_NaN_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var nutrients = new NutrientInfo { CaloriesKcal = 500 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecipeScaler.ScaleNutrients(nutrients, double.NaN));
    }
}
