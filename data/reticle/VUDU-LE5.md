# VUDU-LE5 — EOTech Vudu LE-5 (FFP, milliradian)

The law-enforcement variant of [SR-5](VUDU-SR5.md): **the same reticle in every dimension**, with a
**cross-hair inside the speed ring instead of a dot**. **First focal plane.**

## The pattern

- **Speed ring** — 2.55 mrad OD, **0.2 mrad** thick, illuminated red.
- **Centre cross-hair** inside the ring — the one feature that distinguishes this from SR-5, and the reason
  the file exists separately.
- **Centre cross-hair** confined **inside the ring** — arms reach the ring's inner edge (±1.075 mrad) and stop.
- **Thin horizontal line**, **0.11 mrad**, from **±1.5 mrad** out to the post shoulder.
- **Horizontal hashes** from **2 mrad outward** in 0.5 mrad steps to ±6, **hanging below the line**:
  **0.5 mrad** on whole milliradians, **0.39 mrad** on halves. Numerals **2, 3, 4, 5, 6** above the line.
  **Nothing is graduated inside the ring** — the innermost hash is at 2 mrad.
- **Heavy posts** from **9 mrad** to **11.5 mrad**.
- **Vertical spine** — thin, running **only from 2 mrad to 12 mrad**, hashes crossing it every 0.5 mrad.
- **The tree** — dots on a **1 mrad grid** widening half a milliradian per milliradian of depth, numerals
  **2, 4, 6, 8, 10, 12** at both ends.

## Identical to SR-5 by design

Every subtension on this reticle matches SR-5's. That was confirmed twice: EOTech's manual draws the two on
facing pages (p8 and p9) with the same callouts, and the reticle-index images show the same thing. The pair
is built from one generator with the centre style as its only parameter.

If you own an SR-5, loading this file changes nothing but the aiming point's shape. Both are shipped because
both are real products with real part numbers — the same reasoning as the two low-power Huron files.

## The marks

Twelve `<bdc>` anchors, one per whole milliradian from 1 to 12. Geometric.

## Which numbers came from where

`Vudu_Reticle_Manual_SR4-5-LE_RevA.pdf` p9, cross-checked against the reticle-index image supplied by
Nikolay. The post taper (60° on the etching) is not reproduced.

## Calibration

| | |
|---|---|
| Cartridge / load | **not published** |
| Zero | not published |
| Focal plane | first (FFP) |
| Library entry | — |
