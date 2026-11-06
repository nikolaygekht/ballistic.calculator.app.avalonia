"""Put the episode 10 cards over ep10.mp4 as full frames. Each card runs from (clip, offset) to (clip, offset).
Run after assemble.py:  python overlay_cards.py   (rewrites ep10.mp4)"""
import json, os, pathlib, subprocess, sys
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
HERE = pathlib.Path(__file__).resolve().parent
FFMPEG = r"C:\exe\Video\ffmpeg\bin\ffmpeg.exe"
TITLE_S = 3.0

tl = json.load(open(HERE / "timeline.json", encoding="utf-8-sig"))
# video0 the same way assemble.py finds it: the first bright frame after the black cover
src = open(HERE / "assemble.py", encoding="utf-8").read()
import re
ns = {"subprocess": subprocess, "re": re, "FFMPEG": FFMPEG}
exec(src[src.index("def run("):src.index("if tl.get(\"marker\"):")], ns)
video0 = ns["marker_time"](tl["recording"])
at = {c["clip"]: TITLE_S + video0 + c["at"] for c in tl["cues"]}
CARDS = [("card-bc-by-speed.png", ("b01", 0.0), ("b02", 0.0)),
         ("card-warner-sheet.png", ("v01", 0.0), ("v02", 0.0)),
         ("card-csv.png", ("v04", 0.0), ("v04", 8.6))]
T = lambda ref: at[ref[0]] + ref[1]
args, fg, last = ["-i", str(HERE / "ep10.mp4")], [], "0:v"
for i, (png, a, b) in enumerate(CARDS, 1):
    args += ["-i", str(HERE / png)]
    fg.append(f"[{i}:v]scale=1920:1080[c{i}]")
    fg.append(f"[{last}][c{i}]overlay=0:0:enable='between(t,{T(a):.3f},{T(b):.3f})'[v{i}]")
    last = f"v{i}"
    print(f"{png}: {T(a):.2f} - {T(b):.2f} s")
out = HERE / "ep10.cards.mp4"
subprocess.run([FFMPEG, "-nostdin", "-y", "-hide_banner", "-loglevel", "error"] + args +
               ["-filter_complex", ";".join(fg), "-map", f"[{last}]", "-map", "0:a", "-c:v", "libx264", "-preset", "medium",
                "-crf", "20", "-pix_fmt", "yuv420p", "-c:a", "copy", str(out)], check=True)
os.replace(out, HERE / "ep10.mp4")
print("ep10.mp4 rewritten with clean cards")
