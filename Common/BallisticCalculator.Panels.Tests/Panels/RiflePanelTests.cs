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
        panel.Sight = CreateTestSight();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Default is false, so values stay in original units
        GetSelectedUnit(panel.SightHeightControl).Should().Be(DistanceUnit.Millimeter);
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new RiflePanel();

        panel.Should().NotBeNull();
        panel.SightPresetCombo.Should().NotBeNull();
        panel.SightHeightControl.Should().NotBeNull();
        panel.BarrelPresetCombo.Should().NotBeNull();
        panel.RiflingDirectionCombo.Should().NotBeNull();
        panel.RiflingStepControl.Should().NotBeNull();
        panel.HorizontalClickControl.Should().NotBeNull();
        panel.VerticalClickControl.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullSight()
    {
        var panel = new RiflePanel();

        panel.Sight.Should().BeNull();
        panel.Rifling.Should().BeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_RiflingStepShouldBeDisabled()
    {
        var panel = new RiflePanel();

        panel.RiflingStepControl.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Sight_SetAndGet_MinimalSight_ShouldRoundTrip()
    {
        var panel = new RiflePanel();

        panel.Sight = new Sight() { SightHeight = new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter) };
        var result = panel.Sight;

        result.Should().NotBeNull();
        result!.SightHeight.In(DistanceUnit.Millimeter).Should().BeApproximately(50, 0.5);
        result.VerticalClick.Should().BeNull();
        result.HorizontalClick.Should().BeNull();
        panel.Rifling.Should().BeNull();
    }

    [AvaloniaFact]
    public void Sight_SetAndGet_WithClicks_ShouldRoundTrip()
    {
        var panel = new RiflePanel();

        panel.Sight = CreateTestSight();
        var result = panel.Sight;

        result.Should().NotBeNull();
        result!.SightHeight.In(DistanceUnit.Millimeter).Should().BeApproximately(50, 0.5);
        result.VerticalClick.Should().NotBeNull();
        result.VerticalClick!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.01);
        result.HorizontalClick.Should().NotBeNull();
        result.HorizontalClick!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.01);
    }

    [AvaloniaFact]
    public void Sight_SetNull_ShouldClearFields()
    {
        var panel = new RiflePanel();
        panel.Sight = CreateTestSight();

        panel.Sight = null;

        panel.Sight.Should().BeNull();
        panel.SightHeightControl.IsEmpty.Should().BeTrue();
        panel.VerticalClickControl.IsEmpty.Should().BeTrue();
        panel.HorizontalClickControl.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Rifling_SetAndGet_Right_ShouldRoundTrip()
    {
        var panel = new RiflePanel();

        panel.Rifling = new Rifling(new Measurement<DistanceUnit>(12, DistanceUnit.Inch), TwistDirection.Right);
        var result = panel.Rifling;

        result.Should().NotBeNull();
        result!.Direction.Should().Be(TwistDirection.Right);
        result.RiflingStep.In(DistanceUnit.Inch).Should().BeApproximately(12, 0.5);
        panel.RiflingDirectionCombo.SelectedIndex.Should().Be(2);
        panel.RiflingStepControl.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Rifling_SetAndGet_Left_ShouldRoundTrip()
    {
        var panel = new RiflePanel();

        panel.Rifling = new Rifling(new Measurement<DistanceUnit>(9, DistanceUnit.Inch), TwistDirection.Left);
        var result = panel.Rifling;

        result.Should().NotBeNull();
        result!.Direction.Should().Be(TwistDirection.Left);
        result.RiflingStep.In(DistanceUnit.Inch).Should().BeApproximately(9, 0.5);
        panel.RiflingDirectionCombo.SelectedIndex.Should().Be(1);
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
    public void VerticalClick_QuickAccess_ShouldReturnValue()
    {
        var panel = new RiflePanel();
        panel.Sight = CreateTestSight();

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
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ShouldChangeUnits()
    {
        var panel = new RiflePanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.SightHeightControl).Should().Be(DistanceUnit.Inch);
        GetSelectedUnit(panel.RiflingStepControl).Should().Be(DistanceUnit.Inch);
    }

    [AvaloniaFact]
    public void MeasurementSystem_ClickUnits_ShouldNotBeAffected()
    {
        var panel = new RiflePanel();
        panel.Sight = CreateTestSight(); // clicks in MOA

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Sight;
        result.Should().NotBeNull();
        result!.VerticalClick!.Value.In(AngularUnit.MOA).Should().BeApproximately(0.25, 0.01);
    }

    [AvaloniaFact]
    public void Clear_ShouldResetAllFields()
    {
        var panel = new RiflePanel();
        panel.Sight = CreateTestSight();
        panel.Rifling = new Rifling(new Measurement<DistanceUnit>(12, DistanceUnit.Inch), TwistDirection.Right);

        panel.Clear();

        panel.Sight.Should().BeNull();
        panel.Rifling.Should().BeNull();
        panel.SightHeightControl.IsEmpty.Should().BeTrue();
        panel.HorizontalClickControl.IsEmpty.Should().BeTrue();
        panel.VerticalClickControl.IsEmpty.Should().BeTrue();
        panel.RiflingDirectionCombo.SelectedIndex.Should().Be(0);
        panel.RiflingStepControl.IsEmpty.Should().BeTrue();
        panel.RiflingStepControl.IsEnabled.Should().BeFalse();
    }

    #region Presets

    private static BallisticDictionary TestDictionary() => new(
        new[]
        {
            new SightDictionaryEntry
            {
                Name = "Test Optic",
                SightHeight = new Measurement<DistanceUnit>(3, DistanceUnit.Inch),
                DefaultZero = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
                HorizontalClick = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
                VerticalClick = new Measurement<AngularUnit>(0.1, AngularUnit.Mil),
            },
        },
        new[]
        {
            new BarrelDictionaryEntry
            {
                Name = "Test Barrel",
                Step = new Measurement<DistanceUnit>(7, DistanceUnit.Inch),
                Direction = TwistDirection.Right,
            },
        });

    [AvaloniaFact]
    public void SightPreset_WhenSelected_FillsHeightAndClicks()
    {
        var panel = new RiflePanel();
        panel.SetDictionary(TestDictionary());

        panel.SightPresetCombo.SelectedIndex = 1; // "Test Optic"

        var sight = panel.Sight;
        sight.Should().NotBeNull();
        sight!.SightHeight.In(DistanceUnit.Inch).Should().BeApproximately(3, 0.01);
        sight.VerticalClick!.Value.In(AngularUnit.Mil).Should().BeApproximately(0.1, 0.001);
        sight.HorizontalClick!.Value.In(AngularUnit.Mil).Should().BeApproximately(0.1, 0.001);
        // The selection must stick after applying (it should not snap back to "(custom)").
        panel.SightPresetCombo.SelectedIndex.Should().Be(1);
    }

    [AvaloniaFact]
    public void SightPreset_WhenSelected_RaisesZeroDistanceSuggested()
    {
        var panel = new RiflePanel();
        panel.SetDictionary(TestDictionary());
        Measurement<DistanceUnit>? suggested = null;
        panel.ZeroDistanceSuggested += (_, d) => suggested = d;

        panel.SightPresetCombo.SelectedIndex = 1; // "Test Optic" (default-zero 100yd)

        suggested.Should().NotBeNull();
        suggested!.Value.In(DistanceUnit.Yard).Should().BeApproximately(100, 0.01);
    }

    [AvaloniaFact]
    public void BarrelPreset_WhenSelected_FillsRifling()
    {
        var panel = new RiflePanel();
        panel.SetDictionary(TestDictionary());

        panel.BarrelPresetCombo.SelectedIndex = 1; // "Test Barrel"

        var rifling = panel.Rifling;
        rifling.Should().NotBeNull();
        rifling!.Direction.Should().Be(TwistDirection.Right);
        rifling.RiflingStep.In(DistanceUnit.Inch).Should().BeApproximately(7, 0.01);
        panel.BarrelPresetCombo.SelectedIndex.Should().Be(1);
    }

    [AvaloniaFact]
    public void SightPreset_KeepsSelection_WhenFieldReSetToSameValue()
    {
        // Re-applying values that still match the preset must NOT revert the combo to "(custom)".
        var panel = new RiflePanel();
        panel.SetDictionary(TestDictionary());
        panel.SightPresetCombo.SelectedIndex = 1;

        panel.SightHeightControl.SetValue(new Measurement<DistanceUnit>(3, DistanceUnit.Inch));

        panel.SightPresetCombo.SelectedIndex.Should().Be(1);
    }

    #endregion

    private static object? GetSelectedUnit(BallisticCalculator.Controls.Controls.MeasurementControl control)
    {
        return (control.UnitPart?.SelectedItem as UnitItem)?.Unit;
    }

    private static Sight CreateTestSight()
    {
        return new Sight(
            new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
            new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
            new Measurement<AngularUnit>(0.25, AngularUnit.MOA));
    }
}
