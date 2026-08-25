# Video demo — working context

Written 2026-08-02. Read this first to resume the video-demo work.

## What we are making

A YouTube **playlist of seven short videos** demoing Ballistic Calculator 2, because several users asked
to watch rather than read. (This was one 24-minute video with chapters until 2026-08-05 — see the
packaging row in the decisions table.) Production pipeline:

1. Claude writes the script (done) — narration plus `{ACTION}` cues.
2. Nikolay generates the voice-over with **ElevenLabs** from the narration lines.
3. Nikolay records the screen **while listening to the finished audio** (audio first, video second — the
   other order means re-cutting every take).
4. Nikolay overlays the voice, cuts, and uploads to YouTube.

## Files

| File | What it is |
|---|---|
| `claude/video/demo-script.md` | **The deliverable.** 7 episodes, `{CARD}` / `{ACTION}` / `{FILL}` / `{HOLD}` cues, narration written phonetically for TTS. Also holds the running-time table and the YouTube metadata. |
| `claude/video/demo-numbers.md` | Every number the narration quotes, verified against the app's own calculator. Use it to check the screen matches the script while recording. Section numbers **match the episode numbers**; every figure was recomputed for the isolated-baseline structure on 2026-08-05. |
| `claude/video/context.md` | This file. |
| `claude/video/change-summary-2026-08-05.md` | What the playlist + isolated-baseline restructure changed, and why. Written for Nikolay's review of that change; not needed once reviewed. |

Nothing else in the repository was touched. No code changes were made.

## Decisions locked in

| Decision | Choice | Why |
|---|---|---|
| Packaging | **Playlist of 8 episodes**, 3–5 min each, ~33 min total (2026-08-05, superseded "one video with chapters") | Nikolay's call. Chose grouped episodes over one-per-chapter (half would be under 2 min) and over 4 longer ones (no episode for a search like "spin drift") |
| Per-episode wrapper | **Full wrapper with title cards**: 3 s title card · 8 s shared cold open · ~10 s recap · body · ~8 s next-up · 5 s end card | Nikolay's call. Every episode must be watchable by someone who lands on it from search |
| **Narrative device** | **Save the baseline in ep. 2 (`bc2-demo.trajectory`); eps. 3–6 each `File → Open` it and add exactly ONE feature; ep. 7 turns everything on at once** (2026-08-05, superseded "one shot refined by `Ctrl+E`") | Nikolay's call. Every number is a delta against one table the viewer has already seen; every episode is independently re-recordable; and save/load gets demonstrated instead of being cut |
| Recording | **Per-episode, in order.** Nothing carries over between episodes except the saved file | Order still matters only because the narration refers forward ("remember those numbers") |
| The radar variant | Ep. 6 saves a **second** file, `bc2-demo-radar.trajectory`; ep. 7 opens that one | Nikolay's call. Keeps the baseline pristine and means ep. 7 starts with the drag curve already in place |
| Main load, eps. 1–7 | **.308 Win, Lapua GB550 175 gr Scenar-L** | `data/drg` already ships that exact bullet's radar curve, so ep. 6 changes the drag model without changing the bullet |
| Measurement system | **Two complete passes** — metric first (teaching pass), then the same load in imperial, which becomes the saved baseline | Nikolay's call; the international audience sees metric in the first minute |
| Zero distance | **300 yd / 300 m** (not the app's 100 default) | Makes the table interesting (+4.9 in at 100 yd), makes the mil-dot episodes work, and is the minimum that gives the zero-atmosphere feature any signal |
| Angular units | **Milliradian** throughout (`View → Angular Units → Milliradian`), 0.1 mrad clicks | Modern, international, and the VCOG episode is in mrad/MOA |
| Reticle | **Mil-Dot in ep. 2, the same Mil-Dot re-labelled in ep. 7, VCOG in ep. 8** | Ep. 2 teaches the view; ep. 7 uses it as the payoff (every mark moves 20–30 yd); ep. 8 is too loaded to also teach the view |
| Captions | **YouTube auto-captions**, no separate subtitle job | Nikolay's call, out of budget. (Noted for later: YouTube's "auto-sync" accepts a plain transcript paste with zero timing work, and the script *is* the transcript.) |

### What the isolated-baseline structure changed in the numbers

Re-deriving everything against a clean baseline instead of a cumulative chain moved several figures.
All of `demo-numbers.md` was recomputed; the ones that changed shape:

