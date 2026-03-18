using Bunit;
using Microsoft.AspNetCore.Components;
using OpenCookbook.Domain.Entities;
using OpenCookbook.Web.Components;

namespace OpenCookbook.Web.Tests;

public class IngredientListTests : BunitContext
{
    private static List<IngredientGroup> CreateSampleGroups()
    {
        return
        [
            new IngredientGroup
            {
                Heading = null,
                Items =
                [
                    new Ingredient { Quantity = 500, Unit = "g", Name = "Chicken" },
                    new Ingredient { Quantity = 200, Unit = "ml", Name = "Yogurt" },
                    new Ingredient { Quantity = 5, Unit = "g", Name = "Cumin", VolumeAlt = "1 tsp." },
                ]
            }
        ];
    }

    private static List<string> GetRenderedQuantities(IRenderedComponent<IngredientList> cut)
    {
        // Collect text from all quantity-displaying elements (both lockable buttons and locked spans)
        var lockable = cut.FindAll("button.ingredient-qty-lockable");
        var locked = cut.FindAll(".ingredient-qty-locked");
        var result = new List<string>();
        foreach (var el in lockable)
            result.Add(el.TextContent.Trim());
        foreach (var el in locked)
            result.Add(el.TextContent.Trim());
        return result;
    }

    // ── Multiplier Toolbar ─────────────────────────────

