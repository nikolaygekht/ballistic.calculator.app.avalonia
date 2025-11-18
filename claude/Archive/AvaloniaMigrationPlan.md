# Avalonia Migration Plan for Ballistic Calculator

## Executive Summary

**Objective:** Migrate WinForms Ballistic Calculator to Avalonia with separate desktop and mobile UIs sharing maximum code

**Strategy:** New Avalonia solution with MVVM architecture, 100% shared ViewModels, adaptive controls, platform-optimized UIs

**Timeline:**
- Desktop App (Core): 314-401 hours (27-35 weeks)
- Desktop App (with QA/Polish): 374-481 hours (33-42 weeks)
- Mobile App: +86-118 hours (8-11 weeks) once desktop is complete

**Approach:** Desktop-first development, no compromises on desktop UX, mobile-ready architecture

**Important Notes:**
- Estimates include core development; add 60-80 hours for comprehensive QA and bug fixes
- Platform-specific installers: +20-30 hours per platform (not included)
- Shared projects must be retargeted from net8.0-windows to net8.0 before Phase 1.2
- Using existing XML serialization (100% backward compatible with WinForms)

---

## Architecture Vision

### Core Principle: Shared Logic, Separate UX

```
┌─────────────────────────────────────────────────────────┐
│                  PRESENTATION LAYER                     │
│              (Platform-Specific, Optimized UX)          │
├──────────────────────────┬──────────────────────────────┤
│  Desktop App             │  Mobile App (Future)         │
│  - Multi-column layouts  │  - Single column layouts     │
│  - Split views           │  - Navigation stack          │
│  - Tabs + splitters      │  - Bottom sheets             │
│  - Keyboard shortcuts    │  - Touch gestures            │
│  - Context menus         │  - Swipe navigation          │
│  - Modal dialogs         │  - Full-screen dialogs       │
└──────────────────────────┴──────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│           SHARED VIEWMODELS & BUSINESS LOGIC            │
│                  (90-95% Shared Core)                   │
│  - Core ViewModels: TrajectoryViewModel, ChartViewModel │
│  - AmmoViewModel, WeaponViewModel, AtmosphereViewModel  │
│  - WindViewModel, CompareViewModel, SettingsViewModel   │
│  - All business rules and calculations (100% shared)    │
│  - Platform-agnostic commands (Calculate, Validate)     │
│  - Validation logic                                     │
│  - Service abstractions (IDialogService, IFileService)  │
│                                                          │
│  ⚠️ Platform-specific coordinators needed for:          │
│    • File pickers (OpenFileCommand implementation)      │
│    • Dialogs (modal vs. navigation in mobile)           │
│    • Navigation (tabs on desktop, stack on mobile)      │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│              SHARED VISUAL COMPONENTS                   │
│                   (80% Shared)                          │
│  - MeasurementControl (adaptive sizing)                 │
│  - ChartControl (ScottPlot wrapper)                     │
│  - ReticleControl (Skia rendering)                      │
│  - Input panels (adaptive layouts)                      │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│         PLATFORM-AGNOSTIC CORE (Reuse from WinForms)    │
│  - BallisticCalculatorNet.Types (100% reuse)            │
│  - BallisticCalculatorNet.Api (95% reuse)               │
│  - BallisticCalculatorNet.Common (100% reuse)           │
│  - BallisticCalculator (external NuGet - no change)     │
│  - Gehtsoft.Measurements (external NuGet - no change)   │
└─────────────────────────────────────────────────────────┘
```

---

## Project Structure

### New Avalonia Solution

```
BallisticCalculatorAvalonia/
├── BallisticCalculatorAvalonia.sln
│
├── Core/ (Platform-Agnostic)
│   ├── BallisticCalculatorNet.Types/              ← REUSE from WinForms
│   │   ├── ITrajectoryDisplayForm.cs
│   │   ├── IChartDisplayForm.cs
│   │   ├── IShotForm.cs
│   │   ├── ShotData.cs
│   │   └── Enums (TrajectoryChartMode, DropBase, etc.)
│   │
│   ├── BallisticCalculatorNet.Api/                ← REUSE from WinForms (verify)
│   │   ├── Configuration/
│   │   ├── Extensions/
│   │   ├── Interop/
│   │   ├── ChartController.cs                     (adapt to ViewModel)
│   │   └── CvsExportController.cs                 (reuse as-is)
│   │
│   ├── BallisticCalculatorNet.Common/             ← REUSE from WinForms
│   │   ├── Utilities/
│   │   ├── DataList/
│   │   └── Serialization/
│   │
│   ├── BallisticCalculator.ViewModels/            ← NEW - 100% SHARED
│   │   ├── ViewModelBase.cs
│   │   ├── MainWindowViewModel.cs
│   │   ├── TrajectoryViewModel.cs
│   │   ├── ChartViewModel.cs
│   │   ├── ReticleViewModel.cs
│   │   ├── CompareViewModel.cs
│   │   ├── AmmoViewModel.cs
│   │   ├── WeaponViewModel.cs
│   │   ├── AtmosphereViewModel.cs
│   │   ├── WindViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   ├── Services/
│   │   │   ├── IDialogService.cs
│   │   │   ├── IFileService.cs
│   │   │   ├── INavigationService.cs
│   │   │   └── ICalculationService.cs
│   │   └── Validators/
│   │       ├── AmmoValidator.cs
│   │       └── WeaponValidator.cs
│   │
│   └── BallisticCalculator.Controls.Shared/       ← NEW - ADAPTIVE CONTROLS
│       ├── MeasurementControl.axaml
│       ├── MeasurementControl.axaml.cs
│       ├── ChartControl.axaml                     (ScottPlot 5 wrapper)
│       ├── ChartControl.axaml.cs
│       ├── ReticleControl.axaml                   (Skia rendering)
│       ├── ReticleControl.axaml.cs
│       ├── Panels/                                (adaptive layouts)
│       │   ├── AmmoInputPanel.axaml
│       │   ├── WeaponInputPanel.axaml
│       │   ├── AtmosphereInputPanel.axaml
│       │   └── WindInputPanel.axaml
│       └── Converters/
│           ├── MeasurementConverter.cs
│           ├── UnitConverter.cs
│           └── AngularUnitConverter.cs
│
├── Desktop/                                        ← NEW - Desktop-Optimized
│   └── BallisticCalculator.Desktop/
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── Program.cs
│       ├── ViewLocator.cs
│       │
│       ├── Views/
│       │   ├── MainWindow.axaml                   (desktop layout)
│       │   ├── MainWindow.axaml.cs
│       │   ├── TrajectoryView.axaml               (split view)
│       │   ├── TrajectoryView.axaml.cs
│       │   ├── CompareView.axaml                  (side-by-side)
│       │   ├── CompareView.axaml.cs
│       │   ├── Dialogs/
│       │   │   ├── ShotParametersDialog.axaml
│       │   │   ├── SettingsDialog.axaml
│       │   │   └── AboutDialog.axaml
│       │   └── ReticleEditor/
│       │       ├── ReticleEditorWindow.axaml
│       │       └── EditDialogs/
│       │
│       ├── Services/
│       │   ├── DesktopDialogService.cs
│       │   ├── DesktopFileService.cs
│       │   ├── DesktopNavigationService.cs
│       │   └── CalculationService.cs
│       │
│       ├── Styles/
│       │   ├── DesktopTheme.axaml
│       │   └── DesktopResources.axaml
│       │
│       └── Assets/
│           ├── Icons/
│           └── Images/
│
├── Mobile/                                         ← FUTURE - Mobile-Optimized
│   └── BallisticCalculator.Mobile/
│       ├── App.axaml                               (different from desktop)
│       ├── App.axaml.cs
│       ├── MobileProgram.cs
│       │
│       ├── Views/
│       │   ├── MainPage.axaml                      (mobile navigation)
│       │   ├── TrajectoryPage.axaml                (single column)
│       │   ├── ComparePage.axaml                   (swipeable)
│       │   └── SettingsPage.axaml
│       │
│       ├── Services/
│       │   ├── MobileDialogService.cs
│       │   ├── MobileFileService.cs
│       │   └── MobileNavigationService.cs
│       │
│       └── Platforms/
│           ├── Android/
│           └── iOS/
│
└── Tests/
    ├── BallisticCalculator.ViewModels.Tests/
    │   ├── TrajectoryViewModelTests.cs
    │   ├── AmmoViewModelTests.cs
    │   └── ValidationTests.cs
    │
    ├── BallisticCalculator.Controls.Tests/
    │   ├── MeasurementControlTests.cs
    │   └── ChartControlTests.cs
    │
    └── BallisticCalculator.Desktop.Tests/
        └── IntegrationTests.cs
```

