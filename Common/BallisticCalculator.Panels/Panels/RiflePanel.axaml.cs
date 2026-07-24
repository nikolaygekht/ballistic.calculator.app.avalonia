using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;

namespace BallisticCalculator.Panels.Panels;

public partial class RiflePanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public RiflePanel()
    {
        InitializeComponent();
        InitializeControls();
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

    public bool IsEmpty => SightHeightControl.IsEmpty && ZeroDistanceControl.IsEmpty;

    public Rifle? Rifle
    {
        get
        {
            var sightHeight = SightHeightControl.GetValue<DistanceUnit>();
            var zeroDistance = ZeroDistanceControl.GetValue<DistanceUnit>();

            if (sightHeight == null || zeroDistance == null)
                return null;

            // Clicks are optional
            var vClick = VerticalClickControl.IsEmpty ? null : VerticalClickControl.GetValue<AngularUnit>();
            var hClick = HorizontalClickControl.IsEmpty ? null : HorizontalClickControl.GetValue<AngularUnit>();

            var sight = new Sight()
            {
                SightHeight = sightHeight.Value,
                VerticalClick = vClick,
                HorizontalClick = hClick,
            };

            // Zero parameters
            var zero = new ZeroingParameters(zeroDistance.Value, null, null);

            // Offsets only when checkbox is checked
            if (VerticalOffsetCheckBox.IsChecked == true && !VerticalOffsetControl.IsEmpty)
            {
                var offset = VerticalOffsetControl.GetValue<DistanceUnit>();
                if (offset != null)
                    zero.VerticalOffset = offset.Value;
            }

            if (VerticalOffsetCheckBox.IsChecked == true && !HorizontalOffsetControl.IsEmpty)
            {
                var hoffset = HorizontalOffsetControl.GetValue<DistanceUnit>();
                if (hoffset != null)
                    zero.HorizontalOffset = hoffset.Value;
            }

            // Rifling only when direction is set
            Rifling? rifling = null;
            var dirIndex = RiflingDirectionCombo.SelectedIndex;
            if (dirIndex > 0 && !RiflingStepControl.IsEmpty)
            {
                var step = RiflingStepControl.GetValue<DistanceUnit>();
                if (step != null)
                {
                    var direction = dirIndex == 1 ? TwistDirection.Left : TwistDirection.Right;
                    rifling = new Rifling(step.Value, direction);
                }
            }

            return new Rifle(sight, zero, rifling);
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            SightHeightControl.SetValue(value.Sight.SightHeight);
            ZeroDistanceControl.SetValue(value.Zero.Distance);

            if (value.Sight.VerticalClick.HasValue)
                VerticalClickControl.SetValue(value.Sight.VerticalClick.Value);
            else
                VerticalClickControl.Value = null;

            if (value.Sight.HorizontalClick.HasValue)
                HorizontalClickControl.SetValue(value.Sight.HorizontalClick.Value);
            else
                HorizontalClickControl.Value = null;

            if (value.Rifling != null)
            {
                RiflingDirectionCombo.SelectedIndex = value.Rifling.Direction == TwistDirection.Left ? 1 : 2;
                RiflingStepControl.SetValue(value.Rifling.RiflingStep);
            }
            else
            {
                RiflingDirectionCombo.SelectedIndex = 0;
                RiflingStepControl.Value = null;
            }

            VerticalOffsetCheckBox.IsChecked =
                value.Zero.VerticalOffset.HasValue || value.Zero.HorizontalOffset.HasValue;

            if (value.Zero.VerticalOffset.HasValue)
                VerticalOffsetControl.SetValue(value.Zero.VerticalOffset.Value);
            else
                VerticalOffsetControl.Value = null;

            if (value.Zero.HorizontalOffset.HasValue)
                HorizontalOffsetControl.SetValue(value.Zero.HorizontalOffset.Value);
            else
                HorizontalOffsetControl.Value = null;
        }
    }

    /// <summary>
    /// Zeroing shot (incline) angle. Not part of the library <see cref="Rifle"/>; assembled into
    /// <c>ZeroingData</c> by the host panel.
    /// </summary>
    public Measurement<AngularUnit>? ZeroShotAngle
    {
        get => ZeroShotAngleControl.IsEmpty ? null : ZeroShotAngleControl.GetValue<AngularUnit>();
        set
        {
            if (value.HasValue)
                ZeroShotAngleControl.SetValue(value.Value);
            else
                ZeroShotAngleControl.Value = null;
        }
    }

    /// <summary>
    /// Quick access to the vertical click size, used to convert elevation clicks to an angle.
    /// </summary>
    public Measurement<AngularUnit>? VerticalClick
    {
        get
        {
            if (VerticalClickControl.IsEmpty) return null;
            return VerticalClickControl.GetValue<AngularUnit>();
        }
    }

    /// <summary>
    /// Quick access to the horizontal click size, used to convert windage clicks to an angle.
    /// </summary>
    public Measurement<AngularUnit>? HorizontalClick
    {
        get
        {
            if (HorizontalClickControl.IsEmpty) return null;
            return HorizontalClickControl.GetValue<AngularUnit>();
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        SightHeightControl.UnitType = typeof(DistanceUnit);
        SightHeightControl.Minimum = 0;
        SightHeightControl.Increment = 1;

        ZeroDistanceControl.UnitType = typeof(DistanceUnit);
        ZeroDistanceControl.Minimum = 0;
        ZeroDistanceControl.Increment = 10;

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

        VerticalOffsetControl.UnitType = typeof(DistanceUnit);
        VerticalOffsetControl.Increment = 1;

        HorizontalOffsetControl.UnitType = typeof(DistanceUnit);
        HorizontalOffsetControl.Increment = 1;

        ZeroShotAngleControl.UnitType = typeof(AngularUnit);
        ZeroShotAngleControl.Increment = 1;
        ZeroShotAngleControl.ChangeUnit(AngularUnit.Degree, 1, false);

        // Populate rifling direction combo
        RiflingDirectionCombo.Items.Add("Not Set");
        RiflingDirectionCombo.Items.Add("Left");
        RiflingDirectionCombo.Items.Add("Right");
        RiflingDirectionCombo.SelectedIndex = 0;
    }

    private void WireEvents()
    {
        SightHeightControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        ZeroDistanceControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        HorizontalClickControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        VerticalClickControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        RiflingStepControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        VerticalOffsetControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        HorizontalOffsetControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        ZeroShotAngleControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);

        RiflingDirectionCombo.SelectionChanged += OnRiflingDirectionChanged;
        VerticalOffsetCheckBox.IsCheckedChanged += OnVerticalOffsetCheckChanged;
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            SightHeightControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
            ZeroDistanceControl.ChangeUnit(DistanceUnit.Meter, 0, convert);
            RiflingStepControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
            VerticalOffsetControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
            HorizontalOffsetControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
        }
        else
        {
            SightHeightControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
            ZeroDistanceControl.ChangeUnit(DistanceUnit.Yard, 0, convert);
            RiflingStepControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
            VerticalOffsetControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
            HorizontalOffsetControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
        }
        // Click units (Angular) are NOT affected by measurement system switch
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        SightHeightControl.Value = null;
        ZeroDistanceControl.Value = null;
        HorizontalClickControl.Value = null;
        VerticalClickControl.Value = null;
        RiflingDirectionCombo.SelectedIndex = 0;
        RiflingStepControl.Value = null;
        VerticalOffsetCheckBox.IsChecked = false;
        VerticalOffsetControl.Value = null;
        HorizontalOffsetControl.Value = null;
        ZeroShotAngleControl.Value = null;
    }

    #endregion

    #region Event Handlers

    private void OnRiflingDirectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RiflingStepControl.IsEnabled = RiflingDirectionCombo.SelectedIndex > 0;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnVerticalOffsetCheckChanged(object? sender, RoutedEventArgs e)
    {
        var enabled = VerticalOffsetCheckBox.IsChecked == true;
        VerticalOffsetControl.IsEnabled = enabled;
        HorizontalOffsetControl.IsEnabled = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
