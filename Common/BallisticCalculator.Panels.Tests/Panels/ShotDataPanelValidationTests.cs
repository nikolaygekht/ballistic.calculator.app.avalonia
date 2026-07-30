using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using Xunit;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// Cover for the problem list <see cref="ShotDataPanel.Validate"/> returns: the states that used to be
/// accepted and then either crashed the calculation (F-1, F-1b) or were silently discarded (F-4, F-5).
/// Every problem anywhere on the dialog must come back from a single pass — being told about one fault at
/// a time is the behaviour these tests exist to prevent.
/// </summary>
public class ShotDataPanelValidationTests
{
    #region A complete dialog reports nothing

    [AvaloniaFact]
    public void Validate_CompleteData_ShouldReportNoProblems()
    {
        // Arrange
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();

        // Act
        var (shotData, _, incomplete, problems) = panel.Validate();

        // Assert
        shotData.Should().NotBeNull();
        problems.Should().BeEmpty();
        incomplete.Should().BeEmpty();
    }

    #endregion

    #region F-1 / F-1b — the ammunition the engine cannot use

    [AvaloniaFact]
    public void Validate_FormFactorWithoutDiameter_ShouldReportIt()
    {
        // Arrange
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();
        panel.AmmoLibPanel.AmmoSubPanel.FormFactorCheckBox.IsChecked = true;
        panel.AmmoLibPanel.AmmoSubPanel.BulletDiameterControl.Value = null;

        // Act
        var (shotData, _, _, problems) = panel.Validate();

        // Assert — the ammunition still builds, which is exactly why this needs its own check
        shotData.Should().NotBeNull();
        problems.Should().ContainSingle().Which.Should().Contain("diameter");
    }

    [AvaloniaFact]
    public void Validate_EmptyAmmunition_ShouldNameTheMissingFieldsInsteadOfBeingGeneric()
    {
        // Arrange
        var panel = new ShotDataPanel();

        // Act
        var (shotData, _, _, problems) = panel.Validate();

        // Assert
        shotData.Should().BeNull();
        problems.Should().Contain(p => p.Contains("weight"));
        problems.Should().Contain(p => p.Contains("Ballistic coefficient"));
        problems.Should().Contain(p => p.Contains("Muzzle velocity"));
    }

    #endregion

    #region F-4 — a ticked but half-filled zero override

    [AvaloniaFact]
    public void Validate_ZeroAmmoTickedButIncomplete_ShouldReportIt()
    {
        // Arrange — a BC and a muzzle velocity, but no weight: the getter returns null, which downstream
        // means "same as the shot", so the zero would quietly use the shot's ammunition.
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();
        var zeroAmmo = panel.ZeroSubPanel.ZeroAmmoSubPanel;
        zeroAmmo.EnableCheckBox.IsChecked = true;
        zeroAmmo.AmmoSubPanel.BCControl.Value = new BallisticCoefficient(0.223, DragTableId.G7);
        zeroAmmo.AmmoSubPanel.MuzzleVelocityControl.SetValue(
            new Measurement<VelocityUnit>(2600, VelocityUnit.FeetPerSecond));

        // Act
        var (shotData, _, _, problems) = panel.Validate();

        // Assert
        shotData.Should().NotBeNull();
        problems.Should().ContainSingle();
        problems[0].Should().Contain("Other ammunition for zero");
        problems[0].Should().Contain("weight");
    }

