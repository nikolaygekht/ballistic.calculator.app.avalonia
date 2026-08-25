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

## 1. Episode 1 — metric baseline

`11.3 g · 0.253 G7 · 790 m/s · 50 mm sight · 0.1 mrad clicks · 300 m zero · ICAO standard air · no wind`

Zero elevation **2.935 mrad** (10.09 MOA).

| Range | Velocity | Mach | Drop | Hold | Energy | Time |
|---|---|---|---|---|---|---|
| 0 m | 790 m/s | 2.32 | −5.0 cm | — | 3526 J | 0.00 s |
| 100 m | 732 m/s | 2.15 | **+16.1 cm** | 1.61 | 3025 J | 0.13 s |
| 200 m | 676 m/s | 1.99 | +18.8 cm | 0.94 | 2582 J | 0.27 s |
| 300 m | 623 m/s | 1.83 | **0.0 cm** | 0.00 | 2192 J | 0.43 s |
| 400 m | 572 m/s | 1.68 | −44.1 cm | −1.10 | 1849 J | 0.60 s |
| 500 m | 523 m/s | 1.54 | −118.3 cm | −2.37 | 1548 J | 0.78 s |
| 600 m | 477 m/s | 1.40 | −228.5 cm | −3.81 | 1283 J | 0.98 s |
| 700 m | 432 m/s | 1.27 | −382.0 cm | −5.46 | 1052 J | 1.20 s |
| 800 m | 389 m/s | 1.14 | −588.4 cm | −7.35 | 854 J | 1.44 s |
| 900 m | 349 m/s | 1.03 | −859.9 cm | −9.55 | 688 J | 1.71 s |
| 1000 m | 322 m/s | 0.95 | **−1212 cm** | −12.12 | 585 J | 2.02 s |

Spoken: *"twelve metres below the line of sight"* — −12.12 m.

---

## 2. Episode 2 — THE BASELINE

`175 gr · 0.253 G7 · 2600 ft/s · 2 in sight · 0.1 mrad clicks · 300 yd zero · ICAO standard air · no wind`

**This is the file saved as `bc2-demo.trajectory` and reopened at the start of episodes 3, 4, 5 and 6.**
Every delta in §3 to §6 is measured against this table.

Zero elevation **2.664 mrad** (9.16 MOA).

| Range | Velocity | Mach | Drop | Hold | Energy | Time |
|---|---|---|---|---|---|---|
| 0 yd | 2600 fps | 2.33 | −2.0 in | — | 2626 ftlb | 0.00 s |
| 100 yd | 2425 fps | 2.17 | **+4.9 in** | 1.36 | 2284 ftlb | 0.12 s |
| 200 yd | 2256 fps | 2.02 | +5.9 in | 0.82 | 1978 ftlb | 0.25 s |
| 300 yd | 2095 fps | 1.88 | **0.0 in** | 0.00 | 1705 ftlb | 0.39 s |
| 400 yd | 1940 fps | 1.74 | −13.8 in | −0.96 | 1463 ftlb | 0.53 s |
| 500 yd | 1791 fps | 1.60 | −36.9 in | −2.05 | 1247 ftlb | 0.70 s |
| 600 yd | 1648 fps | 1.48 | −70.8 in | −3.28 | 1055 ftlb | 0.87 s |
| 700 yd | 1510 fps | 1.35 | −117.5 in | −4.66 | 886 ftlb | 1.06 s |
| 800 yd | 1377 fps | 1.23 | −179.6 in | **−6.24** | 736 ftlb | 1.27 s |
| 900 yd | 1250 fps | 1.12 | −260.1 in | −8.03 | 607 ftlb | 1.50 s |
| 1000 yd | 1132 fps | **1.01** | **−362.8 in** | −10.08 | 498 ftlb | 1.75 s |

**Summary view:** goes subsonic at **1017 yd** (930 m).

**Mil-Dot reticle, Far BDC** — where the whole-milliradian dots fall for this load:

| Dot | Range |
|---|---|
| 1 mrad | 405 yd |
| 2 mrad | 498 yd |
| 3 mrad | 580 yd |
| 4 mrad | 656 yd |
| 5 mrad | 725 yd |
| edge of the reticle (6 mrad) | **787 yd** |

