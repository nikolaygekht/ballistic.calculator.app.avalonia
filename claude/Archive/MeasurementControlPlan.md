# Measurement Control Development Plan

## Executive Summary

**Objective:** Create cross-platform Avalonia controls library for measurement input with universal measurement unit support and ballistic coefficient input

**Library:** BallisticCalculatorNew.Controls

**Target Platforms:** Desktop (Windows, macOS, Linux), Mobile (iOS, Android), Web (Browser)

**Core Components:**
1. MeasurementControl - Universal measurement input with unit selection
2. BallisticCoefficientControl - BC input with drag table selection

**Estimated Effort:** 45-55 hours
- Control Development: 30-38 hours
- Testing (UI + Logic): 15-17 hours

---

## Architecture Overview

### Design Principles

1. **Separation of Concerns**
   - UI (Avalonia XAML) - Platform-agnostic visual layer
   - ViewModel - Business logic and state management
   - Controller - Pure logic (reusable across platforms)

2. **Platform Compatibility**
   - Desktop: Full keyboard navigation (Tab, Up/Down arrows)
   - Mobile: Touch-friendly, larger hit targets
   - Web: Browser-compatible input patterns

3. **Reusability**
   - Generic design - single control for all measurement types
   - Data-driven - populate from Gehtsoft.Measurements library
   - Configurable - accuracy, step, validation

### Technology Stack

```xml
<!-- Core Framework -->
<PackageReference Include="Avalonia" Version="11.0.*" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.*" />

<!-- MVVM -->
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.*" />
<PackageReference Include="ReactiveUI" Version="19.*" />

<!-- Business Logic -->
<PackageReference Include="Gehtsoft.Measurements" Version="1.1.16" />
<PackageReference Include="BallisticCalculator" Version="1.1.7.1" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.6.*" />
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="AwesomeAssertions" Version="1.*" />
<PackageReference Include="Avalonia.Headless" Version="11.0.*" /> <!-- For UI testing -->
```

---

## Project Structure

```
BallisticCalculatorNew.Controls/
├── BallisticCalculatorNew.Controls.csproj
├── Controls/
│   ├── MeasurementControl.axaml
│   ├── MeasurementControl.axaml.cs
│   ├── BallisticCoefficientControl.axaml
│   └── BallisticCoefficientControl.axaml.cs
│
├── ViewModels/
│   ├── MeasurementControlViewModel.cs
│   └── BallisticCoefficientControlViewModel.cs
│
├── Controllers/
│   ├── MeasurementControlController.cs      (Pure logic, no UI)
│   └── BallisticCoefficientController.cs
│
├── Models/
│   ├── MeasurementType.cs
│   └── UnitInfo.cs
│
├── Converters/
│   ├── MeasurementConverter.cs
│   └── UnitToStringConverter.cs
│
└── Behaviors/
    ├── NumericInputBehavior.cs
    └── ArrowKeyNavigationBehavior.cs

BallisticCalculatorNew.Controls.Tests/
├── BallisticCalculatorNew.Controls.Tests.csproj
├── Controllers/
│   ├── MeasurementControllerTests.cs
│   └── BallisticCoefficientControllerTests.cs
│
├── ViewModels/
│   ├── MeasurementControlViewModelTests.cs
│   └── BallisticCoefficientControlViewModelTests.cs
│
└── UI/
    ├── MeasurementControlUITests.cs         (Using Avalonia.Headless)
    └── BallisticCoefficientControlUITests.cs

BallisticCalculatorNew.Controls.DebugApp/      (Desktop debug application)
├── BallisticCalculatorNew.Controls.DebugApp.csproj
├── App.axaml
├── App.axaml.cs
├── Program.cs
├── ViewModels/
│   └── MainWindowViewModel.cs
├── Views/
│   ├── MainWindow.axaml
│   └── MainWindow.axaml.cs
└── Assets/
    └── avalonia-logo.ico
```

---

## Phase 1: Foundation & Infrastructure (10-13 hours)

### 1.1 Project Setup (3-4 hours)

**Tasks:**
- [ ] Create solution `BallisticCalculatorNew.Controls.sln`
- [ ] Create `BallisticCalculatorNew.Controls` project (Class Library)
  - Target: net8.0 (cross-platform)
  - Enable Avalonia support
