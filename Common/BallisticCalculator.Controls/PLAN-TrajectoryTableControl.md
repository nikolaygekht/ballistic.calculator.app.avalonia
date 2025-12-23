# TrajectoryTableControl Implementation Plan

## Goal
Port TrajectoryControl from WinForms to Avalonia as TrajectoryTableControl - a DataGrid-based table showing trajectory data in tabular form. Works alongside TrajectoryChartControl (one shows numbers, the other visualizes).

## Analysis

### Original Implementation (WinForms)
- Uses ListView in VirtualMode for performance
- Columns: Range, Velocity, Mach, Path, Hold, Clicks, Windage, Wnd.Adj., Clicks, Time, Energy, O.G.W.
- Uses MeasurementSystemController for unit conversion
- Supports Sight for calculating clicks
- Supports DropBase (SightLine vs MuzzlePoint)
- Virtual mode retrieves items on-demand (efficient - formats only visible rows)

### Avalonia Approach
- Use DataGrid control (built into Avalonia)
- **On-demand formatting** via TrajectoryPointWrapper (like WinForms VirtualMode)
- Extend MeasurementSystemController with formatting methods
- Simple property pattern (same as TrajectoryChartControl)
- When settings change, refresh DataGrid without recreating wrappers

## File Structure

```
Common/
├── BallisticCalculator.Types/
│   └── (existing types - MeasurementSystem, DropBase, etc.)
│
├── BallisticCalculator.Controls/
│   ├── Controllers/
│   │   └── MeasurementSystemController.cs  # EXTEND with Format methods
│   ├── Models/
│   │   └── TrajectoryPointWrapper.cs       # NEW - lightweight wrapper, formats on access
│   └── Controls/
│       ├── TrajectoryTableControl.axaml
│       └── TrajectoryTableControl.axaml.cs
│
└── BallisticCalculator.Controls.Tests/
    └── Controllers/
        └── MeasurementSystemControllerTests.cs  # EXTEND with formatting tests
```

## Implementation Steps

### Step 1: Extend MeasurementSystemController with Format Methods
Add formatting methods to existing controller:

**Controllers/MeasurementSystemController.cs** (additions)
```csharp
// Add to existing MeasurementSystemController class

// Formatting methods - centralized for reuse by table, tooltips, exports, etc.
public string FormatRange(Measurement<DistanceUnit> distance, CultureInfo? culture = null)
    => distance.To(RangeUnit).ToString(RangeFormatString, culture ?? CultureInfo.CurrentCulture);

public string FormatVelocity(Measurement<VelocityUnit> velocity, CultureInfo? culture = null)
    => velocity.To(VelocityUnit).ToString(VelocityFormatString, culture ?? CultureInfo.CurrentCulture);

public string FormatMach(double mach, CultureInfo? culture = null)
    => mach.ToString(MachFormatString, culture ?? CultureInfo.CurrentCulture);

public string FormatAdjustment(Measurement<DistanceUnit> value, CultureInfo? culture = null)
    => value.To(AdjustmentUnit).ToString(AdjustmentFormatString, culture ?? CultureInfo.CurrentCulture);

public string FormatAngular(Measurement<AngularUnit> value, CultureInfo? culture = null)
    => value.To(AngularUnit).ToString(AngularFormatString, culture ?? CultureInfo.CurrentCulture);

public string FormatEnergy(Measurement<EnergyUnit> energy, CultureInfo? culture = null)
    => energy.To(EnergyUnit).ToString(EnergyFormatString, culture ?? CultureInfo.CurrentCulture);

public string FormatWeight(Measurement<WeightUnit> weight, CultureInfo? culture = null)
    => weight.To(WeightUnit).ToString(WeightFormatString, culture ?? CultureInfo.CurrentCulture);

public string FormatTime(TimeSpan time)
    => time.ToString(TimeFormatString);

public string FormatClicks(Measurement<AngularUnit> adjustment, Measurement<AngularUnit>? clickSize, CultureInfo? culture = null)
{
    if (clickSize == null || clickSize.Value.Value <= 0)
        return "n/a";
    return (adjustment / clickSize.Value).ToString(ClickFormatString, culture ?? CultureInfo.CurrentCulture);
}

// Column headers with units
public string RangeHeader => $"Range ({RangeUnitName})";
public string VelocityHeader => $"Velocity ({VelocityUnitName})";
public string DropHeader => $"Drop ({AdjustmentUnitName})";
public string DropAdjustmentHeader => $"Hold ({AngularUnitName})";
public string WindageHeader => $"Windage ({AdjustmentUnitName})";
public string WindageAdjustmentHeader => $"Wnd.Adj. ({AngularUnitName})";
public string EnergyHeader => $"Energy ({EnergyUnitName})";
public string WeightHeader => $"O.G.W. ({WeightUnitName})";
```

