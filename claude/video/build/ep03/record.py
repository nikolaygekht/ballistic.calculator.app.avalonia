"""Episode 3 take — February weather, then a 10 mph wind from the right, on bc2-demo (saved by episode 2).

Usage: python record.py [--dry]
Opens E:\\ngeht\\Documents\\Ballistic Calculator demo\\bc2-demo.trajectory and closes it WITHOUT saving.
Word times inside clips (comments "@ 1.0") come from silencedetect on audio/*.mp3.
"""
import pathlib, sys, time
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

DEMO = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo\bc2-demo.trajectory")
assert DEMO.exists(), DEMO

t = Take(__file__)
W = lambda spec: spec.wrapper_object()     # resolve before a timed cue: a lazy spec re-searches on every use
DROP, WINDAGE, WADJ, WCLICKS = 3, 6, 7, 8   # table columns (Range, Velocity, Mach, Drop, Hold, Clicks, Windage, Wnd.Adj., Clicks…)

# The main menu bar (static): View, and its first item Edit Parameters (measured 2026-11-03: the item
# spans View.left+6 … +430, View.bottom+4 … +52).
mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
traj_m = [c for c in bar.children() if c.window_text() == "Trajectory"][0]
view_m = [c for c in bar.children() if c.window_text() == "View"][0]
vr = view_m.rectangle()
VIEW_XY = center(view_m)
EDITP_XY = (vr.left + 120, vr.bottom + 28)
near_menu = (center(traj_m)[0] + 60, center(traj_m)[1] + 160)
S = {}                                     # positions found during the take


def edit_parameters(t_view, t_item):
    """View (at t_view s) → Edit Parameters (at t_item s) inside the current clip; returns when the dialog shows."""
    s = time.monotonic()
    at(s, t_view - 0.5, lambda: click_xy(*VIEW_XY, 0.38))
    at(s, t_item - 0.6, lambda: click_xy(*EDITP_XY, 0.45))
    t.dlg.wait("visible", timeout=10)
    return s


# ---------------------------------------------------------------- the take
t.start()
t.voice("v00")

# ---- recap
glide(*near_menu, 0.8)                                       # silent: closer to the menu, so "Open" lands on the word
t.voice("r01", lambda d: t.open_file("bc2-demo", 0.35))     # "Trajectory, (0.0) Open. (1.0) This is the file…"
last = t.rows()[-1].descendants(control_type="Text")
drop0 = center(last[DROP])
def a_r02(d):                                                # "At one thousand yards the drop is three hundred sixty inches."
    s = time.monotonic()
    at(s, 0.1, lambda: glide(*drop0, 0.9))
    at(s, 5.0, lambda: glide(drop0[0] + 220, drop0[1] - 250, 1.2))    # "…the default weather…": off the numbers
t.voice("r02", a_r02)

# ---- weather
glide(VIEW_XY[0] + 40, VIEW_XY[1] + 120, 0.6)
def a_w01(d):            # "View, (0.0) Edit Parameters. (0.84) This opens the same dialog again. (2.34) …the Weather tab. (4.72)"
    s = edit_parameters(0.25, 1.0)
    time.sleep(0.6)
    weather = W(t.tab("Weather"))
    S["wind_tab"] = center(W(t.tab("Wind")))
    at(s, 5.0, lambda: click(weather, 0.6))
t.voice("w01", a_w01)
A = "AtmosphereSubPanel"
F = {}
def a_w02(d):                                                # "Let's take a cold day in February."
    F.update(alt=W(t.field(A, "AltitudeControl")), prs=W(t.field(A, "PressureControl")),
             tmp=W(t.field(A, "TemperatureControl")), hum=W(t.by_id(A, "HumidityTextBox")),
             ok=W(t.dlg.child_window(control_type="Button", title="OK")))
    S["ok"] = center(F["ok"])
    point(F["alt"], 0.7)
t.voice("w02", a_w02)
t.voice("w03", lambda d: t.fill(F["alt"], "1000"))           # "The range is one thousand feet above sea level."
t.voice("w04", lambda d: t.fill(F["prs"], "28.9"))           # "The pressure is twenty-eight point nine inches of mercury."
def a_w05(d):            # "This must be the pressure where you stand. (2.6) A weather report… sea level. Don't put that number here."
    s = time.monotonic()
    at(s, 0.1, lambda: point(F["prs"], 0.6, 0.25))
    at(s, 2.6, lambda: point(F["prs"], 1.2, 0.75))