---

## Technology Stack

### Core Frameworks

- **Avalonia UI:** 11.x (latest stable)
- **.NET:** 8.0 (maintain compatibility with existing libraries)
- **MVVM Framework:** ReactiveUI (or CommunityToolkit.Mvvm)
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection
- **Logging:** Serilog (already used in WinForms)
- **Configuration:** Microsoft.Extensions.Configuration (already used)

### Key NuGet Packages

```xml
<!-- Avalonia -->
<PackageReference Include="Avalonia" Version="11.0.*" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.*" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.*" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.*" />
<PackageReference Include="Avalonia.Diagnostics" Version="11.0.*" />

<!-- MVVM -->
<PackageReference Include="ReactiveUI" Version="19.*" />
<PackageReference Include="ReactiveUI.Fody" Version="19.*" />

<!-- Charting - CRITICAL DEPENDENCY -->
<PackageReference Include="ScottPlot.Avalonia" Version="5.0.*" />

<!-- Drawing -->
<PackageReference Include="Avalonia.Skia" Version="11.0.*" />
<PackageReference Include="SkiaSharp" Version="2.88.*" />

<!-- Existing Dependencies (no change) -->
<PackageReference Include="BallisticCalculator" Version="1.1.7.1" />
<PackageReference Include="Gehtsoft.Measurements" Version="1.1.16" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.*" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.*" />
<PackageReference Include="Serilog" Version="4.*" />
<PackageReference Include="System.Text.Json" Version="9.0.*" />
<PackageReference Include="WatsonTcp" Version="6.0.*" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.6.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Moq" Version="4.20.*" />
```

---

## Migration Phases

### Phase 1: Foundation & Setup (3-4 weeks, 39-51 hours)

**Goal:** Establish project structure, base architecture, and shared foundation

#### Tasks

**1.1 Solution Setup (8-10 hours)**
- [ ] Create new solution `BallisticCalculatorAvalonia.sln`
- [ ] Create project structure (Desktop, ViewModels, Controls.Shared)
- [ ] Configure project references
- [ ] Set up .NET 8.0 targets
- [ ] Initialize Git repository

**1.1.5 Retarget Shared Projects (4-6 hours)** ⚠️ **CRITICAL - BLOCKING**
- [ ] Retarget BallisticCalculatorNet.Types to net8.0 (remove -windows)
- [ ] Retarget BallisticCalculatorNet.Common to net8.0
- [ ] Multi-target BallisticCalculatorNet.Api to net8.0 and net8.0-windows
  - Keep Windows-specific APIs under conditional compilation if needed
- [ ] Remove System.Windows.Forms references from Types/Common
- [ ] Verify builds on Windows, macOS, Linux
- [ ] Update tests to target net8.0
- [ ] **Block:** Cannot proceed to Phase 1.2 without completing this step

**1.2 Link Existing Projects (2-3 hours)**
- [ ] Add BallisticCalculatorNet.Types project (as project reference)
- [ ] Add BallisticCalculatorNet.Api project (as project reference)
- [ ] Add BallisticCalculatorNet.Common project (as project reference)
- [ ] Verify cross-platform compatibility
- [ ] Test builds
- [ ] Run quick WatsonTcp cross-platform spike (verify TCP stack works on Linux/macOS)

**1.3 MVVM Infrastructure (10-12 hours)**
- [ ] Install ReactiveUI packages
- [ ] Create `ViewModelBase` class
  ```csharp
  public class ViewModelBase : ReactiveObject, IActivatableViewModel
  {
      public ViewModelActivator Activator { get; }

      protected ViewModelBase()
      {
          Activator = new ViewModelActivator();
      }
  }
  ```
- [ ] Create `ViewLocator` for view resolution
- [ ] Set up property change notifications
- [ ] Create base command helpers

**1.4 Dependency Injection (8-10 hours)**
- [ ] Configure `Microsoft.Extensions.DependencyInjection`
- [ ] Create service registration
- [ ] Define service interfaces:
  - `IDialogService`
  - `IFileService`
  - `INavigationService`
  - `ICalculationService`
  - `IConfigurationService`
- [ ] Implement desktop service stubs
- [ ] Set up service lifetime scopes

**1.5 Base Desktop Application (7-10 hours)**
- [ ] Create Desktop project (Avalonia.Desktop)
- [ ] Set up `Program.cs` with DI
- [ ] Create `App.axaml` and `App.axaml.cs`
- [ ] Create `MainWindow.axaml` (minimal skeleton)
- [ ] Configure application lifecycle
- [ ] Test application startup

**Deliverables:**
- ✅ Compiling solution with proper structure
- ✅ Desktop app launches with empty window
- ✅ DI container configured and working
- ✅ ReactiveUI integrated
- ✅ Service infrastructure in place

---

### Phase 2: Shared ViewModels (4-5 weeks, 45-55 hours)

**Goal:** Create all ViewModels containing business logic (100% shared between platforms)

#### Priority Order

**2.1 SettingsViewModel (6-8 hours)**
- Foundation for measurement system and units
- [ ] Create `SettingsViewModel`
- [ ] Properties:
  - `MeasurementSystem` (Imperial/Metric)
  - `AngularUnit` (MOA, Mil, etc.)
  - `DropBase` (SightLine/MuzzlePoint)
- [ ] Persistence logic
- [ ] Unit conversion coordination
- [ ] Unit tests

**2.2 AmmoViewModel (8-10 hours)**
- [ ] Create `AmmoViewModel`
- [ ] Properties:
  - `BulletWeight` (Measurement<WeightUnit>)
  - `BallisticCoefficient` (BallisticCoefficient)
  - `MuzzleVelocity` (Measurement<VelocityUnit>)
  - `BulletLength` (Measurement<DistanceUnit>)
  - `Name`, `Manufacturer`, `Caliber`
