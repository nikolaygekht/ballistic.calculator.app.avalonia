# BallisticCalculator2 — Main Desktop Application Plan

## Context

All 10 input panels are complete (167 tests passing). The shared controls library has trajectory table, chart, and reticle controls. Now we need the actual desktop application that ties everything together — equivalent to the old WinForms `AppForm` + `TrajectoryForm` + `CompareForm` + `ShotParametersForm`.

The old app used WinForms MDI (Multiple Document Interface) where multiple trajectory windows live inside the main window. We'll replicate this with `iciclecreek.Avalonia.WindowManager` v4.0.0 which provides `WindowsPanel` (MDI container) and `ManagedWindow` (child windows rendered as Avalonia controls, not OS windows).

## Project Structure

```
Desktop/BallisticCalculator/
├── BallisticCalculator.csproj
├── Program.cs
├── App.axaml / App.axaml.cs
├── Models/
│   └── TrajectoryFormState.cs        # Serialization envelope (file format compat with old app)
├── Utilities/
│   ├── TrajectoryCalculator.cs       # Wraps ballistic engine
│   ├── ShotParametersValidator.cs    # Validates ShotData before calculation
│   └── CsvExportController.cs        # Formats trajectory as CSV
├── Views/
│   ├── MainWindow.axaml/.cs          # Menu bar + WindowsPanel + status bar
│   ├── TrajectoryView.axaml/.cs      # UserControl: table/chart/reticle tabs (goes inside ManagedWindow)
│   ├── CompareView.axaml/.cs         # UserControl: multi-trajectory chart (goes inside ManagedWindow)
│   ├── ITrajectoryDisplayView.cs     # Interface for menu state (MeasurementSystem, AngularUnits, etc.)
│   └── Dialogs/
│       └── ShotParametersDialog.axaml/.cs  # Modal window wrapping ShotDataPanel
├── Services/
│   └── FileDialogService.cs          # IFileDialogService impl using Avalonia StorageProvider
Desktop/BallisticCalculator.Tests/
├── TrajectoryCalculatorTests.cs
├── ShotParametersValidatorTests.cs
└── CsvExportControllerTests.cs
```

## Key Architecture Decisions

### MDI via WindowsPanel

```
MainWindow (DockPanel)
├── Menu (Top)
├── StatusBar (Bottom)
└── WindowsPanel (Fill) ← hosts ManagedWindow children
    ├── ManagedWindow [TrajectoryView "Federal 168gr.trajectory"]
    ├── ManagedWindow [TrajectoryView "Hornady 175gr.trajectory"]
    └── ManagedWindow [CompareView "Compare"]
```

### Active Window Tracking

MainWindow tracks `_activeWindow` via ManagedWindow `Activated` events. Menu `UpdateMenus()` reads active window's content to set enable/disable and checkmarks.

### ITrajectoryDisplayView Interface

Both TrajectoryView and CompareView implement this for menu state management:
```csharp
interface ITrajectoryDisplayView
{
    MeasurementSystem MeasurementSystem { get; set; }
    AngularUnit AngularUnits { get; set; }
    DropBase DropBase { get; set; }
    TrajectoryChartMode ChartMode { get; set; }
}
```

Justified: `UpdateMenus()` checks ~15 properties — pattern matching two types everywhere would bloat the code.

### Dialogs as Standard Avalonia Windows

ShotParametersDialog uses regular `Window.ShowDialog<bool?>(owner)`, NOT ManagedWindow. Reasons:
- Blocks entire main window (correct modal behavior)
- Avoids WindowManager's known focus issue (#13)
- Matches ReticleEditor's proven dialog pattern

### Per-Window Display State

Each window independently stores MeasurementSystem, AngularUnits, DropBase, ChartMode. Menu changes apply to active window only (same as old app).

### File Format Compatibility

`TrajectoryFormState` uses same `[BXmlElement]`/`[BXmlProperty]` attributes as old app — files are interchangeable between old and new versions.

```csharp
[BXmlElement("trajectory")]
class TrajectoryFormState {
    ShotData ShotData;
    MeasurementSystem MeasurementSystem;
    AngularUnit AngularUnits;
    TrajectoryChartMode? ChartMode;
}
```

### File Association Per Window

