---
title: Integration modes — Euler and midpoint RK2
math: true
---

# Integration modes — Euler and midpoint RK2

**Goal of this article:** explain how the continuous equations of motion become a finite number of
arithmetic steps, what the two available schemes do differently, and what that difference is measured
to be worth.

This is a method article, and the only one in the series that is about **numerical** accuracy rather than
physics. The equations being discretized are in [the 3DOF model](3dof-model.md); the corrections applied
afterwards are the same in both schemes and are covered in
[the empirical corrections](empirical-corrections.md).

Mathematical symbols are set as inline math; code identifiers, literal values and units stay in `code` style.

## Why there is a step at all

The equations of motion have no closed-form solution: drag depends on velocity through a tabulated
curve, so the trajectory has to be advanced in small increments. Every result the application shows is
the sum of thousands of such increments, and two questions follow — **how big is a step**, and **how is
the acceleration used within it**. The first sets the cost, the second sets how much error each step
leaves behind.

## How big a step

The step you set on the [Parameters tab](../parameters-tab.md) is an **output** step: it decides how
many rows the table has. The integration step is derived from it, and is always finer:

1. Halve the output step.
2. If the result still exceeds `MaximumCalculationStepSize` (default **1 m**), divide it by a power of
   ten until it does — specifically by $10^{(\text{order} - \text{maxOrder} + 1)}$, comparing decimal orders of
   magnitude.

The powers-of-ten quantization means the internal step changes in decade jumps rather than
continuously:

| Output step | Halved | Internal step (1 m cap) | Steps per 1000 yd |
|---|---|---|---|
| 25 yd (22.86 m) | 11.43 m | **11.4 cm** | ~8 000 |
| 100 m | 50 m | **50 cm** | ~1 800 |
| 2.5 m (the fine trajectory) | 1.25 m | **12.5 cm** | ~7 300 |
| 1 m | 0.5 m | **50 cm** — unchanged, already under the cap | ~1 800 |

So the output step affects accuracy only through this quantization, and coarsely. **The number of rows
you ask for is not what decides how accurately they were computed** — the cap is. Two consequences:

- The reticle and summary views use their own 2.5 m fine trajectory, which lands on a 12.5 cm internal
  step regardless of the table's setting.
- The step that would cross an output range is shortened so the row lands on the requested distance
  rather than up to one full step past it. That costs roughly one short step per row.

Time comes from distance via the current horizontal velocity, $\Delta t = \Delta s / v_x$, quantized to a 100 ns
tick. The step is constant in *distance*, so it grows in *time* as the bullet slows.

## The two schemes

Both advance the same state with the same acceleration function

$$\mathbf{a}(\mathbf{v}) = -\,k\,\frac{\rho}{\rho_0}\,C_d(M)\,\lvert\mathbf{v}_a\rvert\,\mathbf{v}_a - g\hat{\mathbf{y}}$$

