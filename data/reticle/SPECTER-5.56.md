# SPECTER-5.56 — Elcan Specter 1-4×, 5.56

An eight-rung 5.56 ladder in the style of the Specter's BDC. Field of view 140 × 190 MOA.

## The pattern

Heavy duplex posts in from the field stop, a fine centre cross whose intersection is the 100 m zero,
then the ladder in two forms:

- **300–600 m** — crossbars on the fine spine, tapering inward so the count reads under stress.
- **700–1000 m** — aiming circles with flanking bars, each shrinking with range.

Etched numerals 3, 5, 6, 7, 8, 9, 10 are hundreds of metres.

## Calibration

| | |
|---|---|
| Projectile | M855, 62 gr |
| Ballistic coefficient | 0.151 G7 |
| Muzzle velocity | 2800 ft/s (a 16 in barrel) |
| Twist | 1:7 RH |
| Sight height | 2.9 in |
| Atmosphere | ICAO sea level |
| Zero | 100 m |
| Library entry | `.223/XM855 Ammo (16in)` — exactly this load, 2800 ft/s for the 16 in barrel |

## The marks

| Drop below zero | Range | Drawn as |
|---|---|---|
| 4.9 MOA | 300 m | crossbar, 7.0 MOA wide |
| 8.9 MOA | 400 m | crossbar, 5.0 MOA wide |
| 14.4 MOA | 500 m | crossbar, 4.0 MOA wide |
| 21.2 MOA | 600 m | crossbar, 2.0 MOA wide |
| 30.0 MOA | 700 m | circle, 3.5 MOA radius |
| 41.0 MOA | 800 m | circle, 3.0 MOA radius |
| 53.3 MOA | 900 m | circle, 2.8 MOA radius |
| 68.5 MOA | 1000 m | circle, 2.3 MOA radius |

Every `<bdc>` anchor sits exactly on the element it labels, and the etched numerals sit a uniform
**0.6 MOA** below their mark — the same convention as the 7.62 file, so the two read alike.

## In the app

Load `.223/XM855 Ammo (16in)`, sight height 2.9 in, 100 m zero — the entry already carries the
2800 ft/s this ladder was computed for.
Left at the library's 3050 ft/s (the 20 in barrel) the same marks label *longer* ranges — correct
arithmetic for a flatter load, not an error.