- [ ] Validation rules
- [ ] `ToAmmunition()` conversion method
- [ ] Load custom ballistic file command
- [ ] Unit tests

**2.3 AtmosphereViewModel (5-7 hours)**
- [ ] Create `AtmosphereViewModel`
- [ ] Properties:
  - `Temperature` (Measurement<TemperatureUnit>)
  - `Pressure` (Measurement<PressureUnit>)
  - `Humidity` (percentage)
  - `Altitude` (Measurement<DistanceUnit>)
- [ ] Altitude/pressure calculation
- [ ] Validation rules
- [ ] Unit tests

**2.4 WeaponViewModel (8-10 hours)**
- [ ] Create `WeaponViewModel`
- [ ] Properties:
  - `TwistRate` (Measurement<DistanceUnit>)
  - `SightHeight` (Measurement<DistanceUnit>)
  - `ZeroDistance` (Measurement<DistanceUnit>)
  - `SightAngle` (Measurement<AngularUnit>)
  - `CantAngle` (Measurement<AngularUnit>)
- [ ] Weapon library integration
- [ ] Validation rules
- [ ] Unit tests

**2.5 WindViewModel (6-8 hours)**
- [ ] Create `WindViewModel`
- [ ] Single wind support
- [ ] Multi-wind zone support (collection)
- [ ] Wind vector calculations
- [ ] Validation rules
- [ ] Unit tests

**2.6 TrajectoryViewModel (10-12 hours)** ⚠️ **CORE COMPONENT**
- [ ] Create `TrajectoryViewModel`
- [ ] Aggregate child ViewModels:
  - `AmmoViewModel`
  - `AtmosphereViewModel`
  - `WeaponViewModel`
  - `WindViewModel`
- [ ] `CalculateTrajectoryCommand` (ReactiveCommand)
- [ ] Integration with `BallisticCalculator` library
- [ ] `TrajectoryPoints` observable collection
- [ ] Validation coordination
- [ ] Error handling
- [ ] Unit tests

**2.7 ChartViewModel (8-10 hours)**
- [ ] Create `ChartViewModel`
- [ ] Properties:
  - `ChartMode` (Velocity, Drop, Windage, Energy, Mach)
  - `XAxisData`, `YAxisData` (series)
  - `XAxisLabel`, `YAxisLabel`
  - `SeriesCollection`
- [ ] Reference `ChartController.cs` from WinForms
- [ ] Data transformation logic
- [ ] Unit conversion for display
- [ ] Multi-series support
- [ ] Unit tests

**2.8 ReticleViewModel (5-7 hours)**
- [ ] Create `ReticleViewModel`
- [ ] Properties:
  - `Reticle` (from BallisticCalculator)
  - `Trajectory` (for holdover)
  - `ZoomLevel`
  - `ShowHoldover`, `ShowPOI`
- [ ] Coordinate calculations
- [ ] Unit tests

**2.9 CompareViewModel (6-8 hours)**
- [ ] Create `CompareViewModel`
- [ ] Manage multiple `TrajectoryViewModel` instances
- [ ] `AddTrajectoryCommand`, `RemoveTrajectoryCommand`
- [ ] Multi-series chart data aggregation
- [ ] Unit tests

**Deliverables:**
- ✅ Core ViewModels with pure business logic (90-95% shared)
- ✅ Service interfaces defined (IDialogService, IFileService, INavigationService)
- ✅ Platform-agnostic commands (Calculate, Validate, unit switching)
- ✅ Business logic complete and testable
- ✅ Unit tests with >80% coverage for business logic
- ✅ No UI dependencies in ViewModels
- ⚠️ Platform-specific command implementations (file open/save, navigation) deferred to Phase 4 (desktop) / Phase 8 (mobile)

---

### Phase 3: Shared Controls (6-8 weeks, 70-90 hours)

**Goal:** Create reusable Avalonia controls that work on both platforms

**Note:** Estimates increased to account for accessibility, culture support, zoom/pan, and platform testing that existed in WinForms versions.

#### 3.1 MeasurementControl (Week 1-2, 15-20 hours) ⚠️ **FOUNDATION**

**Reference:** WinForms `MeasurementControl.cs` and `MeasurmentControlController.cs`

**Tasks:**
- [ ] Create `MeasurementControl.axaml`
  ```xml
  <UserControl xmlns="https://github.com/avaloniaui"
               xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel Orientation="Horizontal" Spacing="5">
      <TextBox Text="{Binding NumericValue}"
               Width="100"
               Watermark="0.0" />
      <ComboBox ItemsSource="{Binding AvailableUnits}"
                SelectedItem="{Binding SelectedUnit}"
                Width="80" />
    </StackPanel>
  </UserControl>
  ```
- [ ] Create `MeasurementControlViewModel`
  - Or adapt `MeasurmentControlController` to ViewModel
- [ ] Implement binding to `Measurement<T>` types
- [ ] Unit conversion logic
- [ ] Validation (min/max, increment)
- [ ] Keyboard support (Up/Down arrow for increment)
- [ ] Culture-aware number parsing
- [ ] Tests

**Key Features:**
- Generic type support
- Real-time unit conversion
- Validation feedback
- Increment/decrement buttons (optional)

#### 3.2 ChartControl (Week 3-4, 25-30 hours) ⚠️ **CRITICAL**

**Reference:** WinForms `ChartControl.cs`, but use ScottPlot 5.x API

**Tasks:**
- [ ] Research ScottPlot 5.x for Avalonia
- [ ] Install `ScottPlot.Avalonia` package
- [ ] Create `ChartControl.axaml`
  ```xml
  <UserControl xmlns="https://github.com/avaloniaui"
               xmlns:scottplot="using:ScottPlot.Avalonia">
    <scottplot:AvaPlot x:Name="Plot" />
  </UserControl>
  ```
- [ ] Create code-behind binding to `ChartViewModel`
- [ ] Implement multi-series support
- [ ] Axis configuration (labels, limits)
- [ ] Legend support
- [ ] Zoom/pan functionality
- [ ] Export chart image capability
- [ ] **Additional quality features (+5-10 hours):**
  - [ ] Accessibility support (screen reader compatibility)
  - [ ] Culture-aware number formatting in axes
  - [ ] Touch gesture support for mobile (pinch zoom, pan)
  - [ ] Multiple export formats (PNG, SVG)
  - [ ] Responsive legend layout
- [ ] Tests

**ScottPlot 5.x API Example:**
```csharp
public void UpdateChart(ChartViewModel viewModel)
{
    Plot.Plot.Clear();

    foreach (var series in viewModel.SeriesCollection)
    {
        var scatter = Plot.Plot.Add.Scatter(series.X, series.Y);
        scatter.Label = series.Name;
        scatter.Color = ConvertColor(series.Color);
        scatter.LineWidth = 2;
    }

    Plot.Plot.Axes.Bottom.Label.Text = viewModel.XAxisLabel;
    Plot.Plot.Axes.Left.Label.Text = viewModel.YAxisLabel;

    if (viewModel.ShowLegend)
        Plot.Plot.ShowLegend();

    Plot.Plot.Axes.AutoScale();
    Plot.Refresh();
}
```

**Migration Note:** ScottPlot 4.x → 5.x has significant API changes. Budget extra time for learning.

#### 3.3 ReticleControl (Week 5-6, 20-25 hours)

