"""Episode 7 take — everything at once on bc2-demo-radar: twist, Coriolis, July air with a February zero, wind;
then the scope: Mil-Dot Far BDC relabelled, Near BDC, a 20 in plate at 600 yd, and the shipped VCOG 7.62.

Usage: python record.py [--dry]
Opens E:\\ngeht\\Documents\\Ballistic Calculator demo\\bc2-demo-radar.trajectory and never saves.
Word times inside clips (comments "(1.4)") come from silencedetect on audio/*.mp3 (= where the next word starts).
q06b / q06c get full-frame cards in post (overlay_cards.py); the take just holds the reticle there.
"""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *
from PIL import ImageGrab
HERE = pathlib.Path(__file__).resolve().parent

RADAR = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo\bc2-demo-radar.trajectory")
assert RADAR.exists(), RADAR

# windows that would cover the recording monitor (OBS, the user's Notepad) go down first
for w in Desktop(backend="uia").windows():
    r = w.rectangle()
    if w.window_text().startswith("OBS") or "Notepad" in w.window_text():
        if r.left < MON[0] + MON[2] and r.right > MON[0] and r.top < MON[1] + MON[3]:
            print("minimizing", w.window_text()); w.minimize()

t = Take(__file__)
W = lambda spec: spec.wrapper_object()
DROP, WIND = 3, 6

mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
menu = {c.window_text(): c for c in bar.children()}
traj_m, view_m = menu["Trajectory"], menu["View"]
vr = view_m.rectangle()
VIEW_XY, TRAJ_XY = center(view_m), center(traj_m)
EDITP_XY = (vr.left + 120, vr.bottom + 28)        # View's first item, Edit Parameters
NEAR_MENU = (TRAJ_XY[0] + 60, TRAJ_XY[1] + 160)
NEAR_VIEW = (VIEW_XY[0] + 40, VIEW_XY[1] + 120)
S = {}


def menu_item_xy(title):
    return center(t.main.child_window(control_type="MenuItem", title=title, found_index=0).wrapper_object())


def open_radar():
    """Trajectory -> Open -> bc2-demo-radar -> Open (~5 s)."""
    click_xy(*TRAJ_XY, 0.35); time.sleep(0.15)
    click_xy(*S["Open"], 0.3)
    d = t.main.child_window(class_name="#32770", title="Open Trajectory"); d.wait("visible", timeout=10)
    f = d.child_window(title_re=r"bc2-demo-radar(\.trajectory)?$", control_type="ListItem", found_index=0)
    if not f.exists(timeout=2):
        # Trajectory -> Open has no start folder of its own: it opens wherever the last native dialog was
        # (Load reticle leaves it in data\reticle). Only the pre-roll should ever get here.
        print("         (open dialog was elsewhere; typing the demo folder)", flush=True)
        click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.3)
        type_text(str(RADAR.parent), 0.01); keyboard.send_keys("{ENTER}")
    f.wait("visible", timeout=5)
    click(f.wrapper_object(), 0.35, 0.15)
    click(d.child_window(auto_id="1", control_type="Button").wrapper_object(), 0.35)
    for _ in range(50):
        if t.main.wrapper_object().descendants(control_type="DataItem"):
            break
        time.sleep(0.1)


def edit_parameters(t_view, t_item):
    s = time.monotonic()
    at(s, t_view - 0.5, lambda: click_xy(*VIEW_XY, 0.38))
    at(s, t_item - 0.6, lambda: click_xy(*EDITP_XY, 0.45))
    t.dlg.wait("visible", timeout=10)
    return s


def scan(spec):
    ids = {}
    for e in W(spec).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    return ids


def num(e):
    return [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]


def rows(*ranges):
    out = {}
    for r in t.rows():
        c = r.descendants(control_type="Text")
        if c and c[0].window_text() in ranges:
            out[c[0].window_text()] = c
    return out


def fill(el, text, pause=0.09, secs=0.45):
    """click + select the old text + type. Not bcrec's Ctrl+A: on the Weather tab Ctrl+A sometimes selects
    nothing and the digits land in front of the old value (1000 -> "10000", 28.9 -> "28929.95") — the
    intermittent "Cannot calculate this shot" of eps. 5/7. End, Shift+Home selects the whole one-line box."""
    click(el, secs); keyboard.send_keys("{END}+{HOME}", pause=0.03); time.sleep(0.08)
    keyboard.send_keys(text, pause=pause, with_spaces=True); time.sleep(0.25)


