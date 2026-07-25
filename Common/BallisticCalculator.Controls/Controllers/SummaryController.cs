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
    public static Measurement<DistanceUnit> TargetSize(MeasurementSystem system)
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

        Measurement<DistanceUnit>? deadZoneMin = null;
        Measurement<DistanceUnit>? deadZoneMax = null;
        Measurement<DistanceUnit>? deadZoneCenterMin = null;
        Measurement<DistanceUnit>? deadZoneCenterMax = null;
        Measurement<DistanceUnit>? nearZero = null;
        Measurement<DistanceUnit>? farZero = null;
        Measurement<DistanceUnit>? subsonic = null;

        // Defensive: drop any trailing nulls before analysis.
        var points = fineTrajectory is null
            ? System.Array.Empty<TrajectoryPoint>()
            : System.Linq.Enumerable.ToArray(System.Linq.Enumerable.TakeWhile(fineTrajectory, p => p != null));

        if (points.Length > 1)
        {
            // Near/far zero are line-of-sight crossings and always well-defined when the path crosses
            // the sight line; compute them directly so they don't disappear when the corridor analysis
            // below can't close (e.g. the trajectory ends before leaving the vital-zone corridor).
            (nearZero, farZero) = FindLineOfSightCrossings(points);

            (deadZoneMin, deadZoneMax) = Corridor(points, TargetSize(system), PointBlankAim.Bottom);
            (deadZoneCenterMin, deadZoneCenterMax) = Corridor(points, TargetSize(system), PointBlankAim.Center);

            subsonic = FindSubsonicDistance(points);
        }

        return new SummaryResult
        {
            ZeroVertical = zero?.ZeroDropAdjustment,
            ZeroHorizontal = zero?.ZeroWindageAdjustment,
            TargetSize = TargetSize(system),
            DeadZoneMin = deadZoneMin,
            DeadZoneMax = deadZoneMax,
            DeadZoneCenterMin = deadZoneCenterMin,
            DeadZoneCenterMax = deadZoneCenterMax,
            NearZero = nearZero,
            FarZero = farZero,
            SubsonicDistance = subsonic,
        };
    }

    /// <summary>
    /// The point-blank (dead-zone) corridor edges for the given aim, or (null, null) when the
    /// trajectory does not fully enter and leave the corridor.
    /// </summary>
    private static (Measurement<DistanceUnit>? Min, Measurement<DistanceUnit>? Max) Corridor(
        TrajectoryPoint[] points, Measurement<DistanceUnit> targetSize, PointBlankAim aim)
    {
        try
        {
            var pbr = PointBlankRange.Analyze(points, targetSize, aim);
            return (pbr.MinimumRange, pbr.MaximumRange);
        }
        catch
        {
            return (null, null);
        }
    }

    /// <summary>
    /// Finds the near and far zero: the ascending and (subsequent) descending ranges at which the
    /// path crosses the line of sight (Drop = 0), linearly interpolated. Either is null when the
    /// corresponding crossing does not occur within the trajectory.
    /// </summary>
    public static (Measurement<DistanceUnit>? Near, Measurement<DistanceUnit>? Far) FindLineOfSightCrossings(
        TrajectoryPoint[] trajectory)
    {
        Measurement<DistanceUnit>? near = null;
        Measurement<DistanceUnit>? far = null;

        for (var i = 1; i < trajectory.Length; i++)
        {
            if (trajectory[i - 1] == null || trajectory[i] == null)
                break;

            double d0 = trajectory[i - 1].Drop.In(DistanceUnit.Meter);
            double d1 = trajectory[i].Drop.In(DistanceUnit.Meter);
            double r0 = trajectory[i - 1].Distance.In(DistanceUnit.Meter);
            double r1 = trajectory[i].Distance.In(DistanceUnit.Meter);

            if (near == null && d0 < 0 && d1 >= 0)
            {
                near = Interpolate(r0, d0, r1, d1);
            }
            else if (near != null && far == null && d0 >= 0 && d1 < 0)
            {
                far = Interpolate(r0, d0, r1, d1);
                break;
            }
        }

        return (near, far);
    }

    private static Measurement<DistanceUnit> Interpolate(double r0, double d0, double r1, double d1)
    {
        var fraction = (0 - d0) / (d1 - d0);
        return new Measurement<DistanceUnit>(r0 + (r1 - r0) * fraction, DistanceUnit.Meter);
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

    /// <summary>The bottom-aimed vital-zone height used for the dead-zone corridor.</summary>
    public Measurement<DistanceUnit>? TargetSize { get; init; }

    /// <summary>Near edge of the point-blank (dead-zone) corridor for a bottom-aimed target.</summary>
    public Measurement<DistanceUnit>? DeadZoneMin { get; init; }

    /// <summary>Far edge of the point-blank (dead-zone) corridor for a bottom-aimed target.</summary>
    public Measurement<DistanceUnit>? DeadZoneMax { get; init; }

    /// <summary>Near edge of the point-blank (dead-zone) corridor for a center-aimed target.</summary>
    public Measurement<DistanceUnit>? DeadZoneCenterMin { get; init; }

    /// <summary>Far edge of the point-blank (dead-zone) corridor for a center-aimed target.</summary>
    public Measurement<DistanceUnit>? DeadZoneCenterMax { get; init; }

    public Measurement<DistanceUnit>? NearZero { get; init; }
    public Measurement<DistanceUnit>? FarZero { get; init; }
    public Measurement<DistanceUnit>? SubsonicDistance { get; init; }
}
