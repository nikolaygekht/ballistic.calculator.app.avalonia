# MeasurementControl Implementation Plan

## Overview
Create a generic `MeasurementControl` for Avalonia following the same composite pattern as `BallisticCoefficientControl`. This control will handle physical measurements (velocity, weight, distance, pressure, temperature, etc.) using the `Gehtsoft.Measurements` library.

**Pattern**: WinForms `MeasurementControl.cs` adapted to Avalonia
**Estimated Time**: 6-8 hours

---

## Key Requirements

### 1. Generic Design
- Single control implementation supporting ALL measurement types
- Use `MeasurementType` enum to configure which type (Distance, Velocity, Weight, Pressure, Temperature, Angular, Volume)
- Get/Set values using generic methods with type constraints

### 2. API Pattern (from WinForms)
```csharp
// Property to set measurement type
public MeasurementType MeasurementType { get; set; }

// Generic value access
public Measurement<T> ValueAsMeasurement<T>() where T : Enum
public T ValueAs<T>() where T : struct
public T UnitAs<T>() where T : Enum

// Non-generic value access (uses object)
public object Value { get; set; }
public object Unit { get; set; }

// Unit conversion
public void ChangeUnit<T>(T unit, int? accuracy = null) where T : Enum

// Other properties (same as BC control)
public double Increment { get; set; }
public double Minimum { get; set; }
public double Maximum { get; set; }
public int? DecimalPoints { get; set; }
public bool IsEmpty { get; }
public CultureInfo Culture { get; set; }

// Events
public event EventHandler Changed;
```

### 3. Unit Conversion Behavior
- **User changes unit in ComboBox**: No conversion - numeric value stays the same, only unit changes
- **Programmatic ChangeUnit<T>() call**: Converts the value to new unit (e.g., 100 meters → 328.08 feet)
- No precision transparency needed (simpler than BC control)

### 4. Composite Layout
- NumericPart (TextBox) - fills remaining space, right-aligned
- UnitPart (ComboBox) - fixed width (80px default), shows unit abbreviations
- ValidationErrorText (TextBlock) - inline validation below control

---

## Phase 1: Controller Implementation (2-3 hours)

### 1.1 Create `MeasurementController.cs`
**Location**: `Common/BallisticCalculator.Controls/Controllers/MeasurementController.cs`

**Key Members**:
```csharp
public class MeasurementController
{
    // Configuration
    public MeasurementType MeasurementType { get; set; }
    public double Increment { get; set; } = 1;
    public double Minimum { get; set; } = -10000;
    public double Maximum { get; set; } = 10000;
    public int? DecimalPoints { get; set; } = null;

    private MeasurementUtility mUtility;

    // Get available units for current measurement type
    public IReadOnlyList<MeasurementUtility.Unit> GetUnits(out int defaultIndex);

    // Parse text + unit -> Measurement object
    public object Value(string text, MeasurementUtility.Unit unit, int? accuracy, CultureInfo culture);

    // Parse Measurement object -> text + unit
    public void ParseValue(object value, out string text, out MeasurementUtility.Unit unit, int? decimalPoints, CultureInfo culture);

    // Increment/decrement numeric value
    public string IncrementValue(string currentText, bool increment, CultureInfo culture);

    // Validate keyboard input
    public bool AllowKeyInEditor(string currentText, int caretIndex, int selectionLength, char character, CultureInfo culture);

    // Type validation
    public void ValidateUnitType<T>() where T : Enum;
    public void ValidateType<T>() where T : struct;

    // Get specific unit by enum value
    public MeasurementUtility.Unit GetUnit(object unitEnumValue);
}
```

**Default Unit Selection Logic** (from WinForms):
- Distance: Meter
- Angular: Mil
- Weight: Gram
- Pressure: Bar
- Temperature: Celsius
- Velocity: MetersPerSecond
- Volume: Liter

**Number Formatting** (from WinForms lines 71-101):
- Use culture-specific decimal separator
- Support thousands separator for integer part
- Preserve full decimal precision from DecimalPoints

### 1.2 Create Unit Tests
**Location**: `Common/BallisticCalculator.Controls.Tests/Controllers/MeasurementControllerTests.cs`

**Test Coverage** (~15 tests):
1. ✅ GetUnits returns correct units for each MeasurementType
2. ✅ Default unit index is correct for each type
3. ✅ Value parsing with different units (Distance: meters, feet, yards)
4. ✅ Value parsing with different cultures (US vs European decimals)
5. ✅ ParseValue correctly formats text with DecimalPoints
6. ✅ IncrementValue respects Minimum/Maximum
7. ✅ IncrementValue uses Increment step
8. ✅ AllowKeyInEditor accepts valid numeric characters
9. ✅ AllowKeyInEditor rejects invalid characters
10. ✅ AllowKeyInEditor handles culture-specific separators
11. ✅ AllowKeyInEditor handles +/- signs
12. ✅ ValidateUnitType throws for wrong enum type
13. ✅ GetUnit finds correct unit by enum value
14. ✅ Switching MeasurementType updates utility
15. ✅ Null/empty text handling

---

## Phase 2: Control XAML & Code-Behind (2-3 hours)

