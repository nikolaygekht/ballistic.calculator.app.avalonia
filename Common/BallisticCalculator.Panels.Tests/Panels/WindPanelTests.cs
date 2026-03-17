using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class WindPanelTests
{
    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldBeFalse()
    {
        var panel = new WindPanel();

        panel.ConvertOnSystemChange.Should().BeFalse();
    }

    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldNotConvertOnSwitch()
    {
        var panel = new WindPanel();
        // Don't set ConvertOnSystemChange — use default (false)
        panel.Wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
        };

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Default is false, so velocity stays in original unit
        GetSelectedUnit(panel.VelocityControl).Should().Be(VelocityUnit.MetersPerSecond);
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new WindPanel();

        panel.Should().NotBeNull();
        panel.DirectionControl.Should().NotBeNull();
        panel.VelocityControl.Should().NotBeNull();
        panel.MaxDistanceControl.Should().NotBeNull();
        panel.MaxDistanceCheckBox.Should().NotBeNull();
        panel.WindIndicator.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullWind()
    {
        var panel = new WindPanel();

        panel.Wind.Should().BeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldBeEmpty()
    {
        var panel = new WindPanel();

        panel.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Wind_SetAndGet_ShouldRoundTrip()
    {
        var panel = new WindPanel();
        var wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
        };

        panel.Wind = wind;
        var result = panel.Wind;

        result.Should().NotBeNull();
        result!.Direction.In(AngularUnit.Degree).Should().BeApproximately(90, 0.5);
        result.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(10, 0.5);
        result.MaximumRange.Should().BeNull();
    }

    [AvaloniaFact]
    public void Wind_SetWithMaxRange_ShouldRoundTrip()
    {
        var panel = new WindPanel();
        var wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(180, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
        };

        panel.Wind = wind;
        var result = panel.Wind;

        result.Should().NotBeNull();
        result!.Direction.In(AngularUnit.Degree).Should().BeApproximately(180, 0.5);
        result.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
        result.MaximumRange.Should().NotBeNull();
        result.MaximumRange!.Value.In(DistanceUnit.Meter).Should().BeApproximately(500, 0.5);
    }

    [AvaloniaFact]
    public void Wind_SetWithMaxRange_ShouldCheckCheckBox()
    {
        var panel = new WindPanel();
        var wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
            MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
        };

        panel.Wind = wind;

        panel.MaxDistanceCheckBox.IsChecked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Wind_SetWithoutMaxRange_ShouldUncheckCheckBox()
    {
        var panel = new WindPanel();
        var wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
        };

        panel.Wind = wind;

        panel.MaxDistanceCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Wind_SetNull_ShouldClear()
    {
        var panel = new WindPanel();
        panel.Wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
        };

        panel.Wind = null;

        panel.Wind.Should().BeNull();
        panel.IsEmpty.Should().BeTrue();
        panel.MaxDistanceCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void MaxDistanceCheckBox_WhenUnchecked_ShouldNotIncludeMaxRange()
    {
        var panel = new WindPanel();
        var wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
            MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
        };
        panel.Wind = wind;

        // Uncheck the checkbox
        panel.MaxDistanceCheckBox.IsChecked = false;

        var result = panel.Wind;
        result.Should().NotBeNull();
        result!.MaximumRange.Should().BeNull();
    }

    [AvaloniaFact]
    public void Direction_ShouldDefaultToDegrees()
    {
        var panel = new WindPanel();

        GetSelectedUnit(panel.DirectionControl).Should().Be(AngularUnit.Degree);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ShouldChangeUnits()
    {
        var panel = new WindPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.VelocityControl).Should().Be(VelocityUnit.MilesPerHour);
        GetSelectedUnit(panel.MaxDistanceControl).Should().Be(DistanceUnit.Yard);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetricWhenEmpty_ShouldChangeUnits()
    {
        var panel = new WindPanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        GetSelectedUnit(panel.VelocityControl).Should().Be(VelocityUnit.MetersPerSecond);
        GetSelectedUnit(panel.MaxDistanceControl).Should().Be(DistanceUnit.Meter);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperial_WithConvert_ShouldPreserveValues()
    {
        var panel = new WindPanel();
        panel.ConvertOnSystemChange = true;
        panel.Wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
        };

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Wind;
        result.Should().NotBeNull();
        result!.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(10, 0.5);
    }

    [AvaloniaFact]
    public void Clear_ShouldResetAllFields()
    {
        var panel = new WindPanel();
        panel.Wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
            MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
        };

        panel.Clear();

        panel.Wind.Should().BeNull();
        panel.IsEmpty.Should().BeTrue();
        panel.MaxDistanceCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void IsEmpty_WhenDirectionAndVelocitySet_ShouldBeFalse()
    {
        var panel = new WindPanel();
        panel.Wind = new Wind()
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(10, VelocityUnit.MetersPerSecond),
        };

        panel.IsEmpty.Should().BeFalse();
    }

    private static object? GetSelectedUnit(BallisticCalculator.Controls.Controls.MeasurementControl control)
    {
        return (control.UnitPart?.SelectedItem as UnitItem)?.Unit;
    }
}
