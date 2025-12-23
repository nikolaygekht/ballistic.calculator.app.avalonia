using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Serialization;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DebugApp.Views;

public partial class MainWindow : Window
{
    private int _basicBCChangeCount = 0;
    private int _distanceChangeCount = 0;
    private int _windChangeCount = 0;

    public MainWindow()
    {
        InitializeComponent();

        // Wire up change event counters
        BasicBC.Changed += (s, e) =>
        {
            _basicBCChangeCount++;
            BasicBCChangeCounter.Text = $"Changed events: {_basicBCChangeCount}";
        };

        DistanceControl.Changed += (s, e) =>
        {
            _distanceChangeCount++;
            DistanceChangeCounter.Text = $"Changed events: {_distanceChangeCount}";
        };

        WindControl.Changed += (s, e) =>
        {
            _windChangeCount++;
            WindChangeCounter.Text = $"Changed events: {_windChangeCount}";
            WindDisplay.Text = $"Direction: {WindControl.Direction:F0}°";
        };
    }

    // Basic BC Test
    private void OnSetBasicBC(object? sender, RoutedEventArgs e)
    {
        BasicBC.Value = new BallisticCoefficient(0.450, DragTableId.G1);
        BasicBCDisplay.Text = "Set: 0.450 G1";
    }

    private void OnGetBasicBC(object? sender, RoutedEventArgs e)
    {
        var value = BasicBC.Value;
        BasicBCDisplay.Text = value != null
            ? $"Value: {value.Value.Value:F5} {value.Value.Table}"
            : "Value: (none)";
    }

    private void OnClearBC(object? sender, RoutedEventArgs e)
    {
        BasicBC.Value = null;
        BasicBCDisplay.Text = "Cleared";
    }

    // Precision Transparency Test (CRITICAL)
    private void OnSetPrecisionBC(object? sender, RoutedEventArgs e)
    {
        PrecisionBC.Value = new BallisticCoefficient(0.45678, DragTableId.G1);

        PrecisionResult.Text = "✓ Step 1 Complete: Set 0.45678";
        PrecisionResult.Foreground = Brushes.Black;
        PrecisionDetails.Text = "Control displays with 3 decimals (0.457).\n" +
                                "Now click '2. Get Value' WITHOUT editing the text.";
    }

    private void OnGetPrecisionBC(object? sender, RoutedEventArgs e)
    {
        var value = PrecisionBC.Value;

        if (value?.Value == 0.45678)
        {
            PrecisionResult.Text = "✅ SUCCESS! Precision transparency working!";
            PrecisionResult.Foreground = Brushes.Green;
            PrecisionDetails.Text = $"Retrieved value: {value.Value.Value:F5} (exact match!)\n" +
                                    "Even though displayed as 0.457, the full precision (0.45678) was preserved!";
        }
        else
        {
            PrecisionResult.Text = "❌ FAILED! Precision lost!";
            PrecisionResult.Foreground = Brushes.Red;
            PrecisionDetails.Text = $"Retrieved value: {value?.Value ?? 0:F5}\n" +
                                    $"Expected: 0.45678\n" +
                                    "This is a critical bug - precision should be preserved when not edited.";
        }
    }

    // Tables Order Test
    private void OnVerifyTables(object? sender, RoutedEventArgs e)
    {
        var tables = TablesBC.GetAvailableTableNames().ToList();
        var expected = new[] { "G1", "G2", "G5", "G6", "G7", "G8", "GI", "GS", "RA4", "GC" };

        bool correct = tables.SequenceEqual(expected);

        if (correct)
        {
            TablesDisplay.Text = $"✅ Correct order!\n{string.Join(", ", tables)}";
            TablesDisplay.Foreground = Brushes.Green;
        }
        else
        {
            TablesDisplay.Text = $"❌ Wrong order!\nGot: {string.Join(", ", tables)}\n" +
                                 $"Expected: {string.Join(", ", expected)}";
            TablesDisplay.Foreground = Brushes.Red;
        }
    }

