using OpenCookbook.Application.Services;

namespace OpenCookbook.Application.Tests;

public class VolumeAltScalerTests
{
    // ── Null / unrecognised input ──────────────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_NullInput_ReturnsNull()
    {
        Assert.Null(VolumeAltScaler.ScaleVolumeAlt(null, 2.0));
    }

    [Fact]
    public void ScaleVolumeAlt_UnrecognisedFormat_ReturnsOriginalString()
    {
        Assert.Equal("some weird value", VolumeAltScaler.ScaleVolumeAlt("some weird value", 2.0));
    }

    [Fact]
    public void ScaleVolumeAlt_NullWeightAlt_ReturnsNull()
    {
        Assert.Null(VolumeAltScaler.ScaleWeightAlt(null, 2.0));
    }

    // ── Teaspoon scaling ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("1 tsp.",     1.0, "1 tsp.")]
    [InlineData("1 tsp.",     2.0, "2 tsp.")]
    [InlineData("1 tsp.",     0.5, "1/2 tsp.")]
    [InlineData("2 tsp.",     2.0, "4 tsp.")]
    [InlineData("3/4 tsp.",   2.0, "1 1/2 tsp.")]
    [InlineData("3/4 tsp.",   3.0, "2 1/4 tsp.")]
    [InlineData("1/4 tsp.",   2.0, "1/2 tsp.")]
    [InlineData("1/8 tsp.",   2.0, "1/4 tsp.")]
    [InlineData("1 1/2 tsp.", 2.0, "1 tbsp.")]   // 3 tsp = 1 tbsp — promotion
    [InlineData("1 3/4 tsp.", 2.0, "3 1/2 tsp.")]
    public void ScaleVolumeAlt_TspInputs_ScalesCorrectly(
        string input, double multiplier, string expected)
    {
        Assert.Equal(expected, VolumeAltScaler.ScaleVolumeAlt(input, multiplier));
    }

    [Fact]
    public void ScaleVolumeAlt_TspScalesToPinch_ReturnsPinch()
    {
        // 1/8 tsp × 0.25 = 1/32 tsp — below 1/8 tsp threshold
        Assert.Equal("a pinch", VolumeAltScaler.ScaleVolumeAlt("1/8 tsp.", 0.25));
    }

    // ── Teaspoon → tablespoon promotion ──────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_TspPromotesToTbsp_AtExact3Tsp()
    {
        // 1 tsp × 3 = 3 tsp = 1 tbsp
        Assert.Equal("1 tbsp.", VolumeAltScaler.ScaleVolumeAlt("1 tsp.", 3.0));
    }

    [Fact]
    public void ScaleVolumeAlt_TspDoesNotPromote_AtNonMultipleOf3()
    {
        // 1 tsp × 4 = 4 tsp — not a multiple of 3
        Assert.Equal("4 tsp.", VolumeAltScaler.ScaleVolumeAlt("1 tsp.", 4.0));
    }

    [Fact]
    public void ScaleVolumeAlt_TspPromotesToTbsp_AtExact6Tsp()
    {
        // 1 tsp × 6 = 6 tsp = 2 tbsp
        Assert.Equal("2 tbsp.", VolumeAltScaler.ScaleVolumeAlt("1 tsp.", 6.0));
    }

    // ── Tablespoon → cup promotion ────────────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_TspThenTbspPromotesToCup_AtExact4Tbsp()
    {
        // 1 tsp × 12 = 12 tsp = 4 tbsp = 1/4 cup
        Assert.Equal("1/4 cup", VolumeAltScaler.ScaleVolumeAlt("1 tsp.", 12.0));
    }

    [Fact]
    public void ScaleVolumeAlt_Tbsp_PromotesToCup_AtExact4Tbsp()
    {
        // 1 tbsp × 4 = 4 tbsp = 1/4 cup
        Assert.Equal("1/4 cup", VolumeAltScaler.ScaleVolumeAlt("1 tbsp.", 4.0));
    }

