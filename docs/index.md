---
title: Home
nav_order: 1
---

# Ballistic Calculator 2 — user guide

A free, open-source ballistic calculator for **Windows, Linux and macOS**, built on Avalonia UI. This is its
user guide: how to describe a load, zero it, run it, and read the answer — plus what the numbers mean
and where they stop being trustworthy.

## Start here

| If you… | Read |
|---|---|
| have never seen this application | [What Ballistic Calculator 2 is](about.md) — what it computes, and what it deliberately does not |
| want it running | [Installation and first run](installation.md) — from the Releases archive to a trajectory on screen |
| want to compute a shot | [Your first trajectory](first-trajectory.md) — the six tabs, the imperial/metric choice, and what OK does |
| are new to external ballistics | [Recommended reading](recommended-reading.md) — the books and references this guide leans on instead of repeating |

Everything else is below, in the [full contents](#all-articles).

## What this guide is, and what it is not

It documents **the application**, not ballistics. A concept is explained only where not explaining it
would leave part of the interface unusable — what a form factor is, why a drag table beats a ballistic
coefficient, why group size has to be entered as a 1σ per-axis figure, station pressure versus
sea-level pressure. Everything beyond that is someone else's book, and better in their hands than
ours: see [Recommended reading](recommended-reading.md).

It is also organised by **task**, not by menu. The order follows the work — describe the load, set the
conditions, zero it, run it, read the answer — and each step of the Shot Parameters dialog has its own
article, written around what you are trying to do rather than walking its fields top to bottom. The
exhaustive field-by-field tables live once, in the reference part.

## All articles

The guide is being written article by article. **Entries that are links are written; plain entries are
planned** — the structure is listed in full so you can see what is coming and what is missing.

**Getting started**

- [What Ballistic Calculator 2 is](about.md) — what it computes, what it does not, and the risk notice
- [Installation and first run](installation.md) — download to running app, and what it stores where
- [Your first trajectory](first-trajectory.md) — the imperial/metric choice, the six tabs, pressing OK
- [Updating the application](updating.md) — what a new release replaces, what it keeps, and how your
  presets survive it

**Building and running a shot** — one article per tab of the Shot Parameters dialog, in tab order

- [The Ammunition tab](ammunition-tab.md) — by hand, from a saved load, from a `.drg` curve; when
  diameter and length matter
- [The Weather tab](weather-tab.md) — altitude, pressure, temperature, humidity, and station versus
  sea-level pressure
- [The Wind tab](wind-tab.md) — the direction convention, zones along the flight path, and why the first
  zone does more than the others
- [The Rifle tab](rifle-tab.md) — sight height, click values, twist direction and rate; sight and
  barrel presets
- [The Zero tab](zero-tab.md) — zero distance, impact offset at zero, and zeroing with a *different*
  cartridge, atmosphere or wind than the shot
- [The Parameters tab](parameters-tab.md) — maximum range and step, shot angle, dialled clicks, and the
  Coriolis effect
- [Reading the table](reading-the-table.md) — every column, and the two conventions behind the drop and
  windage figures
- [The chart view](chart-view.md) — one variable against range, and the two-curve mode that shows the
  bullet crossing the sight line
- [The reticle view](reticle-view.md) — the sight picture, BDC marks, target boxes and moving-target lead
- The summary view — point-blank range, the dead zone, near and far zero, and where the bullet goes
  subsonic
- [Comparing loads](comparing-loads.md) — several trajectories on one chart, and what is worth comparing
- Saving and exporting — `.trajectory` files, and the two CSV formats

**Drag models — where accuracy comes from**

- [Choosing a drag model](choosing-a-drag-model.md) — what a BC actually is, when G1 misleads, and what the
  form-factor switch means
- [Custom drag tables](custom-drag-tables.md) — a projectile's own measured curve, and what a `.drg` does
  not carry
- [Approximating a drag table](approximating-a-drag-table.md) — from a multi-BC curve, or from measured
  velocities
- [Converting a ballistic coefficient](converting-a-bc.md) — the G1 ↔ G7 question, and why it is
  velocity-dependent

**Analysis**

- [Hit probability](hit-probability.md) — building an error budget that is not wishful, and reading the
  three outputs

**Libraries and editors**

- [Ammunition library and presets](library-and-presets.md) — stop re-typing loads, sights and barrels
- [The reticle editor](reticle-editor.md) — what the separate editor application is for, and its window
- [Reticle size and zero](reticle-parameters.md) — the coordinate space every element is placed in
- [Reticle elements](reticle-elements.md) — the six element types, and what draw order decides
- [Reticle paths](reticle-paths.md) — move-to, line-to and arc, for shapes the other types cannot make

**How the calculation works** — the mathematics, for readers who want to check a number rather than
take it on trust

- [The ballistic coefficient](method/ballistic-coefficient.md) — the drag law itself, what a BC divides,
  and why one number cannot describe a bullet at every velocity
- [The 3DOF point-mass model](method/3dof-model.md) — the two forces that are integrated, the frame they
  are written in, and how the state becomes a row of the table
- [The empirical corrections](method/empirical-corrections.md) — spin drift, aerodynamic jump and earth
  rotation: added to the trajectory rather than integrated with it, and why
- [Integration modes](method/integration.md) — Euler against midpoint Runge-Kutta, the internal step, and
  measured figures for what the choice is worth

**Reference**

- Units and measurement systems — every unit the app accepts, and the angular-unit choice
- File formats — `.drg`, ammunition, reticles and the saved shot, by hand
- [What the model includes](what-the-model-includes.md) — which effects are computed, which are
  approximated, which are absent, and the risk notice in full
- [Known problems](known-problems.md) — what is known, what is ours, and what is the platform's fault
- Troubleshooting and FAQ — the recurring stumbles, each pointing at the article that explains it
- [Recommended reading](recommended-reading.md) — the ballistics this guide deliberately does not teach

## Before you trust a number

The application simulates a complex physical process with a great many approximations. Results may be
used for education and for planning, but **must not** be treated as a reliable description of how a
projectile will actually behave — see the [risk notice](about.md#risk-notice) in full.

---

*Source, releases and issue tracker:
[github.com/nikolaygekht/ballistic.calculator.app.avalonia](https://github.com/nikolaygekht/ballistic.calculator.app.avalonia).
This guide lives in the `docs/` folder of that repository, so it can be read there as plain Markdown
as well as here.*
