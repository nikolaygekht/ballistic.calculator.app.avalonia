using System.Linq;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class SummaryControllerTests
{
    private readonly SummaryController _controller = new();

    [Fact]
    public void Compute_LongRange_ProducesZeroAdjustmentDeadZoneAndSubsonic()
    {
        var (shotData, trajectory) = BuildScenario(maxYards: 1500);

        var result = _controller.Compute(shotData, trajectory, MeasurementSystem.Imperial);

        // Zero adjustment (vertical) is always available for a valid rifle.
        result.ZeroVertical.Should().NotBeNull();
        result.ZeroVertical!.Value.In(AngularUnit.Mil).Should().BeGreaterThan(0);

        // 168gr .308 @ 2700 fps goes subsonic well within 1500 yd.
        result.SubsonicDistance.Should().NotBeNull();
        var subYd = result.SubsonicDistance!.Value.In(DistanceUnit.Yard);
        subYd.Should().BeInRange(800, 1500);

        // Point-blank corridor (dead zone) for a bottom-aimed 20" target, reported as a span.
        result.TargetSize.Should().NotBeNull();
        result.TargetSize!.Value.In(DistanceUnit.Inch).Should().BeApproximately(20, 0.01);
        result.DeadZoneMin.Should().NotBeNull();
        result.DeadZoneMax.Should().NotBeNull();
        result.DeadZoneMax!.Value.In(DistanceUnit.Yard)
            .Should().BeGreaterThan(result.DeadZoneMin!.Value.In(DistanceUnit.Yard));
        result.NearZero.Should().NotBeNull();
        result.FarZero.Should().NotBeNull();
        // Far zero is beyond near zero.
        result.FarZero!.Value.In(DistanceUnit.Yard)
            .Should().BeGreaterThan(result.NearZero!.Value.In(DistanceUnit.Yard));
    }

    [Fact]
    public void Compute_ShortRange_LeavesSubsonicUnset()
    {
        var (shotData, trajectory) = BuildScenario(maxYards: 100);

        var result = _controller.Compute(shotData, trajectory, MeasurementSystem.Imperial);

        // Still supersonic at 100 yd.
        result.SubsonicDistance.Should().BeNull();
    }

    [Fact]
    public void Compute_NullData_ReturnsAllNull()
    {
        var result = _controller.Compute(null, null, MeasurementSystem.Metric);

        result.ZeroVertical.Should().BeNull();
        result.DeadZoneMin.Should().BeNull();
        result.DeadZoneMax.Should().BeNull();
        result.SubsonicDistance.Should().BeNull();
    }

    [Fact]
    public void FindSubsonicDistance_InterpolatesBetweenBracketingPoints()
    {
        // Build a real trajectory and confirm the crossing lies between the last supersonic and
        // first subsonic step, at (approximately) Mach 1.
        var (_, trajectory) = BuildScenario(maxYards: 1500);

        var subsonic = SummaryController.FindSubsonicDistance(trajectory);
        subsonic.Should().NotBeNull();

        var distYd = subsonic!.Value.In(DistanceUnit.Yard);
        var lastSuper = trajectory.Where(p => p != null && p.Mach >= 1.0).Max(p => p!.Distance.In(DistanceUnit.Yard));
        var firstSub = trajectory.Where(p => p != null && p.Mach < 1.0).Min(p => p!.Distance.In(DistanceUnit.Yard));
        distYd.Should().BeInRange(lastSuper, firstSub);
    }

    [Fact]
    public void FindLineOfSightCrossings_InterpolatesAscendingAndDescending()
    {
        // Drop (m) vs range (m): rises through 0 between 0 and 50, falls through 0 between 150 and 200.
        var traj = new[]
        {
            Point(0, -0.05), Point(50, 0.03), Point(100, 0.05), Point(150, 0.02), Point(200, -0.04),
        };

        var (near, far) = SummaryController.FindLineOfSightCrossings(traj);

        near.Should().NotBeNull();
        near!.Value.In(DistanceUnit.Meter).Should().BeApproximately(31.25, 0.01);
        far.Should().NotBeNull();
        far!.Value.In(DistanceUnit.Meter).Should().BeApproximately(166.67, 0.01);
    }

    [Fact]
    public void FindLineOfSightCrossings_NoDescentWithinPath_LeavesFarNull()
    {
        // The path rises through the line of sight but never comes back down within the given points.
        var traj = new[] { Point(0, -0.05), Point(50, 0.03), Point(100, 0.10) };

        var (near, far) = SummaryController.FindLineOfSightCrossings(traj);

        near.Should().NotBeNull();
        far.Should().BeNull();
    }

    private static TrajectoryPoint Point(double rangeMeters, double dropMeters) => new(
        time: System.TimeSpan.FromSeconds(rangeMeters / 800.0),
        distance: new Measurement<DistanceUnit>(rangeMeters, DistanceUnit.Meter),
        velocity: new Measurement<VelocityUnit>(800, VelocityUnit.MetersPerSecond),
        mach: 2.0,
        drop: new Measurement<DistanceUnit>(dropMeters, DistanceUnit.Meter),
        windage: Measurement<DistanceUnit>.ZERO,
        energy: new Measurement<EnergyUnit>(1000, EnergyUnit.Joule),
        optimalGameWeight: Measurement<WeightUnit>.ZERO);

    private static (ShotData, TrajectoryPoint[]) BuildScenario(double maxYards)
    {
        var ammo = new Ammunition(
            weight: new Measurement<WeightUnit>(168, WeightUnit.Grain),
            ballisticCoefficient: new BallisticCoefficient(0.223, DragTableId.G7),
            muzzleVelocity: new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));

        var rifle = new Rifle(
            new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
            new ZeroingParameters(new Measurement<DistanceUnit>(300, DistanceUnit.Yard), null, null));

        var atmosphere = new Atmosphere();

        var shotData = new ShotData
        {
            Ammunition = new AmmunitionLibraryEntry { Name = "test", Ammunition = ammo },
            Weapon = rifle,
            Atmosphere = atmosphere,
            Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(300, DistanceUnit.Yard) },
            Parameters = new ShotParameters
            {
                MaximumDistance = new Measurement<DistanceUnit>(maxYards, DistanceUnit.Yard),
                Step = new Measurement<DistanceUnit>(25, DistanceUnit.Yard),
            },
        };

        var inputs = ZeroingCalculator.BuildInputs(shotData, atmosphere);
        var calc = new TrajectoryCalculator();
        var shot = new ShotParameters
        {
            MaximumDistance = shotData.Parameters.MaximumDistance,
            Step = shotData.Parameters.Step,
        };
        shot.Apply(calc.CalculateZeroParameters(inputs.ZeroAmmunition, inputs.ZeroAtmosphere, inputs.Rifle, inputs.ZeroParameters));
        var trajectory = calc.Calculate(ammo, inputs.Rifle, atmosphere, shot).TakeWhile(p => p != null).ToArray();

        return (shotData, trajectory);
    }
}