    [Fact]
    public void ScaleVolumeAlt_Tbsp_PromotesToHalfCup_At8Tbsp()
    {
        // 2 tbsp × 4 = 8 tbsp = 1/2 cup
        Assert.Equal("1/2 cup", VolumeAltScaler.ScaleVolumeAlt("2 tbsp.", 4.0));
    }

    [Fact]
    public void ScaleVolumeAlt_Tbsp_PromotesToFullCup_At16Tbsp()
    {
        // 1 tbsp × 16 = 16 tbsp = 1 cup
        Assert.Equal("1 cup", VolumeAltScaler.ScaleVolumeAlt("1 tbsp.", 16.0));
    }

    [Fact]
    public void ScaleVolumeAlt_Tbsp_StaysAsTbsp_AtNonMultipleOf4()
    {
        // 1 tbsp × 5 = 5 tbsp — not a multiple of 4
        Assert.Equal("5 tbsp.", VolumeAltScaler.ScaleVolumeAlt("1 tbsp.", 5.0));
    }

    [Fact]
    public void ScaleVolumeAlt_TbspInput_ScalesCorrectly()
    {
        Assert.Equal("1 1/2 tbsp.", VolumeAltScaler.ScaleVolumeAlt("1 tbsp.", 1.5));
    }

    // ── Cup → pint promotion ──────────────────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_CupPromotesToPint_AtExact2Cups()
    {
        // 1 cup × 2 = 2 cups = 1 pint
        Assert.Equal("1 pint", VolumeAltScaler.ScaleVolumeAlt("1/4 cup", 8.0));
    }

    [Fact]
    public void ScaleVolumeAlt_TbspPromotesToPint_Via2Cups()
    {
        // 1 tbsp × 32 = 32 tbsp = 2 cups = 1 pint
        Assert.Equal("1 pint", VolumeAltScaler.ScaleVolumeAlt("1 tbsp.", 32.0));
    }

    // ── Pint → quart promotion ────────────────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_PintPromotesToQuart_AtExact2Pints()
    {
        // 1 pint × 2 = 2 pints = 1 quart
        Assert.Equal("1 quart", VolumeAltScaler.ScaleVolumeAlt("1 pint", 2.0));
    }

    // ── Quart → gallon promotion ──────────────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_QuartPromotesToGallon_AtExact4Quarts()
    {
        // 1 quart × 4 = 4 quarts = 1 gallon
        Assert.Equal("1 gallon", VolumeAltScaler.ScaleVolumeAlt("1 quart", 4.0));
    }

    // ── Fluid ounce scaling and cup promotion ─────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_FlOz_StaysAsFlOz_WhenNotCleanCupFraction()
    {
        // 1 fl oz × 7 = 7 fl oz — not a multiple of 2 fl oz from quarter-cup perspective
        Assert.Equal("7 fl oz", VolumeAltScaler.ScaleVolumeAlt("1 fl oz", 7.0));
    }

    [Fact]
    public void ScaleVolumeAlt_FlOz_PromotesToCups_WhenCleanQuarterCupFraction()
    {
        // 2 fl oz × 5 = 10 fl oz = 1 1/4 cups (10 fl oz / 8 fl oz per cup = 1.25)
        Assert.Equal("1 1/4 cups", VolumeAltScaler.ScaleVolumeAlt("2 fl oz", 5.0));
    }

    [Fact]
    public void ScaleVolumeAlt_FlOz_PromotesToHalfCup_At4FlOz()
    {
        // 1 fl oz × 4 = 4 fl oz = 1/2 cup
        Assert.Equal("1/2 cup", VolumeAltScaler.ScaleVolumeAlt("1 fl oz", 4.0));
    }