> **Known 1–2 yd disagreement.** A standalone harness reproducing the app's BDC interpolation reads
> 404 / 496 / 578 / 654 / 722 / 786 for this row and 1014 yd for the subsonic crossing. The values above
> came from the application itself and are the ones to trust; the narration says "about four hundred"
> where the difference could show. Same caveat applies to §7's mark table, which is harness-only.

---

## 3. Episode 3 — weather, then wind

### 3a. Baseline + February weather

Added: `1000 ft · 28.9 inHg station · 40 °F · 60 % RH`. Zero elevation **2.662 mrad**.

| Range | Velocity | Mach | Drop | Hold | vs baseline |
|---|---|---|---|---|---|
| 300 yd | 2096 fps | 1.91 | 0.0 in | 0.00 | — |
| 600 yd | 1652 fps | 1.51 | −70.7 in | −3.27 | +0.1 in |
| 800 yd | 1382 fps | 1.26 | −179.1 in | −6.22 | +0.5 in |
| 1000 yd | 1136 fps | 1.04 | **−361.5 in** | −10.04 | **+1.3 in** |

Only **1.3 in** at 1000 yd — cold air and altitude nearly cancel. The narration says so rather than
overselling it. (This is *not* the interesting weather number; §5b is.)

### 3b. + 10 mph wind at 90° (from the right), no distance limit

Drop is unchanged from 3a. Windage:

| Range | Windage | Wind hold |
|---|---|---|
| 300 yd | 6.9 in | 0.64 |
| 600 yd | **31.1 in** | 1.44 |
| 800 yd | 60.4 in | 2.10 |
| 1000 yd | **104.2 in** | 2.89 |

---

## 4. Episode 4 — spin drift, aerodynamic jump, Coriolis

### 4a. Baseline + wind only — the prerequisite

Added: `10 mph · 90°`. **No weather** (this episode starts from the baseline, not from episode 3).

| Range | Drop | Windage |
|---|---|---|
| 600 yd | −70.8 in | 31.3 in |
| 1000 yd | **−362.8 in** | **104.8 in** |

Drop is bit-for-bit the baseline: with no bullet shape and no twist entered, a crosswind moves the
bullet sideways only.

### 4b. + bullet shape and rifling

Added: `diameter 0.308 in · length 1.394 in · rifling 1:10 in, right`

| Range | Drop | vs 4a | Windage | vs 4a |
|---|---|---|---|---|
| 300 yd | +1.2 in | +1.2 | 7.0 in | 0.0 |
| 600 yd | −68.4 in | +2.4 | 29.7 in | −1.6 |
| 800 yd | −176.4 in | +3.2 | 56.9 in | −3.9 |
| 1000 yd | **−358.8 in** | **+4.0** | **96.8 in** | **−8.0** |

The drop change is **aerodynamic jump**; the windage change is **spin drift** (right twist drifts right)
partly cancelling a left-pushing wind.

### 4c. Bullet shape and rifling, with the wind REMOVED — the honest control

| Range | Drop | vs baseline | Windage |
|---|---|---|---|
| 600 yd | −70.8 in | **0.0** | −1.5 in |
| 1000 yd | **−362.8 in** | **0.0** | **−8.0 in** |

**Aerodynamic jump is exactly zero without wind** — the drop column is identical to the baseline to the
last printed digit, while spin drift alone carries the bullet **8.0 in right** at 1000 yd. This is why
episode 4 must put the wind back on before it can show jump at all, and the script says so out loud.

### 4d. Baseline + Coriolis only

Added: `barrel azimuth 90° (east) · latitude 45° N`. No wind, no shape, no weather.

| Range | Drop | vs baseline | Windage |
|---|---|---|---|
| 600 yd | −69.7 in | +1.1 in | −1.0 in |
| 1000 yd | **−359.0 in** | **+3.8 in** | **−3.2 in** |

Azimuth dependence at 1000 yd (the horizontal term does not depend on azimuth):

| Azimuth | Drop |
|---|---|
| 0° north | −362.8 in |
| 90° east | −359.0 in (up) |
| 180° south | −362.8 in |
| 270° west | −366.7 in (down) |

---

## 5. Episode 5 — dialled clicks, then the zero's own weather

