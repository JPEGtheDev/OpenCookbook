using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Domain.Tests;

public class NutritionEntryTests
{
    [Fact]
    public void NutritionEntry_DefaultId_IsEmpty()
    {
        // Arrange & Act
        var entry = new NutritionEntry();

        // Assert
        Assert.Equal(string.Empty, entry.Id);
    }

    [Fact]
    public void NutritionEntry_DefaultName_IsEmpty()
    {
        // Arrange & Act
        var entry = new NutritionEntry();

        // Assert
        Assert.Equal(string.Empty, entry.Name);
    }

    [Fact]
    public void NutritionEntry_DefaultAliases_IsEmpty()
    {
        // Arrange & Act
        var entry = new NutritionEntry();

        // Assert
        Assert.Empty(entry.Aliases);
    }

    [Fact]
    public void NutritionEntry_DefaultPer100g_IsNotNull()
    {
        // Arrange & Act
        var entry = new NutritionEntry();

        // Assert
        Assert.NotNull(entry.Per100g);
    }

    [Fact]
    public void NutritionEntry_SetValues_ReturnsSetValues()
    {
        // Arrange
        var entry = new NutritionEntry
        {
            Id = "ground-beef",
            Name = "Ground Beef",
            Aliases = ["beef", "minced beef"],
            Per100g = new NutrientInfo { CaloriesKcal = 215 }
        };

        // Assert
        Assert.Equal("ground-beef", entry.Id);
        Assert.Equal("Ground Beef", entry.Name);
        Assert.Equal(2, entry.Aliases.Count);
        Assert.Equal(215, entry.Per100g.CaloriesKcal);
    }
}
