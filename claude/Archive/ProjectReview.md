# Ballistic Calculator Project Review

## Executive Summary

The Ballistic Calculator is a well-structured WinForms application (~22,000 lines of code, 182 files, 11 projects) for calculating and visualizing bullet trajectories. The codebase is modern (.NET 8.0), well-organized, and suitable as a reference for a complete Avalonia rewrite targeting desktop and future mobile platforms.

**Code Quality:** Excellent business logic and infrastructure, but UI layer requires complete architectural redesign for cross-platform targets
**Architecture:** Solid core with Windows-specific presentation layer (WinForms)
**Migration Strategy:** New Avalonia project recommended (reference original codebase)
**Estimated Migration Effort (Core Desktop):** 314-401 hours
**Additional Scope (Excluded from Estimate):**
- QA cycles and bug fixes: +60-80 hours
- Platform-specific installers: +20-30 hours per platform
- Mobile app implementation: +86-118 hours (separate phase)
**Total Realistic Estimate:** 374-481 hours (desktop + testing) or 460-599 hours (desktop + mobile)

---

## Project Purpose

A comprehensive ballistic calculator application that allows users to:
1. Configure ammunition, weapon, and atmospheric parameters
2. Calculate bullet trajectories using physics-based ballistic models
3. Visualize results in multiple formats:
   - **Table view** - Detailed trajectory data points
   - **Chart view** - Graphical trajectory plots (velocity, drop, windage, energy, mach)
   - **Reticle view** - Scope sight picture with holdover points
4. Compare multiple trajectories
5. Export data to CSV
6. Edit and manage custom reticles
7. Support both metric and imperial measurement systems
8. Support multiple angular units (MOA, Mil, MRad, Thousand, Inches/100yd, cm/100m)

---

## Solution Structure

### Current WinForms Solution (11 Projects)

```
BallisticCalculator1.sln
├── Core Projects (7)
│   ├── BallisticCalculatorNet                    # Main WinForms application
│   ├── BallisticCalculatorNet.InputPanels        # Reusable input/output controls
│   ├── BallisticCalculatorNet.MeasurementControl # Custom measurement input control
│   ├── BallisticCalculatorNet.ReticleCanvas      # Reticle rendering library
│   ├── BallisticCalculatorNet.ReticleEditor      # Standalone reticle editor
│   ├── BallisticCalculatorNet.Common             # Shared utilities
│   ├── BallisticCalculatorNet.Api                # API and interop layer
│   └── BallisticCalculatorNet.Types              # Shared interfaces and types
│
├── Extensions (1)
│   └── BallisticCalculatorExtension.SaveToPdf    # PDF export extension
│
├── Testing (2)
│   ├── Gehtsoft.Winforms.FluentAssertions        # WinForms testing utilities
│   └── Gehtsoft.Winforms.FluentAssertions.Test   # Tests for above
│
└── Tests (1)
    └── BallisticCalculatorNet.UnitTest           # Comprehensive unit tests
```

### Codebase Statistics

- **Total C# files:** 182
- **Total lines of code:** ~22,000
- **WinForms forms/controls:** 36 (with Designer files)
- **Resource files:** 37 .resx files
- **Target framework:** .NET 8.0 (Windows-specific)

### Key File Counts by Project

| Project | Files | Key Components |
|---------|-------|----------------|
| BallisticCalculatorNet (Main) | 19 | 6 main forms (AppForm, TrajectoryForm, CompareForm, etc.) |
| InputPanels | 57 | 15 custom UserControls |
| ReticleEditor | 24 | 1 main form + 8 dialog forms |
| MeasurementControl | 7 | 1 custom control + controller |
| ReticleCanvas | 4 | Graphics abstraction |
| Types | 8 | Interfaces and data types |
| Api | 15 | Configuration, extensions, interop |
| Common | 12 | Utilities and helpers |

---

## Architecture Overview

### Layered Architecture

