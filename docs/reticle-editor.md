---
title: The reticle editor
nav_order: 16
---

# The reticle editor

**Goal of this article:** know what the reticle editor is for, when you need it, and how the window is laid
out. The three articles after it cover the work itself.

The editor is a **separate application** in the same folder as the calculator — `ReticleEditor.exe` on
Windows, `ReticleEditor` on Linux. It creates and edits `.reticle` files: the reticle definitions the
calculator's [reticle view](reticle-view.md) draws your solution onto.

<a href="screenshots/reticleeditor.png"><img src="screenshots/reticleeditor.png" width="880"
alt="The reticle editor: the rendered reticle on the left, and on the right the reticle parameters, the element list with each element spelled out as text, and the New/Edit/Duplicate/Delete/Up/Down buttons"></a>

*An M16 iron-sight picture mid-edit: the render on the left, parameters, element list and operations on the
right, cursor coordinates and the last action in the status bar.*

## Why it exists

Because the sight picture is only as truthful as the reticle behind it. The
[reticle view](reticle-view.md) tells you where a hold lands *in your reticle* — which marks to use, whether
a target fits between them, where a moving-target lead sits. All of that is worthless if the marks are on
the wrong subtensions.

Nine reticles ship in `data/reticle` — `mildot`, `h58`, `moa`, `bdc`, `chevron`, `german4`, `pso-1`,
`segmented` and an M16 iron-sight picture — and they cover the common patterns. Your scope's reticle is
quite possibly not among them, and this is what you use to describe it.

Three other reasons people end up here:

- **A simplified reticle to read holds from.** A bare cross with marks at the ranges you actually shoot is
  often more useful on screen than a faithful copy of a busy hunting reticle.
- **BDC anchors for your load.** A reticle can carry BDC points — positions the calculator labels with
  *distances* from the current trajectory. Put them on your reticle's real marks and the sight picture
  tells you which mark is 400.
- **Iron sights.** The shipped M16 picture is an aperture and a post, not a crosshair, and it works the
  same way.

## What it is not

It is not a drawing program. There is **no freehand, no dragging, no zoom or pan, and no undo**. Every
element is described by numbers you type — an angular position, an angular size — and the preview shows the
result. That sounds austere, and it is the right trade: a reticle is a measuring instrument, and 1.5 mil
means 1.5 mil, not "about here".

## The window

| | |
|---|---|
| **Left** | The reticle as it will look, scaled to fit and centred. It redraws after every change |
| **Right, top** | [Reticle parameters](reticle-parameters.md) — name, size, zero, and a **Set** button |
| **Right, middle** | The element list, each element spelled out as text (`Circle(p=(0mil:0mil),r=6mil,w=0.01mil,c=black)`) |
| **Right, bottom** | The element type dropdown and the operations: **New**, **Edit**, **Duplicate**, **Delete**, **Up**, **Down** |
| **Status bar** | The cursor's angular position, and a message reporting the last action |

The splitter between the two halves can be dragged, and its position — along with the window size and the
font size — is remembered between sessions.

The **View** menu holds three things worth knowing about:

- **Increase / Decrease Font Size** (`Ctrl`+`+` / `Ctrl`+`−`) — the editor's own text, useful on a dense
  element list.
- **Coordinate Display Units** — `Mil`, `MOA`, `in/100yd`, `cm/100m`. This changes only the **status bar
  readout**, not the reticle. It follows the reticle's own unit when you open a file.
- **Highlight Current Item** — redraws the selected element in blue on top of the reticle, which is how you
  find out which of forty lines you have selected.

## Files

Reticles are XML, saved as `.reticle`, and both the open and save dialogs start in the `data/reticle`
folder — the same folder the calculator's `Load…` button looks in, so anything you save there is
immediately available to the sight picture.

| | |
|---|---|
| `Ctrl+N` | New — a 10 × 10 mil reticle with zero in the middle |
| `Ctrl+O` | Open |
| `Ctrl+S` | Save |
| `Ctrl+Shift+S` | Save As |

**Nothing warns you about unsaved changes.** `File → New` replaces what you are working on without asking,
and closing the window does not prompt either. Save early and often, and use `Save As` before an
experiment.

## Next

[Reticle size and zero](reticle-parameters.md) — the coordinate space, which has to be set up before any
element can be added.

---

[← Contents](index.md)
