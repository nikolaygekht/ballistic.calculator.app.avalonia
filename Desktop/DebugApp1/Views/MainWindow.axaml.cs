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
    private int _atmoChangeCount;
    private int _windChangeCount;

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

        AtmoTestPanel.Changed += (s, e) =>
        {
            _atmoChangeCount++;
            AtmoChangeCount.Text = $"Changed events: {_atmoChangeCount}";
        };

        WindTestPanel.Changed += (s, e) =>
        {
            _windChangeCount++;
            WindChangeCount.Text = $"Changed events: {_windChangeCount}";
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
        var state = ConvertOnSystemChangeCheckBox.IsChecked;
        if (state == null)
        {
            // Indeterminate: restore each panel's default
            AmmoTestPanel.ConvertOnSystemChange = false;
            AmmoLibTestPanel.ConvertOnSystemChange = false;
            AtmoTestPanel.ConvertOnSystemChange = true;
            WindTestPanel.ConvertOnSystemChange = false;
        }
        else
        {
            var convert = state.Value;
            AmmoTestPanel.ConvertOnSystemChange = convert;
            AmmoLibTestPanel.ConvertOnSystemChange = convert;
            AtmoTestPanel.ConvertOnSystemChange = convert;
            WindTestPanel.ConvertOnSystemChange = convert;
        }
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

    // AmmoLibraryRecordPanel handlers
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

    // AtmospherePanel handlers
    private void OnAtmoMetric(object? sender, RoutedEventArgs e)
        => AtmoTestPanel.MeasurementSystem = MeasurementSystem.Metric;

    private void OnAtmoImperial(object? sender, RoutedEventArgs e)
        => AtmoTestPanel.MeasurementSystem = MeasurementSystem.Imperial;

    private void OnAtmoSetTestData(object? sender, RoutedEventArgs e)
    {
        AtmoTestPanel.Atmosphere = new Atmosphere(
            new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            new Measurement<PressureUnit>(720, PressureUnit.MillimetersOfMercury),
            new Measurement<TemperatureUnit>(20, TemperatureUnit.Celsius),
            0.65);
    }

    private void OnAtmoGetValues(object? sender, RoutedEventArgs e)
    {
        var atmo = AtmoTestPanel.Atmosphere;
        if (atmo == null)
        {
            AtmoOutput.Text = "Atmosphere: null (incomplete data)";
            return;
        }

        AtmoOutput.Text = $"Altitude: {atmo.Altitude}\n" +
                          $"Pressure: {atmo.Pressure}\n" +
                          $"Temperature: {atmo.Temperature}\n" +
                          $"Humidity: {atmo.Humidity * 100:F0}%";
    }

    private void OnAtmoClear(object? sender, RoutedEventArgs e)
        => AtmoTestPanel.Clear();

    // MultiWindPanel handlers
    private void OnWindMetric(object? sender, RoutedEventArgs e)
        => WindTestPanel.MeasurementSystem = MeasurementSystem.Metric;

    private void OnWindImperial(object? sender, RoutedEventArgs e)
        => WindTestPanel.MeasurementSystem = MeasurementSystem.Imperial;

    private void OnWindSetTestData(object? sender, RoutedEventArgs e)
    {
        WindTestPanel.Winds = new Wind[]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
                MaximumRange = new Measurement<DistanceUnit>(500, DistanceUnit.Meter),
            },
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(180, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(3, VelocityUnit.MetersPerSecond),
            },
        };
    }

    private void OnWindGetValues(object? sender, RoutedEventArgs e)
    {
        var winds = WindTestPanel.Winds;
        if (winds == null)
        {
            WindOutput.Text = "Winds: null (no data)";
            return;
        }

        var text = $"Wind count: {winds.Length}\n";
        for (int i = 0; i < winds.Length; i++)
        {
            text += $"\nWind #{i + 1}:\n" +
                    $"  Direction: {winds[i].Direction}\n" +
                    $"  Velocity: {winds[i].Velocity}\n" +
                    $"  Max Range: {winds[i].MaximumRange?.ToString() ?? "unlimited"}";
        }
        WindOutput.Text = text;
    }

    private void OnWindClear(object? sender, RoutedEventArgs e)
        => WindTestPanel.Clear();
}
