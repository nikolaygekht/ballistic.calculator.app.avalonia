---
title: Recommended reading
nav_order: 90
---

# Recommended reading

**Goal of this article:** know where to learn the ballistics this guide deliberately does not teach, and
which source answers which question.

This application's manual documents the application. It explains a concept only where the interface
would otherwise be unusable, because a primer written here would be a different and much larger book —
and it would compete with better ones. This page is what makes that an honest position rather than a
gap: everything the guide skips is below, with one line on why each source is worth your time.

## Free, and enough to follow this guide

If precision shooting or external ballistics is new to you, these four cover the vocabulary the
interface uses:

| Read | For |
|---|---|
| [External ballistics](https://en.wikipedia.org/wiki/External_ballistics) | The whole subject in one page: drag, drop, wind deflection, spin drift, the correction terms this application computes |
| [Projectile motion](https://en.wikipedia.org/wiki/Projectile_motion) | The physics underneath, without air — useful precisely because it shows how much of the problem *is* air |
| [Ballistic coefficient](https://en.wikipedia.org/wiki/Ballistic_coefficient) | What a BC actually is — a ratio to a reference projectile, not a property of the bullet. Worth reading before choosing between G1, G7 and a measured table |
| [Reticle](https://en.wikipedia.org/wiki/Reticle) | Reticle types and subtension, behind the sight-picture view and the MOA/mil choice |

## The practical shelf

The books that repay buying, in the order most readers need them:

| Work | Why it is worth your time | Answers questions from |
|---|---|---|
| Litz, *Applied Ballistics for Long-Range Shooting* | The practical standard. BC, drag models, wind, spin drift and aerodynamic jump explained for shooters rather than engineers — the same terms this application exposes as inputs | Describing the load; atmosphere and wind; choosing a drag model |
| Litz, *Ballistic Performance of Rifle Bullets* | Measured G1 and G7 BCs and form factors for real projectiles. This is where you get trustworthy numbers to type in, instead of a manufacturer's optimistic single figure | Describing the load; choosing a drag model; converting a BC between tables |
| Litz, *Accuracy and Precision for Long Range Shooting* | Hit probability done properly — the WEZ analysis that the hit-probability tool here is a small cousin of, including why group size dominates at some ranges and the wind call at others | Hit probability |
| Vaughn, *Rifle Accuracy Facts* | Where dispersion actually comes from — barrel, ammunition, bedding, the shooter. It supplies the input the hit-probability tool asks for and cannot compute for you | Hit probability |
| McCoy, *Modern Exterior Ballistics* | The technical reference behind the engine: point-mass and 6DOF formulations, drag coefficients, and the standard drag families themselves. Heavier going, and the right place to settle an argument about the model | What the model includes; drag models in general |

Nobody needs all five. One Litz title plus the Wikipedia pages above is enough to use this application
well; McCoy is for when you want to know why it computes what it computes.

## Where this application's own numbers come from

- **The trajectory engine** is the
  [BallisticCalculator](https://github.com/gehtsoft-usa/BallisticCalculator1) library — open source, so
  the integration, the drag tables and the correction terms can be read rather than trusted. If you want
  to check how a specific number is produced, this is the place.
- **Units and conversions** come from
  [Gehtsoft.Measurements](https://github.com/gehtsoft-usa/Gehtsoft.Measurements).
- **The aerodynamic-jump and spin-drift formulations** follow Litz / Applied Ballistics, which is why the
  first book above is also the best commentary on those two columns.
- **The shipped `.drg` tables** are radar-derived curves for Lapua projectiles, published by the
  manufacturer. A measured curve for the projectile in your barrel beats any coefficient you can look
  up — that argument is the reason this application exists, and it is made at length in
  [What Ballistic Calculator 2 is](about.md).

## What no book will give you

Two inputs that decide accuracy more than any of the above, and that only your own range work produces:

- **Muzzle velocity from your barrel**, measured with a chronograph, with its standard deviation. A box
  figure is a different rifle's answer.
- **Your group size**, honestly measured, as the hit-probability tool wants it — a per-axis 1σ figure,
  not the best three shots you ever fired.

Everything else on this page is reading. These two are shooting.

---

[← Contents](index.md)