t.voice("w05", a_w05)
t.voice("w06", lambda d: t.fill(F["tmp"], "40"))             # "The temperature is forty degrees."
def a_w07(d):                                                # "And the humidity is sixty percent. (2.68) OK."
    s = time.monotonic()
    t.fill(F["hum"], "60", 0.09, 0.4)
    at(s, 2.2, lambda: click_xy(*S["ok"], 0.35))
t.voice("w07", a_w07)
def row(rng):
    """The table row whose Range cell reads rng. After a recalculation the DataItem list is NOT in
    screen order (rows()[-1] gave the 0 yd row, rows()[6] the 400 yd row), so never index it."""
    for r in t.rows():
        c = r.descendants(control_type="Text")
        if c and c[0].window_text() == rng:
            return c
    raise LookupError(rng)
def find_last_row(_):
    rest()
    S["drop"] = center(row("1,000")[DROP])
t.hold(2.5, find_last_row)
def a_w08(d):            # "At one thousand yards the drop is now three hundred fifty-nine inches. (4.67) Only about one inch less."
    glide(*S["drop"], 0.9)
t.voice("w08", a_w08)
t.voice("w09", lambda d: (time.sleep(1.5), rest()))           # "Why so small? …" — the cursor leaves the table

# ---- wind
glide(VIEW_XY[0] + 40, VIEW_XY[1] + 120, 0.6)
D = {}
def a_d01(d):            # "Now the wind. (0.95) Edit Parameters again, (1.54) and the Wind tab. (3.07)"
    s = edit_parameters(0.9, 1.75)
    at(s, 2.7, lambda: click_xy(*S["wind_tab"], 0.5))
    time.sleep(0.3); q = time.monotonic()
    # one scan of the panel: five lazy child_window specs took ~2.5 s and overran the clip
    ids = {}
    for e in W(t.by_id("WindSubPanel")).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    num = lambda e: [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]
    D.update(vel=num(ids["VelocityControl"]), dir=num(ids["DirectionControl"]), dial=ids["WindIndicator"],
             dist=ids["MaxDistanceCheckBox"], add=ids["AddButton"])
    print(f"         wind lookups {time.monotonic() - q:.2f} s", flush=True)
t.voice("d01", a_d01)
t.voice("d02", lambda d: t.fill(D["vel"], "10", 0.09, 0.4))   # "Ten miles per hour."
def a_d03(d):            # "It blows from the right. (1.89) On this dial, zero degrees is a wind from behind you, … from the right."
    s = time.monotonic()
    t.fill(D["dir"], "90", 0.09, 0.4)
    at(s, 1.8, lambda: point(D["dial"], 0.7))
t.voice("d03", a_d03)
def a_d04(d):            # "We leave the distance empty, (2.0) so this wind… (4.74) If the wind is different… add more winds here. (9.51) OK."
    s = time.monotonic()
    at(s, 0.1, lambda: point(D["dist"], 0.7, 0.2))
    at(s, 6.8, lambda: point(D["add"], 0.7))
    at(s, 9.0, lambda: click_xy(*S["ok"], 0.4))
t.voice("d04", a_d04)
def find_wind_cells(_):
    rest()
    r600, r1000 = row("600"), row("1,000")
    S.update(w600=center(r600[WINDAGE]), w1000=center(r1000[WINDAGE]),
             wadj=center(r1000[WADJ]), wclk=center(r1000[WCLICKS]),
             whead=center(t.tv_find("HeaderItem", "Windage (in)")))
t.hold(2.5, find_wind_cells)
def a_d05(d):            # "Before, the Windage column was all zeros. (3.34) Now, at six hundred yards… (8.38) At one thousand yards…"
    s = time.monotonic()
    at(s, 0.1, lambda: glide(*S["whead"], 0.9))
    at(s, 3.3, lambda: glide(*S["w600"], 0.9))
    at(s, 8.3, lambda: glide(*S["w1000"], 0.7))
t.voice("d05", a_d05)
def a_d06(d):            # "To correct for it, you need two point nine milliradians. (3.8) That's twenty-nine clicks."
    s = time.monotonic()
    at(s, 0.6, lambda: glide(*S["wadj"], 0.7))
    at(s, 3.6, lambda: glide(*S["wclk"], 0.5))
t.voice("d06", a_d06)
t.voice("d07", lambda d: (time.sleep(2.0), rest()))           # "That's why wind is the hardest part…"

# ---- close without saving
close_btn = [b for b in t.main.wrapper_object().descendants(control_type="Button")
             if b.element_info.automation_id == "PART_CloseButton"][0]
def a_c01(d):                                                # "Now I close this window (2.4) without saving."
    s = time.monotonic()
    at(s, 0.4, lambda: click(close_btn, 0.9))
    at(s, 3.0, rest)
t.voice("c01", a_c01)
t.voice("n01")
t.finish()
