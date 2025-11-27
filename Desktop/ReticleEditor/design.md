# Reticle Editor - Design Document

## Architecture Overview

The application follows a simple, direct UI access pattern inspired by WinForms rather than MVVM. This approach was chosen to avoid the complexity of reactive property systems and circular notification loops.

## Design Principles

### 1. Direct UI Access Pattern
Controls read/write directly from UI elements rather than using reactive properties:
```csharp
// Value getter reads from UI on-demand
public T GetValue<T>() => ReadFromUIControls();

// Value setter writes directly to UI
public void SetValue(T value) => WriteToUIControls(value);
```

### 2. No Intermediate State
- No stored state between UI and data model
- Events notify changes but don't update properties
- Application decides how to handle events

### 3. Modal Dialogs with Direct Modification
Editor dialogs receive the element to edit and modify it directly:
- Save() applies control values to the element
- Revert() restores original values from a clone
- Cancel closes without saving (reverts first)

### 4. Test-Driven Development
- Unit tests written before implementation
- xUnit + Avalonia.Headless for UI testing
- AwesomeAssertions for fluent assertions

## Project Structure

```
Desktop/ReticleEditor/
├── Program.cs                 # Entry point
├── App.axaml(.cs)            # Application setup, resources
├── Models/
│   ├── WindowState.cs        # Window position/size persistence model
│   └── CombinedElementsView.cs # Unified view of elements + BDC points
├── Utilities/
│   ├── DialogFactory.cs      # Creates appropriate dialog for element type
│   ├── WindowStateManager.cs # Save/load window state to JSON
│   └── ColorList.cs          # Extension method for color combo population
├── Views/
│   ├── MainWindow.axaml(.cs) # Main application window
│   └── Dialogs/
│       ├── EditLineDialog.axaml(.cs)
│       ├── EditCircleDialog.axaml(.cs)
│       ├── EditRectangleDialog.axaml(.cs)
│       ├── EditTextDialog.axaml(.cs)
│       ├── EditBdcDialog.axaml(.cs)
│       ├── EditPathDialog.axaml(.cs)
│       ├── EditMoveToDialog.axaml(.cs)
│       ├── EditLineToDialog.axaml(.cs)
│       └── EditArcDialog.axaml(.cs)
```

## Key Components

### MainWindow
The central hub that manages:
- Reticle loading/saving
- Element list display (CombinedElementsView)
- Preview canvas (ReticleCanvasControl)
- Element operations (new/edit/delete/duplicate)
- Window state persistence

### DialogFactory
Factory pattern for creating element-specific editor dialogs:
```csharp
public static Window? CreateDialogForElement(object element, ReticleDefinition? reticle)
{
    return element switch
    {
        ReticleLine line => new EditLineDialog(line),
        ReticleCircle circle => new EditCircleDialog(circle),
        // ... etc
        _ => null
    };
}
```

### Editor Dialogs
All dialogs follow a consistent pattern:

1. **Constructor** receives element to edit and optional reticle context
2. **Clone** original state for revert capability
3. **PopulateFromElement()** initializes controls from element
4. **Save()** writes control values back to element
5. **Revert()** restores original values from clone
6. **OnOK/OnCancel** handle dialog result

### WindowStateManager
Persists window state to JSON in user's local app data:
- Window position and size
- Font size preference
- Splitter position
- Per-dialog sizes (stored by dialog name)

### CombinedElementsView
Provides unified IList view of both Elements and BulletDropCompensator collections for the ListBox.

## Shared Controls (Common/BallisticCalculator.Controls)

### MeasurementControl
Generic measurement input with:
- Numeric text entry
- Unit dropdown (populated from Measurement<T>.GetUnitNames())
- Increment/decrement buttons
- Type set via UnitType property (reflection-based)

### BallisticCoefficientControl
Specialized control for ballistic coefficient entry:
- Coefficient value input
- Drag table selection (G1, G7, etc.)

### ReticleCanvasControl
SkiaSharp-based canvas for reticle rendering:
- Renders ReticleDefinition to canvas
- Supports overlay layer for highlighting
- Provides pixel-to-angular coordinate conversion

## Dialog Design Rules

### Layout Consistency
- Label column width: 100px
- Controls fill remaining space
- Checkboxes align under controls, not labels:
```xml
<Grid ColumnDefinitions="100,*">
    <CheckBox Grid.Column="1" Content="Fill"/>
</Grid>
```

### Standard Button Layout
```xml
<StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Spacing="10">
    <Button Content="OK" Click="OnOK" Width="80"/>
    <Button Content="Cancel" Click="OnCancel" Width="80"/>
    <Button Content="Revert" Click="OnRevert" Width="80"/>
</StackPanel>
```

### Auto-Preview Behavior
Dialogs with preview update automatically on any parameter change:
```csharp
LineWidthControl.Changed += OnParameterChanged;
ColorCombo.SelectionChanged += OnParameterChanged;

private void OnParameterChanged(object? sender, EventArgs e) => UpdatePreview();
```

Preview creates temporary objects rather than modifying the actual element.

## Data Flow

```
User Action → Dialog Control → Save() → Element Modified
                                      ↓
                              RefreshElementsList()
                                      ↓
                              UpdateReticlePreview()
```

## Testing Strategy

### Unit Tests
Located in `Desktop/ReticleEditor.Tests/`:
- Dialog construction and initialization
- Control population from elements
- Save/Revert functionality
- DialogFactory routing

### Test Pattern
```csharp
[AvaloniaFact]
public void Dialog_PopulatesControls_FromElement()
{
    var element = new ReticleElement { /* properties */ };
    var dialog = new EditElementDialog(element);

    dialog.SomeControl.Value.Should().Be(expected);
}
```

## File Locations

| Purpose | Location |
|---------|----------|
| Main window | Views/MainWindow.axaml(.cs) |
| Element dialogs | Views/Dialogs/Edit*Dialog.axaml(.cs) |
| Factory | Utilities/DialogFactory.cs |
| State persistence | Utilities/WindowStateManager.cs |
| Shared controls | Common/BallisticCalculator.Controls/Controls/ |
| Control logic | Common/BallisticCalculator.Controls/Controllers/ |
| Canvas rendering | Common/BallisticCalculator.Controls/Canvas/ |
| Tests | Desktop/ReticleEditor.Tests/ |

## Adding New Element Types

1. Create `EditNewElementDialog.axaml` and `.cs` in Views/Dialogs/
2. Follow existing dialog pattern (constructor, clone, populate, save, revert)
3. Add case to DialogFactory.CreateDialogForElement()
4. Add element type to MainWindow's ElementTypeCombo and OnNewElement()
5. Write tests in ReticleEditor.Tests/Views/Dialogs/
6. Add DialogFactory test case
