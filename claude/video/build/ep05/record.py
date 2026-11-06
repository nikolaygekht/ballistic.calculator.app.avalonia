"""Episode 5 take — V-Clicks dialled for the 800 yd hold; then on a reopened bc2-demo: July air, and the Zero
tab's own February air.

Usage: python record.py [--dry]
Opens E:\\ngeht\\Documents\\Ballistic Calculator demo\\bc2-demo.trajectory twice and never saves.
Word times inside clips (comments "(1.4)") come from silencedetect on audio/*.mp3.
"""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

DEMO = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo\bc2-demo.trajectory")
assert DEMO.exists(), DEMO

# windows that would cover the recording monitor (OBS, the user's Notepad) go down first
for w in Desktop(backend="uia").windows():
    r = w.rectangle()
    if w.window_text().startswith(("OBS", )) or "Notepad" in w.window_text():
        if r.left < MON[0] + MON[2] and r.right > MON[0] and r.top < MON[1] + MON[3]:
            print("minimizing", w.window_text()); w.minimize()

t = Take(__file__)
W = lambda spec: spec.wrapper_object()
DROP, HOLD, CLICKS = 3, 4, 5

mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
traj_m = [c for c in bar.children() if c.window_text() == "Trajectory"][0]
view_m = [c for c in bar.children() if c.window_text() == "View"][0]
vr = view_m.rectangle()
VIEW_XY = center(view_m)
EDITP_XY = (vr.left + 120, vr.bottom + 28)        # View's first item, Edit Parameters
NEAR_MENU = (center(traj_m)[0] + 60, center(traj_m)[1] + 160)
NEAR_VIEW = (VIEW_XY[0] + 40, VIEW_XY[1] + 120)
TRAJ_XY = center(traj_m)
tr = traj_m.rectangle()
OPEN_XY = (tr.left + 6 + 172, tr.bottom + 52 + 24)     # Trajectory's Open item (measured: +6, +52, 344x48)
S = {}


def open_demo():
    """Trajectory -> Open -> bc2-demo -> Open, without bcrec.open_file's safety sleeps (~5 s instead of ~10)."""
    click_xy(*TRAJ_XY, 0.35); time.sleep(0.15)
    click_xy(*OPEN_XY, 0.3)
    d = t.main.child_window(class_name="#32770", title="Open Trajectory"); d.wait("visible", timeout=10)
    f = d.child_window(title_re=r"bc2-demo(\.trajectory)?$", control_type="ListItem", found_index=0)
    f.wait("visible", timeout=5)
    click(f.wrapper_object(), 0.35, 0.15)
    click(d.child_window(auto_id="1", control_type="Button").wrapper_object(), 0.35)
    for _ in range(50):                                      # until the new window's table is there
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
    """One pass over a panel: {automation id: element}. Lazy child_window specs cost ~0.5 s each."""
    ids = {}
    for e in W(spec).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    return ids


def num(e):
    return [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]


def rows(*ranges):
    """{range text: cells}. After a recalculation rows() is NOT in screen order."""
    out = {}
    for r in t.rows():
        c = r.descendants(control_type="Text")
        if c and c[0].window_text() in ranges:
            out[c[0].window_text()] = c
    return out


def check(label, fields):
    """Read back value + unit of every typed field right before OK. On a mismatch stop BEFORE OK, dialog open."""
    bad = False
    for name, e, want, unit in fields:
        v = e.get_value()
        u = ""
        if unit:
            u = [x.window_text() for x in e.parent().descendants(control_type="Text")][0]
        ok_ = v.replace(",", "") == want and (not unit or u == unit)
        bad |= not ok_
        print(f"         {label} {name:4} = {v!r} {u}" + ("" if ok_ else f"   <-- want {want} {unit}"), flush=True)
    if bad:
        print("STOPPED before OK: a field is wrong; the dialog is left open.", flush=True); os._exit(1)


def calc_error():
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.3):
        print("STOPPED: 'Cannot calculate this shot' after OK; left open.", flush=True); os._exit(1)


def cache_dialog():
    S["tabs"] = {e.window_text(): center(e) for e in W(t.dlg).descendants(control_type="TabItem")}
    S["ok"] = center(W(t.dlg.child_window(control_type="Button", title="OK")))


# ---- pre-roll (not recorded): open the file once and cache where the dialog's tabs and OK button sit, so the
# timed cues click a tab the moment the dialog appears instead of searching it first (that made tabs 1-2 s late)
open_demo()
click_xy(*VIEW_XY, 0.2); time.sleep(0.4); click_xy(*EDITP_XY, 0.2)
t.dlg.wait("visible", timeout=10); time.sleep(0.8)
cache_dialog()
print("cached tabs:", S["tabs"], "ok:", S["ok"])
click(t.dlg.child_window(control_type="Button", title="Cancel"), 0.2); time.sleep(0.5)
click([b for b in t.main.wrapper_object().descendants(control_type="Button")
       if b.element_info.automation_id == "PART_CloseButton"][0], 0.2); time.sleep(0.8)
