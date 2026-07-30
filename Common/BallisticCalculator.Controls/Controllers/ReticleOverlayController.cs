using BallisticCalculator;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Tools;
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

        // The mark labels are bare numbers otherwise, and nothing else on the canvas says which system
        // they are in — a 500 reads the same whether it means yards or metres.
        var unitName = Measurement<DistanceUnit>.GetUnitName(distanceUnits);

        for (int i = 0; i < calculator.Points.Count; i++)
        {
            var point = calculator.Points[i];
            if ((point.Location == TrajectoryToReticleCalculator.PointLocation.Far) != far)
                continue;

            var value = point.Distance.In(distanceUnits);
            var text = value.ToString("N0") + unitName;
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
        double reticleScale = 1.0,
        Measurement<AngularUnit>? offsetX = null,
        Measurement<AngularUnit>? offsetY = null)
    {
        if (targetWidth.Value < 0.01 || targetHeight.Value < 0.01 ||
            targetDistance.Value < 0.01)
            return null;

        var item = calculator.FindDistance(targetDistance);
        if (item == null)
            return null;

        var angularWidth = CalculateAngularSize(targetWidth, targetDistance, reticleScale);
        var angularHeight = CalculateAngularSize(targetHeight, targetDistance, reticleScale);

        // The offset moves the box away from the hold, in the reticle's own coordinates: X positive to
        // the RIGHT, Y positive UP. It is not divided by reticleScale, because it names a place on the
        // reticle rather than a trajectory angle to be projected onto it. The point is to let a target
        // be parked on a ranging feature — the stadia at the lower left of the Specter reticles, say —
        // so the scale can actually be tried against a target of known size.
        var centerX = -item.WindageAdjustment / reticleScale + (offsetX ?? Measurement<AngularUnit>.ZERO);
        var centerY = item.DropAdjustment / reticleScale + (offsetY ?? Measurement<AngularUnit>.ZERO);

        return new ReticleRectangle
        {
            TopLeft = new ReticlePosition(
                centerX - angularWidth / 2,
                centerY + angularHeight / 2),
            Size = new ReticlePosition(angularWidth, angularHeight),
            Color = "red",
        };
    }

    /// <summary>
    /// Creates a dotted "aim here" overlay for a moving target: the target box shifted
    /// horizontally by the computed lead for the given crossing motion.
    /// </summary>
    /// <param name="calculator">Calculator with trajectory data</param>
    /// <param name="targetWidth">Physical width of target</param>
    /// <param name="targetHeight">Physical height of target</param>
    /// <param name="targetDistance">Distance to target</param>
    /// <param name="targetSpeed">Target speed</param>
    /// <param name="movingDirection">
    /// Target motion direction (wind convention: 0 = toward the shooter's line of sight/no lead,
    /// 90 = crossing from the right, 270 = crossing from the left).
    /// </param>
    /// <param name="reticleScale">1.0 for FFP, MaxZoom/CurrentZoom for SFP</param>
    public static ReticleRectangle? CreateMovingTargetOverlay(
        TrajectoryToReticleCalculator calculator,
        Measurement<DistanceUnit> targetWidth,
        Measurement<DistanceUnit> targetHeight,
        Measurement<DistanceUnit> targetDistance,
        Measurement<VelocityUnit> targetSpeed,
        Measurement<AngularUnit> movingDirection,
        double reticleScale = 1.0,
        Measurement<AngularUnit>? offsetX = null,
        Measurement<AngularUnit>? offsetY = null)
    {
        if (targetWidth.Value < 0.01 || targetHeight.Value < 0.01 ||
            targetDistance.Value < 0.01)
            return null;

        var item = calculator.FindDistance(targetDistance);
        if (item == null)
            return null;

        var angularWidth = CalculateAngularSize(targetWidth, targetDistance, reticleScale);
        var angularHeight = CalculateAngularSize(targetHeight, targetDistance, reticleScale);

        // Angular lead follows the trajectory windage sign (left +, right -).
        var lead = MovingTargetLead.LeadAngle(targetSpeed, movingDirection, item);

        // The box is a hold-over mark: where to place the reticle relative to the target to hit it.
        // The static target box centers at reticle-X = -WindageAdjustment (the windage hold-off).
        // Lead is another hold-off the shooter applies in the direction of the target's motion, so
        // it composes with the same sign: a target crossing to the left (positive lead) means hold
        // left of the target (reticle-left, negative X). Drop is unchanged (lead is horizontal only).
        // The same offset as the static box, so the two stay together when one is parked on a
        // ranging feature.
        var centerX = -item.WindageAdjustment / reticleScale - lead / reticleScale
                      + (offsetX ?? Measurement<AngularUnit>.ZERO);
        var centerY = item.DropAdjustment / reticleScale + (offsetY ?? Measurement<AngularUnit>.ZERO);

        return new ReticleRectangle
        {
            TopLeft = new ReticlePosition(
                centerX - angularWidth / 2,
                centerY + angularHeight / 2),
            Size = new ReticlePosition(angularWidth, angularHeight),
            Color = "red",
        };
    }

    /// <summary>
    /// Calculates the angular lead (hold-off) for a moving target at the given distance, or null
    /// if the distance is invalid or outside the trajectory. Sign follows the trajectory windage
    /// convention (left +, right -).
    /// </summary>
    public static Measurement<AngularUnit>? CalculateMovingTargetLead(
        TrajectoryToReticleCalculator calculator,
        Measurement<DistanceUnit> targetDistance,
        Measurement<VelocityUnit> targetSpeed,
        Measurement<AngularUnit> movingDirection)
    {
        if (targetDistance.Value < 0.01)
            return null;

        var item = calculator.FindDistance(targetDistance);
        if (item == null)
            return null;

        return MovingTargetLead.LeadAngle(targetSpeed, movingDirection, item);
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
