using Avalonia.Controls;
using BallisticCalculator.Reticle.Data;

namespace ReticleEditor.Utilities;

/// <summary>
/// Extension methods for editing an element's line style (Solid / Dashed / Dotted) in a ComboBox.
/// A null <see cref="ReticleLineStyle"/> (legacy reticles) is treated as Solid.
/// </summary>
public static class LineStyleListExtensions
{
    private static readonly string[] Styles = { "Solid", "Dashed", "Dotted" };

    /// <summary>Populates a ComboBox with the available line styles.</summary>
    public static void PopulateWithLineStyles(this ComboBox comboBox)
    {
        comboBox.Items.Clear();
        foreach (var style in Styles)
            comboBox.Items.Add(style);
    }

    /// <summary>Selects the item matching the given style; null (legacy) selects Solid.</summary>
    public static void SelectLineStyle(this ComboBox comboBox, ReticleLineStyle? style)
    {
        comboBox.SelectedIndex = style switch
        {
            ReticleLineStyle.Dashed => 1,
            ReticleLineStyle.Dotted => 2,
            _ => 0,
        };
    }

    /// <summary>
    /// Returns the selected style, or null for Solid — storing null keeps Solid elements identical to
    /// legacy reticles that never had a style.
    /// </summary>
    public static ReticleLineStyle? SelectedLineStyle(this ComboBox comboBox)
    {
        return comboBox.SelectedIndex switch
        {
            1 => ReticleLineStyle.Dashed,
            2 => ReticleLineStyle.Dotted,
            _ => null,
        };
    }
}