- [ ] Create `BallisticCalculatorNew.Controls.Tests` project (xUnit Test Project)
  - xUnit test framework
  - Configure Avalonia.Headless for UI testing
- [ ] Create `BallisticCalculatorNew.Controls.DebugApp` project (Avalonia Desktop App)
  - Target: net8.0
  - Avalonia.Desktop template
  - Reference Controls project
- [ ] Add NuGet dependencies
  - Gehtsoft.Measurements 1.1.16
  - BallisticCalculator 1.1.7.1
  - Avalonia 11.0.*
  - ReactiveUI
- [ ] Configure project references
  - DebugApp → Controls
  - Tests → Controls
- [ ] Set up build configuration
- [ ] Configure DebugApp as startup project

**Deliverables:**
- ✅ Solution with 3 projects compiles successfully
- ✅ All dependencies resolved
- ✅ Test project can discover tests
- ✅ DebugApp launches with empty window

### 1.2 Core Models (3-4 hours)

**Tasks:**
- [ ] Create `MeasurementType.cs` enum
  ```csharp
  public enum MeasurementType
  {
      Distance,      // DistanceUnit
      Velocity,      // VelocityUnit
      Weight,        // WeightUnit
      Pressure,      // PressureUnit
      Temperature,   // TemperatureUnit
      Angular,       // AngularUnit
      Energy,        // EnergyUnit
      Force,         // ForceUnit
      Area,          // AreaUnit
      Volume,        // VolumeUnit
      Acceleration,  // AccelerationUnit
      Density,       // DensityUnit
      Power,         // PowerUnit
      GasConsumption // GasConsumptionUnit
  }
  ```

- [ ] Create `UnitInfo.cs` model
  ```csharp
  public class UnitInfo
  {
      public object Value { get; init; }      // Enum value
      public string Name { get; init; }       // Display name (e.g., "m")
      public int DefaultAccuracy { get; init; }
  }
  ```

- [ ] Create utility class to extract unit metadata from Gehtsoft.Measurements
  - Use `Measurement<T>.GetUnitNames()` to populate unit lists
  - Use `Measurement<T>.GetUnitDefaultAccuracy()` for precision

**Deliverables:**
- ✅ MeasurementType enum with all types
- ✅ UnitInfo model class
- ✅ Utility methods to extract unit metadata

### 1.3 Base ViewModel Infrastructure (2-2 hours)

**Tasks:**
- [ ] Create `ViewModelBase` class (if not already in shared library)
  ```csharp
  public class ViewModelBase : ReactiveObject
  {
      // Base for all ViewModels
  }
  ```

- [ ] Set up ReactiveUI infrastructure
- [ ] Create base validation support

**Deliverables:**
- ✅ ViewModelBase ready for use
- ✅ ReactiveUI properly configured

### 1.4 Debug Application Setup (3-4 hours)

**Tasks:**
- [ ] Create MainWindow.axaml with demo layout
  ```xml
  <Window xmlns="https://github.com/avaloniaui"
          Title="Controls Debug App"
          Width="800" Height="600">

    <TabControl>
      <!-- Tab 1: MeasurementControl Demo -->
      <TabItem Header="MeasurementControl">
        <StackPanel Margin="20" Spacing="10">
          <TextBlock Text="MeasurementControl Tester"
                     FontSize="20" FontWeight="Bold" />

          <!-- Test all measurement types -->
          <StackPanel Spacing="10">
            <TextBlock Text="Distance:" />
            <controls:MeasurementControl
                x:Name="DistanceControl"
                MeasurementType="Distance" />

            <TextBlock Text="Velocity:" />
            <controls:MeasurementControl
                x:Name="VelocityControl"
                MeasurementType="Velocity" />

            <TextBlock Text="Weight:" />
            <controls:MeasurementControl
                x:Name="WeightControl"
                MeasurementType="Weight" />

            <!-- More types... -->
          </StackPanel>

          <!-- Display current value -->
          <Border Background="LightGray" Padding="10">
            <TextBlock x:Name="ValueDisplay"
                       Text="Value: (none)" />
          </Border>

          <!-- Test buttons -->
          <StackPanel Orientation="Horizontal" Spacing="10">
            <Button Content="Get Value" Click="OnGetValue" />
            <Button Content="Set Test Value" Click="OnSetValue" />
            <Button Content="Clear" Click="OnClear" />
          </StackPanel>
        </StackPanel>
      </TabItem>

      <!-- Tab 2: BallisticCoefficientControl Demo -->
      <TabItem Header="BallisticCoefficient">
        <StackPanel Margin="20" Spacing="10">
          <TextBlock Text="BallisticCoefficientControl Tester"
                     FontSize="20" FontWeight="Bold" />

          <TextBlock Text="Ballistic Coefficient:" />
          <controls:BallisticCoefficientControl
              x:Name="BCControl" />

          <Border Background="LightGray" Padding="10">
            <TextBlock x:Name="BCValueDisplay"
                       Text="Value: (none)" />
          </Border>

          <StackPanel Orientation="Horizontal" Spacing="10">
            <Button Content="Get BC" Click="OnGetBC" />
            <Button Content="Set Test BC" Click="OnSetBC" />
          </StackPanel>
        </StackPanel>
      </TabItem>

      <!-- Tab 3: Future Panels -->
      <TabItem Header="Input Panels">
        <TextBlock Text="Future input panels will be tested here"
                   Margin="20" />
      </TabItem>
    </TabControl>
  </Window>
  ```

