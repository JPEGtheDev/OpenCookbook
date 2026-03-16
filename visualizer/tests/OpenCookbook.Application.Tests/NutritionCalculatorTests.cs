using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class NutritionCalculatorTests
{
    private static readonly Guid GroundBeefId = new("2724c62f-1832-5ccf-97b0-d219812368d8");
    private static readonly Guid FineSeaSaltId = new("5c00ccf3-0bd6-5da0-8f97-a107d381609b");
    private static readonly Guid BlackPepperId = new("de21ba84-93f9-53e7-b961-aa1f6f04fe58");

    private static readonly List<NutritionEntry> SampleEntries =
    [
        new NutritionEntry
        {
            Id = GroundBeefId,
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
            Id = FineSeaSaltId,
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
            Id = BlackPepperId,
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
                        new Ingredient { Quantity = 500, Unit = "g", Name = "Ground Beef", NutritionId = GroundBeefId }
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
                        new Ingredient { Quantity = 200, Unit = "g", Name = "Ground Beef", NutritionId = GroundBeefId }
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
                        new Ingredient { Quantity = 400, Unit = "g", Name = "Ground Beef", NutritionId = GroundBeefId }
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
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Ground Beef", NutritionId = GroundBeefId },
                        new Ingredient { Quantity = 5,   Unit = "g", Name = "Fine Sea Salt", NutritionId = FineSeaSaltId }
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
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Ground Beef", NutritionId = GroundBeefId },
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
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Ground Beef", NutritionId = GroundBeefId }
                    ]
                },
                new IngredientGroup
                {
                    Heading = "Seasoning",
                    Items =
                    [
                        new Ingredient { Quantity = 3, Unit = "g", Name = "Black Pepper", NutritionId = BlackPepperId }
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
                        new Ingredient { Quantity = 100, Unit = "ml", Name = "Ground Beef", NutritionId = GroundBeefId }
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
    public void BuildIdLookup_PopulatesDictionaryWithEntryIds()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entries = new List<NutritionEntry>
        {
            new NutritionEntry { Id = id, Name = "Ground Beef", Aliases = [] }
        };

        // Act
        var idLookup = NutritionCalculator.BuildIdLookup(entries);

        // Assert
        Assert.Contains(id, idLookup);
    }

    [Fact]
    public void FindEntryById_ExistingId_ReturnsEntry()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entries = new List<NutritionEntry>
        {
            new NutritionEntry { Id = id, Name = "Ground Beef", Aliases = [] }
        };
        var idLookup = NutritionCalculator.BuildIdLookup(entries);

        // Act
        var result = NutritionCalculator.FindEntryById(idLookup, id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Ground Beef", result.Name);
    }

    [Fact]
    public void FindEntryById_UnknownId_ReturnsNull()
    {
        // Arrange
        var idLookup = NutritionCalculator.BuildIdLookup([]);

        // Act
        var result = NutritionCalculator.FindEntryById(idLookup, Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CalculateAsync_WithNutritionId_UsesReferencedEntry()
    {
        // Arrange
        var groundBeefId = Guid.NewGuid();
        var chickenId = Guid.NewGuid();
        var entries = new List<NutritionEntry>
        {
            new NutritionEntry
            {
                Id = groundBeefId,
                Name = "Ground Beef",
                Aliases = [],
                Per100g = new NutrientInfo { CaloriesKcal = 200, ProteinG = 20, FatG = 12, CarbsG = 0 }
            },
            new NutritionEntry
            {
                Id = chickenId,
                Name = "Chicken Wings",
                Aliases = [],
                Per100g = new NutrientInfo { CaloriesKcal = 175, ProteinG = 16, FatG = 12, CarbsG = 0 }
            }
        };
        var calculator = new NutritionCalculator(new FakeNutritionRepository(entries));

        // Ingredient name says "Ground Beef" but nutrition_id points to Chicken Wings entry
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Items =
                    [
                        new Ingredient
                        {
                            Quantity = 100,
                            Unit = "g",
                            Name = "Ground Beef",
                            NutritionId = chickenId
                        }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Empty(result.MissingIngredients);
        Assert.Equal(175, result.TotalNutrients.CaloriesKcal);
    }

    [Fact]
    public async Task CalculateAsync_WithoutNutritionId_IsNotMatched()
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
                        new Ingredient { Quantity = 100, Unit = "g", Name = "Ground Beef", NutritionId = null }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Single(result.MissingIngredients);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task CalculateAsync_WithUnknownNutritionId_IsNotMatched()
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
                        new Ingredient
                        {
                            Quantity = 100,
                            Unit = "g",
                            Name = "Ground Beef",
                            NutritionId = Guid.NewGuid() // ID not in the database
                        }
                    ]
                }
            ]
        };

        // Act
        var result = await calculator.CalculateAsync(recipe);

        // Assert
        Assert.Single(result.MissingIngredients);
        Assert.False(result.IsComplete);
    }

}
