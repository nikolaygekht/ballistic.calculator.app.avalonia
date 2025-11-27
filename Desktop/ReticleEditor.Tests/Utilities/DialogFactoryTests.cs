using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using ReticleEditor.Views.Dialogs;
using Xunit;

namespace ReticleEditor.Tests.Utilities;

public class DialogFactoryTests
{
    [AvaloniaFact]
    public void CreateDialogForElement_ReticleLine_ReturnsEditLineDialog()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(0, 0, AngularUnit.Mil),
            End = new ReticlePosition(1, 1, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(line);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditLineDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_ReticleCircle_ReturnsEditCircleDialog()
    {
        // Arrange
        var circle = new ReticleCircle
        {
            Center = new ReticlePosition(0, 0, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(1, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(circle);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditCircleDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_ReticleRectangle_ReturnsEditRectangleDialog()
    {
        // Arrange
        var rect = new ReticleRectangle
        {
            TopLeft = new ReticlePosition(0, 0, AngularUnit.Mil),
            Size = new ReticlePosition(1, 1, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(rect);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditRectangleDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_ReticleText_ReturnsEditTextDialog()
    {
        // Arrange
        var text = new ReticleText
        {
            Position = new ReticlePosition(0, 0, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(1, AngularUnit.Mil),
            Text = "Test"
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(text);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditTextDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_ReticleBdcPoint_ReturnsEditBdcDialog()
    {
        // Arrange
        var bdc = new ReticleBulletDropCompensatorPoint
        {
            Position = new ReticlePosition(0, 0, AngularUnit.Mil),
            TextOffset = new Measurement<AngularUnit>(1, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(bdc);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditBdcDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_ReticlePath_ReturnsEditPathDialog()
    {
        // Arrange
        var path = new ReticlePath();

        // Act
        var dialog = DialogFactory.CreateDialogForElement(path);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditPathDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_ReticlePath_WithReticle_ReturnsEditPathDialog()
    {
        // Arrange
        var path = new ReticlePath();
        var reticle = new ReticleDefinition
        {
            Size = new ReticlePosition(10, 10, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(path, reticle);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditPathDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_MoveTo_ReturnsEditMoveToDialog()
    {
        // Arrange
        var moveTo = new ReticlePathElementMoveTo
        {
            Position = new ReticlePosition(0, 0, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(moveTo);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditMoveToDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_LineTo_ReturnsEditLineToDialog()
    {
        // Arrange
        var lineTo = new ReticlePathElementLineTo
        {
            Position = new ReticlePosition(0, 0, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(lineTo);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditLineToDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_Arc_ReturnsEditArcDialog()
    {
        // Arrange
        var arc = new ReticlePathElementArc
        {
            Position = new ReticlePosition(0, 0, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(1, AngularUnit.Mil)
        };

        // Act
        var dialog = DialogFactory.CreateDialogForElement(arc);

        // Assert
        dialog.Should().NotBeNull();
        dialog.Should().BeOfType<EditArcDialog>();
    }

    [AvaloniaFact]
    public void CreateDialogForElement_UnknownType_ReturnsNull()
    {
        // Arrange
        var unknownObject = new object();

        // Act
        var dialog = DialogFactory.CreateDialogForElement(unknownObject);

        // Assert
        dialog.Should().BeNull();
    }
}
