using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditArcDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidArc_ControlsPopulatedFromElement()
    {
        // Arrange
        var arc = new ReticlePathElementArc
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            ClockwiseDirection = true,
            MajorArc = true
        };

        // Act
        var dialog = new EditArcDialog(arc);

        // Assert
        dialog.PositionX.GetValue<AngularUnit>().Should().Be(arc.Position.X);
        dialog.PositionY.GetValue<AngularUnit>().Should().Be(arc.Position.Y);
        dialog.RadiusControl.GetValue<AngularUnit>().Should().Be(arc.Radius);
        dialog.ClockwiseCheckBox.IsChecked.Should().BeTrue();
        dialog.MajorArcCheckBox.IsChecked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Constructor_ArcWithFalseFlags_CheckboxesUnchecked()
    {
        // Arrange
        var arc = new ReticlePathElementArc
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            ClockwiseDirection = false,
            MajorArc = false
        };

        // Act
        var dialog = new EditArcDialog(arc);

        // Assert
        dialog.ClockwiseCheckBox.IsChecked.Should().BeFalse();
        dialog.MajorArcCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var arc = new ReticlePathElementArc
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            ClockwiseDirection = false,
            MajorArc = false
        };
        var dialog = new EditArcDialog(arc);

        // Act - Modify controls
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        dialog.RadiusControl.SetValue(new Measurement<AngularUnit>(2, AngularUnit.Mil));
        dialog.ClockwiseCheckBox.IsChecked = true;
        dialog.MajorArcCheckBox.IsChecked = true;

        // Assert - Element NOT changed yet
        arc.Position.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        arc.Position.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        arc.Position.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        arc.Radius.Should().Be(new Measurement<AngularUnit>(2, AngularUnit.Mil));
        arc.ClockwiseDirection.Should().BeTrue();
        arc.MajorArc.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Save_UncheckBothFlags_FlagsSetToFalse()
    {
        // Arrange
        var arc = new ReticlePathElementArc
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            ClockwiseDirection = true,
            MajorArc = true
        };
        var dialog = new EditArcDialog(arc);

        // Act
        dialog.ClockwiseCheckBox.IsChecked = false;
        dialog.MajorArcCheckBox.IsChecked = false;
        dialog.Save();

        // Assert
        arc.ClockwiseDirection.Should().BeFalse();
        arc.MajorArc.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Save_NullPosition_InitializesPosition()
    {
        // Arrange
        var arc = new ReticlePathElementArc
        {
            Position = null,
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil)
        };

        // Act
        var dialog = new EditArcDialog(arc);
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.Save();

        // Assert
        arc.Position.Should().NotBeNull();
        arc.Position.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        arc.Position.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
    }
}
