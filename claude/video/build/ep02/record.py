"""Episode 2 take — the imperial baseline, Summary, Mil-Dot reticle, and Save As bc2-demo.

Usage: python record.py [--dry]
Saves E:\\ngeht\\Documents\\Ballistic Calculator demo\\bc2-demo.trajectory (episodes 3-6 open it).
"""
import pathlib, sys, time
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

DEMO_DIR = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo")
DEMO_DIR.mkdir(exist_ok=True)
for f in DEMO_DIR.glob("bc2-demo*.trajectory"):
    f.unlink()

t = Take(__file__)
save_as = t.save_as

# ---------------------------------------------------------------- pre-roll (not recorded)
# Build episode 1's metric trajectory and save it into DEMO_DIR (the file dialogs then open in that
# folder), then restart: the take starts from an empty app and opens the file.
t.new_trajectory("Metric", 0.2)
t.fill_shot("11.3", "0.253", "G7", "790", "PSO-1 Optics", "300", "1000", "100", pause=0.01, secs=0.15)
save_as(str(DEMO_DIR / "bc2-demo-metric"), slow=False)
t.restart()
rest()

# ---------------------------------------------------------------- the take
t.start()
t.voice("v00")
t.open_file("bc2-demo-metric")
rest()
t.hold(2.5)
t.voice("r01")

# ---- the same load in imperial, narrated while it is typed in
A = ("AmmoLibPanel", "AmmoSubPanel")
W = lambda spec: spec.wrapper_object()   # resolve UIA lookups BEFORE a timed cue: a lazy spec re-searches (~1 s) on every use
F = {}
def a_r02(d):                                                  # "Now the same load in imperial…"
    t.new_trajectory("Imperial", 0.5)
    F.update(weight=W(t.field(*A, "WeightControl")), bc=W(t.field(*A, "BCControl")),
             table=W(t.by_id(*A, "BCControl", "TablePart")), mv=W(t.field(*A, "MuzzleVelocityControl")),
             rifle=W(t.tab("Rifle")), zero=W(t.tab("Zero")), par=W(t.tab("Parameters")))
t.voice("r02", a_r02)
bc, table, mv = F["bc"], F["table"], F["mv"]
t.voice("v02g", lambda d: t.fill(F["weight"], "175"))
click(bc, 0.45)                                                 # into the BC field before the voice
def a02v(d):   # "The same coefficient," (BC)  "and two thousand six hundred feet per second." (MV)
    type_text("0.253", 0.06); time.sleep(0.2)
    t.fill(mv, "2600", 0.07, 0.35)
    click(table, 0.35); time.sleep(0.3); click(t.item("G7"), 0.35)   # the slow lookup goes last, unnarrated
t.voice("v02v", a02v)
click(F["rifle"], 0.5); time.sleep(0.3)
height = W(t.field("RifleSubPanel", "SightHeightControl"))
clicks = W(t.field("RifleSubPanel", "VerticalClickControl"))
click(t.by_id("RifleSubPanel", "SightPresetCombo"), 0.5); time.sleep(0.4)
preset = W(t.item("Standard Optics (High, Mil)"))
def a02s(d):
    s = time.monotonic()
    click(preset, 0.5)
    at(s, d * 0.36, lambda: point(height, 0.6))
    at(s, d * 0.62, lambda: point(clicks, 0.6))
t.voice("v02s", a02s)
click(F["zero"], 0.5); time.sleep(0.3)
zero_dist = W(t.field("ZeroSubPanel", "ZeroDistanceControl"))
t.voice("v02z", lambda d: t.fill(zero_dist, "300"))
click(F["par"], 0.45); time.sleep(0.3)
t.fill(t.field("ParametersSubPanel", "MaxRangeControl"), "1000")
t.fill(t.field("ParametersSubPanel", "StepControl"), "100")
t.ok_and_accept_weather()
rest()
MACH, DROP, HOLD = 2, 3, 4
last = t.rows()[-1].descendants(control_type="Text")                  # only the last row is used
drop_xy, hold_xy, mach_xy = [center(last[DROP])], [center(last[HOLD])], [center(last[MACH])]
t.hold(2.5)

