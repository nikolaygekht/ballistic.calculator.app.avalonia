---
title: The empirical corrections
nav_exclude: true
math: true
---

# The empirical corrections — spin drift, aerodynamic jump, earth rotation

**Goal of this article:** document the four effects that are *not* integrated with the trajectory but
added to it afterwards, where each formula comes from, and what each one needs from you to appear at all.

[The 3DOF model](3dof-model.md) integrates two forces, drag and gravity. Everything else in the reported
drop and windage arrives here.

Mathematical symbols are set as inline math; code identifiers, literal values and units stay in `code` style.

## What "applied at output" means

Each of these corrections is evaluated **per output row, in closed form, from the integrated state** —
range, time of flight, velocity — and added to the reported drop or windage. None of them feeds back into
the integration.

The consequence is precise and worth being clear about: **they do not change velocity, time of flight, or
range.** A shot fired east and the same shot fired west have identical velocity and time-of-flight
columns; only the drop differs. In reality the coupling exists but is negligible at these magnitudes, and
this is the form the field references the engine is validated against use.

There is a second, deeper reason these are add-ons rather than terms in the equations of motion: three of
the four are consequences of the bullet's **orientation and spin**, and a point mass has neither. They are
empirical formulae — fitted to measurement and published — grafted onto a model that cannot derive them.
That makes them the least fundamental part of the calculation and the first place to be sceptical when a
number looks odd.

The four are mutually independent and simply sum. Two are horizontal, two are vertical:

| Correction | Moves the impact | Needs |
|---|---|---|
| Spin drift | horizontally, with the twist direction | twist + bullet diameter + bullet **length** |
| Aerodynamic jump | **vertically** | the above, plus a crosswind |
| Coriolis, horizontal | horizontally, right in the N hemisphere | latitude |
| Coriolis, vertical (Eötvös) | vertically | latitude **and** barrel azimuth |

## The gyroscopic stability coefficient

Both spin corrections are scaled by the **Miller twist-rate stability coefficient**, computed once at the
muzzle:

$$S_g = \frac{30\,w_{\text{gr}}}{t^{2}\,d_{\text{in}}^{3}\,L\,(1+L^{2})}\cdot\left(\frac{V_0}{2800}\right)^{1/3}\cdot\frac{T_F+460}{519}\cdot\frac{29.92}{P}$$

- $S_g$ — the stability coefficient, dimensionless; above 1 the bullet is gyroscopically stable
- $w_{\text{gr}}$ — bullet weight in **grains**
- $d_{\text{in}}$ — bullet diameter in **inches**
- $t$ — the twist expressed in **calibres**, $t = \text{twist}/d_{\text{in}}$: inches of barrel per turn,
  divided by the diameter
- $L$ — the bullet length in **calibres**, $L = \text{length}/d_{\text{in}}$
- $V_0$ — muzzle velocity in **ft/s**
- $T_F$ — air temperature in **Fahrenheit**, $P$ — pressure in **inches of mercury**
- $30$ — Miller's empirical constant, which carries the units of the first factor

The three factors are, in order: Miller's base formula from the bullet's geometry and mass, a velocity
correction normalised to 2800 ft/s, and an air correction normalised to standard sea-level conditions —
where $519 = 59 + 460$ is 59 °F in Rankine and 29.92 inHg is standard pressure, so both fractions are 1
in standard air.

Two things about $S_g$ in this engine:

- **It needs three inputs — twist, diameter and length.** Miss any one of them and $S_g$ cannot be
  computed, so spin drift and aerodynamic jump are both **silently absent** from the answer. Not zero
  because physics says so; simply not there. This is the most common reason two solvers disagree on
  windage at long range.
- **It is computed at the muzzle and never updated.** The corrections below use that muzzle value for the
  whole flight. The *reported* $S_g$ column is different: it is grown downrange as

$$S_g(x) = S_g\left(\frac{V_0}{\lvert\mathbf{v}\rvert}\right)^{1.25}$$

  where $S_g(x)$ is the value reported at downrange distance $x$ and $\lvert\mathbf{v}\rvert$ the velocity
  there, both from the integrated trajectory,
  because spin decays more slowly than forward velocity, so stability rises with range. That growth is
  displayed but not fed back into the drift.

