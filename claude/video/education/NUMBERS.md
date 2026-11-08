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
| Euler, ½ m internal step (cap 1 m, ~2000 steps) | **2 cm** off at 1000 m (vs RK2 at 1 cm) |
| Euler, 5 cm internal step (cap 0.1 m) | **2 mm** off |
| Cross wind 4.5 m/s (≈10 mph) from the right, no twist | **3.35 m** left at 1000 m, 65 cm at 500 m |

The two Euler error rows are background only; the script says just "less than a metre each, thousands of
steps per kilometre" (the internal step is capped at 1 m and halved from the row step: 11–50 cm in practice).
The exaggerated big-step picture comes from datagen's own Euler, not from the library, whose step is quantized
in decades.

## Episode 3 — what a point cannot do (G7 0.253, twist 1:10 in right)

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
| Sg (Miller, standard air) | **≈ 1.7** (hand-computed; > 1.4 = well stabilised) — confirm in datagen |
