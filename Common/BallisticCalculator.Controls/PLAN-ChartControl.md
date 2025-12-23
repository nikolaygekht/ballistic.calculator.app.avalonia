# ChartControl Implementation Plan

## Goal
Port ChartControl and MultiChartControl from WinForms to Avalonia using ScottPlot.Avalonia, unified into a single control.

## Analysis

### Original Implementation
- **ChartControl**: Displays single trajectory with various chart modes (Drop, Velocity, Energy, etc.)
- **MultiChartControl**: Displays multiple trajectories overlaid for comparison
- **ChartController**: Pure logic - extracts X/Y data from TrajectoryPoint[] based on chart mode
- **MeasurementSystemController**: Unit conversion helper (Metric/Imperial)

### Unification Decision
**Merge into single `TrajectoryChartControl`** because:
1. Both controls share 95% identical code
2. Single trajectory is just a special case of multiple trajectories (list with 1 item)
3. Simpler API: `AddTrajectory()` / `ClearTrajectories()` works for both use cases
4. Less code to maintain

## File Structure

```
Common/
├── BallisticCalculator.Types/              # Pure types, no UI dependencies
│   ├── MeasurementSystem.cs                # Enum: Metric, Imperial
│   ├── TrajectoryChartMode.cs              # Enum: Velocity, Mach, Drop, etc.
│   ├── DropBase.cs                         # Enum: SightLine, MuzzlePoint
│   └── ChartTrajectory.cs                  # record(Name, Points)
│
├── BallisticCalculator.Controls/           # UI controls, refs Types
│   ├── Controllers/
│   │   ├── MeasurementSystemController.cs  # Unit conversion helper (reusable)
│   │   └── ChartController.cs              # Chart data extraction logic
│   └── Controls/
│       ├── TrajectoryChartControl.axaml
│       └── TrajectoryChartControl.axaml.cs
│
└── BallisticCalculator.Controls.Tests/
    └── Controllers/
        ├── MeasurementSystemControllerTests.cs
        └── ChartControllerTests.cs
```

## Implementation Steps

### Step 1: Add ScottPlot.Avalonia Package
Add to `BallisticCalculator.Controls.csproj`:
```xml
<PackageReference Include="ScottPlot.Avalonia" Version="[5.0.56,5.1)" />
```

### Step 2: Create Types in BallisticCalculator.Types

**MeasurementSystem.cs**
```csharp
namespace BallisticCalculator.Types;

public enum MeasurementSystem
{
    Metric,
    Imperial
}
```

**TrajectoryChartMode.cs**
```csharp
namespace BallisticCalculator.Types;

public enum TrajectoryChartMode
{
    Velocity,
    Mach,
    Drop,
    DropAdjustment,
    Windage,
    WindageAdjustment,
    Energy
}
```

**DropBase.cs**
```csharp
namespace BallisticCalculator.Types;

public enum DropBase
{
    SightLine,
    MuzzlePoint
}
```

**ChartTrajectory.cs**
```csharp
namespace BallisticCalculator.Types;

using BallisticCalculator;

public record ChartTrajectory(string Name, TrajectoryPoint[] Points);
```

### Step 3: Port MeasurementSystemController
Port to `Controllers/MeasurementSystemController.cs`:
- Properties: MeasurementSystem, AngularUnit
- Unit getters: RangeUnit, AdjustmentUnit, VelocityUnit, EnergyUnit, WeightUnit
- Unit names: RangeUnitName, AdjustmentUnitName, etc.
- Format strings: RangeFormatString, etc.

This will be reused by trajectory tables and other views.

### Step 4: Port ChartController
Port to `Controllers/ChartController.cs`:
- Constructor: (MeasurementSystem, AngularUnit, ChartMode, DropBase, TrajectoryPoint[])
- Properties: XAxisTitle, YAxisTitle, SeriesCount
- Methods: GetXAxis(), GetYAxis(), GetSeriesTitle(index)

