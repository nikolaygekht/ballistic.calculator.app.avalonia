using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditLineDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidLine_ControlsPopulatedFromElement()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.MOA),
            LineWidth = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Color = "red"
        };

        // Act
        var dialog = new EditLineDialog(line);

        // Assert
        dialog.StartX.GetValue<AngularUnit>().Should().Be(line.Start.X);
        dialog.StartY.GetValue<AngularUnit>().Should().Be(line.Start.Y);
        dialog.EndX.GetValue<AngularUnit>().Should().Be(line.End.X);
        dialog.EndY.GetValue<AngularUnit>().Should().Be(line.End.Y);
        dialog.LineWidthControl.GetValue<AngularUnit>().Should().Be(line.LineWidth.Value);
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be(line.Color);
    }

    [AvaloniaFact]
    public void Constructor_LineWithNullLineWidth_LineWidthControlSetToZero()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = null,
            Color = "black"
        };

        // Act
        var dialog = new EditLineDialog(line);

        // Assert
        var lineWidth = dialog.LineWidthControl.GetValue<AngularUnit>();
        lineWidth.HasValue.Should().BeTrue();
        lineWidth!.Value.In(lineWidth.Value.Unit).Should().BeApproximately(0, 0.001);
    }

    [AvaloniaFact]
    public void Constructor_LineWithNullColor_ColorComboSetToBlack()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.Mil),
            Color = null
        };

        // Act
        var dialog = new EditLineDialog(line);

        // Assert
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be("black");
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Color = "red"
        };
        var dialog = new EditLineDialog(line);

        // Act - Modify controls
        dialog.StartX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.StartY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        dialog.EndX.SetValue(new Measurement<AngularUnit>(12, AngularUnit.MOA));
        dialog.EndY.SetValue(new Measurement<AngularUnit>(13, AngularUnit.MOA));
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        dialog.ColorCombo.SelectedIndex = dialog.ColorCombo.Items.IndexOf("blue");

        // Assert - Element NOT changed yet
        line.Start.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        line.Start.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        line.Start.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        line.End.X.Should().Be(new Measurement<AngularUnit>(12, AngularUnit.MOA));
        line.End.Y.Should().Be(new Measurement<AngularUnit>(13, AngularUnit.MOA));
        line.LineWidth.Value.Should().Be(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        line.Color.Should().Be("blue");
    }

    [AvaloniaFact]
    public void Save_EmptyLineWidth_LineWidthSetToNull()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Color = "red"
        };
        var dialog = new EditLineDialog(line);

        // Act
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        dialog.Save();

        // Assert
        line.LineWidth.Should().BeNull();
    }

    [AvaloniaFact]
    public void Save_NullStartPosition_InitializesPosition()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = null,
            End = new ReticlePosition(3, 4, AngularUnit.Mil)
        };

        // Act
        var dialog = new EditLineDialog(line);
        dialog.StartX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.StartY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.Save();

        // Assert
        line.Start.Should().NotBeNull();
        line.Start.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        line.Start.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
    }
}
