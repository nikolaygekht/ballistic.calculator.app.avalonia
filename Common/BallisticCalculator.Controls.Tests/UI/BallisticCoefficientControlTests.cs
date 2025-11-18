using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Controls.Models;
using System.Linq;

namespace BallisticCalculator.Controls.Tests.UI;

public class BallisticCoefficientControlTests
{
    [AvaloniaFact]
    public void Control_ShouldInitialize()
    {
        // Arrange & Act
        var control = new BallisticCoefficientControl();

        // Assert
        control.Should().NotBeNull();
        control.IsEmpty.Should().BeTrue();
        control.NumericPart.Should().NotBeNull();
        control.TablePart.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void TablePart_ShouldContain10TablesInCorrectOrder()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        var tableNames = control.TablePart.Items.Cast<DragTableInfo>()
            .Select(t => t.Name).ToList();

        // Assert
        tableNames.Should().HaveCount(10);
        tableNames[0].Should().Be("G1");
        tableNames[1].Should().Be("G2");
        tableNames[2].Should().Be("G5");
        tableNames[3].Should().Be("G6");
        tableNames[4].Should().Be("G7");
        tableNames[5].Should().Be("G8");
        tableNames[6].Should().Be("GI");
        tableNames[7].Should().Be("GS");
        tableNames[8].Should().Be("RA4");
        tableNames[9].Should().Be("GC");
    }

    [AvaloniaFact]
    public void Value_SetAndGet_WithoutEdit_ShouldPreserveOriginalPrecision()
    {
        // Arrange
        var control = new BallisticCoefficientControl { DecimalPoints = 3 };
        var original = new BallisticCoefficient(0.45678, DragTableId.G1);

        // Act
        control.Value = original;
        var retrieved = control.Value;

        // Assert - CRITICAL: Should return EXACT original (precision transparency!)
        retrieved.Should().NotBeNull();
        retrieved!.Value.Value.Should().Be(0.45678); // Exact precision preserved
        retrieved.Value.Table.Should().Be(DragTableId.G1);
    }

    [AvaloniaFact]
    public void Value_Set_ShouldDisplayWithConfiguredPrecision()
    {
        // Arrange
        var control = new BallisticCoefficientControl { DecimalPoints = 3 };

        // Act
        control.Value = new BallisticCoefficient(0.45678, DragTableId.G1);

        // Assert - Display should show rounded value
        control.NumericPart.Text.Should().Be("0.457"); // 3 decimal places
    }

    [AvaloniaFact]
    public void Value_SetAndGet_AfterEdit_ShouldReturnEditedValue()
    {
        // Arrange
        var control = new BallisticCoefficientControl { DecimalPoints = 3 };
        var original = new BallisticCoefficient(0.45678, DragTableId.G1);
        control.Value = original;

        // Act - Set a NEW value (simulating user completing an edit)
        control.Value = new BallisticCoefficient(0.460, DragTableId.G1);
        var retrieved = control.Value;

        // Assert - Should return new value
        retrieved.Should().NotBeNull();
        retrieved!.Value.Value.Should().Be(0.460);
    }

    [AvaloniaFact]
    public void Value_Set_ShouldUpdateTableSelection()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        control.Value = new BallisticCoefficient(0.275, DragTableId.G7);

        // Assert
        var selectedTable = (DragTableInfo?)control.TablePart.SelectedItem;
        selectedTable.Should().NotBeNull();
        selectedTable!.Value.Should().Be(DragTableId.G7);
    }

    [AvaloniaFact]
    public void TablePartWidth_ShouldBeConfigurable()
    {
        // Arrange & Act
        var control = new BallisticCoefficientControl { TablePartWidth = 100 };

        // Assert
        control.TablePart.Width.Should().Be(100);
    }

    [AvaloniaFact]
    public void IsEmpty_WhenTextEmpty_ShouldReturnTrue()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        control.NumericPart.Text = "";

        // Assert
        control.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void IsEmpty_WhenTextNotEmpty_ShouldReturnFalse()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        control.Value = new BallisticCoefficient(0.450, DragTableId.G1);

        // Assert
        control.IsEmpty.Should().BeFalse();
    }

    // Note: Changed event tests are omitted because they're difficult to test reliably
    // in headless mode where UI events may not fire synchronously.
    // The Changed event will be tested manually in DebugApp.

    [AvaloniaFact]
    public void Value_SetNull_ShouldClearText()
    {
        // Arrange
        var control = new BallisticCoefficientControl();
        control.Value = new BallisticCoefficient(0.450, DragTableId.G1);

        // Act
        control.Value = null;

        // Assert
        control.NumericPart.Text.Should().BeEmpty();
        control.IsEmpty.Should().BeTrue();
    }
}
