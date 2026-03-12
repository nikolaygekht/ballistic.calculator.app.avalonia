using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;

namespace BallisticCalculator.Panels.Tests.Panels;

public class AmmoPanelTests
{
    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new AmmoPanel();

        panel.Should().NotBeNull();
        panel.WeightControl.Should().NotBeNull();
        panel.BCControl.Should().NotBeNull();
        panel.FormFactorCheckBox.Should().NotBeNull();
        panel.MuzzleVelocityControl.Should().NotBeNull();
        panel.BulletDiameterControl.Should().NotBeNull();
        panel.BulletLengthControl.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullAmmunition()
    {
        var panel = new AmmoPanel();

        panel.Ammunition.Should().BeNull();
    }

    [AvaloniaFact]
    public void Ammunition_SetAndGet_ShouldRoundTrip()
    {
        var panel = new AmmoPanel();
        var ammo = CreateTestAmmunition();

        panel.Ammunition = ammo;
        var result = panel.Ammunition;

        result.Should().NotBeNull();
        result!.Weight.In(WeightUnit.Grain).Should().BeApproximately(168, 0.5);
        result.MuzzleVelocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(2650, 0.5);
        result.BallisticCoefficient.Value.Should().BeApproximately(0.462, 0.001);
        result.BallisticCoefficient.Table.Should().Be(DragTableId.G1);
    }

    [AvaloniaFact]
    public void Ammunition_SetWithOptionalFields_ShouldRoundTrip()
    {
        var panel = new AmmoPanel();
        var ammo = CreateTestAmmunition();
        ammo.BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch);
        ammo.BulletLength = new Measurement<DistanceUnit>(1.235, DistanceUnit.Inch);

        panel.Ammunition = ammo;
        var result = panel.Ammunition;

