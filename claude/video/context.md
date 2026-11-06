# Video demo — working context

Written 2026-08-02. Read this first to resume the video-demo work.

## What we are making

A YouTube **playlist of seven short videos** demoing Ballistic Calculator 2, because several users asked
to watch rather than read. (This was one 24-minute video with chapters until 2026-08-05 — see the
packaging row in the decisions table.) Production pipeline:

1. Claude writes the script (done) — narration plus `{ACTION}` cues.
2. Claude generates the voice-over with **ElevenLabs** (MCP in `.mcp.json`): Nikolay's own cloned voice
   **"Me. Teaching Tempo"** (`voice_id H7bAez7p9ZBsZzNlxYxB`), model **`eleven_v4`**, default settings
   (approved 2026-11-03 as "close enough"). ~105 credits per 8 s take.
3. The screen is recorded **by script, against the finished audio** (audio first, video second): OBS
   driven over WebSocket (`obsws-python`) and the app driven over UI Automation (`pywinauto`).
4. `ffmpeg` assembles title card + take + end card and lays every voice clip at its logged time.
5. Nikolay reviews and uploads to YouTube.

Steps 2–4 are automated and **proven on the ep. 1 pilot (2026-11-03)** — Nikolay's verdict: "всё заебись".
**Done so far:** `build/ep01/ep01.mp4` (4:05; **remade 2026-11-04** on `bcrec` with the sync marker, voice on every
action and the ammunition library — Load shows the shipped `data/ammo`, Save writes `Documents\Ballistic Calculator
demo\Lapua Scenar-L 11.3 g.ammox`; voice flow `uMSp5UJfabIRAM3ltr7G`, `ep01/sessions.json`) and `build/ep02/ep02.mp4`
(3:41, voice v2 — one clip per action; also writes `bc2-demo-metric.trajectory` and the baseline
`bc2-demo.trajectory` that eps. 3–6 open) and `build/ep03/ep03.mp4` (3:09, take 2; voice flow
`aJecrwAw5o5iIKvLC07y`, session ids in `ep03/sessions.json`; closes the file without saving) and
`build/ep04/ep04.mp4` (3:58; spin drift clean, then + wind for the jump only, then Coriolis on a reopened
file; voice flow `3mz5JEfq5Rl2ZtekuyZb`, `ep04/sessions.json`) and `build/ep05/ep05.mp4` (4:18; click size on the Rifle tab, V-Clicks 62
for the 800 yd hold, then July air 100 °F / 90 % and the Zero tab's February air 40 °F / 40 %; voice flow
`xsoG9WSzGWwObJ1JIdB6`, `ep05/sessions.json`) and `build/ep06/ep06.mp4` (5:59; BC explained on five cards — two of
ours, three PrecisionRifleBlog pictures with a source line —, Tools → Approximate Drag Table, a new Imperial
trajectory on the Lapua radar `.drg`, Compare against the baseline, saved as `bc2-demo-radar.trajectory`; voice
flow `bFr9yQNJGgwnXvuFvyrN`, `ep06/sessions.json`). If OBS's window sits on the recording
monitor, clicks land in OBS. Since 2026-11-04 the user keeps OBS on the main (non-recording) monitor; before
a take, still check its rectangle and minimize it if it is back on (-2560,0)-(0,1440) — WebSocket
recording works minimized.
**Ep. 7 done 2026-11-04**: `build/ep07/ep07.mp4` (5:14; twist, Coriolis, July air on a February zero, wind with
jump; then Mil-Dot Far/Near BDC, a 20 in plate at 600 yd, the VCOG 7.62, and two post-overlay cards — Reticle Editor
and the reticle-designer skill). Voice flow `AhaGBX73Vg3IPGU8PgJ0` (`ep07/sessions.json`, text `ep07/clips.json`;
n01 re-voiced once ep. 9 was planned). Raw take `E:/ngeht/Videos/2026-11-04 15-50-09.mp4`. Accepted with two known
lags (wind values ~2 s after their words at 2:12, the target pointer ~3 s late at 3:34) — fixable for free by
splitting e09/e13/q03 at their pauses if ever re-taken.
**Cards re-rendered 2026-11-04 without the episode count**, eps. 1–6 re-assembled from their kept raw takes (no
new voice, no new take); ep. 9 added to `cards/make_cards.py`, so ep. 8's end card is now a "Next" card.
See *Production pipeline* below for the exact commands and the traps already hit.

