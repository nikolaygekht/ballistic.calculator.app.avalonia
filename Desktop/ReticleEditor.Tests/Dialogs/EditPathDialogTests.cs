using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Dialogs;

public class EditPathDialogTests
{
    private static ReticleDefinition CreateTestReticle()
    {
        return new ReticleDefinition
        {
            Size = new ReticlePosition(10, 10, AngularUnit.Mil),
            Zero = new ReticlePosition(5, 5, AngularUnit.Mil)
        };
    }

    [AvaloniaFact]
    public void Constructor_ValidPath_ControlsPopulatedFromElement()
    {
        // Arrange
        var path = new ReticlePath
        {
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red",
            Fill = true
        };
        path.Elements.Add(new ReticlePathElementMoveTo { Position = new ReticlePosition(0, 0, AngularUnit.Mil) });
        path.Elements.Add(new ReticlePathElementLineTo { Position = new ReticlePosition(1, 1, AngularUnit.Mil) });

        // Act
        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Assert
        dialog.LineWidthControl.GetValue<AngularUnit>().Should().Be(path.LineWidth);
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be(path.Color);
        dialog.FillCheckBox.IsChecked.Should().BeTrue();
        dialog.ElementsList.ItemCount.Should().Be(2);
    }

    [AvaloniaFact]
    public void Constructor_PathWithNullLineWidth_LineWidthControlSetToZero()
    {
        // Arrange
        var path = new ReticlePath
        {
            LineWidth = null,
            Color = "black"
        };

        // Act
        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Assert
        var lineWidth = dialog.LineWidthControl.GetValue<AngularUnit>();
        lineWidth.HasValue.Should().BeTrue();
        lineWidth!.Value.In(lineWidth.Value.Unit).Should().BeApproximately(0, 0.001);
    }

    [AvaloniaFact]
    public void Constructor_PathWithNullColor_ColorComboSetToBlack()
    {
        // Arrange
        var path = new ReticlePath
        {
            Color = null
        };

        // Act
        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Assert
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be("black");
    }

    [AvaloniaFact]
    public void Constructor_PathWithNullFill_FillCheckBoxUnchecked()
    {
        // Arrange
        var path = new ReticlePath
        {
            Fill = null
        };

        // Act
        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Assert
        dialog.FillCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var path = new ReticlePath
        {
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red",
            Fill = false
        };
        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Act - Modify controls
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0.2, AngularUnit.Mil));
        dialog.ColorCombo.SelectedIndex = dialog.ColorCombo.Items.IndexOf("blue");
        dialog.FillCheckBox.IsChecked = true;

        // Assert - Element NOT changed yet
        path.LineWidth.Should().NotBe(new Measurement<AngularUnit>(0.2, AngularUnit.Mil));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        path.LineWidth.Should().Be(new Measurement<AngularUnit>(0.2, AngularUnit.Mil));
        path.Color.Should().Be("blue");
        path.Fill.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Save_EmptyLineWidth_LineWidthSetToNull()
    {
        // Arrange
        var path = new ReticlePath
        {
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red"
        };
        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Act
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        dialog.Save();

        // Assert
        path.LineWidth.Should().BeNull();
    }

    [AvaloniaFact]
    public void AddMoveTo_AddsElementToPathAndList()
    {
        // Arrange
        var path = new ReticlePath();
        var dialog = new EditPathDialog(path, CreateTestReticle());
        var initialCount = path.Elements.Count;

        // Act
        dialog.AddMoveTo();

        // Assert
        path.Elements.Count.Should().Be(initialCount + 1);
        path.Elements[initialCount].Should().BeOfType<ReticlePathElementMoveTo>();
        dialog.ElementsList.ItemCount.Should().Be(initialCount + 1);
    }

    [AvaloniaFact]
    public void AddLineTo_AddsElementToPathAndList()
    {
        // Arrange
        var path = new ReticlePath();
        var dialog = new EditPathDialog(path, CreateTestReticle());
        var initialCount = path.Elements.Count;

        // Act
        dialog.AddLineTo();

        // Assert
        path.Elements.Count.Should().Be(initialCount + 1);
        path.Elements[initialCount].Should().BeOfType<ReticlePathElementLineTo>();
        dialog.ElementsList.ItemCount.Should().Be(initialCount + 1);
    }

    [AvaloniaFact]
    public void AddArc_AddsElementToPathAndList()
    {
        // Arrange
        var path = new ReticlePath();
        var dialog = new EditPathDialog(path, CreateTestReticle());
        var initialCount = path.Elements.Count;

        // Act
        dialog.AddArc();

        // Assert
        path.Elements.Count.Should().Be(initialCount + 1);
        path.Elements[initialCount].Should().BeOfType<ReticlePathElementArc>();
        dialog.ElementsList.ItemCount.Should().Be(initialCount + 1);
    }

    [AvaloniaFact]
    public void DeleteSelectedElement_RemovesElementFromPathAndList()
    {
        // Arrange
        var path = new ReticlePath();
        path.Elements.Add(new ReticlePathElementMoveTo { Position = new ReticlePosition(0, 0, AngularUnit.Mil) });
        path.Elements.Add(new ReticlePathElementLineTo { Position = new ReticlePosition(1, 1, AngularUnit.Mil) });
        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Select first element
        dialog.ElementsList.SelectedIndex = 0;

        // Act
        dialog.DeleteSelectedElement();

        // Assert
        path.Elements.Count.Should().Be(1);
        path.Elements[0].Should().BeOfType<ReticlePathElementLineTo>();
        dialog.ElementsList.ItemCount.Should().Be(1);
    }

    [AvaloniaFact]
    public void DeleteSelectedElement_NothingSelected_DoesNothing()
    {
        // Arrange
        var path = new ReticlePath();
        path.Elements.Add(new ReticlePathElementMoveTo { Position = new ReticlePosition(0, 0, AngularUnit.Mil) });
        var dialog = new EditPathDialog(path, CreateTestReticle());
        dialog.ElementsList.SelectedIndex = -1;

        // Act
        dialog.DeleteSelectedElement();

        // Assert
        path.Elements.Count.Should().Be(1);
    }

    [AvaloniaFact]
    public void Revert_RestoresOriginalValues()
    {
        // Arrange
        var path = new ReticlePath
        {
            LineWidth = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            Color = "red",
            Fill = false
        };
        path.Elements.Add(new ReticlePathElementMoveTo { Position = new ReticlePosition(0, 0, AngularUnit.Mil) });

        var dialog = new EditPathDialog(path, CreateTestReticle());

        // Modify and save
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0.5, AngularUnit.Mil));
        dialog.ColorCombo.SelectedIndex = dialog.ColorCombo.Items.IndexOf("blue");
        dialog.FillCheckBox.IsChecked = true;
        dialog.Save();
        dialog.AddLineTo();

        // Verify modifications
        path.LineWidth.Should().Be(new Measurement<AngularUnit>(0.5, AngularUnit.Mil));
        path.Color.Should().Be("blue");
        path.Fill.Should().BeTrue();
        path.Elements.Count.Should().Be(2);

        // Act
        dialog.Revert();

        // Assert - Original values restored
        path.LineWidth.Should().Be(new Measurement<AngularUnit>(0.1, AngularUnit.Mil));
        path.Color.Should().Be("red");
        path.Fill.Should().BeFalse();
        path.Elements.Count.Should().Be(1);
    }
}
