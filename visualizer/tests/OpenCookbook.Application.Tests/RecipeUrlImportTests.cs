using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

/// <summary>
/// Tests for remote URL-based recipe loading: TryExtractLocalPath and GetRecipeByPathAsync
/// routing to GetRecipeFromUrlAsync.
/// </summary>
public class RecipeUrlImportTests
{
    private const string AppBase = "https://jpegthedev.github.io/OpenCookbook/";

    // ── TryExtractLocalPath ───────────────────────────────────────────────────

    [Fact]
    public void TryExtractLocalPath_SameSiteRecipeUrl_ExtractsPath()
    {
        // Arrange
        var url = "https://jpegthedev.github.io/OpenCookbook/recipe/Shawarma_Like_Chicken.yaml";

        // Act
        var found = RecipeService.TryExtractLocalPath(url, AppBase, out var localPath);

        // Assert
        Assert.True(found);
        Assert.Equal("Shawarma_Like_Chicken.yaml", localPath);
    }

    [Fact]
    public void TryExtractLocalPath_SameSiteRecipeUrlWithSubfolder_ExtractsFullPath()
    {
        // Arrange
        var url = "https://jpegthedev.github.io/OpenCookbook/recipe/Grilling/Kebab_Meat.yaml";

        // Act
        var found = RecipeService.TryExtractLocalPath(url, AppBase, out var localPath);

        // Assert
        Assert.True(found);
        Assert.Equal("Grilling/Kebab_Meat.yaml", localPath);
    }

    [Fact]
    public void TryExtractLocalPath_SameSiteRecipeUrl_DecodesPercentEncoding()
    {
        // Arrange
        var url = "https://jpegthedev.github.io/OpenCookbook/recipe/Grilling%2FKebab_Meat.yaml";

        // Act
        var found = RecipeService.TryExtractLocalPath(url, AppBase, out var localPath);

        // Assert
        Assert.True(found);
        Assert.Equal("Grilling/Kebab_Meat.yaml", localPath);
    }

    [Fact]
    public void TryExtractLocalPath_ExternalUrl_ReturnsFalse()
    {
        // Arrange
        var url = "https://example.com/recipes/Chicken.yaml";

        // Act
        var found = RecipeService.TryExtractLocalPath(url, AppBase, out var localPath);

        // Assert
        Assert.False(found);
        Assert.Empty(localPath);
    }

    [Fact]
    public void TryExtractLocalPath_RelativePath_ReturnsFalse()
    {
        // Arrange
        var url = "Grilling/Kebab_Meat.yaml";

        // Act
        var found = RecipeService.TryExtractLocalPath(url, AppBase, out var localPath);

        // Assert
        Assert.False(found);
        Assert.Empty(localPath);
    }

    [Fact]
    public void TryExtractLocalPath_EmptyInput_ReturnsFalse()
    {
        var found = RecipeService.TryExtractLocalPath(string.Empty, AppBase, out var localPath);

        Assert.False(found);
        Assert.Empty(localPath);
    }

    [Fact]
    public void TryExtractLocalPath_NullInput_ReturnsFalse()
    {
        var found = RecipeService.TryExtractLocalPath(null!, AppBase, out var localPath);

        Assert.False(found);
        Assert.Empty(localPath);
    }

    [Fact]
    public void TryExtractLocalPath_IsCaseInsensitive()
    {
        // Arrange — uppercase scheme and host
        var url = "HTTPS://JPEGTHEDEV.GITHUB.IO/OpenCookbook/recipe/Chicken.yaml";

        // Act
        var found = RecipeService.TryExtractLocalPath(url, AppBase, out var localPath);

        // Assert
        Assert.True(found);
        Assert.Equal("Chicken.yaml", localPath);
    }

    // ── GetRecipeByPathAsync routing ──────────────────────────────────────────

    [Fact]
    public async Task GetRecipeByPathAsync_RelativePath_CallsGetRecipeAsync()
    {
        // Arrange
        var recipe = BuildRecipe("Chicken");
        var repo = new FakeRecipeRepository(new Dictionary<string, Recipe>
        {
            ["Chicken.yaml"] = recipe
        });
        var service = new RecipeService(repo);

        // Act
        var result = await service.GetRecipeByPathAsync("Chicken.yaml");

        // Assert
        Assert.Equal("Chicken", result.Name);
    }

    [Fact]
    public async Task GetRecipeByPathAsync_AbsoluteHttpsUrl_CallsGetRecipeFromUrlAsync()
    {
        // Arrange
        var absoluteUrl = "https://example.com/recipes/Chicken.yaml";
        var recipe = BuildRecipe("Chicken");
        var repo = new FakeRecipeRepository(new Dictionary<string, Recipe>
        {
            [absoluteUrl] = recipe   // FakeRecipeRepository matches URLs by key in GetRecipeFromUrlAsync
        });
        var service = new RecipeService(repo);

        // Act
        var result = await service.GetRecipeByPathAsync(absoluteUrl);

        // Assert
        Assert.Equal("Chicken", result.Name);
    }

    [Fact]
    public async Task GetRecipeByPathAsync_AbsoluteUrl_NotFound_ThrowsKeyNotFound()
    {
        // Arrange
        var repo = new FakeRecipeRepository(new Dictionary<string, Recipe>());
        var service = new RecipeService(repo);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetRecipeByPathAsync("https://example.com/recipes/Missing.yaml"));
    }

    private static Recipe BuildRecipe(string name) => new()
    {
        Name = name,
        Ingredients = []
    };
}
