"""Place the voice-over lines at their timing.json slots and mix them over each music candidate.
Usage: python mix.py            -> audio/vo-<voice>-placed.wav, previews/<voice>+<music>.mp3, placed.json
Music candidates are audio/music-*.mp3. Voice lines are cut from the whole VO file by speech chunks
(silencedetect), so a new take only needs the same 13 chunks; a different count stops the script."""
import json, pathlib, re, subprocess

HERE = pathlib.Path(__file__).resolve().parent
FF = r"C:\exe\Video\ffmpeg\bin\ffmpeg.exe"
FP = r"C:\exe\Video\ffmpeg\bin\ffprobe.exe"
T = json.load(open(HERE / "timing.json", encoding="utf-8"))
LEN = T["frames"] / T["fps"]
(HERE / "previews").mkdir(exist_ok=True)


def duration(f):
    return float(subprocess.run([FP, "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", str(f)],
                                capture_output=True, text=True).stdout)


def chunks(f):
    """Speech intervals between pauses of 0.15 s or more."""
    e = subprocess.run([FF, "-hide_banner", "-i", str(f), "-af", "silencedetect=noise=-38dB:d=0.15", "-f", "null", "-"],
                       capture_output=True, text=True).stderr
    s = [float(x) for x in re.findall(r"silence_start: ([\d.]+)", e)]
    t = [float(x) for x in re.findall(r"silence_end: ([\d.]+)", e)]
    end = duration(f)
    if s and s[0] < 0.05:                       # leading silence
        start, s, t = t[0], s[1:], t[1:]
    else:
        start = 0.0
    out, cur = [], start
    for a, b in zip(s, t):
        out.append((cur, a)); cur = b
    if cur < end - 0.05:
        out.append((cur, end))
    return out


placed = {}
for name, v in T["voices"].items():
    src = HERE / v["file"]
    ch = chunks(src)
    if len(ch) != 13:
        raise SystemExit(f"{name}: {len(ch)} speech chunks, expected 13 -- {ch}")
    inputs, filters, rows = [], [], []
    for i, ln in enumerate(T["lines"]):
        a = max(0.0, ch[ln["chunks"][0]][0] - 0.06)
        b = ch[ln["chunks"][-1]][1] + 0.12
        lead = ch[ln["chunks"][0]][0] - a                   # so the first word, not the padding, lands on 'at'
        at = ln["at"] - lead
        filters.append(f"[0:a]atrim={a:.3f}:{b:.3f},asetpts=PTS-STARTPTS,afade=t=in:d=0.03,"
                       f"afade=t=out:st={b - a - 0.05:.3f}:d=0.05,adelay={int(at * 1000)}:all=1[l{i}]")
        rows.append({"text": ln["text"], "scene": ln["scene"], "start": round(ln["at"], 2),
                     "end": round(at + (ch[ln['chunks'][-1]][1] - a), 2),
                     "words": [round(at + ch[c][0] - a, 3) for c in ln["chunks"]]})   # each chunk's start
    filters.append("".join(f"[l{i}]" for i in range(len(T["lines"]))) +
                   f"amix=inputs={len(T['lines'])}:normalize=0,apad,atrim=0:{LEN}[vo]")
    out = HERE / "audio" / f"vo-{name}-placed.wav"
    subprocess.run([FF, "-y", "-v", "error", "-i", str(src), "-filter_complex", ";".join(filters), "-map", "[vo]",
                    "-ar", "48000", "-ac", "2", str(out)], check=True)
    placed[name] = rows
    print(f"{name}: last word ends at {rows[-1]['end']:.2f} s")
    for r in rows:
        print(f"   {r['start']:6.2f}-{r['end']:6.2f}  {r['text']}")

json.dump(placed, open(HERE / "placed.json", "w", encoding="utf-8"), indent=1, ensure_ascii=False)

def lufs(f):
    e = subprocess.run([FF, "-hide_banner", "-i", str(f), "-af", "ebur128", "-f", "null", "-"],
                       capture_output=True, text=True).stderr
    return float(re.findall(r"I:\s+(-?[\d.]+) LUFS", e)[-1])


MUSIC_LUFS = -27          # every candidate brought to the same loudness by a static gain, so its own build survives
for m in sorted((HERE / "audio").glob("music-*.mp3")):
    gain = MUSIC_LUFS - lufs(m)
    for name in T["voices"]:
        vo = HERE / "audio" / f"vo-{name}-placed.wav"
        out = HERE / "previews" / f"{name}+{m.stem.removeprefix('music-')}.mp3"
        # music ~ -10 dB under the voice, ducked further while speech plays; whole mix to -14 LUFS (YouTube)
        fc = (f"[1:a]aresample=48000,apad,atrim=0:{LEN},afade=t=out:st={LEN - 0.6}:d=0.6,volume={gain:.2f}dB[m];"
              f"[0:a]asplit=2[v][sc];[m][sc]sidechaincompress=threshold=0.03:ratio=4:attack=20:release=350[md];"
              f"[md][v]amix=inputs=2:normalize=0,loudnorm=I=-14:TP=-1.5:LRA=9[out]")
        chosen = name == T.get("voice") and m.stem == f"music-{T.get('music')}"
        if chosen:      # the chosen pair also goes to the Remotion project: voice, music and the ducked mix
            pub = HERE / "remotion" / "public" / "audio"; pub.mkdir(parents=True, exist_ok=True)
            fc_c = fc.replace("[out]", "[o2]")
            subprocess.run([FF, "-y", "-v", "error", "-i", str(vo), "-i", str(m), "-filter_complex", fc_c,
                            "-map", "[o2]", "-ar", "48000", str(pub / "mix.wav")], check=True)
            subprocess.run([FF, "-y", "-v", "error", "-i", str(vo), str(pub / "voiceover.wav")], check=True)
            subprocess.run([FF, "-y", "-v", "error", "-i", str(m), "-ar", "48000", str(pub / "music.wav")], check=True)
            json.dump({"voice": name, "music": m.stem, "lines": placed[name]},
                      open(HERE / "remotion" / "public" / "placed.json", "w", encoding="utf-8"), indent=1)
        subprocess.run([FF, "-y", "-v", "error", "-i", str(vo), "-i", str(m), "-filter_complex", fc, "-map", "[out]",
                        "-ar", "48000", "-b:a", "192k", str(out)], check=True)
        print(out.name, f"{duration(out):.2f} s", "<- chosen" if chosen else "")
