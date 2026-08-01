---
title: The ballistic coefficient
nav_exclude: true
math: true
---

# The ballistic coefficient — what the number actually does

**Goal of this article:** show exactly where the ballistic coefficient enters the calculation, so that
"BC 0.223 G7" stops being a magic number and becomes a term in an equation you can read.

This is a method article. It documents the mathematics the engine runs, not how to fill in the dialog —
for that, see [Choosing a drag model](../choosing-a-drag-model.md).

Mathematical symbols are set as inline math; code identifiers, literal values and units stay in `code` style.

## Start with the drag force, not the coefficient

A projectile in air feels a drag force opposing its motion through that air:

$$F_d = \tfrac{1}{2}\,\rho\,v^2\,C_d(M)\,A$$

- $\rho$ — air density
- $v$ — speed **relative to the air**, not relative to the ground
- $C_d(M)$ — the drag coefficient, which is a function of Mach number $M$, not a constant
- $A$ — the projectile's frontal area, $\pi d^2 / 4$ for a bullet of diameter $d$

What moves the bullet is acceleration, so divide by mass:

$$a_d = \frac{F_d}{m} = \frac{\pi}{8}\,\rho\,v^2\,C_d(M)\,\frac{d^2}{m}$$

Everything about *this particular bullet* has collapsed into two groups: its shape, which lives in
$C_d(M)$, and the ratio $d^2/m$, which is pure geometry and mass. The ballistic coefficient is the
device that packages both.

## Sectional density and the form factor

**Sectional density** is mass over frontal area — strictly, mass over diameter squared:

$$\mathrm{SD} = \frac{m}{d^2}$$

In the engine's units that is grains converted to pounds over square inches, which is how it is
computed from what you type:

$$\mathrm{SD} = \frac{w_{\text{gr}}}{7000\,d_{\text{in}}^{2}} \quad \left[\frac{\mathrm{lb}}{\mathrm{in}^{2}}\right]$$

**Form factor** $i$ is the shape term. It compares this bullet's drag coefficient to that of a
*reference* projectile — a standard shape whose $C_d(M)$ curve has been measured once and tabulated:

$$C_{d,\text{bullet}}(M) = i \cdot C_{d,\text{table}}(M)$$

A sleeker bullet than the reference has $i < 1$; a blunter one has $i > 1$. The assumption buried in
that single line is the important one: it says the bullet's drag curve has the **same shape** as the
reference curve, differing only by a constant multiplier. That assumption is what fails at some
velocities, and it is the reason for everything in the second half of this article.

The ballistic coefficient is the two combined:

$$\mathrm{BC} = \frac{\mathrm{SD}}{i} = \frac{m}{d^{2}\,i}$$

Substituting $d^2/m = 1/\mathrm{SD}$ and $C_{d,\text{bullet}} = i\,C_{d,\text{table}}$ back into the acceleration:

$$a_d = \frac{\pi}{8}\,\rho\,v^2\,\frac{C_{d,\text{table}}(M)}{\mathrm{BC}}$$

That is the whole point of a BC. **One number divides the drag.** Double the BC and the bullet
decelerates half as fast; that is not a rule of thumb, it is the equation.

Its units are mass over area — `lb/in²` here — which is why a BC is not dimensionless and why
comparing one to a drag coefficient is a category error. The G1 reference projectile is one inch in
diameter and one pound in mass, so its own BC is exactly $1.0\ \mathrm{lb/in^2}$; every published BC is
effectively "this bullet's retardation, as a fraction of that standard bullet's".

## What the engine actually evaluates

In code the constants are folded together. $\mathrm{PIR}$ is the fixed part of the coefficient:

$$\mathrm{PIR} = \frac{\pi}{8}\cdot\frac{\rho_0}{144} = 2.08551\times10^{-4}$$

with $\rho_0 = 0.076474\ \mathrm{lb/ft^3}$, the standard sea-level air density, and the $144$ converting the BC's
square inches into square feet so the result comes out in `ft/s²`. Air density enters as a
dimensionless ratio to that standard — the **density factor** $\rho/\rho_0$, which the
[Weather tab](../weather-tab.md) inputs produce. The acceleration is then

$$\mathbf{a}_d = -\,\mathrm{PIR}\cdot\frac{\rho}{\rho_0}\cdot\frac{C_d(M)}{\mathrm{BC}}\cdot
\lvert\mathbf{v}_a\rvert\;\mathbf{v}_a$$

written with the velocity vector rather than the speed, because drag acts *along* the air-relative
velocity $\mathbf{v}_a = \mathbf{v} - \mathbf{w}$: it is $\lvert\mathbf{v}_a\rvert\,\mathbf{v}_a$ that carries both the $v^2$ magnitude and the direction.
This is the drag term of the equations of motion in [the 3DOF model](3dof-model.md).

Two details of the lookup are worth knowing:

- **The Mach number is local.** $M = \lvert\mathbf{v}_a\rvert/c$, where $c$ is the speed of sound at the bullet's
  *current* altitude and temperature — not at the muzzle. A bullet that has dropped far enough for the
  atmosphere to have shifted is read off the drag curve at a different Mach for the same speed.
- **The table is interpolated, not stepped.** Each table row becomes a node carrying a quadratic
  fitted through it and its two neighbours, evaluated as $C + M(B + A M)$. The first and last segments
  degrade to linear and flat respectively. So $C_d$ is a smooth curve through the tabulated points,
  and the transonic drag rise is followed rather than approximated by a staircase.

