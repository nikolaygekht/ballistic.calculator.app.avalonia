"""Download voice clips. Usage: python fetch.py <epNN/urls.json> [only,these,names]
Clips land in the audio/ folder NEXT TO THE JSON (not next to this script — that once overwrote ep01 clips)."""
import json, pathlib, subprocess, sys, urllib.request

HERE = pathlib.Path(sys.argv[1]).resolve().parent
ONLY = set(sys.argv[2].split(",")) if len(sys.argv) > 2 else None
FFPROBE = r"C:\exe\Video\ffmpeg\bin\ffprobe.exe"
(HERE / "audio").mkdir(exist_ok=True)
for name, url in json.load(open(sys.argv[1])).items():
    if ONLY and name not in ONLY:
        continue
    out = HERE / "audio" / f"{name}.mp3"
    urllib.request.urlretrieve(url, out)
    d = subprocess.run([FFPROBE, "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", str(out)],
                       capture_output=True, text=True).stdout.strip()
    print(f"{name}: {d} s")
