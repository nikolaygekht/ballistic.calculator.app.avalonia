using Avalonia;
using Avalonia.Controls;
using System.Collections.Generic;

namespace ReticleEditor.Models;

/// <summary>
/// Stores dialog size for persistence
/// </summary>
public class DialogSize
{
    public double Width { get; set; } = 450;
    public double Height { get; set; } = 420;
}

/// <summary>
/// Stores window state for persistence
/// </summary>
public class WindowState
{
    public double Width { get; set; } = 1200;
    public double Height { get; set; } = 800;
    public double X { get; set; } = 100;
    public double Y { get; set; } = 100;
    public bool IsMaximized { get; set; }
    public double FontSize { get; set; } = 13;
    public double SplitterPosition { get; set; } = 800; // Position of the grid splitter
    public Dictionary<string, DialogSize> DialogSizes { get; set; } = new(); // Sizes for each dialog type

    /// <summary>
    /// Apply this state to a window
    /// </summary>
    public void ApplyToWindow(Window window)
    {
        if (window == null) return;

        window.Width = Width;
        window.Height = Height;
        window.Position = new PixelPoint((int)X, (int)Y);
        window.WindowState = IsMaximized ? Avalonia.Controls.WindowState.Maximized : Avalonia.Controls.WindowState.Normal;
    }

    /// <summary>
    /// Create state from a window
    /// </summary>
    public static WindowState FromWindow(Window window)
    {
        if (window == null)
            return new WindowState();

        return new WindowState
        {
            Width = window.Width,
            Height = window.Height,
            X = window.Position.X,
            Y = window.Position.Y,
            IsMaximized = window.WindowState == Avalonia.Controls.WindowState.Maximized
        };
    }
}
