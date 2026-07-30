using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Utilities;

public static class ShotCalculator
{
    public static void ApplyDefaults(ShotData shotData, MeasurementSystem system)
    {
        // Default atmosphere: standard conditions
        shotData.Atmosphere ??= new Atmosphere();

        // Default rifle: 3" sight height, 100 yd/m zero
        if (shotData.Weapon == null)
        {
            var zeroDistance = system == MeasurementSystem.Imperial
                ? new Measurement<DistanceUnit>(100, DistanceUnit.Yard)
                : new Measurement<DistanceUnit>(100, DistanceUnit.Meter);

            shotData.Weapon = new Rifle()
            {
                Sight = new Sight() { SightHeight = new Measurement<DistanceUnit>(3, DistanceUnit.Inch) },
                Zero = new ZeroingParameters() { Distance = zeroDistance },
            };
        }

        // Default parameters: 1000 yd/m max, 100 yd/m step
        if (shotData.Parameters == null)
        {
            var unit = system == MeasurementSystem.Imperial ? DistanceUnit.Yard : DistanceUnit.Meter;
            shotData.Parameters = new ShotParameters
            {
                MaximumDistance = new Measurement<DistanceUnit>(1000, unit),
                Step = new Measurement<DistanceUnit>(100, unit),
            };
        }
    }

    public static TrajectoryPoint[] Calculate(ShotData shotData, MeasurementSystem system)
    {
        ApplyDefaults(shotData, system);
        return ShotTrajectoryCalculator.Calculate(shotData) ?? System.Array.Empty<TrajectoryPoint>();
    }

    /// <summary>
    /// <see cref="Calculate"/>, with the engine's exceptions returned instead of thrown: a form factor
    /// with no bullet diameter, a GC coefficient whose <c>.drg</c> cannot be found, and a zero the bullet
    /// cannot reach all raise meaningful exceptions (findings F-1, F-1b, F-1c). Every call site is an
    /// <c>async void</c> handler, where an escaping exception is an unhandled UI-thread crash rather than
    /// something the user can act on.
    /// </summary>
    /// <remarks>
    /// This is the last line of defense, not the fix: the states above should be caught by validation
    /// before the user gets this far, and a caught exception is still a dead end. It exists so the dead
    /// end is a message instead of a crash.
    /// </remarks>
    public static bool TryCalculate(ShotData shotData, MeasurementSystem system,
        out TrajectoryPoint[] trajectory, out System.Exception? error)
    {
        try
        {
            trajectory = Calculate(shotData, system);
            error = null;
            return true;
        }
        catch (System.Exception ex)
        {
            trajectory = System.Array.Empty<TrajectoryPoint>();
            error = ex;
            return false;
        }
    }

    /// <summary>
    /// A sentence the user can act on for the failures the engine names as its own, or null for anything
    /// else — which is a bug, not a bad input, and belongs in the exception dialog with its stack trace.
    /// </summary>
    /// <remarks>
    /// BallisticCalculator 1.1.13 introduced both types (before it, these arrived as a bare
    /// <c>InvalidOperationException</c>, indistinguishable from any other). Neither state can be validated
    /// from the dialog: whether a load reaches a distance, or whether a set of numbers integrates at all, is
    /// the calculation's own answer rather than a property of the input. See <c>claude/07-28.md</c> F-1c.
    /// </remarks>
    public static string? Explain(System.Exception? error) => error switch
    {
        ZeroRangeCantBeReachedException => "This load cannot reach the zero distance.\n\n" +
            "Zero it closer, or give it a faster muzzle velocity or a better ballistic coefficient. " +
            "A subsonic load asked for a very long zero is the usual way to arrive here.",

        TrajectoryCannotBeCalculatedException => "The trajectory cannot be calculated from these numbers.\n\n" +
            "Something in the shot leaves the solver with nothing to work with — a zero or absurd " +
            "ballistic coefficient, weight or muzzle velocity is the usual cause. Check the Ammunition tab.",

        _ => null,
    };
}
