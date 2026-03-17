using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class ParametersPanelTests
{
    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldBeTrue()
    {
        var panel = new ParametersPanel();

        panel.ConvertOnSystemChange.Should().BeTrue();
    }

    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldConvertOnSwitch()
    {
        var panel = new ParametersPanel();
        // Don't set ConvertOnSystemChange — use default (true)
        panel.Parameters = CreateTestParameters();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Default is true, so values should be converted
        GetSelectedUnit(panel.MaxRangeControl).Should().Be(DistanceUnit.Yard);
        GetSelectedUnit(panel.StepControl).Should().Be(DistanceUnit.Yard);
        var result = panel.Parameters;
        result.Should().NotBeNull();
        result!.MaximumDistance.In(DistanceUnit.Meter).Should().BeApproximately(1000, 2);
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new ParametersPanel();

        panel.Should().NotBeNull();
        panel.MaxRangeControl.Should().NotBeNull();
        panel.StepControl.Should().NotBeNull();
        panel.AngleControl.Should().NotBeNull();
        panel.ClicksTextBox.Should().NotBeNull();
        panel.ClicksSetButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullParameters()
    {
        var panel = new ParametersPanel();

        panel.Parameters.Should().BeNull();
    }

    [AvaloniaFact]
    public void Parameters_SetAndGet_ShouldRoundTrip()
    {
        var panel = new ParametersPanel();
        var parms = CreateTestParameters();

        panel.Parameters = parms;
        var result = panel.Parameters;

        result.Should().NotBeNull();
        result!.MaximumDistance.In(DistanceUnit.Meter).Should().BeApproximately(1000, 1);
        result.Step.In(DistanceUnit.Meter).Should().BeApproximately(100, 1);
    }

    [AvaloniaFact]
    public void Parameters_WithAngle_ShouldRoundTrip()
    {
        var panel = new ParametersPanel();
        var parms = new ShotParameters()
        {
            MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter),
            Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            ShotAngle = new Measurement<AngularUnit>(5, AngularUnit.Degree),
        };

        panel.Parameters = parms;
        var result = panel.Parameters;

        result.Should().NotBeNull();
        result!.ShotAngle.Should().NotBeNull();
        result.ShotAngle!.Value.In(AngularUnit.Degree).Should().BeApproximately(5, 0.1);
    }

    [AvaloniaFact]
    public void Parameters_WithoutAngle_ShouldReturnNullAngle()
    {
        var panel = new ParametersPanel();
        var parms = CreateTestParameters(); // no angle

        panel.Parameters = parms;
        var result = panel.Parameters;

        result.Should().NotBeNull();
        result!.ShotAngle.Should().BeNull();
    }

    [AvaloniaFact]
    public void Parameters_SetNull_ShouldClear()
    {
        var panel = new ParametersPanel();
        panel.Parameters = CreateTestParameters();

        panel.Parameters = null;

        panel.Parameters.Should().BeNull();
        panel.MaxRangeControl.IsEmpty.Should().BeTrue();
        panel.StepControl.IsEmpty.Should().BeTrue();
        panel.AngleControl.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Parameters_ImperialValues_ShouldRoundTrip()
    {
        var panel = new ParametersPanel();
        var parms = new ShotParameters()
        {
            MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Yard),
            Step = new Measurement<DistanceUnit>(100, DistanceUnit.Yard),
        };

        panel.Parameters = parms;
        var result = panel.Parameters;

        result.Should().NotBeNull();
        result!.MaximumDistance.In(DistanceUnit.Yard).Should().BeApproximately(1000, 1);
        result.Step.In(DistanceUnit.Yard).Should().BeApproximately(100, 1);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperial_WithConvert_ShouldPreserveValues()
    {
        var panel = new ParametersPanel();
        panel.ConvertOnSystemChange = true;
        panel.Parameters = CreateTestParameters();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Parameters;
        result.Should().NotBeNull();
        result!.MaximumDistance.In(DistanceUnit.Meter).Should().BeApproximately(1000, 2);
        result.Step.In(DistanceUnit.Meter).Should().BeApproximately(100, 1);
        GetSelectedUnit(panel.MaxRangeControl).Should().Be(DistanceUnit.Yard);
        GetSelectedUnit(panel.StepControl).Should().Be(DistanceUnit.Yard);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchWithoutConvert_ShouldLeaveValuesUntouched()
    {
        var panel = new ParametersPanel();
        panel.ConvertOnSystemChange = false;
        panel.Parameters = CreateTestParameters();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.MaxRangeControl).Should().Be(DistanceUnit.Meter);
        GetSelectedUnit(panel.StepControl).Should().Be(DistanceUnit.Meter);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ShouldChangeUnits()
    {
        var panel = new ParametersPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.MaxRangeControl).Should().Be(DistanceUnit.Yard);
        GetSelectedUnit(panel.StepControl).Should().Be(DistanceUnit.Yard);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetricWhenEmpty_ShouldChangeUnits()
    {
        var panel = new ParametersPanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        GetSelectedUnit(panel.MaxRangeControl).Should().Be(DistanceUnit.Meter);
        GetSelectedUnit(panel.StepControl).Should().Be(DistanceUnit.Meter);
    }

    [AvaloniaFact]
    public void MeasurementSystem_AngleUnits_ShouldNotBeAffected()
    {
        var panel = new ParametersPanel();
        var parms = new ShotParameters()
        {
            MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter),
            Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            ShotAngle = new Measurement<AngularUnit>(5, AngularUnit.Degree),
        };
        panel.Parameters = parms;

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Parameters;
        result.Should().NotBeNull();
        result!.ShotAngle!.Value.In(AngularUnit.Degree).Should().BeApproximately(5, 0.1);
    }

    [AvaloniaFact]
    public void Clear_ShouldResetAllFields()
    {
        var panel = new ParametersPanel();
        panel.Parameters = new ShotParameters()
        {
            MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter),
            Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            ShotAngle = new Measurement<AngularUnit>(5, AngularUnit.Degree),
        };

        panel.Clear();

        panel.Parameters.Should().BeNull();
        panel.MaxRangeControl.IsEmpty.Should().BeTrue();
        panel.StepControl.IsEmpty.Should().BeTrue();
        panel.AngleControl.IsEmpty.Should().BeTrue();
        panel.ClicksTextBox.Text.Should().BeNullOrEmpty();
    }

    [AvaloniaFact]
    public void ClicksSet_WithRiflePanel_ShouldCalculateAngle()
    {
        var panel = new ParametersPanel();
        // Set up a rifle panel with 0.25 MOA vertical click
        var riflePanel = new RiflePanel();
        riflePanel.Rifle = new Rifle(
            new Sight(
                new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA)),
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null));

        panel.RiflePanel = riflePanel;
        panel.ClicksTextBox.Text = "20";
        panel.SetAngleFromClicks();

        // 20 clicks * 0.25 MOA = 5 MOA
        var angle = panel.AngleControl.GetValue<AngularUnit>();
        angle.Should().NotBeNull();
        angle!.Value.In(AngularUnit.MOA).Should().BeApproximately(5, 0.1);
    }

    [AvaloniaFact]
    public void ClicksSet_WithoutRiflePanel_ShouldDoNothing()
    {
        var panel = new ParametersPanel();
        panel.ClicksTextBox.Text = "20";

        panel.SetAngleFromClicks();

        panel.AngleControl.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void ClicksSet_WithEmptyClicks_ShouldDoNothing()
    {
        var panel = new ParametersPanel();
        var riflePanel = new RiflePanel();
        panel.RiflePanel = riflePanel;

        panel.SetAngleFromClicks();

        panel.AngleControl.IsEmpty.Should().BeTrue();
    }

    private static object? GetSelectedUnit(BallisticCalculator.Controls.Controls.MeasurementControl control)
    {
        return (control.UnitPart?.SelectedItem as UnitItem)?.Unit;
    }

    private static ShotParameters CreateTestParameters()
    {
        return new ShotParameters()
        {
            MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter),
            Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
        };
    }
}
