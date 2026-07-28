---
title: Reticle paths
nav_order: 19
---

# Reticle paths

**Goal of this article:** draw the shapes the other element types cannot — anything with a corner or a curve
that is not a whole circle.

A **path** is a single element made of a sequence of pen movements. One line width, one style, one colour and
one fill setting apply to the whole thing, and the sequence decides the shape. Chevrons, horseshoes, tapered
posts, the outline of an aperture field, a bracket around a target box: all paths.

Choose `Path` in the type dropdown and press **New** (or **Edit** an existing one) and you get a dialog that
is a small editor in its own right.

## The dialog

| Section | What it holds |
|---|---|
| **Path Parameters** | Line width, style (`Solid` / `Dashed` / `Dotted`), colour, and **Fill** — for the path as a whole |
| **Elements** | The sequence of pen movements, in order, with **Move To** / **Line To** / **Arc** to add one and **Edit** / **Delete** / **Up** / **Down** to work on the selection |
| **Preview** | The reticle with this path in it, redrawn after every change |
| Buttons | **OK**, **Cancel**, **Revert** |

**Revert** puts the path back to exactly how it was when the dialog opened — parameters and the whole
sequence — without closing. It is there because the dialog edits the real element as you go, so **Revert** is
your undo. **Cancel** does the same thing and then closes, so a cancelled path edit leaves nothing behind.

## The three movements

Think of a pen on paper. There is a current point; each entry either moves it without drawing or draws to a
new place.

### Move To

**Position X / Y.** Lifts the pen and puts it down somewhere. A path should begin with one — otherwise the
first drawing operation starts from an undefined point — and a later **Move To** in the middle of a path
starts a **new disconnected sub-shape** within the same element. That is how a reticle draws two mirrored
marks as one path.

### Line To

**Position X / Y.** A straight segment from the current point to there. The current point becomes the end.

### Arc

| Field | Notes |
|---|---|
| **End Position** X / Y | Where the arc finishes |
| **Radius** | The radius of the circle the arc is a piece of |
| **Clockwise** | Which way round it sweeps |
| **Major Arc** | The long way round instead of the short way |

An arc is defined by where it ends and how curved it is, not by its centre. Given a start point, an end point
and a radius there are **four** arcs that fit — two circles of that radius pass through both points, and each
offers a short way and a long way round. The two checkboxes pick between them: **Clockwise** chooses the
sweep direction and **Major Arc** chooses the long way. If an arc comes out mirrored or bulging the wrong
side, one of those two boxes is the fix; try them in turn rather than recomputing geometry.

The radius has to be at least half the distance between the two points — no circle smaller than that can
touch both. Keep the radius comfortably above that limit and arcs behave predictably.

## Order is the whole shape

The list is the pen's itinerary, so **Up** and **Down** do not tidy the list, they redraw the shape. Moving a
**Line To** earlier changes which point it starts from and which point the next entry starts from. If a path
comes out looking like a scribble, the sequence is wrong rather than the coordinates.

## Fill

**Fill** treats the path as an enclosed region and paints it. Without it you get an outline of the given line
width.

Two things follow:

- **A filled path does not need to be closed by hand.** The region between the last point and the first is
  filled as though it were.
- **A filled path covers what is under it**, and paths are usually large, so watch its position in the
  element list — see [draw order](reticle-elements.md#draw-order-is-list-order).

## A worked example: a chevron

A chevron with its point on the aiming point, 1 mil wide each side and 1.5 mil tall:

| # | Entry | Position |
|---|---|---|
| 1 | Move To | `−1`, `1.5` |
| 2 | Line To | `0`, `0` |
| 3 | Line To | `1`, `1.5` |

Three entries, no fill: a V with its apex exactly on the crosshair. Set the line width to taste — `0.1 mil`
for a fine mark, `0.3 mil` for something visible against clutter.

To make it a solid triangle instead, tick **Fill**: the open top closes and the shape becomes a filled
wedge. To make it a hollow outline of a thick chevron, extend the sequence back along the inside — five more
`Line To` entries — and fill that instead.

## Next

The manual's remaining parts — the drag-model articles, hit probability, the ammunition library and the
reference section — are listed in [all articles](index.md#all-articles) and still to be written.

---

[← Contents](index.md)