$S_g$ is also never judged. A bullet that comes out marginally stabilised — $S_g$ near or below 1.4 — is
treated exactly like one that flies perfectly, because a point mass cannot be unstable. Nothing warns
you.

## Spin drift

A spin-stabilised bullet flies at a small **yaw of repose**: its nose sits slightly off the velocity
vector, to the side, and the resulting lift component pushes it laterally — right for a right-hand twist.
This is a genuine consequence of gyroscopic precession, which the 3DOF state cannot represent, so it is
supplied by Litz's approximation:

$$\Delta z_{\text{drift}} = 1.25\,(S_g + 1.2)\;t_{\text{flight}}^{1.83}\;\cdot s_{\text{twist}}\cdot\cos\alpha$$

- $\Delta z_{\text{drift}}$ — the lateral displacement, in **inches**, added to the reported windage
- $S_g$ — the muzzle stability coefficient above; $1.25$ and $1.2$ are Litz's fitted constants
- $t_{\text{flight}}$ — time of flight to that row, in **seconds**
- $s_{\text{twist}}$ — the twist sign: $-1$ for a right-hand twist, drift to the right, which is negative in
  the left-positive convention of the [frame](3dof-model.md#the-frame); $+1$ for a left-hand one
- $\alpha$ — the shot angle, so $\cos\alpha$ projects the drift for an inclined shot

Note what it depends on: **time of flight, not range.** A slower bullet drifts more at the same distance,
and the exponent 1.83 means drift grows appreciably faster than linearly — a few inches at 500 yd,
roughly a foot at 1000 yd for a typical match load.

It is folded into the reported **windage** and never listed on its own, so a windage figure in calm air is
spin drift plus Coriolis, not zero.

## Crosswind aerodynamic jump

The counter-intuitive one: **a purely horizontal crosswind moves the impact vertically.**

The mechanism is again gyroscopic. A crosswind changes the direction of the air flow over the bullet as it
leaves the muzzle; the bullet's nose responds by precessing *perpendicular* to that change — upwards for a
wind from the right with a right-hand twist. The deflection is imparted in the first moments of flight and
then persists as a constant **angle**. From Litz, *Applied Ballistics* Eq. 5.4:

$$\Delta y'\;[\mathrm{MOA}] = \left(0.01\,S_g - 0.0024\,L + 0.032\right)\,W_\perp\cdot s_{\text{twist}}$$

- $\Delta y'$ — the jump **angle**, in MOA; the same for every range
- $S_g$ and $L$ — the stability coefficient and the bullet length in calibres, as above
- $W_\perp$ — the crosswind component in **mph**, positive from the right
- $s_{\text{twist}}$ — the same twist sign as for spin drift

Only the **first wind zone** contributes to $W_\perp$, because the jump happens at the muzzle — a wind that
starts 300 yards downrange produces none of it.

Being an angle, it becomes a vertical offset **linear in range**:

$$\Delta\text{drop} = \Delta y' \cdot R$$

with $R$ the line-of-sight distance to the row, and the result added to the reported drop.

That linearity is the signature to look for: a 10 mph full-value crosswind might lift the impact a
fraction of a MOA, constant in angular terms at every distance, which is why it is easy to mistake for a
zeroing error rather than a wind effect.

## Earth rotation, term one: horizontal

The rotating frame of the earth deflects a projectile sideways. This term depends on **latitude only** and
is completely independent of which way you are facing:

$$\Delta z_{\text{Coriolis}} = -\,\Omega\sin\phi\;R\,t_{\text{flight}}, \qquad \Omega = 7.2921159\times10^{-5}\ \mathrm{rad/s}$$

- $\Delta z_{\text{Coriolis}}$ — the lateral displacement added to the reported windage
- $\Omega$ — the earth's rotation rate, in radians per second
- $\phi$ — the shooter's **latitude**, positive north
- $R$ and $t_{\text{flight}}$ — the line-of-sight distance to the row and the time of flight to it

The sign is carried by $\sin\phi$: positive latitudes (northern hemisphere) deflect the bullet **right**,
which is why the term is subtracted in the left-positive windage convention. It vanishes at the equator
and is largest at the poles. The product $R\,t$ means it grows roughly with the square of range.

## Earth rotation, term two: vertical (Eötvös)

The second earth-rotation term depends on the **compass bearing**, and acts vertically. Firing east, in
the direction of the earth's rotation, adds to the bullet's absolute velocity and effectively lightens
it; firing west does the opposite. It is expressed as a ratio of effective to true gravity, constant for
the shot:

$$\frac{g_{\text{eff}}}{g} = 1 - \frac{2\,\Omega\cos\phi\,\sin(Az)\,V_0}{g}$$

- $g_{\text{eff}}/g$ — the effective gravity as a fraction of the true value; dimensionless and constant
  for the shot
- $g$ — gravity, `9.80665 m/s²`, and $\Omega$, $\phi$ — as above
- $Az$ — the barrel **azimuth**: the compass bearing of the shot, measured clockwise from north, so due
  east is 90° and $\sin(Az) = 1$
- $V_0$ — the muzzle velocity, in metres per second to match $g$

East ($\sin Az > 0$) lifts the bullet — less drop; west lowers it; due north or south cancels it entirely.
Note it uses the **muzzle** velocity, not the current one.

Because this is a modification of *gravity*, it may only act on the part of the trajectory that gravity
produced: the fall below the no-gravity bore line. So the fall is scaled and the ordinate rebuilt:

$$y_{\text{bore}} = x\tan\theta - h, \qquad y_{\text{eff}} = y_{\text{bore}} + (y - y_{\text{bore}})\cdot\frac{g_{\text{eff}}}{g}$$

- $y_{\text{bore}}$ — the vacuum bore line: where the bullet would be with no gravity at all
- $x$, $y$, $\theta$, $h$ — as in the [3DOF frame](3dof-model.md#the-frame): downrange distance, vertical
  ordinate, launch elevation and sight height
- $y_{\text{eff}}$ — the ordinate the reported drop is then computed from

Scaling the fall rather than the whole ordinate is what keeps the launch geometry untouched — in
particular the exact $-h$ drop at the muzzle, where the fall is zero and the correction must therefore do
nothing. The integrator's own $y$ is never modified; $y_{\text{eff}}$ is a display quantity, computed per row.

**Latitude alone is a valid input.** Give latitude without a bearing and you get the horizontal term
correctly and the vertical term not at all, which is the honest answer when you do not know which way you
are facing.

## Magnitudes, and why they are ordered this way

For a typical centrefire rifle load at long range, these effects rank roughly:

1. **Spin drift** — inches, growing with $t^{1.83}$; the largest of the four, and the one most often missing
   from a comparison because of the bullet-length requirement.
2. **Aerodynamic jump** — a fraction of a MOA per 10 mph of crosswind, constant in angle.
3. **Horizontal Coriolis** — inches at 1000 yd and beyond, negligible closer.
4. **Eötvös** — the smallest, and zero on a north-south shot.

All four are smaller than a wind call error, and all four are smaller than the difference between a
guessed G1 BC and a measured drag curve. They are included because they are systematic — they do not
average out over a string of shots the way a wind misjudgement does — not because they dominate.
[The Parameters tab](../parameters-tab.md) gives measured figures for the two Coriolis terms.

## Summary

- Four corrections, applied per output row from the integrated state, never fed back into it: velocity,
  time of flight and range are unaffected by all of them.
- Three of the four exist because the bullet spins, which a point mass cannot express — so they are
  empirical formulae (Miller, Litz) rather than derived terms.
- Spin drift and aerodynamic jump both require twist, bullet diameter **and** bullet length; without all
  three they are silently absent.
- Coriolis is two independent effects: horizontal from latitude alone, vertical from latitude and bearing
  together.
- $S_g$ is computed at the muzzle, used constant, reported grown, and never judged.

---

Next in this series: [Integration modes](integration.md) — how the equations of motion are discretized.
Back to [the 3DOF model](3dof-model.md).
