---
title: Custom drag tables
nav_order: 21
---

# Custom drag tables (`.drg`)

**Goal of this article:** use a projectile's own measured drag curve, and know what the file carries, what
it does not, and how it travels.

A `.drg` file is a **measured drag curve**: the drag coefficient of one specific projectile against Mach,
usually recovered from Doppler radar. It replaces the "ratio to a reference shape" bookkeeping described in
[choosing a drag model](choosing-a-drag-model.md) with the thing that bookkeeping was approximating.

## Where they come from

- **Shipped with the application.** `data/drg` holds a large set of radar-derived tables for Lapua
  projectiles, published by the manufacturer, organised by calibre.
- **From a manufacturer.** A handful of makers publish measured curves for their bullets.
- **Built here.** `Tools → Approximate Drag Table` derives one from a multi-BC curve or from measured
  downrange velocities — see [approximating a drag table](approximating-a-drag-table.md).

## Using one

On the [Ammunition tab](ammunition-tab.md#driving-the-shot-from-a-measured-drag-curve), press
**`Browse…`** beside *Drag table* and pick the file. Five things happen, and they are worth knowing
because together they *are* the convention:

1. **The BC becomes `1` and the table becomes `GC`** — "custom curve". There is no coefficient to quote:
   the curve is the drag model.
2. **BC is Form Factor is ticked.** A form factor of 1 says "as draggy as this curve says", which is what
   makes the file's own numbers the answer.
3. **Weight, diameter and length are filled in** from the file's header where it carries them, converted
   into the window's units.
4. **Name and source are filled in** if you left them empty.
5. **Muzzle velocity is not touched.**

That last one is the standing trap and it is not a defect: a `.drg` describes a **projectile**, not a
**load**. The file cannot know what your barrel does with it, so the muzzle velocity is always yours to
enter.

## What the file carries

A `.drg` holds the curve plus a small header: **name**, **source**, **weight**, **diameter** and
**bullet length**.

Two consequences:

- **Weight and diameter are not decoration.** The curve is stored scaled by the projectile's sectional
  density, so those two numbers are part of the drag model. If a file's header lacks a diameter, `Browse…`
  leaves the field empty and the shot **cannot be computed** — a form factor needs a diameter. Type it in.
- **Bullet length is a recent addition.** Files written before it existed store the length and source slots
  as `0`, and the application treats non-positive as absent rather than overwriting a good value with
  nothing. Without a length there is no spin drift and no aerodynamic jump, so fill it in if you know it.

And what it does **not** carry: muzzle velocity, barrel length, powder charge, your zero — nothing about a
load or a rifle. A `.drg` is one bullet's aerodynamics and nothing else.

## How the reference travels

The ammunition record stores the `.drg` **file name**, not a copy of the curve. That has three effects
worth knowing:

- **Editing the file changes your next answer.** The table is read at calculation time, cached by path and
  last-write time, so a corrected `.drg` takes effect without reloading anything by hand.
- **The file is found again by name.** If the stored path no longer exists, the same file name is looked
  for in `data/drg` **and its subfolders**. So a `.ammox` or `.trajectory` you share works on another
  machine as long as the recipient has that `.drg` — one of the shipped ones, or dropped into `data/drg`.
- **If it cannot be found at all, the shot fails.** A `GC` coefficient has no curve to fall back on, and
  there is no standard table hiding behind it. That is the price of the convention.

## Clearing one

**`Clear`** removes the reference. Because a `GC` coefficient without a table is uncomputable, clearing
also resets the BC to **`0.5 G1`** and unticks the form-factor switch — a deliberately implausible
placeholder rather than a number you might mistake for real data.

## Is it worth it?

Yes, and more than any other single input past mid-range — but with two caveats worth stating plainly.

**The curve must be your bullet.** A radar table for the 8.8 g Scenar-L is not a table for the 9.0 g
Scenar, and a table for a bullet from a different lot with a different meplat is only approximately yours.
Matching the exact designation matters.

**It does not fix the load.** A measured curve with a guessed muzzle velocity is a precise answer to the
wrong question. If you are going to the trouble of a measured drag curve, chronograph the load too.

## Next

[Approximating a drag table](approximating-a-drag-table.md) — building a usable curve when nobody has
measured your bullet.

---

[← Contents](index.md)
