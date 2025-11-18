# BallisticCoefficientControl - Implementation & Test Plan

**Last Updated:** 2025-11-17

---

## Overview

Simple composite control for editing ballistic coefficients, following the proven WinForms pattern from BallisticCalculator1.

**Architecture:** TextBox (numeric value) + ComboBox (drag table selection) + Controller (pure logic)

**Estimated Total Time:** 5.5-7.5 hours

---

## Design Pattern (From WinForms)

### Composite Control Pattern

```
┌─────────────────────────────────────────────┐
│  BallisticCoefficientControl                │
│  (Avalonia UserControl)                     │
│                                             │
│  ┌──────────────────┐  ┌─────────────────┐ │
│  │   NumericPart    │  │   TablePart     │ │
│  │   (TextBox)      │  │   (ComboBox)    │ │
│  │   - Fills space  │  │   - Fixed width │ │
│  │   - Right-align  │  │   - Right-align │ │
│  └──────────────────┘  └─────────────────┘ │
│                                             │
│  ┌─────────────────────────────────────────┤
│  │ ValidationError (TextBlock - mobile)    │
│  └─────────────────────────────────────────┘
│                                             │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────────┐
    │ BallisticCoefficientController       │
    │ (Pure Logic - No UI Dependencies)    │
    │                                      │
    │ • GetDragTables()                    │
    │ • Value() / ParseValue()             │
    │ • IncrementValue()                   │
    │ • AllowKeyInEditor()                 │
    └──────────────────────────────────────┘
```

**Key Principle:** Reuse standard TextBox/ComboBox features (editing, copy/paste, navigation) - don't reinvent the wheel!

---

## Requirements

### Functional Requirements

1. **Two-part composite control:**
   - Numeric TextBox (fills available space, right-aligned text)
   - Drag Table ComboBox (fixed width, right-aligned)

2. **Drag tables order:** G1, G2, G5, G6, G7, G8, GI, GS, RA4, GC (10 tables)

3. **Configuration properties:**
   - `Increment` (default: 0.001)
   - `Minimum` (default: 0.001)
   - `Maximum` (default: 2.0)
   - `DecimalPoints` (default: 3)
   - `TablePartWidth` (default: 80)
   - `Culture` (default: InvariantCulture)

4. **Precision transparency (CRITICAL):**
   - If value set programmatically with higher precision than display
   - And user does NOT edit the text
   - Then getting value back returns EXACT original (no rounding)
   - Example: Set 0.45678, displays "0.457", get returns 0.45678

5. **Keyboard behavior:**
   - Filter input: digits and decimal separator only
   - Up/Down arrows: increment/decrement value
   - Tab navigation: NumericPart → TablePart → Exit

6. **Focus management:**
   - When control receives focus, NumericPart gets focus first
   - Tab moves from NumericPart to TablePart

