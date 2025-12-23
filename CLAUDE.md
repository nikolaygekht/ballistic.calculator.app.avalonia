# BallisticCalculator2 - Project Guide for Claude

## Project Goal

Create a new version of the BallisticCalculator application using **Avalonia UI** instead of WinForms. This is a complete rewrite that maintains the functionality of the original application while taking advantage of modern cross-platform UI technology.

## Key References

### Libraries
- **Gehtsoft.Measurements**: Measurement units library located at `/mnt/d/develop/components/BusinessSpecificComponents/Gehtsoft.Measurements/Gehtsoft.Measurements/`
  - Provides generic `Measurement<T>` struct where T is a unit enum (DistanceUnit, VelocityUnit, WeightUnit, etc.)
  - Static method: `Measurement<T>.GetUnitNames()` returns `Tuple<T, string>[]` for populating unit lists

- **BallisticCalculator**: Core ballistic calculation library located at `/mnt/d/develop/components/BusinessSpecificComponents/BallisticCalculator.Net/`
  - Provides `BallisticCoefficient` struct and `DragTableId` enum
  - Core ballistics calculation engine

### Original Implementation
- **Old WinForms Application**: `/mnt/d/develop/homeapps.projects/BallisticCalculator1/`
  - Reference for understanding control behavior patterns
  - Good example of direct UI access pattern (no reactive properties)
  - Use as reference for precision transparency and other WinForms patterns

## Project Structure

```
BallisticCalculator2/
├── Common/
│   ├── BallisticCalculator.Controls/        # Shared UI controls library
│   │   ├── Controls/                         # Custom controls (MeasurementControl, BallisticCoefficientControl)
│   │   ├── Controllers/                      # Pure logic controllers (no UI dependencies)
│   │   └── Models/                           # Data models (UnitItem, DragTableInfo)
│   └── BallisticCalculator.Controls.Tests/  # xUnit tests for controls
├── Desktop/
│   ├── DebugApp/                             # Test/debug application for controls
│   └── ReticleEditor/                        # Desktop-specific features
└── Mobile/                                    # (Future) Mobile-specific apps
```

## Development Approach

### 1. Separate Desktop and Mobile Applications

**Philosophy**: Don't compromise desktop capabilities for mobile compatibility.

- **Desktop applications** should use all available desktop UI capabilities:
  - Complex layouts and multi-window support
  - Keyboard shortcuts and mouse interactions
  - High information density
  - Advanced controls (e.g., data grids, complex input controls)

- **Mobile applications** will be separate projects:
  - Touch-optimized interfaces
  - Simplified workflows
  - Larger touch targets
  - Different navigation patterns

- **Shared code**: Common controls, business logic, and data models go in the `Common/` directory

### 2. KISS - Keep It Simple, Stupid

**Key Principle**: Don't over-engineer. Create only what we actually need.

#### What We Learned About Controls

After struggling with Avalonia's reactive property system causing circular notification loops, we adopted a simpler approach:

**❌ AVOID: Reactive/MVVM Patterns (for our use case)**
- We don't need reactive properties that auto-update from background data changes
- Our app is **action-driven**: everything happens by explicit user interaction
- Reactive frameworks add complexity we don't need

**✅ USE: WinForms Direct UI Access Pattern**
```csharp
// Value property reads directly from UI on-demand (no stored state)
public object? Value
{
    get
    {
        // Always read from UI controls
        return _controller.Value(NumericPart?.Text ?? "", unit, DecimalPoints, Culture);
    }
    set
    {
        // Write directly to UI controls
        NumericPart.Text = text;
        SelectUnit(unit);
    }
}

// Events just notify - don't update properties
NumericPart.TextChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
```

**Benefits**:
- No circular notification loops
- No recursion guards needed
- Simple, predictable behavior
- Easy to debug

#### Control Design Principles

1. **No validation in controls** - Validate at application level where you have full context
2. **Min/Max for UI only** - Use Minimum/Maximum properties only for increment/decrement behavior, NOT for rejecting user input
3. **Precision transparency** - Store original values to preserve precision when display rounds (e.g., 0.45678 displays as 0.457 but returns exact value if unchanged)
4. **Direct UI access** - Value getters read from UI controls directly, no intermediate storage
5. **Simple events** - Controls raise `Changed` events, application decides what to do

#### When NOT to Create Abstractions

- **Don't create interfaces** unless you have multiple implementations
- **Don't create ViewModels** unless you need testable presentation logic separate from views
- **Don't use data binding** for simple value display/entry (direct property access is simpler)
- **Don't use dependency injection** unless you need different implementations at runtime

