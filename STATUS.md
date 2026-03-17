# BallisticCalculator2 — Project Status

Last updated: 2026-03-17

## Completed

### Controls Library (`Common/BallisticCalculator.Controls/`)

| Component | Status | Notes |
|-----------|--------|-------|
| MeasurementControl | Done | Generic measurement input with unit selector, reflection bridge for XAML |
| MeasurementController | Done | Pure logic: Value, ParseValue, ParseValuePreservePrecision, IncrementValue, AllowKeyInEditor |
| BallisticCoefficientControl | Done | BC value + drag table selector |
| WindDirectionControl | Done | Canvas-based visual wind direction indicator with click/drag |
| WindDirectionController | Done | Geometry logic for arrow rendering and click-to-angle |
| ReticleCanvasControl | Done | Reticle rendering with zero-size guard |
| TrajectoryChartControl | Done | ScottPlot-based trajectory chart |
| TrajectoryTableControl | Done | DataGrid-based trajectory table |

### Panels Library (`Common/BallisticCalculator.Panels/`)

| Panel | Status | Tests | Notes |
|-------|--------|-------|-------|
| AmmoPanel | Done | 14 tests | Weight, BC, FormFactor, MuzzleVelocity, BulletDiameter, BulletLength |
| AmmoLibraryRecordPanel | Done | 18 tests | AmmoPanel + Name, Caliber, Type, BarrelLength, Source, Load/Save |
| WindPanel | Done | 15 tests | Direction (degrees), Velocity, optional MaxDistance, WindDirectionControl indicator |
| MultiWindPanel | Done | 15 tests | Dynamic list of WindPanels, Add/Clear, auto-distance, copy prev values |
| AtmospherePanel | Done | 16 tests | Altitude, Pressure, Temperature, Humidity (%), Reset to standard |
| RiflePanel | Not started | — | |
| ParametersPanel | Not started | — | |
| ZeroAmmoPanel | Not started | — | |
| ZeroAtmospherePanel | Not started | — | |
| ShotDataPanel | Not started | — | Container for all panels |

### Desktop Applications

| App | Status | Notes |
|-----|--------|-------|
| ReticleEditor | Working | Reduced spacing, dialog fixes (cancel, zero-size guard, button states) |
| DebugApp | Working | Controls testing (MeasurementControl, BallisticCoefficientControl, etc.) |
| DebugApp1 | Working | Panels testing: AmmoPanel, AmmoLibraryRecordPanel, AtmospherePanel, MultiWindPanel tabs |

### Test Summary

| Project | Test Count | Status |
|---------|-----------|--------|
| BallisticCalculator.Controls.Tests | ~30+ | All passing |
| BallisticCalculator.Panels.Tests | 78 | All passing |

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

## File Structure (Current)

```
Common/
├── BallisticCalculator.Controls/
│   ├── Controls/          (MeasurementControl, BallisticCoefficientControl, WindDirectionControl, ...)
│   ├── Controllers/       (MeasurementController, WindDirectionController)
│   └── Models/            (UnitItem, DragTableInfo, WindArrow)
├── BallisticCalculator.Controls.Tests/
│   ├── Controllers/       (MeasurementControllerTests, WindDirectionControllerTests)
│   └── UI/                (MeasurementControlTests, BallisticCoefficientControlTests)
├── BallisticCalculator.Panels/
│   ├── Panels/            (AmmoPanel, AmmoLibraryRecordPanel, AtmospherePanel, WindPanel, MultiWindPanel)
│   ├── Services/          (IFileDialogService, FileDialogOptions, FileDialogFilter)
│   └── PLAN-InputPanels.md
├── BallisticCalculator.Panels.Tests/
│   └── Panels/            (AmmoPanelTests, AmmoLibraryRecordPanelTests, AtmospherePanelTests, WindPanelTests, MultiWindPanelTests)
├── BallisticCalculator.Types/
│   └── MeasurementSystem.cs, ChartTrajectory.cs, etc.
Desktop/
├── DebugApp/              (Controls testing app)
├── DebugApp1/             (Panels testing app — AmmoPanel, AmmoLibraryRecordPanel, AtmospherePanel, MultiWindPanel)
└── ReticleEditor/         (Reticle editor application)
```

## Next Steps

Per PLAN-InputPanels.md, remaining panels in suggested order:
1. **ParametersPanel** — MaxRange, Step, Angle (+ angle as clicks)
3. **RiflePanel** — SightHeight, ZeroDistance, Rifling, Clicks, VerticalOffset
4. **ZeroAmmoPanel** — Checkbox + embedded AmmoPanel
5. **ZeroAtmospherePanel** — Checkbox + embedded AtmospherePanel
6. **ShotDataPanel** — Container combining all panels
