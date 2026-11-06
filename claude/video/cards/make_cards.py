"""Render the title and end cards for the video playlist.

Usage:  python make_cards.py [title|end|all] [episode numbers...]
Output: out/title-NN.png, out/end-NN.png  (1920x1080), rendered with headless Edge.
The background is background.png beside this script (the upscaled range-with-wind-flags image).
"""
import html
import pathlib
import subprocess
import sys

HERE = pathlib.Path(__file__).resolve().parent
OUT = HERE / "out"
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
BG = (HERE / "background.png").as_uri()

EPISODES = {
    1: "Your First Trajectory",
    2: "Table, Chart, Reticle, Summary",
    3: "Weather and Wind",
    4: "The Effects That Only Matter Far Away",
    5: "Dialled Clicks, and a Winter Zero in July",
    6: "A Measured Drag Curve",
    7: "Everything At Once",
    8: "Zero One Load, Shoot Another",
    9: "Shooting at an Angle",
    10: "Your Own Drag Table",
}
# No "N of M" on the cards: episodes are added as we go (2026-11-04). The last one listed gets the final end card.
TOTAL = len(EPISODES)

LINKS_SHORT = [
    ("Download", "github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases"),
    ("User guide", "nikolaygekht.github.io/ballistic.calculator.app.avalonia"),
]
LINKS_FINAL = [
    ("Download", "github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases"),
    ("Source", "github.com/nikolaygekht/ballistic.calculator.app.avalonia"),
    ("User guide", "nikolaygekht.github.io/ballistic.calculator.app.avalonia"),
]

CSS = """
* { margin: 0; padding: 0; box-sizing: border-box; }
html, body { width: 1920px; height: 1080px; overflow: hidden; background: #1b2330; }
body { font-family: "Segoe UI Variable Display", "Segoe UI", sans-serif; color: #fff; position: relative; }
.bg { position: absolute; inset: 0; background: url('%(bg)s') center / cover no-repeat; }
.shade { position: absolute; inset: 0; }
.content { position: absolute; inset: 0; padding: 110px 130px; display: flex; flex-direction: column; }
.brand { font-size: 34px; font-weight: 600; letter-spacing: 0.22em; text-transform: uppercase; opacity: 0.9;
         text-shadow: 0 2px 12px rgba(0,0,0,.45); }
.title { font-size: 104px; font-weight: 700; line-height: 1.04; margin-top: 26px; max-width: 1500px;
         text-shadow: 0 4px 28px rgba(0,0,0,.5); }
.pill { display: inline-block; margin-top: 40px; padding: 10px 26px; border-radius: 999px;
        background: #c8322a; font-size: 34px; font-weight: 600; letter-spacing: 0.04em;
        box-shadow: 0 6px 24px rgba(0,0,0,.3); align-self: flex-start; }
.next-label { font-size: 36px; font-weight: 600; letter-spacing: 0.18em; text-transform: uppercase; color: #ffb4a8; }
.next-title { font-size: 84px; font-weight: 700; line-height: 1.08; margin-top: 18px; max-width: 1600px;
              text-shadow: 0 4px 24px rgba(0,0,0,.5); }
.next-ep { font-size: 40px; font-weight: 500; margin-top: 18px; opacity: .9; }
.links { margin-top: auto; display: grid; grid-template-columns: auto 1fr; gap: 14px 34px; font-size: 38px; }
.links .k { font-weight: 600; color: #ffb4a8; }
.links .v { font-family: "Cascadia Mono", Consolas, monospace; font-size: 36px; }
"""


def page(body: str, shade: str) -> str:
    return (f"<!doctype html><html><head><meta charset='utf-8'><style>{CSS % {'bg': BG}}</style></head>"
            f"<body><div class='bg'></div><div class='shade' style=\"background:{shade}\"></div>"
            f"<div class='content'>{body}</div></body></html>")


def title_card(n: int) -> str:
    body = (f"<div class='brand'>Ballistic Calculator 2</div>"
            f"<div class='title'>{html.escape(EPISODES[n])}</div>"
            f"<div class='pill'>Episode {n}</div>")
    # Darken the sky, where the text sits; leave the range below untouched.
    return page(body, "linear-gradient(180deg, rgba(18,16,14,.62) 0%, rgba(18,16,14,.30) 45%, rgba(18,16,14,0) 70%)")


def links_html(links) -> str:
    return "<div class='links'>" + "".join(
        f"<div class='k'>{html.escape(k)}</div><div class='v'>{html.escape(v)}</div>" for k, v in links) + "</div>"


def end_card(n: int) -> str:
    shade = "linear-gradient(180deg, rgba(18,16,14,.70) 0%, rgba(18,16,14,.45) 50%, rgba(18,16,14,.78) 100%)"
    if n < TOTAL:
        body = (f"<div class='next-label'>Next</div>"
                f"<div class='next-title'>{html.escape(EPISODES[n + 1])}</div>"
                f"<div class='next-ep'>Episode {n + 1}</div>"
                + links_html(LINKS_SHORT))
    else:
        body = (f"<div class='brand'>Ballistic Calculator 2</div>"
                f"<div class='next-title'>Free and open source.<br>Windows, Linux, macOS.</div>"
                + links_html(LINKS_FINAL))
    return page(body, shade)


def render(name: str, markup: str) -> pathlib.Path:
    OUT.mkdir(exist_ok=True)
    src = OUT / f"{name}.html"
    png = OUT / f"{name}.png"
    src.write_text(markup, encoding="utf-8")
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    "--window-size=1920,1080", f"--screenshot={png}", src.as_uri()],
                   check=True, capture_output=True, timeout=60)
    return png


def main(argv):
    kind = argv[0] if argv else "all"
    eps = [int(a) for a in argv[1:]] or list(EPISODES)
    for n in eps:
        if kind in ("title", "all"):
            print(render(f"title-{n:02d}", title_card(n)))
        if kind in ("end", "all"):
            print(render(f"end-{n:02d}", end_card(n)))


if __name__ == "__main__":
    main(sys.argv[1:])
