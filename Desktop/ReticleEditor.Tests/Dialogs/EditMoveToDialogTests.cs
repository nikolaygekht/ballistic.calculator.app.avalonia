using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditMoveToDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidMoveTo_ControlsPopulatedFromElement()
    {
        // Arrange
        var moveTo = new ReticlePathElementMoveTo
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil)
        };

        // Act
        var dialog = new EditMoveToDialog(moveTo);

        // Assert
        dialog.PositionX.GetValue<AngularUnit>().Should().Be(moveTo.Position.X);
        dialog.PositionY.GetValue<AngularUnit>().Should().Be(moveTo.Position.Y);
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var moveTo = new ReticlePathElementMoveTo
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil)
        };
        var dialog = new EditMoveToDialog(moveTo);

        // Act - Modify controls
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));

        // Assert - Element NOT changed yet
        moveTo.Position.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        moveTo.Position.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        moveTo.Position.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
    }

    [AvaloniaFact]
    public void Save_NullPosition_InitializesPosition()
    {
        // Arrange
        var moveTo = new ReticlePathElementMoveTo
        {
            Position = null
        };

        // Act
        var dialog = new EditMoveToDialog(moveTo);
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.Save();

        // Assert
        moveTo.Position.Should().NotBeNull();
        moveTo.Position.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        moveTo.Position.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
    }
}
