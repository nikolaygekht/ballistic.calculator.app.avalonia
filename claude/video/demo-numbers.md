# Demo script — verified numbers

Every figure quoted in `demo-script.md`, computed through the application's own
`ShotTrajectoryCalculator` and `ZeroingCalculator` (BallisticCalculator 1.1.13). If a number on screen
disagrees with a number here, the recipe was typed differently — check the inputs before re-recording.

Angular units: **milliradian** throughout. Windage sign follows the library: **left positive, right
negative**; the tables below print the sign the engine returns, so a wind from the right reads positive
and spin drift from a right twist reads negative.

**Structure.** Episodes 3 to 6 each start from the *same saved baseline* and add **one** feature, so
every comparison in those episodes is against §2 and nothing else. Episode 7 is the only cumulative one.
That is why the tables below are deltas from the baseline rather than a chain.

---

## 1. Episode 1 — metric baseline: Tigr + PSO-1

`11.3 g · 0.253 G7 · 790 m/s · Sight Preset "PSO-1 Optics" (65 mm, 0.5 ths clicks) · 300 m zero ·
default weather · no wind`

Read off the application by UI Automation on 2026-11-03. With 0.5 ths clicks the table's angular columns
are in **thousandths (th)**, not mrad.

Zero elevation **2.85 th**.

| Range | Velocity | Mach | Drop | Hold (th) | Clicks | Energy | Time |
|---|---|---|---|---|---|---|---|
| 0 m | 790 m/s | 2.32 | −6.5 cm | — | — | 3526 J | 0.00 s |
| 100 m | 732 m/s | 2.15 | **+15.1 cm** | 1.44 | 3 | 3025 J | 0.13 s |
| 200 m | 676 m/s | 1.99 | +18.3 cm | 0.87 | 2 | 2582 J | 0.27 s |
| 300 m | 623 m/s | 1.83 | **0.0 cm** | 0.00 | 0 | 2192 J | 0.43 s |
| 400 m | 572 m/s | 1.68 | −43.6 cm | −1.04 | −2 | 1849 J | 0.60 s |
| 500 m | 523 m/s | 1.54 | −117.3 cm | −2.24 | −4 | 1548 J | 0.78 s |
| 600 m | 477 m/s | 1.40 | −227.0 cm | −3.61 | −7 | 1283 J | 0.98 s |
| 700 m | 432 m/s | 1.27 | −380.0 cm | −5.18 | −10 | 1052 J | 1.20 s |
| 800 m | 389 m/s | 1.14 | −585.9 cm | −6.99 | −14 | 854 J | 1.44 s |
| 900 m | 349 m/s | 1.03 | −857.0 cm | −9.09 | −18 | 688 J | 1.71 s |
| 1000 m | 322 m/s | 0.95 | **−1208.7 cm** | −11.54 | −23 | 585 J | 2.02 s |

Summary: subsonic at 923 m; near zero 23 m, far zero 300 m.

Spoken: *"fifteen centimetres above"* at 100 m; *"twelve metres below the line of sight"* at 1000 m (−12.09 m).

---

## 2. Episode 2 — THE BASELINE: AR-style .308 + standard optics

`175 gr · 0.253 G7 · 2600 ft/s · Sight Preset "Standard Optics (High, Mil)" (3 in, 0.1 mrad clicks) ·
300 yd zero · default weather · no wind`

**This is the file saved as `bc2-demo.trajectory` and reopened at the start of episodes 3, 4, 5 and 6.**
Every delta in §3 to §6 is measured against this table.

Read off the application by UI Automation on 2026-11-03 (the sight changed from 2 in to 3 in that day —
**§3 to §7 still hold the 2 in numbers and must be recomputed** before those episodes are voiced).

Zero elevation **2.76 mrad**.

