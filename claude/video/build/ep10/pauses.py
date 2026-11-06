"""Print the pauses inside each clip (end of speech chunk -> start of next) to time actions to words.
Usage: python pauses.py [clip ...]"""
import json, pathlib, re, subprocess, sys
HERE = pathlib.Path(__file__).resolve().parent
FF = r"C:\exe\Video\ffmpeg\bin\ffmpeg.exe"
clips = dict(json.load(open(HERE / "clips.json", encoding="utf-8")))
for k in (sys.argv[1:] or clips):
    f = HERE / "audio" / f"{k}.mp3"
    if not f.exists():
        continue
    e = subprocess.run([FF, "-hide_banner", "-i", str(f), "-af", "silencedetect=noise=-38dB:d=0.18", "-f", "null", "-"],
                       capture_output=True, text=True).stderr
    s = [float(x) for x in re.findall(r"silence_start: ([\d.]+)", e)]
    t = [float(x) for x in re.findall(r"silence_end: ([\d.]+)", e)]
    chunks = " ".join(f"[{a:.2f}-{b:.2f}]" for a, b in zip(s, t))
    print(f"{k}: {clips[k]}\n     pauses {chunks}")