```
┌─────────────────────────────────────────────────────┐
│   Presentation Layer (WinForms)                     │
│   - AppForm (MDI Container)                         │
│   - TrajectoryForm, CompareForm                     │
│   - Dialogs (ShotParametersForm, AboutForm)         │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│   UI Component Layer                                │
│   - 15 reusable UserControls (InputPanels)          │
│   - MeasurementControl (custom input)               │
│   - ChartControl (ScottPlot integration)            │
│   - TrajectoryControl (table view)                  │
│   - ReticleControl (reticle visualization)          │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│   Types/Interface Layer                             │
│   - ITrajectoryDisplayForm                          │
│   - IChartDisplayForm, IShotForm                    │
│   - Data types (ShotData, WindCollection, etc.)     │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│   API/Business Logic Layer                          │
│   - Configuration management                        │
│   - Extension system                                │
│   - Interop server (WatsonTcp)                      │
│   - Data serialization                              │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│   External Dependencies                             │
│   - BallisticCalculator (NuGet) - Core engine       │
│   - Gehtsoft.Measurements - Unit conversion         │
│   - ScottPlot.WinForms - Charting                   │
└─────────────────────────────────────────────────────┘
```

### Design Patterns

**1. Component-Based Design**
- Heavy use of UserControls for modularity
- Each control is self-contained with its own logic
- Controls expose properties for manual data binding

**2. Property-Based Communication**
- No INotifyPropertyChanged or data binding framework
- Manual property setters trigger UI updates
- Example pattern from MeasurementControl:
  ```csharp
  public MeasurementSystem MeasurementSystem
  {
      get => mMeasurementSystem;
      set
      {
          mMeasurementSystem = value;
          UpdateSystem(); // Manual update
      }
  }
  ```

**3. Interface-Based Polymorphism**
- `ITrajectoryDisplayForm` - Forms displaying trajectories
- `IChartDisplayForm` - Forms with charts
- `IShotForm` - Forms with shot data
- `IMeasurementSystemControl` - Controls with measurement systems

**4. Controller Pattern (Limited)**
- `MeasurmentControlController` - Separates logic from UI
- Most controls mix logic and UI code

**5. MDI (Multiple Document Interface)**
- AppForm is MDI container
- Child forms: TrajectoryForm, CompareForm
- Menu-driven navigation

---

## Key Components Reference

### 1. Main Application (BallisticCalculatorNet)

**AppForm.cs** (398 lines)
- MDI container with extensive menu system
- Manages trajectory and compare windows
- Handles file operations (open, save, export)
- Extension system integration
- Menu-driven UI state management

**TrajectoryForm.cs**
- Displays single trajectory with three views:
  - Table view (TrajectoryControl)
  - Chart view (ChartControl)
  - Reticle view (ReticleControl)
- Implements ITrajectoryDisplayForm, IChartDisplayForm
- Manages shot parameters and trajectory calculation

**CompareForm.cs**
- Multi-trajectory comparison
- Uses MultiChartControl
- Side-by-side or overlaid visualization

**ShotParametersForm.cs**
- Dialog for entering all calculation parameters
- Uses multiple input panels
- Modal dialog pattern

### 2. Input Controls (BallisticCalculatorNet.InputPanels)

**15 Custom UserControls:**

1. **AmmoControl** - Ammunition parameters
   - Weight, ballistic coefficient, velocity, length
   - Custom ballistic file support
   - Uses 5+ MeasurementControls

2. **AtmosphereControl** - Atmospheric conditions
   - Temperature, pressure, humidity
   - Altitude calculation
   - 4 MeasurementControls

3. **WeaponControl** - Weapon/sight configuration
   - Barrel twist, sight height, zero distance
   - Rifle cant angle
   - Sight angle adjustment

4. **WindControl** - Single wind input
   - Direction, velocity
   - Simple 2-field control

5. **MultiWindControl** - Multiple wind zones
   - Up to 6 wind zones at different ranges
   - Supports complex wind scenarios

6. **ChartControl** ⚠️ **CRITICAL COMPONENT**
   - ScottPlot.WinForms integration (v4.1.74)
   - Multiple chart modes: Velocity, Drop, Windage, Energy, Mach
   - Uses ChartController for data transformation
   - Axis management and auto-scaling

7. **MultiChartControl** - Multiple trajectory charts
   - Extends ChartControl
   - Manages multiple data series
   - Legend support

8. **TrajectoryControl** - Table view
   - ListView-based trajectory data display
   - Supports unit conversion for display
   - Formatted output with precision

9. **ReticleControl** - Reticle visualization
   - Custom drawing using ReticleCanvas
   - Trajectory overlay (holdover points)
   - Point of impact display
   - Zoom support