| Range | Velocity | Mach | Drop | Hold | Clicks | Energy | Time |
|---|---|---|---|---|---|---|---|
| 0 yd | 2600 fps | 2.33 | −3.0 in | — | — | 2626 ftlb | 0.00 s |
| 100 yd | 2425 fps | 2.17 | +4.2 in | 1.17 | 12 | 2284 ftlb | 0.12 s |
| 200 yd | 2256 fps | 2.02 | +5.5 in | 0.77 | 8 | 1978 ftlb | 0.25 s |
| 300 yd | 2095 fps | 1.88 | **0.0 in** | 0.00 | 0 | 1705 ftlb | 0.39 s |
| 400 yd | 1940 fps | 1.74 | −13.5 in | −0.94 | −9 | 1463 ftlb | 0.53 s |
| 500 yd | 1791 fps | 1.60 | −36.2 in | −2.01 | −20 | 1247 ftlb | 0.70 s |
| 600 yd | 1648 fps | 1.48 | −69.8 in | −3.23 | −32 | 1055 ftlb | 0.87 s |
| 700 yd | 1510 fps | 1.35 | −116.2 in | −4.61 | −46 | 886 ftlb | 1.06 s |
| 800 yd | 1377 fps | 1.23 | −177.9 in | **−6.18** | **−62** | 736 ftlb | 1.27 s |
| 900 yd | 1250 fps | 1.12 | −258.1 in | −7.96 | −80 | 607 ftlb | 1.50 s |
| 1000 yd | 1132 fps | **1.01** | **−360.5 in** | −10.01 | −100 | 498 ftlb | 1.75 s |

**Summary view:** goes subsonic at **1014 yd**; near zero 33 yd, far zero 300 yd.

**Mil-Dot reticle, Far BDC** — labels read off the screen. The built-in Mil-Dot has **four** dots below the centre, then the
thick post from 5 mrad:

| Mark | Range |
|---|---|
| 1st dot (1 mrad) | 406 yd |
| 2nd dot (2 mrad) | 499 yd |
| 3rd dot (3 mrad) | 582 yd |
| 4th dot (4 mrad) | 657 yd |
| top of the post (5 mrad, no label) | ≈ 725 yd (interpolated from the table: 700 yd −4.61, 800 yd −6.18) |

Ep. 5's "62 clicks" survives the sight change: the 800 yd hold is −6.18 mrad → 62 clicks.

---

## 3. Episode 3 — weather, then wind

Read from the app on 2026-11-03 (`build/ep03/read_numbers.py`): open `bc2-demo`, then edit.

### 3a. Baseline + February weather

Added: `1000 ft · 28.9 inHg (station) · 40 °F · 60 % RH`.

| Range | Velocity | Mach | Drop | Hold | vs baseline |
|---|---|---|---|---|---|
| 300 yd | 2096 fps | 1.91 | 0.0 in | 0.00 | — |
| 600 yd | 1652 fps | 1.51 | −69.65 in | −3.22 | +0.1 in |
| 800 yd | 1382 fps | 1.26 | −177.45 in | −6.16 | +0.5 in |
| 1000 yd | 1136 fps | 1.04 | **−359.18 in** | −9.98 | **+1.3 in** (baseline −360.51) |

Only **1.3 in** at 1000 yd — the cold makes the air denser, the altitude thinner; they nearly cancel.
The narration says "about one inch" rather than overselling it.

### 3b. + 10 mph wind at 90° (from the right), no distance limit

Drop is unchanged from 3a. Windage (the bullet goes left):

| Range | Windage | Wind hold | Clicks (0.1 mrad) |
|---|---|---|---|
| 300 yd | 6.94 in | 0.64 | 6 |
| 600 yd | **31.14 in** | 1.44 | 14 |
| 800 yd | 60.43 in | 2.10 | 21 |
| 1000 yd | **104.17 in** (8.7 ft) | **2.89** | **29** |

---

## 4. Episode 4 — spin drift, aerodynamic jump, Coriolis

Read from the app on 2026-11-03 (`build/ep04/read_numbers.py`), starting from `bc2-demo` (episode 2's
baseline, 3 in sight; standard air, no wind). Windage is signed, **positive is left**.

### 4a. Baseline

| Range | Drop | Windage |
|---|---|---|
| 600 yd | −69.78 in | 0.00 |
| 800 yd | −177.92 in | 0.00 |
| 1000 yd | **−360.51 in** | 0.00 |

### 4b. + bullet shape and rifling, NO wind — spin drift alone

Added: Ammunition `Diameter 0.308 in · Length 1.394 in`; Rifle `Rifling Right · Twist Rate 10 in`.

