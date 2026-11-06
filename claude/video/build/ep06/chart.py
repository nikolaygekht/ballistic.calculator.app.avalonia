"""The card for episode 6: the G7 BC the 175 gr Scenar-L really has along its flight.
BC(M) = SD * Cd_G7(M) / Cd_bullet(M), with Cd_bullet from Lapua's radar .drg and Cd_G7 from the library.
Mach along the flight comes from the app's own table for this load (ep06/read_numbers.py, radar run).
Usage: python chart.py  ->  bc-along-flight.png (2560x1440)"""
import re
import matplotlib; matplotlib.use("Agg")
import matplotlib.pyplot as plt

G7_CS = r"D:\develop\components\BusinessSpecificComponents\BallisticCalculator.Net\BallisticCalculator\Drag\G7DragTable.cs"
DRG = r"E:\ngeht\Documents\Ballistic Calculator demo\30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg"
g7 = [(float(a), float(b)) for a, b in re.findall(r"DragTableDataPoint\(([\d.]+),\s*([\d.]+)\)", open(G7_CS).read())]
drg = [(float(m), float(cd)) for cd, m in (r.split() for r in open(DRG).read().splitlines()[1:] if r.strip())]


def interp(t, x):
    for (x0, y0), (x1, y1) in zip(t, t[1:]):
        if x0 <= x <= x1:
            return y0 + (y1 - y0) * (x - x0) / (x1 - x0)


sd = (11.30 / 1000 * 2.20462) / (7.83 / 25.4) ** 2          # lb/in^2, from the .drg header
mach_by_range = [(0, 2.33), (100, 2.17), (200, 2.02), (300, 1.88), (400, 1.74), (500, 1.60),
                 (600, 1.48), (700, 1.35), (800, 1.23), (900, 1.12), (1000, 1.01)]
xs = list(range(0, 1001, 5))
ys = [sd * interp(g7, m) / interp(drg, m) for m in (interp(mach_by_range, x) for x in xs)]

SURF, INK, INK2, GRID, BLUE = "#fcfcfb", "#0b0b0b", "#52514e", "#e4e3df", "#2a78d6"
plt.rcParams.update({"font.family": "Segoe UI", "font.size": 30})
fig, ax = plt.subplots(figsize=(25.6, 14.4), dpi=100)
fig.patch.set_facecolor(SURF); ax.set_facecolor(SURF)
ax.axhline(0.253, color=INK2, lw=3, ls=(0, (6, 5)))
ax.text(20, 0.2540, "the one number from the box: 0.253", ha="left", va="bottom", color=INK2, fontsize=32)
ax.plot(xs, ys, color=BLUE, lw=7, solid_capstyle="round")
lo = min(range(len(xs)), key=lambda i: ys[i]); hi = len(xs) - 1
for i, ha, va, dy in ((lo, "center", "top", -0.0025), (hi, "right", "bottom", 0.002)):
    ax.plot(xs[i], ys[i], "o", ms=16, color=BLUE, mec=SURF, mew=4)
    ax.text(xs[i], ys[i] + dy, f"{ys[i]:.3f}", ha=ha, va=va, color=INK, fontsize=36, fontweight="bold")
ax.set_xlim(0, 1010); ax.set_ylim(0.225, 0.295)
ax.set_xlabel("Distance, yards", color=INK2, fontsize=32, labelpad=16)
ax.set_ylabel("G7 ballistic coefficient", color=INK2, fontsize=32, labelpad=16)
ax.grid(axis="y", color=GRID, lw=2); ax.set_axisbelow(True)
for s in ("top", "right", "left"):
    ax.spines[s].set_visible(False)
ax.spines["bottom"].set_color(INK2); ax.tick_params(colors=INK2, length=0, pad=12)
fig.text(0.06, 0.93, "The real G7 BC of the 175 grain Scenar-L, along its flight", fontsize=46, fontweight="bold", color=INK)
fig.text(0.06, 0.885, "Computed from Lapua's radar drag curve · 2600 ft/s · standard air", fontsize=32, color=INK2)
fig.subplots_adjust(left=0.08, right=0.96, top=0.84, bottom=0.12)
fig.savefig("bc-along-flight.png", facecolor=SURF)
print("start", round(ys[0], 3), "min at", xs[lo], "yd", round(ys[lo], 3), "end", round(ys[-1], 3))
