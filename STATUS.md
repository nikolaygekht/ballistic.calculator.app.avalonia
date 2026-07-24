# BallisticCalculator2 — Project Status

Last updated: 2026-07-24

## Overview

Avalonia rewrite of the WinForms BallisticCalculator. Core trajectory math comes from the
**BallisticCalculator 1.1.11** NuGet package (+ Gehtsoft.Measurements); the app is action-driven with
direct-UI-access controls (no MVVM/reactive) per `CLAUDE.md`.

## Completed

### Controls Library (`Common/BallisticCalculator.Controls/`)

| Component | Notes |
|-----------|-------|
| MeasurementControl / MeasurementController | Generic measurement input + unit selector; two-path precision (SetValue preserves, Value/ChangeUnit clamp). Tab now uses default value→unit→next navigation. |
| BallisticCoefficientControl | BC value + drag-table selector. |
| WindDirectionControl / WindDirectionController | Wind dial (0°=tailwind, arrow edge→center). |
| AzimuthDirectionControl / AzimuthDirectionController | **New.** Compass dial: 0°=North (up), clockwise, arrow center→target. |
| ReticleCanvasControl | Reticle rendering, Underlay/Overlay collections, AngularToPixel/PixelToAngular. |
| TrajectoryChartControl / ChartController | ScottPlot trajectory chart. |
| TrajectoryTableControl | DataGrid trajectory table + column-width persistence. |
| TrajectoryToReticleCalculator / ReticleOverlayController | BDC point mapping + BDC/target overlays. |
| SummaryController | **New.** Zero adjustments, point-blank dead zone (`Tools.PointBlankRange`, bottom aim), near/far zero, subsonic distance. |

### Types Library (`Common/BallisticCalculator.Types/`)

| Type | Notes |
|------|-------|
| ShotData | Ammunition, Weapon, Atmosphere, Winds, Parameters, **Zeroing (ZeroingData)**. |
| ZeroingData | **New.** All zeroing inputs: distance, zero ammo/atmosphere, V/H impact offsets, zeroing wind, zeroing shot angle. |
| ZeroingCalculator | **New.** Builds library `ZeroingParameters`/`Rifle` from ShotData (`BuildInputs`) and computes the zero (`Compute`). |
| ShotTrajectoryCalculator | **New. Single source of truth** for turning ShotData into a trajectory; `Calculate` (display) + `CalculateFine` (2.5 m step, ≥1500 m). |
| MeasurementSystem, ChartTrajectory, DropBase, TrajectoryChartMode | Shared enums/models. |

### Panels Library (`Common/BallisticCalculator.Panels/`)

| Panel | Notes |
|-------|-------|
| AmmoPanel / AmmoLibraryRecordPanel | Ammunition entry + library load/save (.ammox). |
| WindPanel / MultiWindPanel | Single / multi-wind entry. |
| AtmospherePanel | Altitude, pressure, temperature, humidity. |
| RiflePanel | Sight height, zero distance, H/V clicks, rifling, **impact offset (V + H)**, **zeroing shot angle**. |
| ZeroAmmoPanel / ZeroAtmospherePanel / ZeroWindPanel | Optional zero-condition overrides (ammo / atmosphere / **wind, new**). |
| ParametersPanel | Max range, step, shot angle (degrees); **V/H clicks → ShotDrop/WindageAdjustment**; **Coriolis** (azimuth dial + latitude with N/S). |
| SummaryPanel | **New.** Read-only output table (selectable/copyable): zero adj, dead zone, near/far zero, subsonic. |
| ReticlePanel | Reticle display; BDC (near/far) + target overlay; consumes a provided fine trajectory. |
| ShotDataPanel | TabControl container (Ammo/Weather/Wind/Rifle/Parameters); assembles ZeroingData; `Validate()`. |

### Main Desktop Application (`Desktop/BallisticCalculator/`)

- Full menu (Trajectory / View / Windows / Help), MDI via `iciclecreek.Avalonia.WindowManager`, keyboard
  shortcuts, persistent state (`appstate.json`).
- `TrajectoryView` tabs: **Table, Chart, Reticle, Summary**. Builds the display trajectory (coarse) plus
  one shared **fine** trajectory (via `ShotTrajectoryCalculator.CalculateFine`) handed to the reticle and
  summary.
- `ShotCalculator` = `ApplyDefaults` + delegate to `ShotTrajectoryCalculator`.
- `ShotParametersDialog` (wraps `ShotDataPanel`), `CompareView`, CSV export (local/invariant), About dialog.
- Persistence: `.trajectory` (BXml). Zeroing saved as its own `<zeroing>` element (sight+rifling in
  `<weapon>`); older files where zeroing lived in `<weapon>` are migrated on load.