def fc(label, el, text, pause=0.09, secs=0.45):
    """fill, then read this one field back at once — spreads the read-backs over the clip, so OK lands on its word."""
    glide(*center(el), min(secs, 0.3)); time.sleep(0.05); mouse.click(coords=pos()); time.sleep(0.12)
    keyboard.send_keys("{END}+{HOME}", pause=0.03); time.sleep(0.05)
    keyboard.send_keys(text, pause=min(pause, 0.06), with_spaces=True); time.sleep(0.1)
    check(label, [("", el, text)])


def calc_error():
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.3):
        print("STOPPED: 'Cannot calculate this shot' after OK; left open.", flush=True); os._exit(1)


def check(label, fields):
    """Read typed fields back before OK (while their tab is shown); on a mismatch stop, dialog left open."""
    bad = False
    for name, e, want in fields:
        v = e.get_value(); ok_ = v.replace(",", "") == want; bad |= not ok_
        print(f"         {label} {name:4} = {v!r}" + ("" if ok_ else f"   <-- want {want}"), flush=True)
    if bad:
        print("STOPPED before OK: a field is wrong; the dialog is left open.", flush=True); os._exit(1)


def find_rows(_):
    calc_error(); rest()
    r = rows("300", "1,000")
    S.update(d1000=center(r["1,000"][DROP]), w1000=center(r["1,000"][WIND]), d300=center(r["300"][DROP]))
    print("         1000 yd drop / windage:", r["1,000"][DROP].window_text(), r["1,000"][WIND].window_text(),
          "  300 yd drop:", r["300"][DROP].window_text(), flush=True)


def close_window():
    click([b for b in t.main.wrapper_object().descendants(control_type="Button")
           if b.element_info.automation_id == "PART_CloseButton"][0], 0.2); time.sleep(0.8)


# ---- pre-roll (not recorded): menu items, dialog tabs / OK, and every position on the Reticle tab
click_xy(*TRAJ_XY, 0.2); time.sleep(0.5)
S["Open"] = menu_item_xy("Open")
keyboard.send_keys("{ESC}{ESC}"); time.sleep(0.4)
open_radar()
click_xy(*VIEW_XY, 0.2); time.sleep(0.4); click_xy(*EDITP_XY, 0.2)
t.dlg.wait("visible", timeout=10); time.sleep(0.8)
S["tabs"] = {e.window_text(): center(e) for e in W(t.dlg).descendants(control_type="TabItem")}
S["ok"] = center(W(t.dlg.child_window(control_type="Button", title="OK")))
click(t.dlg.child_window(control_type="Button", title="Cancel"), 0.2); time.sleep(0.5)
TV = t.tv()
S["tab_table"] = center(t.tv_find("TabItem", "Table", tv=TV))
S["tab_reticle"] = center(t.tv_find("TabItem", "Reticle", tv=TV))
click_xy(*S["tab_reticle"], 0.2); time.sleep(3.0)
S["mildot"] = center(t.tv_find("Button", "Mil-Dot", tv=TV))
S["load"] = center([b for b in TV.descendants(control_type="Button") if b.element_info.automation_id == "LoadReticleButton"][0])
for name in ("Far BDC", "Near BDC", "Target"):
    S[name] = center(t.tv_find("RadioButton", name, tv=TV), 0.1)
canvas = t.tv_find("Custom", auto_id="ReticleCanvas", tv=TV).rectangle()
CANVAS = (canvas.left, canvas.top, canvas.right, canvas.bottom)
boxes = [e for e in TV.descendants() if e.element_info.automation_id == "PART_TextBox"]
S["size_w"], S["size_h"] = center(boxes[0]), center(boxes[1])
tid = [e for e in TV.descendants() if e.element_info.automation_id == "TargetDistanceControl"][0]
S["tdist"] = center(num(tid))
click_xy(*S["tab_table"], 0.2); time.sleep(1.0)
# A window opened after closing another one comes up cascaded (smaller, offset), so every position cached on
# this maximized window would be wrong. A fresh app opens the take's window maximized, exactly like this one.
t.restart(); time.sleep(1.0)
print("cached:", S, "canvas:", CANVAS)

