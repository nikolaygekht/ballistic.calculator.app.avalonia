using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Reticle;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using Xunit;
using AwesomeAssertions;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class ReticleOverlayControllerTests
{
    #region Test Data

    private static readonly Measurement<DistanceUnit> ZeroDistance =
        new(100, DistanceUnit.Yard);

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

    private static TrajectoryToReticleCalculator CreateCalculator()
    {
        var reticle = new MilDotReticle();
        return new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), reticle.BulletDropCompensator, ZeroDistance);
    }

    #endregion

    #region CreateBdcOverlay

    [Fact]
    public void CreateBdcOverlay_FarBdc_ReturnsTextElements()
    {
        var calculator = CreateCalculator();

        var elements = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Imperial, far: true);

        elements.Count.Should().BeGreaterThan(0);
        foreach (var element in elements)
            element.Should().BeOfType<ReticleText>();
    }

    [Fact]
    public void CreateBdcOverlay_NearBdc_FlatTrajectory_ReturnsEmpty()
    {
        // With this flat trajectory and 100yd zero, bullet never drops
        // enough before zero to cross MilDot BDC marks
        var calculator = CreateCalculator();

        var elements = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Imperial, far: false);

        elements.Should().BeEmpty();
    }

    [Fact]
    public void CreateBdcOverlay_FarBdc_LabelsAreDistancesInYards()
    {
        var calculator = CreateCalculator();

        var elements = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Imperial, far: true);

        foreach (var element in elements.Cast<ReticleText>())
        {
            // Labels should be whole numbers (distances formatted with N0)
            int.TryParse(element.Text.Replace(",", ""), out int distance).Should().BeTrue();
            distance.Should().BeGreaterThan(100); // past zero
        }
    }

    [Fact]
    public void CreateBdcOverlay_Metric_LabelsAreDistancesInMeters()
    {
        var calculator = CreateCalculator();

        var elements = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Metric, far: true);

        // Metric should produce different numbers than imperial
        // (yards vs meters conversion)
        elements.Count.Should().BeGreaterThan(0);
        foreach (var element in elements.Cast<ReticleText>())
        {
            int.TryParse(element.Text.Replace(",", ""), out int _).Should().BeTrue();
        }
    }

    [Fact]
    public void CreateBdcOverlay_TextColorIsBlue()
    {
        var calculator = CreateCalculator();

        var elements = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Imperial, far: true);

        foreach (var element in elements.Cast<ReticleText>())
            element.Color.Should().Be("blue");
    }

    [Fact]
    public void CreateBdcOverlay_EmptyBdc_ReturnsEmptyCollection()
    {
        var emptyBdc = new ReticleBulletDropCompensatorPointCollection();
        var calculator = new TrajectoryToReticleCalculator(
            CreateTestTrajectory(), emptyBdc, ZeroDistance);

        var elements = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Imperial, far: true);

        elements.Should().BeEmpty();
    }

    [Fact]
    public void CreateBdcOverlay_FarAndNear_DoNotOverlap()
    {
        var calculator = CreateCalculator();

        var far = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Imperial, far: true);
        var near = ReticleOverlayController.CreateBdcOverlay(
            calculator, MeasurementSystem.Imperial, far: false);

        // Far has results, near is empty for this flat trajectory — no overlap
        far.Count.Should().BeGreaterThan(0);
        var farTexts = far.Cast<ReticleText>().Select(t => t.Text).ToHashSet();
        var nearTexts = near.Cast<ReticleText>().Select(t => t.Text).ToHashSet();
        farTexts.Intersect(nearTexts).Should().BeEmpty();
    }

    #endregion

    #region CreateTargetOverlay

    [Fact]
    public void CreateTargetOverlay_ValidParams_ReturnsRectangle()
    {
        var calculator = CreateCalculator();
        var width = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);
        var height = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);
        var distance = new Measurement<DistanceUnit>(200, DistanceUnit.Yard);

        var result = ReticleOverlayController.CreateTargetOverlay(
            calculator, width, height, distance);

        result.Should().NotBeNull();
        result.Should().BeOfType<ReticleRectangle>();
    }

    [Fact]
    public void CreateTargetOverlay_ColorIsRed()
    {
        var calculator = CreateCalculator();
        var width = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);
        var height = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);
        var distance = new Measurement<DistanceUnit>(200, DistanceUnit.Yard);

        var result = (ReticleRectangle)ReticleOverlayController.CreateTargetOverlay(
            calculator, width, height, distance)!;

        result.Color.Should().Be("red");
    }

    [Fact]
    public void CreateTargetOverlay_LargerTarget_LargerAngularSize()
    {
        var calculator = CreateCalculator();
        var distance = new Measurement<DistanceUnit>(200, DistanceUnit.Yard);

        var small = (ReticleRectangle)ReticleOverlayController.CreateTargetOverlay(
            calculator,
            new Measurement<DistanceUnit>(3, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(3, DistanceUnit.Inch),
            distance)!;

        var large = (ReticleRectangle)ReticleOverlayController.CreateTargetOverlay(
            calculator,
            new Measurement<DistanceUnit>(12, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(12, DistanceUnit.Inch),
            distance)!;

        large.Size.X.Abs().Should().BeGreaterThan(small.Size.X.Abs());
        large.Size.Y.Abs().Should().BeGreaterThan(small.Size.Y.Abs());
    }

    [Fact]
    public void CreateTargetOverlay_FartherDistance_SmallerAngularSize()
    {
        var calculator = CreateCalculator();
        var width = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);
        var height = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);

        var near = (ReticleRectangle)ReticleOverlayController.CreateTargetOverlay(
            calculator, width, height,
            new Measurement<DistanceUnit>(100, DistanceUnit.Yard))!;

        var far = (ReticleRectangle)ReticleOverlayController.CreateTargetOverlay(
            calculator, width, height,
            new Measurement<DistanceUnit>(500, DistanceUnit.Yard))!;

        near.Size.X.Abs().Should().BeGreaterThan(far.Size.X.Abs());
        near.Size.Y.Abs().Should().BeGreaterThan(far.Size.Y.Abs());
    }

    [Fact]
    public void CreateTargetOverlay_ZeroWidth_ReturnsNull()
    {
        var calculator = CreateCalculator();

        var result = ReticleOverlayController.CreateTargetOverlay(
            calculator,
            new Measurement<DistanceUnit>(0, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(6, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(200, DistanceUnit.Yard));

        result.Should().BeNull();
    }

    [Fact]
    public void CreateTargetOverlay_ZeroDistance_ReturnsNull()
    {
        var calculator = CreateCalculator();

        var result = ReticleOverlayController.CreateTargetOverlay(
            calculator,
            new Measurement<DistanceUnit>(6, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(6, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(0, DistanceUnit.Yard));

        result.Should().BeNull();
    }

    [Fact]
    public void CreateTargetOverlay_DistancePastTrajectory_ReturnsNull()
    {
        var calculator = CreateCalculator();

        var result = ReticleOverlayController.CreateTargetOverlay(
            calculator,
            new Measurement<DistanceUnit>(6, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(6, DistanceUnit.Inch),
            new Measurement<DistanceUnit>(5000, DistanceUnit.Yard));

        result.Should().BeNull();
    }

    #endregion

    #region CalculateAngularSize

    [Fact]
    public void CalculateAngularSize_KnownValue()
    {
        // 1 cm at 100 m = 1 cm/100m = 0.01 rad ≈ 0.1 mil
        var size = new Measurement<DistanceUnit>(1, DistanceUnit.Centimeter);
        var distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter);

        var angular = ReticleOverlayController.CalculateAngularSize(size, distance);

        angular.In(AngularUnit.CmPer100Meters).Should().BeApproximately(1.0, 0.01);
    }

    [Fact]
    public void CalculateAngularSize_DoubleSize_DoubleAngle()
    {
        var distance = new Measurement<DistanceUnit>(200, DistanceUnit.Yard);

        var small = ReticleOverlayController.CalculateAngularSize(
            new Measurement<DistanceUnit>(3, DistanceUnit.Inch), distance);
        var large = ReticleOverlayController.CalculateAngularSize(
            new Measurement<DistanceUnit>(6, DistanceUnit.Inch), distance);

        (large / small).Should().BeApproximately(2.0, 0.01);
    }

    [Fact]
    public void CalculateAngularSize_DoubleDistance_HalfAngle()
    {
        var size = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);

        var near = ReticleOverlayController.CalculateAngularSize(
            size, new Measurement<DistanceUnit>(100, DistanceUnit.Yard));
        var far = ReticleOverlayController.CalculateAngularSize(
            size, new Measurement<DistanceUnit>(200, DistanceUnit.Yard));

        (near / far).Should().BeApproximately(2.0, 0.01);
    }

    [Fact]
    public void CalculateAngularSize_WithScale_DividesResult()
    {
        var size = new Measurement<DistanceUnit>(6, DistanceUnit.Inch);
        var distance = new Measurement<DistanceUnit>(200, DistanceUnit.Yard);

        var unscaled = ReticleOverlayController.CalculateAngularSize(size, distance, 1.0);
        var scaled = ReticleOverlayController.CalculateAngularSize(size, distance, 2.0);

        (unscaled / scaled).Should().BeApproximately(2.0, 0.01);
    }

    #endregion
}
