using BallisticCalculator;
using BallisticCalculator.Tools;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Controls.Controllers;

/// <summary>
/// Computes the derived output values shown on the summary panel: the zeroing sight adjustments,
/// the point-blank "dead zone" (bottom-aimed target), the near/far zero ranges, and the distance
/// at which the bullet becomes subsonic. Pure logic, no UI.
/// </summary>
public class SummaryController
{
    /// <summary>Bottom-aimed vital-zone height: ~half a metre / 20 inches.</summary>
    private static Measurement<DistanceUnit> TargetSize(MeasurementSystem system)
        => system == MeasurementSystem.Metric
            ? new Measurement<DistanceUnit>(500, DistanceUnit.Millimeter)
            : new Measurement<DistanceUnit>(20, DistanceUnit.Inch);

    /// <summary>
    /// Compute the summary values. <paramref name="fineTrajectory"/> is the shared fine trajectory
    /// (from <see cref="ShotTrajectoryCalculator.CalculateFine"/>); the coarse table trajectory cannot
    /// resolve the point-blank corridor.
    /// </summary>
    public SummaryResult Compute(ShotData? shotData, TrajectoryPoint[]? fineTrajectory, MeasurementSystem system)
    {
        var zero = ZeroingCalculator.Compute(shotData);

        Measurement<DistanceUnit>? deadZone = null;
        Measurement<DistanceUnit>? nearZero = null;
        Measurement<DistanceUnit>? farZero = null;
        Measurement<DistanceUnit>? subsonic = null;

        // Defensive: drop any trailing nulls before analysis.
        var points = fineTrajectory is null
            ? System.Array.Empty<TrajectoryPoint>()
            : System.Linq.Enumerable.ToArray(System.Linq.Enumerable.TakeWhile(fineTrajectory, p => p != null));

        if (points.Length > 1)
        {
            try
            {
                var pbr = PointBlankRange.Analyze(points, TargetSize(system), PointBlankAim.Bottom);
                deadZone = pbr.DangerSpace;
                nearZero = pbr.NearZero;
                farZero = pbr.FarZero;
            }
            catch
            {
                // The trajectory may not extend past the corridor; leave point-blank values unset.
            }

            subsonic = FindSubsonicDistance(points);
        }

        return new SummaryResult
        {
            ZeroVertical = zero?.ZeroDropAdjustment,
            ZeroHorizontal = zero?.ZeroWindageAdjustment,
            DeadZone = deadZone,
            NearZero = nearZero,
            FarZero = farZero,
            SubsonicDistance = subsonic,
        };
    }

    /// <summary>
    /// Distance at which the bullet's speed crosses Mach 1, linearly interpolated between the two
    /// bracketing trajectory points. Null when the bullet stays supersonic across the whole path.
    /// </summary>
    public static Measurement<DistanceUnit>? FindSubsonicDistance(TrajectoryPoint[] trajectory)
    {
        if (trajectory.Length == 0 || trajectory[0] == null)
            return null;

        if (trajectory[0].Mach < 1.0)
            return trajectory[0].Distance;

        for (var i = 1; i < trajectory.Length; i++)
        {
            if (trajectory[i] == null)
                break;

            if (trajectory[i].Mach < 1.0)
            {
                var p0 = trajectory[i - 1];
                var p1 = trajectory[i];
                var fraction = (p0.Mach - 1.0) / (p0.Mach - p1.Mach);
                return p0.Distance + (p1.Distance - p0.Distance) * fraction;
            }
        }

        return null;
    }
}

/// <summary>Computed summary values (raw measurements; the panel formats them for display).</summary>
public sealed class SummaryResult
{
    public Measurement<AngularUnit>? ZeroVertical { get; init; }
    public Measurement<AngularUnit>? ZeroHorizontal { get; init; }
    public Measurement<DistanceUnit>? DeadZone { get; init; }
    public Measurement<DistanceUnit>? NearZero { get; init; }
    public Measurement<DistanceUnit>? FarZero { get; init; }
    public Measurement<DistanceUnit>? SubsonicDistance { get; init; }
}
