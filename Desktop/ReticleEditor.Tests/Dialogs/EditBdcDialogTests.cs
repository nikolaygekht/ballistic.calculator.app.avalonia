using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditBdcDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidBdc_ControlsPopulatedFromElement()
    {
        // Arrange
        var bdc = new ReticleBulletDropCompensatorPoint
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextOffset = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.3, AngularUnit.Mil)
        };

        // Act
        var dialog = new EditBdcDialog(bdc);

        // Assert
        dialog.PositionX.GetValue<AngularUnit>().Should().Be(bdc.Position.X);
        dialog.PositionY.GetValue<AngularUnit>().Should().Be(bdc.Position.Y);
        dialog.TextOffsetControl.GetValue<AngularUnit>().Should().Be(bdc.TextOffset);
        dialog.TextHeightControl.GetValue<AngularUnit>().Should().Be(bdc.TextHeight);
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var bdc = new ReticleBulletDropCompensatorPoint
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextOffset = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.3, AngularUnit.Mil)
        };
        var dialog = new EditBdcDialog(bdc);

        // Act - Modify controls
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        dialog.TextOffsetControl.SetValue(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        dialog.TextHeightControl.SetValue(new Measurement<AngularUnit>(0.5, AngularUnit.Mil));

        // Assert - Element NOT changed yet
        bdc.Position.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        bdc.Position.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        bdc.Position.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        bdc.TextOffset.Should().Be(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        bdc.TextHeight.Should().Be(new Measurement<AngularUnit>(0.5, AngularUnit.Mil));
    }

    [AvaloniaFact]
    public void Save_NullPosition_InitializesPosition()
    {
        // Arrange
        var bdc = new ReticleBulletDropCompensatorPoint
        {
            Position = null,
            TextOffset = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.3, AngularUnit.Mil)
        };

        // Act
        var dialog = new EditBdcDialog(bdc);
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.Save();

        // Assert
        bdc.Position.Should().NotBeNull();
        bdc.Position.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        bdc.Position.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
    }
}
