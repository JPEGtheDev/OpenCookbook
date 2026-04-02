using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Application.Services;
using OpenCookbook.Domain.Entities;

namespace OpenCookbook.Application.Tests;

public class RecipeFilterTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static RecipeService CreateService(IReadOnlyList<RecipeIndex> index)
    {
        var repo = new StubFilterRepository(index);
        return new RecipeService(repo, new DietaryInferenceService());
    }

    private static RecipeIndex MakeEntry(
        string name,
        string[] tags,
        string[] ingredients) =>
        new()
        {
            Name = name,
            Path = $"{name.Replace(' ', '_')}.yaml",
            Status = RecipeStatus.Stable,
            Description = string.Empty,
            Tags = [.. tags],
            Ingredients = [.. ingredients]
        };

    private static readonly IReadOnlyList<RecipeIndex> SampleIndex =
    [
        MakeEntry("Kebab Meat",      ["beef", "grilled"],            ["Ground Beef", "Onion", "Garlic", "Salt", "Cumin"]),
        MakeEntry("Kiev Cutlet",     ["chicken", "freezer ready"],   ["Chicken Thighs", "Butter", "Garlic", "Parsley", "Flour", "Eggs"]),
        MakeEntry("Mashed Potatoes", ["vegetarian", "side dish"],    ["Yellow Potatoes", "Butter", "Heavy Cream", "Salt"]),
        MakeEntry("Tomato Bruschetta", ["vegan", "appetizer"],       ["Tomato", "Garlic", "Olive Oil", "Basil", "Salt"]),
    ];

    // ── FilterRecipesAsync — empty filter ─────────────────────────────────────

    [Fact]
    public async Task FilterRecipesAsync_EmptyFilter_ReturnsAll()
    {
        var service = CreateService(SampleIndex);
        var result = await service.FilterRecipesAsync(new RecipeFilter());
        Assert.Equal(4, result.Count);
    }

    // ── FilterRecipesAsync — text search passthrough ──────────────────────────

    [Fact]
    public async Task FilterRecipesAsync_SearchQuery_FiltersLikeSearchRecipesAsync()
    {
        var service = CreateService(SampleIndex);
        var result = await service.FilterRecipesAsync(new RecipeFilter { SearchQuery = "Kebab" });
        Assert.Single(result);
        Assert.Equal("Kebab Meat", result[0].Name);
    }

    // ── FilterRecipesAsync — ingredient filter (AND) ──────────────────────────

    [Fact]
    public async Task FilterRecipesAsync_SingleIngredient_ReturnsRecipesContainingIt()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { SelectedIngredients = ["Garlic"] };
        var result = await service.FilterRecipesAsync(filter);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.Name == "Kebab Meat");
        Assert.Contains(result, r => r.Name == "Kiev Cutlet");
        Assert.Contains(result, r => r.Name == "Tomato Bruschetta");
        Assert.DoesNotContain(result, r => r.Name == "Mashed Potatoes");
    }

    [Fact]
    public async Task FilterRecipesAsync_MultipleIngredients_RequiresAll()
    {
        var service = CreateService(SampleIndex);
        // Only Kebab Meat has both Garlic and Cumin
        var filter = new RecipeFilter { SelectedIngredients = ["Garlic", "Cumin"] };
        var result = await service.FilterRecipesAsync(filter);

        Assert.Single(result);
        Assert.Equal("Kebab Meat", result[0].Name);
    }

    [Fact]
    public async Task FilterRecipesAsync_IngredientNotInAnyRecipe_ReturnsEmpty()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { SelectedIngredients = ["Truffle Oil"] };
        var result = await service.FilterRecipesAsync(filter);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterRecipesAsync_IngredientMatch_IsCaseInsensitive()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { SelectedIngredients = ["garlic"] };
        var result = await service.FilterRecipesAsync(filter);
        Assert.Equal(3, result.Count);
    }

    // ── FilterRecipesAsync — tag filter (OR) ──────────────────────────────────

    [Fact]
    public async Task FilterRecipesAsync_SingleTag_ReturnsMatchingRecipes()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { SelectedTags = ["grilled"] };
        var result = await service.FilterRecipesAsync(filter);

        Assert.Single(result);
        Assert.Equal("Kebab Meat", result[0].Name);
    }

    [Fact]
    public async Task FilterRecipesAsync_MultipleTags_OrBehavior()
    {
        var service = CreateService(SampleIndex);
        // "beef" or "chicken"
        var filter = new RecipeFilter { SelectedTags = ["beef", "chicken"] };
        var result = await service.FilterRecipesAsync(filter);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "Kebab Meat");
        Assert.Contains(result, r => r.Name == "Kiev Cutlet");
    }

    [Fact]
    public async Task FilterRecipesAsync_TagNotPresent_ReturnsEmpty()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { SelectedTags = ["breakfast"] };
        var result = await service.FilterRecipesAsync(filter);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterRecipesAsync_TagMatch_IsCaseInsensitive()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { SelectedTags = ["GRILLED"] };
        var result = await service.FilterRecipesAsync(filter);
        Assert.Single(result);
    }

    // ── FilterRecipesAsync — dietary filters ──────────────────────────────────

    [Fact]
    public async Task FilterRecipesAsync_DairyFree_ExcludesRecipesWithDairy()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { FilterDairyFree = true };
        var result = await service.FilterRecipesAsync(filter);

        // Kiev Cutlet (butter), Mashed Potatoes (butter, heavy cream) are excluded
        Assert.DoesNotContain(result, r => r.Name == "Kiev Cutlet");
        Assert.DoesNotContain(result, r => r.Name == "Mashed Potatoes");
    }

    [Fact]
    public async Task FilterRecipesAsync_DairyFree_IncludesAllPlantBasedRecipes()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { FilterDairyFree = true };
        var result = await service.FilterRecipesAsync(filter);

        Assert.Contains(result, r => r.Name == "Tomato Bruschetta");
    }

    [Fact]
    public async Task FilterRecipesAsync_Vegetarian_ExcludesMeatRecipes()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { FilterVegetarian = true };
        var result = await service.FilterRecipesAsync(filter);

        Assert.DoesNotContain(result, r => r.Name == "Kebab Meat");
        Assert.DoesNotContain(result, r => r.Name == "Kiev Cutlet");
    }

    [Fact]
    public async Task FilterRecipesAsync_Vegetarian_IncludesDairyRecipes()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { FilterVegetarian = true };
        var result = await service.FilterRecipesAsync(filter);

        // Mashed Potatoes has dairy but is still vegetarian
        Assert.Contains(result, r => r.Name == "Mashed Potatoes");
    }

    [Fact]
    public async Task FilterRecipesAsync_Vegan_ExcludesAllAnimalDerivedRecipes()
    {
        var service = CreateService(SampleIndex);
        var filter = new RecipeFilter { FilterVegan = true };
        var result = await service.FilterRecipesAsync(filter);

        Assert.DoesNotContain(result, r => r.Name == "Kebab Meat");
        Assert.DoesNotContain(result, r => r.Name == "Kiev Cutlet");
        Assert.DoesNotContain(result, r => r.Name == "Mashed Potatoes");
        Assert.Contains(result, r => r.Name == "Tomato Bruschetta");
    }

    [Fact]
    public async Task FilterRecipesAsync_NoMatch_ReturnsEmptyList()
    {
        var service = CreateService(SampleIndex);
        // Requiring "Ground Beef" (meat) AND vegetarian filter — impossible combination
        var filter = new RecipeFilter
        {
            SelectedIngredients = ["Ground Beef"],
            FilterVegetarian = true
        };
        var result = await service.FilterRecipesAsync(filter);
        Assert.Empty(result);
    }

    // ── GetAvailableTagsAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailableTagsAsync_ReturnsAllUniqueTags_Sorted()
    {
        var service = CreateService(SampleIndex);
        var tags = await service.GetAvailableTagsAsync();

        Assert.Contains("beef", tags);
        Assert.Contains("grilled", tags);
        Assert.Contains("vegetarian", tags);
        Assert.Contains("vegan", tags);
        Assert.Contains("freezer ready", tags);

        // sorted alphabetically
        var sorted = tags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(sorted, tags.ToList());
    }

    [Fact]
    public async Task GetAvailableTagsAsync_NoDuplicates()
    {
        // Two recipes with the same tag
        var index = new[]
        {
            MakeEntry("A", ["grilled", "beef"], []),
            MakeEntry("B", ["grilled", "chicken"], [])
        };
        var service = CreateService(index);
        var tags = await service.GetAvailableTagsAsync();

        Assert.Single(tags, t => t.Equals("grilled", StringComparison.OrdinalIgnoreCase));
    }

    // ── GetAvailableIngredientsAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetAvailableIngredientsAsync_ReturnsAllUniqueIngredients_Sorted()
    {
        var service = CreateService(SampleIndex);
        var ingredients = await service.GetAvailableIngredientsAsync();

        Assert.Contains("Garlic", ingredients, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Ground Beef", ingredients, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Yellow Potatoes", ingredients, StringComparer.OrdinalIgnoreCase);

        var sorted = ingredients.OrderBy(i => i, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(sorted, ingredients.ToList());
    }

    [Fact]
    public async Task GetAvailableIngredientsAsync_NoDuplicates()
    {
        // Garlic appears in multiple recipes
        var service = CreateService(SampleIndex);
        var ingredients = await service.GetAvailableIngredientsAsync();

        Assert.Single(ingredients, i => i.Equals("Garlic", StringComparison.OrdinalIgnoreCase));
    }

    // ── stub ──────────────────────────────────────────────────────────────────

    private sealed class StubFilterRepository(IReadOnlyList<RecipeIndex> index) : IRecipeRepository
    {
        public Task<IReadOnlyList<RecipeIndex>> GetRecipeIndexAsync() => Task.FromResult(index);

        public Task<Recipe> GetRecipeAsync(string path) =>
            throw new NotImplementedException();
    }
}
