# BallisticCalculator2 — Project Status

Last updated: 2026-07-25

## Overview

Avalonia rewrite of the WinForms BallisticCalculator. Core trajectory math comes from the
**BallisticCalculator 1.1.11.2** NuGet package (+ Gehtsoft.Measurements); the app is action-driven with
direct-UI-access controls (no MVVM/reactive) per `CLAUDE.md`. Trunk-based development (commit to `main`).

## Completed

### Controls Library (`Common/BallisticCalculator.Controls/`)

| Component | Notes |
|-----------|-------|
| MeasurementControl / MeasurementController | Generic measurement input + unit selector; two-path precision (SetValue preserves, Value/ChangeUnit clamp). |
| BallisticCoefficientControl | BC value + drag-table selector (incl. GC/custom). |
| WindDirectionControl / AzimuthDirectionControl | Wind dial (0°=tailwind) and compass dial (0°=North, clockwise, center→target). |
| ReticleCanvasControl | Reticle rendering, Underlay/Overlay collections, **dashed `MovingTargetBox`** aim overlay, AngularToPixel/PixelToAngular. |
| SkiaReticleCanvas | IReticleCanvas impl; renders the 1.1.11.1 **line styles** (Solid/Dashed/Dotted: dashed 4w/3w, dotted w/2w round caps). |
| TrajectoryChartControl / TrajectoryTableControl | ScottPlot chart + DataGrid table (column-width persistence). |
| TrajectoryToReticleCalculator / ReticleOverlayController | BDC mapping; BDC/target overlays; **moving-target lead overlay** + angular-size / lead helpers (`Tools.MovingTargetLead`). |
| SummaryController | Zero adjustments; **bottom- and center-aimed** point-blank spans (`Tools.PointBlankRange`); near/far zero as line-of-sight crossings (independent of the corridor); target size; subsonic distance. |

### Types Library (`Common/BallisticCalculator.Types/`)

| Type | Notes |
|------|-------|
| ShotData / ZeroingData | Shot inputs; ZeroingData holds distance, zero ammo/atmosphere, V/H impact offsets, zeroing wind, shot angle. |
| ZeroingCalculator / ShotTrajectoryCalculator | Single source of truth: build library inputs + trajectory (`Calculate` / `CalculateFine`). Both **thread custom GC drag tables** into the zero and shot calls. |
| BallisticDictionary | **New.** Loads/saves `data/dictionaries.xml` (sight & barrel presets), sorted by name, malformed entries skipped, missing file → empty. |
| CustomDragTableLoader | **New.** Loads/caches a `.drg` for GC ammunition (resolves path or falls back to `data/drg`). |
| CsvTextTableReader | **New.** Two-column CSV → raw text fields. All-or-nothing: only empty lines skipped, optional header on line 1, any other bad line rejects the file; separator chosen by trying `;`/tab/`,`. |
| MeasurementTextParser | **New.** Unit-suffixed value parsing with the aliases the library lacks (`fps`, `mps`, `mph`, `yds`…), decimal-comma normalization, and bare-number fallback units. |
| DragTableBuilder / DrgMetadata | **New.** Builds `GC` tables from a BC-vs-Mach curve or radar readings, validating first (≥1 knot / ≥3 strictly-decreasing readings) and carrying the `.drg` header metadata. |
| DataFolders | **New.** Standard folders next to the exe: `data/reticle`, `data/legacy-ammo`, `data/drg`. |
| MeasurementSystem, ChartTrajectory, DropBase, TrajectoryChartMode | Shared enums/models. |

### Panels Library (`Common/BallisticCalculator.Panels/`)

| Panel | Notes |
|-------|-------|
| AmmoPanel / AmmoLibraryRecordPanel | Ammunition entry + library load/save (default folder `data/legacy-ammo`). **Custom drag table (GC): Browse `.drg` / Clear** row — fills BC=(1,GC)+form factor + weight/diameter; `CustomTableFileName` round-trips. |
| WindPanel / MultiWindPanel / AtmospherePanel | Wind (single/multi) and atmosphere entry. |
| RiflePanel | **Now sight height + H/V clicks + rifling only.** Sight & barrel **dictionary preset dropdowns** (a sight preset fills height/clicks and suggests the zero distance). |
| ZeroPanel | **New.** The Zero tab: zero distance, impact offset (V/H), zeroing shot angle, and the zero ammo/atmosphere/wind sub-panels. |
| ParametersPanel | Max range, step, shot angle; V/H clicks → ShotDrop/WindageAdjustment; Coriolis (azimuth dial + latitude N/S). Azimuth dial height matches the azimuth/latitude block. |
| SummaryPanel | Compact left-aligned readout: zero adj, target size, bottom- & center-aimed dead-zone spans, near/far zero, subsonic. |
| ReticlePanel | BDC + target overlay; **moving-target** section (enable + direction dial synced with a numeric degrees input + speed); shows target angular size + lead in the current angular unit; target size up to 10000; wider (325) data panel. |
| ShotDataPanel | TabControl: **Ammunition / Weather / Wind / Rifle / Zero / Parameters**; assembles the library `Rifle` from Rifle+Zero tabs; `Validate()`. |
| DrgFromBcPanel | **New.** Builds a `.drg` from a BC-vs-Mach curve: knot list + detail, Mach/velocity display toggle (Mach stays canonical), CSV import that can set the base table from the file's `0.462G7`. |
| DrgFromVelocitiesPanel | **New.** Builds a `.drg` from measured downrange velocities: reading list + detail, reused `AtmospherePanel` for the measurement conditions, CSV-unit fallback combos for bare numbers. |

