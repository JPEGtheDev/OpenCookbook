using System.Globalization;
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

    // ── Qty-first (quantity unit name) ────────────────────────────────────────

    [Fact]
    public void GenerateQtyFirstExport_IncludesRecipeName()
    {
        // Arrange
        var recipe = BuildRecipe("Chicken Shawarma");

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("Name: Chicken Shawarma", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_IncludesAllIngredients()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("400 g Ground Beef", result);
        Assert.Contains("3 g Black Pepper", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_DefaultsToOneServing_WhenNoYieldsOrServingSize()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 1", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_UsesServingSizeWhenPresent()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 4);

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 4", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_UsesYieldsWhenNoServingSize()
    {
        // Arrange
        var recipe = BuildRecipe(yields: 12);

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 12", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_PrefersServingSizeOverYields()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 4, yields: 12);

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("Number of Servings: 4", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_IncludesGroupHeadings()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("# Meat", result);
        Assert.Contains("# Spices", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_FormatsDecimalQuantityCorrectly()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateQtyFirstExport(recipe);

        // Assert
        Assert.Contains("1.5 g Cumin", result);
    }

    [Fact]
    public void GenerateQtyFirstExport_UsesInvariantDecimalSeparator_InCommaCulture()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            // Act
            var result = _service.GenerateQtyFirstExport(recipe);

            // Assert — decimal must use '.' regardless of locale
            Assert.Contains("1.5 g Cumin", result);
            Assert.DoesNotContain("1,5 g Cumin", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    // ── Name-first (name, quantity unit) ──────────────────────────────────────

    [Fact]
    public void GenerateNameFirstExport_IncludesRecipeName()
    {
        // Arrange
        var recipe = BuildRecipe("Kebab Meatballs");

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("Recipe: Kebab Meatballs", result);
    }

    [Fact]
    public void GenerateNameFirstExport_IncludesAllIngredients()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("Ground Beef, 400 g", result);
        Assert.Contains("Black Pepper, 3 g", result);
    }

    [Fact]
    public void GenerateNameFirstExport_DefaultsToOneServing_WhenNoYieldsOrServingSize()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("Servings: 1", result);
    }

    [Fact]
    public void GenerateNameFirstExport_UsesServingSizeWhenPresent()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 6);

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("Servings: 6", result);
    }

    [Fact]
    public void GenerateNameFirstExport_UsesYieldsWhenNoServingSize()
    {
        // Arrange
        var recipe = BuildRecipe(yields: 8);

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("Servings: 8", result);
    }

    [Fact]
    public void GenerateNameFirstExport_IncludesGroupHeadings()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("# Meat", result);
        Assert.Contains("# Spices", result);
    }

    [Fact]
    public void GenerateNameFirstExport_FormatsDecimalQuantityCorrectly()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("Cumin, 1.5 g", result);
    }

    [Fact]
    public void GenerateNameFirstExport_FormatsWholeNumberQuantityWithoutDecimal()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = _service.GenerateNameFirstExport(recipe);

        // Assert
        Assert.Contains("Ground Beef, 400 g", result);
        Assert.DoesNotContain("400.0 g", result);
    }

    [Fact]
    public void GenerateNameFirstExport_UsesInvariantDecimalSeparator_InCommaCulture()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            // Act
            var result = _service.GenerateNameFirstExport(recipe);

            // Assert — decimal must use '.' regardless of locale
            Assert.Contains("Cumin, 1.5 g", result);
            Assert.DoesNotContain("Cumin, 1,5 g", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}

