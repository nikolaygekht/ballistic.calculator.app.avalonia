# ReticleEditor - Avalonia Implementation Plan

## Project Overview

**Goal:** Create a desktop-only Avalonia application that recreates the WinForms ReticleEditor functionality for creating and editing ballistic reticle definitions.

**Reference:** `/mnt/d/develop/homeapps/BallisticCalculator1/BallisticCalculatorNet.ReticleEditor/`

**Target Location:** `/mnt/d/develop/homeapps/BallisticCalculator2/Desktop/ReticleEditor/`

---

## Architecture Principles

Following the project's KISS philosophy (from CLAUDE.md):

1. **Desktop-focused** - No mobile compromises, full desktop UI capabilities
2. **Direct UI access pattern** - Like WinForms, properties read/write UI directly
3. **Simple state management** - No reactive complexity, action-driven updates
4. **Reuse existing controls** - Leverage MeasurementControl, ReticleCanvasControl from Common
5. **Direct element editing** - Dialogs modify elements in-place, no ViewModels
6. **Reflection-based factory** - Same pattern as WinForms for dialog creation
7. **Classic.Avalonia.Theme** - Use Classic theme package (like DebugApp)
8. **Font-size driven layout** - All control sizing scales with application font size
9. **User-adjustable font** - Font+/Font- buttons to increase/decrease interface size

---

## Phase 1: Project Setup & Infrastructure

### Step 1.1: Create Project Structure
- [ ] Create `Desktop/ReticleEditor/` directory
- [ ] Create Avalonia Desktop application project
  - Target: .NET 8.0
  - Template: Avalonia .NET App
- [ ] Add project references:
  - `Common/BallisticCalculator.Controls/`
- [ ] Add NuGet packages:
  - `Avalonia` (v11.3.8)
  - `Classic.Avalonia.Theme` (latest) - Classic Windows theme
  - `BallisticCalculator` (v1.1.7.1)
  - `Gehtsoft.Measurements` (v1.1.16)
  - `Microsoft.Extensions.Configuration` (for config)
  - `Microsoft.Extensions.Configuration.Json`
  - `Microsoft.Extensions.Configuration.CommandLine`
  - `Serilog` (for logging)
  - `Serilog.Sinks.File`

### Step 1.2: Configuration Files
- [ ] Create `reticleEditor.json` for default configuration
- [ ] Create `reticleEditor.state.json` for window state persistence
- [ ] Set up configuration builder in Program.cs to read:
  - JSON config files
  - Command-line arguments (--open, --log-level, --log-destination)

### Step 1.3: Application Startup
- [ ] Configure Program.cs with:
  - Serilog logging setup
  - Configuration loading
  - Unhandled exception handlers
  - Command-line argument parsing
- [ ] Set up App.axaml with application resources:
  - Include Classic.Avalonia.Theme
  - Define `AppFontSize` resource (default: 13)
  - All controls should reference `{DynamicResource AppFontSize}` for FontSize
  - Control dimensions should use relative sizing (e.g., MinWidth based on font size)

### Step 1.4: Utilities & Extensions
- [ ] Create `Utilities/ColorList.cs`
  - Extension method to populate ComboBox with 28 standard colors
  - Or create reusable color picker control
- [ ] Create `Utilities/WindowStateManager.cs`
  - Load/save window position, size, maximized state
  - Save/restore splitter positions
  - Save/restore current font size
- [ ] Create `Utilities/CoordinateTranslator.cs`
  - Convert between pixel and angular coordinates
  - Handle zero offset and scaling
- [ ] Create `Utilities/FontSizeManager.cs`
  - Methods to increase/decrease application font size
  - Update `AppFontSize` resource
  - Persist font size to state file
  - Min/max bounds (e.g., 10-20)

---

## Phase 2: Main Window (Core UI)

### Step 2.1: Main Window Layout (MainWindow.axaml)
Create split layout with Grid:

**Left Side (Canvas Area):**
- [ ] ReticleCanvasControl (from Common library)
  - Width/Height bind to available space
  - Mouse move event for coordinate tracking
  - Mouse click event for area measurement
- [ ] StatusBar at bottom:
  - TextBlock for X coordinate
  - TextBlock for Y coordinate
  - TextBlock for area coordinates (on click)
  - ComboBox for unit selection (Default/MOA/MIL/Thousand)

**Right Side (Controls Panel):**
- [ ] Create three sections in StackPanel:

