using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Domain.Tests;

public class RecipeNutritionTests
{
    [Fact]
    public void RecipeNutrition_DefaultTotalNutrients_IsNotNull()
    {
        // Arrange & Act
        var nutrition = new RecipeNutrition();

        // Assert
        Assert.NotNull(nutrition.TotalNutrients);
    }

    [Fact]
    public void RecipeNutrition_DefaultPerServingNutrients_IsNull()
    {
        // Arrange & Act
        var nutrition = new RecipeNutrition();

        // Assert
        Assert.Null(nutrition.PerServingNutrients);
    }

    [Fact]
    public void RecipeNutrition_DefaultServings_IsZero()
    {
        // Arrange & Act
        var nutrition = new RecipeNutrition();

        // Assert
        Assert.Equal(0, nutrition.Servings);
    }

    [Fact]
    public void RecipeNutrition_DefaultIngredients_IsEmpty()
    {
        // Arrange & Act
        var nutrition = new RecipeNutrition();

        // Assert
        Assert.Empty(nutrition.Ingredients);
    }

    [Fact]
    public void RecipeNutrition_DefaultMissingIngredients_IsEmpty()
    {
        // Arrange & Act
        var nutrition = new RecipeNutrition();

        // Assert
        Assert.Empty(nutrition.MissingIngredients);
    }

    [Fact]
    public void RecipeNutrition_IsComplete_TrueWhenNoMissingIngredients()
    {
        // Arrange & Act
        var nutrition = new RecipeNutrition();

        // Assert
        Assert.True(nutrition.IsComplete);
    }

    [Fact]
    public void RecipeNutrition_IsComplete_FalseWhenHasMissingIngredients()
    {
        // Arrange
        var nutrition = new RecipeNutrition();
        nutrition.MissingIngredients.Add("Unknown Spice");

        // Act
        var result = nutrition.IsComplete;

        // Assert
        Assert.False(result);
    }
}
