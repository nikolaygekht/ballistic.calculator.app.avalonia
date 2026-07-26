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
/// Knots are keyed by Mach because that is what <c>DrgDragTableFactory</c> takes, but they can be entered
/// and displayed as velocities — published multi-BC data usually comes in velocity bands. The model keeps
/// Mach, so switching the display mode never loses precision.
/// </para>
/// </summary>
public partial class DrgFromBcPanel : UserControl
{
    private const string ModeMach = "Mach";
    private const string ModeVelocity = "Velocity";

    private readonly ObservableCollection<BcKnotEditModel> _knots = new();
    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;
    private bool _loadingDetail;

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

    /// <summary>The knots currently in the list, in list order.</summary>
    internal IReadOnlyList<BcKnotEditModel> Knots => _knots;

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

        // GC is excluded: the base curve must be a standard table.
        foreach (var id in Enum.GetValues<DragTableId>().Where(id => id != DragTableId.GC).OrderBy(id => id.ToString()))
            BaseTableCombo.Items.Add(new DragTableInfo(id, id.ToString()));
        SelectBaseTable(DragTableId.G7);

        KnotModeCombo.Items.Add(ModeMach);
        KnotModeCombo.Items.Add(ModeVelocity);
        KnotModeCombo.SelectedIndex = 0;

        foreach (var (unit, name) in Measurement<VelocityUnit>.GetUnitNames())
            VelocityUnitCombo.Items.Add(new UnitItem(unit, name));
        VelocityUnitCombo.SelectedIndex = 0;
        VelocityUnitCombo.IsVisible = false;

