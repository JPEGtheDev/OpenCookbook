using OpenCookbook.Application.Services;

namespace OpenCookbook.Application.Tests;

public class DietaryInferenceServiceTests
{
    private readonly DietaryInferenceService _sut = new();

    // ── Classify ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Ground Beef")]
    [InlineData("Chicken Thighs")]
    [InlineData("Pork Shoulder")]
    [InlineData("Salmon Fillet")]
    [InlineData("Shrimp")]
    [InlineData("Turkey Breast")]
    [InlineData("Lamb Chops")]
    [InlineData("Duck Legs")]
    public void Classify_MeatIngredients_ReturnsMeat(string name)
    {
        Assert.Equal(IngredientCategory.Meat, _sut.Classify(name));
    }

    [Theory]
    [InlineData("Whole Milk")]
    [InlineData("Cheddar Cheese")]
    [InlineData("Heavy Cream")]
    [InlineData("Butter")]
    [InlineData("Sour Cream")]
    [InlineData("Mozzarella")]
    [InlineData("Parmesan")]
    [InlineData("Yogurt")]
    public void Classify_DairyIngredients_ReturnsDairy(string name)
    {
        Assert.Equal(IngredientCategory.Dairy, _sut.Classify(name));
    }

    [Theory]
    [InlineData("Eggs")]
    [InlineData("Egg Whites")]
    [InlineData("Honey")]
    [InlineData("Gelatin")]
    [InlineData("Lard")]
    public void Classify_OtherAnimalDerived_ReturnsOtherAnimalDerived(string name)
    {
        Assert.Equal(IngredientCategory.OtherAnimalDerived, _sut.Classify(name));
    }

    [Theory]
    [InlineData("Yellow Potatoes")]
    [InlineData("Garlic")]
    [InlineData("Olive Oil")]
    [InlineData("Salt")]
    [InlineData("Black Pepper")]
    [InlineData("Flour")]
    [InlineData("Onion")]
    [InlineData("Tomato Paste")]
    [InlineData("Paprika")]
    [InlineData("Cumin")]
    [InlineData("Lemon")]
    [InlineData("Almond Milk")]
    [InlineData("Coconut Milk")]
    public void Classify_PlantBasedIngredients_ReturnsPlantBased(string name)
    {
        Assert.Equal(IngredientCategory.PlantBased, _sut.Classify(name));
    }

    // ── Infer — empty input ───────────────────────────────────────────────────

    [Fact]
    public void Infer_EmptyIngredients_ReturnsAllNull()
    {
        var profile = _sut.Infer([]);

        Assert.Null(profile.IsDairyFree);
        Assert.Null(profile.IsVegetarian);
        Assert.Null(profile.IsVegan);
    }

    // ── Infer — Dairy Free ────────────────────────────────────────────────────

    [Fact]
    public void Infer_NoDairyIngredients_IsDairyFreeTrue()
    {
        var profile = _sut.Infer(["Yellow Potatoes", "Garlic", "Olive Oil", "Salt"]);

        Assert.True(profile.IsDairyFree);
    }

    [Fact]
    public void Infer_WithMilk_IsDairyFreeFalse()
    {
        var profile = _sut.Infer(["Yellow Potatoes", "Whole Milk", "Salt"]);

        Assert.False(profile.IsDairyFree);
    }

    [Fact]
    public void Infer_WithCheese_IsDairyFreeFalse()
    {
        var profile = _sut.Infer(["Pasta", "Butter", "Cheddar Cheese"]);

        Assert.False(profile.IsDairyFree);
    }

    [Fact]
    public void Infer_WithButter_IsDairyFreeFalse()
    {
        var profile = _sut.Infer(["Yellow Potatoes", "Butter", "Salt"]);

        Assert.False(profile.IsDairyFree);
    }

    // ── Infer — Vegetarian ────────────────────────────────────────────────────

    [Fact]
    public void Infer_NoMeatIngredients_IsVegetarianTrue()
    {
        var profile = _sut.Infer(["Yellow Potatoes", "Butter", "Cream", "Salt"]);

        Assert.True(profile.IsVegetarian);
    }

