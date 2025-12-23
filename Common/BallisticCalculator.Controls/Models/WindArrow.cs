namespace BallisticCalculator.Controls.Models;

/// <summary>
/// Contains all geometry needed to draw the wind direction arrow
/// </summary>
public readonly struct WindArrow
{
    /// <summary>
    /// Main arrow line start point (at circle edge)
    /// </summary>
    public double StartX { get; init; }
    public double StartY { get; init; }

    /// <summary>
    /// Main arrow line end point (at center)
    /// </summary>
    public double EndX { get; init; }
    public double EndY { get; init; }

    /// <summary>
    /// First arrowhead line endpoint (from center)
    /// </summary>
    public double Head1X { get; init; }
    public double Head1Y { get; init; }

    /// <summary>
    /// Second arrowhead line endpoint (from center)
    /// </summary>
    public double Head2X { get; init; }
    public double Head2Y { get; init; }

    /// <summary>
    /// Circle center X coordinate
    /// </summary>
    public double CenterX { get; init; }

    /// <summary>
    /// Circle center Y coordinate
    /// </summary>
    public double CenterY { get; init; }

    /// <summary>
    /// Circle radius
    /// </summary>
    public double Radius { get; init; }
}