- Uses the 1.1.11 zeroing API: `CalculateZeroParameters(...)` + `ShotParameters.Apply(...)` (the removed
  `SightAngle`).

### Other Desktop Applications / Tools

| App | Notes |
|-----|-------|
| ReticleEditor | Reticle editor. |
| DebugApp / DebugApp1 | Controls / panels test harnesses. |
| Tools/DependencyUpdater (`depupdate`) | Console tool: bumps PackageReference versions within their declared ranges (respects upper bounds), reading feeds from NuGet.config. `UpdateDeps.bat` launcher. |

### Version policy

Avalonia / SkiaSharp / ScottPlot references carry explicit upper bounds (e.g. Avalonia `[11.3.x,12)`),
so a transitive dependency can never cross a major boundary. `depupdate` enforces/reports this.

### Test Summary

| Project | Tests | Status |
|---------|------:|--------|
| BallisticCalculator.Controls.Tests | 287 | passing |
| BallisticCalculator.Panels.Tests | 195 | passing |
| ReticleEditor.Tests | 66 | passing |

## Key Design Decisions

### Trajectory calculation — one source of truth
`ShotTrajectoryCalculator.Calculate(shotData, stepOverride?, maxDistanceOverride?)` is the only place that
turns a `ShotData` into a trajectory. Table/chart use the coarse display trajectory; the reticle and the
summary analysis share **one fine trajectory** (`CalculateFine`: 2.5 m step, reaching ≥1500 m or the
configured max). The coarse trajectory can't resolve the point-blank corridor, which is why the summary/
reticle need the fine one.

### Zeroing model
`ZeroingData` collects all zeroing inputs in one place, referenced once by `ShotData.Zeroing`. At calc
time `ZeroingCalculator.BuildInputs` produces the library `ZeroingParameters` (distance + ammo/atmosphere
overrides + V/H offsets) plus the zeroing wind and shot angle passed to `CalculateZeroParameters`.

### Parameters: clicks vs shot angle, Coriolis
Shot angle (line-of-sight incline, degrees) is separate from dialed clicks. V/H clicks are converted to
`ShotParameters.ShotDropAdjustment` / `ShotWindageAdjustment` via the sight's click sizes. Azimuth
(compass, 0°=N) + latitude (magnitude + N/S selector) feed `BarrelAzimuth` / `Latitude` for Coriolis,
gated by a checkbox.

### Value formatting — two-path precision
`SetValue<T>()` → `ParseValuePreservePrecision` (keeps meaningful precision, trims zeros). `Value` setter /
`ChangeUnit` → `ParseValue` (strict DecimalPoints) to stop float noise accumulating through conversions.

### File format — .trajectory
`TrajectoryFormState` (BXml) wraps ammo, sight+rifling, `<zeroing>` (ZeroingData), atmosphere, winds,
parameters + display state. `FromShotData`/`ToShotData` bridge runtime ↔ serialized; old-format files
(zeroing inside `<weapon>`) are migrated.

### Defaults (`ShotCalculator.ApplyDefaults`)
Atmosphere: sea-level standard. Rifle: 3″ sight, 100 yd/m zero. Parameters: 1000 yd/m max, 100 yd/m step.

## File Structure (current, abridged)

```
Common/
├── BallisticCalculator.Controls/ (Controls/, Controllers/, Canvas/, Models/)
├── BallisticCalculator.Panels/   (Panels/, Services/)
├── BallisticCalculator.Types/    (ShotData, ZeroingData, ZeroingCalculator, ShotTrajectoryCalculator, enums)
└── *.Tests/
Desktop/
├── BallisticCalculator/ (Models/, Views/ + Views/Dialogs/, Utilities/, Services/, Assets/)
├── DebugApp/, DebugApp1/, ReticleEditor/ (+ ReticleEditor.Tests)
Tools/
└── DependencyUpdater/ (depupdate console tool)
```

## Next Steps

Planned in **`claude/07-25-plan.md`** (implementation 2026-07-25), all on the 1.1.11 `Tools` namespace:
1. Moving target — lead-off aim (dotted target outline on the reticle) via `Tools.MovingTargetLead`.
2. Tools menu → Approximate DRG table from BCs / from Velocities (`DrgDragTableFactory` /
   `Tools.RadarDragTableFactory`), saved as `.drg`.
3. Tools menu → Hit probability (`Tools.HitProbability`).

The original phased plan is archived at `claude/Archive/APP_PLAN.md`.