10. **ParametersControl** - Shot parameters summary
11. **ZeroAmmunitionControl** - Zeroing ammunition
12. **ZeroAtmosphereControl** - Zeroing atmosphere
13. **AmmoLibEntryControl** - Ammunition library entry
14. **ShotDataControl** - Complete shot data input
15. **DataList controls** - Library management UI

### 3. MeasurementControl (BallisticCalculatorNet.MeasurementControl)

**Purpose:** Universal input control for measurements with units

**Structure:**
- TextBox for numeric input
- ComboBox for unit selection
- TableLayoutPanel layout

**Key Features:**
- Generic type support: `Measurement<T>` where T is unit enum
- Unit conversion on the fly
- Validation and formatting
- Keyboard navigation (Up/Down for increment/decrement)
- Culture-aware number parsing
- Controller separation: `MeasurmentControlController`

**API:**
```csharp
// Get/set value as specific measurement type
public Measurement<WeightUnit> ValueAsMeasurement<WeightUnit>();
public T ValueAs<T>() where T : struct;

// Get/set unit
public object Unit { get; set; }

// Change unit with value conversion
public void ChangeUnit<T>(T unit, int? accuracy = null);
```

**Good Architecture:** Already has UI/logic separation via controller - excellent reference for MVVM conversion.

### 4. ReticleCanvas (BallisticCalculatorNet.ReticleCanvas)

**Purpose:** Abstraction layer for reticle rendering

**Components:**
- Graphics abstraction interfaces
- Reticle drawing logic
- Coordinate system management

**Current Implementation:** System.Drawing.Graphics-based

**Migration Note:** Will need Skia or Avalonia DrawingContext equivalent

### 5. Types (BallisticCalculatorNet.Types)

**100% REUSABLE - No UI dependencies**

**Key Interfaces:**
```csharp
public interface ITrajectoryDisplayForm
{
    MeasurementSystem MeasurementSystem { get; set; }
    AngularUnit AngularUnits { get; set; }
    DropBase DropBase { get; set; }
}

public interface IChartDisplayForm
{
    TrajectoryChartMode ChartMode { get; set; }
    void UpdateYToVisibleArea();
}

public interface IShotForm
{
    ShotData ShotData { get; set; }
}
```

**Key Types:**
- `ShotData` - Complete shot configuration
- `WindCollection` - Wind data collection
- `TrajectoryFormState` - Serialization state
- Enums: `TrajectoryChartMode`, `DropBase`, `AngularUnit`

### 6. Api (BallisticCalculatorNet.Api)

**Configuration Management:**
- Microsoft.Extensions.Configuration integration
- JSON-based configuration
- Command-line argument support

**Extension System:**
- Plugin architecture
- Command registration
- WatsonTcp-based interop server
- External tool integration

**Key Classes:**
- `ControlConfiguration` - Global configuration
- `ExtensionsManager` - Extension management
- `InteropServer` - TCP server for extensions
- `ChartController` - Chart data transformation
- `CvsExportController` - CSV export logic

### 7. Common (BallisticCalculatorNet.Common)

**Utilities and Helpers:**
- Form state persistence
- Data list management
- DataItem collections
- Serialization helpers
- Logging integration (Serilog)

**100% REUSABLE** - Minimal to no changes needed

### 8. ReticleEditor (BallisticCalculatorNet.ReticleEditor)

**Standalone Application** (665 lines main form)

**Features:**
- Visual reticle editor
- 8 dialog forms for editing different reticle elements
- Save/load reticle files
- Drawing tools
- Preview functionality

**Migration:** Separate Avalonia application, similar approach to main app

---

## External Dependencies

### Critical NuGet Packages

```xml
<!-- Core Business Logic -->
<PackageReference Include="BallisticCalculator" Version="1.1.7.1" />
<PackageReference Include="Gehtsoft.Measurements" Version="1.1.16" />

<!-- UI Components -->
<PackageReference Include="ScottPlot.WinForms" Version="[4.1.74]" />
  ⚠️ CRITICAL: Must migrate to ScottPlot 5.x for Avalonia

<!-- Infrastructure -->
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.9" />
<PackageReference Include="Serilog" Version="4.3.0" />
<PackageReference Include="System.Text.Json" Version="9.0.9" />
<PackageReference Include="WatsonTcp" Version="6.0.10" />
```

### External Library Locations

**BallisticCalculator (Core Engine):**
- Location: `/mnt/d/develop/components/BusinessSpecificComponents/BallisticCalculator.Net`
- Purpose: Physics-based trajectory calculation
- Status: External NuGet package - no changes needed
- Cross-platform: Should be compatible with Avalonia

