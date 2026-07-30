# BallisticCalculator2 — Project Status

Last updated: 2026-07-29

## Overview

Avalonia rewrite of the WinForms BallisticCalculator. Core trajectory math comes from the
**BallisticCalculator 1.1.13** NuGet package (+ Gehtsoft.Measurements 1.1.18); the app is action-driven with
direct-UI-access controls (no MVVM/reactive) per `CLAUDE.md`. Trunk-based development (commit to `main`).

## Completed

### Controls Library (`Common/BallisticCalculator.Controls/`)

| Component | Notes |
|-----------|-------|
| MeasurementControl / MeasurementController | Generic measurement input + unit selector; two-path precision (SetValue preserves, Value/ChangeUnit clamp). Tab walks value → unit → next field (see the focus note below). |
| BallisticCoefficientControl | BC value + drag-table selector (incl. GC/custom); same Tab behaviour. **`AllowCustomTable="False"`** drops GC from the list where a custom curve is meaningless (BC conversion, BC-vs-Mach knots) — and then the control refuses to hold a GC value at all, rather than showing the number under a table it was not quoted against. |
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
| CsvTextTableReader | **New.** Two-column CSV → raw text fields. All-or-nothing: only empty lines skipped, optional header on line 1, any other bad line rejects the whole file quoting that line. Separator chosen by trying `;`/tab/`,` and keeping the one under which *every* row parses; the caller supplies a "why is this row unusable" function so the message says what to fix. |
| MeasurementTextParser | **New.** Unit-suffixed value parsing with the aliases the library lacks (`fps`, `mps`, `mph`, `yds`…) and decimal-comma normalization. A null fallback unit means the text must carry its own unit — what file import uses. |
| HitProbabilityCalculator | **New.** Wraps `Tools.HitProbability` — `ShotData` + `HitProbabilityInputs` → `HitProbabilityEstimate` (probability, impacts, shots-to-hit, mean and 90% radial miss), with the `ShootingPosition` presets (Supported 1/1, Prone 2/2, Kneeling 4/3, Standing 5/4, Custom) and `SampleImpacts` for thinning the plot. Shots bounded 1000…50 000. Carries the shot's geometry but **not** its dialed clicks: the library models the come-up itself, so a pre-dialed scope would count the hold twice. |
| BcConversionCalculator | **New.** Converts a BC between standard tables at a reference velocity, returning the reference Mach and a transonic flag with it — the number is exact only at that reference. Refuses form factors and GC on either side. |
| DragTableBuilder / DrgMetadata | **New.** Builds `GC` tables from a BC-vs-Mach curve or radar readings, validating first (≥1 knot / ≥3 strictly-decreasing readings, positive weight and diameter) with user-facing messages. `NormalizeCurve` converts knots quoted against another standard table at their own Mach. |
| DataFolders | **New.** Standard folders next to the exe: `data/reticle`, `data/ammo`, `data/drg`. |
| MeasurementSystem, ChartTrajectory, DropBase, TrajectoryChartMode | Shared enums/models. |

### Panels Library (`Common/BallisticCalculator.Panels/`)

