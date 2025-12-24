# Input Panels Implementation Plan

## Overview

This plan covers the implementation of input panels for the ballistic calculator. These panels collect user input for ammunition, weather/atmosphere, wind, rifle/weapon settings, and shot parameters.

**Reference**: Old WinForms implementation in `BallisticCalculatorNet.InputPanels/`

## Design Philosophy

Following CLAUDE.md principles:
- **Simple property pattern**: No reactive/MVVM - direct UI access like WinForms
- **No validation in controls**: Validation at application level
- **Controllers for complex logic**: Pure C# logic classes, easy to test
- **Changed events**: Panels raise `Changed` events, application decides what to do
- **MeasurementSystem support**: All panels support Metric/Imperial switching

## Panel List

### Core Input Panels
1. **AmmoPanel** - Ammunition/bullet properties (BC, weight, velocity, dimensions)
2. **AmmoLibraryPanel** - AmmoPanel + library metadata (name, caliber, type, barrel length, source) + Load/Save
3. **AtmospherePanel** - Weather conditions
4. **WindPanel** - Single wind definition
5. **WindArrayPanel** - Multiple wind zones
6. **RiflePanel** - Weapon and sight settings
7. **ParametersPanel** - Shot calculation parameters

### Zero Condition Panels (for zeroing at different conditions)
8. **ZeroAmmoPanel** - Optional different ammo used when zeroing (checkbox + AmmoPanel)
9. **ZeroAtmospherePanel** - Optional different atmosphere when zeroing (checkbox + AtmospherePanel)

### Container Panel
10. **ShotDataPanel** - Combines all panels into one cohesive input form

---

## 1. AmmoPanel

### Fields (from old AmmoControl)
| Label | Control Type | Unit Type | Notes |
|-------|-------------|-----------|-------|
| Weight | MeasurementControl | WeightUnit | Bullet weight (gr/g) |
| BC | BallisticCoefficientControl | - | BC value + drag table |
| BC is Form Factor | CheckBox | - | Toggles BC interpretation |
| Custom table | TextBox + Button | - | Path to .drg file |
| Muzzle Velocity | MeasurementControl | VelocityUnit | m/s or ft/s |
| Bullet Diameter | MeasurementControl | DistanceUnit | Optional, mm/in |
| Bullet Length | MeasurementControl | DistanceUnit | Optional, mm/in |
| SD as BC | Button | - | Calculate sectional density as BC |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public Ammunition Ammunition { get; set; }  // Get/Set all values
public string CustomBallisticFile { get; set; }
public DrgDragTable CustomBallistic { get; }  // Read-only loaded table
```

### Events
```csharp
public event EventHandler Changed;
public event EventHandler CustomTableChanged;
```

### Unit Switching (MeasurementSystem)
- **Metric**: Weight=Gram(2dp), Velocity=m/s(1dp), Diameter/Length=mm(2dp)
- **Imperial**: Weight=Grain(1dp), Velocity=ft/s(1dp), Diameter/Length=in(3dp)

---

## 2. AmmoLibraryPanel

Wrapper around AmmoPanel that adds library/catalog metadata for saving and loading ammunition profiles.
Used in ShotDataPanel as the main ammunition input.

### Fields (from old AmmoLibEntryControl)
| Label | Control Type | Notes |
|-------|-------------|-------|
| Name | TextBox | Ammunition name (e.g., "Federal Gold Medal 168gr") |
| Load/Save | Buttons | Load/Save .ammox files |
| *AmmoPanel* | Embedded | All AmmoPanel fields |
| Caliber | TextBox + Button | Caliber name + selector (e.g., ".308 Winchester") |
| Bullet Type | ComboBox | Ammunition type (FMJ, HP, BTHP, etc.) |
| Barrel Length | MeasurementControl | Reference barrel length for velocity |
| Source | TextBox | Data source reference |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public AmmunitionLibraryEntry LibraryEntry { get; set; }  // Full entry with metadata
public Ammunition Ammunition { get; set; }  // Quick access to just ammunition data
```

### Events
```csharp
public event EventHandler Changed;
```