### 3. Controller Pattern (for complex controls)

For controls with complex logic, separate pure logic from UI:

**Controller** (Controllers/MeasurementController.cs):
- Pure C# logic
- No Avalonia dependencies
- Easy to unit test
- Methods like `Value()`, `ParseValue()`, `IncrementValue()`, `AllowKeyInEditor()`

**Control** (Controls/MeasurementControl.axaml.cs):
- Thin UI layer
- Calls controller methods via reflection (for generic types)
- Direct UI manipulation

### 4. Avoid Avalonia Pitfalls We Encountered

**Issue**: Circular notification loops when using StyledProperty with TwoWay binding
```csharp
// ❌ This caused endless loops:
private void RaiseChanged()
{
    var newValue = GetValueInternal();
    SetCurrentValue(ValueProperty, newValue);  // Triggers SelectionChanged → RaiseChanged → infinite loop
    Changed?.Invoke(this, EventArgs.Empty);
}
```

**Solution**: Don't update property values in event handlers, just notify
```csharp
// ✅ Simple and works:
NumericPart.TextChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
```

**Issue**: Generic controls aren't supported in XAML
```csharp
// ❌ Can't do this:
public class MeasurementControl<T> : UserControl where T : Enum
```

**Solution**: Use non-generic control with Type property + reflection
```csharp
// ✅ Works:
public class MeasurementControl : UserControl
{
    public Type? UnitType { get; set; }  // Set to typeof(DistanceUnit)
    // Use reflection to create MeasurementController<T>
}
```

## Development Guidelines

### For New Features

1. **Start simple** - Implement the minimal working version first
2. **Write tests first (TDD)** - Unit tests are your primary verification tool
3. **Avoid premature abstraction** - Don't create interfaces/patterns until you need them
4. **Reference WinForms** - When in doubt, check how the old app did it

### For Controls

1. **UI in XAML** - Keep XAML simple (layout only, no complex bindings)
2. **Logic in Controller** - Complex logic goes in controller classes
3. **No validation** - Controls accept any parseable input
4. **Events over properties** - Raise Changed events, let application handle updates

### For Testing (TDD Approach)

**We use Test-Driven Development** - This is a core principle of the project.

1. **Write tests FIRST** - Tests define expected behavior before implementation
2. **Do the heavy lifting in unit tests** - Comprehensive test coverage catches bugs early
3. **xUnit + Avalonia Headless** - For automated UI tests
4. **Test actual behavior** - Don't test implementation details
5. **Use DebugApp for exploratory testing** - After tests pass, verify visually
6. **Test on real data** - Use realistic values from ballistics domain

**Why TDD matters here:**
- One reason we rejected the reactive approach was that it made tests almost useless
- With reactive properties, we had to debug everything manually despite having tests
- Our simplified WinForms-style pattern makes tests reliable and meaningful
- When tests pass, the code actually works (unlike with reactive complexity)

## Common Patterns

### Creating a New Control

1. Create controller in `Controllers/` (pure logic, no UI)
2. Create control in `Controls/` (XAML + code-behind)
3. Wire up controller in control constructor
4. Add test page to DebugApp
5. Test manually, then add automated tests

### Using Measurement Library

```csharp
// Create measurement
var distance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter);

// Get unit names for dropdown
var unitNames = Measurement<DistanceUnit>.GetUnitNames();
// Returns Tuple<DistanceUnit, string>[] like (DistanceUnit.Meter, "m")

// Convert units
var feet = distance.To(DistanceUnit.Foot);
```

### Reading/Writing Control Values

```csharp
// Generic controls
var value = control.GetValue<DistanceUnit>();
control.SetValue(new Measurement<DistanceUnit>(100, DistanceUnit.Meter));

// BallisticCoefficient control
var bc = control.Value;
control.Value = new BallisticCoefficient(0.450, DragTableId.G1);
```

## What to Avoid

1. **Over-engineering** - Don't build frameworks, build features
2. **Reactive complexity** - Our app doesn't need background data updates
3. **Deep inheritance** - Prefer composition over inheritance
4. **Premature optimization** - Make it work, then make it fast if needed
5. **Complex data binding** - Direct property access is often simpler

## Questions to Ask

Before adding complexity, ask:
- Do we actually need this abstraction?
- Will this feature be used in multiple places?
- Is there a simpler way?
- How did the WinForms version handle this?

## Summary

**Core Philosophy**: Build a functional, maintainable application using the simplest approach that works. Avoid framework complexity that doesn't serve our action-driven, desktop-focused use case. Reference the working WinForms implementation when uncertain.
