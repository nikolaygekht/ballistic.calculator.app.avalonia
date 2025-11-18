using BallisticCalculator;

namespace BallisticCalculator.Controls.Models;

/// <summary>
/// Information about a drag table for ballistic coefficients
/// </summary>
public class DragTableInfo
{
    /// <summary>
    /// The drag table identifier
    /// </summary>
    public DragTableId Value { get; init; }

    /// <summary>
    /// Display name (e.g., "G1", "G7")
    /// </summary>
    public string Name { get; init; } = string.Empty;

    public DragTableInfo(DragTableId value, string name)
    {
        Value = value;
        Name = name;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is DragTableInfo other)
            return Value == other.Value;
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
