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
    private int _rifleChangeCount;
    private int _atmoChangeCount;
    private int _paramsChangeCount;
    private int _windChangeCount;
    private int _shotChangeCount;

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

        RifleTestPanel.Changed += (s, e) =>
        {
            _rifleChangeCount++;
            RifleChangeCount.Text = $"Changed events: {_rifleChangeCount}";
        };

        AtmoTestPanel.Changed += (s, e) =>
        {
            _atmoChangeCount++;
            AtmoChangeCount.Text = $"Changed events: {_atmoChangeCount}";
        };

        ParamsTestPanel.RiflePanel = RifleTestPanel;
        ParamsTestPanel.Changed += (s, e) =>
        {
            _paramsChangeCount++;
            ParamsChangeCount.Text = $"Changed events: {_paramsChangeCount}";
        };

        WindTestPanel.Changed += (s, e) =>
        {
            _windChangeCount++;
            WindChangeCount.Text = $"Changed events: {_windChangeCount}";
        };

        ShotDataTestPanel.Changed += (s, e) =>
        {
            _shotChangeCount++;
            ShotChangeCount.Text = $"Changed events: {_shotChangeCount}";
        };

        var fileDialogService = new AvaloniaFileDialogService(this);
        AmmoLibTestPanel.FileDialogService = fileDialogService;
        ShotDataTestPanel.FileDialogService = fileDialogService;
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
            RifleTestPanel.ConvertOnSystemChange = false;
            AtmoTestPanel.ConvertOnSystemChange = true;
            ParamsTestPanel.ConvertOnSystemChange = true;
            WindTestPanel.ConvertOnSystemChange = false;
        }
        else
        {
            var convert = state.Value;
            AmmoTestPanel.ConvertOnSystemChange = convert;
            AmmoLibTestPanel.ConvertOnSystemChange = convert;
            RifleTestPanel.ConvertOnSystemChange = convert;
            AtmoTestPanel.ConvertOnSystemChange = convert;
            ParamsTestPanel.ConvertOnSystemChange = convert;
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

    // RiflePanel handlers
    private void OnRifleMetric(object? sender, RoutedEventArgs e)
        => RifleTestPanel.MeasurementSystem = MeasurementSystem.Metric;

    private void OnRifleImperial(object? sender, RoutedEventArgs e)
        => RifleTestPanel.MeasurementSystem = MeasurementSystem.Imperial;

    private void OnRifleSetTestData(object? sender, RoutedEventArgs e)
    {
        RifleTestPanel.Rifle = new Rifle(
            new Sight(
                new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                new Measurement<AngularUnit>(0.25, AngularUnit.MOA)),
            new ZeroingParameters(
                new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                null, null),
            new Rifling(
                new Measurement<DistanceUnit>(12, DistanceUnit.Inch),
                TwistDirection.Right));
    }

    private void OnRifleGetValues(object? sender, RoutedEventArgs e)
    {
        var rifle = RifleTestPanel.Rifle;
        if (rifle == null)
        {
            RifleOutput.Text = "Rifle: null (incomplete data)";
            return;
        }

        RifleOutput.Text = $"Sight Height: {rifle.Sight.SightHeight}\n" +
                           $"Zero Distance: {rifle.Zero.Distance}\n" +
                           $"V Click: {rifle.Sight.VerticalClick?.ToString() ?? "not set"}\n" +
                           $"H Click: {rifle.Sight.HorizontalClick?.ToString() ?? "not set"}\n" +
                           $"Rifling: {(rifle.Rifling != null ? $"{rifle.Rifling.Direction} 1:{rifle.Rifling.RiflingStep}" : "not set")}\n" +
                           $"V Offset: {rifle.Zero.VerticalOffset?.ToString() ?? "not set"}";
    }

    private void OnRifleClear(object? sender, RoutedEventArgs e)
        => RifleTestPanel.Clear();

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

    // ParametersPanel handlers
    private void OnParamsMetric(object? sender, RoutedEventArgs e)
        => ParamsTestPanel.MeasurementSystem = MeasurementSystem.Metric;

    private void OnParamsImperial(object? sender, RoutedEventArgs e)
        => ParamsTestPanel.MeasurementSystem = MeasurementSystem.Imperial;

    private void OnParamsSetTestData(object? sender, RoutedEventArgs e)
    {
        ParamsTestPanel.Parameters = new ShotParameters()
        {
            MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter),
            Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            ShotAngle = new Measurement<AngularUnit>(5, AngularUnit.Degree),
        };
    }

    private void OnParamsGetValues(object? sender, RoutedEventArgs e)
    {
        var parms = ParamsTestPanel.Parameters;
        if (parms == null)
        {
            ParamsOutput.Text = "Parameters: null (incomplete data)";
            return;
        }

        ParamsOutput.Text = $"Max Range: {parms.MaximumDistance}\n" +
                            $"Step: {parms.Step}\n" +
                            $"Shot Angle: {parms.ShotAngle?.ToString() ?? "not set"}";
    }

    private void OnParamsClear(object? sender, RoutedEventArgs e)
        => ParamsTestPanel.Clear();

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

    // ShotDataPanel handlers
    private void OnShotMetric(object? sender, RoutedEventArgs e)
        => ShotDataTestPanel.MeasurementSystem = MeasurementSystem.Metric;

    private void OnShotImperial(object? sender, RoutedEventArgs e)
        => ShotDataTestPanel.MeasurementSystem = MeasurementSystem.Imperial;

    private void OnShotSetTestData(object? sender, RoutedEventArgs e)
    {
        ShotDataTestPanel.ShotData = new ShotData()
        {
            Ammunition = new AmmunitionLibraryEntry()
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
                },
            },
            Weapon = new Rifle(
                new Sight(
                    new Measurement<DistanceUnit>(50, DistanceUnit.Millimeter),
                    new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
                    new Measurement<AngularUnit>(0.25, AngularUnit.MOA)),
                new ZeroingParameters(
                    new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
                    null, null),
                new Rifling(
                    new Measurement<DistanceUnit>(12, DistanceUnit.Inch),
                    TwistDirection.Right)),
            Atmosphere = new Atmosphere(
                new Measurement<DistanceUnit>(0, DistanceUnit.Meter),
                new Measurement<PressureUnit>(760, PressureUnit.MillimetersOfMercury),
                new Measurement<TemperatureUnit>(15, TemperatureUnit.Celsius),
                0.78),
            Winds = new Wind[]
            {
                new Wind()
                {
                    Direction = new Measurement<AngularUnit>(90, AngularUnit.Degree),
                    Velocity = new Measurement<VelocityUnit>(5, VelocityUnit.MetersPerSecond),
                },
            },
            Parameters = new ShotParameters()
            {
                MaximumDistance = new Measurement<DistanceUnit>(1000, DistanceUnit.Meter),
                Step = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            },
        };
    }

    private void OnShotGetValues(object? sender, RoutedEventArgs e)
    {
        var data = ShotDataTestPanel.ShotData;
        if (data == null)
        {
            ShotOutput.Text = "ShotData: null (incomplete data)";
            return;
        }

        ShotOutput.Text = $"Ammo: {data.Ammunition?.Name ?? "?"}\n" +
                          $"Weight: {data.Ammunition?.Ammunition.Weight}\n" +
                          $"Sight Height: {data.Weapon?.Sight.SightHeight}\n" +
                          $"Zero: {data.Weapon?.Zero.Distance}\n" +
                          $"Atmosphere: {data.Atmosphere?.Temperature}, {data.Atmosphere?.Pressure}\n" +
                          $"Winds: {data.Winds?.Length ?? 0}\n" +
                          $"Max Range: {data.Parameters?.MaximumDistance}\n" +
                          $"Step: {data.Parameters?.Step}";
    }

    private void OnShotClear(object? sender, RoutedEventArgs e)
        => ShotDataTestPanel.Clear();
}
