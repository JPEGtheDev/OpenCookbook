using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

/// <summary>
/// End-to-end nutrition calculation tests using real recipe data from the repository.
/// Proves that the full pipeline (ingredient matching + nutrition calculation) produces correct values.
/// </summary>
public class BrisketRubNutritionTests
{
    private static readonly Guid GuajilloChilesId = new("de2f4f3c-e40c-5c1c-a862-fdb070804391");
    private static readonly Guid PasillaChilesId = new("fa672744-7a09-56f7-9ff6-99cca4fedfd7");
    private static readonly Guid FineSeaSaltId = new("5c00ccf3-0bd6-5da0-8f97-a107d381609b");
    private static readonly Guid BrownSugarId = new("6a128e75-ada9-53e5-b4f0-6f101aa16dc9");
    private static readonly Guid GarlicPowderId = new("6fd97682-cd9c-54a6-a8a9-94273d4f6137");
    private static readonly Guid OnionPowderId = new("5fd3cc2d-5d2a-56c1-90de-d30638cf3a45");
    private static readonly Guid BlackPepperId = new("de21ba84-93f9-53e7-b961-aa1f6f04fe58");
    private static readonly Guid MustardSeedId = new("7ee3a8f3-767f-54bd-8a0e-f8440467c11c");

    /// <summary>
    /// Real nutrition data for Brisket Rub ingredients, matching nutrition-db.json values.
    /// </summary>
    private static readonly List<NutritionEntry> RealNutritionEntries =
    [
        new NutritionEntry
        {
            Id = GuajilloChilesId,
            Name = "Guajillo Chiles",
            Aliases = ["guajillo chile", "dried guajillo"],
            Per100g = new NutrientInfo { CaloriesKcal = 314, ProteinG = 11.0, FatG = 5.8, CarbsG = 56.0 }
        },
        new NutritionEntry
        {
            Id = PasillaChilesId,
            Name = "Pasilla Chiles",
            Aliases = ["pasilla chile", "dried pasilla"],
            Per100g = new NutrientInfo { CaloriesKcal = 314, ProteinG = 11.0, FatG = 5.8, CarbsG = 56.0 }
        },
        new NutritionEntry
        {
            Id = FineSeaSaltId,
            Name = "Fine Sea Salt",
            Aliases = ["salt", "sea salt"],
            Per100g = new NutrientInfo { CaloriesKcal = 0, ProteinG = 0, FatG = 0, CarbsG = 0 }
        },
        new NutritionEntry
        {
            Id = BrownSugarId,
            Name = "Brown Sugar",
            Aliases = ["light brown sugar", "dark brown sugar"],
            Per100g = new NutrientInfo { CaloriesKcal = 380, ProteinG = 0.1, FatG = 0, CarbsG = 98.1 }
        },
        new NutritionEntry
        {
            Id = GarlicPowderId,
            Name = "Garlic Powder",
            Aliases = ["garlic pwd"],
            Per100g = new NutrientInfo { CaloriesKcal = 331, ProteinG = 16.6, FatG = 0.7, CarbsG = 72.7 }
        },
        new NutritionEntry
        {
            Id = OnionPowderId,
            Name = "Onion Powder",
            Aliases = ["onion pwd"],
            Per100g = new NutrientInfo { CaloriesKcal = 341, ProteinG = 10.4, FatG = 1.0, CarbsG = 79.1 }
        },
        new NutritionEntry
        {
            Id = BlackPepperId,
            Name = "Black Pepper",
            Aliases = ["pepper", "ground black pepper"],
            Per100g = new NutrientInfo { CaloriesKcal = 251, ProteinG = 10.4, FatG = 3.3, CarbsG = 63.9 }
        },
        new NutritionEntry
        {
            Id = MustardSeedId,
            Name = "Mustard Seed",
            Aliases = ["mustard seeds", "yellow mustard seed"],
            Per100g = new NutrientInfo { CaloriesKcal = 508, ProteinG = 26.1, FatG = 36.2, CarbsG = 28.1 }
        }
    ];

