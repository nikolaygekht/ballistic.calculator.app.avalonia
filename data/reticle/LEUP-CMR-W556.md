# LEUP-CMR-W556 — Leupold CMR-W 5.56 Illum. FFP

Leupold's 5.56 competition/patrol reticle: a red horseshoe over a milliradian drop ladder, with a wide
1 mil hash scale on the horizontal and a small mil ruler for range estimation. **First focal plane**, so
the subtensions hold at every magnification.

Framed **to the 20 mil mark** (42 × 22 mrad) — the second tall tick out on the medium arm. The etched
reticle carries those arms on to about ±39.7 mil, closed by 3.00 mil heavy field-stop blocks, but drawing
that far makes the drop ladder disproportionately small for no gain, so the arms simply run off-frame
here. Leupold's own sheet notes that its circle is "the available field of view at high magnification"
and that "reducing magnification will reveal more of the reticle", so no single frame is the whole truth
on a 1-6× anyway.

## The pattern

- **Red horseshoe** — 2.18 mil OD, 1.46 mil ID (a 0.36 mil ring), open at the bottom, with a **0.15 mil
  red dot** at the aiming point.
- **Post in the horseshoe gap** — its tip is the **300 m hold**, the same trick the V-COG uses with its
  centre crosshair.
- **Horizontal hash scale** — a hash every **1 mil** out to ±10, numbered every 2. Fine lines are
  **0.20 mil**.
- **Medium arms** — **1.00 mil** thick, from ±10.4 mil outward, carrying taller ticks at **15 and
  20 mil**. The 20 mil tick is where this file's frame ends.
- **Heavy field-stop blocks** — **3.00 mil**, at ±40 to ±43 mil on the real glass. Outside this frame,
  so not drawn.
- **Mil ruler** — a vertical scale above the left arm at x −10.1 mil, ticks on whole milliradians 1…5
  (Leupold's published 1.00 mil spacing), for measuring and range estimation.
- **Drop ladder** — a crossbar at each published range, tapering with distance, plus unnumbered
  **half-range ticks** between them.

## Calibration

| | |
|---|---|
| Cartridge | .223 / 5.56, 62 gr |
| Zero to use here | **100 m** — see below |
| Focal plane | first (FFP) |
| Sight height | not published by Leupold |
| Atmosphere | not published by Leupold |
| Library entry | `.223/XM855 Ammo`, unchanged at its own 3050 ft/s |
| Leupold's stated basis | a 62 gr round at **2970 ft/s** with a **50 m zero** |

### The zero: 50 m published, 100 m in practice

Leupold states this reticle is "based on a .223/5.56 62 gr round at 2970 FPS" with "a 50 meter zero".
Tested in this application against the shipped `.223/XM855 Ammo` entry, the labels land on the etched
ranges at a **100 m zero**, not a 50 m one — so 100 m is what to enter.

The **7.62 sibling behaves identically** — [LEUP-CMR-W762](LEUP-CMR-W762.md) also lines up at 100 m
rather than the 50 m Leupold publishes. Two different cartridges and two different ladders showing the
same offset makes this systematic rather than an artefact of one load, and it suggests Leupold's
"50 metre zero" describes how the reticle is meant to be set up on the rifle rather than the line its
drop table was computed from. Leupold publishes neither sight height nor atmosphere, so the sheet cannot
settle it. What is settled is which zero makes the labels land on the etched ranges.

## The marks

Positions are **Leupold's own published drop table**, not a trajectory computed here:

| Drop below zero | Range | Drawn as |
|---|---|---|
| 0.86 mil | 300 m | tip of the post in the horseshoe gap |
| 1.82 mil | 400 m | crossbar, 1.23 mil wide |
| 2.98 mil | 500 m | crossbar, 0.97 mil wide |
| 4.41 mil | 600 m | crossbar, 0.85 mil wide |
| 6.15 mil | 700 m | crossbar, 0.78 mil wide |
| 8.24 mil | 800 m | crossbar, 0.72 mil wide |
| 10.78 mil | 900 m | crossbar, 0.66 mil wide |

Half-range ticks (0.53 mil wide) sit at 1.29 / 2.39 / 3.64 / 5.22 / 7.15 / 9.41 mil — the 350, 450,
550, 650, 750 and 850 m holds.

## What is not reproduced

The etched reticle carries a **christmas tree of wind holds** — six rows (400…900 m) of 5 / 10 / 15 /
20 mph marks each side of the spine, dots at 0.15 mil and squares at 0.20 mil, with a bracket at the
left end of each row spanning **12 in at that row's range**. Only the mark *sizes* are published, never
their positions, so the tree is left out of this file rather than guessed at. The drop ladder above is
complete and is what the application labels.

## Which numbers came from where

Everything structural is published: the drop table, all three line weights, the horseshoe diameters,
the dot, and the 1.00 mil ruler spacing. The crossbar widths, the half-range tick positions and the
15/20 mil outer ticks are **measured** off Leupold's sheet — which is safe here because that sheet is
drawn to scale, and provably so: its horizontal hash axis gives 47.3 px/mil while its vertical ladder
gives 47.0 px/mil against the published 10.78 mil drop, two independent axes agreeing to 0.6 %. The
measured ladder positions came out within 0.06 mil of the published ones; the published values are what
this file uses.

## Known label overlap

The **300 m** anchor sits only 0.86 mil below a busy axis, so its distance label lands on the horizontal
hash numerals whichever side it is offset to. The other six are clear. This is cosmetic — blue label
over black etching — and the alternative was moving Leupold's own numerals somewhere they are not.
