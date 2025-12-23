using System.Globalization;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using Xunit;
using AwesomeAssertions;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class MeasurementSystemControllerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithMetric_ShouldSetMeasurementSystem()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.MeasurementSystem.Should().Be(MeasurementSystem.Metric);
    }

    [Fact]
    public void Constructor_WithImperial_ShouldSetMeasurementSystem()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
    }

    [Fact]
    public void Constructor_DefaultAngularUnit_ShouldBeMOA()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.AngularUnit.Should().Be(AngularUnit.MOA);
    }

    [Fact]
    public void Constructor_WithAngularUnit_ShouldSetAngularUnit()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MRad);

        controller.AngularUnit.Should().Be(AngularUnit.MRad);
    }

    #endregion

    #region Metric Unit Tests

    [Fact]
    public void Metric_RangeUnit_ShouldBeMeter()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.RangeUnit.Should().Be(DistanceUnit.Meter);
    }

    [Fact]
    public void Metric_AdjustmentUnit_ShouldBeCentimeter()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.AdjustmentUnit.Should().Be(DistanceUnit.Centimeter);
    }

    [Fact]
    public void Metric_VelocityUnit_ShouldBeMetersPerSecond()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.VelocityUnit.Should().Be(VelocityUnit.MetersPerSecond);
    }

    [Fact]
    public void Metric_EnergyUnit_ShouldBeJoule()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.EnergyUnit.Should().Be(EnergyUnit.Joule);
    }

    [Fact]
    public void Metric_WeightUnit_ShouldBeKilogram()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.WeightUnit.Should().Be(WeightUnit.Kilogram);
    }

    #endregion

    #region Imperial Unit Tests

    [Fact]
    public void Imperial_RangeUnit_ShouldBeYard()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.RangeUnit.Should().Be(DistanceUnit.Yard);
    }

    [Fact]
    public void Imperial_AdjustmentUnit_ShouldBeInch()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.AdjustmentUnit.Should().Be(DistanceUnit.Inch);
    }

    [Fact]
    public void Imperial_VelocityUnit_ShouldBeFeetPerSecond()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.VelocityUnit.Should().Be(VelocityUnit.FeetPerSecond);
    }

    [Fact]
    public void Imperial_EnergyUnit_ShouldBeFootPound()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.EnergyUnit.Should().Be(EnergyUnit.FootPound);
    }

    [Fact]
    public void Imperial_WeightUnit_ShouldBePound()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.WeightUnit.Should().Be(WeightUnit.Pound);
    }

    #endregion

    #region Metric Unit Name Tests

    [Fact]
    public void Metric_RangeUnitName_ShouldBeM()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.RangeUnitName.Should().Be("m");
    }

    [Fact]
    public void Metric_AdjustmentUnitName_ShouldBeCm()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.AdjustmentUnitName.Should().Be("cm");
    }

    [Fact]
    public void Metric_VelocityUnitName_ShouldBeMS()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.VelocityUnitName.Should().Be("m/s");
    }

    [Fact]
    public void Metric_EnergyUnitName_ShouldBeJ()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.EnergyUnitName.Should().Be("J");
    }

    [Fact]
    public void Metric_WeightUnitName_ShouldBeKg()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.WeightUnitName.Should().Be("kg");
    }

    #endregion

    #region Imperial Unit Name Tests

    [Fact]
    public void Imperial_RangeUnitName_ShouldBeYd()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.RangeUnitName.Should().Be("yd");
    }

    [Fact]
    public void Imperial_AdjustmentUnitName_ShouldBeIn()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.AdjustmentUnitName.Should().Be("in");
    }

    [Fact]
    public void Imperial_VelocityUnitName_ShouldBeFtS()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.VelocityUnitName.Should().Be("ft/s");
    }

    [Fact]
    public void Imperial_EnergyUnitName_ShouldBeFtLb()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.EnergyUnitName.Should().Be("ft·lb");
    }

    [Fact]
    public void Imperial_WeightUnitName_ShouldBeLb()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.WeightUnitName.Should().Be("lb");
    }

    #endregion

    #region Angular Unit Name Tests

    [Theory]
    [InlineData(AngularUnit.MOA, "moa")]
    [InlineData(AngularUnit.MRad, "mrad")]
    [InlineData(AngularUnit.Mil, "mil")]
    [InlineData(AngularUnit.Degree, "deg")]
    [InlineData(AngularUnit.Radian, "rad")]
    [InlineData(AngularUnit.CmPer100Meters, "cm/100m")]
    [InlineData(AngularUnit.InchesPer100Yards, "in/100yd")]
    public void AngularUnitName_ForUnit_ShouldReturnCorrectName(AngularUnit unit, string expectedName)
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, unit);

        controller.AngularUnitName.Should().Be(expectedName);
    }

    #endregion

    #region Static Accuracy Tests

    [Fact]
    public void RangeAccuracy_ShouldBeZero()
    {
        MeasurementSystemController.RangeAccuracy.Should().Be(0);
    }

    [Fact]
    public void AdjustmentAccuracy_ShouldBeTwo()
    {
        MeasurementSystemController.AdjustmentAccuracy.Should().Be(2);
    }

    [Fact]
    public void VelocityAccuracy_ShouldBeZero()
    {
        MeasurementSystemController.VelocityAccuracy.Should().Be(0);
    }

    [Fact]
    public void EnergyAccuracy_ShouldBeZero()
    {
        MeasurementSystemController.EnergyAccuracy.Should().Be(0);
    }

    [Fact]
    public void MachAccuracy_ShouldBeTwo()
    {
        MeasurementSystemController.MachAccuracy.Should().Be(2);
    }

    #endregion

    #region Format String Tests

    [Fact]
    public void RangeFormatString_ShouldBeN0()
    {
        MeasurementSystemController.RangeFormatString.Should().Be("N0");
    }

    [Fact]
    public void AdjustmentFormatString_ShouldBeN2()
    {
        MeasurementSystemController.AdjustmentFormatString.Should().Be("N2");
    }

    [Fact]
    public void VelocityFormatStringF_ShouldBeF0()
    {
        MeasurementSystemController.VelocityFormatStringF.Should().Be("F0");
    }

    [Fact]
    public void MachFormatStringF_ShouldBeF2()
    {
        MeasurementSystemController.MachFormatStringF.Should().Be("F2");
    }

    [Fact]
    public void TimeFormatString_ShouldBeCorrect()
    {
        MeasurementSystemController.TimeFormatString.Should().Be(@"mm\:ss\.fff");
    }

    #endregion

    #region Property Mutability Tests

    [Fact]
    public void MeasurementSystem_ShouldBeMutable()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.MeasurementSystem = MeasurementSystem.Imperial;

        controller.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
        controller.RangeUnit.Should().Be(DistanceUnit.Yard);
    }

    [Fact]
    public void AngularUnit_ShouldBeMutable()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MOA);

        controller.AngularUnit = AngularUnit.MRad;

        controller.AngularUnit.Should().Be(AngularUnit.MRad);
        controller.AngularUnitName.Should().Be("mrad");
    }

    #endregion

    #region Formatting Method Tests

    [Fact]
    public void FormatRange_Metric_FormatsInMeters()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);
        var distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter);

        var result = controller.FormatRange(distance, CultureInfo.InvariantCulture);

        result.Should().Be("100");
    }

    [Fact]
    public void FormatRange_Imperial_FormatsInYards()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);
        var distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter);

        var result = controller.FormatRange(distance, CultureInfo.InvariantCulture);

        // 100m ≈ 109.36 yards
        result.Should().Be("109");
    }

    [Fact]
    public void FormatVelocity_Metric_FormatsInMps()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);
        var velocity = new Measurement<VelocityUnit>(800, VelocityUnit.MetersPerSecond);

        var result = controller.FormatVelocity(velocity, CultureInfo.InvariantCulture);

        result.Should().Be("800");
    }

    [Fact]
    public void FormatVelocity_Imperial_FormatsInFps()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);
        var velocity = new Measurement<VelocityUnit>(800, VelocityUnit.MetersPerSecond);

        var result = controller.FormatVelocity(velocity, CultureInfo.InvariantCulture);

        // 800 m/s ≈ 2625 ft/s
        result.Should().Be("2,625");
    }

    [Fact]
    public void FormatMach_FormatsWithTwoDecimals()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        var result = controller.FormatMach(2.345, CultureInfo.InvariantCulture);

        result.Should().Be("2.35");
    }

    [Fact]
    public void FormatAdjustment_Metric_FormatsInCentimeters()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);
        var drop = new Measurement<DistanceUnit>(-15.5, DistanceUnit.Centimeter);

        var result = controller.FormatAdjustment(drop, CultureInfo.InvariantCulture);

        result.Should().Be("-15.50");
    }

    [Fact]
    public void FormatAdjustment_Imperial_FormatsInInches()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);
        var drop = new Measurement<DistanceUnit>(-15.5, DistanceUnit.Centimeter);

        var result = controller.FormatAdjustment(drop, CultureInfo.InvariantCulture);

        // -15.5 cm ≈ -6.10 in
        result.Should().Be("-6.10");
    }

    [Fact]
    public void FormatAngular_MOA_FormatsInMOA()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MOA);
        var adjustment = new Measurement<AngularUnit>(4.5, AngularUnit.MOA);

        var result = controller.FormatAngular(adjustment, CultureInfo.InvariantCulture);

        result.Should().Be("4.50");
    }

    [Fact]
    public void FormatAngular_MRad_ConvertsAndFormats()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MRad);
        var adjustment = new Measurement<AngularUnit>(4.5, AngularUnit.MOA);

        var result = controller.FormatAngular(adjustment, CultureInfo.InvariantCulture);

        // 4.5 MOA ≈ 1.31 mrad
        result.Should().Be("1.31");
    }

    [Fact]
    public void FormatEnergy_Metric_FormatsInJoules()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);
        var energy = new Measurement<EnergyUnit>(3200, EnergyUnit.Joule);

        var result = controller.FormatEnergy(energy, CultureInfo.InvariantCulture);

        result.Should().Be("3,200");
    }

    [Fact]
    public void FormatEnergy_Imperial_FormatsInFootPounds()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);
        var energy = new Measurement<EnergyUnit>(3200, EnergyUnit.Joule);

        var result = controller.FormatEnergy(energy, CultureInfo.InvariantCulture);

        // 3200 J ≈ 2360 ft-lb
        result.Should().Be("2,360");
    }

    [Fact]
    public void FormatWeight_Metric_FormatsInKilograms()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);
        var weight = new Measurement<WeightUnit>(50, WeightUnit.Kilogram);

        var result = controller.FormatWeight(weight, CultureInfo.InvariantCulture);

        result.Should().Be("50.0");
    }

    [Fact]
    public void FormatWeight_Imperial_FormatsInPounds()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);
        var weight = new Measurement<WeightUnit>(50, WeightUnit.Kilogram);

        var result = controller.FormatWeight(weight, CultureInfo.InvariantCulture);

        // 50 kg ≈ 110.2 lb
        result.Should().Be("110.2");
    }

    [Fact]
    public void FormatTime_FormatsCorrectly()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);
        var time = TimeSpan.FromSeconds(1.234);

        var result = controller.FormatTime(time);

        result.Should().Be("00:01.234");
    }

    [Fact]
    public void FormatClicks_WithValidClickSize_CalculatesClicks()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MOA);
        var adjustment = new Measurement<AngularUnit>(4.5, AngularUnit.MOA);
        var clickSize = new Measurement<AngularUnit>(0.25, AngularUnit.MOA);

        var result = controller.FormatClicks(adjustment, clickSize, CultureInfo.InvariantCulture);

        // 4.5 / 0.25 = 18 clicks
        result.Should().Be("18");
    }

    [Fact]
    public void FormatClicks_WithNullClickSize_ReturnsNA()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MOA);
        var adjustment = new Measurement<AngularUnit>(4.5, AngularUnit.MOA);

        var result = controller.FormatClicks(adjustment, null, CultureInfo.InvariantCulture);

        result.Should().Be("n/a");
    }

    [Fact]
    public void FormatClicks_WithZeroClickSize_ReturnsNA()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MOA);
        var adjustment = new Measurement<AngularUnit>(4.5, AngularUnit.MOA);
        var clickSize = new Measurement<AngularUnit>(0, AngularUnit.MOA);

        var result = controller.FormatClicks(adjustment, clickSize, CultureInfo.InvariantCulture);

        result.Should().Be("n/a");
    }

    #endregion

    #region Column Header Tests

    [Fact]
    public void RangeHeader_Metric_IncludesMeters()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.RangeHeader.Should().Be("Range (m)");
    }

    [Fact]
    public void RangeHeader_Imperial_IncludesYards()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.RangeHeader.Should().Be("Range (yd)");
    }

    [Fact]
    public void VelocityHeader_Metric_IncludesMps()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.VelocityHeader.Should().Be("Velocity (m/s)");
    }

    [Fact]
    public void VelocityHeader_Imperial_IncludesFps()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.VelocityHeader.Should().Be("Velocity (ft/s)");
    }

    [Fact]
    public void MachHeader_IsMach()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.MachHeader.Should().Be("Mach");
    }

    [Fact]
    public void DropHeader_Metric_IncludesCentimeters()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.DropHeader.Should().Be("Drop (cm)");
    }

    [Fact]
    public void DropHeader_Imperial_IncludesInches()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.DropHeader.Should().Be("Drop (in)");
    }

    [Fact]
    public void DropAdjustmentHeader_MOA_IncludesMOA()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MOA);

        controller.DropAdjustmentHeader.Should().Be("Hold (moa)");
    }

    [Fact]
    public void DropAdjustmentHeader_MRad_IncludesMRad()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric, AngularUnit.MRad);

        controller.DropAdjustmentHeader.Should().Be("Hold (mrad)");
    }

    [Fact]
    public void EnergyHeader_Metric_IncludesJoules()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.EnergyHeader.Should().Be("Energy (J)");
    }

    [Fact]
    public void EnergyHeader_Imperial_IncludesFootPounds()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.EnergyHeader.Should().Be("Energy (ft·lb)");
    }

    [Fact]
    public void WeightHeader_Metric_IncludesKilograms()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Metric);

        controller.WeightHeader.Should().Be("O.G.W. (kg)");
    }

    [Fact]
    public void WeightHeader_Imperial_IncludesPounds()
    {
        var controller = new MeasurementSystemController(MeasurementSystem.Imperial);

        controller.WeightHeader.Should().Be("O.G.W. (lb)");
    }

    #endregion
}
