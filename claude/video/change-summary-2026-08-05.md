# What changed on 2026-08-05 — playlist + isolated baselines

Summary of the restructure, for review. Files touched: `demo-script.md` (8 episodes),
`demo-numbers.md` (every figure recomputed), `context.md` (decisions + gotchas).

## The structure

Episode 2 ends with `File → Save As` → `bc2-demo.trajectory`. Episodes 3–6 each open that same file and
add exactly one feature, so every number is a delta against one table the viewer has already been shown.
Episode 6 saves the radar version as a **second** file; episode 7 opens that one and piles everything
else on top.

**One judgment call:** "the demo that uses everything" became a new episode 7 built on the .308, shown
through the **Mil-Dot from episode 2** — every mark in the glass moves 20–30 yards, which is a strong
close. The VCOG episode stays clean at ICAO sea level, because the diamonds only line up to 0.001 MOA in
the air Trijicon's sheet was fitted in. Adding weather and wind there would break the one comparison that
proves the calculator honest.

## What the isolated baselines changed

- **Aerodynamic jump is exactly zero without wind** — spin-drift-only drop is identical to baseline to
  the last printed digit. So episode 4 must put the crosswind back on before jump exists, and the script
  now says that out loud. A hidden dependency in the old chain became a teaching point.
- **62 clicks, not 60.** The naked baseline's 800 yd hold is −6.24 mrad.
- **Both weather figures survived**: February→July is still 27.9 in at 1000 yd, the Zero tab's own
  atmosphere still 1.2 in at a 300 yd zero. Now measured in one episode, so the script states the small
  number plainly and shows the more convincing artefact — the 300 yd row reading +0.4 in, the rifle no
  longer zeroed at its own zero distance.
- **The radar curve is 8.6 in and both approximations are optimistic**: G7 0.253 is 8.6 in high at
  1000 yd, the box's G1 0.505 is 14.0 in high. Dropped the old "arrives slower" claim at 1000 yd — the
  curves converge on 1132 fps there. It holds at 800 yd (20 fps), which is what the narration now quotes.
- **Episode 7 nearly cancels**, and that became the point rather than something to hide: everything-on
  gives −333.9 in, July-air-alone gave −333.6 in. The radar curve's extra 8 in is offset by jump plus
  Coriolis. Same number, wholly different reasons.

Also re-verified the two tables carried over rather than measured — the Coriolis azimuth sweep and the
zero-air share by zero distance. Both reproduce against the new baseline (one 0.1 in correction).

## One thing not settled

The harness reads BDC marks 1–2 yd under the values recorded from the app itself (404 vs 405, 496 vs
498). Cause not chased down. The app's labels are authoritative, so episode 2 keeps the recorded numbers,
episode 7's shift table is marked harness-only, and both narrations say "about". Open item 6 in
`context.md`, to settle on the first take.

## Runtime

~32:40 across eight episodes, up from 27:55 — the extra is the Open/Save ceremony plus episode 7.
Save/load moved off the cut list as a side effect: it is demonstrated seven times instead of appearing in
the outro montage.