| Panel | Notes |
|-------|-------|
| AmmoPanel / AmmoLibraryRecordPanel | Ammunition entry + library load/save (default folder `data/ammo`). **Custom drag table (GC): Browse `.drg` / Clear** row — fills BC=(1,GC)+form factor, weight, diameter and **bullet length**, converted to the panel's units (the format stores SI), and fills empty **name/source** from the header; `CustomTableFileName` round-trips. Only positive values are copied, since pre-1.1.11.2 files store the unused slots as 0. |
| WindPanel / MultiWindPanel / AtmospherePanel | Wind (single/multi) and atmosphere entry. |
| RiflePanel | **Now sight height + H/V clicks + rifling only.** Sight & barrel **dictionary preset dropdowns** (a sight preset fills height/clicks and suggests the zero distance). |
| ZeroPanel | **New.** The Zero tab: zero distance, impact offset (V/H), zeroing shot angle, and the zero ammo/atmosphere/wind sub-panels. |
| ParametersPanel | Max range, step, shot angle; V/H clicks → ShotDrop/WindageAdjustment; Coriolis (azimuth dial + latitude N/S). Azimuth dial height matches the azimuth/latitude block. |
| SummaryPanel | Compact left-aligned readout: zero adj, target size, bottom- & center-aimed dead-zone spans, near/far zero, subsonic. |
| ReticlePanel | BDC + target overlay (mark labels carry their unit — `552yd` / `505m`); **moving-target** section (enable + direction dial synced with a numeric degrees input + speed); shows target angular size + lead in the current angular unit; target size up to 10000; wider (325) data panel. The **Mil-Dot** button builds the library's `MilDotReticle` (milliradians, 12 mrad across). |
| ShotDataPanel | TabControl: **Ammunition / Weather / Wind / Rifle / Zero / Parameters**; assembles the library `Rifle` from Rifle+Zero tabs; `Validate()` returns **(shotData, emptyPanels, incompletePanels, problems)**. `problems` collects the named faults from every tab in one pass — `AmmoPanel.Problems()` (missing field, form factor without diameter, unresolvable `.drg`), `ZeroPanel.Problems()` (a ticked override that is not filled in) and `ParametersPanel.Problems()` (clicks with no click size) — so the dialog reports all of them at once instead of one per OK. |
| DrgFromBcPanel | **New.** Builds a `.drg` from a BC-vs-Mach curve. Knots are always keyed by Mach; each knot's coefficient is entered with `BallisticCoefficientControl` (`AllowCustomTable="False"` — a GC knot was already refused on Add, so it is no longer offered) and keeps its own drag table, so a mixed G1/G7 curve is normalized on save (converted at each knot's own Mach, count reported). Load Csv adopts the base table when every coefficient names the same one. |
| DrgFromVelocitiesPanel | **New.** Builds a `.drg` from measured downrange velocities. `Set Atmosphere` (the shell's dialog) sets the air the data was measured in — it drives the recovered drag, and the current conditions are named in the status line. |
| (both editors) | Stacked header (Name / Source / Weight / Diameter / Length, one field per row), a two-column `DataGrid`, an entry row, and Add / Change / Delete / Sort / Load Csv + Save Drg / Close. Editing is explicit: select a row to load it, `Change` writes it back. CSV files must carry their units — a bare number is refused rather than assumed. |
| BcConverterPanel | **New.** Converts a BC between standard tables (G1 ↔ G7) at a reference velocity. **GC is offered on neither side** — the source control sets `AllowCustomTable="False"` and the destination list is `BcConversionCalculator.StandardTables`. Source BC / Destination Table / Reference Velocity → a read-only Target BC that follows the inputs — no Convert button, because the point is watching the answer move with the reference. `Set Atmosphere` (the same shell dialog the velocities editor uses) sets only the speed of sound. Always states the reference Mach and the air; warns below Mach 1.5. |

| HitProbabilityPanel | **New.** Monte-Carlo hit probability for the active shot: target distance + vital zone, group size (1σ per axis), a shooting-position combo that fills two always-editable spread multipliers (`NumericUpDown`), the range/wind estimation errors, the ammunition's MV deviation (a separate group — it is ammo quality, not a shooter error), and Shots/Seed — every plain number is a `NumericUpDown`, though **not** clipped to its bounds (Avalonia's default), so an out-of-range shot count is reported rather than silently rewritten. **Runs on the Estimate button only**: it is cheap enough to run live (~28 ms at 10 000 shots) but the inputs are guesses, and a probability from untouched defaults would imply they had been considered. A shown result persists when inputs change, so two set-ups can be compared. Target distance defaults to 300 yd/m, not the shot's maximum. Shows the probability, shots-to-hit at 50/75/90/95/98%, a ScottPlot impact scatter with the vital zone drawn to scale and **equal axis scaling**, and the mean / 90% radial miss. States that group size is 1σ (≈¼ of extreme spread) and that a correct come-up is assumed. A failed estimate is reported, not thrown (2026-07-30): `Explain` maps `ZeroRangeCantBeReachedException` and `TrajectoryCannotBeCalculatedException` to sentences saying where to fix it, shows the library's own `ArgumentException` message as it stands, and names anything else by type so it can be reported. Before this only `ArgumentException` was caught, so an unzeroable shot reached the app's exception dialog as a stack trace. |