    [Fact]
    public void Infer_WithBeef_IsVegetarianFalse()
    {
        var profile = _sut.Infer(["Ground Beef", "Onion", "Salt"]);

        Assert.False(profile.IsVegetarian);
    }

    [Fact]
    public void Infer_WithChicken_IsVegetarianFalse()
    {
        var profile = _sut.Infer(["Chicken Thighs", "Garlic", "Lemon"]);

        Assert.False(profile.IsVegetarian);
    }

    // ── Infer — Vegan ─────────────────────────────────────────────────────────

    [Fact]
    public void Infer_AllPlantBased_IsVeganTrue()
    {
        var profile = _sut.Infer(["Yellow Potatoes", "Olive Oil", "Salt", "Garlic"]);

        Assert.True(profile.IsVegan);
    }

    [Fact]
    public void Infer_WithMeat_IsVeganFalse()
    {
        var profile = _sut.Infer(["Ground Beef", "Onion", "Salt"]);

        Assert.False(profile.IsVegan);
    }

    [Fact]
    public void Infer_WithDairy_IsVeganFalse()
    {
        var profile = _sut.Infer(["Pasta", "Butter", "Salt"]);

        Assert.False(profile.IsVegan);
    }

    [Fact]
    public void Infer_WithEggs_IsVeganFalse()
    {
        var profile = _sut.Infer(["Flour", "Eggs", "Sugar", "Olive Oil"]);

        Assert.False(profile.IsVegan);
    }

    [Fact]
    public void Infer_WithHoney_IsVeganFalse()
    {
        var profile = _sut.Infer(["Almond", "Honey", "Oat"]);

        Assert.False(profile.IsVegan);
    }

    // ── Infer — unknown ingredient handling ───────────────────────────────────

    [Fact]
    public void Infer_WithUnknownIngredient_IsDairyFreeNull()
    {
        // "UnknownIngredient123" is not in any keyword list
        var profile = _sut.Infer(["Yellow Potatoes", "UnknownIngredient123", "Salt"]);

        // dairy not detected, but unclassified ingredient → unknown
        Assert.Null(profile.IsDairyFree);
    }

    [Fact]
    public void Infer_WithUnknownIngredient_IsVegetarianNull()
    {
        var profile = _sut.Infer(["Yellow Potatoes", "UnknownIngredient123", "Salt"]);

        Assert.Null(profile.IsVegetarian);
    }

    [Fact]
    public void Infer_WithUnknownIngredient_IsVeganNull()
    {
        var profile = _sut.Infer(["Yellow Potatoes", "UnknownIngredient123", "Salt"]);

        Assert.Null(profile.IsVegan);
    }

    [Fact]
    public void Infer_WithUnknownIngredientButDairyDetected_IsDairyFreeFalse()
    {
        // Even with an unknown ingredient, detecting dairy makes IsDairyFree = false
        var profile = _sut.Infer(["UnknownIngredient123", "Butter", "Salt"]);

        Assert.False(profile.IsDairyFree);
    }

    [Fact]
    public void Infer_WithUnknownIngredientButMeatDetected_IsVegetarianFalse()
    {
        var profile = _sut.Infer(["UnknownIngredient123", "Ground Beef", "Salt"]);

        Assert.False(profile.IsVegetarian);
        Assert.False(profile.IsVegan);
    }

    // ── Vegetarian/Vegan distinction ──────────────────────────────────────────

    [Fact]
    public void Infer_WithEggsAndDairy_IsVegetarianTrueIsVeganFalse()
    {
        // Eggs + cheese → vegetarian (no meat) but not vegan
        var profile = _sut.Infer(["Flour", "Eggs", "Cheddar Cheese", "Garlic"]);

        Assert.True(profile.IsVegetarian);
        Assert.False(profile.IsVegan);
    }

    [Fact]
    public void Infer_DairyOnlyRecipe_IsVegetarianTrueIsVeganFalse()
    {
        var profile = _sut.Infer(["Whole Milk", "Sugar", "Vanilla"]);

        Assert.True(profile.IsVegetarian);
        Assert.False(profile.IsVegan);
    }
}
