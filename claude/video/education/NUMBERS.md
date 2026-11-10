# Numbers quoted in SCRIPTS.md

All from BallisticCalculator 1.1.13: `numbers/` (`dotnet run -c Release`) and `datagen/` (the animation data).
Load for the whole series: **175 gr Lapua Scenar-L .308** (the first series' bullet), 790 m/s (2600 fps),
diameter 0.308 in, length 1.394 in, sight 50 mm, 100 m zero, standard air (ρ 1.220 kg/m³, sound 340 m/s).

## Episode 1 — drag (datagen, level flight)

| What | Value |
|---|---|
| Drag deceleration at the muzzle | 472 m/s² = **48 g**; losing **47 m/s per 0.1 s** |
| Drag at 1600 m | 22 m/s² = **2.3 g** |
| Speed 1600 m | 260 m/s — **3× slower**, drag **21× smaller** (v² ≈ 9×, Cd ≈ 2.3×) |
| Mach 1 | at **930 m**, 1.8 s |
| Formula vs engine | 0.05 % supersonic; ≤ 4 % subsonic = gravity along the falling path |

Same bullet, three drag models, zero 100 m (drop below the line of sight):

| | G1 0.505 | G7 0.253 | Lapua radar `.drg` |
|---|---|---|---|
| 300 m | 47.8 cm | 47.9 cm | 48.1 cm — all within **3 mm** |
| 600 m | 3.22 m | 3.23 m | 3.27 m |
| 1000 m | 13.33 m | 13.62 m | 13.90 m — G1 **57 cm** high, G7 **28 cm** high |
| 1200 m | 22.88 m | 23.85 m | 24.25 m |

## Episode 2 — the point mass (G7 0.253)

| What | Value |
|---|---|
| At 1000 m | drop 13.62 m, time 2.0 s, 322 m/s |
| The demo step (scenes 4–5), at the muzzle | Mach 790/340 = **2.3**; drag 472 m/s² (**48 g**), gravity 9.81 m/s² (**1 g**) |
| One 0.1 s step from the muzzle | drag −47.2 m/s (**−47**), gravity +0.98 m/s down (**≈ 1**); 790 → **743 m/s** |
| Euler, ½ m internal step (cap 1 m, ~2000 steps) | **2 cm** off at 1000 m (vs RK2 at 1 cm) |
| Euler, 5 cm internal step (cap 0.1 m) | **2 mm** off |
| The app's default: RK2, ½ m internal step (cap 1 m) | < 0.005 mm off (prints 0.000 cm) — "less than a tenth of a millimetre" |
| datagen's own Euler, the scene-7 picture (level barrel, its own reference) | 0.1 s steps **2.8 m** off, 0.05 s 1.3 m, 0.02 s 52 cm, 0.005 s 12 cm |
| Episode 3 · Cross wind 4.5 m/s (≈10 mph) from the right, no twist | **3.35 m** left at 1000 m, 65 cm at 500 m |
| Fall from a level barrel (no sight height, no zero), 1000 m | with air **14.90 m** in 2.008 s (½gt² would be **19.76 m** — drag's vertical part brakes the fall); no air 1000/790 = **1.266 s**, ½gt² = **7.86 m** |
| Episode 3 · Wind, the air-relative velocity at the muzzle | tilted atan(4.5/790) = **0.33°** (5.7 mrad) |
| Episode 3 · Side drag at the muzzle | 472 × 4.5/790 = 2.69 m/s² = **0.27 g** (hand-computed from the drag above) |
| Episode 3 · Lag rule check | balloon 4.5 × 2.007 = **9.0 m**; drift ≈ W·(t − x/V₀) = 4.5 × (2.007 − 1.266) = 4.5 × 0.741 = **3.33 m** vs the engine's 3.35 m |

Scene 7 quotes the Euler error rows (and the RK2 row) (reference: RK2 at a 1 cm cap; re-run 2026-11-08, errors −1.95 cm
and −0.19 cm, drop 13.62 m) and the real step "ten to fifty centimetres" (the internal step is capped at 1 m
and halved from the row step: 11–50 cm in practice). At the muzzle, 790 m/s: 11 cm = 0.14 ms, 50 cm = 0.63 ms,
so "less than one thousandth of a second" holds at the muzzle only (50 cm at 322 m/s is 1.6 ms).
The exaggerated big-step picture comes from datagen's own Euler, not from the library, whose step is quantized
in decades.

## Episode 4 — what a point cannot do (G7 0.253, twist 1:10 in right)

| What | 500 m | 1000 m |
|---|---|---|
| Spin drift (no wind) | 4.7 cm right | **31 cm right** |
| Aerodynamic jump (4.5 m/s from the right, with twist) | 5.6 cm up | **11 cm up** (the wind itself: 3.35 m left) |
| Coriolis horizontal, 45° N, any bearing | 2.0 cm right | **10 cm right** |
| Eötvös, 45° N, shooting east / west | 2.2 cm up / 2.1 cm down | **12 cm up / 12 cm down**; north: 0 |

| What | Value |
|---|---|
| Spin rate at the muzzle | 790 m/s ÷ 0.254 m = **3 100 turns per second** |
| Effective gravity, 45° N, east / west (Eötvös factor) | **0.992 / 1.008** (−0.83 % / +0.83 %) |
| Sg (Miller, standard air) | **≈ 1.7** (1.708 by the formula: t = 32.5 cal, L = 4.53 cal; > 1.4 = well stabilised) |
| Spin drift by the bare formula at 2.0 s | 12.9 in = 32.8 cm; the engine's **31 cm** is net of the windage zero at 100 m (≈ 0.2 cm × 10) — the voice quotes the engine |
| Aerodynamic jump by the formula | 0.385 MOA = 11.2 cm at 1000 m, 5.6 cm at 500 m — matches the engine |
| Coriolis by the formula | 7.292e-5 × sin 45° × 1000 × 2.007 = 10.3 cm |
| The ground's own velocity to the east at 45° | Ω · R_earth · cos 45° = **329 m/s** ("more than three hundred") |