### Main Desktop Application (`Desktop/BallisticCalculator/`)

- Full menu (Trajectory / View / **Tools** / Windows / Help), MDI via `iciclecreek.Avalonia.WindowManager`,
  keyboard shortcuts, persistent state (`appstate.json`).
- **Tools menu:** Approximate Drag Table → From BC Curve / From Measured Velocities — thin scrollable `Window`
  shells around the two editor panels (always enabled), plus `AtmosphereDialog` for the velocities editor. Convert Ballistic Coefficient — the same
  shell pattern around `BcConverterPanel`, sharing that `AtmosphereDialog`. Hit Probability — the only Tools
  entry that reads the active shot (enabled with an active trajectory window only); its title names the shot.
  **The drag table editors and the BC converter deliberately open empty** — they describe a bullet or a
  published coefficient the user is working from, not whatever trajectory happens to be open; only the
  measurement system follows the active window, being a display preference rather than data.
  Edit Sights / Edit Barrels —
  master-detail dictionary editors that save the merged `data/dictionaries.xml`
  (`SightListEditorDialog` / `BarrelListEditorDialog`).
- `TrajectoryView` tabs: Table, Chart, Reticle, Summary. Coarse display trajectory + one shared **fine**
  trajectory (reticle + summary). `AngularUnits` now also flows to the reticle panel.
- `ShotParametersDialog` (wraps `ShotDataPanel`), `CompareView`, CSV export, About dialog.
- **Error reporting.** Two of the engine's failures are named types as of 1.1.13 —
  `ZeroRangeCantBeReachedException` and `TrajectoryCannotBeCalculatedException` — and
  `ShotCalculator.Explain` maps those to a sentence shown in a plain `MessageDialog`: they are the user's to
  fix, and a bad input dressed up with a stack trace reads as a crash. Everything the engine has *not* named
  keeps the stack trace, because that is the thing worth reporting.
- **`ExceptionDialog`** shows what the app was doing, the exception message, and the whole
  exception chain with stack traces in a read-only monospace box, with **Copy**. Every calculation call site
  goes through `ShotCalculator.TryCalculate` (exceptions returned, not thrown) and shows it; so do the
  open and save paths, which used to write to `Console.Error` where nobody saw them. This is the net under
  validation, not a substitute for it — see `claude/07-28.md`.
- Persistence: `.trajectory` (BXml); `<zeroing>` element with migration of older files.
- **`data/` folder is copied next to the binaries** (csproj `Content` link), so presets/reticles/ammo/drg
  ship with the app; open/save dialogs default to the matching `data/*` subfolder.

### Other Desktop Applications / Tools

| App | Notes |
|-----|-------|
| ReticleEditor | Save / Save As implemented (Save falls back to Save As when unnamed; default folder `data/reticle`); fixed added elements not listing; **Move Up/Down** for elements and path sub-elements; **line-style editing** (Solid/Dashed/Dotted) on line/circle/rectangle/path. **Unsaved-changes guard**: a dirty flag set by every element operation, the Set button and any parameter-field edit; New / Open / Close prompt Save / Don't Save / Cancel; the title carries the file name plus `*`. Cleared only by a successful save, so a cancelled picker or failed write still blocks the operation. The dirty check compares field content against a snapshot taken when clean — a load's own control notifications would otherwise read as edits, and timing-based suppression could not tell them apart reliably. |
| DebugApp / DebugApp1 | Controls / panels test harnesses. |
| Tools/DependencyUpdater (`depupdate`) | Bumps PackageReference versions within declared ranges. |