**Reference:** WinForms `ReticleControl.cs` and `BallisticCalculatorNet.ReticleCanvas`

**Tasks:**
- [ ] Install SkiaSharp for Avalonia
- [ ] Create `ReticleControl.axaml`
  ```xml
  <UserControl xmlns="https://github.com/avaloniaui">
    <Panel>
      <skia:SKXamlCanvas PaintSurface="OnPaintReticle" />
    </Panel>
  </UserControl>
  ```
- [ ] Implement Skia rendering
- [ ] Draw reticle from `Reticle` object
- [ ] Overlay trajectory holdover points
- [ ] Point of impact display
- [ ] Zoom support (pinch/scroll)
- [ ] Pan support
- [ ] Target overlay (optional)
- [ ] **Cross-platform QA (+2-3 hours):**
  - [ ] Test on Windows (Skia rendering)
  - [ ] Test on macOS (Metal backend)
  - [ ] Test on Linux (Skia/OpenGL)
  - [ ] Visual regression checks
- [ ] Tests

**Skia Rendering Example:**
```csharp
private void OnPaintReticle(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    var info = e.Info;

    canvas.Clear(SKColors.White);

    // Draw reticle
    var paint = new SKPaint
    {
        Color = SKColors.Black,
        StrokeWidth = 2,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke
    };

    // Reference ReticleCanvas logic from WinForms
    // Adapt to Skia API
    DrawReticleLines(canvas, paint);
    DrawHoldoverPoints(canvas, paint);
}
```

**Deliverables:**
- ✅ MeasurementControl working with all measurement types
- ✅ ChartControl displaying trajectories correctly (with accessibility and culture support)
- ✅ ReticleControl rendering reticles with overlays (tested on all platforms)
- ✅ All controls testable and documented
- ✅ Basic input panels (Ammo, Weapon, Atmosphere, Wind) with adaptive layouts

---

### Phase 4: Desktop Application (6-7 weeks, 70-85 hours)

**Goal:** Build complete desktop-optimized UI

#### 4.1 Main Window & Navigation (Week 1-2, 20-25 hours)

**Tasks:**
- [ ] Create `MainWindow.axaml` with desktop layout
- [ ] Implement menu system
  ```xml
  <Menu DockPanel.Dock="Top">
    <MenuItem Header="_File">
      <MenuItem Header="_New">
        <MenuItem Header="Imperial" Command="{Binding NewImperialCommand}" />
        <MenuItem Header="Metric" Command="{Binding NewMetricCommand}" />
      </MenuItem>
      <MenuItem Header="_Open..." InputGesture="Ctrl+O"
                Command="{Binding OpenCommand}" />
      <MenuItem Header="_Save" InputGesture="Ctrl+S"
                Command="{Binding SaveCommand}" />
      <MenuItem Header="Save _As..." Command="{Binding SaveAsCommand}" />
      <Separator />
      <MenuItem Header="E_xport CSV..." Command="{Binding ExportCsvCommand}" />
      <Separator />
      <MenuItem Header="E_xit" Command="{Binding ExitCommand}" />
    </MenuItem>
    <MenuItem Header="_View">
      <!-- Measurement system, angular units, chart mode -->
    </MenuItem>
    <MenuItem Header="_Tools">
      <MenuItem Header="Reticle Editor..." />
      <MenuItem Header="Settings..." />
    </MenuItem>
    <MenuItem Header="_Help">
      <MenuItem Header="About..." />
    </MenuItem>
  </Menu>
  ```
- [ ] Tab-based document management
  ```xml
  <TabControl ItemsSource="{Binding OpenDocuments}"
              SelectedItem="{Binding ActiveDocument}">
    <TabControl.ItemTemplate>
      <DataTemplate>
        <TextBlock Text="{Binding Title}" />
      </DataTemplate>
    </TabControl.ItemTemplate>
  </TabControl>
  ```
- [ ] Keyboard shortcuts implementation
- [ ] Status bar
- [ ] Toolbar (optional)
- [ ] Window state persistence

**Desktop Keyboard Shortcuts:**
- Ctrl+N: New trajectory
- Ctrl+O: Open file
- Ctrl+S: Save file
- Ctrl+W: Close tab
- Ctrl+E: Export CSV
- F5: Recalculate
- Ctrl+Tab: Switch tabs

#### 4.2 Input Panels (Week 2-3, 20-25 hours)

**Desktop-optimized multi-column layouts**

**Tasks:**
- [ ] Create `AmmoInputPanel.axaml`
  ```xml
  <UserControl>
    <Grid ColumnDefinitions="Auto,200,20,Auto,200" RowDefinitions="Auto,Auto,Auto,Auto">
      <!-- Desktop: 2 columns, compact -->
      <TextBlock Grid.Row="0" Grid.Column="0">Weight:</TextBlock>
      <controls:MeasurementControl Grid.Row="0" Grid.Column="1"
                                    Value="{Binding BulletWeight}" />

      <TextBlock Grid.Row="0" Grid.Column="3">BC:</TextBlock>
      <controls:MeasurementControl Grid.Row="0" Grid.Column="4"
                                    Value="{Binding BallisticCoefficient}" />

      <TextBlock Grid.Row="1" Grid.Column="0">Velocity:</TextBlock>
      <controls:MeasurementControl Grid.Row="1" Grid.Column="1"
                                    Value="{Binding MuzzleVelocity}" />
      <!-- ... more fields -->
    </Grid>
  </UserControl>
  ```
- [ ] Create `WeaponInputPanel.axaml` (2-column layout)
- [ ] Create `AtmosphereInputPanel.axaml` (2-column layout)
- [ ] Create `WindInputPanel.axaml`
- [ ] Create `MultiWindInputPanel.axaml`
- [ ] Data binding to ViewModels
- [ ] Validation feedback (error display)

#### 4.3 TrajectoryView (Week 4-5, 25-30 hours) ⚠️ **MOST COMPLEX**

**Desktop Layout: Parameters (top) + Table (40%) + Chart/Reticle Tabs (60%)**

**Tasks:**
- [ ] Create `TrajectoryView.axaml`
  ```xml
  <UserControl>
    <DockPanel>
      <!-- Parameters (collapsible) -->
      <Expander DockPanel.Dock="Top" IsExpanded="True" Header="Parameters">
        <Grid ColumnDefinitions="*,*,*">
          <panels:AmmoInputPanel Grid.Column="0" DataContext="{Binding AmmoVM}" />
          <panels:AtmosphereInputPanel Grid.Column="1" DataContext="{Binding AtmosphereVM}" />
          <panels:WeaponInputPanel Grid.Column="2" DataContext="{Binding WeaponVM}" />
        </Grid>
      </Expander>

      <!-- Calculate button -->
      <Button DockPanel.Dock="Top" Command="{Binding CalculateCommand}"
              Content="Calculate Trajectory" HorizontalAlignment="Center" />

      <!-- Results split view -->
      <Grid RowDefinitions="2*,5,3*">
        <!-- Trajectory Table (top 40%) -->
        <DataGrid Grid.Row="0" ItemsSource="{Binding TrajectoryPoints}"
                  IsReadOnly="True">
          <DataGrid.Columns>
            <DataGridTextColumn Header="Distance" Binding="{Binding Distance}" />
            <DataGridTextColumn Header="Drop" Binding="{Binding Drop}" />
            <DataGridTextColumn Header="Windage" Binding="{Binding Windage}" />
            <DataGridTextColumn Header="Velocity" Binding="{Binding Velocity}" />
            <DataGridTextColumn Header="Energy" Binding="{Binding Energy}" />
            <DataGridTextColumn Header="Time" Binding="{Binding Time}" />
          </DataGrid.Columns>
        </DataGrid>

        <!-- Splitter -->
        <GridSplitter Grid.Row="1" ResizeDirection="Rows" />

        <!-- Chart/Reticle tabs (bottom 60%) -->
        <TabControl Grid.Row="2">
          <TabItem Header="Chart">
            <controls:ChartControl DataContext="{Binding ChartVM}" />
          </TabItem>
          <TabItem Header="Reticle">
            <controls:ReticleControl DataContext="{Binding ReticleVM}" />
          </TabItem>
        </TabControl>
      </Grid>
    </DockPanel>
  </UserControl>
  ```
