using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Domain.Tests;

public class RecipeEntityTests
{
    [Fact]
    public void Recipe_DefaultValues_AreCorrect()
    {
        var recipe = new Recipe();

        Assert.Equal(string.Empty, recipe.Name);
        Assert.Equal(string.Empty, recipe.Version);
        Assert.Equal(string.Empty, recipe.Author);
        Assert.Equal(string.Empty, recipe.Description);
        Assert.Equal(RecipeStatus.Draft, recipe.Status);
        Assert.Empty(recipe.Ingredients);
        Assert.Null(recipe.Utensils);
        Assert.Empty(recipe.Instructions);
        Assert.Null(recipe.Related);
        Assert.Null(recipe.Notes);
    }

    [Fact]
    public void Recipe_CanSetAllProperties()
    {
        var recipe = new Recipe
        {
            Name = "Test Recipe",
            Version = "1.0",
            Author = "Test Author | Handle",
            Description = "A test recipe",
            Status = RecipeStatus.Stable,
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items = [new Ingredient { Quantity = 100, Unit = "g", Name = "Flour" }]
                }
            ],
            Instructions =
            [
                new Section
                {
                    Heading = null,
                    Type = SectionType.Sequence,
                    Steps = [new Step { Text = "Mix ingredients" }]
                }
            ]
        };

        Assert.Equal("Test Recipe", recipe.Name);
        Assert.Equal(RecipeStatus.Stable, recipe.Status);
        Assert.Single(recipe.Ingredients);
        Assert.Single(recipe.Instructions);
    }

    [Fact]
    public void Ingredient_VolumeAlt_IsOptional()
    {
        var withAlt = new Ingredient
        {
            Quantity = 3,
            Unit = "g",
            Name = "Black Pepper",
            VolumeAlt = "3/4 tsp."
        };

        var withoutAlt = new Ingredient
        {
            Quantity = 100,
            Unit = "g",
            Name = "Flour"
        };

        Assert.Equal("3/4 tsp.", withAlt.VolumeAlt);
        Assert.Null(withoutAlt.VolumeAlt);
    }

    [Fact]
    public void Section_BranchType_HasBranchGroup()
    {
        var branch = new Section
        {
            Heading = "Grilled",
            Type = SectionType.Branch,
            BranchGroup = "cooking-method",
            Steps = [new Step { Text = "Preheat grill" }]
        };

        Assert.Equal(SectionType.Branch, branch.Type);
        Assert.Equal("cooking-method", branch.BranchGroup);
    }

    [Fact]
    public void Step_Notes_IsOptional()
    {
        var withNotes = new Step
        {
            Text = "Bake for 20 minutes",
            Notes = ["Check after 15 minutes"]
        };

        var withoutNotes = new Step { Text = "Mix ingredients" };

        Assert.Single(withNotes.Notes!);
        Assert.Null(withoutNotes.Notes);
    }

    [Theory]
    [InlineData(RecipeStatus.Stable)]
    [InlineData(RecipeStatus.Beta)]
    [InlineData(RecipeStatus.Draft)]
    public void RecipeStatus_AllValues_AreDefined(RecipeStatus status)
    {
        Assert.True(Enum.IsDefined(status));
    }
}
