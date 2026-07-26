using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Models;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Tools;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Recovers a custom drag table from measured downrange velocities (radar or chronograph data) and saves it
/// as a <c>.drg</c> file. Weight, diameter and the atmosphere the data was measured in are physics inputs
/// here, not documentation: air density drives the recovered drag coefficients.
/// </summary>
public partial class DrgFromVelocitiesPanel : UserControl
{
    private readonly ObservableCollection<RadarReadingEditModel> _readings = new();
    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;
    private bool _loadingDetail;

    // The row the detail pane currently edits. Tracked separately from ReadingsList.SelectedItem so edits
    // can be committed into the row being left when the selection moves.
    private RadarReadingEditModel? _editing;

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

    /// <summary>Fills the header metadata from an ammunition (typically the active shot's).</summary>
    public Ammunition? Prefill
    {
        set
        {
            if (value == null)
                return;

            if (value.Weight.Value > 0)
                WeightControl.SetValue(value.Weight);
            if (value.BulletDiameter.HasValue && value.BulletDiameter.Value.Value > 0)
                DiameterControl.SetValue(value.BulletDiameter.Value);
            if (value.BulletLength.HasValue && value.BulletLength.Value.Value > 0)
                LengthControl.SetValue(value.BulletLength.Value);
        }
    }

    /// <summary>The atmosphere the readings were taken in; defaults to sea-level standard.</summary>
    public Atmosphere? Atmosphere
    {
        get => AtmosphereSubPanel.Atmosphere;
        set => AtmosphereSubPanel.Atmosphere = value;
    }

    /// <summary>The readings currently in the list, in list order.</summary>
    internal IReadOnlyList<RadarReadingEditModel> Readings => _readings;

    /// <summary>The message shown under the list; also the error surface for a refused import or save.</summary>
    internal string Status => StatusText.Text ?? "";

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

        foreach (var (unit, name) in Measurement<DistanceUnit>.GetUnitNames())
            CsvDistanceUnitCombo.Items.Add(new UnitItem(unit, name));
        foreach (var (unit, name) in Measurement<VelocityUnit>.GetUnitNames())
            CsvVelocityUnitCombo.Items.Add(new UnitItem(unit, name));

