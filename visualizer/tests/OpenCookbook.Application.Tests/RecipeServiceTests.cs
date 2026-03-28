using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class RecipeServiceTests
{
    private static RecipeService CreateService(IReadOnlyList<RecipeIndex> index)
    {
        var repo = new StubIndexRepository(index);
        return new RecipeService(repo);
    }

    private static RecipeIndex MakeEntry(string name, string[] tags, string[] ingredients) =>
        new()
        {
            Name = name,
            Path = $"{name.Replace(' ', '_')}.yaml",
            Status = RecipeStatus.Stable,
            Description = string.Empty,
            Tags = [.. tags],
            Ingredients = [.. ingredients]
        };

    // ── GetAllRecipesAsync ──────────────────────────────

    [Fact]
    public async Task GetAllRecipesAsync_ReturnsAllEntries()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled", "beef"], ["Ground Beef", "Onion"]),
            MakeEntry("Mashed Potatoes", ["vegetarian", "side dish"], ["Yellow Potatoes"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.GetAllRecipesAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ── SearchRecipesAsync — empty/whitespace query ─────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchRecipesAsync_EmptyQuery_ReturnsAllRecipes(string query)
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled"], ["Ground Beef"]),
            MakeEntry("Mashed Potatoes", ["vegetarian"], ["Yellow Potatoes"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync(query);

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ── SearchRecipesAsync — search by ingredient ───────

    [Fact]
    public async Task SearchRecipesAsync_IngredientExactMatch_ReturnsMatchingRecipes()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled"], ["Ground Beef", "Onion", "Garlic"]),
            MakeEntry("Mashed Potatoes", ["vegetarian"], ["Yellow Potatoes", "Garlic", "Cream"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("Garlic");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "Kebab Meat");
        Assert.Contains(result, r => r.Name == "Mashed Potatoes");
    }

    [Fact]
    public async Task SearchRecipesAsync_IngredientPartialMatch_ReturnsMatchingRecipes()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled"], ["Ground Beef", "Fresh Garlic"]),
            MakeEntry("Shawarma Chicken", ["chicken"], ["Chicken Thighs", "Garlic Powder"]),
            MakeEntry("Mashed Potatoes", ["vegetarian"], ["Yellow Potatoes"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("garlic");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "Kebab Meat");
        Assert.Contains(result, r => r.Name == "Shawarma Chicken");
        Assert.DoesNotContain(result, r => r.Name == "Mashed Potatoes");
    }

    [Fact]
    public async Task SearchRecipesAsync_IngredientSearch_IsCaseInsensitive()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Shawarma Chicken", ["chicken"], ["Chicken Thighs", "Garlic Powder"])
        };
        var service = CreateService(index);

        // Act
        var resultLower = await service.SearchRecipesAsync("chicken thighs");
        var resultUpper = await service.SearchRecipesAsync("CHICKEN THIGHS");

        // Assert
        Assert.Single(resultLower);
        Assert.Single(resultUpper);
    }

    // ── SearchRecipesAsync — search by tag ──────────────

    [Fact]
    public async Task SearchRecipesAsync_TagExactMatch_ReturnsMatchingRecipes()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled", "beef", "middle eastern"], ["Ground Beef"]),
            MakeEntry("Kebab Meatballs", ["grilled", "beef", "freezer ready"], ["Ground Beef"]),
            MakeEntry("Mashed Potatoes", ["vegetarian", "side dish"], ["Yellow Potatoes"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("grilled");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "Kebab Meat");
        Assert.Contains(result, r => r.Name == "Kebab Meatballs");
        Assert.DoesNotContain(result, r => r.Name == "Mashed Potatoes");
    }

    [Fact]
    public async Task SearchRecipesAsync_TagSearch_IsCaseInsensitive()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kiev Cutlet", ["chicken", "freezer ready"], ["Chicken Thighs"])
        };
        var service = CreateService(index);

        // Act
        var resultLower = await service.SearchRecipesAsync("freezer ready");
        var resultUpper = await service.SearchRecipesAsync("FREEZER READY");

        // Assert
        Assert.Single(resultLower);
        Assert.Single(resultUpper);
    }

    [Fact]
    public async Task SearchRecipesAsync_TagPartialMatch_ReturnsMatchingRecipes()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kiev Cutlet", ["chicken", "freezer ready"], ["Chicken Thighs"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("freezer");

        // Assert
        Assert.Single(result);
        Assert.Equal("Kiev Cutlet", result[0].Name);
    }

    // ── SearchRecipesAsync — search by name ─────────────

    [Fact]
    public async Task SearchRecipesAsync_NameExactMatch_ReturnsMatchingRecipe()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled"], ["Ground Beef"]),
            MakeEntry("Mashed Potatoes", ["vegetarian"], ["Yellow Potatoes"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("Kebab Meat");

        // Assert
        Assert.Single(result);
        Assert.Equal("Kebab Meat", result[0].Name);
    }

    [Fact]
    public async Task SearchRecipesAsync_NamePartialMatch_ReturnsMatchingRecipes()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled"], ["Ground Beef"]),
            MakeEntry("Kebab Meatballs", ["grilled"], ["Ground Beef"]),
            MakeEntry("Mashed Potatoes", ["vegetarian"], ["Yellow Potatoes"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("Kebab");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "Kebab Meat");
        Assert.Contains(result, r => r.Name == "Kebab Meatballs");
        Assert.DoesNotContain(result, r => r.Name == "Mashed Potatoes");
    }

    [Fact]
    public async Task SearchRecipesAsync_NameSearch_IsCaseInsensitive()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Chicken Shawarma", ["chicken"], ["Chicken Thighs"])
        };
        var service = CreateService(index);

        // Act
        var resultLower = await service.SearchRecipesAsync("chicken shawarma");
        var resultUpper = await service.SearchRecipesAsync("CHICKEN SHAWARMA");

        // Assert
        Assert.Single(resultLower);
        Assert.Single(resultUpper);
    }

    [Fact]
    public async Task SearchRecipesAsync_NameSearch_ReturnsExactMatchFirst()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meatballs", ["grilled"], ["Ground Beef"]),
            MakeEntry("Kebab Meat", ["grilled"], ["Ground Beef"]),
            MakeEntry("Spicy Kebab Meat Sauce", ["grilled"], ["Ground Beef"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("Kebab Meat");

        // Assert — exact match is first
        Assert.Equal(3, result.Count);
        Assert.Equal("Kebab Meat", result[0].Name);
    }

    [Fact]
    public async Task SearchRecipesAsync_NameSearch_ReturnsStartsWithBeforeContains()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Spicy Kebab", ["grilled"], ["Ground Beef"]),
            MakeEntry("Kebab Meat", ["grilled"], ["Ground Beef"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("Kebab");

        // Assert — starts-with match comes before contains match
        Assert.Equal(2, result.Count);
        Assert.Equal("Kebab Meat", result[0].Name);
        Assert.Equal("Spicy Kebab", result[1].Name);
    }

    // ── SearchRecipesAsync — no results ─────────────────

    [Fact]
    public async Task SearchRecipesAsync_NoMatch_ReturnsEmptyList()
    {
        // Arrange
        var index = new[]
        {
            MakeEntry("Kebab Meat", ["grilled", "beef"], ["Ground Beef", "Onion"]),
            MakeEntry("Mashed Potatoes", ["vegetarian"], ["Yellow Potatoes"])
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("sushi");

        // Assert
        Assert.Empty(result);
    }

    // ── SearchRecipesAsync — recipe with no tags or ingredients ──

    [Fact]
    public async Task SearchRecipesAsync_RecipeWithNoTagsOrIngredients_DoesNotThrow()
    {
        // Arrange
        var index = new[]
        {
            new RecipeIndex { Name = "Empty Recipe", Path = "Empty.yaml", Tags = [], Ingredients = [] }
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("garlic");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchRecipesAsync_RecipeWithNullTagsAndIngredients_DoesNotThrow()
    {
        // Arrange — simulate a legacy JSON entry where both fields deserialize as null
        var index = new[]
        {
            new RecipeIndex { Name = "Legacy Recipe", Path = "Legacy.yaml", Tags = null!, Ingredients = null! }
        };
        var service = CreateService(index);

        // Act
        var result = await service.SearchRecipesAsync("garlic");

        // Assert
        Assert.Empty(result);
    }

    // ── Stub ─────────────────────────────────────────────

    private sealed class StubIndexRepository(IReadOnlyList<RecipeIndex> index) : IRecipeRepository
    {
        public Task<IReadOnlyList<RecipeIndex>> GetRecipeIndexAsync() =>
            Task.FromResult(index);

        public Task<Recipe> GetRecipeAsync(string path) =>
            throw new NotImplementedException();

        public Task<Recipe> GetRecipeFromUrlAsync(string absoluteUrl) =>
            throw new NotImplementedException();
    }
}
