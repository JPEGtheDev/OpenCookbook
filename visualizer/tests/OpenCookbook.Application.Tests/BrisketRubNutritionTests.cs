using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

/// <summary>
/// End-to-end nutrition calculation tests using real recipe data from the repository.
/// Proves that the full pipeline (ingredient matching + nutrition calculation) produces correct values.
/// </summary>
public class BrisketRubNutritionTests
{
    /// <summary>
    /// Real nutrition data for Brisket Rub ingredients, matching nutrition-db.json values.
    /// </summary>
    private static readonly List<NutritionEntry> RealNutritionEntries =
    [
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Guajillo Chiles",
            Aliases = ["guajillo chile", "dried guajillo"],
            Per100g = new NutrientInfo { CaloriesKcal = 314, ProteinG = 11.0, FatG = 5.8, CarbsG = 56.0 }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Pasilla Chiles",
            Aliases = ["pasilla chile", "dried pasilla"],
            Per100g = new NutrientInfo { CaloriesKcal = 314, ProteinG = 11.0, FatG = 5.8, CarbsG = 56.0 }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Fine Sea Salt",
            Aliases = ["salt", "sea salt"],
            Per100g = new NutrientInfo { CaloriesKcal = 0, ProteinG = 0, FatG = 0, CarbsG = 0 }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Brown Sugar",
            Aliases = ["light brown sugar", "dark brown sugar"],
            Per100g = new NutrientInfo { CaloriesKcal = 380, ProteinG = 0.1, FatG = 0, CarbsG = 98.1 }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Garlic Powder",
            Aliases = ["garlic pwd"],
            Per100g = new NutrientInfo { CaloriesKcal = 331, ProteinG = 16.6, FatG = 0.7, CarbsG = 72.7 }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Onion Powder",
            Aliases = ["onion pwd"],
            Per100g = new NutrientInfo { CaloriesKcal = 341, ProteinG = 10.4, FatG = 1.0, CarbsG = 79.1 }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Black Pepper",
            Aliases = ["pepper", "ground black pepper"],
            Per100g = new NutrientInfo { CaloriesKcal = 251, ProteinG = 10.4, FatG = 3.3, CarbsG = 63.9 }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
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
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Guajillo Chiles" },
                        new Ingredient { Quantity = 50,  Unit = "g", Name = "Pasilla Chiles" },
                        new Ingredient { Quantity = 300, Unit = "g", Name = "Fine Sea Salt" },
                        new Ingredient { Quantity = 300, Unit = "g", Name = "Brown Sugar" },
                        new Ingredient { Quantity = 20,  Unit = "g", Name = "Garlic Powder" },
                        new Ingredient { Quantity = 35,  Unit = "g", Name = "Onion Powder" },
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Black Pepper" },
                        new Ingredient { Quantity = 55,  Unit = "g", Name = "Mustard Seed" }
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