rest()

# ---------------------------------------------------------------- the take
t.start()
t.voice("v00")

# ---- recap
glide(*NEAR_MENU, 0.8)
t.voice("r01", lambda d: open_demo())     # "Trajectory, (1.08) Open. The same file from episode two."
def a_r02(d):            # "Look at the 800 yard row. (2.26) The hold is 6.18 mrad. (5.64) One click… (8.98) so this is 62 clicks. (11.35) …Clicks column."
    s = time.monotonic()
    c = rows("800")["800"]
    S["h800"], S["c800"] = center(c[HOLD]), center(c[CLICKS])
    at(s, 0.1, lambda: glide(*S["h800"], 0.9))
    at(s, 8.9, lambda: glide(*S["c800"], 0.7))
t.voice("r02", a_r02)

# ---- part one: clicks already on the turret
t.voice("d00", lambda d: (time.sleep(2.0), rest(), time.sleep(7.0), glide(*NEAR_VIEW, 1.2)))
P = {}
R = {}
def a_d01a(d):           # "View, (0.83) Edit Parameters, (2.06) and the Rifle tab."
    s = edit_parameters(0.25, 1.0)
    at(s, 2.1, lambda: click_xy(*S["tabs"]["Rifle"], 0.4))
    time.sleep(0.3)
    ids = scan(t.by_id("RifleSubPanel"))
    R.update(v=ids["VerticalClickControl"], h=ids["HorizontalClickControl"])
t.voice("d01a", a_d01a)
def a_d01b(d):           # "The size of one click is set here. (2.8) V Click is 0.1 mrad, (5.79) and H Click is the same. (7.84) They came…"
    s = time.monotonic()
    at(s, 2.6, lambda: point(R["v"], 0.6, 0.3))
    at(s, 5.6, lambda: point(R["h"], 0.6, 0.3))
    at(s, 8.5, lambda: glide(*S["tabs"]["Parameters"], 1.2))
t.voice("d01b", a_d01b)
def a_d01c(d):           # "And now the Parameters tab."
    s = time.monotonic()
    at(s, 0.7, lambda: click_xy(*S["tabs"]["Parameters"], 0.3))
    time.sleep(0.3)
    boxes = [e for e in W(t.by_id("ParametersSubPanel")).descendants() if e.element_info.automation_id == "PART_TextBox"]
    P.update(v=boxes[0], h=boxes[1])                         # NumericUpDowns have no id of their own: V, then H
t.voice("d01c", a_d01c)
def a_d02(d):            # "V-Clicks is the number of clicks… (4.1) H-Clicks is the same for windage."
    s = time.monotonic()
    at(s, 0.1, lambda: point(P["v"], 0.7))
    at(s, 3.9, lambda: point(P["h"], 0.6))
t.voice("d02", a_d02)
def a_d03(d):            # "Sixty-two clicks up, (1.82) the number from the 800 yard row. (4.29) OK."
    s = time.monotonic()
    t.fill(P["v"], "62")
    at(s, 3.85, lambda: click_xy(*S["ok"], 0.4))
t.voice("d03", a_d03)
def find_part1(_):
    calc_error()
    rest()
    r = rows("400", "800", "1,000")
    S.update(d800=center(r["800"][DROP]), d400=center(r["400"][DROP]), h1000=center(r["1,000"][HOLD]))
t.hold(2.5, find_part1)
t.voice("d04", lambda d: glide(*S["d800"], 0.8))             # "Now, at 800 yards, the drop is almost zero…"
def a_d05(d):            # "Everything closer goes high. (2.39) At four hundred yards…"
    s = time.monotonic()
    at(s, 2.2, lambda: glide(*S["d400"], 0.8))
t.voice("d05", a_d05)
t.voice("d06", lambda d: glide(*S["h1000"], 0.8))            # "And at one thousand yards you still need to hold 3.8 mrad."
def a_d07(d):            # "So after you dial, this table shows what is left to hold."
    time.sleep(1.5); rest()
    S["close"] = [b for b in t.main.wrapper_object().descendants(control_type="Button")
                  if b.element_info.automation_id == "PART_CloseButton"][0]
t.voice("d07", a_d07)
def a_d08(d):            # "Now I close this window without saving."
    s = time.monotonic()
    at(s, 0.3, lambda: click(S["close"], 0.9))
    at(s, 1.9, lambda: glide(*NEAR_MENU, 0.8))
t.voice("d08", a_d08)

# ---- part two: zeroed in winter, shooting in July
t.voice("w01", lambda d: open_demo())     # "Trajectory, (1.24) Open, the same file again."
t.voice("w02", lambda d: (time.sleep(11.0), glide(*NEAR_VIEW, 1.2)))   # "Now the second situation…"
A = {}
def a_w03(d):            # "Edit Parameters, (1.45) and the Weather tab."
    s = edit_parameters(0.3, 0.9)
    at(s, 1.45, lambda: click_xy(*S["tabs"]["Weather"], 0.4))
    time.sleep(0.3)
    ids = scan(t.by_id("AtmosphereSubPanel"))
    A.update(alt=num(ids["AltitudeControl"]), prs=num(ids["PressureControl"]),
             tmp=num(ids["TemperatureControl"]), hum=ids["HumidityTextBox"])
