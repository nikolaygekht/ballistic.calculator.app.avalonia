using System;
using System.Linq;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Types;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class ShotTrajectoryCalculatorTests
{
    [Fact]
    public void Calculate_NullOrIncompleteData_ReturnsNull()
    {
        ShotTrajectoryCalculator.Calculate(null).Should().BeNull();

        var noAmmo = BuildShotData();
        noAmmo.Ammunition = null;
        ShotTrajectoryCalculator.Calculate(noAmmo).Should().BeNull();

        var noWeapon = BuildShotData();
        noWeapon.Weapon = null;
        ShotTrajectoryCalculator.Calculate(noWeapon).Should().BeNull();

        var noParams = BuildShotData();
        noParams.Parameters = null;
        ShotTrajectoryCalculator.Calculate(noParams).Should().BeNull();
    }

    [Fact]
    public void Calculate_CompleteData_ReturnsTrimmedTrajectoryToMaxDistance()
    {
        var trajectory = ShotTrajectoryCalculator.Calculate(BuildShotData());

        trajectory.Should().NotBeNull();
        trajectory!.Should().NotContainNulls();
        // .308 stays supersonic through 500 m, so the run reaches the configured max.
        trajectory.Last().Distance.In(DistanceUnit.Meter).Should().BeApproximately(500, 1);
    }

    [Fact]
    public void Calculate_StepOverride_MatchesMainExceptStep()
    {
        var data = BuildShotData(); // main step = 100 m

        var main = ShotTrajectoryCalculator.Calculate(data)!;
        var fine = ShotTrajectoryCalculator.Calculate(data, new Measurement<DistanceUnit>(25, DistanceUnit.Meter))!;

        // Finer step -> more points, but the same overall range.
        fine.Length.Should().BeGreaterThan(main.Length);
        fine.Last().Distance.In(DistanceUnit.Meter)
            .Should().BeApproximately(main.Last().Distance.In(DistanceUnit.Meter), 1);

        // Same trajectory at a shared distance (300 m) -> identical drop and windage.
        DropCm(main, 300).Should().BeApproximately(DropCm(fine, 300), 0.5);
        WindageCm(main, 300).Should().BeApproximately(WindageCm(fine, 300), 0.5);
    }

    [Fact]
    public void Calculate_MaxDistanceOverride_ExtendsRangeBeyondConfigured()
    {
        var data = BuildShotData(); // configured max = 500 m

        var extended = ShotTrajectoryCalculator.Calculate(
            data, maxDistanceOverride: new Measurement<DistanceUnit>(1000, DistanceUnit.Meter))!;

        extended.Last().Distance.In(DistanceUnit.Meter).Should().BeApproximately(1000, 1);
    }

    [Fact]
    public void CalculateFine_ReturnsFinerTrajectoryReachingAtLeast3000m()
    {
        var data = BuildShotData(); // configured max = 500 m

        var display = ShotTrajectoryCalculator.Calculate(data)!;
        var fine = ShotTrajectoryCalculator.CalculateFine(data)!;

        fine.Length.Should().BeGreaterThan(display.Length);
        fine.Last().Distance.In(DistanceUnit.Meter).Should().BeGreaterThanOrEqualTo(2999);
    }

    [Fact]
    public void CalculateFine_ConfiguredMaxBeyondTheFineMinimum_Wins()
    {
        var data = BuildShotData();
        data.Parameters!.MaximumDistance = new Measurement<DistanceUnit>(4000, DistanceUnit.Meter);

        var fine = ShotTrajectoryCalculator.CalculateFine(data)!;

        fine.Last().Distance.In(DistanceUnit.Meter).Should().BeGreaterThanOrEqualTo(3999);
    }

    [Fact]
    public void CalculateFine_IncompleteData_ReturnsNull()
    {
        ShotTrajectoryCalculator.CalculateFine(null).Should().BeNull();
    }

    private static double DropCm(TrajectoryPoint[] t, double meters)
        => PointAt(t, meters).Drop.In(DistanceUnit.Centimeter);

    private static double WindageCm(TrajectoryPoint[] t, double meters)
        => PointAt(t, meters).Windage.In(DistanceUnit.Centimeter);

    private static TrajectoryPoint PointAt(TrajectoryPoint[] t, double meters)
        => t.First(p => Math.Abs(p.Distance.In(DistanceUnit.Meter) - meters) < 0.6);

    private static ShotData BuildShotData()
    {
        var ammo = new Ammunition(
            weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
            ballisticCoefficient: new BallisticCoefficient(0.223, DragTableId.G7),
            muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));

        var rifle = new Rifle(
            new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
            new ZeroingParameters(new Measurement<DistanceUnit>(100, DistanceUnit.Meter), null, null));

        return new ShotData
        {
            Ammunition = new AmmunitionLibraryEntry { Name = "test", Ammunition = ammo },
            Weapon = rifle,
            Atmosphere = new Atmosphere(),
            Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter) },
            Parameters = new ShotParameters
            {
                MaximumDistance = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
                Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            },
        };
    }
}