- **Aerodynamic jump is exactly zero without wind.** Spin-drift-only drop is identical to the baseline to
  the last printed digit. So ep. 4 must put the crosswind back on before it can show jump at all, and the
  script says so out loud — it turned a hidden dependency into a teaching point.
- **The clicks example is 62, not 60.** The naked baseline's 800 yd hold is −6.24 mrad, not −6.03.
- **Weather's honest split survived.** February→July is still **27.9 in** at 1000 yd and the Zero tab's own
  atmosphere is still worth **1.2 in** at a 300 yd zero. Both are now measured in one episode (5) instead
  of split across two, so the script states the small number plainly and shows the +0.4 in at the 300 yd
  row — the rifle no longer zeroed at its own zero distance — which is the more convincing artefact.
- **The radar curve is 8.6 in, and both approximations are optimistic.** G7 0.253 is 8.6 in high at
  1000 yd against the measured curve; the box's G1 0.505 is 14.0 in high. The old "arrives slower" claim
  only holds at 800 yd (20 fps) — at 1000 yd the curves converge on 1132 fps.
- **Ep. 7 nearly cancels, and that is the point.** Everything-on gives −333.9 in at 1000 yd; July air
  alone gave −333.6 in. The radar curve's extra 8 in is offset by jump plus Coriolis. Same number, wholly
  different reasons — the script makes this explicit rather than hiding a coincidence.

## The recipe, in one place

Set `View → Angular Units → Milliradian` before recording.

### Episodes 1–7: .308 Win 175 gr Scenar-L

The baseline, typed once in ep. 1 (metric) and again in ep. 2 (imperial), then **saved as
`bc2-demo.trajectory`**:

| Field | Metric (ep. 1) | Imperial (ep. 2, the saved baseline) |
|---|---|---|
| Weight | 11.3 g | 175 gr |
| Ballistic coefficient | 0.253 G7 | 0.253 G7 |
| Muzzle velocity | 790 m/s | 2600 ft/s |
| Sight height | 50 mm | 2 in |
| Clicks V / H | 0.1 mrad | 0.1 mrad |
| Zero distance | 300 m | 300 yd |
| Max range / step | 1000 m / 100 m | 1000 yd / 100 yd |
| Weather | ICAO standard, untouched | ICAO standard, untouched |

Then each episode **opens `bc2-demo.trajectory`** and adds only its own row. Never save from eps. 3–5.

| Ep. | Tab | What to add | Save |
|---|---|---|---|
| 3 | Weather, then Wind | Altitude 1000 ft · Pressure 28.9 inHg **station** · 40 °F · 60 % RH. Then 10 mph · direction 90° (from the right) · no distance limit | no |
| 4 | Ammunition + Rifle, then Wind | Diameter 0.308 in · Length 1.394 in · Rifling 1:10 in, **Right** — then add the 10 mph 90° wind, **required** before aerodynamic jump exists at all. Reopen the baseline and add Parameters → azimuth 90° (East) · Latitude 45° | no |
| 5 | Parameters, then Weather + Zero | Shot drop adjustment **6.2 mrad** (= 62 clicks up; positive means dialled up). Reopen the baseline: Weather → 1000 ft, 28.9 inHg, **100 °F**, 30 % RH, then the Zero tab's own atmosphere → 1000 ft, 28.9 inHg, **40 °F**, 60 % RH | no |
| 6 | Ammunition | Drag table `G7` → `GC`, load `data/drg/Lapua/30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg` | **`bc2-demo-radar.trajectory`** |
| 7 | all of them | Opens **`bc2-demo-radar`** and adds every row above except the clicks: July weather + February zero atmosphere + wind + diameter/length/rifling + azimuth/latitude. Then Reticle → **Mil-Dot** → **Far BDC** | no |

### Episode 8: 300 Blackout + Trijicon VCOG

New window, `Trajectory → New → Metric`. Both loads and the rifle come from
`data/reticle/VCOG16-300BLK.md`, which fitted them to Trijicon's published sheet to 0.17 / 0.18 MOA RMS.

| | Shot load (subsonic) | Zero load (supersonic) |
|---|---|---|
| Weight | 208 gr | 115 gr |
| BC | 0.648 G1 | 0.290 G1 |
| Muzzle velocity | 1010 ft/s | 2330 ft/s |
| Diameter / length | 0.308 in / 1.44 in | — |