- [ ] Implement MainWindow.axaml.cs with event handlers
  ```csharp
  public partial class MainWindow : Window
  {
      public MainWindow()
      {
          InitializeComponent();
      }

      private void OnGetValue(object sender, RoutedEventArgs e)
      {
          var value = DistanceControl.Value;
          ValueDisplay.Text = $"Value: {value}";
      }

      private void OnSetValue(object sender, RoutedEventArgs e)
      {
          DistanceControl.Value = new Measurement<DistanceUnit>(100, DistanceUnit.Meter);
      }

      // More handlers...
  }
  ```

- [ ] Configure App.axaml and Program.cs
- [ ] Test application launches and displays tabs

**Purpose:**
- Interactive testing during development
- Visual verification of controls
- Quick iteration without writing tests first
- Demo application for documentation

**Deliverables:**
- ✅ Debug app launches successfully
- ✅ Tabs render correctly
- ✅ Ready to add controls as they're developed
- ✅ Can be used to manually verify features

---

## Phase 2: MeasurementControl Development (18-22 hours)

### 2.1 MeasurementControlController (Pure Logic) (6-8 hours)

**Reference:** `/mnt/d/develop/homeapps/BallisticCalculator1/BallisticCalculatorNet.MeasurementControl/MeasurmentControlController.cs`

**Tasks:**
- [ ] Create `MeasurementControlController.cs`
  - Pure C# logic, no UI dependencies
  - 100% testable

**Properties:**
```csharp
public class MeasurementControlController
{
    public MeasurementType MeasurementType { get; set; }
    public double Increment { get; set; } = 1.0;
    public double Minimum { get; set; } = -10000;
    public double Maximum { get; set; } = 10000;
    public int? DecimalPoints { get; set; } = null;

    // Get available units for current measurement type
    public IReadOnlyList<UnitInfo> GetUnits(out int defaultIndex);

    // Parse text + unit to create measurement value
    public object Value(string text, object unit, int? decimalPoints);

    // Parse measurement value to text + unit
    public void ParseValue(object value, out string text, out UnitInfo unit);

    // Validate if key is allowed in numeric input
    public bool AllowKeyInEditor(string currentText, int position, int length, char key);

    // Increment/decrement value by step
    public string IncrementValue(string currentText, object unit, bool increment);

    // Validate type compatibility
    public void ValidateUnitType<T>() where T : Enum;
    public void ValidateType<T>() where T : struct;
}
```

**Key Features:**
- Generic measurement type support using `Measurement<T>`
- Culture-aware parsing (use `CultureInfo.InvariantCulture` for storage)
- Validation logic for min/max/increment
- Keyboard input filtering

**Tests:**
- [ ] Unit selection for each MeasurementType
- [ ] Value parsing and formatting
- [ ] Increment/decrement logic
- [ ] Validation (min/max, type checking)
- [ ] Culture-specific number parsing
- [ ] Keyboard input filtering

**Deliverables:**
- ✅ Controller with pure business logic
- ✅ 100% test coverage for controller
- ✅ All measurement types supported

### 2.2 MeasurementControlViewModel (8-10 hours)

