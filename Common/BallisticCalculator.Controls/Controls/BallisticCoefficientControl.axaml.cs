using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Controls.Models;
using System;
using System.Globalization;
using System.Linq;

namespace BallisticCalculator.Controls.Controls;

public partial class BallisticCoefficientControl : UserControl
{
    private readonly BallisticCoefficientController _controller;

    // Precision transparency tracking (WinForms pattern)
    private BallisticCoefficient? _originalValue = null;
    private string? _originalText = null;
    private DragTableInfo? _originalTable = null;

    #region Styled Properties

    public static readonly StyledProperty<double> IncrementProperty =
        AvaloniaProperty.Register<BallisticCoefficientControl, double>(
            nameof(Increment), 0.001);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<BallisticCoefficientControl, double>(
            nameof(Minimum), 0.001);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<BallisticCoefficientControl, double>(
            nameof(Maximum), 2.0);

    public static readonly StyledProperty<int?> DecimalPointsProperty =
        AvaloniaProperty.Register<BallisticCoefficientControl, int?>(
            nameof(DecimalPoints), 3);

    public static readonly StyledProperty<double> TablePartWidthProperty =
        AvaloniaProperty.Register<BallisticCoefficientControl, double>(
            nameof(TablePartWidth), 80.0);

    public static readonly StyledProperty<CultureInfo> CultureProperty =
        AvaloniaProperty.Register<BallisticCoefficientControl, CultureInfo>(
            nameof(Culture), CultureInfo.InvariantCulture);

    #endregion

    #region Properties

    /// <summary>
    /// The ballistic coefficient value - reads directly from UI (WinForms pattern with precision transparency)
    /// </summary>
    public BallisticCoefficient? Value
    {
        get
        {
            // Precision transparency: If text and table unchanged, return original value
            if (_originalValue.HasValue &&
                _originalText == NumericPart?.Text &&
                _originalTable != null &&
                TablePart?.SelectedItem is DragTableInfo currentTable &&
                currentTable.Value == _originalTable.Value)
            {
                return _originalValue;
            }

            // Otherwise parse from current text
            if (TablePart?.SelectedItem is not DragTableInfo table)
                return null;

            return _controller.Value(NumericPart?.Text ?? "", table, DecimalPoints, Culture);
        }
        set
        {
            // Write directly to UI controls (like WinForms)
            if (NumericPart == null || TablePart == null)
                return;

            if (value == null)
            {
                NumericPart.Text = "";
                _originalText = "";
                _originalValue = null;
                _originalTable = (DragTableInfo?)TablePart.SelectedItem;
                return;
            }

            // Parse value to text and table
            _controller.ParseValue(value.Value, out string text, out DragTableInfo table, DecimalPoints, Culture);

            // Update UI
            NumericPart.Text = text;
            SelectTable(table);

            // Store original for precision transparency
            _originalText = text;
            _originalTable = table;
            _originalValue = value;
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

    public double TablePartWidth
    {
        get => GetValue(TablePartWidthProperty);
        set => SetValue(TablePartWidthProperty, value);
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

    public BallisticCoefficientControl()
    {
        _controller = new BallisticCoefficientController();
        InitializeComponent();

        // Initialize drag tables
        UpdateTables();

        // Set initial TablePart width
        if (TablePart != null)
            TablePart.Width = TablePartWidth;

        // Wire up events
        WireEvents();

        // Sync properties to controller
        SyncPropertiesToController();
    }

    private void UpdateTables()
    {
        if (TablePart == null) return;

        TablePart.Items.Clear();
        var tables = _controller.GetDragTables(out int defaultIndex);
        foreach (var table in tables)
            TablePart.Items.Add(table);
        TablePart.SelectedIndex = defaultIndex;
    }

    private void WireEvents()
    {
        // Focus management
        this.GotFocus += (s, e) => NumericPart?.Focus();

        // Keyboard handling
        if (NumericPart != null)
        {
            NumericPart.KeyDown += NumericPart_KeyDown;
            NumericPart.AddHandler(TextInputEvent, NumericPart_TextInput, RoutingStrategies.Tunnel);
            NumericPart.TextChanged += (s, e) =>
            {
                // Clear precision transparency when user edits text
                if (_originalText != null && NumericPart?.Text != _originalText)
                {
                    _originalValue = null;
                    _originalText = null;
                    _originalTable = null;
                }
                Changed?.Invoke(this, EventArgs.Empty);
            };
        }

        // ComboBox changes
        if (TablePart != null)
        {
            TablePart.SelectionChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        }

        // Property changed
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == IncrementProperty)
                _controller.Increment = (double)e.NewValue!;
            else if (e.Property == MinimumProperty)
                _controller.Minimum = (double)e.NewValue!;
            else if (e.Property == MaximumProperty)
                _controller.Maximum = (double)e.NewValue!;
            else if (e.Property == DecimalPointsProperty)
                _controller.DecimalPoints = (int?)e.NewValue;
            else if (e.Property == TablePartWidthProperty && TablePart != null)
                TablePart.Width = (double)e.NewValue!;
        };
    }

    private void SyncPropertiesToController()
    {
        _controller.Increment = Increment;
        _controller.Minimum = Minimum;
        _controller.Maximum = Maximum;
        _controller.DecimalPoints = DecimalPoints;
    }

    #endregion

    #region Keyboard Input Handling

    private void NumericPart_TextInput(object? sender, TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text) || NumericPart == null)
            return;

        foreach (char c in e.Text)
        {
            if (c == '\b') // Backspace
                continue;

            if (!_controller.AllowKeyInEditor(
                NumericPart.Text ?? "",
                NumericPart.CaretIndex,
                NumericPart.SelectionEnd - NumericPart.SelectionStart,
                c,
                Culture))
            {
                e.Handled = true;
                return;
            }
        }
    }

    private void NumericPart_KeyDown(object? sender, KeyEventArgs e)
    {
        if (NumericPart == null || TablePart == null)
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
            // Tab from NumericPart moves to TablePart
            TablePart.Focus();
            e.Handled = true;
        }
    }

    private void DoIncrement(bool increment)
    {
        if (NumericPart == null || TablePart?.SelectedItem is not DragTableInfo table)
            return;

        NumericPart.Text = _controller.IncrementValue(
            NumericPart.Text ?? "",
            table,
            increment,
            Culture);

        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Helper Methods

    private void SelectTable(DragTableInfo table)
    {
        if (TablePart == null) return;

        foreach (var item in TablePart.Items)
        {
            if (item is DragTableInfo tableItem && tableItem.Value == table.Value)
            {
                TablePart.SelectedItem = item;
                return;
            }
        }
    }

    #endregion

    #region Public Helper Methods

    public void ForceCulture(CultureInfo cultureInfo)
    {
        BallisticCoefficient? value = null;
        if (!IsEmpty)
            value = Value;

        Culture = cultureInfo;

        if (value != null)
            Value = value;
    }

    /// <summary>
    /// Gets the available drag table names in order (for debugging/testing)
    /// </summary>
    public IEnumerable<string> GetAvailableTableNames()
    {
        return TablePart?.Items.Cast<DragTableInfo>().Select(t => t.Name) ?? Enumerable.Empty<string>();
    }

    #endregion
}
