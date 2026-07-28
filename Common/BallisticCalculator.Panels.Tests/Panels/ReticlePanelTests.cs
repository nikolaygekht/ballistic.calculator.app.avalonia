using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using AwesomeAssertions;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Serialization;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace BallisticCalculator.Panels.Tests.Panels;

public class ReticlePanelTests
{
    #region The shipped Mil-Dot reticle is in milliradians

    /// <summary>
    /// A mil-dot reticle is a <b>milliradian</b> instrument: one dot spacing is 1 mrad. Written in
    /// <see cref="AngularUnit.Mil"/> — the military mil, 1/6400 of a circle — every subtension would be about
    /// 1.9 % small, which is a hold error at any distance worth holding for.
    /// </summary>
    [Fact]
    public void ShippedMilDotFile_UsesMilliradiansThroughout()
    {
        // Arrange & Act
        var reticle = LoadShippedMilDot();

        // Assert — the canvas and the aiming point
        reticle.Size!.X.Unit.Should().Be(AngularUnit.MRad);
        reticle.Size.Y.Unit.Should().Be(AngularUnit.MRad);
        reticle.Zero!.X.Unit.Should().Be(AngularUnit.MRad);
        reticle.Zero.Y.Unit.Should().Be(AngularUnit.MRad);

        // Assert — and every BDC mark, which is what a reader holds with
        reticle.BulletDropCompensator.Should().NotBeEmpty();
        foreach (var bdc in reticle.BulletDropCompensator)
        {
            bdc.Position.X.Unit.Should().Be(AngularUnit.MRad);
            bdc.Position.Y.Unit.Should().Be(AngularUnit.MRad);
        }
    }

    /// <summary>The dots are where a mil-dot reticle's dots are: whole milliradians from the centre.</summary>
    [Fact]
    public void ShippedMilDotFile_HasItsDotsOnWholeMilliradians()
    {
        var reticle = LoadShippedMilDot();

        reticle.Size!.X.In(AngularUnit.MRad).Should().BeApproximately(12, 1e-9);
        reticle.Zero!.X.In(AngularUnit.MRad).Should().BeApproximately(6, 1e-9);

        var marks = reticle.BulletDropCompensator
            .Select(b => b.Position.Y.In(AngularUnit.MRad))
            .ToArray();

        marks.Should().Contain(m => Math.Abs(m - -1) < 1e-9, "the first drop mark is 1 mrad below centre");
        marks.Should().AllSatisfy(m => Math.Abs(m - Math.Round(m)).Should().BeLessThan(1e-9,
            "mil-dot marks sit on whole milliradians"));
    }

    #endregion

    #region The Mil-Dot button reads that file

    /// <summary>
    /// The button loads the shipped file rather than the library's <c>MilDotReticle</c> object, which is built
    /// in military mils for the same reason the file used to be.
    /// </summary>
    [AvaloniaFact]
    public void MilDotButton_LoadsTheShippedFile()
    {
        // Arrange
        var panel = new ReticlePanel();
        panel.Reticle.Should().BeNull("nothing is loaded until asked");

        // Act
        panel.MilDotButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Assert — it is the file: its name, its units, its BDC marks
        panel.Reticle.Should().NotBeNull();
        panel.Reticle!.Name.Should().Be("Mil-Dot Reticle");
        panel.Reticle.Size!.X.Unit.Should().Be(AngularUnit.MRad);
        panel.Reticle.BulletDropCompensator.Should().NotBeEmpty();
        panel.ReticleNameText.Text.Should().Contain("Mil-Dot");
    }

    #endregion

    private static ReticleDefinition LoadShippedMilDot()
    {
        var path = Path.Combine(DataFolders.Reticles, "mildot.reticle");
        File.Exists(path).Should().BeTrue($"the shipped reticles are copied beside the tests ({path})");

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return stream.BallisticXmlDeserialize<ReticleDefinition>()!;
    }
}
