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

        var calc = new global::BallisticCalculator.TrajectoryCalculator();
        var ammo = shotData.Ammunition!.Ammunition;
        var weapon = shotData.Weapon!;
        var atmosphere = shotData.Atmosphere!;
        var parameters = shotData.Parameters!;

        var zeroAmmo = weapon.Zero?.Ammunition ?? ammo;
        var zeroAtmosphere = weapon.Zero?.Atmosphere ?? atmosphere;

        var shotParameters = new ShotParameters()
        {
            BarrelAzimuth = parameters.BarrelAzimuth,
            CantAngle = parameters.CantAngle,
            MaximumDistance = parameters.MaximumDistance,
            ShotAngle = parameters.ShotAngle,
            Step = parameters.Step,
            SightAngle = calc.SightAngle(zeroAmmo, weapon, zeroAtmosphere),
        };

        var trajectory = calc.Calculate(ammo, weapon, atmosphere, shotParameters, shotData.Winds);

        // Trim trailing nulls (if trajectory goes beyond effective range)
        var count = 0;
        for (var i = 0; i < trajectory.Length; i++)
        {
            if (trajectory[i] == null)
                break;
            count++;
        }

        if (count < trajectory.Length)
        {
            var trimmed = new TrajectoryPoint[count];
            System.Array.Copy(trajectory, trimmed, count);
            return trimmed;
        }

        return trajectory;
    }
}
