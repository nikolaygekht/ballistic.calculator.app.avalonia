---
title: The 3DOF point-mass model
nav_exclude: true
math: true
---

# The 3DOF point-mass model

**Goal of this article:** write down the equations the engine integrates, the frame they are written in,
and how the integrated state becomes a row of the table.

Two forces are integrated: drag and gravity. Everything else the output contains — spin drift,
aerodynamic jump, earth rotation — is added afterwards in closed form and is the subject of a separate
article, [the empirical corrections](empirical-corrections.md).

This is a method article. For the plain-language account of which effects are in and which are out, read
[What the model includes](../what-the-model-includes.md) instead.

Mathematical symbols are set as inline math; code identifiers, literal values and units stay in `code` style.

## Three degrees of freedom

The projectile is a **point with mass**. Its state is six numbers — three of position, three of
velocity — and the model advances them through time:

$$\mathbf{r} = (x, y, z), \qquad \mathbf{v} = (v_x, v_y, v_z)$$

Three degrees of freedom means the three translational ones. The bullet's **orientation** is not part of
the state: there is no yaw, no pitch, no precession, and no equation governing them. Everything the
model says about spin is therefore an add-on rather than a consequence, which is the single fact that
explains most of the model's limits.

## The frame

Axes are fixed to the ground at the muzzle:

| Axis | Direction | Sign convention |
|---|---|---|
| $x$ | horizontal, downrange, in the plane of fire | positive towards the target |
| $y$ | true vertical, along gravity | positive **up** |
| $z$ | horizontal, lateral | positive **left** |

Two consequences of that choice are worth stating explicitly, because they show up all over the output:

- **$y$ is vertical, not perpendicular to the line of sight.** Gravity is a clean $-g\hat{\mathbf{y}}$ and needs no
  rotation. The line of sight is rotated *at output time* instead, which is where inclined-fire drop
  comes from below.
- **$x$ is horizontal, not along the line of sight.** So the line-of-sight distance a row reports is
  $x/\cos\alpha$, where $\alpha$ is the shot angle.

The lateral sign — left positive — is what makes a positive windage adjustment cancel a right-hand spin
drift, mirroring the way a positive elevation adjustment cancels drop.

## Initial conditions

The muzzle sits $h$ below the sight line, $h$ being the sight height above the bore:

$$\mathbf{r}(0) = (0,\; -h,\; 0)$$

so the very first row of any trajectory shows a drop of exactly $-h$. The velocity vector is the muzzle
velocity $V_0$ pointed along the bore:

$$\mathbf{v}(0) = V_0\,(\cos\theta\cos\psi,\;\; \sin\theta,\;\; \cos\theta\sin\psi)$$

where the two launch angles accumulate everything that tilts the barrel:

$$\theta = \underbrace{\theta_{\text{zero}}}_{\text{elevation that zeroes the rifle}} + \underbrace{\theta_{\text{dialled}}}_{\text{clicks on the turret}} + \underbrace{\alpha}_{\text{shot angle}}$$

$$\psi = \psi_{\text{zero}} + \psi_{\text{dialled}}$$

The shot angle enters here, as part of the launch direction, rather than as a rotation of gravity.

**Barrel azimuth is not in this vector.** The compass bearing does not tilt the muzzle: the bullet is
always integrated along $x$, and the azimuth enters only as a scalar in the Coriolis terms. That is
deliberate — steering the velocity vector by the bearing would collapse $v_x$ (and with it the time
step, which is derived from $v_x$) for a shot fired east.

## The equations of motion

$$\frac{d\mathbf{r}}{dt} = \mathbf{v}$$

$$\frac{d\mathbf{v}}{dt} = -\,k\,\frac{\rho}{\rho_0}\,C_d(M)\,\lvert\mathbf{v}_a\rvert\,\mathbf{v}_a \;-\; g\,\hat{\mathbf{y}}$$

with

$$\mathbf{v}_a = \mathbf{v} - \mathbf{w}, \qquad M = \frac{\lvert\mathbf{v}_a\rvert}{c}, \qquad k = \frac{\mathrm{PIR}}{\mathrm{BC}}$$

Two forces, and that is all: **drag along the air-relative velocity, and constant gravity.**

