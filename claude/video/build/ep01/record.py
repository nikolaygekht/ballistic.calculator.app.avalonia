"""Episode 1 take (remake, 2026-11-04) — a metric trajectory from nothing, narrated while typed, with the
ammunition library: Load shows the shipped samples, Save keeps the load in our own folder.

Usage: python record.py [--dry]
Writes E:\\ngeht\\Documents\\Ballistic Calculator demo\\Lapua Scenar-L 11.3 g.ammox (deleted first if present).
Word times inside clips (comments "(1.4)") come from silencedetect on audio/*.mp3.
"""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

DEMO_DIR = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo")
AMMO = DEMO_DIR / "Lapua Scenar-L 11.3 g.ammox"
if AMMO.exists():
    AMMO.unlink()

for w in Desktop(backend="uia").windows():                  # nothing over the recording monitor
    r = w.rectangle()
    if w.window_text().startswith("OBS") or "Notepad" in w.window_text():
        if r.left < MON[0] + MON[2] and r.right > MON[0] and r.top < MON[1] + MON[3]:
            print("minimizing", w.window_text()); w.minimize()

t = Take(__file__)
W = lambda spec: spec.wrapper_object()
DROP = 3

mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
menu = {c.window_text(): c for c in bar.children()}
TRAJ_XY = center(menu["Trajectory"])
S = {}


def menu_item_xy(title):
    return center(t.main.child_window(control_type="MenuItem", title=title, found_index=0).wrapper_object())


def check(fields):
    """Read typed fields back before OK; on a mismatch stop with the dialog left open."""
    for name, e, want in fields:
        v = e.get_value(); print(f"         check {name} = {v!r}", flush=True)
        if v.replace(",", "") != want:
            print("STOPPED before OK: a field is wrong; the dialog is left open.", flush=True); os._exit(1)


def rows(*ranges):
    out = {}
    for r in t.rows():
        c = r.descendants(control_type="Text")
        if c and c[0].window_text() in ranges:
            out[c[0].window_text()] = c
    return out


def scan(spec):
    ids = {}
    for e in W(spec).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    return ids


num = lambda e: [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]

# ---- pre-roll (not recorded): Trajectory -> New -> Metric positions
mw.set_focus(); time.sleep(0.5)
for attempt in range(3):                                    # the first click on a fresh window may only activate it
    click_xy(*TRAJ_XY, 0.2); time.sleep(0.6)
    if t.main.child_window(control_type="MenuItem", title="New", found_index=0).exists(timeout=1.0):
        break
S["New"] = menu_item_xy("New"); glide(*S["New"], 0.2); time.sleep(0.8)
S["Metric"] = menu_item_xy("Metric")
keyboard.send_keys("{ESC}{ESC}"); time.sleep(0.4)
print("cached:", S)
rest()

# ---------------------------------------------------------------- the take
t.start()
t.voice("v00")
t.voice("i01", lambda d: (time.sleep(d - 2.0), glide(TRAJ_XY[0] + 40, TRAJ_XY[1] + 120, 1.2)))   # "In this episode…"
def a_n01(d):            # "Everything starts here: (1.74) Trajectory, (2.76) New. (3.65) First, the program asks: (5.5) metric or imperial?"
    s = time.monotonic()
    at(s, 1.35, lambda: click_xy(*TRAJ_XY, 0.35))
    at(s, 2.5, lambda: glide(*S["New"], 0.35))
    at(s, 5.3, lambda: glide(*S["Metric"], 0.6))
t.voice("n01", a_n01)
A = {}
def a_n02(d):            # "I choose metric. This setting is for this window only…"
    click_xy(*S["Metric"], 0.2)
    t.dlg.wait("visible", timeout=10); time.sleep(0.5)
    ids = scan(t.by_id("AmmoLibPanel"))
    A.update(name=ids["NameTextBox"], w=num(ids["WeightControl"]), bc=num(ids["BCControl"]), mv=num(ids["MuzzleVelocityControl"]),
             table=[c for c in ids["BCControl"].descendants() if c.element_info.automation_id == "TablePart"][0],
             load=ids["LoadButton"], save=ids["SaveButton"])
    S["tabs"] = {e.window_text(): center(e) for e in W(t.dlg).descendants(control_type="TabItem")}
    S["ok"] = center(W(t.dlg.child_window(control_type="Button", title="OK")))
    time.sleep(1.0); rest()