Each TrajectoryView has `string? FileName`. ManagedWindow.Title shows the file name. Save writes to FileName, SaveAs prompts and updates it.

## Menu Structure

**File**: New Imperial, New Metric, Open, Save, Save As, Export CSV, Exit
**View**: Edit Parameters, Measurement System ▸ (Imperial ✓/Metric), Angular Units ▸ (MOA/Mil/MRad/Thousand/In-100yd/Cm-100m), Drop ▸ (Sight Line/Muzzle), Chart ▸ (Velocity/Mach/Drop/Windage/Energy), Chart Zoom Y, Show ▸ (Table/Chart/Reticle), Compare ▸ (Add/Remove Last)
**Windows**: Tile, Cascade
**Help**: About

Dynamic state: View menu items enabled only when trajectory window is active. Checkmarks reflect active window's current settings.

## Implementation Phases

### Phase 1: Skeleton — One Trajectory Window
- Create project, csproj, Program.cs, App.axaml (ClassicTheme + AppFontSize)
- MainWindow: menu bar (File > New/Exit only) + WindowsPanel
- TrajectoryView: TabControl with TrajectoryTableControl + TrajectoryChartControl
- Port TrajectoryCalculator from old `TrajectoryPointsCalculator`
- Port ShotParametersValidator
- ShotParametersDialog wrapping ShotDataPanel
- "New Metric/Imperial": opens dialog → calculates trajectory → shows in ManagedWindow
- Active window tracking, basic menu enable/disable
- Tests: TrajectoryCalculator, Validator

### Phase 2: File I/O
- TrajectoryFormState with BXml serialization
- FileDialogService implementation
- Open, Save, Save As
- Edit Parameters (reopen dialog with current data)
- ManagedWindow titles with file names
- Tests: serialization round-trip

### Phase 3: Display Settings
- Measurement system switching via menu → active window
- Angular unit switching
- Drop base switching
- Chart mode switching
- Menu checkmarks (UpdateMenus)
- Show Table/Chart/Reticle tab switching
- Chart Zoom Y axis

### Phase 4: Compare
- CompareView with TrajectoryChartControl
- Add to Compare: copies active trajectory to CompareView
- Remove Last
- CompareView implements ITrajectoryDisplayView

### Phase 5: Polish
- CSV Export (port CsvExportController)
- Windows > Tile/Cascade (programmatic positioning if library lacks layout methods)
- MainWindow state persistence (position/size)
- About dialog
- Tests: CsvExportController

## Dependencies

```xml
<PackageReference Include="iciclecreek.Avalonia.WindowManager" Version="4.0.0" />
<!-- Transitive: Avalonia.ReactiveUI, IconPacks.Avalonia.Codicons -->
<!-- Plus existing: Avalonia 11.3.12, BallisticCalculator, Gehtsoft.Measurements -->
<ProjectReference Include="BallisticCalculator.Controls" />
<ProjectReference Include="BallisticCalculator.Panels" />
<ProjectReference Include="BallisticCalculator.Types" />
```

## Key Reference Files

| Purpose | File |
|---------|------|
| Menu wiring pattern | Old `BallisticCalculator1/BallisticCalculatorNet/AppForm.cs` |
| TrajectoryView pattern | Old `BallisticCalculatorNet/TrajectoryForm.cs` |
| Serialization model | Old `BallisticCalculatorNet/Utils/TrajectoryFormState.cs` |
| Avalonia menu/dialog pattern | `Desktop/ReticleEditor/Views/MainWindow.axaml.cs` |
| Input panel API | `Common/BallisticCalculator.Panels/Panels/ShotDataPanel.axaml.cs` |

## Known Risks

- **WindowManager focus after dialog close** (#13): mitigated by using standard Avalonia Window for dialogs
- **WindowManager resize glitches** (#8): monitor during development, may need workaround
- **Tile/Cascade**: WindowManager may lack layout methods — implement manually if needed
- **ReactiveUI transitive dep**: present but unused, no impact on our direct-access pattern

## Verification

- Unit tests for TrajectoryCalculator, Validator, CsvExport
- Manual testing: create trajectories, save/open files, switch units, compare
- Cross-test: open files created by old WinForms app in new app and vice versa
