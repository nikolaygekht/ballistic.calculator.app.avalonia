using System;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// The BC converter is a pure ratio of two drag curves at one reference, so it can be pinned down exactly:
/// a round trip must return the input, and the G1/G7 ratio must land where the published curves put it.
/// </summary>
public class BcConversionCalculatorTests
{
    private static readonly Measurement<VelocityUnit> Fps2700 = new(2700, VelocityUnit.FeetPerSecond);

    private static BallisticCoefficient G1(double value) => new(value, DragTableId.G1);

    #region Conversion

    [Fact]
    public void Convert_G1ToG7_ShouldRoughlyHalveTheCoefficient()
    {
        // Arrange: 2700 ft/s is about Mach 2.4, where the G1/G7 drag ratio sits near 2.0
        var source = G1(0.462);

        // Act
        var result = BcConversionCalculator.Convert(source, DragTableId.G7, Fps2700);

        // Assert
        result.Converted.Table.Should().Be(DragTableId.G7);
        var ratio = source.Value / result.Converted.Value;
        ratio.Should().BeInRange(1.85, 2.15, "the G1/G7 drag ratio is near 2.0 at supersonic velocity");
    }

    [Fact]
    public void Convert_ThereAndBack_ShouldReturnTheSourceAtTheSameReference()
    {
        // Arrange
        var source = G1(0.365);

        // Act — the conversion is a ratio, so the same reference must undo it exactly
        var g7 = BcConversionCalculator.Convert(source, DragTableId.G7, Fps2700).Converted;
        var back = BcConversionCalculator.Convert(g7, DragTableId.G1, Fps2700).Converted;

        // Assert
        back.Table.Should().Be(DragTableId.G1);
        back.Value.Should().BeApproximately(source.Value, 1e-9);
    }

    [Fact]
    public void Convert_ToTheSameTable_ShouldReturnTheInputUnchanged()
    {
        var result = BcConversionCalculator.Convert(G1(0.365), DragTableId.G1, Fps2700);

        result.Converted.Table.Should().Be(DragTableId.G1);
        result.Converted.Value.Should().Be(0.365);
    }

    [Fact]
    public void Convert_ShouldReportTheReferenceMach()
    {
        // Arrange & Act — standard atmosphere sound velocity is about 1116 ft/s
        var result = BcConversionCalculator.Convert(G1(0.365), DragTableId.G7, Fps2700);

        // Assert
        result.ReferenceMach.Should().BeApproximately(2.42, 0.05);
        result.ReferenceVelocity.Should().Be(Fps2700);
    }

    [Fact]
    public void Convert_InThinnerAirAtLowerTemperature_ShouldGiveADifferentMachForTheSameVelocity()
    {
        // Arrange: only the speed of sound matters, and that follows temperature
        var cold = new Atmosphere(new Measurement<DistanceUnit>(10000, DistanceUnit.Foot),
                                  new Measurement<PressureUnit>(20.6, PressureUnit.InchesOfMercury),
                                  new Measurement<TemperatureUnit>(-5, TemperatureUnit.Fahrenheit),
                                  0.2);

        // Act
        var standard = BcConversionCalculator.Convert(G1(0.365), DragTableId.G7, Fps2700);
        var thin = BcConversionCalculator.Convert(G1(0.365), DragTableId.G7, Fps2700, cold);

        // Assert — colder air, slower sound, higher Mach for the same velocity
        thin.ReferenceMach.Should().BeGreaterThan(standard.ReferenceMach);
        thin.Converted.Value.Should().NotBe(standard.Converted.Value);
    }

    #endregion

    #region Transonic honesty

    [Theory]
    [InlineData(1200, true)]   // about Mach 1.08 — the conversion is unreliable here
    [InlineData(1600, true)]   // about Mach 1.43
    [InlineData(1800, false)]  // about Mach 1.61
    [InlineData(2700, false)]  // about Mach 2.42 — the good band
    public void Convert_BelowMachOneAndAHalf_ShouldFlagTheReferenceAsTransonic(double fps, bool expected)
    {
        var result = BcConversionCalculator.Convert(G1(0.365), DragTableId.G7,
                                                    new Measurement<VelocityUnit>(fps, VelocityUnit.FeetPerSecond));

        result.IsTransonic.Should().Be(expected);
    }