## Files

| File | What it is |
|---|---|
| `claude/video/demo-script.md` | **The deliverable.** 7 episodes, `{CARD}` / `{ACTION}` / `{FILL}` / `{HOLD}` cues, narration written phonetically for TTS. Also holds the running-time table and the YouTube metadata. |
| `claude/video/demo-numbers.md` | Every number the narration quotes, verified against the app's own calculator. Use it to check the screen matches the script while recording. Section numbers **match the episode numbers**; every figure was recomputed for the isolated-baseline structure on 2026-08-05. |
| `claude/video/context.md` | This file. |
| `claude/video/change-summary-2026-08-05.md` | What the playlist + isolated-baseline restructure changed, and why. Written for Nikolay's review of that change; not needed once reviewed. |
| `claude/video/cards/` | `make_cards.py` (title/end cards via headless Edge), `background.png`, `out/` (rendered PNGs). |
| `claude/video/build/epNN/` | Per-episode production: `audio/` (voice clips), `fetch.py`, `record.py`, `assemble.py`, `timeline.json`, the final `epNN.mp4`. `ep01/` is the template for the rest. |

Outside `claude/video/`, the 2026-11-03 session also fixed the shipped sight presets in
`data/dictionaries.xml` (`Standard Optics (High, Mil)` 0.1 mil → **0.1 mrad**; `PSO-1 Optics` 5 cm/100 m →
**0.5 ths**) and the docs that quoted them (`docs/rifle-tab.md`, `docs/library-and-presets.md`,
`data/reticle/PSO-1.md`, `data/reticle/pso-1.reticle`). No code changes.

## Decisions locked in

