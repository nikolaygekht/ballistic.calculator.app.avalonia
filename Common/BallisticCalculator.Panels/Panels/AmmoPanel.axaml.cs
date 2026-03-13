using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;

[assembly: InternalsVisibleTo("BallisticCalculator.Panels.Tests")]

namespace BallisticCalculator.Panels.Panels;

public partial class AmmoPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public AmmoPanel()
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
    }

    #endregion
}
