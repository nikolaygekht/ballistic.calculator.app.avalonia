# Screenshot catalogue

Captured **2026-07-27** on Windows 11. Sixteen PNGs from the Avalonia desktop app
(`Desktop/BallisticCalculator`) and the reticle editor (`Desktop/ReticleEditor`).

**Five are used by the root `README.md`**: `ballistic_table.png` as the hero, and `reticle.png`,
`compare_charts.png`, `hit_probability.png`, `custom_drg.png` as a clickable thumbnail strip under
"What it does". Deliberately restrained — a README is not documentation, and the detailed manual is
still to be written. The other eleven, including all eight Shot Parameters tab captures, are held
here for that manual.

On GitHub a bare `![]()` image is not clickable, so each one is wrapped in
`<a href="doc/screenshots/…"><img width="…"></a>`: small preview in the page, full size on click.
Widths are set per image because the set mixes aspect ratios — landscape trajectory windows
(~1740×866), portrait dialogs (~1024×1035) and the very wide reticle editor (2260×876) cannot share
one width without something becoming either illegible or overwhelming.

Windows-only is deliberate: Avalonia renders the app identically on Linux and under WSL, so a Linux
capture would be indistinguishable from these and is not worth the upkeep.

## The shot used

Almost every capture is the same load, so the numbers agree across images:

| | |
|---|---|
| Ammunition | `.223 69gr Sierra (16in)` — 69 gr, BC 0.365 G1, 2,600 ft/s, Ø0.224 in, 0.98 in long |
| Weather | ICAO standard — 0 ft, 29.92 inHg, 59 °F, 78 % humidity |
| Wind | three zones — 3 m/s @ 97° from 0 yd, 5 m/s @ 77° from 300 yd, 4 m/s @ 52° from 500 yd |
| Rifle | M16 iron sight, 2.6 in sight height, 0.5 / 1 in-100yd clicks; M16A3/M4 barrel, right twist 1:7 in |
| Zero | 300 yd |
| Run | 0–1,000 yd in 100 yd steps, Coriolis on (azimuth 2°, latitude 55° N) |

`hit_probability.png` is the exception — it uses `.223 Rem 55gr FMJ (16in)`, and `custom_drg.png`
is a different projectile entirely (a .338 radar dataset).

## Shot Parameters dialog — the six tabs

The dialog is `Trajectory → Parameters`; the files are numbered in tab order.

| File | Tab | What it shows |
|---|---|---|
| `params_1_ammo.png` | Ammunition | Bullet identity and drag inputs: name with Load/Save to the library, weight, BC with drag-table selector (G1), the *BC is Form Factor* switch, a `Browse…` slot for a custom `.drg` table (empty → "(standard table)"), muzzle velocity, diameter, length, plus descriptive caliber / bullet type / barrel length / source fields |
| `params_1_ammo_gc.png` | Ammunition | The same tab driving a **custom `.drg` drag table** instead of a standard one — the counterpart to the shot above. `6.5mm Lapua GB546 Scenar-L 8.8g (136gr)`: the drag-table slot holds `6,5mm-lapua gb546 8,8g (136gr) scenar-l_radar.drg`, the BC unit is **GC** (custom curve) with a value of 1.000, and *BC is Form Factor* is ticked — the form-factor-of-1 convention a radar-derived table carries. Source "Radar Data". Everything except **Muzzle Vel.** came out of the `.drg` — the file describes the projectile but not the load, so the velocity is always the user's to enter. Also a good illustration of precision transparency: the metric source values survive as 135.80475 gr, Ø0.26417 in, 1.33858 in rather than being rounded to the control's precision |
| `params_2_weather.png` | Weather | Atmosphere: altitude, pressure, temperature, humidity, and *Reset to Standard*. Shown at ICAO standard values |
| `params_3_wind.png` | Wind | Multi-zone wind — three stacked zones, each with direction, velocity and a start distance, an arrow dial rendering the direction, and a per-zone `X` to remove it. The first zone's `X` is disabled (it starts at 0 yd) |
| `params_4_rifle.png` | Rifle | Sight preset with sight height and per-click H/V adjustment values; barrel preset with rifling direction and twist rate (needed for spin drift) |
| `params_5_zero.png` | Zero | Zero distance and shot angle, optional impact offset at zero (V/H), optional *other ammunition for zero*, and the start of *other atmosphere for zero*. The optional groups are unchecked, so their fields are greyed. The tab scrolls — the atmosphere group is cut off at the bottom edge |
| `params_5_zero_1.png` | Zero (scrolled) | The bottom half of the same tab, completing it: the rest of the *other ammunition for zero* group (BC with drag-table selector, form-factor switch, custom `.drg` slot, muzzle velocity, diameter, length), the whole *other atmosphere for zero* group with its own *Reset to Standard*, and *Wind at zero* — direction with dial, velocity, optional distance. All three groups unchecked, so everything is greyed |
| `params_6_shot.png` | Parameters | Run settings: max range and step; shot angle; dialed V/H clicks; and the Coriolis group — azimuth with a compass dial, latitude with N/S selector |

## Trajectory window — the four views

One child window with `Table` / `Chart` / `Reticle` / `Summary` tabs, titled with the ammunition name.

