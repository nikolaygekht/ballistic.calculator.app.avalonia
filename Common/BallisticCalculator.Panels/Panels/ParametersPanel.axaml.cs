using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;

namespace BallisticCalculator.Panels.Panels;

public partial class ParametersPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public ParametersPanel()
    {
        InitializeComponent();
        InitializeControls();
        WireEvents();
        ApplyMeasurementSystem();
    }

    #region Properties

    public bool ConvertOnSystemChange { get; set; } = true;

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
    /// Reference to the RiflePanel, used to convert V/H clicks to an angle via the sight's click sizes.
    /// </summary>
    public RiflePanel? RiflePanel { get; set; }

    public bool IsEmpty => MaxRangeControl.IsEmpty && StepControl.IsEmpty;

    public ShotParameters? Parameters
    {
        get
        {
            var maxRange = MaxRangeControl.GetValue<DistanceUnit>();
            var step = StepControl.GetValue<DistanceUnit>();

            if (maxRange == null || step == null)
                return null;

            var parms = new ShotParameters()
            {
                MaximumDistance = maxRange.Value,
                Step = step.Value,
            };

            if (!AngleControl.IsEmpty)
            {
                var angle = AngleControl.GetValue<AngularUnit>();
                if (angle != null)
                    parms.ShotAngle = angle.Value;
            }

            // Dialed clicks -> angular adjustment (needs the sight's click size). These are
            // passed separately from the shot angle and accumulate on top of the zero.
            parms.ShotDropAdjustment = ClicksToAngle((double)(VClicksControl.Value ?? 0), RiflePanel?.VerticalClick);
            parms.ShotWindageAdjustment = ClicksToAngle((double)(HClicksControl.Value ?? 0), RiflePanel?.HorizontalClick);

            // Coriolis / Eötvös inputs, only when enabled.
            if (CoriolisCheckBox.IsChecked == true)
            {
                if (!AzimuthControl.IsEmpty)
                {
                    var azimuth = AzimuthControl.GetValue<AngularUnit>();
                    if (azimuth != null)
                        parms.BarrelAzimuth = azimuth.Value;
                }

                // Latitude magnitude (0-90) + N/S hemisphere -> signed degrees (North +, South -).
                double latMagnitude = 0;
                if (!LatitudeControl.IsEmpty)
                {
                    var latitude = LatitudeControl.GetValue<AngularUnit>();
                    if (latitude != null)
                        latMagnitude = latitude.Value.In(AngularUnit.Degree);
                }
                var south = LatitudeHemisphere.SelectedIndex == 1;
                parms.Latitude = new Measurement<AngularUnit>(south ? -latMagnitude : latMagnitude, AngularUnit.Degree);
            }

            return parms;
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            MaxRangeControl.SetValue(value.MaximumDistance);
            StepControl.SetValue(value.Step);

            if (value.ShotAngle.HasValue)
                AngleControl.SetValue(value.ShotAngle.Value);
            else
                AngleControl.Value = null;

            VClicksControl.Value = AngleToClicks(value.ShotDropAdjustment, RiflePanel?.VerticalClick);
            HClicksControl.Value = AngleToClicks(value.ShotWindageAdjustment, RiflePanel?.HorizontalClick);

            if (value.BarrelAzimuth.HasValue || value.Latitude.HasValue)
            {
                CoriolisCheckBox.IsChecked = true;

                if (value.BarrelAzimuth.HasValue)
                {
                    AzimuthControl.SetValue(value.BarrelAzimuth.Value);
                    AzimuthIndicator.Direction = value.BarrelAzimuth.Value.In(AngularUnit.Degree);
                }
                else
                {
                    AzimuthControl.Value = null;
                    AzimuthIndicator.Direction = 0;
                }

                if (value.Latitude.HasValue)
                {
                    var latDegrees = value.Latitude.Value.In(AngularUnit.Degree);
                    LatitudeControl.SetValue(new Measurement<AngularUnit>(Math.Abs(latDegrees), AngularUnit.Degree));
                    LatitudeHemisphere.SelectedIndex = latDegrees < 0 ? 1 : 0;
                }
                else
                {
                    LatitudeControl.Value = null;
                    LatitudeHemisphere.SelectedIndex = 0;
                }
            }
            else
            {
                CoriolisCheckBox.IsChecked = false;
                AzimuthControl.Value = null;
                AzimuthIndicator.Direction = 0;
                LatitudeControl.Value = null;
                LatitudeHemisphere.SelectedIndex = 0;
            }
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        MaxRangeControl.UnitType = typeof(DistanceUnit);
        MaxRangeControl.Minimum = 0;
        MaxRangeControl.Increment = 100;

        StepControl.UnitType = typeof(DistanceUnit);
        StepControl.Minimum = 0;
        StepControl.Increment = 10;

        AngleControl.UnitType = typeof(AngularUnit);
        AngleControl.Increment = 1;
        AngleControl.ChangeUnit(AngularUnit.Degree, 1, false);

        AzimuthControl.UnitType = typeof(AngularUnit);
        AzimuthControl.Minimum = 0;
        AzimuthControl.Maximum = 360;
        AzimuthControl.Increment = 1;
        AzimuthControl.ChangeUnit(AngularUnit.Degree, 1, false);

        LatitudeControl.UnitType = typeof(AngularUnit);
        LatitudeControl.Minimum = 0;
        LatitudeControl.Maximum = 90;
        LatitudeControl.Increment = 1;
        LatitudeControl.ChangeUnit(AngularUnit.Degree, 1, false);
        // LatitudeHemisphere (N/S ComboBox) is configured in XAML.
    }

    private void WireEvents()
    {
        MaxRangeControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        StepControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        AngleControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        VClicksControl.ValueChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        HClicksControl.ValueChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);

        AzimuthControl.Changed += (s, e) =>
        {
            var az = AzimuthControl.IsEmpty ? null : AzimuthControl.GetValue<AngularUnit>();
            if (az != null)
                AzimuthIndicator.Direction = az.Value.In(AngularUnit.Degree);
            Changed?.Invoke(this, EventArgs.Empty);
        };
        AzimuthIndicator.Changed += (s, e) =>
        {
            AzimuthControl.SetValue(new Measurement<AngularUnit>(AzimuthIndicator.Direction, AngularUnit.Degree));
            Changed?.Invoke(this, EventArgs.Empty);
        };

        LatitudeControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        LatitudeHemisphere.SelectionChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        CoriolisCheckBox.IsCheckedChanged += OnCoriolisChanged;
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            MaxRangeControl.ChangeUnit(DistanceUnit.Meter, 0, convert);
            StepControl.ChangeUnit(DistanceUnit.Meter, 0, convert);
        }
        else
        {
            MaxRangeControl.ChangeUnit(DistanceUnit.Yard, 0, convert);
            StepControl.ChangeUnit(DistanceUnit.Yard, 0, convert);
        }
        // Angle units (shot angle, azimuth, latitude) are NOT affected by measurement system switch
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        MaxRangeControl.Value = null;
        StepControl.Value = null;
        AngleControl.Value = null;
        VClicksControl.Value = 0;
        HClicksControl.Value = 0;
        CoriolisCheckBox.IsChecked = false;
        AzimuthControl.Value = null;
        AzimuthIndicator.Direction = 0;
        LatitudeControl.Value = null;
        LatitudeHemisphere.SelectedIndex = 0;
    }

    #endregion

    #region Private Methods

    /// <summary>Convert a click count to an angular adjustment using the sight's click size.</summary>
    private static Measurement<AngularUnit>? ClicksToAngle(double clicks, Measurement<AngularUnit>? clickSize)
    {
        if (clicks == 0 || clickSize == null)
            return null;
        return new Measurement<AngularUnit>(clickSize.Value.Value * clicks, clickSize.Value.Unit);
    }

    /// <summary>Convert an angular adjustment back to a (rounded) click count using the sight's click size.</summary>
    private static decimal AngleToClicks(Measurement<AngularUnit>? adjustment, Measurement<AngularUnit>? clickSize)
    {
        if (adjustment == null || clickSize == null || clickSize.Value.Value == 0)
            return 0;
        var clicks = Math.Round(adjustment.Value.In(clickSize.Value.Unit) / clickSize.Value.Value);
        return (decimal)clicks;
    }

    #endregion

    #region Event Handlers

    private void OnCoriolisChanged(object? sender, RoutedEventArgs e)
    {
        var enabled = CoriolisCheckBox.IsChecked == true;
        AzimuthControl.IsEnabled = enabled;
        AzimuthIndicator.IsEnabled = enabled;
        LatitudeControl.IsEnabled = enabled;
        LatitudeHemisphere.IsEnabled = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
