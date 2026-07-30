# VUDU-MD2 — EOTech Vudu MD2 (FFP, MOA)

The MOA counterpart of [MD1](VUDU-MD1.md): the same plain precision crosshair, graduated in minutes instead of
milliradians — and over a much wider field. **First focal plane.**

## The pattern

- **Fine crosshair**, **0.2 MOA** lines, illuminated (drawn red).
- **Graduated every 1 MOA** on all four arms, out to **±30 MOA**.
- **Long hashes 2 MOA** on the numbered 10-MOA marks, **1 MOA** on every other mark — exactly two lengths,
  with nothing intermediate at the 5-MOA positions.
- **Numerals 10, 20, 30** on all four arms.
- **Heavy posts**, **2 MOA** thick, outboard of the graduation.

## The marks

Six `<bdc>` anchors, every **5 MOA**: 5, 10, 15, 20, 25 and 30. Geometric — minutes on a ruler.

The graduation itself runs every 1 MOA, so there are thirty marks per arm, and only every fifth carries an
anchor. Anchoring every minute would put thirty overlapping labels down the arm; anchoring only the three
numbered 10 MOA marks — which is how this file was first built — gives too coarse a ladder to range with on a
reticle this deep. Every 5 MOA is the useful middle.

## Not the same field as MD1

MD1 is **±5 mrad (±17.2 MOA)**; MD2 is **±30 MOA (±8.7 mrad)**. They share a construction — fine red
crosshair, exactly two hash lengths, four heavy posts, no ring or dot — but not an extent. One generator
builds both, with unit, step, extent and hash lengths all as parameters.

## Which numbers came from where

The **reticle-index drawing, supplied by Nikolay**. Printed: the 0.2 MOA line width, the 1 MOA graduation,
the 2 MOA and 1 MOA hash lengths, the numerals every 10 MOA, and the 2 MOA post thickness.

That there are **only two hash lengths** was confirmed directly by Nikolay — the 5-MOA positions are ordinary
1 MOA hashes, not a third size. It is recorded because the drawing invites the opposite assumption.

## Calibration

| | |
|---|---|
| Cartridge / load | **not published** — a measuring reticle, not a BDC |
| Zero | not published |
| Focal plane | first (FFP) |
| Library entry | — |
