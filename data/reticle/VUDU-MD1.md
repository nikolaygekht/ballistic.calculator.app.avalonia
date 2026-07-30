# VUDU-MD1 — EOTech Vudu MD1 (FFP, milliradian)

A plain precision milliradian crosshair — no ring, no dot, no tree. Built for medium-to-long-range work where
a clean sight picture matters more than features. **First focal plane.**

## The pattern

- **Fine crosshair**, **0.05 mrad** lines, illuminated (drawn red, as EOTech's sheet does).
- **Graduated every 0.5 mrad** on all four arms, out to **±5 mrad**.
- **Long hashes 0.5 mrad** on whole milliradians, **short hashes 0.3 mrad** on halves — exactly two lengths.
- **Heavy posts** on all four arms, outboard of 5 mrad.

## Extent

**±5 mrad.** EOTech's `10 MIL` callout is the **full open width between the posts**, not the distance from
centre — its dimension line runs between the posts' inner edges, and the hash count agrees at ten 0.5 mrad
marks per arm.

Worth stating because it is easy to get backwards: [MD2](VUDU-MD2.md), the MOA version, graduates to ±30 MOA
(±8.7 mrad), so the two reticles **do not share a field**. Do not infer one's extent from the other.

## The marks

Five `<bdc>` anchors, on the whole milliradians 1 through 5. Geometric — milliradians on a ruler, so the
application supplies the ranges.

## Which numbers came from where

The **reticle-index drawing, supplied by Nikolay**. Every value above is a printed callout: the 0.05 mrad line
width, the 0.5/0.3 mrad hash lengths, the 0.5 mrad graduation (stated as a `1 MIL` bracket split by two
`0.5 MIL` halves) and the 10 mrad open width.

**Not published:** the heavy posts' thickness and how far out they run. Drawn as plain heavy lines to the frame
edge.

## Calibration

| | |
|---|---|
| Cartridge / load | **not published** — a measuring reticle, not a BDC |
| Zero | not published |
| Focal plane | first (FFP) |
| Library entry | — |

## See also

[MD2](VUDU-MD2.md) — the same construction graduated in MOA. Together they are what
[MILDOT](MILDOT.md) and [MOA-GRID](MOA-GRID.md) are to each other, except that these two differ in field as
well as unit.
