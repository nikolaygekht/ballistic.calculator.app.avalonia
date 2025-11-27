# ReticleEditor - Element Editors Implementation Plan

**Created:** 2025-11-21
**Status:** Planning Phase

---

## Overview

This plan covers the implementation of modal dialog editors for all reticle element types. Each editor will follow the KISS principle with direct UI access patterns (no MVVM), matching the architecture established in CLAUDE.md.

---

## Element Types to Implement

### ReticleElement Types (6 editors)
1. **ReticleLine** - Line from start to end point
2. **ReticleCircle** - Circle with center and radius
3. **ReticleRectangle** - Rectangle with position and size
4. **ReticleText** - Text with position, height, and anchor
5. **ReticlePath** - Complex path with sub-elements (special handling)
6. **ReticleBulletDropCompensatorPoint** - BDC point with position and text parameters

### ReticlePathElement Types (3 sub-editors)
These are used within the Path editor:
1. **ReticlePathElementMoveTo** - Move to position
2. **ReticlePathElementLineTo** - Line to position
3. **ReticlePathElementArc** - Arc with radius and direction

---

## Architecture Principles

Following the project's KISS philosophy:

### 1. Direct UI Access Pattern
```csharp
// Constructor: Populate controls from element
public EditLineDialog(ReticleLine line)
{
    _element = line;
    InitializeComponent();

    // Read from element, write to controls
    StartX.SetValue(line.Start.X);
    StartY.SetValue(line.Start.Y);
    EndX.SetValue(line.End.X);
    EndY.SetValue(line.End.Y);
    LineWidthControl.SetValue(line.LineWidth ?? new Measurement<AngularUnit>(0, AngularUnit.Mil));
    ColorCombo.SelectedItem = line.Color ?? "black";
}

// Save: Read from controls, write to element
private void Save()
{
    var startX = StartX.GetValue<AngularUnit>();
    var startY = StartY.GetValue<AngularUnit>();
    var endX = EndX.GetValue<AngularUnit>();
    var endY = EndY.GetValue<AngularUnit>();

    if (startX.HasValue) _element.Start.X = startX.Value;
    if (startY.HasValue) _element.Start.Y = startY.Value;
    if (endX.HasValue) _element.End.X = endX.Value;
    if (endY.HasValue) _element.End.Y = endY.Value;

    // Handle optional LineWidth
    var lineWidth = LineWidthControl.GetValue<AngularUnit>();
    if (lineWidth.HasValue && lineWidth.Value.In(lineWidth.Value.Unit) > 0)
        _element.LineWidth = lineWidth.Value;
    else
        _element.LineWidth = null;

    _element.Color = ColorCombo.SelectedItem?.ToString();
}

// OK button calls Save and closes
private void OnOK(object? sender, RoutedEventArgs e)
{
    Save();
    Close();
}
```

### 2. Modal Dialog Pattern
- All editors are `Window` instances
- Shown via `await dialog.ShowDialog(owner)`
- OK button calls `Save()` then closes
- Cancel button just closes (no Save)
- DialogResult not used (element modified in-place)
- **Resizable with ScrollViewer** - Users can resize dialogs, content scrolls if needed
- **Size persistence** - Each dialog type remembers its last used size

### 3. No Validation in Dialogs
- Dialogs accept any parseable input
- Validation happens at application level if needed
- MeasurementControls handle their own parsing
- Empty/zero values are allowed

### 4. Color Selection
- ComboBox populated with standard HTML color names
- Utility: `Utilities/ColorList.cs` extension method
- 28 standard colors (same as WinForms version)

### 5. Font Size Awareness
- All controls use `FontSize="{DynamicResource AppFontSize}"`
- Dialogs inherit application font size
- Control sizing relative to font (not fixed pixels)
- **ScrollViewer ensures buttons always accessible** even at large font sizes

### 6. Dialog Size Persistence
- Each dialog type saves its size individually in `WindowState.DialogSizes` dictionary
- Restored on next open via `WindowStateManager.RestoreDialogSize()`
- Saved on close via `WindowStateManager.SaveDialogSize()`
- Shared storage with main window state in `%LOCALAPPDATA%/ReticleEditor/windowState.json`

---

## Element Property Reference

### ReticleLine
```csharp
- ReticlePosition Start          // X, Y coordinates
- ReticlePosition End            // X, Y coordinates
- Measurement<AngularUnit>? LineWidth  // Optional
- string Color                   // HTML color name, optional
```

