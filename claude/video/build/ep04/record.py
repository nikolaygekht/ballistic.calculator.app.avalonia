"""Episode 4 take — spin drift (clean, no wind), aerodynamic jump (+ wind), then Coriolis on a reopened bc2-demo.

Usage: python record.py [--dry]
Opens E:\\ngeht\\Documents\\Ballistic Calculator demo\\bc2-demo.trajectory twice and never saves.
Word times inside clips (comments "(1.4)") come from silencedetect on audio/*.mp3.
"""
import pathlib, sys, time
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

DEMO = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo\bc2-demo.trajectory")
assert DEMO.exists(), DEMO

t = Take(__file__)
W = lambda spec: spec.wrapper_object()
DROP, WINDAGE = 3, 6

mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
traj_m = [c for c in bar.children() if c.window_text() == "Trajectory"][0]
view_m = [c for c in bar.children() if c.window_text() == "View"][0]
vr = view_m.rectangle()
VIEW_XY = center(view_m)
EDITP_XY = (vr.left + 120, vr.bottom + 28)        # View's first item, Edit Parameters
NEAR_MENU = (center(traj_m)[0] + 60, center(traj_m)[1] + 160)
NEAR_VIEW = (VIEW_XY[0] + 40, VIEW_XY[1] + 120)
S = {}


def edit_parameters(t_view, t_item):
    s = time.monotonic()
    at(s, t_view - 0.5, lambda: click_xy(*VIEW_XY, 0.38))
    at(s, t_item - 0.6, lambda: click_xy(*EDITP_XY, 0.45))
    t.dlg.wait("visible", timeout=10)
    return s


def scan(spec):
    """One pass over a panel: {automation id: element}. Lazy child_window specs cost ~0.5 s each."""
    ids = {}
    for e in W(spec).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    return ids


def num(e):
    return [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]


def row(rng):
    """Row cells by the Range text: after a recalculation rows() is NOT in screen order."""
    for r in t.rows():
        c = r.descendants(control_type="Text")
        if c and c[0].window_text() == rng:
            return c
    raise LookupError(rng)


def find_1000(_):
    rest()
    c = row("1,000")
    S["drop"], S["wind"] = center(c[DROP]), center(c[WINDAGE])


# ---------------------------------------------------------------- the take
t.start()
t.voice("v00")

# ---- recap
glide(*NEAR_MENU, 0.8)
t.voice("r01", lambda d: t.open_file("bc2-demo", 0.35))     # "Trajectory, (1.14) Open. The same file…"
find_1000(None)
def a_r02(d):            # "At one thousand yards the drop is three hundred sixty inches, (4.13) and the Windage column is all zeros."
    s = time.monotonic()
    at(s, 0.1, lambda: glide(*S["drop"], 0.9))
    at(s, 4.0, lambda: glide(*S["wind"], 0.7))
    at(s, 8.0, rest)
t.voice("r02", a_r02)

# ---- spin drift
t.voice("s00", lambda d: glide(*NEAR_VIEW, 1.0))            # "The first one is spin drift…"
A = {}
def a_s01(d):            # "View, (0.83) Edit Parameters. (2.38) For spin drift, the program needs…"
    edit_parameters(0.25, 1.0)
    time.sleep(0.5)
    for name in ("Rifle", "Wind", "Parameters"):
        S["tab_" + name] = center(W(t.tab(name)))
    ids = scan(t.by_id("AmmoLibPanel", "AmmoSubPanel"))
    A.update(dia=num(ids["BulletDiameterControl"]), len=num(ids["BulletLengthControl"]))
    point(A["dia"], 0.8)
t.voice("s01", a_s01)
t.voice("s02", lambda d: t.fill(A["dia"], "0.308"))          # "The diameter is zero point three oh eight inches."
t.voice("s03", lambda d: t.fill(A["len"], "1.394"))          # "The length is one point three nine four inches."
R = {}
def a_s04(d):            # "On the Rifle tab, (1.4) the rifling is right hand."
    s = time.monotonic()
    click_xy(*S["tab_Rifle"], 0.5)
    time.sleep(0.3)
    ids = scan(t.by_id("RifleSubPanel"))
    R.update(combo=ids["RiflingDirectionCombo"], twist=num(ids["RiflingStepControl"]))
    at(s, 1.3, lambda: click(R["combo"], 0.5))
    time.sleep(0.3)
    click(W(t.item("Right")), 0.45)