        SourceBox.Text = "radar data";
        ReadingsList.ItemsSource = _readings;
    }

    private void WireEvents()
    {
        // MeasurementControl raises Changed for both text and unit edits, so the detail pane needs no
        // property watching of its own.
        DistanceControl.Changed += (_, _) => OnDetailEdited();
        VelocityControl.Changed += (_, _) => OnDetailEdited();
    }

    private void ApplyMeasurementSystem()
    {
        var metric = _measurementSystem == MeasurementSystem.Metric;

        WeightControl.ChangeUnit(metric ? WeightUnit.Gram : WeightUnit.Grain);
        DiameterControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
        LengthControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
        DistanceControl.ChangeUnit(metric ? DistanceUnit.Meter : DistanceUnit.Yard);
        VelocityControl.ChangeUnit(metric ? VelocityUnit.MetersPerSecond : VelocityUnit.FeetPerSecond);
        AtmosphereSubPanel.MeasurementSystem = _measurementSystem;

        Select(CsvDistanceUnitCombo, metric ? DistanceUnit.Meter : DistanceUnit.Yard);
        Select(CsvVelocityUnitCombo, metric ? VelocityUnit.MetersPerSecond : VelocityUnit.FeetPerSecond);
    }

    #endregion

    #region Reading list

    private RadarReadingEditModel? Selected => ReadingsList.SelectedItem as RadarReadingEditModel;

    /// <summary>
    /// Copies the detail controls into the row they belong to. Called both from the controls' Changed event
    /// (so the list updates as the user types) and at every point that consumes the rows, so a value is
    /// never lost just because no change event arrived — a unit switch and a programmatic set are two cases
    /// where it does not.
    /// </summary>
    private void CommitDetail()
    {
        var target = _editing;
        if (target == null)
            return;

        var distance = DistanceControl.GetValue<DistanceUnit>();
        if (distance != null)
            target.Distance = distance.Value;

        var velocity = VelocityControl.GetValue<VelocityUnit>();
        if (velocity != null)
            target.Velocity = velocity.Value;

        target.Display = DisplayFor(target);
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        CommitDetail();

        // Continue the series: one step further out, a little slower — a plausible next row that only
        // needs the measured number typed over it.
        var last = _readings.LastOrDefault();
        var distanceUnit = CsvDistanceUnit;
        var step = distanceUnit == DistanceUnit.Meter ? 100 : 100;

        var reading = new RadarReadingEditModel
        {
            Distance = last == null
                ? new Measurement<DistanceUnit>(0, distanceUnit)
                : new Measurement<DistanceUnit>(last.Distance.In(distanceUnit) + step, distanceUnit),
            Velocity = last == null
                ? new Measurement<VelocityUnit>(CsvVelocityUnit == VelocityUnit.MetersPerSecond ? 850 : 2800, CsvVelocityUnit)
                : new Measurement<VelocityUnit>(last.Velocity.Value * 0.97, last.Velocity.Unit),
        };

        _readings.Add(reading);
        RefreshDisplay();
        ReadingsList.SelectedItem = reading;
        UpdateStatus();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
            return;

        _editing = null;                       // the row is going away; nothing to commit into
        var index = _readings.IndexOf(selected);
        _readings.Remove(selected);

        if (_readings.Count > 0)
            ReadingsList.SelectedIndex = Math.Min(index, _readings.Count - 1);

        UpdateStatus();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Save whatever is in the detail pane into the row being left before loading the new one.
        CommitDetail();

        var selected = Selected;
        _editing = selected;
        DetailPanel.IsEnabled = selected != null;

        _loadingDetail = true;
        try
        {
            if (selected == null)
            {
                DistanceControl.Value = null;
                VelocityControl.Value = null;
            }
            else
            {
                DistanceControl.SetValue(selected.Distance);
                VelocityControl.SetValue(selected.Velocity);
            }
        }
        finally
        {
            _loadingDetail = false;
        }
    }

    private void OnDetailEdited()
    {
        if (_loadingDetail || _editing == null)
            return;

        CommitDetail();
        UpdateStatus();
    }

    private void RefreshDisplay()
    {
        foreach (var reading in _readings)
            reading.Display = DisplayFor(reading);
    }

    private static string DisplayFor(RadarReadingEditModel reading) =>
        $"{reading.Distance}   {reading.Velocity}";

    #endregion

    #region Import

    private async void OnImport(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null)
            return;

        var path = await FileDialogService.OpenFileAsync(new FileDialogOptions
        {
            Title = "Import Measured Velocities",
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
    /// Reads a whole file or nothing: on any unusable line the list is left exactly as it was and the
    /// offending line is quoted in the status.
    /// </summary>
    internal void Import(string path)
    {
        CommitDetail();

        if (!CsvTextTableReader.TryReadFile(path, IsUsableRow, out var table, out var error))
        {
            ShowError(error);
            return;
        }

        var distanceColumn = DistanceColumnOf(table);

        var imported = new List<RadarReadingEditModel>(table.Rows.Count);
        foreach (var row in table.Rows)
        {
            var distanceText = distanceColumn == 0 ? row.First : row.Second;
            var velocityText = distanceColumn == 0 ? row.Second : row.First;

            if (!MeasurementTextParser.TryParseDistance(distanceText, CsvDistanceUnit, out var distance) ||
                !MeasurementTextParser.TryParseVelocity(velocityText, CsvVelocityUnit, out var velocity))
            {
                ShowError($"{System.IO.Path.GetFileName(path)}: line {row.LineNumber} could not be read as a " +
                          "distance and velocity pair. Nothing was imported.");
                return;
            }

            imported.Add(new RadarReadingEditModel { Distance = distance, Velocity = velocity });
        }

        _editing = null;
        _readings.Clear();
        foreach (var reading in imported.OrderBy(r => r.Distance.In(DistanceUnit.Meter)))
            _readings.Add(reading);
        RefreshDisplay();
        if (_readings.Count > 0)
            ReadingsList.SelectedIndex = 0;

        ShowInfo($"Imported {_readings.Count} reading{(_readings.Count == 1 ? "" : "s")} from " +
                 $"{System.IO.Path.GetFileName(path)}. {Range()}");
    }

    /// <summary>
    /// A row is usable if it reads as (distance, velocity) in either column order — the header decides
    /// which, but the reader must know a row is parseable before the header has been interpreted.
    /// </summary>
    private bool IsUsableRow(string first, string second) =>
        (MeasurementTextParser.TryParseDistance(first, CsvDistanceUnit, out _) &&
         MeasurementTextParser.TryParseVelocity(second, CsvVelocityUnit, out _)) ||
        (MeasurementTextParser.TryParseDistance(second, CsvDistanceUnit, out _) &&
         MeasurementTextParser.TryParseVelocity(first, CsvVelocityUnit, out _));

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

        ShowInfo($"Saved {System.IO.Path.GetFileName(path)} — {table.Count} points. " +
                 "Use it with a ballistic coefficient of 1.0 on table GC.");
    }

    /// <summary>Builds the table from the current inputs, throwing <see cref="ArgumentException"/> for the UI.</summary>
    internal DrgDragTable Build()
    {
        CommitDetail();
        return DragTableBuilder.FromRadarReadings(BuildMetadata(),
                                                  _readings.Select(r => new RadarReading(r.Distance, r.Velocity)),
                                                  Atmosphere);
    }

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

    #endregion

    #region Status

    private void UpdateStatus()
    {
        if (_readings.Count == 0)
        {
            ShowInfo($"No readings yet — add one or import a CSV. " +
                     $"At least {DragTableBuilder.MinimumRadarReadings} are needed.");
            return;
        }

        ShowInfo($"{_readings.Count} reading{(_readings.Count == 1 ? "" : "s")}. {Range()}");
    }

    private string Range()
    {
        if (_readings.Count == 0)
            return "";

        var ordered = _readings.OrderBy(r => r.Distance.In(DistanceUnit.Meter)).ToArray();
        return $"{ordered[0].Distance}–{ordered[^1].Distance}, " +
               $"{ordered[0].Velocity}→{ordered[^1].Velocity}.";
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

    #region Combo helpers

    private DistanceUnit CsvDistanceUnit =>
        (DistanceUnit)((CsvDistanceUnitCombo.SelectedItem as UnitItem)?.Unit ?? DistanceUnit.Yard);

    private VelocityUnit CsvVelocityUnit =>
        (VelocityUnit)((CsvVelocityUnitCombo.SelectedItem as UnitItem)?.Unit ?? VelocityUnit.FeetPerSecond);

    private static void Select(ComboBox combo, object unit)
    {
        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is UnitItem item && item.Unit.Equals(unit))
            {
                combo.SelectedIndex = i;
                return;
            }
        }
    }

    #endregion
}
