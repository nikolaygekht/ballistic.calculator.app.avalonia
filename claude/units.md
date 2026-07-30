# Units per dialog — the scheme to agree before changing anything

Written **2026-07-27**, prompted by the Set Atmosphere dialog showing metric altitude and temperature
next to an imperial pressure. This describes what the app does **today**, what it **should** do, and
the handful of decisions that need a yes before the work starts.

## The three rules

1. **A dialog is either imperial or metric — never both.** No field opts out. **One carve-out, settled
   2026-07-29:** a value *loaded from a file* keeps the units it was saved in. A load saved as `0.308 in`
   or `3.4 g` is the author's own record of that load, and restating it in the window's system would throw
   that record away — the same reasoning as precision transparency. So an imperial window showing a metric
   loaded ammunition is correct, not a bug (see `07-28.md` F-3, closed as not-a-defect). The exception to
   the exception: a `.drg` header stores SI (kg / m) as an artefact of the file format, carrying no user
   intent, so those values *are* converted to the panel's units on load.
2. **Angular units are a separate axis.** MOA / mil / mrad / thousandths / in-100yd / cm-100m is the
   user's own choice (View → Angular Units) and does **not** follow the measurement system. Scope
   clicks in the sight dictionary are always mil. This is deliberate: a metric shooter with an MOA
   scope is normal.
3. **Mach and humidity carry no unit** — a bare number and a percent.

## Where the system comes from

Proposed, and the part most worth confirming:

| Dialog kind | Which dialogs | System follows |
|---|---|---|
| **Window-bound** — belongs to one open trajectory and shows its data | Shot Parameters, Hit Probability | **that trajectory window** |
| **Standalone** — describes something outside any trajectory | Approximate .drg (both), BC Converter, Set Atmosphere, Sight/Barrel dictionaries | **the stored preference** |

The stored preference is `LastMeasurementSystem` in `appstate.json`:

- written **only** when a trajectory is created — `Trajectory → New → Imperial/Metric`;
- **not** written by `View → Measurement System`, nor by opening a `.trj` file (see decision 2);
- **`Imperial` when nothing has ever been created**.

Today every one of these dialogs instead reads `_activeChild?.MeasurementSystem ?? Imperial`, so with
no window focused they silently fall back to imperial even for a metric user.

## Canonical units

One row per quantity, so the same thing never appears in two guises:

| Quantity | Imperial | Metric | Decimals (imp / met) |
|---|---|---|---|
| Range, zero distance, target distance | yard | meter | 0 / 0 |
| Altitude | foot | meter | 0 / 0 |
| Small distance — sight height, impact offset, target size, bullet diameter/length | inch | millimeter | 3 for bullet ø and length, 1 for the rest / 2 for bullet ø and length, 0 for the rest |
| Rifling step (twist) | inch | millimeter | 1 / 0 |
| Projectile velocity | ft/s | m/s | 1 / 1 |
| Target speed (moving-target lead) | mph | km/h | 1 / 1 |
| Bullet weight | grain | gram | 1 / 2 |
| Pressure | inHg | **hPa** | 2 / 1 |
| Temperature | °F | °C | 1 / 1 |
| Energy | ft·lb | joule | 0 / 0 |

## Per dialog

**Shot Parameters** (window-bound) — Ammunition: weight, muzzle velocity, bullet ø, length ·
Weather: altitude, pressure, temperature, humidity % · Wind: velocity, direction (deg), zone
distance · Rifle: sight height, twist step, clicks (mil) · Zero: zero distance, V/H offset, shot
angle (deg) · Parameters: max range, step, angle/azimuth/latitude (deg), clicks (count).

**Set Atmosphere** (standalone; opened from the .drg editors) — altitude, pressure, temperature,
humidity %. **This is the one that is currently broken.**

**Approximate .drg — From Measured Velocities** (standalone) — weight, bullet ø, length, reading
distance, reading velocity; the grid restates distance and velocity in the same units.

**Approximate .drg — From BC Curve** (standalone) — weight, bullet ø, length, entry velocity; Mach
and BC are unitless. The Air line quotes the speed of sound in the projectile-velocity unit.

**BC Converter** (standalone) — reference velocity only.

**Hit Probability** (window-bound) — target distance, vital zone size, group size (**angular**, in
the window's angular unit), range/wind error %, MV deviation %, shot count.

**Sight / Barrel dictionaries** (standalone) — sight height, default zero distance, clicks (mil),
twist step.

## What is wrong today

1. **Set Atmosphere mixes systems.** Cause found: `AtmospherePanel.Atmosphere`'s setter uses
   `SetValue`, which by design *preserves each value's own unit* rather than converting to the
   panel's system (the two-path rule in `CLAUDE.md`). A library `Atmosphere` carries its own mix, so
   the panel shows whatever the object happened to hold. Fix: restate all fields in the panel's
   system after loading one.
2. **Standalone dialogs follow the focused window**, not a stored preference, so they are imperial
   whenever nothing is focused.
3. **The same quantity uses different precision in different panels** — bullet ø is 3 decimals in
   the ammunition panel but set separately in the .drg editors. Worth unifying on the table above
   while we are here.

## Decisions (settled 2026-07-27)

1. **Window-bound vs standalone split: confirmed.** Shot Parameters is the one dialog where the
   units are strictly formalised — the values arrive with the shot and their units are already
   chosen — so it follows its own window. **Hit Probability follows its trajectory** too.
2. **`View → Measurement System` does not touch the stored preference.** The preference means
   literally *what I last created*; we run with that and see how it behaves.
   - Consequence to watch: **opening a metric `.trj` file does not update it either** — only
     `Trajectory → New → Metric` does. Open a metric file into a fresh session and the standalone
     tools still come up imperial. Deliberate for now; one line to change if it annoys.
3. **Metric pressure is hPa**, not mmHg — `PressureUnit.Hectopascal`, which the measurement library
   already provides and displays as `hPa`. **Shot Parameters changes to hPa as well**, so there is
   one metric pressure unit in the app. Standard sea level reads 1013.2 hPa.
4. Metric bullet weight stays **gram, 2 decimals**.
5. The `.drg` editors **pass their own system down** to Set Atmosphere. Same value in practice, but
   it keeps the child dialog consistent with the editor that opened it.