t.voice("s04", a_s04)
def a_s05(d):            # "And the twist is one turn in ten inches. (3.72) OK."
    s = time.monotonic()
    S["ok"] = center(W(t.dlg.child_window(control_type="Button", title="OK")))
    t.fill(R["twist"], "10")
    at(s, 3.25, lambda: click_xy(*S["ok"], 0.4))
t.voice("s05", a_s05)
t.hold(2.5, find_1000)
def a_s06(d):            # "There is still no wind. (2.1) But now, at one thousand yards, … eight inches to the right."
    s = time.monotonic()
    at(s, 2.0, lambda: glide(*S["wind"], 0.9))
t.voice("s06", a_s06)
t.voice("s07", lambda d: glide(*S["drop"], 0.8))             # "And the drop did not change at all…"

# ---- aerodynamic jump
t.voice("j00", lambda d: (time.sleep(1.5), glide(*NEAR_VIEW, 1.2)))   # "The second effect is aerodynamic jump…"
D = {}
def a_j01(d):            # "Edit Parameters, (1.43) and the Wind tab."
    s = edit_parameters(0.3, 0.9)
    at(s, 1.4, lambda: click_xy(*S["tab_Wind"], 0.45))
    time.sleep(0.3)
    ids = scan(t.by_id("WindSubPanel"))
    D.update(vel=num(ids["VelocityControl"]), dir=num(ids["DirectionControl"]))
t.voice("j01", a_j01)
t.voice("j02", lambda d: t.fill(D["vel"], "10", 0.08, 0.4))  # "Ten miles per hour,"
def a_j03(d):            # "from the right. (1.42) OK."
    s = time.monotonic()
    t.fill(D["dir"], "90", 0.07, 0.35)
    at(s, 1.1, lambda: click_xy(*S["ok"], 0.35))
t.voice("j03", a_j03)
t.hold(2.5, find_1000)
t.voice("j04", lambda d: glide(*S["drop"], 0.8))             # "Look at the drop. Three hundred fifty-six and a half…"
def a_j05(d):                                                # "This is aerodynamic jump…"
    time.sleep(2.0); rest()
    S["close"] = [b for b in t.main.wrapper_object().descendants(control_type="Button")
                  if b.element_info.automation_id == "PART_CloseButton"][0]
t.voice("j05", a_j05)

# ---- Coriolis on a fresh copy
def a_c01(d):                                                # "Now I close this window without saving."
    s = time.monotonic()
    at(s, 0.3, lambda: click(S["close"], 0.9))
    at(s, 1.9, lambda: glide(*NEAR_MENU, 0.8))
t.voice("c01", a_c01)
t.voice("c02", lambda d: t.open_file("bc2-demo", 0.35))     # "Trajectory, (1.14) Open, the same file again…"
t.voice("c03", lambda d: (time.sleep(3.0), glide(*NEAR_VIEW, 1.2)))   # "The Earth turns while the bullet flies…"
P = {}
def a_c04(d):            # "Edit Parameters, and the Parameters tab."
    s = edit_parameters(0.3, 0.9)
    at(s, 1.4, lambda: click_xy(*S["tab_Parameters"], 0.45))
    time.sleep(0.3)
    ids = scan(t.by_id("ParametersSubPanel"))
    P.update(chk=ids["CoriolisCheckBox"], az=num(ids["AzimuthControl"]), lat=num(ids["LatitudeControl"]),
             dial=ids["AzimuthIndicator"])
t.voice("c04", a_c04)
t.voice("c05", lambda d: click(P["chk"], 0.7, 0.1))          # "I turn on Calculate Coriolis Effect."
def a_c06(d):            # "The azimuth is ninety degrees. (2.46) We shoot to the east."
    s = time.monotonic()
    t.fill(P["az"], "90")
    at(s, 2.3, lambda: point(P["dial"], 0.7))
t.voice("c06", a_c06)
def a_c07(d):            # "The latitude is forty-five degrees north. (3.38) OK."
    s = time.monotonic()
    t.fill(P["lat"], "45")
    at(s, 2.95, lambda: click_xy(*S["ok"], 0.4))
t.voice("c07", a_c07)
t.hold(2.5, find_1000)
t.voice("c08", lambda d: glide(*S["wind"], 0.8))             # "At one thousand yards, the bullet moves three inches to the right."
t.voice("c09", lambda d: glide(*S["drop"], 0.8))             # "And it hits almost four inches higher…"
t.voice("c10")                                               # "If you shoot to the west…"
t.voice("c11", lambda d: (time.sleep(1.0), rest()))          # "Is it worth it?…"
t.voice("n01")
t.finish()