### ReticleCircle
```csharp
- ReticlePosition Center         // X, Y coordinates
- Measurement<AngularUnit> Radius      // Required
- Measurement<AngularUnit>? LineWidth  // Optional
- string Color                   // HTML color name, optional
- bool? Fill                     // Optional, default false
```

### ReticleRectangle
```csharp
- ReticlePosition TopLeft        // X, Y coordinates (top-left corner)
- ReticlePosition Size           // Width (X), Height (Y)
- Measurement<AngularUnit>? LineWidth  // Optional
- string Color                   // HTML color name, optional
- bool? Fill                     // Optional, default false
```

### ReticleText
```csharp
- ReticlePosition Position       // X, Y coordinates (depends on anchor)
- Measurement<AngularUnit> TextHeight  // Required
- string Text                    // Text content
- string Color                   // HTML color name, optional
- TextAnchor? Anchor             // Left, Center, Right (optional, default Left)
```

### ReticleBulletDropCompensatorPoint
```csharp
- ReticlePosition Position       // X, Y coordinates
- Measurement<AngularUnit> TextOffset  // Horizontal offset for text
- Measurement<AngularUnit> TextHeight  // Text height
```

### ReticlePath
```csharp
- Measurement<AngularUnit>? LineWidth  // Optional
- string Color                   // HTML color name, optional
- bool? Fill                     // Optional, default false
- ReticlePathElementsCollection Elements  // Collection of path elements
```

### ReticlePathElementMoveTo
```csharp
- ReticlePosition Position       // X, Y coordinates
```

### ReticlePathElementLineTo
```csharp
- ReticlePosition Position       // X, Y coordinates
```

### ReticlePathElementArc
```csharp
- ReticlePosition Position       // End position (X, Y)
- Measurement<AngularUnit> Radius      // Arc radius
- bool ClockwiseDirection        // True = clockwise, false = counter-clockwise
- bool MajorArc                  // True = major arc, false = minor arc
```

---

## Implementation Plan

### Phase 1: Utilities & Infrastructure

#### Step 1.1: Color Selection Utility
**File:** `Desktop/ReticleEditor/Utilities/ColorList.cs`

```csharp
public static class ColorListExtensions
{
    private static readonly string[] StandardColors = new[]
    {
        "black", "white", "gray", "red", "green", "blue",
        "yellow", "cyan", "magenta", "orange", "pink", "purple",
        "brown", "navy", "teal", "lime", "olive", "maroon",
        "aqua", "fuchsia", "silver", "darkgray", "lightgray",
        "darkred", "darkgreen", "darkblue", "gold", "indigo"
    };

    public static void PopulateWithColors(this ComboBox comboBox)
    {
        comboBox.Items.Clear();
        foreach (var color in StandardColors)
            comboBox.Items.Add(color);
    }
}
```

**Test:** Create simple test showing colors populate correctly

---

### Phase 2: Simple Element Editors

#### Step 2.1: Line Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditLineDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditLineDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditLineDialogTests.cs`

**Layout:**
```
Title: "Edit Line"
Size: 400x250 (scaled with font)

┌─────────────────────────────────┐
│ Start Point                     │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│ End Point                       │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│ Line Width: [MeasurementControl]│
│ Color: [ComboBox ▼]            │
│                                 │
│          [OK]  [Cancel]         │
└─────────────────────────────────┘
```

**Tests:**
1. `PopulateFromElement_ValidLine_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`
3. `Save_EmptyLineWidth_LineWidthSetToNull`

---

#### Step 2.2: Circle Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditCircleDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditCircleDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditCircleDialogTests.cs`

**Layout:**
```
Title: "Edit Circle"
Size: 400x280

┌─────────────────────────────────┐
│ Center                          │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│ Radius: [MeasurementControl]    │
│ Line Width: [MeasurementControl]│
│ Color: [ComboBox ▼]            │
│ [✓] Fill                        │
│                                 │
│          [OK]  [Cancel]         │
└─────────────────────────────────┘
```

**Tests:**
1. `PopulateFromElement_ValidCircle_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`
3. `Save_FillChecked_FillSetToTrue`
4. `Save_FillUnchecked_FillSetToFalse`

---

#### Step 2.3: Rectangle Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditRectangleDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditRectangleDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditRectangleDialogTests.cs`

**Layout:**
```
Title: "Edit Rectangle"
Size: 400x300

┌─────────────────────────────────┐
│ Top-Left Corner                 │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│ Size                            │
│   Width:  [MeasurementControl]  │
│   Height: [MeasurementControl]  │
│                                 │
│ Line Width: [MeasurementControl]│
│ Color: [ComboBox ▼]            │
│ [✓] Fill                        │
│                                 │
│          [OK]  [Cancel]         │
└─────────────────────────────────┘
```