- [ ] Bind to `TrajectoryViewModel`
- [ ] Implement splitter functionality
- [ ] Context menu for table (copy, export)
- [ ] Right-click chart for zoom options

#### 4.4 CompareView (Week 6, 15-20 hours)

**Tasks:**
- [ ] Create `CompareView.axaml`
- [ ] Multi-trajectory chart display
- [ ] Add/remove trajectory controls
- [ ] Legend management
- [ ] Bind to `CompareViewModel`

#### 4.5 Dialogs (Week 7, 10-15 hours)

**Tasks:**
- [ ] Create `ShotParametersDialog.axaml` (modal input)
- [ ] Create `SettingsDialog.axaml`
- [ ] Create `AboutDialog.axaml`
- [ ] Implement `DesktopDialogService`
- [ ] File open/save dialogs via Avalonia APIs

**Deliverables:**
- ✅ Fully functional desktop application
- ✅ All views implemented and tested
- ✅ Desktop-optimized UX
- ✅ Keyboard shortcuts working
- ✅ Menu system complete

---

### Phase 5: File Operations & Extensions (2-3 weeks, 25-35 hours)

**Goal:** Complete application features

**Note:** Using existing `BallisticXmlSerializer`/`BallisticXmlDeserializer` from BallisticCalculator NuGet package (v1.1.7.1). This provides 100% backward compatibility with WinForms `.trajectory` files at zero development cost.

#### 5.1 File Operations (15-20 hours)

**Tasks:**
- [ ] Implement `DesktopFileService`
- [ ] XML serialization integration (reuse from BallisticCalculator NuGet)
  - [ ] Reference: `BallisticXmlSerializer` / `BallisticXmlDeserializer` (already cross-platform)
  - [ ] Use existing `TrajectoryFormState` class for serialization
- [ ] Open trajectory file
- [ ] Save trajectory file
- [ ] Save As functionality
- [ ] Recent files list
- [ ] Auto-save (optional)
- [ ] CSV export
  - Reference: `CvsExportController.cs`
- [ ] File association (.trajectory files)
- [ ] **File Format Validation (2-3 hours):**
  - [ ] Test loading existing .trajectory files from WinForms app
  - [ ] Verify round-trip serialization (load → save → reload)
  - [ ] Test with complex scenarios (multi-wind zones, custom BC files, various unit systems)
  - [ ] Verify error handling for corrupted/invalid files
  - [ ] Confirm seamless interoperability between WinForms and Avalonia versions

#### 5.2 Extension System (8-12 hours)

**Tasks:**
- [ ] Validate WatsonTcp cross-platform compatibility
- [ ] Port `InteropServer.cs` to Avalonia
- [ ] Port `ExtensionsManager.cs`
- [ ] Menu integration for extensions
- [ ] Test with PDF export extension

#### 5.3 Configuration (2-3 hours)

**Tasks:**
- [ ] Settings persistence
- [ ] Window state (size, position, splitter positions)
- [ ] User preferences (default units, theme)
- [ ] Configuration migration from WinForms (optional)

**Deliverables:**
- ✅ File operations working
- ✅ 100% compatibility with WinForms .trajectory files verified
- ✅ Extension system functional (WatsonTcp cross-platform tested)
- ✅ Configuration persistence

---

### Phase 6: ReticleEditor (3-4 weeks, 30-40 hours)

**Goal:** Port standalone reticle editor application

**Reference:** `BallisticCalculatorNet.ReticleEditor` project (24 files, 665-line main form)

#### Tasks

**6.1 Main Editor Window (15-20 hours)**
- [ ] Create `ReticleEditorWindow.axaml`
- [ ] Port main form layout
- [ ] Drawing tools integration
- [ ] Reticle preview using `ReticleControl`
- [ ] File operations (open, save reticle)

**6.2 Edit Dialogs (15-20 hours)**
- [ ] Port 8 edit dialog forms:
  - Reticle properties
  - Line edit
  - Circle edit
  - Rectangle edit
  - Bullet drop compensator (BDC) edit
  - Range finder edit
  - Text edit
  - Image edit
- [ ] Data binding to ViewModels
- [ ] Validation

**Deliverables:**
- ✅ Functional reticle editor
- ✅ All editing features ported
- ✅ Compatible file format

---

### Phase 7: Testing & Polish (3-4 weeks, 35-45 hours)

**Goal:** Production-ready desktop application

#### 7.1 Testing (20-25 hours)

**Unit Tests:**
- [ ] ViewModel tests (target >80% coverage)
- [ ] Business logic tests
- [ ] Validation tests
- [ ] Calculation tests

**Integration Tests:**
- [ ] File operations
- [ ] Extension system
- [ ] Configuration

**UI Tests:**
- [ ] Manual testing checklist
- [ ] Smoke tests for all views
- [ ] Cross-platform testing (Windows, macOS, Linux)

**Reference WinForms Tests:**
- `BallisticCalculatorNet.UnitTest` project patterns

#### 7.2 Polish (15-20 hours)

**Styling:**
- [ ] Apply FluentTheme consistently
- [ ] Custom styles for special controls
- [ ] Dark mode support (optional)
- [ ] Consistent spacing and alignment

**Resources:**
- [ ] Application icon
- [ ] Menu icons
- [ ] Splash screen (optional)

**Performance:**
- [ ] Profile chart rendering
- [ ] Optimize trajectory calculation (async if needed)
- [ ] Memory leak checks

**Documentation:**
- [ ] User guide (optional)
- [ ] Developer documentation
- [ ] Code comments and XML docs

**Installer:**
- [ ] Windows installer (optional)
- [ ] macOS bundle (optional)
- [ ] Linux package (optional)

**Deliverables:**
- ✅ Production-quality desktop application
- ✅ Comprehensive test coverage
- ✅ Professional appearance
- ✅ Cross-platform tested

---

## Phase 8-9: Mobile App (FUTURE, 8-11 weeks, 80-110 hours)

**Goal:** Create mobile-optimized UI reusing ViewModels and controls

**Key Point:** Much faster because ViewModels and core controls are already done!

### Phase 8: Mobile UI (6-8 weeks, 60-80 hours)

