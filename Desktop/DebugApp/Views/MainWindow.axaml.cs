using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using Gehtsoft.Measurements;
using System.Linq;

namespace DebugApp.Views;

public partial class MainWindow : Window
{
    private int _basicBCChangeCount = 0;
    private int _distanceChangeCount = 0;

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
}