using System.Globalization;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

/// <summary>
/// Parses the unit-suffixed values found in real BC and radar exports, where the library's own parser is
/// too strict or too lenient:
/// <list type="bullet">
/// <item>it accepts <c>ft/s</c> but rejects <c>fps</c>, which is what chronograph and radar tools write;</item>
/// <item>it accepts a decimal comma as a <i>thousands</i> separator, so <c>780,2m/s</c> silently becomes
/// 7802 m/s — the value is normalized here before the library sees it;</item>
/// <item>a bare number carries no unit, so the caller's column unit is applied as a fallback.</item>
/// </list>
/// </summary>
public static class MeasurementTextParser
{
    // Unit spellings the library does not know, mapped to the ones it does. Compared case-insensitively.
    // Only unambiguous, widely used spellings belong here.
    private static readonly Dictionary<string, string> VelocityAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fps"] = "ft/s",
        ["f/s"] = "ft/s",
        ["ft/sec"] = "ft/s",
        ["mps"] = "m/s",
        ["m/sec"] = "m/s",
        ["mph"] = "mi/h",
        ["kph"] = "km/h",
        ["kmh"] = "km/h",
        ["knots"] = "kt",
        ["kn"] = "kt",
    };

    private static readonly Dictionary<string, string> DistanceAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["yds"] = "yd",
        ["yard"] = "yd",
        ["yards"] = "yd",
        ["meter"] = "m",
        ["meters"] = "m",
        ["metres"] = "m",
        ["feet"] = "ft",
        ["inch"] = "in",
        ["inches"] = "in",
        ["mtr"] = "m",
    };

    /// <summary>Parses a distance, applying <paramref name="fallbackUnit"/> to a bare number.</summary>
    public static bool TryParseDistance(string? text, DistanceUnit fallbackUnit, out Measurement<DistanceUnit> value) =>
        TryParseMeasurement(text, fallbackUnit, DistanceAliases, out value);

    /// <summary>Parses a velocity, applying <paramref name="fallbackUnit"/> to a bare number.</summary>
    public static bool TryParseVelocity(string? text, VelocityUnit fallbackUnit, out Measurement<VelocityUnit> value) =>
        TryParseMeasurement(text, fallbackUnit, VelocityAliases, out value);

    /// <summary>
    /// Parses a ballistic coefficient, keeping the drag table the text names (<c>0.462G7</c>) and falling
    /// back to <paramref name="fallbackTable"/> for a bare number. Non-positive values fail: a BC of zero
    /// or less cannot scale a drag curve.
    /// </summary>
    public static bool TryParseBc(string? text, DragTableId fallbackTable, out BallisticCoefficient value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = NormalizeDecimalMark(text.Trim());

        if (BallisticCoefficient.TryParse(normalized, CultureInfo.InvariantCulture, out var parsed))
        {
            // A form factor of 1 on GC is the documented way to use a custom table, so only the
            // coefficient form is range-checked.
            if (parsed.Value <= 0)
                return false;
            value = parsed;
            return true;
        }

        if (TryParseDouble(normalized, out var bare) && bare > 0)
        {
            value = new BallisticCoefficient(bare, fallbackTable);
            return true;
        }

        return false;
    }

    /// <summary>Parses a plain number (a Mach value) accepting either decimal mark. Rejects unit suffixes.</summary>
    public static bool TryParseDouble(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return double.TryParse(NormalizeDecimalMark(text.Trim()),
                               NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseMeasurement<TUnit>(string? text, TUnit fallbackUnit,
                                                   Dictionary<string, string> aliases,
                                                   out Measurement<TUnit> value)
        where TUnit : Enum
    {
        value = new Measurement<TUnit>(0, fallbackUnit);
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = NormalizeDecimalMark(text.Trim());

        // A bare number takes the column's unit — that is what the dialogs' unit combos are for.
        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var bare))
        {
            value = new Measurement<TUnit>(bare, fallbackUnit);
            return true;
        }

        var split = SplitNumberAndUnit(normalized);
        if (split == null)
            return false;

        var (number, unit) = split.Value;
        if (aliases.TryGetValue(unit, out var canonical))
            unit = canonical;

        // The library's parse is case-sensitive and takes "<number><unit>" with no space.
        return Measurement<TUnit>.TryParse(CultureInfo.InvariantCulture, number + unit, out value);
    }

    /// <summary>
    /// Splits "3078.800 fps" into ("3078.800", "fps"). Null when the text does not start with a number or
    /// carries no unit at all.
    /// </summary>
    private static (string Number, string Unit)? SplitNumberAndUnit(string text)
    {
        int i = 0;
        while (i < text.Length && (char.IsDigit(text[i]) || text[i] == '.' || text[i] == '-' || text[i] == '+'))
            i++;

        if (i == 0 || i == text.Length)
            return null;

        var number = text[..i];
        var unit = text[i..].Trim();

        if (unit.Length == 0 ||
            !double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            return null;

        return (number, unit);
    }

    /// <summary>
    /// Rewrites a decimal comma to a dot. Necessary because the library parses with
    /// <see cref="NumberStyles.AllowThousands"/> under the invariant culture, where a comma is the group
    /// separator — so "780,2m/s" would otherwise parse as 7802 m/s instead of failing or being 780.2.
    /// </summary>
    private static string NormalizeDecimalMark(string text) =>
        text.Contains(',') ? text.Replace(',', '.') : text;
}
