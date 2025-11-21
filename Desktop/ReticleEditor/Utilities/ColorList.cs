using Avalonia.Controls;

namespace ReticleEditor.Utilities;

/// <summary>
/// Extension methods for populating ComboBox with standard HTML color names
/// </summary>
public static class ColorListExtensions
{
    /// <summary>
    /// Standard HTML color names used in reticle definitions
    /// </summary>
    private static readonly string[] StandardColors = new[]
    {
        "black", "white", "gray", "red", "green", "blue",
        "yellow", "cyan", "magenta", "orange", "pink", "purple",
        "brown", "navy", "teal", "lime", "olive", "maroon",
        "aqua", "fuchsia", "silver", "darkgray", "lightgray",
        "darkred", "darkgreen", "darkblue", "gold", "indigo"
    };

    /// <summary>
    /// Populates a ComboBox with standard color names
    /// </summary>
    /// <param name="comboBox">The ComboBox to populate</param>
    public static void PopulateWithColors(this ComboBox comboBox)
    {
        comboBox.Items.Clear();
        foreach (var color in StandardColors)
        {
            comboBox.Items.Add(color);
        }
    }
}
