"""YouTube thumbnails, 1280x720 JPEG (< 2 MB).
Episodes: their title cards (cards/out/title-NN.png, made by cards/make_cards.py), scaled down - the user's choice.
The ad has no title card: it gets its own in the ad's look (dark field, big words left, a frame of the ad right).
Usage: python make_thumbs.py [ep01 ep02 ... ad30]  ->  thumbs/<name>.jpg"""
import html, pathlib, subprocess, sys
from PIL import Image

HERE = pathlib.Path(__file__).resolve().parent
VIDEO = HERE.parent
FF = r"C:\exe\Video\ffmpeg\bin\ffmpeg.exe"
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
FONTS = VIDEO / "ad30" / "remotion" / "node_modules" / "@fontsource-variable"
INTER = (FONTS / "inter" / "files" / "inter-latin-wght-normal.woff2").as_uri()
MONO = (FONTS / "jetbrains-mono" / "files" / "jetbrains-mono-latin-wght-normal.woff2").as_uri()

CARDS = VIDEO / "cards" / "out"
EPISODES = [f"ep{n:02d}" for n in range(1, 11)]

# The ad: (video, time s, crop box in the 1920x1080 frame, title, call-out, badge)
T = {
    "ad30": ("ad30/remotion/out/ballistic-calculator-2-16x9.mp4", 13.6, (110, 230, 1260, 900), "Free ballistic calculator",
             "Windows · Linux · macOS", None),
}


def from_card(name):
    jpg = HERE / f"{name}.jpg"
    Image.open(CARDS / f"title-{name[2:]}.png").convert("RGB").resize((1280, 720), Image.LANCZOS).save(jpg, quality=92)
    print(jpg.name, f"{jpg.stat().st_size / 1e6:.2f} MB  (title card)")

CSS = f"""
@font-face {{ font-family: Inter; src: url('{INTER}') format('woff2'); font-weight: 100 900; }}
@font-face {{ font-family: JBM; src: url('{MONO}') format('woff2'); font-weight: 100 800; }}
* {{ margin: 0; padding: 0; box-sizing: border-box; }}
html, body {{ width: 1280px; height: 720px; overflow: hidden; background: #0a0d12; }}
body {{ position: relative; font-family: Inter, sans-serif; color: #f3f6f9; }}
.grid {{ position: absolute; inset: 0; background-image: linear-gradient(#121821 1px, transparent 1px),
        linear-gradient(90deg, #121821 1px, transparent 1px); background-size: 40px 40px; opacity: .8; }}
.glow {{ position: absolute; inset: 0; background: radial-gradient(ellipse at 78% 50%, rgba(34,211,238,.16), transparent 60%); }}
.shot {{ position: absolute; right: -30px; top: 50%; transform: translateY(-50%) rotate(-2deg); width: 700px;
        border-radius: 14px; overflow: hidden; border: 2px solid #2a3442;
        box-shadow: 0 24px 70px rgba(0,0,0,.65), 0 0 0 6px rgba(34,211,238,.10); }}
.shot img {{ display: block; width: 100%; max-height: 600px; object-fit: cover; object-position: left top; }}
.fade {{ position: absolute; inset: 0; background: linear-gradient(90deg, #0a0d12 0%, #0a0d12 44%, rgba(10,13,18,.0) 62%); }}
.left {{ position: absolute; left: 60px; top: 64px; bottom: 60px; width: 600px; display: flex; flex-direction: column; }}
.brand {{ font-weight: 700; font-size: 25px; letter-spacing: .2em; color: #22d3ee; }}
.badge {{ align-self: flex-start; margin-top: 22px; background: #ffb020; color: #0a0d12; font-weight: 800; font-size: 32px;
         padding: 4px 18px; border-radius: 8px; letter-spacing: .04em; }}
.title {{ margin-top: auto; font-weight: 850; font-size: 84px; line-height: .98; letter-spacing: -.015em; text-transform: uppercase;
         text-shadow: 0 6px 30px rgba(0,0,0,.6); }}
.call {{ margin-top: 24px; font-family: JBM; font-weight: 600; font-size: 29px; color: #ffb020; }}
"""


