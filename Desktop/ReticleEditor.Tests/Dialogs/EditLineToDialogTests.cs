using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditLineToDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidLineTo_ControlsPopulatedFromElement()
    {
        // Arrange
        var lineTo = new ReticlePathElementLineTo
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil)
        };

        // Act
        var dialog = new EditLineToDialog(lineTo);

        // Assert
        dialog.PositionX.GetValue<AngularUnit>().Should().Be(lineTo.Position.X);
        dialog.PositionY.GetValue<AngularUnit>().Should().Be(lineTo.Position.Y);
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var lineTo = new ReticlePathElementLineTo
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil)
        };
        var dialog = new EditLineToDialog(lineTo);

        // Act - Modify controls
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));

        // Assert - Element NOT changed yet
        lineTo.Position.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        lineTo.Position.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        lineTo.Position.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
    }

    [AvaloniaFact]
    public void Save_NullPosition_InitializesPosition()
    {
        // Arrange
        var lineTo = new ReticlePathElementLineTo
        {
            Position = null
        };

        // Act
        var dialog = new EditLineToDialog(lineTo);
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.Save();

        // Assert
        lineTo.Position.Should().NotBeNull();
        lineTo.Position.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        lineTo.Position.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
    }
}