        result.Should().NotBeNull();
        result!.BulletDiameter.Should().NotBeNull();
        result.BulletDiameter!.Value.In(DistanceUnit.Inch).Should().BeApproximately(0.308, 0.001);
        result.BulletLength.Should().NotBeNull();
        result.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.235, 0.001);
    }

    [AvaloniaFact]
    public void Ammunition_SetWithoutOptionalFields_ShouldReturnNullForOptional()
    {
        var panel = new AmmoPanel();
        var ammo = CreateTestAmmunition();

        panel.Ammunition = ammo;
        var result = panel.Ammunition;

        result.Should().NotBeNull();
        result!.BulletDiameter.Should().BeNull();
        result.BulletLength.Should().BeNull();
    }

    [AvaloniaFact]
    public void Ammunition_SetNull_ShouldClear()
    {
        var panel = new AmmoPanel();
        panel.Ammunition = CreateTestAmmunition();

        panel.Ammunition = null;

        panel.Ammunition.Should().BeNull();
        panel.WeightControl.IsEmpty.Should().BeTrue();
        panel.BCControl.IsEmpty.Should().BeTrue();
        panel.MuzzleVelocityControl.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void FormFactor_WhenChecked_ShouldSetFormFactorType()
    {
        var panel = new AmmoPanel();
        var ammo = CreateTestAmmunition();
        ammo.BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch);

        panel.Ammunition = ammo;
        panel.FormFactorCheckBox.IsChecked = true;

        var result = panel.Ammunition;
        result.Should().NotBeNull();
        result!.BallisticCoefficient.ValueType.Should().Be(BallisticCoefficientValueType.FormFactor);
    }

    [AvaloniaFact]
    public void FormFactor_SetFromAmmunition_ShouldCheckBox()
    {
        var panel = new AmmoPanel();
        var ammo = new Ammunition()
        {
            Weight = new Measurement<WeightUnit>(168, WeightUnit.Grain),
            BallisticCoefficient = new BallisticCoefficient(1.05, DragTableId.G7, BallisticCoefficientValueType.FormFactor),
            MuzzleVelocity = new Measurement<VelocityUnit>(2650, VelocityUnit.FeetPerSecond),
            BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
        };

        panel.Ammunition = ammo;

        panel.FormFactorCheckBox.IsChecked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperial_WithConvert_ShouldPreserveValues()
    {
        var panel = new AmmoPanel();
        panel.ConvertOnSystemChange = true;
        var ammo = CreateTestAmmunition();
        panel.Ammunition = ammo;

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.Ammunition;
        result.Should().NotBeNull();
        result!.Weight.In(WeightUnit.Grain).Should().BeApproximately(168, 0.5);
        result.MuzzleVelocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(2650, 1);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetric_WithConvert_ShouldPreserveValues()
    {
        var panel = new AmmoPanel();
        panel.ConvertOnSystemChange = true;
        panel.MeasurementSystem = MeasurementSystem.Imperial;
        var ammo = CreateTestAmmunition();
        panel.Ammunition = ammo;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        var result = panel.Ammunition;
        result.Should().NotBeNull();
        result!.Weight.In(WeightUnit.Grain).Should().BeApproximately(168, 0.5);
        result.MuzzleVelocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(2650, 1);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchWithoutConvert_ShouldLeaveValuesUntouched()
    {
        var panel = new AmmoPanel();
        // Default: ConvertOnSystemChange = false
        var ammo = CreateTestAmmunition(); // 168 grain, 2650 fps
        panel.Ammunition = ammo;
        // In metric mode, weight displays as ~10.89 gram, velocity as ~807.7 m/s

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // With flag=false, filled controls are not touched at all (value AND unit stay)
        var result = panel.Ammunition;
        result.Should().NotBeNull();
        result!.Weight.In(WeightUnit.Gram).Should().BeApproximately(
            new Measurement<WeightUnit>(168, WeightUnit.Grain).In(WeightUnit.Gram), 0.5);
        GetSelectedUnit(panel.WeightControl).Should().Be(WeightUnit.Gram);
        GetSelectedUnit(panel.MuzzleVelocityControl).Should().Be(VelocityUnit.MetersPerSecond);
    }

    // Note: Changed event tests removed - Avalonia Headless doesn't fire TextChanged
    // when TextBox.Text is set programmatically. The Changed event works in real UI.

    [AvaloniaFact]
    public void Clear_ShouldResetAllFields()
    {
        var panel = new AmmoPanel();
        var ammo = CreateTestAmmunition();
        ammo.BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch);
        panel.Ammunition = ammo;
        panel.FormFactorCheckBox.IsChecked = true;

        panel.Clear();

        panel.Ammunition.Should().BeNull();
        panel.FormFactorCheckBox.IsChecked.Should().BeFalse();
        panel.BulletDiameterControl.IsEmpty.Should().BeTrue();
    }

    // Bug fix tests: unit switching when empty
    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ShouldChangeUnits()
    {
        var panel = new AmmoPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Unit dropdowns should show Imperial units even though controls are empty
        GetSelectedUnit(panel.WeightControl).Should().Be(WeightUnit.Grain);
        GetSelectedUnit(panel.MuzzleVelocityControl).Should().Be(VelocityUnit.FeetPerSecond);
        GetSelectedUnit(panel.BulletDiameterControl).Should().Be(DistanceUnit.Inch);
        GetSelectedUnit(panel.BulletLengthControl).Should().Be(DistanceUnit.Inch);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetricWhenEmpty_ShouldChangeUnits()
    {
        var panel = new AmmoPanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        GetSelectedUnit(panel.WeightControl).Should().Be(WeightUnit.Gram);
        GetSelectedUnit(panel.MuzzleVelocityControl).Should().Be(VelocityUnit.MetersPerSecond);
        GetSelectedUnit(panel.BulletDiameterControl).Should().Be(DistanceUnit.Millimeter);
        GetSelectedUnit(panel.BulletLengthControl).Should().Be(DistanceUnit.Millimeter);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWithData_AndConvert_ShouldChangeUnits()
    {
        var panel = new AmmoPanel();
        panel.ConvertOnSystemChange = true;
        panel.Ammunition = CreateTestAmmunition();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // Units in dropdowns should be Imperial
        GetSelectedUnit(panel.WeightControl).Should().Be(WeightUnit.Grain);
        GetSelectedUnit(panel.MuzzleVelocityControl).Should().Be(VelocityUnit.FeetPerSecond);
    }

    private static object? GetSelectedUnit(BallisticCalculator.Controls.Controls.MeasurementControl control)
    {
        return (control.UnitPart?.SelectedItem as UnitItem)?.Unit;
    }

    private static Ammunition CreateTestAmmunition()
    {
        return new Ammunition()
        {
            Weight = new Measurement<WeightUnit>(168, WeightUnit.Grain),
            BallisticCoefficient = new BallisticCoefficient(0.462, DragTableId.G1),
            MuzzleVelocity = new Measurement<VelocityUnit>(2650, VelocityUnit.FeetPerSecond),
        };
    }
}
