# LEUP-CCH — Leupold FFP CCH (Mark 5HD)

The densest reticle in this folder: a milliradian grid with a **twenty-row christmas tree**, four chevrons
flanking the aiming point, and a graduated horizontal axis. **First focal plane**, so every subtension
holds at any magnification.

Leupold optimise it for night vision and thermal use and note the reticle "will measure correctly" across
the range; the sheet marks the field of view available at **18×, 25× and 35×**, with the note that
"reducing magnification will reveal more of the reticle". This file draws the whole etched pattern, so at
high magnification you see the middle of it.

## The pattern

- **Centre** — a **0.05 mil** dot at the aiming point. Nothing heavier: the grid is the reticle.
- **Horizontal axis** — graduated every **0.25 mil** out to ±4 mil, with three hash lengths: **0.50 mil**
  on whole milliradians, **0.40** on halves, **0.25** on quarters. Numerals 1–4 each side.
- **Vertical spine** — the same graduation upward to +3 mil (numerals 1–3), and running the length of the
  tree below.
- **Four chevrons** — **0.50 × 0.50 mil**, at **±1.60 and ±3.20 mil**, apex down. They sit **above the axis
  numerals**, not against the axis line, as the sheet draws them: the numeral row occupies 0.62–1.04 mrad
  and the chevrons 1.15–1.65. That matters because the ±3.20 chevron shares its x span with the "3"
  numeral, so at the same height the two collide. These are the only heavy elements in the reticle.
- **The tree** — twenty rows at exactly **1.00 mil** intervals, each a graduated bar plus dots outboard,
  with **half-mil dot rows** in between. All fine lines are **0.04 mil**.

## The tree, row by row

| Rows | Bar half-width | Dots outboard, every 0.5 mil | Numerals at |
|---|---|---|---|
| 1 | ±1.0 mil | to ±3.0 mil | ±3.3 mil |
| 2–4 | ±2.0 mil | to ±4.0 mil | ±4.35 mil |
| 5–20 | ±3.0 mil | to ±4.0 mil | ±4.35 mil |

Dots are **0.10 mil** on whole milliradians and **0.08 mil** on halves. Between each pair of numbered rows
sits a dot row at the half milliradian, carrying dots every 0.5 mil out to the width of the row below it.

Every row carries a `<bdc>` anchor, so the application labels all twenty with the range your trajectory
crosses them — which is the natural way to use a grid this fine.

## Geometric, not load-calibrated

The marks are places on a ruler: whole milliradians, 1 through 20. There is no load, no zero and no
cartridge behind them, and none is needed. That is why this reticle lives in the *Geometric* table of the
[README](README.md) rather than the load-calibrated one.

## Which numbers came from where

**Printed on the sheet:** the 0.04 mil fine lines, all three hash lengths (0.50 / 0.40 / 0.25), the
0.25 mil graduation, the 1.00 mil row pitch, the chevron size (0.50 × 0.50) and positions (±1.60, ±3.20),
and all three dot diameters (0.05 centre, 0.08 small, 0.10 large).

**Measured off the sheet:** the per-row bar half-widths and the outboard dot extents in the table above.
That is safe here because the sheet is drawn to scale, and provably: its horizontal 0.25 mil graduation
comes out at 59 px, giving 236 px per milliradian, against 239 px per milliradian measured down the twenty
rows of the tree — two independent axes agreeing to **1.3%**. The measured row drops landed on 1.00, 2.00,
… 20.00 mil, which is the second confirmation.

## The field is wider than the etching

The pattern itself only reaches ±5 mrad, but the file's field is **15 mrad** wide. Leupold puts row
numerals on **both** sides of the tree, out to ±4.35 mrad, so there is no free lane inside the etched
pattern for the application's distance labels — with a tighter field, 19 of the 20 collided with a numeral
or an outboard dot. The margin gives the labels a corridor at −7.2 mrad, clear of the numerals' left edge
by 1.29 mrad.

## Two things not reproduced

- **The axis and row bars are drawn as solid fine lines.** On the real etching they are broken into short
  dashes (the sheet dimensions a 0.20 mil dash and a 0.07 mil gap). The `.reticle` format has `line-style`
  but no control over dash length, so a solid line is closer than a wrong dash pattern.
- **The field-stop posts** visible in the sheet's full-reticle view — four heavy tapered wedges coming in
  from the edge — are outside this frame and omitted, in line with framing the file around the tree.

## Wind holds

The tree *is* the wind hold: every intersection is a hold, which is the point of a grid reticle. There are
no separate wind marks to reproduce, and nothing here needed a computed trajectory.
