using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class AtmospherePanelTests
{
    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldBeTrue()
    {
        var panel = new AtmospherePanel();

        panel.ConvertOnSystemChange.Should().BeTrue();
    }

    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldConvertOnSwitch()
    {
        var panel = new AtmospherePanel();
        // Don't set ConvertOnSystemChange — use default (true)
        panel.Atmosphere = CreateTestAtmosphere(); // 500m, 720mmHg, 20C

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Default is true, so values should be converted to imperial units
        GetSelectedUnit(panel.AltitudeControl).Should().Be(DistanceUnit.Foot);
        GetSelectedUnit(panel.PressureControl).Should().Be(PressureUnit.InchesOfMercury);
        GetSelectedUnit(panel.TemperatureControl).Should().Be(TemperatureUnit.Fahrenheit);
        // Values should still be equivalent
        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Meter).Should().BeApproximately(500, 1);
        result.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(20, 1);
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new AtmospherePanel();

        panel.Should().NotBeNull();
        panel.AltitudeControl.Should().NotBeNull();
        panel.PressureControl.Should().NotBeNull();
        panel.TemperatureControl.Should().NotBeNull();
        panel.HumidityTextBox.Should().NotBeNull();
        panel.ResetButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullAtmosphere()
    {
        var panel = new AtmospherePanel();

        panel.Atmosphere.Should().BeNull();
    }

    [AvaloniaFact]
    public void Atmosphere_SetAndGet_ShouldRoundTrip()
    {
        var panel = new AtmospherePanel();
        var atmo = CreateTestAtmosphere();

        panel.Atmosphere = atmo;
        var result = panel.Atmosphere;

        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Meter).Should().BeApproximately(500, 1);
        result.Pressure.In(PressureUnit.MillimetersOfMercury).Should().BeApproximately(720, 1);
        result.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(20, 0.5);
        result.Humidity.Should().BeApproximately(0.65, 0.01);
    }

    [AvaloniaFact]
    public void Atmosphere_SetNull_ShouldClear()
    {
        var panel = new AtmospherePanel();
        panel.Atmosphere = CreateTestAtmosphere();

        panel.Atmosphere = null;

        panel.Atmosphere.Should().BeNull();
        panel.AltitudeControl.IsEmpty.Should().BeTrue();
        panel.PressureControl.IsEmpty.Should().BeTrue();
        panel.TemperatureControl.IsEmpty.Should().BeTrue();
        panel.HumidityTextBox.Text.Should().BeNullOrEmpty();
    }

    [AvaloniaFact]
    public void Atmosphere_SetImperialValues_ShouldRoundTrip()
    {
        var panel = new AtmospherePanel();
        var atmo = new Atmosphere(
            new Measurement<DistanceUnit>(1000, DistanceUnit.Foot),
            new Measurement<PressureUnit>(29.5, PressureUnit.InchesOfMercury),
            new Measurement<TemperatureUnit>(68, TemperatureUnit.Fahrenheit),
            0.50);

        panel.Atmosphere = atmo;
        var result = panel.Atmosphere;

        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Foot).Should().BeApproximately(1000, 1);
        result.Pressure.In(PressureUnit.InchesOfMercury).Should().BeApproximately(29.5, 0.1);
        result.Temperature.In(TemperatureUnit.Fahrenheit).Should().BeApproximately(68, 1);
        result.Humidity.Should().BeApproximately(0.50, 0.01);
    }

    [AvaloniaFact]
    public void Humidity_ShouldConvertBetweenPercentAndDecimal()
    {
        // Humidity in Atmosphere is 0-1, but displayed as 0-100 percentage
        var panel = new AtmospherePanel();
        var atmo = new Atmosphere(
            new Measurement<DistanceUnit>(0, DistanceUnit.Meter),
            new Measurement<PressureUnit>(760, PressureUnit.MillimetersOfMercury),
            new Measurement<TemperatureUnit>(15, TemperatureUnit.Celsius),
            0.78);

        panel.Atmosphere = atmo;

        // Display should show percentage
        panel.HumidityTextBox.Text.Should().Be("78");

        // Getting back should return decimal
        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        result!.Humidity.Should().BeApproximately(0.78, 0.01);
    }

    [AvaloniaFact]
    public void Reset_ShouldSetMetricDefaults()
    {
        var panel = new AtmospherePanel();
        // Set some values first
        panel.Atmosphere = CreateTestAtmosphere();

        panel.Reset();

        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Meter).Should().BeApproximately(0, 0.1);
        result.Pressure.In(PressureUnit.MillimetersOfMercury).Should().BeApproximately(760, 0.1);
        result.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(15, 0.1);
        result.Humidity.Should().BeApproximately(0.78, 0.01);
    }

    [AvaloniaFact]
    public void Reset_Imperial_ShouldSetImperialDefaults()
    {
        var panel = new AtmospherePanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.Reset();

        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Foot).Should().BeApproximately(0, 0.1);
        result.Pressure.In(PressureUnit.InchesOfMercury).Should().BeApproximately(29.92, 0.1);
        result.Temperature.In(TemperatureUnit.Fahrenheit).Should().BeApproximately(59, 0.5);
        result.Humidity.Should().BeApproximately(0.78, 0.01);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperial_WithConvert_ShouldPreserveValues()
    {
        var panel = new AtmospherePanel();
        panel.ConvertOnSystemChange = true;
        panel.Atmosphere = CreateTestAtmosphere();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Meter).Should().BeApproximately(500, 1);
        result.Pressure.In(PressureUnit.MillimetersOfMercury).Should().BeApproximately(720, 1);
        result.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(20, 1);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetric_WithConvert_ShouldPreserveValues()
    {
        var panel = new AtmospherePanel();
        panel.ConvertOnSystemChange = true;
        panel.MeasurementSystem = MeasurementSystem.Imperial;
        panel.Atmosphere = new Atmosphere(
            new Measurement<DistanceUnit>(1000, DistanceUnit.Foot),
            new Measurement<PressureUnit>(29.5, PressureUnit.InchesOfMercury),
            new Measurement<TemperatureUnit>(68, TemperatureUnit.Fahrenheit),
            0.50);

        panel.MeasurementSystem = MeasurementSystem.Metric;

        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Foot).Should().BeApproximately(1000, 2);
        result.Pressure.In(PressureUnit.InchesOfMercury).Should().BeApproximately(29.5, 0.1);
        result.Temperature.In(TemperatureUnit.Fahrenheit).Should().BeApproximately(68, 1);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchWithoutConvert_ShouldLeaveValuesUntouched()
    {
        var panel = new AtmospherePanel();
        panel.ConvertOnSystemChange = false;
        panel.Atmosphere = CreateTestAtmosphere();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        // Values stay in original units
        result!.Altitude.In(DistanceUnit.Meter).Should().BeApproximately(500, 1);
        GetSelectedUnit(panel.AltitudeControl).Should().Be(DistanceUnit.Meter);
        GetSelectedUnit(panel.PressureControl).Should().Be(PressureUnit.MillimetersOfMercury);
        GetSelectedUnit(panel.TemperatureControl).Should().Be(TemperatureUnit.Celsius);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ShouldChangeUnits()
    {
        var panel = new AtmospherePanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.AltitudeControl).Should().Be(DistanceUnit.Foot);
        GetSelectedUnit(panel.PressureControl).Should().Be(PressureUnit.InchesOfMercury);
        GetSelectedUnit(panel.TemperatureControl).Should().Be(TemperatureUnit.Fahrenheit);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetricWhenEmpty_ShouldChangeUnits()
    {
        var panel = new AtmospherePanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        GetSelectedUnit(panel.AltitudeControl).Should().Be(DistanceUnit.Meter);
        GetSelectedUnit(panel.PressureControl).Should().Be(PressureUnit.MillimetersOfMercury);
        GetSelectedUnit(panel.TemperatureControl).Should().Be(TemperatureUnit.Celsius);
    }

    [AvaloniaFact]
    public void Clear_ShouldResetAllFields()
    {
        var panel = new AtmospherePanel();
        panel.Atmosphere = CreateTestAtmosphere();

        panel.Clear();

        panel.Atmosphere.Should().BeNull();
        panel.AltitudeControl.IsEmpty.Should().BeTrue();
        panel.PressureControl.IsEmpty.Should().BeTrue();
        panel.TemperatureControl.IsEmpty.Should().BeTrue();
        panel.HumidityTextBox.Text.Should().BeNullOrEmpty();
    }

    [AvaloniaFact]
    public void Humidity_EmptyString_ShouldReturnNullAtmosphere()
    {
        var panel = new AtmospherePanel();
        // Set all measurement controls but leave humidity empty
        panel.AltitudeControl.SetValue(new Measurement<DistanceUnit>(0, DistanceUnit.Meter));
        panel.PressureControl.SetValue(new Measurement<PressureUnit>(760, PressureUnit.MillimetersOfMercury));
        panel.TemperatureControl.SetValue(new Measurement<TemperatureUnit>(15, TemperatureUnit.Celsius));
        // HumidityTextBox is empty

        panel.Atmosphere.Should().BeNull();
    }

    [AvaloniaFact]
    public void Humidity_ZeroValue_ShouldBeValid()
    {
        var panel = new AtmospherePanel();
        var atmo = new Atmosphere(
            new Measurement<DistanceUnit>(0, DistanceUnit.Meter),
            new Measurement<PressureUnit>(760, PressureUnit.MillimetersOfMercury),
            new Measurement<TemperatureUnit>(15, TemperatureUnit.Celsius),
            0.0);

        panel.Atmosphere = atmo;
        var result = panel.Atmosphere;

        result.Should().NotBeNull();
        result!.Humidity.Should().BeApproximately(0.0, 0.01);
        panel.HumidityTextBox.Text.Should().Be("0");
    }

    private static object? GetSelectedUnit(BallisticCalculator.Controls.Controls.MeasurementControl control)
    {
        return (control.UnitPart?.SelectedItem as UnitItem)?.Unit;
    }

    private static Atmosphere CreateTestAtmosphere()
    {
        return new Atmosphere(
            new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            new Measurement<PressureUnit>(720, PressureUnit.MillimetersOfMercury),
            new Measurement<TemperatureUnit>(20, TemperatureUnit.Celsius),
            0.65);
    }
}
