using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Zero (zeroing) inputs: the zero distance, impact offset at zero, zeroing shot angle, and the
/// ammunition / atmosphere / wind that were in effect while the rifle was zeroed. Produces a
/// <see cref="ZeroingData"/>.
/// </summary>
public partial class ZeroPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public ZeroPanel()
    {
        InitializeComponent();
        InitializeControls();
        WireEvents();
        ApplyMeasurementSystem();
    }

    #region Properties

    public bool ConvertOnSystemChange { get; set; }

    public IFileDialogService? FileDialogService
    {
        get => ZeroAmmoSubPanel.FileDialogService;
        set => ZeroAmmoSubPanel.FileDialogService = value;
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            ApplyMeasurementSystem();
            ZeroAmmoSubPanel.MeasurementSystem = value;
            ZeroAtmosphereSubPanel.MeasurementSystem = value;
            ZeroWindSubPanel.MeasurementSystem = value;
        }
    }

    public bool IsEmpty =>
        ZeroDistanceControl.IsEmpty && ZeroShotAngleControl.IsEmpty &&
        VerticalOffsetControl.IsEmpty && HorizontalOffsetControl.IsEmpty;

    /// <summary>The zero distance (needed to build the library <see cref="Rifle"/>), or null.</summary>
    public Measurement<DistanceUnit>? ZeroDistance
        => ZeroDistanceControl.IsEmpty ? null : ZeroDistanceControl.GetValue<DistanceUnit>();

    /// <summary>The full set of zeroing inputs, assembled/loaded as one <see cref="ZeroingData"/>.</summary>
    public ZeroingData? Zeroing
    {
        get
        {
            var data = new ZeroingData
            {
                Distance = ZeroDistance,
                Ammunition = ZeroAmmoSubPanel.Ammunition,
                Atmosphere = ZeroAtmosphereSubPanel.Atmosphere,
                Wind = ZeroWindSubPanel.Wind,
                ShotAngle = ZeroShotAngleControl.IsEmpty ? null : ZeroShotAngleControl.GetValue<AngularUnit>(),
            };

            if (VerticalOffsetCheckBox.IsChecked == true)
            {
                if (!VerticalOffsetControl.IsEmpty)
                    data.VerticalOffset = VerticalOffsetControl.GetValue<DistanceUnit>();
                if (!HorizontalOffsetControl.IsEmpty)
                    data.HorizontalOffset = HorizontalOffsetControl.GetValue<DistanceUnit>();
            }

            return data;
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            if (value.Distance.HasValue)
                ZeroDistanceControl.SetValue(value.Distance.Value);
            else
                ZeroDistanceControl.Value = null;

            if (value.ShotAngle.HasValue)
                ZeroShotAngleControl.SetValue(value.ShotAngle.Value);
            else
                ZeroShotAngleControl.Value = null;

            VerticalOffsetCheckBox.IsChecked =
                value.VerticalOffset.HasValue || value.HorizontalOffset.HasValue;

            if (value.VerticalOffset.HasValue)
                VerticalOffsetControl.SetValue(value.VerticalOffset.Value);
            else
                VerticalOffsetControl.Value = null;

            if (value.HorizontalOffset.HasValue)
                HorizontalOffsetControl.SetValue(value.HorizontalOffset.Value);
            else
                HorizontalOffsetControl.Value = null;

            ZeroAmmoSubPanel.Ammunition = value.Ammunition;
            ZeroAtmosphereSubPanel.Atmosphere = value.Atmosphere;
            ZeroWindSubPanel.Wind = value.Wind;
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        ZeroDistanceControl.UnitType = typeof(DistanceUnit);
        ZeroDistanceControl.Minimum = 0;
        ZeroDistanceControl.Increment = 10;

        ZeroShotAngleControl.UnitType = typeof(AngularUnit);
        ZeroShotAngleControl.Increment = 1;
        ZeroShotAngleControl.ChangeUnit(AngularUnit.Degree, 1, false);

        VerticalOffsetControl.UnitType = typeof(DistanceUnit);
        VerticalOffsetControl.Increment = 1;

        HorizontalOffsetControl.UnitType = typeof(DistanceUnit);
        HorizontalOffsetControl.Increment = 1;
    }

    private void WireEvents()
    {
        ZeroDistanceControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        ZeroShotAngleControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        VerticalOffsetControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        HorizontalOffsetControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        VerticalOffsetCheckBox.IsCheckedChanged += OnVerticalOffsetCheckChanged;

        ZeroAmmoSubPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        ZeroAtmosphereSubPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        ZeroWindSubPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            ZeroDistanceControl.ChangeUnit(DistanceUnit.Meter, 0, convert);
            VerticalOffsetControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
            HorizontalOffsetControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
        }
        else
        {
            ZeroDistanceControl.ChangeUnit(DistanceUnit.Yard, 0, convert);
            VerticalOffsetControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
            HorizontalOffsetControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
        }
        // Shot angle (Angular) is NOT affected by measurement system switch
    }

    #endregion

    #region Public Methods

    /// <summary>Sets just the zero distance (used when a sight preset suggests a default zero).</summary>
    public void SetZeroDistance(Measurement<DistanceUnit> distance)
        => ZeroDistanceControl.SetValue(distance);

    public void Clear()
    {
        ZeroDistanceControl.Value = null;
        ZeroShotAngleControl.Value = null;
        VerticalOffsetCheckBox.IsChecked = false;
        VerticalOffsetControl.Value = null;
        HorizontalOffsetControl.Value = null;
        ZeroAmmoSubPanel.Clear();
        ZeroAtmosphereSubPanel.Clear();
        ZeroWindSubPanel.Clear();
    }

    #endregion

    #region Event Handlers

    private void OnVerticalOffsetCheckChanged(object? sender, RoutedEventArgs e)
    {
        var enabled = VerticalOffsetCheckBox.IsChecked == true;
        VerticalOffsetControl.IsEnabled = enabled;
        HorizontalOffsetControl.IsEnabled = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
