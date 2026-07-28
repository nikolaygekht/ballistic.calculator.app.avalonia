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
    #region The zeroing wind has no zones

    /// <summary>
    /// Zeroing is one wind, deliberately: it happens at a short, controlled distance, where splitting the
    /// air into zones would be nonsense. So the start distance is pinned at the muzzle and not offered for
    /// editing — a lone wind's range is ignored by the engine anyway, which would make any other value a
    /// field that silently does nothing.
    /// </summary>
    [AvaloniaFact]
    public void StartDistance_IsPinnedToTheMuzzleAndNotEditable()
    {
        // Arrange & Act
        var panel = new ZeroWindPanel();

        // Assert
        panel.WindSubPanel.MaxDistanceCheckBox.IsChecked.Should().BeTrue();
        panel.WindSubPanel.MaxDistanceCheckBox.IsEnabled.Should().BeFalse();
        panel.WindSubPanel.MaxDistanceControl.IsEnabled.Should().BeFalse();
        panel.WindSubPanel.MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().Be(0);
    }

    [AvaloniaFact]
    public void Wind_SetWithAStartDistance_IsStillShownFromTheMuzzle()
    {
        // Arrange — an older file, or a hand-edited one, carrying a range on the zeroing wind
        var panel = new ZeroWindPanel();

        // Act
        panel.Wind = new Wind
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            MaximumRange = new Measurement<DistanceUnit>(250, DistanceUnit.Meter),
        };

        // Assert — the wind is taken, the range is not
        panel.WindSubPanel.MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().Be(0);
        panel.Wind!.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
        panel.Wind!.MaximumRange!.Value.Value.Should().Be(0, "the zeroing wind blows for the whole zeroing shot");
    }

    [AvaloniaFact]
    public void Clear_LeavesTheStartDistancePinnedToTheMuzzle()
    {
        // Arrange
        var panel = new ZeroWindPanel();
        panel.Wind = new Wind
        {
            Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
        };

        // Act
        panel.Clear();

        // Assert
        panel.WindSubPanel.MaxDistanceCheckBox.IsChecked.Should().BeTrue();
        panel.WindSubPanel.MaxDistanceControl.GetValue<DistanceUnit>()!.Value.Value.Should().Be(0);
    }

    #endregion

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
