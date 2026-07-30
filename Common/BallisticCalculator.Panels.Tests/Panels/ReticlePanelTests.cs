using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using AwesomeAssertions;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Reticle;
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
    #region The library Mil-Dot reticle is in milliradians

    /// <summary>
    /// A mil-dot reticle is a <b>milliradian</b> instrument: one dot spacing is 1 mrad. Built in
    /// <see cref="AngularUnit.Mil"/> instead — the military mil, 1/6400 of a circle — every subtension would be
    /// about 1.9 % small, a hold error at any distance worth holding for. Pinned here so a regression upstream
    /// is caught by this suite.
    /// </summary>
    [Fact]
    public void LibraryMilDot_UsesMilliradiansThroughout()
    {
        // Arrange & Act
        var reticle = new MilDotReticle();

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
    public void LibraryMilDot_HasItsDotsOnWholeMilliradians()
    {
        var reticle = new MilDotReticle();

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

    #region The Mil-Dot button builds that object

    /// <summary>
    /// The button builds the library object. Nothing on this path touches the file system, so it cannot fail on
    /// a missing or misnamed file.
    /// </summary>
    [AvaloniaFact]
    public void MilDotButton_BuildsTheLibraryReticle()
    {
        // Arrange
        var panel = new ReticlePanel();
        panel.Reticle.Should().BeNull("nothing is loaded until asked");

        // Act
        panel.MilDotButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Assert
        panel.Reticle.Should().NotBeNull();
        panel.Reticle.Should().BeOfType<MilDotReticle>();
        panel.Reticle!.Name.Should().Be("Mil-Dot Reticle");
        panel.Reticle.Size!.X.Unit.Should().Be(AngularUnit.MRad);
        panel.Reticle.Size.X.In(AngularUnit.MRad).Should().BeApproximately(12, 1e-9);
        panel.Reticle.BulletDropCompensator.Should().NotBeEmpty();
        panel.ReticleNameText.Text.Should().Contain("Mil-Dot");
    }

    #endregion

}
