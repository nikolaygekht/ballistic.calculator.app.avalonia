# VUDU-SR5 — EOTech Vudu SR-5 (FFP, milliradian)

A speed ring over a milliradian **christmas tree** — twelve rows of holdover with a dot grid for wind.
**First focal plane.**

Geometrically identical to [LE-5](VUDU-LE5.md); the only difference is the centre, a **dot** here and a
**cross-hair** there.

## The pattern

- **Speed ring** — 2.55 mrad OD, **0.2 mrad** thick, illuminated red.
- **Centre dot** — **0.25 mrad**.
- **Thin horizontal line**, **0.11 mrad**, from **±1.5 mrad** out to the post shoulder — it does not touch the
  ring.
- **Horizontal hashes** from **2 mrad outward** in 0.5 mrad steps to ±6, **hanging below the line** rather than
  crossing it: **0.5 mrad** on whole milliradians, **0.39 mrad** on halves. Numerals **2, 3, 4, 5, 6** sit
  above the line. **Nothing is graduated inside the ring.**
- **Heavy posts** from **9 mrad** to **11.5 mrad**, tapering at **60°** on the real etching.
- **Vertical spine** — a thin line running **only from 2 mrad to 12 mrad**. It does not reach the ring, so the
  first dot row at 1 mrad stands clear of any spine. Hashes cross it every 0.5 mrad, same two lengths.
- **The tree** — dots on a **1 mrad grid**, widening with depth, numerals **2, 4, 6, 8, 10, 12** at both ends
  of the numbered rows.

## The tree's envelope

Dots run out to **±(⌊d/2⌋ + 1) mrad** at a depth of *d* milliradians, and each row's numeral sits one step
beyond the outermost dot. Checked against EOTech's catalogue rendering row by row: ±1 at 1 mrad, ±2 at 2,
±3 at 4, ±4 at 6, ±5 at 8, ±6 at 10.

An earlier cut of this file used ⌊d/2⌋ and was **one column short on every row** — worth recording, because
the error is invisible unless you count dots against the catalogue image.

## The marks

Twelve `<bdc>` anchors, one per whole milliradian from 1 to 12. Geometric — the marks are milliradians, so
the application supplies the ranges from whatever load is loaded.

## Which numbers came from where

`Vudu_Reticle_Manual_SR4-5-LE_RevA.pdf` p8, cross-checked against the reticle-index image. Printed: the
**0.5 mrad** long hash, the **0.11 mrad line width**, the 1 mrad dot grid, the 9 and 11.5 mrad extents, the
0.37 mrad numeral height and the 60° post taper.

The **0.39 mrad short hash** is not printed on this page; it is [SR-4's](VUDU-SR4.md) printed value, borrowed
because the two come from the same drawing template. The sheet's `0.11` is the **line width**, not a hash
length — misreading it as a hash was what initially left this reticle with no horizontal line at all.

**Not reproduced:** the post taper — the posts are drawn as plain heavy lines, as they are on the Huron and
Specter files.

## Calibration

| | |
|---|---|
| Cartridge / load | **not published** |
| Zero | not published |
| Focal plane | first (FFP) |
| Library entry | — |