### Main Desktop Application (`Desktop/BallisticCalculator/`)

- Full menu (Trajectory / View / **Tools** / Windows / Help), MDI via `iciclecreek.Avalonia.WindowManager`,
  keyboard shortcuts, persistent state (`appstate.json`).
- **Tools menu:** Approximate Drag Table → From BC Curve / From Measured Velocities — thin `Window` shells
  around the two editor panels (always enabled; prefill bullet + weather from the active window when there
  is one). Edit Sights / Edit Barrels — master-detail dictionary editors that save the merged
  `data/dictionaries.xml` (`SightListEditorDialog` / `BarrelListEditorDialog`).
- `TrajectoryView` tabs: Table, Chart, Reticle, Summary. Coarse display trajectory + one shared **fine**
  trajectory (reticle + summary). `AngularUnits` now also flows to the reticle panel.
- `ShotParametersDialog` (wraps `ShotDataPanel`), `CompareView`, CSV export, About dialog.
- Persistence: `.trajectory` (BXml); `<zeroing>` element with migration of older files.
- **`data/` folder is copied next to the binaries** (csproj `Content` link), so presets/reticles/ammo/drg
  ship with the app; open/save dialogs default to the matching `data/*` subfolder.

### Other Desktop Applications / Tools

| App | Notes |
|-----|-------|
| ReticleEditor | Save / Save As implemented (Save falls back to Save As when unnamed; default folder `data/reticle`); fixed added elements not listing; **Move Up/Down** for elements and path sub-elements; **line-style editing** (Solid/Dashed/Dotted) on line/circle/rectangle/path. |
| DebugApp / DebugApp1 | Controls / panels test harnesses. |
| Tools/DependencyUpdater (`depupdate`) | Bumps PackageReference versions within declared ranges. |

### Test Summary

| Project | Tests | Status |
|---------|------:|--------|
| BallisticCalculator.Controls.Tests | 305 | passing |
| BallisticCalculator.Panels.Tests | 209 | passing |
| ReticleEditor.Tests | 66 | passing |

## Key Design Decisions

### Trajectory calculation — one source of truth
`ShotTrajectoryCalculator` is the only place that turns a `ShotData` into a trajectory. Table/chart use the
coarse display trajectory; reticle + summary share one **fine** trajectory (`CalculateFine`: 2.5 m step,
≥1500 m). GC ("custom") coefficients have no built-in curve, so `CustomDragTableLoader` supplies the `.drg`
`DragTable` to **both** `CalculateZeroParameters` and `Calculate`.

### Rifle vs Zero split
The old combined Rifle tab is split: `RiflePanel` (sight + clicks + rifling) and `ZeroPanel` (zero distance,
impact offset, shot angle, zero ammo/atmosphere/wind). `ShotDataPanel.BuildRifle` assembles the library
`Rifle` from the sight (Rifle tab) + zero distance/offsets (Zero tab) + rifling.

### Dictionary presets
`BallisticDictionary` (`data/dictionaries.xml`) supplies sight and barrel presets, edited via the Tools-menu
editors and consumed by the Rifle-tab dropdowns. A sight preset fills height/clicks and cross-fills the
zero distance; a combo reverts to "(custom)" only when a field no longer matches the preset.

### Reticle line styles (1.1.11.1)
Elements carry a nullable `LineStyle` (Solid/Dashed/Dotted); **null = Solid** for legacy reticles, and the
editor stores Solid as null so Solid elements stay identical to legacy files. `SkiaReticleCanvas` renders
the dash patterns.

### Summary readouts
Near/far zero are computed as line-of-sight crossings, independent of the point-blank corridor (so they no
longer vanish when the corridor can't close). The dead zone is shown as a range span for **both** bottom-
and center-aim, alongside the target size.

### Value formatting — two-path precision
`SetValue<T>()` preserves meaningful precision; `Value`/`ChangeUnit` apply strict DecimalPoints to stop
float noise accumulating through unit conversions.

## File Structure (current, abridged)

```
Common/
├── BallisticCalculator.Controls/ (Controls/, Controllers/, Canvas/, Models/)
├── BallisticCalculator.Panels/   (Panels/, Services/)
├── BallisticCalculator.Types/    (ShotData, ZeroingData, calculators, BallisticDictionary,
│                                   CustomDragTableLoader, DataFolders, enums)
└── *.Tests/
Desktop/
├── BallisticCalculator/ (Models/, Views/ + Views/Dialogs/, Utilities/, Services/, Assets/)
├── DebugApp/, DebugApp1/, ReticleEditor/ (+ ReticleEditor.Tests)
Tools/
└── DependencyUpdater/ (depupdate console tool)
data/  (dictionaries.xml, reticle/, legacy-ammo/, drg/ — copied next to the binaries)
```

## Next Steps

From **`claude/07-25-plan.md`** (1.1.11 `Tools` namespace):
1. ~~Moving target — lead-off aim on the reticle (`Tools.MovingTargetLead`).~~ **Done.**
2. ~~Tools menu → **Approximate DRG table** generation from BCs / from Velocities
   (`DrgDragTableFactory` / `Tools.RadarDragTableFactory`), saved as `.drg`.~~ **Done** — see
   `claude/07-26-drg-plan.md`. Two editors under Tools → Approximate Drag Table, with all-or-nothing CSV
   import and full header metadata. Interactive smoke pass still to do.
3. Tools menu → **Hit probability** (`Tools.HitProbability`). **Pending.**

The original phased plan is archived at `claude/Archive/APP_PLAN.md`.
