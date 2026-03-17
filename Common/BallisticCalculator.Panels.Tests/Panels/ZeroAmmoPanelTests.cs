using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class ZeroAmmoPanelTests
{
    [AvaloniaFact]
    public void ConvertOnSystemChange_Default_ShouldBeFalse()
    {
        var panel = new ZeroAmmoPanel();

        panel.ConvertOnSystemChange.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new ZeroAmmoPanel();

        panel.Should().NotBeNull();
        panel.EnableCheckBox.Should().NotBeNull();
        panel.AmmoSubPanel.Should().NotBeNull();
        panel.LoadButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_CheckboxUnchecked()
    {
        var panel = new ZeroAmmoPanel();

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.AmmoSubPanel.IsEnabled.Should().BeFalse();
        panel.LoadButton.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullAmmunition()
    {
        var panel = new ZeroAmmoPanel();

        panel.Ammunition.Should().BeNull();
    }

    [AvaloniaFact]
    public void Ammunition_WhenUnchecked_ShouldReturnNull()
    {
        var panel = new ZeroAmmoPanel();
        // Set data then uncheck
        panel.Ammunition = CreateTestAmmo();
        panel.EnableCheckBox.IsChecked = false;

        panel.Ammunition.Should().BeNull();
    }

    [AvaloniaFact]
    public void Ammunition_SetValue_ShouldCheckBoxAndRoundTrip()
    {
        var panel = new ZeroAmmoPanel();

        panel.Ammunition = CreateTestAmmo();

        panel.EnableCheckBox.IsChecked.Should().BeTrue();
        panel.AmmoSubPanel.IsEnabled.Should().BeTrue();

        var result = panel.Ammunition;
        result.Should().NotBeNull();
        result!.Weight.In(WeightUnit.Grain).Should().BeApproximately(150, 0.5);
        result.MuzzleVelocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(2700, 1);
    }

    [AvaloniaFact]
    public void Ammunition_SetNull_ShouldUncheckAndDisable()
    {
        var panel = new ZeroAmmoPanel();
        panel.Ammunition = CreateTestAmmo();

        panel.Ammunition = null;

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.AmmoSubPanel.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void CheckBox_WhenChecked_ShouldEnablePanelAndLoadButton()
    {
        var panel = new ZeroAmmoPanel();

        panel.EnableCheckBox.IsChecked = true;

        panel.AmmoSubPanel.IsEnabled.Should().BeTrue();
        panel.LoadButton.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void CheckBox_WhenUnchecked_ShouldDisablePanelAndLoadButton()
    {
        var panel = new ZeroAmmoPanel();
        panel.EnableCheckBox.IsChecked = true;

        panel.EnableCheckBox.IsChecked = false;

        panel.AmmoSubPanel.IsEnabled.Should().BeFalse();
        panel.LoadButton.IsEnabled.Should().BeFalse();
    }

    [AvaloniaFact]
    public void MeasurementSystem_ShouldPropagateToSubPanel()
    {
        var panel = new ZeroAmmoPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.AmmoSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
    }

    [AvaloniaFact]
    public void ConvertOnSystemChange_ShouldPropagateToSubPanel()
    {
        var panel = new ZeroAmmoPanel();

        panel.ConvertOnSystemChange = true;

        panel.AmmoSubPanel.ConvertOnSystemChange.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Clear_ShouldUncheckAndClearSubPanel()
    {
        var panel = new ZeroAmmoPanel();
        panel.Ammunition = CreateTestAmmo();

        panel.Clear();

        panel.EnableCheckBox.IsChecked.Should().BeFalse();
        panel.AmmoSubPanel.IsEnabled.Should().BeFalse();
        panel.Ammunition.Should().BeNull();
    }

    private static Ammunition CreateTestAmmo()
    {
        return new Ammunition()
        {
            Weight = new Measurement<WeightUnit>(150, WeightUnit.Grain),
            BallisticCoefficient = new BallisticCoefficient(0.415, DragTableId.G1),
            MuzzleVelocity = new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
        };
    }
}
