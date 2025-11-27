using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditTextDialogTests
{
    [AvaloniaFact]
    public void Constructor_ValidText_ControlsPopulatedFromElement()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Text = "Hello",
            Color = "red",
            Anchor = TextAnchor.Center
        };

        // Act
        var dialog = new EditTextDialog(text);

        // Assert
        dialog.PositionX.GetValue<AngularUnit>().Should().Be(text.Position.X);
        dialog.PositionY.GetValue<AngularUnit>().Should().Be(text.Position.Y);
        dialog.TextHeightControl.GetValue<AngularUnit>().Should().Be(text.TextHeight);
        dialog.TextContent.Text.Should().Be(text.Text);
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be(text.Color);
        dialog.AnchorCombo.SelectedItem.Should().Be(TextAnchor.Center);
    }

    [AvaloniaFact]
    public void Constructor_TextWithNullColor_ColorComboSetToBlack()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Text = "Test",
            Color = null
        };

        // Act
        var dialog = new EditTextDialog(text);

        // Assert
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be("black");
    }

    [AvaloniaFact]
    public void Constructor_TextWithNullAnchor_AnchorComboSetToLeft()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Text = "Test",
            Anchor = null
        };

        // Act
        var dialog = new EditTextDialog(text);

        // Assert
        dialog.AnchorCombo.SelectedItem.Should().Be(TextAnchor.Left);
    }

    [AvaloniaFact]
    public void Constructor_TextWithNullText_TextContentEmpty()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Text = null
        };

        // Act
        var dialog = new EditTextDialog(text);

        // Assert
        dialog.TextContent.Text.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Text = "Original",
            Color = "red",
            Anchor = TextAnchor.Left
        };
        var dialog = new EditTextDialog(text);

        // Act - Modify controls
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        dialog.TextHeightControl.SetValue(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        dialog.TextContent.Text = "Modified";
        dialog.ColorCombo.SelectedIndex = dialog.ColorCombo.Items.IndexOf("blue");
        dialog.AnchorCombo.SelectedItem = TextAnchor.Right;

        // Assert - Element NOT changed yet
        text.Position.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        text.Position.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        text.Position.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        text.TextHeight.Should().Be(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        text.Text.Should().Be("Modified");
        text.Color.Should().Be("blue");
        text.Anchor.Should().Be(TextAnchor.Right);
    }

    [AvaloniaFact]
    public void Save_NullPosition_InitializesPosition()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = null,
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Text = "Test"
        };

        // Act
        var dialog = new EditTextDialog(text);
        dialog.PositionX.SetValue(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        dialog.PositionY.SetValue(new Measurement<AngularUnit>(6, AngularUnit.Mil));
        dialog.Save();

        // Assert
        text.Position.Should().NotBeNull();
        text.Position.X.Should().Be(new Measurement<AngularUnit>(5, AngularUnit.Mil));
        text.Position.Y.Should().Be(new Measurement<AngularUnit>(6, AngularUnit.Mil));
    }

    [AvaloniaFact]
    public void Save_AnchorCenter_AnchorSetToCenter()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = new ReticlePosition(1, 2, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Text = "Test",
            Anchor = TextAnchor.Left
        };
        var dialog = new EditTextDialog(text);

        // Act
        dialog.AnchorCombo.SelectedItem = TextAnchor.Center;
        dialog.Save();

        // Assert
        text.Anchor.Should().Be(TextAnchor.Center);
    }
}
