# Ballistic Calculator 2 — 30 s ad

Brief: `../ballistic_calculator_2_ad_30s.md`. Plan, decisions and timing: `PLAN.md`.

Two compositions, 900 frames at 30 fps:

| Composition | Size | Output |
|---|---|---|
| `Ad16x9` | 1920×1080 | `remotion/out/ballistic-calculator-2-16x9.mp4` — first video on the channel |
| `Ad9x16` | 1080×1920 | `remotion/out/ballistic-calculator-2-9x16.mp4` — YouTube Short |

## Pipeline

1. **Data** — `datagen/`, .NET 8 with BallisticCalculator 1.1.13. It writes `data.json`.
   - Run `dotnet run -c Release -- ../data.json` in `datagen/`, then copy the file to `remotion/public/data.json`.
   - Every plotted number comes from the library:
     - bc2-demo trajectory;
     - the wind, spin, Coriolis and drag curves;
     - the .338 Flatline comparison;
     - Lapua's radar drag table;
     - Mil-Dot BDC holds;
     - the `HitProbability.Estimate` impacts, with a fixed seed.
2. **Timing** — `timing.json` holds the scene cuts on the beats of music A2 and each voice line's start.
   - `beats.py` measures the tempo and envelope of every music candidate.
3. **Audio** — `python mix.py`.
   - It cuts each voice line out of the VO at its pauses and places it at its time.
   - It ducks the music under the voice and normalizes to −14 LUFS.
   - It writes `previews/*.mp3` for every voice × music pair.
   - For the chosen pair (`voice` / `music` in `timing.json`) it also writes:
     - `remotion/public/audio/{voiceover,music,mix}.wav`;
     - `remotion/public/placed.json`, the line and word times the scenes use.
4. **Video** — run these in `remotion/`:

```
npm install
npm run studio          # preview in the browser
npm run render:h        # 16:9
npm run render:v        # 9:16
node stills.mjs 30,120  # review stills -> out/stills/
python sheet.py Ad16x9  # contact sheet -> out/sheet-Ad16x9.jpg
```

## Replacing the audio

- **Another voice take:** put the new file in `audio/` and add or point to it under `voices` in `timing.json`. Set
  `"voice"` and re-run `mix.py`. The take must have the same 13 speech chunks (one per pause); `mix.py` stops if it
  does not.
- **Another track:** add it as `audio/music-<name>.mp3`, set `"music": "<name>"` and re-run `mix.py`. Then
  re-snap the cuts in `timing.json` to its beats (`beats.py`).
- **No audio:** if `remotion/public/audio/mix.wav` is missing, the video renders silent with a visible
  "NO AUDIO" note.

## Sources

| Asset | Source |
|---|---|
| Voice | ElevenLabs eleven_v4, library voice "Justin Case — Warm, Trustworthy, Clear" (`DtQLDxHbTiQTpVVanTy1`), flow `g7YpXHmJoq6CfOzSZb9A`, generation `91VtFYL4d6Ee7hLLY21y` |
| Music | ElevenLabs Music v2.5, same flow, generation `7tqMU3XKXIA4c2TfqZbZ` ("A2-pulse"), 128 BPM |
| Fonts | Inter and JetBrains Mono (SIL OFL), bundled through `@fontsource-variable` |

Commercial use is under the paid ElevenLabs plan. Check the current Eleven Music terms before paid promotion.

## Known limitations

- **Point-mass engine.** The plotted shapes are what the engine computes: 3DOF with corrections. The hit
  probability is a simulated estimate, and the video says so.
- **Data paths.** `datagen` reads the Lapua `.drg` from the demo folder on drive E: and Warner's velocities
  from the library's test resources. Both paths are absolute.