**Section 1: Reticle Parameters**
- [ ] TextBox: Reticle Name
- [ ] MeasurementControl: Width (AngularUnit)
- [ ] MeasurementControl: Height (AngularUnit)
- [ ] MeasurementControl: Zero X (AngularUnit)
- [ ] MeasurementControl: Zero Y (AngularUnit)
- [ ] Buttons: New, Load, Save, Save As, Set
- [ ] CheckBox: "Highlight Current Element"
- [ ] Font size controls:
  - Button: Font+ (increase font size)
  - Button: Font- (decrease font size)
  - TextBlock: Current font size display

**Section 2: Elements List**
- [ ] ListBox showing all elements
  - Display ReticleElements and BulletDropCompensatorPoints
  - Custom ItemTemplate showing element type and summary
  - SelectionChanged event to update preview
  - DoubleClick to edit

**Section 3: Element Operations**
- [ ] Button grid for creating elements:
  - New Line, New Circle, New Rectangle
  - New Path, New Text, New BDC Point
- [ ] Buttons: Edit, Delete, Duplicate

**Grid Splitter:**
- [ ] Add GridSplitter between left and right panels
- [ ] Save/restore position to state file

### Step 2.2: Main Window Code-Behind (MainWindow.axaml.cs)

**Properties:**
```csharp
public string? ReticleFileName { get; set; }
public ReticleDefinition? Reticle { get; set; }
public bool Changed { get; set; }
private object? SelectedElement { get; set; }
private AngularUnit StatusDisplayUnit { get; set; }
```

**Font Size Management:**
- [ ] `IncreaseFontSize()` - Increase AppFontSize by 1, update display
- [ ] `DecreaseFontSize()` - Decrease AppFontSize by 1, update display
- [ ] Load saved font size from state on startup
- [ ] Save current font size to state on exit

**Core Methods to Implement:**
- [ ] `NewReticle()` - Create blank reticle
- [ ] `LoadReticle(string fileName)` - Load from file
- [ ] `SaveReticle()` - Save to current file
- [ ] `SaveReticleAs(string fileName)` - Save with new name
- [ ] `UpdateReticleControls()` - Load reticle data into UI
- [ ] `GatherReticleDefinition()` - Extract UI values into model
- [ ] `UpdatePreview()` - Redraw canvas with selection highlight
- [ ] `OnMouseMove(Point position)` - Update status coordinates
- [ ] `OnCanvasClick(Point position)` - Show area coordinates
- [ ] `DeleteSelectedElement()` - Remove from reticle
- [ ] `DuplicateSelectedElement()` - Clone element
- [ ] `EditSelectedElement()` - Open edit dialog

**File Operations:**
- [ ] Use Avalonia's StorageProvider for Open/Save dialogs
- [ ] File filter: `*.reticle` files
- [ ] Use `BallisticXmlDeserializer.ReadFromFile<ReticleDefinition>()`
- [ ] Use `BallisticXmlSerializer.SerializeToFile<ReticleDefinition>()`
- [ ] Track "Changed" flag, prompt to save on close

**Preview Rendering:**
- [ ] Set `ReticleCanvasControl.Reticle` property
- [ ] If "Highlight Current Element" checked:
  - Clone selected element
  - Change color to blue
  - Create temporary ReticleDefinition with highlight
  - Set to canvas

---

## Phase 3: Element Edit Dialogs

Following WinForms pattern: Each dialog edits element in-place.

**Common Dialog Pattern:**
- Constructor takes element as parameter
- Populate controls from element in constructor
- `Save()` method transfers values back to element
- OK button calls `Save()`, Cancel just closes
- **All dialogs inherit font size from application** via `{DynamicResource AppFontSize}`
- Control sizing should be relative to font size (not fixed pixels)

### Step 3.1: Circle Dialog (Views/Dialogs/EditCircleDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: Center X
  - MeasurementControl: Center Y
  - MeasurementControl: Radius
  - MeasurementControl: Line Width
  - ComboBox: Color (use ColorList utility)
  - CheckBox: Fill
  - Buttons: OK, Cancel

### Step 3.2: Line Dialog (Views/Dialogs/EditLineDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: Start X
  - MeasurementControl: Start Y
  - MeasurementControl: End X
  - MeasurementControl: End Y
  - MeasurementControl: Line Width
  - ComboBox: Color
  - Buttons: OK, Cancel

### Step 3.3: Rectangle Dialog (Views/Dialogs/EditRectangleDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: Top-Left X
  - MeasurementControl: Top-Left Y
  - MeasurementControl: Width
  - MeasurementControl: Height
  - MeasurementControl: Line Width
  - ComboBox: Color
  - CheckBox: Fill
  - Buttons: OK, Cancel