**Tasks:**
- [ ] Create `MeasurementControlViewModel.cs`

**Properties:**
```csharp
public class MeasurementControlViewModel : ViewModelBase
{
    private readonly MeasurementControlController _controller;

    // Observable properties
    [Reactive] public string NumericText { get; set; }
    [Reactive] public UnitInfo SelectedUnit { get; set; }
    [Reactive] public ObservableCollection<UnitInfo> AvailableUnits { get; set; }

    // Configuration
    [Reactive] public MeasurementType MeasurementType { get; set; }
    [Reactive] public double Increment { get; set; }
    [Reactive] public double Minimum { get; set; }
    [Reactive] public double Maximum { get; set; }
    [Reactive] public int? DecimalPoints { get; set; }

    // Computed value
    public object Value { get; set; }

    // Commands
    public ReactiveCommand<Unit, Unit> IncrementCommand { get; }
    public ReactiveCommand<Unit, Unit> DecrementCommand { get; }

    // Validation
    public bool IsValid { get; }
    public string ValidationError { get; }
}
```

**Key Reactive Behaviors:**
- `MeasurementType` change → Update `AvailableUnits`
- `NumericText` or `SelectedUnit` change → Update `Value`
- `Value` property set → Update `NumericText` and `SelectedUnit`
- Increment/Decrement commands update `NumericText`

**Tests:**
- [ ] MeasurementType switching updates units
- [ ] Value property round-trip (set → get)
- [ ] Increment/Decrement commands work
- [ ] Validation triggers on invalid input
- [ ] Reactive updates work correctly
- [ ] Min/Max enforcement

**Deliverables:**
- ✅ ViewModel with reactive properties
- ✅ Commands for increment/decrement
- ✅ Validation support
- ✅ >90% test coverage

### 2.3 MeasurementControl UI (Avalonia XAML) (4-4 hours)

**Reference:** `/mnt/d/develop/homeapps/BallisticCalculator1/BallisticCalculatorNet.MeasurementControl/MeasurementControl.Designer.cs` (for layout)

**Tasks:**
- [ ] Create `MeasurementControl.axaml`

**Layout:**
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:BallisticCalculatorNew.Controls.ViewModels"
             x:Class="BallisticCalculatorNew.Controls.MeasurementControl">

  <Design.DataContext>
    <vm:MeasurementControlViewModel />
  </Design.DataContext>

  <Grid ColumnDefinitions="*,5,Auto">
    <!-- Numeric input part -->
    <TextBox Grid.Column="0"
             x:Name="NumericPart"
             Text="{Binding NumericText}"
             Watermark="0.0"
             HorizontalAlignment="Stretch">
      <!-- Behaviors for keyboard support -->
      <i:Interaction.Behaviors>
        <behaviors:NumericInputBehavior />
        <behaviors:ArrowKeyNavigationBehavior
            IncrementCommand="{Binding IncrementCommand}"
            DecrementCommand="{Binding DecrementCommand}" />
      </i:Interaction.Behaviors>
    </TextBox>

    <!-- Unit selection part -->
    <ComboBox Grid.Column="2"
              x:Name="UnitPart"
              ItemsSource="{Binding AvailableUnits}"
              SelectedItem="{Binding SelectedUnit}"
              MinWidth="80">
      <ComboBox.ItemTemplate>
        <DataTemplate>
          <TextBlock Text="{Binding Name}" />
        </DataTemplate>
      </ComboBox.ItemTemplate>
    </ComboBox>
  </Grid>

</UserControl>
```

**Code-Behind:**
```csharp
public partial class MeasurementControl : UserControl
{
    public static readonly StyledProperty<object> ValueProperty =
        AvaloniaProperty.Register<MeasurementControl, object>(nameof(Value));

    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<MeasurementType> MeasurementTypeProperty =
        AvaloniaProperty.Register<MeasurementControl, MeasurementType>(
            nameof(MeasurementType), MeasurementType.Distance);

    public MeasurementType MeasurementType
    {
        get => GetValue(MeasurementTypeProperty);
        set => SetValue(MeasurementTypeProperty, value);
    }

    // Additional properties: Increment, Minimum, Maximum, DecimalPoints

