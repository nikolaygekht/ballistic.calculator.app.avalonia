using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Controls.Models;
using System.Globalization;
using Xunit;
using AwesomeAssertions;

namespace BallisticCalculator.Controls.Tests.Controllers;

public class BallisticCoefficientControllerTests
{
    #region DragTable Enumeration Tests

    [Fact]
    public void GetDragTables_ShouldReturnTablesInCorrectOrder()
    {
        // Arrange
        var controller = new BallisticCoefficientController();

        // Act
        var tables = controller.GetDragTables(out int defaultIndex);

        // Assert - Expected order: Alphabetical (G1, G2, G5, G6, G7, G8, GI, GS, RA4) + GC last
        tables[0].Value.Should().Be(DragTableId.G1);
        tables[1].Value.Should().Be(DragTableId.G2);
        tables[2].Value.Should().Be(DragTableId.G5);
        tables[3].Value.Should().Be(DragTableId.G6);
        tables[4].Value.Should().Be(DragTableId.G7);
        tables[5].Value.Should().Be(DragTableId.G8);
        tables[6].Value.Should().Be(DragTableId.GI);
        tables[7].Value.Should().Be(DragTableId.GS);
        tables[8].Value.Should().Be(DragTableId.RA4);
        tables[9].Value.Should().Be(DragTableId.GC);
    }

    [Fact]
    public void GetDragTables_ShouldReturnAllEnumValues()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var allDragTableValues = Enum.GetValues<DragTableId>().ToList();

        // Act
        var tables = controller.GetDragTables(out int defaultIndex);

        // Assert - Should have all enum values
        tables.Should().HaveCount(allDragTableValues.Count);

        // Verify each enum value is present
        foreach (var enumValue in allDragTableValues)
        {
            tables.Should().Contain(t => t.Value == enumValue,
                $"DragTableId.{enumValue} should be in the returned tables");
        }
    }

    [Fact]
    public void GetDragTables_ShouldSetG1AsDefault()
    {
        // Arrange
        var controller = new BallisticCoefficientController();

        // Act
        var tables = controller.GetDragTables(out int defaultIndex);

        // Assert
        defaultIndex.Should().Be(0);
        tables[defaultIndex].Value.Should().Be(DragTableId.G1);
    }

    [Fact]
    public void GetDragTables_ShouldHaveNames()
    {
        // Arrange
        var controller = new BallisticCoefficientController();

        // Act
        var tables = controller.GetDragTables(out int defaultIndex);

        // Assert
        tables[0].Name.Should().Be("G1");
        tables[4].Name.Should().Be("G7");
        tables[8].Name.Should().Be("RA4");  // RA4 is now at index 8 (alphabetically)
        tables.Should().AllSatisfy(t => t.Name.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GetDragTables_ShouldEndWithGC()
    {
        // Arrange
        var controller = new BallisticCoefficientController();

        // Act
        var tables = controller.GetDragTables(out int defaultIndex);

        // Assert - GC (custom) should be last
        tables[^1].Value.Should().Be(DragTableId.GC);
    }

    #endregion

    #region Value Parsing Tests (Text → BallisticCoefficient)

    [Fact]
    public void Value_ShouldParseSimpleCoefficient()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("0.450", table, 3);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().BeApproximately(0.450, 0.00001);
        result.Value.Table.Should().Be(DragTableId.G1);
    }

    [Fact]
    public void Value_ShouldHandleDifferentDragTables()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G7, "G7");

        // Act
        var result = controller.Value("0.275", table, 3);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().BeApproximately(0.275, 0.00001);
        result.Value.Table.Should().Be(DragTableId.G7);
    }

    [Fact]
    public void Value_ShouldHandleNullDecimalPoints()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("0.45678", table, null);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().BeApproximately(0.45678, 0.000001);
    }

    [Fact]
    public void Value_ShouldUseInvariantCulture()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // Act
            var result = controller.Value("0.450", table, 3);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Value.Should().BeApproximately(0.450, 0.00001);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void Value_ShouldHandleCommaDecimalSeparator()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");
        var culture = new CultureInfo("de-DE");

        // Act
        var result = controller.Value("0,450", table, 3, culture);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().BeApproximately(0.450, 0.00001);
    }

    [Fact]
    public void Value_ShouldHandleMinimumValue()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("0.001", table, 3);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().BeApproximately(0.001, 0.00001);
    }

    [Fact]
    public void Value_ShouldHandleMaximumValue()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("2.000", table, 3);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().BeApproximately(2.000, 0.00001);
    }

    [Fact]
    public void Value_ShouldReturnNullOnInvalidText_Empty()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("", table, 3);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Value_ShouldReturnNullOnInvalidText_NonNumeric()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("abc", table, 3);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Value_ShouldReturnNullOnInvalidText_Negative()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("-0.5", table, 3);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Value_ShouldReturnNullOnInvalidText_Zero()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act
        var result = controller.Value("0", table, 3);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Value_ShouldAcceptAnyPositiveValue()
    {
        // Arrange: Min/Max are only for increment/decrement, not validation
        var controller = new BallisticCoefficientController();
        var table = new DragTableInfo(DragTableId.G1, "G1");

        // Act: Value above Maximum should still be accepted
        var result = controller.Value("10.0", table, 3);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Value.Should().Be(10.0);
        result.Value.Table.Should().Be(DragTableId.G1);
    }

    #endregion

    #region Value Formatting Tests (BallisticCoefficient → Text)

    [Fact]
    public void ParseValue_ShouldExtractValueAndTable()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var bc = new BallisticCoefficient(0.450, DragTableId.G1);

        // Act
        controller.ParseValue(bc, out string text, out DragTableInfo table);

        // Assert
        text.Should().Be("0.450");
        table.Value.Should().Be(DragTableId.G1);
    }

    [Fact]
    public void ParseValue_ShouldHandleDifferentTables()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var bc = new BallisticCoefficient(0.275, DragTableId.G7);

        // Act
        controller.ParseValue(bc, out string text, out DragTableInfo table);

        // Assert
        text.Should().Be("0.275");
        table.Value.Should().Be(DragTableId.G7);
    }

    [Fact]
    public void ParseValue_ShouldRespectDecimalPoints()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var bc = new BallisticCoefficient(0.45678, DragTableId.G1);

        // Act
        controller.ParseValue(bc, out string text, out DragTableInfo table, 3);

        // Assert
        // Should be rounded/truncated to 3 decimal places
        text.Should().BeOneOf("0.457", "0.456"); // Allow for rounding or truncation
        table.Value.Should().Be(DragTableId.G1);
    }

    [Fact]
    public void ParseValue_WithInvariantCulture_ShouldUsePeriod()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var bc = new BallisticCoefficient(0.450, DragTableId.G1);

        // Act
        controller.ParseValue(bc, out string text, out DragTableInfo table);

        // Assert
        text.Should().Contain("."); // Period as decimal separator
        text.Should().NotContain(",");
    }

    [Fact]
    public void ParseValue_WithGermanCulture_ShouldUseComma()
    {
        // Arrange
        var controller = new BallisticCoefficientController();
        var bc = new BallisticCoefficient(0.450, DragTableId.G1);
        var culture = new CultureInfo("de-DE");

        // Act
        controller.ParseValue(bc, out string text, out DragTableInfo table, culture: culture);

        // Assert
        text.Should().Contain(","); // Comma as decimal separator for German
        text.Should().NotContain(".");
    }

    #endregion
}