### Step 3.4: Text Dialog (Views/Dialogs/EditTextDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: Position X
  - MeasurementControl: Position Y
  - MeasurementControl: Text Height
  - TextBox: Text Content
  - ComboBox: Color
  - ComboBox: Text Anchor (Left/Center/Right)
  - Buttons: OK, Cancel

### Step 3.5: BDC Point Dialog (Views/Dialogs/EditBdcDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: Position X
  - MeasurementControl: Position Y
  - MeasurementControl: Text Offset
  - MeasurementControl: Text Height
  - Buttons: OK, Cancel

### Step 3.6: Path Dialog (Views/Dialogs/EditPathDialog.axaml)
**Most complex dialog - has sub-dialogs for path elements**

- [ ] Create Window with:
  - MeasurementControl: Line Width
  - ComboBox: Color
  - CheckBox: Fill
  - ReticleCanvasControl: Path preview
  - ListBox: Path elements
  - Buttons: Add MoveTo, Add LineTo, Add Arc
  - Buttons: Edit Element, Delete Element
  - Buttons: Preview, Revert (Undo), OK, Cancel

- [ ] Keep copy of original path for Revert functionality
- [ ] Preview button updates preview without saving
- [ ] Revert button restores original path

**Path Sub-Dialogs:**

### Step 3.7: MoveTo Dialog (Views/Dialogs/EditMoveToDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: Position X
  - MeasurementControl: Position Y
  - Buttons: OK, Cancel

### Step 3.8: LineTo Dialog (Views/Dialogs/EditLineToDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: Position X
  - MeasurementControl: Position Y
  - Buttons: OK, Cancel

### Step 3.9: Arc Dialog (Views/Dialogs/EditArcDialog.axaml)
- [ ] Create Window with controls:
  - MeasurementControl: End Position X
  - MeasurementControl: End Position Y
  - MeasurementControl: Radius
  - CheckBox: Major Arc
  - CheckBox: Clockwise Direction
  - Buttons: OK, Cancel

---

## Phase 4: Dialog Factory Pattern

### Step 4.1: Create DialogFactory.cs
Reflection-based factory to map element types to dialog types (same pattern as WinForms).

```csharp
public static class DialogFactory
{
    public static Window? CreateDialogForElement(object element)
    {
        // Map element type to dialog type
        var dialogType = element switch
        {
            ReticleCircle => typeof(EditCircleDialog),
            ReticleLine => typeof(EditLineDialog),
            ReticleRectangle => typeof(EditRectangleDialog),
            ReticleText => typeof(EditTextDialog),
            ReticleBulletDropCompensatorPoint => typeof(EditBdcDialog),
            ReticlePath => typeof(EditPathDialog),
            ReticlePathElementMoveTo => typeof(EditMoveToDialog),
            ReticlePathElementLineTo => typeof(EditLineToDialog),
            ReticlePathElementArc => typeof(EditArcDialog),
            _ => null
        };

        if (dialogType == null)
            return null;

        // Create instance using reflection
        var constructor = dialogType.GetConstructor(new[] { element.GetType() });
        return constructor?.Invoke(new[] { element }) as Window;
    }
}
```

- [ ] Implement DialogFactory class
- [ ] Use from MainWindow when creating/editing elements

---

## Phase 5: Element Creation & Management

### Step 5.1: New Element Handlers
For each "New [Type]" button:

- [ ] Create default element instance with zero values
- [ ] Open dialog using DialogFactory
- [ ] On OK:
  - Add to `Reticle.Elements` or `Reticle.BulletDropCompensator`
  - Set as selected element
  - Update preview
  - Set `Changed = true`

### Step 5.2: Edit Element Handler
- [ ] Get selected element from ListBox
- [ ] Open dialog using DialogFactory
- [ ] Dialog modifies element in-place
- [ ] On OK:
  - Update preview
  - Set `Changed = true`

### Step 5.3: Delete Element Handler
- [ ] Get selected element
- [ ] Confirm deletion (optional)
- [ ] Remove from `Reticle.Elements` or `Reticle.BulletDropCompensator`
- [ ] Clear selection
- [ ] Update preview
- [ ] Set `Changed = true`

### Step 5.4: Duplicate Element Handler
- [ ] Get selected element
- [ ] Deep clone element (serialize/deserialize or manual clone)
- [ ] Add clone to collection
- [ ] Select clone
- [ ] Update preview
- [ ] Set `Changed = true`

---

## Phase 6: Coordinate System & Status Display

### Step 6.1: Coordinate Translation
- [ ] Implement pixel-to-angular conversion:
  - Account for canvas size
  - Account for reticle size
  - Account for zero offset
  - Account for image scaling/centering