## The two ways to give the engine a BC

The `BallisticCoefficient` value carries a type, and the two are not interchangeable.

| Type | What you supply | What the engine computes |
|---|---|---|
| **Coefficient** | the published BC, e.g. `0.223 G7` | used as-is: $\mathrm{BC} = 0.223$ |
| **Form factor** | the shape factor $i$, e.g. `1.05 G7` | $\mathrm{BC} = \mathrm{SD}/i$, from the bullet's weight and diameter |

The coefficient path needs nothing but the number. The form-factor path **requires** the bullet weight
and diameter, because without them there is no sectional density to divide, and the engine refuses the
calculation rather than guessing.

Either way the number that reaches the drag law is a BC in `lb/in²`. The form factor is simply the
honest way in when what you know is the bullet's shape relative to a standard, rather than a
manufacturer's quoted figure.

## Why the same bullet has two different BCs

A 168 gr .308 match bullet is quoted at roughly `0.450 G1` and `0.223 G7`. Both are correct. Neither
describes the bullet on its own: each describes the bullet **relative to a different reference shape**.

From $\mathrm{BC} = \mathrm{SD}/i$, sectional density is a property of the bullet and cancels:

$$\frac{\mathrm{BC}_{G1}}{\mathrm{BC}_{G7}} = \frac{i_{G7}}{i_{G1}}$$

The ratio near 2.0 says nothing about the bullet and everything about the two reference projectiles:
the G7 standard is a long boat-tailed shape that a modern match bullet resembles closely, so $i_{G7}$
lands near 1; the G1 standard is a blunt flat-based shape that it does not resemble at all, so $i_{G1}$
is about half. Quoting a BC without its table is meaningless, which is why the value and the
`DragTableId` are one inseparable struct in the engine.

## Where a single BC breaks down

The whole construction rests on $C_{d,\text{bullet}}(M) = i\,C_{d,\text{table}}(M)$ — one constant multiplier at every
Mach number. Real bullets do not oblige. A modern boat-tail against the G1 reference disagrees most in
the transonic region, so the multiplier that fits at 2700 ft/s is wrong by the time the bullet is
subsonic, and the discrepancy is systematic, not noise.

This is why:

- **G1 BCs are published in velocity bands.** The band is an admission that $i$ is not constant.
- **G7 works better for pointed rifle bullets.** Not because it is a better *table*, but because the
  reference shape is closer, so the constant-multiplier assumption is asked to do less work.
- **The engine accepts a whole curve instead.** Two of them, in fact.

### A multi-BC profile

Given BC quoted at several Mach numbers, the engine stops treating BC as a scalar and builds the
projectile's own drag curve on the base table's Mach grid:

$$C_{d,\text{own}}(M) = \frac{C_{d,\text{base}}(M)}{\mathrm{BC}(M)}\cdot\mathrm{SD}$$

$\mathrm{BC}(M)$ is interpolated linearly between the supplied knots and held flat beyond the end ones. The
resulting table is then run with a **form factor of exactly 1**, which the factory stamps into the
ammunition so the pair cannot be mismatched. Put the two together and the drag law becomes

$$a_d = \mathrm{PIR}\cdot\frac{\rho}{\rho_0}\cdot\frac{C_{d,\text{base}}(M)}{\mathrm{BC}(M)}\,v^2$$

— the original equation with a **velocity-dependent BC** substituted for the constant one. The
sectional density in the numerator is what puts the curve on the physical scale a `.drg` file uses, so
the bullet weight and diameter are as much an input as the BC knots themselves.

See [Approximating a drag table](../approximating-a-drag-table.md).

### A measured curve

A `.drg` custom table stores $C_d(M)$ for the projectile directly — measured, usually by radar, not
inferred from a reference shape. There is no form factor left to fit, so again it runs with a form
factor of 1 and the drag law reads the bullet's own curve at each Mach number.

At that point the BC has disappeared from the model as a concept. It was never a physical property of
the bullet; it was a compression of the bullet's drag curve into one number, useful exactly as long as
the reference shape was close enough. This is the sense in which better drag data beats a better
integrator, and beats a higher-order flight model too — see
[What the model includes](../what-the-model-includes.md).

See [Custom drag tables](../custom-drag-tables.md).

## Converting between tables

Because $\mathrm{BC} = \mathrm{SD}/i$ and the two $i$ values belong to different reference curves, converting a G1 BC to
a G7 BC means finding the pair that produce the same retardation — and they only do so at one
velocity, because the curves differ in shape. Any single conversion factor is therefore tied to a
reference velocity, and the engine's converter asks for one rather than pretending otherwise. See
[Converting a ballistic coefficient](../converting-a-bc.md).

## Summary

- Drag acceleration is $(\pi/8)\,\rho\,v^2\,C_d(M)/\mathrm{BC}$. The BC is a divisor of drag, nothing more.
- $\mathrm{BC} = \mathrm{SD}/i$: sectional density carries the mass and the calibre, the form factor carries the shape.
- Units are $\mathrm{lb/in^2}$; the G1 standard projectile is 1 inch and 1 lb, hence $\mathrm{BC} = 1$.
- A BC is meaningless without its drag table, because $i$ is measured against that table's reference.
- One scalar BC assumes the bullet's drag curve is a constant multiple of the reference's. When that
  assumption hurts, replace the scalar with a curve — a multi-BC profile or a measured `.drg`.

---

Next in this series: [The 3DOF point-mass model](3dof-model.md) — the equations of motion the drag
term above sits inside.