t.voice("n02", a_n02)
def a_n03(d):            # "One dialog describes the whole shot. It has six tabs, (2.93) but only the first one is required."
    s = time.monotonic()
    tabs = S["tabs"]
    at(s, 0.2, lambda: glide(*tabs["Ammunition"], 0.6))
    at(s, 0.9, lambda: glide(*tabs["Parameters"], 1.4))
    at(s, 3.0, lambda: glide(*tabs["Ammunition"], 0.8))
t.voice("n03", a_n03)

# ---- ammunition, typed on the words
def a_a01(d):            # "On the Ammunition tab, (1.65) first a name, (2.9) so we can find this load later: (5.06) Lapua Scenar L, 11.3 grams."
    s = time.monotonic()
    at(s, 1.6, lambda: point(A["name"], 0.6, 0.3))
    at(s, 4.5, lambda: t.fill(A["name"], "Lapua Scenar-L 11.3 g", 0.05, 0.3))
t.voice("a01", a_a01)
t.voice("a02", lambda d: (time.sleep(0.5), t.fill(A["w"], "11.3", 0.09, 0.4)))   # "The bullet weight is eleven point three grams."
def a_a03(d):            # "The ballistic coefficient shows how well… (4.5) It is zero point two five three, (6.71) on the G seven table."
    s = time.monotonic()
    at(s, 4.2, lambda: t.fill(A["bc"], "0.253", 0.09, 0.4))
    at(s, 6.2, lambda: click(A["table"], 0.35))
    item = W(t.item("G7"))
    at(s, 7.1, lambda: click(item, 0.3))
t.voice("a03", a_a03)
def a_a04(d):            # "And the muzzle velocity, (1.78) the speed of the bullet… (4.62) seven hundred ninety metres per second."
    s = time.monotonic()
    at(s, 4.3, lambda: t.fill(A["mv"], "790", 0.09, 0.4))
t.voice("a04", a_a04)
def a_a05(d):            # "This is the only tab you can't skip."
    check([("weight", A["w"], "11.3"), ("bc", A["bc"], "0.253"), ("mv", A["mv"], "790")])
t.voice("a05", a_a05)

# ---- the ammunition library
def a_l01(d):            # "You don't want to type a load every time. (2.56) …as files. (5.22) Load opens a small library… (9.36) sorted by calibre."
    s = time.monotonic()
    at(s, 2.8, lambda: point(A["load"], 1.0))
    at(s, 4.7, lambda: click(A["load"], 0.3))
    ld = t.main.child_window(class_name="#32770", title="Load Ammunition"); ld.wait("visible", timeout=10)
    f = W(ld.child_window(title=".308 Win", control_type="ListItem"))
    S["load_dlg"] = ld
    at(s, 9.2, lambda: point(f, 0.8, 0.3))
t.voice("l01", a_l01)
def a_l02(d):            # "Here it is. (1.09) We don't need one now. (2.51) Cancel."
    s = time.monotonic()
    cancel = W(S["load_dlg"].child_window(auto_id="2", control_type="Button"))
    at(s, 2.15, lambda: click(cancel, 0.35))
t.voice("l02", a_l02)
def a_l03(d):            # "Save keeps our load as a file. (2.79) The program suggests its own folder, and the name… (7.69) I keep my own loads… (13.43) Save."
    s = time.monotonic()
    at(s, 0.6, lambda: click(A["save"], 0.4))
    sd = t.main.child_window(class_name="#32770", title="Save Ammunition"); sd.wait("visible", timeout=10)
    addr = [e for e in W(sd).descendants() if e.element_info.class_name == "ToolbarWindow32" and e.window_text().startswith("Address")]
    name = W(sd.child_window(auto_id="1001", control_type="Edit"))
    btn = W(sd.child_window(auto_id="1", control_type="Button"))
    if addr:
        at(s, 3.0, lambda: point(addr[0], 0.8, 0.4))
    at(s, 5.6, lambda: point(name, 0.6, 0.3))
    at(s, 8.6, lambda: t.fill(name, str(AMMO.with_suffix("")), 0.025, 0.3))
    at(s, 13.0, lambda: click(btn, 0.35))
    time.sleep(1.0)
    print("         saved:", AMMO.exists(), flush=True)
t.voice("l03", a_l03)

# ---- rifle, zero, parameters
def a_f01(d):            # "On the Rifle tab we describe the scope… (9.79) The program has a list of common sights, so I just pick it."
    s = time.monotonic()
    at(s, 0.1, lambda: click_xy(*S["tabs"]["Rifle"], 0.4))
    time.sleep(0.3)
    ids = scan(t.by_id("RifleSubPanel"))
    A.update(combo=ids["SightPresetCombo"], height=num(ids["SightHeightControl"]) if "SightHeightControl" in ids else None,
             hclick=num(ids["HorizontalClickControl"]), vclick=num(ids["VerticalClickControl"]))
    at(s, 9.7, lambda: click(A["combo"], 0.5))
    time.sleep(0.3)
    item = W(t.item("PSO-1 Optics"))
    at(s, 11.3, lambda: click(item, 0.5))