### Test Summary

| Project | Tests | Status |
|---------|------:|--------|
| BallisticCalculator.Controls.Tests | 319 | passing |
| BallisticCalculator.Panels.Tests | 513 | passing |
| BallisticCalculator.Tests (desktop app) | 38 | passing |
| ReticleEditor.Tests | 89 | passing |
| **Total** | **959** | |

Types-layer classes are tested from these suites (there is no separate `Types.Tests`); the four real CSV
exports live in `Panels.Tests/TestData/` so no test depends on a path outside the repo.

## Key Design Decisions

### Trajectory calculation — one source of truth
`ShotTrajectoryCalculator` is the only place that turns a `ShotData` into a trajectory. Table/chart use the
coarse display trajectory; reticle + summary share one **fine** trajectory (`CalculateFine`: 2.5 m step,
≥3000 m). GC ("custom") coefficients have no built-in curve, so `CustomDragTableLoader` supplies the `.drg`
`DragTable` to **both** `CalculateZeroParameters` and `Calculate`.

### Rifle vs Zero split
The old combined Rifle tab is split: `RiflePanel` (sight + clicks + rifling) and `ZeroPanel` (zero distance,
impact offset, shot angle, zero ammo/atmosphere/wind). `ShotDataPanel.BuildRifle` assembles the library
`Rifle` from the sight (Rifle tab) + zero distance/offsets (Zero tab) + rifling.

### Dictionary presets
`BallisticDictionary` (`data/dictionaries.xml`) supplies sight and barrel presets, edited via the Tools-menu
editors and consumed by the Rifle-tab dropdowns. A sight preset fills height/clicks and cross-fills the
zero distance; a combo reverts to "(select)" only when a field no longer matches the preset.

### Reticle line styles (1.1.11.1)
Elements carry a nullable `LineStyle` (Solid/Dashed/Dotted); **null = Solid** for legacy reticles, and the
editor stores Solid as null so Solid elements stay identical to legacy files. `SkiaReticleCanvas` renders
the dash patterns.