    public MeasurementControl()
    {
        InitializeComponent();
        DataContext = new MeasurementControlViewModel();
    }
}
```

**Behaviors to Implement:**
- [ ] `NumericInputBehavior` - Filter non-numeric keys
- [ ] `ArrowKeyNavigationBehavior` - Up/Down increment/decrement

**Keyboard Navigation:**
- Tab → Focus numeric input first
- Up/Down arrows → Increment/decrement value when numeric input focused
- Tab from numeric input → Focus unit dropdown
- Up/Down arrows → Select unit when dropdown focused
- Tab from dropdown → Exit control

**Tests (UI):**
- [ ] Control renders correctly
- [ ] Numeric input accepts only valid characters
- [ ] Unit dropdown populated correctly
- [ ] Tab navigation works (numeric → unit)
- [ ] Up/Down arrows increment/decrement
- [ ] Value property binding works

**Deliverables:**
- ✅ Functional MeasurementControl
- ✅ Full keyboard navigation support
- ✅ Platform-agnostic (works desktop/mobile/web)
- ✅ UI tests passing

---

## Phase 3: BallisticCoefficientControl Development (10-12 hours)

### 3.1 BallisticCoefficientController (3-4 hours)

**Reference:**
- `/mnt/d/develop/components/BusinessSpecificComponents/BallisticCalculator.Net/BallisticCalculator/Drag/BallisticCoefficient.cs`
- `/mnt/d/develop/components/BusinessSpecificComponents/BallisticCalculator.Net/BallisticCalculator/Drag/DragTableId.cs`

**Tasks:**
- [ ] Create `BallisticCoefficientController.cs`

**Properties:**
```csharp
public class BallisticCoefficientController
{
    public double Increment { get; set; } = 0.001;
    public double Minimum { get; set; } = 0.001;
    public double Maximum { get; set; } = 2.0;
    public int? DecimalPoints { get; set; } = 3;

    // Get available drag tables
    public IReadOnlyList<DragTableInfo> GetDragTables(out int defaultIndex);

    // Parse text + table to create BallisticCoefficient
    public BallisticCoefficient Value(string text, DragTableId table, int? decimalPoints);

    // Parse BallisticCoefficient to text + table
    public void ParseValue(BallisticCoefficient value, out string text, out DragTableInfo table);

    // Increment/decrement coefficient
    public string IncrementValue(string currentText, DragTableId table, bool increment);

    // Validate numeric input
    public bool AllowKeyInEditor(string currentText, int position, int length, char key);
}

public class DragTableInfo
{
    public DragTableId Value { get; init; }
    public string Name { get; init; }      // "G1", "G7", etc.
    public string Description { get; init; } // From DragTableId enum comments
}
```

**Drag Table Extraction:**
```csharp
private static DragTableInfo[] GetDragTableInfo()
{
    return Enum.GetValues<DragTableId>()
        .Select(id => new DragTableInfo
        {
            Value = id,
            Name = id.ToString(),
            Description = GetEnumDescription(id) // Extract from [Description] attribute
        })
        .ToArray();
}
```

**Tests:**
- [ ] Drag table list population
- [ ] BC value parsing and formatting
- [ ] Increment/decrement logic
- [ ] Min/max validation
- [ ] Numeric input filtering

**Deliverables:**
- ✅ Controller with BC-specific logic
- ✅ 100% test coverage

### 3.2 BallisticCoefficientControlViewModel (4-5 hours)

**Tasks:**
- [ ] Create `BallisticCoefficientControlViewModel.cs`

**Properties:**
```csharp
public class BallisticCoefficientControlViewModel : ViewModelBase
{
    private readonly BallisticCoefficientController _controller;

    // Observable properties
    [Reactive] public string NumericText { get; set; }
    [Reactive] public DragTableInfo SelectedTable { get; set; }
    [Reactive] public ObservableCollection<DragTableInfo> AvailableTables { get; set; }

    // Configuration
    [Reactive] public double Increment { get; set; }
    [Reactive] public double Minimum { get; set; }
    [Reactive] public double Maximum { get; set; }
    [Reactive] public int? DecimalPoints { get; set; }

    // Computed value
    public BallisticCoefficient Value { get; set; }

    // Commands
    public ReactiveCommand<Unit, Unit> IncrementCommand { get; }
    public ReactiveCommand<Unit, Unit> DecrementCommand { get; }

