using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditRectangleDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidRectangle_ControlsPopulatedFromElement()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = new ReticlePosition(1, 2, AngularUnit.Mil),
            Size = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red",
            Fill = true
        };

        // Act
        var dialog = new EditRectangleDialog(rect);

        // Assert
        dialog.TopLeftX.GetValue<AngularUnit>().Should().Be(rect.TopLeft.X);
        dialog.TopLeftY.GetValue<AngularUnit>().Should().Be(rect.TopLeft.Y);
        dialog.WidthControl.GetValue<AngularUnit>().Should().Be(rect.Size.X);
        dialog.HeightControl.GetValue<AngularUnit>().Should().Be(rect.Size.Y);
        dialog.LineWidthControl.GetValue<AngularUnit>().Should().Be(rect.LineWidth.Value);
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be(rect.Color);
        dialog.FillCheckBox.IsChecked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Constructor_RectangleWithNullLineWidth_LineWidthControlSetToZero()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = new ReticlePosition(1, 2, AngularUnit.Mil),
            Size = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = null
        };

        // Act
        var dialog = new EditRectangleDialog(rect);

        // Assert
        var lineWidth = dialog.LineWidthControl.GetValue<AngularUnit>();
        lineWidth.HasValue.Should().BeTrue();
        lineWidth!.Value.In(lineWidth.Value.Unit).Should().BeApproximately(0, 0.001);
    }

    [AvaloniaFact]
    public void Constructor_RectangleWithNullColor_ColorComboSetToBlack()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = new ReticlePosition(1, 2, AngularUnit.Mil),
            Size = new ReticlePosition(3, 4, AngularUnit.Mil),
            Color = null
        };

        // Act
        var dialog = new EditRectangleDialog(rect);

        // Assert
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be("black");
    }

    [AvaloniaFact]
    public void Constructor_RectangleWithNullFill_FillCheckBoxUnchecked()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = new ReticlePosition(1, 2, AngularUnit.Mil),
            Size = new ReticlePosition(3, 4, AngularUnit.Mil),
            Fill = null
        };

        // Act
        var dialog = new EditRectangleDialog(rect);

        // Assert
        dialog.FillCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = new ReticlePosition(1, 2, AngularUnit.Mil),
            Size = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red",
            Fill = false
        };
        var dialog = new EditRectangleDialog(rect);

        // Act - Modify controls
        dialog.TopLeftX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.TopLeftY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        dialog.WidthControl.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.HeightControl.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0.2, AngularUnit.Mil));
        dialog.ColorCombo.SelectedIndex = dialog.ColorCombo.Items.IndexOf("blue");
        dialog.FillCheckBox.IsChecked = true;

        // Assert - Element NOT changed yet
        rect.TopLeft.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        rect.TopLeft.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        rect.TopLeft.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        rect.Size.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        rect.Size.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        rect.LineWidth!.Value.Should().Be(new Measurement<AngularUnit>(0.2, AngularUnit.Mil));
        rect.Color.Should().Be("blue");
        rect.Fill.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Save_EmptyLineWidth_LineWidthSetToNull()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = new ReticlePosition(1, 2, AngularUnit.Mil),
            Size = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil)
        };
        var dialog = new EditRectangleDialog(rect);

        // Act
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        dialog.Save();

        // Assert
        rect.LineWidth.Should().BeNull();
    }

    [AvaloniaFact]
    public void Save_NullPositions_InitializesPositions()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = null,
            Size = null
        };

        // Act
        var dialog = new EditRectangleDialog(rect);
        dialog.TopLeftX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.TopLeftY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.WidthControl.SetValue(new Measurement<AngularUnit>(2, AngularUnit.Mil));
        dialog.HeightControl.SetValue(new Measurement<AngularUnit>(3, AngularUnit.Mil));
        dialog.Save();

        // Assert
        rect.TopLeft.Should().NotBeNull();
        rect.TopLeft.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        rect.TopLeft.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        rect.Size.Should().NotBeNull();
        rect.Size.X.Should().Be(new Measurement<AngularUnit>(2, AngularUnit.Mil));
        rect.Size.Y.Should().Be(new Measurement<AngularUnit>(3, AngularUnit.Mil));
    }
}