#### 8.1 Mobile Project Setup (10-12 hours)
- [ ] Create `BallisticCalculator.Mobile` project
- [ ] Configure Android/iOS targets
- [ ] **Implement mobile-specific services (6-8 hours):**
  - [ ] MobileDialogService (bottom sheets, full-screen dialogs)
  - [ ] MobileFileService (platform file pickers, scoped storage on Android)
  - [ ] MobileNavigationService (stack-based navigation, deep linking)
  - [ ] Test service contracts match desktop interfaces
- [ ] Navigation infrastructure

#### 8.2 Mobile Views (40-55 hours)
- [ ] `MainPage.axaml` (mobile navigation)
- [ ] `TrajectoryPage.axaml` (single-column, scrollable)
- [ ] `ComparePage.axaml` (swipeable charts)
- [ ] `SettingsPage.axaml`
- [ ] Bottom sheet dialogs
- [ ] Touch gesture support

#### 8.3 Mobile Adaptations (12-15 hours)
- [ ] Adapt input panels for mobile (single column)
- [ ] Touch-friendly control sizes
- [ ] Mobile-specific navigation patterns
- [ ] File picker integration (platform-specific)

### Phase 9: Mobile Testing & Deployment (2-3 weeks, 20-30 hours)

- [ ] Mobile device testing
- [ ] Performance optimization
- [ ] Battery usage optimization
- [ ] App store preparation
- [ ] Deployment

---

## Technical Migration Details

### ScottPlot 4.x → 5.x Migration

**Critical Changes:**

| ScottPlot 4.x | ScottPlot 5.x |
|---------------|---------------|
| `Plot.AddScatter(x, y, label)` | `Plot.Add.Scatter(x, y).Label = label` |
| `Plot.XAxis.Label(text)` | `Plot.Axes.Bottom.Label.Text = text` |
| `Plot.YAxis.Label(text)` | `Plot.Axes.Left.Label.Text = text` |
| `Plot.AxisAuto()` | `Plot.Axes.AutoScale()` |
| `Plot.Legend()` | `Plot.ShowLegend()` |
| `Plot.Clear()` | `Plot.Clear()` (same) |
| `formsPlot1.Refresh()` | `avaPlot.Refresh()` (same) |

**Installation:**
```bash
dotnet add package ScottPlot.Avalonia --version 5.0.*
```

**Reference:**
- WinForms `ChartControl.cs` for logic
- ScottPlot 5 docs: https://scottplot.net/quickstart/avalonia/

### System.Drawing → Skia Migration

**Reticle Rendering Changes:**

```csharp
// OLD (System.Drawing)
using (var graphics = e.Graphics)
{
    graphics.Clear(Color.White);
    var pen = new Pen(Color.Black, 2);
    graphics.DrawLine(pen, x1, y1, x2, y2);
}

// NEW (Skia)
var canvas = e.Surface.Canvas;
canvas.Clear(SKColors.White);
var paint = new SKPaint
{
    Color = SKColors.Black,
    StrokeWidth = 2,
    IsAntialias = true,
    Style = SKPaintStyle.Stroke
};
canvas.DrawLine(x1, y1, x2, y2, paint);
```

**Color Conversion:**
```csharp
// System.Drawing.Color → SkiaSharp.SKColor
SKColor ToSkiaColor(System.Drawing.Color color)
{
    return new SKColor(color.R, color.G, color.B, color.A);
}
```

### WinForms → Avalonia Control Mapping

| WinForms | Avalonia | Notes |
|----------|----------|-------|
| `Form` | `Window` | Different event lifecycle |
| `UserControl` | `UserControl` | Very similar |
| `TextBox` | `TextBox` | Same |
| `ComboBox` | `ComboBox` | Same |
| `Button` | `Button` | Same |
| `CheckBox` | `CheckBox` | Same |
| `ListView` | `DataGrid` or `ListBox` | DataGrid for tabular data |
| `TabControl` | `TabControl` | Same |
| `MenuStrip` | `Menu` | Different API |
| `TableLayoutPanel` | `Grid` | More powerful in Avalonia |
| `SplitContainer` | `Grid` with `GridSplitter` | Different approach |
| `Panel` | `Panel` or `Canvas` | Multiple options |

### Data Binding Patterns

**WinForms (Manual):**
```csharp
// Manual property update
public MeasurementSystem MeasurementSystem
{
    get => mMeasurementSystem;
    set
    {
        mMeasurementSystem = value;
        UpdateAllControls(); // Manual
    }
}
```

**Avalonia + ReactiveUI (Automatic):**
```csharp
// Automatic with ReactiveUI
private MeasurementSystem _measurementSystem;
public MeasurementSystem MeasurementSystem
{
    get => _measurementSystem;
    set => this.RaiseAndSetIfChanged(ref _measurementSystem, value);
}

// In constructor - reactive subscription
this.WhenAnyValue(x => x.MeasurementSystem)
    .Subscribe(_ => UpdateAllControls());
```

**XAML Binding:**
```xml
<ComboBox ItemsSource="{Binding MeasurementSystems}"
          SelectedItem="{Binding MeasurementSystem}" />
```

---

## Timeline Summary

### Desktop-Only Path

| Phase | Duration | Hours | Cumulative |
|-------|----------|-------|-----------|
| 1. Foundation | 3-4 weeks | 39-51 | 39-51 |
| 2. ViewModels | 4-5 weeks | 45-55 | 84-106 |
| 3. Controls | 6-8 weeks | 70-90 | 154-196 |
| 4. Desktop UI | 6-7 weeks | 70-85 | 224-281 |
| 5. File & Ext | 2-3 weeks | 25-35 | 249-316 |
| 6. ReticleEditor | 3-4 weeks | 30-40 | 279-356 |
| 7. Testing | 3-4 weeks | 35-45 | 314-401 |
| **CORE TOTAL** | **27-35 weeks** | **314-401 hours** | - |
| **+ QA/Polish** | **+5-7 weeks** | **+60-80 hours** | **374-481 hours** |

**Timeline with Different Work Schedules (Core + QA):**
- **10 hours/week:** 38-49 weeks (9-12 months)
- **20 hours/week:** 19-24 weeks (5-6 months)
- **30 hours/week:** 13-16 weeks (3-4 months)
- **40 hours/week:** 9-12 weeks (2-3 months)

### Mobile Addition (After Desktop Complete)

| Phase | Duration | Hours | Cumulative |
|-------|----------|-------|-----------|
| 8. Mobile UI | 6-8 weeks | 66-88 | 66-88 |
| 9. Mobile Test | 2-3 weeks | 20-30 | 86-118 |
| **TOTAL** | **8-11 weeks** | **86-118 hours** | - |

**Total Desktop + Mobile:** 400-519 hours (core) or 460-599 hours (with comprehensive QA)

---

## Risk Mitigation

### High-Risk Areas

**0. Target Framework Retargeting** 🔴 **CRITICAL - MUST DO FIRST**
- **Risk:** Types/Api/Common projects still target net8.0-windows, blocking Avalonia references
- **Impact:** CRITICAL - Blocks all development after Phase 1.1
- **Mitigation:**
  - Dedicate Phase 1.1.5 to retargeting (4-6 hours)
  - Multi-target Api project if Windows-specific APIs needed
  - Test builds on Windows, macOS, Linux before Phase 1.2
  - **Block:** Cannot proceed to Phase 1.2 without completing this

