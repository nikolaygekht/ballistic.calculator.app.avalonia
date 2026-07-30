# Ballistic Calculator 2

A free, open-source ballistic calculator for **Windows, Linux and macOS**, built on Avalonia UI.

This is the successor to the WinForms [Ballistic Calculator .NET](https://github.com/nikolaygekht/ballistic.calculator.app),
rewritten from the ground up to be genuinely cross-platform rather than Windows-with-Wine. The trajectory
mathematics comes from the [BallisticCalculator](https://github.com/gehtsoft-usa/BallisticCalculator1) library.

<a href="docs/screenshots/ballistic_table.png"><img src="docs/screenshots/ballistic_table.png" width="880"
alt="The trajectory table: range, velocity, Mach, drop, hold, clicks, windage, time of flight, energy and optimal game weight from the muzzle out to 1,000 yards"></a>

*Drop, windage, clicks, energy and time of flight out to 1,000 yd — .223 69 gr Sierra, 300 yd zero. Click any
image to open it full size.*

📖 **[User guide](https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/)** — installation, what the
model includes, and where to learn the ballistics behind the inputs.

## Goals

**Accuracy comparable to commercial and 4DOF solvers.** The engine is a 3DOF (point-mass) integration with the
correction terms that matter in the field — spin drift, crosswind aerodynamic jump and the Coriolis effect —
and it accepts **measured drag curves**, which is where real accuracy comes from. A point-mass solver running
the projectile's own Cd curve tracks a 4DOF solver closely; what a 4DOF model adds is the projectile's angular
motion, not a better drag model. So rather than approximating that, this calculator lets you supply or build
the actual curve.

**Truly cross-platform.** One codebase, native builds for Windows, Linux and macOS, no emulation layer. Each release
ships both.

**Android next.** The shared calculation and domain layers (`Common/`) carry no desktop UI dependencies, so a
touch-first mobile app can be built on them rather than around them. That work has not started yet.

## What it does

<table>
<tr>
<td align="center"><a href="docs/screenshots/reticle.png"><img src="docs/screenshots/reticle.png" width="200" alt="Sight picture: a Mil-Dot reticle with a 6 by 6 inch target box drawn to scale at 100 yards"></a><br><sub>Sight picture</sub></td>
<td align="center"><a href="docs/screenshots/compare_charts.png"><img src="docs/screenshots/compare_charts.png" width="200" alt="Drop curves for two cartridges compared on one chart with a legend"></a><br><sub>Loads compared</sub></td>
<td align="center"><a href="docs/screenshots/hit_probability.png"><img src="docs/screenshots/hit_probability.png" width="200" alt="Hit probability dialog: error budget inputs on the left, an 18.3 percent single-shot result and an impact scatter against the vital zone on the right"></a><br><sub>Hit probability</sub></td>
<td align="center"><a href="docs/screenshots/custom_drg.png"><img src="docs/screenshots/custom_drg.png" width="200" alt="Approximate Drag Table dialog with sixteen measured downrange velocities loaded from a CSV file"></a><br><sub>Drag table from radar data</sub></td>
</tr>
</table>

**Trajectory**

* 3DOF point-mass integration, with **spin drift**, **crosswind aerodynamic jump** (Litz / Applied Ballistics)
  and the **Coriolis effect** (barrel azimuth + shooter latitude)
* Uphill and downhill shots, scope cant, multiple wind zones along the flight path
* Zeroing with a *different* cartridge, atmosphere or wind than the shot itself — zero with supersonic, shoot
  subsonic
* Results as a **table**, a **chart**, and a **sight picture** through your own reticle; several trajectories
  can be compared on one chart, and any table can be exported to CSV

*Spin drift and aerodynamic jump need the rifling twist plus the bullet's diameter and length; Coriolis needs
the barrel azimuth and your latitude. Leave them out and those terms are simply absent.*

**Drag models**

* All the standard curves — G1, G2, G5, G6, G7, G8, GI, GS, RA4
* **Custom drag tables** (`.drg`) — the projectile's own measured Cd curve
* **Approximate a drag table** you do not have, two ways:
  * from a **multi-BC curve** (BC quoted at several Mach numbers, as published on many data sheets)
  * from **measured downrange velocities** (radar or chronograph data)
* **Convert a ballistic coefficient between standard tables** — the everyday G1 ↔ G7 question, at a reference
  velocity you choose, with the accuracy caveat stated rather than hidden

**Analysis**

* **Hit probability** — a Monte-Carlo estimate over your error budget: group size, shooting position, range and
  wind estimation error, and the ammunition's muzzle-velocity deviation. Reports the single-shot probability,
  how many shots a first hit needs at 50/75/90/95/98%, and an impact scatter against the vital zone
* Point-blank range and the dead-zone span, near and far zero, the distance where the bullet goes subsonic
* Moving-target lead, shown as an aim-off box on the reticle

**Libraries and editors**

* Ammunition library, and sight and barrel preset dictionaries
* A separate **reticle editor** — build your own reticle

## Download

Grab the archive for your platform from [Releases](https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases)
— `.zip` for Windows, `.tar.gz` for Linux and macOS — unpack it into a folder you can write to, and run it.
There is no installer.

* **Windows** — run `BallisticCalculator2.exe` (the reticle editor is `ReticleEditor.exe`)
* **Linux** — `./BallisticCalculator2` (or `dotnet BallisticCalculator2.dll`)
* **macOS** — `./BallisticCalculator2`, matching the archive to the machine: `osx-arm64` for Apple
  Silicon, `osx-x64` for Intel

One archive per platform, each holding that platform's binaries alongside the shared `data` folder of
reticles, drag tables and the sample ammunition library — keep that folder beside the executable. The builds are
framework-dependent, so the **[.NET 8 runtime](https://dotnet.microsoft.com/download/dotnet/8.0)** has to be
installed; the plain runtime is enough, Avalonia does not need the Desktop Runtime.

Full instructions, including what the application stores and where, are in
[Installation and first run](https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/installation.html).

## Recommended reading

If precision shooting or external ballistics is new to you:

* [External ballistics](https://en.wikipedia.org/wiki/External_ballistics)
* [Projectile motion](https://en.wikipedia.org/wiki/Projectile_motion)
* [Ballistic coefficient](https://en.wikipedia.org/wiki/Ballistic_coefficient)
* [Scope reticle](https://en.wikipedia.org/wiki/Reticle)

The user guide's [Recommended reading](https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/recommended-reading.html)
adds the standard books, each mapped to the question it answers.

## RISK NOTICE

The application performs a very limited simulation of a complex physical process and therefore makes a great
many approximations. The calculation results MUST NOT be considered as completely and reliably reflecting the
actual behaviour or characteristics of projectiles. While these results may be used for educational purposes,
they must NOT be considered reliable in any area where an incorrect calculation could lead to a wrong decision,
financial harm, or risk to human life.

THE CODE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE MATERIALS OR THE USE OR OTHER
DEALINGS IN THE MATERIALS.

## Related projects

* [BallisticCalculator](https://github.com/gehtsoft-usa/BallisticCalculator1) — the trajectory engine this
  application is built on ([nuget](https://www.nuget.org/packages/BallisticCalculator))
* [Gehtsoft.Measurements](https://github.com/gehtsoft-usa/Gehtsoft.Measurements) — the unit and measurement
  library used throughout ([nuget](https://www.nuget.org/packages/Gehtsoft.Measurements))
* [Ballistic Calculator .NET](https://github.com/nikolaygekht/ballistic.calculator.app) — the original WinForms
  application this one replaces

## Third-party components

* [Avalonia UI](https://avaloniaui.net/) and [Classic.Avalonia.Theme](https://github.com/AvaloniaCommunity/Classic.Avalonia)
* [ScottPlot](https://scottplot.net/) — charts and the impact scatter
* [SkiaSharp](https://github.com/mono/SkiaSharp) — Cross-plaform 2D graphics for reticle rendering
* [Iciclecreek.Avalonia.WindowManager](https://github.com/tomlm/Iciclecreek.Avalonia.WindowManager) — the MDI
  window surface

## Project structure

```
Common/
  BallisticCalculator.Types      Shared models and domain logic, no UI — the layer a mobile app will reuse
  BallisticCalculator.Controls   Shared controls: measurement entry, reticle canvas, chart, table
  BallisticCalculator.Panels     Input and output panels assembled from those controls
Desktop/
  BallisticCalculator            The calculator application (MDI, menus, dialogs)
  ReticleEditor                  The reticle editor
  DebugApp, DebugApp1            Harnesses for exercising controls and panels by hand
Tools/
  DependencyUpdater              `depupdate` — bumps dependencies within their declared version bounds
Mobile/                          Reserved for the Android application
data/                            Reticles, drag tables, dictionaries and the sample ammunition library
```

Every library has an xUnit test project beside it; UI tests run headless through `Avalonia.Headless`.

## Building

```
dotnet build BallisticCalculator2.sln          # or BuildDebug.bat
dotnet test BallisticCalculator2.sln           # or TestDebug.bat
Setup\prepare.bat                              # publish win-x64 + linux-x64 and pack the portable archive
```

.NET 8 SDK is the only prerequisite. `App.bat` runs the calculator and `ReticleEditor.bat` the editor.

## License

GNU General Public License v2 — see [LICENSE](LICENSE).
