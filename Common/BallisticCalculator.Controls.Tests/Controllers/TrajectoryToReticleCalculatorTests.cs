using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Reticle;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using Xunit;
using AwesomeAssertions;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class TrajectoryToReticleCalculatorTests
{
    #region Test Data

    private static readonly Measurement<DistanceUnit> ZeroDistance =
        new(100, DistanceUnit.Yard);

    /// <summary>
    /// Creates trajectory from g1_nowind.txt test data:
    /// 0.365 G1, 65gr, 2600 ft/s, zero at 100yd
    /// </summary>
    private static TrajectoryPoint[] CreateTestTrajectory()
    {
        var data = new (double dist, double drop, double vel, double mach, double energy, double time)[]
        {
            (0, -2.5, 2600.0, 2.329, 975.5, 0.000),
            (50, -0.5, 2478.7, 2.220, 886.6, 0.059),
            (100, 0.0, 2360.6, 2.114, 804.1, 0.121),
            (150, -1.0, 2245.7, 2.011, 727.7, 0.186),
            (200, -3.8, 2133.9, 1.911, 657.1, 0.255),
            (250, -8.4, 2025.3, 1.814, 591.9, 0.327),
            (300, -15.2, 1920.1, 1.720, 532.0, 0.403),
            (350, -24.3, 1818.4, 1.629, 477.2, 0.483),
            (400, -36.1, 1720.5, 1.541, 427.2, 0.568),
            (450, -50.8, 1626.8, 1.457, 381.9, 0.658),
            (500, -68.8, 1537.5, 1.377, 341.1, 0.753),
            (550, -90.4, 1453.3, 1.302, 304.8, 0.853),
            (600, -116.2, 1374.5, 1.231, 272.6, 0.959),
            (650, -146.6, 1301.8, 1.166, 244.6, 1.071),
            (700, -182.1, 1235.8, 1.107, 220.4, 1.190),
            (750, -223.3, 1177.2, 1.054, 200.0, 1.314),
            (800, -270.8, 1126.2, 1.009, 183.0, 1.444),
            (850, -325.1, 1082.6, 0.970, 169.1, 1.580),
            (900, -386.9, 1045.3, 0.936, 157.7, 1.722),
            (950, -456.6, 1013.1, 0.907, 148.1, 1.868),
            (1000, -534.8, 984.7, 0.882, 139.9, 2.018),
        };

        var trajectory = new TrajectoryPoint[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            trajectory[i] = new TrajectoryPoint(
                time: TimeSpan.FromSeconds(d.time),
                distance: new Measurement<DistanceUnit>(d.dist, DistanceUnit.Yard),
                velocity: new Measurement<VelocityUnit>(d.vel, VelocityUnit.FeetPerSecond),
                mach: d.mach,
                drop: new Measurement<DistanceUnit>(d.drop, DistanceUnit.Inch),
                windage: new Measurement<DistanceUnit>(0, DistanceUnit.Inch),
                energy: new Measurement<EnergyUnit>(d.energy, EnergyUnit.FootPound),
                optimalGameWeight: Measurement<WeightUnit>.ZERO);
        }
        return trajectory;
    }

    private static ReticleBulletDropCompensatorPointCollection CreateMilDotBdc()
    {
        var reticle = new MilDotReticle();
        return reticle.BulletDropCompensator;
    }

    #endregion

    #region UpdatePoints

    [Fact]
    public void UpdatePoints_WithMilDotBdc_FindsPoints()
    {
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), CreateMilDotBdc(), ZeroDistance);

        calculator.UpdatePoints();

        calculator.Points.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UpdatePoints_AllPointsHaveBdcPointAndDistance()
    {
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), CreateMilDotBdc(), ZeroDistance);

        calculator.UpdatePoints();

        foreach (var point in calculator.Points)
        {
            point.BDCPoint.Should().NotBeNull();
            point.Distance.Value.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void UpdatePoints_ClassifiesNearAndFarCorrectly()
    {
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), CreateMilDotBdc(), ZeroDistance);

        calculator.UpdatePoints();

        // Far points should be past zero (100yd)
        foreach (var point in calculator.Points)
        {
            if (point.Location == TrajectoryToReticleCalculator.PointLocation.Far)
                point.Distance.In(DistanceUnit.Yard).Should().BeGreaterThanOrEqualTo(100);
            else
                point.Distance.In(DistanceUnit.Yard).Should().BeLessThan(100);
        }
    }

    [Fact]
    public void UpdatePoints_FarPointsExist()
    {
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), CreateMilDotBdc(), ZeroDistance);

        calculator.UpdatePoints();

        // Trajectory drops significantly past 100yd, so MilDot BDC marks (-1 to -4 mil)
        // should map to far distances
        calculator.Points.Any(p => p.Location == TrajectoryToReticleCalculator.PointLocation.Far)
            .Should().BeTrue();
    }

    [Fact]
    public void UpdatePoints_FlatTrajectory_NoNearPoints()
    {
        // With a 100yd zero and flat-shooting bullet, the drop before
        // zero is tiny (-2.5in at muzzle to 0 at 100yd) — never reaches
        // -1 mil BDC marks, so no near BDC points exist.
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), CreateMilDotBdc(), ZeroDistance);

        calculator.UpdatePoints();

        calculator.Points.Any(p => p.Location == TrajectoryToReticleCalculator.PointLocation.Near)
            .Should().BeFalse();
    }

    [Fact]
    public void UpdatePoints_FarDistancesAreIncreasing()
    {
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), CreateMilDotBdc(), ZeroDistance);

        calculator.UpdatePoints();

        var farPoints = calculator.Points
            .Where(p => p.Location == TrajectoryToReticleCalculator.PointLocation.Far)
            .ToList();

        for (int i = 1; i < farPoints.Count; i++)
            farPoints[i].Distance.Should().BeGreaterThan(farPoints[i - 1].Distance);
    }

    [Fact]
    public void UpdatePoints_EmptyBdc_NoPoints()
    {
        var emptyBdc = new ReticleBulletDropCompensatorPointCollection();
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), emptyBdc, ZeroDistance);

        calculator.UpdatePoints();

        calculator.Points.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePoints_ClearsOldPointsOnRerun()
    {
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), CreateMilDotBdc(), ZeroDistance);

        calculator.UpdatePoints();
        var firstCount = calculator.Points.Count;

        calculator.UpdatePoints();
        calculator.Points.Count.Should().Be(firstCount);
    }

    #endregion

    [Fact]
    public void UpdatePoints_FineTrajectory_HasNearPoints()
    {
        // With fine-grained trajectory (2.5m steps), the bullet is well below
        // sight line at close range (2.5in sight height). At ~3yd the drop
        // adjustment is about -2 mil, crossing the -1 mil BDC mark.
        var calc = new TrajectoryCalculator();
        var ammo = new Ammunition(
            ballisticCoefficient: new BallisticCoefficient(0.365, DragTableId.G1),
            weight: new Measurement<WeightUnit>(65, WeightUnit.Grain),
            muzzleVelocity: new Measurement<VelocityUnit>(2600, VelocityUnit.FeetPerSecond));
        var weapon = new Rifle(
            new Sight(
                new Measurement<DistanceUnit>(2.5, DistanceUnit.Inch),
                Measurement<AngularUnit>.ZERO,
                Measurement<AngularUnit>.ZERO),
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Yard), null, null),
            null);
        var atmosphere = new Atmosphere();
        var shotParams = new ShotParameters
        {
            Step = new Measurement<DistanceUnit>(2.5, DistanceUnit.Meter),
            MaximumDistance = new Measurement<DistanceUnit>(1500, DistanceUnit.Meter),
            SightAngle = calc.SightAngle(ammo, weapon, atmosphere),
        };

        var trajectory = calc.Calculate(ammo, weapon, atmosphere, shotParams);
        // Trim nulls
        var count = Array.FindIndex(trajectory, t => t == null);
        if (count < 0) count = trajectory.Length;
        var trimmed = trajectory[..count];

        var calculator = new TrajectoryToReticleCalculator(
            trimmed, CreateMilDotBdc(), ZeroDistance);
        calculator.UpdatePoints();

        calculator.Points.Any(p => p.Location == TrajectoryToReticleCalculator.PointLocation.Near)
            .Should().BeTrue("fine trajectory should have near BDC points from close-range steep drop");
    }

    #region FindDistance

    [Fact]
    public void FindDistance_AtExactTrajectoryPoint_ReturnsIt()
    {
        var trajectory = CreateTestTrajectory();
        var calculator = new TrajectoryToReticleCalculator(
            trajectory, CreateMilDotBdc(), ZeroDistance);

        var result = calculator.FindDistance(new Measurement<DistanceUnit>(300, DistanceUnit.Yard));

        result.Should().NotBeNull();
        result!.Distance.In(DistanceUnit.Yard).Should().BeApproximately(300, 1);
    }

    [Fact]
    public void FindDistance_BetweenPoints_ReturnsNearest()
    {
        var trajectory = CreateTestTrajectory();
        var calculator = new TrajectoryToReticleCalculator(
            trajectory, CreateMilDotBdc(), ZeroDistance);

        var result = calculator.FindDistance(new Measurement<DistanceUnit>(310, DistanceUnit.Yard));

        result.Should().NotBeNull();
        result!.Distance.In(DistanceUnit.Yard).Should().BeApproximately(300, 1);
    }

    [Fact]
    public void FindDistance_BeforeFirstPoint_ReturnsNull()
    {
        var trajectory = CreateTestTrajectory();
        var calculator = new TrajectoryToReticleCalculator(
            trajectory, CreateMilDotBdc(), ZeroDistance);

        // First point is at 0 yd, so -10 is before
        var result = calculator.FindDistance(new Measurement<DistanceUnit>(-10, DistanceUnit.Yard));

        result.Should().BeNull();
    }

    [Fact]
    public void FindDistance_PastLastPoint_ReturnsNull()
    {
        var trajectory = CreateTestTrajectory();
        var calculator = new TrajectoryToReticleCalculator(
            trajectory, CreateMilDotBdc(), ZeroDistance);

        var result = calculator.FindDistance(new Measurement<DistanceUnit>(1500, DistanceUnit.Yard));

        result.Should().BeNull();
    }

    [Fact]
    public void FindDistance_AtLastPoint_ReturnsIt()
    {
        var trajectory = CreateTestTrajectory();
        var calculator = new TrajectoryToReticleCalculator(
            trajectory, CreateMilDotBdc(), ZeroDistance);

        var result = calculator.FindDistance(new Measurement<DistanceUnit>(1000, DistanceUnit.Yard));

        result.Should().NotBeNull();
        result!.Distance.In(DistanceUnit.Yard).Should().BeApproximately(1000, 1);
    }

    [Fact]
    public void FindDistance_EmptyTrajectory_ReturnsNull()
    {
        var calculator = new TrajectoryToReticleCalculator(
            Array.Empty<TrajectoryPoint>(), CreateMilDotBdc(), ZeroDistance);

        var result = calculator.FindDistance(new Measurement<DistanceUnit>(100, DistanceUnit.Yard));

        result.Should().BeNull();
    }

    #endregion
}