**1. ScottPlot 5.x Migration** 🔴 **CRITICAL**
- **Risk:** API significantly different, learning curve
- **Impact:** HIGH - Charts are core functionality
- **Mitigation:**
  - Prototype early (Phase 3.2)
  - Buffer time already added (+5 hours from original estimate)
  - Alternative: LiveCharts2, OxyPlot if ScottPlot 5.x has issues

**2. System.Drawing → Skia Migration** 🟡 **MEDIUM-HIGH**
- **Risk:** Reticle pipeline heavily uses System.Drawing (Windows-only)
- **Impact:** MEDIUM - Blocks reticle visualization on non-Windows platforms
- **Mitigation:**
  - Prototype Skia rendering early (with Phase 3.3)
  - Test on macOS/Linux immediately after implementation
  - Keep WinForms reference code accessible during development
  - Plan QA time for visual regression testing on each platform (added to Phase 3.3)
  - Profile performance early and optimize drawing code
  - Use caching where possible

**3. WatsonTcp Cross-Platform** 🟡 **MEDIUM**
- **Risk:** May have platform-specific issues (TCP stack differences)
- **Impact:** MEDIUM - Extension system depends on it
- **Mitigation:**
  - Quick spike in Phase 1.2 to verify basic TCP functionality
  - Test thoroughly on macOS/Linux in Phase 5.2
  - Consider alternatives if issues found (gRPC, HTTP/REST, named pipes)
  - Document fallback plan if cross-platform issues discovered

**4. Scope Creep** 🟡
- **Risk:** Adding features not in WinForms version
- **Mitigation:**
  - Strict feature parity focus
  - Defer enhancements to post-release

### Medium-Risk Areas

**1. ReactiveUI Learning Curve** 🟡
- **Mitigation:** Invest time in Phase 1, use examples

**2. Cross-Platform Quirks** 🟡
- **Mitigation:** Regular testing on all platforms

**3. File Dialog Differences** 🟡
- **Mitigation:** Abstract via `IFileService`, test early

---

## Success Criteria

### Must Have (MVP)

✅ **Functional Parity with WinForms:**
- All calculation features
- All visualization modes
- File operations
- Measurement system switching
- Extension system

✅ **Desktop UX Quality:**
- Multi-column layouts
- Split views
- Keyboard shortcuts
- Professional appearance

✅ **Architecture Quality:**
- MVVM with ReactiveUI
- >80% ViewModel test coverage
- Clean separation of concerns
- Cross-platform (Windows, macOS, Linux)

### Should Have

✅ ReticleEditor ported
✅ Configuration persistence
✅ Recent files
✅ Context menus

### Nice to Have (Future)

🔄 Mobile app
🔄 Dark mode
🔄 Advanced charts (more series types)
🔄 Cloud sync
🔄 Internationalization

---

## Development Recommendations

### Phase Execution Strategy

1. **Complete phases sequentially** - Don't skip ahead
2. **Test thoroughly at each phase** - Catch issues early
3. **Prototype risky components first** - ScottPlot, Skia
4. **Maintain WinForms reference** - Don't delete, keep for reference
5. **Commit frequently** - Small, focused commits

### Code Quality Standards

- **Follow Avalonia conventions** - XAML styling, naming
- **Use ReactiveUI patterns** - `WhenAnyValue`, `ReactiveCommand`
- **Write tests first for ViewModels** - TDD approach
- **Document complex logic** - XML comments, README files
- **Code reviews** - If team, review all major components

### Testing Strategy

**Unit Tests (Phase 2 onwards):**
- Every ViewModel must have tests
- Validation logic must be tested
- Business logic must be tested
- Target: >80% coverage

**Integration Tests (Phase 5 onwards):**
- File operations
- Extension system
- End-to-end scenarios

**Manual Testing (Every phase):**
- Smoke test after each feature
- Cross-platform testing weekly
- Performance profiling (Phase 7)

---

## Migration Checklist

### Pre-Migration

**Development Environment:**
- [ ] .NET 8.0 SDK (8.0.x or later) installed
- [ ] IDE: JetBrains Rider 2024.x OR Visual Studio 2022 v17.8+
- [ ] Avalonia for VS extension (if using Visual Studio)
- [ ] Git 2.40+ installed
- [ ] Optional: Avalonia UI Designer preview extension

**Learning Resources (Budget 8-12 hours):**
- [ ] Read `ProjectReview.md` thoroughly (1 hour)
- [ ] Complete Avalonia "Music Store" tutorial (4-6 hours)
- [ ] Read ReactiveUI handbook sections on ViewModels and Commands (2-3 hours)
- [ ] Review ScottPlot 5.x Avalonia quickstart (1 hour)
- [ ] Review SkiaSharp drawing basics (1 hour)

**WinForms Reference Project:**
- [ ] Clone or access `/mnt/d/develop/homeapps/BallisticCalculator1`
- [ ] Build and run to understand functionality
- [ ] Keep solution open for reference (do not modify)

### Phase Checkpoints

After each phase, verify:
- [ ] All tasks completed
- [ ] Tests passing (if applicable)
- [ ] Code committed to Git
- [ ] No blocking issues
- [ ] Phase deliverables achieved
- [ ] Ready to proceed to next phase

### Final Release Checklist

- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] Manual testing complete on all platforms
- [ ] Performance acceptable
- [ ] No memory leaks
- [ ] Documentation updated
- [ ] Version number set
- [ ] Installer built (if applicable)
- [ ] Release notes written

---

## Resources

### External Dependencies (Local Paths)

**Core Ballistics Engine:**
- **BallisticCalculator** (NuGet)
  - Source: `/mnt/d/develop/components/BusinessSpecificComponents/BallisticCalculator.Net`
  - NuGet Version: 1.1.7.1
  - Purpose: Physics-based trajectory calculation
  - Status: Reuse as-is (cross-platform compatible)

**Measurement System:**
- **Gehtsoft.Measurements** (NuGet)
  - Source: `/mnt/d/develop/components/BusinessSpecificComponents/Gehtsoft.Measurements`
  - NuGet Version: 1.1.16
  - Purpose: Unit conversion and measurement types
  - Status: Reuse as-is (cross-platform compatible)

**WinForms Reference Project:**
- **BallisticCalculator1** (Reference only - do not modify)
  - Path: `/mnt/d/develop/homeapps/BallisticCalculator1`
  - Solution: `BallisticCalculator1.sln`
  - Git Branch: main
  - Last Commit: `a559797 Fix wind control behavior, add a few new ammo profiles`
  - Use for: Business logic patterns, control behavior reference, data structures

### Avalonia & UI Framework Documentation

- **Avalonia UI:** https://docs.avaloniaui.net/
  - Getting Started: https://docs.avaloniaui.net/docs/getting-started
  - Tutorials: https://docs.avaloniaui.net/docs/tutorials/
  - XAML Reference: https://docs.avaloniaui.net/docs/basics/user-interface/xaml
  - Data Binding: https://docs.avaloniaui.net/docs/basics/data/data-binding
  - Styling: https://docs.avaloniaui.net/docs/styling/

- **Avalonia GitHub:** https://github.com/AvaloniaUI/Avalonia
  - Source Code: https://github.com/AvaloniaUI/Avalonia
  - Issues: https://github.com/AvaloniaUI/Avalonia/issues
  - Releases: https://github.com/AvaloniaUI/Avalonia/releases

### MVVM Framework Documentation

