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

    private static List<WindPanel> GetWindPanelsViaReflection(MultiWindPanel panel)
    {
        var method = panel.GetType().GetMethod("GetWindPanels",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return (List<WindPanel>)method!.Invoke(panel, null)!;
    }
}