### Step 6.2: Mouse Tracking
- [ ] On MouseMove over canvas:
  - Convert pixel coordinates to angular
  - Display in status bar with selected unit
  - Update continuously as mouse moves

### Step 6.3: Area Measurement
- [ ] On MouseDown on canvas:
  - Store "area" coordinates
  - Display in status bar
  - Useful for measuring distances on reticle

### Step 6.4: Unit Selection
- [ ] ComboBox with options:
  - Default (uses reticle's native unit)
  - MOA (Minutes of Angle)
  - MIL (Milliradians)
  - Thousand (Thousandths)
- [ ] On unit change, update status display
- [ ] Convert current coordinates to new unit

---

## Phase 7: Preview Rendering

### Step 7.1: Basic Preview
- [ ] Use ReticleCanvasControl (already created in Phase 1)
- [ ] Set `Reticle` property to current ReticleDefinition
- [ ] Set `BackgroundColor` to white or user preference

### Step 7.2: Selection Highlighting
- [ ] When "Highlight Current Element" is checked:
  - Create temporary ReticleDefinition clone
  - Find selected element in clone
  - Change its color to blue (or highlight color)
  - Set temporary definition to canvas
- [ ] When unchecked:
  - Show original reticle without highlighting

### Step 7.3: Image Scaling
- [ ] Use `ReticleCanvasUtils.CalculateReticleImageSize()`
- [ ] Maintain aspect ratio
- [ ] Center preview in available space

---

## Phase 8: State Persistence

### Step 8.1: Window State
- [ ] On window load:
  - Read `reticleEditor.state.json`
  - Restore window position, size
  - Restore maximized state
  - Restore splitter position
  - Restore font size (apply to AppFontSize resource)
- [ ] On window close:
  - Save current window state
  - Save splitter position
  - Save current font size
  - Write to `reticleEditor.state.json`

### Step 8.2: Unsaved Changes
- [ ] Track `Changed` flag on any modification
- [ ] On window closing:
  - If Changed, prompt to save
  - Options: Save, Don't Save, Cancel
  - Cancel prevents window close

### Step 8.3: Command-Line Arguments
- [ ] Support `--open <filename>` to load file at startup
- [ ] Read from configuration in Program.cs
- [ ] Call `LoadReticle()` if file specified

---

## Phase 9: Menu & Keyboard Shortcuts

### Step 9.1: Main Menu
- [ ] Create MenuBar with:

**File Menu:**
- New (Ctrl+N)
- Open... (Ctrl+O)
- Save (Ctrl+S)
- Save As... (Ctrl+Shift+S)
- Exit

**Edit Menu:**
- New Line
- New Circle
- New Rectangle
- New Path
- New Text
- New BDC Point
- Separator
- Edit Element (Enter or Ctrl+E)
- Delete Element (Delete)
- Duplicate Element (Ctrl+D)

**View Menu:**
- Highlight Current Element (Toggle)
- Separator
- Increase Font Size (Ctrl++)
- Decrease Font Size (Ctrl+-)

**Help Menu:**
- About

### Step 9.2: Keyboard Shortcuts
- [ ] Implement KeyBindings in XAML or code-behind
- [ ] Wire up to command handlers

---

## Phase 10: Error Handling & Validation

### Step 10.1: File Operations
- [ ] Try-catch around Load/Save operations
- [ ] Show error dialog on failure
- [ ] Log errors using Serilog

### Step 10.2: Element Validation
- [ ] Validate measurements in dialogs before saving
- [ ] Show validation errors inline (like MeasurementControl does)
- [ ] Prevent OK if validation fails (optional - WinForms allows invalid values)

### Step 10.3: Global Exception Handler
- [ ] Show friendly error dialog
- [ ] Log exception details
- [ ] Allow user to continue or exit

---

## Phase 11: Testing & Polish

### Step 11.1: Manual Testing
- [ ] Test all element creation dialogs
- [ ] Test file load/save
- [ ] Test coordinate display and conversion
- [ ] Test selection highlighting
- [ ] Test duplicate and delete
- [ ] Test path editor with all path element types
- [ ] Test state persistence
- [ ] Test command-line file loading
- [ ] Test unsaved changes prompt

### Step 11.2: UI Polish
- [ ] Consistent spacing and padding
- [ ] Tab order for keyboard navigation
- [ ] Proper focus handling in dialogs
- [ ] Tooltips for buttons
- [ ] Proper window titles
- [ ] Icon for application

### Step 11.3: Documentation
- [ ] Add README.md for ReticleEditor
- [ ] Document keyboard shortcuts
- [ ] Document file format (reference BallisticCalculator docs)

---

## Phase 12: Optional Enhancements

These are nice-to-have features not in original WinForms app:

### Step 12.1: Undo/Redo
- [ ] Command pattern for actions
- [ ] Undo stack
- [ ] Redo stack
- [ ] Ctrl+Z / Ctrl+Y shortcuts

### Step 12.2: Grid/Snap
- [ ] Optional grid overlay on canvas
- [ ] Snap-to-grid for element positioning
- [ ] Configurable grid spacing

### Step 12.3: Zoom
- [ ] Zoom in/out on canvas
- [ ] Mouse wheel zoom
- [ ] Fit to window

### Step 12.4: Export
- [ ] Export preview as PNG/SVG
- [ ] Copy to clipboard

### Step 12.5: Element Ordering
- [ ] Move element up/down in drawing order
- [ ] Affects which elements are drawn on top

---

## Implementation Order (Summary)

**Recommended sequence for implementation:**

1. **Phase 1** - Project setup (foundation)
2. **Phase 2.1** - Main window layout (see it working)
3. **Phase 2.2** - Basic file operations (New, Load, Save)
4. **Phase 7** - Preview rendering (visual feedback)
5. **Phase 3.1-3.4** - Simple element dialogs (Circle, Line, Rectangle, Text)
6. **Phase 4** - Dialog factory (infrastructure)
7. **Phase 5** - Element creation/editing (core functionality)
8. **Phase 3.5** - BDC dialog
9. **Phase 3.6-3.9** - Path dialogs (most complex)
10. **Phase 6** - Coordinate system & status
11. **Phase 8** - State persistence
12. **Phase 9** - Menu & shortcuts
13. **Phase 10** - Error handling
14. **Phase 11** - Testing & polish
15. **Phase 12** - Optional enhancements (if time permits)

---

## Key Architectural Decisions

### Why No MVVM?
Following the project's KISS principle:
- **Action-driven app** - Everything happens via user interaction
- **No background updates** - Reticle only changes when user edits
- **Direct UI access** - Simpler than property binding for this use case
- **Dialogs modify in-place** - No need for intermediate ViewModels

### Why Reflection Factory?
- **Proven pattern** - Works well in WinForms version
- **Type-safe** - Compile-time checking via constructor signatures
- **Simple** - No complex dependency injection needed
- **Extensible** - Easy to add new element types

### Why Direct Element Editing?
- **Preserves precision** - Like MeasurementControl pattern
- **Immediate feedback** - Changes visible on OK
- **Simple state** - No sync between model and ViewModel
- **Undo via Cancel** - Dialog preserves original values

---

## Success Criteria

The Avalonia ReticleEditor will be considered complete when it can:

1. ✓ Create new reticle definitions
2. ✓ Load reticle files (*.reticle)
3. ✓ Save reticle files
4. ✓ Edit all element types (Circle, Line, Rectangle, Text, Path, BDC)
5. ✓ Create, edit, delete, duplicate elements
6. ✓ Show real-time preview with selection highlighting
7. ✓ Display mouse coordinates in multiple units
8. ✓ Persist window state between sessions
9. ✓ Handle unsaved changes gracefully
10. ✓ Support command-line file opening

**Bonus (Optional):**
11. ○ Undo/Redo functionality
12. ○ Grid and snap-to-grid
13. ○ Zoom controls
14. ○ Export as image

---

## Resources & References

**WinForms Source:** `/mnt/d/develop/homeapps/BallisticCalculator1/BallisticCalculatorNet.ReticleEditor/`

**Avalonia Target:** `/mnt/d/develop/homeapps/BallisticCalculator2/Desktop/ReticleEditor/`

**Shared Controls:** `/mnt/d/develop/homeapps/BallisticCalculator2/Common/BallisticCalculator.Controls/`

**BallisticCalculator Library:** NuGet package v1.1.7.1
- Documentation: Check package docs or decompile for API reference
- Key classes: ReticleDefinition, ReticleElement derivatives, ReticleDrawController

**Avalonia Documentation:**
- Official docs: https://docs.avaloniaui.net/
- Dialog API: File pickers, message boxes
- Custom drawing: Canvas, DrawingContext
- SkiaSharp integration (already implemented in ReticleCanvasControl)

---

## Notes

- **Keep it simple** - Follow WinForms patterns where possible
- **Reuse controls** - MeasurementControl, ReticleCanvasControl already work
- **Test frequently** - Run app after each major feature
- **Desktop-only** - No mobile compromises, use full desktop power
- **Precision matters** - Use Measurement<T> types, preserve exact values

---

*End of Plan*
