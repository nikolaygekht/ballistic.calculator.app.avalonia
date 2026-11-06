"""Episode 6 take — what a BC is (full-screen cards), Tools -> Approximate Drag Table, then the Lapua radar .drg via
Browse, the result, and Save As bc2-demo-radar.

Usage: python record.py [--dry]
Opens bc2-demo, saves bc2-demo-radar.trajectory next to it (the baseline is not touched).
Word times inside clips (comments "(1.4)") come from silencedetect on audio/*.mp3.
"""
import pathlib, queue, sys, threading, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

HERE = pathlib.Path(__file__).resolve().parent
DEMO_DIR = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo")
DRG = DEMO_DIR / "30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg"
RADAR = DEMO_DIR / "bc2-demo-radar.trajectory"
assert (DEMO_DIR / "bc2-demo.trajectory").exists() and DRG.exists()
assert not RADAR.exists(), f"{RADAR} exists — delete it first (the take saves it)"

# windows that would cover the recording monitor (OBS, the user's Notepad) go down first
for w in Desktop(backend="uia").windows():
    r = w.rectangle()
    if w.window_text().startswith("OBS") or "Notepad" in w.window_text():
        if r.left < MON[0] + MON[2] and r.right > MON[0] and r.top < MON[1] + MON[3]:
            print("minimizing", w.window_text()); w.minimize()


class Cards:
    """Full-screen picture cards over the recording monitor, in their own Tk thread (a Tk window that does not
    pump messages for >5 s turns into a 'Not Responding' ghost)."""
    def __init__(self):
        self.q = queue.Queue(); self.ready = threading.Event()
        threading.Thread(target=self._run, daemon=True).start(); self.ready.wait(5)

    def _run(self):
        import tkinter as tk
        root = tk.Tk(); root.withdraw()
        win = tk.Toplevel(root); win.overrideredirect(True); win.attributes("-topmost", True)
        win.geometry(f"{MON[2]}x{MON[3]}+{MON[0]}+{MON[1]}"); win.withdraw()
        label = tk.Label(win, bd=0, bg="#fcfcfb"); label.pack(fill="both", expand=True)
        imgs = {}

        def poll():
            try:
                while True:
                    cmd = self.q.get_nowait()
                    if cmd is None:
                        win.withdraw()
                    else:
                        imgs.setdefault(cmd, tk.PhotoImage(file=str(HERE / cmd)))
                        label.configure(image=imgs[cmd]); win.deiconify(); win.lift()
            except queue.Empty:
                pass
            root.after(30, poll)
        root.after(30, poll); self.ready.set(); root.mainloop()

    def show(self, name):
        self.q.put(name)

    def hide(self):
        self.q.put(None)


EDGE = (MON[0] + MON[2] - 2, MON[1] + 720)      # cursor parked on the monitor's right edge while a card is up
t = Take(__file__)
W = lambda spec: spec.wrapper_object()
DROP, CLICKS, VEL = 3, 5, 1

mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
menu = {c.window_text(): c for c in bar.children()}
traj_m, view_m, tools_m = menu["Trajectory"], menu["View"], menu["Tools"]
vr, tr = view_m.rectangle(), traj_m.rectangle()
VIEW_XY, TRAJ_XY, TOOLS_XY = center(view_m), center(traj_m), center(tools_m)
EDITP_XY = (vr.left + 120, vr.bottom + 28)        # View's first item, Edit Parameters
NEAR_MENU = (TRAJ_XY[0] + 60, TRAJ_XY[1] + 160)
S = {}


def menu_item_xy(title):
    return center(t.main.child_window(control_type="MenuItem", title=title, found_index=0).wrapper_object())


def open_demo():
    """Trajectory -> Open -> bc2-demo -> Open (~5 s)."""
    click_xy(*TRAJ_XY, 0.35); time.sleep(0.15)
    click_xy(*S["Open"], 0.3)
    d = t.main.child_window(class_name="#32770", title="Open Trajectory"); d.wait("visible", timeout=10)
    f = d.child_window(title_re=r"bc2-demo(\.trajectory)?$", control_type="ListItem", found_index=0)
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


def rows(*ranges):
    out = {}
    for r in t.rows():
        c = r.descendants(control_type="Text")
        if c and c[0].window_text() in ranges:
            out[c[0].window_text()] = c
    return out


def calc_error():
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.3):
        print("STOPPED: 'Cannot calculate this shot' after OK; left open.", flush=True); os._exit(1)


def check(fields):
    """Read typed fields back before OK; on a mismatch stop with the dialog left open."""
    for name, e, want in fields:
        v = e.get_value(); print(f"         check {name} = {v!r}", flush=True)
        if v.replace(",", "") != want:
            print("STOPPED before OK: a field is wrong; the dialog is left open.", flush=True); os._exit(1)


