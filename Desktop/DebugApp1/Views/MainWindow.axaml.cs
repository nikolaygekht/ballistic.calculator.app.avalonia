using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Types;
using DebugApp1.Services;
using Gehtsoft.Measurements;
using System;

namespace DebugApp1.Views;

public partial class MainWindow : Window
{
    private int _ammoChangeCount;
    private int _ammoLibChangeCount;

    public MainWindow()
    {
        InitializeComponent();

        AmmoTestPanel.Changed += (s, e) =>
        {
            _ammoChangeCount++;
            AmmoChangeCount.Text = $"Changed events: {_ammoChangeCount}";
        };

        AmmoLibTestPanel.Changed += (s, e) =>
        {
            _ammoLibChangeCount++;
            AmmoLibChangeCount.Text = $"Changed events: {_ammoLibChangeCount}";
        };

        AmmoLibTestPanel.FileDialogService = new AvaloniaFileDialogService(this);
    }

    // Font Size Control
    private void OnSetFontSize10(object? sender, RoutedEventArgs e) => SetApplicationFontSize(10);
    private void OnSetFontSize12(object? sender, RoutedEventArgs e) => SetApplicationFontSize(12);
    private void OnSetFontSize13(object? sender, RoutedEventArgs e) => SetApplicationFontSize(13);
    private void OnSetFontSize14(object? sender, RoutedEventArgs e) => SetApplicationFontSize(14);
    private void OnSetFontSize16(object? sender, RoutedEventArgs e) => SetApplicationFontSize(16);

    private void SetApplicationFontSize(double fontSize)
    {
        var app = Avalonia.Application.Current;
        if (app == null) return;

        app.Resources["AppFontSize"] = fontSize;
        FontSizeDisplay.Text = $"Current font size: {fontSize}";
    }

    private void OnConvertFlagChanged(object? sender, RoutedEventArgs e)
    {
        var convert = ConvertOnSystemChangeCheckBox.IsChecked == true;
        AmmoTestPanel.ConvertOnSystemChange = convert;
        AmmoLibTestPanel.ConvertOnSystemChange = convert;
    }

    // AmmoPanel handlers
    private void OnAmmoMetric(object? sender, RoutedEventArgs e)
        => AmmoTestPanel.MeasurementSystem = MeasurementSystem.Metric;

    private void OnAmmoImperial(object? sender, RoutedEventArgs e)
        => AmmoTestPanel.MeasurementSystem = MeasurementSystem.Imperial;

    private void OnAmmoSetTestData(object? sender, RoutedEventArgs e)
    {
        AmmoTestPanel.Ammunition = new Ammunition()
        {
            Weight = new Measurement<WeightUnit>(168, WeightUnit.Grain),
            BallisticCoefficient = new BallisticCoefficient(0.462, DragTableId.G1),
            MuzzleVelocity = new Measurement<VelocityUnit>(2650, VelocityUnit.FeetPerSecond),
            BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
            BulletLength = new Measurement<DistanceUnit>(1.235, DistanceUnit.Inch),
        };
    }

    private void OnAmmoGetValues(object? sender, RoutedEventArgs e)
    {
        var ammo = AmmoTestPanel.Ammunition;
        if (ammo == null)
        {
            AmmoOutput.Text = "Ammunition: null (incomplete data)";
            return;
        }

        AmmoOutput.Text = $"Weight: {ammo.Weight}\n" +
                          $"BC: {ammo.BallisticCoefficient} ({ammo.BallisticCoefficient.ValueType})\n" +
                          $"Muzzle Velocity: {ammo.MuzzleVelocity}\n" +
                          $"Bullet Diameter: {ammo.BulletDiameter?.ToString() ?? "not set"}\n" +
                          $"Bullet Length: {ammo.BulletLength?.ToString() ?? "not set"}";
    }

    private void OnAmmoClear(object? sender, RoutedEventArgs e)
        => AmmoTestPanel.Clear();

    // AmmoLibraryPanel handlers
    private void OnAmmoLibMetric(object? sender, RoutedEventArgs e)
        => AmmoLibTestPanel.MeasurementSystem = MeasurementSystem.Metric;

    private void OnAmmoLibImperial(object? sender, RoutedEventArgs e)
        => AmmoLibTestPanel.MeasurementSystem = MeasurementSystem.Imperial;

    private void OnAmmoLibSetTestData(object? sender, RoutedEventArgs e)
    {
        AmmoLibTestPanel.LibraryEntry = new AmmunitionLibraryEntry()
        {
            Name = "Federal Gold Medal 168gr",
            Caliber = ".308 Winchester",
            AmmunitionType = "HPBT",
            Source = "Federal Premium",
            BarrelLength = new Measurement<DistanceUnit>(24, DistanceUnit.Inch),
            Ammunition = new Ammunition()
            {
                Weight = new Measurement<WeightUnit>(168, WeightUnit.Grain),
                BallisticCoefficient = new BallisticCoefficient(0.462, DragTableId.G1),
                MuzzleVelocity = new Measurement<VelocityUnit>(2650, VelocityUnit.FeetPerSecond),
                BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
                BulletLength = new Measurement<DistanceUnit>(1.235, DistanceUnit.Inch),
            },
        };
    }

    private void OnAmmoLibGetValues(object? sender, RoutedEventArgs e)
    {
        var entry = AmmoLibTestPanel.LibraryEntry;
        if (entry == null)
        {
            AmmoLibOutput.Text = "LibraryEntry: null (incomplete data)";
            return;
        }

        AmmoLibOutput.Text = $"Name: {entry.Name}\n" +
                             $"Caliber: {entry.Caliber ?? "not set"}\n" +
                             $"Type: {entry.AmmunitionType ?? "not set"}\n" +
                             $"Barrel Length: {entry.BarrelLength?.ToString() ?? "not set"}\n" +
                             $"Source: {entry.Source ?? "not set"}\n" +
                             $"Weight: {entry.Ammunition.Weight}\n" +
                             $"BC: {entry.Ammunition.BallisticCoefficient}\n" +
                             $"Muzzle Velocity: {entry.Ammunition.MuzzleVelocity}";
    }

    private void OnAmmoLibClear(object? sender, RoutedEventArgs e)
        => AmmoLibTestPanel.Clear();
}
