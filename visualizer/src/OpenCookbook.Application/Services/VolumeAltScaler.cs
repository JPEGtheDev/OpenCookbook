using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenCookbook.Application.Services;

/// <summary>
/// Scales and reformats <c>volume_alt</c> and <c>weight_alt</c> strings
/// when a recipe is scaled by a multiplier.
/// Supports tsp, tbsp, fl oz, cup, pint, quart, gallon (volume)
/// and oz, lb (weight). Unrecognised formats are returned as-is.
/// </summary>
public static partial class VolumeAltScaler
{
    private const double Epsilon = 1e-9;

    /// <summary>
    /// Scales a <c>volume_alt</c> string (e.g. "3/4 tsp.") by <paramref name="multiplier"/>
    /// and returns the reformatted result, applying unit-promotion rules.
    /// Returns <see langword="null"/> if the input is <see langword="null"/>.
    /// Returns the original string unchanged if the format is unrecognised.
    /// </summary>
    public static string? ScaleVolumeAlt(string? volumeAlt, double multiplier)
    {
        if (volumeAlt is null) return null;
        if (!TryParseAlt(volumeAlt, out double quantity, out string unit))
            return volumeAlt;

        double scaled = quantity * multiplier;

        return NormalizeUnit(unit) switch
        {
            "tsp"    => FormatFromTsp(scaled),
            "tbsp"   => FormatFromTbsp(scaled),
            "floz"   => FormatFromFlOz(scaled),
            "cup"    => FormatFromCup(scaled),
            "pint"   => FormatFromPint(scaled),
            "quart"  => FormatFromQuart(scaled),
            "gallon" => FormatFromGallon(scaled),
            "oz"     => FormatWeightOz(scaled),
            "lb"     => FormatFromLb(scaled),
            _        => volumeAlt,
        };
    }

    /// <summary>
    /// Scales a <c>weight_alt</c> string (e.g. "1 lb") by <paramref name="multiplier"/>.
    /// Delegates to <see cref="ScaleVolumeAlt"/> because the same parse/format
    /// pipeline handles both volume and weight units.
    /// </summary>
    public static string? ScaleWeightAlt(string? weightAlt, double multiplier)
        => ScaleVolumeAlt(weightAlt, multiplier);

    // ── Parsing ───────────────────────────────────────────────────────────────

    // "1 1/2 tbsp."  — whole + fraction + unit
    [GeneratedRegex(@"^(\d+)\s+(\d+)/(\d+)\s+(.+?)\s*$")]
    private static partial Regex MixedFractionRegex();

    // "3/4 tsp."  — pure fraction + unit
    [GeneratedRegex(@"^(\d+)/(\d+)\s+(.+?)\s*$")]
    private static partial Regex SimpleFractionRegex();

    // "2 tsp." / "1.5 cup"  — whole or decimal + unit
    [GeneratedRegex(@"^(\d+(?:\.\d+)?)\s+(.+?)\s*$")]
    private static partial Regex WholeOrDecimalRegex();

    private static bool TryParseAlt(string s, out double quantity, out string unit)
    {
        s = s.Trim();

        var m = MixedFractionRegex().Match(s);
        if (m.Success)
        {
            int whole = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            int num   = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            int den   = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
            quantity  = whole + (double)num / den;
            unit      = m.Groups[4].Value.Trim();
            return true;
        }

        m = SimpleFractionRegex().Match(s);
        if (m.Success)
        {
            int num  = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            int den  = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            quantity = (double)num / den;
            unit     = m.Groups[3].Value.Trim();
            return true;
        }

        m = WholeOrDecimalRegex().Match(s);
        if (m.Success)
        {
            quantity = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            unit     = m.Groups[2].Value.Trim();
            return true;
        }

        quantity = 0;
        unit     = string.Empty;
        return false;
    }