### AmmunitionLibraryEntry Structure
```csharp
public class AmmunitionLibraryEntry
{
    public string Name { get; set; }
    public string Caliber { get; set; }
    public string AmmunitionType { get; set; }  // Bullet type
    public Measurement<DistanceUnit>? BarrelLength { get; set; }
    public string Source { get; set; }
    public Ammunition Ammunition { get; set; }
}
```

### Unit Switching (MeasurementSystem)
- BarrelLength: Metric=mm, Imperial=in
- Plus all AmmoPanel unit switching

---

## 3. AtmospherePanel (Weather)

### Fields (from old AtmosphereControl)
| Label | Control Type | Unit Type | Notes |
|-------|-------------|-----------|-------|
| Altitude | MeasurementControl | DistanceUnit | m or ft |
| Pressure | MeasurementControl | PressureUnit | mmHg or inHg |
| Temperature | MeasurementControl | TemperatureUnit | C or F |
| Humidity | TextBox + % label | - | 0-100% |
| Reset | Button | - | Reset to standard atmosphere |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public Atmosphere Atmosphere { get; set; }  // Get/Set all values
```

### Events
```csharp
public event EventHandler Changed;
```

### Unit Switching (MeasurementSystem)
- **Metric**: Altitude=m(0dp), Pressure=mmHg(1dp), Temp=C(1dp)
- **Imperial**: Altitude=ft(0dp), Pressure=inHg(2dp), Temp=F(1dp)

### Default Values
- **Metric**: 0m, 760mmHg, 15C, 78%
- **Imperial**: 0ft, 29.95inHg, 59F, 78%

---

## 4. WindPanel

### Fields (from old WindControl)
| Label | Control Type | Unit Type | Notes |
|-------|-------------|-----------|-------|
| Distance | CheckBox + MeasurementControl | DistanceUnit | Optional max range for this wind |
| Direction | MeasurementControl | AngularUnit | 0-360 degrees |
| Velocity | MeasurementControl | VelocityUnit | Wind speed |
| Direction Indicator | Canvas/Custom | - | Visual arrow showing wind direction |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public Wind Wind { get; set; }  // Get/Set all values (null if empty)
public bool IsEmpty { get; }
```

### Events
```csharp
public event EventHandler Changed;
```

### Wind Direction Visual
- Circle with arrow pointing wind direction
- Click/drag to set direction interactively
- 0 = wind from behind (tailwind), 90 = from right, 180 = headwind, 270 = from left

### Unit Switching (MeasurementSystem)
- **Metric**: Distance=m, Velocity=m/s
- **Imperial**: Distance=ft, Velocity=ft/s

---

## 5. WindArrayPanel

### Fields (from old MultiWindControl)
| Control | Notes |
|---------|-------|
| WindPanel (first) | Always visible, distance enabled |
| Add button | Add another wind zone |
| Clear button | Remove all except first |
| Dynamic WindPanel list | Additional wind zones |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public WindCollection Winds { get; set; }  // Get/Set all wind zones
```

### Events
```csharp
public event EventHandler Changed;
```

### Behavior
- First WindPanel always has distance checkbox enabled
- "Add" creates new WindPanel with distance = previous + 100m/yd
- Each wind zone applies up to its MaximumRange

---

## 6. RiflePanel

### Fields (from old WeaponControl)
| Label | Control Type | Unit Type | Notes |
|-------|-------------|-----------|-------|
| Sight Height | MeasurementControl | DistanceUnit | Height above bore |
| Zero Distance | MeasurementControl | DistanceUnit | Zero range |
| Rifling | ComboBox | - | Not Set/Left/Right |
| Rifling Step | MeasurementControl | DistanceUnit | Twist rate (enabled when rifling set) |
| Horizontal Click | MeasurementControl | AngularUnit | Scope click value |
| Vertical Click | MeasurementControl | AngularUnit | Scope click value |
| Impact offset checkbox | CheckBox | - | Enable vertical offset at zero |
| Vertical offset | MeasurementControl | DistanceUnit | Enabled when checkbox checked |
| Dictionary buttons | Button | - | Load from sight/barrel dictionaries |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public Rifle Rifle { get; set; }  // Get/Set all values
public Measurement<AngularUnit> VerticalClick { get; }  // Quick access for Parameters panel
```

