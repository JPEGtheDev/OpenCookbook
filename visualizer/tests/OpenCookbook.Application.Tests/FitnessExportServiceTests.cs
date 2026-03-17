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

    // ── GenerateExportAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateExport_IncludesRecipeName()
    {
        // Arrange
        var recipe = BuildRecipe("Chicken Shawarma");

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("Name: Chicken Shawarma", result);
    }

    [Fact]
    public async Task GenerateExport_IncludesAllIngredients()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("400 g Ground Beef", result);
        Assert.Contains("3 g Black Pepper", result);
    }

    [Fact]
    public async Task GenerateExport_DefaultsToOneServing_WhenNoYieldsOrServingSize()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("Servings: 1", result);
    }

    [Fact]
    public async Task GenerateExport_ShowsServingSizeOnly_WhenNoYields()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 4);

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("Serving Size: 4 serving", result);
        Assert.DoesNotContain("Yield:", result);
        Assert.DoesNotContain("Servings:", result);
    }

    [Fact]
    public async Task GenerateExport_ShowsYield_WhenNoServingSize()
    {
        // Arrange
        var recipe = BuildRecipe(yields: 12);

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("Yield: 12 meatball", result);
        Assert.DoesNotContain("Serving Size:", result);
    }

    [Fact]
    public async Task GenerateExport_ShowsYieldAndServingSize_WhenBothPresent()
    {
        // Arrange
        var recipe = BuildRecipe(servingSize: 4, yields: 24);

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("Yield: 24 meatball", result);
        Assert.Contains("Serving Size: 4 serving", result);
        Assert.DoesNotContain("Servings:", result);
    }

    [Fact]
    public async Task GenerateExport_IncludesGroupHeadings()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("# Meat", result);
        Assert.Contains("# Spices", result);
    }

    [Fact]
    public async Task GenerateExport_FormatsDecimalQuantityCorrectly()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("1.5 g Cumin", result);
    }

    [Fact]
    public async Task GenerateExport_FormatsWholeNumberQuantityWithoutDecimal()
    {
        // Arrange
        var recipe = BuildRecipe();

        // Act
        var result = await _service.GenerateExportAsync(recipe);

        // Assert
        Assert.Contains("400 g Ground Beef", result);
        Assert.DoesNotContain("400.0 g", result);
    }

    [Fact]
    public async Task GenerateExport_UsesInvariantDecimalSeparator_InCommaCulture()
    {
        // Arrange
        var recipe = BuildRecipeWithGroups();
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            // Act
            var result = await _service.GenerateExportAsync(recipe);

            // Assert — decimal must use '.' regardless of locale
            Assert.Contains("1.5 g Cumin", result);
            Assert.DoesNotContain("1,5 g Cumin", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    // ── Sub-recipe inlining ────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateExport_InlinesSubRecipeIngredients_WhenDocLinkPresent()
    {
        // Arrange
        var subRecipe = new Recipe
        {
            Name = "Kebab Meat",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 907, Unit = "g", Name = "Ground Beef" },
                        new Ingredient { Quantity = 7, Unit = "g", Name = "Paprika" },
                    ]
                }
            ]
        };

        var parentRecipe = new Recipe
        {
            Name = "Kebab Meatballs",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 1, Unit = "whole", Name = "Kebab Meat Recipe (full batch)", DocLink = "./Kebab_Meat.yaml" },
                        new Ingredient { Quantity = 40, Unit = "g", Name = "Panko Bread Crumbs" },
                    ]
                }
            ]
        };

        var repo = new FakeRecipeRepository(new Dictionary<string, Recipe>
        {
            ["Kebab_Meat.yaml"] = subRecipe
        });
        var service = new FitnessExportService(repo);

        // Act
        var result = await service.GenerateExportAsync(parentRecipe);

        // Assert — sub-recipe ingredients are present
        Assert.Contains("907 g Ground Beef", result);
        Assert.Contains("7 g Paprika", result);
        Assert.Contains("40 g Panko Bread Crumbs", result);
        // Assert — the sub-recipe reference line itself is NOT present
        Assert.DoesNotContain("Kebab Meat Recipe (full batch)", result);
    }

    [Fact]
    public async Task GenerateExport_ScalesSubRecipeIngredients_ByParentQuantity()
    {
        // Arrange
        var subRecipe = new Recipe
        {
            Name = "Spice Mix",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 10, Unit = "g", Name = "Cumin" },
                    ]
                }
            ]
        };

        var parentRecipe = new Recipe
        {
            Name = "Seasoned Dish",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 2, Unit = "batch", Name = "Spice Mix", DocLink = "./Spice_Mix.yaml" },
                    ]
                }
            ]
        };

        var repo = new FakeRecipeRepository(new Dictionary<string, Recipe>
        {
            ["Spice_Mix.yaml"] = subRecipe
        });
        var service = new FitnessExportService(repo);

        // Act
        var result = await service.GenerateExportAsync(parentRecipe);

        // Assert — 2 batches × 10 g = 20 g
        Assert.Contains("20 g Cumin", result);
    }

    [Fact]
    public async Task GenerateExport_FallsBackToReferenceLine_WhenSubRecipeNotFound()
    {
        // Arrange
        var parentRecipe = new Recipe
        {
            Name = "Test Recipe",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 1, Unit = "whole", Name = "Missing Sub Recipe", DocLink = "./Missing.yaml" },
                        new Ingredient { Quantity = 50, Unit = "g", Name = "Salt" },
                    ]
                }
            ]
        };

        var repo = new FakeRecipeRepository(new Dictionary<string, Recipe>());
        var service = new FitnessExportService(repo);

        // Act
        var result = await service.GenerateExportAsync(parentRecipe);

        // Assert — falls back to showing the reference ingredient
        Assert.Contains("1 whole Missing Sub Recipe", result);
        Assert.Contains("50 g Salt", result);
    }

    [Fact]
    public async Task GenerateExport_ListsDocLinkIngredientAsIs_WhenNoRepoProvided()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "Test Recipe",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 1, Unit = "whole", Name = "Kebab Meat Recipe", DocLink = "./Kebab_Meat.yaml" },
                        new Ingredient { Quantity = 40, Unit = "g", Name = "Panko" },
                    ]
                }
            ]
        };

        // No repo injected — service created without one
        var service = new FitnessExportService();

        // Act
        var result = await service.GenerateExportAsync(recipe);

        // Assert — without a repo, shows the reference line as-is
        Assert.Contains("1 whole Kebab Meat Recipe", result);
        Assert.Contains("40 g Panko", result);
    }
}

