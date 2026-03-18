using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.Globalization;

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
    /// Reference to the RiflePanel for angle-as-clicks calculation.
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
        AngleControl.Increment = 0.1;
        AngleControl.ChangeUnit(AngularUnit.Mil, 2, false);
    }

    private void WireEvents()
    {
        MaxRangeControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        StepControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        AngleControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        ClicksSetButton.Click += OnClicksSetClick;
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
        // Angle units are NOT affected by measurement system switch
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        MaxRangeControl.Value = null;
        StepControl.Value = null;
        AngleControl.Value = null;
        ClicksTextBox.Text = "";
    }

    /// <summary>
    /// Calculate shot angle from clicks using RiflePanel's vertical click value.
    /// </summary>
    public void SetAngleFromClicks()
    {
        var vClick = RiflePanel?.VerticalClick;
        if (vClick == null)
            return;

        var text = ClicksTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var clicks))
            return;

        var angle = new Measurement<AngularUnit>(
            vClick.Value.Value * clicks,
            vClick.Value.Unit);
        AngleControl.SetValue(angle);
    }

    #endregion

    #region Event Handlers

    private void OnClicksSetClick(object? sender, RoutedEventArgs e)
    {
        SetAngleFromClicks();
    }

    #endregion
}
