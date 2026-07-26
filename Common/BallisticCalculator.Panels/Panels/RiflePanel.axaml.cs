using Avalonia.Controls;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Rifle inputs: the sight (height + H/V clicks) and the barrel rifling (twist step + direction).
/// Zero distance, impact offsets and the zeroing conditions live on the separate Zero panel.
/// Sight and barrel presets from <see cref="BallisticDictionary"/> prefill these fields; picking a
/// sight preset also raises <see cref="ZeroDistanceSuggested"/> so the host can set the zero distance.
/// </summary>
public partial class RiflePanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;
    private BallisticDictionary _dictionary = BallisticDictionary.Empty;
    private bool _applyingPreset;

    // The presets currently reflected by the fields; the combos revert to "(select)" only once a
    // field no longer matches its preset (i.e. the user edited it), not merely because a field
    // raised a change while the preset was being applied.
    private SightDictionaryEntry? _sightPreset;
    private BarrelDictionaryEntry? _barrelPreset;

    public RiflePanel()
    {
        InitializeComponent();
        InitializeControls();
        LoadDictionary();
        WireEvents();
        ApplyMeasurementSystem();
    }

    #region Properties

    public bool ConvertOnSystemChange { get; set; }

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

    public bool IsEmpty => SightHeightControl.IsEmpty;

    /// <summary>The sight (height + optional clicks), or null when the sight height is not set.</summary>
    public Sight? Sight
    {
        get
        {
            var sightHeight = SightHeightControl.GetValue<DistanceUnit>();
            if (sightHeight == null)
                return null;

            var vClick = VerticalClickControl.IsEmpty ? null : VerticalClickControl.GetValue<AngularUnit>();
            var hClick = HorizontalClickControl.IsEmpty ? null : HorizontalClickControl.GetValue<AngularUnit>();

            return new Sight()
            {
                SightHeight = sightHeight.Value,
                VerticalClick = vClick,
                HorizontalClick = hClick,
            };
        }
        set
        {
            if (value == null)
            {
                SightHeightControl.Value = null;
                VerticalClickControl.Value = null;
                HorizontalClickControl.Value = null;
                return;
            }

            SightHeightControl.SetValue(value.SightHeight);

            if (value.VerticalClick.HasValue)
                VerticalClickControl.SetValue(value.VerticalClick.Value);
            else
                VerticalClickControl.Value = null;

            if (value.HorizontalClick.HasValue)
                HorizontalClickControl.SetValue(value.HorizontalClick.Value);
            else
                HorizontalClickControl.Value = null;
        }
    }

    /// <summary>The barrel rifling (twist step + direction), or null when no direction is selected.</summary>
    public Rifling? Rifling
    {
        get
        {
            var dirIndex = RiflingDirectionCombo.SelectedIndex;
            if (dirIndex <= 0 || RiflingStepControl.IsEmpty)
                return null;

            var step = RiflingStepControl.GetValue<DistanceUnit>();
            if (step == null)
                return null;

            var direction = dirIndex == 1 ? TwistDirection.Left : TwistDirection.Right;
            return new Rifling(step.Value, direction);
        }
        set
        {
            if (value == null)
            {
                RiflingDirectionCombo.SelectedIndex = 0;
                RiflingStepControl.Value = null;
                return;
            }

            RiflingDirectionCombo.SelectedIndex = value.Direction == TwistDirection.Left ? 1 : 2;
            RiflingStepControl.SetValue(value.RiflingStep);
        }
    }

    /// <summary>
    /// Quick access to the vertical click size, used to convert elevation clicks to an angle.
    /// </summary>
    public Measurement<AngularUnit>? VerticalClick
        => VerticalClickControl.IsEmpty ? null : VerticalClickControl.GetValue<AngularUnit>();

    /// <summary>
    /// Quick access to the horizontal click size, used to convert windage clicks to an angle.
    /// </summary>
    public Measurement<AngularUnit>? HorizontalClick
        => HorizontalClickControl.IsEmpty ? null : HorizontalClickControl.GetValue<AngularUnit>();

    #endregion

    #region Events

    public event EventHandler? Changed;

    /// <summary>
    /// Raised when a sight preset with a default zero distance is selected, so the host can prefill
    /// the zero distance on the Zero panel.
    /// </summary>
    public event EventHandler<Measurement<DistanceUnit>>? ZeroDistanceSuggested;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        SightHeightControl.UnitType = typeof(DistanceUnit);
        SightHeightControl.Minimum = 0;
        SightHeightControl.Increment = 1;

        HorizontalClickControl.UnitType = typeof(AngularUnit);
        HorizontalClickControl.Minimum = 0;
        HorizontalClickControl.Increment = 0.01;
        HorizontalClickControl.ChangeUnit(AngularUnit.Mil, 2, false);

        VerticalClickControl.UnitType = typeof(AngularUnit);
        VerticalClickControl.Minimum = 0;
        VerticalClickControl.Increment = 0.01;
        VerticalClickControl.ChangeUnit(AngularUnit.Mil, 2, false);

        RiflingStepControl.UnitType = typeof(DistanceUnit);
        RiflingStepControl.Minimum = 0;
        RiflingStepControl.Increment = 1;

        // Populate rifling direction combo
        RiflingDirectionCombo.Items.Add("Not Set");
        RiflingDirectionCombo.Items.Add("Left");
        RiflingDirectionCombo.Items.Add("Right");
        RiflingDirectionCombo.SelectedIndex = 0;
    }

    private void LoadDictionary() => SetDictionary(BallisticDictionary.LoadDefault());

    /// <summary>Replaces the preset dictionary and repopulates the preset combos.</summary>
    internal void SetDictionary(BallisticDictionary dictionary)
    {
        _dictionary = dictionary;

        _applyingPreset = true;
        SightPresetCombo.Items.Clear();
        SightPresetCombo.Items.Add("(select)");
        foreach (var sight in _dictionary.Sights)
            SightPresetCombo.Items.Add(sight.Name);
        SightPresetCombo.SelectedIndex = 0;

        BarrelPresetCombo.Items.Clear();
        BarrelPresetCombo.Items.Add("(select)");
        foreach (var barrel in _dictionary.Barrels)
            BarrelPresetCombo.Items.Add(barrel.Name);
        BarrelPresetCombo.SelectedIndex = 0;
        _applyingPreset = false;
    }

    private void WireEvents()
    {
        SightHeightControl.Changed += OnFieldChanged;
        HorizontalClickControl.Changed += OnFieldChanged;
        VerticalClickControl.Changed += OnFieldChanged;
        RiflingStepControl.Changed += OnFieldChanged;

        RiflingDirectionCombo.SelectionChanged += OnRiflingDirectionChanged;
        SightPresetCombo.SelectionChanged += OnSightPresetChanged;
        BarrelPresetCombo.SelectionChanged += OnBarrelPresetChanged;
    }

    /// <summary>
    /// When a field changes, drop the corresponding preset back to "(select)" only if the fields no
    /// longer match the applied preset. This keeps the preset selected after it is applied (the apply
    /// itself sets values that still match) while reverting on a genuine user edit.
    /// </summary>
    private void OnFieldChanged(object? sender, EventArgs e)
    {
        if (!_applyingPreset)
        {
            if (sender == SightHeightControl || sender == HorizontalClickControl || sender == VerticalClickControl)
            {
                if (_sightPreset != null && !SightMatches(_sightPreset))
                    ClearPreset(SightPresetCombo, ref _sightPreset);
            }
            else if (sender == RiflingStepControl)
            {
                if (_barrelPreset != null && !BarrelMatches(_barrelPreset))
                    ClearPreset(BarrelPresetCombo, ref _barrelPreset);
            }
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ClearPreset<T>(ComboBox combo, ref T? preset) where T : class
    {
        preset = null;
        if (combo.SelectedIndex != 0)
        {
            _applyingPreset = true;
            combo.SelectedIndex = 0;
            _applyingPreset = false;
        }
    }

    private bool SightMatches(SightDictionaryEntry preset)
    {
        var height = SightHeightControl.IsEmpty ? (Measurement<DistanceUnit>?)null : SightHeightControl.GetValue<DistanceUnit>();
        return CloseDistance(height, preset.SightHeight) &&
               CloseAngular(VerticalClick, preset.VerticalClick) &&
               CloseAngular(HorizontalClick, preset.HorizontalClick);
    }

    private bool BarrelMatches(BarrelDictionaryEntry preset)
    {
        var rifling = Rifling;
        return rifling != null &&
               rifling.Direction == preset.Direction &&
               CloseDistance(rifling.RiflingStep, preset.Step);
    }

    private static bool CloseDistance(Measurement<DistanceUnit>? a, Measurement<DistanceUnit>? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return Math.Abs(a.Value.In(DistanceUnit.Millimeter) - b.Value.In(DistanceUnit.Millimeter)) < 0.05;
    }

    private static bool CloseAngular(Measurement<AngularUnit>? a, Measurement<AngularUnit>? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return Math.Abs(a.Value.In(AngularUnit.Radian) - b.Value.In(AngularUnit.Radian)) < 1e-6;
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            SightHeightControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
            RiflingStepControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
        }
        else
        {
            SightHeightControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
            RiflingStepControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
        }
        // Click units (Angular) are NOT affected by measurement system switch
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        _sightPreset = null;
        _barrelPreset = null;

        _applyingPreset = true;
        SightPresetCombo.SelectedIndex = 0;
        BarrelPresetCombo.SelectedIndex = 0;
        _applyingPreset = false;

        SightHeightControl.Value = null;
        HorizontalClickControl.Value = null;
        VerticalClickControl.Value = null;
        RiflingDirectionCombo.SelectedIndex = 0;
        RiflingStepControl.Value = null;
    }

    #endregion

    #region Event Handlers

    private void OnRiflingDirectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RiflingStepControl.IsEnabled = RiflingDirectionCombo.SelectedIndex > 0;
        if (!_applyingPreset)
        {
            if (_barrelPreset != null && !BarrelMatches(_barrelPreset))
                ClearPreset(BarrelPresetCombo, ref _barrelPreset);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnSightPresetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_applyingPreset)
            return;

        var index = SightPresetCombo.SelectedIndex - 1; // 0 == "(select)"
        if (index < 0 || index >= _dictionary.Sights.Count)
        {
            _sightPreset = null;
            return;
        }

        var preset = _dictionary.Sights[index];

        _applyingPreset = true;
        SightHeightControl.SetValue(preset.SightHeight);
        if (preset.VerticalClick.HasValue)
            VerticalClickControl.SetValue(preset.VerticalClick.Value);
        else
            VerticalClickControl.Value = null;
        if (preset.HorizontalClick.HasValue)
            HorizontalClickControl.SetValue(preset.HorizontalClick.Value);
        else
            HorizontalClickControl.Value = null;
        _applyingPreset = false;

        _sightPreset = preset;

        if (preset.DefaultZero.HasValue)
            ZeroDistanceSuggested?.Invoke(this, preset.DefaultZero.Value);

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnBarrelPresetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_applyingPreset)
            return;

        var index = BarrelPresetCombo.SelectedIndex - 1; // 0 == "(select)"
        if (index < 0 || index >= _dictionary.Barrels.Count)
        {
            _barrelPreset = null;
            return;
        }

        var preset = _dictionary.Barrels[index];

        _applyingPreset = true;
        RiflingDirectionCombo.SelectedIndex = preset.Direction == TwistDirection.Left ? 1 : 2;
        RiflingStepControl.IsEnabled = true;
        RiflingStepControl.SetValue(preset.Step);
        _applyingPreset = false;

        _barrelPreset = preset;

        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
