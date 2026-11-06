"""Episode 6 cards (2560x1440):
  card-shapes.png  - schematic G1 (flat base) and G7 (boat tail) standard projectiles
  card-curves.png  - drag coefficient vs Mach: Lapua radar curve vs G7 scaled to BC 0.253 and G1 scaled to 0.505
Equivalent Cd of a BC on a standard table: Cd_std(M) * SD / BC (SD in lb/in^2), the same scale a .drg stores.
Usage: python cards.py"""
import re
import matplotlib; matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Polygon

LIB = r"D:\develop\components\BusinessSpecificComponents\BallisticCalculator.Net\BallisticCalculator\Drag"
DRG = r"E:\ngeht\Documents\Ballistic Calculator demo\30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg"
table = lambda name: [(float(a), float(b)) for a, b in
                      re.findall(r"DragTableDataPoint\(([\d.]+),\s*([\d.]+)\)", open(fr"{LIB}\{name}DragTable.cs").read())]
G1, G7 = table("G1"), table("G7")
RADAR = [(float(m), float(cd)) for cd, m in (r.split() for r in open(DRG).read().splitlines()[1:] if r.strip())]
SD = (11.30 / 1000 * 2.20462) / (7.83 / 25.4) ** 2

SURF, INK, INK2, GRID = "#fcfcfb", "#0b0b0b", "#52514e", "#e4e3df"
BLUE, ORANGE, AQUA = "#2a78d6", "#eb6834", "#1baf7a"
plt.rcParams.update({"font.family": "Segoe UI", "font.size": 30})


def interp(t, x):
    for (x0, y0), (x1, y1) in zip(t, t[1:]):
        if x0 <= x <= x1:
            return y0 + (y1 - y0) * (x - x0) / (x1 - x0)


def frame(title, subtitle):
    fig = plt.figure(figsize=(25.6, 14.4), dpi=100); fig.patch.set_facecolor(SURF)
    fig.text(0.06, 0.93, title, fontsize=46, fontweight="bold", color=INK)
    fig.text(0.06, 0.885, subtitle, fontsize=32, color=INK2)
    return fig


# ---- card 1: the two standard bullets (schematic, 1 calibre = 1 unit)
def bullet(ax, x0, y0, nose, body, tail, base, color, label, note):
    """Side profile: nose ogive (pointed), cylinder, optional boat tail down to base diameter."""
    import math
    pts = []
    for i in range(41):                                   # tangent-ogive-like nose, tip at the left
        u = i / 40; pts.append((x0 + nose * u, y0 + 0.5 * math.sin(u * math.pi / 2)))
    pts.append((x0 + nose + body, y0 + 0.5))
    pts.append((x0 + nose + body + tail, y0 + base / 2))
    pts += [(x, 2 * y0 - y) for x, y in reversed(pts)]
    ax.add_patch(Polygon(pts, closed=True, facecolor=color, edgecolor=INK, lw=3))
    ax.text(x0 - 0.35, y0, label, ha="right", va="center", fontsize=60, fontweight="bold", color=INK)
    ax.text(x0, y0 - 0.95, note, ha="left", va="top", fontsize=34, color=INK2)


fig = frame("Two standard bullets", "A ballistic coefficient always compares your bullet to one of them  ·  schematic, not to scale")
ax = fig.add_axes([0.12, 0.08, 0.84, 0.74]); ax.set_xlim(-0.5, 8.5); ax.set_ylim(-3.6, 2.2); ax.set_aspect("equal"); ax.axis("off")
bullet(ax, 0.8, 1.0, 1.4, 1.9, 0.0, 1.0, "#c9c7c0", "G1", "an old standard: short, flat base")
bullet(ax, 0.8, -2.0, 3.1, 1.6, 0.6, 0.75, "#c9c7c0", "G7", "a newer one: long, boat tail, pointed nose — like a modern long range bullet")
fig.savefig("card-shapes.png", facecolor=SURF); plt.close(fig)

# ---- card 2: drag curves
ms = [m / 100 for m in range(90, 241)]
fig = frame("Drag of the 175 grain Scenar-L: measured, and what the coefficients assume",
            "Drag coefficient against speed  ·  G7 scaled to BC 0.253, G1 scaled to BC 0.505  ·  speed of sound = 1")
ax = fig.add_axes([0.08, 0.12, 0.88, 0.70]); ax.set_facecolor(SURF)
# direct labels where the three curves are well apart (label mach, offset): the x axis is inverted
series = [("G1 × 0.505", G1, 0.505, AQUA, (0, (2, 3)), 1.30, -0.022),
          ("G7 × 0.253", G7, 0.253, ORANGE, (0, (8, 4)), 1.12, 0.018),
          ("measured by radar, Lapua", None, None, BLUE, "-", 1.75, 0.022)]
for name, tab, bc, color, ls, lm, dy in series:
    f = (lambda m: interp(RADAR, m)) if tab is None else (lambda m, tab=tab, bc=bc: interp(tab, m) * SD / bc)
    ax.plot(ms, [f(m) for m in ms], color=color, lw=7 if tab is None else 5, ls=ls, solid_capstyle="round")
    ax.text(lm, f(lm) + dy, name, ha="center", va="bottom" if dy > 0 else "top", fontsize=36, fontweight="bold", color=INK)
ax.set_xlim(0.9, 2.4); ax.invert_xaxis()                  # read left to right as the bullet slows down
ax.set_ylim(0.15, 0.45)
ax.set_xlabel("Speed, Mach  (the bullet slows down from left to right)", color=INK2, fontsize=32, labelpad=16)
ax.set_ylabel("Drag coefficient", color=INK2, fontsize=32, labelpad=16)
ax.grid(axis="y", color=GRID, lw=2); ax.set_axisbelow(True)
for s in ("top", "right", "left"):
    ax.spines[s].set_visible(False)
ax.spines["bottom"].set_color(INK2); ax.tick_params(colors=INK2, length=0, pad=12)
fig.savefig("card-curves.png", facecolor=SURF); plt.close(fig)
print("ok")
