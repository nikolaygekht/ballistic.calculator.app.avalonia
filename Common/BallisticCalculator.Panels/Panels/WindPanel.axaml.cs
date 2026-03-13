using System.Runtime.CompilerServices;
using Avalonia.Controls;
using BallisticCalculator;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;

[assembly: InternalsVisibleTo("BallisticCalculator.Panels.Tests")]

namespace BallisticCalculator.Panels.Panels;

public partial class WindPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public WindPanel()
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

    public bool IsEmpty => DirectionControl.IsEmpty && VelocityControl.IsEmpty;

    public Wind? Wind
    {
        get
        {
            var direction = DirectionControl.GetValue<AngularUnit>();
            var velocity = VelocityControl.GetValue<VelocityUnit>();

            if (direction == null || velocity == null)
                return null;

            var wind = new Wind()
            {
                Direction = direction.Value,
                Velocity = velocity.Value,
            };

            if (MaxDistanceCheckBox.IsChecked == true)
            {
                var maxRange = MaxDistanceControl.GetValue<DistanceUnit>();
                if (maxRange != null && !MaxDistanceControl.IsEmpty)
                    wind.MaximumRange = maxRange.Value;
            }

            return wind;
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            DirectionControl.SetValue(value.Direction);
            VelocityControl.SetValue(value.Velocity);

            if (value.MaximumRange.HasValue)
            {
                MaxDistanceCheckBox.IsChecked = true;
                MaxDistanceControl.SetValue(value.MaximumRange.Value);
            }
            else
            {
                MaxDistanceCheckBox.IsChecked = false;
                MaxDistanceControl.Value = null;
            }

            // Sync wind indicator
            WindIndicator.Direction = value.Direction.In(AngularUnit.Degree);
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        DirectionControl.UnitType = typeof(AngularUnit);
        DirectionControl.ChangeUnit(AngularUnit.Degree, 0);
        DirectionControl.Minimum = 0;
        DirectionControl.Maximum = 360;
        DirectionControl.Increment = 1;

        VelocityControl.UnitType = typeof(VelocityUnit);
        VelocityControl.Minimum = 0;
        VelocityControl.Increment = 0.1;

        MaxDistanceControl.UnitType = typeof(DistanceUnit);
        MaxDistanceControl.Minimum = 0;
        MaxDistanceControl.Increment = 10;
    }

    private void WireEvents()
    {
        DirectionControl.Changed += (s, e) =>
        {
            // Sync direction to wind indicator
            var dir = DirectionControl.GetValue<AngularUnit>();
            if (dir != null)
                WindIndicator.Direction = dir.Value.In(AngularUnit.Degree);
            Changed?.Invoke(this, EventArgs.Empty);
        };

        VelocityControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        MaxDistanceControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);

        MaxDistanceCheckBox.IsCheckedChanged += (s, e) =>
        {
            MaxDistanceControl.IsEnabled = MaxDistanceCheckBox.IsChecked == true;
            Changed?.Invoke(this, EventArgs.Empty);
        };

        WindIndicator.Changed += (s, e) =>
        {
            // Sync wind indicator back to direction control
            DirectionControl.SetValue(new Measurement<AngularUnit>(WindIndicator.Direction, AngularUnit.Degree));
            Changed?.Invoke(this, EventArgs.Empty);
        };
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            VelocityControl.ChangeUnit(VelocityUnit.MetersPerSecond, 1, convert);
            MaxDistanceControl.ChangeUnit(DistanceUnit.Meter, 0, convert);
        }
        else
        {
            VelocityControl.ChangeUnit(VelocityUnit.MilesPerHour, 1, convert);
            MaxDistanceControl.ChangeUnit(DistanceUnit.Yard, 0, convert);
        }
        // Direction is always in degrees - no system switch needed
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        DirectionControl.Value = null;
        VelocityControl.Value = null;
        MaxDistanceControl.Value = null;
        MaxDistanceCheckBox.IsChecked = false;
        WindIndicator.Direction = 0;
    }

    #endregion
}
