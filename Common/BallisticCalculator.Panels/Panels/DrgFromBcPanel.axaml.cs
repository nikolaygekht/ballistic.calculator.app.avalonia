using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Models;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Builds a custom drag table from a BC-vs-Mach curve and saves it as a <c>.drg</c> file.
/// <para>
/// Knots are always keyed by Mach — that is what <c>DrgDragTableFactory</c> takes, and a velocity would need
/// a reference atmosphere to be meaningful. Each knot keeps the drag table its coefficient was quoted
/// against, so a curve typed from a report records what the report said; knots quoted against another table
/// are converted to the base table at their own Mach when the table is built.
/// </para>
/// </summary>
public partial class DrgFromBcPanel : UserControl
{
    private readonly ObservableCollection<BcKnotEditModel> _knots = new();
    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;

    public DrgFromBcPanel()
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

    /// <summary>The knots currently in the grid, in grid order.</summary>
    internal IReadOnlyList<BcKnotEditModel> Knots => _knots;

    /// <summary>The message under the buttons; also the error surface for a refused import or save.</summary>
    internal string Status => StatusText.Text ?? "";

    #endregion

    #region Events

    /// <summary>Raised by the Close button; the hosting window closes itself.</summary>
    public event EventHandler? CloseRequested;

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

        // GC is excluded: the base curve must be a standard table.
        foreach (var id in Enum.GetValues<DragTableId>().Where(id => id != DragTableId.GC).OrderBy(id => id.ToString()))
            BaseTableCombo.Items.Add(new DragTableInfo(id, id.ToString()));
        SelectBaseTable(DragTableId.G7);

