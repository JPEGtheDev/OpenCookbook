using OpenCookbook.Web.Pages;

namespace OpenCookbook.Web.Tests;

public class RecipeUrlHelperTests
{
    // ── EscapeRecipePath — .yaml extension stripping ───────────────────────────

    [Fact]
    public void EscapeRecipePath_PathWithYamlExtension_StripsExtension()
    {
        var result = RecipeUrlHelper.EscapeRecipePath("Recipes/Beta/Shrimp_Scampi.yaml");
        Assert.Equal("Recipes/Beta/Shrimp_Scampi", result);
    }

    [Fact]
    public void EscapeRecipePath_PathWithoutYamlExtension_ReturnsUnchanged()
    {
        var result = RecipeUrlHelper.EscapeRecipePath("Recipes/Beta/Shrimp_Scampi");
        Assert.Equal("Recipes/Beta/Shrimp_Scampi", result);
    }

    [Fact]
    public void EscapeRecipePath_YamlExtensionCheck_IsCaseInsensitive()
    {
        var result = RecipeUrlHelper.EscapeRecipePath("Recipes/Kebab_Meat.YAML");
        Assert.Equal("Recipes/Kebab_Meat", result);
    }

    // ── EscapeRecipePath — percent-encoding ────────────────────────────────────

    [Fact]
    public void EscapeRecipePath_SpecialCharsInSegment_AreEncoded()
    {
        var result = RecipeUrlHelper.EscapeRecipePath("Recipes/Chicken & Rice.yaml");
        Assert.Equal("Recipes/Chicken%20%26%20Rice", result);
    }

    [Fact]
    public void EscapeRecipePath_ForwardSlashSeparators_ArePreserved()
    {
        var result = RecipeUrlHelper.EscapeRecipePath("Recipes/Grilling/Kebab_Meat.yaml");
        Assert.Equal("Recipes/Grilling/Kebab_Meat", result);
    }

    [Fact]
    public void EscapeRecipePath_SimpleNameWithoutSubdirectory_Works()
    {
        var result = RecipeUrlHelper.EscapeRecipePath("Kebab_Meat.yaml");
        Assert.Equal("Kebab_Meat", result);
    }
}
