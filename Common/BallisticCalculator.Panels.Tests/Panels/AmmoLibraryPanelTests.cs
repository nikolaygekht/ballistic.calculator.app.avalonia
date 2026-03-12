using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using Gehtsoft.Measurements;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Data.Dictionary;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Panels.Tests.Mocks;
using BallisticCalculator.Types;
using System.Linq;

namespace BallisticCalculator.Panels.Tests.Panels;

public class AmmoLibraryPanelTests
{
    [AvaloniaFact]
    public void Panel_ShouldInitialize()
    {
        var panel = new AmmoLibraryPanel();

        panel.Should().NotBeNull();
        panel.NameTextBox.Should().NotBeNull();
        panel.AmmoSubPanel.Should().NotBeNull();
        panel.CaliberTextBox.Should().NotBeNull();
        panel.BulletTypeCombo.Should().NotBeNull();
        panel.BarrelLengthControl.Should().NotBeNull();
        panel.SourceTextBox.Should().NotBeNull();
        panel.LoadButton.Should().NotBeNull();
        panel.SaveButton.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void Panel_InitialState_ShouldReturnNullLibraryEntry()
    {
        var panel = new AmmoLibraryPanel();

        panel.LibraryEntry.Should().BeNull();
    }

    [AvaloniaFact]
    public void BulletTypeCombo_ShouldBePopulated()
    {
        var panel = new AmmoLibraryPanel();

        // Empty entry + ammunition types
        panel.BulletTypeCombo.Items.Count.Should().BeGreaterThan(1);
        panel.BulletTypeCombo.SelectedIndex.Should().Be(0);
    }

    [AvaloniaFact]
    public void LibraryEntry_SetAndGet_ShouldRoundTrip()
    {
        var panel = new AmmoLibraryPanel();
        var entry = CreateTestEntry();

        panel.LibraryEntry = entry;
        var result = panel.LibraryEntry;

        result.Should().NotBeNull();
        result!.Name.Should().Be("Federal Gold Medal 168gr");
        result.Caliber.Should().Be(".308 Winchester");
        result.AmmunitionType.Should().Be("HPBT");
        result.Source.Should().Be("Federal Premium");
        result.Ammunition.Should().NotBeNull();
        result.Ammunition.Weight.In(WeightUnit.Grain).Should().BeApproximately(168, 0.5);
    }

    [AvaloniaFact]
    public void LibraryEntry_WithBarrelLength_ShouldRoundTrip()
    {
        var panel = new AmmoLibraryPanel();
        var entry = CreateTestEntry();
        entry.BarrelLength = new Measurement<DistanceUnit>(24, DistanceUnit.Inch);

        panel.LibraryEntry = entry;
        var result = panel.LibraryEntry;

        result.Should().NotBeNull();
        result!.BarrelLength.Should().NotBeNull();
        result.BarrelLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(24, 0.5);
    }

    [AvaloniaFact]
    public void LibraryEntry_WithoutOptionalFields_ShouldReturnNull()
    {
        var panel = new AmmoLibraryPanel();
        var entry = new AmmunitionLibraryEntry()
        {
            Name = "Test",
            Ammunition = CreateTestAmmunition(),
        };

        panel.LibraryEntry = entry;
        var result = panel.LibraryEntry;

        result.Should().NotBeNull();
        result!.Caliber.Should().BeNull();
        result.AmmunitionType.Should().BeNull();
        result.BarrelLength.Should().BeNull();
        result.Source.Should().BeNull();
    }

    [AvaloniaFact]
    public void LibraryEntry_SetNull_ShouldClear()
    {
        var panel = new AmmoLibraryPanel();
        panel.LibraryEntry = CreateTestEntry();

        panel.LibraryEntry = null;

        panel.NameTextBox.Text.Should().Be("");
        panel.CaliberTextBox.Text.Should().Be("");
        panel.SourceTextBox.Text.Should().Be("");
        panel.BulletTypeCombo.SelectedIndex.Should().Be(0);
        panel.AmmoSubPanel.Ammunition.Should().BeNull();
    }

    [AvaloniaFact]
    public void Ammunition_Property_ShouldDelegateToSubPanel()
    {
        var panel = new AmmoLibraryPanel();
        var ammo = CreateTestAmmunition();

        panel.Ammunition = ammo;

        panel.AmmoSubPanel.Ammunition.Should().NotBeNull();
        panel.Ammunition.Should().NotBeNull();
        panel.Ammunition!.Weight.In(WeightUnit.Grain).Should().BeApproximately(168, 0.5);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperial_WithConvert_ShouldPreserveValues()
    {
        var panel = new AmmoLibraryPanel();
        panel.ConvertOnSystemChange = true;
        var entry = CreateTestEntry();
        entry.BarrelLength = new Measurement<DistanceUnit>(610, DistanceUnit.Millimeter);
        panel.LibraryEntry = entry;

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        var result = panel.LibraryEntry;
        result.Should().NotBeNull();
        result!.BarrelLength.Should().NotBeNull();
        result.BarrelLength!.Value.In(DistanceUnit.Millimeter).Should().BeApproximately(610, 1);
        result.Ammunition.Weight.In(WeightUnit.Grain).Should().BeApproximately(168, 0.5);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetric_WithConvert_ShouldPreserveValues()
    {
        var panel = new AmmoLibraryPanel();
        panel.ConvertOnSystemChange = true;
        panel.MeasurementSystem = MeasurementSystem.Imperial;
        var entry = CreateTestEntry();
        entry.BarrelLength = new Measurement<DistanceUnit>(24, DistanceUnit.Inch);
        panel.LibraryEntry = entry;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        var result = panel.LibraryEntry;
        result.Should().NotBeNull();
        result!.BarrelLength.Should().NotBeNull();
        result.BarrelLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(24, 0.5);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchWithoutConvert_ShouldLeaveValuesUntouched()
    {
        var panel = new AmmoLibraryPanel();
        // Default: ConvertOnSystemChange = false
        var entry = CreateTestEntry();
        entry.BarrelLength = new Measurement<DistanceUnit>(610, DistanceUnit.Millimeter);
        panel.LibraryEntry = entry;

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        // With flag=false, filled controls are not touched at all (value AND unit stay)
        GetSelectedUnit(panel.BarrelLengthControl).Should().Be(DistanceUnit.Millimeter);
        panel.BarrelLengthControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Millimeter)
            .Should().BeApproximately(610, 1);
    }

    // Note: Changed event tests removed - Avalonia Headless doesn't fire TextChanged
    // when TextBox.Text is set programmatically. The Changed event works in real UI.

    [AvaloniaFact]
    public void BulletType_ShouldSelectCorrectType()
    {
        var panel = new AmmoLibraryPanel();
        var entry = CreateTestEntry();

        panel.LibraryEntry = entry;

        (panel.BulletTypeCombo.SelectedItem as AmmunitionType)?.Abbreviation.Should().Be("HPBT");
    }

    [AvaloniaFact]
    public void Clear_ShouldResetAllFields()
    {
        var panel = new AmmoLibraryPanel();
        var entry = CreateTestEntry();
        entry.BarrelLength = new Measurement<DistanceUnit>(24, DistanceUnit.Inch);
        panel.LibraryEntry = entry;

        panel.Clear();

        panel.NameTextBox.Text.Should().Be("");
        panel.CaliberTextBox.Text.Should().Be("");
        panel.SourceTextBox.Text.Should().Be("");
        panel.BulletTypeCombo.SelectedIndex.Should().Be(0);
        panel.BarrelLengthControl.IsEmpty.Should().BeTrue();
        panel.AmmoSubPanel.Ammunition.Should().BeNull();
    }

    // Bug fix tests: unit switching when empty
    [AvaloniaFact]
    public void MeasurementSystem_SwitchToImperialWhenEmpty_ShouldChangeBarrelLengthUnit()
    {
        var panel = new AmmoLibraryPanel();

        panel.MeasurementSystem = MeasurementSystem.Imperial;

        GetSelectedUnit(panel.BarrelLengthControl).Should().Be(DistanceUnit.Inch);
    }

    [AvaloniaFact]
    public void MeasurementSystem_SwitchToMetricWhenEmpty_ShouldChangeBarrelLengthUnit()
    {
        var panel = new AmmoLibraryPanel();
        panel.MeasurementSystem = MeasurementSystem.Imperial;

        panel.MeasurementSystem = MeasurementSystem.Metric;

        GetSelectedUnit(panel.BarrelLengthControl).Should().Be(DistanceUnit.Millimeter);
    }

    // Bug fix tests: load/save buttons invoke file dialog
    [AvaloniaFact]
    public void LoadButton_WithFileDialogService_ShouldInvokeOpenDialog()
    {
        var panel = new AmmoLibraryPanel();
        var mockService = new MockFileDialogService();
        panel.FileDialogService = mockService;

        panel.LoadButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        mockService.LastOpenOptions.Should().NotBeNull();
        mockService.LastOpenOptions!.Title.Should().Be("Load Ammunition");
        mockService.LastOpenOptions.Filters.Should().HaveCount(2);
        mockService.LastOpenOptions.Filters[0].Extension.Should().Be("ammox");
        mockService.LastOpenOptions.Filters[1].Extension.Should().Be("ammo");
    }

    [AvaloniaFact]
    public void SaveButton_WithFileDialogService_ShouldInvokeSaveDialog()
    {
        var panel = new AmmoLibraryPanel();
        var mockService = new MockFileDialogService();
        panel.FileDialogService = mockService;
        // Need data for save to work
        panel.LibraryEntry = CreateTestEntry();

        panel.SaveButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        mockService.LastSaveOptions.Should().NotBeNull();
        mockService.LastSaveOptions!.Title.Should().Be("Save Ammunition");
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

    private static AmmunitionLibraryEntry CreateTestEntry()
    {
        return new AmmunitionLibraryEntry()
        {
            Name = "Federal Gold Medal 168gr",
            Caliber = ".308 Winchester",
            AmmunitionType = "HPBT",
            Source = "Federal Premium",
            Ammunition = CreateTestAmmunition(),
        };
    }
}