- $\mathbf{w}$ — the wind vector. Drag depends on motion through the *air*, so wind enters by shifting the
  velocity the drag law sees, not as a force of its own. A tailwind reduces drag; a crosswind produces a
  lateral component of drag which is what pushes the bullet sideways.
- $C_d(M)$ — read from the drag table at the local Mach number. See
  [the ballistic coefficient](ballistic-coefficient.md) for the drag term in full, including `PIR`.
- $g$ — `9.80665 m/s²`, constant. No altitude variation.
- $c$ — the local speed of sound, so $M$ is evaluated against the air the bullet is *currently* in.

Note that the acceleration depends on **velocity alone**; position enters only through the atmosphere,
which is held fixed across a step. That is why both integration schemes in
[integration modes](integration.md) are written on the velocity.

## Velocity, and what is read off it

The speed reported per row is the magnitude of the full velocity vector,
$\lvert\mathbf{v}\rvert = \sqrt{v_x^2 + v_y^2 + v_z^2}$ — the ground-relative speed, not the air-relative one. The distinction
matters in a headwind: $\mathbf{v}_a$ is what the drag law and the Mach number use, $\mathbf{v}$ is what the table shows
and what energy is computed from.

Three published columns are functions of that magnitude only:

$$M_{\text{reported}} = \frac{\lvert\mathbf{v}\rvert}{c}, \qquad E = \tfrac{1}{2}m\lvert\mathbf{v}\rvert^{2}$$

$$\text{OGW} = w^{2}\,\lvert\mathbf{v}\rvert^{3}\cdot1.5\times10^{-12}\ \mathrm{lb}$$

with $\mathbf{w}$ in grains and the velocity in ft/s for the last one. Note the reported Mach uses $\lvert\mathbf{v}\rvert$, while
the drag lookup inside the step uses $\lvert\mathbf{v}_a\rvert$; in calm air they are identical.

Velocity decay is not modelled by any formula — it is whatever the integration produces. There is no
retardation coefficient, no $v(x)$ approximation, no Pejsa-style closed form: velocity falls out of
accumulating the drag deceleration step by step, which is why a measured $C_d(M)$ curve translates
directly into a better velocity prediction.

## Wind

Wind is a **horizontal** vector, given as a speed $W$ and a direction $\varphi$ (the
[direction convention](../wind-tab.md) is documented with the tab). It is decomposed into a range
component and a cross component,

$$W_{\text{range}} = W\cos\varphi, \qquad W_{\text{cross}} = W\sin\varphi$$

and then rotated into the shooting frame by the sight incline $\theta$ and the cant angle $\kappa$:

$$w_x = W_{\text{range}}\cos\theta$$

$$w_y = -W_{\text{range}}\sin\theta\,\cos\kappa + W_{\text{cross}}\sin\kappa$$

$$w_z = W_{\text{cross}}\cos\kappa + W_{\text{range}}\sin\theta\,\sin\kappa$$

A head- or tailwind on an inclined shot therefore acquires a vertical component, and a cant mixes the
range and cross components into each other — both fall straight out of the rotation, with no special
casing.

Wind acts on the trajectory through **one mechanism only**: it changes $\mathbf{v}_a$, and therefore the
magnitude and direction of the drag vector. A crosswind is not a sideways push applied to the bullet;
it is the bullet's drag acquiring a lateral component because the air it flies through is moving. The
familiar consequence — that wind deflection grows faster than linearly with range, and that the first
part of the flight path matters most — is a result of the integration, not an input to it.

**Zones.** Winds are ordered by their maximum range. The active zone's vector is substituted the moment
$x$ crosses that boundary, and the last zone extends to the end of the flight. The vector is piecewise
constant: no blending across a boundary.

What is *not* here is vertical wind. $w_y$ above is non-zero only as a projection of a horizontal wind
on an inclined shot; updraughts and thermals have no representation.

## The atmosphere along the flight path

Density and speed of sound are functions of altitude, and the bullet's altitude changes as it falls: its
vertical displacement is accumulated onto the launch altitude. The atmosphere is re-evaluated whenever
that altitude has moved more than **1 m** from the last evaluation, which for a long shot with a lot of
drop means several times.

Within one integration step, density and Mach are held constant. What is *not* modelled is any
horizontal variation — one set of surface conditions describes the whole flight.

## The time step

