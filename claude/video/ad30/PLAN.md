# Ballistic Calculator 2 — 30 s ad: production plan (stage A)

The brief is `../ballistic_calculator_2_ad_30s.md`. This file records what we decided, the timing and the audio
on hand. Nothing in the product, docs or app changes for the ad.

## Chosen by the user (2026-11-06)

- **Voice:** stock, Justin Case.
- **Music:** A2-pulse.
- **Cuts:** snapped to A2's beats (128.00 BPM, first beat 0.230 s), each cut before its line starts:

  | Cut | Time |
  |---|---|
  | hook → variables | 3.043 s |
  | variables → product | 6.793 s |
  | product → compare | 11.011 s |
  | compare → drag | 14.293 s |
  | drag → reticle | 17.105 s |
  | reticle → montecarlo | 19.449 s |
  | montecarlo → end | 23.199 s |
  | end card locked | 26.48 s |

  The drag and reticle lines moved to 14.45 s and 17.25 s. Current values are in `timing.json`. The scene table
  below shows the first draft.
- **Drag scene data changed.** It now shows **Lapua's Doppler-radar table** of the 175 gr Scenar-L: 37 points
  from Mach 0.5 to 3 with the transonic rise, and the G7 shape at BC 0.253 dashed for contrast. The Warner
  velocities cover only Mach 1.78–2.68, where the curve is almost a straight line, too weak for a 3-second shot.

## Decisions (2026-11-06)

| Topic | Decision |
|---|---|
| Use | **Advertising.** 16:9 goes on the channel as the first video (first in the playlist or the channel trailer). 9:16 is a YouTube Short. |
| Folder | `claude/video/ad30/`, next to the episodes, not `marketing/` in the repository root. |
| Voice | Two takes to compare, both on eleven_v4, one generation each: **own** = the user's clone "Me. Teaching Tempo"; **stock** = "Justin Case — Warm, Trustworthy, Clear" (library voice for SaaS and product explainers, "a knowledgeable friend, not a pushy announcer", the closest match to the brief's voice direction). The user picks one. |
| Text | Exactly the brief's 47 words. Written "Ballistic Calculator **Two**" for the TTS. |
| Music | Four ElevenLabs Music v2.5 candidates (30 s, instrumental). The user picks one. |
| Engine | Remotion + React/TypeScript (Node 22 is installed). Remotion's license is free for individuals and companies of up to 3 people; a larger company needs a paid license. |

## Timing (30 fps, 900 frames)

The scene cuts are fixed. Each voice line is cut out of the whole VO file at its pauses and placed at its own
start, so the speech is never sped up. Data: `timing.json`. Placement per voice: `placed.json`
(written by `mix.py`).

| # | Scene | Frames | Seconds | Line starts | Voice line | On screen |
|---|---|---|---|---|---|---|
| 1 | hook | 0–89 | 0.0–3.0 | 0.30 | A trajectory is more than a number. | MORE THAN A NUMBER |
| 2 | variables | 90–209 | 3.0–7.0 | 3.20 | Wind. Drag. Spin. Earth's rotation. | WIND · DRAG · SPIN · CORIOLIS, one word per spoken beat |
| 3 | product | 210–329 | 7.0–11.0 | 7.25 | Ballistic Calculator Two brings the variables into focus. | BALLISTIC CALCULATOR 2 |
| 4 | compare | 330–425 | 11.0–14.2 | 11.20 | Explore trajectories. Compare loads. | TRAJECTORIES → COMPARE LOADS |
| 5 | drag | 426–509 | 14.2–17.0 | 14.40 | Build drag curves from measured data. | MEASURED DRAG DATA |
| 6 | reticle | 510–590 | 17.0–19.7 | 17.20 | Visualize your reticle. | RETICLE VIEW |
| 7 | montecarlo | 591–701 | 19.7–23.4 | 19.90 | Estimate hit probability with Monte Carlo analysis. | MONTE CARLO / HIT PROBABILITY |
| 8 | end | 702–899 | 23.4–30.0 | 23.60 | Advanced external ballistics. Free and open source. | brand → FREE & OPEN SOURCE → WINDOWS · LINUX · macOS → GET IT ON GITHUB; the full card is still from **26.5 s** (3.5 s hold) |

When the last word ends:

| Voice | Last word ends | Brief's target |
|---|---|---|
| own | 27.37 s | ≤ 27.5 s |
| stock | 27.22 s | ≤ 27.5 s |

Both voices fit. Line lengths differ by at most 0.15 s between the two voices, so one scene table serves both.

### Word beats inside scene 2

These come from the pauses in the VO and are used for the callouts. Times are seconds from the line start.

