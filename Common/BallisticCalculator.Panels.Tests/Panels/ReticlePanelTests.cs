using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using AwesomeAssertions;
using BallisticCalculator;
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

    #region The Near/Far BDC split

    private static ShotData ShotZeroedAt(double yards) => new()
    {
        Ammunition = new AmmunitionLibraryEntry
        {
            Name = "test",
            Ammunition = new Ammunition(
                new Measurement<WeightUnit>(40, WeightUnit.Grain),
                new BallisticCoefficient(0.125, DragTableId.G1),
                new Measurement<VelocityUnit>(1050, VelocityUnit.FeetPerSecond)),
        },
        Weapon = new Rifle(
            new Sight(new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                      Measurement<AngularUnit>.ZERO, Measurement<AngularUnit>.ZERO),
            new ZeroingParameters(new Measurement<DistanceUnit>(yards, DistanceUnit.Yard), null, null)),
    };

    [AvaloniaFact]
    public void BdcSplit_ShouldDefaultToTheShotsZero()
    {
        // The default is unchanged behaviour: the split is the zero, which for a centrefire is the
        // 100 yd everyone saw before this control existed.
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(100) };

        panel.BdcSplit.In(DistanceUnit.Yard).Should().BeApproximately(100, 1e-6);
        panel.BdcSplitControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Yard)
             .Should().BeApproximately(100, 1e-6);
    }

    [AvaloniaFact]
    public void BdcSplit_ShouldFollowANonStandardZero()
    {
        // A .22 LR zeroed at 50 yd splits at 50, not at a hardcoded 100.
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(50) };

        panel.BdcSplit.In(DistanceUnit.Yard).Should().BeApproximately(50, 1e-6);
    }

    [AvaloniaFact]
    public void BdcSplit_WhenTheUserSetsOne_ShouldBeUsed()
    {
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(100) };

        panel.BdcSplitControl.SetValue(new Measurement<DistanceUnit>(60, DistanceUnit.Yard));

        panel.BdcSplit.In(DistanceUnit.Yard).Should().BeApproximately(60, 1e-6);
    }

    [AvaloniaFact]
    public void BdcSplit_WhenCleared_ShouldFallBackToTheZero()
    {
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(75) };

        panel.BdcSplitControl.Value = null;

        panel.BdcSplit.In(DistanceUnit.Yard).Should().BeApproximately(75, 1e-6);
    }

    [AvaloniaFact]
    public void BdcSplit_WhenCleared_ShouldFollowTheZeroAgainAndShowIt()
    {
        // Clearing the field is how the user hands the split back. It must not stay frozen at the value
        // they had typed, and it must not sit empty either — the box shows the zero it is following.
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(100) };
        panel.BdcSplitControl.SetValue(new Measurement<DistanceUnit>(60, DistanceUnit.Yard));
        panel.OnBdcSplitChanged(panel.BdcSplitControl, EventArgs.Empty);

        panel.BdcSplitControl.Value = null;
        panel.OnBdcSplitChanged(panel.BdcSplitControl, EventArgs.Empty);

        panel.BdcSplitControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Yard)
             .Should().BeApproximately(100, 1e-6, "the box shows the zero it has gone back to following");

        panel.ShotData = ShotZeroedAt(250);

        panel.BdcSplit.In(DistanceUnit.Yard).Should().BeApproximately(250, 1e-6,
            "and it follows the zero again from here on");
    }

    [AvaloniaFact]
    public void BdcSplit_ShouldFollowTheMeasurementSystem()
    {
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(100) };

        panel.MeasurementSystem = MeasurementSystem.Metric;

        panel.BdcSplitControl.GetValue<DistanceUnit>()!.Value.Unit.Should().Be(DistanceUnit.Meter);
    }

    [AvaloniaFact]
    public void BdcSplit_SwitchingUnits_ShouldNotCountAsTheUserOverridingIt()
    {
        // Converting the field into the new unit raises Changed on the control. If that were taken for a
        // user override, the split would stop following the zero after any Ctrl+Shift+M.
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(100) };

        panel.MeasurementSystem = MeasurementSystem.Metric;
        panel.ShotData = ShotZeroedAt(300);

        panel.BdcSplit.In(DistanceUnit.Yard).Should().BeApproximately(300, 1e-6,
            "the split still follows the zero, because switching units is not a choice about the split");
    }

    [AvaloniaFact]
    public void BdcSplit_OnceTheUserSetsIt_ShouldSurviveAChangeOfShot()
    {
        // Editing the shot must not silently discard a split chosen for this load.
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(100) };
        panel.BdcSplitControl.SetValue(new Measurement<DistanceUnit>(60, DistanceUnit.Yard));
        // Headless Avalonia raises no change event for a programmatic SetValue; in the app typing or the
        // spinner does, and that is what marks the split as the user's own.
        panel.OnBdcSplitChanged(panel.BdcSplitControl, EventArgs.Empty);

        panel.ShotData = ShotZeroedAt(200);

        panel.BdcSplit.In(DistanceUnit.Yard).Should().BeApproximately(60, 1e-6);
    }

    [AvaloniaFact]
    public void BdcSplitPanel_ShouldBeEnabledOnlyForTheBdcOverlays()
    {
        var panel = new ReticlePanel { ShotData = ShotZeroedAt(100) };

        panel.BdcSplitPanel.IsEnabled.Should().BeFalse("None is selected to start with");

        panel.RadioFarBdc.IsChecked = true;
        panel.BdcSplitPanel.IsEnabled.Should().BeTrue();

        panel.RadioNearBdc.IsChecked = true;
        panel.BdcSplitPanel.IsEnabled.Should().BeTrue();

        panel.RadioTarget.IsChecked = true;
        panel.BdcSplitPanel.IsEnabled.Should().BeFalse();
    }

    #endregion

}
