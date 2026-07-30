---
title: Installation and first run
nav_order: 3
---

# Installation and first run

**Goal of this article:** get from the download to a trajectory on screen, on Windows or Linux, and
know where the application keeps its files.

There is no installer. The application ships as one archive **per platform**; you unzip it wherever you
like and run it. Nothing is registered, no services are added, and removing it is deleting the folder.

## What you need

- **A 64-bit Windows, Linux or macOS desktop.** Six archives are built: `win-x64`, `win-arm64`,
  `linux-x64`, `linux-arm64`, `osx-x64` and `osx-arm64`. There is no 32-bit build.
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

The way that always works is to name the assembly, exactly as on macOS:

```bash
cd /path/to/BallisticCalculator2
dotnet BallisticCalculator2.dll     # or ReticleEditor.dll
```

The archive may also contain extension-less launchers, **`BallisticCalculator2`** and
**`ReticleEditor`**. If yours has them they are the more convenient route, but unzip tools routinely drop
the executable bit:

```bash
chmod +x BallisticCalculator2 ReticleEditor
./BallisticCalculator2
```

On a desktop distribution nothing else is needed. On a minimal or server install, the pieces usually
missing are **fontconfig** (Skia will not render text without it) and **libicu** (which .NET needs for
globalization) — install your distribution's `fontconfig` and `libicu` packages.

## macOS

Take **`…-osx-arm64.zip`** on Apple Silicon or **`…-osx-x64.zip`** on an Intel Mac, and install the
matching [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).

Start it by naming the **assembly**, not a launcher:

```bash
cd /path/to/BallisticCalculator2
dotnet BallisticCalculator2.dll
```

and the reticle editor the same way:

```bash
dotnet ReticleEditor.dll
```

The archive *does* contain a launcher — an extension-less `BallisticCalculator2` — and it works. It just
needs its execute bit back first, because the archive is built on Windows in `.zip` format, which stores
no Unix permissions:

```bash
chmod +x BallisticCalculator2 ReticleEditor
./BallisticCalculator2
```

`dotnet BallisticCalculator2.dll` is listed first only because it needs nothing at all: `dotnet` is
already executable, and a managed `.dll` has no permission bit of its own to lose.

**The application is not notarised**, so `spctl --assess` reports it as rejected and Finder will refuse to
open the launcher by double-click. Running it from Terminal is unaffected — macOS is far stricter about
double-clicked application bundles than about a command-line binary. If you do meet a Gatekeeper refusal,
clear the quarantine flag the download left behind:

```bash
xattr -dr com.apple.quarantine .
```

`dotnet <name>.dll` works on Windows and Linux too, and is worth remembering whenever a launcher will not
start. It does **not** cross architectures, though: each archive's `.deps.json` is pinned to its own
runtime identifier, so an Intel `dotnet` cannot run the `osx-arm64` build and vice versa. Matching the
archive to the machine comes first.

### If it will not start

| What you see | What it means |
|---|---|
| `Bad CPU type in executable` | Wrong archive for the machine — an `osx-arm64` build on an Intel Mac, or an `osx-x64` build on Apple Silicon without Rosetta. Apple Silicon can run x64 under Rosetta; Intel can **never** run arm64 |
| `dotnet` reports it cannot load the assembly | The same mismatch reached through `dotnet`: the runtime is one architecture and the build is pinned to the other |
| `zsh: permission denied` | The launcher lost its execute bit in the `.zip` — `chmod +x` it |
| `"…" cannot be opened because the developer cannot be verified` | A Finder double-click on an un-notarised binary. Launch it from Terminal instead, or clear the quarantine flag: `xattr -dr com.apple.quarantine .` |
| `dotnet: command not found` | The runtime is not installed, or not on your `PATH`. It normally lives at `/usr/local/share/dotnet/dotnet`, which is not always symlinked into `/usr/local/bin` |

Not sure which Mac you have? `uname -m` answers it: `arm64` for Apple Silicon, `x86_64` for Intel.

## What is in the folder

Everything sits in one flat directory: the managed assemblies, and the native rendering libraries for
**that** platform — `libSkiaSharp.dll` on Windows, `libSkiaSharp.so` on Linux, `libSkiaSharp.dylib` on
macOS. Each archive carries one platform's natives, so take the one matching the machine.

The one subdirectory that matters is **`data`**:

| Path | Holds |
|---|---|
| `data/drg` | Custom drag tables (`.drg`) — a large set of radar-derived Lapua tables, plus others |
| `data/reticle` | Reticle definitions (`.reticle`) — two dozen: measuring grids (Mil-Dot, MOA, H58, Leupold CCH and CMR-MIL), hunting and military pictures (German #4, PSO-1, an M16 iron sight), and a dozen-odd real optics with calibrated drop ladders (Trijicon ACOG, V-COG and Huron, Elcan Specter, Leupold CMR-W). `README.md` there indexes them with their calibration, and each reticle has a companion `.md` with the full detail |
| `data/ammo` | The sample ammunition library (`.ammox` and legacy `.ammo`), organised by cartridge |
| `data/dictionaries.xml` | The sight and barrel presets the application **ships with**. Your own copy is `user-dictionaries.xml` beside the executable; see [Updating](updating.md) |

**Keep `data` next to the executable.** The application looks for it beside the binary it is running
from; moved or renamed, the shipped drag tables, reticles and presets simply will not be found. These
folders are also the default locations the Open and Save dialogs start in, so anything you add to them
is one click away.

## Where your settings go

- **`appstate.json`, next to the executable** — main-window size and position, child-window size, the
  Shot Parameters dialog size, trajectory-table column widths, and the measurement system of the last
  trajectory you created. Created on first exit; delete it to get the defaults back. If the folder is
  read-only, the application still runs — it just starts with default geometry every time.
- **`user-dictionaries.xml`, next to the executable** — your sight and barrel presets, created on
  first run from the shipped `data/dictionaries.xml`. See [Updating](updating.md).
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

To update, unzip the new archive over the old folder, or beside it. Unzipping *over* the folder is a
merge: it replaces every file the archive contains and leaves your own files alone. Deleting `data`
first does not — that takes your files with it. Your presets and window layout live outside `data` and
are never at risk.

[Updating the application](updating.md) sets out exactly what a release replaces, what it keeps, and how
sight and barrel presets survive an update.

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

[Your first trajectory](first-trajectory.md) — the imperial/metric choice, what each of the six tabs
owns, and what pressing OK actually checks.

---

[← Contents](index.md)
