namespace BallisticCalculator.Controls.Models;

/// <summary>
/// Wrapper for unit display in ComboBox
/// </summary>
public class UnitItem
{
    public object Unit { get; }
    public string Name { get; }

    public UnitItem(object unit, string name)
    {
        Unit = unit;
        Name = name;
    }
}
