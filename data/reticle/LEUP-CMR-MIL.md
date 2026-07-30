# LEUP-CMR-MIL — Leupold CMR-MIL Illum. FFP (Mark 4HD 2-10×)

A milliradian christmas tree: a graduated horizontal axis, five widening rows below it, and a small red
horseshoe at the aiming point. **First focal plane**, so the subtensions hold at every magnification.

## Everything is on a round milliradian

This is the simplest reticle in the folder to reproduce, because it is a pure mil grid — no load, no drop
table, no unit ambiguity. Every feature sits on a whole or half milliradian, and Leupold's drawing numbers
the axis and every row. Reading those numerals *is* reading the dimensions.

- **Horizontal axis** — graduated every **0.5 mil** out to **±10 mil**: a long tick on each whole mil, a
  short one on each half. Numerals on the even mils, 2 through 10, both sides.
- **Vertical spine** — graduated the same way, so the odd mils between rows are still measurable.
- **Five rows** at **2, 4, 6, 8 and 10 mil**, each graduated every 0.5 mil like the axis, and each **one
  milliradian wider than the row above**:

| Row | Bar half-width |
|---|---|
| 2 mil | ±2 mil |
| 4 mil | ±3 mil |
| 6 mil | ±4 mil |
| 8 mil | ±5 mil |
| 10 mil | ±6 mil |

- **Centre** — a red horseshoe open at the bottom, about **1.2 mil** across, with a small red aiming dot
  inside it.
- **Heavy posts** left, right and below, starting where the graduation ends and running to the field stop.

Each of the five rows carries a `<bdc>` anchor, so the application labels all five with the range your
trajectory crosses them.

## Geometric, not load-calibrated

The marks are places on a ruler. There is no cartridge, muzzle velocity or zero behind them, which is why
this file sits in the *Geometric* table of the [README](README.md). It is the mil-grid counterpart to
[LEUP-CCH](LEUP-CCH.md), with a far lighter tree — five rows against twenty.

## What is not published

Leupold's drawing gives the mil geometry and nothing else, so a few cosmetic choices are ours:

- **Line widths.** Not published. Fine lines are drawn at 0.05 mil and the heavy posts at 1.0 mil, which
  matches the drawing's proportions.
- **Tick lengths.** 0.40 mil on whole mils and 0.20 on halves — the drawing shows two lengths, and these
  are round values in that ratio.
- **The post taper.** The real posts taper from the field stop to a point where the graduation ends; they
  are drawn here as plain heavy lines.
- **The horseshoe's exact size** — about 1.2 mil across, read off the drawing rather than published.

None of those affect a measurement taken with the reticle; the graduation, the row positions and the row
widths are all exact.

## Wind holds

The tree itself is the wind hold — every intersection on every row is one, which is the point of a mil
grid. There are no separate wind marks.
