using BallisticCalculator;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Types;

/// <summary>
/// The single source of truth for turning a <see cref="ShotData"/> into a trajectory. Both the main
/// (table/chart) trajectory and the reticle's fine trajectory go through here; the reticle only overrides
/// the output step, so everything else (range, zero, shot geometry, winds) matches the main trajectory.
/// </summary>
public static class ShotTrajectoryCalculator
{
    private static readonly Measurement<DistanceUnit> FineStep = new(2.5, DistanceUnit.Meter);
    private static readonly Measurement<DistanceUnit> FineMinimumDistance = new(1500, DistanceUnit.Meter);

    /// <summary>
    /// Fine-step trajectory reaching at least 1500 m (or the configured max distance, whichever is
    /// greater). Shared by the reticle (BDC marks) and the summary analysis (point-blank corridor +
    /// subsonic), which the coarse table trajectory can't resolve.
    /// </summary>
    public static TrajectoryPoint[]? CalculateFine(ShotData? shotData)
    {
        var configured = shotData?.Parameters?.MaximumDistance;
        var max = configured != null &&
                  configured.Value.In(DistanceUnit.Meter) >= FineMinimumDistance.In(DistanceUnit.Meter)
            ? configured.Value
            : FineMinimumDistance;
        return Calculate(shotData, stepOverride: FineStep, maxDistanceOverride: max);
    }

    /// <summary>
    /// Compute the shot trajectory, or null when the shot data is incomplete (ammunition, weapon, or
    /// parameters missing). <paramref name="stepOverride"/> replaces the output step and
    /// <paramref name="maxDistanceOverride"/> the maximum distance (both used by the reticle's fine
    /// trajectory); everything else matches the table trajectory.
    /// </summary>
    public static TrajectoryPoint[]? Calculate(ShotData? shotData,
        Measurement<DistanceUnit>? stepOverride = null,
        Measurement<DistanceUnit>? maxDistanceOverride = null)
    {
        if (shotData?.Ammunition?.Ammunition == null || shotData.Weapon == null || shotData.Parameters == null)
            return null;

        var ammo = shotData.Ammunition.Ammunition;
        var atmosphere = shotData.Atmosphere ?? new Atmosphere();
        var p = shotData.Parameters;
        var inputs = ZeroingCalculator.BuildInputs(shotData, atmosphere);

        var shot = new ShotParameters
        {
            BarrelAzimuth = p.BarrelAzimuth,
            CantAngle = p.CantAngle,
            MaximumDistance = maxDistanceOverride ?? p.MaximumDistance,
            ShotAngle = p.ShotAngle,
            ShotDropAdjustment = p.ShotDropAdjustment,
            ShotWindageAdjustment = p.ShotWindageAdjustment,
            Latitude = p.Latitude,
            Step = stepOverride ?? p.Step,
        };

        // GC ballistic coefficients need their custom .drg table supplied to both calls.
        var zeroTable = CustomDragTableLoader.ForAmmunition(inputs.ZeroAmmunition);
        var shotTable = CustomDragTableLoader.ForAmmunition(ammo);

        var calc = new TrajectoryCalculator();
        shot.Apply(calc.CalculateZeroParameters(
            inputs.ZeroAmmunition, inputs.ZeroAtmosphere, inputs.Rifle, inputs.ZeroParameters,
            shot: inputs.ZeroShot, wind: inputs.ZeroWind, dragTable: zeroTable));

        return TrimTrailingNulls(calc.Calculate(ammo, inputs.Rifle, atmosphere, shot, shotData.Winds, shotTable));
    }

    /// <summary>Trim trailing nulls the calculator pads when the run stops early (subsonic/steep).</summary>
    private static TrajectoryPoint[] TrimTrailingNulls(TrajectoryPoint[] trajectory)
    {
        var count = 0;
        while (count < trajectory.Length && trajectory[count] != null)
            count++;

        if (count == trajectory.Length)
            return trajectory;

        var trimmed = new TrajectoryPoint[count];
        System.Array.Copy(trajectory, trimmed, count);
        return trimmed;
    }
}