**Gehtsoft.Measurements:**
- Location: `/mnt/d/develop/components/BusinessSpecificComponents/Gehtsoft.Measurements`
- Purpose: Unit conversion and measurement types
- Status: External NuGet package - no changes needed
- Cross-platform: Should be compatible with Avalonia

---

## Code Quality Assessment

### Strengths

✅ **Modern .NET 8.0** - Already on latest framework
✅ **Clean Project Structure** - Well-organized into logical projects
✅ **Type Abstraction** - Good use of interfaces
✅ **External Business Logic** - Core calculations separate from UI
✅ **Component-Based** - Reusable UserControls
✅ **Test Infrastructure Present** - xUnit tests with FluentAssertions pattern
⚠️ **Note:** Tests use `Gehtsoft.Winforms.FluentAssertions` (WinForms-specific contracts). These cannot be ported to Avalonia. New ViewModel/control tests must be written from scratch, targeting ~80% coverage for ViewModels.
✅ **Configuration Abstracted** - Uses Microsoft.Extensions.Configuration
✅ **Logging Integration** - Serilog properly integrated
✅ **Extension System** - Plugin architecture in place
✅ **No Legacy Cruft** - Clean, modern C# code

### Areas for Improvement (in Avalonia)

🔄 **Manual Property Updates** - No data binding framework
🔄 **Tight UI/Logic Coupling** - Logic mixed with UI in many controls (note: MeasurementControl is well-separated and serves as good reference)
🔄 **Event-Driven** - No command pattern
🔄 **No ViewModels** - Business logic in code-behind
🔄 **MDI Architecture** - Windows-specific pattern (will be replaced with tab-based navigation)
🔄 **UI Layer is Intentionally Throwaway** - While business logic is excellent, the entire WinForms UI layer must be discarded and rebuilt for cross-platform compatibility

### Separation of Concerns

**Well Separated:**
- Core ballistics calculations (external package)
- Data types (Types project)
- Configuration management (Api project)
- Measurement conversions (external library)

**Needs Separation in Avalonia:**
- Control logic from UI presentation
- Event handlers → Commands
- Manual updates → Data binding
- Property setters → INotifyPropertyChanged

---

## Migration Challenges

### Challenge 1: ScottPlot Migration 🔴 **CRITICAL**

**Current:** ScottPlot.WinForms 4.1.74
**Target:** ScottPlot 5.x for Avalonia
**Impact:** HIGH - Charts are core functionality
**Effort:** 40-50 hours

**API Changes:**
```csharp
// ScottPlot 4.x (WinForms)
formsPlot1.Plot.AddScatter(x, y, label: label);
formsPlot1.Plot.XAxis.Label(title);
formsPlot1.Plot.AxisAuto();
formsPlot1.Refresh();

// ScottPlot 5.x (Avalonia)
avaPlot.Plot.Add.Scatter(x, y).Label = label;
avaPlot.Plot.Axes.Bottom.Label.Text = title;
avaPlot.Plot.Axes.AutoScale();
avaPlot.Refresh();
```

**Files Affected:**
- `ChartControl.cs` (172 lines)
- `MultiChartControl.cs`
- `ChartController.cs` (data transformation logic - reusable)

### Challenge 2: Custom Rendering (Reticle) 🟡 **MEDIUM-HIGH**

**Current:** System.Drawing.Graphics
**Target:** Skia (SkiaSharp) or Avalonia DrawingContext
**Impact:** MEDIUM - Reticle view is important but not critical
**Effort:** 25-30 hours

**Solution:** SkiaSharp provides very similar API to System.Drawing

**Files Affected:**
- `ReticleControl.cs`
- `BallisticCalculatorNet.ReticleCanvas` project
- ReticleEditor drawing code

### Challenge 3: MDI Architecture 🟡 **MEDIUM**

**Current:** WinForms MDI container
**Target:** Tab-based or window management
**Impact:** MEDIUM - Affects main window architecture
**Effort:** 15-25 hours

**Recommendation:** Tab-based navigation for desktop (mobile-compatible)

### Challenge 4: No MVVM Pattern 🟢 **OPPORTUNITY**

**Current:** Manual property updates, event-driven
**Target:** ReactiveUI with data binding
**Impact:** HIGH - Major architecture improvement
**Effort:** Included in overall migration (not additional)

