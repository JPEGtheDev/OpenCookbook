using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class FitnessExportServiceTests
{
    private static Recipe BuildRecipe(string name = "Test Recipe", int? servingSize = null, int? yields = null)
    {
        var recipe = new Recipe
        {
            Name = name,
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 400, Unit = "g", Name = "Ground Beef" },
                        new Ingredient { Quantity = 3, Unit = "g", Name = "Black Pepper" },
                    ]
                }
            ]
        };

        if (servingSize.HasValue)
            recipe.ServingSize = new RecipeServingSize { Quantity = servingSize.Value, Unit = "serving" };

        if (yields.HasValue)
            recipe.Yields = new RecipeYield { Quantity = yields.Value, Unit = "meatball" };

        return recipe;
    }

    private static Recipe BuildRecipeWithGroups()
    {
        return new Recipe
        {
            Name = "Layered Recipe",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = "Meat",
                    Items =
                    [
                        new Ingredient { Quantity = 907, Unit = "g", Name = "Ground Beef" },
                    ]
                },
                new IngredientGroup
                {
                    Heading = "Spices",
                    Items =
                    [
                        new Ingredient { Quantity = 5, Unit = "g", Name = "Fine Sea Salt" },
                        new Ingredient { Quantity = 1.5, Unit = "g", Name = "Cumin" },
                    ]
                }
            ]
        };
    }

    private readonly FitnessExportService _service = new();

    // ── MyFitnessPal ──────────────────────────────────────────────────────────

    [Fact]
    public void GenerateMyFitnessPalExport_IncludesRecipeName()
    {
        // Arrange
        var recipe = BuildRecipe("Chicken Shawarma");

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("Name: Chicken Shawarma", result);
    }

    [Fact]
    public void GenerateMyFitnessPalExport_IncludesAllIngredients()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("400 g Ground Beef", result);
        Assert.Contains("3 g Black Pepper", result);
    }

    [Fact]
    public void GenerateMyFitnessPalExport_DefaultsToOneServing_WhenNoYieldsOrServingSize()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 1", result);
    }

    [Fact]
    public void GenerateMyFitnessPalExport_UsesServingSizeWhenPresent()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 4);

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 4", result);
    }

    [Fact]
    public void GenerateMyFitnessPalExport_UsesYieldsWhenNoServingSize()
    {
        // Arrange
        var recipe = BuildRecipe(yields: 12);

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 12", result);
    }

    [Fact]
    public void GenerateMyFitnessPalExport_PrefersServingSizeOverYields()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 4, yields: 12);

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 4", result);
    }

    [Fact]
    public void GenerateMyFitnessPalExport_IncludesGroupHeadings()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("# Meat", result);
        Assert.Contains("# Spices", result);
    }

    [Fact]
    public void GenerateMyFitnessPalExport_FormatsDecimalQuantityCorrectly()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateMyFitnessPalExport(recipe);

        // Assert
        Assert.Contains("1.5 g Cumin", result);
    }

    // ── Lose It! ──────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateLoseItExport_IncludesRecipeName()
    {
        // Arrange
        var recipe = BuildRecipe("Kebab Meatballs");

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("Recipe: Kebab Meatballs", result);
    }

    [Fact]
    public void GenerateLoseItExport_IncludesAllIngredients()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("Ground Beef, 400 g", result);
        Assert.Contains("Black Pepper, 3 g", result);
    }

    [Fact]
    public void GenerateLoseItExport_DefaultsToOneServing_WhenNoYieldsOrServingSize()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("Servings: 1", result);
    }

    [Fact]
    public void GenerateLoseItExport_UsesServingSizeWhenPresent()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 6);

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("Servings: 6", result);
    }

    [Fact]
    public void GenerateLoseItExport_UsesYieldsWhenNoServingSize()
    {
        // Arrange
        var recipe = BuildRecipe(yields: 8);

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("Servings: 8", result);
    }

    [Fact]
    public void GenerateLoseItExport_IncludesGroupHeadings()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("# Meat", result);
        Assert.Contains("# Spices", result);
    }

    [Fact]
    public void GenerateLoseItExport_FormatsDecimalQuantityCorrectly()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("Cumin, 1.5 g", result);
    }

    [Fact]
    public void GenerateLoseItExport_FormatsWholeNumberQuantityWithoutDecimal()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateLoseItExport(recipe);

        // Assert
        Assert.Contains("Ground Beef, 400 g", result);
        Assert.DoesNotContain("400.0 g", result);
    }
}