### 2.1 Create XAML Layout
**Location**: `Common/BallisticCalculator.Controls/Controls/MeasurementControl.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:measurements="using:Gehtsoft.Measurements"
             x:Class="BallisticCalculator.Controls.Controls.MeasurementControl">

  <Grid RowDefinitions="Auto,Auto">
    <!-- Row 0: Main controls -->
    <Grid Grid.Row="0" ColumnDefinitions="*,5,Auto">

      <!-- Numeric TextBox -->
      <TextBox Grid.Column="0"
               x:Name="NumericPart"
               Watermark="0.0"
               HorizontalAlignment="Stretch"
               TextAlignment="Right" />

      <!-- Unit ComboBox -->
      <ComboBox Grid.Column="2"
                x:Name="UnitPart"
                HorizontalAlignment="Right">
        <ComboBox.ItemTemplate>
          <DataTemplate DataType="measurements:MeasurementUtility+Unit">
            <TextBlock Text="{Binding Name}" />
          </DataTemplate>
        </ComboBox.ItemTemplate>
      </ComboBox>
    </Grid>

    <!-- Row 1: Validation error -->
    <TextBlock Grid.Row="1"
               x:Name="ValidationErrorText"
               Foreground="Red"
               FontSize="12"
               Margin="0,2,0,0"
               IsVisible="False" />
  </Grid>
</UserControl>
```

### 2.2 Create Code-Behind
**Location**: `Common/BallisticCalculator.Controls/Controls/MeasurementControl.axaml.cs`

**Styled Properties**:
```csharp
public static readonly StyledProperty<MeasurementType> MeasurementTypeProperty
public static readonly StyledProperty<object?> ValueProperty  // TwoWay binding
public static readonly StyledProperty<double> IncrementProperty
public static readonly StyledProperty<double> MinimumProperty
public static readonly StyledProperty<double> MaximumProperty
public static readonly StyledProperty<int?> DecimalPointsProperty
public static readonly StyledProperty<double> UnitPartWidthProperty  // Default: 80px
public static readonly StyledProperty<CultureInfo> CultureProperty
```

**Key Methods**:
```csharp
// Value property logic (no precision transparency)
private object? GetValueInternal()
{
    if (UnitPart?.SelectedItem is not MeasurementUtility.Unit unit)
        return null;

    // Always parse from current text
    return _controller.Value(NumericPart?.Text, unit, DecimalPoints, Culture);
}

private void SetValueInternal(object? value)
{
    if (NumericPart == null || UnitPart == null)
        return;

    if (value == null)
    {
        NumericPart.Text = "";
        return;
    }

    // Parse value to text and unit
    _controller.ParseValue(value, out string text, out MeasurementUtility.Unit unit, DecimalPoints, Culture);

    // Update UI (DO NOT convert when user changes unit later)
    NumericPart.Text = text;
    UnitPart.SelectedItem = unit;
}

// Generic accessor methods (from WinForms lines 139-156)
public Measurement<T> ValueAsMeasurement<T>() where T : Enum
{
    _controller.ValidateUnitType<T>();
    return (Measurement<T>)Value;
}

public T ValueAs<T>() where T : struct
{
    _controller.ValidateType<T>();
    return (T)Value;
}

public T UnitAs<T>() where T : Enum
{
    _controller.ValidateUnitType<T>();
    var unit = (MeasurementUtility.Unit)UnitPart.SelectedItem;
    return (T)unit.Value;
}

// Unit conversion (from WinForms lines 222-232)
public void ChangeUnit<T>(T unit, int? accuracy = null) where T : Enum
{
    _controller.ValidateUnitType<T>();
    bool empty = IsEmpty;
    var value = ValueAsMeasurement<T>();
    var v = value.In(unit);
    DecimalPoints = accuracy;
    Value = new Measurement<T>(v, unit);
    if (empty)
        NumericPart.Text = "";
}
```

**Event Handlers**:
- NumericPart_TextChanged: Raise Changed event, validate
- NumericPart_KeyDown: Handle Up/Down arrows for increment/decrement
- NumericPart_TextInput: Filter invalid characters
- UnitPart_SelectionChanged: Raise Changed event (NO conversion - user just changes unit label)
- OnPropertyChanged: Handle MeasurementType changes (rebuild unit list)

### 2.3 Update Projects
Add `Gehtsoft.Measurements` package reference to:
- `Common/BallisticCalculator.Controls/BallisticCalculator.Controls.csproj`

---

## Phase 3: Integration Tests (1-2 hours)

**Location**: `Common/BallisticCalculator.Controls.Tests/UI/MeasurementControlTests.cs`

**Test Coverage** (~11 tests):
1. ✅ Control initializes with default MeasurementType (Distance)
2. ✅ Changing MeasurementType updates unit list
3. ✅ Setting Measurement<DistanceUnit> value displays correctly
4. ✅ User changing unit in ComboBox does NOT convert value (100m → 100ft, not 328.08ft)
5. ✅ ValueAsMeasurement<T>() returns correct type
6. ✅ ValueAs<T>() extracts value correctly
7. ✅ UnitAs<T>() returns selected unit enum
8. ✅ ChangeUnit() converts between units correctly (100m → 328.08ft)
9. ✅ Generic type validation throws for mismatched types
10. ✅ UnitPartWidth is configurable
11. ✅ IsEmpty returns correct state