def close_window():
    click([b for b in t.main.wrapper_object().descendants(control_type="Button")
           if b.element_info.automation_id == "PART_CloseButton"][0], 0.2); time.sleep(0.8)


# ---- pre-roll (not recorded): menu item positions (menus are looked up lazily, ~1 s each — never inside a cue)
def measure(menu_xy, *path):
    """Open a menu, hover through submenus, return the centres of the named items of every level."""
    click_xy(*menu_xy, 0.2); time.sleep(0.5)
    out = {}
    for i, level in enumerate(path):
        for name in level:
            out[name] = menu_item_xy(name)
        if i < len(path) - 1:
            glide(*out[level[-1]], 0.2); time.sleep(0.8)
    keyboard.send_keys("{ESC}{ESC}{ESC}"); time.sleep(0.4)
    return out


S.update(measure(TRAJ_XY, ["Open", "Save As", "New"], ["Imperial"]))
S.update(measure(TOOLS_XY, ["Approximate Drag Table"], ["From BC Curve...", "From Measured Velocities..."]))
open_demo()
S.update(measure(VIEW_XY, ["Compare"], ["Add"]))
S.update(measure(VIEW_XY, ["Chart"], ["Velocity"]))
WINDOWS_XY = center(menu["Windows"])
click_xy(*WINDOWS_XY, 0.2); time.sleep(0.5)
cx, cy = menu_item_xy("Cascade")
S["win1"], S["win2"] = (cx, cy + 60), (cx, cy + 108)        # the window list starts under Cascade, 48 px a line
keyboard.send_keys("{ESC}"); time.sleep(0.4)
print("cached:", S)
close_window()
rest()

# ---------------------------------------------------------------- the take
t.start()
cards = Cards()                                              # after start(): its black cover is a Tk of its own
t.voice("v00")

# ---- recap
glide(*NEAR_MENU, 0.8)
t.voice("r01", lambda d: open_demo())                       # "Trajectory, (1.14) Open. The same file from episode two…"
def a_r02(d):            # "At one thousand yards the drop is 360 inches. (4.3) All the numbers… 0.253 G7."
    c = rows("1,000")["1,000"]
    glide(*center(c[DROP]), 0.9)
    time.sleep(6.0); rest()
t.voice("r02", a_r02)

# ---- part one: cards
def card(name):
    def act(d):
        cards.show(name); glide(*EDGE, 0.5)
    return act
t.voice("t01", card("card-prb-profiles.png"))              # "What is this number? … G one … G seven …"
t.voice("t02", card("card-curves.png"))                     # "The drag of the standard bullet was measured…"
t.voice("t03")                                              # "And here, in blue, is the real drag of our bullet…"
t.voice("t04", card("card-prb-hornady.png"))               # "…four Hornady bullets against G seven…"
t.voice("t05", card("bc-along-flight.png"))                 # "Here is the real G seven coefficient of our bullet…"
t.voice("t06", card("card-prb-radar.png"))                  # "So where do these curves come from? … Doppler radar…"
t.voice("t07")                                              # "Some of these pictures are from the Precision Rifle Blog…"
def a_t08(d):            # "Lapua measured its bullets this way…"
    cards.hide(); time.sleep(0.5); rest()
    time.sleep(d - 3.5); glide(TOOLS_XY[0] + 40, TOOLS_XY[1] + 140, 1.2)
t.voice("t08", a_t08)
def a_t09(d):            # "…build a curve for you. (5.02) Tools, (5.82) Approximate Drag Table. (7.78) From BC Curve… (12.96) From Measured Velocities…"
    s = time.monotonic()
    at(s, 4.6, lambda: click_xy(*TOOLS_XY, 0.35))
    at(s, 5.7, lambda: glide(*S["Approximate Drag Table"], 0.5))
    at(s, 7.6, lambda: glide(*S["From BC Curve..."], 0.6))
    at(s, 12.8, lambda: glide(*S["From Measured Velocities..."], 0.5))
    at(s, d - 0.4, lambda: keyboard.send_keys("{ESC}{ESC}{ESC}"))
t.voice("t09", a_t09)