### Step 2: Create TrajectoryPointWrapper
Lightweight wrapper that formats on property access:

**Models/TrajectoryPointWrapper.cs**
```csharp
namespace BallisticCalculator.Controls.Models;

using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

/// <summary>
/// Lightweight wrapper around TrajectoryPoint that formats values on-demand.
/// When settings change, just refresh the DataGrid - no need to recreate wrappers.
/// </summary>
public class TrajectoryPointWrapper
{
    private readonly TrajectoryPoint _point;
    private readonly Func<MeasurementSystemController> _getController;
    private readonly Func<DropBase> _getDropBase;
    private readonly Func<Measurement<AngularUnit>?> _getVerticalClick;
    private readonly Func<Measurement<AngularUnit>?> _getHorizontalClick;

    public TrajectoryPointWrapper(
        TrajectoryPoint point,
        Func<MeasurementSystemController> getController,
        Func<DropBase> getDropBase,
        Func<Measurement<AngularUnit>?> getVerticalClick,
        Func<Measurement<AngularUnit>?> getHorizontalClick)
    {
        _point = point;
        _getController = getController;
        _getDropBase = getDropBase;
        _getVerticalClick = getVerticalClick;
        _getHorizontalClick = getHorizontalClick;
    }

    // Raw point access if needed
    public TrajectoryPoint Point => _point;

    // Formatted properties - format on each access using current settings
    public string Range => _getController().FormatRange(_point.Distance);
    public string Velocity => _getController().FormatVelocity(_point.Velocity);
    public string Mach => _getController().FormatMach(_point.Mach);

    public string Drop => _getController().FormatAdjustment(
        _getDropBase() == DropBase.SightLine ? _point.Drop : _point.DropFlat);

    public string DropAdjustment => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatAngular(_point.DropAdjustment);

    public string DropClicks => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatClicks(_point.DropAdjustment, _getVerticalClick());

    public string Windage => _getController().FormatAdjustment(_point.Windage);

    public string WindageAdjustment => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatAngular(_point.WindageAdjustment);

    public string WindageClicks => _point.Distance.Value < 1e-8
        ? "n/a"
        : _getController().FormatClicks(_point.WindageAdjustment, _getHorizontalClick());

    public string Time => _getController().FormatTime(_point.Time);
    public string Energy => _getController().FormatEnergy(_point.Energy);
    public string OptimalGameWeight => _getController().FormatWeight(_point.OptimalGameWeight);
}
```

### Step 3: Create TrajectoryTableControl
**XAML** (`Controls/TrajectoryTableControl.axaml`):
```xml
<UserControl>
    <DataGrid x:Name="DataGrid"
              AutoGenerateColumns="False"
              IsReadOnly="True"
              CanUserReorderColumns="False"
              CanUserResizeColumns="True"
              CanUserSortColumns="False"
              GridLinesVisibility="All">
        <DataGrid.Columns>
            <DataGridTextColumn x:Name="ColRange" Header="Range" Binding="{Binding Range}" />
            <DataGridTextColumn x:Name="ColVelocity" Header="Velocity" Binding="{Binding Velocity}" />
            <DataGridTextColumn x:Name="ColMach" Header="Mach" Binding="{Binding Mach}" />
            <DataGridTextColumn x:Name="ColDrop" Header="Drop" Binding="{Binding Drop}" />
            <DataGridTextColumn x:Name="ColDropAdj" Header="Hold" Binding="{Binding DropAdjustment}" />
            <DataGridTextColumn x:Name="ColDropClicks" Header="Clicks" Binding="{Binding DropClicks}" />
            <DataGridTextColumn x:Name="ColWindage" Header="Windage" Binding="{Binding Windage}" />
            <DataGridTextColumn x:Name="ColWindageAdj" Header="Wnd.Adj." Binding="{Binding WindageAdjustment}" />
            <DataGridTextColumn x:Name="ColWindageClicks" Header="Clicks" Binding="{Binding WindageClicks}" />
            <DataGridTextColumn x:Name="ColTime" Header="Time" Binding="{Binding Time}" />
            <DataGridTextColumn x:Name="ColEnergy" Header="Energy" Binding="{Binding Energy}" />
            <DataGridTextColumn x:Name="ColOGW" Header="O.G.W." Binding="{Binding OptimalGameWeight}" />
        </DataGrid.Columns>
    </DataGrid>
</UserControl>
```

