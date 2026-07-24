using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class ShotDataPanelTests
{
    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new ShotDataPanel();

        panel.Should().NotBeNull();
        panel.AmmoLibPanel.Should().NotBeNull();
        panel.AtmosphereSubPanel.Should().NotBeNull();
        panel.WindSubPanel.Should().NotBeNull();
        panel.RifleSubPanel.Should().NotBeNull();
        panel.ZeroAmmoSubPanel.Should().NotBeNull();
        panel.ZeroAtmosphereSubPanel.Should().NotBeNull();
        panel.ParametersSubPanel.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullShotData()
    {
        var panel = new ShotDataPanel();

        panel.ShotData.Should().BeNull();
    }

    [AvaloniaFact]
    public void ShotData_SetAndGet_MinimalData_ShouldRoundTrip()
    {
        var panel = new ShotDataPanel();
        var data = CreateMinimalShotData();

        panel.ShotData = data;
        var result = panel.ShotData;

        result.Should().NotBeNull();
        result!.Ammunition.Should().NotBeNull();
        result.Ammunition!.Name.Should().Be("Test Ammo");
        result.Weapon.Should().NotBeNull();
        result.Weapon!.Sight.SightHeight.In(DistanceUnit.Millimeter).Should().BeApproximately(50, 0.5);
        result.Atmosphere.Should().NotBeNull();
        result.Atmosphere!.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(15, 0.5);
        result.Parameters.Should().NotBeNull();
        result.Parameters!.MaximumDistance.In(DistanceUnit.Meter).Should().BeApproximately(1000, 1);
    }

    [AvaloniaFact]
    public void ShotData_SetAndGet_WithWinds_ShouldRoundTrip()
    {
        var panel = new ShotDataPanel();
        var data = CreateMinimalShotData();
        data.Winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
            },
        };

        panel.ShotData = data;
        var result = panel.ShotData;

        result.Should().NotBeNull();
        result!.Winds.Should().NotBeNull();
        result.Winds!.Length.Should().Be(1);
        result.Winds[0].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(5, 0.5);
    }

    [AvaloniaFact]
    public void ShotData_SetAndGet_WithZeroAmmo_ShouldRoundTrip()
    {
        var panel = new ShotDataPanel();
        var data = CreateMinimalShotData();
        data.Weapon!.Zero.Ammunition = new Ammunition()
        {
            Weight = new Measurement<WeightUnit>(150, WeightUnit.Grain),
            BallisticCoefficient = new BallisticCoefficient(0.415, DragTableId.G1),
            MuzzleVelocity = new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond),
        };

        panel.ShotData = data;
        var result = panel.ShotData;

        result.Should().NotBeNull();
        result!.Zeroing!.Ammunition.Should().NotBeNull();
        result.Zeroing.Ammunition!.Weight.In(WeightUnit.Grain).Should().BeApproximately(150, 0.5);
    }

    [AvaloniaFact]
    public void ShotData_SetAndGet_WithZeroAtmosphere_ShouldRoundTrip()
    {
        var panel = new ShotDataPanel();
        var data = CreateMinimalShotData();
        data.Weapon!.Zero.Atmosphere = new Atmosphere(
            new Measurement<DistanceUnit>(300, DistanceUnit.Meter),
            new Measurement<PressureUnit>(740, PressureUnit.MillimetersOfMercury),
            new Measurement<TemperatureUnit>(25, TemperatureUnit.Celsius),
            0.60);

        panel.ShotData = data;
        var result = panel.ShotData;

        result.Should().NotBeNull();
        result!.Zeroing!.Atmosphere.Should().NotBeNull();
        result.Zeroing.Atmosphere!.Temperature.In(TemperatureUnit.Celsius).Should().BeApproximately(25, 0.5);
    }

    [AvaloniaFact]
    public void ShotData_SetAndGet_WithFullZeroing_ShouldRoundTrip()
    {
        var panel = new ShotDataPanel();
        var data = CreateMinimalShotData();
        // Offsets are entered on the Rifle panel and carried in Weapon.Zero.
        data.Weapon!.Zero.VerticalOffset = new Measurement<DistanceUnit>(20, DistanceUnit.Millimeter);
        data.Weapon.Zero.HorizontalOffset = new Measurement<DistanceUnit>(10, DistanceUnit.Millimeter);
        data.Zeroing = new ZeroingData
        {
            Distance = data.Weapon.Zero.Distance,
            VerticalOffset = data.Weapon.Zero.VerticalOffset,
            HorizontalOffset = data.Weapon.Zero.HorizontalOffset,
            Wind = new Wind()
            {
                Velocity = new Measurement<VelocityUnit>(4, VelocityUnit.MetersPerSecond),
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
            },
            ShotAngle = new Measurement<AngularUnit>(2, AngularUnit.Mil),
        };

        panel.ShotData = data;
        var result = panel.ShotData;

        result.Should().NotBeNull();
        result!.Zeroing.Should().NotBeNull();
        result.Zeroing!.HorizontalOffset!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(10, 0.5);
        result.Zeroing.VerticalOffset!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(20, 0.5);
        result.Zeroing.Wind.Should().NotBeNull();
        result.Zeroing.Wind!.Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(4, 0.5);
        result.Zeroing.ShotAngle.Should().NotBeNull();
        result.Zeroing.ShotAngle!.Value.In(AngularUnit.Mil).Should().BeApproximately(2, 0.05);
        // Weapon carries sight + rifling, not the zeroing wind / shot angle.
        result.Weapon.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void ShotData_SetNull_ShouldClearAll()
    {
        var panel = new ShotDataPanel();
        panel.ShotData = CreateMinimalShotData();

        panel.ShotData = null;

        panel.ShotData.Should().BeNull();
        panel.AmmoLibPanel.LibraryEntry.Should().BeNull();
        panel.AtmosphereSubPanel.Atmosphere.Should().BeNull();
        panel.RifleSubPanel.Rifle.Should().BeNull();
        panel.ParametersSubPanel.Parameters.Should().BeNull();
    }

    [AvaloniaFact]
    public void MeasurementSystem_ShouldPropagateToAllChildren()
    {
        var panel = new ShotDataPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.AmmoLibPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
        panel.AtmosphereSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
        panel.WindSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
        panel.RifleSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
        panel.ZeroAmmoSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
        panel.ZeroAtmosphereSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
        panel.ParametersSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Imperial);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchBackToMetric_ShouldPropagate()
    {
        var panel = new ShotDataPanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        panel.AmmoLibPanel.MeasurementSystem.Should().Be(MeasurementSystem.Metric);
        panel.AtmosphereSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Metric);
        panel.RifleSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Metric);
        panel.ParametersSubPanel.MeasurementSystem.Should().Be(MeasurementSystem.Metric);
    }

    [AvaloniaFact]
    public void ParametersPanel_ShouldBeWiredToRiflePanel()
    {
        var panel = new ShotDataPanel();

        panel.ParametersSubPanel.RiflePanel.Should().BeSameAs(panel.RifleSubPanel);
    }

    [AvaloniaFact]
    public void ShotData_WithNoZeroAmmo_ShouldReturnNullZeroAmmo()
    {
        var panel = new ShotDataPanel();
        var data = CreateMinimalShotData();

        panel.ShotData = data;
        var result = panel.ShotData;

        result.Should().NotBeNull();
        result!.Weapon!.Zero.Ammunition.Should().BeNull();
    }

    [AvaloniaFact]
    public void ShotData_WithNoWinds_ShouldReturnNullWinds()
    {
        var panel = new ShotDataPanel();
        var data = CreateMinimalShotData();

        panel.ShotData = data;
        var result = panel.ShotData;

        result.Should().NotBeNull();
        result!.Winds.Should().BeNull();
    }

    [AvaloniaFact]
    public void Clear_ShouldResetEverything()
    {
        var panel = new ShotDataPanel();
        panel.ShotData = CreateMinimalShotData();

        panel.Clear();

        panel.ShotData.Should().BeNull();
    }

    private static ShotData CreateMinimalShotData()
    {
        return new ShotData()
        {
            Ammunition = new AmmunitionLibraryEntry()
            {
                Name = "Test Ammo",
                Ammunition = new Ammunition()
                {
                    Weight = new Measurement<WeightUnit>(168, WeightUnit.Grain),
                    BallisticCoefficient = new BallisticCoefficient(0.462, DragTableId.G1),
                    MuzzleVelocity = new Measurement<VelocityUnit>(2650, VelocityUnit.FeetPerSecond),
                },
            },
            Weapon = new Rifle(
                new Sight(
                    new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                    new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                    new Measurement<AngularUnit>(0.25, AngularUnit.MOA)),
                new ZeroingParameters(
                    new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                    null, null)),
            Atmosphere = new Atmosphere(
                new Measurement<DistanceUnit>(0, DistanceUnit.Meter),
                new Measurement<PressureUnit>(760, PressureUnit.MillimetersOfMercury),
                new Measurement<TemperatureUnit>(15, TemperatureUnit.Celsius),
                0.78),
            Parameters = new ShotParameters()
            {
                MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter),
                Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            },
        };
    }
}