        SourceBox.Text = "BC curve";
        KnotsList.ItemsSource = _knots;
    }

    private void WireEvents()
    {
        KnotModeCombo.SelectionChanged += (_, _) => ApplyKnotMode();
        VelocityUnitCombo.SelectionChanged += (_, _) => RefreshKnotDisplay();

        // Watch the Text property rather than TextChanged: the event is not raised for programmatic text
        // in headless mode (see the note in MeasurementControlTests), while property changes always are.
        XValueBox.PropertyChanged += OnDetailBoxPropertyChanged;
        BcBox.PropertyChanged += OnDetailBoxPropertyChanged;
    }

    private void OnDetailBoxPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBox.TextProperty)
            OnDetailEdited();
    }

    private void ApplyMeasurementSystem()
    {
        var metric = _measurementSystem == MeasurementSystem.Metric;

        WeightControl.ChangeUnit(metric ? WeightUnit.Gram : WeightUnit.Grain);
        DiameterControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
        LengthControl.ChangeUnit(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch);
        SelectVelocityUnit(metric ? VelocityUnit.MetersPerSecond : VelocityUnit.FeetPerSecond);
    }

    #endregion

    #region Knot list

    private BcKnotEditModel? Selected => KnotsList.SelectedItem as BcKnotEditModel;

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        // A new knot continues the curve: one step past the last Mach, same BC, so only one field needs
        // typing in the common case.
        var last = _knots.LastOrDefault();
        var knot = new BcKnotEditModel
        {
            Mach = last == null ? 1.5 : last.Mach + 0.25,
            Bc = last?.Bc ?? 0.5,
        };

        _knots.Add(knot);
        RefreshKnotDisplay();
        KnotsList.SelectedItem = knot;
        UpdateStatus();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        var selected = Selected;
        if (selected == null)
            return;

        var index = _knots.IndexOf(selected);
        _knots.Remove(selected);

        if (_knots.Count > 0)
            KnotsList.SelectedIndex = Math.Min(index, _knots.Count - 1);

        UpdateStatus();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = Selected;
        DetailPanel.IsEnabled = selected != null;

        _loadingDetail = true;
        try
        {
            XValueBox.Text = selected == null ? "" : FormatXValue(selected);
            BcBox.Text = selected == null ? "" : Format(selected.Bc, 4);
        }
        finally
        {
            _loadingDetail = false;
        }
    }

    /// <summary>
    /// Detail edits write straight back into the selected model (the direct-UI-access pattern) so there is
    /// no commit step to forget. Unparseable text is simply not stored — validation happens on save.
    /// </summary>
    private void OnDetailEdited()
    {
        var selected = Selected;
        if (_loadingDetail || selected == null)
            return;

        if (MeasurementTextParser.TryParseDouble(XValueBox.Text, out var x))
            selected.Mach = IsVelocityMode ? DragTableBuilder.VelocityToMach(new Measurement<VelocityUnit>(x, SelectedVelocityUnit)) : x;

        if (MeasurementTextParser.TryParseDouble(BcBox.Text, out var bc))
            selected.Bc = bc;

        selected.Display = DisplayFor(selected);
        UpdateStatus();
    }

    private void ApplyKnotMode()
    {
        VelocityUnitCombo.IsVisible = IsVelocityMode;
        XValueLabel.Text = IsVelocityMode ? "Velocity:" : "Mach:";
        RefreshKnotDisplay();

        // Re-render the detail box in the new mode without treating it as an edit.
        OnSelectionChanged(null, null!);
    }

    private void RefreshKnotDisplay()
    {
        foreach (var knot in _knots)
            knot.Display = DisplayFor(knot);
    }

    private string DisplayFor(BcKnotEditModel knot) =>
        IsVelocityMode
            ? $"{Format(DragTableBuilder.MachToVelocity(knot.Mach, SelectedVelocityUnit).Value, 1)} {UnitName(SelectedVelocityUnit)}   BC {Format(knot.Bc, 4)}"
            : $"M {Format(knot.Mach, 4)}   BC {Format(knot.Bc, 4)}";

    private string FormatXValue(BcKnotEditModel knot) =>
        IsVelocityMode
            ? Format(DragTableBuilder.MachToVelocity(knot.Mach, SelectedVelocityUnit).Value, 1)
            : Format(knot.Mach, 4);

    #endregion

    #region Import

    private async void OnImport(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null)
            return;

        var path = await FileDialogService.OpenFileAsync(new FileDialogOptions
        {
            Title = "Import BC Curve",
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
    /// offending line is quoted in the status. A curve silently missing a knot looks plausible and is wrong.
    /// </summary>
    internal void Import(string path)
    {
        if (!CsvTextTableReader.TryReadFile(path, IsUsableRow, out var table, out var error))
        {
            ShowError(error);
            return;
        }

        var (xRole, _) = MapColumns(table);

        var imported = new List<BcKnotEditModel>(table.Rows.Count);
        DragTableId? fileTable = null;
        var mixedTables = false;

        foreach (var row in table.Rows)
        {
            var xText = xRole == 0 ? row.First : row.Second;
            var bcText = xRole == 0 ? row.Second : row.First;

            if (!TryParseX(xText, out var mach) || !MeasurementTextParser.TryParseBc(bcText, SelectedBaseTable, out var bc))
            {
                // TryRead already accepted every row under one of the roles; a failure here means the
                // header pointed at the wrong columns.
                ShowError($"{System.IO.Path.GetFileName(path)}: line {row.LineNumber} could not be read as " +
                          "a Mach/velocity and BC pair. Nothing was imported.");
                return;
            }

            if (fileTable == null)
                fileTable = bc.Table;
            else if (fileTable != bc.Table)
                mixedTables = true;

            imported.Add(new BcKnotEditModel { Mach = mach, Bc = bc.Value });
        }

        _knots.Clear();
        foreach (var knot in imported.OrderBy(k => k.Mach))
            _knots.Add(knot);
        RefreshKnotDisplay();
        if (_knots.Count > 0)
            KnotsList.SelectedIndex = 0;

        var note = "";
        if (mixedTables)
        {
            note = " The file names more than one drag table; the base table was left unchanged.";
        }
        else if (fileTable != null && fileTable != DragTableId.GC)
        {
            SelectBaseTable(fileTable.Value);
            note = $" Base table {fileTable} taken from the file.";
        }

        ShowInfo($"Imported {_knots.Count} knot{(_knots.Count == 1 ? "" : "s")} from " +
                 $"{System.IO.Path.GetFileName(path)}.{note}");
    }

    /// <summary>
    /// A row is usable if it reads as (Mach|velocity, BC) in either column order — the header decides which,
    /// but the reader needs to know a row is parseable before the header has been interpreted.
    /// </summary>
    private bool IsUsableRow(string first, string second) =>
        (TryParseX(first, out _) && MeasurementTextParser.TryParseBc(second, SelectedBaseTable, out _)) ||
        (TryParseX(second, out _) && MeasurementTextParser.TryParseBc(first, SelectedBaseTable, out _));

    private bool TryParseX(string text, out double mach)
    {
        mach = 0;

        // An explicit velocity unit wins over the display mode: a file saying "2700ft/s" is unambiguous.
        if (MeasurementTextParser.TryParseVelocity(text, SelectedVelocityUnit, out var velocity) &&
            text.Any(char.IsLetter))
        {
            mach = DragTableBuilder.VelocityToMach(velocity);
            return true;
        }

        if (!MeasurementTextParser.TryParseDouble(text, out var value) || value <= 0)
            return false;

        mach = IsVelocityMode
            ? DragTableBuilder.VelocityToMach(new Measurement<VelocityUnit>(value, SelectedVelocityUnit))
            : value;
        return true;
    }

    /// <summary>
    /// Decides which column holds the Mach/velocity value: the header when it names the columns, otherwise
    /// the documented default order (mach;bc). Returns the column index of each role.
    /// </summary>
    private (int XColumn, int BcColumn) MapColumns(CsvTextTable table)
    {
        var first = (table.HeaderFirst ?? "").ToLowerInvariant();
        var second = (table.HeaderSecond ?? "").ToLowerInvariant();

        static bool IsBc(string h) => h.Contains("bc") || h.Contains("coefficient");
        static bool IsX(string h) => h.Contains("mach") || h.Contains("vel") || h.Contains("speed");

        if (IsBc(first) || IsX(second))
            return (1, 0);

        return (0, 1);
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
    internal DrgDragTable Build() =>
        DragTableBuilder.FromBcCurve(BuildMetadata(), SelectedBaseTable,
                                     _knots.Select(k => new BcAtMach(k.Mach, k.Bc)));

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

    #endregion

    #region Status

    private void UpdateStatus()
    {
        if (_knots.Count == 0)
        {
            ShowInfo("No knots yet — add one or import a CSV.");
            return;
        }

        var machs = _knots.Select(k => k.Mach).ToArray();
        ShowInfo($"{_knots.Count} knot{(_knots.Count == 1 ? "" : "s")}, " +
                 $"Mach {Format(machs.Min(), 4)}–{Format(machs.Max(), 4)}.");
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

    private bool IsVelocityMode => (KnotModeCombo.SelectedItem as string) == ModeVelocity;

    private VelocityUnit SelectedVelocityUnit =>
        (VelocityUnit)((VelocityUnitCombo.SelectedItem as UnitItem)?.Unit ?? VelocityUnit.FeetPerSecond);

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

    private void SelectVelocityUnit(VelocityUnit unit)
    {
        for (int i = 0; i < VelocityUnitCombo.Items.Count; i++)
        {
            if (VelocityUnitCombo.Items[i] is UnitItem item && item.Unit is VelocityUnit u && u.Equals(unit))
            {
                VelocityUnitCombo.SelectedIndex = i;
                return;
            }
        }
    }

    private static string UnitName(VelocityUnit unit) =>
        Measurement<VelocityUnit>.GetUnitNames().FirstOrDefault(t => t.Item1.Equals(unit))?.Item2 ?? "";

    private static string Format(double value, int decimals) =>
        Math.Round(value, decimals).ToString("0.####", CultureInfo.CurrentCulture);

    #endregion
}
