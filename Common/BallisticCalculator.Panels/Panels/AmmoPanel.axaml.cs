using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
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
        var tableAmmo = table.Ammunition?.Ammunition;
        if (tableAmmo != null)
        {
            if (tableAmmo.Weight.Value > 0)
                WeightControl.SetValue(tableAmmo.Weight);
            if (tableAmmo.BulletDiameter.HasValue && tableAmmo.BulletDiameter.Value.Value > 0)
                BulletDiameterControl.SetValue(tableAmmo.BulletDiameter.Value);
            if (tableAmmo.BulletLength.HasValue && tableAmmo.BulletLength.Value.Value > 0)
                BulletLengthControl.SetValue(tableAmmo.BulletLength.Value);
        }

        _customTableFileName = path;
        UpdateCustomTableDisplay();
        Changed?.Invoke(this, EventArgs.Empty);
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