    // Validation
    public bool IsValid { get; }
    public string ValidationError { get; }
}
```

**Tests:**
- [ ] Value property round-trip
- [ ] Increment/Decrement commands
- [ ] Validation
- [ ] Reactive updates

**Deliverables:**
- ✅ ViewModel with reactive properties
- ✅ >90% test coverage

### 3.3 BallisticCoefficientControl UI (3-3 hours)

**Tasks:**
- [ ] Create `BallisticCoefficientControl.axaml`

**Layout:** (Very similar to MeasurementControl)
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="BallisticCalculatorNew.Controls.BallisticCoefficientControl">

  <Grid ColumnDefinitions="*,5,Auto">
    <!-- Coefficient input -->
    <TextBox Grid.Column="0"
             x:Name="CoefficientPart"
             Text="{Binding NumericText}"
             Watermark="0.000">
      <i:Interaction.Behaviors>
        <behaviors:NumericInputBehavior AllowDecimal="True" />
        <behaviors:ArrowKeyNavigationBehavior />
      </i:Interaction.Behaviors>
    </TextBox>

    <!-- Drag table selection -->
    <ComboBox Grid.Column="2"
              x:Name="DragTablePart"
              ItemsSource="{Binding AvailableTables}"
              SelectedItem="{Binding SelectedTable}"
              MinWidth="80">
      <ComboBox.ItemTemplate>
        <DataTemplate>
          <StackPanel>
            <TextBlock Text="{Binding Name}" FontWeight="Bold" />
            <TextBlock Text="{Binding Description}"
                       FontSize="10"
                       Opacity="0.7" />
          </StackPanel>
        </DataTemplate>
      </ComboBox.ItemTemplate>
    </ComboBox>
  </Grid>

</UserControl>
```

**Tests (UI):**
- [ ] Control renders
- [ ] Drag table dropdown populated
- [ ] Keyboard navigation works
- [ ] Value binding works

**Deliverables:**
- ✅ Functional BallisticCoefficientControl
- ✅ UI tests passing

---

## Phase 4: Advanced Features & Polish (4-5 hours)

### 4.1 Adaptive Layouts for Mobile (2-2 hours)

**Tasks:**
- [ ] Increase touch target size on mobile
- [ ] Adjust spacing for mobile
- [ ] Test on touch devices

**Adaptive Styling:**
```xml
<Style Selector="MeasurementControl">
  <Setter Property="MinHeight" Value="32" />
</Style>

<Style Selector="MeasurementControl[IsTouch=True]">
  <Setter Property="MinHeight" Value="44" />
  <!-- Larger for touch -->
</Style>
```

### 4.2 Validation Visual Feedback (1-2 hours)

**Tasks:**
- [ ] Add validation border color (red for invalid)
- [ ] Add tooltip with validation message
- [ ] Visual indicator for required fields

**Example:**
```xml
<TextBox Classes.error="{Binding !IsValid}">
  <TextBox.Styles>
    <Style Selector="TextBox.error">
      <Setter Property="BorderBrush" Value="Red" />
    </Style>
  </TextBox.Styles>
</TextBox>

<ToolTip.Tip>
  <TextBlock Text="{Binding ValidationError}"
             IsVisible="{Binding !IsValid}" />
</ToolTip.Tip>
```

### 4.3 Documentation (1-1 hour)

**Tasks:**
- [ ] XML documentation comments for public APIs
- [ ] Usage examples in README
- [ ] Sample application demonstrating controls

---

## Phase 5: Testing (15-17 hours)

### 5.1 Controller Unit Tests (6-7 hours)

**MeasurementControlController Tests:**
- [ ] GetUnits for all MeasurementTypes
- [ ] Value parsing (valid and invalid inputs)
- [ ] ParseValue for all measurement types
- [ ] Increment/Decrement logic
- [ ] Min/Max validation
- [ ] Keyboard input filtering
- [ ] Culture-specific parsing
- [ ] Edge cases (null, empty, overflow)

**BallisticCoefficientController Tests:**
- [ ] GetDragTables
- [ ] Value parsing
- [ ] Increment/Decrement
- [ ] Validation
- [ ] Edge cases

**Target:** >95% code coverage for controllers

### 5.2 ViewModel Unit Tests (5-6 hours)

