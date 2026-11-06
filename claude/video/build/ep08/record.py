"""Episode 8 take — 300 Blackout + Trijicon V-COG: zero one load, shoot another.

Window 1: the supersonic 115 gr load alone (library, MV 2295 -> 2330), zero 100 yd -> m, V-COG Far BDC 200-600 m.
Window 2: the subsonic 208 gr Hornady (library, MV 1020 -> 1010), Other ammunition for zero = 115 gr at 2330,
200 m / 25 m; the cost (-42 cm at 100 m, -188 cm at 200 m); V-COG Far BDC, Split at 100 -> 50: 55 / 102 / 151 m.
Then a click back to window 1.

Usage: python record.py [--dry]      never saves anything.
Word times inside clips (comments "(1.4)") come from silencedetect on audio/*.mp3 (= where the next word starts).
"""
import pathlib, re, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *
from PIL import ImageGrab
HERE = pathlib.Path(__file__).resolve().parent

SUPER = (r"^\.300 AAC 115gr(\.ammo)?$", r".300 AAC\.300 AAC 115gr.ammo")
SUB = (r"^\.300 AAC 208gr Hornady(\.ammo)?$", r".300 AAC\.300 AAC 208gr Hornady.ammo")
PRESET = "Standard Optics (High, Mil)"
RETICLE = "VCOG16-300BLK.reticle"
# VCOG16-300BLK: 172 x 172 MOA, zero in the middle; ladder anchors (MOA below the crosshair)
LADDER = {"200": 3.201, "300": 8.289, "400": 14.939, "500": 23.362, "600": 33.759}

# windows that would cover the recording monitor (OBS, the user's Notepad) go down first
for w in Desktop(backend="uia").windows():
    r = w.rectangle()
    if w.window_text().startswith("OBS") or "Notepad" in w.window_text():
        if r.left < MON[0] + MON[2] and r.right > MON[0] and r.top < MON[1] + MON[3]:
            print("minimizing", w.window_text()); w.minimize()

t = Take(__file__)
W = lambda spec: spec.wrapper_object()
S, W1, W2 = {}, {}, {}


# ---------------------------------------------------------------- helpers
def scan(spec):
    ids = {}
    for e in W(spec).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    return ids


def num(e):
    return [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]


def unit_part(e):
    return [c for c in e.descendants() if c.element_info.automation_id == "UnitPart"][0]


def unit_text(e):
    ch = unit_part(e).children()
    return ch[0].window_text() if ch else "?"


def check(label, fields):
    """Read typed fields back before OK; on a mismatch stop, dialog left open."""
    bad = False
    for name, e, want in fields:
        v = e.get_value(); ok_ = v.replace(",", "") == want; bad |= not ok_
        print(f"         {label} {name:5} = {v!r}" + ("" if ok_ else f"   <-- want {want}"), flush=True)
    if bad:
        print("STOPPED: a field is wrong; the app is left open.", flush=True); os._exit(1)


def fc(label, el, text, pause=0.07, secs=0.35):
    """click, End + Shift+Home (not Ctrl+A, see ep07), type, read back."""
    glide(*center(el), secs); time.sleep(0.05); mouse.click(coords=pos()); time.sleep(0.12)
    keyboard.send_keys("{END}+{HOME}", pause=0.03); time.sleep(0.05)
    keyboard.send_keys(text, pause=pause, with_spaces=True); time.sleep(0.1)
    check(label, [("", el, text)])


def pick(combo, title, secs=0.35):
    click(combo, secs); time.sleep(0.35)
    click(W(t.item(title)), secs); time.sleep(0.2)


def set_unit(ctrl, unit, secs=0.35):
    pick(unit_part(ctrl), unit, secs)


def calc_error():
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.3):
        print("STOPPED: 'Cannot calculate this shot' after OK; left open.", flush=True); os._exit(1)