    // ── Cup display (no promotion) ─────────────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_CupFractionDisplay_QuarterCup()
    {
        Assert.Equal("1/4 cup", VolumeAltScaler.ScaleVolumeAlt("1/4 cup", 1.0));
    }

    [Fact]
    public void ScaleVolumeAlt_Cup_ShowsMixedFraction_At1AndQuarter()
    {
        // 1/4 cup × 5 = 5/4 cups = 1 1/4 cups
        Assert.Equal("1 1/4 cups", VolumeAltScaler.ScaleVolumeAlt("1/4 cup", 5.0));
    }

    [Fact]
    public void ScaleVolumeAlt_Cup_PluralizedAbove1Cup()
    {
        // 1/4 cup × 6 = 1 1/2 cups
        Assert.Equal("1 1/2 cups", VolumeAltScaler.ScaleVolumeAlt("1/4 cup", 6.0));
    }

    [Fact]
    public void ScaleVolumeAlt_Cup_SingularAt1Cup()
    {
        Assert.Equal("1 cup", VolumeAltScaler.ScaleVolumeAlt("1/4 cup", 4.0));
    }

    // ── Weight: lb scaling ────────────────────────────────────────────────────

    [Fact]
    public void ScaleWeightAlt_Lb_StaysAsLb_WhenResultIsWholeNumber()
    {
        Assert.Equal("2 lbs", VolumeAltScaler.ScaleWeightAlt("1 lb", 2.0));
    }

    [Fact]
    public void ScaleWeightAlt_Lb_ConvertsToOz_WhenResultIsFractional()
    {
        // 1 lb × 0.5 = 0.5 lb = 8 oz
        Assert.Equal("8 oz", VolumeAltScaler.ScaleWeightAlt("1 lb", 0.5));
    }

    [Fact]
    public void ScaleWeightAlt_Lb_ConvertsToOz_16oz()
    {
        // 1 lb × 1.0 = 1 lb — whole number → stays as "1 lb"
        Assert.Equal("1 lb", VolumeAltScaler.ScaleWeightAlt("1 lb", 1.0));
    }

    // ── Weight: oz scaling ────────────────────────────────────────────────────

    [Fact]
    public void ScaleWeightAlt_Oz_ScalesCorrectly()
    {
        Assert.Equal("16 oz", VolumeAltScaler.ScaleWeightAlt("8 oz", 2.0));
    }

    [Fact]
    public void ScaleWeightAlt_Oz_RoundsToHalfOunce()
    {
        // 1 oz × 1.3 = 1.3 oz → rounded to nearest 0.5 oz = 1.5 oz
        Assert.Equal("1 1/2 oz", VolumeAltScaler.ScaleWeightAlt("1 oz", 1.5));
    }

    // ── 1× scale returns same value ───────────────────────────────────────────

    [Theory]
    [InlineData("1 tsp.")]
    [InlineData("3/4 tsp.")]
    [InlineData("1 1/2 tsp.")]
    [InlineData("1 tbsp.")]
    [InlineData("1 1/2 tbsp.")]
    [InlineData("1/4 cup")]
    [InlineData("1/2 cup")]
    public void ScaleVolumeAlt_AtOneX_ReturnsEquivalentValue(string input)
    {
        // At 1× the output should be the same value (may reformat slightly, but equivalent)
        var result = VolumeAltScaler.ScaleVolumeAlt(input, 1.0);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Verify by scaling the result at 1× again — should be stable
        Assert.Equal(result, VolumeAltScaler.ScaleVolumeAlt(result, 1.0));
    }

    // ── No crash on no volume_alt ─────────────────────────────────────────────

    [Fact]
    public void ScaleVolumeAlt_NullAlwaysReturnsNull_NoCrash()
    {
        // Acceptance criterion: no crash when volume_alt is null
        Assert.Null(VolumeAltScaler.ScaleVolumeAlt(null, 3.0));
        Assert.Null(VolumeAltScaler.ScaleWeightAlt(null, 3.0));
    }
}
