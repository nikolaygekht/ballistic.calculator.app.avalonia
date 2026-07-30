using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class MultiWindPanelTests
{
    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldBeFalse()
    {
        var panel = new MultiWindPanel();

        panel.ConvertOnSystemChange.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new MultiWindPanel();

        panel.Should().NotBeNull();
        panel.AddButton.Should().NotBeNull();
        panel.ClearButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldHaveOneWindPanel()
    {
        var panel = new MultiWindPanel();

        panel.WindPanelCount.Should().Be(1);
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullWinds()
    {
        var panel = new MultiWindPanel();

        panel.Winds.Should().BeNull();
    }

    [AvaloniaFact]
    public void Winds_SetSingleWind_ShouldRoundTrip()
    {
        var panel = new MultiWindPanel();
        var winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
            }
        };

        panel.Winds = winds;
        var result = panel.Winds;

        result.Should().NotBeNull();
        result!.Length.Should().Be(1);
        result[0].Direction.In(AngularUnit.Degree).Should().BeApproximately(90, 0.5);
        result[0].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(10, 0.5);
    }

    [AvaloniaFact]
    public void Winds_SetMultipleWinds_ShouldRoundTrip()
    {
        var panel = new MultiWindPanel();
        var winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
                MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            },
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(180, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            }
        };

        panel.Winds = winds;
        var result = panel.Winds;

        result.Should().NotBeNull();
        result!.Length.Should().Be(2);
        result[0].MaximumRange.Should().NotBeNull();
        result[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(500, 0.5);
        result[1].Direction.In(AngularUnit.Degree).Should().BeApproximately(180, 0.5);
    }

    [AvaloniaFact]
    public void Winds_SetMultipleWinds_ShouldCreateCorrectPanelCount()
    {
        var panel = new MultiWindPanel();
        var winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
                MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            },
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(180, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            }
        };

        panel.Winds = winds;

        panel.WindPanelCount.Should().Be(2);
    }

    [AvaloniaFact]
    public void Winds_SetNull_ShouldClearToOnePanel()
    {
        var panel = new MultiWindPanel();
        panel.Winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
                MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            },
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(180, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            }
        };

        panel.Winds = null;

        panel.WindPanelCount.Should().Be(1);
        panel.Winds.Should().BeNull();
    }

    [AvaloniaFact]
    public void Clear_ShouldResetToOneEmptyPanel()
    {
        var panel = new MultiWindPanel();
        panel.Winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
                MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            },
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(180, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            }
        };

        panel.Clear();

        panel.WindPanelCount.Should().Be(1);
        panel.Winds.Should().BeNull();
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperial_ShouldAffectAllPanels()
    {
        var panel = new MultiWindPanel();
        panel.Winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
                MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            },
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(180, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            }
        };

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Values should be preserved (no conversion by default)
        var result = panel.Winds;
        result.Should().NotBeNull();
        result!.Length.Should().Be(2);
    }

    [AvaloniaFact]
    public void InitialPanel_ShouldHaveDistanceDisabled()
    {
        var panel = new MultiWindPanel();
        var panels = GetWindPanelsViaReflection(panel);

        panels.Should().HaveCount(1);
        panels[0].MaxDistanceCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void AddPanel_ShouldEnableFirstPanelDistanceAtZero()
    {
        var panel = new MultiWindPanel();
        // Simulate clicking Add
        panel.AddButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        var panels = GetWindPanelsViaReflection(panel);
        panels.Should().HaveCount(2);

        // First panel should now have distance enabled at 0
        panels[0].MaxDistanceCheckBox.IsChecked.Should().BeTrue();
        var dist0 = panels[0].MaxDistanceControl.GetValue<DistanceUnit>();
        dist0.Should().NotBeNull();
        dist0!.Value.Value.Should().Be(0);

        // Second panel should have distance = 100
        panels[1].MaxDistanceCheckBox.IsChecked.Should().BeTrue();
        var dist1 = panels[1].MaxDistanceControl.GetValue<DistanceUnit>();
        dist1.Should().NotBeNull();
        dist1!.Value.Value.Should().BeApproximately(100, 0.5);
    }

    [AvaloniaFact]
    public void AddMultiplePanels_ShouldIncrementDistance()
    {
        var panel = new MultiWindPanel();
        // Add two more panels
        panel.AddButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        panel.AddButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        var panels = GetWindPanelsViaReflection(panel);
        panels.Should().HaveCount(3);

        // Panel 0: 0m, Panel 1: 100m, Panel 2: 200m
        panels[0].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().BeApproximately(0, 0.5);
        panels[1].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().BeApproximately(100, 0.5);
        panels[2].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().BeApproximately(200, 0.5);
    }

    [AvaloniaFact]
    public void AddPanel_WhenFirstAlreadyHasDistance_ShouldNotResetIt()
    {
        var panel = new MultiWindPanel();
        var panels = GetWindPanelsViaReflection(panel);

        // User manually enables distance on first panel and sets to 50
        panels[0].MaxDistanceCheckBox.IsChecked = true;
        panels[0].MaxDistanceControl.SetValue(new Measurement<DistanceUnit>(50, DistanceUnit.Meter));

        // Click Add
        panel.AddButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        panels = GetWindPanelsViaReflection(panel);
        // First panel should keep its 50
        panels[0].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().BeApproximately(50, 0.5);
        // Second panel should be 50 + 100 = 150
        panels[1].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().BeApproximately(150, 0.5);
    }

    [AvaloniaFact]
    public void AddPanel_ShouldCopyDirectionAndVelocityFromPrevious()
    {
        var panel = new MultiWindPanel();
        var panels = GetWindPanelsViaReflection(panel);

        // Set direction and velocity on first panel
        panels[0].Wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(45, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(8, VelocityUnit.MetersPerSecond),
        };

        // Click Add
        panel.AddButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        panels = GetWindPanelsViaReflection(panel);
        panels.Should().HaveCount(2);

        // New panel should have same direction and velocity
        var newDir = panels[1].DirectionControl.GetValue<AngularUnit>();
        newDir.Should().NotBeNull();
        newDir!.Value.In(AngularUnit.Degree).Should().BeApproximately(45, 0.5);

        var newVel = panels[1].VelocityControl.GetValue<VelocityUnit>();
        newVel.Should().NotBeNull();
        newVel!.Value.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(8, 0.5);
    }

    [AvaloniaFact]
    public void Winds_SkipsEmptyPanels_WhenGetting()
    {
        var panel = new MultiWindPanel();
        // Set one wind, which creates one filled panel
        // The first panel will have data, if we add another it will be empty
        var winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
            }
        };
        panel.Winds = winds;

        // Now the panel has 1 filled wind panel. Result should be 1 wind.
        var result = panel.Winds;
        result.Should().NotBeNull();
        result!.Length.Should().Be(1);
    }

    #region Zone boundaries — entered as "starts at", handed to the library as "ends at"

    /// <summary>
    /// The row distance is the range at which that wind <b>starts</b>; the library's
    /// <see cref="Wind.MaximumRange"/> is where it <b>ends</b>. So each zone must be handed over ending
    /// where the next one begins, and the last one open-ended.
    /// </summary>
    [AvaloniaFact]
    public void Winds_ZonesEnteredAsStartDistances_EachZoneEndsWhereTheNextBegins()
    {
        // Arrange — three zones as the user enters them: from the muzzle, from 300 m, from 500 m
        var panel = new MultiWindPanel();
        SetRows(panel,
            (0, 97, 3),
            (300, 77, 5),
            (500, 52, 4));

        // Act
        var winds = panel.Winds;

        // Assert — boundaries shift one row up, and the last zone runs to the end of the trajectory
        winds.Should().NotBeNull();
        winds!.Length.Should().Be(3);

        winds[0].Direction.In(AngularUnit.Degree).Should().BeApproximately(97, 0.5);
        winds[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(300, 0.5);

        winds[1].Direction.In(AngularUnit.Degree).Should().BeApproximately(77, 0.5);
        winds[1].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(500, 0.5);

        winds[2].Direction.In(AngularUnit.Degree).Should().BeApproximately(52, 0.5);
        winds[2].MaximumRange.Should().BeNull("the last zone holds to the end of the trajectory");
    }

    /// <summary>
    /// The regression this fixes: a first zone whose own distance became its end range never blew at
    /// all, because the engine switches away from it as soon as the bullet passes that range — zero.
    /// </summary>
    [AvaloniaFact]
    public void Winds_FirstZone_IsNotEndedByItsOwnStartDistance()
    {
        // Arrange — the first row starts at the muzzle even when a distance is showing in it
        var panel = new MultiWindPanel();
        SetRows(panel,
            (0, 90, 10),
            (300, 180, 5));

        // Act
        var winds = panel.Winds;

        // Assert
        winds!.Length.Should().Be(2);
        winds[0].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(10, 0.5);
        winds[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should()
            .BeApproximately(300, 0.5, "the first zone ends where the second starts, not at its own 0");
    }

    [AvaloniaFact]
    public void Winds_SingleZoneFromTheMuzzle_HoldsEverywhere()
    {
        // Arrange — one zone starting at 0
        var panel = new MultiWindPanel();
        SetRows(panel, (0, 90, 10));

        // Act
        var winds = panel.Winds;

        // Assert — one open-ended wind
        winds!.Length.Should().Be(1);
        winds[0].MaximumRange.Should().BeNull();
    }

    /// <summary>
    /// "5 m/s at 250 m" means there is <b>no wind</b> until 250 m. The library cannot say that with one
    /// zone — a lone wind blows for the whole flight, its range ignored — so a still-air zone has to be
    /// put in front of it.
    /// </summary>
    [AvaloniaFact]
    public void Winds_FirstZoneStartingDownrange_GetsStillAirInFrontOfIt()
    {
        // Arrange — a single row, starting at 250 m
        var panel = new MultiWindPanel();
        SetRows(panel, (250, 90, 5));

        // Act
        var winds = panel.Winds;

        // Assert — calm out to 250 m, then the wind the user entered, open-ended
        winds.Should().NotBeNull();
        winds!.Length.Should().Be(2);

        winds[0].Velocity.Value.Should().Be(0, "there is no wind before the first zone starts");
        winds[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(250, 0.5);

        winds[1].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
        winds[1].Direction.In(AngularUnit.Degree).Should().BeApproximately(90, 0.5);
        winds[1].MaximumRange.Should().BeNull();
    }

    [AvaloniaFact]
    public void Winds_SeveralZonesStartingDownrange_StillAirComesFirst()
    {
        // Arrange — calm until 250 m, then two zones
        var panel = new MultiWindPanel();
        SetRows(panel,
            (250, 90, 5),
            (500, 45, 4));

        // Act
        var winds = panel.Winds;

        // Assert
        winds!.Length.Should().Be(3);
        winds[0].Velocity.Value.Should().Be(0);
        winds[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(250, 0.5);
        winds[1].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
        winds[1].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(500, 0.5);
        winds[2].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(4, 0.5);
        winds[2].MaximumRange.Should().BeNull();
    }

    /// <summary>
    /// The other half of the synthesis: a leading still-air zone is the panel's own doing, so loading one
    /// back must fold it into the first real zone's start rather than showing a row of nothing.
    /// </summary>
    [AvaloniaFact]
    public void Winds_LeadingStillAirZone_FoldsBackIntoTheFirstRowOnLoad()
    {
        // Arrange — what a save produces for "5 m/s at 250 m"
        var panel = new MultiWindPanel();
        var saved = new[]
        {
            Wind(0, 0, 250),
            Wind(90, 5, null),
        };

        // Act
        panel.Winds = saved;

        // Assert — one row, showing the wind starting at 250 m
        panel.WindPanelCount.Should().Be(1, "the still-air zone is not a zone the user entered");
        var rows = GetWindPanelsViaReflection(panel);
        rows[0].VelocityControl.GetValue<VelocityUnit>()!.Value.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
        rows[0].MaxDistanceCheckBox.IsChecked.Should().BeTrue();
        rows[0].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Meter).Should().BeApproximately(250, 0.5);

        // Assert — and it goes back out unchanged
        var reread = panel.Winds;
        reread!.Length.Should().Be(2);
        reread[0].Velocity.Value.Should().Be(0);
        reread[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(250, 0.5);
        reread[1].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
    }

    [AvaloniaFact]
    public void Winds_ZonesEnteredOutOfOrder_AreSortedByStartDistance()
    {
        // Arrange — the library requires ascending zones; the user is under no such obligation
        var panel = new MultiWindPanel();
        SetRows(panel,
            (500, 45, 4),
            (200, 90, 10));

        // Act
        var winds = panel.Winds;

        // Assert — sorted, and with still air in front since the earliest zone starts at 200 m
        winds!.Length.Should().Be(3);
        winds[0].Velocity.Value.Should().Be(0);
        winds[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(200, 0.5);
        winds[1].Direction.In(AngularUnit.Degree).Should().BeApproximately(90, 0.5, "the 200 m zone comes first");
        winds[1].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(500, 0.5);
        winds[2].Direction.In(AngularUnit.Degree).Should().BeApproximately(45, 0.5);
        winds[2].MaximumRange.Should().BeNull();
    }

    /// <summary>
    /// The conversion has to survive a save and reload: what a <c>.trajectory</c> file stores is the
    /// library's end ranges, and the panel must show them as starts and hand back the same ends.
    /// </summary>
    [AvaloniaFact]
    public void Winds_ThreeZones_SurviveALoadAndReadUnchanged()
    {
        // Arrange — as a saved file holds them: ends at 300, 500, then open-ended
        var panel = new MultiWindPanel();
        var saved = new[]
        {
            Wind(97, 3, 300),
            Wind(77, 5, 500),
            Wind(52, 4, null),
        };

        // Act
        panel.Winds = saved;
        var reread = panel.Winds;

        // Assert — same ends back out, and the rows show the starts (muzzle, 300, 500)
        reread!.Length.Should().Be(3);
        reread[0].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(300, 0.5);
        reread[1].MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(500, 0.5);
        reread[2].MaximumRange.Should().BeNull();
        reread[0].Direction.In(AngularUnit.Degree).Should().BeApproximately(97, 0.5);
        reread[2].Direction.In(AngularUnit.Degree).Should().BeApproximately(52, 0.5);

        var rows = GetWindPanelsViaReflection(panel);
        rows[1].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Meter).Should()
            .BeApproximately(300, 0.5, "the second row starts where the first zone ended");
        rows[2].MaxDistanceControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Meter).Should()
            .BeApproximately(500, 0.5);
    }

    /// <summary>
    /// The regression at the level that matters: run the zones the panel produces through the engine and
    /// check the first row's wind actually deflects the bullet. Before the conversion this came out at
    /// exactly zero — the wind was entered, stored, saved, and never applied.
    /// </summary>
    [AvaloniaFact]
    public void Winds_FirstRowWind_ReachesTheEngineAndDeflectsTheBullet()
    {
        // Arrange — 10 m/s from the right for the first 250 m, then still air
        var zoned = new MultiWindPanel();
        SetRows(zoned,
            (0, 90, 10),
            (250, 90, 0));

        var everywhere = new MultiWindPanel();
        SetRows(everywhere, (0, 90, 10));

        // Act
        var zonedWindage = WindageAt500m(zoned.Winds);
        var fullWindage = WindageAt500m(everywhere.Winds);

        // Assert
        fullWindage.Should().BeGreaterThan(0, "a wind from the right pushes the bullet to one side");
        zonedWindage.Should().BeGreaterThan(0, "the wind in the first row must act from the muzzle");
        zonedWindage.Should().BeLessThan(fullWindage, "it stops at 250 m, so it deflects less than a wind holding all the way");
    }

    private static double WindageAt500m(Wind[]? winds)
    {
        var shot = new ShotData
        {
            Ammunition = new AmmunitionLibraryEntry
            {
                Name = "test",
                Ammunition = new Ammunition
                {
                    Weight = new Measurement<WeightUnit>(69, WeightUnit.Grain),
                    BallisticCoefficient = new BallisticCoefficient(0.365, DragTableId.G1),
                    MuzzleVelocity = new Measurement<VelocityUnit>(800, VelocityUnit.MetersPerSecond),
                },
            },
            Weapon = new Rifle
            {
                Sight = new Sight { SightHeight = new Measurement<DistanceUnit>(65, DistanceUnit.Millimeter) },
                Zero = new ZeroingParameters { Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter) },
            },
            Zeroing = new ZeroingData { Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter) },
            Atmosphere = new Atmosphere(),
            Winds = winds,
            Parameters = new ShotParameters
            {
                MaximumDistance = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
                Step = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            },
        };

        var trajectory = ShotTrajectoryCalculator.Calculate(shot)!;
        return trajectory[trajectory.Length - 1].Windage.In(DistanceUnit.Millimeter);
    }

    #endregion

    private static Wind Wind(double directionDegrees, double velocityMetersPerSecond, double? maximumRangeMeters) =>
        new()
        {
            Direction = new Measurement<AngularUnit>(directionDegrees, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(velocityMetersPerSecond, VelocityUnit.MetersPerSecond),
            MaximumRange = maximumRangeMeters == null
                ? null
                : new Measurement<DistanceUnit>(maximumRangeMeters.Value, DistanceUnit.Meter),
        };

    /// <summary>
    /// Fills the panel's rows as a user would: one row per zone, each with the distance the wind
    /// starts at, its direction in degrees and its velocity in m/s.
    /// </summary>
    private static void SetRows(MultiWindPanel panel, params (double StartMeters, double DirectionDegrees, double VelocityMetersPerSecond)[] rows)
    {
        while (panel.WindPanelCount < rows.Length)
            panel.AddButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        var windPanels = GetWindPanelsViaReflection(panel);
        for (var i = 0; i < rows.Length; i++)
        {
            windPanels[i].DirectionControl.SetValue(new Measurement<AngularUnit>(rows[i].DirectionDegrees, AngularUnit.Degree));
            windPanels[i].VelocityControl.SetValue(new Measurement<VelocityUnit>(rows[i].VelocityMetersPerSecond, VelocityUnit.MetersPerSecond));
            windPanels[i].MaxDistanceCheckBox.IsChecked = true;
            windPanels[i].MaxDistanceControl.SetValue(new Measurement<DistanceUnit>(rows[i].StartMeters, DistanceUnit.Meter));
        }
    }

    private static List<WindPanel> GetWindPanelsViaReflection(MultiWindPanel panel)
    {
        var method = panel.GetType().GetMethod("GetWindPanels",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return (List<WindPanel>)method!.Invoke(panel, null)!;
    }
}
