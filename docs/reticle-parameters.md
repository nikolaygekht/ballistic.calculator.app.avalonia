---
title: Reticle size and zero
nav_order: 17
---

# Reticle size and zero

**Goal of this article:** set up the reticle's coordinate space, and understand what the three parameters
mean — because every element you add afterwards is positioned in terms of them.

The top section of the right-hand panel holds three things and a button:

| Field | What it is |
|---|---|
| **Name** | What the reticle is called. It appears in the calculator's reticle view when the file is loaded |
| **Size (W×H)** | The total angular field the definition covers |
| **Zero (X,Y)** | Where the aiming point sits inside that field, measured **from the top-left corner** |
| **Set** | Applies all three. **Nothing takes effect until you press it** |

All four numbers are angular measurements, and you can type them in whichever angular unit you think in —
mil, MOA, in/100 yd, cm/100 m. The unit you type is the unit stored in the file.

## The coordinate space

This is the part worth getting straight once, because everything else depends on it.

**Size** is a rectangle of sky. A 12 × 12 mil reticle describes a 12 × 12 mil field; nothing outside it is
drawn. It is not your scope's field of view and it is not related to magnification — it is simply how much
angular space your drawing needs.

**Zero** places the aiming point within that rectangle, measured from the **top-left corner**, X rightwards
and Y downwards. For the shipped Mil-Dot reticle the size is 12 × 12 mrad and the zero is 6 / 6 mrad: dead
centre.

**Element coordinates are then measured from the zero point** — and this is the flip that catches people:

- **X is positive to the right**, negative to the left.
- **Y is positive upwards**, negative downwards.

So a drop mark 2 mrad below the crosshair is at `Y = −2`, and the shipped Mil-Dot file draws its crosshair as
a line from `(−5, 0)` to `(5, 0)` and another from `(0, −5)` to `(0, 5)`, with a circle centred on `(0, 0)`.

That is two different conventions in one dialog — the zero measured from a corner, the elements measured
from the zero — and it is worth reading twice. The reason is that the zero is a property of the *canvas*,
while elements are things you place relative to the *crosshair*.

## Choosing a size

Since everything is scaled to fit the preview, the size does not change how big the reticle looks on
screen. What it changes is **proportion**: a 20 mil-wide reticle drawn with 0.2 mil lines has finer-looking
lines than a 6 mil-wide one drawn with the same 0.2 mil.

Two practical points:

- **Make it big enough for the marks you need.** If your BDC marks run 8 mil below the crosshair, a 12 mil
  tall reticle with a centred zero cannot hold them — you have 6 mil of room below.
- **Off-centre zeros are normal for BDC work.** Size 12 × 20 with the zero at 6 / 6 gives you 6 mil above
  the crosshair and 14 below, which is the shape a drop-compensating reticle actually wants.

At the other extreme, the shipped M16 iron-sight picture is **350 × 350 moa with the zero at 175 / 175** —
an aperture sight subtends vastly more than a scope, and the numbers simply follow.

## The status bar is your measuring tool

Move the mouse over the preview and the status bar reports the cursor's position **relative to the zero
point**, to three decimals, in the display unit chosen under `View → Coordinate Display Units`. Outside the
drawn area it reads `--`.

That is the intended workflow, given there is no dragging: hover where you want something, read the
coordinates off the status bar, and type them into the element dialog. It is also how you check a drawing —
hover over a mark and confirm it is where you meant it to be.

Note that the display unit is a **readout preference only**. It does not convert the reticle, and it does
not change what the element dialogs expect; those accept any unit you type.

## Parameters first, elements second

The element type dropdown and all six operation buttons stay **disabled until the reticle has a non-zero
size**. That is deliberate: an element position means nothing without a coordinate space to be positioned
in.

So the order of work is always:

1. `File → New` (which starts you at 10 × 10 mil, zero at 5 / 5) or `File → Open`.
2. Set the name, size and zero, and press **Set**.
3. Add elements.

You can come back and change the size or the zero later — press **Set** again and the preview redraws — but
be aware that **element coordinates do not move with it**. Changing the zero shifts where every element
sits relative to the crosshair; changing the size can push elements outside the drawn field. Both are
occasionally what you want, and both are easy to do by accident.

## Next

[Reticle elements](reticle-elements.md) — the six element types, the fields each one needs, and how draw
order decides what covers what.

---

[← Contents](index.md)