    // Font Size Test
    private void OnSetFontSize10(object? sender, RoutedEventArgs e) => SetApplicationFontSize(10);
    private void OnSetFontSize12(object? sender, RoutedEventArgs e) => SetApplicationFontSize(12);
    private void OnSetFontSize13(object? sender, RoutedEventArgs e) => SetApplicationFontSize(13);
    private void OnSetFontSize14(object? sender, RoutedEventArgs e) => SetApplicationFontSize(14);
    private void OnSetFontSize16(object? sender, RoutedEventArgs e) => SetApplicationFontSize(16);

    private void SetApplicationFontSize(double fontSize)
    {
        var app = Avalonia.Application.Current;
        if (app == null) return;

        // Update the dynamic resource
        app.Resources["AppFontSize"] = fontSize;

        // Update display
        FontSizeDisplay.Text = $"Current font size: {fontSize}";
    }

    #region Measurement Control Tests

    // Distance Test
    private void OnSetDistance(object? sender, RoutedEventArgs e)
    {
        DistanceControl.SetValue(new Measurement<DistanceUnit>(100, DistanceUnit.Meter));
        DistanceDisplay.Text = "Set: 100 meters";
    }

    private void OnGetDistance(object? sender, RoutedEventArgs e)
    {
        var value = DistanceControl.GetValue<DistanceUnit>();
        DistanceDisplay.Text = value != null
            ? $"Value: {value.Value.Value:F5} {value.Value.Unit}"
            : "Value: (none)";
    }

    private void OnClearDistance(object? sender, RoutedEventArgs e)
    {
        DistanceControl.Value = null;
        DistanceDisplay.Text = "Cleared";
    }

    // Velocity Test
    private void OnSetVelocity(object? sender, RoutedEventArgs e)
    {
        VelocityControl.SetValue(new Measurement<VelocityUnit>(800, VelocityUnit.MetersPerSecond));
        VelocityDisplay.Text = "Set: 800 m/s";
    }

    private void OnGetVelocity(object? sender, RoutedEventArgs e)
    {
        var value = VelocityControl.GetValue<VelocityUnit>();
        VelocityDisplay.Text = value != null
            ? $"Value: {value.Value.Value:F5} {value.Value.Unit}"
            : "Value: (none)";
    }

    // Weight Test
    private void OnSetWeight(object? sender, RoutedEventArgs e)
    {
        WeightControl.SetValue(new Measurement<WeightUnit>(150, WeightUnit.Grain));
        WeightDisplay.Text = "Set: 150 grains";
    }

    private void OnGetWeight(object? sender, RoutedEventArgs e)
    {
        var value = WeightControl.GetValue<WeightUnit>();
        WeightDisplay.Text = value != null
            ? $"Value: {value.Value.Value:F5} {value.Value.Unit}"
            : "Value: (none)";
    }

    // Conversion Test
    private void OnSetForConversion(object? sender, RoutedEventArgs e)
    {
        ConversionControl.SetValue(new Measurement<DistanceUnit>(100, DistanceUnit.Meter));
        ConversionDisplay.Text = "✓ Step 1: Set 100 meters";
    }

    private void OnConvertToFeet(object? sender, RoutedEventArgs e)
    {
        ConversionControl.ChangeUnit(DistanceUnit.Foot, 2);
        ConversionDisplay.Text = "✓ Step 2: Converted to feet (100m ≈ 328.08 ft)";
    }

    private void OnGetConverted(object? sender, RoutedEventArgs e)
    {
        var value = ConversionControl.GetValue<DistanceUnit>();
        if (value != null)
        {
            ConversionDisplay.Text = $"✓ Step 3 Complete!\n" +
                                   $"Value: {value.Value.Value:F5} {value.Value.Unit}\n" +
                                   $"Expected: ~328.08 feet";
            if (value.Value.Unit == DistanceUnit.Foot &&
                value.Value.Value > 328 && value.Value.Value < 329)
            {
                ConversionDisplay.Foreground = Brushes.Green;
                ConversionDisplay.Text += "\n✓ PASS: Conversion correct!";
            }
        }
        else
        {
            ConversionDisplay.Text = "Value: (none)";
        }
    }