def accept_weather(secs=0.35):
    c = t.main.child_window(class_name="Window", title="Confirm")
    c.wait("visible", timeout=5)
    click(c.child_window(control_type="Button", title="Yes"), secs)


def ammo_dialog(title):
    d = t.main.child_window(class_name="#32770", title=title); d.wait("visible", timeout=10)
    return d


def list_item(d, pattern, timeout=3):
    f = d.child_window(title_re=pattern, control_type="ListItem", found_index=0)
    return W(f) if f.exists(timeout=timeout) else None


def open_folder(d, secs=0.4):
    """Double-click the .300 AAC folder (the dialogs start in data\\ammo)."""
    f = list_item(d, r"^\.300 AAC$")
    if f is None:
        return False
    glide(*center(f, 0.3), secs); time.sleep(0.05); mouse.double_click(coords=pos()); time.sleep(0.3)
    return True


def open_file(d, pattern, rel, secs=0.4):
    f = list_item(d, pattern)
    if f is None:                                           # not listed: type the path instead
        print("         (file not listed; typing the path)", flush=True)
        click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.3)
        type_text(rel, 0.01); keyboard.send_keys("{ENTER}"); time.sleep(0.8); return
    click(f, secs, 0.3); time.sleep(0.15)
    click(d.child_window(auto_id="1", control_type="Button"), 0.3); time.sleep(0.6)


def load_reticle(type_pause=0.03):
    d = t.main.child_window(class_name="#32770"); d.wait("visible", timeout=10)
    click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.3)
    type_text(RETICLE, type_pause); keyboard.send_keys("{ENTER}")


def menu_new_metric(t_traj, t_new, t_metric, s):
    at(s, t_traj, lambda: click_xy(*S["traj"], 0.35))
    at(s, t_new, lambda: glide(*S["new"], 0.35))
    # L-shaped path: straight right along New into its submenu, then down to Metric. The diagonal
    # crossed "Open" below New and closed the submenu (dry run 1: the dialog never came up).
    at(s, t_metric - 0.25, lambda: glide(S["metric"][0], S["new"][1], 0.25))
    at(s, t_metric, lambda: click_xy(*S["metric"], 0.2))
    t.dlg.wait("visible", timeout=10); time.sleep(0.4)


def cache_window(D, table_rows=()):
    """Positions on the newest trajectory window (taken in the pre-roll, reused in the take)."""
    tv = t.tv(); r = tv.rectangle()
    D["title"] = (r.left + 300, r.top - 22)
    D["tab_table"] = center(t.tv_find("TabItem", "Table", tv=tv))
    D["tab_reticle"] = center(t.tv_find("TabItem", "Reticle", tv=tv))
    if table_rows:
        heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
        drop = next(i for i, h in enumerate(heads) if h.startswith("Drop"))
        for row in tv.descendants(control_type="DataItem"):
            c = row.descendants(control_type="Text")
            if c and c[0].window_text() in table_rows:
                D["d" + c[0].window_text()] = center(c[drop])
                print(f"   row {c[0].window_text()}: drop = {c[drop].window_text()}  (col {drop}: {heads[drop]})")
    click_xy(*D["tab_reticle"], 0.15); time.sleep(3.0)
    D["load"] = center([b for b in tv.descendants(control_type="Button") if b.element_info.automation_id == "LoadReticleButton"][0])
    click_xy(*D["load"], 0.15); load_reticle(0.005); time.sleep(3.0)
    D["far"] = center(t.tv_find("RadioButton", "Far BDC", tv=tv), 0.1)
    D["split"] = center(num([e for e in tv.descendants() if e.element_info.automation_id == "BdcSplitControl"][0]))
    cv = t.tv_find("Custom", auto_id="ReticleCanvas", tv=tv).rectangle()
    D["canvas"] = (cv.left, cv.top, cv.right, cv.bottom)
    moa = min(cv.width(), cv.height()) / 172.0
    cx, cy = (cv.left + cv.right) // 2, (cv.top + cv.bottom) // 2
    D["cross"] = (cx + int(3 * moa), cy + int(1 * moa))                  # just off the crosshair's centre
    D["label"] = {k: (cx - int(4.5 * moa), cy + int(v * moa) + 2) for k, v in LADDER.items()}   # labels: left of the ladder
    D["bar"] = {k: (cx + int(2.0 * moa), cy + int(v * moa)) for k, v in LADDER.items()}
    click_xy(*D["far"], 0.15); time.sleep(1.5)


