using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Domain.Tests;

public class RecipeEntityTests
{
    [Fact]
    public void Recipe_DefaultName_IsEmpty()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Equal(string.Empty, recipe.Name);
    }

    [Fact]
    public void Recipe_DefaultVersion_IsEmpty()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Equal(string.Empty, recipe.Version);
    }

    [Fact]
    public void Recipe_DefaultAuthor_IsEmpty()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Equal(string.Empty, recipe.Author);
    }

    [Fact]
    public void Recipe_DefaultDescription_IsEmpty()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Equal(string.Empty, recipe.Description);
    }

    [Fact]
    public void Recipe_DefaultStatus_IsDraft()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Equal(RecipeStatus.Draft, recipe.Status);
    }

    [Fact]
    public void Recipe_DefaultIngredients_IsEmpty()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Empty(recipe.Ingredients);
    }

    [Fact]
    public void Recipe_DefaultUtensils_IsNull()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Null(recipe.Utensils);
    }

    [Fact]
    public void Recipe_DefaultInstructions_IsEmpty()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Empty(recipe.Instructions);
    }

    [Fact]
    public void Recipe_DefaultRelated_IsNull()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Null(recipe.Related);
    }

    [Fact]
    public void Recipe_DefaultNotes_IsNull()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        Assert.Null(recipe.Notes);
    }

    [Fact]
    public void Recipe_SetName_ReturnsSetValue()
    {
        // Arrange
        var recipe = new Recipe { Name = "Test Recipe" };

        // Act
        var result = recipe.Name;

        // Assert
        Assert.Equal("Test Recipe", result);
    }

    [Fact]
    public void Recipe_SetStatus_ReturnsSetValue()
    {
        // Arrange
        var recipe = new Recipe { Status = RecipeStatus.Stable };

        // Act
        var result = recipe.Status;

        // Assert
        Assert.Equal(RecipeStatus.Stable, result);
    }

    [Fact]
    public void Recipe_SetIngredients_ReturnsSetCollection()
    {
        // Arrange
        var recipe = new Recipe
        {
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items = [new Ingredient { Quantity = 100, Unit = "g", Name = "Flour" }]
                }
            ]
        };

        // Act
        var result = recipe.Ingredients;

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Recipe_SetInstructions_ReturnsSetCollection()
    {
        // Arrange
        var recipe = new Recipe
        {
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

        // Act
        var result = recipe.Instructions;

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void Ingredient_WithVolumeAlt_ReturnsSetValue()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Quantity = 3,
            Unit = "g",
            Name = "Black Pepper",
            VolumeAlt = "3/4 tsp."
        };

        // Act
        var result = ingredient.VolumeAlt;

        // Assert
        Assert.Equal("3/4 tsp.", result);
    }

    [Fact]
    public void Ingredient_WithoutVolumeAlt_DefaultsToNull()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Quantity = 100,
            Unit = "g",
            Name = "Flour"
        };

        // Act
        var result = ingredient.VolumeAlt;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Section_BranchType_ReturnsSetValue()
    {
        // Arrange
        var branch = new Section
        {
            Heading = "Grilled",
            Type = SectionType.Branch,
            BranchGroup = "cooking-method",
            Steps = [new Step { Text = "Preheat grill" }]
        };

        // Act
        var result = branch.Type;

        // Assert
        Assert.Equal(SectionType.Branch, result);
    }

    [Fact]
    public void Section_BranchGroup_ReturnsSetValue()
    {
        // Arrange
        var branch = new Section
        {
            Heading = "Grilled",
            Type = SectionType.Branch,
            BranchGroup = "cooking-method",
            Steps = [new Step { Text = "Preheat grill" }]
        };

        // Act
        var result = branch.BranchGroup;

        // Assert
        Assert.Equal("cooking-method", result);
    }

    [Fact]
    public void Step_WithNotes_ReturnsSetCollection()
    {
        // Arrange
        var step = new Step
        {
            Text = "Bake for 20 minutes",
            Notes = ["Check after 15 minutes"]
        };

        // Act
        var result = step.Notes;

        // Assert
        Assert.Single(result!);
    }

    [Fact]
    public void Step_WithoutNotes_DefaultsToNull()
    {
        // Arrange
        var step = new Step { Text = "Mix ingredients" };

        // Act
        var result = step.Notes;

        // Assert
        Assert.Null(result);
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