    #endregion

    #region Wind Direction Control Tests

    private void OnSetWind0(object? sender, RoutedEventArgs e) => WindControl.Direction = 0;
    private void OnSetWind90(object? sender, RoutedEventArgs e) => WindControl.Direction = 90;
    private void OnSetWind180(object? sender, RoutedEventArgs e) => WindControl.Direction = 180;
    private void OnSetWind270(object? sender, RoutedEventArgs e) => WindControl.Direction = 270;
    private void OnSetWind45(object? sender, RoutedEventArgs e) => WindControl.Direction = 45;
    private void OnSetWind135(object? sender, RoutedEventArgs e) => WindControl.Direction = 135;
    private void OnSetWind225(object? sender, RoutedEventArgs e) => WindControl.Direction = 225;
    private void OnSetWind315(object? sender, RoutedEventArgs e) => WindControl.Direction = 315;

    #endregion

    #region Reticle Canvas Tests

    private async void OnLoadReticle(object? sender, RoutedEventArgs e)
    {
        try
        {
            var storageProvider = StorageProvider;
            if (storageProvider == null)
            {
                ReticleDisplay.Text = "Error: Storage provider not available";
                return;
            }

            var fileTypeFilter = new FilePickerFileType("Reticle Files")
            {
                Patterns = new[] { "*.reticle" },
                MimeTypes = new[] { "application/xml" }
            };

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Reticle File",
                AllowMultiple = false,
                FileTypeFilter = new[] { fileTypeFilter }
            });

            if (files.Count == 0)
            {
                ReticleDisplay.Text = "No file selected";
                return;
            }

            var file = files[0];
            var path = file.TryGetLocalPath();

            if (string.IsNullOrEmpty(path))
            {
                ReticleDisplay.Text = "Error: Could not get file path";
                return;
            }

            // Load reticle from file
            var reticleDefinition = BallisticXmlDeserializer.ReadFromFile<ReticleDefinition>(path);

            if (reticleDefinition == null)
            {
                ReticleDisplay.Text = "Error: Could not parse reticle file";
                return;
            }

            // Set reticle to canvas
            ReticleCanvas.Reticle = reticleDefinition;