    private static string NormalizeUnit(string unit) =>
        unit.ToLowerInvariant().TrimEnd('.') switch
        {
            "tsp"                       => "tsp",
            "tbsp"                      => "tbsp",
            "fl oz"                     => "floz",
            "cup" or "cups"             => "cup",
            "pint" or "pints" or "pt"   => "pint",
            "quart" or "quarts" or "qt" => "quart",
            "gallon" or "gallons"
                     or "gal"           => "gallon",
            "oz"                        => "oz",
            "lb" or "lbs"               => "lb",
            _                           => unit.ToLowerInvariant(),
        };

    // ── Volume formatters ─────────────────────────────────────────────────────

    // Number of 1/8-tsp units in one teaspoon — used for tsp rounding.
    private const double TspEighths = 8.0;

    /// <summary>
    /// Formats a value that originated (or promotes from) teaspoons.
    /// Rounds to nearest 1/8 tsp.  Values &lt; 1/8 tsp become "a pinch".
    /// Promotes to tbsp when the rounded value is an exact multiple of 3 tsp.
    /// </summary>
    private static string FormatFromTsp(double tspValue)
    {
        double rounded = Math.Round(tspValue * TspEighths, MidpointRounding.AwayFromZero) / TspEighths;

        if (rounded < 0.125 - Epsilon)
            return "a pinch";

        // Promote: exact multiple of 3 tsp → tablespoons (and further up the chain)
        if (IsNearMultipleOf(rounded, 3.0))
            return FormatFromTbsp(rounded / 3.0);

        return FormatFraction(rounded, 8) + " tsp.";
    }

    /// <summary>
    /// Formats a value in tablespoons.
    /// Rounds to nearest 1/2 tbsp.
    /// Promotes to cups when the value is an exact multiple of 4 tbsp (1/4 cup = 4 tbsp).
    /// </summary>
    private static string FormatFromTbsp(double tbspValue)
    {
        // Promote: exact multiple of 4 tbsp → cups (1/4 cup = 4 tbsp, 1 cup = 16 tbsp)
        if (IsNearMultipleOf(tbspValue, 4.0))
            return FormatFromCup(tbspValue / 16.0);

        double rounded = Math.Round(tbspValue * 2.0, MidpointRounding.AwayFromZero) / 2.0;
        return FormatFraction(rounded, 2) + " tbsp.";
    }

    /// <summary>
    /// Formats a value in fluid ounces.
    /// When the scaled value is less than 1 fl oz, falls back to teaspoon formatting.
    /// Otherwise rounds to nearest whole fl oz.
    /// Promotes to cups when the rounded value is a multiple of 2 fl oz (1/4 cup = 2 fl oz).
    /// </summary>
    private static string FormatFromFlOz(double flOzValue)
    {
        // Sub-1 fl oz: fall back to tsp display (1 fl oz = 6 tsp)
        if (flOzValue < 1.0 - Epsilon)
            return FormatFromTsp(flOzValue * 6.0);

        double rounded = Math.Round(flOzValue, MidpointRounding.AwayFromZero);

        // Promote: multiple of 2 fl oz → quarter-cup fractions
        if (IsNearMultipleOf(rounded, 2.0))
            return FormatFromCup(rounded / 8.0);   // 1 cup = 8 fl oz

        return $"{rounded:0} fl oz";
    }

    /// <summary>
    /// Formats a value in cups (1/4-cup fractions).
    /// Promotes to pints when the value is an exact multiple of 2 cups.
    /// </summary>
    private static string FormatFromCup(double cupValue)
    {
        // Promote: 2 cups → 1 pint (and further up the chain)
        if (IsNearMultipleOf(cupValue, 2.0))
            return FormatFromPint(cupValue / 2.0);

        double rounded = Math.Round(cupValue * 4.0, MidpointRounding.AwayFromZero) / 4.0;
        string fraction = FormatFraction(rounded, 4);

        // Plural for amounts greater than 1 cup
        return rounded > 1.0 + Epsilon ? $"{fraction} cups" : $"{fraction} cup";
    }

