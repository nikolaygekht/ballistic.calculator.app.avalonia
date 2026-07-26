using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// Tolerant parsing of the unit-suffixed values found in real BC/radar exports: the library's own parser
/// accepts "ft/s" but not "fps", and accepts a decimal comma as a thousands separator.
/// </summary>
public class MeasurementTextParserTests
{
    #region Velocity

    [Theory]
    [InlineData("3078.800ft/s", 3078.8)]
    [InlineData("3078.800fps", 3078.8)]      // alias the library itself rejects
    [InlineData("2695.9 fps", 2695.9)]
    [InlineData("1554FPS", 1554)]            // aliases are case-insensitive
    [InlineData("850m/s", 2788.71)]
    [InlineData("850mps", 2788.71)]
    public void TryParseVelocity_ShouldAcceptUnitsAndAliases(string text, double expectedFps)
    {
        MeasurementTextParser.TryParseVelocity(text, VelocityUnit.FeetPerSecond, out var value).Should().BeTrue();

        value.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(expectedFps, 0.01);
    }

    [Fact]
    public void TryParseVelocity_BareNumber_ShouldUseFallbackUnit()
    {
        MeasurementTextParser.TryParseVelocity("2333", VelocityUnit.MetersPerSecond, out var value).Should().BeTrue();

        value.Unit.Should().Be(VelocityUnit.MetersPerSecond);
        value.Value.Should().Be(2333);
    }

    [Fact]
    public void TryParseVelocity_DecimalComma_ShouldNotBecomeThousands()
    {
        // The library's own parser returns 7802 m/s here, because ',' is the invariant group separator.
        MeasurementTextParser.TryParseVelocity("780,2m/s", VelocityUnit.MetersPerSecond, out var value).Should().BeTrue();

        value.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(780.2, 0.001);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("velocity")]
    [InlineData("1400 d")]      // the real typo from velocity2.csv
    [InlineData("abc ft/s")]
    public void TryParseVelocity_Garbage_ShouldFail(string text)
    {
        MeasurementTextParser.TryParseVelocity(text, VelocityUnit.FeetPerSecond, out _).Should().BeFalse();
    }

    #endregion

    #region Distance

    [Theory]
    [InlineData("0yd", 0)]
    [InlineData("100 yd", 100)]
    [InlineData("1500yds", 1500)]        // alias
    [InlineData("300m", 328.084)]
    public void TryParseDistance_ShouldAcceptUnitsAndAliases(string text, double expectedYards)
    {
        MeasurementTextParser.TryParseDistance(text, DistanceUnit.Yard, out var value).Should().BeTrue();

        value.In(DistanceUnit.Yard).Should().BeApproximately(expectedYards, 0.01);
    }

    [Fact]
    public void TryParseDistance_BareNumber_ShouldUseFallbackUnit()
    {
        MeasurementTextParser.TryParseDistance("250", DistanceUnit.Meter, out var value).Should().BeTrue();

        value.Unit.Should().Be(DistanceUnit.Meter);
        value.Value.Should().Be(250);
    }

    [Fact]
    public void TryParseDistance_Zero_ShouldBeAccepted()
    {
        // Radar files start at the muzzle; 0 is a legitimate distance, not a missing value.
        MeasurementTextParser.TryParseDistance("0", DistanceUnit.Meter, out var value).Should().BeTrue();

        value.Value.Should().Be(0);
    }

    [Theory]
    [InlineData("1400 d")]
    [InlineData("distance")]
    [InlineData("")]
    public void TryParseDistance_Garbage_ShouldFail(string text)
    {
        MeasurementTextParser.TryParseDistance(text, DistanceUnit.Yard, out _).Should().BeFalse();
    }

    #endregion

    #region Ballistic coefficient

    [Fact]
    public void TryParseBc_WithTableId_ShouldKeepTheFilesTable()
    {
        MeasurementTextParser.TryParseBc("0.462G7", DragTableId.G1, out var bc).Should().BeTrue();

        bc.Value.Should().BeApproximately(0.462, 1e-9);
        bc.Table.Should().Be(DragTableId.G7);
    }

    [Fact]
    public void TryParseBc_BareNumber_ShouldUseFallbackTable()
    {
        MeasurementTextParser.TryParseBc("0.480", DragTableId.G7, out var bc).Should().BeTrue();

        bc.Value.Should().BeApproximately(0.480, 1e-9);
        bc.Table.Should().Be(DragTableId.G7);
    }

    [Fact]
    public void TryParseBc_DecimalComma_ShouldBeAccepted()
    {
        MeasurementTextParser.TryParseBc("0,462G1", DragTableId.G7, out var bc).Should().BeTrue();

        bc.Value.Should().BeApproximately(0.462, 1e-9);
        bc.Table.Should().Be(DragTableId.G1);
    }

    [Fact]
    public void TryParseBc_FormFactor_ShouldBeAccepted()
    {
        MeasurementTextParser.TryParseBc("F1GC", DragTableId.G7, out var bc).Should().BeTrue();

        bc.ValueType.Should().Be(BallisticCoefficientValueType.FormFactor);
        bc.Table.Should().Be(DragTableId.GC);
    }

    [Theory]
    [InlineData("bc")]
    [InlineData("")]
    [InlineData("0.462G9")]      // no such drag table
    [InlineData("-0.5")]         // a BC must be positive
    [InlineData("0")]
    public void TryParseBc_Garbage_ShouldFail(string text)
    {
        MeasurementTextParser.TryParseBc(text, DragTableId.G7, out _).Should().BeFalse();
    }

    #endregion

    #region Plain numbers (Mach)

    [Theory]
    [InlineData("1.5", 1.5)]
    [InlineData("1,5", 1.5)]
    [InlineData(" 2 ", 2)]
    public void TryParseDouble_ShouldAcceptBothDecimalMarks(string text, double expected)
    {
        MeasurementTextParser.TryParseDouble(text, out var value).Should().BeTrue();

        value.Should().BeApproximately(expected, 1e-9);
    }

    [Theory]
    [InlineData("mach")]
    [InlineData("")]
    [InlineData("1.5ft/s")]      // a Mach number carries no unit
    public void TryParseDouble_Garbage_ShouldFail(string text)
    {
        MeasurementTextParser.TryParseDouble(text, out _).Should().BeFalse();
    }

    #endregion
}