**Tests:**
1. `PopulateFromElement_ValidRectangle_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`
3. `Save_FillChecked_FillSetToTrue`

---

#### Step 2.4: Text Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditTextDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditTextDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditTextDialogTests.cs`

**Layout:**
```
Title: "Edit Text"
Size: 400x280

┌─────────────────────────────────┐
│ Position                        │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│ Text: [TextBox________________]│
│ Text Height: [MeasurementControl│
│ Color: [ComboBox ▼]            │
│ Anchor: [ComboBox ▼]           │
│         (Left/Center/Right)     │
│                                 │
│          [OK]  [Cancel]         │
└─────────────────────────────────┘
```

**Tests:**
1. `PopulateFromElement_ValidText_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`
3. `Save_AnchorCenter_AnchorSetToCenter`

---

#### Step 2.5: BDC Point Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditBdcDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditBdcDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditBdcDialogTests.cs`

**Layout:**
```
Title: "Edit BDC Point"
Size: 400x240

┌─────────────────────────────────┐
│ Position                        │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│ Text Offset: [MeasurementControl│
│ Text Height: [MeasurementControl│
│                                 │
│          [OK]  [Cancel]         │
└─────────────────────────────────┘
```

**Tests:**
1. `PopulateFromElement_ValidBdc_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`

---

### Phase 3: Path Element Sub-Editors

#### Step 3.1: MoveTo Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditMoveToDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditMoveToDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditMoveToDialogTests.cs`

**Layout:**
```
Title: "Edit Move To"
Size: 400x160

┌─────────────────────────────────┐
│ Position                        │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│          [OK]  [Cancel]         │
└─────────────────────────────────┘
```

**Tests:**
1. `PopulateFromElement_ValidMoveTo_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`

---

#### Step 3.2: LineTo Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditLineToDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditLineToDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditLineToDialogTests.cs`

**Layout:** (Same as MoveTo)

**Tests:**
1. `PopulateFromElement_ValidLineTo_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`

---

#### Step 3.3: Arc Editor
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditArcDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditArcDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditArcDialogTests.cs`

**Layout:**
```
Title: "Edit Arc"
Size: 400x240

┌─────────────────────────────────┐
│ End Position                    │
│   X: [MeasurementControl]       │
│   Y: [MeasurementControl]       │
│                                 │
│ Radius: [MeasurementControl]    │
│ [✓] Clockwise Direction         │
│ [✓] Major Arc                   │
│                                 │
│          [OK]  [Cancel]         │
└─────────────────────────────────┘
```

**Tests:**
1. `PopulateFromElement_ValidArc_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`
3. `Save_BothCheckboxes_PropertiesSetCorrectly`

---

### Phase 4: Complex Path Editor

#### Step 4.1: Path Editor (Complex)
**Files:**
- `Desktop/ReticleEditor/Views/Dialogs/EditPathDialog.axaml`
- `Desktop/ReticleEditor/Views/Dialogs/EditPathDialog.axaml.cs`
- `Common/BallisticCalculator.Controls.Tests/Dialogs/EditPathDialogTests.cs`

**Layout:**
```
Title: "Edit Path"
Size: 600x500

┌─────────────────────────────────────────────┐
│ Line Width: [MeasurementControl]            │
│ Color: [ComboBox ▼]                        │
│ [✓] Fill                                    │
│                                             │
│ ┌─────────────────────────────────────────┐│
│ │ Path Elements                           ││
│ │ ┌─────────────────────────────────────┐ ││
│ │ │ M(0,0)                              │ ││
│ │ │ L(1,1)                              │ ││
│ │ │ A(2,2,0.5mil,min,cw)                │ ││
│ │ │                                     │ ││
│ │ └─────────────────────────────────────┘ ││
│ │ [New MoveTo] [New LineTo] [New Arc]   ││
│ │ [Edit] [Delete] [Move Up] [Move Down] ││
│ └─────────────────────────────────────────┘│
│                                             │
│ ┌─────────────────────────────────────────┐│
│ │ Preview                                 ││
│ │ [ReticleCanvasControl]                  ││
│ │                                         ││
│ └─────────────────────────────────────────┘│
│                                             │
│       [Preview] [Revert] [OK] [Cancel]     │
└─────────────────────────────────────────────┘
```

**Special Features:**
1. **Element List Management:**
   - ListBox shows all path elements
   - Uses `ToString()` for display
   - Selection changes enable/disable buttons

2. **Element Operations:**
   - **New MoveTo/LineTo/Arc:** Create default element, open sub-editor, add to collection
   - **Edit:** Open appropriate sub-editor for selected element
   - **Delete:** Remove selected element from collection
   - **Move Up/Down:** Reorder elements in collection

3. **Preview Functionality:**
   - Small ReticleCanvasControl showing path
   - **Preview button:** Update preview without saving to original element
   - **Revert button:** Restore path from backup copy
   - Working copy vs. original pattern

4. **Path Backup:**
```csharp
private ReticlePath _element;
private ReticlePath _workingCopy;