**Benefits:**
- Testable business logic (ViewModels)
- Automatic UI updates
- Cleaner code separation
- Mobile-ready architecture

### Challenge 5: 15 Custom Controls 🟡 **MEDIUM**

**Current:** 15 WinForms UserControls
**Target:** Avalonia UserControls with XAML
**Impact:** MEDIUM - Significant conversion work
**Effort:** 55-70 hours total

**Strategy:** Convert in priority order, reuse ViewModels

---

## Key Learnings for Avalonia

### What Works Well (Keep)

1. **Project structure** - Logical separation into projects
2. **Interface-based design** - ITrajectoryDisplayForm, etc.
3. **Types project** - 100% reusable as-is
4. **Api project** - Mostly reusable
5. **Common utilities** - Reusable helpers
6. **Controller pattern** - MeasurmentControlController is good separation
7. **Extension system** - Plugin architecture
8. **Test infrastructure** - Testing approach is sound

### What Needs Redesign (Improve)

1. **MDI architecture** → Tab-based navigation
2. **Manual property updates** → Data binding
3. **Event handlers** → Commands (ICommand/ReactiveCommand)
4. **Code-behind logic** → ViewModels
5. **ScottPlot 4.x** → ScottPlot 5.x
6. **System.Drawing** → Skia
7. **WinForms controls** → Avalonia XAML

### Reusable Code Patterns

**Good Controller Separation Example:**
```csharp
// From MeasurmentControlController.cs - Excellent reference!
public class MeasurmentControlController
{
    public MeasurementType MeasurementType { get; set; }
    public double Increment { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }

    public object Value(string text, object unit, int? decimalPoints)
    {
        // Pure logic - no UI dependencies
    }

    public bool AllowKeyInEditor(string text, int position, int length, char key)
    {
        // Validation logic - reusable in Avalonia
    }
}
```

**This pattern should be extended to ALL controls in Avalonia version!**

---

## Testing Infrastructure

### Current Testing Approach

**xUnit Framework:**
- Comprehensive unit tests
- Integration tests for UI controls
- Custom FluentAssertions for WinForms

**Test Categories:**
- MeasurementControl tests
- InputPanels tests (AmmoPanel, AtmoPanel, WindPanel, WeaponPanel)
- OutputPanels tests (TrajectoryTest)
- ReticleEditor tests

**Test Project:** BallisticCalculatorNet.UnitTest

### Avalonia Testing Strategy

**Advantages of MVVM for Testing:**
- ViewModels are pure C# classes (easy to test)
- No UI dependencies in business logic
- Mocking is straightforward
- Higher test coverage possible

**Example Test Strategy:**
```csharp
// ViewModel testing (no UI needed!)
[Fact]
public void AmmoViewModel_ShouldValidate_WhenWeightIsZero()
{
    var vm = new AmmoViewModel();
    vm.BulletWeight = new Measurement<WeightUnit>(0, WeightUnit.Grain);

    Assert.False(vm.IsValid);
    Assert.Contains("Weight must be greater than zero", vm.ValidationErrors);
}
```

---

## File Operations and Serialization

### Current Implementation

**XML Serialization:**
- Uses `BallisticXmlSerializer` / `BallisticXmlDeserializer` from **BallisticCalculator NuGet package (v1.1.7.1)**
- Custom attribute-based serialization framework designed for ballistic data
- Handles `Measurement<T>` types, ballistic coefficients, wind zones natively
- Saves complete trajectory state to `.trajectory` files (XML format)
- **Already cross-platform** - part of .NET Standard library

**CSV Export:**
- `CvsExportController` handles export
- Trajectory data with proper unit formatting

**Configuration:**
- JSON-based configuration files
- Microsoft.Extensions.Configuration

### Avalonia Migration Notes

✅ **Serialization is 100% reusable** - Same BallisticCalculator NuGet package works on all platforms
✅ **Zero development time** - No new serialization code needed
✅ **100% backward compatible** - Existing .trajectory files work seamlessly in Avalonia app
✅ **CvsExportController is reusable** - Pure data transformation
⚠️ **File dialogs need platform-specific implementations**
  - Desktop: Native file dialogs (Avalonia.Desktop)
  - Mobile: Platform-specific file pickers

**Key Decision:** Keep existing XML serialization format. No benefits to changing it, and it ensures seamless migration for users.