**Code-behind** (`Controls/TrajectoryTableControl.axaml.cs`):
```csharp
public partial class TrajectoryTableControl : UserControl
{
    private MeasurementSystemController _controller;
    private DropBase _dropBase = DropBase.SightLine;
    private Measurement<AngularUnit>? _verticalClick;
    private Measurement<AngularUnit>? _horizontalClick;
    private List<TrajectoryPointWrapper>? _wrappers;

    public TrajectoryTableControl()
    {
        InitializeComponent();
        _controller = new MeasurementSystemController(MeasurementSystem.Metric);
    }

    // Configuration properties - setters trigger RefreshTable()
    public MeasurementSystem MeasurementSystem
    {
        get => _controller.MeasurementSystem;
        set
        {
            _controller.MeasurementSystem = value;
            UpdateColumnHeaders();
            RefreshTable();
        }
    }

    public AngularUnit AngularUnits
    {
        get => _controller.AngularUnit;
        set
        {
            _controller.AngularUnit = value;
            UpdateColumnHeaders();
            RefreshTable();
        }
    }

    public DropBase DropBase
    {
        get => _dropBase;
        set
        {
            _dropBase = value;
            RefreshTable();
        }
    }

    public Measurement<AngularUnit>? VerticalClick
    {
        get => _verticalClick;
        set
        {
            _verticalClick = value;
            RefreshTable();
        }
    }

    public Measurement<AngularUnit>? HorizontalClick
    {
        get => _horizontalClick;
        set
        {
            _horizontalClick = value;
            RefreshTable();
        }
    }

    // Data management
    public void SetTrajectory(TrajectoryPoint[] trajectory)
    {
        _wrappers = trajectory.Select(p => new TrajectoryPointWrapper(
            p,
            () => _controller,
            () => _dropBase,
            () => _verticalClick,
            () => _horizontalClick
        )).ToList();

        DataGrid.ItemsSource = _wrappers;
        UpdateColumnHeaders();
    }

    public void Clear()
    {
        _wrappers = null;
        DataGrid.ItemsSource = null;
    }

    // Refresh without recreating wrappers
    private void RefreshTable()
    {
        if (_wrappers == null) return;

        // Force DataGrid to re-read all cell values
        var items = DataGrid.ItemsSource;
        DataGrid.ItemsSource = null;
        DataGrid.ItemsSource = items;
    }

    private void UpdateColumnHeaders()
    {
        ColRange.Header = _controller.RangeHeader;
        ColVelocity.Header = _controller.VelocityHeader;
        ColDrop.Header = _controller.DropHeader;
        ColDropAdj.Header = _controller.DropAdjustmentHeader;
        ColWindage.Header = _controller.WindageHeader;
        ColWindageAdj.Header = _controller.WindageAdjustmentHeader;
        ColEnergy.Header = _controller.EnergyHeader;
        ColOGW.Header = _controller.WeightHeader;
    }
}
```

### Step 4: Write Tests
Extend existing MeasurementSystemControllerTests:
- Test FormatRange for Metric/Imperial
- Test FormatVelocity for Metric/Imperial
- Test FormatClicks with valid/null click size
- Test FormatTime
- Test all column headers

### Step 5: Add to DebugApp
Add table below chart in "Trajectory Chart" tab:
- Share same trajectory data
- Share configuration controls

## Design Principles

### On-Demand Formatting (Efficient)
```csharp
// Wrapper formats on each property access - uses current settings
public string Range => _getController().FormatRange(_point.Distance);
```
- No pre-computed strings to regenerate
- When settings change, just refresh DataGrid
- Wrappers stay the same, only formatting changes

### Centralized Formatting in MeasurementSystemController
- All format methods in one place
- Reusable by table, charts, tooltips, exports
- Single source of truth for unit conversion and formatting

### Refresh vs Recreate
```csharp
private void RefreshTable()
{
    // Just refresh - wrappers format on access with current settings
    var items = DataGrid.ItemsSource;
    DataGrid.ItemsSource = null;
    DataGrid.ItemsSource = items;
}
```

## Column Definitions

| Column | Source | Format | Notes |
|--------|--------|--------|-------|
| Range | Distance | N0 | m or yd |
| Velocity | Velocity | N0 | m/s or ft/s |
| Mach | Mach | N2 | dimensionless |
| Drop | Drop or DropFlat | N2 | cm or in, depends on DropBase |
| Hold | DropAdjustment | N2 | moa/mrad/mil, "n/a" at 0 |
| Clicks | DropAdjustment / VerticalClick | N0 | "n/a" if no sight or at 0 |
| Windage | Windage | N2 | cm or in |
| Wnd.Adj. | WindageAdjustment | N2 | moa/mrad/mil, "n/a" at 0 |
| Clicks | WindageAdjustment / HorizontalClick | N0 | "n/a" if no sight or at 0 |
| Time | Time | mm:ss.fff | TimeSpan |
| Energy | Energy | N0 | J or ft-lb |
| O.G.W. | OptimalGameWeight | N1 | kg or lb |

## Test Order
1. MeasurementSystemControllerTests - formatting methods
2. Manual testing in DebugApp - visual verification

## Notes
- Wrappers are lightweight - just hold reference + lambdas
- Formatting happens on property access, not upfront
- Settings changes trigger refresh, not recreation
- Same MeasurementSystemController instance shared across wrappers