public EditPathDialog(ReticlePath path)
{
    _element = path;
    _workingCopy = (ReticlePath)path.Clone();  // Work on copy
    // ... populate from _workingCopy
}

private void Save()
{
    // Copy working copy back to original
    _element.LineWidth = _workingCopy.LineWidth;
    _element.Color = _workingCopy.Color;
    _element.Fill = _workingCopy.Fill;
    _element.Elements.Clear();
    foreach (var elem in _workingCopy.Elements)
        _element.Elements.Add(elem.Clone());
}

private void OnRevert()
{
    // Restore working copy from original
    _workingCopy = (ReticlePath)_element.Clone();
    UpdateElementsList();
    UpdatePreview();
}
```

**Tests:**
1. `PopulateFromElement_ValidPath_ControlsMatchElement`
2. `Save_ModifiedControls_ElementUpdated`
3. `AddElement_NewMoveTo_ElementAddedToCollection`
4. `EditElement_ModifyLineTo_CollectionUpdated`
5. `DeleteElement_RemoveArc_CollectionUpdated`
6. `MoveUp_SecondElement_BecomesFirst`
7. `MoveDown_FirstElement_BecomesSecond`
8. `Preview_ModifiedPath_PreviewUpdates`
9. `Revert_AfterChanges_RestoredFromOriginal`

---

### Phase 5: Dialog Factory

#### Step 5.1: Dialog Factory Implementation
**File:** `Desktop/ReticleEditor/Utilities/DialogFactory.cs`

```csharp
public static class DialogFactory
{
    public static Window? CreateDialogForElement(object element)
    {
        return element switch
        {
            ReticleLine line => new EditLineDialog(line),
            ReticleCircle circle => new EditCircleDialog(circle),
            ReticleRectangle rect => new EditRectangleDialog(rect),
            ReticleText text => new EditTextDialog(text),
            ReticlePath path => new EditPathDialog(path),
            ReticleBulletDropCompensatorPoint bdc => new EditBdcDialog(bdc),
            ReticlePathElementMoveTo moveTo => new EditMoveToDialog(moveTo),
            ReticlePathElementLineTo lineTo => new EditLineToDialog(lineTo),
            ReticlePathElementArc arc => new EditArcDialog(arc),
            _ => null
        };
    }
}
```

**Usage in MainWindow:**
```csharp
private async void OnEditElement()
{
    var selectedElement = ReticleItems.SelectedItem;
    if (selectedElement == null) return;

    var dialog = DialogFactory.CreateDialogForElement(selectedElement);
    if (dialog == null)
    {
        StatusArea.Text = "Cannot edit this element type";
        return;
    }

    var result = await dialog.ShowDialog<bool?>(this);
    // Element modified in-place by dialog's Save method

    UpdateReticlePreview();
    RefreshElementsList();
}
```

**Tests:**
1. `CreateDialog_ReticleLine_ReturnsEditLineDialog`
2. `CreateDialog_ReticleCircle_ReturnsEditCircleDialog`
3. `CreateDialog_UnknownType_ReturnsNull`

---

### Phase 6: Integration with MainWindow

#### Step 6.1: Wire Up New Button
```csharp
private async void OnNewElement(object? sender, RoutedEventArgs e)
{
    if (_currentReticle == null) return;

    // Get selected type from combo
    var selectedIndex = ElementTypeCombo.SelectedIndex;

    object newElement = selectedIndex switch
    {
        0 => new ReticleLine
        {
            Start = new ReticlePosition(0, 0, AngularUnit.Mil),
            End = new ReticlePosition(0, 0, AngularUnit.Mil)
        },
        1 => new ReticleCircle
        {
            Center = new ReticlePosition(0, 0, AngularUnit.Mil),
            Radius = new Measurement<AngularUnit>(0, AngularUnit.Mil)
        },
        2 => new ReticleRectangle
        {
            TopLeft = new ReticlePosition(0, 0, AngularUnit.Mil),
            Size = new ReticlePosition(0, 0, AngularUnit.Mil)
        },
        3 => new ReticlePath(),
        4 => new ReticleText
        {
            Position = new ReticlePosition(0, 0, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0, AngularUnit.Mil),
            Text = "Text"
        },
        5 => new ReticleBulletDropCompensatorPoint
        {
            Position = new ReticlePosition(0, 0, AngularUnit.Mil),
            TextOffset = new Measurement<AngularUnit>(0, AngularUnit.Mil),
            TextHeight = new Measurement<AngularUnit>(0, AngularUnit.Mil)
        },
        _ => null
    };

    if (newElement == null) return;

    var dialog = DialogFactory.CreateDialogForElement(newElement);
    if (dialog == null) return;

    var result = await dialog.ShowDialog<bool?>(this);

    // Add to appropriate collection
    if (newElement is ReticleElement element)
        _currentReticle.Elements.Add(element);
    else if (newElement is ReticleBulletDropCompensatorPoint bdc)
        _currentReticle.BulletDropCompensator.Add(bdc);

    RefreshElementsList();
    UpdateReticlePreview();

    // Select newly added item
    ReticleItems.SelectedIndex = _elementsView.Count - 1;
}
```

#### Step 6.2: Wire Up Edit Button
```csharp
private async void OnEditElement(object? sender, RoutedEventArgs e)
{
    var selectedElement = ReticleItems.SelectedItem;
    if (selectedElement == null) return;

    var dialog = DialogFactory.CreateDialogForElement(selectedElement);
    if (dialog == null)
    {
        StatusArea.Text = "Cannot edit this element type";
        return;
    }

    await dialog.ShowDialog<bool?>(this);

    RefreshElementsList();
    UpdateReticlePreview();
}
```

#### Step 6.3: Wire Up Delete Button
```csharp
private void OnDeleteElement(object? sender, RoutedEventArgs e)
{
    if (_currentReticle == null) return;

    var selectedElement = ReticleItems.SelectedItem;
    var selectedIndex = ReticleItems.SelectedIndex;

    if (selectedElement == null) return;

    if (selectedElement is ReticleElement element)
        _currentReticle.Elements.Remove(element);
    else if (selectedElement is ReticleBulletDropCompensatorPoint bdc)
        _currentReticle.BulletDropCompensator.Remove(bdc);

    RefreshElementsList();
    UpdateReticlePreview();

    // Select next item
    if (_elementsView.Count > 0)
    {
        var newIndex = Math.Min(selectedIndex, _elementsView.Count - 1);
        ReticleItems.SelectedIndex = newIndex;
    }
}
```

#### Step 6.4: Wire Up Duplicate Button
```csharp
private void OnDuplicateElement(object? sender, RoutedEventArgs e)
{
    if (_currentReticle == null) return;

    var selectedElement = ReticleItems.SelectedItem;
    if (selectedElement == null) return;

    object clone = null;

    if (selectedElement is ReticleElement element)
    {
        clone = element.Clone();
        _currentReticle.Elements.Add((ReticleElement)clone);
    }
    else if (selectedElement is ReticleBulletDropCompensatorPoint bdc)
    {
        clone = bdc.Clone();
        _currentReticle.BulletDropCompensator.Add((ReticleBulletDropCompensatorPoint)clone);
    }

    if (clone == null) return;

    RefreshElementsList();
    UpdateReticlePreview();

    // Select duplicated item
    ReticleItems.SelectedIndex = _elementsView.Count - 1;
}
```

---

## Testing Strategy

### Unit Test Structure
All tests use xUnit, Avalonia.Headless, and **AwesomeAssertions** for fluent assertions.

**Required using statements:**
```csharp
using Avalonia.Headless.XUnit;
using AwesomeAssertions;  // <-- Fluent assertion library
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views.Dialogs;
using Xunit;
```

**Test Class Pattern:**
```csharp
public class EditLineDialogTests
{
    [AvaloniaFact]
    public void PopulateFromElement_ValidLine_ControlsMatchElement()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.MOA),
            LineWidth = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Color = "red"
        };

        // Act
        var dialog = new EditLineDialog(line);

        // Assert - Using AwesomeAssertions fluent syntax
        dialog.StartX.GetValue<AngularUnit>().Should().Be(line.Start.X);
        dialog.StartY.GetValue<AngularUnit>().Should().Be(line.Start.Y);
        dialog.EndX.GetValue<AngularUnit>().Should().Be(line.End.X);
        dialog.EndY.GetValue<AngularUnit>().Should().Be(line.End.Y);
        dialog.LineWidthControl.GetValue<AngularUnit>().Should().Be(line.LineWidth.Value);
        dialog.ColorCombo.SelectedItem?.ToString().Should().Be(line.Color);
    }

    [AvaloniaFact]
    public void Save_ModifiedControls_ElementUpdated()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Color = "red"
        };
        var dialog = new EditLineDialog(line);

        // Act - Modify controls
        dialog.StartX.SetValue(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        dialog.StartY.SetValue(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        dialog.EndX.SetValue(new Measurement<AngularUnit>(12, AngularUnit.MOA));
        dialog.EndY.SetValue(new Measurement<AngularUnit>(13, AngularUnit.MOA));
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        dialog.ColorCombo.SelectedIndex = dialog.ColorCombo.Items.IndexOf("blue");

        // Assert - Element NOT changed yet
        line.Start.X.Should().NotBe(new Measurement<AngularUnit>(10, AngularUnit.MOA));

        // Act - Save
        dialog.Save();

        // Assert - Element IS changed
        line.Start.X.Should().Be(new Measurement<AngularUnit>(10, AngularUnit.MOA));
        line.Start.Y.Should().Be(new Measurement<AngularUnit>(11, AngularUnit.MOA));
        line.End.X.Should().Be(new Measurement<AngularUnit>(12, AngularUnit.MOA));
        line.End.Y.Should().Be(new Measurement<AngularUnit>(13, AngularUnit.MOA));
        line.LineWidth.Value.Should().Be(new Measurement<AngularUnit>(1, AngularUnit.Mil));
        line.Color.Should().Be("blue");
    }

    [AvaloniaFact]
    public void Save_EmptyLineWidth_LineWidthSetToNull()
    {
        // Arrange
        var line = new ReticleLine
        {
            Start = new ReticlePosition(1, 2, AngularUnit.Mil),
            End = new ReticlePosition(3, 4, AngularUnit.Mil),
            LineWidth = new Measurement<AngularUnit>(0.5, AngularUnit.Mil),
            Color = "red"
        };
        var dialog = new EditLineDialog(line);

        // Act
        dialog.LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        dialog.Save();

        // Assert
        line.LineWidth.Should().BeNull();
    }
}
```

### Test Coverage Requirements
For each dialog:
1. ✓ Constructor populates controls from element
2. ✓ Save() transfers control values to element
3. ✓ Optional properties handled correctly (null/empty)
4. ✓ Unit conversions preserved
5. ✓ Special cases (checkboxes, enums, etc.)

---

## File Structure

```
ReticleEditor/
├── Utilities/
│   ├── ColorList.cs                    # Color selection helper
│   ├── DialogFactory.cs                # Dialog creation factory
│   ├── WindowStateManager.cs           # (existing)
│   └── CoordinateTranslator.cs         # (future)
├── Views/
│   ├── Dialogs/
│   │   ├── EditLineDialog.axaml
│   │   ├── EditLineDialog.axaml.cs
│   │   ├── EditCircleDialog.axaml
│   │   ├── EditCircleDialog.axaml.cs
│   │   ├── EditRectangleDialog.axaml
│   │   ├── EditRectangleDialog.axaml.cs
│   │   ├── EditTextDialog.axaml
│   │   ├── EditTextDialog.axaml.cs
│   │   ├── EditBdcDialog.axaml
│   │   ├── EditBdcDialog.axaml.cs
│   │   ├── EditPathDialog.axaml
│   │   ├── EditPathDialog.axaml.cs
│   │   ├── EditMoveToDialog.axaml
│   │   ├── EditMoveToDialog.axaml.cs
│   │   ├── EditLineToDialog.axaml
│   │   ├── EditLineToDialog.axaml.cs
│   │   ├── EditArcDialog.axaml
│   │   └── EditArcDialog.axaml.cs
│   ├── MainWindow.axaml               # (existing, update)
│   └── MainWindow.axaml.cs            # (existing, update)
└── ReticleEditor_Editors_Plan.md      # This file

