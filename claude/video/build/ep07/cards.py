"""Episode 7 cards (1920x1080, headless Edge): the Reticle Editor (q06b) and the reticle-designer skill (q06c).
Usage: python cards.py  ->  card-editor.png, card-skill.png"""
import pathlib, subprocess

HERE = pathlib.Path(__file__).resolve().parent
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
SHOT = (HERE.parents[3] / "docs" / "screenshots" / "reticleeditor.png").as_uri()

CSS = """
* { margin: 0; padding: 0; box-sizing: border-box; }
html, body { width: 1920px; height: 1080px; overflow: hidden; background: #1b2330; color: #fff;
             font-family: "Segoe UI Variable Display", "Segoe UI", sans-serif; }
.wrap { position: absolute; inset: 0; padding: 80px 110px; display: flex; flex-direction: column; }
.kicker { font-size: 32px; font-weight: 600; letter-spacing: .2em; text-transform: uppercase; color: #ffb4a8; }
h1 { font-size: 76px; font-weight: 700; line-height: 1.08; margin-top: 14px; }
.sub { font-size: 38px; margin-top: 18px; opacity: .88; max-width: 1600px; line-height: 1.3; }
.shot { margin-top: 44px; width: 1700px; border-radius: 10px; box-shadow: 0 10px 40px rgba(0,0,0,.45); }
.flow { margin-top: 64px; display: flex; gap: 34px; align-items: center; font-size: 40px; }
.box { background: #26303f; border: 2px solid #3a4658; border-radius: 14px; padding: 26px 34px; line-height: 1.35; }
.arrow { font-size: 64px; color: #ffb4a8; }
.url { margin-top: auto; font-family: "Cascadia Mono", Consolas, monospace; font-size: 34px; line-height: 1.5; }
.url .k { color: #ffb4a8; font-family: "Segoe UI", sans-serif; font-weight: 600; }
"""


def page(body):
    return f"<!doctype html><html><head><meta charset='utf-8'><style>{CSS}</style></head><body><div class='wrap'>{body}</div></body></html>"


EDITOR = page(f"""
<div class='kicker'>Comes with the calculator</div>
<h1>Reticle Editor</h1>
<div class='sub'>Draw your own reticle, or open one of the reticles that ship with the program.</div>
<img class='shot' src='{SHOT}'>
""")

SKILL = page("""
<div class='kicker'>For Claude Code, Codex and other AI assistants</div>
<h1>The reticle-designer skill</h1>
<div class='flow'>
  <div class='box'>"a mil hash ladder,<br>0.2 mil marks out to 5 mil"<br><span style='opacity:.7'>or a picture / data sheet</span></div>
  <div class='arrow'>&rarr;</div>
  <div class='box'><b>.reticle</b> file<br>SVG preview<br>subtension table</div>
</div>
<div class='url'><span class='k'>In the repository:</span><br>
github.com/gehtsoft-usa/BallisticCalculator1/tree/main/SKILL/SKILLS/reticle-designer</div>
""")

for name, markup in (("card-editor", EDITOR), ("card-skill", SKILL)):
    src, png = HERE / f"{name}.html", HERE / f"{name}.png"
    src.write_text(markup, encoding="utf-8")
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    "--window-size=1920,1080", f"--screenshot={png}", src.as_uri()], check=True, capture_output=True, timeout=60)
    src.unlink()
    print(png)