    [AvaloniaFact]
    public void Validate_ZeroAmmoNotTicked_ShouldReportNothing()
    {
        // Arrange
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();
        panel.ZeroSubPanel.ZeroAmmoSubPanel.EnableCheckBox.IsChecked = false;

        // Act & Assert — an override that is off is not a problem, it is the normal case
        panel.Validate().Problems.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void Validate_ZeroAtmosphereTickedButEmpty_ShouldReportIt()
    {
        // Arrange
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();
        panel.ZeroSubPanel.ZeroAtmosphereSubPanel.EnableCheckBox.IsChecked = true;
        panel.ZeroSubPanel.ZeroAtmosphereSubPanel.AtmoSubPanel.Clear();

        // Act
        var problems = panel.Validate().Problems;

        // Assert
        problems.Should().ContainSingle().Which.Should().Contain("Other atmosphere for zero");
    }

    [AvaloniaFact]
    public void Validate_ZeroWindTickedButEmpty_ShouldReportIt()
    {
        // Arrange
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();
        panel.ZeroSubPanel.ZeroWindSubPanel.EnableCheckBox.IsChecked = true;
        panel.ZeroSubPanel.ZeroWindSubPanel.WindSubPanel.Clear();

        // Act
        var problems = panel.Validate().Problems;

        // Assert
        problems.Should().ContainSingle().Which.Should().Contain("Wind at zero");
    }

    #endregion

    #region F-5 — clicks dialled with no click size to convert them

    [AvaloniaFact]
    public void Validate_ClicksDialledWithoutClickSize_ShouldReportBothAxes()
    {
        // Arrange — the sight has no click values, as several shipped presets do not
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData(withClicks: false);
        panel.ParametersSubPanel.VClicksControl.Value = 4;
        panel.ParametersSubPanel.HClicksControl.Value = -2;

        // Act
        var problems = panel.Validate().Problems;

        // Assert
        problems.Should().HaveCount(2);
        problems.Should().Contain(p => p.Contains("V-Clicks") && p.Contains("Rifle tab"));
        problems.Should().Contain(p => p.Contains("H-Clicks") && p.Contains("Rifle tab"));
    }

    [AvaloniaFact]
    public void Validate_ClicksDialledWithClickSize_ShouldReportNothing()
    {
        // Arrange
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData(withClicks: true);
        panel.ParametersSubPanel.VClicksControl.Value = 4;

        // Act & Assert
        panel.Validate().Problems.Should().BeEmpty();
    }

    #endregion

    #region Everything at once

    /// <summary>
    /// The requirement that drove this: a user who has three things wrong is told all three, not the first
    /// one, then the second after a fix, then the third.
    /// </summary>
    [AvaloniaFact]
    public void Validate_SeveralFaultsAcrossTabs_ShouldReportThemAllInOnePass()
    {
        // Arrange — a form-factor ammunition with no diameter (Ammunition tab), a ticked-but-empty wind
        // override (Zero tab) and clicks with no click size (Parameters + Rifle tabs)
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData(withClicks: false);
        panel.AmmoLibPanel.AmmoSubPanel.FormFactorCheckBox.IsChecked = true;
        panel.AmmoLibPanel.AmmoSubPanel.BulletDiameterControl.Value = null;
        panel.ZeroSubPanel.ZeroWindSubPanel.EnableCheckBox.IsChecked = true;
        panel.ZeroSubPanel.ZeroWindSubPanel.WindSubPanel.Clear();
        panel.ParametersSubPanel.VClicksControl.Value = 4;

        // Act
        var problems = panel.Validate().Problems;

        // Assert
        problems.Should().HaveCount(3);
        problems.Should().Contain(p => p.Contains("diameter"));
        problems.Should().Contain(p => p.Contains("Wind at zero"));
        problems.Should().Contain(p => p.Contains("V-Clicks"));
    }

    #endregion

    #region The zero distance, and which tab gets blamed for it

    [AvaloniaFact]
    public void ShotData_WithAZeroingBlockCarryingNoDistance_ShouldTakeItFromTheRiflesZero()
    {
        // The <zeroing> block is the source of truth, but the distance is mirrored on the rifle's own
        // ZeroingParameters. The fallback used to be all-or-nothing, so a block that existed *without* a
        // distance blanked the Zero tab even though the rifle knew the zero — and OK then refused the
        // shot, naming the Rifle tab, whose every field was filled in.
        var shot = CreateShotData();
        shot.Zeroing = new ZeroingData
        {
            ShotAngle = new Measurement<AngularUnit>(0, AngularUnit.Degree),
            // Distance deliberately absent, while Weapon.Zero.Distance is 100 yd
        };

        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial, ShotData = shot };

        panel.ZeroSubPanel.ZeroDistance.Should().NotBeNull("the rifle's own zero supplies it");
        panel.ZeroSubPanel.ZeroDistance!.Value.In(DistanceUnit.Yard).Should().BeApproximately(100, 1e-6);

        var (built, _, incomplete, problems) = panel.Validate();
        built!.Weapon.Should().NotBeNull();
        incomplete.Should().BeEmpty();
        problems.Should().BeEmpty();
    }