    /// <summary>
    /// Creates the Brisket Rub recipe matching Recipes/Brisket/Guajillo_Brisket_Rub.yaml.
    /// </summary>
    private static Recipe CreateBrisketRubRecipe()
    {
        return new Recipe
        {
            Name = "Guajillo Brisket Rub",
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Guajillo Chiles", NutritionId = GuajilloChilesId },
                        new Ingredient { Quantity = 50,  Unit = "g", Name = "Pasilla Chiles",  NutritionId = PasillaChilesId },
                        new Ingredient { Quantity = 300, Unit = "g", Name = "Fine Sea Salt",   NutritionId = FineSeaSaltId },
                        new Ingredient { Quantity = 300, Unit = "g", Name = "Brown Sugar",     NutritionId = BrownSugarId },
                        new Ingredient { Quantity = 20,  Unit = "g", Name = "Garlic Powder",   NutritionId = GarlicPowderId },
                        new Ingredient { Quantity = 35,  Unit = "g", Name = "Onion Powder",    NutritionId = OnionPowderId },
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Black Pepper",    NutritionId = BlackPepperId },
                        new Ingredient { Quantity = 55,  Unit = "g", Name = "Mustard Seed",    NutritionId = MustardSeedId }
                    ]
                }
            ]
        };
    }

    [Fact]
    public async Task CalculateAsync_BrisketRub_IsComplete()
    {
        // Arrange
        var calculator = new NutritionCalculator(new FakeNutritionRepository(RealNutritionEntries));
        var recipe = CreateBrisketRubRecipe();

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.True(result.IsComplete);
    }

    [Fact]
    public async Task CalculateAsync_BrisketRub_HasNoMissingIngredients()
    {
        // Arrange
        var calculator = new NutritionCalculator(new FakeNutritionRepository(RealNutritionEntries));
        var recipe = CreateBrisketRubRecipe();

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Empty(result.MissingIngredients);
    }

    [Fact]
    public async Task CalculateAsync_BrisketRub_TotalCaloriesAreNonZero()
    {
        // Arrange
        var calculator = new NutritionCalculator(new FakeNutritionRepository(RealNutritionEntries));
        var recipe = CreateBrisketRubRecipe();

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.True(result.TotalNutrients.CaloriesKcal > 0,
            $"Expected non-zero total calories but got {result.TotalNutrients.CaloriesKcal}");
    }

    [Fact]
    public async Task CalculateAsync_BrisketRub_TotalCaloriesAreCorrect()
    {
        // Arrange
        var calculator = new NutritionCalculator(new FakeNutritionRepository(RealNutritionEntries));
        var recipe = CreateBrisketRubRecipe();

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        // Guajillo: 100g × 314/100 = 314.0
        // Pasilla:   50g × 314/100 = 157.0
        // Salt:     300g ×   0/100 =   0.0
        // Brown Sugar: 300g × 380/100 = 1140.0
        // Garlic Pwd:   20g × 331/100 =  66.2
        // Onion Pwd:    35g × 341/100 = 119.4 (rounds from 119.35)
        // Black Pepper: 100g × 251/100 = 251.0
        // Mustard Seed:  55g × 508/100 = 279.4
        // Total = 2327.0
        Assert.Equal(2327, result.TotalNutrients.CaloriesKcal);
    }

    [Fact]
    public async Task CalculateAsync_BrisketRub_BrownSugarContribution()
    {
        // Arrange
        var calculator = new NutritionCalculator(new FakeNutritionRepository(RealNutritionEntries));
        var recipe = CreateBrisketRubRecipe();

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert — Brown Sugar is 300g at 380 kcal/100g = 1140 kcal
        var brownSugar = result.Ingredients.First(i => i.IngredientName == "Brown Sugar");
        Assert.True(brownSugar.IsMatch);
        Assert.NotNull(brownSugar.Nutrients);
        Assert.Equal(1140, brownSugar.Nutrients!.CaloriesKcal);
    }

    [Fact]
    public async Task CalculateAsync_BrisketRub_AllMacrosAreNonZero()
    {
        // Arrange
        var calculator = new NutritionCalculator(new FakeNutritionRepository(RealNutritionEntries));
        var recipe = CreateBrisketRubRecipe();

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.True(result.TotalNutrients.ProteinG > 0);
        Assert.True(result.TotalNutrients.FatG > 0);
        Assert.True(result.TotalNutrients.CarbsG > 0);
    }

    [Fact]
    public async Task CalculateAsync_BrisketRub_MatchesAllEightIngredients()
    {
        // Arrange
        var calculator = new NutritionCalculator(new FakeNutritionRepository(RealNutritionEntries));
        var recipe = CreateBrisketRubRecipe();

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Equal(8, result.Ingredients.Count);
        Assert.All(result.Ingredients, i => Assert.True(i.IsMatch));
    }

}
