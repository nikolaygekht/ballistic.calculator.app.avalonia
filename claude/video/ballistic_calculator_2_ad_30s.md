# Ballistic Calculator 2 — 30-Second Motion Ad

**Creative brief and coding-agent production prompt — draft v1**  
**Master formats:** 1920 × 1080 (16:9) and 1080 × 1920 (9:16)  
**Duration:** exactly 30 seconds (900 frames at 30 fps)  
**Language:** English  
**Voice-over and music:** ElevenLabs, delivered as separate audio assets  
**Animation:** programmatic motion graphics, preferably Remotion/React with SVG and Canvas

Source repository: https://github.com/nikolaygekht/ballistic.calculator.app.avalonia  
User guide: https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/

## 1. Creative direction

### Concept: Physics in Motion

A fast, polished engineering-software advertisement that visualizes a trajectory becoming understandable through data. Avoid a generic app screen recording: translate **actual product capabilities** into elegant, readable kinetic graphics. The viewer should understand that Ballistic Calculator 2 is a serious, free, open-source desktop tool for exploring external ballistics.

**Core message:** Understand the variables behind the trajectory.

**Look:** dark graphite / near-black background, precise thin grid lines, electric cyan as the primary data color, amber for secondary variables and emphasis, white typography. Restrained bloom and controlled motion blur. Strong contrast. Modern technical design, not military-game aesthetics or science-fiction holograms.

**Motion:** rapid but purposeful. Data points assemble into curves, curves transform into comparison charts, grid lines resolve into a reticle, and a statistical scatter plot takes shape. Transitions should feel logically related, not a random collection of effects.

**Do not depict:** live firing, people or animal silhouettes, gunshot sound effects, explosions, muzzle flashes, bullet impacts, exaggerated claims of accuracy, or fabricated application features.

## 2. Final draft English voice-over (47 words)

> A trajectory is more than a number.  
> Wind. Drag. Spin. Earth's rotation.  
> Ballistic Calculator 2 brings the variables into focus.  
> Explore trajectories. Compare loads.  
> Build drag curves from measured data.  
> Visualize your reticle.  
> Estimate hit probability with Monte Carlo analysis.  
> Advanced external ballistics. Free and open source.

Speak with deliberate micro-pauses between the named variables. The last spoken line should finish by approximately 27.5 seconds, leaving a moment for the end card and musical resolution. Do **not** compress the speech unnaturally to match the draft timestamps; adjust scene word timing to the actual generated voice track while keeping total runtime 30 seconds.

## 3. Scene-by-scene storyboard

| Time | Voice-over | On-screen content | Motion treatment |
|---|---|---|---|
| **00:00–00:03.5** | A trajectory is more than a number. | `MORE THAN A NUMBER` | A precise luminous trajectory draws across a dark technical grid; camera follows the path and reveals scale marks. Instant visual hook within first half-second. |
| **00:03.5–00:07** | Wind. Drag. Spin. Earth's rotation. | `WIND · DRAG · SPIN · CORIOLIS` | Four short, beat-synced variable callouts enter; a single trajectory visualization reacts subtly, with physically plausible vector/curve relationships rather than arbitrary waves. |
| **00:07–00:10.5** | Ballistic Calculator 2 brings the variables into focus. | `BALLISTIC CALCULATOR 2` | The visual grid resolves into a clean application-inspired trajectory chart. The product name appears early, not only in the last frame. |
| **00:10.5–00:14** | Explore trajectories. Compare loads. | `TRAJECTORIES` → `COMPARE LOADS` | Two labeled curves materialize on one chart, animated sequentially and contrasted with restrained cyan/amber coding. Use app-referenced rather than invented conventions. |
| **00:14–00:18** | Build drag curves from measured data. | `MEASURED DRAG DATA` | Measured points appear, then become a smooth Cd/Mach-style curve. A concise cue indicates this comes from data, not a generic G1/G7 label swap. |
| **00:18–00:21.5** | Visualize your reticle. | `RETICLE VIEW` | The plot grid transforms into a reticle geometry inspired by the actual application. Limited BDC marks and a simple non-human target-size box appear. |
| **00:21.5–00:25.5** | Estimate hit probability with Monte Carlo analysis. | `MONTE CARLO` / `HIT PROBABILITY` | Points accumulate into a scatter cloud around a circular abstract scoring zone. This is clearly a simulated distribution, with no false promise of a hit. |
| **00:25.5–00:30** | Advanced external ballistics. Free and open source. | `BALLISTIC CALCULATOR 2` / `FREE & OPEN SOURCE` / `WINDOWS · LINUX · macOS` / `GET IT ON GITHUB` | The scatter resolves into clean brand/title typography. Music resolves on a decisive but restrained final beat. Hold the call to action clearly for the final 2.5–3 seconds. |

