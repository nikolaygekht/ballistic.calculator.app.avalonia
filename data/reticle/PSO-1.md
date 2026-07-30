# PSO-1 — SVD, and the one reticle here that needs the turret dialled

Drawn in **Soviet thousandths**, which is what the scope is graduated in: 1 ths = 1/6000 of a circle =
1.0472 mrad = 3.6 MOA. One windage mark and one drum click are both 0.5 ths. (A 5 cm/100 m click would
be 0.4775 ths, 4.5 % under half a thousandth — the real drum is the coarser one.)

Field of view 79 × 79 ths. The circle is the **drawn field boundary, not an etched element** — it is
sized to enclose the etching, while the real 6° field is 50 ths in radius. Layout follows the
canonical PSO-1 drawing.

## Why the three chevrons are not ordinary hold-overs

The **main chevron** is the aiming mark for whatever range the elevation drum is dialled to
(100…1000 m). The three chevrons below it mean something only with the **drum set to 10** — dialled
for 1000 m. With that on the turret:

- the **main chevron** is the 1000 m hold;
- the three below are **1100, 1200 and 1300 m**.

**Dialling to 1000 m is 30 vertical clicks** — 30 × 0.5 ths = 15 ths up from the 100 m setting.

## Calibration

| | |
|---|---|
| Projectile | 57-N-323S LPS ball, 9.6 g |
| Ballistic coefficient | 0.400 G1 |
| Muzzle velocity | 830 m/s |
| Twist | 1:320 mm RH |
| Sight height | 65 mm |
| Atmosphere | ICAO sea level |
| Reference | hold(*R*) minus hold(1000 m) — **not** drop from a 100 m zero |
| Library entry | `7.62x54/57N323S` — the LPS ball the chevrons were computed from |

## The marks

| Below the main chevron | Range (drum at 10) |
|---|---|
| 3.162 ths | 1100 m |
| 6.714 ths | 1200 m |
| 10.647 ths | 1300 m |

Recomputing the load puts those three holds at **3.166 / 6.719 / 10.654 ths** against the etched
**3.162 / 6.714 / 10.647** — agreement to five thousandths of a thousandth, which is what identifies
the load and the drum setting together. Read at any other drum setting the spacing does not fit.

## In the app

The *Far BDC* overlay labels marks from your *current* solution, so there are two ways to see this:

1. **Set the zero to 1000 m** — simplest, and the labels land on 1100 / 1200 / 1300.
2. **Keep the 100 m zero and dial it** — put `0.5 ths` in the sight's vertical click on the Rifle tab,
   then enter **30** V-Clicks on the Parameters tab.

With an ordinary 100 m zero and *nothing* dialled, the overlay labels these three marks with whatever
ranges they happen to correspond to from the crosshair — correct arithmetic, but not what the etching
means.

## One honest discrepancy

The real drum was cut to the issue firing tables, and this model puts the 1000 m come-up at **14.0 ths
(28 clicks)** rather than 15. It moves where the main chevron lands by about 1 ths; it does not touch
the spacing of the three chevrons below it, which is what they are read by.

## The rangefinder

At lower left, and nothing to do with drop: put the target's feet on the horizontal base line and its
head on the curve, then read the range off the numbers — hundreds of metres, 2 nearest the centre out
to 10 at the far left. It assumes the true **1.7 m** target height.