        SourceBox.Text = "BC curve";
        KnotsGrid.ItemsSource = _knots;
    }

    private void WireEvents()
    {
        KnotsGrid.SelectionChanged += OnSelectionChanged;
    }

    private void ApplyMeasurementSystem()
    {
        var metric = _measurementSystem == MeasurementSystem.Metric;

        WeightControl.ChangeUnit(metric ? WeightUnit.Gram : WeightUnit.Grain);
        DiameterControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
        LengthControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
    }

    #endregion

    #region Knot list

    private BcKnotEditModel? Selected => KnotsGrid.SelectedItem as BcKnotEditModel;

    /// <summary>Selecting a row loads it into the entry fields, where Change writes it back.</summary>
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
            return;

        MachBox.Text = selected.MachText;
        BcControl.Value = selected.Bc;
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        if (!TryReadEntry(out var mach, out var bc, out var problem))
        {
            ShowError(problem);
            return;
        }

        var knot = new BcKnotEditModel { Mach = mach, Bc = bc };
        _knots.Add(knot);
        KnotsGrid.SelectedItem = knot;
        UpdateStatus();
    }

    private void OnChange(object? sender, RoutedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
        {
            ShowError("Select the knot to change first.");
            return;
        }

        if (!TryReadEntry(out var mach, out var bc, out var problem))
        {
            ShowError(problem);
            return;
        }

        selected.Mach = mach;
        selected.Bc = bc;
        UpdateStatus();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
        {
            ShowError("Select the knot to delete first.");
            return;
        }

        var index = _knots.IndexOf(selected);
        _knots.Remove(selected);

        if (_knots.Count > 0)
            KnotsGrid.SelectedIndex = Math.Min(index, _knots.Count - 1);

        UpdateStatus();
    }

    private void OnSort(object? sender, RoutedEventArgs e)
    {
        var sorted = _knots.OrderBy(k => k.Mach).ToArray();
        _knots.Clear();
        foreach (var knot in sorted)
            _knots.Add(knot);

        UpdateStatus();
    }

    /// <summary>
    /// Reads the entry fields. Mach carries no unit, so it is a plain number; the coefficient comes from the
    /// BC control with its own drag table.
    /// </summary>
    private bool TryReadEntry(out double mach, out BallisticCoefficient bc, out string problem)
    {
        mach = 0;
        bc = default;
        problem = "";

        if (!MeasurementTextParser.TryParseDouble(MachBox.Text, out mach) || mach <= 0)
        {
            problem = "Enter a Mach number greater than zero.";
            return false;
        }

        var value = BcControl.Value;
        if (value == null || value.Value.Value <= 0)
        {
            problem = "Enter a ballistic coefficient greater than zero.";
            return false;
        }

        bc = value.Value;
        if (bc.Table == DragTableId.GC)
        {
            problem = "A knot must be quoted against a standard table (G1…RA4), not GC.";
            return false;
        }

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
            Title = "Load BC Curve",
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
    /// offending line is quoted. A curve silently missing a knot looks plausible and is wrong.
    /// </summary>
    internal void Import(string path)
    {
        if (!CsvTextTableReader.TryReadFile(path, RowProblem, out var table, out var error))
        {
            ShowError(error);
            return;
        }

        var machFirst = MachColumnOf(table) == 0;

        var imported = new List<BcKnotEditModel>(table.Rows.Count);
        foreach (var row in table.Rows)
        {
            var machText = machFirst ? row.First : row.Second;
            var bcText = machFirst ? row.Second : row.First;

            if (!MeasurementTextParser.TryParseDouble(machText, out var mach) ||
                !MeasurementTextParser.TryParseBc(bcText, SelectedBaseTable, out var bc))
            {
                ShowError($"{System.IO.Path.GetFileName(path)}: line {row.LineNumber} could not be read as a " +
                          "Mach and BC pair. Nothing was imported.");
                return;
            }

            imported.Add(new BcKnotEditModel { Mach = mach, Bc = bc });
        }

        _knots.Clear();
        foreach (var knot in imported.OrderBy(k => k.Mach))
            _knots.Add(knot);
        if (_knots.Count > 0)
            KnotsGrid.SelectedIndex = 0;

        // When every coefficient names the same table, that is the curve they belong to — adopt it rather
        // than making the user match the combo to the file by hand.
        var tables = _knots.Select(k => k.Bc.Table).Distinct().ToArray();
        var note = "";
        if (tables.Length == 1 && tables[0] != DragTableId.GC)
        {
            SelectBaseTable(tables[0]);
            note = $" Base table {tables[0]} taken from the file.";
        }
        else if (tables.Length > 1)
        {
            note = $" Knots name {tables.Length} different tables; they will be converted to {SelectedBaseTable} on save.";
        }

        ShowInfo($"Loaded {_knots.Count} knot{(_knots.Count == 1 ? "" : "s")} from " +
                 $"{System.IO.Path.GetFileName(path)}.{note}");
    }

    /// <summary>
    /// A row is usable when one field is a plain number (the Mach) and the other a coefficient. Mach has no
    /// unit, so a bare number is expected here — unlike the velocities editor, nothing has to be guessed.
    /// </summary>
    private string? RowProblem(string first, string second)
    {
        if (IsPair(first, second) || IsPair(second, first))
            return null;

        return "expected a Mach number and a ballistic coefficient (for example 1.5;0.462G7)";
    }

    // Both columns are bare numbers here, so plausibility is the only thing separating them: a drag curve
    // runs to about Mach 5, and no ballistic coefficient reaches 5. Without the bounds, a velocity-keyed
    // file ("2700;0.307") would be read as Mach 2700 — or transposed into Mach 0.307 with a BC of 2700.
    private const double MaximumPlausibleMach = 10;
    private const double MaximumPlausibleBc = 5;

    private bool IsPair(string machText, string bcText) =>
        MeasurementTextParser.TryParseDouble(machText, out var mach) && mach > 0 && mach < MaximumPlausibleMach &&
        MeasurementTextParser.TryParseBc(bcText, SelectedBaseTable, out var bc) && bc.Value <= MaximumPlausibleBc;

    /// <summary>
    /// Which column holds the Mach: the header when it names the columns, otherwise the documented default
    /// order (mach;bc). A header naming BC first transposes the pair.
    /// </summary>
    private static int MachColumnOf(CsvTextTable table)
    {
        var first = (table.HeaderFirst ?? "").ToLowerInvariant();
        var second = (table.HeaderSecond ?? "").ToLowerInvariant();

        static bool IsBc(string h) => h.Contains("bc") || h.Contains("coefficient");
        static bool IsMach(string h) => h.Contains("mach");

        if (IsBc(first) || IsMach(second))
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
        int converted;
        try
        {
            table = Build(out converted);
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

        var note = converted == 0
            ? ""
            : $" {converted} knot{(converted == 1 ? "" : "s")} converted to {SelectedBaseTable} at their own Mach.";
        ShowInfo($"Saved {System.IO.Path.GetFileName(path)} — {table.Count} points.{note}");
    }

    /// <summary>Builds the table from the current inputs, throwing <see cref="ArgumentException"/> for the UI.</summary>
    internal DrgDragTable Build(out int converted)
    {
        var curve = DragTableBuilder.NormalizeCurve(
            _knots.Select(k => (k.Mach, k.Bc)), SelectedBaseTable, out converted);

        return DragTableBuilder.FromBcCurve(BuildMetadata(), SelectedBaseTable, curve);
    }

    internal DrgDragTable Build() => Build(out _);

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
        return string.IsNullOrWhiteSpace(name) ? "custom.drg" : name + ".drg";
    }

    private void OnClose(object? sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    #endregion

    #region Status

    private void UpdateStatus()
    {
        if (_knots.Count == 0)
        {
            ShowInfo("No knots yet — add one or load a CSV. A CSV without a header is read as mach;bc.");
            return;
        }

        var machs = _knots.Select(k => k.Mach).ToArray();
        var tables = _knots.Select(k => k.Bc.Table).Distinct().ToArray();
        var tableNote = tables.Length == 1
            ? $"against {tables[0]}"
            : $"against {string.Join(", ", tables)} — converted to {SelectedBaseTable} on save";

        ShowInfo($"{_knots.Count} knot{(_knots.Count == 1 ? "" : "s")}, " +
                 $"Mach {Format(machs.Min())}–{Format(machs.Max())}, {tableNote}.");
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

    #region Helpers

    private DragTableId SelectedBaseTable =>
        (BaseTableCombo.SelectedItem as DragTableInfo)?.Value ?? DragTableId.G7;

    private void SelectBaseTable(DragTableId id)
    {
        for (int i = 0; i < BaseTableCombo.Items.Count; i++)
        {
            if (BaseTableCombo.Items[i] is DragTableInfo info && info.Value == id)
            {
                BaseTableCombo.SelectedIndex = i;
                return;
            }
        }
    }

    private static string Format(double value) => value.ToString("0.####", CultureInfo.CurrentCulture);

    #endregion
}