### Events
```csharp
public event EventHandler Changed;
```

### Unit Switching (MeasurementSystem)
- **Metric**: SightHeight=mm(0dp), ZeroDistance=m(0dp), RiflingStep=mm(0dp), VerticalOffset=mm(0dp)
- **Imperial**: SightHeight=in(1dp), ZeroDistance=yd(0dp), RiflingStep=in(1dp), VerticalOffset=in(1dp)

### Default Values
- **Metric**: SightHeight=50mm, Zero=100m
- **Imperial**: SightHeight=2.6in, Zero=25yd
- Clicks: 0.25 MOA

---

## 7. ParametersPanel

### Fields (from old ParametersControl)
| Label | Control Type | Unit Type | Notes |
|-------|-------------|-----------|-------|
| Maximum Range | MeasurementControl | DistanceUnit | Calculation end distance |
| Step | MeasurementControl | DistanceUnit | Distance between points |
| Angle | MeasurementControl | AngularUnit | Shot angle (uphill/downhill) |
| Angle as clicks | TextBox + Set button | - | Convert clicks to angle |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public ShotParameters Parameters { get; set; }  // Get/Set all values
public IRiflePanel RiflePanel { get; set; }  // Reference for click conversion
```

### Events
```csharp
public event EventHandler Changed;
```

### Unit Switching (MeasurementSystem)
- **Metric**: Distance/Step=m(0dp)
- **Imperial**: Distance/Step=yd(0dp)

### Default Values
- **Metric**: MaxRange=1000m, Step=100m
- **Imperial**: MaxRange=1000yd, Step=100yd

### Angle as Clicks Feature
- User enters number of clicks
- "Set" button multiplies by RiflePanel.VerticalClick to calculate angle

---

## 8. ZeroAmmoPanel

Wrapper around AmmoPanel that adds an "Other ammunition for zero" checkbox.
When unchecked, returns null (use same ammo as shot).
When checked, enables embedded AmmoPanel for specifying different zero ammo.

### Fields
| Control | Notes |
|---------|-------|
| CheckBox | "Other ammunition for zero" |
| AmmoPanel | Enabled only when checkbox is checked |
| Load button | Load ammo from file |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public Ammunition Ammunition { get; set; }  // null if checkbox unchecked
```

### Behavior
- Returns `null` when checkbox unchecked (use shot ammunition for zero)
- Returns AmmoPanel.Ammunition when checkbox checked

---

## 9. ZeroAtmospherePanel

Wrapper around AtmospherePanel that adds an "Other atmosphere for zero" checkbox.
When unchecked, returns null (use same atmosphere as shot).
When checked, enables embedded AtmospherePanel for specifying different zero conditions.

### Fields
| Control | Notes |
|---------|-------|
| CheckBox | "Other atmosphere for zero" |
| AtmospherePanel | Enabled only when checkbox is checked |

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }
public Atmosphere Atmosphere { get; set; }  // null if checkbox unchecked
```

### Behavior
- Returns `null` when checkbox unchecked (use shot atmosphere for zero)
- Returns AtmospherePanel.Atmosphere when checkbox checked

---

## 10. ShotDataPanel

Container panel that combines all input panels into a single cohesive form.
Coordinates MeasurementSystem changes across all child panels.
Provides single ShotData property for get/set of all values.

### Layout (TabControl or Expanders)
- **Ammunition Tab**: AmmoLibraryPanel (includes AmmoPanel + metadata)
- **Weather Tab**: AtmospherePanel
- **Wind Tab**: WindArrayPanel
- **Rifle Tab**: RiflePanel + ZeroAmmoPanel + ZeroAtmospherePanel
- **Parameters Tab**: ParametersPanel

### Properties
```csharp
public MeasurementSystem MeasurementSystem { get; set; }  // Propagates to all children
public ShotData ShotData { get; set; }  // Get/Set all values at once
```

### Wiring
- RiflePanel.ZeroAmmunition = ZeroAmmoPanel
- RiflePanel.ZeroAtmosphere = ZeroAtmospherePanel
- ParametersPanel.RiflePanel = RiflePanel (for click conversion)

### Events
```csharp
public event EventHandler Changed;  // Any child panel changed
```

---

## Implementation Order

### Phase 1: Simple Panels
1. **AtmospherePanel** - Straightforward, no complex interactions
2. **ParametersPanel** - Simple fields, minor click conversion logic

### Phase 2: Core Panels
3. **AmmoPanel** - Uses existing BallisticCoefficientControl and MeasurementControl
4. **AmmoLibraryPanel** - Wrapper around AmmoPanel with metadata
5. **RiflePanel** - Multiple MeasurementControls, conditional enabling

### Phase 3: Wind Panels
6. **WindPanel** - Includes custom direction indicator control
7. **WindDirectionControl** - Canvas-based wind direction visualization (already started)
8. **WindArrayPanel** - Dynamic panel management

### Phase 4: Zero & Container Panels
9. **ZeroAmmoPanel** - Wrapper with checkbox
10. **ZeroAtmospherePanel** - Wrapper with checkbox
11. **ShotDataPanel** - Container combining all panels

---

## File Dialog Abstraction

Abstraction for file open/save dialogs to enable:
- Platform-specific implementations (Avalonia desktop, mobile, etc.)
- Mock implementations for unit testing
- Consistent API across all panels

### IFileDialogService Interface
```csharp
public interface IFileDialogService
{
    /// <summary>
    /// Shows an open file dialog and returns the selected file path.
    /// Returns null if canceled.
    /// </summary>
    Task<string?> OpenFileAsync(FileDialogOptions options);