7. **Mobile support:**
   - Touch target sizes: 32px (desktop), 44px (iOS), 48dp (Android)
   - InputScope="Number" for numeric keyboard
   - Inline validation (tooltips don't work on mobile)

### Non-Functional Requirements

1. **Culture support:** Respect decimal separator (period vs comma)
2. **Validation:** Real-time feedback for invalid input
3. **Events:** `Changed` event when value changes
4. **Testing:** Controller >95% coverage, integration tests for critical paths

---

## Phase 1: Review & Adjust Controller (1 hour)

### Current Status

✅ **Already have:** `BallisticCoefficientController` with 22/22 tests passing

**Existing methods:**
- `GetDragTables(out int defaultIndex)` - Returns 10 tables in correct order
- `Value(text, table, decimalPoints, culture)` - Parse text to BallisticCoefficient
- `ParseValue(bc, out text, out table, decimalPoints, culture)` - Format BC to text
- `IncrementValue(text, table, increment, culture)` - Increment/decrement
- `AllowKeyInEditor(text, caretIndex, selectionLength, char, culture)` - Key validation

**Existing properties:**
- `Increment`, `Minimum`, `Maximum`, `DecimalPoints`

### Step 1.1: Verify Controller Implementation

**Review checklist:**

```bash
# Run existing tests
cd /mnt/d/develop/homeapps/BallisticCalculator2
dotnet test Common/BallisticCalculator.Controls.Tests --filter "BallisticCoefficientControllerTests"
```

- [ ] All 22 tests passing
- [ ] GetDragTables returns correct order: G1, G2, G5, G6, G7, G8, GI, GS, RA4, GC
- [ ] Value parsing handles culture correctly
- [ ] ParseValue formats with DecimalPoints
- [ ] IncrementValue respects min/max bounds
- [ ] AllowKeyInEditor filters non-numeric characters

### Step 1.2: Minor Adjustments (if needed)

**Optional:** Add convenience method to match WinForms pattern exactly:

```csharp
/// <summary>
/// Increments or decrements value (WinForms pattern)
/// </summary>
/// <param name="currentText">Current text value</param>
/// <param name="direction">1 for increment, -1 for decrement</param>
/// <param name="culture">Culture for formatting</param>
public string DoIncrement(string currentText, int direction, CultureInfo? culture = null)
{
    bool increment = direction > 0;
    // Assumes SelectedTable is tracked externally
    // This is just a wrapper if needed
    return IncrementValue(currentText, /* table */, increment, culture ?? CultureInfo.InvariantCulture);
}
```

**Decision:** Only add if it simplifies the control code. Current `IncrementValue` method is sufficient.

### Deliverable

- ✅ Controller verified and ready
- ✅ All existing tests passing

---

## Phase 2: Create BallisticCoefficientControl (2.5-3.5 hours)

### Step 2.1: Create Control XAML

**File:** `Common/BallisticCalculator.Controls/Controls/BallisticCoefficientControl.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="300" d:DesignHeight="32"
             x:Class="BallisticCalculator.Controls.Controls.BallisticCoefficientControl">

  <!-- Platform-adaptive minimum height -->
  <UserControl.MinHeight>
    <OnPlatform x:TypeArguments="x:Double">
      <On Content="32">
        <On.Options>
          <OnPlatformOptions>
            <OnPlatformOptions.Platforms>Windows,macOS,Linux</OnPlatformOptions.Platforms>
          </OnPlatformOptions>
        </On.Options>
      </On>
      <On Content="44">
        <On.Options>
          <OnPlatformOptions>
            <OnPlatformOptions.Platforms>iOS</OnPlatformOptions.Platforms>
          </OnPlatformOptions>
        </On.Options>
      </On>
      <On Content="48">
        <On.Options>
          <OnPlatformOptions>
            <OnPlatformOptions.Platforms>Android</OnPlatformOptions.Platforms>
          </OnPlatformOptions>
        </On.Options>
      </On>
    </OnPlatform>
  </UserControl.MinHeight>

  <!-- Layout: Two rows (controls + validation) -->
  <Grid RowDefinitions="Auto,Auto">

    <!-- Row 0: Main controls -->
    <Grid Grid.Row="0" ColumnDefinitions="*,5,Auto">

      <!-- Column 0: Numeric TextBox (fills remaining space) -->
      <TextBox Grid.Column="0"
               x:Name="NumericPart"
               Watermark="0.000"
               HorizontalAlignment="Stretch"
               VerticalAlignment="Stretch"
               TextAlignment="Right"
               InputScope="Number">

        <!-- Platform-adaptive height -->
        <TextBox.MinHeight>
          <OnPlatform x:TypeArguments="x:Double">
            <On Content="32">
              <On.Options>
                <OnPlatformOptions>
                  <OnPlatformOptions.Platforms>Windows,macOS,Linux</OnPlatformOptions.Platforms>
                </OnPlatformOptions>
              </On.Options>
            </On>
            <On Content="44">
              <On.Options>
                <OnPlatformOptions>
                  <OnPlatformOptions.Platforms>iOS</OnPlatformOptions.Platforms>
                </OnPlatformOptions>
              </On.Options>
            </On>
            <On Content="48">
              <On.Options>
                <OnPlatformOptions>
                  <OnPlatformOptions.Platforms>Android</OnPlatformOptions.Platforms>
                </OnPlatformOptions>
              </On.Options>
            </On>
          </OnPlatform>
        </TextBox.MinHeight>
      </TextBox>

      <!-- Column 2: Drag Table ComboBox (fixed width, right-aligned) -->
      <ComboBox Grid.Column="2"
                x:Name="TablePart"
                HorizontalAlignment="Right"
                VerticalAlignment="Stretch"
                DropdownStyle="DropDown">

        <!-- Platform-adaptive height -->
        <ComboBox.MinHeight>
          <OnPlatform x:TypeArguments="x:Double">
            <On Content="32">
              <On.Options>
                <OnPlatformOptions>
                  <OnPlatformOptions.Platforms>Windows,macOS,Linux</OnPlatformOptions.Platforms>
                </OnPlatformOptions>
              </On.Options>
            </On>
            <On Content="44">
              <On.Options>
                <OnPlatformOptions>
                  <OnPlatformOptions.Platforms>iOS</OnPlatformOptions.Platforms>
                </OnPlatformOptions>
              </On.Options>
            </On>
            <On Content="48">
              <On.Options>
                <OnPlatformOptions>
                  <OnPlatformOptions.Platforms>Android</OnPlatformOptions.Platforms>
                </OnPlatformOptions>
              </On.Options>
            </On>
          </OnPlatform>
        </ComboBox.MinHeight>

        <ComboBox.ItemTemplate>
          <DataTemplate>
            <TextBlock Text="{Binding Name}" />
          </DataTemplate>
        </ComboBox.ItemTemplate>
      </ComboBox>
    </Grid>

    <!-- Row 1: Inline validation error (for mobile - tooltips don't work) -->
    <TextBlock Grid.Row="1"
               x:Name="ValidationErrorText"
               Foreground="Red"
               FontSize="12"
               Margin="0,2,0,0"
               TextWrapping="Wrap"
               IsVisible="False" />
  </Grid>
</UserControl>
```

**Key features:**
- ✅ Grid layout with 2 columns (TextBox fills, ComboBox fixed width)
- ✅ Platform-adaptive MinHeight (32/44/48)
- ✅ InputScope="Number" for mobile keyboards
- ✅ Inline validation TextBlock for mobile
- ✅ TextBox right-aligned
- ✅ ComboBox right-aligned

**Tasks:**
- [ ] Create XAML file
- [ ] Set up Grid layout (2 rows, inner grid 2 columns)
- [ ] Add NumericPart (TextBox) with right alignment
- [ ] Add TablePart (ComboBox) with ItemTemplate
- [ ] Add ValidationErrorText (TextBlock) for mobile
- [ ] Add platform-adaptive MinHeight to all controls

### Step 2.2: Create Control Code-Behind

**File:** `Common/BallisticCalculator.Controls/Controls/BallisticCoefficientControl.axaml.cs`

```csharp
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

    // Precision transparency tracking (WinForms pattern - lines 106-109)
    private BallisticCoefficient? _originalValue = null;
    private string? _originalText = null;
    private DragTableInfo? _originalTable = null;

    #region Styled Properties

    public static readonly StyledProperty<BallisticCoefficient?> ValueProperty =
        AvaloniaProperty.Register<BallisticCoefficientControl, BallisticCoefficient?>(
            nameof(Value),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

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

    public BallisticCoefficient? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
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
        this.GotFocus += OnControlGotFocus;

        // Keyboard handling
        if (NumericPart != null)
        {
            NumericPart.KeyDown += NumericPart_KeyDown;
            NumericPart.AddHandler(TextInputEvent, NumericPart_TextInput, RoutingStrategies.Tunnel);
            NumericPart.TextChanged += NumericPart_TextChanged;
        }

        // ComboBox changes
        if (TablePart != null)
        {
            TablePart.SelectionChanged += TablePart_SelectionChanged;
        }

        // Property changed
        this.PropertyChanged += OnPropertyChanged;
    }

    private void SyncPropertiesToController()
    {
        _controller.Increment = Increment;
        _controller.Minimum = Minimum;
        _controller.Maximum = Maximum;
        _controller.DecimalPoints = DecimalPoints;
    }

    #endregion

    #region Value Property Logic (Precision Transparency)

    // WinForms pattern - lines 110-118
    private BallisticCoefficient? GetValueInternal()
    {
        // If text and table unchanged, return original value (precision transparency!)
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

    // WinForms pattern - lines 119-137
    private void SetValueInternal(BallisticCoefficient? value)
    {
        if (NumericPart == null || TablePart == null)
            return;

        if (value == null)
        {
            NumericPart.Text = "";
            _originalTable = (DragTableInfo?)TablePart.SelectedItem;
            _originalText = "";
            _originalValue = null;
            ClearValidationError();
            return;
        }

        // Parse value to text and table
        _controller.ParseValue(value.Value, out string text, out DragTableInfo table, DecimalPoints, Culture);

        // Update UI
        NumericPart.Text = text;
        TablePart.SelectedItem = table;

        // Store original for precision transparency
        _originalText = text;
        _originalTable = table;
        _originalValue = value;

        // Validate
        ValidateValue();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
        {
            SetValueInternal((BallisticCoefficient?)e.NewValue);
        }
        else if (e.Property == IncrementProperty)
        {
            _controller.Increment = (double)e.NewValue!;
        }
        else if (e.Property == MinimumProperty)
        {
            _controller.Minimum = (double)e.NewValue!;
            ValidateValue();
        }
        else if (e.Property == MaximumProperty)
        {
            _controller.Maximum = (double)e.NewValue!;
            ValidateValue();
        }
        else if (e.Property == DecimalPointsProperty)
        {
            _controller.DecimalPoints = (int?)e.NewValue;
        }
        else if (e.Property == TablePartWidthProperty)
        {
            if (TablePart != null)
                TablePart.Width = (double)e.NewValue!;
        }
    }

    #endregion

    #region Focus Management

    private void OnControlGotFocus(object? sender, GotFocusEventArgs e)
    {
        // When control receives focus, focus NumericPart first
        NumericPart?.Focus();
    }

    #endregion

    #region Keyboard Input Handling

    // WinForms pattern - lines 185-192
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

    // WinForms pattern - lines 206-220
    private void NumericPart_KeyDown(object? sender, KeyEventArgs e)
    {
        if (NumericPart == null || TablePart == null)
            return;

        if (e.Key == Key.Up && e.KeyModifiers == KeyModifiers.None)
        {
            DoIncrement(1);
            e.Handled = true;
        }
        else if (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.None)
        {
            DoIncrement(-1);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab && e.KeyModifiers == KeyModifiers.None)
        {
            // Tab from NumericPart moves to TablePart
            TablePart.Focus();
            e.Handled = true;
        }
    }

    private void DoIncrement(int direction)
    {
        if (NumericPart == null || TablePart?.SelectedItem is not DragTableInfo table)
            return;

        bool increment = direction > 0;
        NumericPart.Text = _controller.IncrementValue(
            NumericPart.Text ?? "",
            table,
            increment,
            Culture);

        // Raise Changed event
        RaiseChanged();
    }

    #endregion

    #region Validation

    private void ValidateValue()
    {
        if (NumericPart == null)
            return;

        var value = GetValueInternal();

        if (string.IsNullOrWhiteSpace(NumericPart.Text))
        {
            ClearValidationError();
            return;
        }

        if (value == null)
        {
            ShowValidationError("Value must be a valid number");
            return;
        }

        if (value.Value.Value < Minimum)
        {
            ShowValidationError($"Value must be at least {Minimum}");
            return;
        }

        if (value.Value.Value > Maximum)
        {
            ShowValidationError($"Value must not exceed {Maximum}");
            return;
        }

        ClearValidationError();
    }

    private void ShowValidationError(string message)
    {
        if (ValidationErrorText == null)
            return;

        ValidationErrorText.Text = message;
        ValidationErrorText.IsVisible = true;

        // Also add visual indicator to TextBox
        if (NumericPart != null)
        {
            NumericPart.BorderBrush = Avalonia.Media.Brushes.Red;
            NumericPart.BorderThickness = new Thickness(2);
        }
    }

    private void ClearValidationError()
    {
        if (ValidationErrorText != null)
        {
            ValidationErrorText.IsVisible = false;
        }

        // Reset TextBox border
        if (NumericPart != null)
        {
            NumericPart.ClearValue(TextBox.BorderBrushProperty);
            NumericPart.ClearValue(TextBox.BorderThicknessProperty);
        }
    }

    #endregion

    #region Change Events

    // WinForms pattern - lines 196-204
    private void NumericPart_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ValidateValue();
        RaiseChanged();
    }

    private void TablePart_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RaiseChanged();
    }

    private void RaiseChanged()
    {
        // Update Value property (triggers precision logic)
        var newValue = GetValueInternal();
        if (newValue != Value)
        {
            SetCurrentValue(ValueProperty, newValue);
        }

        // Raise Changed event
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Public Helper Methods

    // WinForms pattern - lines 170-178
    public void ForceCulture(CultureInfo cultureInfo)
    {
        BallisticCoefficient? value = null;
        if (!IsEmpty)
            value = Value;

        Culture = cultureInfo;

        if (value != null)
            Value = value;
    }

    #endregion
}
```

**Key implementation points:**
- ✅ Precision transparency with `_originalValue`, `_originalText`, `_originalTable`
- ✅ Focus management (control focus → NumericPart focus)
- ✅ Tab navigation (NumericPart → TablePart)
- ✅ Keyboard filtering (AllowKeyInEditor)
- ✅ Up/Down arrow increment/decrement
- ✅ Validation with inline error display
- ✅ Changed event
- ✅ Culture support

**Tasks:**
- [ ] Create code-behind file
- [ ] Implement constructor and initialization
- [ ] Implement precision transparency logic (GetValueInternal/SetValueInternal)
- [ ] Wire up keyboard events (TextInput, KeyDown)
- [ ] Implement focus management
- [ ] Implement validation (ShowValidationError/ClearValidationError)
- [ ] Add Changed event
- [ ] Test tab navigation
- [ ] Test precision transparency

### Deliverable

- ✅ Working composite control
- ✅ Precision transparency working
- ✅ Keyboard navigation working
- ✅ Mobile-ready (adaptive sizing, inline validation)

---

## Phase 3: Testing (1-2 hours)

### Step 3.1: Controller Tests

**Status:** ✅ Already have 22/22 tests passing

**File:** `Common/BallisticCalculator.Controls.Tests/Controllers/BallisticCoefficientControllerTests.cs`

**Verify:**
```bash
dotnet test --filter "BallisticCoefficientControllerTests"
```

- [ ] All 22 tests passing
- [ ] >95% code coverage

### Step 3.2: Control Integration Tests

**File:** `Common/BallisticCalculator.Controls.Tests/UI/BallisticCoefficientControlTests.cs`

```csharp
using Avalonia.Headless.XUnit;
using Xunit;
using FluentAssertions;
using BallisticCalculator;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Controls.Models;
using System.Linq;

namespace BallisticCalculator.Controls.Tests.UI;

public class BallisticCoefficientControlTests
{
    [AvaloniaFact]
    public void Control_ShouldInitialize()
    {
        // Arrange & Act
        var control = new BallisticCoefficientControl();

        // Assert
        control.Should().NotBeNull();
        control.IsEmpty.Should().BeTrue();
        control.NumericPart.Should().NotBeNull();
        control.TablePart.Should().NotBeNull();
    }

    [AvaloniaFact]
    public void TablePart_ShouldContain10TablesInCorrectOrder()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        var tableNames = control.TablePart.Items.Cast<DragTableInfo>()
            .Select(t => t.Name).ToList();

        // Assert
        tableNames.Should().HaveCount(10);
        tableNames[0].Should().Be("G1");
        tableNames[1].Should().Be("G2");
        tableNames[2].Should().Be("G5");
        tableNames[3].Should().Be("G6");
        tableNames[4].Should().Be("G7");
        tableNames[5].Should().Be("G8");
        tableNames[6].Should().Be("GI");
        tableNames[7].Should().Be("GS");
        tableNames[8].Should().Be("RA4");
        tableNames[9].Should().Be("GC");
    }

    [AvaloniaFact]
    public void Value_SetAndGet_WithoutEdit_ShouldPreserveOriginalPrecision()
    {
        // Arrange
        var control = new BallisticCoefficientControl { DecimalPoints = 3 };
        var original = new BallisticCoefficient(0.45678, DragTableId.G1);

        // Act
        control.Value = original;
        var retrieved = control.Value;

        // Assert - CRITICAL: Should return EXACT original (precision transparency!)
        retrieved.Should().NotBeNull();
        retrieved!.Value.Value.Should().Be(0.45678); // Exact precision preserved
        retrieved.Value.Table.Should().Be(DragTableId.G1);
    }

    [AvaloniaFact]
    public void Value_Set_ShouldDisplayWithConfiguredPrecision()
    {
        // Arrange
        var control = new BallisticCoefficientControl { DecimalPoints = 3 };

        // Act
        control.Value = new BallisticCoefficient(0.45678, DragTableId.G1);

        // Assert - Display should show rounded value
        control.NumericPart.Text.Should().Be("0.457"); // 3 decimal places
    }

    [AvaloniaFact]
    public void Value_SetAndGet_AfterEdit_ShouldReturnEditedValue()
    {
        // Arrange
        var control = new BallisticCoefficientControl { DecimalPoints = 3 };
        var original = new BallisticCoefficient(0.45678, DragTableId.G1);
        control.Value = original;

        // Act - User edits text
        control.NumericPart.Text = "0.460";
        var retrieved = control.Value;

        // Assert - Should return edited value, not original
        retrieved.Should().NotBeNull();
        retrieved!.Value.Value.Should().Be(0.460);
    }

    [AvaloniaFact]
    public void Value_Set_ShouldUpdateTableSelection()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        control.Value = new BallisticCoefficient(0.275, DragTableId.G7);

        // Assert
        var selectedTable = (DragTableInfo?)control.TablePart.SelectedItem;
        selectedTable.Should().NotBeNull();
        selectedTable!.Value.Should().Be(DragTableId.G7);
    }

    [AvaloniaFact]
    public void TablePartWidth_ShouldBeConfigurable()
    {
        // Arrange & Act
        var control = new BallisticCoefficientControl { TablePartWidth = 100 };

        // Assert
        control.TablePart.Width.Should().Be(100);
    }

    [AvaloniaFact]
    public void Increment_ShouldRespectMinimum()
    {
        // Arrange
        var control = new BallisticCoefficientControl
        {
            Minimum = 0.001,
            Increment = 0.001
        };
        control.Value = new BallisticCoefficient(0.001, DragTableId.G1);

        // Act - Try to decrement below minimum
        control.NumericPart.Text = "0.001";
        // Simulate Down arrow (would call DoIncrement(-1))
        var table = (DragTableInfo)control.TablePart.SelectedItem!;
        var newText = control._controller.IncrementValue("0.001", table, false, control.Culture);

        // Assert - Should clamp to minimum
        double.Parse(newText).Should().BeGreaterThanOrEqualTo(0.001);
    }

    [AvaloniaFact]
    public void Increment_ShouldRespectMaximum()
    {
        // Arrange
        var control = new BallisticCoefficientControl
        {
            Maximum = 2.0,
            Increment = 0.001
        };
        control.Value = new BallisticCoefficient(2.0, DragTableId.G1);

        // Act - Try to increment above maximum
        var table = (DragTableInfo)control.TablePart.SelectedItem!;
        var newText = control._controller.IncrementValue("2.000", table, true, control.Culture);

        // Assert - Should clamp to maximum
        double.Parse(newText).Should().BeLessThanOrEqualTo(2.0);
    }

    [AvaloniaFact]
    public void IsEmpty_WhenTextEmpty_ShouldReturnTrue()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        control.NumericPart.Text = "";

        // Assert
        control.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void IsEmpty_WhenTextNotEmpty_ShouldReturnFalse()
    {
        // Arrange
        var control = new BallisticCoefficientControl();

        // Act
        control.Value = new BallisticCoefficient(0.450, DragTableId.G1);

        // Assert
        control.IsEmpty.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Changed_Event_ShouldFireWhenTextChanges()
    {
        // Arrange
        var control = new BallisticCoefficientControl();
        bool eventFired = false;
        control.Changed += (s, e) => eventFired = true;

        // Act
        control.NumericPart.Text = "0.450";

        // Assert
        eventFired.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Changed_Event_ShouldFireWhenTableChanges()
    {
        // Arrange
        var control = new BallisticCoefficientControl();
        control.Value = new BallisticCoefficient(0.450, DragTableId.G1);
        bool eventFired = false;
        control.Changed += (s, e) => eventFired = true;

        // Act
        var g7Table = control.TablePart.Items.Cast<DragTableInfo>()
            .First(t => t.Value == DragTableId.G7);
        control.TablePart.SelectedItem = g7Table;

        // Assert
        eventFired.Should().BeTrue();
    }
}
```

**Test coverage:**
- ✅ Control initialization
- ✅ Drag tables order and count
- ✅ Precision transparency (CRITICAL)
- ✅ Display precision
- ✅ Edit behavior
- ✅ Table selection
- ✅ TablePartWidth configuration
- ✅ Min/Max enforcement
- ✅ IsEmpty property
- ✅ Changed event

**Tasks:**
- [ ] Create test file
- [ ] Write 12 integration tests
- [ ] Run tests: `dotnet test --filter "BallisticCoefficientControlTests"`
- [ ] Verify all tests pass
- [ ] Verify precision transparency test passes (CRITICAL)

### Step 3.3: Test Execution

```bash
# Run all tests
cd /mnt/d/develop/homeapps/BallisticCalculator2
dotnet test Common/BallisticCalculator.Controls.Tests

# Run only controller tests
dotnet test --filter "BallisticCoefficientControllerTests"

# Run only control tests
dotnet test --filter "BallisticCoefficientControlTests"
```

**Success criteria:**
- [ ] All controller tests pass (22/22)
- [ ] All control tests pass (12/12)
- [ ] Total: 34 tests passing
- [ ] Zero test failures

### Deliverable

- ✅ 34 tests passing
- ✅ Precision transparency verified
- ✅ Critical paths tested

---

## Phase 4: Integration with DebugApp (1 hour)

### Step 4.1: Add Control to DebugApp

**File:** `Desktop/DebugApp/Views/MainWindow.axaml`

Add tab for testing:

```xml
<TabItem Header="Ballistic Coefficient">
  <ScrollViewer>
    <StackPanel Margin="20" Spacing="15">

      <!-- Title -->
      <TextBlock Text="BallisticCoefficientControl Test"
                 FontSize="20" FontWeight="Bold"
                 Margin="0,0,0,10"/>

      <!-- Basic Test -->
      <Border BorderBrush="Gray" BorderThickness="1" Padding="15" CornerRadius="5">
        <StackPanel Spacing="10">
          <TextBlock Text="Basic Usage" FontWeight="Bold" FontSize="16" />

          <TextBlock Text="Ballistic Coefficient:" />
          <controls:BallisticCoefficientControl
              x:Name="BasicBC"
              Width="300"
              HorizontalAlignment="Left" />

          <StackPanel Orientation="Horizontal" Spacing="10">
            <Button Content="Set 0.450 G1" Click="OnSetBasicBC" />
            <Button Content="Get Value" Click="OnGetBasicBC" />
            <Button Content="Clear" Click="OnClearBC" />
          </StackPanel>

          <Border Background="LightGray" Padding="10" CornerRadius="3">
            <TextBlock x:Name="BasicBCDisplay"
                       Text="Value: (none)"
                       TextWrapping="Wrap" />
          </Border>
        </StackPanel>
      </Border>

      <!-- Precision Transparency Test (CRITICAL) -->
      <Border BorderBrush="DarkOrange" BorderThickness="2" Padding="15" CornerRadius="5">
        <StackPanel Spacing="10">
          <TextBlock Text="⭐ Precision Transparency Test"
                     FontWeight="Bold" FontSize="16"
                     Foreground="DarkOrange" />

          <TextBlock TextWrapping="Wrap">
            <Run Text="Critical test: Set value with 5 decimals (0.45678), control displays 3 decimals (0.457)." />
            <LineBreak />
            <Run Text="Without editing, getting value back should return EXACT original (0.45678)." FontWeight="Bold" />
          </TextBlock>

          <controls:BallisticCoefficientControl
              x:Name="PrecisionBC"
              DecimalPoints="3"
              Width="300"
              HorizontalAlignment="Left" />

          <StackPanel Orientation="Horizontal" Spacing="10">
            <Button Content="1. Set 0.45678" Click="OnSetPrecisionBC" />
            <Button Content="2. Get Value" Click="OnGetPrecisionBC" />
            <Button Content="3. Edit &amp; Get" Click="OnEditPrecisionBC" />
          </StackPanel>

          <Border Background="LightYellow" Padding="10" CornerRadius="3">
            <StackPanel Spacing="5">
              <TextBlock x:Name="PrecisionResult"
                         Text="Test result: Click buttons in order"
                         FontWeight="Bold"
                         TextWrapping="Wrap" />
              <TextBlock x:Name="PrecisionDetails"
                         Text=""
                         FontSize="12"
                         TextWrapping="Wrap" />
            </StackPanel>
          </Border>
        </StackPanel>
      </Border>

      <!-- Drag Tables Order Test -->
      <Border BorderBrush="Blue" BorderThickness="1" Padding="15" CornerRadius="5">
        <StackPanel Spacing="10">
          <TextBlock Text="Drag Tables Order Test" FontWeight="Bold" FontSize="16" />

          <TextBlock TextWrapping="Wrap">
            <Run Text="Expected order: " />
            <Run Text="G1, G2, G5, G6, G7, G8, GI, GS, RA4, GC" FontWeight="Bold" />
          </TextBlock>

          <controls:BallisticCoefficientControl
              x:Name="TablesBC"
              Width="300"
              HorizontalAlignment="Left" />

          <Button Content="Verify Tables Order" Click="OnVerifyTables" />

          <Border Background="LightGray" Padding="10" CornerRadius="3">
            <TextBlock x:Name="TablesDisplay"
                       Text="Click button to verify"
                       TextWrapping="Wrap" />
          </Border>
        </StackPanel>
      </Border>

      <!-- Keyboard Navigation Test -->
      <Border BorderBrush="Green" BorderThickness="1" Padding="15" CornerRadius="5">
        <StackPanel Spacing="10">
          <TextBlock Text="Keyboard Navigation Test" FontWeight="Bold" FontSize="16" />

          <TextBlock TextWrapping="Wrap">
            <Run Text="Test:" />
            <LineBreak />
            <Run Text="• Tab into control → NumericPart gets focus" />
            <LineBreak />
            <Run Text="• Tab again → TablePart gets focus" />
            <LineBreak />
            <Run Text="• Up/Down arrows in NumericPart → increment/decrement" />
          </TextBlock>

          <controls:BallisticCoefficientControl
              x:Name="KeyboardBC"
              Width="300"
              Increment="0.010"
              HorizontalAlignment="Left" />

          <TextBlock Text="Increment: 0.010" FontStyle="Italic" FontSize="12" />
        </StackPanel>
      </Border>

      <!-- Min/Max Test -->
      <Border BorderBrush="Red" BorderThickness="1" Padding="15" CornerRadius="5">
        <StackPanel Spacing="10">
          <TextBlock Text="Min/Max Validation Test" FontWeight="Bold" FontSize="16" />

          <TextBlock TextWrapping="Wrap">
            <Run Text="Min: 0.100, Max: 1.000" FontWeight="Bold" />
            <LineBreak />
            <Run Text="Try entering values outside this range or use arrows at boundaries." />
          </TextBlock>

          <controls:BallisticCoefficientControl
              x:Name="ValidationBC"
              Minimum="0.100"
              Maximum="1.000"
              Width="300"
              HorizontalAlignment="Left" />

          <TextBlock Text="Validation errors will appear below the control (mobile-friendly inline validation)."
                     FontStyle="Italic" FontSize="12" />
        </StackPanel>
      </Border>

      <!-- Sizing Test -->
      <Border BorderBrush="Purple" BorderThickness="1" Padding="15" CornerRadius="5">
        <StackPanel Spacing="10">
          <TextBlock Text="Sizing Test" FontWeight="Bold" FontSize="16" />

          <TextBlock TextWrapping="Wrap">
            <Run Text="TablePartWidth can be configured. Default: 80px" />
          </TextBlock>

          <TextBlock Text="Default (80px):" />
          <controls:BallisticCoefficientControl
              x:Name="SizingBC1"
              Width="300"
              HorizontalAlignment="Stretch" />

          <TextBlock Text="Custom (120px):" />
          <controls:BallisticCoefficientControl
              x:Name="SizingBC2"
              TablePartWidth="120"
              Width="300"
              HorizontalAlignment="Stretch" />
        </StackPanel>
      </Border>

    </StackPanel>
  </ScrollViewer>
</TabItem>
```

### Step 4.2: Add Event Handlers

**File:** `Desktop/DebugApp/Views/MainWindow.axaml.cs`

```csharp
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using System.Linq;

namespace DebugApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Basic BC Test
    private void OnSetBasicBC(object? sender, RoutedEventArgs e)
    {
        BasicBC.Value = new BallisticCoefficient(0.450, DragTableId.G1);
        BasicBCDisplay.Text = "Set: 0.450 G1";
    }

    private void OnGetBasicBC(object? sender, RoutedEventArgs e)
    {
        var value = BasicBC.Value;
        BasicBCDisplay.Text = value != null
            ? $"Value: {value.Value.Value:F5} {value.Value.Table}\nDisplay text: \"{BasicBC.NumericPart.Text}\""
            : "Value: (none)";
    }

    private void OnClearBC(object? sender, RoutedEventArgs e)
    {
        BasicBC.Value = null;
        BasicBCDisplay.Text = "Cleared";
    }

    // Precision Transparency Test (CRITICAL)
    private void OnSetPrecisionBC(object? sender, RoutedEventArgs e)
    {
        PrecisionBC.Value = new BallisticCoefficient(0.45678, DragTableId.G1);

        PrecisionResult.Text = "✓ Step 1 Complete: Set 0.45678";
        PrecisionResult.Foreground = Brushes.Black;
        PrecisionDetails.Text = $"Display shows: \"{PrecisionBC.NumericPart.Text}\" (should be \"0.457\" with 3 decimals)\n" +
                                "Now click '2. Get Value' WITHOUT editing the text.";
    }

    private void OnGetPrecisionBC(object? sender, RoutedEventArgs e)
    {
        var value = PrecisionBC.Value;

        if (value?.Value == 0.45678)
        {
            PrecisionResult.Text = "✅ SUCCESS! Precision transparency working!";
            PrecisionResult.Foreground = Brushes.Green;
            PrecisionDetails.Text = $"Retrieved value: {value.Value.Value:F5} (exact match!)\n" +
                                    $"Display text: \"{PrecisionBC.NumericPart.Text}\"\n" +
                                    "Even though displayed as 0.457, the full precision (0.45678) was preserved!";
        }
        else
        {
            PrecisionResult.Text = "❌ FAILED! Precision lost!";
            PrecisionResult.Foreground = Brushes.Red;
            PrecisionDetails.Text = $"Retrieved value: {value?.Value ?? 0:F5}\n" +
                                    $"Expected: 0.45678\n" +
                                    "This is a critical bug - precision should be preserved when not edited.";
        }
    }

    private void OnEditPrecisionBC(object? sender, RoutedEventArgs e)
    {
        // Simulate user edit
        PrecisionBC.NumericPart.Text = "0.460";
        var value = PrecisionBC.Value;

        if (value?.Value == 0.460)
        {
            PrecisionResult.Text = "✅ Edit behavior correct!";
            PrecisionResult.Foreground = Brushes.Green;
            PrecisionDetails.Text = $"After editing to 0.460, retrieved value is: {value.Value.Value:F3}\n" +
                                    "Correct: edited value is returned, not the original.";
        }
        else
        {
            PrecisionResult.Text = "❌ Edit behavior incorrect!";
            PrecisionResult.Foreground = Brushes.Red;
            PrecisionDetails.Text = $"After editing to 0.460, got: {value?.Value ?? 0:F3}\n" +
                                    "Expected: 0.460";
        }
    }

    // Tables Order Test
    private void OnVerifyTables(object? sender, RoutedEventArgs e)
    {
        var tables = TablesBC.TablePart.Items.Cast<DragTableInfo>().Select(t => t.Name).ToList();
        var expected = new[] { "G1", "G2", "G5", "G6", "G7", "G8", "GI", "GS", "RA4", "GC" };

        bool correct = tables.SequenceEqual(expected);

        if (correct)
        {
            TablesDisplay.Text = $"✅ Correct order!\n{string.Join(", ", tables)}";
            TablesDisplay.Foreground = Brushes.Green;
        }
        else
        {
            TablesDisplay.Text = $"❌ Wrong order!\nGot: {string.Join(", ", tables)}\n" +
                                 $"Expected: {string.Join(", ", expected)}";
            TablesDisplay.Foreground = Brushes.Red;
        }
    }
}
```

### Step 4.3: Manual Testing

**Run DebugApp:**
```bash
cd /mnt/d/develop/homeapps/BallisticCalculator2
dotnet run --project Desktop/DebugApp/DebugApp.csproj
```

**Manual testing checklist:**

#### Basic Functionality
- [ ] Control renders correctly
- [ ] TextBox accepts numeric input (digits, one decimal separator)
- [ ] TextBox rejects letters
- [ ] Can copy text from TextBox (Ctrl+C)
- [ ] Can paste valid numbers into TextBox
- [ ] ComboBox shows 10 drag tables
- [ ] ComboBox allows selection

#### Precision Transparency (CRITICAL)
- [ ] Set 0.45678 → displays "0.457"
- [ ] Get value without editing → returns 0.45678 exactly ✅
- [ ] Edit to "0.460" → get returns 0.460
- [ ] Changed event fires on both text and table changes

#### Tables Order
- [ ] Tables in correct order: G1, G2, G5, G6, G7, G8, GI, GS, RA4, GC
- [ ] G1 is default (selected on init)

#### Keyboard Navigation
- [ ] Tab into control → NumericPart gets focus
- [ ] Tab from NumericPart → TablePart gets focus
- [ ] Tab from TablePart → exits control
- [ ] Up arrow in NumericPart → increments value
- [ ] Down arrow in NumericPart → decrements value
- [ ] Typing numbers works correctly

#### Sizing
- [ ] NumericPart fills available space (expands with control width)
- [ ] TablePart has fixed width (80px default, 120px when configured)
- [ ] TablePart aligned to right

#### Validation
- [ ] Empty value shows no error
- [ ] Invalid value (letters) shows inline error
- [ ] Value below minimum shows inline error with message
- [ ] Value above maximum shows inline error with message
- [ ] Valid value clears error
- [ ] Error appears below control (inline, not tooltip)

#### Mobile (if testing on iOS/Android)
- [ ] Touch target height: 44px (iOS) or 48dp (Android)
- [ ] Tap TextBox → numeric keyboard appears
- [ ] Inline validation visible (not tooltip)
- [ ] Controls usable with touch

### Deliverable

- ✅ DebugApp runs successfully
- ✅ All manual tests pass
- ✅ Precision transparency verified
- ✅ Tables order verified
- ✅ Keyboard navigation works
- ✅ Mobile-ready

---

## Manual Testing Checklist (Complete)

### Desktop Testing

#### Visual Appearance
- [ ] Control looks good on Windows
- [ ] Control looks good on macOS (if available)
- [ ] Control looks good on Linux (if available)
- [ ] NumericPart and TablePart aligned properly
- [ ] 5px spacing between parts
- [ ] Right alignment working

#### Input & Editing
- [ ] Can type digits (0-9)
- [ ] Can type decimal separator (period)
- [ ] Cannot type letters (a-z, A-Z)
- [ ] Cannot type special chars (@, #, $, etc.)
- [ ] Can use Backspace/Delete
- [ ] Can use arrow keys for cursor movement
- [ ] Can use Home/End
- [ ] Can select text (Shift+arrows, double-click)
- [ ] Can copy (Ctrl+C)
- [ ] Can paste valid numbers
- [ ] Paste invalid text is rejected (or shows error)

#### Keyboard Shortcuts
- [ ] Up arrow increments value
- [ ] Down arrow decrements value
- [ ] Tab moves from NumericPart to TablePart
- [ ] Shift+Tab moves backwards
- [ ] Enter key (no special action, just normal)

#### Focus Management
- [ ] Click control → NumericPart gets focus
- [ ] Tab into control → NumericPart gets focus first
- [ ] Visual focus indicator works

#### Value Behavior
- [ ] Set value programmatically updates both parts
- [ ] Get value returns correct BallisticCoefficient
- [ ] Changed event fires on text change
- [ ] Changed event fires on table change
- [ ] IsEmpty works correctly

#### Precision Transparency (CRITICAL)
- [ ] Set 0.45678, display shows "0.457"
- [ ] Get value (no edit) returns 0.45678 exactly
- [ ] Edit text, get returns new value
- [ ] Change table (no text edit), get returns original with new table
- [ ] Increment/decrement marks as modified

#### Configuration
- [ ] Increment property works (test with 0.001, 0.01, 0.1)
- [ ] Minimum property enforced
- [ ] Maximum property enforced
- [ ] DecimalPoints affects display (test 2, 3, 4, null)
- [ ] TablePartWidth changes ComboBox width

#### Edge Cases
- [ ] Empty text → Value is null
- [ ] Set Value to null → clears text
- [ ] Very small number (0.001)
- [ ] Very large number (1.999)
- [ ] At minimum boundary
- [ ] At maximum boundary
- [ ] Long precision (0.123456789)

### Mobile Testing (iOS/Android)

#### Touch Targets
- [ ] TextBox height: 44px (iOS) or 48dp (Android)
- [ ] ComboBox height: 44px (iOS) or 48dp (Android)
- [ ] Easy to tap with finger (no mis-taps)

#### Keyboards
- [ ] Tap NumericPart → numeric keyboard appears
- [ ] Can type numbers on mobile keyboard
- [ ] Can type decimal separator on mobile keyboard
- [ ] Can delete with keyboard backspace

#### Validation
- [ ] Invalid input shows inline error TextBlock
- [ ] Error text readable on mobile
- [ ] Error appears below control (not tooltip)
- [ ] Error clears when value becomes valid

#### Navigation
- [ ] Can tap between NumericPart and TablePart
- [ ] ComboBox opens native picker
- [ ] Can select table from picker
- [ ] Focus management works with touch

### Platform-Specific

#### Windows
- [ ] Control height: 32px
- [ ] Standard Windows styling
- [ ] Mouse wheel in ComboBox works

#### macOS
- [ ] Control height: 32px
- [ ] macOS styling applied
- [ ] Native ComboBox behavior

#### Linux
- [ ] Control height: 32px
- [ ] Linux styling applied
- [ ] Works with different window managers

#### iOS
- [ ] Control height: 44px
- [ ] Native iOS styling
- [ ] Numeric keyboard appears
- [ ] Native picker for ComboBox

#### Android
- [ ] Control height: 48dp
- [ ] Material Design styling
- [ ] Numeric keyboard appears
- [ ] Native spinner for ComboBox

---

## Success Criteria

### Implementation
- ✅ Control follows WinForms composite pattern
- ✅ Uses standard TextBox and ComboBox (no custom behaviors)
- ✅ Code-behind handles coordination with Controller
- ✅ All properties configurable via XAML/code

### Functionality
- ✅ Precision transparency working (CRITICAL)
- ✅ Drag tables in correct order
- ✅ Keyboard navigation working
- ✅ Focus management working
- ✅ Validation with inline errors
- ✅ Changed event firing correctly

### Testing
- ✅ 22 controller tests passing
- ✅ 12 control integration tests passing
- ✅ Total: 34 tests passing
- ✅ All manual tests pass

### Mobile Support
- ✅ Platform-adaptive touch targets
- ✅ InputScope for numeric keyboard
- ✅ Inline validation (no tooltips)
- ✅ Usable on iOS and Android

### Code Quality
- ✅ Clean, readable code
- ✅ Following established patterns
- ✅ Well-commented (especially precision transparency)
- ✅ No warnings or errors

---

## Timeline Summary

| Phase | Tasks | Time |
|-------|-------|------|
| **Phase 1** | Review & adjust controller | 1 hour |
| **Phase 2** | Create control XAML + code-behind | 2.5-3.5 hours |
| **Phase 3** | Write integration tests | 1-2 hours |
| **Phase 4** | Add to DebugApp & manual testing | 1 hour |
| **TOTAL** | - | **5.5-7.5 hours** |

**Comparison:**
- Original plan (over-engineered): 18-22 hours
- Simplified plan (WinForms pattern): 5.5-7.5 hours
- **Time saved: ~13-15 hours (70% reduction!)**

---

## Key Design Decisions

### ✅ Following WinForms Pattern

1. **Composite control:** TextBox + ComboBox (standard controls)
2. **Controller:** Pure logic, no UI dependencies (already built!)
3. **Code-behind:** Thin coordination layer
4. **No ViewModel:** Not needed for simple composite
5. **No custom behaviors:** Use standard control features

### ✅ Precision Transparency

**Implementation (WinForms lines 106-137):**
```csharp
private BallisticCoefficient? _originalValue;
private string? _originalText;
private DragTableInfo? _originalTable;

// Get: If text/table unchanged, return original (exact precision)
// Otherwise parse from current text
private BallisticCoefficient? GetValueInternal()
{
    if (_originalValue.HasValue &&
        _originalText == NumericPart.Text &&
        _originalTable?.Value == CurrentTable?.Value)
    {
        return _originalValue; // Exact original!
    }
    return Parse(NumericPart.Text); // User's value
}

// Set: Store original for later retrieval
private void SetValueInternal(BallisticCoefficient? value)
{
    _originalValue = value;
    _originalText = Format(value);
    _originalTable = GetTable(value);
    NumericPart.Text = _originalText;
}
```

### ✅ Mobile Support

1. **Adaptive sizing:** OnPlatform for 32/44/48
2. **Numeric keyboard:** InputScope="Number"
3. **Inline validation:** TextBlock (not tooltip)
4. **Touch-friendly:** No hover required

### ✅ Focus Rules

1. Control focus → NumericPart focus (GotFocus event)
2. Tab from NumericPart → TablePart (KeyDown with Tab)
3. Tab from TablePart → exits (default behavior)

### ✅ Sizing Rules

1. **NumericPart:** Fills space (Grid.Column="*")
2. **TablePart:** Fixed width via `TablePartWidth` property (default 80)
3. **TablePart:** Right-aligned (HorizontalAlignment="Right")
4. **Spacing:** 5px gap between parts

---

## Next Steps After Completion

Once BallisticCoefficientControl is complete:

1. **Verify in DebugApp:** All manual tests passing
2. **Test on multiple platforms:** Windows, macOS, Linux, iOS, Android (if available)
3. **Use as template:** Create MeasurementControl following same pattern
4. **Build input panels:** AmmoPanel, WeaponPanel, etc.
5. **Integrate into main app:** Use in actual ballistic calculator views

---

## Reference Files

### Controller (Already Built)
- Implementation: `Common/BallisticCalculator.Controls/Controllers/BallisticCoefficientController.cs`
- Tests: `Common/BallisticCalculator.Controls.Tests/Controllers/BallisticCoefficientControllerTests.cs`
- Status: ✅ 22/22 tests passing

### Model (Already Built)
- Implementation: `Common/BallisticCalculator.Controls/Models/DragTableInfo.cs`
- Status: ✅ Complete

### To Be Created
- Control XAML: `Common/BallisticCalculator.Controls/Controls/BallisticCoefficientControl.axaml`
- Control Code: `Common/BallisticCalculator.Controls/Controls/BallisticCoefficientControl.axaml.cs`
- Control Tests: `Common/BallisticCalculator.Controls.Tests/UI/BallisticCoefficientControlTests.cs`
- DebugApp Integration: `Desktop/DebugApp/Views/MainWindow.axaml` (add tab)

---

**Ready to implement! Start with Phase 1: Review Controller** ✅