cx, cy = (CANVAS[0] + CANVAS[2]) // 2, (CANVAS[1] + CANVAS[3]) // 2
MRAD = min(CANVAS[2] - CANVAS[0], CANVAS[3] - CANVAS[1]) / 2 / 6          # Mil-Dot: 12 mrad across the field
DOTS = [(cx, int(cy + k * MRAD)) for k in (1, 2, 3, 4)]
FAR_LABEL = [(x + int(MRAD * 1.6), y) for x, y in DOTS]                  # Far BDC labels sit right of the dots, too
NEAR_LABEL = [(x + int(MRAD * 1.4), y) for x, y in DOTS]                 # Near BDC labels sit right of the dots
# VCOG-16-762-175gr: 96 x 100 MOA, zero at (48, 40) MOA from the top-left, so it is fitted by height
MOA = (CANVAS[3] - CANVAS[1]) / 100.0
VC = (cx, int(CANVAS[1] + 40 * MOA))
VCOG = {k: int(VC[1] + y * MOA) for k, y in (("4", 9.74), ("10", 51.88))}
rest()


def red_box():
    """Centre of the red target box on the canvas (Mil-Dot is all black, so red = the box)."""
    from PIL import ImageChops
    im = ImageGrab.grab(bbox=CANVAS, all_screens=True).convert("RGB")
    r, g, b = im.split()
    m = ImageChops.multiply(ImageChops.multiply(r.point(lambda v: 255 if v > 170 else 0),
                                                g.point(lambda v: 255 if v < 100 else 0)),
                            b.point(lambda v: 255 if v < 100 else 0))
    bb = m.getbbox()
    if t.dry:
        im.save(HERE / "dry-target.png")
    if not bb:
        print("         ! no red target box found", flush=True); return cx, cy
    return CANVAS[0] + (bb[0] + bb[2]) // 2, CANVAS[1] + (bb[1] + bb[3]) // 2


# ---------------------------------------------------------------- the take
t.start()
t.voice("v00")

# ---- recap
glide(*NEAR_MENU, 0.8)
t.voice("r01", lambda d: open_radar())                       # "Trajectory, (1.03) Open. (2.09) This time the file from episode six…"
def a_r02(d):            # "At one thousand yards the drop is 371 inches, (4.51) and there is no windage at all."
    s = time.monotonic()
    r = rows("1,000")["1,000"]
    S.update(d1000=center(r[DROP]), w1000=center(r[WIND]))
    at(s, 0.1, lambda: glide(*S["d1000"], 0.9))
    at(s, 4.4, lambda: glide(*S["w1000"], 0.7))
t.voice("r02", a_r02)
def a_r03(d):            # "And the scope: (1.33) the Reticle tab, (2.61) Mil-Dot, (3.40) Far B D C. (5.18) The dots below the centre are
    s = time.monotonic()  #  405, 496, 578 and 651 yards. (12.68) Remember these numbers."
    at(s, 0.9, lambda: click_xy(*S["tab_reticle"], 0.4))
    at(s, 2.2, lambda: click_xy(*S["mildot"], 0.4))
    at(s, 3.1, lambda: click_xy(*S["Far BDC"], 0.35))
    for k, tt in enumerate((6.9, 8.3, 9.6, 10.9)):
        at(s, tt, lambda k=k: glide(*FAR_LABEL[k], 0.5))
    at(s, 12.9, rest)
t.voice("r03", a_r03)
def a_r04(d):            # "Until now we added one effect at a time. (3.21) Today we add them on top of each other…"
    s = time.monotonic()
    at(s, 0.4, lambda: click_xy(*S["tab_table"], 0.6))
    at(s, d - 1.6, lambda: glide(*NEAR_VIEW, 1.0))
t.voice("r04", a_r04)