**MeasurementControlViewModel Tests:**
- [ ] MeasurementType change updates AvailableUnits
- [ ] Value property round-trip for each type
- [ ] IncrementCommand increases value
- [ ] DecrementCommand decreases value
- [ ] Validation triggers correctly
- [ ] Reactive property updates
- [ ] Min/Max enforcement

**BallisticCoefficientControlViewModel Tests:**
- [ ] Value property round-trip
- [ ] Commands work
- [ ] Validation
- [ ] Reactive updates

**Target:** >90% code coverage for ViewModels

### 5.3 UI Integration Tests (4-4 hours)

**Using Avalonia.Headless:**

**MeasurementControl UI Tests:**
- [ ] Control renders with default values
- [ ] Numeric TextBox accepts input
- [ ] Unit ComboBox populated
- [ ] Tab navigation: NumericPart → UnitPart → Next control
- [ ] Up arrow increments value
- [ ] Down arrow decrements value
- [ ] Value binding updates UI
- [ ] UI updates binding

**BallisticCoefficientControl UI Tests:**
- [ ] Control renders
- [ ] Drag table dropdown works
- [ ] Keyboard navigation
- [ ] Value binding

**Example Test Structure:**
```csharp
[Fact]
public void MeasurementControl_Should_Render_With_Default_Values()
{
    using var app = AvaloniaApp.GetApp();
    using var window = new Window();

    var control = new MeasurementControl
    {
        MeasurementType = MeasurementType.Distance
    };

    window.Content = control;
    window.Show();

    // Find controls
    var numericPart = control.FindControl<TextBox>("NumericPart");
    var unitPart = control.FindControl<ComboBox>("UnitPart");

    numericPart.Should().NotBeNull();
    unitPart.Should().NotBeNull();
    unitPart.Items.Should().NotBeEmpty();
}

[Fact]
public void MeasurementControl_Should_Increment_On_UpArrow()
{
    // Test keyboard navigation
}
```

---

## Testing Strategy

### Test Pyramid

```
       /\
      /UI\         4-4 hours (Integration tests)
     /────\
    /ViewModel\    5-6 hours (ViewModel tests)
   /──────────\
  /Controllers \   6-7 hours (Pure logic tests)
 /──────────────\
```

### Test Categories

1. **Unit Tests (Controllers)** - Fast, isolated, 100% coverage
2. **ViewModel Tests** - Reactive behavior, commands, validation
3. **UI Tests** - Headless UI tests, keyboard navigation, binding

### Test Tools

- **xUnit** - Test framework
- **Moq** - Mocking dependencies
- **AwesomeAssertions** - Readable assertions
- **Avalonia.Headless** - UI testing without GUI

