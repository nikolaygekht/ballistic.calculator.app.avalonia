# BallisticCalculator2 — Project Status

Last updated: 2026-03-19

## Completed

### Controls Library (`Common/BallisticCalculator.Controls/`)

| Component | Status | Notes |
|-----------|--------|-------|
| MeasurementControl | Done | Generic measurement input with unit selector, reflection bridge for XAML |
| MeasurementController | Done | Pure logic: Value, ParseValue, ParseValuePreservePrecision, IncrementValue, AllowKeyInEditor |
| BallisticCoefficientControl | Done | BC value + drag table selector |
| WindDirectionControl | Done | Canvas-based visual wind direction indicator with click/drag |
| WindDirectionController | Done | Geometry logic for arrow rendering and click-to-angle |
| ReticleCanvasControl | Done | Reticle rendering with zero-size guard, Underlay/Overlay collections |
| TrajectoryChartControl | Done | ScottPlot-based trajectory chart |
| TrajectoryTableControl | Done | DataGrid-based trajectory table, GetColumnWidths/SetColumnWidths for persistence |
| TrajectoryToReticleCalculator | Done | Maps BDC points to distances by interpolating trajectory, Near/Far classification |
| ReticleOverlayController | Done | Creates BDC text labels and target rectangle overlays for reticle display |

### Panels Library (`Common/BallisticCalculator.Panels/`)

| Panel | Status | Tests | Notes |
|-------|--------|-------|-------|
| AmmoPanel | Done | 14 tests | Weight, BC, FormFactor, MuzzleVelocity, BulletDiameter, BulletLength |
| AmmoLibraryRecordPanel | Done | 18 tests | AmmoPanel + Name, Caliber, Type, BarrelLength, Source, Load/Save |
| WindPanel | Done | 15 tests | Direction (degrees), Velocity, optional MaxDistance, IsEmpty |
| MultiWindPanel | Done | 15 tests | Dynamic list of WindPanels, Add/Clear, auto-distance, copy prev values |
| AtmospherePanel | Done | 16 tests | Altitude, Pressure, Temperature, Humidity (%), Reset to standard, IsEmpty |
| RiflePanel | Done | 25 tests | SightHeight, ZeroDistance, H/V Click, Rifling (direction+step), VerticalOffset, IsEmpty |
| ParametersPanel | Done | 18 tests | MaxRange, Step, Angle, angle-as-clicks (via RiflePanel.VerticalClick), IsEmpty |
| ZeroAmmoPanel | Done | 12 tests | Checkbox + embedded AmmoPanel, propagates MeasurementSystem/ConvertFlag |
| ZeroAtmospherePanel | Done | 12 tests | Checkbox + embedded AtmospherePanel, propagates MeasurementSystem/ConvertFlag |
| ShotDataPanel | Done | 13 tests | TabControl container, Validate() method for partial data handling |
| ReticlePanel | Done | — | Reticle display with BDC (near/far) and target overlay, accepts ShotData |

### Main Desktop Application (`Desktop/BallisticCalculator/`) — Phases 1-2 Complete

| Component | Status | Notes |
|-----------|--------|-------|
| Project setup | Done | WinExe, net8.0, WindowManager 4.0.0, icon from old app |
| App/Program.cs | Done | ClassicTheme + DataGrid theme, AppFontSize, global exception handling |
| IAppChildWindow | Done | Base interface: MeasurementSystem, AngularUnits, DropBase, ChartMode |
| ITrajectoryChildWindow | Done | Extends base: ShotData, Trajectory, FileName, Show/Zoom methods |
| IComparisonChartChildWindow | Done | Extends base: Add/RemoveTrajectory, TrajectoryCount |
| MainWindow + Menu | Done | Full menu matching old WinForms app (Trajectory/View/Windows/Help) |
| Keyboard shortcuts | Done | All accelerators via KeyDown handler (Ctrl+I/M/O/S/E/X/T/C/R/F1, Ctrl+Shift, Ctrl+Alt) |
| Menu enable/disable | Done | Based on active child window type (trajectory vs comparison vs none) |
| Menu checkmarks | Done | Radio-style checkmarks for MeasurementSystem, Angular, Drop, Chart |
| MDI via WindowsPanel | Done | ManagedWindow children, activation tracking |
| Windows menu - window list | Done | Dynamic numbered entries, checkmark on active, click to switch |
| Windows > Cascade | Done | Same-size windows filling MDI area with 30px offset |
| New window placement | Done | MDI-style staggered: 0,0 → 30,30 → ... → 270,270 → wraps |
| Persistent state | Done | appstate.json: main window geometry, child window size, table column widths, dialog size |
| ShotParametersDialog | Done | Modal wrapping ShotDataPanel, FileDialogService, smart validation (see below) |
| ShotCalculator | Done | Wraps ballistic engine, ApplyDefaults for empty panels |
| TrajectoryView | Done | TabControl: Table (DataGrid), Chart (ScottPlot), Reticle (ReticlePanel) |
| FileDialogService | Done | Avalonia StorageProvider implementation of IFileDialogService |
| TrajectoryFormState | Done | BXml serialization model for .trajectory files, compatible with old app |
| Open / Save / Save As | Done | BXml XML serialization, restores MeasurementSystem/AngularUnits/ChartMode |
| Edit Parameters | Done | Reopens ShotParametersDialog with current data, recalculates trajectory |
| Export CSV | Done | Two formats: Local (locale numbers + list separator for Excel) and Invariant (portable) |
| CsvExportController | Done | Range, Velocity, Mach, Path, Hold, Clicks, Windage, Win.Adj, Clicks, Time, Energy, OGW |

