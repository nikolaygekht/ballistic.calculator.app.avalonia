using BallisticCalculator;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Controls.Controllers;

public static class ReticleOverlayController
{
    /// <summary>
    /// Creates BDC distance labels for the reticle overlay.
    /// </summary>
    /// <param name="calculator">Calculator with trajectory and BDC points</param>
    /// <param name="measurementSystem">Determines distance units (yd vs m)</param>
    /// <param name="far">true for far BDC (past zero), false for near BDC (before zero)</param>
    /// <param name="reticleScale">1.0 for FFP, MaxZoom/CurrentZoom for SFP</param>
    public static ReticleElementsCollection CreateBdcOverlay(
        TrajectoryToReticleCalculator calculator,
        MeasurementSystem measurementSystem,
        bool far,
        double reticleScale = 1.0)
    {
        var elements = new ReticleElementsCollection();

        calculator.UpdatePoints(reticleScale);

        var distanceUnits = measurementSystem == MeasurementSystem.Imperial
            ? DistanceUnit.Yard
            : DistanceUnit.Meter;

        for (int i = 0; i < calculator.Points.Count; i++)
        {
            var point = calculator.Points[i];
            if ((point.Location == TrajectoryToReticleCalculator.PointLocation.Far) != far)
                continue;

            var value = point.Distance.In(distanceUnits);
            var text = value.ToString("N0");
            var element = new ReticleText
            {
                Text = text,
                Position = new ReticlePosition(
                    point.BDCPoint.Position.X + point.BDCPoint.TextOffset,
                    point.BDCPoint.Position.Y - point.BDCPoint.TextHeight / 2),
                TextHeight = point.BDCPoint.TextHeight,
                Color = "blue",
            };
            elements.Add(element);
        }

        return elements;
    }

    /// <summary>
    /// Creates a target rectangle overlay at the given distance on the reticle.
    /// </summary>
    /// <param name="calculator">Calculator with trajectory data</param>
    /// <param name="targetWidth">Physical width of target</param>
    /// <param name="targetHeight">Physical height of target</param>
    /// <param name="targetDistance">Distance to target</param>
    /// <param name="reticleScale">1.0 for FFP, MaxZoom/CurrentZoom for SFP</param>
    public static ReticleElement? CreateTargetOverlay(
        TrajectoryToReticleCalculator calculator,
        Measurement<DistanceUnit> targetWidth,
        Measurement<DistanceUnit> targetHeight,
        Measurement<DistanceUnit> targetDistance,
        double reticleScale = 1.0)
    {
        if (targetWidth.Value < 0.01 || targetHeight.Value < 0.01 ||
            targetDistance.Value < 0.01)
            return null;

        var item = calculator.FindDistance(targetDistance);
        if (item == null)
            return null;

        var angularWidth = CalculateAngularSize(targetWidth, targetDistance, reticleScale);
        var angularHeight = CalculateAngularSize(targetHeight, targetDistance, reticleScale);

        return new ReticleRectangle
        {
            TopLeft = new ReticlePosition(
                -item.WindageAdjustment / reticleScale - angularWidth / 2,
                item.DropAdjustment / reticleScale + angularHeight / 2),
            Size = new ReticlePosition(angularWidth, angularHeight),
            Color = "red",
        };
    }

    /// <summary>
    /// Calculates the angular size of a physical object at a given distance.
    /// </summary>
    public static Measurement<AngularUnit> CalculateAngularSize(
        Measurement<DistanceUnit> targetSize,
        Measurement<DistanceUnit> targetDistance,
        double reticleScale = 1.0)
    {
        return (targetSize.In(DistanceUnit.Centimeter) /
                (targetDistance.In(DistanceUnit.Meter) / 100))
            .As(AngularUnit.CmPer100Meters) / reticleScale;
    }
}