    [Fact]
    public void IngredientList_ShowsMultiplierButtons()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Assert
        var buttons = cut.FindAll(".scale-btn");
        Assert.Equal(4, buttons.Count);
        Assert.Contains("0.5×", buttons[0].TextContent);
        Assert.Contains("1×", buttons[1].TextContent);
        Assert.Contains("2×", buttons[2].TextContent);
        Assert.Contains("3×", buttons[3].TextContent);
    }

    [Fact]
    public void IngredientList_DefaultIs1X_ShowsOriginalQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Assert — verify exact text of each quantity element
        var quantities = GetRenderedQuantities(cut);
        Assert.Contains("500 g", quantities);
        Assert.Contains("200 ml", quantities);
        Assert.Contains("5 g", quantities);
    }

    [Fact]
    public void IngredientList_1XButtonIsActiveByDefault()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Assert
        var activeButtons = cut.FindAll(".scale-btn-active");
        Assert.Single(activeButtons);
        Assert.Contains("1×", activeButtons[0].TextContent);
    }

    [Fact]
    public void IngredientList_WithMultiplier2X_DoublesQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.Multiplier, 2.0);
        });

        // Assert — verify exact text of each quantity element
        var quantities = GetRenderedQuantities(cut);
        Assert.Contains("1000 g", quantities);
        Assert.Contains("400 ml", quantities);
        Assert.Contains("10 g", quantities);
    }

    [Fact]
    public void IngredientList_WithMultiplierHalf_HalvesQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.Multiplier, 0.5);
        });

        // Assert — verify exact text of each quantity element
        var quantities = GetRenderedQuantities(cut);
        Assert.Contains("250 g", quantities);
        Assert.Contains("100 ml", quantities);
        Assert.Contains("2.5 g", quantities);
    }

    [Fact]
    public void IngredientList_ClickingPreset_RaisesMultiplierChanged()
    {
        // Arrange
        var groups = CreateSampleGroups();
        double? receivedMultiplier = null;

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.MultiplierChanged,
                EventCallback.Factory.Create<double>(this, m => receivedMultiplier = m));
        });

        var twoXBtn = cut.FindAll(".scale-btn")[2]; // "2×"
        twoXBtn.Click();

        // Assert
        Assert.Equal(2.0, receivedMultiplier);
    }

    [Fact]
    public void IngredientList_Scaling_PreservesVolumeAlt()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.Multiplier, 2.0);
        });

        // Assert — volume_alt stays as reference, not scaled
        Assert.Contains("≈ 1 tsp.", cut.Markup);
    }

    // ── Yields Display ────────────────────────────────

    [Fact]
    public void IngredientList_WithYields_ShowsScaledAndPluralizedYields()
    {
        // Arrange
        var groups = CreateSampleGroups();
        var yields = new RecipeYield { Quantity = 8, Unit = "serving" };

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.Yields, yields);
            p.Add(x => x.Multiplier, 2.0);
        });

        // Assert — 8 × 2 = 16 and unit must be pluralized
        var yieldsText = cut.Find(".scale-yields").TextContent;
        Assert.Contains("16", yieldsText);
        Assert.Contains("servings", yieldsText);
    }

    [Fact]
    public void IngredientList_WithYields_SingularAt1X_NotPluralized()
    {
        // Arrange
        var groups = CreateSampleGroups();
        var yields = new RecipeYield { Quantity = 1, Unit = "batch" };

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.Yields, yields);
            p.Add(x => x.Multiplier, 1.0);
        });

        // Assert — quantity is 1 so unit stays singular
        var yieldsText = cut.Find(".scale-yields").TextContent;
        Assert.Contains("1", yieldsText);
        Assert.Contains("batch", yieldsText);
        Assert.DoesNotContain("batchs", yieldsText);
    }

    [Fact]
    public void IngredientList_WithMetricYields_NeverPluralized()
    {
        // Arrange
        var groups = CreateSampleGroups();
        var yields = new RecipeYield { Quantity = 500, Unit = "g" };

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.Yields, yields);
            p.Add(x => x.Multiplier, 2.0);
        });

        // Assert — metric units are never pluralized
        var yieldsText = cut.Find(".scale-yields").TextContent;
        Assert.Contains("1000", yieldsText);
        Assert.Contains(" g", yieldsText);
        Assert.DoesNotContain("gs", yieldsText);
    }

    [Fact]
    public void IngredientList_WithoutYields_DoesNotShowYieldsLine()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Assert
        Assert.Empty(cut.FindAll(".scale-yields"));
    }

    // ── Ingredient Locking ─────────────────────────────

    [Fact]
    public void IngredientList_ClickingIngredientQty_LocksIt()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        var qtyButtons = cut.FindAll("button.ingredient-qty-lockable");
        qtyButtons[0].Click(); // Lock chicken

        // Assert — lock icon and input should appear
        Assert.Contains("🔒", cut.Markup);
        var lockInput = cut.Find(".lock-input");
        Assert.NotNull(lockInput);
    }

    [Fact]
    public void IngredientList_UnlockButton_ClearsLock()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Lock chicken
        cut.FindAll("button.ingredient-qty-lockable")[0].Click();
        Assert.Contains("🔒", cut.Markup);

        // Unlock
        cut.Find(".lock-icon").Click();

        // Assert — no more lock UI
        Assert.DoesNotContain("🔒", cut.Markup);
    }

    [Fact]
    public void IngredientList_ChangingLockedQty_RaisesMultiplierChanged()
    {
        // Arrange
        var groups = CreateSampleGroups();
        double? receivedMultiplier = null;

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.MultiplierChanged,
                EventCallback.Factory.Create<double>(this, m => receivedMultiplier = m));
        });

        // Lock chicken (500g)
        cut.FindAll("button.ingredient-qty-lockable")[0].Click();

        // Change locked value to 1000 (expect 2× multiplier raised to parent)
        var lockInput = cut.Find(".lock-input");
        lockInput.Change("1000");

        // Assert — parent should receive multiplier of 2.0
        Assert.NotNull(receivedMultiplier);
        Assert.Equal(2.0, receivedMultiplier!.Value, precision: 10);
    }

    [Fact]
    public void IngredientList_SelectingPresetAfterLock_ClearsLock()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
        {
            p.Add(x => x.Groups, groups);
            p.Add(x => x.MultiplierChanged,
                EventCallback.Factory.Create<double>(this, _ => { }));
        });

        // Lock an ingredient
        cut.FindAll("button.ingredient-qty-lockable")[0].Click();
        Assert.Contains("🔒", cut.Markup);

        // Click 1× preset
        cut.FindAll(".scale-btn")[1].Click();

        // Assert — lock cleared
        Assert.DoesNotContain("🔒", cut.Markup);
    }

    [Fact]
    public void IngredientList_LockedIngredient_HasHighlight()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        cut.FindAll("button.ingredient-qty-lockable")[0].Click();

        // Assert
        var lockedItems = cut.FindAll(".ingredient-locked");
        Assert.Single(lockedItems);
    }

    [Fact]
    public void IngredientList_LockButton_HasAriaLabel()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Assert — lockable buttons have aria-label for accessibility
        var qtyButtons = cut.FindAll("button.ingredient-qty-lockable");
        Assert.All(qtyButtons, btn =>
            Assert.False(string.IsNullOrEmpty(btn.GetAttribute("aria-label"))));
    }

    [Fact]
    public void IngredientList_UnlockButton_HasAriaLabel()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        cut.FindAll("button.ingredient-qty-lockable")[0].Click();

        // Assert
        var unlockBtn = cut.Find(".lock-icon");
        Assert.False(string.IsNullOrEmpty(unlockBtn.GetAttribute("aria-label")));
    }
}