def quick_shot(ammo, zero_ammo=None, params=None):
    """Pre-roll only: the same shot as the take, as fast as possible (paths typed)."""
    click_xy(*S["traj"], 0.15); time.sleep(0.3); glide(*S["new"], 0.15); time.sleep(0.3)
    glide(S["metric"][0], S["new"][1], 0.15); time.sleep(0.2); click_xy(*S["metric"], 0.15)
    t.dlg.wait("visible", timeout=10); time.sleep(0.6)
    if "tabs" not in S:
        S["tabs"] = {e.window_text(): center(e) for e in W(t.dlg).descendants(control_type="TabItem")}
        S["ok"] = center(W(t.dlg.child_window(control_type="Button", title="OK")))
    a = scan(t.by_id("AmmoLibPanel"))
    click(a["LoadButton"], 0.15); d = ammo_dialog("Load Ammunition")
    click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.15); type_text(ammo[1], 0.005); keyboard.send_keys("{ENTER}"); time.sleep(1.0)
    a = scan(t.by_id("AmmoLibPanel")); fc("pre mv", num(a["MuzzleVelocityControl"]), ammo[2], 0.01, 0.15)
    click_xy(*S["tabs"]["Rifle"], 0.15); time.sleep(0.4)
    r = scan(t.by_id("RifleSubPanel"))
    pick(r["SightPresetCombo"], PRESET, 0.15)
    r = scan(t.by_id("RifleSubPanel"))
    fc("pre sh", num(r["SightHeightControl"]), "2.9", 0.01, 0.15)
    pick(r["RiflingDirectionCombo"], "Right", 0.15)
    print("   twist unit before:", unit_text(r["RiflingStepControl"]))
    if unit_text(r["RiflingStepControl"]) != "in":
        set_unit(r["RiflingStepControl"], "in", 0.15)
    fc("pre tw", num(r["RiflingStepControl"]), "8", 0.01, 0.15)
    click_xy(*S["tabs"]["Zero"], 0.15); time.sleep(0.4)
    z = scan(t.by_id("ZeroSubPanel"))
    print("   zero unit before:", unit_text(z["ZeroDistanceControl"]))
    if unit_text(z["ZeroDistanceControl"]) != "m":
        set_unit(z["ZeroDistanceControl"], "m", 0.15)
    fc("pre zero", num(z["ZeroDistanceControl"]), "100", 0.01, 0.15)
    if zero_ammo:
        za = scan(t.by_id("ZeroSubPanel", "ZeroAmmoSubPanel"))
        click(za["EnableCheckBox"], 0.15, 0.1); time.sleep(0.4)
        click(za["LoadButton"], 0.15); d = ammo_dialog("Load Ammunition for Zero")
        click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.15); type_text(zero_ammo[1], 0.005); keyboard.send_keys("{ENTER}"); time.sleep(1.0)
        za = scan(t.by_id("ZeroSubPanel", "ZeroAmmoSubPanel")); fc("pre zmv", num(za["MuzzleVelocityControl"]), zero_ammo[2], 0.01, 0.15)
    if params:
        click_xy(*S["tabs"]["Parameters"], 0.15); time.sleep(0.4)
        p = scan(t.by_id("ParametersSubPanel"))
        fc("pre max", num(p["MaxRangeControl"]), params[0], 0.01, 0.15)
        fc("pre step", num(p["StepControl"]), params[1], 0.01, 0.15)
    click_xy(*S["ok"], 0.15); accept_weather(0.15); time.sleep(2.0); calc_error()


