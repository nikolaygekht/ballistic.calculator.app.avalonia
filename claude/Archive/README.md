# Ballistic Calculator 2

Cross-platform ballistic calculator application built with Avalonia UI.

## Solution Structure

```
BallisticCalculator2/
├── Common/                          # Cross-platform libraries
│   ├── BallisticCalculator.Controls/
│   │   └── BallisticCalculator.Controls.csproj
│   │       - Avalonia controls library (cross-platform)
│   │       - MeasurementControl, BallisticCoefficientControl, etc.
│   │       - Includes: Avalonia 11.2.2, ReactiveUI
│   │       - Business logic: Gehtsoft.Measurements, BallisticCalculator
│   │
│   └── BallisticCalculator.Controls.Tests/
│       └── BallisticCalculator.Controls.Tests.csproj
│           - xUnit test project
│           - Tools: Moq, AwesomeAssertions, Avalonia.Headless
│
├── Desktop/                         # Desktop applications
│   ├── DebugApp/
│   │   └── DebugApp.csproj
│   │       - Avalonia MVVM desktop app
│   │       - Interactive testing/demo application
│   │       - References: BallisticCalculator.Controls
│   │
│   ├── ReticleEditor/
│   │   └── ReticleEditor.csproj
│   │       - Avalonia MVVM desktop app
│   │       - Reticle editing application
│   │       - References: BallisticCalculator.Controls
│   │
│   └── ReticleEditor.Tests/
│       └── ReticleEditor.Tests.csproj
│           - xUnit test project
│           - Tools: Moq, AwesomeAssertions, Avalonia.Headless
│
└── Mobile/                          # Mobile applications (future)
    └── (Reserved for mobile apps)

```

## Project Details

### Common Projects

**BallisticCalculator.Controls**
- **Type:** Class Library (.NET 8.0)
- **Purpose:** Cross-platform Avalonia controls library
- **Key Dependencies:**
  - Avalonia 11.2.2
  - Avalonia.Themes.Fluent 11.2.2
  - Avalonia.ReactiveUI 11.2.2
  - Gehtsoft.Measurements 1.1.16
  - BallisticCalculator 1.1.7.1

**BallisticCalculator.Controls.Tests**
- **Type:** xUnit Test Project (.NET 8.0)
- **Purpose:** Unit and UI tests for controls library
- **Key Dependencies:**
  - xunit 2.9.2
  - Moq 4.20.72
  - AwesomeAssertions 9.3.0
  - Avalonia.Headless.XUnit 11.2.2

### Desktop Projects

**DebugApp**
- **Type:** Avalonia Desktop Application (.NET 8.0)
- **Purpose:** Interactive testing and demo application
- **Template:** avalonia.mvvm
- **References:** BallisticCalculator.Controls

**ReticleEditor**
- **Type:** Avalonia Desktop Application (.NET 8.0)
- **Purpose:** Reticle editing application
- **Template:** avalonia.mvvm
- **References:** BallisticCalculator.Controls

**ReticleEditor.Tests**
- **Type:** xUnit Test Project (.NET 8.0)
- **Purpose:** Unit and UI tests for ReticleEditor
- **Key Dependencies:**
  - xunit 2.9.2
  - Moq 4.20.72
  - AwesomeAssertions 9.3.0
  - Avalonia.Headless.XUnit 11.2.2

## Build and Run

### Build the Solution

```bash
dotnet build BallisticCalculator2.sln
```

### Run Tests

```bash
dotnet test BallisticCalculator2.sln
```

### Run Applications

```bash
# Run DebugApp
dotnet run --project Desktop/DebugApp/DebugApp.csproj

# Run ReticleEditor
dotnet run --project Desktop/ReticleEditor/ReticleEditor.csproj
```

## Development Plan

This solution follows the **MeasurementControlPlan.md** development plan:

1. **Phase 1:** Foundation & Infrastructure ✅ COMPLETED
   - Solution structure created
   - All projects configured
   - Dependencies installed
   - Build verification passed

2. **Phase 2:** MeasurementControl Development (next)
   - MeasurementControlController (pure logic)
   - MeasurementControlViewModel (MVVM)
   - MeasurementControl UI (Avalonia XAML)

3. **Phase 3:** BallisticCoefficientControl Development
4. **Phase 4:** Advanced Features & Polish
5. **Phase 5:** Testing

## Technology Stack

- **.NET:** 8.0
- **UI Framework:** Avalonia 11.2.2
- **MVVM:** ReactiveUI
- **Testing:** xUnit, Moq, AwesomeAssertions, Avalonia.Headless
- **Business Logic:** BallisticCalculator 1.1.7.1, Gehtsoft.Measurements 1.1.16

## Reference Documentation

- **ProjectReview.md** - Analysis of original WinForms application
- **MeasurementControlPlan.md** - Development plan for controls library
- **AvaloniaMigrationPlan.md** - Complete migration strategy

## License

This project is licensed under the GNU General Public License v2.0 (GPL-2.0).

Copyright (C) 2025 Ballistic Calculator Contributors

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

See [LICENSE](LICENSE) for the full license text.