### 5a. Baseline + 62 clicks dialled up

The baseline hold at 800 yd is **−6.24 mrad** → **62 clicks** of 0.1 mrad. Enter `6.2 mrad` as the shot
drop adjustment. (The old cumulative script said 60 clicks; against the naked baseline it is 62.)

| Range | Drop before | Drop after | Hold after |
|---|---|---|---|
| 100 yd | +4.9 in | +27.2 in | 7.56 |
| 300 yd | 0.0 in | +67.0 in | 6.20 |
| 400 yd | −13.8 in | **+75.5 in** | 5.24 |
| 800 yd | −179.6 in | **−1.0 in** | −0.04 |
| 1000 yd | −362.8 in | −139.6 in | −3.88 |

Sign check: a **positive** shot drop adjustment means clicks dialled **up**. Negative doubles the drop
instead — if the 800 yd row does not go to zero, the sign is wrong.

### 5b. Baseline + July air, the zero inheriting it

Shot air `1000 ft · 28.9 inHg · 100 °F · 30 % RH`, Zero tab left empty. Zero elevation **2.628 mrad**.

| Range | Drop | Hold |
|---|---|---|
| 300 yd | 0.0 in | 0.00 |
| 600 yd | −67.5 in | −3.12 |
| 800 yd | −168.8 in | −5.86 |
| 1000 yd | **−334.8 in** | −9.30 |

### 5c. + the Zero tab keeping the February air

Zero atmosphere `1000 ft · 28.9 inHg · 40 °F · 60 % RH`. Zero elevation back to **2.662 mrad**.

| Range | Drop | vs 5b | Hold |
|---|---|---|---|
| 300 yd | **+0.4 in** | +0.4 | 0.03 |
| 600 yd | −66.8 in | +0.7 | −3.09 |
| 800 yd | −167.8 in | +1.0 | −5.83 |
| 1000 yd | **−333.6 in** | **+1.2 in** | −9.27 |

**The two figures the narration quotes, and which is which:**

- **27.9 in** — February air (§3a, −361.5) versus July air (5c, −333.6) at 1000 yd. Almost all of it is
  *today's* air, which is episode 3's material.
- **1.2 in** — what the *Zero tab's own atmosphere* is worth by itself (5b → 5c). Small at a 300 yd
  zero, and the script states that honestly instead of claiming the 27.9.

The zero-air share grows with the zero distance, because the sight angle is set over that flight:

| Zero distance | Zero-air share at 1000 yd |
|---|---|
| 100 yd | 0.1 in |
| 300 yd | 1.2 in |
| 500 yd | 4.1 in |
| 600 yd | 6.5 in |

Note the 300 yd row in 5c: **+0.4 in**, not zero. The rifle is no longer exactly zeroed at its zero
distance, because the zero was taken in different air. That is a real, visible consequence.

Air densities: 40 °F → 1.2695 kg/m³ (density altitude −1227 ft); 100 °F → 1.1214 kg/m³ (2988 ft).

---

## 6. Episode 6 — the measured drag curve

`data/drg/Lapua/30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg`, table `GC`, form factor **1.0**.
Header confirms the same bullet: 11.30 g, 7.83 mm, 35.4 mm. Standard air, no wind — a pure
drag-model comparison against §2. Zero elevation **2.670 mrad**.

| Range | Baseline G7 0.253 | Radar .drg | Change | Box G1 0.505 |
|---|---|---|---|---|
| 300 yd | 0.0 in | 0.0 in | — | 0.0 in |
| 600 yd | −70.8 in | −71.9 in | −1.1 in | −70.6 in |
| 800 yd | −179.6 in | −183.5 in | −3.9 in | −178.6 in |
| 1000 yd | **−362.8 in** | **−371.4 in** | **−8.6 in** | **−357.4 in** |

Both approximations are **optimistic** against the bullet's own measured curve:

| Against the radar curve at 1000 yd | Error |
|---|---|
| G7 0.253 (the good approximation) | 8.6 in high |
| G1 0.505 (the number on the box) | **14.0 in high** |

**Velocity.** At 800 yd the radar curve arrives **20 fps slower** (1357 vs 1377). By 1000 yd both read
1132 fps — the curves converge near Mach 1, so quote the 800 yd figure, not the 1000 yd one.