| Range | Drop | vs 4a | Windage | Wnd.Adj. | Clicks |
|---|---|---|---|---|---|
| 600 yd | −69.78 in | **0.00** | −1.55 in (right) | −0.07 mrad | −1 |
| 800 yd | −177.92 in | **0.00** | −3.92 in | −0.14 mrad | −1 |
| 1000 yd | **−360.51 in** | **0.00** | **−8.00 in (8 in right)** | −0.22 mrad | −2 |

The drop is identical to the baseline: no aerodynamic jump without wind.

### 4c. + wind 10 mph from 90° (from the right), no distance limit

Episode 4 shows only the **drop** here (jump: −360.51 → −356.51, 4.0 in up); the windage is not discussed.

| Range | Drop | vs 4b | Windage | Wnd.Adj. | Clicks |
|---|---|---|---|---|---|
| 300 yd | +1.20 in | +1.2 | 6.97 in | 0.65 mrad | 6 |
| 600 yd | −67.38 in | +2.4 | 29.74 in | 1.38 mrad | 14 |
| 800 yd | −174.72 in | +3.2 | 56.85 in | 1.97 mrad | 20 |
| 1000 yd | **−356.51 in** | **+4.0 (up)** | **96.83 in (left)** | 2.69 mrad | 27 |

The drop change is **aerodynamic jump**. Wind alone on the baseline ≈ 96.83 + 8.00 = **104.8 in** left at
1000 yd (the script says "about one hundred and five"; episode 3's 104.17 had the February weather on).

### 4d. Baseline + Coriolis only

Added: Parameters `Calculate Coriolis Effect` · Azimuth · `Latitude 45 N`. No shape, no twist, no wind.

| Azimuth | 600 yd drop | 1000 yd drop | vs 4a | 1000 yd windage |
|---|---|---|---|---|
| 90° (east) | −68.73 in | **−356.70 in** | **+3.8 (up)** | −3.25 in (right) |
| 270° (west) | −70.83 in | **−364.32 in** | **−3.8 (down)** | −3.25 in (right) |
| 0° (north) | −69.78 in | −360.51 in | 0.0 | −3.25 in (right) |
| 180° (south) | −69.78 in | −360.51 in | 0.0 | −3.25 in (right) |

The horizontal part does not depend on the azimuth (−3.25 in every direction); due north or south the
vertical part is exactly zero (read from the app).

---

## 5. Episode 5 — dialled clicks, then the zero's own weather

Read from the app on 2026-11-04 (`build/ep05/read_numbers.py`), starting from `bc2-demo` (episode 2's
baseline, 3 in sight, 300 yd zero; standard air, no wind).

### 5a. Baseline, then V-Clicks 62

The baseline hold at 800 yd is **−6.18 mrad**, Clicks **−62** (0.1 mrad). Parameters tab **V-Clicks `62`**
(a count; positive is up — the 800 yd row goes to zero).

| Range | Drop before | Drop after | Hold after |
|---|---|---|---|
| 300 yd | 0.00 in | +66.96 in | 6.20 |
| 400 yd | −13.47 in | **+75.81 in** | 5.26 |
| 600 yd | −69.78 in | +64.13 in | 2.97 |
| 800 yd | −177.92 in | **+0.63 in** | 0.02 |
| 1000 yd | −360.51 in | −137.30 in | **−3.81** |

V-Clicks is a NumericUpDown: no automation id of its own; its text box is the first `PART_TextBox` under
`ParametersSubPanel` (H-Clicks is the second).

### 5b. July air on the Weather tab, the zero inheriting it

Shot air `1000 ft · 28.9 inHg · 100 °F · 90 %`, Zero tab untouched. (Every typed field read back and
checked by `read_numbers.py`.)

| Range | Drop | Hold |
|---|---|---|
| 300 yd | 0.00 in | 0.00 |
| 600 yd | −66.03 in | −3.06 |
| 800 yd | −165.57 in | −5.75 |
| 1000 yd | **−328.59 in** | −9.13 |

Versus episode 3's February table (−359.18 at 1000 yd): **30.6 in less** ("thirty").

### 5c. + Zero tab, Other atmosphere for zero = February

Zero air `1000 ft · 28.9 inHg · 40 °F · 40 %` (Zero tab → scroll down → `Other atmosphere for zero`).
Episode 3 had 60 % in February; at 40 °F the humidity is worth 0.01 in, so the episode uses a realistic
dry winter 40 %.

| Range | Drop | vs 5b | Hold |
|---|---|---|---|
| 300 yd | **+0.42 in** | +0.42 | 0.04 |
| 600 yd | −65.18 in | +0.85 | −3.02 |
| 800 yd | −164.43 in | +1.14 | −5.71 |
| 1000 yd | **−327.17 in** | **+1.42** | −9.09 |

(With 60 % zero humidity: −327.18 at 1000 yd, +0.42 at 300 yd.) The 300 yd row is no longer zero: the
rifle hits almost half an inch high at its own zero distance.

### 5d. The same pair with a 600 yd zero

| | 600 yd drop | 1000 yd drop |
|---|---|---|
| July air, zero inherits it | 0.00 in | −218.54 in |
| + February zero air (40 %) | **+4.50 in** | **−211.04 in** (**+7.50**) |

The zero-air share grows with the zero distance: 1.4 in at a 300 yd zero, 7.5 in at a 600 yd zero.

---

## 6. Episode 6 — the measured drag curve

Read from the app on 2026-11-04 (`build/ep06/read_numbers.py`; the take printed −370.81 again). Radar file
`30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg` (copied into `Documents\Ballistic Calculator demo`); Browse
gives BC `1.000 GC`, form factor on, weight **174.38565 gr** (11.30 g), diameter 0.30827 in, length 1.3937 in, name
`.308 Lapua GB550 Scenar-L 11.3 ( 175gr )`, source `Radar Data`. The take builds it as **Trajectory → New → Imperial**
(MV 2600, Standard Optics (High, Mil), zero 300 yd, standard weather), next to the baseline window.

`bc2-demo.trajectory`'s load was renamed **Baseline G7** on 2026-11-04 (for the Compare legend); backup of the
unnamed one in the session scratchpad.

| Range | G7 0.253 (baseline) | Radar .drg | G1 0.505 |
|---|---|---|---|
| 600 yd | −69.78 in · 1648 fps | −71.14 in · 1616 fps | −69.63 in · 1656 fps |
| 800 yd | −177.92 in · 1377 fps | −182.48 in · **1352 fps** (−25) | −176.97 in · 1407 fps |
| 1000 yd | **−360.51 in** · −10.01 mrad · −100 | **−370.81 in** · −10.30 mrad · **−103** | −355.03 in · 1207 fps |

Radar vs G7: **10.3 in** more drop at 1000 yd, **3 clicks**. G1 0.505 vs radar: **15.8 in** ("sixteen").

The real G7 BC along the flight (`chart.py`: SD·Cd_G7(M)/Cd_radar(M), Mach from the app's table): 0.250 at the
muzzle, **0.239 at ~285 yd**, back to 0.253 near 650 yd, **0.290 at 1000 yd** (Mach 1.01).

---

## 7. Episode 7 — everything at once

**Read from the app 2026-11-04** (`build/ep07/read_numbers.py`; the same numbers came up in the take, printed by
`build/ep07/record.py`). Opens `bc2-demo-radar.trajectory` (ep. 6: radar `.drg`, which also supplies diameter
and length) and adds, one step at a time: rifling **1:10 right** · Coriolis **azimuth 90° (east), latitude 45 N**
· July air **1000 ft / 28.9 inHg / 100 °F / 90 %** with the Zero tab's February air **1000 ft / 28.9 / 40 °F /
40 %** (the ep. 5 pair) · wind **10 mph from 90°**. No dialled clicks.

| Step | Drop at 1000 yd | Windage at 1000 yd | Drop at 300 yd |
|---|---|---|---|
| Radar baseline | −370.81 in | 0 | 0 |
| + twist 1:10 right | −370.81 in | −8.22 in (right) | 0 |
| + Coriolis, east, 45 N | −366.91 in | −11.51 in | +0.22 in |
| + July air / February zero air | −332.87 in | −11.04 in | +0.62 in |
| + wind 10 mph from 90° | **−328.66 in** (jump: +4.2) | **+81.69 in** (left) | **+1.88 in** |

Everything-on table at 600 yd: drop −62.96 in, hold −2.91 mrad, windage +26.02 in.

**Mil-Dot, Far BDC (read off the screen)** — radar baseline **405 / 496 / 578 / 651 yd**, everything on
**428 / 522 / 607 / 684 yd**: every dot is worth 20–30 yd more.

**Near BDC** labels only the dots *below* the centre: **22 / 18 / 15 / 12 yd** (1–4 mrad). The bullet is also
above the line of sight between the near and far zero (+1.18 mrad at 100 yd), but the library's Mil-Dot declares
BDC points below the centre only — library defect #1 (`BallisticCalculator.Net/CLAUDE/DEFECTS.md`,
gehtsoft-usa/BallisticCalculator1#50). The narration speaks only of the very short ranges.

**Target**: 20 × 20 in at 600 yd = **0.93 × 0.93 mrad**, the box low and left of the centre (at 800 yd it sits on
the bottom edge of the field, so 600 was used).

**Trijicon VCOG-16-762-175gr, Far BDC** (etched in hundreds of metres for its own load and zero): etched 4 =
**593 yd**, 6 = **809**, 8 = **1031**, 10 = **1263**; the unnumbered ticks between are 490 / 702 / 918 / 1143 yd.

---

## 8. Episode 8 — 300 Blackout, zeroed with the other cartridge

Both loads and the rifle come from `data/reticle/VCOG16-300BLK.md`, which fitted them to Trijicon's
published sheet to 0.17 / 0.18 MOA RMS. **ICAO sea level, no wind, no Coriolis** — the fit was made in
that air, and adding episode 3 to 7's effects here would break the match with the etched diamonds.

| | Supersonic (the zero) | Subsonic (the shot) |
|---|---|---|
| Weight | 115 gr | 208 gr |
| BC | 0.290 G1 | 0.648 G1 |
| Muzzle velocity | 2330 ft/s | 1010 ft/s |
| Diameter / length | — | 0.308 in / 1.44 in |

Rifle: sight height **2.9 in**, clicks 0.1 mrad, rifling **1:8 in right**. Zero **100 m**, ICAO sea
level. Parameters 200 m in 25 m steps.

Zero elevation **1.805 mrad** — the supersonic load's, not the subsonic load's (which alone would need
6.035 mrad).

| Range | Velocity | Drop | Hold |
|---|---|---|---|
| 25 m | 305 m/s | −6.1 cm | −2.44 |
| 50 m | 302 m/s | −11.4 cm | −2.29 |
| 75 m | 300 m/s | −23.5 cm | −3.13 |
| 100 m | 297 m/s | **−42.3 cm** | −4.23 |
| 150 m | 293 m/s | −100.9 cm | −6.73 |
| 200 m | 288 m/s | **−188.1 cm** | −9.40 |

Sanity contrast for the narration: zeroed with **itself**, the subsonic load would of course be dead on
at 100 m. Zeroed with the supersonic load it is **42 cm low** there.

### The computed holds against the etched diamonds

`VCOG16-300BLK.reticle` draws three subsonic diamonds. They carry **no** BDC anchors — they belong to a
different load than the ladder — so the labels the application draws are computed entirely from the
shot above.

| Range | Computed hold | Diamond centre in the file | Difference |
|---|---|---|---|
| 50 m | −7.859 MOA (−2.29 mrad) | −7.859 MOA | **0.000 MOA** |
| 100 m | −14.541 MOA (−4.23 mrad) | −14.540 MOA | −0.001 MOA |
| 150 m | −23.121 MOA (−6.73 mrad) | −23.121 MOA | **0.000 MOA** |

### Read from the app (2026-11-04, `build/ep08/read_numbers.py`) — what the episode shows

Both loads are **loaded from the library** (`data/ammo/.300 AAC/`), then the MV is typed over:
`.300 AAC 208gr Hornady` (G1 0.648, 208 gr, library **1020** → **1010** ft/s) and, as Other ammunition
for zero, `.300 AAC 115gr` (G1 0.290, 115 gr, library **2295** → **2330** ft/s). Rifle: sight preset
**Standard Optics (High, Mil)** → height 3 → **2.9 in**; Right, **8 in**. The preset sets the zero to
**100 yd** — switch the unit to **m** (missed once: the supersonic check showed −0.31 cm at "100 m").

Subsonic, zeroed 100 m with the supersonic load, 200 m / 25 m:

| m | 0 | 25 | 50 | 75 | 100 | 125 | 150 | 175 | 200 |
|---|---|---|---|---|---|---|---|---|---|
| Drop, cm | −7.37 | −6.11 | −11.43 | −23.46 | **−42.30** | −68.07 | −100.89 | −140.86 | **−188.10** |
| Hold, mrad | | −2.44 | −2.29 | −3.13 | −4.23 | −5.45 | −6.73 | −8.05 | −9.40 |

Velocity 308 m/s at the muzzle, 297 m/s at 100 m.

**Reticle `VCOG16-300BLK.reticle`, Far BDC, subsonic window.** Far BDC labels only ranges past
**Split at** (`BdcSplitControl`, default = the zero, 100 m). With 100: the top bar is unlabelled, the next
bars read **102 / 151** m (and the bottom one 208 m). With **Split at = 50** (Nikolay checked in the app):
**55 / 102 / 151** m from the top of the ladder down — the subsonic 50 / 100 / 150. Split 50 does not
disturb the supersonic labels either (checked).

**Supersonic alone, zeroed 100 m** (second trajectory): holds 50 −0.18, 100 0.00, 150 −0.37, 200 −0.93,
300 −2.41, 400 −4.35, 500 −6.80, 600 −9.82 mrad; Far BDC labels **200 / 300 / 400 / 500 / 600 m**, as on
Trijicon's sheet.

**Target 19 × 19 in** (subsonic): 9.65 mrad at 50 m, 4.83 at 100, 3.22 at 150; the 100 and 150 m boxes
sit on the bars. Not in the script.

**Near BDC (subsonic) — app defect, not shown:** lower bars 14 / 9 / 7 m, but the top bar gets two
overlapping labels (≈25 m and ≈55 m: the bullet crosses that hold twice). Not yet filed anywhere.

**Display mode lost across windows — app defect (found in the Ep. 8 take, 2026-11-05):** the
None / Far BDC / Near BDC radios in `ReticlePanel.axaml` use `GroupName="DisplayMode"`, and Avalonia scopes a
group to the root window, so every trajectory window shares one group. Picking Far BDC in window 2 clears the
radios in window 1, and window 1's reticle loses its BDC labels. The take works around it (r05 re-clicks Far BDC
in window 1). Filed as D-003 in `claude/Archive/DEFECTS.md`.

---

## Reproducing these numbers

The harness lives in the session scratchpad, not in the repository: a `net8.0` console project with a
`ProjectReference` to `Common/BallisticCalculator.Types`, the repo's `NuGet.config` beside it, one `Opt`
record per feature flag (weather / July / zero-atmosphere / wind / shape / Coriolis / clicks / drg / G1),
building `ShotData` and calling `ShotTrajectoryCalculator.Calculate` and `.CalculateFine`.

Gotchas that cost time:

- The `.drg` path must be **absolute**, or the file must sit under `data/drg` beside the executable, for
  `CustomDragTableLoader.ResolvePath` to find it.
- For a `GC` load, set `Ammunition.CustomTableFileName` **and** a form-factor BC of 1.0
  (`BallisticCoefficientValueType.FormFactor`) — **and a bullet diameter**, or the library throws
  `ArgumentException` for a form factor with no diameter.
- `ShotData.Weapon` still needs a `ZeroingParameters` of its own even when `ShotData.Zeroing` carries the
  real zero — `ZeroingCalculator.BuildInputs` reads `weapon.Zero` as the fallback for every field.
- `DropAdjustment` blows up near the muzzle (the bullet is below the sight line), so any search for
  "which range sits at N mrad" must skip everything inside the zero distance.
- Positive `ShotParameters.ShotDropAdjustment` = clicks dialled **up**. Negative doubles the drop.
- The `.drg` header carries the bullet: `11.30 g / 7.83 mm / 35.4 mm` for the GB550 — that is where the
  episode 4 diameter and length come from.

---

## 9. Shooting at an angle

Read from the app on 2026-11-05 (`build/ep09/read_numbers.py`): `bc2-demo` (.308 175 gr G7 0.253, 2600 ft/s,
zero 300 yd, standard air), Parameters → Shot Angle. Hold in mrad (= Over Line of Sight; the Hold column does
not change with View → Drop).

| Range | Level: drop · hold | 30° uphill | 30° downhill |
|---|---|---|---|
| 300 yd | −0.00 in · 0 | +3.57 in · +0.33 | +3.60 in · +0.33 |
| 600 yd | −69.78 in · −3.23 | −52.85 in · −2.45 | −52.87 in · −2.45 |
| 800 yd | −177.92 in · −6.18 | −143.28 in · −4.97 | −144.45 in · −5.02 |
| 1000 yd | −360.51 in · −10.01 | −296.49 in · −8.24 | −302.31 in · −8.40 |

- Up and down are almost the same; the small difference at 1000 yd is the air (downhill the bullet goes ~1500 ft
  lower into denser air: 1124 vs 1140 ft/s).
- Over Muzzle Level, 30° uphill: +17,658 in at 1000 yd (1471 ft above the muzzle; the target is 1500 ft up);
  downhill −18,349 in. On the chart (Drop, Over Muzzle Level) the two curves — bullet and line of sight — are
  straight slanted lines that nearly overlap at full scale; the ~300 in gap needs a zoom to be seen.
- **Rifleman's rule** (hold for the horizontal distance, 1000 × cos 30° = 866 yd): level 866 yd → −7.33 mrad
  (−228.52 in), but the real hold at 1000 yd / 30° is −8.24 mrad: the rule leaves the shot ~0.9 mrad
  (≈33 in) low at 1000 yd.

## 10. Your own drag table

Read from the app 2026-11-05 (`build/ep10/probe.py`), library 1.1.13.

**Part 1 — five G7 BCs taken off Lapua's radar curve** (`BC(M) = SD · Cd_G7(M) / Cd_drg(M)`, standard air,
1116.45 ft/s): 0.250 @ 2600, 0.240 @ 2200, 0.244 @ 1800, 0.263 @ 1400, 0.293 @ 1100 ft/s (Mach 2.329 … 0.985).
Built with From BC Curve (175 gr, 0.308 in, base G7; the BC field's own table must be switched from G1 to G7),
saved as `scenar-l-5bc.drg` (84 points) and put on `bc2-demo`:

| bc2-demo at 1000 yd | Drop, in | Hold, mrad | Velocity, ft/s |
|---|---|---|---|
| one BC, G7 0.253 | −360.51 | −10.01 | 1,132 |
| five BCs | −370.01 | −10.28 | 1,135 |
| Lapua radar `.drg` | −370.81 | −10.30 | 1,127 |

Three BCs (2600/2000/1400) already give −368.4 in (library console run).

**Part 2 — Warner .338 Flatline 285 gr, 16 published velocities 0–1500 yd** (3078.8 → 1994.6 ft/s), built
with From Measured Velocities in the air of the range: 6000 ft, 23.98 inHg, 78 °F, 25 % — density 0.770 of
standard, density altitude ≈ 8,650 ft. That air is fitted, not logged: it is the one at which the velocity table
reproduces the sheet's own CD column (WarnerReportTest in the library). EMRTC (New Mexico Tech, Socorro NM): town
~4,600 ft. Recovered CD vs the sheet: within 0.5 % at Mach 1.75–2.5; with standard air every CD comes out ~22 % low.

`338-flatline.trajectory` (3079 ft/s, sight 1.5 in, zero 100 yd, standard air, to 1500 yd):

| at 1500 yd | Hold, mrad | Drop, in |
|---|---|---|
| Warner's G7 0.484 | −10.36 | −559.56 |
| our velocity curve (right air) | −10.40 | −561.41 |
| same curve built with standard air (console only, not in the video) | −9.32 | −503.4 |

At sea level the bullet reaches Mach ~1.55 at 1500 yd, below the measured span (Mach ≥ 1.78 in Socorro air), so
the last few hundred yards use the curve's extension — it still matches the sheet's own BC column to 0.4 in.