**End card:** Show the exact repository URL if it remains readable, otherwise show `GET IT ON GITHUB` and prepare the exact clickable URL for the video description. A QR code is optional in 16:9, but is **not** the sole call to action for 9:16 (viewers may be watching on the phone they would need to scan). The verified URL is `https://github.com/nikolaygekht/ballistic.calculator.app.avalonia`.

## 4. ElevenLabs voice direction

**Delivery:** English, neutral American accent, adult voice with a warm mid-low register; precise, knowledgeable, confident and modern. A technical storyteller, not a movie-trailer announcer. Moderate tempo with short, intentional pauses. No exaggerated enthusiasm or breathy whispering. Close-mic studio quality, minimal reverb, clear diction for “Coriolis” (if used in an alternate take), “Monte Carlo,” and “Ballistic Calculator Two.”

Use the exact voice-over text above. Generate several variations with the same selected voice if needed; prefer the one that lands the last line around 27 seconds. Export clean WAV if available. Do not bake music into the voice file.

File contract: `public/audio/voiceover.wav`. If a different filename or codec is supplied, update the project configuration without changing scene logic.

## 5. ElevenLabs Music prompt (English)

> Create a **30-second instrumental electronic technology-ad soundtrack**. Premium, energetic, precise, and intelligent. Approximately 124–132 BPM. Deep but controlled synth bass pulse, clean electronic percussion, subtle ticking and click-like rhythmic textures, sharp transient accents, glassy high-frequency arpeggios, and a restrained atmospheric layer. Start immediately with an intriguing pulse; build momentum through the first 20 seconds; create a satisfying rhythmic peak around 21–25 seconds and a clear, confident resolved ending at 29–30 seconds. Leave ample midrange space for a spoken male voice-over. Designed for data visualizations, motion graphics, and rapid but deliberate cuts. **No vocals, no lyrics, no gunshot sounds, no militaristic drums, no huge cinematic trailer booms, no recognizable or imitative existing melody, no abrupt genre changes.** A modern software-product campaign, not a combat game.

Create a clean instrumental track. The suggested transition times are **editorial intentions**, not a claim that a text-to-music model can honor frame-accurate cue points. Synchronize animation to the actual accepted audio's beats during final edit.

File contract: `public/audio/music.wav` (MP3 is acceptable if WAV is unavailable). Keep audio rights and the generated track metadata documented for the publishing channel and the actual ElevenLabs plan.

## 6. Full coding-agent prompt for Claude Code or Codex

You are a senior motion designer and React/Remotion engineer. Create a **production-quality, exactly 30-second animated promotional video** for **Ballistic Calculator 2**, a free, open-source external-ballistics desktop application. Produce **two separately designed aspect-ratio compositions**: horizontal 1920 × 1080 and vertical 1080 × 1920. Use 30 fps, exactly 900 frames each.

### Source of truth

Inspect the repository at `https://github.com/nikolaygekht/ballistic.calculator.app.avalonia`, especially:

- `README.md`
- `docs/about.md`
- `docs/chart-view.md`
- `docs/comparing-loads.md`
- `docs/custom-drag-tables.md`
- `docs/approximating-a-drag-table.md`
- `docs/reticle-view.md`
- `docs/hit-probability.md`
- `docs/screenshots/ballistic_table.png`
- `docs/screenshots/compare_charts.png`
- `docs/screenshots/reticle.png`
- `docs/screenshots/hit_probability.png`
- `docs/screenshots/custom_drg.png`

Use those materials to understand actual features, terminology, conventions, and UI style. Use existing product assets where appropriate. Do not falsely portray a mock UI as a literal screenshot. Create a stylized **data-visualization advertisement**, not a product tutorial.

### Production scope