# ---- part two: a new trajectory on the radar curve
A = {}
def a_p01(d):            # "Now let's use the file. (1.97) I keep this window as it is… (5.85) Trajectory, (6.95) New, (7.79) Imperial."
    s = time.monotonic()
    at(s, 5.45, lambda: click_xy(*TRAJ_XY, 0.35))
    at(s, 6.6, lambda: glide(*S["New"], 0.35))
    at(s, 7.45, lambda: click_xy(*S["Imperial"], 0.35))
    t.dlg.wait("visible", timeout=10); time.sleep(0.3)
    ids = {}
    for e in W(t.by_id("AmmoLibPanel")).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    num = lambda e: [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]
    A.update(bc=num(ids["BCControl"]), w=num(ids["WeightControl"]), mv=num(ids["MuzzleVelocityControl"]),
             dia=num(ids["BulletDiameterControl"]), len=num(ids["BulletLengthControl"]), name=ids["NameTextBox"],
             table=[c for c in ids["BCControl"].descendants() if c.element_info.automation_id == "TablePart"][0],
             ff=ids["FormFactorCheckBox"], browse=ids["BrowseTableButton"])
    S["ok"] = center(W(t.dlg.child_window(control_type="Button", title="OK")))
    S["tabs"] = {e.window_text(): center(e) for e in W(t.dlg).descendants(control_type="TabItem")}
t.voice("p01", a_p01)
t.voice("p02", lambda d: (time.sleep(2.9), click(A["browse"], 0.5)))   # "On the Ammunition tab, (1.57) next to Drag table, (3.15) I press Browse."
def a_p03(d):            # "I go to my folder. (1.71) Here is the file. (3.08) Open."
    fd = t.main.child_window(class_name="#32770", title="Open Custom Drag Table"); fd.wait("visible", timeout=10)
    click(fd.child_window(auto_id="1148", control_type="ComboBox"), 0.3)
    type_text(str(DEMO_DIR), 0.015); keyboard.send_keys("{ENTER}")
    f = fd.child_window(title_re=r"30-lapua gb550.*", control_type="ListItem", found_index=0); f.wait("visible", timeout=5)
    click(f.wrapper_object(), 0.4, 0.15)
    click(fd.child_window(auto_id="1", control_type="Button").wrapper_object(), 0.35)
t.voice("p03", a_p03)
def a_p04(d):            # "The coefficient is one, (1.87) and the table is G C… (5.19) And BC is Form Factor is on. (8.62) So this number…
    s = time.monotonic()  #  (12.1) It is a form factor… (17.68) One means… (20.51) If your bullets fly a little better… 0.98 here."
    at(s, 0.1, lambda: point(A["bc"], 0.6))
    at(s, 1.8, lambda: point(A["table"], 0.5))
    at(s, 5.1, lambda: point(A["ff"], 0.5, 0.1))
    at(s, 8.5, lambda: point(A["bc"], 0.6))
    at(s, 20.4, lambda: point(A["bc"], 0.4, 0.3))
t.voice("p04", a_p04)
def a_p05(d):            # "…took the name, the weight, the diameter and the length… (5.72) …11.3 grams. (8.47) That's 174.4 grains…"
    s = time.monotonic()
    at(s, 0.9, lambda: point(A["name"], 0.5, 0.3))
    at(s, 2.0, lambda: point(A["w"], 0.5))
    at(s, 3.0, lambda: point(A["dia"], 0.4))
    at(s, 3.8, lambda: point(A["len"], 0.4))
    at(s, 8.3, lambda: point(A["w"], 0.6))
t.voice("p05", a_p05)
def a_p06(d):            # "The muzzle velocity is ours: (2.37) two thousand six hundred feet per second. (5.15) The file knows the bullet…"
    s = time.monotonic()
    at(s, 1.9, lambda: t.fill(A["mv"], "2600", 0.09, 0.45))
    check([("mv", A["mv"], "2600"), ("bc", A["bc"], "1.000")])       # read while the Ammunition tab is still shown
t.voice("p06", a_p06)
R = {}
def a_p07(d):            # "On the Rifle tab, (1.36) the same sight as in episode two: (3.74) Standard Optics, High, Mil."
    s = time.monotonic()
    at(s, 0.2, lambda: click_xy(*S["tabs"]["Rifle"], 0.4))
    time.sleep(0.3)
    R["combo"] = W(t.by_id("RifleSubPanel", "SightPresetCombo"))
    at(s, 2.5, lambda: click(R["combo"], 0.5))
    time.sleep(0.3)
    item = W(t.item("Standard Optics (High, Mil)"))
    at(s, 3.7, lambda: click(item, 0.5))
t.voice("p07", a_p07)
def a_p08(d):            # "On the Zero tab, (1.36) three hundred yards. (2.9) OK, and standard weather, yes."
    s = time.monotonic()
    at(s, 0.1, lambda: click_xy(*S["tabs"]["Zero"], 0.35))
    time.sleep(0.3)
    zd = W(t.field("ZeroSubPanel", "ZeroDistanceControl"))
    at(s, 1.0, lambda: t.fill(zd, "300", 0.09, 0.35))
    check([("zero", zd, "300")])
    at(s, 2.6, lambda: click_xy(*S["ok"], 0.35))
    c = t.main.child_window(class_name="Window", title="Confirm")
    if c.exists(timeout=5):
        click(c.child_window(control_type="Button", title="Yes"), 0.45)