### Test Execution

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific category
dotnet test --filter "Category=Controller"
```

---

## Definition of Done

### MeasurementControl
- ✅ Supports all 14 measurement types from Gehtsoft.Measurements
- ✅ Full keyboard navigation (Tab, Up/Down arrows)
- ✅ Configurable increment, min/max, accuracy
- ✅ Culture-aware number parsing
- ✅ Validation with visual feedback
- ✅ Cross-platform (desktop/mobile/web)
- ✅ >90% test coverage (controller + ViewModel)
- ✅ UI tests passing
- ✅ XML documentation complete

### BallisticCoefficientControl
- ✅ Supports all drag tables from BallisticCalculator library
- ✅ Full keyboard navigation
- ✅ Configurable increment, min/max, accuracy
- ✅ Validation with visual feedback
- ✅ Cross-platform
- ✅ >90% test coverage
- ✅ UI tests passing
- ✅ XML documentation complete

### Library
- ✅ NuGet package ready
- ✅ Debug application demonstrating both controls (and future panels)
- ✅ README with usage examples
- ✅ All tests passing (100% success rate)
- ✅ No compiler warnings
- ✅ Code style consistent

### Debug Application
- ✅ Launches successfully on all platforms
- ✅ All controls visible and functional
- ✅ Interactive testing for all measurement types
- ✅ Can be used for manual regression testing
- ✅ Serves as live documentation

---

## Timeline Summary

| Phase | Duration | Hours | Cumulative |
|-------|----------|-------|------------|
| 1. Foundation (inc. DebugApp) | 1.5-2 weeks | 10-13 | 10-13 |
| 2. MeasurementControl | 2-3 weeks | 18-22 | 28-35 |
| 3. BallisticCoefficientControl | 1.5-2 weeks | 10-12 | 38-47 |
| 4. Advanced Features | 0.5-1 week | 4-5 | 42-52 |
| 5. Testing | 2-2.5 weeks | 15-17 | 57-69 |
| **TOTAL** | **7.5-10.5 weeks** | **57-69 hours** | - |

**Breakdown:**
- Foundation (with DebugApp): 10-13 hours
- Pure development: 32-39 hours
- Testing: 15-17 hours
- **Total: 57-69 hours**

**Timeline with Different Work Schedules:**
- 10 hours/week: 6-7 weeks
- 20 hours/week: 3-4 weeks
- 30 hours/week: 2-3 weeks
- 40 hours/week: 1.5-2 weeks

**Note:** The Debug Application is integrated into Phase 1 and will be continuously used throughout development for manual verification and testing of controls and panels.

---

## Success Criteria

### Functional Requirements
✅ Universal measurement control supporting all types
✅ Ballistic coefficient control with drag tables
✅ Full keyboard navigation (desktop mode)
✅ Touch-friendly (mobile mode)
✅ Configurable precision, step, validation
✅ Culture-aware number parsing

### Quality Requirements
✅ >90% test coverage
✅ All UI tests passing
✅ Cross-platform verified (Windows, Linux, macOS)
✅ No memory leaks
✅ Performance: <16ms render time

### Documentation Requirements
✅ XML documentation for all public APIs
✅ README with usage examples
✅ Sample application
✅ Architecture documentation

---

## Risk Mitigation

### Risk 1: Avalonia.Headless UI Testing 🟡 MEDIUM
**Issue:** Headless UI testing may have limitations
**Mitigation:**
- Prototype headless tests early (Phase 1)
- Focus on ViewModel tests (easier to test)
- Manual UI verification on real platforms

### Risk 2: Keyboard Navigation Cross-Platform 🟡 MEDIUM
**Issue:** Arrow key behaviors may differ across platforms
**Mitigation:**
- Test on all platforms (Windows, macOS, Linux)
- Use platform-agnostic Avalonia behaviors
- Fallback to standard behavior if issues

### Risk 3: Mobile Touch Support 🟢 LOW
**Issue:** Controls designed for desktop may not be touch-friendly
**Mitigation:**
- Adaptive styling based on input type
- Test on actual mobile devices
- Increase hit targets for mobile

---

## Reference Materials

### WinForms Reference
- `/mnt/d/develop/homeapps/BallisticCalculator1/BallisticCalculatorNet.MeasurementControl/`
  - `MeasurementControl.cs` - UI behavior reference
  - `MeasurmentControlController.cs` - Business logic reference (excellent separation!)
  - `MeasurementType.cs` - Enum structure

- `/mnt/d/develop/homeapps/BallisticCalculator1/BallisticCalculatorNet.UnitTest/MsrmentControl/`
  - `ControlTest.cs` - Test patterns reference
  - `MeasurementUtilTest.cs` - Utility test patterns

### External Libraries
- **Gehtsoft.Measurements**
  - Location: `/mnt/d/develop/components/BusinessSpecificComponents/Gehtsoft.Measurements/`
  - Key type: `Measurement<T>` struct
  - Static methods: `GetUnitNames()`, `GetUnitDefaultAccuracy()`

- **BallisticCalculator**
  - Location: `/mnt/d/develop/components/BusinessSpecificComponents/BallisticCalculator.Net/`
  - Key types: `BallisticCoefficient`, `DragTableId`

### Documentation
- Avalonia UI: https://docs.avaloniaui.net/
- ReactiveUI: https://www.reactiveui.net/docs/
- Avalonia.Headless: https://github.com/AvaloniaUI/Avalonia/tree/master/tests/Avalonia.Headless.XUnit

---

## Next Steps

1. **Review and Approve Plan** - Get feedback on approach
2. **Set Up Projects** - Create solution structure (Phase 1.1)
3. **Prototype Key Features** - Test Avalonia.Headless early
4. **Implement Phase by Phase** - Follow plan sequentially
5. **Continuous Testing** - Write tests as you develop

---

**Document Version:** 1.0
**Date:** 2025-01-15
**Status:** Ready for Review