def crop(name):
    video, t, box, *_ = T[name]
    frame = HERE / "crops" / f"{name}-frame.png"
    frame.parent.mkdir(exist_ok=True)
    subprocess.run([FF, "-y", "-v", "error", "-ss", str(t), "-i", str(VIDEO / video), "-frames:v", "1", str(frame)], check=True)
    out = HERE / "crops" / f"{name}.png"
    Image.open(frame).crop(box).save(out)
    frame.unlink()
    return out


def build(name):
    _, _, _, title, call, badge = T[name]
    img = crop(name).as_uri()
    size = 84 if len(title) <= 22 else 74
    page = f"""<!doctype html><html><head><meta charset="utf-8"><style>{CSS}</style></head><body>
<div class="grid"></div><div class="glow"></div>
<div class="shot"><img src="{img}"></div><div class="fade"></div>
<div class="left">
  <div class="brand">BALLISTIC CALCULATOR 2</div>
  {f'<div class="badge">{badge}</div>' if badge else ''}
  <div class="title" style="font-size:{size}px">{html.escape(title)}</div>
  <div class="call">{html.escape(call)}</div>
</div></body></html>"""
    src = HERE / f"_{name}.html"; png = HERE / f"_{name}.png"
    src.write_text(page, encoding="utf-8")
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    "--window-size=1280,720", f"--screenshot={png}", src.as_uri()], check=True, capture_output=True, timeout=60)
    jpg = HERE / f"{name}.jpg"
    Image.open(png).convert("RGB").save(jpg, quality=92)
    src.unlink(); png.unlink()
    print(jpg.name, f"{jpg.stat().st_size / 1e6:.2f} MB")


def build_short():
    """1080x1920 cover for the Short: the four effect cards from the Short itself under the hook words."""
    frame = HERE / "crops" / "short-frame.png"; frame.parent.mkdir(exist_ok=True)
    subprocess.run([FF, "-y", "-v", "error", "-ss", "6.6", "-i", str(VIDEO / "ad30/remotion/out/ballistic-calculator-2-9x16.mp4"),
                    "-frames:v", "1", str(frame)], check=True)
    cards = HERE / "crops" / "short.png"
    Image.open(frame).crop((60, 610, 1020, 1500)).save(cards); frame.unlink()
    page = f"""<!doctype html><html><head><meta charset="utf-8"><style>{CSS}
html, body {{ width: 1080px; height: 1920px; }}
.glow {{ background: radial-gradient(ellipse at 50% 62%, rgba(34,211,238,.14), transparent 60%); }}
.top {{ position: absolute; left: 72px; right: 72px; top: 230px; }}
.brand {{ font-size: 34px; }}
.title {{ margin-top: 26px; font-size: 106px; line-height: .98; white-space: nowrap; }}
.sub {{ margin-top: 28px; font-family: JBM; font-weight: 600; font-size: 40px; color: #ffb020; }}
.cards {{ position: absolute; left: 60px; top: 760px; width: 960px; }}
.cards img {{ display: block; width: 100%; }}
.foot {{ position: absolute; left: 72px; right: 72px; top: 1690px; font-weight: 800; font-size: 44px; letter-spacing: .1em; }}
.foot span {{ display: block; margin-top: 10px; font-weight: 600; font-size: 32px; letter-spacing: .14em; color: #a9b4c2; }}
</style></head><body>
<div class="grid"></div><div class="glow"></div>
<div class="top"><div class="brand">BALLISTIC CALCULATOR 2</div>
  <div class="title">WIND · DRAG<br>SPIN · CORIOLIS</div>
  <div class="sub">one free ballistic calculator</div></div>
<div class="cards"><img src="{cards.as_uri()}"></div>
<div class="foot">FREE &amp; OPEN SOURCE<span>WINDOWS · LINUX · macOS</span></div>
</body></html>"""
    src = HERE / "_short.html"; png = HERE / "_short.png"
    src.write_text(page, encoding="utf-8")
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    "--window-size=1080,1920", f"--screenshot={png}", src.as_uri()], check=True, capture_output=True, timeout=60)
    jpg = HERE / "ad30-short.jpg"
    Image.open(png).convert("RGB").save(jpg, quality=92)
    src.unlink(); png.unlink()
    print(jpg.name, f"{jpg.stat().st_size / 1e6:.2f} MB")


for n in (sys.argv[1:] or EPISODES + list(T) + ["short"]):
    from_card(n) if n in EPISODES else build_short() if n == "short" else build(n)