BallisticCalculator.Controls.Tests/
└── Dialogs/
    ├── EditLineDialogTests.cs
    ├── EditCircleDialogTests.cs
    ├── EditRectangleDialogTests.cs
    ├── EditTextDialogTests.cs
    ├── EditBdcDialogTests.cs
    ├── EditPathDialogTests.cs
    ├── EditMoveToDialogTests.cs
    ├── EditLineToDialogTests.cs
    └── EditArcDialogTests.cs
```

---

## Implementation Order (TDD Approach)

Following Test-Driven Development:

### Week 1: Infrastructure & Simple Editors
1. **ColorList utility** (30 min)
   - Write test for color population
   - Implement extension method
   - Test in DebugApp

2. **Line Editor** (2-3 hours)
   - Write tests first
   - Create XAML layout
   - Implement code-behind
   - Run tests until green
   - Manual test in DebugApp

3. **Circle Editor** (2 hours)
   - Write tests first
   - Create XAML layout
   - Implement code-behind
   - Run tests until green

4. **Rectangle Editor** (2 hours)
   - Write tests first
   - Create XAML layout
   - Implement code-behind
   - Run tests until green

### Week 2: Text, BDC, Path Sub-Editors
5. **Text Editor** (2 hours)
   - Write tests first
   - Create XAML layout (with Anchor enum)
   - Implement code-behind
   - Run tests until green

6. **BDC Editor** (1.5 hours)
   - Write tests first
   - Create XAML layout
   - Implement code-behind
   - Run tests until green

7. **MoveTo Editor** (1 hour)
   - Write tests first
   - Create XAML layout
   - Implement code-behind
   - Run tests until green

8. **LineTo Editor** (1 hour)
   - Copy MoveTo, adapt for LineTo
   - Write tests
   - Run tests until green

9. **Arc Editor** (2 hours)
   - Write tests first
   - Create XAML layout (with checkboxes)
   - Implement code-behind
   - Run tests until green

### Week 3: Path Editor & Integration
10. **Path Editor** (6-8 hours)
    - Write tests first (complex)
    - Create XAML layout
    - Implement element list management
    - Implement preview/revert
    - Run tests until green
    - Manual testing

11. **Dialog Factory** (1 hour)
    - Write tests first
    - Implement factory
    - Test with all element types

12. **MainWindow Integration** (3-4 hours)
    - Wire up New button
    - Wire up Edit button
    - Wire up Delete button
    - Wire up Duplicate button
    - Test end-to-end workflow

### Week 4: Polish & Documentation
13. **Manual Testing** (2-3 hours)
    - Test all dialogs with real reticles
    - Test edge cases
    - Test font size scaling
    - Fix any issues

14. **Update Status Document** (30 min)
    - Update ReticleEditor_Status.md
    - Mark Phase 2 as complete

---

## Success Criteria

The element editors will be considered complete when:

1. ✓ All 6 main element editors implemented and tested
2. ✓ All 3 path sub-editors implemented and tested
3. ✓ Path editor manages element collection correctly
4. ✓ Dialog factory creates appropriate dialogs
5. ✓ MainWindow New/Edit/Delete/Duplicate buttons work
6. ✓ All unit tests pass (minimum 30 tests total)
7. ✓ Dialogs scale properly with font size
8. ✓ Manual testing confirms usability
9. ✓ Can create, edit, and save a complete reticle

---

## Common Patterns & Code Snippets

### Standard Dialog Constructor Pattern
```csharp
private readonly ReticleLine _element;