| Decision | Choice | Why |
|---|---|---|
| Packaging | **Growing playlist**, 3–5 min each: eps. 1–7 build one shot, eps. 8+ are special cases (8 = zero one load / shoot another, 9 = shooting at an angle: drop over line of sight vs muzzle level, the line of sight on the chart; 10 = your own drag table, from several BCs or measured velocities). Cards say "Episode N", **no "of M"** (2026-11-04; was "8 episodes", 2026-08-05). Later topics in view: Reticle Editor + reticle-designer skill, `.drg` table building, hit probability | Nikolay's call. Chose grouped episodes over one-per-chapter (half would be under 2 min) and over 4 longer ones (no episode for a search like "spin drift") |
| Per-episode wrapper | **Full wrapper with title cards**: 3 s title card · 8 s shared cold open · ~10 s recap · body · ~8 s next-up · 5 s end card | Nikolay's call. Every episode must be watchable by someone who lands on it from search |
| **Narrative device** | **Save the baseline in ep. 2 (`bc2-demo.trajectory`); eps. 3–6 each `File → Open` it and add exactly ONE feature; ep. 7 turns everything on at once** (2026-08-05, superseded "one shot refined by `Ctrl+E`") | Nikolay's call. Every number is a delta against one table the viewer has already seen; every episode is independently re-recordable; and save/load gets demonstrated instead of being cut |
| Recording | **Per-episode, in order.** Nothing carries over between episodes except the saved file | Order still matters only because the narration refers forward ("remember those numbers") |
| The radar variant | Ep. 6 saves a **second** file, `bc2-demo-radar.trajectory`; ep. 7 opens that one | Nikolay's call. Keeps the baseline pristine and means ep. 7 starts with the drag curve already in place |
| Main load, eps. 1–7 | **.308 Win, Lapua GB550 175 gr Scenar-L** | `data/drg` already ships that exact bullet's radar curve, so ep. 6 changes the drag model without changing the bullet |
| Measurement system | **Two complete passes** — metric first (teaching pass), then the same load in imperial, which becomes the saved baseline | Nikolay's call; the international audience sees metric in the first minute |
| Zero distance | **300 yd / 300 m** (not the app's 100 default) | Makes the table interesting (+4.9 in at 100 yd), makes the mil-dot episodes work, and is the minimum that gives the zero-atmosphere feature any signal |
| Angular units | **Milliradian** from ep. 2 on, 0.1 mrad clicks. **Ep. 1 is in thousandths**: with the PSO-1 preset's 0.5 ths clicks the app shows the Hold columns in `th` on its own | Modern, international, and the VCOG episode is in mrad/MOA; the PSO-1 is graduated in thousandths |
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

## Recording setup (verified 2026-11-03)

| Piece | State |
|---|---|
| Recording monitor | **Odyssey G5, 2560×1440 @ 200 %, at (−2560, 0)**, left of the primary. The app runs maximized there; Nikolay reads on the primary (3440×1440). The app restores its position from `appstate.json` beside the exe — it is set to that monitor. |
| OBS | 33.0.1, WebSocket 5.7.6 on `localhost:4455`, password in env `OBS_WS_PASSWORD`. Canvas and output **1920×1080 @ 60 fps**. Records to `E:\ngeht\Videos`. |
| OBS source | **Display Capture** of the Odyssey G5, fitted to the 1920×1080 canvas (both 16:9, an exact 0.75× downscale). **Not Window Capture**: Avalonia opens the Shot Parameters dialog and every combo-box dropdown as separate top-level windows, so a window capture would miss all of the data entry. Window Capture, camera and audio inputs are disabled, not deleted. |
| ffmpeg | `C:\exe\Video\ffmpeg\bin\` (`ffmpeg`, `ffprobe`, `ffplay`; gyan.dev full build 2026-08-03). Not on PATH — call by full path. |
| Python | 3.12, `obsws-python` 1.8.1, `pywinauto` 0.6.9 (user site). Scripts must call `SetProcessDpiAwareness(2)` or coordinates come back in logical pixels. |

**UI Automation works end to end — no XAML changes needed.** Every input has an `AutomationId` from its
`x:Name`, so a field is addressed as panel → control → part, e.g.
`ShotParametersDialog / AmmoSubPanel / WeightControl / NumericPart` (unit combo = `UnitPart`, drag table
= `BCControl / TablePart`). Tabs, buttons, check boxes and dropdown `ListItem`s are found by their text.
The dialog appears in the UIA tree **as a child of `MainWindow`**, not as a top-level window. On the Zero
tab the nested ammo/atmosphere panels reuse the same ids, so scope through `ZeroAmmoSubPanel` /
`ZeroAtmosphereSubPanel`. The trajectory table is a `DataGrid` whose cells are readable (`DataItem` →
`Text`), so a take can verify the on-screen numbers against `demo-numbers.md` automatically; the Summary
values have ids (`ZeroVValue`, `SubsonicValue`, …). Chart and reticle canvases are opaque (one `Custom`
each) — fine, they are only looked at; the reticle tab's buttons and radio buttons are addressable.

**2026-11-03, later: rifles changed** (Nikolay's call) — ep. 1 is a Tigr picking `PSO-1 Optics` from
the sight dictionary, ep. 2 an AR-style .308 picking `Standard Optics (High, Mil)` (3 in). `demo-numbers.md`
§1–§2 were re-read from the app; **§3–§7 still hold 2 in numbers and must be recomputed** before those
episodes are voiced. For the record, ep. 1 *with the old 50 mm sight* was typed in by script on 2026-11-03: **all eleven rows match `demo-numbers.md` §1 exactly**, zero
adjustment 2.93 mrad.

**Before the first real take:**
- Trajectory child window: Nikolay shrank its saved size (2026-11-03: `ChildWindowHeight` 827 → 485.5 in
  `appstate.json`), so it now fits the MDI area, reticle included — no maximizing needed.
- Taskbar: **auto-hide is on** (2026-11-03), so the frame is clean — but it pops up if the cursor touches the
  bottom edge. Recording scripts must keep the mouse off the last few pixels of the monitor.
- Some table headers are truncated (`Velocity (m/s`, `Windage (cm`) by the saved `TableColumnWidths`.
- **OK** with the Weather tab untouched raises `Confirm: "Weather not filled. Use default values?"` → **Yes**.

## Production pipeline (proven on the ep. 1 pilot, 2026-11-03)

**Sync model — audio first, video timed to it.** Each `VOICE:` block (or a piece of one — see below) is its
own ElevenLabs clip with a known duration. `record.py` walks the episode: `{FILL}` runs silent at natural
typing speed, `{HOLD n s}` waits n s, and every `voice(clip, action)` logs its start, runs the on-screen
action *within* the clip's length, then waits out the clip + 0.5 s. No sound plays during the take. The log
goes to `timeline.json`; `assemble.py` places every clip at its logged offset. **No manual alignment.**
Re-voicing one line = regenerate that clip, re-run the take (≈4 min, unattended) and the assembly (≈30 s).

### The steps, per episode (copy `build/ep01/` to `build/epNN/`)

1. **Voice.** One flow per episode (`creative_create_flow`), then one `creative_generate_speech` per clip
   with `generations_count=1`, voice `H7bAez7p9ZBsZzNlxYxB`, `eleven_v4`. All can be fired in parallel —
   ElevenLabs queues the overflow itself ("concurrency limit … retrying later") and finishes them a minute
   later; poll `creative_get_flow_run_status` with all session ids. Ep. 1: 15 clips, ≈2800 credits.
   The cold open (`v00`) is generated once and copied into every episode. Content URLs expire after ~2 h —
   re-poll the run to get fresh ones.
2. **Fetch.** Put `{clip: url}` in a JSON file, `python fetch.py urls.json` → `audio/<clip>.mp3` + durations.
3. **Rehearse.** `python record.py --dry` — same take, no OBS. Watch for `! action overran <clip>` lines.
4. **Take.** `python record.py` — kills and restarts the app, starts OBS, drives the episode, stops OBS,
   writes `timeline.json` with the recording path and `video0`.
5. **Assemble.** `python assemble.py` → `epNN.mp4`. Then check: sample frames at cue starts against the
   clip text (the ep. 1 pilot was checked at 12 points), and `ffprobe` the duration (= 3 + take + 5 s).

### Traps already hit — do not repeat

- **The weather flake was Ctrl+A** (found 2026-11-04, ep. 7 rehearsal, caught by the read-back): on the Weather
  tab `type_text`'s Ctrl+A sometimes selects nothing and the digits go in front of the old value (1000 → "10000",
  28.9 → "28929.95") — the intermittent "Cannot calculate this shot" of eps. 5/7. `ep07/record.py`'s `fill`/`fc`
  select with **End, Shift+Home** instead and read each field back at once. Use that in new takes.
- **A window opened after closing another comes up cascaded** (offset ~60 px, smaller), so positions cached on
  the pre-roll window are wrong for the take's window. Ep. 7 restarts the app (`t.restart()`) after the pre-roll
  instead of closing the window; the take's first window then opens exactly where the pre-roll's did.
- **Trajectory → Open has no start folder of its own**; it opens where the last native dialog was (Load reticle
  leaves it in `data
eticle`). `open_radar()` types the demo folder if the file is not listed (pre-roll only).
- **Far and Near BDC labels both sit to the right of the Mil-Dot dots.**

- **OBS's `output_duration` is NOT the video time of "now" — it under-reports by ~1.0 s.** Eps. 1-2 v1
  were assembled with it (`video0` 0.38 s) and the voice ran a second ahead of the picture; every frame
  sheet checked with the same wrong offset looked "late" instead. Measured with a flip test (a Tk
  window turning black at a known script time): true offset 1.40 s. Fix: `Take.start()` covers the
  recording monitor with a black window and removes it at T0; `assemble.py` finds the first bright frame
  (`marker_time`) and uses that as `video0`. **Check frame sheets with the marker's `video0` only.**
  Eps. 1-2 (recorded before the marker) carry `video0: 1.40` set by hand in their timeline.json; ep. 1
  was re-assembled and checked against its menu clicks (±0.1 s).
- **Keep only the latest version.** One `epNN.mp4` per episode, one raw recording in `E:/ngeht/Videos`
  (the one `timeline.json` points to). A retake overwrites; delete the superseded take, its raw
  recording, dry-run timelines and `_f*.png` stills as soon as the new cut is checked.
- **The app blocks UIA while it recalculates** (opening the Reticle tab: 2-3 s). Lookups made inside a
  timed cue stall the cue; mouse clicks don't. Resolve elements/coordinates before the cue, and for a
  tab that recalculates click it ~2 s ahead of its voice. A lazy pywinauto spec re-searches on every
  use — resolve with `.wrapper_object()` first.
- **`glide` is clock-driven** (`timeBeginPeriod(1)` + elapsed-time easing). The old step loop ran
  ~1.4x longer than asked.
- **Say it while doing it — the voice starts when the operation starts.** This holds for every action
  (menu, dialog, typing, save), not only for fills. Ep. 2's first cut saved the file silently and then
  said "Trajectory, Save As…" over the finished window — late.
- **`fetch.py` writes into `audio/` next to the JSON you pass** (`python ../ep01/fetch.py urls.json`). It
  used to write next to the script and once overwrote five ep. 1 clips with ep. 2 ones. Lost clips are
  recoverable without credits: `creative_get_flow_run_status(flow_id, session_ids)` returns fresh links
  (the session id is the path segment after `content_generation/` in the old link). Ep. 1 flow:
  `L587i8MCsQraVZszxiRZ`; ep. 2 voice v2 flow: `TNEETTyachZL8xixM9bu`.
- **Don't edit UTF-8 scripts with Windows PowerShell `Get-Content -Raw` / `Set-Content`** — it reads them
  as cp1251 and turns `…`/`→` into mojibake. Use the Edit tool or Python.
- **Never a silent fill followed by narration about it.** The first ep. 2 cut typed the whole imperial
  load in silence (~45 s) and then said "one hundred seventy-five grains…": the user heard the numbers a
  minute after seeing them typed. Speak each value **while** it is typed: one short clip per field. An
  existing long clip can be cut at its pauses (`silencedetect=noise=-40dB:d=0.25`, cut mid-pause) — no
  new generation needed.
- **Every episode after the first starts from an empty app and opens its file** (`Trajectory → Open`),
  never from a window built in the pre-roll. The pre-roll may build and save files, then `Take.restart()`.
  Helpers: `Take.open_file(name)`, `Take.save_as(name)`. The native dialogs remember the last folder per
  exe across restarts, so one pre-roll save into the demo folder makes both dialogs open there.
- **After a recalculation `rows()` is NOT in screen order.** Ep. 3 take 1 pointed at the 0 yd row for
  "one thousand yards" (`rows()[-1]`) and at 400 yd for "six hundred" (`rows()[6]`). Find a row by its
  Range text (`row("1,000")` in `ep03/record.py`), never by index.
- **Main-menu View is clickable via the menu bar**: the `Menu` with an empty automation id (not
  `PART_SystemMenu`) → children `Trajectory`/`View`/…; `Edit Parameters` is View's first item, at
  (View.left+120, View.bottom+28). The trajectory window's close button is `PART_CloseButton`.
- **Look a whole panel up in one scan** (`panel.descendants()` → dict by automation id). Five lazy
  `child_window` specs on the Wind tab took ~2.5 s; one scan takes 0.35 s.
- **A weather entry sometimes ends in "Cannot calculate this shot / cannot reach the zero distance"**
  (2026-11-04, twice in ~6 runs, cause not found — the dialog closes on OK, so the typed values are lost).
  `ep05/record.py` therefore reads every typed field back (value **and** unit) right before OK and stops with
  the dialog open on a mismatch, and stops on the error box after OK. Keep that `check()` in any take that
  types weather.
- **`bcrec.open_file` costs ~10 s** (menu lookups + safety sleeps): 2-6 s past a "Trajectory, Open" clip.
  `ep05/record.py`'s `open_demo()` clicks Open at a fixed offset from the Trajectory menu (+6+172, +52+24)
  and takes ~5 s. Likewise cache the dialog's tab and OK positions in an unrecorded **pre-roll** (open the
  file, open the dialog, read them, Cancel, close): searching them inside a cue made tabs 1-2 s late.
- **V-Clicks / H-Clicks are NumericUpDowns** with no automation id: their boxes are the first and second
  `PART_TextBox` under `ParametersSubPanel`. The Zero tab scrolls: wheel over `ZeroSubPanel` (3 x -5) brings
  `Other atmosphere for zero` and all four of its fields into view.
- **pywinauto `send_keys` eats `(` `)`** (they group modifiers) — typing a file name like `… (175 gr) …` fails
  with "File not found". Type only the folder path (no parentheses) and click the file in the list.
- **Fields on a hidden dialog tab have no Value pattern** (`NoPatternInterfaceError`): read a field back while its
  tab is shown.
- **Activate MDI windows through the `Windows` menu** (`1 <name>`, `2 <name>`, … under `Cascade`, 48 px a line),
  not by clicking a cascaded title strip — that hit the wrong window and added it to Compare twice. An unnamed
  load gives an empty window title and no legend entry: `bc2-demo`'s load is now named `Baseline G7`.
- **Full-screen cards during a take** (ep. 6): the take shows them with a Tk window in its own thread, which
  came out shifted (cropped titles). `ep06/overlay_cards.py` lays the PNGs over the assembled video for the
  exact cue intervals instead — run it after `assemble.py`. For the next episode with cards, skip the Tk window
  and only overlay.

- **ffmpeg hung** (output stuck at 262 KB, no CPU progress) when one `-filter_complex` took the two
  looped card images, the take **and** 16 `adelay`ed mp3s at once. Fixed by **two passes**: (1) voice track
  alone — `adelay` each clip, `amix normalize=0` → `voice.wav`; (2) `concat` the cards and the take, `apad`
  the voice, `-shortest`. Always pass **`-nostdin`**. Two passes take ~30 s for a 4-minute episode.
- **OBS start latency.** The take does not begin at `start_record()`. At T0 the script reads
  `get_record_status().output_duration` (`video0`, 0.38 s on the pilot) and every clip is placed at
  `3 s (title card) + video0 + at`.
- **UIA lookups inside a timed action are slow.** Walking 11 table rows by `descendants()` during a 8.4 s
  clip overran it by 4.9 s. Take every cell/tab centre **before** the `voice()` call and only `glide()`
  inside it.
- **Duplicate menu names.** `View → Drop` (submenu) and `View → Chart → Drop` share a title; `found_index=0`
  picked the wrong one and left the menu hanging open. The chart opens on Drop anyway — the step was
  dropped. Address menu items by `x:Name` (`MenuViewChartDrop`, …) if one is ever needed.
- **Split a VOICE block when an action must land mid-sentence** (`v02a` "Trajectory, New … metric or
  imperial?" with the menu opening, then `v02b` "I choose metric…" starting on the click). Timing inside a
  clip is by fraction of its length (`at(start, d * 0.4, fn)`), which is good enough for pointing at fields.
- **`user-dictionaries.xml` beside the exe overrides the shipped presets** (matched by name, never
  updated). Delete it after changing `data/dictionaries.xml`, or the take records the old values.
- **Mouse:** `pywinauto`'s `click_input` teleports the cursor; `record.py` glides with an ease-in-out
  (`glide`) and parks the cursor at (−260, 700) between actions — never near the bottom edge, where the
  auto-hidden taskbar pops up.
- **OBS finishes the mp4 after `stop_record()` returns** — assembling right away failed with "moov atom
  not found". `bcrec.Take.finish()` now waits for `output_active == False` and a stable file size.
- **Save dialog folder.** The native "Save Trajectory" dialog (`#32770`, a child of `MainWindow` in UIA;
  file-name box `auto_id=1001`, Save button `auto_id=1`) opens in the last folder used — by default
  `bin\…\data\ammo\.223`, which looks bad on camera. Ep. 2's pre-roll saves the recap window once into
  **`E:\ngeht\Documents\Ballistic Calculator demo\`** by typing the full path; the on-camera Save As then
  opens there and only `bc2-demo` is typed. Side effect: the recap window's title reads `bc2-demo-metric`.
- **The app has no File menu** — save/open live under **Trajectory** (`Trajectory → Save As`, `Trajectory →
  Open`). Ep. 2's narration was fixed; **eps. 3–7 still say "File → Open" / "File, Open" in 10 places** —
  fix them when those episodes are produced.
- Shared helpers live in **`build/bcrec.py`** (`Take`: app restart, OBS, `voice`/`hold`, `fill_shot`,
  `new_trajectory`, `cell_xy`, …). `ep01/record.py` predates it and is self-contained; `ep02/record.py` is
  the template for eps. 3+. Reticle marks are not in UIA — compute them: field centre = canvas centre,
  1 mrad = min(canvas w, h) / 12 (Mil-Dot field is 12 mrad across); checked pixel-exact on ep. 2.
- Loudness: the pilot's voice peaks at −15 dB, mean −38 dB. Nikolay was fine with it; if needed, add
  `loudnorm=I=-14` to pass (1).

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
| Sight (preset) | **PSO-1 Optics** — 65 mm, 0.5 ths (Tigr, civilian SVD in .308) | **Standard Optics (High, Mil)** — 3 in, 0.1 mrad (AR-style .308) |
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
Reticle: `Load…` → `VCOG16-300BLK.reticle`, Display → **Far BDC**, **Split at 50 m** (`BdcSplitControl`).

**Ep. 8 done 2026-11-05**: `build/ep08/ep08.mp4` (5:05; text `build/ep08/ep8-script.md`, 28 clips, voice flow
`gei13xisNM4ETQiygtkE`, `ep08/sessions.json`). Raw take `E:/ngeht/Videos/2026-11-05 11-05-46.mp4`. Supersonic
window first (115 gr, Far BDC 200–600), then the subsonic window with Other ammunition for zero (−42 / −188 cm,
Split at 50 → 55/102/151), back to window 1. Accepted with silent gaps where the action outruns the voice (b01
6.5 s, b03 4.9 s). Found while recording: **D-003** (`claude/Archive/DEFECTS.md`) — the reticle Display radios
are one group across all trajectory windows; r05 re-clicks Far BDC in window 1 as the workaround. Traps hit: a
diagonal glide from `New` to `Metric` closes the submenu (go right along New's row, then down); OBS had a
webcam source on top of Display Capture — check the scene's enabled sources before a take.
Ep. 8's section in `demo-script.md`, description and chapters were written 2026-11-05. The Near BDC overlap defect (`demo-numbers.md` §8) is still not filed.

**Ep. 9 done 2026-11-05**: `build/ep09/ep09.mp4` (3:42; text `build/ep09/ep9-script.md`, 20 clips, voice flow
`Mh7WiwJeD3ZlQp4n8ZXE`, `ep09/sessions.json`). Raw take `E:/ngeht/Videos/2026-11-05 12-29-33.mp4`. bc2-demo,
flat hold 10.0 mrad → chart Over Muzzle Level (rectangle zoom 0–360 yd: the classic picture) → 30° up (wheel
zoom at 1000 yd: the gap) → table 8.2 → −30° 8.4 → rifleman's rule card (`card-rifleman.png`, `cards.py`, put on
by `overlay_cards.py` after `assemble.py`). Numbers: `demo-numbers.md` §9. Nikolay's calls: two kinds of drop
only on the chart; the program is "a full model of the flight", never "exact"; no closing line, a next-up for
ep. 10 ("Your Own Drag Table": building a .drg from several BCs or from measured velocities; cards rendered).
Recording traps found (all handled in `build/ep09/record.py`):
- **A diagonal glide out of the menu bar crosses the next header** and Avalonia switches to that menu (View →
  Edit Parameters opened Tools → Edit Sights). Out of the bar go straight down; into a submenu go along the
  parent's row.
- **Closed menus stay in the UIA tree**: look items up by automation id and only if on screen.
- **ScottPlot: only the first middle-drag zoom rectangle per app works**; a second one comes out with the first
  one's X range (an empty plot). Use the mouse wheel (keeps the point under the cursor) for later zooms;
  a middle click autoscales.
- **A selected numeric field rejects "-"**: End, Shift+Home, **Del**, then type.
- **The Open dialog's ListItem lookup once hung 10 s** in a take (fine in the rehearsal): cache the file and
  Open button positions in the pre-roll and click those.
- The assembled video's time = cue time + 3 s title + `video0` (≈1.9 s), when picking frames to check.
- Under `*>` redirection Python block-buffers stdout and `os._exit` loses the STOPPED line: line-buffer it.
Still to do for ep. 9: nothing but review; description + chapters are in `demo-script.md` (with ep. 8's).
**Ep. 10 done 2026-11-05** (recorded while Nikolay was out, with his go for the whole run): `build/ep10/ep10.mp4`
(5:36; text `build/ep10/ep10-script.md`, 27 clips, voice flow `SSKSKh8dClEvOH0rzlEG`, `ep10/sessions.json`). Raw
take `E:/ngeht/Videos/2026-11-05 17-46-36.mp4`. Part 1: From BC Curve with five G7 BCs read off the Lapua radar
curve (`card-bc-by-speed.png`), on bc2-demo 10.28 mrad vs radar 10.30 vs one BC 10.01; Compare shows the two curves
on top of each other. Part 2: From Measured Velocities, Warner .338 Flatline 285 gr (`card-warner-sheet.png`, our own
rendering of the sheet's velocity table with a source line; `card-csv.png` shows the CSV), Load Csv
(`E:\ngeht\Documents\Ballistic Calculator demo\338-flatline-velocities.csv`), Set Atmosphere 6000 ft / 23.98 inHg /
78 °F / 25 % (fitted, see `demo-numbers.md` §10), checked in the prepared `338-flatline.trajectory` (G7 0.484):
10.36 → 10.40 at 1500 yd. Ends with its own last-episode line (x02) and the final end card. Nikolay's calls: no
"measure it yourself" (home radars give muzzle velocity, ~25–100 yd at best); no wrong-air comparison; the CSV file
shown on screen; the CSV format written into the user guide (`docs/approximating-a-drag-table.md`). Changed
after his OK of the text, to match the app: exact holds 10.36/10.40, b09 renames the load ("Five G7 BCs") so the
Compare legend reads right, and x02 (the series ending).
Recording traps found (handled in `build/ep10/record.py`):
- **The drag-table dialogs take their units from `LastMeasurementSystem` in `appstate.json`** (0 = metric), not
  from the open window: set it to 1 before the take, or they open in g / mm / m/s.
- **The BC field of From BC Curve defaults to G1**, separately from Base table (G7): switch it once, or the knots
  are G1 numbers converted to G7.
- **Browse in Edit Parameters keeps the load's name**: rename it, or Compare and the Windows menu say "Baseline G7".
  Windows menu items are "N <ammunition name>".
- **Fast typing into a file dialog dropped a letter** ("Docments" → "Path does not exist"): paste the path via the
  clipboard (`win32clipboard`) into the focused file-name box. The From BC/velocities Save dialogs always open in
  `data/drg`, so the full path is needed.
- **A dialog's button centre measured right after it appears can be stale** (Atmosphere OK missed): wait ~1.5 s and
  re-read before caching.
- **The 4th cascaded child window cut off the 1500 yd row**: `ChildWindowHeight` 560 in `appstate.json`; the
  pre-roll stops if the row is not fully visible.
- One rehearsal hit "no active desktop" (SetCursorPos failed) once; the next run was fine.

## Open items for tomorrow

1. **Nikolay reviews the script.** Nothing else should start before that.
2. **Length — resolved by the playlist.** The two cuts that were on the table when this was one long
   video (the metric pass, and Coriolis) are no longer needed: metric-then-imperial is now the
   episode 1 / 2 boundary, and episode 4 needs Coriolis to justify its own upload.
3. **Cards — done 2026-11-03.** `cards/make_cards.py` renders all 16 title/end cards (1920×1080, headless
   Edge) into `cards/out/`. One shared background, `cards/background.png`: a generated (gpt-image-2.5)
   long-range scene with wind flags blowing from the right, Topaz-upscaled to 2560×1440 and cropped of a
   9 px upscale-artifact border — Nikolay picked it. Links on end cards: GitHub **Releases** and the
   **GitHub Pages** user guide (+ source on ep. 8). Still to do: thumbnails (screenshot + that background).
   Old note: **Graphics to produce:** 8 title cards, 8 end cards, 8 thumbnails, and one shared 8-second cold-open
   clip. The cold open is voiced once and reused verbatim on all eight — do not re-generate it per
   episode.
4. **Guard the baseline file.** Keep a spare copy of `bc2-demo.trajectory` outside the recording folder.
   If a stray save from ep. 3, 4 or 5 modifies it, every later recap number is wrong and the mistake is
   invisible until the edit.
5. **The three `{FILL}` steps written from docs — two resolved 2026-11-03 by driving the app**, and the
   script corrected:
   - shot drop adjustment is the Parameters tab's **V-Clicks** spinner — a **click count** (`62`), not mrad;
   - the Zero tab's own groups open with check boxes **Other ammunition for zero** / **Other atmosphere
     for zero** / **Wind at zero**;
   - still open: the reticle view's **Split at** behaviour once a custom value is typed.
   Also found: the Rifle tab's click units default to **`mil`** (military, 1/6400) — the script now says to
   switch both to `mrad`; and Coriolis needs **Calculate Coriolis Effect** ticked (added to eps. 4 and 7).
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
