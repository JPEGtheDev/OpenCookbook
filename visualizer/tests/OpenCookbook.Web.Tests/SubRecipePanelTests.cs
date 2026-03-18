using Bunit;
using Microsoft.Extensions.DependencyInjection;
using OpenCookbook.Application.Interfaces;
using OpenCookbook.Application.Models;
using OpenCookbook.Domain.Entities;
using OpenCookbook.Web.Components;

namespace OpenCookbook.Web.Tests;

public class SubRecipePanelTests : BunitContext
{
    private static Recipe CreateSubRecipe()
    {
        return new Recipe
        {
            Name = "Kebab Meat",
            Version = "1.0",
            Author = "Test Author",
            Description = "Sub-recipe for testing",
            Status = RecipeStatus.Stable,
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = null,
                    Items =
                    [
                        new Ingredient { Quantity = 907, Unit = "g", Name = "Ground Beef" },
                        new Ingredient { Quantity = 3, Unit = "g", Name = "Black Pepper", VolumeAlt = "3/4 tsp." },
                    ]
                }
            ],
            Instructions =
            [
                new Section
                {
                    Heading = null,
                    Type = SectionType.Sequence,
                    Steps =
                    [
                        new Step { Text = "Mix all ingredients in a bowl" },
                        new Step { Text = "Form into a loaf shape" },
                    ]
                }
            ]
        };
    }

    private void RegisterMockRepository(Recipe? recipeToReturn = null, bool shouldThrow = false)
    {
        var repo = new FakeRecipeRepository(recipeToReturn, shouldThrow);
        Services.AddSingleton<IRecipeRepository>(repo);
    }

    // ── Rendering with Loaded Sub-Recipe ──────────────────

    [Fact]
    public void SubRecipePanel_WhenLoaded_ShowsDetailsElement()
    {
        // Arrange
        RegisterMockRepository(CreateSubRecipe());

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Kebab_Meat.yaml");
            p.Add(x => x.RecipePath, "Grilling/Kebab_Meatballs.yaml");
            p.Add(x => x.IngredientName, "Kebab Meat Recipe");
        });

        // Assert — a <details> element is rendered
        var details = cut.Find("details.sub-recipe-details");
        Assert.NotNull(details);
    }

    [Fact]
    public void SubRecipePanel_WhenLoaded_ShowsIngredientNameAsSummary()
    {
        // Arrange
        RegisterMockRepository(CreateSubRecipe());

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Kebab_Meat.yaml");
            p.Add(x => x.RecipePath, "Grilling/Kebab_Meatballs.yaml");
            p.Add(x => x.IngredientName, "Kebab Meat Recipe");
        });

        // Assert — summary contains the ingredient name
        var summary = cut.Find("summary.sub-recipe-toggle");
        Assert.Contains("Kebab Meat Recipe", summary.TextContent);
    }

    [Fact]
    public void SubRecipePanel_WhenLoaded_ShowsSubRecipeIngredients()
    {
        // Arrange
        RegisterMockRepository(CreateSubRecipe());

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Kebab_Meat.yaml");
            p.Add(x => x.RecipePath, "Grilling/Kebab_Meatballs.yaml");
            p.Add(x => x.IngredientName, "Kebab Meat Recipe");
        });

        // Assert — sub-recipe ingredients are listed
        Assert.Contains("Ground Beef", cut.Markup);
        Assert.Contains("907 g", cut.Markup);
        Assert.Contains("Black Pepper", cut.Markup);
    }

    [Fact]
    public void SubRecipePanel_WhenLoaded_ShowsVolumeAlt()
    {
        // Arrange
        RegisterMockRepository(CreateSubRecipe());

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Kebab_Meat.yaml");
            p.Add(x => x.RecipePath, "Grilling/Kebab_Meatballs.yaml");
            p.Add(x => x.IngredientName, "Kebab Meat Recipe");
        });

        // Assert — volume_alt shown for small-quantity spices
        Assert.Contains("≈ 3/4 tsp.", cut.Markup);
    }

    [Fact]
    public void SubRecipePanel_WhenLoaded_ShowsInstructions()
    {
        // Arrange
        RegisterMockRepository(CreateSubRecipe());

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Kebab_Meat.yaml");
            p.Add(x => x.RecipePath, "Grilling/Kebab_Meatballs.yaml");
            p.Add(x => x.IngredientName, "Kebab Meat Recipe");
        });

        // Assert — instructions are shown
        Assert.Contains("Mix all ingredients in a bowl", cut.Markup);
        Assert.Contains("Form into a loaf shape", cut.Markup);
        var steps = cut.FindAll(".sub-recipe-step-list li");
        Assert.Equal(2, steps.Count);
    }

    // ── Fallback on Error ──────────────────────────

    [Fact]
    public void SubRecipePanel_WhenLoadFails_ShowsFallbackLink()
    {
        // Arrange
        RegisterMockRepository(shouldThrow: true);

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Missing.yaml");
            p.Add(x => x.RecipePath, "Grilling/Kebab_Meatballs.yaml");
            p.Add(x => x.IngredientName, "Missing Recipe");
            p.Add(x => x.FallbackUrl, "http://localhost/recipe/Missing.yaml");
        });

        // Assert — no <details>, a plain link instead
        Assert.Empty(cut.FindAll("details"));
        var link = cut.Find("a");
        Assert.Contains("Missing Recipe", link.TextContent);
        Assert.Equal("http://localhost/recipe/Missing.yaml", link.GetAttribute("href"));
    }

    [Fact]
    public void SubRecipePanel_WhenLoadFails_WithoutFallbackUrl_ShowsPlainName()
    {
        // Arrange
        RegisterMockRepository(shouldThrow: true);

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Missing.yaml");
            p.Add(x => x.RecipePath, "Grilling/Kebab_Meatballs.yaml");
            p.Add(x => x.IngredientName, "Missing Recipe");
        });

        // Assert — no details, no link, just plain text
        Assert.Empty(cut.FindAll("details"));
        Assert.Empty(cut.FindAll("a"));
        Assert.Contains("Missing Recipe", cut.Markup);
    }

    // ── Path Resolution ──────────────────────────

    [Theory]
    [InlineData("Grilling/Kebab_Meatballs.yaml", "./Kebab_Meat.yaml", "Grilling/Kebab_Meat.yaml")]
    [InlineData("Grilling/Kebab_Meatballs.yaml", "Kebab_Meat.yaml", "Grilling/Kebab_Meat.yaml")]
    [InlineData("Simple_Recipe.yaml", "./Other.yaml", "Other.yaml")]
    [InlineData(null, "./Other.yaml", "Other.yaml")]
    public void ResolveDocLink_ReturnsExpectedPath(string? recipePath, string docLink, string expected)
    {
        // Act
        var result = SubRecipePanel.ResolveDocLink(recipePath, docLink);

        // Assert
        Assert.Equal(expected, result);
    }

    // ── Grouped Ingredients ──────────────────────────

    [Fact]
    public void SubRecipePanel_WithGroupedIngredients_ShowsGroupHeadings()
    {
        // Arrange
        var subRecipe = new Recipe
        {
            Name = "Test Recipe",
            Version = "1.0",
            Author = "Test",
            Description = "Test",
            Ingredients =
            [
                new IngredientGroup
                {
                    Heading = "Dry Ingredients",
                    Items = [new Ingredient { Quantity = 100, Unit = "g", Name = "Flour" }]
                },
                new IngredientGroup
                {
                    Heading = "Wet Ingredients",
                    Items = [new Ingredient { Quantity = 200, Unit = "ml", Name = "Milk" }]
                }
            ],
            Instructions = []
        };
        RegisterMockRepository(subRecipe);

        // Act
        var cut = Render<SubRecipePanel>(p =>
        {
            p.Add(x => x.DocLink, "./Test.yaml");
            p.Add(x => x.RecipePath, "Recipes/Main.yaml");
            p.Add(x => x.IngredientName, "Test Recipe");
        });

        // Assert — group headings shown
        var headings = cut.FindAll(".sub-recipe-group-heading");
        Assert.Equal(2, headings.Count);
        Assert.Contains("Dry Ingredients", headings[0].TextContent);
        Assert.Contains("Wet Ingredients", headings[1].TextContent);
    }

    // ── Fake Repository ──────────────────────────

    private sealed class FakeRecipeRepository : IRecipeRepository
    {
        private readonly Recipe? _recipe;
        private readonly bool _shouldThrow;

        public FakeRecipeRepository(Recipe? recipe = null, bool shouldThrow = false)
        {
            _recipe = recipe;
            _shouldThrow = shouldThrow;
        }

        public Task<IReadOnlyList<RecipeIndex>> GetRecipeIndexAsync()
        {
            return Task.FromResult<IReadOnlyList<RecipeIndex>>([]);
        }

        public Task<Recipe> GetRecipeAsync(string path)
        {
            if (_shouldThrow)
                throw new HttpRequestException("Not found");

            return Task.FromResult(_recipe ?? throw new InvalidOperationException("No recipe configured"));
        }
    }
}