— the drag scale $k$, the density factor $\rho/\rho_0$, the drag coefficient $C_d(M)$ at the air-relative
Mach number, the air-relative velocity $\mathbf{v}_a$ and gravity $g$, all exactly as defined in
[the equations of motion](3dof-model.md#the-equations-of-motion) — and differ only in where that function
is sampled inside the step. Below, $\mathbf{v}_n$ and $\mathbf{r}_n$ are the velocity and position at the
start of a step, $n+1$ the same at its end, and $\Delta t$ the step length in time. Note the acceleration
depends on velocity alone —
position enters only through the atmosphere, which is held fixed across a step — which is why both
schemes are written on the velocity.

### Semi-implicit Euler

$$\mathbf{v}_{n+1} = \mathbf{v}_n + \mathbf{a}(\mathbf{v}_n)\,\Delta t$$

$$\mathbf{r}_{n+1} = \mathbf{r}_n + \mathbf{v}_{n+1}\,\Delta t$$

One acceleration evaluation per step. It is *semi-implicit* (symplectic) rather than plain explicit
Euler because the position update uses the **already updated** velocity — cheap, and better behaved
than using the old one.

Local truncation error is $O(\Delta t^2)$, global error $O(\Delta t)$: **halve the step, halve the error.** This was
the original engine's only scheme.

### Midpoint Runge–Kutta (RK2)

$$\mathbf{a}_1 = \mathbf{a}(\mathbf{v}_n)$$

$$\mathbf{v}_m = \mathbf{v}_n + \mathbf{a}_1\,\tfrac{\Delta t}{2}$$

$$\mathbf{a}_2 = \mathbf{a}(\mathbf{v}_m)$$

$$\mathbf{v}_{n+1} = \mathbf{v}_n + \mathbf{a}_2\,\Delta t, \qquad
\mathbf{r}_{n+1} = \mathbf{r}_n + \mathbf{v}_m\,\Delta t$$

$\mathbf{a}_1$ is the acceleration at the start of the step, $\mathbf{v}_m$ the velocity half a step later
— the **midpoint**, hence the name — and $\mathbf{a}_2$ the acceleration there.

Two evaluations per step: probe half a step forward, then use the acceleration found *there* for the
whole step. Position advances on the midpoint velocity, which is the matching second-order estimate for
$\mathbf{r}' = \mathbf{v}$.

Global error is $O(\Delta t^2)$: **halve the step, quarter the error.** The extra evaluation costs about 2× per
step and buys an entire order of convergence, which is a good trade whenever the step can then be
lengthened — and it can.

## What the difference is worth, measured

The engine ships a harness that answers this rather than asserting it. Method: a **converged reference**
from a fine Euler run at a 0.114 mm internal step (800 000 steps per 1000 yd), then each candidate
compared against it at its own emitted distances by interpolation, so output-placement artefacts do not
contaminate the integrator comparison. Worst per-point deviation over the whole trajectory, in the units
you actually read.

The .308 168 gr G7 at 2700 ft/s, 100 yd zero, out to 1000 yd, 25 yd rows:

| Scheme | Cap | Internal step | Worst Δdrop | Worst Δvelocity | Worst ΔTOF | Time per run |
|---|---|---|---|---|---|---|
| Euler | 0.01 m | 0.114 mm | 0.0001 MOA | 0.00 fps | 0.00 ms | 17.1 ms |
| Euler | 0.1 m | 1.14 cm | 0.0018 MOA | 0.02 fps | 0.02 ms | 1.67 ms |
| Euler | 1 m | 11.4 cm | 0.0183 MOA | 0.18 fps | 0.25 ms | 0.19 ms |
| Euler | 10 m | 1.14 m | 0.1834 MOA | 1.76 fps | 2.54 ms | 0.03 ms |
| **RK2** | 0.01 m | 0.114 mm | < 0.0001 MOA | 0.00 fps | 0.00 ms | 33.9 ms |
| **RK2** | 0.1 m | 1.14 cm | < 0.0001 MOA | 0.00 fps | 0.00 ms | 3.41 ms |
| **RK2** | **1 m** | **11.4 cm** | **< 0.0001 MOA** | **0.00 fps** | **0.00 ms** | **0.36 ms** |
| **RK2** | 10 m | 1.14 m | < 0.0001 MOA | 0.00 fps | 0.00 ms | 0.05 ms |

Read the Euler column downwards: `0.0018 → 0.0183 → 0.1834` for each 10× coarsening. Exactly linear —
first-order convergence, confirmed rather than assumed. RK2 stays below the reporting resolution at
every step tested, including 1.14 m, where Euler is off by nearly two tenths of a MOA.

The comparison that matters is the bold row against the second: **RK2 at its 11.4 cm default is about
4.7× faster than Euler at the historical 1.14 cm default, and simultaneously two orders of magnitude
closer to the converged answer.** That is the whole reason for the change — it is not a
speed-for-accuracy trade.

The other three cases in the harness — a .338 250 gr to 1500 yd, a subsonic .22 LR to 300 yd, and the
.308 with a 10 mph full-value crosswind — behave the same. The largest Euler-at-default drop error among
them is 0.0039 MOA (the subsonic case, where velocities are low and the step in time is long); RK2 never
exceeded 0.0001 MOA anywhere, at any step.

*Times are one development machine's, .NET 8 release build, and only the ratios are meaningful.*

## What the application uses

**Always RK2, at the 1 m cap** — the defaults. The application constructs the calculator without
touching either property, so a table row is computed with the bold line above: roughly 8 000 integration
steps per 1000 yd, at a numerical error far below anything else in the answer.

The zeroing solve inherits the same configuration, and runs the trajectory once per Newton pass, so the
integrator choice affects the zero at exactly the same relative cost.

Euler is not exposed in the user interface. It is retained in the library for reproducing the historical
engine's numbers when a result needs to be checked against an older version.

## Putting the numbers in perspective

Numerical error is the smallest error in the system, and deliberately so:

| Source | Order of magnitude |
|---|---|
| Integration (RK2 at the default step) | < 0.0001 MOA |
| Zero solve tolerance | 0.1 mm at the zero distance |
| A published G1 BC, used outside its velocity band | tenths of a MOA to several MOA at distance |
| Wind call, muzzle velocity spread, unmodelled vertical wind | the honest error budget |

Which is the practical argument for not fiddling with the step. Lowering
`MaximumCalculationStepSize` below the default with RK2 buys nothing measurable — the answer is already
on the converged reference — and costs time linearly. If you deliberately select Euler for a
comparison, lower the cap to 0.1 m to get the historical accuracy back.

Two caveats for anyone driving the library directly:

- **The dangerous end of the step range is the large one, not the small one.** $\Delta t = \Delta s / v_x$ means a
  coarse cap combined with a very slow projectile produces a long step in time; the guards described in
  [the 3DOF model](3dof-model.md#stopping) exist for that case.
- **A higher-order scheme is not the next improvement.** RK4 would cost two more drag evaluations per
  step to reduce an error that is already three orders of magnitude below the drag data's. Better
  $C_d(M)$ is always the better investment — see
  [the ballistic coefficient](ballistic-coefficient.md#where-a-single-bc-breaks-down).

## Summary

- The output step is halved and then decade-quantized down to the `MaximumCalculationStepSize` cap; rows
  requested and accuracy delivered are nearly independent.
- Semi-implicit Euler: one drag evaluation, error $O(\Delta t)$.
- Midpoint RK2: two drag evaluations, error $O(\Delta t^2)$.
- Measured, RK2 at a 10× coarser step is both faster and far more accurate than the Euler default it
  replaced; the application always uses it.
- Numerical error is the least of the errors in a ballistic solution, and should be kept that way rather
  than optimized further.

---

The series: [The ballistic coefficient](ballistic-coefficient.md) ·
[The 3DOF point-mass model](3dof-model.md) · [The empirical corrections](empirical-corrections.md) ·
Integration modes