# ---- pre-roll (not recorded): the whole episode once, fast, to cache every position on both windows
mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
S["traj"] = center({c.window_text(): c for c in bar.children()}["Trajectory"])
click_xy(*S["traj"], 0.2); time.sleep(0.5)
S["new"] = center(W(t.menu_item("New"))); glide(*S["new"], 0.2); time.sleep(0.5)
S["metric"] = center(W(t.menu_item("Metric")))
keyboard.send_keys("{ESC}{ESC}{ESC}"); time.sleep(0.4)
quick_shot(SUPER + ("2330",))
cache_window(W1)
quick_shot(SUB + ("1010",), SUPER + ("2330",), ("200", "25"))
cache_window(W2, ("100", "200"))                     # a new window opens on its Table tab
if t.dry:
    ImageGrab.grab(bbox=W2["canvas"], all_screens=True).save(HERE / "dry-preroll-w2.png")
t.restart(); time.sleep(1.0)
print("cached:", S, "\nW1:", W1, "\nW2:", W2, flush=True)
rest()

# ---------------------------------------------------------------- the take
t.start()
t.voice("v00")
t.voice("i01")
t.voice("i02")
t.voice("i03", lambda d: (time.sleep(d - 1.2), glide(S["traj"][0] + 40, S["traj"][1] + 120, 1.0)))

# ---- window 1: the supersonic load
t.voice("a01", lambda d: menu_new_metric(0.3, 0.95, 1.75, time.monotonic()))   # "Trajectory, (1.11) New, (1.77) and Metric, (2.87) not by chance…"
A = {}
def a_a02(d):            # "We start with the supersonic load, (2.21) because this is the load the rifle is zeroed with. (5.18) Load,
    s = time.monotonic()  #  (5.89) three hundred A A C, (7.63) the other name of the same cartridge, (9.56) and the 115 grain load."
    A.update(scan(t.by_id("AmmoLibPanel")))
    at(s, 4.8, lambda: click(A["LoadButton"], 0.35))
    dd = ammo_dialog("Load Ammunition")
    at(s, 5.6, lambda: open_folder(dd))
    at(s, 9.5, lambda: open_file(dd, SUPER[0], SUPER[1]))
t.voice("a02", a_a02)
def a_a03(d):            # "One hundred fifteen grains, (2.0) B C zero point two nine, G one. (6.08) The library says 2295 feet per
    s = time.monotonic()  #  second. (10.34) The V-COG reticle was made for 2330, (13.73) so I type that."
    a = scan(t.by_id("AmmoLibPanel"))
    A.update(w=num(a["WeightControl"]), bc=num(a["BCControl"]), mv=num(a["MuzzleVelocityControl"]))
    at(s, 0.2, lambda: point(A["w"], 0.5))
    at(s, 2.0, lambda: point(A["bc"], 0.5))
    at(s, 6.1, lambda: point(A["mv"], 0.5))
    at(s, 10.6, lambda: fc("w1 mv", A["mv"], "2330"))
t.voice("a03", a_a03)
R = {}
def a_a04(d):            # "On the Rifle tab, (1.47) a scope with milliradian clicks."
    s = time.monotonic()
    at(s, 0.2, lambda: click_xy(*S["tabs"]["Rifle"], 0.35))
    time.sleep(0.3); R.update(scan(t.by_id("RifleSubPanel")))
    at(s, 1.4, lambda: pick(R["SightPresetCombo"], PRESET))
t.voice("a04", a_a04)
def a_a05(d):            # "The V-COG sits on a tall mount: (2.69) two point nine inches above the barrel."
    s = time.monotonic()
    R.update(scan(t.by_id("RifleSubPanel")))
    at(s, 2.6, lambda: fc("w1 height", num(R["SightHeightControl"]), "2.9"))
