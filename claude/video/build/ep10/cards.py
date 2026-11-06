"""Episode 10 cards (1920x1080): the five BC points (matplotlib), Warner's data sheet and the CSV file (headless Edge).
Usage: python cards.py  ->  card-bc-by-speed.png, card-warner-sheet.png, card-csv.png"""
import html, pathlib, re, subprocess
import matplotlib; matplotlib.use("Agg")
import matplotlib.pyplot as plt

HERE = pathlib.Path(__file__).resolve().parent
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
G7_CS = r"D:\develop\components\BusinessSpecificComponents\BallisticCalculator.Net\BallisticCalculator\Drag\G7DragTable.cs"
DRG = r"E:\ngeht\Documents\Ballistic Calculator demo\30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg"
WARNER = r"D:\develop\components\BusinessSpecificComponents\BallisticCalculator.Net\BallisticCalculator.Test\resources\warner_338_flatline.txt"
BG, INK, INK2, GRID, ACC, BLUE = "#1b2330", "#ffffff", "#b8c2d0", "#2c3646", "#ffb4a8", "#7fb2ff"

# ---------------------------------------------------------------- 1. the five BC points
g7 = [(float(a), float(b)) for a, b in re.findall(r"DragTableDataPoint\(([\d.]+),\s*([\d.]+)\)", open(G7_CS).read())]
drg = [(float(m), float(cd)) for cd, m in (r.split() for r in open(DRG).read().splitlines()[1:] if r.strip())]


def interp(t, x):
    for (x0, y0), (x1, y1) in zip(t, t[1:]):
        if x0 <= x <= x1:
            return y0 + (y1 - y0) * (x - x0) / (x1 - x0)


SOUND = 1116.45
sd = (11.30 / 1000 * 2.20462) / (7.83 / 25.4) ** 2
bc = lambda v: sd * interp(g7, v / SOUND) / interp(drg, v / SOUND)
vs = list(range(2650, 1099, -10))
KNOTS = [(2600, 0.250), (2200, 0.240), (1800, 0.244), (1400, 0.263), (1100, 0.293)]
plt.rcParams.update({"font.family": "Segoe UI", "font.size": 24})
fig, ax = plt.subplots(figsize=(19.2, 10.8), dpi=100)
fig.patch.set_facecolor(BG); ax.set_facecolor(BG)
ax.axhline(0.253, color=INK2, lw=2.5, ls=(0, (6, 5)))
ax.text(2455, 0.2537, "one number from the box: 0.253", ha="left", va="bottom", color=INK2, fontsize=26)
ax.plot(vs, [bc(v) for v in vs], color=BLUE, lw=5, solid_capstyle="round")
for v, b in KNOTS:
    ax.plot(v, b, "o", ms=20, color=ACC, mec=BG, mew=4, zorder=5)
    ax.text(v, b + 0.0035, f"{b:.3f}\n{v} ft/s", ha="center", va="bottom", color=INK, fontsize=28, fontweight="bold",
            linespacing=1.15)
ax.set_xlim(2700, 1000); ax.set_ylim(0.228, 0.312)
ax.set_xlabel("Velocity, ft/s   (the bullet slows down from left to right)", color=INK2, fontsize=26, labelpad=14)
ax.set_ylabel("G7 ballistic coefficient", color=INK2, fontsize=26, labelpad=14)
ax.grid(axis="y", color=GRID, lw=1.5); ax.set_axisbelow(True)
for s in ("top", "right", "left"):
    ax.spines[s].set_visible(False)
ax.spines["bottom"].set_color(INK2); ax.tick_params(colors=INK2, length=0, pad=10)
fig.text(0.057, 0.925, "FIVE POINTS FROM THE RADAR CURVE", fontsize=26, fontweight="bold", color=ACC)
fig.text(0.057, 0.865, "The real G7 BC of the 175 grain Scenar-L, by speed", fontsize=46, fontweight="bold", color=INK)
fig.text(0.057, 0.04, "Computed from Lapua's radar drag curve, standard air (episode 6)", fontsize=22, color=INK2)
fig.subplots_adjust(left=0.10, right=0.96, top=0.80, bottom=0.17)
fig.savefig(HERE / "card-bc-by-speed.png", facecolor=BG)
plt.close(fig)
print(HERE / "card-bc-by-speed.png")

