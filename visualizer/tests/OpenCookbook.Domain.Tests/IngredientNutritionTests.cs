using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Domain.Tests;

public class IngredientNutritionTests
{
    [Fact]
    public void IngredientNutrition_DefaultIngredientName_IsEmpty()
    {
        // Arrange & Act
        var item = new IngredientNutrition();

        // Assert
        Assert.Equal(string.Empty, item.IngredientName);
    }

    [Fact]
    public void IngredientNutrition_DefaultQuantityG_IsZero()
    {
        // Arrange & Act
        var item = new IngredientNutrition();

        // Assert
        Assert.Equal(0, item.QuantityG);
    }

    [Fact]
    public void IngredientNutrition_DefaultIsMatch_IsFalse()
    {
        // Arrange & Act
        var item = new IngredientNutrition();

        // Assert
        Assert.False(item.IsMatch);
    }

    [Fact]
    public void IngredientNutrition_DefaultNutrients_IsNull()
    {
        // Arrange & Act
        var item = new IngredientNutrition();

        // Assert
        Assert.Null(item.Nutrients);
    }

    [Fact]
    public void IngredientNutrition_SetValues_ReturnsSetValues()
    {
        // Arrange
        var item = new IngredientNutrition
        {
            IngredientName = "Ground Beef",
            QuantityG = 500,
            IsMatch = true,
            Nutrients = new NutrientInfo { CaloriesKcal = 1075 }
        };

        // Assert
        Assert.Equal("Ground Beef", item.IngredientName);
        Assert.Equal(500, item.QuantityG);
        Assert.True(item.IsMatch);
        Assert.Equal(1075, item.Nutrients!.CaloriesKcal);
    }
}