    [Fact]
    public void Convert_NearTheTransonicRegion_ShouldDifferMateriallyFromTheSupersonicAnswer()
    {
        // Arrange: the two curves diverge in shape below Mach 1.5, which is why one number is not enough
        var source = G1(0.365);

        // Act
        var near = BcConversionCalculator.Convert(source, DragTableId.G7,
                                                  new Measurement<VelocityUnit>(1450, VelocityUnit.FeetPerSecond));
        var far = BcConversionCalculator.Convert(source, DragTableId.G7, Fps2700);

        // Assert
        var difference = Math.Abs(near.Converted.Value - far.Converted.Value) / far.Converted.Value;
        difference.Should().BeGreaterThan(0.02, "a transonic reference gives a visibly different answer");
    }

    #endregion

    #region Refused input

    [Fact]
    public void Convert_WithNoSource_ShouldSayWhatIsMissing()
    {
        var act = () => BcConversionCalculator.Convert(null, DragTableId.G7, Fps2700);

        act.Should().Throw<ArgumentException>().WithMessage("*source ballistic coefficient*");
    }

    [Fact]
    public void Convert_WithAFormFactor_ShouldRefuseItReadably()
    {
        var formFactor = new BallisticCoefficient(1.0, DragTableId.G1, BallisticCoefficientValueType.FormFactor);

        var act = () => BcConversionCalculator.Convert(formFactor, DragTableId.G7, Fps2700);

        act.Should().Throw<ArgumentException>().WithMessage("*form factor*");
    }

    [Fact]
    public void Convert_FromTheCustomTable_ShouldRefuseItReadably()
    {
        var custom = new BallisticCoefficient(0.5, DragTableId.GC);

        var act = () => BcConversionCalculator.Convert(custom, DragTableId.G7, Fps2700);

        act.Should().Throw<ArgumentException>().WithMessage("*GC*");
    }

    [Fact]
    public void Convert_ToTheCustomTable_ShouldRefuseItReadably()
    {
        var act = () => BcConversionCalculator.Convert(G1(0.365), DragTableId.GC, Fps2700);

        act.Should().Throw<ArgumentException>().WithMessage("*GC*");
    }

    [Fact]
    public void Convert_WithNoReferenceVelocity_ShouldSayWhatIsMissing()
    {
        var act = () => BcConversionCalculator.Convert(G1(0.365), DragTableId.G7, null);

        act.Should().Throw<ArgumentException>().WithMessage("*reference velocity*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Convert_WithANonPositiveReferenceVelocity_ShouldSayWhatIsWrong(double fps)
    {
        var act = () => BcConversionCalculator.Convert(G1(0.365), DragTableId.G7,
                                                       new Measurement<VelocityUnit>(fps, VelocityUnit.FeetPerSecond));

        act.Should().Throw<ArgumentException>().WithMessage("*greater than zero*");
    }

    [Fact]
    public void Convert_WithANonPositiveCoefficient_ShouldSayWhatIsWrong()
    {
        var act = () => BcConversionCalculator.Convert(G1(0), DragTableId.G7, Fps2700);

        act.Should().Throw<ArgumentException>().WithMessage("*greater than zero*");
    }

    #endregion

    #region Table list

    [Fact]
    public void StandardTables_ShouldListEveryTableExceptTheCustomOne()
    {
        var tables = BcConversionCalculator.StandardTables;

        tables.Should().NotContain(DragTableId.GC, "the custom table has no fixed curve to convert against");
        tables.Should().Contain(DragTableId.G1).And.Contain(DragTableId.G7);
        tables.Should().HaveCount(Enum.GetValues<DragTableId>().Length - 1);
    }

    #endregion
}
