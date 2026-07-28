---
title: Your first trajectory
nav_order: 4
---

# Your first trajectory

**Goal of this article:** get one shot from an empty window to a firing solution, and know which of the
six tabs owns which decision. It is the map; each step has (or will have) its own article for the
detail.

Everything starts with **`Trajectory → New`**. One dialog — *Shot Parameters* — describes the whole
shot across six tabs, and **OK** computes it. There is no separate "calculate" step and nothing is
computed while you type.

## First, imperial or metric

`Trajectory → New` does not open the dialog straight away; it asks for a measurement system first:

| | |
|---|---|
| **`Trajectory → New → Imperial`** (`Ctrl+I`) | grains, feet per second, inches, yards, inHg, °F |
| **`Trajectory → New → Metric`** (`Ctrl+M`) | grams, metres per second, millimetres, metres, hPa, °C |

Three things are worth knowing about that choice, because it is less binding than it looks:

- **It belongs to the window, not to the application.** Several trajectory windows can be open in
  different systems at once, and each keeps its own.
- **It is not permanent.** `View → Measurement System` (`Ctrl+Shift+I` / `Ctrl+Shift+M`) restates the
  active window in the other system, *converting* the values rather than relabelling them. You will not
  lose a load by starting it in the wrong system.
- **Angular units are a separate choice.** `View → Angular Units` — MOA, mils, thousandths,
  milliradians, in/100 yd, cm/100 m — does not follow imperial versus metric, because a metric shooter
  with an MOA scope is perfectly normal. Scope click values in the sight dictionary are always mils.

Two smaller consequences, in case you meet them: the standalone tools under `Tools` (both drag-table
builders, the BC converter, the sight and barrel dictionaries) belong to no window, so they open in the
system of the **last trajectory you created** — remembered between sessions, imperial until you create
your first. And **opening a saved `.trajectory` file restores the system that file was saved in**,
without changing that preference.

## The six tabs

The dialog is one tab per step of the work, in the order the work happens: **Ammunition**, **Weather**,
**Wind**, **Rifle**, **Zero**, **Parameters**. Each has its own article — see
[all articles](index.md#all-articles); only [the Ammunition tab](ammunition-tab.md) is written so far.

Only the first tab has to be filled in: the projectile. The other five have defaults and the dialog will
offer them, so a load on its own is enough for a first answer.

## Pressing OK

The dialog checks three things, in this order:

1. **Ammunition must be complete.** Missing weight, BC or muzzle velocity gives *"Ammunition data is
   required."* — nothing else is checked until it is filled in.
2. **A half-filled tab is an error.** If you have entered part of the Weather, Rifle, Zero or
   Parameters tab, the dialog names the tab: *"Not all required data filled in: …"*. It will not guess
   the rest.
3. **An untouched tab is offered a default.** Tabs left completely empty are listed with *"… not
   filled. Use default values?"* — answer yes and these are used:

   | Tab | Default |
   |---|---|
   | Weather | the standard (ICAO) atmosphere |
   | Wind | none — still air |
   | Rifle | a 3 in sight height, and no rifling, so no spin drift or aerodynamic jump |
   | Zero | 100 yd (imperial) or 100 m (metric) |
   | Parameters | out to 1,000 yd/m in 100 yd/m steps |

So the shortest possible first run is: fill in the Ammunition tab, press **OK**, accept the defaults.
That is a real trajectory — enough to see the machinery work before you start refining inputs.

## Reading the answer

OK opens one window, titled with the ammunition name, holding four views of the same shot:

| View | | Shortcut |
|---|---|---|
| **Table** | the numbers, row per step: range, velocity, Mach, drop, hold, clicks, windage, time of flight, energy, optimal game weight | `Ctrl+T` |
| **Chart** | one variable against range — velocity, Mach, drop, windage or energy | `Ctrl+C` |
| **Reticle** | the sight picture through your reticle, with BDC and target overlays | `Ctrl+R` |
| **Summary** | point-blank range and dead zone, near and far zero, and where the bullet goes subsonic | |

<a href="screenshots/ballistic_table.png"><img src="screenshots/ballistic_table.png" width="880"
alt="The trajectory table out to 1,000 yards: range, velocity, Mach, drop, hold, clicks, windage, windage adjustment, time of flight, energy and optimal game weight"></a>

*Articles to come: reading the table; chart, reticle and summary.*

## Changing your mind

**`View → Edit Parameters`** (`Ctrl+E`) reopens the dialog for the active window with everything as you
left it, and recomputes on OK. Iterating on one shot is the normal way to work — you do not need a new
window to try a different zero or a stiffer wind.

When the answer is worth keeping, `Trajectory → Save` writes a `.trajectory` file holding the whole
shot (and the window's measurement and angular units); `Trajectory → Export As CSV` writes the table
itself, in either a local Excel-friendly format or a portable invariant one. To put two loads side by
side, open both and use `View → Compare → Add`.

---

[← Contents](index.md)
