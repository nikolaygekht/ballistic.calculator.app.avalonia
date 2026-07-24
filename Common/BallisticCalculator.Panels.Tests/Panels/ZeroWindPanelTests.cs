using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class ZeroWindPanelTests
{
    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new ZeroWindPanel();

        panel.Should().NotBeNull();
        panel.EnableCheckBox.Should().NotBeNull();
        panel.WindSubPanel.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_CheckboxUncheckedAndSubPanelDisabled()
    {
        var panel = new ZeroWindPanel();

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.WindSubPanel.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullWind()
    {
        var panel = new ZeroWindPanel();

        panel.Wind.Should().BeNull();
    }

    [AvaloniaFact]
    public void Wind_WhenUnchecked_ShouldReturnNull()
    {
        var panel = new ZeroWindPanel();
        panel.Wind = CreateTestWind();
        panel.EnableCheckBox.IsChecked = false;

        panel.Wind.Should().BeNull();
    }

    [AvaloniaFact]
    public void Wind_SetValue_ShouldCheckBoxAndRoundTrip()
    {
        var panel = new ZeroWindPanel();

        panel.Wind = CreateTestWind();

        panel.EnableCheckBox.IsChecked.Should().BeTrue();
        panel.WindSubPanel.IsEnabled.Should().BeTrue();

        var result = panel.Wind;
        result.Should().NotBeNull();
        result!.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
        result.Direction.In(AngularUnit.Degree).Should().BeApproximately(90, 0.5);
    }

    [AvaloniaFact]
    public void Wind_SetNull_ShouldUncheckAndDisable()
    {
        var panel = new ZeroWindPanel();
        panel.Wind = CreateTestWind();

        panel.Wind = null;

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.WindSubPanel.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void CheckBox_WhenUnchecked_ShouldDisablePanel()
    {
        var panel = new ZeroWindPanel();
        panel.EnableCheckBox.IsChecked = true;

        panel.EnableCheckBox.IsChecked = false;

        panel.WindSubPanel.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void MeasurementSystem_ShouldPropagateToSubPanel()
    {
        var panel = new ZeroWindPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.WindSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
    }

    [AvaloniaFact]
    public void Clear_ShouldUncheckAndClearSubPanel()
    {
        var panel = new ZeroWindPanel();
        panel.Wind = CreateTestWind();

        panel.Clear();

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.WindSubPanel.IsEnabled.Should().BeFalse();
        panel.Wind.Should().BeNull();
    }

    private static Wind CreateTestWind()
    {
        return new Wind()
        {
            Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
        };
    }
}
