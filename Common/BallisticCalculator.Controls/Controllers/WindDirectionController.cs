using BallisticCalculator.Controls.Models;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BallisticCalculator.Controls.Tests")]

namespace BallisticCalculator.Controls.Controllers;

/// <summary>
/// Controller for WindDirectionControl - pure geometry logic, no UI dependencies
///
/// Direction convention (where wind COMES FROM):
/// - 0° = from behind shooter (tailwind toward target)
/// - 90° = from shooter's right (crosswind blowing left)
/// - 180° = from in front (headwind toward shooter)
/// - 270° = from shooter's left (crosswind blowing right)
///
/// Visual: shooter at center facing UP toward target at top of control.
/// Arrow shows the wind direction toward the shooter - starts at circle edge, points to center.
/// </summary>
public class WindDirectionController
{
    private const double ArrowHeadAngleOffset = 30.0; // degrees
    private const double ArrowHeadLengthRatio = 0.2;  // relative to radius

    /// <summary>
    /// Calculate all arrow geometry from control dimensions and direction
    /// </summary>
    /// <param name="width">Control width</param>
    /// <param name="height">Control height</param>
    /// <param name="directionDegrees">Wind direction in degrees (0-360)</param>
    /// <returns>Arrow geometry for rendering</returns>
    public WindArrow CalculateArrow(double width, double height, double directionDegrees)
    {
        double cx = width / 2.0;
        double cy = height / 2.0;
        double radius = Math.Min(cx, cy);

        // Convert direction to radians and reverse (arrow points FROM wind source TO center)
        double directionRadians = DegreesToRadians(directionDegrees);
        double reversed = directionRadians + Math.PI;

        // Calculate arrow start point (at circle edge)
        // Using same coordinate system as old WinForms: Y increases downward
        double startX = cx - radius * Math.Sin(reversed);
        double startY = cy - radius * Math.Cos(reversed);

        // Arrow end point is at center
        double endX = cx;
        double endY = cy;

        // Calculate arrowhead points (two short lines from center at ±30°)
        double headLength = radius * ArrowHeadLengthRatio;
        double head1Radians = directionRadians - DegreesToRadians(ArrowHeadAngleOffset);
        double head2Radians = directionRadians + DegreesToRadians(ArrowHeadAngleOffset);

        double head1X = cx - headLength * Math.Sin(head1Radians + Math.PI);
        double head1Y = cy - headLength * Math.Cos(head1Radians + Math.PI);

        double head2X = cx - headLength * Math.Sin(head2Radians + Math.PI);
        double head2Y = cy - headLength * Math.Cos(head2Radians + Math.PI);

        return new WindArrow
        {
            StartX = startX,
            StartY = startY,
            EndX = endX,
            EndY = endY,
            Head1X = head1X,
            Head1Y = head1Y,
            Head2X = head2X,
            Head2Y = head2Y,
            CenterX = cx,
            CenterY = cy,
            Radius = radius
        };
    }

    /// <summary>
    /// Calculate direction (0-360°) from click position within control
    /// </summary>
    /// <param name="width">Control width</param>
    /// <param name="height">Control height</param>
    /// <param name="clickX">Click X coordinate</param>
    /// <param name="clickY">Click Y coordinate</param>
    /// <returns>Direction in degrees (0-360)</returns>
    public double DirectionFromClick(double width, double height, double clickX, double clickY)
    {
        double cx = width / 2.0;
        double cy = height / 2.0;

        double dx = clickX - cx;
        double dy = clickY - cy;

        // Using atan2(dx, dy) to match the coordinate system:
        // 0° at bottom, 90° at right, 180° at top, 270° at left
        double radians = Math.Atan2(dx, dy);
        double degrees = RadiansToDegrees(radians);

        return NormalizeAngle(degrees);
    }

    /// <summary>
    /// Normalize angle to 0-360 range
    /// </summary>
    /// <param name="degrees">Angle in degrees</param>
    /// <returns>Normalized angle (0-360)</returns>
    public double NormalizeAngle(double degrees)
    {
        degrees = degrees % 360.0;
        if (degrees < 0)
            degrees += 360.0;
        return degrees;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;
}