t.voice("a05", a_a05)
def a_a06(d):            # "And the barrel: (1.28) right hand, (2.2) one turn in eight inches."
    s = time.monotonic()
    at(s, 0.6, lambda: pick(R["RiflingDirectionCombo"], "Right"))
    if unit_text(R["RiflingStepControl"]) != "in":
        at(s, 2.0, lambda: set_unit(R["RiflingStepControl"], "in"))
    fc("w1 twist", num(R["RiflingStepControl"]), "8")
t.voice("a06", a_a06)
Z = {}
def a_a07(d):            # "On the Zero tab, (1.39) the sight preset gave us one hundred yards. (4.24) We zero at one hundred metres,
    s = time.monotonic()  #  so I switch it to metres."
    at(s, 0.2, lambda: click_xy(*S["tabs"]["Zero"], 0.35))
    time.sleep(0.3); Z.update(scan(t.by_id("ZeroSubPanel")))
    at(s, 1.9, lambda: point(num(Z["ZeroDistanceControl"]), 0.6))
    at(s, 3.3, lambda: point(unit_part(Z["ZeroDistanceControl"]), 0.5))
    at(s, 6.2, lambda: set_unit(Z["ZeroDistanceControl"], "m"))
    fc("w1 zero", num(Z["ZeroDistanceControl"]), "100")
t.voice("a07", a_a07)
def a_a08(d):            # "The rifle is zeroed with the load we shoot, (2.74) so there is nothing more to set. (4.95) OK, (5.54) and standard weather."
    s = time.monotonic()
    at(s, 4.6, lambda: click_xy(*S["ok"], 0.35))
    at(s, 5.4, accept_weather)
t.voice("a08", a_a08)
t.hold(1.5, lambda _: (time.sleep(1.0), calc_error()))
def a_q01(d):            # "Now the scope. (1.46) The Reticle tab, (2.73) Load, (3.77) the V-COG for three hundred Blackout, (6.02) and Far B D C."
    s = time.monotonic()
    at(s, 1.2, lambda: click_xy(*W1["tab_reticle"], 0.4))
    at(s, 2.6, lambda: click_xy(*W1["load"], 0.4))
    at(s, 3.5, load_reticle)
    at(s, 6.0, lambda: click_xy(*W1["far"], 0.4))
    at(s, d - 0.2, rest)
t.voice("q01", a_q01)
def a_q02(d):            # "The crosshair is the zero, one hundred metres. (3.3) And every mark below it is where Trijicon put it:
    s = time.monotonic()  #  (6.51) two hundred, (7.3) three hundred, four hundred, five hundred (9.04) and six hundred metres."
    at(s, 0.3, lambda: glide(*W1["cross"], 0.7))
    for k, tt in (("200", 6.5), ("300", 7.3), ("400", 7.85), ("500", 8.4), ("600", 9.05)):
        at(s, tt, lambda k=k: glide(*W1["label"][k], 0.35))
    at(s, d - 0.3, rest)
t.voice("q02", a_q02)

# ---- window 2: the subsonic load, zeroed with the supersonic one
B = {}
def a_b01(d):            # "Now the subsonic load, (1.78) in a second trajectory. (3.48) New, (4.11) Metric, Load, (5.73) and the 208 grain Hornady."
    s = time.monotonic()
    menu_new_metric(2.8, 3.4, 4.1, s)
    B.update(scan(t.by_id("AmmoLibPanel")))
    click(B["LoadButton"], 0.3)
    dd = ammo_dialog("Load Ammunition")
    open_folder(dd, 0.3)
    at(s, 5.9, lambda: open_file(dd, SUB[0], SUB[1], 0.35))
