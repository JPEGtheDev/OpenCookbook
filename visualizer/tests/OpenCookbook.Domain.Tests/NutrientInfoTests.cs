using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Domain.Tests;

public class NutrientInfoTests
{
    [Fact]
    public void NutrientInfo_DefaultCaloriesKcal_IsZero()
    {
        // Arrange & Act
        var info = new NutrientInfo();

        // Assert
        Assert.Equal(0, info.CaloriesKcal);
    }

    [Fact]
    public void NutrientInfo_DefaultProteinG_IsZero()
    {
        // Arrange & Act
        var info = new NutrientInfo();

        // Assert
        Assert.Equal(0, info.ProteinG);
    }

    [Fact]
    public void NutrientInfo_DefaultFatG_IsZero()
    {
        // Arrange & Act
        var info = new NutrientInfo();

        // Assert
        Assert.Equal(0, info.FatG);
    }

    [Fact]
    public void NutrientInfo_DefaultCarbsG_IsZero()
    {
        // Arrange & Act
        var info = new NutrientInfo();

        // Assert
        Assert.Equal(0, info.CarbsG);
    }

    [Fact]
    public void NutrientInfo_SetValues_ReturnsSetValues()
    {
        // Arrange
        var info = new NutrientInfo
        {
            CaloriesKcal = 215,
            ProteinG = 20.0,
            FatG = 15.0,
            CarbsG = 0.0
        };

        // Assert
        Assert.Equal(215, info.CaloriesKcal);
        Assert.Equal(20.0, info.ProteinG);
        Assert.Equal(15.0, info.FatG);
        Assert.Equal(0.0, info.CarbsG);
    }
}