t.voice("p08", a_p08)
def find_rows(_):
    calc_error(); rest()
    r = rows("800", "1,000")
    S.update(d1000=center(r["1,000"][DROP]), c1000=center(r["1,000"][CLICKS]))
    print("         radar 1000 yd drop:", r["1,000"][DROP].window_text(), flush=True)
t.hold(2.5, find_rows)
t.voice("u06", lambda d: glide(*S["d1000"], 0.8))            # "At one thousand yards the drop is now 371 inches…"
t.voice("u07", lambda d: (glide(*S["c1000"], 0.6), time.sleep(1.0), glide(VIEW_XY[0] + 40, VIEW_XY[1] + 140, 0.8)))   # "…three more clicks."

# ---- compare
def view_compare_add(s, t_view, t_cmp, t_add):
    at(s, t_view, lambda: click_xy(*VIEW_XY, 0.3))
    at(s, t_cmp, lambda: glide(*S["Compare"], 0.3))
    at(s, t_add, lambda: click_xy(*S["Add"], 0.3))
def a_c01(d):            # "Now let's put both on one chart. (2.61) View, (3.37) Compare, (4.31) Add."
    view_compare_add(time.monotonic(), 2.2, 3.1, 4.0)
t.voice("c01", a_c01)
def a_c02(d):            # "Windows, (0.88) our first trajectory. (2.61) View, (3.28) Compare, (4.25) Add again."
    s = time.monotonic()
    at(s, 0.0, lambda: click_xy(*WINDOWS_XY, 0.3))
    at(s, 1.1, lambda: click_xy(*S["win1"], 0.4))
    view_compare_add(s, 2.2, 3.0, 3.9)
t.voice("c02", a_c02)
def chart_xy(fx, fy):
    r = [c for c in t.main.wrapper_object().descendants() if c.element_info.class_name == "TrajectoryChartControl"][-1].rectangle()
    return r.left + int(r.width() * fx), r.top + int(r.height() * fy)
def a_c03(d):            # "Here are the two drop curves. (2.11) Up to about five hundred yards they are almost the same. (6.0) Further out…"
    s = time.monotonic()
    p500, p950 = chart_xy(0.52, 0.30), chart_xy(0.90, 0.72)
    at(s, 2.0, lambda: glide(*p500, 0.9))
    at(s, 6.0, lambda: glide(*p950, 1.0))
t.voice("c03", a_c03)
def a_c04(d):            # "View, (0.80) Chart, (1.51) Velocity. (2.64) Here the radar bullet is slower all the way."
    s = time.monotonic()
    at(s, 0.0, lambda: click_xy(*VIEW_XY, 0.3))
    at(s, 0.6, lambda: glide(*S["Chart"], 0.3))
    at(s, 1.3, lambda: click_xy(*S["Velocity"], 0.3))
    p = chart_xy(0.45, 0.45)
    at(s, 3.0, lambda: glide(*p, 0.8))
t.voice("c04", a_c04)
t.voice("u08", lambda d: glide(*chart_xy(0.77, 0.70), 0.8))  # "And at eight hundred yards … twenty-five feet per second slower…"
t.voice("u09", lambda d: (time.sleep(1.5), rest()))           # "And zero point two five three is a good number…"
t.voice("u10", lambda d: (time.sleep(d - 2.0), glide(WINDOWS_XY[0] + 40, WINDOWS_XY[1] + 140, 1.2)))   # "Inside three hundred yards…"

# ---- part three: save the radar version
def a_s01(d):            # "This one I keep. (1.63) Windows, (2.2) the Lapua window. (3.92) Trajectory, (4.97) Save As."
    s = time.monotonic()
    at(s, 1.3, lambda: click_xy(*WINDOWS_XY, 0.35))
    at(s, 2.4, lambda: click_xy(*S["win2"], 0.4))
    at(s, 3.6, lambda: click_xy(*TRAJ_XY, 0.35))
    at(s, 4.7, lambda: click_xy(*S["Save As"], 0.35))
t.voice("s01", a_s01)

def a_s02(d):            # "A new name: (1.18) b c two demo radar. (3.76) Save. The first file stays as it was."
    s = time.monotonic()
    sd = t.main.child_window(class_name="#32770", title="Save Trajectory"); sd.wait("visible", timeout=10)
    click(sd.child_window(auto_id="1001", control_type="Edit"), 0.3)
    at(s, 1.1, lambda: type_text("bc2-demo-radar", 0.08))
    at(s, 3.4, lambda: click(sd.child_window(auto_id="1", control_type="Button"), 0.35))
    time.sleep(1.0); rest()
t.voice("s02", a_s02)
t.voice("n01")
t.finish()
print("saved:", RADAR.exists(), RADAR)
