using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using Xunit;
using AwesomeAssertions;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class ChartControllerTests
{
    #region Test Data

    /// <summary>
    /// Creates sample trajectory points for testing.
    /// Simulates a typical rifle trajectory from 0 to 500 meters.
    /// </summary>
    private static TrajectoryPoint[] CreateSampleTrajectory()
    {
        return new[]
        {
            CreatePoint(0, 900, 2.70, 0, 0, 0, 0, 0, 0),
            CreatePoint(100, 820, 2.46, 2.5, 2.0, 0.8, 1.5, 1.0, 0.4),
            CreatePoint(200, 745, 2.24, -1.2, -1.0, 0.5, 3.2, 2.1, 0.9),
            CreatePoint(300, 675, 2.03, -12.5, -10.0, 4.3, 5.1, 3.4, 1.5),
            CreatePoint(400, 610, 1.83, -35.8, -30.0, 11.6, 7.3, 4.9, 2.1),
            CreatePoint(500, 550, 1.65, -75.2, -65.0, 19.5, 9.8, 6.5, 2.8),
        };
    }

    private static TrajectoryPoint CreatePoint(
        double distanceM, double velocityMps, double mach,
        double dropCm, double dropFlatCm, double dropAdjMoa,
        double windageCm, double losElevationCm, double windageAdjMoa)
    {
        // Constructor signature from TrajectoryPoint.cs:
        // time, distance, distanceFlat, velocity, mach, drop, dropFlat, dropAdjustment,
        // lineOfSightElevation, lineOfDepartureElevation, windage, windageAdjustment, energy, optimalGameWeight
        return new TrajectoryPoint(
            time: distanceM > 0 ? TimeSpan.FromSeconds(distanceM / velocityMps) : TimeSpan.Zero,
            distance: new Measurement<DistanceUnit>(distanceM, DistanceUnit.Meter),
            distanceFlat: new Measurement<DistanceUnit>(distanceM, DistanceUnit.Meter),
            velocity: new Measurement<VelocityUnit>(velocityMps, VelocityUnit.MetersPerSecond),
            mach: mach,
            drop: new Measurement<DistanceUnit>(dropCm, DistanceUnit.Centimeter),
            dropFlat: new Measurement<DistanceUnit>(dropFlatCm, DistanceUnit.Centimeter),
            dropAdjustment: new Measurement<AngularUnit>(dropAdjMoa, AngularUnit.MOA),
            lineOfSightElevation: new Measurement<DistanceUnit>(losElevationCm, DistanceUnit.Centimeter),
            lineOfDepartureElevation: new Measurement<DistanceUnit>(0, DistanceUnit.Centimeter),
            windage: new Measurement<DistanceUnit>(windageCm, DistanceUnit.Centimeter),
            windageAdjustment: new Measurement<AngularUnit>(windageAdjMoa, AngularUnit.MOA),
            energy: new Measurement<EnergyUnit>(velocityMps * velocityMps * 0.01 / 2, EnergyUnit.Joule),
            optimalGameWeight: new Measurement<WeightUnit>(0, WeightUnit.Kilogram)
        );
    }

    #endregion

    #region Axis Title Tests

    [Theory]
    [InlineData(MeasurementSystem.Metric, "Range (m)")]
    [InlineData(MeasurementSystem.Imperial, "Range (yd)")]
    public void XAxisTitle_ForMeasurementSystem_ReturnsCorrectTitle(MeasurementSystem system, string expected)
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(system, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, trajectory);

        controller.XAxisTitle.Should().Be(expected);
    }

    [Theory]
    [InlineData(TrajectoryChartMode.Velocity, MeasurementSystem.Metric, "Velocity (m/s)")]
    [InlineData(TrajectoryChartMode.Velocity, MeasurementSystem.Imperial, "Velocity (ft/s)")]
    [InlineData(TrajectoryChartMode.Mach, MeasurementSystem.Metric, "Mach")]
    [InlineData(TrajectoryChartMode.Mach, MeasurementSystem.Imperial, "Mach")]
    [InlineData(TrajectoryChartMode.Energy, MeasurementSystem.Metric, "Energy (J)")]
    [InlineData(TrajectoryChartMode.Energy, MeasurementSystem.Imperial, "Energy (ft·lb)")]
    [InlineData(TrajectoryChartMode.Drop, MeasurementSystem.Metric, "Drop (cm)")]
    [InlineData(TrajectoryChartMode.Drop, MeasurementSystem.Imperial, "Drop (in)")]
    [InlineData(TrajectoryChartMode.Windage, MeasurementSystem.Metric, "Windage (cm)")]
    [InlineData(TrajectoryChartMode.Windage, MeasurementSystem.Imperial, "Windage (in)")]
    public void YAxisTitle_ForChartMode_ReturnsCorrectTitle(TrajectoryChartMode mode, MeasurementSystem system, string expected)
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(system, AngularUnit.MOA, mode, DropBase.SightLine, trajectory);

        controller.YAxisTitle.Should().Be(expected);
    }

    [Theory]
    [InlineData(TrajectoryChartMode.DropAdjustment, AngularUnit.MOA, "Drop (moa)")]
    [InlineData(TrajectoryChartMode.DropAdjustment, AngularUnit.MRad, "Drop (mrad)")]
    [InlineData(TrajectoryChartMode.WindageAdjustment, AngularUnit.MOA, "Windage (moa)")]
    [InlineData(TrajectoryChartMode.WindageAdjustment, AngularUnit.MRad, "Windage (mrad)")]
    public void YAxisTitle_ForAngularModes_ReturnsCorrectAngularUnit(TrajectoryChartMode mode, AngularUnit angularUnit, string expected)
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, angularUnit, mode, DropBase.SightLine, trajectory);

        controller.YAxisTitle.Should().Be(expected);
    }

    #endregion

    #region Series Count Tests

    [Theory]
    [InlineData(TrajectoryChartMode.Velocity, DropBase.SightLine, 1)]
    [InlineData(TrajectoryChartMode.Mach, DropBase.SightLine, 1)]
    [InlineData(TrajectoryChartMode.Energy, DropBase.SightLine, 1)]
    [InlineData(TrajectoryChartMode.Drop, DropBase.SightLine, 1)]
    [InlineData(TrajectoryChartMode.DropAdjustment, DropBase.SightLine, 1)]
    [InlineData(TrajectoryChartMode.Windage, DropBase.SightLine, 1)]
    [InlineData(TrajectoryChartMode.WindageAdjustment, DropBase.SightLine, 1)]
    [InlineData(TrajectoryChartMode.Drop, DropBase.MuzzlePoint, 2)]  // Special case
    [InlineData(TrajectoryChartMode.Velocity, DropBase.MuzzlePoint, 1)]  // MuzzlePoint only affects Drop mode
    public void SeriesCount_ForChartMode_ReturnsCorrectCount(TrajectoryChartMode mode, DropBase dropBase, int expected)
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, mode, dropBase, trajectory);

        controller.SeriesCount.Should().Be(expected);
    }

    #endregion

    #region Series Title Tests

    [Fact]
    public void GetSeriesTitle_ForDropModeWithMuzzlePoint_ReturnsTwoTitles()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.MuzzlePoint, trajectory);

        controller.GetSeriesTitle(0).Should().Be("Drop (cm)");
        controller.GetSeriesTitle(1).Should().Be("Line of Sight Elevation (cm)");
    }

    [Fact]
    public void GetSeriesTitle_ForDropModeWithSightLine_ReturnsYAxisTitle()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, trajectory);

        controller.GetSeriesTitle(0).Should().Be("Drop (cm)");
    }

    [Fact]
    public void GetSeriesTitle_ForVelocityMode_ReturnsYAxisTitle()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Velocity, DropBase.SightLine, trajectory);

        controller.GetSeriesTitle(0).Should().Be("Velocity (m/s)");
    }

    #endregion

    #region X Axis Data Tests

    [Fact]
    public void GetXAxis_Metric_ReturnsDistanceInMeters()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, trajectory);

        var xAxis = controller.GetXAxis();

        xAxis.Length.Should().Be(6);
        xAxis[0].Should().BeApproximately(0, 0.01);
        xAxis[1].Should().BeApproximately(100, 0.01);
        xAxis[2].Should().BeApproximately(200, 0.01);
        xAxis[5].Should().BeApproximately(500, 0.01);
    }

    [Fact]
    public void GetXAxis_Imperial_ReturnsDistanceInYards()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Imperial, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, trajectory);

        var xAxis = controller.GetXAxis();

        // 100m ≈ 109.36 yards
        xAxis[1].Should().BeApproximately(109.36, 0.5);
    }

    [Fact]
    public void GetXAxisPoint_ReturnsCorrectValue()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, trajectory);

        controller.GetXAxisPoint(0).Should().BeApproximately(0, 0.01);
        controller.GetXAxisPoint(3).Should().BeApproximately(300, 0.01);
    }

    #endregion

    #region Y Axis Data Tests - Velocity

    [Fact]
    public void GetYAxis_VelocityMetric_ReturnsVelocityInMps()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Velocity, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(1);
        yAxis[0][0].Should().BeApproximately(900, 0.01);
        yAxis[0][1].Should().BeApproximately(820, 0.01);
    }

    [Fact]
    public void GetYAxis_VelocityImperial_ReturnsVelocityInFps()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Imperial, AngularUnit.MOA, TrajectoryChartMode.Velocity, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        // 900 m/s ≈ 2953 ft/s
        yAxis[0][0].Should().BeApproximately(2953, 5);
    }

    #endregion

    #region Y Axis Data Tests - Mach

    [Fact]
    public void GetYAxis_Mach_ReturnsMachNumber()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Mach, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(1);
        yAxis[0][0].Should().BeApproximately(2.70, 0.01);
        yAxis[0][5].Should().BeApproximately(1.65, 0.01);
    }

    #endregion

    #region Y Axis Data Tests - Drop

    [Fact]
    public void GetYAxis_DropSightLine_ReturnsDropInCm()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(1);
        yAxis[0][0].Should().BeApproximately(0, 0.01);
        yAxis[0][2].Should().BeApproximately(-1.2, 0.01);  // Drop at 200m
        yAxis[0][5].Should().BeApproximately(-75.2, 0.01); // Drop at 500m
    }

    [Fact]
    public void GetYAxis_DropMuzzlePoint_ReturnsTwoSeries()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.MuzzlePoint, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(2);
        // Series 0: DropFlat
        yAxis[0][2].Should().BeApproximately(-1.0, 0.01);
        // Series 1: LineOfSightElevation
        yAxis[1][2].Should().BeApproximately(2.1, 0.01);
    }

    [Fact]
    public void GetYAxis_DropImperial_ReturnsDropInInches()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Imperial, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        // -1.2 cm ≈ -0.47 inches
        yAxis[0][2].Should().BeApproximately(-0.47, 0.05);
    }

    #endregion

    #region Y Axis Data Tests - Drop Adjustment

    [Fact]
    public void GetYAxis_DropAdjustmentMOA_ReturnsAdjustmentInMOA()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.DropAdjustment, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(1);
        yAxis[0][3].Should().BeApproximately(4.3, 0.01);  // At 300m
    }

    [Fact]
    public void GetYAxis_DropAdjustmentMRad_ReturnsAdjustmentInMRad()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MRad, TrajectoryChartMode.DropAdjustment, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        // 4.3 MOA ≈ 1.25 mrad
        yAxis[0][3].Should().BeApproximately(1.25, 0.05);
    }

    #endregion

    #region Y Axis Data Tests - Windage

    [Fact]
    public void GetYAxis_Windage_ReturnsWindageInCm()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Windage, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(1);
        yAxis[0][3].Should().BeApproximately(5.1, 0.01);  // At 300m
    }

    [Fact]
    public void GetYAxis_WindageAdjustment_ReturnsWindageInMOA()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.WindageAdjustment, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis[0][3].Should().BeApproximately(1.5, 0.01);  // At 300m
    }

    #endregion

    #region Y Axis Data Tests - Energy

    [Fact]
    public void GetYAxis_EnergyMetric_ReturnsEnergyInJoules()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Energy, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(1);
        // Energy should decrease as velocity decreases
        yAxis[0][0].Should().BeGreaterThan(yAxis[0][5]);
    }

    [Fact]
    public void GetYAxis_EnergyImperial_ReturnsEnergyInFootPounds()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Imperial, AngularUnit.MOA, TrajectoryChartMode.Energy, DropBase.SightLine, trajectory);

        var yAxis = controller.GetYAxis();

        // Energy in foot-pounds should be less than Joules (1 J ≈ 0.74 ft-lb)
        var metricController = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Energy, DropBase.SightLine, trajectory);
        var metricEnergy = metricController.GetYAxis()[0][0];

        yAxis[0][0].Should().BeLessThan(metricEnergy);
    }

    #endregion

    #region GetYAxisPoint Tests

    [Fact]
    public void GetYAxisPoint_ReturnsCorrectValue()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Velocity, DropBase.SightLine, trajectory);

        controller.GetYAxisPoint(0).Should().BeApproximately(900, 0.01);
        controller.GetYAxisPoint(3).Should().BeApproximately(675, 0.01);
    }

    [Fact]
    public void GetYAxisPoint_DropMuzzlePoint_SeriesIndex0_ReturnsDropFlat()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.MuzzlePoint, trajectory);

        controller.GetYAxisPoint(2, 0).Should().BeApproximately(-1.0, 0.01);
    }

    [Fact]
    public void GetYAxisPoint_DropMuzzlePoint_SeriesIndex1_ReturnsLineOfSightElevation()
    {
        var trajectory = CreateSampleTrajectory();
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.MuzzlePoint, trajectory);

        controller.GetYAxisPoint(2, 1).Should().BeApproximately(2.1, 0.01);
    }

    #endregion

    #region Empty Trajectory Tests

    [Fact]
    public void GetXAxis_EmptyTrajectory_ReturnsEmptyArray()
    {
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, Array.Empty<TrajectoryPoint>());

        var xAxis = controller.GetXAxis();

        xAxis.Should().BeEmpty();
    }

    [Fact]
    public void GetYAxis_EmptyTrajectory_ReturnsListWithEmptyArray()
    {
        var controller = new ChartController(MeasurementSystem.Metric, AngularUnit.MOA, TrajectoryChartMode.Drop, DropBase.SightLine, Array.Empty<TrajectoryPoint>());

        var yAxis = controller.GetYAxis();

        yAxis.Count.Should().Be(1);
        yAxis[0].Should().BeEmpty();
    }

    #endregion
}
