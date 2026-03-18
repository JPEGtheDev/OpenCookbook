using Bunit;
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

        // Assert
        Assert.Contains("500", cut.Markup);
        Assert.Contains("200", cut.Markup);
        Assert.Contains("5", cut.Markup);
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
    public void IngredientList_Clicking2X_DoublesQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        var twoXBtn = cut.FindAll(".scale-btn")[2]; // "2×"
        twoXBtn.Click();

        // Assert
        Assert.Contains("1000", cut.Markup);
        Assert.Contains("400", cut.Markup);
        Assert.Contains("10", cut.Markup);
    }

    [Fact]
    public void IngredientList_ClickingHalfX_HalvesQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        var halfXBtn = cut.FindAll(".scale-btn")[0]; // "0.5×"
        halfXBtn.Click();

        // Assert
        Assert.Contains("250", cut.Markup);
        Assert.Contains("100", cut.Markup);
    }

    [Fact]
    public void IngredientList_ResetTo1X_RestoresOriginalQuantities()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Scale to 2×
        cut.FindAll(".scale-btn")[2].Click();
        Assert.Contains("1000", cut.Markup);

        // Reset to 1×
        cut.FindAll(".scale-btn")[1].Click();

        // Assert — original quantities restored
        Assert.Contains("500", cut.Markup);
        Assert.Contains("200", cut.Markup);
    }

    [Fact]
    public void IngredientList_Scaling_PreservesVolumeAlt()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        cut.FindAll(".scale-btn")[2].Click(); // 2×

        // Assert — volume_alt stays as reference, not scaled
        Assert.Contains("≈ 1 tsp.", cut.Markup);
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

        var qtySpans = cut.FindAll(".ingredient-qty-lockable");
        qtySpans[0].Click(); // Lock chicken

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
        cut.FindAll(".ingredient-qty-lockable")[0].Click();
        Assert.Contains("🔒", cut.Markup);

        // Unlock
        cut.Find(".lock-icon").Click();

        // Assert — no more lock UI
        Assert.DoesNotContain("🔒", cut.Markup);
    }

    [Fact]
    public void IngredientList_ChangingLockedQty_ScalesOtherIngredients()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Lock chicken (500g)
        cut.FindAll(".ingredient-qty-lockable")[0].Click();

        // Change locked value to 1000 (2× multiplier)
        var lockInput = cut.Find(".lock-input");
        lockInput.Change("1000");

        // Assert — yogurt should be 400, cumin should be 10
        Assert.Contains("400", cut.Markup);
        Assert.Contains("10", cut.Markup);
    }

    [Fact]
    public void IngredientList_SelectingPresetAfterLock_ClearsLock()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        // Lock an ingredient
        cut.FindAll(".ingredient-qty-lockable")[0].Click();
        Assert.Contains("🔒", cut.Markup);

        // Click 1× preset
        cut.FindAll(".scale-btn")[1].Click();

        // Assert — lock cleared
        Assert.DoesNotContain("🔒", cut.Markup);
        Assert.Contains("500", cut.Markup);
    }

    [Fact]
    public void IngredientList_LockedIngredient_HasHighlight()
    {
        // Arrange
        var groups = CreateSampleGroups();

        // Act
        var cut = Render<IngredientList>(p =>
            p.Add(x => x.Groups, groups));

        cut.FindAll(".ingredient-qty-lockable")[0].Click();

        // Assert
        var lockedItems = cut.FindAll(".ingredient-locked");
        Assert.Single(lockedItems);
    }
}
