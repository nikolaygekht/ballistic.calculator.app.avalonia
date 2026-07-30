# VCOG16-300BLK — Trijicon V-COG 1-6×24, 300 BLK

The busiest reticle in this folder, because 300 BLK is two cartridges in one chamber: a supersonic
ladder and a separate set of subsonic holds. Framed the way Trijicon's own reticle sheet draws it —
the **6× field of view, 25 mils (84.5 MOA) in radius**, so 172 × 172 MOA overall.

## The pattern

- **Centre crosshair** — 19 in at 100 m, and the 100 m supersonic zero. Its **bottom end is the
  200 m** supersonic hold.
- **Stadia below** — 300, 400, 500 and 600 m, each 19 in wide at its own range.
- **Three diamonds** — the *subsonic* holds: 25/50 m, 100 m and 150 m, centred on the hold. The
  25/50 m diamond sits on the **50 m** hold; 25 m is 0.54 MOA lower.
- **Ring** — 25 MOA gaps on both axes; 66.4 MOA across the outer corners, which is 19 in at 25 m.
- **Horizontal stadia** — marked every 5 mils (16.9 MOA). The sheet's "16.9 MOA × 12" says the etched
  pattern carries twelve of them, one mark further out than this field of view shows.

## Calibration

Two loads, zeroed with the supersonic one.

| | Supersonic | Subsonic |
|---|---|---|
| Projectile | 115 gr | 208 gr A-MAX |
| Ballistic coefficient | 0.290 G1 | 0.648 G1 |
| Muzzle velocity | 2330 ft/s | 1010 ft/s |
| Library entry | `.300 AAC/.300 AAC 115gr` (2295 ft/s) | `.300 AAC 208gr Hornady` (1020 ft/s) |

| | |
|---|---|
| Twist | 1:8 RH |
| Sight height | 2.9 in (the V-COG's tall integral mount) |
| Atmosphere | ICAO sea level |
| Zero | 100 m, with the **supersonic** load |

The library velocities are 2295 and 1020 ft/s against the ladder's 2330 and 1010 — close enough that
the marks label where they should.

## The marks

The `<bdc>` anchors are the supersonic ladder only:

| Drop below zero | Range | Drawn as |
|---|---|---|
| 3.201 MOA | 200 m | bottom end of the centre crosshair |
| 8.289 MOA | 300 m | stadia bar |
| 14.939 MOA | 400 m | stadia bar |
| 23.362 MOA | 500 m | stadia bar |
| 33.759 MOA | 600 m | stadia bar |

The subsonic diamonds are drawn but carry no anchors — they belong to a different load, and labelling
them from the supersonic solution would be nonsense.

## How the loads were identified

Neither load is printed on the sheet; both were fitted to the sheet's own geometry, to **0.17 and
0.18 MOA RMS** across every mark it draws. So this ladder matches the published pattern, not merely a
plausible 300 BLK.