### Other Desktop Applications

| App | Status | Notes |
|-----|--------|-------|
| ReticleEditor | Working | Reduced spacing, dialog fixes (cancel, zero-size guard, button states) |
| DebugApp | Working | Controls testing (MeasurementControl, BallisticCoefficientControl, etc.) |
| DebugApp1 | Working | Panels testing: all input panels + ReticlePanel tab with test ShotData |

### Test Summary

| Project | Test Count | Status |
|---------|-----------|--------|
| BallisticCalculator.Controls.Tests | 265 | All passing |
| BallisticCalculator.Panels.Tests | 167 | All passing |

## Key Design Decisions

### Value Formatting — Two-Path Precision

- **`ParseValuePreservePrecision`** — used by `SetValue<T>()` (programmatic/load). Preserves original precision up to 5 meaningful digits, trims trailing zeros (e.g., 0.308 stays "0.308", 40 stays "40" not "40.00").
- **`ParseValue`** — used by `Value` property setter and `ChangeUnit` conversions. Strict DecimalPoints formatting (e.g., 2dp → "100.46").

### Measurement System Switching

- `ConvertOnSystemChange = true`: convert filled values, switch empty units
- `ConvertOnSystemChange = false`: leave filled values untouched, switch empty units
- Setting values via property (loading data): NO conversion, pass as-is

### Wind Panel Units

| System | Distance | Velocity | Direction |
|--------|----------|----------|-----------|
| Metric | m (0dp) | m/s (1dp) | degrees (0dp) |
| Imperial | yd (0dp) | mph (1dp) | degrees (0dp) |

### MultiWindPanel Add Behavior

1. Initially: single panel, distance disabled ("wind everywhere")
2. On Add: if first panel distance disabled → enable at 0
3. New panel: distance = previous + 100, copies previous direction & velocity
4. All panels have X button (disabled for first) for consistent layout

### MDI Window Management

- `iciclecreek.Avalonia.WindowManager` v4.0.0 provides WindowsPanel + ManagedWindow
- ManagedWindow.Activated/Deactivated events drive active window tracking
- ManagedWindow.Resized event is declared but never raised — use Closed event to capture child size
- SizeToContent must be set to Manual for explicit Width/Height to take effect
- DataGrid requires `Avalonia.Controls.DataGrid/Themes/Simple.xaml` style include in App.axaml

### Shot Parameters Dialog — Smart Validation

Three-tier validation when user clicks OK:
1. **Ammunition missing** → error message, stay in dialog
2. **Partially filled panels** (e.g. zero distance but no sight height) → error listing incomplete panels, stay in dialog
3. **Completely empty panels** → confirm "use default values?", Yes proceeds with defaults, No stays in dialog

Default values applied by `ShotCalculator.ApplyDefaults`:
- Atmosphere: standard (sea level, 59°F, 29.92 inHg, 0% humidity)
- Rifle: 3" sight height, 100 yd/m zero distance (based on measurement system)
- Parameters: 1000 yd/m max range, 100 yd/m step (based on measurement system)

### ReticlePanel — Fine-Grained Trajectory for BDC