# ---- step 1: spin drift
R = {}
def a_e01(d):            # "View, (0.83) Edit Parameters, (2.13) the Rifle tab."
    s = edit_parameters(0.3, 1.0)
    at(s, 2.0, lambda: click_xy(*S["tabs"]["Rifle"], 0.35))
    time.sleep(0.3)
    ids = scan(t.by_id("RifleSubPanel"))
    R.update(dir=ids["RiflingDirectionCombo"], step=num(ids["RiflingStepControl"]))
t.voice("e01", a_e01)
def a_e02(d):            # "The drag file already gave us the diameter and the length… (4.34) So we only need the rifling: (6.69) right hand,"
    s = time.monotonic()
    at(s, 4.3, lambda: point(R["dir"], 0.8))
    at(s, 5.8, lambda: click(R["dir"], 0.3))
    time.sleep(0.3)
    item = W(t.item("Right"))
    at(s, 6.7, lambda: click(item, 0.4))
t.voice("e02", a_e02)
def a_e03(d):            # "one turn in ten inches. (2.40) OK."
    s = time.monotonic()
    fill(R["step"], "10", 0.09, 0.4)
    check("rifle", [("step", R["step"], "10")])
    at(s, 2.2, lambda: click_xy(*S["ok"], 0.35))
t.voice("e03", a_e03)
t.hold(2.5, find_rows)
def a_e04(d):            # "Spin drift: (1.16) eight inches to the right. (2.91) The drop did not change."
    s = time.monotonic()
    at(s, 0.6, lambda: glide(*S["w1000"], 0.7))
    at(s, 2.8, lambda: glide(*S["d1000"], 0.6))
t.voice("e04", a_e04)

# ---- step 2: Coriolis
P = {}
def a_e05(d):            # "Edit Parameters, (1.47) the Parameters tab. (3.12) Calculate Coriolis Effect,"
    s = edit_parameters(0.5, 1.0)
    at(s, 1.4, lambda: click_xy(*S["tabs"]["Parameters"], 0.35))
    time.sleep(0.3)
    ids = scan(t.by_id("ParametersSubPanel"))
    P.update(cor=ids["CoriolisCheckBox"], az=num(ids["AzimuthControl"]), lat=num(ids["LatitudeControl"]))
    at(s, 3.1, lambda: click(P["cor"], 0.45, 0.1))
t.voice("e05", a_e05)
t.voice("e06", lambda d: fill(P["az"], "90", 0.09, 0.4))   # "we shoot to the east,"
def a_e07(d):            # "at forty-five degrees north. (2.28) OK."
    s = time.monotonic()
    fill(P["lat"], "45", 0.09, 0.4)
    check("coriolis", [("az", P["az"], "90"), ("lat", P["lat"], "45")])
    at(s, 2.1, lambda: click_xy(*S["ok"], 0.35))
t.voice("e07", a_e07)
t.hold(2.5, find_rows)
def a_e08(d):            # "The bullet hits almost four inches higher, (2.91) and the drift to the right grows to 11.5 inches."
    s = time.monotonic()
    at(s, 0.1, lambda: glide(*S["d1000"], 0.8))
    at(s, 2.8, lambda: glide(*S["w1000"], 0.7))
    at(s, d - 1.0, lambda: glide(*NEAR_VIEW, 1.0))
t.voice("e08", a_e08)

# ---- step 3: the weather from episode five
A = {}
def a_e09(d):            # "Now the weather from episode five. (2.58) Edit Parameters, (3.87) the Weather tab. (5.15) July:
    s = edit_parameters(2.7, 3.4)  # (6.12) one thousand feet, (7.37) 28.9, (8.87) 100 degrees, (10.21) ninety percent."
    at(s, 3.8, lambda: click_xy(*S["tabs"]["Weather"], 0.35))
    time.sleep(0.3)
    ids = scan(t.by_id("AtmosphereSubPanel"))
    A.update(alt=num(ids["AltitudeControl"]), prs=num(ids["PressureControl"]),
             tmp=num(ids["TemperatureControl"]), hum=ids["HumidityTextBox"])
    at(s, 5.3, lambda: fc("july alt", A["alt"], "1000", 0.07, 0.35))
    at(s, 7.25, lambda: fc("july prs", A["prs"], "28.9", 0.07, 0.35))
    at(s, 8.7, lambda: fc("july tmp", A["tmp"], "100", 0.07, 0.35))
    at(s, 10.05, lambda: fc("july hum", A["hum"], "90", 0.07, 0.35))