The step is chosen in **distance** and converted to time using the current horizontal velocity:

$$\Delta t = \frac{\Delta s}{v_x}$$

so the along-bore advance per step is approximately constant, and steps automatically lengthen in time
as the bullet slows. $\Delta t$ is quantized to a `TimeSpan` tick (100 ns) and floored there.

$\Delta s$ comes from the requested output step, not from the user's step directly — see
[Integration modes](integration.md) for how one becomes the other, and for the two schemes that consume
it. The sub-step that would cross the next output range is shortened so the row lands on the distance
you asked for.

## Stopping

The run ends at the first of:

| Condition | Value |
|---|---|
| requested maximum range reached | one calculation step past `MaximumDistance` |
| output row array full | `MaximumDistance / Step + 1` rows |
| velocity floor | `\|v\| < 50 ft/s`, **or** `v_x < 50 ft/s` |
| drop floor | more than `10 000 ft` below the sight line |
| non-finite velocity | raises `TrajectoryCannotBeCalculatedException` |
| a step that does not advance $x$ | raises `TrajectoryCannotBeCalculatedException` |

The $v_x$ floor is the guard for a projectile plunging near-vertically: total speed alone stays above the
floor at terminal velocity, but a bullet that is no longer going downrange has no trajectory left to
report — and since $\Delta t = \Delta s / v_x$, letting it continue would inflate the step without limit.

## Turning the state into a row

At each requested range the integrated state is converted into the reported figures. The
[empirical corrections](empirical-corrections.md) are applied at this point — they adjust the $y$ and $z$
used below, and nothing else.

**Drop.** With no shot angle, drop is the vertical ordinate $y$. With one, it is measured **perpendicular
to the line of sight**, so the ordinate is rotated into the sight frame:

$$\text{drop} = \big(-x\sin\alpha + (y + h)\cos\alpha\big) - h$$

The sight height is added in the vertical frame and subtracted in the rotated one; the leftover
$h(\cos\alpha - 1)$ term is intentional, and pins the muzzle row to exactly $-h$. Both conventions are
reported: `Drop` is this perpendicular figure, `DropFlat` the plain vertical one relative to the muzzle.

**Windage** is the lateral ordinate $z$, corrected as described in the other article.

**Adjustments.** The correction that would move the impact to the aim point, as an angle:

$$\theta_{\text{adj}} = \arctan\!\left(\frac{\text{drop}}{R}\right)$$

and the same for windage. This is a true angle, not the small-angle approximation, and it is what the
click columns are derived from.

**Reference lines.** Two straight lines are reported alongside, both linear in $x$: the line of sight
$x\tan\alpha$, and the line of departure (the bore line) $x\tan\theta - h$. The chart's two-curve mode draws the
bullet crossing them.

## Zeroing is the same model, solved backwards

The launch angle $\theta_{\text{zero}}$ is not given, it is found. The engine runs the **full** trajectory to the zero
distance and corrects the launch angles by Newton steps until the impact lands on the aim point:

$$\theta \leftarrow \theta + \arctan\!\left(\frac{\text{miss}}{D}\right)$$

The linear miss over the zero distance is, to first order, exactly the angular correction needed, so
convergence takes a handful of passes; the loop allows up to 100 and a default tolerance of **0.1 mm**.

Because each pass runs the complete trajectory, the corrections and the zeroing wind are all *inside*
the zero — which is what lets a rifle be zeroed with one cartridge, atmosphere or wind and fired with
another. Windage is solved the same way, and only when there is a horizontal effect to correct.

If the required elevation walks past the vertical, the launch no longer sends the projectile downrange
and the solve reports `ZeroRangeCantBeReachedException` rather than a bad number.

## What this buys, and what it does not

The model is exact about the two forces it includes and honest about the rest. Its accuracy is set,
overwhelmingly, by the quality of $C_d(M)$ — not by the integrator, and not by the missing degrees of
freedom. A point-mass solver running a measured drag curve tracks a 4DOF solver closely; the same solver
fed a guessed G1 coefficient does not.

For the list of what a fourth degree of freedom would add, and the full catalogue of unmodelled effects,
see [What the model includes](../what-the-model-includes.md).

---

Next in this series: [The empirical corrections](empirical-corrections.md) — the four effects added to
the integrated trajectory rather than integrated with it.