---

## Extension System

### Current Architecture

**WatsonTcp-based Interop Server:**
- TCP server for inter-process communication
- Allows external tools to interact with calculator
- Extension commands registered in menu

**Extension Points:**
- Commands (executable + command ID)
- PDF export extension (separate project)

**Files:**
- `InteropServer.cs` - TCP server implementation
- `ExtensionsManager.cs` - Extension management
- `IInteropServerHost.cs` - Host interface

### Avalonia Migration Notes

⚠️ **WatsonTcp cross-platform compatibility needs verification**
✅ **Extension architecture is sound** - Can be ported to Avalonia
🔄 **Menu integration needs redesign** - Avalonia menu system

---

## User Experience Patterns

### Current Desktop UX

**Menu-Driven Interface:**
- Extensive menu system (File, View, Tools, Help, Extensions)
- Keyboard shortcuts (not explicitly implemented, but standard)
- Status updates via menu state

**MDI Windowing:**
- Multiple trajectory windows
- Compare window
- Window management (tile, cascade)

**View Switching:**
- Table/Chart/Reticle tabs within trajectory window
- Chart mode switching (Velocity, Drop, Windage, Energy, Mach)
- Unit system switching (Imperial/Metric)
- Angular unit switching (MOA, Mil, etc.)

**Data Entry:**
- Modal dialog for shot parameters
- Direct editing in trajectory window
- Library-based selection (ammo, weapon data)

### Avalonia Desktop UX Goals

**Keep Desktop Advantages:**
- Full menu system
- Keyboard shortcuts
- Multi-column layouts
- Split views (table + chart simultaneously)
- Context menus
- Tabs or multi-window

**Improve:**
- Modern UI styling (FluentTheme)
- Better data binding (automatic updates)
- Command pattern (undo/redo potential)
- Responsive layouts (adapt to window size)

---

## Performance Considerations

### Current Performance Characteristics

**Trajectory Calculation:**
- Handled by external BallisticCalculator library
- Generally fast (< 1 second for typical trajectories)
- Calculation on demand (button click)

**Chart Rendering:**
- ScottPlot handles rendering efficiently
- Refresh on data change
- Y-axis zoom calculation (UpdateYAxis method)

**UI Responsiveness:**
- Manual property updates are immediate
- No async operations observed
- Synchronous file I/O

### Avalonia Performance Goals

**Maintain:**
- Fast trajectory calculation
- Responsive UI updates

**Improve:**
- Async file operations (async/await)
- Background trajectory calculation (if needed)
- Reactive updates (ReactiveUI efficiency)

---

## Internationalization

### Current Implementation

**Measurement Systems:**
- Imperial (yards, feet, inches, pounds, fps)
- Metric (meters, centimeters, kilograms, m/s)
- Switchable at runtime

**Angular Units:**
- MOA (Minute of Angle)
- Mil (Milliradian)
- MRad (Milliradian)
- Thousand (Russian system)
- Inches per 100 yards
- Centimeters per 100 meters

**Culture Support:**
- MeasurementControl has `ForceCulture` method
- Number formatting respects culture
- Decimal separator handling

### Avalonia I18N Strategy

✅ **Measurement system support is excellent** - Reuse as-is
✅ **Unit conversion logic is external** - Gehtsoft.Measurements
🔄 **UI localization not implemented** - Could be added in Avalonia
🔄 **Resource strings not externalized** - Opportunity for improvement

---

## Summary for Avalonia Migration

### What to Reuse Directly (After Retargeting)

1. **BallisticCalculatorNet.Types** - All interfaces and types (retarget to net8.0 from net8.0-windows)
2. **BallisticCalculatorNet.Common** - Utilities and helpers (retarget to net8.0 from net8.0-windows)
3. **BallisticCalculatorNet.Api** - Configuration and most helpers (multi-target to net8.0 and net8.0-windows)
   - ✅ Configuration management - fully reusable
   - ✅ ChartController, CvsExportController - fully reusable
   - ⚠️ InteropServer/ExtensionsManager - verify WatsonTcp cross-platform compatibility in early spike (Phase 1.2)
   - **Action Required:** Test WatsonTcp on macOS/Linux before relying on it