---

## Phase 4: DebugApp Testing (1 hour)

### 4.1 Add Test Tab to MainWindow.axaml
**Location**: `Desktop/DebugApp/Views/MainWindow.axaml`

Add new TabItem: "Measurement Control"

**Test Sections**:
1. **Distance Test**
   - MeasurementType = Distance
   - Set value: 100 meters
   - Get value as Measurement<DistanceUnit>
   - Convert to feet/yards

2. **Velocity Test**
   - MeasurementType = Velocity
   - Set value: 800 m/s
   - Convert to fps (feet per second)

3. **Weight Test**
   - MeasurementType = Weight
   - Set value: 150 grains
   - Convert to grams

4. **User Unit Change Test** (CRITICAL)
   - Set value: 100 meters
   - User selects "Foot" in ComboBox
   - Display should show: 100 (NOT 328.08)
   - Value should be: 100 feet

5. **Programmatic Unit Conversion Test**
   - Set value: 100 meters
   - Call ChangeUnit(DistanceUnit.Foot)
   - Display should show: 328.08
   - Value should be: 328.08 feet

6. **Type Validation Test**
   - Try getting Measurement<VelocityUnit> from Distance control
   - Should throw exception with clear message

7. **Unit Switching Test**
   - Set measurement type at runtime
   - Verify unit list updates

### 4.2 Event Handlers
**Location**: `Desktop/DebugApp/Views/MainWindow.axaml.cs`

```csharp
private void OnSetDistance(object? sender, RoutedEventArgs e)
{
    DistanceControl.Value = new Measurement<DistanceUnit>(100, DistanceUnit.Meter);
}

private void OnGetDistance(object? sender, RoutedEventArgs e)
{
    var value = DistanceControl.ValueAsMeasurement<DistanceUnit>();
    ResultDisplay.Text = $"Value: {value.Value:F6} {value.Unit}";
}

private void OnConvertToFeet(object? sender, RoutedEventArgs e)
{
    DistanceControl.ChangeUnit(DistanceUnit.Foot, 2);
}
```

---

## Implementation Notes

### Key Differences from BC Control
1. **Generic Type System**: Uses `Measurement<T>` generic struct instead of concrete `BallisticCoefficient`
2. **Multiple Measurement Types**: Single control handles 7+ measurement types via `MeasurementType` property
3. **Unit Conversion**: `ChangeUnit<T>()` method converts values between compatible units
4. **Type Validation**: Runtime validation that generic type matches current `MeasurementType`
5. **Sign Support**: Numbers can be negative (unlike BC which is always positive)
6. **Thousands Separator**: Supports culture-specific thousands separator in formatting
7. **NO Precision Transparency**: Simpler - always parses from current text
8. **Unit Change Behavior**: User changing unit in ComboBox does NOT convert - only `ChangeUnit()` converts

### Similarities to BC Control
1. ✅ Composite TextBox + ComboBox layout
2. ✅ Avalonia source generator for internal controls
3. ✅ Controller handles pure logic
4. ✅ Increment/decrement with arrow keys
5. ✅ Culture-aware number parsing/formatting
6. ✅ Inline validation errors

### Number Formatting Complexity
The WinForms controller has special logic (lines 71-101) to:
- Use thousands separator for integer part only
- Use full precision for decimal part
- Handle culture-specific separators

This must be preserved in Avalonia version.

### Keyboard Input Validation
More complex than BC because:
- Supports +/- signs
- Supports thousands separator
- Position matters (e.g., sign only at start)

---

## Manual Testing Checklist

After Phase 4 completion:

- [ ] Distance: Set 100m, programmatically convert to feet using ChangeUnit(), verify 328.08ft
- [ ] Distance: Set 100m, user changes ComboBox to feet, verify shows 100ft (NOT converted)
- [ ] Velocity: Set 800 m/s, convert to fps using ChangeUnit()
- [ ] Weight: Set 150 grains, convert to grams using ChangeUnit()
- [ ] Type safety: Try wrong generic type, verify exception
- [ ] Culture: Switch to European culture (comma decimal separator)
- [ ] Keyboard: Up/Down arrows increment/decrement
- [ ] Keyboard: Type negative number
- [ ] Keyboard: Type with thousands separator
- [ ] Validation: Enter value below Minimum
- [ ] Validation: Enter value above Maximum
- [ ] Unit switching: Change MeasurementType at runtime

---

## Success Criteria

1. All controller tests pass (15 tests)
2. All integration tests pass (11 tests)
3. Manual testing checklist complete
4. User changing unit does NOT convert (100m stays 100 when changed to ft)
5. ChangeUnit() DOES convert (100m becomes 328.08ft)
6. Generic type system works with compile-time safety
7. Unit conversion produces correct values
8. Works on both Windows and Linux
9. Font size changes apply correctly (inherited from app)

**Total Tests**: 26 automated + manual checklist
**Total Time**: 6-8 hours
