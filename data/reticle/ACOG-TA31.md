# ACOG-TA31 — Trijicon TA31 (ACOG 4×32), 5.56 chevron

A chevron-and-ladder pattern in the style of the TA31's 5.56 BDC. Field of view 16 × 40 MOA,
zero 8 MOA from the left edge and 4 MOA from the top.

## The pattern

- **Chevron tip** is the 100 m zero.
- **Chevron base** is the 300 m hold, and is **19 in wide** — silhouette shoulders at 300 m, which is
  the ranging trick the whole pattern is designed around.
- Below it, six crossbars for **400…800 m**, each drawn **19 in wide at its own range**, so a target
  that fills a bar is at that bar's range.

## Calibration

| | |
|---|---|
| Projectile | M855, 62 gr |
| Ballistic coefficient | 0.151 G7 |
| Muzzle velocity | 3050 ft/s (a 20 in barrel) |
| Twist | 1:7 RH |
| Sight height | 2.4 in |
| Atmosphere | ICAO sea level |
| Zero | 100 m |
| Library entry | `.223/XM855 Ammo` — exactly this load |

## The marks

| Drop below zero | Range | Drawn as |
|---|---|---|
| 4.073 MOA | 300 m | chevron base |
| 7.496 MOA | 400 m | crossbar |
| 11.801 MOA | 500 m | crossbar |
| 17.236 MOA | 600 m | crossbar |
| 24.212 MOA | 700 m | crossbar |
| 33.188 MOA | 800 m | crossbar |

## In the app

Load `.223/XM855 Ammo` on the Ammunition tab, set the 100 m zero and 2.4 in sight height, and the
*Far BDC* overlay should label these six marks with the ranges above. Anything else behind the glass
and the labels move — correctly; see the README on how marks are labelled.
