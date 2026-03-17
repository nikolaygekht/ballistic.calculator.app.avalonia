using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.Globalization;

namespace BallisticCalculator.Panels.Panels;

public partial class AtmospherePanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public AtmospherePanel()
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

    public Atmosphere? Atmosphere
    {
        get
        {
            var altitude = AltitudeControl.GetValue<DistanceUnit>();
            var pressure = PressureControl.GetValue<PressureUnit>();
            var temperature = TemperatureControl.GetValue<TemperatureUnit>();

            if (altitude == null || pressure == null || temperature == null)
                return null;

            if (!TryParseHumidity(out var humidity))
                return null;

            return new Atmosphere(
                altitude.Value,
                pressure.Value,
                temperature.Value,
                humidity);
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            AltitudeControl.SetValue(value.Altitude);
            PressureControl.SetValue(value.Pressure);
            TemperatureControl.SetValue(value.Temperature);
            HumidityTextBox.Text = Math.Round(value.Humidity * 100).ToString(CultureInfo.InvariantCulture);
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        AltitudeControl.UnitType = typeof(DistanceUnit);
        AltitudeControl.Minimum = 0;
        AltitudeControl.Increment = 10;

        PressureControl.UnitType = typeof(PressureUnit);
        PressureControl.Minimum = 0;
        PressureControl.Increment = 1;

        TemperatureControl.UnitType = typeof(TemperatureUnit);
        TemperatureControl.Increment = 1;
    }

    private void WireEvents()
    {
        AltitudeControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        PressureControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        TemperatureControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        HumidityTextBox.TextChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        ResetButton.Click += OnResetClick;
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            AltitudeControl.ChangeUnit(DistanceUnit.Meter, 0, convert);
            PressureControl.ChangeUnit(PressureUnit.MillimetersOfMercury, 1, convert);
            TemperatureControl.ChangeUnit(TemperatureUnit.Celsius, 1, convert);
        }
        else
        {
            AltitudeControl.ChangeUnit(DistanceUnit.Foot, 0, convert);
            PressureControl.ChangeUnit(PressureUnit.InchesOfMercury, 2, convert);
            TemperatureControl.ChangeUnit(TemperatureUnit.Fahrenheit, 1, convert);
        }
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        AltitudeControl.Value = null;
        PressureControl.Value = null;
        TemperatureControl.Value = null;
        HumidityTextBox.Text = "";
    }

    public void Reset()
    {
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            AltitudeControl.SetValue(new Measurement<DistanceUnit>(0, DistanceUnit.Meter));
            PressureControl.SetValue(new Measurement<PressureUnit>(760, PressureUnit.MillimetersOfMercury));
            TemperatureControl.SetValue(new Measurement<TemperatureUnit>(15, TemperatureUnit.Celsius));
        }
        else
        {
            AltitudeControl.SetValue(new Measurement<DistanceUnit>(0, DistanceUnit.Foot));
            PressureControl.SetValue(new Measurement<PressureUnit>(29.92, PressureUnit.InchesOfMercury));
            TemperatureControl.SetValue(new Measurement<TemperatureUnit>(59, TemperatureUnit.Fahrenheit));
        }
        HumidityTextBox.Text = "78";
    }

    #endregion

    #region Private Methods

    private void OnResetClick(object? sender, RoutedEventArgs e)
    {
        Reset();
    }

    private bool TryParseHumidity(out double humidity)
    {
        humidity = 0;
        var text = HumidityTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return false;

        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent))
            return false;

        humidity = percent / 100.0;
        return true;
    }

    #endregion
}
