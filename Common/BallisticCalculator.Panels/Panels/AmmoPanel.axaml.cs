using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.Collections.Generic;
using System.IO;

[assembly: InternalsVisibleTo("BallisticCalculator.Panels.Tests")]

namespace BallisticCalculator.Panels.Panels;

public partial class AmmoPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;
    private string? _customTableFileName;

    public AmmoPanel()
    {
        InitializeComponent();
        InitializeControls();
        WireEvents();
        ApplyMeasurementSystem();
    }

    #region Properties

    public bool ConvertOnSystemChange { get; set; }

    /// <summary>Service used by the "Browse..." button to pick a custom drag table (.drg).</summary>
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

    public Ammunition? Ammunition
    {
        get
        {
            var weight = WeightControl.GetValue<WeightUnit>();
            var bc = BCControl.Value;
            var velocity = MuzzleVelocityControl.GetValue<VelocityUnit>();

            if (weight == null || bc == null || velocity == null)
                return null;

            var ammo = new Ammunition()
            {
                Weight = weight.Value,
                BallisticCoefficient = bc.Value,
                MuzzleVelocity = velocity.Value,
            };

            if (FormFactorCheckBox.IsChecked == true)
            {
                ammo.BallisticCoefficient = new BallisticCoefficient(
                    bc.Value.Value, bc.Value.Table, BallisticCoefficientValueType.FormFactor);
            }

            var diameter = BulletDiameterControl.GetValue<DistanceUnit>();
            if (diameter != null && !BulletDiameterControl.IsEmpty)
                ammo.BulletDiameter = diameter.Value;

            var length = BulletLengthControl.GetValue<DistanceUnit>();
            if (length != null && !BulletLengthControl.IsEmpty)
                ammo.BulletLength = length.Value;

            if (!string.IsNullOrEmpty(_customTableFileName))
                ammo.CustomTableFileName = _customTableFileName;

            return ammo;
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            WeightControl.SetValue(value.Weight);
            BCControl.Value = new BallisticCoefficient(
                value.BallisticCoefficient.Value,
                value.BallisticCoefficient.Table);
            FormFactorCheckBox.IsChecked = value.BallisticCoefficient.ValueType == BallisticCoefficientValueType.FormFactor;
            MuzzleVelocityControl.SetValue(value.MuzzleVelocity);

            if (value.BulletDiameter.HasValue)
                BulletDiameterControl.SetValue(value.BulletDiameter.Value);
            else
                BulletDiameterControl.Value = null;

            if (value.BulletLength.HasValue)
                BulletLengthControl.SetValue(value.BulletLength.Value);
            else
                BulletLengthControl.Value = null;

            _customTableFileName = value.CustomTableFileName;
            UpdateCustomTableDisplay();
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    /// <summary>
    /// Raised after a custom drag table (<c>.drg</c>) is loaded, carrying the metadata from its header.
    /// The bullet fields are filled by this panel; the name and source belong to the library entry around
    /// it, so the host (see <see cref="AmmoLibraryRecordPanel"/>) fills those.
    /// </summary>
    public event EventHandler<AmmunitionLibraryEntry>? CustomTableLoaded;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        WeightControl.UnitType = typeof(WeightUnit);
        WeightControl.Minimum = 0;
        WeightControl.Increment = 0.1;

        MuzzleVelocityControl.UnitType = typeof(VelocityUnit);
        MuzzleVelocityControl.Minimum = 0;
        MuzzleVelocityControl.Increment = 1;

        BulletDiameterControl.UnitType = typeof(DistanceUnit);
        BulletDiameterControl.Minimum = 0;
        BulletDiameterControl.Increment = 0.01;

        BulletLengthControl.UnitType = typeof(DistanceUnit);
        BulletLengthControl.Minimum = 0;
        BulletLengthControl.Increment = 0.01;
    }

    private void WireEvents()
    {
        WeightControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        BCControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        MuzzleVelocityControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        BulletDiameterControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        BulletLengthControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);

        FormFactorCheckBox.IsCheckedChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            WeightControl.ChangeUnit(WeightUnit.Gram, 2, convert);
            MuzzleVelocityControl.ChangeUnit(VelocityUnit.MetersPerSecond, 1, convert);
            BulletDiameterControl.ChangeUnit(DistanceUnit.Millimeter, 2, convert);
            BulletLengthControl.ChangeUnit(DistanceUnit.Millimeter, 2, convert);
        }
        else
        {
            WeightControl.ChangeUnit(WeightUnit.Grain, 1, convert);
            MuzzleVelocityControl.ChangeUnit(VelocityUnit.FeetPerSecond, 1, convert);
            BulletDiameterControl.ChangeUnit(DistanceUnit.Inch, 3, convert);
            BulletLengthControl.ChangeUnit(DistanceUnit.Inch, 3, convert);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Why this ammunition cannot be used for a calculation, as sentences to show the user; empty when it
    /// can. Two of these states build a perfectly good <see cref="Ammunition"/> and only fail inside the
    /// engine, so "is every field filled in" does not catch them:
    /// <list type="bullet">
    /// <item>a <b>form factor</b> is turned into a coefficient through the bullet's sectional density, so
    /// it needs the diameter — and every <c>.drg</c> shot is a form-factor shot (finding F-1);</item>
    /// <item>a <b>GC</b> ("custom") coefficient has no built-in drag curve, so the <c>.drg</c> file must be
    /// found — a shared ammunition can name a table this machine does not have (finding F-1b).</item>
    /// </list>
    /// The remaining checks replace the old blanket "Ammunition data is required" with the field that is
    /// actually missing.
    /// </summary>
    public List<string> Problems()
    {
        var problems = new List<string>();

        var weight = WeightControl.GetValue<WeightUnit>();
        var bc = BCControl.Value;
        var velocity = MuzzleVelocityControl.GetValue<VelocityUnit>();

        if (weight == null)
            problems.Add("Bullet weight is not specified.");
        if (bc == null)
            problems.Add("Ballistic coefficient is not specified.");
        if (velocity == null)
            problems.Add("Muzzle velocity is not specified.");

        // A zero or negative value in any of the three leaves the solver with nothing to integrate — the
        // engine raises TrajectoryCannotBeCalculatedException. Cheaper to say which field it is.
        if (bc != null && bc.Value.Value <= 0 && FormFactorCheckBox.IsChecked != true)
            problems.Add("Ballistic coefficient must be greater than zero.");
        if (weight != null && weight.Value.Value <= 0 && FormFactorCheckBox.IsChecked != true)
            problems.Add("Bullet weight must be greater than zero.");
        if (velocity != null && velocity.Value.Value <= 0)
            problems.Add("Muzzle velocity must be greater than zero.");

        var diameter = BulletDiameterControl.IsEmpty ? null : BulletDiameterControl.GetValue<DistanceUnit>();

        if (FormFactorCheckBox.IsChecked == true)
        {
            if (diameter == null || diameter.Value.Value <= 0)
                problems.Add("Bullet diameter is required when the ballistic coefficient is a form factor.");
            if (bc != null && bc.Value.Value <= 0)
                problems.Add("A form factor must be greater than zero.");
            if (weight != null && weight.Value.Value <= 0)
                problems.Add("Bullet weight is required when the ballistic coefficient is a form factor.");
        }

        if (bc?.Table == DragTableId.GC &&
            CustomDragTableLoader.ResolvePath(_customTableFileName) == null)
        {
            problems.Add(string.IsNullOrEmpty(_customTableFileName)
                ? "A custom (GC) ballistic coefficient needs a custom drag table — use Browse... to pick " +
                  "the .drg file it was measured from."
                : $"The custom drag table \"{Path.GetFileName(_customTableFileName)}\" cannot be found — " +
                  $"neither as saved nor in {DataFolders.Drg}.");
        }

        return problems;
    }

    public void Clear()
    {
        WeightControl.Value = null;
        BCControl.Value = null;
        FormFactorCheckBox.IsChecked = false;
        MuzzleVelocityControl.Value = null;
        BulletDiameterControl.Value = null;
        BulletLengthControl.Value = null;
        _customTableFileName = null;
        UpdateCustomTableDisplay();
    }

    #endregion

    #region Custom Drag Table

    private void UpdateCustomTableDisplay()
    {
        CustomTableBox.Text = string.IsNullOrEmpty(_customTableFileName)
            ? ""
            : Path.GetFileName(_customTableFileName);
    }

    private async void OnBrowseCustomTable(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null)
            return;

        var path = await FileDialogService.OpenFileAsync(new FileDialogOptions
        {
            Title = "Open Custom Drag Table",
            DefaultExtension = "drg",
            InitialDirectory = DataFolders.Drg,
            Filters = { new Services.FileDialogFilter("Custom Drag Table", "drg") },
        });

        if (path == null)
            return;

        DrgDragTable table;
        try
        {
            table = DrgDragTable.Open(path);
        }
        catch (Exception ex)
        {
            CustomTableBox.Text = $"Error: {ex.Message}";
            return;
        }

        // A GC table has no built-in curve; the table itself carries it (used with a form-factor BC of 1).
        BCControl.Value = new BallisticCoefficient(1, DragTableId.GC);
        FormFactorCheckBox.IsChecked = true;

        // The .drg header carries weight, diameter and (since BallisticCalculator 1.1.11.2) bullet
        // length. Only positive values are copied: older files store the unused slots as 0, and
        // overwriting a good field with zero would silently break spin drift, which needs both the
        // diameter and the length.
        //
        // The format is always SI (kilograms and metres), so the values arrive as e.g. 0.010886kg /
        // 0.0078232m. Unlike a saved ammunition file, those units carry no user intent worth preserving —
        // they are an artefact of the file format — so they are converted to the panel's current units,
        // which is the one place a load deliberately overrides SetValue's precision-preserving behaviour.
        var tableAmmo = table.Ammunition?.Ammunition;
        if (tableAmmo != null)
        {
            var metric = _measurementSystem == MeasurementSystem.Metric;

            if (tableAmmo.Weight.Value > 0)
                WeightControl.SetValue(tableAmmo.Weight.To(metric ? WeightUnit.Gram : WeightUnit.Grain));
            if (tableAmmo.BulletDiameter.HasValue && tableAmmo.BulletDiameter.Value.Value > 0)
                BulletDiameterControl.SetValue(tableAmmo.BulletDiameter.Value.To(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch));
            if (tableAmmo.BulletLength.HasValue && tableAmmo.BulletLength.Value.Value > 0)
                BulletLengthControl.SetValue(tableAmmo.BulletLength.Value.To(metric ? DistanceUnit.Millimeter : DistanceUnit.Inch));
        }

        _customTableFileName = path;
        UpdateCustomTableDisplay();
        Changed?.Invoke(this, EventArgs.Empty);

        // The name and source live on the library entry, not here; let the host pick them up.
        if (table.Ammunition != null)
            CustomTableLoaded?.Invoke(this, table.Ammunition);
    }

    private void OnClearCustomTable(object? sender, RoutedEventArgs e)
    {
        _customTableFileName = null;
        UpdateCustomTableDisplay();

        // A cleared custom table with a GC coefficient can't be calculated; fall back to a standard table.
        if (BCControl.Value?.Table == DragTableId.GC)
        {
            BCControl.Value = new BallisticCoefficient(0.5, DragTableId.G1);
            FormFactorCheckBox.IsChecked = false;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
