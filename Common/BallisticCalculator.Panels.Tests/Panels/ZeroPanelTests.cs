using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class ZeroPanelTests
{
    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new ZeroPanel();

        panel.Should().NotBeNull();
        panel.ZeroDistanceControl.Should().NotBeNull();
        panel.ZeroShotAngleControl.Should().NotBeNull();
        panel.VerticalOffsetCheckBox.Should().NotBeNull();
        panel.VerticalOffsetControl.Should().NotBeNull();
        panel.HorizontalOffsetControl.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_OffsetControlsDisabled()
    {
        var panel = new ZeroPanel();

        panel.VerticalOffsetControl.IsEnabled.Should().BeFalse();
        panel.HorizontalOffsetControl.IsEnabled.Should().BeFalse();
        panel.VerticalOffsetCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void ZeroDistance_WhenEmpty_ReturnsNull()
    {
        var panel = new ZeroPanel();

        panel.ZeroDistance.Should().BeNull();
    }

    [AvaloniaFact]
    public void SetZeroDistance_SetsTheControl()
    {
        var panel = new ZeroPanel();

        panel.SetZeroDistance(new Measurement<DistanceUnit>(200, DistanceUnit.Yard));

        panel.ZeroDistance.Should().NotBeNull();
        panel.ZeroDistance!.Value.In(DistanceUnit.Yard).Should().BeApproximately(200, 0.5);
    }

    [AvaloniaFact]
    public void Zeroing_SetAndGet_DistanceAndAngle_RoundTrip()
    {
        var panel = new ZeroPanel();

        panel.Zeroing = new ZeroingData
        {
            Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            ShotAngle = new Measurement<AngularUnit>(3, AngularUnit.Mil),
        };
        var result = panel.Zeroing;

        result.Should().NotBeNull();
        result!.Distance!.Value.In(DistanceUnit.Meter).Should().BeApproximately(100, 0.5);
        result.ShotAngle!.Value.In(AngularUnit.Mil).Should().BeApproximately(3, 0.05);
    }

    [AvaloniaFact]
    public void Zeroing_SetWithOffsets_EnablesCheckboxAndRoundTrips()
    {
        var panel = new ZeroPanel();

        panel.Zeroing = new ZeroingData
        {
            Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            VerticalOffset = new Measurement<DistanceUnit>(20, DistanceUnit.Millimeter),
            HorizontalOffset = new Measurement<DistanceUnit>(10, DistanceUnit.Millimeter),
        };

        panel.VerticalOffsetCheckBox.IsChecked.Should().BeTrue();
        panel.VerticalOffsetControl.IsEnabled.Should().BeTrue();

        var result = panel.Zeroing;
        result!.VerticalOffset!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(20, 0.5);
        result.HorizontalOffset!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(10, 0.5);
    }

    [AvaloniaFact]
    public void Zeroing_OffsetsIgnored_WhenCheckboxUnchecked()
    {
        var panel = new ZeroPanel();
        panel.Zeroing = new ZeroingData
        {
            Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            VerticalOffset = new Measurement<DistanceUnit>(20, DistanceUnit.Millimeter),
        };

        panel.VerticalOffsetCheckBox.IsChecked = false;

        panel.Zeroing!.VerticalOffset.Should().BeNull();
    }

    [AvaloniaFact]
    public void Zeroing_SetWithZeroAmmoAndWind_RoundTrip()
    {
        var panel = new ZeroPanel();

        panel.Zeroing = new ZeroingData
        {
            Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            Ammunition = new Ammunition()
            {
                Weight = new Measurement<WeightUnit>(150, WeightUnit.Grain),
                BallisticCoefficient = new BallisticCoefficient(0.415, DragTableId.G1),
                MuzzleVelocity = new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
            },
            Wind = new Wind()
            {
                Velocity = new Measurement<VelocityUnit>(4, VelocityUnit.MetersPerSecond),
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            },
        };
        var result = panel.Zeroing;

        result!.Ammunition.Should().NotBeNull();
        result.Ammunition!.Weight.In(WeightUnit.Grain).Should().BeApproximately(150, 0.5);
        result.Wind.Should().NotBeNull();
        result.Wind!.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(4, 0.5);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ChangesUnits()
    {
        var panel = new ZeroPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.ZeroDistanceControl).Should().Be(DistanceUnit.Yard);
        GetSelectedUnit(panel.VerticalOffsetControl).Should().Be(DistanceUnit.Inch);
    }

    [AvaloniaFact]
    public void Clear_ResetsEverything()
    {
        var panel = new ZeroPanel();
        panel.Zeroing = new ZeroingData
        {
            Distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            VerticalOffset = new Measurement<DistanceUnit>(20, DistanceUnit.Millimeter),
            ShotAngle = new Measurement<AngularUnit>(3, AngularUnit.Mil),
        };

        panel.Clear();

        panel.ZeroDistance.Should().BeNull();
        panel.ZeroDistanceControl.IsEmpty.Should().BeTrue();
        panel.ZeroShotAngleControl.IsEmpty.Should().BeTrue();
        panel.VerticalOffsetCheckBox.IsChecked.Should().BeFalse();
        panel.VerticalOffsetControl.IsEnabled.Should().BeFalse();
    }

    private static object? GetSelectedUnit(BallisticCalculator.Controls.Controls.MeasurementControl control)
    {
        return (control.UnitPart?.SelectedItem as UnitItem)?.Unit;
    }
}
