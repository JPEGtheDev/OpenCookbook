using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class AlternateSelectorTests
{
    private static Recipe BuildRecipeWithAlternates()
    {
        return new Recipe
        {
            Name = "Test Recipe",
            Version = "1.0",
            Author = "Test",
            Description = "Test",
            Status = RecipeStatus.Stable,
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient
                        {
                            Quantity = 907,
                            Unit = "g",
                            Name = "88/12 Ground Beef",
                            NutritionId = Guid.Parse("2724c62f-1832-5ccf-97b0-d219812368d8"),
                            Alternates =
                            [
                                new IngredientAlternate
                                {
                                    Name = "80/20 Ground Beef",
                                    NutritionId = Guid.Parse("ca7b2dfc-90a9-57e5-995d-605dfef2baf8"),
                                },
                                new IngredientAlternate
                                {
                                    Name = "70/30 Ground Beef",
                                    Note = "Juiciest option",
                                },
                            ]
                        },
                        new Ingredient
                        {
                            Quantity = 3,
                            Unit = "g",
                            Name = "Black Pepper",
                        }
                    ]
                }
            ],
            Instructions = [],
        };
    }

    [Fact]
    public void ApplySelections_EmptySelections_ReturnsSameRecipe()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>();

        var result = AlternateSelector.ApplySelections(recipe, selections);

        Assert.Same(recipe, result);
    }

    [Fact]
    public void ApplySelections_SelectFirstAlternate_SwapsNameAndNutritionId()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = 0 // Select 80/20
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        var item = result.Ingredients[0].Items[0];
        Assert.Equal("80/20 Ground Beef", item.Name);
        Assert.Equal(Guid.Parse("ca7b2dfc-90a9-57e5-995d-605dfef2baf8"), item.NutritionId);
        Assert.Equal(907, item.Quantity); // Inherited from original
        Assert.Equal("g", item.Unit); // Inherited from original
    }

    [Fact]
    public void ApplySelections_SelectSecondAlternate_SwapsNameAndNote()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = 1 // Select 70/30
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        var item = result.Ingredients[0].Items[0];
        Assert.Equal("70/30 Ground Beef", item.Name);
        Assert.Equal("Juiciest option", item.Note);
        // NutritionId not set on alternate → falls back to original
        Assert.Equal(Guid.Parse("2724c62f-1832-5ccf-97b0-d219812368d8"), item.NutritionId);
    }

    [Fact]
    public void ApplySelections_NegativeOneIndex_KeepsDefault()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = -1
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        var item = result.Ingredients[0].Items[0];
        Assert.Equal("88/12 Ground Beef", item.Name);
    }

    [Fact]
    public void ApplySelections_OutOfRangeIndex_KeepsDefault()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = 99
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        var item = result.Ingredients[0].Items[0];
        Assert.Equal("88/12 Ground Beef", item.Name);
    }

    [Fact]
    public void ApplySelections_SelectionOnIngredientWithoutAlternates_KeepsDefault()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 1)] = 0 // Black Pepper has no alternates
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        var item = result.Ingredients[0].Items[1];
        Assert.Equal("Black Pepper", item.Name);
    }

    [Fact]
    public void ApplySelections_DoesNotMutateOriginalRecipe()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = 0
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        Assert.NotSame(recipe, result);
        Assert.Equal("88/12 Ground Beef", recipe.Ingredients[0].Items[0].Name);
        Assert.Equal("80/20 Ground Beef", result.Ingredients[0].Items[0].Name);
    }

    [Fact]
    public void ApplySelections_PreservesAlternatesListOnSwappedIngredient()
    {
        var recipe = BuildRecipeWithAlternates();
        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = 0
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        // The swapped ingredient still has its alternates so the user can switch back
        Assert.NotNull(result.Ingredients[0].Items[0].Alternates);
        Assert.Equal(2, result.Ingredients[0].Items[0].Alternates!.Count);
    }

    [Fact]
    public void ApplySelections_AlternateWithQuantityOverride_UsesOverride()
    {
        var recipe = new Recipe
        {
            Name = "Test",
            Version = "1.0",
            Author = "Test",
            Description = "Test",
            Status = RecipeStatus.Stable,
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient
                        {
                            Quantity = 100,
                            Unit = "g",
                            Name = "Ingredient A",
                            Alternates =
                            [
                                new IngredientAlternate
                                {
                                    Name = "Ingredient B",
                                    Quantity = 150,
                                    Unit = "ml",
                                }
                            ]
                        }
                    ]
                }
            ],
            Instructions = [],
        };

        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = 0
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        var item = result.Ingredients[0].Items[0];
        Assert.Equal("Ingredient B", item.Name);
        Assert.Equal(150, item.Quantity);
        Assert.Equal("ml", item.Unit);
    }

    [Fact]
    public void ApplySelections_PreservesRecipeMetadata()
    {
        var recipe = BuildRecipeWithAlternates();
        recipe.Yields = new RecipeYield { Quantity = 24, Unit = "meatball" };
        recipe.ServingSize = new RecipeServingSize { Quantity = 4, Unit = "meatball" };

        var selections = new Dictionary<AlternateSelector.IngredientKey, int>
        {
            [new AlternateSelector.IngredientKey(0, 0)] = 0
        };

        var result = AlternateSelector.ApplySelections(recipe, selections);

        Assert.Equal("Test Recipe", result.Name);
        Assert.Equal("1.0", result.Version);
        Assert.NotNull(result.Yields);
        Assert.Equal(24, result.Yields!.Quantity);
        Assert.NotNull(result.ServingSize);
        Assert.Equal(4, result.ServingSize!.Quantity);
    }
}
