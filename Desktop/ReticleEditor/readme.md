# Reticle Editor

A cross-platform desktop application for designing and editing ballistic reticles, built with Avalonia UI.

## Overview

Reticle Editor allows users to create, edit, and preview optical reticle designs used in rifle scopes. The application supports various reticle elements including lines, circles, rectangles, paths, text labels, and bullet drop compensator (BDC) points.

## Features

### Reticle Management
- **New/Open/Save** - Create new reticles, open existing `.reticle` files, save designs
- **Reticle Parameters** - Set reticle name, size (width/height), and zero point offset
- **Live Preview** - Real-time rendering of the reticle design with mouse coordinate tracking

### Supported Elements
- **Line** - Simple line segments with start/end positions, color, and line width
- **Circle** - Circles with center, radius, optional fill, color, and line width
- **Rectangle** - Rectangles with top-left position, size, optional fill, color, and line width
- **Path** - Complex shapes built from MoveTo, LineTo, and Arc segments
- **Text** - Text labels with position, height, color, and anchor point
- **BDC Point** - Bullet drop compensator markers with position and text offset

### Element Operations
- **New** - Create new elements with type-specific editor dialogs
- **Edit** - Modify existing elements through editor dialogs
- **Duplicate** - Clone selected elements
- **Delete** - Remove elements from the reticle

### View Options
- **Font Size** - Adjustable UI font size (Ctrl+/Ctrl-)
- **Coordinate Units** - Display coordinates in Mil, MOA, in/100yd, or cm/100m
- **Highlight Current** - Visual highlighting of selected element in preview

### User Experience
- **Window State Persistence** - Remembers window size, position, and splitter location
- **Dialog Size Persistence** - Dialogs remember their sizes between uses
- **Keyboard Shortcuts** - Standard shortcuts for common operations

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+N | New reticle |
| Ctrl+O | Open file |
| Ctrl+S | Save |
| Ctrl+Shift+S | Save As |
| Ctrl++ | Increase font size |
| Ctrl+- | Decrease font size |

## File Format

Reticle files use the `.reticle` extension and are stored in XML format using the BallisticCalculator library's serialization.

## Requirements

- .NET 8.0 or later
- Cross-platform: Windows, macOS, Linux

## Building

```bash
dotnet build Desktop/ReticleEditor/ReticleEditor.csproj
```

## Running

```bash
dotnet run --project Desktop/ReticleEditor/ReticleEditor.csproj
```

## Testing

```bash
dotnet test Desktop/ReticleEditor.Tests/ReticleEditor.Tests.csproj
```

## Dependencies

- **Avalonia** - Cross-platform UI framework
- **SkiaSharp** - 2D graphics rendering
- **Gehtsoft.Measurements** - Unit conversion and measurement handling
- **BallisticCalculator** - Core ballistic calculation and reticle data structures
