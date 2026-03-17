using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class RiflePanelTests
{
    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldBeFalse()
    {
        var panel = new RiflePanel();

        panel.ConvertOnSystemChange.Should().BeFalse();
    }

    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldNotConvertOnSwitch()
    {
        var panel = new RiflePanel();
        panel.Rifle = CreateTestRifle();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Default is false, so values stay in original units
        GetSelectedUnit(panel.SightHeightControl).Should().Be(DistanceUnit.Millimeter);
        GetSelectedUnit(panel.ZeroDistanceControl).Should().Be(DistanceUnit.Meter);
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new RiflePanel();

        panel.Should().NotBeNull();
        panel.SightHeightControl.Should().NotBeNull();
        panel.ZeroDistanceControl.Should().NotBeNull();
        panel.RiflingDirectionCombo.Should().NotBeNull();
        panel.RiflingStepControl.Should().NotBeNull();
        panel.HorizontalClickControl.Should().NotBeNull();
        panel.VerticalClickControl.Should().NotBeNull();
        panel.VerticalOffsetCheckBox.Should().NotBeNull();
        panel.VerticalOffsetControl.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullRifle()
    {
        var panel = new RiflePanel();

        panel.Rifle.Should().BeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_RiflingStepShouldBeDisabled()
    {
        var panel = new RiflePanel();

        panel.RiflingStepControl.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Panel_InitialState_VerticalOffsetShouldBeDisabled()
    {
        var panel = new RiflePanel();

        panel.VerticalOffsetControl.IsEnabled.Should().BeFalse();
        panel.VerticalOffsetCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Rifle_SetAndGet_MinimalRifle_ShouldRoundTrip()
    {
        var panel = new RiflePanel();
        var rifle = CreateMinimalRifle();

        panel.Rifle = rifle;
        var result = panel.Rifle;

        result.Should().NotBeNull();
        result!.Sight.SightHeight.In(DistanceUnit.Millimeter).Should().BeApproximately(50, 0.5);
        result.Zero.Distance.In(DistanceUnit.Meter).Should().BeApproximately(100, 0.5);
        result.Sight.VerticalClick.Should().BeNull();
        result.Sight.HorizontalClick.Should().BeNull();
        result.Rifling.Should().BeNull();
        result.Zero.VerticalOffset.Should().BeNull();
    }

    [AvaloniaFact]
    public void Rifle_SetAndGet_FullRifle_ShouldRoundTrip()
    {
        var panel = new RiflePanel();
        var rifle = CreateTestRifle();

        panel.Rifle = rifle;
        var result = panel.Rifle;

        result.Should().NotBeNull();
        result!.Sight.SightHeight.In(DistanceUnit.Millimeter).Should().BeApproximately(50, 0.5);
        result.Zero.Distance.In(DistanceUnit.Meter).Should().BeApproximately(100, 0.5);
        result.Sight.VerticalClick.Should().NotBeNull();
        result.Sight.VerticalClick!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.01);
        result.Sight.HorizontalClick.Should().NotBeNull();
        result.Sight.HorizontalClick!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.01);
        result.Rifling.Should().NotBeNull();
        result.Rifling!.RiflingStep.In(DistanceUnit.Inch).Should().BeApproximately(12, 0.5);
        result.Rifling.Direction.Should().Be(TwistDirection.Right);
    }

    [AvaloniaFact]
    public void Rifle_SetWithVerticalOffset_ShouldRoundTrip()
    {
        var panel = new RiflePanel();
        var rifle = new Rifle(
            new Sight(
                new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA)),
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null)
            {
                VerticalOffset = new Measurement<DistanceUnit>(25, DistanceUnit.Millimeter)
            });

        panel.Rifle = rifle;

        panel.VerticalOffsetCheckBox.IsChecked.Should().BeTrue();
        panel.VerticalOffsetControl.IsEnabled.Should().BeTrue();

        var result = panel.Rifle;
        result.Should().NotBeNull();
        result!.Zero.VerticalOffset.Should().NotBeNull();
        result.Zero.VerticalOffset!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(25, 0.5);
    }

    [AvaloniaFact]
    public void Rifle_SetNull_ShouldClear()
    {
        var panel = new RiflePanel();
        panel.Rifle = CreateTestRifle();

        panel.Rifle = null;

        panel.Rifle.Should().BeNull();
        panel.SightHeightControl.IsEmpty.Should().BeTrue();
        panel.ZeroDistanceControl.IsEmpty.Should().BeTrue();
        panel.RiflingDirectionCombo.SelectedIndex.Should().Be(0); // "Not Set"
        panel.RiflingStepControl.IsEmpty.Should().BeTrue();
        panel.RiflingStepControl.IsEnabled.Should().BeFalse();
        panel.VerticalOffsetCheckBox.IsChecked.Should().BeFalse();
        panel.VerticalOffsetControl.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void RiflingDirection_WhenSet_ShouldEnableRiflingStep()
    {
        var panel = new RiflePanel();

        panel.RiflingDirectionCombo.SelectedIndex = 1; // "Left"

        panel.RiflingStepControl.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void RiflingDirection_WhenCleared_ShouldDisableRiflingStep()
    {
        var panel = new RiflePanel();
        panel.RiflingDirectionCombo.SelectedIndex = 2; // "Right"

        panel.RiflingDirectionCombo.SelectedIndex = 0; // "Not Set"

        panel.RiflingStepControl.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void RiflingDirection_SetFromRifle_ShouldSelectCorrectItem()
    {
        var panel = new RiflePanel();
        var rifle = CreateTestRifle(); // Has Right twist

        panel.Rifle = rifle;

        panel.RiflingDirectionCombo.SelectedIndex.Should().Be(2); // "Right"
        panel.RiflingStepControl.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void RiflingDirection_Left_ShouldRoundTrip()
    {
        var panel = new RiflePanel();
        var rifle = new Rifle(
            new Sight() { SightHeight = new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter) },
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null),
            new Rifling(
                new Measurement<DistanceUnit>(9, DistanceUnit.Inch),
                TwistDirection.Left));

        panel.Rifle = rifle;
        var result = panel.Rifle;

        result.Should().NotBeNull();
        result!.Rifling.Should().NotBeNull();
        result.Rifling!.Direction.Should().Be(TwistDirection.Left);
        result.Rifling.RiflingStep.In(DistanceUnit.Inch).Should().BeApproximately(9, 0.5);
    }

    [AvaloniaFact]
    public void VerticalOffsetCheckBox_WhenChecked_ShouldEnableControl()
    {
        var panel = new RiflePanel();

        panel.VerticalOffsetCheckBox.IsChecked = true;

        panel.VerticalOffsetControl.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void VerticalOffsetCheckBox_WhenUnchecked_ShouldDisableControl()
    {
        var panel = new RiflePanel();
        panel.VerticalOffsetCheckBox.IsChecked = true;

        panel.VerticalOffsetCheckBox.IsChecked = false;

        panel.VerticalOffsetControl.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void VerticalOffset_WhenUnchecked_ShouldReturnNull()
    {
        var panel = new RiflePanel();
        var rifle = new Rifle(
            new Sight() { SightHeight = new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter) },
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null)
            {
                VerticalOffset = new Measurement<DistanceUnit>(25, DistanceUnit.Millimeter)
            });
        panel.Rifle = rifle;

        // Uncheck the offset
        panel.VerticalOffsetCheckBox.IsChecked = false;

        var result = panel.Rifle;
        result.Should().NotBeNull();
        result!.Zero.VerticalOffset.Should().BeNull();
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperial_WithConvert_ShouldPreserveValues()
    {
        var panel = new RiflePanel();
        panel.ConvertOnSystemChange = true;
        panel.Rifle = CreateTestRifle();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Rifle;
        result.Should().NotBeNull();
        result!.Sight.SightHeight.In(DistanceUnit.Millimeter).Should().BeApproximately(50, 1);
        result.Zero.Distance.In(DistanceUnit.Meter).Should().BeApproximately(100, 1);
        GetSelectedUnit(panel.SightHeightControl).Should().Be(DistanceUnit.Inch);
        GetSelectedUnit(panel.ZeroDistanceControl).Should().Be(DistanceUnit.Yard);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ShouldChangeUnits()
    {
        var panel = new RiflePanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.SightHeightControl).Should().Be(DistanceUnit.Inch);
        GetSelectedUnit(panel.ZeroDistanceControl).Should().Be(DistanceUnit.Yard);
        GetSelectedUnit(panel.RiflingStepControl).Should().Be(DistanceUnit.Inch);
        GetSelectedUnit(panel.VerticalOffsetControl).Should().Be(DistanceUnit.Inch);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetricWhenEmpty_ShouldChangeUnits()
    {
        var panel = new RiflePanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        GetSelectedUnit(panel.SightHeightControl).Should().Be(DistanceUnit.Millimeter);
        GetSelectedUnit(panel.ZeroDistanceControl).Should().Be(DistanceUnit.Meter);
        GetSelectedUnit(panel.RiflingStepControl).Should().Be(DistanceUnit.Millimeter);
        GetSelectedUnit(panel.VerticalOffsetControl).Should().Be(DistanceUnit.Millimeter);
    }

    [AvaloniaFact]
    public void MeasurementSystem_ClickUnits_ShouldNotBeAffected()
    {
        var panel = new RiflePanel();
        var rifle = CreateTestRifle(); // clicks in MOA
        panel.Rifle = rifle;

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Angular units should not change on system switch
        var result = panel.Rifle;
        result.Should().NotBeNull();
        result!.Sight.VerticalClick!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.01);
    }

    [AvaloniaFact]
    public void VerticalClick_QuickAccess_ShouldReturnValue()
    {
        var panel = new RiflePanel();
        panel.Rifle = CreateTestRifle();

        var click = panel.VerticalClick;

        click.Should().NotBeNull();
        click!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.01);
    }

    [AvaloniaFact]
    public void VerticalClick_WhenEmpty_ShouldReturnNull()
    {
        var panel = new RiflePanel();

        panel.VerticalClick.Should().BeNull();
    }

    [AvaloniaFact]
    public void Clear_ShouldResetAllFields()
    {
        var panel = new RiflePanel();
        var rifle = CreateTestRifle();
        panel.Rifle = rifle;
        panel.VerticalOffsetCheckBox.IsChecked = true;

        panel.Clear();

        panel.Rifle.Should().BeNull();
        panel.SightHeightControl.IsEmpty.Should().BeTrue();
        panel.ZeroDistanceControl.IsEmpty.Should().BeTrue();
        panel.HorizontalClickControl.IsEmpty.Should().BeTrue();
        panel.VerticalClickControl.IsEmpty.Should().BeTrue();
        panel.RiflingDirectionCombo.SelectedIndex.Should().Be(0);
        panel.RiflingStepControl.IsEmpty.Should().BeTrue();
        panel.RiflingStepControl.IsEnabled.Should().BeFalse();
        panel.VerticalOffsetCheckBox.IsChecked.Should().BeFalse();
        panel.VerticalOffsetControl.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Rifle_ClicksOptional_ShouldReturnNullWhenEmpty()
    {
        var panel = new RiflePanel();
        var rifle = CreateMinimalRifle();

        panel.Rifle = rifle;
        var result = panel.Rifle;

        result.Should().NotBeNull();
        result!.Sight.VerticalClick.Should().BeNull();
        result.Sight.HorizontalClick.Should().BeNull();
        panel.HorizontalClickControl.IsEmpty.Should().BeTrue();
        panel.VerticalClickControl.IsEmpty.Should().BeTrue();
    }

    private static object? GetSelectedUnit(BallisticCalculator.Controls.Controls.MeasurementControl control)
    {
        return (control.UnitPart?.SelectedItem as UnitItem)?.Unit;
    }

    private static Rifle CreateMinimalRifle()
    {
        return new Rifle(
            new Sight() { SightHeight = new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter) },
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null));
    }

    private static Rifle CreateTestRifle()
    {
        return new Rifle(
            new Sight(
                new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA)),
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null),
            new Rifling(
                new Measurement<DistanceUnit>(12, DistanceUnit.Inch),
                TwistDirection.Right));
    }
}