t.voice("w03", a_w03)
def a_w04(d):            # "The altitude and the pressure are the same as in February: (3.7) one thousand feet, and 28.9."
    s = time.monotonic()
    at(s, 3.25, lambda: t.fill(A["alt"], "1000", 0.08, 0.4))
    at(s, 4.75, lambda: t.fill(A["prs"], "28.9", 0.08, 0.4))
t.voice("w04", a_w04)
t.voice("w05", lambda d: t.fill(A["tmp"], "100"))           # "The temperature is one hundred degrees."
def a_w06(d):            # "And the humidity is ninety percent. (2.66) OK."
    s = time.monotonic()
    t.fill(A["hum"], "90")
    check("shot", [("alt", A["alt"], "1000", "ft"), ("prs", A["prs"], "28.9", "inHg"),
                   ("tmp", A["tmp"], "100", "°F"), ("hum", A["hum"], "90", None)])
    at(s, 2.3, lambda: click_xy(*S["ok"], 0.35))
t.voice("w06", a_w06)
def find_1000(_):
    calc_error()
    rest()
    S["d1000"] = center(rows("1,000")["1,000"][DROP])
t.hold(2.5, find_1000)
t.voice("w07", lambda d: glide(*S["d1000"], 0.8))            # "At one thousand yards the drop is now 328 and a half…"
t.voice("w08", lambda d: (time.sleep(1.5), rest(), time.sleep(d - 5.0), glide(*NEAR_VIEW, 1.2)))   # "But this table is for a rifle zeroed today…"
Z = {}
def a_w09(d):            # "Edit Parameters, (1.42) and the Zero tab. (3.18) The Zero tab has its own weather, (5.5) down here."
    s = edit_parameters(0.3, 0.9)
    at(s, 1.45, lambda: click_xy(*S["tabs"]["Zero"], 0.4))
    time.sleep(0.4)
    zr = W(t.by_id("ZeroSubPanel")).rectangle()
    zc = ((zr.left + zr.right) // 2, (zr.top + zr.bottom) // 2)
    at(s, 3.0, lambda: glide(*zc, 0.4))
    for _ in range(3):                                       # the tab scrolls; the zero air is below the fold
        mouse.scroll(coords=zc, wheel_dist=-5); time.sleep(0.15)
    time.sleep(0.3)
    ids = scan(t.by_id("ZeroSubPanel", "ZeroAtmosphereSubPanel"))
    Z.update(chk=ids["EnableCheckBox"], alt=num(ids["AltitudeControl"]), prs=num(ids["PressureControl"]),
             tmp=num(ids["TemperatureControl"]), hum=ids["HumidityTextBox"])
    zr = W(t.by_id("ZeroSubPanel")).rectangle()
    for k in ("chk", "hum"):
        r = Z[k].rectangle()
        if not (zr.top <= r.top and r.bottom <= zr.bottom):
            print(f"         ! zero-air {k} not fully visible: {r} outside {zr}", flush=True)
    at(s, 5.3, lambda: point(Z["chk"], 0.5, 0.1))
t.voice("w09", a_w09)
t.voice("w10", lambda d: click(Z["chk"], 0.4, 0.1))          # "I turn on Other atmosphere for zero…"
def a_w11(d):            # "One thousand feet, (2.01) and twenty-eight point nine, as before."
    s = time.monotonic()
    t.fill(Z["alt"], "1000", 0.08, 0.4)
    at(s, 1.6, lambda: t.fill(Z["prs"], "28.9", 0.08, 0.4))
t.voice("w11", a_w11)
t.voice("w12", lambda d: t.fill(Z["tmp"], "40", 0.08, 0.35))  # "Forty degrees."
def a_w13(d):            # "And forty percent. (1.84) Winter air is dry. (3.96) OK."
    s = time.monotonic()
    t.fill(Z["hum"], "40")
    check("zero", [("alt", Z["alt"], "1000", "ft"), ("prs", Z["prs"], "28.9", "inHg"),
                   ("tmp", Z["tmp"], "40", "°F"), ("hum", Z["hum"], "40", None)])
    at(s, 3.55, lambda: click_xy(*S["ok"], 0.4))
t.voice("w13", a_w13)
def find_part2(_):
    calc_error()
    rest()
    r = rows("300", "1,000")
    S.update(d1000=center(r["1,000"][DROP]), d300=center(r["300"][DROP]))
t.hold(2.5, find_part2)
t.voice("w14", lambda d: glide(*S["d1000"], 0.8))            # "At one thousand yards the drop is 327 inches…"
t.voice("w15", lambda d: (glide(*S["d300"], 0.8), time.sleep(d - 3.0), rest()))   # "But look at three hundred yards…"
t.voice("n01")
t.finish()
