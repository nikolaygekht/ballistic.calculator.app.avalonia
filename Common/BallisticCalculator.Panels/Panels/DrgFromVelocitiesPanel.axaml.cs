using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Panels.Models;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Tools;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Recovers a custom drag table from measured downrange velocities (radar or chronograph data) and saves it
/// as a <c>.drg</c> file. Weight, diameter and the atmosphere the data was measured in are physics inputs
/// here rather than documentation: air density drives the recovered drag coefficients.
/// </summary>
public partial class DrgFromVelocitiesPanel : UserControl
{
    private readonly ObservableCollection<RadarReadingEditModel> _readings = new();
    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;
    private Atmosphere? _atmosphere;

    public DrgFromVelocitiesPanel()
    {
        InitializeComponent();
        InitializeControls();
        WireEvents();
        ApplyMeasurementSystem();
        UpdateStatus();
    }

    #region Properties

    public IFileDialogService? FileDialogService { get; set; }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            ApplyMeasurementSystem();
        }
    }

    /// <summary>
    /// The air the readings were measured in. Null means sea-level standard; the Set Atmosphere button asks
    /// the host to edit it, because the same velocities recorded in thinner air mean a different drag curve.
    /// </summary>
    public Atmosphere? Atmosphere
    {
        get => _atmosphere;
        set
        {
            _atmosphere = value;
            UpdateStatus();
        }
    }

    /// <summary>The readings currently in the grid, in grid order.</summary>
    internal IReadOnlyList<RadarReadingEditModel> Readings => _readings;

    /// <summary>The message under the buttons; also the error surface for a refused import or save.</summary>
    internal string Status => StatusText.Text ?? "";

    #endregion

    #region Events

    /// <summary>Raised by the Close button; the hosting window closes itself.</summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Raised by Set Atmosphere. The host shows the atmosphere editor and writes the result back to
    /// <see cref="Atmosphere"/> — windows belong to the application, not to a panel library.
    /// </summary>
    public event EventHandler? AtmosphereRequested;

    #endregion

    #region Setup

    private void InitializeControls()
    {
        WeightControl.UnitType = typeof(WeightUnit);
        WeightControl.Minimum = 0;
        DiameterControl.UnitType = typeof(DistanceUnit);
        DiameterControl.Minimum = 0;
        DiameterControl.DecimalPoints = 3;
        LengthControl.UnitType = typeof(DistanceUnit);
        LengthControl.Minimum = 0;
        LengthControl.DecimalPoints = 3;
        DistanceControl.UnitType = typeof(DistanceUnit);
        DistanceControl.Minimum = 0;
        VelocityControl.UnitType = typeof(VelocityUnit);
        VelocityControl.Minimum = 0;
        VelocityControl.DecimalPoints = 1;

        SourceBox.Text = "radar data";
        ReadingsGrid.ItemsSource = _readings;
    }

    private void WireEvents()
    {
        ReadingsGrid.SelectionChanged += OnSelectionChanged;
    }

    private void ApplyMeasurementSystem()
    {
        var metric = _measurementSystem == MeasurementSystem.Metric;

        WeightControl.ChangeUnit(metric ? WeightUnit.Gram : WeightUnit.Grain);
        DiameterControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
        LengthControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
        DistanceControl.ChangeUnit(metric ? DistanceUnit.Meter : DistanceUnit.Yard);
        VelocityControl.ChangeUnit(metric ? VelocityUnit.MetersPerSecond : VelocityUnit.FeetPerSecond);
    }

    #endregion

    #region Reading list

    private RadarReadingEditModel? Selected => ReadingsGrid.SelectedItem as RadarReadingEditModel;

    /// <summary>Selecting a row loads it into the entry fields, where Change writes it back.</summary>
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
            return;

        DistanceControl.SetValue(selected.Distance);
        VelocityControl.SetValue(selected.Velocity);
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        if (!TryReadEntry(out var distance, out var velocity, out var problem))
        {
            ShowError(problem);
            return;
        }

        var reading = new RadarReadingEditModel { Distance = distance, Velocity = velocity };
        _readings.Add(reading);
        ReadingsGrid.SelectedItem = reading;
        UpdateStatus();
    }

    private void OnChange(object? sender, RoutedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
        {
            ShowError("Select the reading to change first.");
            return;
        }

        if (!TryReadEntry(out var distance, out var velocity, out var problem))
        {
            ShowError(problem);
            return;
        }

        selected.Distance = distance;
        selected.Velocity = velocity;
        UpdateStatus();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
        {
            ShowError("Select the reading to delete first.");
            return;
        }

        var index = _readings.IndexOf(selected);
        _readings.Remove(selected);

        if (_readings.Count > 0)
            ReadingsGrid.SelectedIndex = Math.Min(index, _readings.Count - 1);

        UpdateStatus();
    }

    private void OnSort(object? sender, RoutedEventArgs e)
    {
        var sorted = _readings.OrderBy(r => r.Distance.In(DistanceUnit.Meter)).ToArray();
        _readings.Clear();
        foreach (var reading in sorted)
            _readings.Add(reading);

        UpdateStatus();
    }

    private bool TryReadEntry(out Measurement<DistanceUnit> distance, out Measurement<VelocityUnit> velocity,
                              out string problem)
    {
        distance = default;
        velocity = default;
        problem = "";

        var d = DistanceControl.IsEmpty ? null : DistanceControl.GetValue<DistanceUnit>();
        if (d == null)
        {
            problem = "Enter the distance of the reading.";
            return false;
        }

        var v = VelocityControl.IsEmpty ? null : VelocityControl.GetValue<VelocityUnit>();
        if (v == null || v.Value.Value <= 0)
        {
            problem = "Enter a velocity greater than zero.";
            return false;
        }

        distance = d.Value;
        velocity = v.Value;
        return true;
    }

    #endregion

    #region Import

    private async void OnImport(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null)
            return;

        var path = await FileDialogService.OpenFileAsync(new FileDialogOptions
        {
            Title = "Load Measured Velocities",
            DefaultExtension = "csv",
            Filters =
            {
                new Services.FileDialogFilter("CSV Files", "csv", "txt"),
                new Services.FileDialogFilter("All Files", "*"),
            },
        });

        if (path == null)
            return;

        Import(path);
    }

    /// <summary>
    /// Reads a whole file or nothing: on any unusable line the grid is left exactly as it was and the
    /// offending line is quoted.
    /// </summary>
    internal void Import(string path)
    {
        if (!CsvTextTableReader.TryReadFile(path, RowProblem, out var table, out var error))
        {
            ShowError(error);
            return;
        }

        var distanceFirst = DistanceColumnOf(table) == 0;

        var imported = new List<RadarReadingEditModel>(table.Rows.Count);
        foreach (var row in table.Rows)
        {
            var distanceText = distanceFirst ? row.First : row.Second;
            var velocityText = distanceFirst ? row.Second : row.First;

            if (!MeasurementTextParser.TryParseDistance(distanceText, null, out var distance) ||
                !MeasurementTextParser.TryParseVelocity(velocityText, null, out var velocity))
            {
                ShowError($"{System.IO.Path.GetFileName(path)}: line {row.LineNumber} could not be read as a " +
                          "distance and velocity pair. Nothing was imported.");
                return;
            }

            imported.Add(new RadarReadingEditModel { Distance = distance, Velocity = velocity });
        }

        _readings.Clear();
        foreach (var reading in imported.OrderBy(r => r.Distance.In(DistanceUnit.Meter)))
            _readings.Add(reading);
        if (_readings.Count > 0)
            ReadingsGrid.SelectedIndex = 0;

        ShowInfo($"Loaded {_readings.Count} reading{(_readings.Count == 1 ? "" : "s")} from " +
                 $"{System.IO.Path.GetFileName(path)}. {Range()}");
    }

    /// <summary>
    /// A row is usable when one field is a distance and the other a velocity, <b>each with its own unit</b>.
    /// A bare number is refused rather than assumed: reading a yards file as metres yields a plausible curve
    /// that is quietly wrong, and the file is the only place that knows which it is.
    /// </summary>
    private string? RowProblem(string first, string second)
    {
        if (IsPair(first, second) || IsPair(second, first))
            return null;

        return "expected a distance and a velocity, each with its unit (for example 100yd;3001.2ft/s)";
    }

    private static bool IsPair(string distanceText, string velocityText) =>
        MeasurementTextParser.TryParseDistance(distanceText, null, out _) &&
        MeasurementTextParser.TryParseVelocity(velocityText, null, out _);

    /// <summary>
    /// Which column holds the distance: the header when it names the columns, otherwise the documented
    /// default order (distance;velocity). A header naming velocity first transposes the pair.
    /// </summary>
    private static int DistanceColumnOf(CsvTextTable table)
    {
        var first = (table.HeaderFirst ?? "").ToLowerInvariant();
        var second = (table.HeaderSecond ?? "").ToLowerInvariant();

        static bool IsVelocity(string h) => h.Contains("vel") || h.Contains("speed") || h.Contains("mv");
        static bool IsDistance(string h) => h.Contains("dist") || h.Contains("range") || h.Contains("yard") ||
                                            h.Contains("meter") || h.Contains("metre");

        if (IsVelocity(first) || IsDistance(second))
            return 1;

        return 0;
    }

    #endregion

    #region Save

    private void OnSetAtmosphere(object? sender, RoutedEventArgs e) =>
        AtmosphereRequested?.Invoke(this, EventArgs.Empty);

    private async void OnSave(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null)
            return;

        DrgDragTable table;
        try
        {
            table = Build();
        }
        catch (ArgumentException ex)
        {
            ShowError(ex.Message);
            return;
        }

        var path = await FileDialogService.SaveFileAsync(new FileDialogOptions
        {
            Title = "Save Custom Drag Table",
            DefaultExtension = "drg",
            InitialDirectory = DataFolders.Drg,
            InitialFileName = SuggestFileName(),
            Filters = { new Services.FileDialogFilter("Custom Drag Table", "drg") },
        });

        if (path == null)
            return;

        try
        {
            table.Save(path);
        }
        catch (Exception ex)
        {
            ShowError($"Could not save {System.IO.Path.GetFileName(path)}: {ex.Message}");
            return;
        }

        ShowInfo($"Saved {System.IO.Path.GetFileName(path)} — {table.Count} points.");
    }

    /// <summary>Builds the table from the current inputs, throwing <see cref="ArgumentException"/> for the UI.</summary>
    internal DrgDragTable Build() =>
        DragTableBuilder.FromRadarReadings(BuildMetadata(),
                                           _readings.Select(r => new RadarReading(r.Distance, r.Velocity)),
                                           _atmosphere);

    internal DrgMetadata BuildMetadata() => new(
        NameBox.Text ?? "",
        SourceBox.Text,
        WeightControl.IsEmpty ? null : WeightControl.GetValue<WeightUnit>(),
        DiameterControl.IsEmpty ? null : DiameterControl.GetValue<DistanceUnit>(),
        LengthControl.IsEmpty ? null : LengthControl.GetValue<DistanceUnit>());

    private string SuggestFileName()
    {
        var name = (NameBox.Text ?? "").Trim();
        foreach (var invalid in System.IO.Path.GetInvalidFileNameChars())
            name = name.Replace(invalid, ' ');
        return string.IsNullOrWhiteSpace(name) ? "radar.drg" : name + ".drg";
    }

    private void OnClose(object? sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    #endregion

    #region Status

    private void UpdateStatus()
    {
        var air = _atmosphere == null
            ? "standard atmosphere"
            : $"{_atmosphere.Altitude.ToString("ND", System.Globalization.CultureInfo.CurrentCulture)}, " +
              $"{_atmosphere.Temperature.ToString("ND", System.Globalization.CultureInfo.CurrentCulture)}";

        if (_readings.Count == 0)
        {
            ShowInfo($"No readings yet — add one or load a CSV with units (100yd;3001.2ft/s). " +
                     $"At least {DragTableBuilder.MinimumRadarReadings} are needed. Measured in {air}.");
            return;
        }

        ShowInfo($"{_readings.Count} reading{(_readings.Count == 1 ? "" : "s")}. {Range()} Measured in {air}.");
    }

    private string Range()
    {
        if (_readings.Count == 0)
            return "";

        var ordered = _readings.OrderBy(r => r.Distance.In(DistanceUnit.Meter)).ToArray();
        return $"{ordered[0].DistanceText}–{ordered[^1].DistanceText}, " +
               $"{ordered[0].VelocityText}→{ordered[^1].VelocityText}.";
    }

    private void ShowInfo(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = Avalonia.Media.Brushes.Gray;
    }

    private void ShowError(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = Avalonia.Media.Brushes.Firebrick;
    }

    #endregion
}