### Summary readouts
Near/far zero are computed as line-of-sight crossings, independent of the point-blank corridor (so they no
longer vanish when the corridor can't close). The dead zone is shown as a range span for **both** bottom-
and center-aim, alongside the target size.

### Custom drag tables — one scale, two generators
Every `.drg` holds the projectile's **own drag coefficient** and is run with a **form factor of 1** on table
`GC`. Both generators produce that scale: `RadarDragTableFactory` recovers it from velocity decay, and since
**1.1.11.3** `DrgDragTableFactory.Build` multiplies its `Cd_base(M)/BC(M)` curve by the sectional density (so
bullet weight and diameter are inputs, not documentation). Before that a built table needed a BC *value* of
1.0 and came back 1/SD — about 2.8× — too draggy once saved. A mixed G1/G7 knot set is normalized by
converting each knot at **its own Mach**, which is exact here: the synthesized table is `Cd_base(M)/BC(M)` and
the conversion scales BC by `Cd_target(M)/Cd_source(M)`, so the base-curve factors cancel and every knot lands
on the same Cd either way.

Cross-checking the two generators needs the **right atmosphere**: a published sheet's CD/BC columns are
density-independent aerodynamic values, while its velocity table is raw measurement (the Warner sheets come
from a ~6000 ft range), so at sea level the two disagree by a constant density ratio, not a bug.

### Keyboard focus in composite controls
`MeasurementControl` and `BallisticCoefficientControl` used to forward every bubbling `GotFocus` to their
numeric part, which trapped focus: tabbing to the unit combo bounced straight back, so Tab died at the first
measurement field of a panel. The forwarding is gone (a `UserControl` is not focusable by default, so it never
did what it was for). Headless tests now cover forward and backward navigation through both controls — they
are the only tests here that show a window and simulate keystrokes.

### Value formatting — two-path precision
`SetValue<T>()` preserves meaningful precision; `Value`/`ChangeUnit` apply strict DecimalPoints to stop
float noise accumulating through unit conversions.

## File Structure (current, abridged)

```
Common/
├── BallisticCalculator.Controls/ (Controls/, Controllers/, Canvas/, Models/)
├── BallisticCalculator.Panels/   (Panels/, Services/, Models/)
├── BallisticCalculator.Types/    (ShotData, ZeroingData, calculators, BallisticDictionary,
│                                   CustomDragTableLoader, CsvTextTableReader, MeasurementTextParser,
│                                   DragTableBuilder, DataFolders, enums)
└── *.Tests/                      (Panels.Tests/TestData/ holds the real CSV samples)
Desktop/
├── BallisticCalculator/ (Models/, Views/ + Views/Dialogs/, Utilities/, Services/, Assets/)
├── DebugApp/, DebugApp1/, ReticleEditor/ (+ ReticleEditor.Tests)
Tools/
└── DependencyUpdater/ (depupdate console tool)
data/  (dictionaries.xml, reticle/, ammo/, drg/ — copied next to the binaries)
```

## Next Steps

From **`claude/07-25-plan.md`** (1.1.11 `Tools` namespace):
1. ~~Moving target — lead-off aim on the reticle (`Tools.MovingTargetLead`).~~ **Done.**
2. ~~Tools menu → **Approximate DRG table** generation from BCs / from Velocities
   (`DrgDragTableFactory` / `Tools.RadarDragTableFactory`), saved as `.drg`.~~ **Done** — see
   `claude/Archive/07-26-drg-plan.md`. Two editors under Tools → Approximate Drag Table, with all-or-nothing CSV
   import and full header metadata. Interactive smoke pass still to do.
3. ~~Tools menu → **Hit probability** (`Tools.HitProbability`).~~ **Done** (2026-07-27) — see
   `claude/07-27-hit-probability-plan.md`. Interactive smoke pass still to do. The error-budget defaults
   (range 2%, wind 30%, MV 0.7%) and the 1 MOA group default were **reviewed and accepted** (2026-07-30),
   checked against another ballistic calculator: <https://ptosis.ch/ebalka/ebalka.html>.
4. ~~Tools menu → **BC converter** (`Tools.BallisticCoefficientConverter`).~~ **Done** (2026-07-27).
   `BcConversionCalculator` + `BcConverterPanel` + `BcConverterDialog`: Source BC / Destination Table /
   Reference Velocity → a live read-only Target BC. A converted BC is exact only at its reference — ~1% at
   Mach 1.8–2.5 but ~9% low near Mach 1.3 — so the panel always names the reference Mach, keeps a standing note
   that the number holds only there, and adds a warning below Mach 1.5. The reference band table from the
   original design was dropped in favour of the single Target BC field. Interactive smoke pass still to do.

### Smaller follow-ups

| Item | Where |
|------|-------|
| ~~`data/drg/Lapua/308-lapua n558 11,0g (170gr) naturalis_radar.drg` cannot be opened.~~ **Fixed** (2026-07-30): the header had a period where a comma belonged (`.007830. .03370`). `ShippedDrgLibraryTests` now opens every shipped `.drg` from the source tree, so the next such typo fails a test instead of a hand check. | shipped data |
| A deliberately-broken CSV set to exercise the import rejection paths (the four real samples are all clean). | test data |
| Library wart: with a null `Source`, `Save` writes literal `0` and `Open` reports `"0"`. The app filters it, so nothing is user-visible. | BallisticCalculator.Net |

The original phased plan is archived at `claude/Archive/APP_PLAN.md`.
