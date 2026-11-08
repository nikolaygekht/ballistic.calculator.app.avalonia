# Education mini-series — how the calculator works inside

Three short animated explainers built on `docs/method/`. English narration in the author's own ElevenLabs voice.
Look: **line art** (one champagne line on deep midnight, coral for the drag force), after
`D:\develop\ai\3rdparty\video.styles\mg-styles-15` style 02.

| # | Episode | Core idea | Docs |
|---|---|---|---|
| 1 | Drag | Drag = ½ρv²·Cd·A/m; Cd(M) and the transonic hump; G1 vs G7; BC is only a scale | `method/ballistic-coefficient.md`, `choosing-a-drag-model.md` |
| 2 | The 3DOF point mass | Six numbers, two forces, drag relative to the air, the time step (**semi-implicit Euler only**; the app itself runs RK2 — say so) | `method/3dof-model.md`, `method/integration.md` |
| 3 | What a point cannot do | Sg, spin drift, aerodynamic jump (Litz's formula on top of 3DOF, not 4DOF), Coriolis/Eötvös; what 4DOF would add | `method/empirical-corrections.md`, `what-the-model-includes.md` |

## Files

- `SCRIPTS.md` — the three scripts: what is said (`VOICE:`) and what is shown (`{ANIM:}`).
- `NUMBERS.md` — every number the scripts quote, from `numbers/` (`dotnet run -c Release`).

## Pipeline

1. **Data** — `datagen/` (.NET 8, BallisticCalculator 1.1.13): `dotnet run -c Release -- ../remotion/public/data.json`.
   Every moving number is the library's. The drag column is the formula with the bullet's own Cd (G7 × SD / BC);
   the console line reports its gap to the engine's own deceleration (0.05 % supersonic; the subsonic gap of up
   to ~4 % is gravity's component along the falling path, which the engine includes and pure drag does not).
   It also writes the G1/G7 standard curves, the own-Cd curves implied by each BC, Lapua's radar curve and the
   three-model race (drops every 10 m) for episode 1, scenes 7–9.
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

## Pilots

- `DecelPilot` (22 s, silent) — the forces on a flying bullet, to scale, and how drag slows it: 175 gr .308,
  G7 0.253, 2600 fps. Drag 48 g at the muzzle → 2.3 g at 1600 m; Mach 1 at 930 m.
