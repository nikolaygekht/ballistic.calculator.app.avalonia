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
        panel.VClicksControl.Should().NotBeNull();
        panel.HClicksControl.Should().NotBeNull();
        panel.CoriolisCheckBox.Should().NotBeNull();
        panel.AzimuthControl.Should().NotBeNull();
        panel.LatitudeControl.Should().NotBeNull();
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
        panel.VClicksControl.Value.Should().Be(0m);
        panel.HClicksControl.Value.Should().Be(0m);
        panel.CoriolisCheckBox.IsChecked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void VClicks_WithClickSize_ShouldProduceShotDropAdjustment()
    {
        var panel = new ParametersPanel { RiflePanel = CreateRiflePanelWithClicks() };
        panel.Parameters = CreateTestParameters();
        panel.VClicksControl.Value = 20m;

        var result = panel.Parameters;
        result.Should().NotBeNull();
        result!.ShotDropAdjustment.Should().NotBeNull();
        // 20 clicks * 0.25 MOA = 5 MOA
        result.ShotDropAdjustment!.Value.In(AngularUnit.MOA).Should().BeApproximately(5, 0.1);
        // Shot angle is separate and left untouched.
        result.ShotAngle.Should().BeNull();
    }

    [AvaloniaFact]
    public void HClicks_WithClickSize_ShouldProduceShotWindageAdjustment()
    {
        var panel = new ParametersPanel { RiflePanel = CreateRiflePanelWithClicks() };
        panel.Parameters = CreateTestParameters();
        panel.HClicksControl.Value = -8m;

        var result = panel.Parameters;
        result!.ShotWindageAdjustment.Should().NotBeNull();
        // -8 clicks * 0.25 MOA = -2 MOA
        result.ShotWindageAdjustment!.Value.In(AngularUnit.MOA).Should().BeApproximately(-2, 0.1);
    }

    [AvaloniaFact]
    public void Clicks_WithoutClickSize_ShouldProduceNoAdjustment()
    {
        var panel = new ParametersPanel(); // no RiflePanel -> no click size
        panel.Parameters = CreateTestParameters();
        panel.VClicksControl.Value = 20m;
        panel.HClicksControl.Value = 5m;

        var result = panel.Parameters;
        result!.ShotDropAdjustment.Should().BeNull();
        result.ShotWindageAdjustment.Should().BeNull();
    }

    [AvaloniaFact]
    public void ZeroClicks_ShouldProduceNoAdjustment()
    {
        var panel = new ParametersPanel { RiflePanel = CreateRiflePanelWithClicks() };
        panel.Parameters = CreateTestParameters(); // clicks default 0

        var result = panel.Parameters;
        result!.ShotDropAdjustment.Should().BeNull();
        result.ShotWindageAdjustment.Should().BeNull();
    }

    [AvaloniaFact]
    public void ShotDropAdjustment_ShouldRoundTripToVClicks()
    {
        var panel = new ParametersPanel { RiflePanel = CreateRiflePanelWithClicks() };
        var parms = CreateTestParameters();
        parms.ShotDropAdjustment = new Measurement<AngularUnit>(5, AngularUnit.MOA); // 5 / 0.25 = 20 clicks
        panel.Parameters = parms;

        panel.VClicksControl.Value.Should().Be(20m);
        var result = panel.Parameters;
        result!.ShotDropAdjustment!.Value.In(AngularUnit.MOA).Should().BeApproximately(5, 0.1);
    }

    [AvaloniaFact]
    public void Coriolis_WhenUnchecked_ShouldDisableAndReturnNull()
    {
        var panel = new ParametersPanel();
        panel.Parameters = CreateTestParameters();

        panel.CoriolisCheckBox.IsChecked.Should().BeFalse();
        panel.AzimuthControl.IsEnabled.Should().BeFalse();
        panel.LatitudeControl.IsEnabled.Should().BeFalse();

        var result = panel.Parameters;
        result!.BarrelAzimuth.Should().BeNull();
        result.Latitude.Should().BeNull();
    }

    [AvaloniaFact]
    public void Coriolis_Checkbox_ShouldEnableAzimuthAndLatitude()
    {
        var panel = new ParametersPanel();

        panel.CoriolisCheckBox.IsChecked = true;

        panel.AzimuthControl.IsEnabled.Should().BeTrue();
        panel.LatitudeControl.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Coriolis_WithAzimuthAndLatitude_ShouldRoundTrip()
    {
        var panel = new ParametersPanel();
        var parms = CreateTestParameters();
        parms.BarrelAzimuth = new Measurement<AngularUnit>(135, AngularUnit.Degree);
        parms.Latitude = new Measurement<AngularUnit>(45, AngularUnit.Degree);

        panel.Parameters = parms;

        panel.CoriolisCheckBox.IsChecked.Should().BeTrue();
        var result = panel.Parameters;
        result!.BarrelAzimuth.Should().NotBeNull();
        result.BarrelAzimuth!.Value.In(AngularUnit.Degree).Should().BeApproximately(135, 0.5);
        result.Latitude.Should().NotBeNull();
        result.Latitude!.Value.In(AngularUnit.Degree).Should().BeApproximately(45, 0.5);
    }

    [AvaloniaFact]
    public void Latitude_South_ShouldRoundTripAsNegative()
    {
        var panel = new ParametersPanel();
        var parms = CreateTestParameters();
        parms.Latitude = new Measurement<AngularUnit>(-33.5, AngularUnit.Degree);

        panel.Parameters = parms;

        panel.CoriolisCheckBox.IsChecked.Should().BeTrue();
        panel.LatitudeControl.GetValue<AngularUnit>()!.Value.In(AngularUnit.Degree)
            .Should().BeApproximately(33.5, 0.1);               // magnitude only
        panel.LatitudeHemisphere.SelectedIndex.Should().Be(1);  // S

        var result = panel.Parameters;
        result!.Latitude.Should().NotBeNull();
        result.Latitude!.Value.In(AngularUnit.Degree).Should().BeApproximately(-33.5, 0.1);
    }

    [AvaloniaFact]
    public void Latitude_HemisphereSelector_ShouldControlSign()
    {
        var panel = new ParametersPanel();
        panel.Parameters = CreateTestParameters(); // provides max range + step
        panel.CoriolisCheckBox.IsChecked = true;
        panel.LatitudeControl.SetValue(new Measurement<AngularUnit>(20, AngularUnit.Degree));
        panel.LatitudeHemisphere.SelectedIndex = 1; // S

        var result = panel.Parameters;
        result!.Latitude!.Value.In(AngularUnit.Degree).Should().BeApproximately(-20, 0.1);
    }

    [AvaloniaFact]
    public void Coriolis_Checkbox_ShouldEnableAzimuthDialAndHemisphere()
    {
        var panel = new ParametersPanel();

        panel.CoriolisCheckBox.IsChecked = true;

        panel.AzimuthIndicator.IsEnabled.Should().BeTrue();
        panel.LatitudeHemisphere.IsEnabled.Should().BeTrue();
    }

    private static RiflePanel CreateRiflePanelWithClicks()
    {
        var riflePanel = new RiflePanel();
        riflePanel.Rifle = new Rifle(
            new Sight(
                new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA)),
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null));
        return riflePanel;
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
