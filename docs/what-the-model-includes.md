---
title: What the model includes
nav_order: 32
---

# What the model includes — and what it does not

**Goal of this article:** judge the numbers. Know which physical effects the engine actually computes,
which it approximates, which it ignores entirely, and where that puts the limit on how far a result can
be trusted.

Every ballistic solver is a simplification. The useful question is never "is it accurate?" but "which
effects does it include, and are the ones it leaves out big enough to matter for the shot I am planning?"
This article answers the first half so you can answer the second.

## The engine is a 3DOF point-mass model

The bullet is treated as a **point with mass**. Three degrees of freedom means three position
coordinates — downrange, vertical, lateral — integrated forward in time. The forces acting on that point
are drag along the air-relative velocity vector and gravity, plus several explicit correction terms
listed below.

What a point has no notion of is **which way it is pointing**. The bullet's orientation — yaw, pitch,
precession, nutation — is not part of the state being integrated. That is the single most important thing
to understand about the model, and most of the "not included" list below follows from it.

## What is computed

| Effect | How |
|---|---|
| **Drag** | From a drag table, at the current Mach number, divided by the ballistic coefficient. Standard curves G1, G2, G5, G6, G7, G8, GI, GS and RA4, or a projectile's own measured curve from a `.drg`, or a multi-BC profile |
| **Gravity** | Constant, 9.80665 m/s² |
| **Air** | Density from the station pressure, temperature and humidity; speed of sound, which is what turns a velocity into the Mach number the drag curve is read at |
| **Wind** | A horizontal vector split into range and cross components, in as many zones along the flight path as you define |
| **Spin drift** | Litz's approximation from the Miller stability coefficient — `1.25 × (Sg + 1.2)`. Folded into the windage figure, not reported separately |
| **Crosswind aerodynamic jump** | Litz, *Applied Ballistics* Eq 5.4. A pure crosswind moves the impact **vertically** as well as horizontally |
| **Coriolis** | Two distinct effects — see below |
| **Shot angle and cant** | The line-of-sight incline, and a rotated sight |
| **Sight geometry** | Sight height above the bore, the zero, and clicks already dialled |

Three of these deserve more than a table row.

### Spin drift and aerodynamic jump need three inputs

Both are computed from the **Miller twist-rate stability coefficient** (Sg), which needs the barrel
twist, the bullet diameter **and** the bullet length. Leave any of the three out and both effects are
silently **absent** — not wrong, not zero-by-physics, simply not in the answer. This is the most common
reason two solvers disagree at long range, and the most common reason a windage figure looks too small.