    [AvaloniaFact]
    public void ShotData_WithAZeroingBlockCarryingNoDistance_ShouldNotDisturbTheRestOfTheBlock()
    {
        // Filling the gap must copy the block rather than replace it.
        var shot = CreateShotData();
        shot.Zeroing = new ZeroingData
        {
            ShotAngle = new Measurement<AngularUnit>(12, AngularUnit.Degree),
            Wind = new Wind(new Measurement<VelocityUnit>(5, VelocityUnit.MilesPerHour),
                            new Measurement<AngularUnit>(90, AngularUnit.Degree)),
        };

        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial, ShotData = shot };

        var zeroing = panel.ZeroSubPanel.Zeroing!;
        zeroing.ShotAngle!.Value.In(AngularUnit.Degree).Should().BeApproximately(12, 1e-6);
        zeroing.Wind.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Validate_WithNoZeroDistance_ShouldBlameTheZeroTabAndNotTheRifle()
    {
        // BuildRifle returns null for a missing sight *or* a missing zero distance, and the Rifle tab was
        // reported for both. Naming the one tab that is complete is the worst possible message.
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();
        panel.ZeroSubPanel.ZeroDistanceControl.Value = null;

        var (_, empty, incomplete, _) = panel.Validate();

        panel.RifleSubPanel.Sight.Should().NotBeNull("the Rifle tab is complete");
        incomplete.Should().NotContain("Rifle");
        empty.Should().NotContain("Rifle");
        incomplete.Should().Contain("Zero");
    }

    [AvaloniaFact]
    public void Validate_WithNoSightHeight_ShouldStillBlameTheRifleTab()
    {
        // The other half: when the sight really is the missing piece, Rifle is the right answer.
        var panel = new ShotDataPanel { MeasurementSystem = MeasurementSystem.Imperial };
        panel.ShotData = CreateShotData();
        panel.RifleSubPanel.SightHeightControl.Value = null;

        var (_, empty, incomplete, _) = panel.Validate();

        (incomplete.Contains("Rifle") || empty.Contains("Rifle")).Should().BeTrue();
        incomplete.Should().NotContain("Zero");
        empty.Should().NotContain("Zero");
    }

    #endregion

    private static ShotData CreateShotData(bool withClicks = true)
    {
        Measurement<AngularUnit>? click = withClicks
            ? new Measurement<AngularUnit>(0.25, AngularUnit.MOA)
            : null;

        return new ShotData
        {
            Ammunition = new AmmunitionLibraryEntry
            {
                Name = "168gr .308",
                Ammunition = new Ammunition
                {
                    Weight = new Measurement<WeightUnit>(168, WeightUnit.Grain),
                    BallisticCoefficient = new BallisticCoefficient(0.223, DragTableId.G7),
                    MuzzleVelocity = new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
                    BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
                },
            },
            Weapon = new Rifle(
                new Sight
                {
                    SightHeight = new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
                    VerticalClick = click,
                    HorizontalClick = click,
                },
                new ZeroingParameters(new Measurement<DistanceUnit>(100, DistanceUnit.Yard), null, null)),
            Atmosphere = new Atmosphere(),
            Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Yard) },
            Parameters = new ShotParameters
            {
                MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Yard),
                Step = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
            },
        };
    }
}
