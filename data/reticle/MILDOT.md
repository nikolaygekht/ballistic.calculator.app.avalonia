# MILDOT — Mil-Dot

The standard mil-dot picture, built in **true milliradians**. Field of view 12 × 12 mrad, zero in the
centre.

Note that `AngularUnit.Mil` in the measurement library is the *military* mil — 1/6400 of a circle, and
about 1.9 % off a milliradian. This file is in `mrad`, and the dots are on whole milliradians.

## The pattern

- Fine cross out to 5 mrad each way, then heavy posts from 5 to 6 mrad — a duplex field stop.
- A bounding circle at 6 mrad radius.
- Dots of 0.1 mrad radius on every whole milliradian from 1 to 4, on all four arms.

## The marks

Geometric — the `<bdc>` anchors are places on a ruler, not one load's drops:

| Position | |
|---|---|
| +2, +1 mrad | above the zero |
| −1, −2, −3, −4 mrad | below the zero |

Whatever load is in the Shot Parameters dialog, the overlay labels each dot with the range at which
*that* trajectory crosses it. The dots do not encode a load.