public EditLineDialog(ReticleLine line)
{
    _element = line ?? throw new ArgumentNullException(nameof(line));
    InitializeComponent();

    PopulateFromElement();

    // Restore saved dialog size
    Utilities.WindowStateManager.RestoreDialogSize(this, nameof(EditLineDialog));

    // Save dialog size when closing
    Closing += (s, e) => Utilities.WindowStateManager.SaveDialogSize(this, nameof(EditLineDialog));
}

private void PopulateFromElement()
{
    // Always initialize ReticlePosition if null
    if (_element.Start == null)
        _element.Start = new ReticlePosition(0, 0, AngularUnit.Mil);
    if (_element.End == null)
        _element.End = new ReticlePosition(0, 0, AngularUnit.Mil);

    StartX.SetValue(_element.Start.X);
    StartY.SetValue(_element.Start.Y);
    EndX.SetValue(_element.End.X);
    EndY.SetValue(_element.End.Y);

    // Handle nullable LineWidth
    if (_element.LineWidth.HasValue)
        LineWidthControl.SetValue(_element.LineWidth.Value);
    else
        LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));

    // Populate color combo
    ColorCombo.PopulateWithColors();
    ColorCombo.SelectedItem = _element.Color ?? "black";
}
```

### Standard Save Method Pattern
```csharp
private void Save()
{
    var startX = StartX.GetValue<AngularUnit>();
    var startY = StartY.GetValue<AngularUnit>();
    var endX = EndX.GetValue<AngularUnit>();
    var endY = EndY.GetValue<AngularUnit>();

    if (startX.HasValue) _element.Start.X = startX.Value;
    if (startY.HasValue) _element.Start.Y = startY.Value;
    if (endX.HasValue) _element.End.X = endX.Value;
    if (endY.HasValue) _element.End.Y = endY.Value;

    // Handle optional LineWidth
    var lineWidth = LineWidthControl.GetValue<AngularUnit>();
    if (lineWidth.HasValue && lineWidth.Value.In(lineWidth.Value.Unit) > 0)
        _element.LineWidth = lineWidth.Value;
    else
        _element.LineWidth = null;

    _element.Color = ColorCombo.SelectedItem?.ToString();
}