4. **External NuGet packages** - BallisticCalculator (includes XML serialization), Gehtsoft.Measurements (already cross-platform)
5. **Business logic** - Calculation logic, data transformation
6. **Serialization logic** - BallisticXmlSerializer/Deserializer from BallisticCalculator NuGet (100% backward compatible)
7. **Test approach** - xUnit testing patterns (but not WinForms-specific tests themselves)

### What to Reference and Adapt

1. **MeasurmentControlController** - Controller pattern → ViewModel
2. **ChartController** - Data transformation → ChartViewModel
3. **CvsExportController** - Export logic (100% reusable)
4. **Control layouts** - Reference for XAML design
5. **Validation logic** - Port to ViewModels
6. **File operations** - Serialization 100% reusable, dialogs need platform implementations
7. **TrajectoryFormState** - Use as-is for file serialization

### What to Redesign

1. **UI Architecture** - WinForms → Avalonia XAML + MVVM
2. **ScottPlot integration** - v4.1.74 → v5.x
3. **Custom rendering** - System.Drawing → Skia
4. **Navigation** - MDI → Tab-based
5. **Data binding** - Manual → ReactiveUI
6. **Event handling** - Events → Commands

---

## Migration Success Criteria

### Functional Parity

✅ All trajectory calculation features
✅ All visualization modes (table, chart, reticle)
✅ Multi-trajectory comparison
✅ File operations (open, save, export)
✅ Measurement system switching
✅ Extension system
✅ Reticle editor

### Architecture Improvements

✅ MVVM architecture with ReactiveUI
✅ Testable ViewModels (>80% coverage)
✅ Clean separation of concerns
✅ Cross-platform compatibility (desktop)
✅ Mobile-ready architecture

### Desktop UX Maintained

✅ Multi-column layouts
✅ Split views (table + chart)
✅ Keyboard shortcuts
✅ Full menu system
✅ Context menus
✅ Responsive design

### Future Mobile-Ready

✅ Shared ViewModels (100% reuse)
✅ Shared controls (80% reuse)
✅ Adaptive layouts
✅ Platform-specific services (IDialogService, IFileService)

---

## Reference Project Location

**Original WinForms Project:**
- Path: `/mnt/d/develop/homeapps/BallisticCalculator1`
- Solution: `BallisticCalculator1.sln`
- Git Repository: Yes (main branch)
- Last Commit: `a559797 Fix wind control behavior, add a few new ammo profiles`

**Use as Reference For:**
- Business logic implementation
- Control layouts and behavior
- Calculation flow
- Data structures
- Validation rules
- File formats
- Extension system architecture
- Test patterns

**Do Not Copy:**
- WinForms-specific code (Designer.cs files)
- Manual property update patterns
- Event-driven architecture
- MDI window management
- System.Drawing rendering code

---

## Recommended Reading Order for Reference

When referencing the original codebase during Avalonia migration:

1. **Start with Types:**
   - `BallisticCalculatorNet.Types/ITrajectoryDisplayForm.cs`
   - `BallisticCalculatorNet.Types/ShotData.cs`
   - Understand the data contracts

2. **Study Controller Pattern:**
   - `MeasurementControl/MeasurmentControlController.cs`
   - Best example of UI/logic separation

3. **Understand Business Logic:**
   - `BallisticCalculatorNet.Api/ChartController.cs`
   - `BallisticCalculatorNet.Api/CvsExportController.cs`
   - Pure logic, highly reusable

4. **Reference Control Behavior:**
   - `InputPanels/MeasurementControl.cs` (ignore Designer.cs)
   - `InputPanels/AmmoControl.cs`
   - `InputPanels/ChartControl.cs`
   - Understand what they do, not how they're implemented

5. **Study Main Application Flow:**
   - `BallisticCalculatorNet/AppForm.cs`
   - Menu structure and navigation
   - `BallisticCalculatorNet/TrajectoryForm.cs`
   - Main workflow

6. **Review Test Patterns:**
   - `BallisticCalculatorNet.UnitTest/` project
   - Testing approach and coverage

---

## Version Information

**Framework:** .NET 8.0 (net80-windows)
**Language:** C# 12
**IDE:** Compatible with Visual Studio 2022 / Rider

**Key Package Versions:**
- BallisticCalculator: 1.1.7.1
- Gehtsoft.Measurements: 1.1.16
- ScottPlot.WinForms: 4.1.74
- Microsoft.Extensions.Configuration: 9.0.9
- Serilog: 4.3.0

**Last Updated:** November 2025
