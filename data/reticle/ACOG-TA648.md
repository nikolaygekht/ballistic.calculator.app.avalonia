# ACOG-TA648 — Trijicon TA648, .50 BMG

A nine-rung .50 BMG ladder in the style of the TA648. Field of view 28 × 42 mrad, zero 14 mrad from
the left edge and 6 mrad from the top — most of the glass is below the crosshair, because most of
this reticle is ladder.

## The pattern

Nine marks, **400…2000 m in 200 m steps**. On the real glass the etched numerals are hundreds of
metres (4, 6, 8, … 20).

Each rung sits slightly **right of the spine** — 0.02 mrad at 400 m growing to 0.72 mrad at 2000 m.
That is the load's own spin drift (1:15 RH), carried into the mark position, so placing the target on
a rung holds off for drift as well as drop.

## Calibration

| | |
|---|---|
| Projectile | M2 AP, 709 gr |
| Muzzle velocity | 2810 ft/s |
| Twist | 1:15 RH (the M2 barrel) |
| Sight height | 3.5 in — the TA648 over an M2 Browning's bore |
| Atmosphere | ICAO sea level |
| Zero | 300 m |
| Library entry | `50BMG M2` — 710 gr at 2830 ft/s against the ladder's 709 at 2810 |

The sight height is the **one input the file's comment never recorded**. 3.5 in is the mount geometry —
the TA648 sits high over the M2's receiver — and it is what to enter; it is not a value read back out
of the original calibration.

## The marks

| Drop below zero | Windage | Range |
|---|---|---|
| 0.819 mrad | 0.019 mrad R | 400 m |
| 2.743 mrad | 0.062 mrad R | 600 m |
| 5.069 mrad | 0.113 mrad R | 800 m |
| 7.880 mrad | 0.176 mrad R | 1000 m |
| 11.296 mrad | 0.252 mrad R | 1200 m |
| 15.457 mrad | 0.344 mrad R | 1400 m |
| 20.491 mrad | 0.454 mrad R | 1600 m |
| 26.471 mrad | 0.579 mrad R | 1800 m |
| 33.414 mrad | 0.717 mrad R | 2000 m |

## In the app

Load `50BMG M2`, set a **300 m** zero — not 100 m, this is the one calibrated file here that is not
zeroed at 100 — and the *Far BDC* labels land on 400…2000.
