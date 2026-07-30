---
title: Reticle elements
nav_order: 18
---

# Reticle elements

**Goal of this article:** build a reticle out of the six element types, and control what covers what.

A reticle is a list of elements drawn in order. The middle section of the right-hand panel is that list,
each entry spelled out as text — `Line(s=(-5mil:0mil),e=(5mil:0mil),w=0.01mil,c=black)` — and the bottom
section is what you do to it.

Every position and size below is an **angular measurement relative to the zero point**, X positive right and
Y positive up, as [the previous article](reticle-parameters.md#the-coordinate-space) describes.

## The operations

| Button | What it does |
|---|---|
| **New** | Adds an element of the type chosen in the dropdown, opening its dialog empty |
| **Edit** | Opens the selected element's dialog |
| **Duplicate** | Appends an exact copy of the selected element |
| **Delete** | Removes the selected element |
| **Up** / **Down** | Moves the selected element earlier or later in the list |

All but **New** need a selection. The status bar reports what happened after each one — including
`Edit cancelled`, so you can tell a cancelled dialog from one that did nothing.

**Duplicate lands exactly on top of the original.** It is an exact copy appended at the end of the list, so
you will not see anything change; the copy is selected, so `Edit` it straight away and move it. That is the
normal way to build a row of identical marks: duplicate, edit the Y, repeat.

## Draw order is list order

Elements are drawn in the order they appear, so **later elements cover earlier ones**. That makes **Up** and
**Down** more than tidying:

- A filled rectangle added late will hide everything underneath it. Move it up the list — earlier — and it
  becomes a backdrop instead.
- A fine mark that has vanished is usually not mis-positioned but painted over by something coarser.
- White elements are how you cut a hole in something dark: draw the dark shape first, the white one after.
  The M16 aperture picture is built that way.

## The five drawing types

### Line

| Field | Notes |
|---|---|
| **Start Point** X / Y, **End Point** X / Y | Both ends, in reticle coordinates |
| **Line Width** | Angular, like everything else. In the shipped Mil-Dot, `0.01 mrad` is the hairline crosshair and `0.2 mrad` the heavy outer bars |
| **Style** | `Solid`, `Dashed` or `Dotted` |
| **Color** | 28 named colours, from `black` and `white` through `gray`, `red`, `darkblue`, `gold` and the rest |

The workhorse. Crosshairs, tick marks, ladders, posts.

### Circle

Centre X / Y, **Radius**, line width, style, colour, and a **Fill** checkbox. Filled circles make dots — the
"dot" in Mil-Dot — and unfilled ones make rings, which are useful as range or reference circles.

### Rectangle

Position X / Y (its **top-left corner**), **Size** width × height, line width, style, colour, **Fill**. Used
for boxes, blocks, and as filled backdrops.

### Text

| Field | Notes |
|---|---|
| **Position** X / Y | Where the text sits |
| **Text** | What it says |
| **Text Height** | Angular — the text is measured in the reticle's own units, so it scales with everything else |
| **Anchor** | `Left`, `Center` or `Right` — which end of the string sits at the position |
| **Color** | As above |

Anchor is what makes a column of numbers line up: right-anchored labels to the left of a vertical ladder,
left-anchored to the right of it.

### Path

Anything with a corner or a curve that is not a circle: chevrons, horseshoes, tapered posts. It has enough
in it to deserve [its own article](reticle-paths.md).

## BDC points are not drawings

The sixth type, **BDC Point**, is different in kind. It does not draw a shape — it marks a **position the
calculator will label with a distance**.

| Field | Notes |
|---|---|
| **Position** X / Y | Where the mark is |
| **Text Height** | The angular height of the label the calculator will draw |
| **Text Offset** | How far to the side of the position that label sits |

In the editor's preview, each BDC point shows as a **dark blue circle** with its own Y coordinate as the
label — the editor has no trajectory, so the only number it can show you is where the point is.

In the calculator, the same point gets labelled with the **range** at which your current load's drop lands
there, which is what the [reticle view](reticle-view.md#far-bdc)'s Far and Near BDC overlays are made of. So
BDC points are anchors: you put them on the marks your reticle really has, and the solver names them for the
load you are shooting.

They live in the same list as the drawing elements and take the same Duplicate / Delete / Up / Down
operations.

## Seeing what you have selected

`View → Highlight Current Item` redraws the selected element **in blue, on top of everything**. On a busy
reticle it is the only quick way to tell which of many similar lines is selected.

It is a copy of the element with the colour changed, so a filled shape highlights as a filled blue shape —
which can hide what is underneath it while the highlight is on. Turn it off when you want to judge the
finished drawing.

## Next

[Reticle paths](reticle-paths.md) — the one element type with its own editor inside the editor.

---

[← Contents](index.md)
