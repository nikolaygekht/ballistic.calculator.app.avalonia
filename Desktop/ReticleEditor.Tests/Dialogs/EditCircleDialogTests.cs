using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditCircleDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidCircle_ControlsPopulatedFromElement()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red",
            Fill = true
        };

        // Act
        var dialog = new EditCircleDialog(circle);

        // Assert
        dialog.CenterX.GetValue<AngularUnit>().Should().Be(circle.Center.X);
        dialog.CenterY.GetValue<AngularUnit>().Should().Be(circle.Center.Y);
        dialog.RadiusControl.GetValue<AngularUnit>().Should().Be(circle.Radius);
        dialog.LineWidthControl.GetValue<AngularUnit>().Should().Be(circle.LineWidth.Value);
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be(circle.Color);
        dialog.FillCheckBox.IsChecked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Constructor_CircleWithNullLineWidth_LineWidthControlSetToZero()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            LineWidth = null,
            Color = "black"
        };

        // Act
        var dialog = new EditCircleDialog(circle);

        // Assert
        var lineWidth = dialog.LineWidthControl.GetValue<AngularUnit>();
        lineWidth.HasValue.Should().BeTrue();
        lineWidth!.Value.In(lineWidth.Value.Unit).Should().BeApproximately(0, 0.001);
    }

    [AvaloniaFact]
    public void Constructor_CircleWithNullColor_ColorComboSetToBlack()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Color = null
        };

        // Act
        var dialog = new EditCircleDialog(circle);

        // Assert
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be("black");
    }

    [AvaloniaFact]
    public void Constructor_CircleWithNullFill_FillCheckBoxUnchecked()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Fill = null
        };

        // Act
        var dialog = new EditCircleDialog(circle);

        // Assert
        dialog.FillCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Constructor_CircleWithFillFalse_FillCheckBoxUnchecked()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Fill = false
        };

        // Act
        var dialog = new EditCircleDialog(circle);

        // Assert
        dialog.FillCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red",
            Fill = false
        };
        var dialog = new EditCircleDialog(circle);

        // Act - Modify controls
        dialog.CenterX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.CenterY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        dialog.RadiusControl.SetValue(new Measurement<AngularUnit>(2, AngularUnit.Mil));
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0.2, AngularUnit.Mil));
        dialog.ColorCombo.SelectedIndex = dialog.ColorCombo.Items.IndexOf("blue");
        dialog.FillCheckBox.IsChecked = true;

        // Assert - Element NOT changed yet
        circle.Center.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        circle.Center.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        circle.Center.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        circle.Radius.Should().Be(new Measurement<AngularUnit>(2, AngularUnit.Mil));
        circle.LineWidth!.Value.Should().Be(new Measurement<AngularUnit>(0.2, AngularUnit.Mil));
        circle.Color.Should().Be("blue");
        circle.Fill.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Save_EmptyLineWidth_LineWidthSetToNull()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red"
        };
        var dialog = new EditCircleDialog(circle);

        // Act
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        dialog.Save();

        // Assert
        circle.LineWidth.Should().BeNull();
    }

    [AvaloniaFact]
    public void Save_FillUnchecked_FillSetToFalse()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(1, 2, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Fill = true
        };
        var dialog = new EditCircleDialog(circle);

        // Act
        dialog.FillCheckBox.IsChecked = false;
        dialog.Save();

        // Assert
        circle.Fill.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Save_NullCenterPosition_InitializesPosition()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = null,
            Radius = new Measurement<AngularUnit>(0.5, AngularUnit.Mil)
        };

        // Act
        var dialog = new EditCircleDialog(circle);
        dialog.CenterX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.CenterY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.Save();

        // Assert
        circle.Center.Should().NotBeNull();
        circle.Center.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        circle.Center.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
    }
}
