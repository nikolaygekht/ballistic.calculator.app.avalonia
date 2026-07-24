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
}