t.voice("b01", a_b01)
def a_b02(d):            # "B C zero point six four eight, (2.65) G one. (3.85) The library says 1020 feet per second, (7.21) the reticle was made for 1010."
    s = time.monotonic()
    a = scan(t.by_id("AmmoLibPanel"))
    B.update(bc=num(a["BCControl"]), mv=num(a["MuzzleVelocityControl"]))
    at(s, 0.1, lambda: point(B["bc"], 0.5))
    at(s, 3.9, lambda: point(B["mv"], 0.5))
    at(s, 7.3, lambda: fc("w2 mv", B["mv"], "1010"))
t.voice("b02", a_b02)
def a_b03(d):            # "The same rifle: (1.54) the scope with milliradian clicks, (3.72) two point nine inches, (5.33) right hand, (6.34) one in eight."
    s = time.monotonic()
    at(s, 0.2, lambda: click_xy(*S["tabs"]["Rifle"], 0.35))
    time.sleep(0.3); r = scan(t.by_id("RifleSubPanel"))
    at(s, 1.4, lambda: pick(r["SightPresetCombo"], PRESET))
    r = scan(t.by_id("RifleSubPanel"))
    at(s, 3.6, lambda: fc("w2 height", num(r["SightHeightControl"]), "2.9"))
    at(s, 5.0, lambda: pick(r["RiflingDirectionCombo"], "Right"))
    if unit_text(r["RiflingStepControl"]) != "in":
        set_unit(r["RiflingStepControl"], "in")
    fc("w2 twist", num(r["RiflingStepControl"]), "8")
t.voice("b03", a_b03)
ZA = {}
def a_b04(d):            # "Zero at one hundred metres. (2.53) And now the important part: (4.67) Other ammunition for zero.
    s = time.monotonic()  #  (6.64) The rifle was zeroed with the supersonic load, (9.48) not with this one."
    click_xy(*S["tabs"]["Zero"], 0.35)
    time.sleep(0.3); z = scan(t.by_id("ZeroSubPanel"))
    set_unit(z["ZeroDistanceControl"], "m")
    fc("w2 zero", num(z["ZeroDistanceControl"]), "100")
    za = scan(t.by_id("ZeroSubPanel", "ZeroAmmoSubPanel"))
    at(s, 4.5, lambda: click(za["EnableCheckBox"], 0.4, 0.1))
    time.sleep(0.4); ZA.update(scan(t.by_id("ZeroSubPanel", "ZeroAmmoSubPanel")))
    at(s, 7.0, lambda: point(ZA["LoadButton"], 0.8))
t.voice("b04", a_b04)
def a_b05(d):            # "Load, (0.79) the one hundred fifteen grain load, at 2330, (5.35) the same as in the first trajectory."
    s = time.monotonic()
    click(ZA["LoadButton"], 0.25)
    dd = ammo_dialog("Load Ammunition for Zero")
    open_folder(dd, 0.3)
    open_file(dd, SUPER[0], SUPER[1], 0.3)
    za = scan(t.by_id("ZeroSubPanel", "ZeroAmmoSubPanel"))
    fc("w2 zmv", num(za["MuzzleVelocityControl"]), "2330", 0.05, 0.3)
t.voice("b05", a_b05)
def a_b06(d):            # "Parameters: (1.17) two hundred metres, every twenty-five. (4.04) OK, and standard weather."
    s = time.monotonic()
    click_xy(*S["tabs"]["Parameters"], 0.35)
    time.sleep(0.3); p = scan(t.by_id("ParametersSubPanel"))
    at(s, 1.1, lambda: fc("w2 max", num(p["MaxRangeControl"]), "200"))
    at(s, 2.6, lambda: fc("w2 step", num(p["StepControl"]), "25"))
    at(s, 3.9, lambda: click_xy(*S["ok"], 0.3))
    at(s, 4.7, accept_weather)
t.voice("b06", a_b06)
t.hold(1.5, lambda _: (time.sleep(1.0), calc_error()))

