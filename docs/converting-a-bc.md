---
title: Converting a ballistic coefficient
nav_order: 23
---

# Converting a ballistic coefficient

**Goal of this article:** answer the everyday "my bullet is quoted G1 but I want G7" question, and
understand why the answer comes with a reference velocity attached.

`Tools → Convert Ballistic Coefficient…` does one thing: restates a coefficient quoted against one standard
drag table as a coefficient against another.

## The dialog

Four fields, and **no Convert button** — the answer follows the inputs as you type them, because the
interesting output is the *relationship*, not a one-shot calculation.

| Field | What it is |
|---|---|
| **Source BC** | The coefficient you have, with the table it is quoted against |
| **Destination Table** | The table you want it against |
| **Reference Velocity** | The velocity at which the two curves are matched |
| **Target BC** | The result, read-only |

**Set Atmosphere** is there because the reference velocity has to be turned into a Mach number, and Mach
depends on temperature. Standard air is assumed if you leave it alone; the difference only matters if you
are working at the edge of the transonic region.

Underneath, the dialog states the answer as a sentence including the reference — `0.243 G7 at 2,600 ft/s`,
say — rather than presenting the number bare. That is deliberate, and the next section is why.

## Why there is a reference velocity at all

The same projectile has the same drag whatever curve you describe it with. What differs is the **shape** of
the reference curves: at one Mach number the ratio between the G1 and G7 curves is one value, at another it
is a different value. The conversion is exactly that ratio:

> BC_target = BC_source × Cd_target(M) ÷ Cd_source(M)

So a converted number is **exact only at the velocity it was computed for**. There is no such thing as "the
G7 equivalent" of a G1 BC — only its G7 equivalent *at 2,600 ft/s*, which is a slightly different number
from its equivalent at 1,800.

How much does it matter in practice? Between roughly **Mach 1.8 and 2.5** the conversion lands within about
**1 %** of manufacturer-published G1/G7 pairs. Near **Mach 1.3** it comes out around **9 % low**. The
dialog warns you when the reference falls below **Mach 1.5**, where the curves diverge in shape and the
number stops deserving three decimal places.

## Choosing a reference velocity

Pick the velocity where you care about being right:

- **The velocity band you actually shoot in.** If your shots land between 700 and 500 yd, use the velocity
  the bullet has around there, not the muzzle velocity.
- **Not the muzzle velocity by default.** It is the highest speed in the flight and the shortest part of it;
  matching the curves there makes the conversion worst where the bullet spends most of its time.
- **Not a transonic velocity.** If the reference has to be transonic for your purpose, the honest conclusion
  is that a converted single number is the wrong tool — see below.

## When not to use this at all

Converting is a **compromise, made necessary by a missing number**. Three cases where something better is
available:

1. **The maker publishes both.** Use the published G7 rather than converting the published G1. Measured
   pairs beat computed ones.
2. **You have BCs at several velocities.** That is a curve, not a number — feed it to
   [From BC Curve](approximating-a-drag-table.md#from-a-bc-curve) and get a `.drg`, which is strictly more
   information than any single converted coefficient.
3. **A measured `.drg` exists for your bullet.** Then the whole question dissolves: there is no coefficient
   to convert because there is no reference shape in play. See
   [custom drag tables](custom-drag-tables.md).

Where converting genuinely earns its place is the ordinary case: one published G1 number, a long shot, and
a desire for the drag bookkeeping to extrapolate less badly than G1 does. Convert at a sensible reference,
note what that reference was, and treat the result as a good approximation rather than a datum.

## Next

[Hit probability](hit-probability.md) — the Monte-Carlo tool, and the error budget it needs from you.

---

[← Contents](index.md)
