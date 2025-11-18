using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BallisticCalculator.Controls.Tests")]

namespace BallisticCalculator.Controls.Controllers;

/// <summary>
/// Controller for BallisticCoefficient control - pure logic, no UI dependencies
/// </summary>
public class BallisticCoefficientController
{
    // Configuration properties
    public double Increment { get; set; } = 0.001;
    public double Minimum { get; set; } = 0.001;
    public double Maximum { get; set; } = 2.0;
    public int? DecimalPoints { get; set; } = 3;

    /// <summary>
    /// Gets all available drag tables in alphabetical order with GC (custom) last
    /// </summary>
    /// <param name="defaultIndex">Index of the default table (G1)</param>
    /// <returns>List of drag table information</returns>
    public IReadOnlyList<DragTableInfo> GetDragTables(out int defaultIndex)
    {
        // Get all enum values via reflection and create DragTableInfo objects
        var allEnumValues = Enum.GetValues<DragTableId>();
        var tables = new List<DragTableInfo>();
        DragTableInfo? gcTable = null;

        // Build list: all except GC
        foreach (var value in allEnumValues)
        {
            var table = new DragTableInfo(value, value.ToString());
            if (value == DragTableId.GC)
                gcTable = table;
            else
                tables.Add(table);
        }

        // Sort alphabetically by name
        tables.Sort((a, b) => a.Name.CompareTo(b.Name));

        // Add GC at the end
        if (gcTable != null)
            tables.Add(gcTable);

        // G1 is the default (should be first after alphabetical sort)
        defaultIndex = 0;
        for (int i = 0; i < tables.Count; i++)
        {
            if (tables[i].Value == DragTableId.G1)
            {
                defaultIndex = i;
                break;
            }
        }

        return tables.AsReadOnly();
    }

    /// <summary>
    /// Parses text into a BallisticCoefficient value
    /// </summary>
    /// <param name="text">Text to parse</param>
    /// <param name="table">Drag table to use</param>
    /// <param name="decimalPoints">Number of decimal points (null = no rounding)</param>
    /// <param name="culture">Culture for parsing (defaults to InvariantCulture)</param>
    /// <returns>BallisticCoefficient or null if invalid</returns>
    public BallisticCoefficient? Value(string text, DragTableInfo table, int? decimalPoints, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        culture ??= CultureInfo.InvariantCulture;

        // Try to parse the text
        if (!double.TryParse(text, NumberStyles.Float, culture, out double value))
            return null;

        // Validate that BC must be positive
        if (value <= 0)
            return null;

        // Note: Min/Max are only used for increment/decrement, not validation

        // Round if decimal points specified
        if (decimalPoints.HasValue)
        {
            value = Math.Round(value, decimalPoints.Value);
        }

        return new BallisticCoefficient(value, table.Value);
    }

    /// <summary>
    /// Parses a BallisticCoefficient into text and table info
    /// </summary>
    /// <param name="bc">BallisticCoefficient to parse</param>
    /// <param name="text">Output text representation</param>
    /// <param name="table">Output drag table info</param>
    /// <param name="decimalPoints">Number of decimal points (null = use default)</param>
    /// <param name="culture">Culture for formatting (defaults to InvariantCulture)</param>
    public void ParseValue(BallisticCoefficient bc, out string text, out DragTableInfo table, int? decimalPoints = null, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;
        decimalPoints ??= DecimalPoints;

        // Format the value with specified decimal points
        string format = $"F{decimalPoints}";
        text = bc.Value.ToString(format, culture);

        // Find the corresponding DragTableInfo
        var tables = GetDragTables(out _);
        table = tables.First(t => t.Value == bc.Table);
    }

    /// <summary>
    /// Increments or decrements the current value
    /// </summary>
    /// <param name="currentText">Current text value</param>
    /// <param name="table">Current drag table</param>
    /// <param name="increment">True to increment, false to decrement</param>
    /// <param name="culture">Culture for parsing/formatting</param>
    /// <returns>New text value</returns>
    public string IncrementValue(string currentText, DragTableInfo table, bool increment, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;

        // Parse current value
        if (!double.TryParse(currentText, NumberStyles.Float, culture, out double current))
        {
            // If empty or invalid, start from 0 (or Minimum if 0 is below Minimum)
            current = Math.Max(0, Minimum) - Increment;  // Subtract increment so first increment lands on 0 or Minimum
        }

        // Apply increment/decrement
        double newValue = increment ? current + Increment : current - Increment;

        // Clamp to min/max
        newValue = Math.Max(Minimum, Math.Min(Maximum, newValue));

        // Round to decimal points
        int decimalPoints = DecimalPoints ?? 3;
        newValue = Math.Round(newValue, decimalPoints);

        // Format back to string
        string format = $"F{decimalPoints}";
        return newValue.ToString(format, culture);
    }

    /// <summary>
    /// Validates if a character is allowed in the editor
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

        // Get decimal separator for the culture
        string decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;

        // Allow decimal separator if not already present
        if (character.ToString() == decimalSeparator)
        {
            return !currentText.Contains(decimalSeparator);
        }

        // Reject everything else (including minus sign - BC must be positive)
        return false;
    }
}
