using BallisticCalculator;

namespace BallisticCalculator.Types;

/// <summary>
/// Builds the library zeroing inputs from a <see cref="ShotData"/> (honoring its <see cref="ZeroingData"/>,
/// falling back to the rifle's own zero for older data) and computes the zero adjustments.
/// </summary>
public static class ZeroingCalculator
{
    /// <summary>
    /// Assemble the library <see cref="ZeroingParameters"/>, the effective zero ammo/atmosphere, the
    /// rifle to feed the calculator, and the optional zeroing shot/wind from a <see cref="ShotData"/>.
    /// </summary>
    public static ZeroingInputs BuildInputs(ShotData shotData, Atmosphere atmosphere)
    {
        var weapon = shotData.Weapon!;
        var ammo = shotData.Ammunition!.Ammunition;
        var z = shotData.Zeroing;
        var wz = weapon.Zero;

        var zeroParams = new ZeroingParameters(
            z?.Distance ?? wz.Distance,
            z?.Ammunition ?? wz.Ammunition,
            z?.Atmosphere ?? wz.Atmosphere)
        {
            VerticalOffset = z?.VerticalOffset ?? wz.VerticalOffset,
            HorizontalOffset = z?.HorizontalOffset ?? wz.HorizontalOffset,
        };

        return new ZeroingInputs
        {
            ZeroParameters = zeroParams,
            ZeroAmmunition = zeroParams.Ammunition ?? ammo,
            ZeroAtmosphere = zeroParams.Atmosphere ?? atmosphere,
            Rifle = new Rifle(weapon.Sight, zeroParams, weapon.Rifling),
            ZeroShot = z?.ShotAngle is null ? null : new ShotParameters { ShotAngle = z.ShotAngle },
            ZeroWind = z?.Wind is null ? null : new[] { z.Wind },
        };
    }

    /// <summary>Compute the zero (sight) adjustments for a shot, or null when data is incomplete.</summary>
    public static ZeroCalculatedParameters? Compute(ShotData? shotData)
    {
        if (shotData?.Ammunition?.Ammunition == null || shotData.Weapon == null)
            return null;

        var atmosphere = shotData.Atmosphere ?? new Atmosphere();
        var inputs = BuildInputs(shotData, atmosphere);

        var calc = new TrajectoryCalculator();
        return calc.CalculateZeroParameters(
            inputs.ZeroAmmunition, inputs.ZeroAtmosphere, inputs.Rifle, inputs.ZeroParameters,
            shot: inputs.ZeroShot, wind: inputs.ZeroWind,
            dragTable: CustomDragTableLoader.ForAmmunition(inputs.ZeroAmmunition));
    }

    public sealed class ZeroingInputs
    {
        public required ZeroingParameters ZeroParameters { get; init; }
        public required Ammunition ZeroAmmunition { get; init; }
        public required Atmosphere ZeroAtmosphere { get; init; }
        public required Rifle Rifle { get; init; }
        public ShotParameters? ZeroShot { get; init; }
        public Wind[]? ZeroWind { get; init; }
    }
}