t.voice("e09", a_e09)
Z = {}
def a_e10(d):            # "And the Zero tab: (1.66) the rifle was zeroed in February."
    s = time.monotonic()
    at(s, 0.3, lambda: click_xy(*S["tabs"]["Zero"], 0.4))
    time.sleep(0.4)
    zr = W(t.by_id("ZeroSubPanel")).rectangle()
    zc = ((zr.left + zr.right) // 2, (zr.top + zr.bottom) // 2)
    glide(*zc, 0.4)
    for _ in range(3):                                       # the tab scrolls; the zero air is below the fold
        mouse.scroll(coords=zc, wheel_dist=-5); time.sleep(0.15)
    time.sleep(0.3)
    ids = scan(t.by_id("ZeroSubPanel", "ZeroAtmosphereSubPanel"))
    Z.update(chk=ids["EnableCheckBox"], alt=num(ids["AltitudeControl"]), prs=num(ids["PressureControl"]),
             tmp=num(ids["TemperatureControl"]), hum=ids["HumidityTextBox"])
    at(s, 2.6, lambda: click(Z["chk"], 0.45, 0.1))
t.voice("e10", a_e10)
def a_e11(d):            # "One thousand feet, (1.51) 28.9, (3.07) forty degrees, (4.16) forty percent. (5.42) OK."
    s = time.monotonic()
    fc("feb alt", Z["alt"], "1000", 0.06, 0.3)
    at(s, 1.4, lambda: fc("feb prs", Z["prs"], "28.9", 0.06, 0.3))
    at(s, 2.95, lambda: fc("feb tmp", Z["tmp"], "40", 0.06, 0.3))
    at(s, 4.05, lambda: fc("feb hum", Z["hum"], "40", 0.06, 0.3))
    at(s, 5.3, lambda: click_xy(*S["ok"], 0.35))
t.voice("e11", a_e11)
t.hold(2.5, find_rows)
t.voice("e12", lambda d: (glide(*S["d1000"], 0.8), time.sleep(d - 2.2), glide(*NEAR_VIEW, 1.0)))   # "333 inches: 34 less than before."

# ---- step 4: the wind, and aerodynamic jump
WD = {}
def a_e13(d):            # "And last, the wind. (0.95) Edit Parameters, (2.09) the Wind tab. (3.41) Ten miles per hour,
    s = edit_parameters(1.4, 2.0)   # (4.77) from the right. (6.13) OK."
    at(s, 2.05, lambda: click_xy(*S["tabs"]["Wind"], 0.35))
    time.sleep(0.3)
    ids = scan(t.by_id("WindSubPanel"))
    WD.update(vel=num(ids["VelocityControl"]), dir=num(ids["DirectionControl"]))
    at(s, 3.0, lambda: fc("wind vel", WD["vel"], "10", 0.09, 0.4))
    at(s, 4.5, lambda: fc("wind dir", WD["dir"], "90", 0.09, 0.4))
    at(s, 6.0, lambda: click_xy(*S["ok"], 0.35))
t.voice("e13", a_e13)
t.hold(2.5, find_rows)
t.voice("e14", lambda d: glide(*S["w1000"], 0.8))            # "The wind takes the bullet 82 inches to the left."
def a_e15(d):            # "And look at the drop: (1.49) 329, four inches less than without the wind. (6.03) This is aerodynamic jump…"
    s = time.monotonic()
    at(s, 0.3, lambda: glide(*S["d1000"], 0.8))
    at(s, 8.9, rest)
t.voice("e15", a_e15)
t.voice("e16", lambda d: (glide(*S["d300"], 0.8), time.sleep(d - 1.5), rest()))   # "And at three hundred yards… almost two inches high."
def a_e17(d):            # "Some effects add up, some cancel each other. You cannot guess that. You put the real numbers in."
    s = time.monotonic()
    at(s, d - 1.3, lambda: click_xy(*S["tab_reticle"], 0.8))    # the reticle recalculates ~2 s; clicks do not block
t.voice("e17", a_e17)

# ---- the scope
def a_q01(d):            # "The Reticle tab again. (1.73) 405 is now 428. (5.09) 496 is 522. (7.85) 578 is 607. (10.40) And 651 is 684.
    s = time.monotonic()  #  (13.44) Every dot is worth twenty to thirty yards more."
    for k, tt in enumerate((1.7, 5.0, 7.8, 10.3)):
        at(s, tt, lambda k=k: glide(*FAR_LABEL[k], 0.6))
    at(s, 13.6, rest)
t.voice("q01", a_q01)
def a_q02(d):            # "Near B D C (4.12) shows… (6.99) … (11.48) At twenty-two yards you hold one milliradian high, (15.18) at twelve yards four."
    s = time.monotonic()
    at(s, 0.1, lambda: click_xy(*S["Near BDC"], 0.5))
    at(s, 2.0, rest)
    at(s, 11.4, lambda: glide(*NEAR_LABEL[0], 0.7))
    at(s, 15.0, lambda: glide(*NEAR_LABEL[3], 0.6))
t.voice("q02", a_q02)
def a_q03(d):            # "Target (2.56) puts a target into the scope. A twenty inch plate at six hundred yards. (5.45) The box is where
    s = time.monotonic()  #  the bullet lands: (7.70) low, and to the left, because of the wind. (10.71) The plate is less than one mrad wide…"
    click_xy(*S["Target"], 0.5)
    for key, val in (("size_w", "20"), ("size_h", "20"), ("tdist", "600")):
        click_xy(*S[key], 0.35); keyboard.send_keys("{END}+{HOME}", pause=0.03); keyboard.send_keys(val, pause=0.06); keyboard.send_keys("{TAB}"); time.sleep(0.15)
    time.sleep(0.8); rest(); time.sleep(0.3)
    box = red_box(); print("         target box at", box, flush=True)
    at(s, 5.6, lambda: glide(box[0] + int(MRAD * 0.6), box[1] + int(MRAD * 0.3), 0.7))   # just off the box's right edge
    at(s, 11.0, lambda: glide(box[0] + int(MRAD * 0.6), box[1] - int(MRAD * 0.3), 0.4))
    at(s, d - 0.5, rest)
t.voice("q03", a_q03)
def a_q04(d):            # "The program comes with thirty-four reticles. (3.01) Load. (3.88) For example, (4.72) the Trijicon V-COG for 7.62…"
    s = time.monotonic()
    at(s, 2.6, lambda: click_xy(*S["load"], 0.4))
    fd = t.main.child_window(class_name="#32770"); fd.wait("visible", timeout=10)
    click(fd.child_window(auto_id="1148", control_type="ComboBox"), 0.3)
    at(s, 4.6, lambda: type_text("VCOG-16-762-175gr.reticle", 0.03))
    keyboard.send_keys("{ENTER}")
    at(s, 8.6, lambda: click_xy(*S["Far BDC"], 0.4))
    at(s, d - 0.3, rest)
t.voice("q04", a_q04)
def a_q05(d):            # "Its marks say four to ten: (2.48) hundreds of metres… (6.48) With our zero at 300 yards and today's air,
    s = time.monotonic()  #  the four is 593 yards, (12.69) and the ten is 1263."
    four, ten = (VC[0] + int(4.5 * MOA), VCOG["4"]), (VC[0] + int(4.5 * MOA), VCOG["10"])
    at(s, 0.6, lambda: glide(*four, 0.6))
    at(s, 1.6, lambda: glide(*ten, 0.6))
    at(s, 9.6, lambda: glide(VC[0] - int(10 * MOA), VCOG["4"], 0.8))
    at(s, 12.6, lambda: glide(VC[0] - int(10 * MOA), VCOG["10"], 0.8))
t.voice("q05", a_q05)
t.voice("q06", lambda d: (time.sleep(3.5), rest()))           # "An etched B D C is true only for… The program labels it for yours."
t.voice("q06b")                                               # card in post: the Reticle Editor
t.voice("q06c")                                               # card in post: the reticle-designer skill
t.voice("q07")                                                # "The scope did not change…"
t.voice("n01")
t.finish()