    /// <summary>
    /// Shows a save file dialog and returns the selected file path.
    /// Returns null if canceled.
    /// </summary>
    Task<string?> SaveFileAsync(FileDialogOptions options);
}

public class FileDialogOptions
{
    public string? Title { get; set; }
    public string? InitialDirectory { get; set; }
    public string? InitialFileName { get; set; }
    public string? DefaultExtension { get; set; }
    public List<FileDialogFilter> Filters { get; } = new();
}

public class FileDialogFilter
{
    public string Name { get; set; }      // e.g., "Ammunition Files"
    public string Extension { get; set; }  // e.g., "ammox"

    public FileDialogFilter(string name, string extension)
    {
        Name = name;
        Extension = extension;
    }
}
```

### Implementations

#### AvaloniaFileDialogService (Desktop)
```csharp
public class AvaloniaFileDialogService : IFileDialogService
{
    private readonly Window _parentWindow;

    public AvaloniaFileDialogService(Window parentWindow)
    {
        _parentWindow = parentWindow;
    }

    public async Task<string?> OpenFileAsync(FileDialogOptions options)
    {
        var dialog = new OpenFileDialog
        {
            Title = options.Title,
            Directory = options.InitialDirectory,
            InitialFileName = options.InitialFileName,
            Filters = options.Filters.Select(f => new FileDialogFilter
            {
                Name = f.Name,
                Extensions = new List<string> { f.Extension }
            }).ToList()
        };

        var result = await dialog.ShowAsync(_parentWindow);
        return result?.FirstOrDefault();
    }

    public async Task<string?> SaveFileAsync(FileDialogOptions options)
    {
        var dialog = new SaveFileDialog
        {
            Title = options.Title,
            Directory = options.InitialDirectory,
            InitialFileName = options.InitialFileName,
            DefaultExtension = options.DefaultExtension,
            Filters = options.Filters.Select(f => new FileDialogFilter
            {
                Name = f.Name,
                Extensions = new List<string> { f.Extension }
            }).ToList()
        };

        return await dialog.ShowAsync(_parentWindow);
    }
}
```

#### MockFileDialogService (Testing)
```csharp
public class MockFileDialogService : IFileDialogService
{
    public string? NextOpenResult { get; set; }
    public string? NextSaveResult { get; set; }
    public FileDialogOptions? LastOpenOptions { get; private set; }
    public FileDialogOptions? LastSaveOptions { get; private set; }

    public Task<string?> OpenFileAsync(FileDialogOptions options)
    {
        LastOpenOptions = options;
        return Task.FromResult(NextOpenResult);
    }

    public Task<string?> SaveFileAsync(FileDialogOptions options)
    {
        LastSaveOptions = options;
        return Task.FromResult(NextSaveResult);
    }
}
```

### Usage in Panels
```csharp
public partial class AmmoLibraryPanel : UserControl
{
    public IFileDialogService? FileDialogService { get; set; }

