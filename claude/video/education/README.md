# Education mini-series — how the calculator works inside

Four short animated explainers built on `docs/method/`. English narration in the author's own ElevenLabs voice.
Look: **line art** (one champagne line on deep midnight, coral for the drag force), after
`D:\develop\ai\3rdparty\video.styles\mg-styles-15` style 02.

| # | Episode | Core idea | Docs |
|---|---|---|---|
| 1 | Drag | Drag = ½ρv²·Cd·A/m; Cd(M) and the transonic hump; G1 vs G7; BC is only a scale | `method/ballistic-coefficient.md`, `choosing-a-drag-model.md` |
| 2 | The 3DOF point mass | Six numbers, two forces, the time step (**semi-implicit Euler for the idea**; the app itself runs RK2 — say so) | `method/3dof-model.md`, `method/integration.md` |
| 3 | The wind | Drag relative to the air (v_air = v − w), the side line a_z, why the bullet drifts 3.35 m and not 9 m | `method/3dof-model.md` |
| 4 | What a point cannot do | Sg, spin drift, aerodynamic jump (Litz's formula on top of 3DOF, not 4DOF), Coriolis/Eötvös; what 4DOF would add | `method/empirical-corrections.md`, `what-the-model-includes.md` |

## Files

- `SCRIPTS.md` — the four scripts: what is said (`VOICE:`) and what is shown (`{ANIM:}`).
- `NUMBERS.md` — every number the scripts quote, from `numbers/` (`dotnet run -c Release`).
- `YouTube.md` — titles, descriptions with chapters, tags, the playlist and the channel text; thumbnails in
  `thumbs/epNN.jpg` (`node thumbs.mjs` in `remotion/`, compositions `Thumb01`…`Thumb04`).

## Pipeline

1. **Data** — `datagen/` (.NET 8, BallisticCalculator 1.1.13): `dotnet run -c Release -- ../remotion/public/data.json`.
   Every moving number is the library's. The drag column is the formula with the bullet's own Cd (G7 × SD / BC);
   the console line reports its gap to the engine's own deceleration (0.05 % supersonic; the subsonic gap of up
   to ~4 % is gravity's component along the falling path, which the engine includes and pure drag does not).
   It also writes the G1/G7 standard curves, the own-Cd curves implied by each BC, Lapua's radar curve and the
   three-model race (drops every 10 m) for episode 1, scenes 7–9.
   Episode 2 data goes to `remotion/public/ep02.json`: the level-bore flight to 1000 m with and without the
   4.5 m/s cross wind (library), datagen's own semi-implicit Euler at 0.1/0.05/0.02/0.005 s from the narrated
   790 m/s, and the same model at a 0.0002 s step as the "real" path for the step pictures.
2. **Voice** — ElevenLabs, the author's clone "Me. Teaching Tempo", `eleven_v4`, one clip per scene, two takes
   each, in `remotion/public/voice/ep01/takes/sNNa|b.mp3` (episode 1 flow `Lq3RqjcU9a4kjdGDh1Wq`).
   - `python voice/transcribe.py remotion/public/voice/ep01/takes SCRIPTS.md voice/ep01-words.json` — word times
     (faster-whisper, decoded through Remotion's bundled ffmpeg) and a diff against the script for every take.
   - `python voice/build_timing.py [04b ...]` — picks one take per scene (take A unless named), copies it to
     `sNN.mp3` and writes `timing.json`. Scenes key their beats to words through `cue()` in `src/ep01/timing.ts`,
     so a new take re-times the animation by itself.
3. **Video** — in `remotion/`: `npm ci`, `npm run studio`, `npm run render:ep01` (→ `out/ep01-drag.mp4`).
   Review: `node stills.mjs "Ep01-S04:20s,34s" "Ep01-S09:30s"` (one composition per scene, `Ep01-S01`…`S10`), then
   `python sheet.py Ep01-S04` for a contact sheet of the stills rendered in the last 15 minutes.

## Episode 1 — "Drag: where the velocity goes"

`Ep01`, ~5.3 min, ten scenes in `remotion/src/ep01/` (`S01Hook` … `S10Outro`), shared drawing kit in
`src/lib/draw.tsx` (bullet profiles with a fake 3D turn and shape morphs, formulas written term by term, pens,
arrows). Scene 5 reuses the pilot (`Decel`), timed so Mach 1 lands on the word "930".

## Episode 2 — "How a dot flies: the point-mass model"

`Ep02`, ~8 min, nine scenes in `remotion/src/ep02/` (`S01Hook` … `S08Flight`, `S09Outro`), timed to
`public/voice/ep02/timing.json` (`python voice/build_timing.py --ep 2 07b`); `src/ep02/kit.tsx` holds the scene
frame, the data access and the props (dot, stopwatch, magnifier, bookmark, the three component lines, wind streaks),
shared with episode 3. Timing and the episode assembly come from `src/lib/voiceTiming.tsx` (`makeTiming`,
`makeEpisode`). Formulas mark a vector with an arrow over the letter (`Tok.vec`). No corner tags (the user never
looked there): what the viewer must know to read a picture is said in a caption next to it (scene 5: "LATER IN THE
FLIGHT · AT 1000 m"; scene 6: "MUCH LONGER THAN REAL"; scene 7: "ONE STEP, EXAGGERATED"). `npm run render:ep02` →
`out/ep02-point-mass.mp4`; stills: `node stills.mjs "Ep02-S06:35s"`.
Faster after an edit: `node render-scenes.mjs 2 4 7` re-renders only scenes 4 and 7 into `out/ep02/sNN.mp4`
(and any scene not rendered yet), then joins all scenes into the same mp4 without re-encoding; `--all`
re-renders every scene. A whole episode this way takes a few minutes (16 tabs, Chrome on the GPU via ANGLE),
about half the single-composition render; one scene takes ~30–60 s.

## Episode 3 — "The wind"

`Ep03`, ~4 min, four scenes in `remotion/src/ep03/`: a new hook (`S01Hook`), then episode 2's former scenes 9–11
(`S02WindAir`, `S03WindSide`, `S04Outro`; their voice takes moved over unchanged), drawn with `src/ep02/kit.tsx`
and `ep02.json`. Timing: `public/voice/ep03/timing.json` (`python voice/build_timing.py --ep 3`).
`node render-scenes.mjs 3` → `out/ep03-wind.mp4`.

## Episode 4 — "What a dot can't do: spin, wind and the turning Earth"

`Ep04`, ~9 min, nine scenes in `remotion/src/ep04/` (`S01Hook`, `S02WhySpin`, `S03Stability`, `S04Drift`,
`S05Jump`, `S06Coriolis`, `S07EastWest`, `S08Honest`, `S09Outro`). Every effect goes physics first (no numbers),
then whose formula the program uses (Miller, Litz — fitted; Coriolis, Eötvös — from physics), then each term, then
our numbers. The precession rule is scene 2's clock face seen from behind ("a push → a quarter turn further, the
way it spins"); it stays as a corner badge in scenes 4–5. `src/ep04/stage3d.tsx` is a hand-projected 2.5D stage (camera, ground grid, the dot's path in its upright plane, the four effects bending it); scenes 1, 5 and 8 use it. Every formula carries a `Legend` (letter — meaning, unit) and, at "our shot", a line with our numbers substituted. Eötvös (scene 7) is told with the Earth's turn split in two: a carousel around the vertical (sin φ, Coriolis) and a wheel on its edge around the north–south line (cos φ): the ground in the east keeps dropping, so a shot east falls slower (g × 0.992), west faster; the carousel / "thrown outwards" story is wrong once the bullet outruns the ground (790 > 329 m/s). `src/ep04/kit.tsx`: the spinning bullet, the clock face,
`MathLine` (formulas with fractions; `Tok.sup` for powers), the S_g gauge, the globe, the effect cards; drift numbers
come from Litz's formula on `ep02.json` times, net of the 100 m windage zero (31 cm at 1000 m, as the engine).
Timing: `python voice/build_timing.py --ep 4 03b 04b 08b`. `node render-scenes.mjs 4` → `out/ep04-spin-earth.mp4`.

## Pilots

- `DecelPilot` (22 s, silent) — the forces on a flying bullet, to scale, and how drag slows it: 175 gr .308,
  G7 0.253, 2600 fps. Drag 48 g at the muzzle → 2.3 g at 1600 m; Mach 1 at 930 m.
