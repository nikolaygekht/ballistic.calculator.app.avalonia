using Gehtsoft.Measurements;
using BallisticCalculator.Controls.Controllers;
using System.Globalization;
using Xunit;
using AwesomeAssertions;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class MeasurementControllerTests
{
    #region Unit Enumeration Tests

    [Fact]
    public void GetUnits_ForDistanceUnit_ShouldReturnAllUnits()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var units = controller.GetUnits(out int defaultIndex);

        // Assert - Should have all distance units
        units.Should().NotBeEmpty();
        units.Should().Contain(u => u.Name == "m"); // Meter
        units.Should().Contain(u => u.Name == "ft"); // Foot
        units.Should().Contain(u => u.Name == "yd"); // Yard
    }

    [Fact]
    public void GetUnits_ForDistanceUnit_ShouldHaveInchAsBaseUnit()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var units = controller.GetUnits(out int defaultIndex);

        // Assert - Base unit for Distance is Inch
        units[defaultIndex].Unit.Should().Be(DistanceUnit.Inch);
    }

    [Fact]
    public void GetUnits_ForVelocityUnit_ShouldReturnAllUnits()
    {
        // Arrange
        var controller = new MeasurementController<VelocityUnit>();

        // Act
        var units = controller.GetUnits(out int defaultIndex);

        // Assert
        units.Should().NotBeEmpty();
        units.Should().Contain(u => u.Name == "m/s"); // MetersPerSecond
        units.Should().Contain(u => u.Name == "ft/s"); // FeetPerSecond
        units.Should().Contain(u => u.Name == "mi/h"); // MilesPerHour
    }

    [Fact]
    public void GetUnits_ForVelocityUnit_ShouldHaveMetersPerSecondAsBaseUnit()
    {
        // Arrange
        var controller = new MeasurementController<VelocityUnit>();

        // Act
        var units = controller.GetUnits(out int defaultIndex);

        // Assert - Base unit for Velocity is MetersPerSecond
        units[defaultIndex].Unit.Should().Be(VelocityUnit.MetersPerSecond);
    }

    [Fact]
    public void GetUnits_ForWeightUnit_ShouldReturnAllUnits()
    {
        // Arrange
        var controller = new MeasurementController<WeightUnit>();

        // Act
        var units = controller.GetUnits(out int defaultIndex);

        // Assert
        units.Should().NotBeEmpty();
        units.Should().Contain(u => u.Name == "gr"); // Grain
        units.Should().Contain(u => u.Name == "g"); // Gram
        units.Should().Contain(u => u.Name == "lb"); // Pound
    }

    [Fact]
    public void GetUnits_ForWeightUnit_ShouldHaveGrainAsBaseUnit()
    {
        // Arrange
        var controller = new MeasurementController<WeightUnit>();

        // Act
        var units = controller.GetUnits(out int defaultIndex);

        // Assert - Base unit for Weight is Grain
        units[defaultIndex].Unit.Should().Be(WeightUnit.Grain);
    }

    #endregion

    #region Value Parsing Tests

    [Fact]
    public void Value_WithValidText_ShouldReturnMeasurement()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.Value("100", DistanceUnit.Meter, null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().NotBeNull();
        result.Value.Value.Should().Be(100);
        result.Value.Unit.Should().Be(DistanceUnit.Meter);
    }

    [Fact]
    public void Value_WithEmptyText_ShouldReturnNull()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.Value("", DistanceUnit.Meter, null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Value_WithWhitespaceText_ShouldReturnNull()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.Value("   ", DistanceUnit.Meter, null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Value_WithInvalidText_ShouldReturnNull()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.Value("abc", DistanceUnit.Meter, null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Value_WithNegativeNumber_ShouldReturnMeasurement()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            Minimum = -1000
        };

        // Act
        var result = controller.Value("-50", DistanceUnit.Meter, null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().NotBeNull();
        result.Value.Value.Should().Be(-50);
    }

    [Fact]
    public void Value_BelowMinimum_ShouldStillAccept()
    {
        // Arrange: Min/Max are only for increment/decrement, not validation
        var controller = new MeasurementController<DistanceUnit>
        {
            Minimum = 0
        };

        // Act: Negative values should be accepted
        var result = controller.Value("-10", DistanceUnit.Meter, null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be(-10);
        result.Value.Unit.Should().Be(DistanceUnit.Meter);
    }

    [Fact]
    public void Value_AboveMaximum_ShouldStillAccept()
    {
        // Arrange: Min/Max are only for increment/decrement, not validation
        var controller = new MeasurementController<DistanceUnit>
        {
            Maximum = 100
        };

        // Act: Values above Maximum should be accepted
        var result = controller.Value("150", DistanceUnit.Meter, null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be(150);
        result.Value.Unit.Should().Be(DistanceUnit.Meter);
    }

    [Fact]
    public void Value_WithThousandsSeparator_ShouldParse()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();
        var culture = new CultureInfo("en-US"); // Uses comma as thousands separator

        // Act
        var result = controller.Value("1,000", DistanceUnit.Meter, null, culture);

        // Assert
        result.Should().NotBeNull();
        result.Value.Value.Should().Be(1000);
    }

    [Fact]
    public void Value_WithEuropeanCulture_ShouldParseDecimalComma()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();
        var culture = new CultureInfo("de-DE"); // Uses comma as decimal separator

        // Act
        var result = controller.Value("100,5", DistanceUnit.Meter, null, culture);

        // Assert
        result.Should().NotBeNull();
        result.Value.Value.Should().Be(100.5);
    }

    #endregion

    #region ParseValue Tests

    [Fact]
    public void ParseValue_ShouldFormatWithDecimalPoints()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            DecimalPoints = 2
        };
        var measurement = new Measurement<DistanceUnit>(100.456, DistanceUnit.Meter);

        // Act
        controller.ParseValue(measurement, out string text, out DistanceUnit unit, 2, CultureInfo.InvariantCulture);

        // Assert
        text.Should().Be("100.46"); // Rounded to 2 decimal places
        unit.Should().Be(DistanceUnit.Meter);
    }

    [Fact]
    public void ParseValue_WithNullDecimalPoints_ShouldUseDefaultFormat()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            DecimalPoints = null
        };
        var measurement = new Measurement<DistanceUnit>(100, DistanceUnit.Meter);

        // Act
        controller.ParseValue(measurement, out string text, out DistanceUnit unit, null, CultureInfo.InvariantCulture);

        // Assert
        text.Should().NotBeEmpty();
        unit.Should().Be(DistanceUnit.Meter);
    }

    [Fact]
    public void ParseValue_WithThousandsSeparator_ShouldFormat()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            DecimalPoints = 0
        };
        var measurement = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter);
        var culture = new CultureInfo("en-US");

        // Act
        controller.ParseValue(measurement, out string text, out DistanceUnit unit, 0, culture);

        // Assert
        text.Should().Be("1,000"); // With thousands separator
        unit.Should().Be(DistanceUnit.Meter);
    }

    #endregion

    #region IncrementValue Tests

    [Fact]
    public void IncrementValue_ShouldIncreaseByIncrement()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            Increment = 10,
            DecimalPoints = 0
        };

        // Act
        var result = controller.IncrementValue("100", true, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("110");
    }

    [Fact]
    public void IncrementValue_ShouldDecreaseByIncrement()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            Increment = 10,
            DecimalPoints = 0
        };

        // Act
        var result = controller.IncrementValue("100", false, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("90");
    }

    [Fact]
    public void IncrementValue_AtMaximum_ShouldNotExceedMaximum()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            Increment = 10,
            Maximum = 100,
            DecimalPoints = 0
        };

        // Act
        var result = controller.IncrementValue("100", true, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("100"); // Clamped to maximum
    }

    [Fact]
    public void IncrementValue_AtMinimum_ShouldNotGoBelowMinimum()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            Increment = 10,
            Minimum = 0,
            DecimalPoints = 0
        };

        // Act
        var result = controller.IncrementValue("0", false, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("0"); // Clamped to minimum
    }

    [Fact]
    public void IncrementValue_WithInvalidText_ShouldStartFromZero()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>
        {
            Increment = 10,
            Minimum = 0,
            DecimalPoints = 0
        };

        // Act: First increment from empty/invalid starts from 0
        var result = controller.IncrementValue("invalid", true, CultureInfo.InvariantCulture);

        // Assert: Starts from Max(0, Minimum) - Increment, then adds Increment = 0
        result.Should().Be("0");
    }

    #endregion

    #region AllowKeyInEditor Tests

    [Fact]
    public void AllowKeyInEditor_WithDigit_ShouldReturnTrue()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("100", 3, 0, '5', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AllowKeyInEditor_WithLetter_ShouldReturnFalse()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("100", 3, 0, 'a', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AllowKeyInEditor_WithFirstDecimalSeparator_ShouldReturnTrue()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("100", 3, 0, '.', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AllowKeyInEditor_WithSecondDecimalSeparator_ShouldReturnFalse()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("100.5", 5, 0, '.', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AllowKeyInEditor_WithMinusAtStart_ShouldReturnTrue()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("100", 0, 0, '-', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AllowKeyInEditor_WithMinusNotAtStart_ShouldReturnFalse()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("100", 2, 0, '-', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AllowKeyInEditor_WithPlusAtStart_ShouldReturnTrue()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("100", 0, 0, '+', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AllowKeyInEditor_WithThousandsSeparator_ShouldReturnTrue()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("1000", 1, 0, ',', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AllowKeyInEditor_WithThousandsSeparatorAtStart_ShouldReturnFalse()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var result = controller.AllowKeyInEditor("1000", 0, 0, ',', CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AllowKeyInEditor_WithEuropeanDecimalComma_ShouldReturnTrue()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();
        var culture = new CultureInfo("de-DE"); // Uses comma as decimal separator

        // Act
        var result = controller.AllowKeyInEditor("100", 3, 0, ',', culture);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetUnitName Tests

    [Fact]
    public void GetUnitName_ForDistanceUnit_ShouldReturnCorrectName()
    {
        // Arrange
        var controller = new MeasurementController<DistanceUnit>();

        // Act
        var name = controller.GetUnitName(DistanceUnit.Meter);

        // Assert
        name.Should().Be("m");
    }

    [Fact]
    public void GetUnitName_ForVelocityUnit_ShouldReturnCorrectName()
    {
        // Arrange
        var controller = new MeasurementController<VelocityUnit>();

        // Act
        var name = controller.GetUnitName(VelocityUnit.MetersPerSecond);

        // Assert
        name.Should().Be("m/s");
    }

    #endregion
}