The [Ammunition tab](ammunition-tab.md) article says which fields these are; the
[Rifle tab](rifle-tab.md#the-barrel-what-the-twist-actually-buys) covers the twist.

### Coriolis is two effects, not one

- **Horizontal** — depends on **latitude only**, and is independent of which way you are facing.
  Deflects right in the northern hemisphere.
- **Vertical (Eötvös)** — depends on the **compass bearing**. Firing east makes the bullet effectively
  lighter, west heavier; due north or south it vanishes.

So latitude alone is the honest answer when you do not know the bearing: it gets the horizontal term
right and leaves the vertical one out. [The Parameters tab](parameters-tab.md) gives measured magnitudes
for both, and they are smaller than most people expect.

### Sg is used, but never judged

The stability coefficient is computed to scale drift and jump. It is **not** reported, and nothing warns
you when it comes out marginal. A bullet that would be barely stabilised in reality — an Sg near or below
1.4 — is integrated here exactly as if it flew perfectly, because a point mass cannot be unstable.

## What is not computed

- **Bullet orientation, and everything that follows from it.** No yaw, no pitch, no precession or
  nutation, no yaw of repose beyond the spin-drift approximation above, and no dynamic instability. The
  drag curve does contain the transonic drag rise, so the *drag* through Mach 1 is modelled; what is not
  modelled is a bullet becoming unsettled there.
- **Vertical wind.** Wind is a horizontal vector. Updraughts, downdraughts and thermals over a valley
  have no representation — and they are one of the larger unmodelled effects in real field shooting.
- **Spin decay.** The stability coefficient is computed once, at the muzzle, from the muzzle velocity. In
  flight, spin decays more slowly than forward velocity, so Sg actually rises with range; the model holds
  it constant.
- **Any variation in the ammunition.** One muzzle velocity, one ballistic coefficient, for every shot.
  Lot-to-lot differences, velocity spread and BC scatter are not here — [hit
  probability](hit-probability.md) is the separate tool that models the *spread* rather than the flight.
- **Powder temperature sensitivity.** The muzzle velocity you enter is the muzzle velocity used, whatever
  the Weather tab says. A load that loses 20 ft/s per 10 °C will not do so here; enter the velocity for
  the conditions you expect.
- **A changing atmosphere along the flight path.** One set of conditions covers the whole flight. Shooting
  from a cold valley floor into warm air above, or across 500 m of altitude change, is outside the model.
- **The barrel.** No harmonics, no muzzle blast, no tip-off, no barrel-time effects.
- **Anything after impact.** No deformation, tumbling, penetration, ricochet or terminal ballistics.

## What a 4DOF model would add — and what it would not

A 4DOF model tracks the bullet's angular motion as well as its position. That buys a computed yaw of
repose instead of an approximated one, better long-range spin drift, and honest transonic behaviour.

It is worth being clear about what it does **not** buy: **a better drag curve.** The dominant source of
error in any solver is the drag data — a 4DOF model fed a guessed G1 coefficient is less accurate than
this 3DOF one fed a measured `.drg`. If you want a better answer, better drag data is nearly always the
cheaper improvement. See [choosing a drag model](choosing-a-drag-model.md) and
[custom drag tables](custom-drag-tables.md).

## Numerical accuracy, as distinct from physical accuracy

Even a perfect model has to be integrated numerically. Three things are worth knowing:

- **The integrator is midpoint Runge–Kutta (RK2)**, evaluating the acceleration twice per step. The
  engine also offers plain Euler for historical comparison; the application does not expose it, so you
  always get RK2.
- **The internal step is not the step you set.** The engine halves your output step and, if that is still
  coarse, divides it down by powers of ten. The `Step` on the Parameters tab decides how many **rows** you
  get, not how accurately they were computed — see [the Parameters
  tab](parameters-tab.md). The reticle and summary views ignore it entirely and use their own fine
  trajectory.
- **The run has two stop conditions.** It ends when the projectile drops below **50 ft/s** or falls more
  than **10,000 ft** below the sight line. A table that stops before your maximum range has hit one of
  them; nothing is wrong.

The zero is found iteratively, to a default accuracy of 0.1 mm — far finer than any other error here.

## Checking a number yourself

The engine is a separate open-source project, and it is the place to look when you want to know how a
figure was produced rather than take this article's word for it:
[BallisticCalculator](https://github.com/gehtsoft-usa/BallisticCalculator1). The formulae named above —
Miller for stability, Litz for spin drift and aerodynamic jump, the Eötvös term for vertical Coriolis —
are all standard and published; [recommended reading](recommended-reading.md) lists where.

## Risk notice

Repeated here in full rather than left as fine print at the end of [another
article](about.md#risk-notice), because this is the article about how far to trust the output:

> The application performs a very limited simulation of a complex physical process and therefore makes a
> great many approximations. The calculation results MUST NOT be considered as completely and reliably
> reflecting the actual behaviour or characteristics of projectiles. While these results may be used for
> educational purposes, they must NOT be considered reliable in any area where an incorrect calculation
> could lead to a wrong decision, financial harm, or risk to human life.

The practical reading of that: this is a tool for **planning and learning**, and the ground truth is
always what your rifle does on paper at a measured distance. Use the model to decide what to test, then
test it.

## Next

[Known problems](known-problems.md) — the defects and platform quirks that are known rather than
theoretical.

---

[← Contents](index.md)
