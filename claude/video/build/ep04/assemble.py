"""Assemble an episode: title card + recorded take + end card, with every voice clip placed at its logged time.

Usage: python assemble.py [timeline.json]   ->  epNN.mp4   (the folder name gives the episode)
Copy this file into each build/epNN/ folder.
"""
import json, pathlib, re, subprocess, sys

HERE = pathlib.Path(__file__).resolve().parent
EP = HERE.name                       # "ep02"
CARDS = HERE.parent.parent / "cards" / "out"
FFMPEG = r"C:\exe\Video\ffmpeg\bin\ffmpeg.exe"
TITLE_S, END_S, FPS = 3.0, 5.0, 60

tl = json.load(open(HERE / (sys.argv[1] if len(sys.argv) > 1 else "timeline.json"), encoding="utf-8-sig"))

def run(args):
    subprocess.run([FFMPEG, "-nostdin", "-y", "-hide_banner", "-loglevel", "error"] + args, check=True)


def marker_time(rec):
    """Video time of the first bright frame after the black cover Take.start() shows (= script T0)."""
    p = subprocess.run([FFMPEG, "-nostdin", "-hide_banner", "-t", "8", "-i", rec, "-vf",
                        "crop=400:300:760:390,signalstats,metadata=print:key=lavfi.signalstats.YAVG",
                        "-f", "null", "-"], capture_output=True, text=True)
    t, seen_black = None, False
    for line in p.stderr.splitlines():
        m = re.search(r"pts_time:([\d.]+)", line)
        if m:
            t = float(m.group(1)); continue
        m = re.search(r"YAVG=([\d.]+)", line)
        if m:
            y = float(m.group(1))
            if y < 25:
                seen_black = True
            elif seen_black and y > 60:
                return t
    raise SystemExit("sync marker not found in the first 8 s of " + rec)


if tl.get("marker"):
    video0 = marker_time(tl["recording"])
    print(f"video0 from the marker: {video0:.3f} s (OBS said {tl.get('video0_obs')})")
else:                                  # takes recorded before the marker existed: video0 measured by hand
    video0 = tl["video0"]

# step 1: the voice track alone
ins, fg, labels = [], [], []
for i, c in enumerate(tl["cues"]):
    ins += ["-i", str(HERE / "audio" / f"{c['clip']}.mp3")]
    ms = int(round((TITLE_S + video0 + c["at"]) * 1000))
    fg.append(f"[{i}:a]aresample=48000,adelay={ms}|{ms}[a{i}]")
    labels.append(f"[a{i}]")
fg.append("".join(labels) + f"amix=inputs={len(labels)}:normalize=0:duration=longest[a]")
voice = HERE / "voice.wav"
run(ins + ["-filter_complex", ";".join(fg), "-map", "[a]", "-ac", "2", str(voice)])

# step 2: title card + take + end card, with the voice track
norm = f"scale=1920:1080,fps={FPS},format=yuv420p,setsar=1"
out = HERE / f"{EP}.mp4"
run(["-loop", "1", "-framerate", str(FPS), "-t", str(TITLE_S), "-i", str(CARDS / f"title-{EP[2:]}.png"),
     "-i", tl["recording"],
     "-loop", "1", "-framerate", str(FPS), "-t", str(END_S), "-i", str(CARDS / f"end-{EP[2:]}.png"),
     "-i", str(voice),
     "-filter_complex", f"[0:v]{norm}[t];[1:v]{norm}[b];[2:v]{norm}[e];[t][b][e]concat=n=3:v=1:a=0[v];[3:a]apad[a]",
     "-map", "[v]", "-map", "[a]", "-shortest",
     "-c:v", "libx264", "-preset", "medium", "-crf", "18", "-c:a", "aac", "-b:a", "192k",
     "-movflags", "+faststart", str(out)])
print(out)