            ReticleDisplay.Text = $"Loaded: {Path.GetFileName(path)}\n" +
                                 $"Name: {reticleDefinition.Name ?? "(unnamed)"}\n" +
                                 $"Size: {reticleDefinition.Size.X} x {reticleDefinition.Size.Y}\n" +
                                 $"Elements: {reticleDefinition.Elements?.Count ?? 0}";
        }
        catch (Exception ex)
        {
            ReticleDisplay.Text = $"Error loading reticle:\n{ex.Message}";
        }
    }

    private void OnClearReticle(object? sender, RoutedEventArgs e)
    {
        ReticleCanvas.Reticle = null;
        ReticleDisplay.Text = "Reticle cleared";
    }

    private void OnSetBgWhite(object? sender, RoutedEventArgs e)
    {
        ReticleCanvas.BackgroundColor = Colors.White;
    }

    private void OnSetBgBlack(object? sender, RoutedEventArgs e)
    {
        ReticleCanvas.BackgroundColor = Colors.Black;
    }

    private void OnSetBgGray(object? sender, RoutedEventArgs e)
    {
        ReticleCanvas.BackgroundColor = Colors.LightGray;
    }

    #endregion

    #region Trajectory Chart Tests

    private void OnChartModeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryChart == null || ChartModeCombo == null) return;

        TrajectoryChart.ChartMode = ChartModeCombo.SelectedIndex switch
        {
            0 => TrajectoryChartMode.Velocity,
            1 => TrajectoryChartMode.Mach,
            2 => TrajectoryChartMode.Drop,
            3 => TrajectoryChartMode.DropAdjustment,
            4 => TrajectoryChartMode.Windage,
            5 => TrajectoryChartMode.WindageAdjustment,
            6 => TrajectoryChartMode.Energy,
            _ => TrajectoryChartMode.Drop
        };
    }

    private void OnMeasurementSystemChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryChart == null || MeasurementSystemCombo == null) return;

        TrajectoryChart.MeasurementSystem = MeasurementSystemCombo.SelectedIndex == 0
            ? MeasurementSystem.Metric
            : MeasurementSystem.Imperial;
    }

    private void OnAngularUnitChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryChart == null || AngularUnitCombo == null) return;

        TrajectoryChart.AngularUnits = AngularUnitCombo.SelectedIndex switch
        {
            0 => AngularUnit.MOA,
            1 => AngularUnit.MRad,
            2 => AngularUnit.Mil,
            _ => AngularUnit.MOA
        };
    }

    private void OnDropBaseChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryChart == null || DropBaseCombo == null) return;

        TrajectoryChart.DropBase = DropBaseCombo.SelectedIndex == 0
            ? DropBase.SightLine
            : DropBase.MuzzlePoint;
    }

    private void OnLoadSampleTrajectory(object? sender, RoutedEventArgs e)
    {
        // Create sample trajectory data (simulated .308 Winchester at 100m intervals)
        var trajectory = CreateSampleTrajectory();

        TrajectoryChart.SetTrajectory(".308 Win 168gr", trajectory);
        UpdateTrajectoryDisplay();

        ChartInfoDisplay.Text = "Loaded sample trajectory: .308 Winchester 168gr HPBT\n" +
                                "Muzzle velocity: 800 m/s, zeroed at 100m\n" +
                                "Try changing chart modes and measurement systems!";
    }

    private void OnAddSecondTrajectory(object? sender, RoutedEventArgs e)
    {
        // Add a second trajectory for comparison (different load)
        var trajectory = CreateSampleTrajectory2();

        TrajectoryChart.AddTrajectory(".223 Rem 55gr", trajectory);
        UpdateTrajectoryDisplay();

        ChartInfoDisplay.Text = "Added second trajectory: .223 Remington 55gr FMJ\n" +
                                "Muzzle velocity: 975 m/s\n" +
                                "Compare the two trajectories!";
    }

    private void OnClearTrajectories(object? sender, RoutedEventArgs e)
    {
        TrajectoryChart.ClearTrajectories();
        UpdateTrajectoryDisplay();
        ChartInfoDisplay.Text = "All trajectories cleared.";
    }

    private void UpdateTrajectoryDisplay()
    {
        TrajectoryCountDisplay.Text = $"Trajectories: {TrajectoryChart.TrajectoryCount}";
    }

    /// <summary>
    /// Creates sample .308 Winchester trajectory data
    /// </summary>
    private TrajectoryPoint[] CreateSampleTrajectory()
    {
        // Simulated data for .308 Win 168gr HPBT, 800 m/s muzzle velocity
        var points = new (double distance, double velocity, double mach, double drop, double dropFlat, double dropAdj, double windage, double losElev, double windageAdj)[]
        {
            (0, 800, 2.35, 0, 0, 0, 0, 0, 0),
            (100, 735, 2.16, 0, -4.5, 0, 2.1, 4.5, 0.7),
            (200, 675, 1.98, -8.5, -17.5, 1.5, 4.8, 9.0, 0.8),
            (300, 618, 1.81, -32.0, -45.5, 3.7, 8.2, 13.5, 0.9),
            (400, 565, 1.66, -75.0, -93.0, 6.5, 12.5, 18.0, 1.1),
            (500, 515, 1.51, -142.0, -165.0, 9.8, 17.8, 23.0, 1.2),
            (600, 470, 1.38, -238.0, -268.0, 13.7, 24.2, 30.0, 1.4),
            (700, 428, 1.26, -370.0, -408.0, 18.3, 31.8, 38.0, 1.6),
            (800, 390, 1.14, -545.0, -592.0, 23.5, 40.8, 47.0, 1.8),
            (900, 356, 1.04, -772.0, -830.0, 29.6, 51.2, 58.0, 2.0),
            (1000, 325, 0.95, -1060.0, -1130.0, 36.6, 63.2, 70.0, 2.2),
        };

        return points.Select(p => CreateTrajectoryPoint(p.distance, p.velocity, p.mach, p.drop, p.dropFlat, p.dropAdj, p.windage, p.losElev, p.windageAdj)).ToArray();
    }

    /// <summary>
    /// Creates sample .223 Remington trajectory data (flatter trajectory)
    /// </summary>
    private TrajectoryPoint[] CreateSampleTrajectory2()
    {
        // Simulated data for .223 Rem 55gr FMJ, 975 m/s muzzle velocity
        var points = new (double distance, double velocity, double mach, double drop, double dropFlat, double dropAdj, double windage, double losElev, double windageAdj)[]
        {
            (0, 975, 2.86, 0, 0, 0, 0, 0, 0),
            (100, 880, 2.58, 0, -2.8, 0, 3.5, 2.8, 1.2),
            (200, 795, 2.33, -5.2, -10.8, 0.9, 8.0, 5.6, 1.4),
            (300, 715, 2.10, -19.5, -28.0, 2.2, 13.8, 8.5, 1.6),
            (400, 642, 1.88, -46.5, -58.5, 4.0, 21.0, 12.0, 1.8),
            (500, 575, 1.69, -90.0, -107.0, 6.2, 29.8, 17.0, 2.1),
            (600, 514, 1.51, -154.0, -178.0, 8.9, 40.2, 24.0, 2.3),
            (700, 460, 1.35, -244.0, -275.0, 12.0, 52.5, 31.0, 2.6),
            (800, 412, 1.21, -366.0, -405.0, 15.8, 66.8, 39.0, 2.9),
            (900, 370, 1.09, -526.0, -575.0, 20.2, 83.2, 49.0, 3.2),
            (1000, 335, 0.98, -732.0, -792.0, 25.3, 102.0, 60.0, 3.5),
        };

        return points.Select(p => CreateTrajectoryPoint(p.distance, p.velocity, p.mach, p.drop, p.dropFlat, p.dropAdj, p.windage, p.losElev, p.windageAdj)).ToArray();
    }

    private TrajectoryPoint CreateTrajectoryPoint(
        double distanceM, double velocityMps, double mach,
        double dropCm, double dropFlatCm, double dropAdjMoa,
        double windageCm, double losElevationCm, double windageAdjMoa)
    {
        // Estimate energy (simplified: E = 0.5 * m * v^2, assuming ~10g bullet)
        double energyJoules = 0.5 * 0.010 * velocityMps * velocityMps;

        return new TrajectoryPoint(
            time: distanceM > 0 ? TimeSpan.FromSeconds(distanceM / velocityMps) : TimeSpan.Zero,
            distance: new Measurement<DistanceUnit>(distanceM, DistanceUnit.Meter),
            distanceFlat: new Measurement<DistanceUnit>(distanceM, DistanceUnit.Meter),
            velocity: new Measurement<VelocityUnit>(velocityMps, VelocityUnit.MetersPerSecond),
            mach: mach,
            drop: new Measurement<DistanceUnit>(dropCm, DistanceUnit.Centimeter),
            dropFlat: new Measurement<DistanceUnit>(dropFlatCm, DistanceUnit.Centimeter),
            dropAdjustment: new Measurement<AngularUnit>(dropAdjMoa, AngularUnit.MOA),
            lineOfSightElevation: new Measurement<DistanceUnit>(losElevationCm, DistanceUnit.Centimeter),
            lineOfDepartureElevation: new Measurement<DistanceUnit>(0, DistanceUnit.Centimeter),
            windage: new Measurement<DistanceUnit>(windageCm, DistanceUnit.Centimeter),
            windageAdjustment: new Measurement<AngularUnit>(windageAdjMoa, AngularUnit.MOA),
            energy: new Measurement<EnergyUnit>(energyJoules, EnergyUnit.Joule),
            optimalGameWeight: new Measurement<WeightUnit>(0, WeightUnit.Kilogram)
        );
    }

    #endregion

    #region Trajectory Table Tests

    private void OnTableMeasurementSystemChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryTable == null || TableMeasurementSystemCombo == null) return;

        TrajectoryTable.MeasurementSystem = TableMeasurementSystemCombo.SelectedIndex == 0
            ? MeasurementSystem.Metric
            : MeasurementSystem.Imperial;

        TableInfoDisplay.Text = $"Changed measurement system to {TrajectoryTable.MeasurementSystem}. Column headers and values updated.";
    }

    private void OnTableAngularUnitChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryTable == null || TableAngularUnitCombo == null) return;

        TrajectoryTable.AngularUnits = TableAngularUnitCombo.SelectedIndex switch
        {
            0 => AngularUnit.MOA,
            1 => AngularUnit.MRad,
            2 => AngularUnit.Mil,
            _ => AngularUnit.MOA
        };

        TableInfoDisplay.Text = $"Changed angular units to {TrajectoryTable.AngularUnits}. Hold and Windage adjustment columns updated.";
    }

    private void OnTableDropBaseChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryTable == null || TableDropBaseCombo == null) return;

        TrajectoryTable.DropBase = TableDropBaseCombo.SelectedIndex == 0
            ? DropBase.SightLine
            : DropBase.MuzzlePoint;

        TableInfoDisplay.Text = $"Changed drop base to {TrajectoryTable.DropBase}. Drop column updated.";
    }

    private void OnVerticalClickChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryTable == null || VerticalClickCombo == null) return;

        TrajectoryTable.VerticalClick = VerticalClickCombo.SelectedIndex switch
        {
            0 => null,
            1 => new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
            2 => new Measurement<AngularUnit>(0.125, AngularUnit.MOA),
            3 => new Measurement<AngularUnit>(0.1, AngularUnit.MRad),
            _ => null
        };

        var clickText = TrajectoryTable.VerticalClick != null
            ? $"{TrajectoryTable.VerticalClick.Value.Value} {TrajectoryTable.VerticalClick.Value.Unit}"
            : "None";
        TableInfoDisplay.Text = $"Changed vertical click to {clickText}. Drop clicks column updated.";
    }

    private void OnHorizontalClickChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TrajectoryTable == null || HorizontalClickCombo == null) return;

        TrajectoryTable.HorizontalClick = HorizontalClickCombo.SelectedIndex switch
        {
            0 => null,
            1 => new Measurement<AngularUnit>(0.25, AngularUnit.MOA),
            2 => new Measurement<AngularUnit>(0.125, AngularUnit.MOA),
            3 => new Measurement<AngularUnit>(0.1, AngularUnit.MRad),
            _ => null
        };

        var clickText = TrajectoryTable.HorizontalClick != null
            ? $"{TrajectoryTable.HorizontalClick.Value.Value} {TrajectoryTable.HorizontalClick.Value.Unit}"
            : "None";
        TableInfoDisplay.Text = $"Changed horizontal click to {clickText}. Windage clicks column updated.";
    }

    private void OnLoadTableTrajectory(object? sender, RoutedEventArgs e)
    {
        // Use same sample trajectory as chart
        var trajectory = CreateSampleTrajectory();

        TrajectoryTable.SetTrajectory(trajectory);
        UpdateTableRowCount();

        TableInfoDisplay.Text = "Loaded sample trajectory: .308 Winchester 168gr HPBT\n" +
                                "Muzzle velocity: 800 m/s, zeroed at 100m\n" +
                                "Try changing units, angular, drop base, and click settings!";
    }

    private void OnClearTableTrajectory(object? sender, RoutedEventArgs e)
    {
        TrajectoryTable.Clear();
        UpdateTableRowCount();
        TableInfoDisplay.Text = "Table cleared.";
    }

    private void UpdateTableRowCount()
    {
        TableRowCountDisplay.Text = $"Rows: {TrajectoryTable.RowCount}";
    }

    #endregion
}