1. Work in an isolated promotional project, such as `marketing/ads/ballistic-calculator-2-30s/`; if a folder already exists, inspect it before editing. Do not modify the calculator's production code, project files, or docs.
2. Prefer Remotion + TypeScript/React, SVG for trajectories/reticle geometry and Canvas when useful for larger particle/scatter effects. Use a reproducible render; avoid browser-dependent random motion. Do not depend on subscription video-generation services or unreviewed stock footage.
3. Implement the eight scenes and exact voice-over text in sections 2 and 3 of this brief. Follow the durations as the initial storyboard, but make the text-to-animation timing data-driven and adjustable based on the actual audio.
4. Make the two aspect ratios true layout adaptations. Do not simply crop 16:9 into 9:16. Reposition labels and graphs, change data density and line breaks, and preserve the central visual story. Keep text readable on a phone, with large type and protected top/bottom zones for vertical platform UI.
5. Use one shared motion/design system: near-black graphite, white, electric cyan and restrained amber. Crisp geometry, purposeful movement, controlled parallax, eased transforms, quick match cuts, light glow but no gratuitous HUD clutter.
6. All numeric values and plotted shapes must be explicitly labeled `illustrative` if not generated from app-verified examples. Prefer using shape and pattern without arbitrary numerical claims. Respect the application's documentation about simulation limits. Do not assert guaranteed accuracy, commercial/4DOF equivalence, automatic live environmental measurement, Android availability, or real-world hit guarantees.
7. The software supports trajectory tables and charts, load comparison, custom drag data (including measured downrange velocities), reticle views and a Monte Carlo hit-probability tool. These are the actual features to advertise. Avoid implying that the model includes yaw/precession/6DOF motion or that it retrieves live online bullet data.
8. Keep the end card visible for at least 2.5 seconds, with `BALLISTIC CALCULATOR 2`, `FREE & OPEN SOURCE`, `WINDOWS · LINUX · macOS`, and a GitHub call to action. Provide a suitable end-card arrangement for each format. Do not use a made-up domain, logo, pricing, review badge, or app-store icon.

### Audio

- Make audio modular. Accept `public/audio/voiceover.wav` and `public/audio/music.wav`, plus optional subtle UI-like sound-design effects. Do not hardcode personal credentials or call ElevenLabs without the owner's explicit authorization.
- If files are absent, render a **silent animatic** and a preview using clearly marked missing-audio status; do not invent a synthetic voice or substitute an unrelated licensed song. Video rendering should still succeed.
- Once audio is provided, place speech intelligibly above the music, apply smooth ducking under voice as needed, and align major motion beats to the actual music waveform/transients rather than arbitrary clock ticks. No gunshots or explosion effects.
- Verify overall mix is free of clipping and has a clean fade/tail. Document the accepted voice/music sources and their licensing metadata.

### Implementation and review stages

**Stage A — plan:** Inspect the repository and write a concise production plan with a precise scene timing table, asset inventory, and any discrepancies found. Do not change product claims without pointing out the issue.

**Stage B — animatic:** Build a functional first pass. Render low-resolution previews for both orientations and representative stills around frames 30, 120, 225, 345, 465, 590, 690 and 825. Check readability, transitions, and consistent styling.

**Stage C — refine:** Improve typography, motion easing, scene-to-scene visual continuity, plot and reticle fidelity, glow/shadow restraint, and vertical layout. Export a contact sheet for each orientation so scenes can be reviewed quickly.

**Stage D — integrate audio:** If owner-provided ElevenLabs tracks exist, synchronize and mix them. If not, keep a clearly documented audio-integration step and provide silent previews now.

**Stage E — verify/export:** Run the build; render both MP4s; inspect stills at scene boundaries and the last frame; confirm exactly 30 seconds, 30 fps, correct dimensions, no clipped/cropped copy, no missing assets, no text outside safe areas, no black accidental frames, and no audio clipping. State what was verified and what remains pending.

### Deliverables

- Editable Remotion source and required assets.
- `ballistic-calculator-2-16x9.mp4` — 1920 × 1080, 30 fps, 30 sec, H.264/AAC if audio is present.
- `ballistic-calculator-2-9x16.mp4` — 1080 × 1920, 30 fps, 30 sec, H.264/AAC if audio is present.
- Two visual contact sheets and representative scene stills.
- `voiceover.txt` containing the approved 47-word script.
- `elevenlabs_music_prompt.txt` and `elevenlabs_voice_direction.txt`.
- A short `README.md` with install, preview, rendering, and replacing-audio commands, plus known limitations.

### Definition of done

An attractive and technically credible advertisement, immediately understandable without sound, legible in both 16:9 and 9:16, editable without rebuilding from scratch, with clear separation between app-related visuals and explanatory illustration. All product claims are traceable to the repository docs. The exported versions run exactly 30 seconds.

Before final delivery, show previews and clearly surface any missing audio or unverified product detail. Do not commit, push, or publish anything unless the owner requests it.

## 7. Product and publishing accuracy checklist

- The application is **desktop** software for Windows, Linux and macOS; Android is planned but not available.
- Free and open source (GPL v2); avoid the phrase “no install required” because the .NET 8 runtime is required.
- Use **3DOF plus documented corrections**; do not call the application a 4DOF or 6DOF solver.
- Show statistical hit-probability estimates as **simulation**, not proof or guarantee.
- The user guide says numerical output is a model with limitations. Avoid real-world safety or reliability promises.
- Music commercial rights depend on the ElevenLabs plan and usage terms: verify rights for paid advertising before publishing.

