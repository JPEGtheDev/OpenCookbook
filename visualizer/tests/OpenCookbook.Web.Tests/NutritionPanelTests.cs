using Bunit;
using OpenCookbook.Domain.Entities;
using OpenCookbook.Web.Components;

namespace OpenCookbook.Web.Tests;

public class NutritionPanelTests : BunitContext
{
    [Fact]
    public void NutritionPanel_WithNullNutrition_ShowsLoadingState()
    {
        // Arrange
        RecipeNutrition? nutrition = null;

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("Calculating nutrition", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithCompleteNutrition_ShowsCalories()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo
            {
                CaloriesKcal = 2327,
                ProteinG = 75.6,
                FatG = 47.8,
                CarbsG = 401.9
            },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("2327", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithCompleteNutrition_ShowsProtein()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo
            {
                CaloriesKcal = 2327,
                ProteinG = 75.6,
                FatG = 47.8,
                CarbsG = 401.9
            },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("75.6", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithCompleteNutrition_ShowsFat()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo
            {
                CaloriesKcal = 2327,
                ProteinG = 75.6,
                FatG = 47.8,
                CarbsG = 401.9
            },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("47.8", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithCompleteNutrition_ShowsCarbs()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo
            {
                CaloriesKcal = 2327,
                ProteinG = 75.6,
                FatG = 47.8,
                CarbsG = 401.9
            },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("401.9", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithCompleteNutrition_DoesNotShowWarning()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 100 },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.DoesNotContain("incomplete", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithMissingIngredients_ShowsWarning()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 100 },
            MissingIngredients = ["Garlic"],
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("incomplete", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithMissingIngredients_ListsMissingNames()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 100 },
            MissingIngredients = ["Garlic", "Onion"],
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("Garlic", cut.Markup);
        Assert.Contains("Onion", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithServingsGreaterThanOne_ShowsPerServing()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 800, ProteinG = 40, FatG = 24, CarbsG = 0 },
            PerServingNutrients = new NutrientInfo { CaloriesKcal = 200, ProteinG = 10, FatG = 6, CarbsG = 0 },
            Servings = 4
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("Per Serving", cut.Markup);
        Assert.Contains("4 servings", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithSingleServing_DoesNotShowPerServing()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 800 },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.DoesNotContain("Per Serving", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_BrisketRubCalories_AreNonZero()
    {
        // Arrange — simulates the Brisket Rub calculation result
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo
            {
                CaloriesKcal = 2327,
                ProteinG = 75.6,
                FatG = 47.8,
                CarbsG = 401.9
            },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert — the panel must show non-zero calories (regression test for 0-calorie bug)
        Assert.DoesNotContain(">0<", cut.Markup);
        Assert.Contains("2327", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WholeNumberCalories_FormatsWithoutDecimal()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 500 },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert — whole numbers should not show ".0"
        Assert.Contains("500", cut.Markup);
        Assert.DoesNotContain("500.0", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_DecimalCalories_FormatsWithOneDecimal()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 119.4 },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("119.4", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithPerUnitNutrients_ShowsPerUnitCard()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 960 },
            PerUnitNutrients = new NutrientInfo { CaloriesKcal = 40, ProteinG = 4, FatG = 2.4, CarbsG = 0 },
            YieldsQuantity = 24,
            YieldsUnit = "meatball",
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("Per Meatball", cut.Markup);
        Assert.Contains("24 total", cut.Markup);
        Assert.Contains("40", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithPerUnitAndServingSize_ShowsPerServingCard()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 960 },
            PerUnitNutrients = new NutrientInfo { CaloriesKcal = 40, ProteinG = 4, FatG = 2.4, CarbsG = 0 },
            PerServingNutrients = new NutrientInfo { CaloriesKcal = 120, ProteinG = 12, FatG = 7.2, CarbsG = 0 },
            YieldsQuantity = 24,
            YieldsUnit = "meatball",
            ServingSizeQuantity = 3,
            ServingSizeUnit = "meatball",
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.Contains("Per Serving", cut.Markup);
        Assert.Contains("3 meatballs", cut.Markup);
        Assert.Contains("120", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithoutYields_DoesNotShowPerUnitCard()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 960 },
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert
        Assert.DoesNotContain("total)", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithServingSizeOfOne_DoesNotShowPerServingCard()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 9259 },
            PerUnitNutrients = new NutrientInfo { CaloriesKcal = 514, ProteinG = 30.7, FatG = 37.8, CarbsG = 11.3 },
            PerServingNutrients = new NutrientInfo { CaloriesKcal = 514, ProteinG = 30.7, FatG = 37.8, CarbsG = 11.3 },
            YieldsQuantity = 18,
            YieldsUnit = "cutlet",
            ServingSizeQuantity = 1,
            ServingSizeUnit = "cutlet",
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert — per-unit card is shown but per-serving card is suppressed (it would be identical)
        Assert.Contains("Per Cutlet", cut.Markup);
        Assert.DoesNotContain("Per Serving", cut.Markup);
    }

    [Fact]
    public void NutritionPanel_WithGramServingSize_ShowsUnitWithoutPluralS()
    {
        // Arrange
        var nutrition = new RecipeNutrition
        {
            TotalNutrients = new NutrientInfo { CaloriesKcal = 2600 },
            PerUnitNutrients = new NutrientInfo { CaloriesKcal = 1.6, ProteinG = 0.1, FatG = 0.1, CarbsG = 0.1 },
            PerServingNutrients = new NutrientInfo { CaloriesKcal = 192, ProteinG = 12, FatG = 8, CarbsG = 10 },
            YieldsQuantity = 1624,
            YieldsUnit = "g",
            ServingSizeQuantity = 120,
            ServingSizeUnit = "g",
            Servings = 1
        };

        // Act
        var cut = Render<NutritionPanel>(parameters =>
            parameters.Add(p => p.Nutrition, nutrition));

        // Assert — "g" should not be pluralized to "gs"
        Assert.Contains("120 g", cut.Markup);
        Assert.DoesNotContain("120 gs", cut.Markup);
    }
}
