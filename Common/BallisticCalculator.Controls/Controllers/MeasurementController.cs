using Gehtsoft.Measurements;
using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BallisticCalculator.Controls.Tests")]

namespace BallisticCalculator.Controls.Controllers;

/// <summary>
/// Generic controller for Measurement control - pure logic, no UI dependencies
/// Works with any measurement type from Gehtsoft.Measurements library
/// </summary>
/// <typeparam name="T">Unit enum type (DistanceUnit, VelocityUnit, WeightUnit, etc.)</typeparam>
public class MeasurementController<T> where T : Enum
{
    // Configuration properties
    public double Increment { get; set; } = 1.0;
    public double Minimum { get; set; } = -10000.0;
    public double Maximum { get; set; } = 10000.0;
    public int? DecimalPoints { get; set; } = null;

    /// <summary>
    /// Gets all available units for the measurement type T
    /// </summary>
    /// <param name="defaultIndex">Index of the default unit (base unit)</param>
    /// <returns>List of unit tuples (enum value, display name)</returns>
    public IReadOnlyList<(T Unit, string Name)> GetUnits(out int defaultIndex)
    {
        var unitNames = Measurement<T>.GetUnitNames();
        var units = new List<(T, string)>();

        foreach (var tuple in unitNames)
        {
            units.Add((tuple.Item1, tuple.Item2));
        }

        // Default is the base unit
        T baseUnit = Measurement<T>.BaseUnit;
        defaultIndex = 0;
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].Item1.Equals(baseUnit))
            {
                defaultIndex = i;
                break;
            }
        }

        return units.AsReadOnly();
    }

    /// <summary>
    /// Parses text into a Measurement value
    /// </summary>
    /// <param name="text">Text to parse</param>
    /// <param name="unit">Unit to use</param>
    /// <param name="accuracy">Number of decimal points (null = no rounding)</param>
    /// <param name="culture">Culture for parsing (defaults to InvariantCulture)</param>
    /// <returns>Measurement object or null if invalid</returns>
    public Measurement<T>? Value(string text, T unit, int? accuracy, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        culture ??= CultureInfo.InvariantCulture;

        // Try to parse the text (remove thousands separators first)
        string cleanText = RemoveThousandsSeparator(text, culture);
        if (!double.TryParse(cleanText, NumberStyles.Float, culture, out double value))
            return null;

        // Note: Min/Max are only used for increment/decrement, not validation

        // Create measurement
        return new Measurement<T>(value, unit);
    }

    /// <summary>
    /// Parses a Measurement object into text and unit
    /// </summary>
    /// <param name="measurement">Measurement object to parse</param>
    /// <param name="text">Output text representation</param>
    /// <param name="unit">Output unit</param>
    /// <param name="decimalPoints">Number of decimal points (null = use default)</param>
    /// <param name="culture">Culture for formatting (defaults to InvariantCulture)</param>
    public void ParseValue(Measurement<T> measurement, out string text, out T unit, int? decimalPoints = null, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;
        decimalPoints ??= DecimalPoints;

        // Get value and unit from the measurement object
        double numericValue = measurement.Value;
        unit = measurement.Unit;

        // Format the value with specified decimal points and thousands separator
        text = FormatNumber(numericValue, decimalPoints, culture);
    }

    /// <summary>
    /// Formats a number with culture-specific separators and decimal points
    /// Supports thousands separator for integer part and full precision for decimal part
    /// </summary>
    private string FormatNumber(double value, int? decimalPoints, CultureInfo culture)
    {
        if (decimalPoints.HasValue)
        {
            // Use standard N format with thousands separator
            string format = $"N{decimalPoints.Value}";
            return value.ToString(format, culture);
        }
        else
        {
            // No decimal points specified - use default formatting
            return value.ToString("G", culture);
        }
    }

    /// <summary>
    /// Increments or decrements the current value
    /// </summary>
    /// <param name="currentText">Current text value</param>
    /// <param name="increment">True to increment, false to decrement</param>
    /// <param name="culture">Culture for parsing/formatting</param>
    /// <returns>New text value</returns>
    public string IncrementValue(string currentText, bool increment, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        // Parse current value (remove thousands separators)
        string cleanText = RemoveThousandsSeparator(currentText, culture);
        if (!double.TryParse(cleanText, NumberStyles.Float, culture, out double current))
        {
            // If empty or invalid, start from 0 (or Minimum if 0 is below Minimum)
            current = Math.Max(0, Minimum) - Increment;  // Subtract increment so first increment lands on 0 or Minimum
        }

        // Apply increment/decrement
        double newValue = increment ? current + Increment : current - Increment;

        // Clamp to min/max
        newValue = Math.Max(Minimum, Math.Min(Maximum, newValue));

        // Round to decimal points if specified
        if (DecimalPoints.HasValue)
        {
            newValue = Math.Round(newValue, DecimalPoints.Value);
        }

        // Format back to string
        return FormatNumber(newValue, DecimalPoints, culture);
    }

    /// <summary>
    /// Removes thousands separator from text for parsing
    /// </summary>
    private string RemoveThousandsSeparator(string text, CultureInfo culture)
    {
        string thousandsSeparator = culture.NumberFormat.NumberGroupSeparator;
        return text.Replace(thousandsSeparator, "");
    }

    /// <summary>
    /// Validates if a character is allowed in the editor
    /// Supports digits, decimal separator, +/- signs, and thousands separator
    /// </summary>
    /// <param name="currentText">Current text in editor</param>
    /// <param name="caretIndex">Current caret position</param>
    /// <param name="selectionLength">Length of current selection</param>
    /// <param name="character">Character to validate</param>
    /// <param name="culture">Culture for decimal separator</param>
    /// <returns>True if character is allowed</returns>
    public bool AllowKeyInEditor(string currentText, int caretIndex, int selectionLength, char character, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        // Allow digits
        if (char.IsDigit(character))
            return true;

        // Get separators for the culture
        string decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
        string thousandsSeparator = culture.NumberFormat.NumberGroupSeparator;

        // Allow decimal separator if not already present
        if (character.ToString() == decimalSeparator)
        {
            // Remove selected text to simulate what text would be after character insertion
            string textAfterSelection = currentText.Remove(caretIndex, selectionLength);
            return !textAfterSelection.Contains(decimalSeparator);
        }

        // Allow thousands separator (but not at the beginning)
        if (character.ToString() == thousandsSeparator && caretIndex > 0)
        {
            return true;
        }

        // Allow +/- signs only at the beginning
        if ((character == '+' || character == '-') && caretIndex == 0)
        {
            // Remove selected text to simulate what text would be after character insertion
            string textAfterSelection = currentText.Remove(0, selectionLength);
            return !textAfterSelection.StartsWith("+") && !textAfterSelection.StartsWith("-");
        }

        // Reject everything else
        return false;
    }

    /// <summary>
    /// Gets the display name for a unit
    /// </summary>
    public string GetUnitName(T unit)
    {
        return Measurement<T>.GetUnitName(unit);
    }
}
