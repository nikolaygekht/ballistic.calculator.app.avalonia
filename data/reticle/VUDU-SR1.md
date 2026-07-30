# VUDU-SR1 — EOTech Vudu SR-1 (FFP, milliradian)

EOTech's most general-purpose Vudu reticle: a fine milliradian crosshair for ranging, holdover and wind.
**First focal plane**, so the subtensions hold at every magnification.

## The pattern

- **Fine crosshair**, **0.05 mrad** lines, illuminated (drawn red as EOTech's sheet does).
- **Hashes on every whole milliradian, 1 through 5**, on all four arms, each **0.8 mrad** long.
- **0.29 mrad (1 MOA)** centre dot.

That is the whole reticle as this file draws it — see below.

## The marks

| Position | |
|---|---|
| 1, 2, 3, 4, 5 mrad | on all four arms |

Geometric: the marks are whole milliradians, not one load's drops, so the application labels them from
whatever trajectory is loaded. The `<bdc>` anchors are on the five below-centre hashes.

## The speed ring is deliberately absent

The SR-1 is a *Speed Ring* reticle, and this file **does not draw the ring**. That is a scoping decision, not
an oversight: EOTech's manual dimensions only the inner crosshair — p7 draws the crosshair alone, and p8, the
"Speed Ring" section, gives no dimensions at all. Its siblings' rings are 2.15 mrad OD ([SR-2](VUDU-SR2.md),
[SR-3](VUDU-SR3.md)) and 2.55 mrad OD ([SR-4](VUDU-SR4.md), [SR-5](VUDU-SR5.md)), so a diameter could have
been borrowed — but which family SR-1 belongs to is exactly what is unpublished, and shipping a guessed
aiming feature is worse than shipping none.

## Which numbers came from where

All four values above are printed on `Vudu_Reticle_Manual_SR1_RevB.pdf` p7. The page states *"Subtensions
measured in MRADs. Image shown is for representation only"* — so it is a dimensioned schematic, not a scale
drawing, and nothing here was measured off it.

The **line width of 0.05 mrad** is the one value not printed on the SR-1 page; it is taken from
[MD1](VUDU-MD1.md), where EOTech print exactly that figure for the same style of fine crosshair.

## Calibration

| | |
|---|---|
| Cartridge / load | **not published** — this is a measuring reticle, not a BDC |
| Zero | not published |
| Focal plane | first (FFP) |
| Library entry | — |
