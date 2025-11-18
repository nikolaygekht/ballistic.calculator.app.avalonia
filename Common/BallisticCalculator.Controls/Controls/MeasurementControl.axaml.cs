using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Gehtsoft.Measurements;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Controls.Models;
using System;
using System.Globalization;
using System.Reflection;

namespace BallisticCalculator.Controls.Controls;

public partial class MeasurementControl : UserControl
{
    private object? _controller; // Will be MeasurementController<T>
    private Type? _unitType;

    #region Styled Properties

    public static readonly StyledProperty<Type?> UnitTypeProperty =
        AvaloniaProperty.Register<MeasurementControl, Type?>(
            nameof(UnitType),
            coerce: (obj, value) =>
            {
                if (obj is MeasurementControl control && value != null)
                {
                    control._unitType = (Type)value;
                    control.InitializeController();
                }
                return value;
            });

    public static readonly StyledProperty<double> IncrementProperty =
        AvaloniaProperty.Register<MeasurementControl, double>(
            nameof(Increment), 1.0);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<MeasurementControl, double>(
            nameof(Minimum), -10000.0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<MeasurementControl, double>(
            nameof(Maximum), 10000.0);

    public static readonly StyledProperty<int?> DecimalPointsProperty =
        AvaloniaProperty.Register<MeasurementControl, int?>(
            nameof(DecimalPoints), null);

    public static readonly StyledProperty<double> UnitPartWidthProperty =
        AvaloniaProperty.Register<MeasurementControl, double>(
            nameof(UnitPartWidth), 80.0);

    public static readonly StyledProperty<CultureInfo> CultureProperty =
        AvaloniaProperty.Register<MeasurementControl, CultureInfo>(
            nameof(Culture), CultureInfo.InvariantCulture);

    #endregion

    #region Properties

    public Type? UnitType
    {
        get => GetValue(UnitTypeProperty);
        set => SetValue(UnitTypeProperty, value);
    }

    /// <summary>
    /// The measurement value - reads directly from UI (WinForms pattern)
    /// </summary>
    public object? Value
    {
        get
        {
            // Always read from UI controls (like WinForms)
            if (UnitPart?.SelectedItem == null || _controller == null)
                return null;

            var unit = GetSelectedUnit();
            if (unit == null) return null;

            var valueMethod = _controller.GetType().GetMethod("Value");
            return valueMethod?.Invoke(_controller, new object?[] { NumericPart?.Text ?? "", unit, DecimalPoints, Culture });
        }
        set
        {
            // Write directly to UI controls (like WinForms)
            if (NumericPart == null || UnitPart == null)
                return;

            if (value == null)
            {
                NumericPart.Text = "";
                return;
            }

            if (_controller == null)
                return;

            var parseValueMethod = _controller.GetType().GetMethod("ParseValue");
            var parameters = new object?[] { value, null, null, DecimalPoints, Culture };
            parseValueMethod?.Invoke(_controller, parameters);

            string text = (string)(parameters[1] ?? "");
            object? unit = parameters[2];

            NumericPart.Text = text;
            SelectUnit(unit);
        }
    }

    public double Increment
    {
        get => GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int? DecimalPoints
    {
        get => GetValue(DecimalPointsProperty);
        set => SetValue(DecimalPointsProperty, value);
    }

    public double UnitPartWidth
    {
        get => GetValue(UnitPartWidthProperty);
        set => SetValue(UnitPartWidthProperty, value);
    }

    public CultureInfo Culture
    {
        get => GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(NumericPart?.Text);

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Constructor & Initialization

    public MeasurementControl()
    {
        InitializeComponent();

        if (UnitPart != null)
            UnitPart.Width = UnitPartWidth;

        if (_unitType != null)
            UpdateUnits();

        WireEvents();
    }

    private void WireEvents()
    {
        this.GotFocus += (s, e) => NumericPart?.Focus();

        if (NumericPart != null)
        {
            NumericPart.KeyDown += NumericPart_KeyDown;
            NumericPart.AddHandler(TextInputEvent, NumericPart_TextInput, RoutingStrategies.Tunnel);
            NumericPart.TextChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        }

        if (UnitPart != null)
        {
            UnitPart.SelectionChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        }

        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == IncrementProperty)
                _controller?.GetType().GetProperty("Increment")?.SetValue(_controller, (double)e.NewValue!);
            else if (e.Property == MinimumProperty)
                _controller?.GetType().GetProperty("Minimum")?.SetValue(_controller, (double)e.NewValue!);
            else if (e.Property == MaximumProperty)
                _controller?.GetType().GetProperty("Maximum")?.SetValue(_controller, (double)e.NewValue!);
            else if (e.Property == DecimalPointsProperty)
                _controller?.GetType().GetProperty("DecimalPoints")?.SetValue(_controller, (int?)e.NewValue);
            else if (e.Property == UnitPartWidthProperty && UnitPart != null)
                UnitPart.Width = (double)e.NewValue!;
        };
    }

    private void InitializeController()
    {
        if (_unitType == null) return;

        var controllerType = typeof(MeasurementController<>).MakeGenericType(_unitType);
        _controller = Activator.CreateInstance(controllerType);

        SyncPropertiesToController();
        UpdateUnits();
    }

    private void UpdateUnits()
    {
        if (UnitPart == null || _unitType == null) return;

        UnitPart.Items.Clear();

        var measurementType = typeof(Measurement<>).MakeGenericType(_unitType);
        var getUnitNamesMethod = measurementType.GetMethod("GetUnitNames", BindingFlags.Public | BindingFlags.Static);
        if (getUnitNamesMethod == null) return;

        var tuples = getUnitNamesMethod.Invoke(null, Array.Empty<object>()) as System.Collections.IEnumerable;
        if (tuples == null) return;

        var tupleType = typeof(Tuple<,>).MakeGenericType(_unitType, typeof(string));
        var unitProperty = tupleType.GetProperty("Item1");
        var nameProperty = tupleType.GetProperty("Item2");

        foreach (var tuple in tuples)
        {
            var unitValue = unitProperty?.GetValue(tuple);
            var unitName = nameProperty?.GetValue(tuple) as string;

            if (unitValue != null && unitName != null)
                UnitPart.Items.Add(new UnitItem(unitValue, unitName));
        }

        if (UnitPart.Items.Count > 0)
            UnitPart.SelectedIndex = 0;
    }

    private void SyncPropertiesToController()
    {
        if (_controller == null) return;

        var controllerType = _controller.GetType();
        controllerType.GetProperty("Increment")?.SetValue(_controller, Increment);
        controllerType.GetProperty("Minimum")?.SetValue(_controller, Minimum);
        controllerType.GetProperty("Maximum")?.SetValue(_controller, Maximum);
        controllerType.GetProperty("DecimalPoints")?.SetValue(_controller, DecimalPoints);
    }

    #endregion

    #region Keyboard Input

    private void NumericPart_TextInput(object? sender, TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text) || NumericPart == null || _controller == null)
            return;