t.voice("f01", a_f01)
def a_f02(d):            # "It fills two things. (1.76) Sight height… sixty-five millimetres. (7.11) And the click value… half a thousandth."
    s = time.monotonic()
    if A["height"] is not None:
        at(s, 1.6, lambda: point(A["height"], 0.6))
    at(s, 7.0, lambda: point(A["hclick"], 0.5))
    at(s, 8.6, lambda: point(A["vclick"], 0.5))
t.voice("f02", a_f02)
t.voice("f03", lambda d: (time.sleep(1.0), rest()))           # "Sight height matters at every distance…"
def a_z01(d):            # "On the Zero tab… (6.2) The sight preset suggested one hundred metres. (9.12) We use three hundred."
    s = time.monotonic()
    at(s, 0.1, lambda: click_xy(*S["tabs"]["Zero"], 0.4))
    time.sleep(0.3)
    A["zero"] = W(t.field("ZeroSubPanel", "ZeroDistanceControl"))
    at(s, 6.1, lambda: point(A["zero"], 0.6))
    at(s, 8.8, lambda: t.fill(A["zero"], "300", 0.09, 0.3))
    check([("zero", A["zero"], "300")])
t.voice("z01", a_z01)
def a_p01(d):            # "On the Parameters tab: (1.72) how far to calculate, (3.35) one thousand metres, (4.97) and the step… (7.15) one hundred. (8.28) Now OK."
    s = time.monotonic()
    at(s, 0.1, lambda: click_xy(*S["tabs"]["Parameters"], 0.4))
    time.sleep(0.3)
    mx, st = W(t.field("ParametersSubPanel", "MaxRangeControl")), W(t.field("ParametersSubPanel", "StepControl"))
    at(s, 3.0, lambda: t.fill(mx, "1000", 0.08, 0.35))
    at(s, 6.8, lambda: t.fill(st, "100", 0.08, 0.3))
    check([("max", mx, "1000"), ("step", st, "100")])
    at(s, 8.0, lambda: click_xy(*S["ok"], 0.3))
t.voice("p01", a_p01)
def a_w01(d):            # "We did not fill the Weather tab, so the program offers the default weather… (14.78) Yes."
    s = time.monotonic()
    c = t.main.child_window(class_name="Window", title="Confirm"); c.wait("visible", timeout=10)
    yes = W(c.child_window(control_type="Button", title="Yes"))
    at(s, 1.0, lambda: point(yes, 0.8, 0.5, -0.8))
    at(s, 14.4, lambda: click(yes, 0.4))
t.voice("w01", a_w01)

# ---- the result
def find_rows(_):
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.3):
        print("STOPPED: 'Cannot calculate this shot'; left open.", flush=True); os._exit(1)
    rest()
    r = rows("100", "300", "400", "1,000")
    S.update({k: center(r[k][DROP]) for k in r})
    S["tabs_tv"] = {e.window_text(): center(e) for e in t.tv().descendants(control_type="TabItem")}
    print("         1000 m drop:", r["1,000"][DROP].window_text(), flush=True)
t.hold(1.5, find_rows)
t.voice("x01", lambda d: glide(S["100"][0] - 200, S["100"][1] - 60, 1.0))   # "Here is the result. One line for every hundred metres."
t.voice("x02", lambda d: glide(*S["300"], 0.8))              # "At three hundred metres the drop is zero…"
t.voice("x03", lambda d: glide(*S["100"], 0.8))              # "At one hundred metres … fifteen centimetres above…"
def a_x04(d):            # "And at one thousand metres (1.78) the bullet is twelve metres below… (5.18) Twelve metres, after two seconds of flight."
    glide(*S["400"], 0.6); glide(*S["1,000"], 1.4)
t.voice("x04", a_x04)
t.voice("c01", lambda d: click_xy(*S["tabs_tv"]["Chart"], 0.5))   # "This is the same shot as a chart…"
def a_c02(d):            # "There is one more view: (1.78) the Reticle tab. It shows the trajectory through your scope…"
    s = time.monotonic()
    at(s, 1.4, lambda: glide(*S["tabs_tv"]["Reticle"], 0.6))
    at(s, d - 1.0, rest)
t.voice("c02", a_c02)
t.voice("nx1")
t.finish()