Rifle: sight height **2.9 in** (the V-COG's tall integral mount), clicks 0.1 mrad, rifling **1:8 in
Right**. Zero **100 m** with the *supersonic* load, ICAO sea level. Parameters **200 m in 25 m steps**.
Reticle: `Load…` → `VCOG16-300BLK.reticle`, Display → **Far BDC**, **Split at 25 m**.

## Open items for tomorrow

1. **Nikolay reviews the script.** Nothing else should start before that.
2. **Length — resolved by the playlist.** The two cuts that were on the table when this was one long
   video (the metric pass, and Coriolis) are no longer needed: metric-then-imperial is now the
   episode 1 / 2 boundary, and episode 4 needs Coriolis to justify its own upload.
3. **Graphics to produce:** 8 title cards, 8 end cards, 8 thumbnails, and one shared 8-second cold-open
   clip. The cold open is voiced once and reused verbatim on all eight — do not re-generate it per
   episode.
4. **Guard the baseline file.** Keep a spare copy of `bc2-demo.trajectory` outside the recording folder.
   If a stray save from ep. 3, 4 or 5 modifies it, every later recap number is wrong and the mistake is
   invisible until the edit.
5. **Three `{FILL}` steps were written from `docs/` and the panel names, not from driving the app** —
   expect a small surprise on the first take and correct the script:
   - whether the Parameters tab's **shot drop adjustment** wants mrad or a click count;
   - the exact control that opens the Zero tab's **own atmosphere and ammunition** groups;
   - the reticle view's **Split at** field behaviour once a custom value is typed (per `docs/reticle-view.md`
     it then stops following the zero until cleared).
6. **Read the ep. 2 and ep. 7 mil-dot label numbers off the screen** on the first take. A standalone
   harness reproducing the app's BDC interpolation lands 1–2 yd under the values recorded from the app
   itself (404 vs 405, 496 vs 498, …); the ep. 7 shift table is harness-only, so the narration says
   "about". If the on-screen labels differ by more than a couple of yards, correct `demo-numbers.md` §7.
7. **Re-time the in-episode chapter markers** after the edit; the ones in the script are estimates.
8. Not yet decided: the card design itself, and whether to keep the closing silent montage in episode 8
   (compare charts · hit probability · CSV export · ammo library · drag-table builder).

## Deliberately left out of the videos

CSV export, load comparison, hit probability, the ammunition library, the BC converter, the drag-table
builders, the Reticle Editor, angular-unit switching. All of these appear only as an optional
five-seconds-each silent montage in episode 8's outro. The playlist makes a ninth episode cheap if any of
them turns out to be worth one — that is the obvious place to put it, not inside the eight.

**No longer left out:** save/load `.trajectory`, which the isolated-baseline structure demonstrates for
free — `Save As` in eps. 2 and 6, `File → Open` at the top of eps. 3 to 7.

## Rebuilding the number harness

The harness was scratchpad-only and is **not** in the repository. To recreate it: a `net8.0` console
project with `<ProjectReference>` to `Common/BallisticCalculator.Types/BallisticCalculator.Types.csproj`,
copy the repo's `NuGet.config` beside it, build `ShotData` exactly as the recipe above describes, and call
`ShotTrajectoryCalculator.Calculate` / `.CalculateFine` / `ZeroingCalculator.Compute`.

Structure it as **one options record of feature flags** (weather / July / zero-atmosphere / wind / shape /
Coriolis / clicks / drg / G1) and one `Build(opt)` method, then print a table per scenario. The isolated
structure means every episode is one flag, so that shape maps straight onto `demo-numbers.md`.

Gotchas found while writing it:

- The `.drg` path must be **absolute**, or the file must sit under `data/drg` beside the executable, for
  `CustomDragTableLoader.ResolvePath` to find it.
- For a `GC` load, set `Ammunition.CustomTableFileName` **and** a form-factor BC of 1.0
  (`BallisticCoefficientValueType.FormFactor`) **and a bullet diameter** — a form factor without a diameter
  throws `ArgumentException`.
- `ShotData.Weapon` still needs a `ZeroingParameters` of its own even when `ShotData.Zeroing` carries the
  real zero: `ZeroingCalculator.BuildInputs` reads `weapon.Zero` as the per-field fallback.
- `DropAdjustment` blows up near the muzzle (the bullet is below the sight line), so any search for "which
  range sits at N mrad" must skip everything inside the zero distance.
- Positive `ShotParameters.ShotDropAdjustment` = clicks dialled **up**. Negative doubles the drop.
- The `.drg` header carries the bullet: `11.30 g / 7.83 mm / 35.4 mm` for the GB550 — that is where the
  episode 4 diameter and length come from.
- A harness-computed BDC mark lands 1–2 yd under what the app displays. Cause not chased down; the app's
  own labels are authoritative. See open item 6.