# ---------------------------------------------------------------- 2 + 3: HTML cards
CSS = f"""
* {{ margin: 0; padding: 0; box-sizing: border-box; }}
html, body {{ width: 1920px; height: 1080px; overflow: hidden; background: {BG}; color: {INK};
             font-family: "Segoe UI Variable Display", "Segoe UI", sans-serif; }}
.wrap {{ position: absolute; inset: 0; padding: 70px 110px; display: flex; flex-direction: column; }}
.kicker {{ font-size: 30px; font-weight: 700; letter-spacing: .2em; text-transform: uppercase; color: {ACC}; }}
h1 {{ font-size: 64px; font-weight: 700; line-height: 1.08; margin-top: 12px; }}
.src {{ position: absolute; left: 110px; bottom: 40px; font-size: 24px; color: {INK2}; }}
"""
rows = [l.strip().split(";") for l in open(WARNER, encoding="utf-8") if l.startswith("v;")]
vel = [(d.replace("yd", ""), v.replace("ft/s", ""), tt) for _, d, v, tt in rows]

half = (len(vel) + 1) // 2
def table(part):
    tr = "".join(f"<tr><td>{d}</td><td>{float(v):,.1f}</td><td>{tt}</td></tr>" for d, v, tt in part)
    return f"<table><tr><th>Distance, yd</th><th>Velocity, ft/s</th><th>Time, s</th></tr>{tr}</table>"
SHEET = f"""<!doctype html><html><head><meta charset='utf-8'><style>{CSS}
.row {{ display: flex; gap: 60px; margin-top: 60px; align-items: flex-start; }}
.head {{ flex: 0 0 440px; font-size: 32px; line-height: 1.55; }}
.head b {{ display: inline-block; width: 210px; color: {INK2}; font-weight: 500; }}
.head .note {{ margin-top: 34px; font-size: 28px; line-height: 1.4; color: {INK2}; }}
table {{ border-collapse: collapse; font-size: 36px; font-variant-numeric: tabular-nums; }}
th {{ text-align: right; font-weight: 600; color: {INK2}; padding: 0 26px 10px; font-size: 24px; }}
td {{ text-align: right; padding: 10px 26px; border-top: 1px solid {GRID}; }}
td:nth-child(2) {{ color: {ACC}; font-weight: 700; }}
</style></head><body><div class='wrap'>
<div class='kicker'>A published Doppler radar data sheet</div>
<h1>Warner Tool · .338 Flatline, 285 grains</h1>
<div class='row'>
  <div class='head'><b>Company</b>Warner<br><b>Bullet</b>338 Flatline<br><b>Weight</b>285 gr<br><b>Diameter</b>0.338 in
    <div class='note'>The sheet also has a drag table: Mach, CD, and G1 and G7 coefficients.</div></div>
  {table(vel[:half])}
  {table(vel[half:])}
</div>
<div class='src'>Source: Warner Tool Company Doppler data, warner-tool.com/doppler-data · the velocity table, as published</div>
</div></body></html>"""

csv_text = (HERE / "338-flatline-velocities.csv").read_text(encoding="utf-8").rstrip("\n").split("\n")
lines = "".join(f"<div class='ln'><span class='no'>{i}</span>{html.escape(l)}</div>" for i, l in enumerate(csv_text, 1))
CSV = f"""<!doctype html><html><head><meta charset='utf-8'><style>{CSS}
.win {{ margin-top: 30px; width: 1100px; background: #f7f7f5; color: #1b1b1b; border-radius: 10px; overflow: hidden;
        box-shadow: 0 20px 60px rgba(0,0,0,.45); }}
.bar {{ background: #e4e4e0; padding: 12px 22px; font-size: 24px; color: #333; }}
.body {{ padding: 14px 0 18px; font-family: Consolas, "Cascadia Mono", monospace; font-size: 31px; line-height: 1.32; columns: 2; column-gap: 0; }}
.ln {{ padding-left: 22px; break-inside: avoid; }}
.no {{ display: inline-block; width: 52px; color: #9a9a96; }}
.ln:first-child {{ color: #b0412e; font-weight: 700; }}
.side {{ position: absolute; left: 1290px; top: 300px; width: 520px; font-size: 34px; line-height: 1.45; color: {INK2}; }}
.side b {{ color: {INK}; }}
</style></head><body><div class='wrap'>
<div class='kicker'>The file for Load Csv</div>
<h1>338-flatline-velocities.csv</h1>
<div class='win'><div class='bar'>338-flatline-velocities.csv — a plain text file</div><div class='body'>{lines}</div></div>
<div class='side'><b>Line 1:</b> a header, optional.<br><br><b>Then:</b> a distance and a velocity, with <b>;</b> between them.<br><br><b>Every value has its unit:</b> yd, m, ft/s, m/s.</div>
</div></body></html>"""

for name, markup in (("card-warner-sheet", SHEET), ("card-csv", CSV)):
    src, png = HERE / f"{name}.html", HERE / f"{name}.png"
    src.write_text(markup, encoding="utf-8")
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    "--window-size=1920,1080", f"--screenshot={png}", src.as_uri()], check=True, capture_output=True, timeout=60)
    src.unlink()
    print(png)
