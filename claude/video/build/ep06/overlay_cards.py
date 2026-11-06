"""Put the part-one cards over ep06.mp4 as clean full frames, for exactly the intervals the take showed them.
The take's own Tk card window came out shifted (cropped titles); this replaces those frames.
Run after assemble.py:  python overlay_cards.py   (rewrites ep06.mp4)"""
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
CARDS = [("card-prb-profiles.png", "t01", "t02"), ("card-curves.png", "t02", "t04"), ("card-prb-hornady.png", "t04", "t05"),
         ("bc-along-flight.png", "t05", "t06"), ("card-prb-radar.png", "t06", "t08")]
args, fg, last = ["-i", str(HERE / "ep06.mp4")], [], "0:v"
for i, (png, a, b) in enumerate(CARDS, 1):
    args += ["-i", str(HERE / png)]
    fg.append(f"[{i}:v]scale=1920:1080[c{i}]")
    fg.append(f"[{last}][c{i}]overlay=0:0:enable='between(t,{at[a]:.3f},{at[b]:.3f})'[v{i}]")
    last = f"v{i}"
    print(f"{png}: {at[a]:.2f} - {at[b]:.2f} s")
out = HERE / "ep06.cards.mp4"
subprocess.run([FFMPEG, "-nostdin", "-y", "-hide_banner", "-loglevel", "error"] + args +
               ["-filter_complex", ";".join(fg), "-map", f"[{last}]", "-map", "0:a", "-c:v", "libx264", "-preset", "medium",
                "-crf", "20", "-pix_fmt", "yuv420p", "-c:a", "copy", str(out)], check=True)
os.replace(out, HERE / "ep06.mp4")
print("ep06.mp4 rewritten with clean cards")