    private async void OnLoadClick(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null) return;

        var options = new FileDialogOptions
        {
            Title = "Load Ammunition",
            DefaultExtension = "ammox",
            Filters = { new FileDialogFilter("Ammunition Files", "ammox") }
        };

        var fileName = await FileDialogService.OpenFileAsync(options);
        if (fileName != null)
        {
            // Load file...
        }
    }
}
```

### File Structure Addition
```
Common/BallisticCalculator.Panels/
├── Services/
│   ├── IFileDialogService.cs
│   ├── FileDialogOptions.cs
│   └── FileDialogFilter.cs
```

Desktop app provides `AvaloniaFileDialogService` implementation.
Tests use `MockFileDialogService`.

---

## Shared Components

### Already Implemented
- `MeasurementControl` - Generic measurement input
- `BallisticCoefficientControl` - BC + drag table input
- `WindDirectionControl` - Wind direction visualization (in progress)

### New Support Needed
- Panel base class or interface for common patterns (optional)
- Unit switching coordination between panels
- `IFileDialogService` - File dialog abstraction for Load/Save operations

---

## Testing Strategy

### Unit Tests (Controllers)
- Test property get/set round-trip
- Test unit switching preserves values
- Test default value initialization
- Test null/empty handling

### Integration Tests (DebugApp)
- Add panel test pages to DebugApp
- Visual verification of layouts
- Interactive testing of unit switching
- Verify Changed events fire correctly

---

## File Structure

```
Common/BallisticCalculator.Panels/
├── BallisticCalculator.Panels.csproj
├── Panels/
│   ├── AmmoPanel.axaml
│   ├── AmmoPanel.axaml.cs
│   ├── AmmoLibraryPanel.axaml
│   ├── AmmoLibraryPanel.axaml.cs
│   ├── AtmospherePanel.axaml
│   ├── AtmospherePanel.axaml.cs
│   ├── WindPanel.axaml
│   ├── WindPanel.axaml.cs
│   ├── WindArrayPanel.axaml
│   ├── WindArrayPanel.axaml.cs
│   ├── RiflePanel.axaml
│   ├── RiflePanel.axaml.cs
│   ├── ParametersPanel.axaml
│   ├── ParametersPanel.axaml.cs
│   ├── ZeroAmmoPanel.axaml
│   ├── ZeroAmmoPanel.axaml.cs
│   ├── ZeroAtmospherePanel.axaml
│   ├── ZeroAtmospherePanel.axaml.cs
│   ├── ShotDataPanel.axaml
│   └── ShotDataPanel.axaml.cs
├── Models/
│   └── AmmunitionLibraryEntry.cs
├── Services/
│   ├── IFileDialogService.cs
│   ├── FileDialogOptions.cs
│   └── FileDialogFilter.cs
└── PLAN-InputPanels.md

Common/BallisticCalculator.Panels.Tests/
├── BallisticCalculator.Panels.Tests.csproj
├── Panels/
│   ├── AmmoPanelTests.cs
│   ├── AmmoLibraryPanelTests.cs
│   ├── AtmospherePanelTests.cs
│   ├── WindPanelTests.cs
│   ├── WindArrayPanelTests.cs
│   ├── RiflePanelTests.cs
│   ├── ParametersPanelTests.cs
│   ├── ZeroAmmoPanelTests.cs
│   ├── ZeroAtmospherePanelTests.cs
│   └── ShotDataPanelTests.cs
└── Mocks/
    └── MockFileDialogService.cs

Desktop/DebugApp/
└── Services/
    └── AvaloniaFileDialogService.cs
```

---

## Notes

### From Old Implementation
- Panels use `IMeasurementSystemControl` interface for unit switching
- All panels follow same pattern: private field + public property with UpdateSystem()
- Labels use bold font for required fields (Weight, BC, Muzzle Velocity)
- Optional fields use regular font (Bullet Diameter, Length)

### Simplifications for New Implementation
- Skip dictionary buttons initially (sight/barrel selection from data files)
- Skip custom ballistic table loading initially
- Focus on core input functionality first