        foreach (char c in e.Text)
        {
            if (c == '\b') continue;

            var allowKeyMethod = _controller.GetType().GetMethod("AllowKeyInEditor");
            var allowed = (bool)(allowKeyMethod?.Invoke(_controller, new object?[]
            {
                NumericPart.Text ?? "",
                NumericPart.CaretIndex,
                NumericPart.SelectionEnd - NumericPart.SelectionStart,
                c,
                Culture
            }) ?? false);

            if (!allowed)
            {
                e.Handled = true;
                return;
            }
        }
    }

    private void NumericPart_KeyDown(object? sender, KeyEventArgs e)
    {
        if (NumericPart == null || UnitPart == null)
            return;

        if (e.Key == Key.Up && e.KeyModifiers == KeyModifiers.None)
        {
            DoIncrement(true);
            e.Handled = true;
        }
        else if (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.None)
        {
            DoIncrement(false);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab && e.KeyModifiers == KeyModifiers.None)
        {
            UnitPart.Focus();
            e.Handled = true;
        }
    }

    private void DoIncrement(bool increment)
    {
        if (NumericPart == null || _controller == null)
            return;

        var incrementMethod = _controller.GetType().GetMethod("IncrementValue");
        var result = incrementMethod?.Invoke(_controller, new object?[]
        {
            NumericPart.Text ?? "",
            increment,
            Culture
        }) as string;

        if (result != null)
        {
            NumericPart.Text = result;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Helper Methods

    private object? GetSelectedUnit()
    {
        if (UnitPart?.SelectedItem is UnitItem unitItem)
            return unitItem.Unit;
        return null;
    }

    private void SelectUnit(object? unit)
    {
        if (UnitPart == null || unit == null) return;

        foreach (var item in UnitPart.Items)
        {
            if (item is UnitItem unitItem && unitItem.Unit.Equals(unit))
            {
                UnitPart.SelectedItem = item;
                return;
            }
        }
    }

    #endregion

    #region Public API

    public Measurement<T>? GetValue<T>() where T : Enum
    {
        if (Value is Measurement<T> measurement)
            return measurement;
        return null;
    }

    public void SetValue<T>(Measurement<T> value) where T : Enum
    {
        Value = value;
    }

    public void ChangeUnit<T>(T unit, int? accuracy = null) where T : Enum
    {
        var measurement = GetValue<T>();
        if (measurement == null) return;

        var converted = measurement.Value.To(unit);
        DecimalPoints = accuracy;
        Value = converted;
    }

    #endregion
}
