# MOA-GRID — MOA grid

A plain MOA-graduated crosshair: the MOA counterpart of [MILDOT](MILDOT.md), built on **the same field of
view** so the two can be compared directly. Zero in the centre.

## The same glass as MILDOT, graduated differently

The field is declared as **12 × 12 mrad** — MILDOT's own, to the digit rather than a rounded 41.25 MOA —
and the heavy posts start at **5 mrad (17.19 MOA)**, exactly where MILDOT's do. Everything inside is
graduated in **MOA**. Switching between the two files therefore changes the ruler and nothing else, which
is the point: the same sight picture measured in minutes instead of milliradians.

## The pattern

- **Bounding circle** at 20.62 MOA radius (6 mrad, the field stop).
- **Fine cross** across the full field, with **heavy posts** on all four arms from 17.19 MOA out to the
  field stop.
- **Hash marks every 1 MOA** out to **±14 MOA** on both axes, alternating short (odd MOA, 0.5) and long
  (even MOA, 0.7) so the count reads in twos.

±14 MOA is the match to MILDOT's outermost dots at ±4 mrad (±13.75 MOA) — near enough on a round MOA
value, and comfortably past the ±12 MOA this grid needed.

## The marks

Geometric — the `<bdc>` anchors are every 2 MOA:

| Position | |
|---|---|
| +4, +2 MOA | above the zero |
| −2 … −14 MOA | below the zero, every 2 MOA |

Places on a ruler; the overlay supplies the ranges from whatever load is loaded. The two anchors above the
zero label hold-*unders* — distances nearer than the zero — which is what the *Near BDC* overlay draws.

## What changed, and why

This file was previously a **32 × 32 MOA** field graduated only to **±8 MOA**, with posts from 9 MOA. That
was too small to compare against MILDOT: it covered barely half the angular field, so the same target
filled very different fractions of the two pictures.

The rebuild also converted the line widths from `mil` to `moa` — 0.05 mil and 0.1 mil became 0.17 and
0.34 MOA, the same weights expressed in the unit the rest of the file uses. Widths are never measured with,
so nothing about the reticle's use changes; it removes a stray military-mil value from a MOA reticle, which
is the sort of thing that misleads a later reader.