### Step 5: Create TrajectoryChartControl
**XAML** (`Controls/TrajectoryChartControl.axaml`):
```xml
<UserControl xmlns:ScottPlot="clr-namespace:ScottPlot.Avalonia;assembly=ScottPlot.Avalonia">
    <ScottPlot:AvaPlot x:Name="AvaPlot" />
</UserControl>
```

**Code-behind** (`Controls/TrajectoryChartControl.axaml.cs`):
```csharp
public partial class TrajectoryChartControl : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;
    private AngularUnit _angularUnits = AngularUnit.MOA;
    private TrajectoryChartMode _chartMode = TrajectoryChartMode.Drop;
    private DropBase _dropBase = DropBase.SightLine;
    private readonly List<ChartTrajectory> _trajectories = new();

    // Configuration properties - setters trigger UpdateChart()
    public MeasurementSystem MeasurementSystem { get; set; }
    public AngularUnit AngularUnits { get; set; }
    public TrajectoryChartMode ChartMode { get; set; }
    public DropBase DropBase { get; set; }

    // Data management
    public void SetTrajectory(TrajectoryPoint[] points);      // Single trajectory (convenience)
    public void AddTrajectory(string name, TrajectoryPoint[] points);
    public void RemoveLastTrajectory();
    public void ClearTrajectories();

    // Internal
    private void UpdateChart();
    public void UpdateYAxis();  // Adjust Y to visible X range after zoom
}
```

### Step 6: Write Tests

**Controller Tests** (`BallisticCalculator.Controls.Tests/Controllers/`):
- `MeasurementSystemControllerTests.cs`
  - Test unit selection for Metric/Imperial
  - Test unit names
  - Test format strings

- `ChartControllerTests.cs`
  - Test axis titles for each chart mode
  - Test data extraction for each mode
  - Test SeriesCount (2 for Drop+MuzzlePoint, 1 otherwise)
  - Use sample TrajectoryPoint[] data

**No UI tests needed** - display-only control with no user interaction logic

### Step 7: Add to DebugApp
Create test page for visual verification:
- Chart mode selector (ComboBox)
- Measurement system toggle
- Load sample trajectory button
- Add second trajectory button (for multi-chart testing)
- Clear button

## Design Principles

### Simple Property Pattern (No Reactive)
```csharp
public TrajectoryChartMode ChartMode
{
    get => _chartMode;
    set
    {
        _chartMode = value;
        UpdateChart();  // Immediate redraw
    }
}
```

### ScottPlot 5.x API
```csharp
private void UpdateChart()
{
    AvaPlot.Plot.Clear();

    foreach (var trajectory in _trajectories)
    {
        var controller = new ChartController(..., trajectory.Points);
        var x = controller.GetXAxis();
        var ySeries = controller.GetYAxis();

        for (int i = 0; i < ySeries.Count; i++)
        {
            var scatter = AvaPlot.Plot.Add.Scatter(x, ySeries[i]);
            scatter.LegendText = GetLegendText(trajectory, controller, i);
        }
    }

    AvaPlot.Plot.Axes.Bottom.Label.Text = controller.XAxisTitle;
    AvaPlot.Plot.Axes.Left.Label.Text = controller.YAxisTitle;
    AvaPlot.Plot.Axes.AutoScale();
    AvaPlot.Refresh();
}
```

## Dependencies
- ScottPlot.Avalonia [5.0.56,5.1) (NuGet) - add to Controls. Version 5.1+ requires SkiaSharp 3.x, we use 2.88
- BallisticCalculator (already referenced - provides TrajectoryPoint)
- Gehtsoft.Measurements (already referenced - provides units)

## Test Order
1. MeasurementSystemControllerTests - unit conversion logic
2. ChartControllerTests - data extraction logic
3. Manual testing in DebugApp - visual verification

## Notes
- ScottPlot 5.x API differs from 4.x (used in old WinForms app)
- Key differences: `Plot.Add.Scatter()` instead of `Plot.AddScatter()`, different axis access
- Check https://scottplot.net/cookbook/5.0/ for current API