def a02d(d):
    s = time.monotonic()
    at(s, d * 0.08, lambda: glide(*drop_xy[-1], 0.9))
    at(s, d * 0.67, lambda: glide(*hold_xy[-1], 0.6))      # "The hold is ten milliradians"
t.voice("v02d", a02d)
t.voice("v03", lambda d: glide(*mach_xy[-1], 0.8))

# ---- Summary
# Tabs are clicked ON the voice ("The Summary view…", "And here is the third view…"); positions inside
# a tab can only be looked up once it is open, so the lookups run inside the action, before the first
# timed point (at() simply runs late rather than skipping, if a lookup is slow).
TV = t.tv()                                                     # cached: t.tv() scans the whole main window
summary_tab, reticle_tab = t.tv_find("TabItem", "Summary", tv=TV), t.tv_find("TabItem", "Reticle", tv=TV)
def a04(d):
    s = time.monotonic()
    click(summary_tab, 0.7); time.sleep(0.5)
    sv = {k: center(t.tv_find("Text", auto_id=k, tv=TV), 0.15) for k in ("DeadZoneValue", "NearZeroValue", "FarZeroValue", "SubsonicValue")}
    at(s, d * 0.28, lambda: glide(*sv["DeadZoneValue"], 0.7))
    at(s, d * 0.45, lambda: glide(*sv["NearZeroValue"], 0.5))
    at(s, d * 0.55, lambda: glide(*sv["FarZeroValue"], 0.4))
    at(s, d * 0.75, lambda: glide(*sv["SubsonicValue"], 0.6))
t.voice("v04", a04)

# ---- Reticle: Mil-Dot, Far BDC
# The tab is clicked ~2 s BEFORE the voice: opening it recalculates the reticle trajectory and every UIA
# lookup blocks until that is done (measured: 2-3 s). Mouse clicks do not block, so Mil-Dot and Far BDC
# land exactly on their words once their positions are known.
click(reticle_tab, 0.7)
mildot = t.tv_find("Button", "Mil-Dot", tv=TV); far = t.tv_find("RadioButton", "Far BDC", tv=TV)
canvas = t.tv_find("Custom", auto_id="ReticleCanvas", tv=TV).rectangle()
cx, cy = (canvas.left + canvas.right) // 2, (canvas.top + canvas.bottom) // 2
mrad = min(canvas.right - canvas.left, canvas.bottom - canvas.top) / 2 / 6     # 12 mrad across the field
dots = [(cx, int(cy + k * mrad)) for k in (1, 2, 3, 4)]
post = (cx, int(cy + 5.5 * mrad))
mildot_xy, far_xy = center(mildot), center(far, 0.1)
def a05(d):
    s = time.monotonic()                                           # "And here is the third view: your scope."
    at(s, 2.6, lambda: click_xy(*mildot_xy, 0.6))                  # "I click Mil-Dot" (3.25 s)
    at(s, 5.6, lambda: click_xy(*far_xy, 0.6))                     # "and choose Far B D C" (6.30 s)
    at(s, 8.5, lambda: glide(dots[0][0] + int(mrad * 1.4), dots[0][1], 0.9))   # "The blue labels" (9.10 s)
t.voice("v05", a05)
def a06(d):
    s = time.monotonic()
    for k, f in enumerate((0.02, 0.17, 0.28, 0.39)):
        at(s, d * f, lambda k=k: glide(*dots[k], 0.7))
t.voice("v06", a06)
t.voice("v07", lambda d: glide(*post, 0.8))

# ---- save it
t.voice("v08a", lambda d: rest())
title_xy = (MON[0] + 1280, MON[1] + 22)
def a08b(d):
    # "Trajectory," (menu) "Save As." (item) "This saves the whole shot…" (name typed, saved)
    s = time.monotonic()
    save_as("bc2-demo")
    at(s, d * 0.57, lambda: glide(*title_xy, 0.8))      # "Every next episode opens this file…"
t.voice("v08b", a08b)
t.voice("v09")
t.finish()
print("saved:", sorted(p.name for p in DEMO_DIR.glob("*.trajectory")))