private void OnOK(object? sender, RoutedEventArgs e)
{
    Save();
    Close();
}

private void OnCancel(object? sender, RoutedEventArgs e)
{
    Close();
}
```

### Standard XAML Layout Pattern
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="using:BallisticCalculator.Controls.Controls"
        x:Class="ReticleEditor.Views.Dialogs.EditLineDialog"
        Title="Edit Line"
        Width="450" Height="420"
        MinWidth="400" MinHeight="350"
        FontSize="{DynamicResource AppFontSize}"
        WindowStartupLocation="CenterOwner"
        CanResize="true">

    <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled">
        <StackPanel Margin="20" Spacing="10">
            <TextBlock Text="Start Point" FontWeight="SemiBold" FontSize="{DynamicResource AppFontSize}"/>

            <Grid ColumnDefinitions="60,*" RowDefinitions="Auto,Auto">
                <TextBlock Grid.Row="0" Grid.Column="0" Text="X:" VerticalAlignment="Center" FontSize="{DynamicResource AppFontSize}"/>
                <controls:MeasurementControl Grid.Row="0" Grid.Column="1" x:Name="StartX" FontSize="{DynamicResource AppFontSize}"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="Y:" VerticalAlignment="Center" FontSize="{DynamicResource AppFontSize}"/>
                <controls:MeasurementControl Grid.Row="1" Grid.Column="1" x:Name="StartY" FontSize="{DynamicResource AppFontSize}"/>
            </Grid>

            <TextBlock Text="End Point" FontWeight="SemiBold" FontSize="{DynamicResource AppFontSize}" Margin="0,10,0,0"/>

            <Grid ColumnDefinitions="60,*" RowDefinitions="Auto,Auto">
                <TextBlock Grid.Row="0" Grid.Column="0" Text="X:" VerticalAlignment="Center" FontSize="{DynamicResource AppFontSize}"/>
                <controls:MeasurementControl Grid.Row="0" Grid.Column="1" x:Name="EndX" FontSize="{DynamicResource AppFontSize}"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="Y:" VerticalAlignment="Center" FontSize="{DynamicResource AppFontSize}"/>
                <controls:MeasurementControl Grid.Row="1" Grid.Column="1" x:Name="EndY" FontSize="{DynamicResource AppFontSize}"/>
            </Grid>

            <Grid ColumnDefinitions="100,*" Margin="0,10,0,0">
                <TextBlock Grid.Column="0" Text="Line Width:" VerticalAlignment="Center" FontSize="{DynamicResource AppFontSize}"/>
                <controls:MeasurementControl Grid.Column="1" x:Name="LineWidthControl" FontSize="{DynamicResource AppFontSize}"/>
            </Grid>

            <Grid ColumnDefinitions="100,*">
                <TextBlock Grid.Column="0" Text="Color:" VerticalAlignment="Center" FontSize="{DynamicResource AppFontSize}"/>
                <ComboBox Grid.Column="1" x:Name="ColorCombo" FontSize="{DynamicResource AppFontSize}"/>
            </Grid>

            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Spacing="10" Margin="0,20,0,0">
                <Button Content="OK" Click="OnOK" Width="80" FontSize="{DynamicResource AppFontSize}"/>
                <Button Content="Cancel" Click="OnCancel" Width="80" FontSize="{DynamicResource AppFontSize}"/>
            </StackPanel>
        </StackPanel>
    </ScrollViewer>
</Window>
```

---

## Notes & Considerations

### Performance
- Dialogs are lightweight, instantiate on demand
- No caching needed
- Preview in Path editor should be efficient (small canvas)

### Accessibility
- Tab order important for keyboard navigation
- OK/Cancel buttons accessible via Enter/Escape (future enhancement)
- All controls scale with font size

### Error Handling
- Dialogs don't validate (KISS principle)
- Invalid measurement input handled by MeasurementControl
- Null checks in constructors

### Future Enhancements (Not in Scope)
- Keyboard shortcuts in dialogs (Enter=OK, Escape=Cancel)
- Color picker instead of combo box
- Live preview while editing (not just Path editor)
- Drag-and-drop in Path editor element list
- Undo/Redo in Path editor

---

**End of Plan**
