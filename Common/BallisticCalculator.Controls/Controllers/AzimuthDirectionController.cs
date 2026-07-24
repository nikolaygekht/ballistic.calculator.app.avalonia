using BallisticCalculator.Controls.Models;

namespace BallisticCalculator.Controls.Controllers;

/// <summary>
/// Controller for <c>AzimuthDirectionControl</c> — pure geometry, no UI dependencies.
///
/// Compass convention (bearing of the line of fire, shooter -> target):
/// - 0°   = North (UP)
/// - 90°  = East  (right)
/// - 180° = South (down)
/// - 270° = West  (left)
///
/// Visual: shooter at the center; the arrow points OUTWARD from the center toward the target
/// along the bearing (the opposite of the wind indicator, whose arrow points inward).
/// </summary>
public class AzimuthDirectionController
{
    private const double ArrowHeadAngleOffset = 30.0; // degrees
    private const double ArrowHeadLengthRatio = 0.25; // relative to radius

    /// <summary>Calculate arrow geometry (reuses <see cref="WindArrow"/>) for the given bearing.</summary>
    public WindArrow CalculateArrow(double width, double height, double directionDegrees)
    {
        double cx = width / 2.0;
        double cy = height / 2.0;
        double radius = System.Math.Min(cx, cy);

        // Tip sits on the circle edge along the bearing; a unit vector for bearing φ
        // (0 = up, clockwise) is (sin φ, -cos φ) in screen coordinates (Y increases downward).
        double dir = DegreesToRadians(directionDegrees);
        double tipX = cx + radius * System.Math.Sin(dir);
        double tipY = cy - radius * System.Math.Cos(dir);

        // Arrowhead barbs point back from the tip at ±30° off the reverse bearing.
        double headLength = radius * ArrowHeadLengthRatio;
        double barb1 = dir + DegreesToRadians(180 - ArrowHeadAngleOffset);
        double barb2 = dir + DegreesToRadians(180 + ArrowHeadAngleOffset);

        return new WindArrow
        {
            // Shaft: center -> tip.
            StartX = cx,
            StartY = cy,
            EndX = tipX,
            EndY = tipY,
            Head1X = tipX + headLength * System.Math.Sin(barb1),
            Head1Y = tipY - headLength * System.Math.Cos(barb1),
            Head2X = tipX + headLength * System.Math.Sin(barb2),
            Head2Y = tipY - headLength * System.Math.Cos(barb2),
            CenterX = cx,
            CenterY = cy,
            Radius = radius
        };
    }

    /// <summary>Bearing (0-360°, 0 = up, clockwise) from a click position within the control.</summary>
    public double DirectionFromClick(double width, double height, double clickX, double clickY)
    {
        double cx = width / 2.0;
        double cy = height / 2.0;

        double dx = clickX - cx;
        double dy = clickY - cy;

        // atan2(dx, -dy): up = 0°, right = 90°, down = 180°, left = 270°.
        double degrees = RadiansToDegrees(System.Math.Atan2(dx, -dy));
        return NormalizeAngle(degrees);
    }

    /// <summary>Normalize an angle to the 0-360 range.</summary>
    public double NormalizeAngle(double degrees)
    {
        degrees %= 360.0;
        if (degrees < 0)
            degrees += 360.0;
        return degrees;
    }

    private static double DegreesToRadians(double degrees) => degrees * System.Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / System.Math.PI;
}