    /// <summary>
    /// Formats a value in pints.
    /// Promotes to quarts when the value is an exact multiple of 2 pints.
    /// </summary>
    private static string FormatFromPint(double pintValue)
    {
        if (IsNearMultipleOf(pintValue, 2.0))
            return FormatFromQuart(pintValue / 2.0);

        double rounded = Math.Round(pintValue, MidpointRounding.AwayFromZero);
        return rounded == 1.0 ? "1 pint" : $"{(int)rounded} pints";
    }

    /// <summary>
    /// Formats a value in quarts.
    /// Promotes to gallons when the value is an exact multiple of 4 quarts.
    /// </summary>
    private static string FormatFromQuart(double quartValue)
    {
        if (IsNearMultipleOf(quartValue, 4.0))
            return FormatFromGallon(quartValue / 4.0);

        double rounded = Math.Round(quartValue, MidpointRounding.AwayFromZero);
        return rounded == 1.0 ? "1 quart" : $"{(int)rounded} quarts";
    }

    /// <summary>Formats a value in gallons (whole gallons only).</summary>
    private static string FormatFromGallon(double gallonValue)
    {
        double rounded = Math.Round(gallonValue, MidpointRounding.AwayFromZero);
        return rounded == 1.0 ? "1 gallon" : $"{(int)rounded} gallons";
    }

    // ── Weight formatters ─────────────────────────────────────────────────────

    /// <summary>
    /// Formats a weight value in ounces, rounded to nearest 1/2 oz.
    /// </summary>
    private static string FormatWeightOz(double ozValue)
    {
        double rounded = Math.Round(ozValue * 2.0, MidpointRounding.AwayFromZero) / 2.0;
        return FormatFraction(rounded, 2) + " oz";
    }

    /// <summary>
    /// Formats a weight value that originated in pounds.
    /// Stays as whole pounds when the result is an exact whole number of lbs;
    /// otherwise converts to ounces for readability.
    /// </summary>
    private static string FormatFromLb(double lbValue)
    {
        // Whole-pound result → keep as lb
        if (IsNearMultipleOf(lbValue, 1.0))
        {
            int lbs = (int)Math.Round(lbValue, MidpointRounding.AwayFromZero);
            return lbs == 1 ? "1 lb" : $"{lbs} lbs";
        }

        // Fractional result → display in ounces (1 lb = 16 oz)
        return FormatWeightOz(lbValue * 16.0);
    }

    // ── Shared helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true when <paramref name="value"/> is within <see cref="Epsilon"/>
    /// of an exact integer multiple of <paramref name="multiple"/>.
    /// Zero is deliberately excluded — a scaled quantity of zero is a degenerate
    /// case that should not trigger unit promotions.
    /// </summary>
    private static bool IsNearMultipleOf(double value, double multiple)
    {
        if (value <= Epsilon) return false;
        double ratio   = value / multiple;
        double nearest = Math.Round(ratio, MidpointRounding.AwayFromZero);
        return Math.Abs(ratio - nearest) < Epsilon;
    }

    /// <summary>
    /// Formats <paramref name="value"/> as a mixed-number fraction reduced to lowest
    /// terms, where the denominator of the fractional part is <paramref name="denominator"/>.
    /// E.g. FormatFraction(1.5, 8) → "1 1/2", FormatFraction(0.25, 4) → "1/4".
    /// </summary>
    private static string FormatFraction(double value, int denominator)
    {
        int totalUnits = (int)Math.Round(value * denominator, MidpointRounding.AwayFromZero);
        int whole      = totalUnits / denominator;
        int remainder  = totalUnits % denominator;

        if (remainder == 0)
            return whole.ToString(CultureInfo.InvariantCulture);

        int gcd = Gcd(remainder, denominator);
        int num = remainder / gcd;
        int den = denominator / gcd;

        return whole == 0 ? $"{num}/{den}" : $"{whole} {num}/{den}";
    }

    private static int Gcd(int a, int b)
    {
        while (b != 0) (a, b) = (b, a % b);
        return a;
    }
}
