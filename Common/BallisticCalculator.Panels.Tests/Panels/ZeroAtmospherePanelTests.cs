using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class ZeroAtmospherePanelTests
{
    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldBeTrue()
    {
        var panel = new ZeroAtmospherePanel();

        // Should inherit AtmospherePanel's default (true)
        panel.ConvertOnSystemChange.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new ZeroAtmospherePanel();

        panel.Should().NotBeNull();
        panel.EnableCheckBox.Should().NotBeNull();
        panel.AtmoSubPanel.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_CheckboxUnchecked()
    {
        var panel = new ZeroAtmospherePanel();

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.AtmoSubPanel.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullAtmosphere()
    {
        var panel = new ZeroAtmospherePanel();

        panel.Atmosphere.Should().BeNull();
    }

    [AvaloniaFact]
    public void Atmosphere_WhenUnchecked_ShouldReturnNull()
    {
        var panel = new ZeroAtmospherePanel();
        panel.Atmosphere = CreateTestAtmosphere();
        panel.EnableCheckBox.IsChecked = false;

        panel.Atmosphere.Should().BeNull();
    }

    [AvaloniaFact]
    public void Atmosphere_SetValue_ShouldCheckBoxAndRoundTrip()
    {
        var panel = new ZeroAtmospherePanel();

        panel.Atmosphere = CreateTestAtmosphere();

        panel.EnableCheckBox.IsChecked.Should().BeTrue();
        panel.AtmoSubPanel.IsEnabled.Should().BeTrue();

        var result = panel.Atmosphere;
        result.Should().NotBeNull();
        result!.Altitude.In(DistanceUnit.Meter).Should().BeApproximately(300, 1);
        result.Pressure.In(PressureUnit.MillimetersOfMercury).Should().BeApproximately(740, 1);
        result.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(25, 0.5);
        result.Humidity.Should().BeApproximately(0.60, 0.01);
    }

    [AvaloniaFact]
    public void Atmosphere_SetNull_ShouldUncheckAndDisable()
    {
        var panel = new ZeroAtmospherePanel();
        panel.Atmosphere = CreateTestAtmosphere();

        panel.Atmosphere = null;

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.AtmoSubPanel.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void CheckBox_WhenChecked_ShouldEnablePanel()
    {
        var panel = new ZeroAtmospherePanel();

        panel.EnableCheckBox.IsChecked = true;

        panel.AtmoSubPanel.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void CheckBox_WhenUnchecked_ShouldDisablePanel()
    {
        var panel = new ZeroAtmospherePanel();
        panel.EnableCheckBox.IsChecked = true;

        panel.EnableCheckBox.IsChecked = false;

        panel.AtmoSubPanel.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void MeasurementSystem_ShouldPropagateToSubPanel()
    {
        var panel = new ZeroAtmospherePanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.AtmoSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
    }

    [AvaloniaFact]
    public void ConvertOnSystemChange_ShouldPropagateToSubPanel()
    {
        var panel = new ZeroAtmospherePanel();

        panel.ConvertOnSystemChange = false;

        panel.AtmoSubPanel.ConvertOnSystemChange.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Clear_ShouldUncheckAndClearSubPanel()
    {
        var panel = new ZeroAtmospherePanel();
        panel.Atmosphere = CreateTestAtmosphere();

        panel.Clear();

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.AtmoSubPanel.IsEnabled.Should().BeFalse();
        panel.Atmosphere.Should().BeNull();
    }

    private static Atmosphere CreateTestAtmosphere()
    {
        return new Atmosphere(
            new Measurement<DistanceUnit>(300, DistanceUnit.Meter),
            new Measurement<PressureUnit>(740, PressureUnit.MillimetersOfMercury),
            new Measurement<TemperatureUnit>(25, TemperatureUnit.Celsius),
            0.60);
    }
}