| File | View | What it shows |
|---|---|---|
| `ballistic_table.png` | Table | Full solution 0–1,000 yd: range, velocity, Mach, drop, hold, clicks, windage, windage adjustment, clicks, time of flight, energy, optimal game weight. Zeroed at 300 yd, so drop crosses zero there and reaches −473 in at 1,000 yd |
| `chart.png` | Chart | The same drop curve plotted against range, one series, markers at each 100 yd step |
| `reticle.png` | Reticle | Mil-Dot reticle with the *Target* overlay selected: a 6 × 6 in target box at 100 yd drawn to scale on the reticle, with the angular size echoed underneath (6.00 × 6.00 in/100yd). The panel also offers *None* / *Far BDC* / *Near BDC* overlays and a `Load…` for a custom reticle |
| — | Summary | **Not captured** |

## Other windows

| File | Window | What it shows |
|---|---|---|
| `compare_charts.png` | Compare | Two trajectories on one chart with a legend — `.223 Rem 55gr FMJ (16in)` against `.223 69gr Sierra (16in)`. The curves separate past ~450 yd, and the heavier, higher-BC bullet drops visibly less at 1,000 yd. This is the shot that shows off comparison; `chart.png` is the single-curve version |
| `hit_probability.png` | Tools → Hit Probability, after *Estimate* | Left: target distance and vital-zone size, shooter group size and position with H/V spread multipliers, range and wind estimation error, muzzle-velocity deviation, and the shot count / RNG seed. Right: the result — **18.3 %** single-shot at 300 yd on an 8 in vital zone with a 2 MOA supported group, the shots-for-a-first-hit row (4 / 7 / 12 / 15 / 20 for 50–98 %), and the impact scatter with the vital-zone circle. 10,000 shots simulated, 2,000 plotted |
| `custom_drg.png` | Tools → Approximate Drag Table — From Measured Velocities | The radar-data `.drg` builder with a real dataset loaded: Warner 338 Flatline, 285 gr, Ø0.338 in, 2.05 in, 16 readings from `velocity1.csv` spanning 0–1,500 yd and 3,078.8 → 1,994.6 ft/s. Shows the readings grid, the add/change/delete/sort/load-CSV row, and `Set Atmosphere` / `Save Drg` |
| `reticleeditor.png` | Reticle Editor (separate app) | `M-16 Iron Sight 2" Eye Relief` mid-edit. Right: the reticle's own coordinate space — 350 × 350 moa with zero at 175 / 175 moa — over the element list, where each element is spelled out as text (`Rectangle(p=…,s=…,c=black,f=true)`, a `Path` outlining the field, `Circle(p=(0moa:0moa),r=60moa,w=1mil,c=gray)`), with an element-type picker (`Line`) and New / Edit / Duplicate / Delete / Up / Down. Left: the live render — the iron-sight picture of a black front post inside the white aperture field, with the gray 60 moa reference circle. The status bar tracks the cursor in moa and reports the last action ("Edit cancelled") |

## Not captured yet

Gaps against the wish list in [`../../claude/SCREENSHOTS.md`](../../claude/SCREENSHOTS.md):

- **Approximate Drag Table → From BC Curve** — the sibling of `custom_drg.png`, with knots loaded.
- **Summary view** of the trajectory window.
- **Ammunition / reticle library** browsing.

Two wish-list items are now closed: the reticle editor is `reticleeditor.png`, and the **Linux**
shot is dropped by decision (2026-07-27) — Avalonia looks the same there, so it would prove nothing
a reader could see. If the README wants to back the cross-platform claim, it needs words, not a
screenshot.

## Notes for a retake

Nothing here is broken enough to block use in the README; these are the things to fix if the set is
recaptured.

- `hit_probability.png` uses `.223 Rem 55gr FMJ` where most shots use the 69 gr Sierra, and its title
  bar names it. **Not worth a retake** — that cartridge is already introduced by
  `compare_charts.png` as the deliberate second load, so it reads as part of the same session. A
  caption naming the load is enough if the image lands near `ballistic_table.png`.
- `params_4_rifle.png` (barrel preset) and `params_6_shot.png` (latitude N/S) were captured with a
  combo box focus-highlighted. Reviewed and **accepted as-is** (2026-07-27) — the highlight reads as
  a normal focused control, not as an artefact. Not a retake item.
- `params_1_ammo_gc.png` has an **empty Muzzle Vel.** field. Correct behaviour, not a defect: a
  `.drg` carries the projectile (name, source, weight, diameter, length) but no muzzle velocity, so
  loading one always leaves that field for the user. Still worth typing a velocity in before the
  image goes in a README — a blank required field photographs as an unfinished form.
- `params_5_zero.png` is cut off mid-scroll, which is honest — the tab really does scroll — and
  `params_5_zero_1.png` now covers the remainder, so the two together document the whole tab. Use
  them as a pair; a single taller capture would need a window size no other shot uses.
- `custom_drg.png` happens to show [D-002](../../claude/DEFECTS.md) — 16 readings loaded, six rows
  visible, no vertical scroll bar on the grid. Useful as evidence while the defect is open, and no
  reason to retake once it is fixed: a scroll bar appearing is not a change anyone reads a
  screenshot for.
- The file name `custom_drg.png` does not say which of the two drag-table dialogs it is. If a
  From-BC-Curve shot is added, rename the pair to something like `drg_from_velocities.png` /
  `drg_from_bc.png`.
