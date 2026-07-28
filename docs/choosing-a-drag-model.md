---
title: Choosing a drag model
nav_order: 20
---

# Choosing a drag model

**Goal of this article:** decide what to describe your bullet's drag with, understanding what a ballistic
coefficient actually is and where it stops being good enough.

This is the part of the manual that is also the application's argument. Everything else — the atmosphere,
the wind, the zero — is arithmetic once the drag is right, and drag is where solvers differ.

## What a ballistic coefficient is

A **drag table** (G1, G7 and the rest) is a curve: the drag coefficient of one particular reference
projectile, measured across the whole speed range. There are nine of them here — `G1`, `G2`, `G5`, `G6`,
`G7`, `G8`, `GI`, `GS`, `RA4` — each describing a differently shaped standard shell or bullet.

A **ballistic coefficient** is then a single number saying *how much less* your bullet slows down than
that reference does. BC 0.5 against G1 means "half as draggy, in G1's proportions, at every speed".

That last clause is the whole problem. A BC is not a property of your bullet — it is a **ratio between
your bullet and a reference shape**, and it is only constant if the two have the same drag *shape*. They
do not. Real bullets and reference projectiles diverge most where it matters least conveniently: through
the transonic region.

## G1 versus G7, briefly

- **G1** is a flat-based, blunt reference from the 19th century. Almost nothing modern looks like it, so
  the ratio to G1 changes noticeably with speed. A G1 BC quoted as one number is an average over some
  velocity band, and it is high at the muzzle and low downrange.
- **G7** is a boat-tailed, pointed reference. A modern long-range bullet is genuinely similar in shape, so
  its ratio to G7 stays much more nearly constant — which is why a G7 BC extrapolates better.

The practical rule: **take the table from wherever the number came from.** A G7 coefficient entered as G1
is not a small error, it is a different bullet. Where a manufacturer publishes both, prefer the G7 figure
for long range.

## The four ways to describe drag here

In ascending order of how much they actually know about your bullet:

| | What you supply | When it is right |
|---|---|---|
| **A single BC** on a standard table | One number and the table it belongs to | Ordinary shooting, moderate range, a number you trust |
| **A form factor** on a standard table | A shape factor plus weight and diameter | Data sheets that quote form factor rather than BC |
| **An approximated curve** | A multi-BC table, or measured downrange velocities | You have more than one number but not a measured curve — see [approximating a drag table](approximating-a-drag-table.md) |
| **A measured `.drg` curve** | The projectile's own Cd against Mach | Whenever you can get one. See [custom drag tables](custom-drag-tables.md) |

The last row is the point of this application. A point-mass solver running the projectile's **own** drag
curve tracks a 4DOF solver closely, because the thing 4DOF adds is the bullet's angular motion, not a
better drag model. Chasing drag beats chasing degrees of freedom.

## The form-factor switch

On the [Ammunition tab](ammunition-tab.md) the **BC is Form Factor** checkbox changes how the number
beside it is read.

- **Unticked** — the number is a **coefficient**: the ratio described above, used directly.
- **Ticked** — the number is a **form factor**, and the coefficient is computed from it and the bullet's
  **sectional density**: weight over diameter squared. Which is why ticking it makes the **diameter
  mandatory** — see [when diameter and length are needed](ammunition-tab.md#when-diameter-and-length-are-needed).

A form factor of **1** means "exactly as draggy as the reference shape, scaled by sectional density". That
convention is what makes measured tables work: a `.drg` file carries the projectile's own drag, so it is
used with a form factor of 1 on the `GC` (custom) table, and the file's own curve does the rest.

## When to stop worrying

Being honest about proportion matters as much as being right about physics:

- **Inside 300 m, a decent single BC is plenty.** The difference between G1 and G7 bookkeeping, or between
  a published BC and a measured curve, is smaller than your group.
- **From 500 to 800 the drag model starts to show**, and a G7 figure or a measured curve earns its keep.
- **Past 800, and through the transonic region, it dominates.** This is where a single G1 number
  extrapolated from a muzzle-band average goes visibly wrong, and where a measured curve is worth more
  than any amount of care over the atmosphere.

And in all three cases the muzzle velocity you measured yourself matters more than the drag model you
chose. A perfect curve on a 50 ft/s velocity error is still a miss.

## Next

[Custom drag tables](custom-drag-tables.md) — the projectile's own measured curve, and what a `.drg` file
does and does not carry.

---

[← Contents](index.md)