# ---- the cost
t.voice("t01", lambda d: (time.sleep(0.2), glide(*W2["d100"], 0.8)))        # "Look at one hundred metres, … forty-two centimetres low."
t.voice("t02", lambda d: (time.sleep(0.1), glide(*W2["d200"], 0.6)))        # "At two hundred metres it is almost two metres low. …"
def a_t03(d):            # "This is not a mistake. It is what a shared zero costs, and now you see it before you shoot."
    s = time.monotonic()
    at(s, 1.0, rest)
    at(s, d - 1.4, lambda: click_xy(*W2["tab_reticle"], 0.8))
t.voice("t03", a_t03)

# ---- the same scope for the subsonic load
def a_r01(d):            # "The same reticle, (1.45) and Far B D C."
    s = time.monotonic()
    click_xy(*W2["load"], 0.35)
    load_reticle(0.02)
    at(s, 2.0, lambda: click_xy(*W2["far"], 0.35))
    at(s, d - 0.2, rest)
t.voice("r01", a_r01)
def a_r02(d):            # "Trijicon uses one ladder for both loads. (3.17) For the supersonic load it starts at the crosshair.
    s = time.monotonic()  #  (6.28) For the subsonic load it starts lower, (8.6) at the top of the ladder: (10.15) fifty, (10.85) one hundred, (11.68) one hundred fifty."
    at(s, 4.6, lambda: glide(*W2["cross"], 0.7))
    at(s, 8.8, lambda: glide(*W2["bar"]["300"], 0.7))
    at(s, 10.15, lambda: glide(*W2["bar"]["300"], 0.1))
    at(s, 10.8, lambda: glide(*W2["bar"]["400"], 0.35))
    at(s, 11.6, lambda: glide(*W2["bar"]["500"], 0.35))
t.voice("r02", a_r02)
def a_r03(d):            # "Far B D C labels the marks after the Split at distance. (4.79) By default it is the zero, one hundred metres,
    s = time.monotonic()  #  (8.12) so the top of the ladder has no label. (11.02) The subsonic count starts at fifty, (13.23) so I type fifty."
    at(s, 2.6, lambda: glide(*W2["split"], 0.8))
    at(s, 8.4, lambda: glide(*W2["label"]["300"], 0.8))
    at(s, 12.9, lambda: click_xy(*W2["split"], 0.4))
    keyboard.send_keys("{END}+{HOME}", pause=0.03); time.sleep(0.05)
    keyboard.send_keys("50", pause=0.08); keyboard.send_keys("{TAB}")
    at(s, d - 0.3, rest)
t.voice("r03", a_r03)
def a_r04(d):            # "And the program agrees: (1.74) fifty-five, one hundred two, and one hundred fifty-one metres."
    s = time.monotonic()
    at(s, 1.6, lambda: glide(*W2["label"]["300"], 0.4))
    at(s, 2.7, lambda: glide(*W2["label"]["400"], 0.4))
    at(s, 4.0, lambda: glide(*W2["label"]["500"], 0.4))
    if t.dry:
        ImageGrab.grab(bbox=W2["canvas"], all_screens=True).save(HERE / "dry-r04.png")
t.voice("r04", a_r04)
def a_r05(d):            # "One scope, (1.05) two loads, (2.02) one zero. (3.16) Tell the program which load the zero was made with…"
    s = time.monotonic()
    # App defect: the DisplayMode radio group is shared by all trajectory windows (GroupName scope = the main
    # window), so r01's Far BDC in window 2 cleared window 1's. Re-select it after switching back.
    at(s, 0.4, lambda: click_xy(*W1["title"], 0.5))
    at(s, 1.2, lambda: click_xy(*W1["far"], 0.45))
    at(s, 2.3, lambda: glide(*W1["label"]["300"], 0.6))
    at(s, 3.6, rest)
t.voice("r05", a_r05)
t.voice("n01")
t.finish()
