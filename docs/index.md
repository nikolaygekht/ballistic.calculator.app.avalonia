---
title: Home
nav_order: 1
---

# Ballistic Calculator 2 — user guide

A free, open-source ballistic calculator for **Windows and Linux**, built on Avalonia UI. This is its
user guide: how to describe a load, zero it, run it, and read the answer — plus what the numbers mean
and where they stop being trustworthy.

## Start here

| If you… | Read |
|---|---|
| have never seen this application | [What Ballistic Calculator 2 is](about.md) — what it computes, and what it deliberately does not |
| want it running | [Installation and first run](installation.md) — from the Releases archive to a trajectory on screen |
| want to compute a shot | [Your first trajectory](first-trajectory.md) — the six tabs, the imperial/metric choice, and what OK does |
| are describing a bullet | [The Ammunition tab](ammunition-tab.md) — by hand, from a saved load, or from a measured `.drg` curve |
| are new to external ballistics | [Recommended reading](recommended-reading.md) — the books and references this guide leans on instead of repeating |

## What this guide is, and what it is not

It documents **the application**, not ballistics. A concept is explained only where not explaining it
would leave part of the interface unusable — what a form factor is, why a drag table beats a ballistic
coefficient, why group size has to be entered as a 1σ per-axis figure, station pressure versus
sea-level pressure. Everything beyond that is someone else's book, and better in their hands than
ours: see [Recommended reading](recommended-reading.md).

It is also organised by **task**, not by menu. Pages follow the order the work actually happens in —
describe the load, set the conditions, zero, run, read — rather than walking the dialog tab by tab.
Fields are documented where a task needs them.

## Still to come

The guide is being written article by article. Planned, in reading order:

- **Getting started** — [installation](installation.md) and
  [your first trajectory](first-trajectory.md) (both done).
- **Building and running a shot** — one article per step of the Shot Parameters dialog.
  [The Ammunition tab](ammunition-tab.md) is written; still to come are the Weather and Wind tabs
  (atmosphere, wind zones and the direction convention), the Rifle tab (sight height, click values,
  twist), the Zero tab (including zeroing with a *different* cartridge, atmosphere or wind than the
  shot), and the Parameters tab (range, step, shot angle, dialled clicks, the Coriolis effect). Then
  reading the table; the chart, sight picture and summary views; comparing loads, saving and CSV export.
- **Drag models** — choosing between a standard curve and a measured one; custom `.drg` tables;
  approximating a table you do not have from a multi-BC curve or from measured downrange velocities;
  converting a ballistic coefficient between standard tables.
- **Analysis** — hit probability, and how to build an error budget that is not wishful.
- **Libraries and editors** — the ammunition library, sight and barrel presets, and the reticle editor.
- **Reference** — units and measurement systems; file formats; what the model does and does not
  include; troubleshooting; [recommended reading](recommended-reading.md) (done).

## Before you trust a number

The application simulates a complex physical process with a great many approximations. Results may be
used for education and for planning, but **must not** be treated as a reliable description of how a
projectile will actually behave — see the [risk notice](about.md#risk-notice) in full.

---

*Source, releases and issue tracker:
[github.com/nikolaygekht/ballistic.calculator.app.avalonia](https://github.com/nikolaygekht/ballistic.calculator.app.avalonia).
This guide lives in the `docs/` folder of that repository, so it can be read there as plain Markdown
as well as here.*