The ReticlePanel accepts `ShotData` and internally recalculates trajectory at 2.5m steps (up to 1500m). This is necessary because the display trajectory (50-100yd steps) is too coarse for BDC matching — at close range the bullet is well below the sight line (e.g., 2.5in sight height = ~2 mil at 3yd), and these steep angular drops are missed by coarse steps. The old WinForms app used the same approach.

### ReticleCanvasControl — Overlay Equality Fix

`CustomDrawOp.Equals()` must compare `_underlay` and `_overlay` references in addition to `_reticle` and `_bounds`. Without this, Avalonia skips re-rendering when only the overlay changes (same reticle, same bounds = "equal" draw op).

### File Format — .trajectory

`TrajectoryFormState` wraps `TrajectoryFormShotData` (BXml-serializable analog of `ShotData`) plus display state (MeasurementSystem, AngularUnits, ChartMode). Uses `WindCollection` wrapper because BXml collection deserialization requires a type with `Add` method. `FromShotData()`/`ToShotData()` methods convert between the serializable and runtime types. Format is compatible with the old WinForms app.

### CSV Export — Locale Option

Two export modes via submenu:
- **Local Format**: uses `CultureInfo.CurrentCulture` for numbers and `TextInfo.ListSeparator` as delimiter (e.g. semicolon in comma-decimal locales). Opens correctly in Excel on the same machine.
- **Invariant Format**: uses `CultureInfo.InvariantCulture` with comma delimiter. Portable across systems and programmatic parsers.

## File Structure (Current)

```
Common/
├── BallisticCalculator.Controls/
│   ├── Controls/          (MeasurementControl, BallisticCoefficientControl, WindDirectionControl, ReticleCanvasControl, ...)
│   ├── Controllers/       (MeasurementController, WindDirectionController, ChartController, TrajectoryToReticleCalculator, ReticleOverlayController)
│   └── Models/            (UnitItem, DragTableInfo, WindArrow)
├── BallisticCalculator.Controls.Tests/
│   ├── Controllers/       (MeasurementControllerTests, WindDirectionControllerTests, ChartControllerTests, TrajectoryToReticleCalculatorTests, ReticleOverlayControllerTests)
│   └── UI/                (MeasurementControlTests, BallisticCoefficientControlTests)
├── BallisticCalculator.Panels/
│   ├── Panels/            (AmmoPanel, AmmoLibraryRecordPanel, AtmospherePanel, RiflePanel, WindPanel, MultiWindPanel, ReticlePanel)
│   ├── Services/          (IFileDialogService, FileDialogOptions, FileDialogFilter)
│   └── PLAN-InputPanels.md
├── BallisticCalculator.Panels.Tests/
│   └── Panels/            (AmmoPanelTests, AmmoLibraryRecordPanelTests, AtmospherePanelTests, RiflePanelTests, WindPanelTests, MultiWindPanelTests)
├── BallisticCalculator.Types/
│   └── MeasurementSystem.cs, ChartTrajectory.cs, DropBase.cs, TrajectoryChartMode.cs, ShotData.cs
Desktop/
├── BallisticCalculator/       (Main desktop application)
│   ├── Models/                (AppState, AppStateManager, TrajectoryFormState)
│   ├── Views/                 (MainWindow, TrajectoryView, TestTrajectoryView)
│   │   ├── Dialogs/           (ShotParametersDialog)
│   │   └── Interfaces/        (IAppChildWindow, ITrajectoryChildWindow, IComparisonChartChildWindow)
│   ├── Utilities/             (ShotCalculator, CsvExportController)
│   ├── Services/              (FileDialogService)
│   └── Assets/                (Shooting.ico)
├── DebugApp/                  (Controls testing app)
├── DebugApp1/                 (Panels testing app)
└── ReticleEditor/             (Reticle editor application)
```

## Next Steps — APP_PLAN.md Phases 3-5

### Phase 3: Display Settings
- Measurement system switching via menu → active window (wired, needs TrajectoryView refresh)
- Chart mode switching (wired)
- Show Table/Chart/Reticle tab switching (wired)
- Chart Zoom Y axis (wired)

### Phase 4: Compare
- CompareView with TrajectoryChartControl
- Add to Compare / Remove Last
- CompareView implements IComparisonChartChildWindow (interface exists)

### Phase 5: Polish
- About dialog
- Tests: ShotCalculator, CsvExport