- **ReactiveUI:** https://www.reactiveui.net/docs/
  - Getting Started: https://www.reactiveui.net/docs/getting-started/
  - Handbook: https://www.reactiveui.net/docs/handbook/
  - View Models: https://www.reactiveui.net/docs/handbook/view-models/
  - Commands: https://www.reactiveui.net/docs/handbook/commands/
  - Data Binding: https://www.reactiveui.net/docs/handbook/data-binding/

- **ReactiveUI GitHub:** https://github.com/reactiveui/ReactiveUI
  - Samples: https://github.com/reactiveui/ReactiveUI.Samples

- **Alternative: CommunityToolkit.Mvvm**
  - Docs: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
  - GitHub: https://github.com/CommunityToolkit/dotnet
  - NuGet: https://www.nuget.org/packages/CommunityToolkit.Mvvm

### Charting Library Documentation

- **ScottPlot 5.x** (Avalonia Support)
  - Website: https://scottplot.net/
  - Avalonia Quickstart: https://scottplot.net/quickstart/avalonia/
  - Cookbook: https://scottplot.net/cookbook/5.0/
  - GitHub: https://github.com/ScottPlot/ScottPlot
  - NuGet: https://www.nuget.org/packages/ScottPlot.Avalonia/
  - API Migration Guide (4.x → 5.x): https://scottplot.net/faq/version-5/

- **ScottPlot 4.x** (WinForms - Reference Only)
  - Cookbook: https://scottplot.net/cookbook/4.1/
  - GitHub: https://github.com/ScottPlot/ScottPlot/tree/4.1-stable

- **Alternative Charting Libraries:**
  - **LiveCharts2:** https://livecharts.dev/ (Avalonia support)
  - **OxyPlot:** https://oxyplot.github.io/ (cross-platform)
  - **Microcharts:** https://github.com/microcharts-dotnet/Microcharts (simple, mobile-friendly)

### Drawing and Rendering Documentation

- **SkiaSharp**
  - Website: https://github.com/mono/SkiaSharp
  - API Docs: https://learn.microsoft.com/en-us/dotnet/api/skiasharp
  - Samples: https://github.com/mono/SkiaSharp/tree/main/samples
  - NuGet: https://www.nuget.org/packages/SkiaSharp/
  - Avalonia Integration: https://www.nuget.org/packages/Avalonia.Skia/

- **Avalonia DrawingContext** (Alternative)
  - Docs: https://docs.avaloniaui.net/docs/guides/graphics-and-animation/how-to-draw-with-the-drawingcontext

### Configuration and Logging

- **Microsoft.Extensions.Configuration**
  - Docs: https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
  - NuGet: https://www.nuget.org/packages/Microsoft.Extensions.Configuration/

- **Serilog**
  - Website: https://serilog.net/
  - GitHub: https://github.com/serilog/serilog
  - File Sink: https://github.com/serilog/serilog-sinks-file
  - NuGet: https://www.nuget.org/packages/Serilog/

### Dependency Injection

- **Microsoft.Extensions.DependencyInjection**
  - Docs: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
  - NuGet: https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/

### Inter-Process Communication

- **WatsonTcp** (Current)
  - GitHub: https://github.com/jchristn/WatsonTcp
  - NuGet: https://www.nuget.org/packages/WatsonTcp/
  - Docs: https://github.com/jchristn/WatsonTcp/wiki

- **Alternatives (if cross-platform issues):**
  - **gRPC:** https://grpc.io/docs/languages/csharp/
  - **HTTP/REST:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

### Learning Resources

- **Avalonia Samples:** https://github.com/AvaloniaUI/Avalonia.Samples
  - Control Catalog: https://github.com/AvaloniaUI/Avalonia/tree/master/samples/ControlCatalog
  - Music Store Tutorial: https://docs.avaloniaui.net/docs/tutorials/music-store-app/

- **ReactiveUI Samples:** https://github.com/reactiveui/ReactiveUI.Samples
  - Avalonia Examples: https://github.com/reactiveui/ReactiveUI.Samples/tree/main/avalonia

- **ScottPlot Examples:**
  - Avalonia Demo: https://github.com/ScottPlot/ScottPlot/tree/main/src/ScottPlot5/ScottPlot5%20Demos/ScottPlot5%20Avalonia%20Demo
  - Cookbook: https://scottplot.net/cookbook/5.0/

### Video Tutorials

- **Avalonia UI Introduction:**
  - Official Playlist: https://www.youtube.com/playlist?list=PLH3eGMSe9BPYPh2LLUcLqO7uKXlLb6vVq
  - Community Tutorials: https://www.youtube.com/results?search_query=avalonia+ui+tutorial

- **ReactiveUI with Avalonia:**
  - Getting Started: https://www.youtube.com/watch?v=sAqQwAKtOxE

### Community & Support

- **Avalonia Community:**
  - Telegram: https://t.me/Avalonia
  - Discord: https://discord.gg/avalonia
  - Stack Overflow: https://stackoverflow.com/questions/tagged/avaloniaui
  - GitHub Discussions: https://github.com/AvaloniaUI/Avalonia/discussions

- **ReactiveUI Community:**
  - Slack: https://reactiveui.net/slack
  - Stack Overflow: https://stackoverflow.com/questions/tagged/reactiveui

- **ScottPlot Community:**
  - Discord: https://scottplot.net/discord/
  - GitHub Issues: https://github.com/ScottPlot/ScottPlot/issues

### NuGet Package Gallery

**Essential Packages for Migration:**

```xml
<!-- Core Avalonia -->
<PackageReference Include="Avalonia" Version="11.0.*" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.*" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.*" />

<!-- MVVM -->
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.*" />
<PackageReference Include="ReactiveUI" Version="19.*" />

<!-- Charting -->
<PackageReference Include="ScottPlot.Avalonia" Version="5.0.*" />

<!-- Drawing -->
<PackageReference Include="Avalonia.Skia" Version="11.0.*" />
<PackageReference Include="SkiaSharp" Version="2.88.*" />

<!-- Existing Dependencies (no change) -->
<PackageReference Include="BallisticCalculator" Version="1.1.7.1" />
<PackageReference Include="Gehtsoft.Measurements" Version="1.1.16" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.*" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.*" />
<PackageReference Include="Serilog" Version="4.*" />
<PackageReference Include="WatsonTcp" Version="6.0.*" />
```

**NuGet Search:**
- NuGet.org: https://www.nuget.org/
- Avalonia Packages: https://www.nuget.org/packages?q=Avalonia
- All Package Versions: https://www.nuget.org/packages/[PackageName]/

---

## Contact & Support

**WinForms Reference Project Location:**
`/mnt/d/develop/homeapps/BallisticCalculator1`

**External Dependencies:**
- BallisticCalculator: `/mnt/d/develop/components/BusinessSpecificComponents/BallisticCalculator.Net`
- Gehtsoft.Measurements: `/mnt/d/develop/components/BusinessSpecificComponents/Gehtsoft.Measurements`

**Recommended Next Steps:**
1. Prototype MeasurementControl (validate approach)
2. Prototype ChartControl with ScottPlot 5 (validate critical dependency)
3. Create Phase 1 foundation (establish architecture)

---

**Document Version:** 1.0
**Date:** November 2025
**Status:** Ready to Execute
