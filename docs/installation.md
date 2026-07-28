---
title: Installation and first run
nav_order: 3
---

# Installation and first run

**Goal of this article:** get from the download to a trajectory on screen, on Windows or Linux, and
know where the application keeps its files.

There is no installer. The application ships as one archive holding both platforms' binaries; you unzip
it wherever you like and run it. Nothing is registered, no services are added, and removing it is
deleting the folder.

## What you need

- **A 64-bit Windows or Linux desktop** (x64). There is no 32-bit or ARM build, and no macOS build.
- **The .NET 8 runtime.** The builds are framework-dependent, so the runtime has to be present. The
  plain [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) is enough — Avalonia does
  not need the *Desktop* Runtime, and on Linux the distribution's `dotnet-runtime-8.0` package works.
- About 60 MB of disk space once unpacked.

## Download

Take the latest archive from the
[Releases page](https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases) and unzip
it into any folder **you can write to** — your home directory, `Documents`, a USB stick. Avoid
`C:\Program Files` and `/usr/local`: the application keeps its window layout in a file beside the
executable, and in a read-only folder those settings are silently not remembered (see
[Where your settings go](#where-your-settings-go)).

## Windows

Run **`BallisticCalculator2.exe`**. The reticle editor is a separate program in the same folder,
**`ReticleEditor.exe`**.

Both executables are code-signed. Windows SmartScreen may still show a "don't run" prompt for a release
it has not seen before — *More info → Run anyway*.

## Linux

The Linux binaries have no extension: **`BallisticCalculator2`** and **`ReticleEditor`**. Some unzip
tools drop the executable bit, so:

```bash
chmod +x BallisticCalculator2 ReticleEditor
./BallisticCalculator2
```

On a desktop distribution nothing else is needed. On a minimal or server install, the pieces usually
missing are **fontconfig** (Skia will not render text without it) and **libicu** (which .NET needs for
globalization) — install your distribution's `fontconfig` and `libicu` packages.

## What is in the folder

Everything sits in one flat directory: the two Windows executables, the two Linux ones, the managed
assemblies they share, and the native rendering libraries for both platforms (`libSkiaSharp.dll` /
`libSkiaSharp.so`). Having the other platform's files present is harmless.

The one subdirectory that matters is **`data`**:

| Path | Holds |
|---|---|
| `data/drg` | Custom drag tables (`.drg`) — a large set of radar-derived Lapua tables, plus others |
| `data/reticle` | Reticle definitions (`.reticle`) — Mil-Dot, MOA, BDC, chevron, German #4, PSO-1, segmented, and an M16 iron-sight picture |
| `data/legacy-ammo` | The sample ammunition library (`.ammo` / `.ammox`), organised by cartridge |
| `data/dictionaries.xml` | The sight and barrel presets |

**Keep `data` next to the executable.** The application looks for it beside the binary it is running
from; moved or renamed, the shipped drag tables, reticles and presets simply will not be found. These
folders are also the default locations the Open and Save dialogs start in, so anything you add to them
is one click away.

## Where your settings go

- **`appstate.json`, next to the executable** — main-window size and position, child-window size, the
  Shot Parameters dialog size, trajectory-table column widths, and the measurement system of the last
  trajectory you created. Created on first exit; delete it to get the defaults back. If the folder is
  read-only, the application still runs — it just starts with default geometry every time.
- **The reticle editor is the exception**: it keeps its window state under your user profile, in
  `%LOCALAPPDATA%\ReticleEditor\windowState.json` on Windows and
  `~/.local/share/ReticleEditor/windowState.json` on Linux.

Your own work — saved shots, drag tables, reticles, ammunition — is only ever where you put it. The
application writes nothing else outside its own folder.

## First run

The main window opens empty: a menu bar and a blank workspace that child windows will fill. Nothing is
calculated until you describe a shot.

1. **`Trajectory → New`**, then **`Imperial`** or **`Metric`** (`Ctrl+I` / `Ctrl+M`). This is the only
   choice the application asks you to make up front, and it decides nothing more than the units the new
   window's fields are labelled and entered in.
2. The **Shot Parameters** dialog opens on its *Ammunition* tab. The remaining tabs — Weather, Wind,
   Rifle, Zero, Parameters — hold the conditions, the rifle, the zero and the run settings. Every tab
   has usable defaults, so you can press **OK** immediately to see the machinery work, then come back
   and enter a real load.
3. **OK** computes the trajectory and opens a window titled with the ammunition name, holding four
   views: **Table**, **Chart**, **Reticle** and **Summary** (`Ctrl+T`, `Ctrl+C`, `Ctrl+R`; the
   `View → Show` menu lists them).
4. **`View → Edit Parameters`** (`Ctrl+E`) reopens the dialog for that window and recalculates on OK.
   Iterating on one shot is the normal way to work, rather than opening a new window each time.

A few things worth knowing on day one:

- **The measurement system belongs to the window, not the application.** `View → Measurement System`
  (`Ctrl+Shift+I` / `Ctrl+Shift+M`) restates the active window's values in the other system, converting
  rather than relabelling. Several windows can be open in different systems at once.
- **The standalone tools follow the last trajectory you created** — the two drag-table builders, the BC
  converter, Set Atmosphere and the sight/barrel dictionaries. They belong to no window, so they use the
  system of your most recent `Trajectory → New`, remembered between sessions and imperial until then.
- **`View → Angular Units`** picks how holds and clicks are expressed — MOA, mils, thousandths,
  milliradians, in/100 yd or cm/100 m — independently of imperial versus metric.
- Saved shots use the `.trajectory` extension (`Trajectory → Save`), and any table exports to CSV in
  either a local (Excel-friendly) or invariant (portable) format.

## Updating and removing

To update, unzip the new archive over the old folder, or beside it. **If you have added your own files
to `data`**, keep them somewhere else as well — a fresh archive brings its own `data` folder and an
overwrite can take yours with it.

To remove the application, delete the folder. On Windows, also delete
`%LOCALAPPDATA%\ReticleEditor` if you used the reticle editor.

## Running from source instead

If you would rather build it, the only prerequisite is the **.NET 8 SDK**:

```bash
git clone https://github.com/nikolaygekht/ballistic.calculator.app.avalonia
cd ballistic.calculator.app.avalonia
dotnet build BallisticCalculator2.sln
dotnet test BallisticCalculator2.sln          # optional, but it is the project's own check
```

On Windows the `App.bat` and `ReticleEditor.bat` scripts in the repository root launch the two
applications, and `BuildDebug.bat` / `TestDebug.bat` wrap the two commands above.

## Next

A full worked example — one real load entered, zeroed and run end to end — is the next article to be
written. Until it lands, [What Ballistic Calculator 2 is](about.md) covers what each view is for, and
[Recommended reading](recommended-reading.md) covers the ballistics behind the inputs.

---

[← Contents](index.md)