| Voice | Wind | Drag | Spin | Earth's rotation |
|---|---|---|---|---|
| own | 0 | 0.85 | 1.66 | 2.60 |
| stock | 0 | 0.83 | 1.70 | 2.63 |

After the music is chosen, every cut snaps to its nearest beat, ±0.25 s. The scene 2 callouts stay on the words.

## Music candidates

`audio/music-*.mp3`. Tempo and envelope come from `beats.py`. The RMS curves are per second.

| Track | Tempo | Shape | Fit |
|---|---|---|---|
| **A1-pulse** | ~128 BPM | Flat: −21 dB rising to −17 dB, ends cleanly at 28–29 s | Steady. Little build. |
| **A2-pulse** | ~128 BPM | **Real build** from −29 to −12 dB, peak at 22–25 s, resolves at 27–29 s | Closest to the brief: peak under Monte Carlo, ending under the end card. |
| **B1-cinematic** | ~118 BPM | Even level with an accent every 4 s (two bars), ends at 28–29 s | Calm. The 4 s accents could carry the scene cuts. |
| **B2-cinematic** | ~118 BPM | Even, then **fades from 24 s** | The ending is soft: no "decisive final beat" under the end card. |

### Mixing

Done in `mix.py`.

- Each music track is brought to −27 LUFS by a static gain, so its own build is kept.
- The music ducks under the voice with a sidechain.
- The whole mix is −14 LUFS, true peak ≤ −1.5 dBFS.
- All 8 previews measure −12.8 to −14.5 LUFS, with peaks of −1.4 to −1.7 dBFS. No clipping.

Previews to listen to, `previews/<voice>+<music>.mp3`:

| Voice | A1-pulse | A2-pulse | B1-cinematic | B2-cinematic |
|---|---|---|---|---|
| own | own+A1-pulse | own+A2-pulse | own+B1-cinematic | own+B2-cinematic |
| stock | stock+A1-pulse | stock+A2-pulse | stock+B1-cinematic | stock+B2-cinematic |

### Rights

The music comes from a paid ElevenLabs plan, which carries a commercial license per the help center. Third-party
summaries say self-serve plans exclude film, TV, radio and games, but cover online ads and YouTube. Check the
current Eleven Music terms before **paid** promotion.

## Discrepancies with the brief

1. **§4 voice direction vs the user's own voice.** The brief describes a stock "neutral American" voice and
   several variations. We generated one take per voice, and the user's own voice is a candidate.
2. **§6 project folder.** `claude/video/ad30/` is used, as decided above.
3. **Brief §1 "Coriolis" vs VO "Earth's rotation".** These are kept as the brief has them: the screen says
   CORIOLIS while the voice says "Earth's rotation".

## Asset inventory

| File | Source | Credits |
|---|---|---|
| `audio/vo-own.mp3` (24.4 s) | ElevenLabs flow `g7YpXHmJoq6CfOzSZb9A`, generation RTceguHRQ1HtkFzMLSYl | 332 |
| `audio/vo-stock.mp3` (24.16 s) | same flow, generation 91VtFYL4d6Ee7hLLY21y | 332 |
| `audio/music-A1/A2-pulse.mp3` | same flow, node "Music A — electronic pulse" | 2 × 795 |
| `audio/music-B1/B2-cinematic.mp3` | same flow, node "Music B — minimal cinematic" | 2 × 795 |

The prompts are in the flow and in `urls.json` (generation ids).

## Status (2026-11-06)

Stages B–E are done as a first full pass. Both MP4s are in `remotion/out/` and were checked:

- 900 frames, 30.000 s, 30 fps, 1920×1080 and 1080×1920;
- AAC 48 kHz stereo;
- −13.6 LUFS, true peak −1.4 dBFS;
- no black frames (`blackdetect`).

Review material is in `remotion/out/`:

| What | Files |
|---|---|
| Contact sheets | `sheet-Ad16x9.jpg`, `sheet-Ad9x16.jpg` |
| Stills | `stills/` |
| Cut frames | `cuts*` |

What is left is the user's review.

## Stage B plan (as written before the work)

- A Remotion project in `ad30/remotion/`, with two compositions: `Ad16x9` (1920×1080) and `Ad9x16` (1080×1920).
- Scenes read `timing.json`. The audio is the chosen `vo-<voice>-placed.wav` plus the music.
- App-referenced visuals come from the docs screenshots and the user guide's conventions: the chart, Compare,
  Cd/Mach, the Mil-Dot reticle with its Target box, and the hit-probability scatter against the vital-zone circle.
  The shapes are stylized and labelled *illustrative* wherever they are not app output.
- Stills at frames 30, 120, 225, 345, 465, 590, 690 and 825 for both orientations, plus a contact sheet.
