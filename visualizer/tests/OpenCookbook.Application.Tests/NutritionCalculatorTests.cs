using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class NutritionCalculatorTests
{
    private static readonly List<NutritionEntry> SampleEntries =
    [
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Ground Beef",
            Aliases = ["beef", "minced beef"],
            Per100g = new NutrientInfo
            {
                CaloriesKcal = 200,
                ProteinG = 20,
                FatG = 12,
                CarbsG = 0
            }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Fine Sea Salt",
            Aliases = ["salt", "sea salt"],
            Per100g = new NutrientInfo
            {
                CaloriesKcal = 0,
                ProteinG = 0,
                FatG = 0,
                CarbsG = 0
            }
        },
        new NutritionEntry
        {
            Id = Guid.NewGuid(),
            Name = "Black Pepper",
            Aliases = ["pepper"],
            Per100g = new NutrientInfo
            {
                CaloriesKcal = 251,
                ProteinG = 10.4,
                FatG = 3.3,
                CarbsG = 63.9
            }
        }
    ];

    private NutritionCalculator CreateCalculator()
    {
        return new NutritionCalculator(new FakeNutritionRepository(SampleEntries));
    }

    [Fact]
    public async Task CalculateAsync_WithMatchedIngredient_ReturnsCorrectCalories()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 500, Unit = "g", Name = "Ground Beef" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Equal(1000, result.TotalNutrients.CaloriesKcal);
    }

    [Fact]
    public async Task CalculateAsync_WithMatchedIngredient_ReturnsCorrectMacros()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 200, Unit = "g", Name = "Ground Beef" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Equal(400, result.TotalNutrients.CaloriesKcal);
        Assert.Equal(40, result.TotalNutrients.ProteinG);
        Assert.Equal(24, result.TotalNutrients.FatG);
        Assert.Equal(0, result.TotalNutrients.CarbsG);
    }

    [Fact]
    public async Task CalculateAsync_WithUnknownIngredient_AddsMissingIngredient()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Unknown Spice" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Single(result.MissingIngredients);
        Assert.Contains("Unknown Spice", result.MissingIngredients);
    }

    [Fact]
    public async Task CalculateAsync_WithNonGramUnit_AddsMissingIngredient()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 2, Unit = "cloves", Name = "Garlic" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Single(result.MissingIngredients);
        Assert.Contains("Garlic", result.MissingIngredients);
    }

    [Fact]
    public async Task CalculateAsync_WithAlias_MatchesEntry()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Minced Beef" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Empty(result.MissingIngredients);
        Assert.Equal(200, result.TotalNutrients.CaloriesKcal);
    }

    [Fact]
    public async Task CalculateAsync_WithParenthetical_StripsAndMatches()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 18, Unit = "g", Name = "Fine Sea Salt (for the water)" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Empty(result.MissingIngredients);
    }

    [Fact]
    public async Task CalculateAsync_WithServings_ReturnsPerServingValues()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 400, Unit = "g", Name = "Ground Beef" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe, servings: 4);

        // Assert
        Assert.Equal(4, result.Servings);
        Assert.True(result.IsComplete);
        Assert.Empty(result.MissingIngredients);
        Assert.Single(result.Ingredients);

        Assert.Equal(800, result.TotalNutrients.CaloriesKcal);
        Assert.Equal(80, result.TotalNutrients.ProteinG);
        Assert.Equal(48, result.TotalNutrients.FatG);
        Assert.Equal(0, result.TotalNutrients.CarbsG);

        Assert.NotNull(result.PerServingNutrients);
        Assert.Equal(200, result.PerServingNutrients!.CaloriesKcal);
        Assert.Equal(20, result.PerServingNutrients.ProteinG);
        Assert.Equal(12, result.PerServingNutrients.FatG);
        Assert.Equal(0, result.PerServingNutrients.CarbsG);

        var ingredientResult = result.Ingredients[0];
        Assert.Equal("Ground Beef", ingredientResult.IngredientName);
        Assert.Equal(400, ingredientResult.QuantityG);
        Assert.True(ingredientResult.IsMatch);
        Assert.NotNull(ingredientResult.Nutrients);
        Assert.Equal(800, ingredientResult.Nutrients!.CaloriesKcal);
    }

    [Fact]
    public async Task CalculateAsync_IsComplete_TrueWhenAllMatch()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Ground Beef" },
                        new Ingredient { Quantity = 5, Unit = "g", Name = "Fine Sea Salt" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.True(result.IsComplete);
    }

    [Fact]
    public async Task CalculateAsync_IsComplete_FalseWhenPartialMatch()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Ground Beef" },
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Unknown Ingredient" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CalculateAsync_MultipleGroups_SumsAllIngredients()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Ground Beef" }
                    ]
                },
                new IngredientGroup
                {
                    Heading = "Seasoning",
                    Items =
                    [
                        new Ingredient { Quantity = 3, Unit = "g", Name = "Black Pepper" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Equal(2, result.Ingredients.Count);
        Assert.True(result.TotalNutrients.CaloriesKcal > 200);
    }

    [Fact]
    public async Task CalculateAsync_MlUnit_IsAccepted()
    {
        // Arrange
        var calculator = CreateCalculator();
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient { Quantity = 100, Unit = "ml", Name = "Ground Beef" }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Empty(result.MissingIngredients);
    }

    [Fact]
    public void BuildLookup_PopulatesNamesAndAliases()
    {
        // Arrange
        var entries = new List<NutritionEntry>
        {
            new NutritionEntry
            {
                Name = "Salt",
                Aliases = ["sea salt", "table salt"]
            }
        };

        // Act
        var lookup = NutritionCalculator.BuildLookup(entries);

        // Assert
        Assert.True(lookup.ContainsKey("salt"));
        Assert.True(lookup.ContainsKey("sea salt"));
        Assert.True(lookup.ContainsKey("table salt"));
    }

    [Fact]
    public void BuildLookup_IsCaseInsensitive()
    {
        // Arrange
        var entries = new List<NutritionEntry>
        {
            new NutritionEntry { Name = "Ground Beef", Aliases = [] }
        };

        // Act
        var lookup = NutritionCalculator.BuildLookup(entries);

        // Assert
        Assert.True(lookup.ContainsKey("ground beef"));
        Assert.True(lookup.ContainsKey("GROUND BEEF"));
    }

    [Fact]
    public void FindEntry_ExactMatch_ReturnsEntry()
    {
        // Arrange
        var entries = new List<NutritionEntry>
        {
            new NutritionEntry { Name = "Ground Beef", Aliases = [] }
        };
        var lookup = NutritionCalculator.BuildLookup(entries);

        // Act
        var result = NutritionCalculator.FindEntry(lookup, "Ground Beef");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FindEntry_NoMatch_ReturnsNull()
    {
        // Arrange
        var lookup = NutritionCalculator.BuildLookup([]);

        // Act
        var result = NutritionCalculator.FindEntry(lookup, "Unknown");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindEntry_ParentheticalMatch_ReturnsEntry()
    {
        // Arrange
        var entries = new List<NutritionEntry>
        {
            new NutritionEntry { Name = "Fine Sea Salt", Aliases = [] }
        };
        var lookup = NutritionCalculator.BuildLookup(entries);

        // Act
        var result = NutritionCalculator.FindEntry(lookup, "Fine Sea Salt (for the water)");

        // Assert
        Assert.NotNull(result);
    }

}