Saved as **`bc2-demo-radar.trajectory`**, a second file — the baseline is left untouched, and episode 7
opens the radar one.

---

## 7. Episode 7 — everything at once

Opens `bc2-demo-radar.trajectory` and adds every feature from episodes 3 to 6 except the dialled clicks:
`July air 100 °F/30 % at 1000 ft/28.9 inHg` · `February zero air 40 °F/60 %` · `10 mph at 90°` ·
`0.308 in / 1.394 in / 1:10 right` · `azimuth 90° / latitude 45°` · radar `.drg`.

Zero elevation **2.667 mrad**.

| Range | Velocity | Mach | Drop | Hold | Windage | Wind hold | Energy | Time |
|---|---|---|---|---|---|---|---|---|
| 0 yd | 2600 fps | 2.24 | −2.0 in | — | 0.0 in | — | 2626 ftlb | 0.00 s |
| 100 yd | 2438 fps | 2.10 | +5.4 in | 1.49 | 0.8 in | 0.21 | 2309 ftlb | 0.12 s |
| 200 yd | 2279 fps | 1.96 | +6.9 in | 0.96 | 2.8 in | 0.39 | 2017 ftlb | 0.25 s |
| 300 yd | 2124 fps | 1.83 | **+1.8 in** | 0.17 | 6.2 in | 0.57 | 1752 ftlb | 0.38 s |
| 400 yd | 1975 fps | 1.70 | −10.9 in | −0.76 | 11.2 in | 0.78 | 1516 ftlb | 0.53 s |
| 500 yd | 1834 fps | 1.58 | −32.5 in | −1.81 | 17.8 in | 0.99 | 1307 ftlb | 0.69 s |
| 600 yd | 1699 fps | 1.47 | −64.4 in | −2.98 | 26.4 in | 1.22 | 1122 ftlb | 0.86 s |
| 700 yd | 1571 fps | 1.35 | −108.2 in | −4.30 | 36.9 in | 1.47 | 959 ftlb | 1.04 s |
| 800 yd | 1451 fps | 1.25 | −166.1 in | −5.77 | 49.8 in | 1.73 | 818 ftlb | 1.24 s |
| 900 yd | 1339 fps | 1.15 | −240.3 in | −7.42 | 65.0 in | 2.01 | 696 ftlb | 1.45 s |
| 1000 yd | 1234 fps | 1.06 | **−333.9 in** | −9.27 | **82.9 in** | 2.30 | 592 ftlb | 1.69 s |

**The three things worth saying about this table.**

1. **Not zeroed at its zero distance any more.** 300 yd reads **+1.8 in**, because the zero was taken in
   February air and this shot is in July air, with jump and Coriolis on top.
2. **The effects do not pile up in one direction.** Drop at 1000 yd is −333.9 in; the July-air-alone
   figure (§5c) is −333.6 in. The radar curve's extra 8 in of drop is almost exactly cancelled by
   aerodynamic jump plus Coriolis. Nearly the same number, arrived at for completely different reasons —
   the script makes this the point rather than hiding it.
3. **The range card moved.** Subsonic now at **1079 yd** against the baseline's 1017 — 62 yd further,
   because the July air is thinner.

**Mil-Dot reticle, Far BDC — the same reticle, re-labelled.** Harness values; see the caveat in §2, so
the narration speaks these as "about":

| Dot | Baseline (§2) | Everything | Shift |
|---|---|---|---|
| 1 mrad | 405 yd | ~424 yd | +19 yd |
| 2 mrad | 498 yd | ~517 yd | +19 yd |
| 3 mrad | 580 yd | ~601 yd | +21 yd |
| 4 mrad | 656 yd | ~678 yd | +22 yd |
| 5 mrad | 725 yd | ~749 yd | +24 yd |
| 6 mrad (edge) | 787 yd | ~815 yd | +28 yd |

Every mark in the glass is now worth roughly twenty to thirty yards more than the naked baseline said.
That is the closing argument of the .308 half of the playlist.

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

This is the closing shot of the playlist. Set **Split at** to a small value (25 m) so the subsonic holds
are labelled as *Far* BDC rather than falling into the *Near* side of the 100 m zero.

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
