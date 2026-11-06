"""Episode 10 take: your own drag table. Part 1 builds the Scenar-L curve from five G7 BCs (Tools → Approximate
Drag Table → From BC Curve), puts it on bc2-demo and compares it with the ep. 6 radar file; part 2 builds the
Warner .338 Flatline 285 gr from its published velocities (Load Csv + the air of the range) and checks it in a
prepared trajectory (338-flatline.trajectory, Warner's G7 0.484). Cards b01, v01, v04 are overlaid after assembly.

Pre-roll (not recorded): the whole story once, fast, caching every menu item, dialog field and table cell in the
same window order as the take; then the drg files it wrote are deleted and a fresh app records the take.
File dialogs: the full path is typed into the focused file-name box (no list lookups — one hung 10 s in ep. 9).
Usage: python record.py [--dry]
"""
import json, pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8", line_buffering=True)   # os._exit skips the flush
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *
from PIL import ImageGrab
import win32clipboard

HERE = pathlib.Path(__file__).resolve().parent
DEMO = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo")
BC_DRG, VEL_DRG = DEMO / "scenar-l-5bc.drg", DEMO / "338 flatline.drg"
KNOTS = [("0.250", "2600"), ("0.240", "2200"), ("0.244", "1800"), ("0.263", "1400"), ("0.293", "1100")]
_st = EXE.parent / "appstate.json"
_j = json.loads(_st.read_text()); _j["LastMeasurementSystem"] = 1   # Imperial dialogs
_j["ChildWindowHeight"] = 560          # the .338 window (4th, cascaded lowest) must show its 1500 yd row
_st.write_text(json.dumps(_j, indent=2))
for f in (BC_DRG, VEL_DRG):
    f.unlink(missing_ok=True)

t = Take(__file__)
W = lambda s: s.wrapper_object()
S = {}


# ---------------------------------------------------------------- helpers
def minimize_strays():
    for w in Desktop(backend="uia").windows():
        try:
            r = w.rectangle(); name = w.window_text()
            if r.left < -100 and ("OBS" in name or "Notepad" in name or "Блокнот" in name):
                w.minimize()
        except Exception:
            pass


def visible_item(pred):
    """A menu item on screen now (closed menus stay in the UIA tree). pred: id/title string or a function."""
    test = pred if callable(pred) else (lambda e: pred in (e.element_info.automation_id, e.window_text()))
    for _ in range(25):
        for e in t.main.wrapper_object().descendants(control_type="MenuItem"):
            if test(e):
                r = e.rectangle()
                if r.width() > 0 and r.height() > 0 and not e.element_info.element.CurrentIsOffscreen:
                    return e
        time.sleep(0.2)
    raise LookupError(str(pred))


def bar_item(name):
    bar = [m for m in t.main.wrapper_object().descendants(control_type="Menu") if not m.element_info.automation_id][0]
    return [c for c in bar.children() if c.window_text() == name][0]


def menu_path(top, items, times=None, s=None, g=0.3):
    """Cached centres. Out of the bar straight down; into a submenu along the parent's row first."""
    pts = [S[top]] + [S[i] for i in items]
    for k, (x, y) in enumerate(pts):
        if times:
            at(s, times[k], lambda: None)
        if k == 0:
            click_xy(x, y, g)
        else:
            px, py = pts[k - 1]
            glide(px, y, g * 0.6) if k == 1 else glide(x, py, g * 0.6)
            if k < len(pts) - 1:
                glide(x, y, g * 0.4); time.sleep(0.35)        # hover a submenu parent open
            else:
                click_xy(x, y, g * 0.4)


def measure_menu(top, *keys):
    """Pre-roll: open `top`, hover down the chain of keys, cache each centre; ESC out."""
    click(bar_item(top), 0.15); time.sleep(0.5)
    S[top] = center(bar_item(top)); prev = S[top]
    for i, (name, key) in enumerate(keys):
        e = visible_item(key); S[name] = center(e)
        glide(prev[0], S[name][1], 0.15) if i == 0 else glide(S[name][0], prev[1], 0.15)
        glide(*S[name], 0.15); time.sleep(0.6); prev = S[name]
    keyboard.send_keys("{ESC}{ESC}{ESC}"); time.sleep(0.4)


def ids(win):
    out = {}
    for e in W(win).descendants():
        a = e.element_info.automation_id
        if a and a not in out:
            out[a] = e
    return out


def num(e):
    return center([c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0])


def typed_path(title, path, pause=0.012):
    """A native file dialog: its file-name box has the focus — type the whole path and press Enter."""
    d = t.main.child_window(class_name="#32770", title=title); d.wait("visible", timeout=10); time.sleep(0.35)
    win32clipboard.OpenClipboard(); win32clipboard.EmptyClipboard()
    win32clipboard.SetClipboardText(str(path), win32clipboard.CF_UNICODETEXT); win32clipboard.CloseClipboard()
    keyboard.send_keys("^a^v", pause=0.05); time.sleep(0.25); keyboard.send_keys("{ENTER}")   # pasted: typing dropped a letter at speed
    c = t.main.child_window(title_re="Confirm Save As.*")
    if title.startswith("Save") and c.exists(timeout=0.4):
        c.child_window(title="Yes", control_type="Button").click_input()


def put(xy, text, pause=0.09, secs=0.3):
    click_xy(*xy, secs); type_text(text, pause)


def status(cls):
    try:
        print("         status:", t.main.child_window(class_name=cls).child_window(auto_id="StatusText").window_text(), flush=True)
    except Exception as e:
        print("         status: ?", e, flush=True)


def calc_error():
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.2):
        print("STOPPED: 'Cannot calculate this shot'"); os._exit(1)


def confirm_yes():
    c = t.main.child_window(class_name="Window", title="Confirm")
    if c.exists(timeout=0.4):
        click(c.child_window(control_type="Button", title="Yes"), 0.2)


def cells(prefix, keep):
    """Cache the centres of Range / Velocity / Drop / Hold in the newest trajectory window."""
    tv = t.tv()
    heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
    col = {"range": 0, "vel": 1, "drop": next(i for i, h in enumerate(heads) if h.startswith("Drop")),
           "hold": next(i for i, h in enumerate(heads) if h.startswith("Hold"))}
    for row in tv.descendants(control_type="DataItem"):
        c = row.descendants(control_type="Text")
        if c and c[0].window_text() in keep:
            k = c[0].window_text().replace(",", "")
            for name, i in col.items():
                S[f"{prefix}_{name}{k}"] = center(c[i])
            print(f"   {prefix} {k}: hold {c[col['hold']].window_text()} drop {c[col['drop']].window_text()}")


# ---------------------------------------------------------------- pre-roll: the whole story, fast
minimize_strays()
measure_menu("Trajectory", ("open", "MenuFileOpen"))
measure_menu("View", ("edit", "MenuViewEditParameters"))
measure_menu("View", ("compare", lambda e: e.window_text() == "Compare"), ("add", "MenuViewCompareAdd"))
measure_menu("Tools", ("adt", "Approximate Drag Table"), ("fromBc", "MenuToolsDrgFromBc"))
measure_menu("Tools", ("adt", "Approximate Drag Table"), ("fromVel", "MenuToolsDrgFromVelocities"))
S["Windows"] = center(bar_item("Windows"))
print("menus:", {k: S[k] for k in ("Trajectory", "open", "View", "edit", "compare", "add", "Tools", "adt", "fromBc", "fromVel")})


def open_traj(name, g=0.15):
    menu_path("Trajectory", ["open"], g=g); typed_path("Open Trajectory", DEMO / name)
    for _ in range(60):
        if t.main.wrapper_object().descendants(control_type="DataItem"):
            break
        time.sleep(0.1)
    time.sleep(1.2)


open_traj("bc2-demo.trajectory"); cells("w1", ("1,000",))

# BC dialog
menu_path("Tools", ["adt", "fromBc"], g=0.15)
bcd = t.main.child_window(class_name="ApproximateDrgFromBcDialog"); bcd.wait("visible", timeout=10); time.sleep(0.8)
B = ids(bcd)
S.update(bc_name=center(B["NameBox"]), bc_w=num(B["WeightControl"]), bc_d=num(B["DiameterControl"]),
         bc_base=center(B["BaseTableCombo"]), bc_num=num(B["BcControl"]), bc_vel=num(B["VelocityControl"]),
         bc_add=center(B["AddButton"]), bc_save=center(B["SaveButton"]), bc_close=center(B["CloseButton"]),
         bc_air=center(B["AtmosphereText"]), bc_grid=center(B["KnotsGrid"], 0.3, 0.5))
S["bc_table"] = center([c for c in B["BcControl"].descendants() if c.element_info.automation_id == "TablePart"][0])
S["bc_mach"] = center([e for e in W(bcd).descendants() if e.element_info.automation_id == "PART_TextBox"][0])
S["bc_note"] = center([e for e in W(bcd).descendants() if e.window_text().startswith("Weight and diameter")][0], 0.3)
put(S["bc_name"], "Scenar-L 175, five G7 BCs", 0.01, 0.15); put(S["bc_w"], "175", 0.01, 0.15); put(S["bc_d"], "0.308", 0.01, 0.15)
click_xy(*S["bc_table"], 0.15); time.sleep(0.5)
g7 = t.item("G7"); S["bc_g7"] = center(W(g7)); click_xy(*S["bc_g7"], 0.15); time.sleep(0.3)
for bc, v in KNOTS:
    put(S["bc_num"], bc, 0.01, 0.15); put(S["bc_vel"], v, 0.01, 0.15); click_xy(*S["bc_add"], 0.15)
print("   bc status:", B["StatusText"].window_text())
click_xy(*S["bc_save"], 0.15); typed_path("Save Custom Drag Table", BC_DRG); time.sleep(0.8)
print("   bc save:", B["StatusText"].window_text())
click_xy(*S["bc_close"], 0.15); time.sleep(0.8)

# shot dialog: Browse the new file, rename, OK
menu_path("View", ["edit"], g=0.15); t.dlg.wait("visible", timeout=10); time.sleep(0.8)
A = ids(t.by_id("AmmoLibPanel"))
S.update(sd_browse=center(A["BrowseTableButton"]), sd_name=center(A["NameTextBox"]),
         sd_ok=center(W(t.dlg.child_window(control_type="Button", title="OK"))))
click_xy(*S["sd_browse"], 0.15); typed_path("Open Custom Drag Table", BC_DRG); time.sleep(0.8)
put(S["sd_name"], "Five G7 BCs", 0.01, 0.15)
click_xy(*S["sd_ok"], 0.15); time.sleep(1.5); confirm_yes(); calc_error(); time.sleep(0.5)
cells("w1", ("1,000",))

open_traj("bc2-demo-radar.trajectory"); cells("w2", ("1,000",))
menu_path("View", ["compare", "add"], g=0.15); time.sleep(1.5)
click_xy(*S["Windows"], 0.15); time.sleep(0.5)
S["win_bc2"] = center(visible_item(lambda e: e.window_text() == "1 Five G7 BCs"))
print("   windows menu:", [e.window_text() for e in t.main.wrapper_object().descendants(control_type="MenuItem")
                          if e.rectangle().width() > 0 and not e.element_info.element.CurrentIsOffscreen])
click_xy(*S["win_bc2"], 0.15); time.sleep(0.6)
menu_path("View", ["compare", "add"], g=0.15); time.sleep(1.5)
ch = [c for c in t.main.wrapper_object().descendants() if c.element_info.class_name == "TrajectoryChartControl"]
print("   chart controls:", [(c.rectangle()) for c in ch])
R = ch[-1].rectangle(); S["chart"] = (R.left, R.top, R.right, R.bottom)
ImageGrab.grab(bbox=S["chart"], all_screens=True).save(HERE / ("dry-compare.png" if t.dry else "pre-compare.png"))

# velocities dialog + atmosphere
menu_path("Tools", ["adt", "fromVel"], g=0.15)
vd = t.main.child_window(class_name="ApproximateDrgFromVelocitiesDialog"); vd.wait("visible", timeout=10); time.sleep(0.8)
V = ids(vd)
S.update(v_name=center(V["NameBox"]), v_w=num(V["WeightControl"]), v_d=num(V["DiameterControl"]),
         v_import=center(V["ImportButton"]), v_air=center(V["AtmosphereButton"]), v_save=center(V["SaveButton"]),
         v_close=center(V["CloseButton"]), v_status=center(V["StatusText"], 0.25), v_grid=center(V["ReadingsGrid"], 0.3, 0.6))
put(S["v_name"], "338 Flatline 285 gr", 0.01, 0.15); put(S["v_w"], "285", 0.01, 0.15); put(S["v_d"], "0.338", 0.01, 0.15)
click_xy(*S["v_import"], 0.15); typed_path("Load Measured Velocities", DEMO / "338-flatline-velocities.csv"); time.sleep(0.8)
print("   csv:", V["StatusText"].window_text())
click_xy(*S["v_air"], 0.15)
ad = t.main.child_window(class_name="AtmosphereDialog"); ad.wait("visible", timeout=10); time.sleep(1.5)
AT = ids(ad)
S.update(a_alt=num(AT["AltitudeControl"]), a_p=num(AT["PressureControl"]), a_t=num(AT["TemperatureControl"]),
         a_h=center(AT["HumidityTextBox"]), a_ok=center(W(ad.child_window(control_type="Button", title="OK"))))
for k, v in (("a_alt", "6000"), ("a_p", "23.98"), ("a_t", "78"), ("a_h", "25")):
    put(S[k], v, 0.01, 0.15)
S["a_ok"] = center(W(ad.child_window(control_type="Button", title="OK")))   # settled position
print("   atmosphere dialog:", W(ad).rectangle(), "OK at", S["a_ok"])
click_xy(*S["a_ok"], 0.15); time.sleep(0.6)
print("   air:", V["StatusText"].window_text())
click_xy(*S["v_save"], 0.15); typed_path("Save Custom Drag Table", VEL_DRG); time.sleep(0.8)
print("   vel save:", V["StatusText"].window_text())
click_xy(*S["v_close"], 0.15); time.sleep(0.8)

open_traj("338-flatline.trajectory"); cells("w4", ("0", "100", "1,500"))
_grid = [g for g in t.tv().descendants(control_type="DataGrid")][0].rectangle()
print("   w4 grid", _grid, "1500 row at", S["w4_hold1500"])
if S["w4_hold1500"][1] > _grid.bottom - 20:
    print("STOPPED: the 1500 yd row is not fully visible in the .338 window"); os._exit(1)
menu_path("View", ["edit"], g=0.15); t.dlg.wait("visible", timeout=10); time.sleep(0.8)
click_xy(*S["sd_browse"], 0.15); typed_path("Open Custom Drag Table", VEL_DRG); time.sleep(0.8)
click_xy(*S["sd_ok"], 0.15); time.sleep(1.5); confirm_yes(); calc_error(); time.sleep(0.5)
cells("w4", ("1,500",))
json.dump(S, open(HERE / "positions.json", "w"), indent=1)

for f in (BC_DRG, VEL_DRG):
    f.unlink(missing_ok=True)
t.restart(); minimize_strays()


# ---------------------------------------------------------------- the take
def rest_edge():
    glide(MON[0] + MON[2] - 4, MON[1] + 720, 0.5)


t.start()
t.voice("v00")
t.voice("i01")
t.voice("i02")
t.voice("b01", lambda d: rest_edge())                       # card-bc-by-speed overlaid


def a_b02(d):            # "Tools, (0.86) Approximate Drag Table, (2.63) From B C Curve."
    s = time.monotonic()
    menu_path("Tools", ["adt", "fromBc"], times=(0.0, 0.8, 2.5), s=s, g=0.35)
    t.main.child_window(class_name="ApproximateDrgFromBcDialog").wait("visible", timeout=10)
t.voice("b02", a_b02)


def a_b03(d):            # "A name. (1.22) The weight, one hundred seventy-five grains, (4.23) and the diameter, point three oh eight. (6.95) The program needs both…"
    s = time.monotonic()
    put(S["bc_name"], "Scenar-L 175, five G7 BCs", 0.02, 0.35)
    at(s, 1.4, lambda: click_xy(*S["bc_w"], 0.4))
    at(s, 2.2, lambda: type_text("175", 0.15))
    at(s, 4.3, lambda: click_xy(*S["bc_d"], 0.4))
    at(s, 5.2, lambda: type_text("0.308", 0.12))
    at(s, 7.4, lambda: glide(*S["bc_note"], 0.7))
t.voice("b03", a_b03)


def a_b04(d):            # "The base table is G seven, (2.27) because our coefficients are G seven numbers."
    s = time.monotonic()
    at(s, 0.3, lambda: glide(*S["bc_base"], 0.6))
    at(s, 2.4, lambda: click_xy(*S["bc_table"], 0.4))
    at(s, 3.4, lambda: click_xy(*S["bc_g7"], 0.3))
t.voice("b04", a_b04)


def a_b05(d):            # "The first point: (1.32) zero point two five oh, (3.49) at two thousand six hundred feet per second. (6.31) Add.
                         #  (7.21) The program turns the speed into a Mach number, (9.97) for standard air."
    s = time.monotonic()
    at(s, 0.8, lambda: click_xy(*S["bc_num"], 0.4))
    at(s, 1.4, lambda: type_text("0.250", 0.12))
    at(s, 3.0, lambda: click_xy(*S["bc_vel"], 0.4))
    at(s, 3.6, lambda: type_text("2600", 0.15))
    at(s, 5.8, lambda: click_xy(*S["bc_add"], 0.4))
    at(s, 7.4, lambda: glide(*S["bc_mach"], 0.6))
    at(s, 9.6, lambda: glide(*S["bc_air"], 0.6))
t.voice("b05", a_b05)


def a_b06(d):            # four sentences, starting at 0, 3.46, 6.68, 9.83
    s = time.monotonic()
    for (bc, v), t0 in zip(KNOTS[1:], (0.0, 3.3, 6.5, 9.6)):
        at(s, t0, lambda: None)
        put(S["bc_num"], bc, 0.03, 0.2); put(S["bc_vel"], v, 0.03, 0.2); click_xy(*S["bc_add"], 0.2)
t.voice("b06", a_b06)


def a_b07(d):            # "Save D R G. (2.27) I put it next to the radar file. (4.49) Close."
    s = time.monotonic()
    click_xy(*S["bc_save"], 0.3)
    typed_path("Save Custom Drag Table", BC_DRG)
    at(s, 4.2, lambda: click_xy(*S["bc_close"], 0.35))
t.voice("b07", a_b07)


def open_take(name, s, t_traj, t_open):
    menu_path("Trajectory", ["open"], times=(t_traj, t_open), s=s, g=0.35)
    typed_path("Open Trajectory", DEMO / name)


def a_b08(d):            # "Now let's check it. (1.56) Trajectory, (2.53) Open: (3.33) our file from episode two, with one coefficient.
                         #  (6.74) At one thousand yards we hold ten point oh one."
    s = time.monotonic()
    open_take("bc2-demo.trajectory", s, 1.3, 2.3)
    at(s, 6.9, lambda: glide(*S["w1_hold1000"], 0.7))
t.voice("b08", a_b08)


def a_b09(d):            # "Edit Parameters, (1.38) Browse, (2.33) and the new file. (3.84) A new name: (4.86) five G seven B C's. (7.46) OK.
                         #  (8.19) Now we hold ten point two eight."
    s = time.monotonic()
    menu_path("View", ["edit"], times=(0.0, 0.6), s=s, g=0.3)
    t.dlg.wait("visible", timeout=10)
    at(s, 1.4, lambda: click_xy(*S["sd_browse"], 0.35))
    typed_path("Open Custom Drag Table", BC_DRG)
    at(s, 4.0, lambda: click_xy(*S["sd_name"], 0.4))
    at(s, 4.9, lambda: type_text("Five G7 BCs", 0.05))
    at(s, 7.2, lambda: click_xy(*S["sd_ok"], 0.35))
    time.sleep(0.9); confirm_yes(); calc_error()
    at(s, 8.4, lambda: glide(*S["w1_hold1000"], 0.6))
t.voice("b09", a_b09)


def a_b10(d):            # "And the real radar curve from episode six: (3.09) ten point three oh. (4.94) Five coefficients came within one inch…"
    s = time.monotonic()
    open_take("bc2-demo-radar.trajectory", s, 0.0, 0.7)
    at(s, 3.2, lambda: glide(*S["w2_hold1000"], 0.6))
    at(s, 5.4, lambda: glide(*S["w2_drop1000"], 0.6))
    at(s, d - 0.6, rest)
t.voice("b10", a_b10)


def a_b11(d):            # "Let's put both on one chart. (2.25) In the radar window: View, (3.70) Compare, (4.48) Add."
    s = time.monotonic()
    menu_path("View", ["compare", "add"], times=(2.7, 3.5, 4.3), s=s, g=0.3)
t.voice("b11", a_b11)


def a_b12(d):            # "Windows, (0.90) our five coefficients. (2.74) View, (3.45) Compare, (4.33) Add again. (5.39) And the chart: the two curves…"
    s = time.monotonic()
    at(s, 0.0, lambda: click_xy(*S["Windows"], 0.3))
    at(s, 1.0, lambda: (glide(S["Windows"][0], S["win_bc2"][1], 0.3), click_xy(*S["win_bc2"], 0.2)))
    menu_path("View", ["compare", "add"], times=(2.7, 3.4, 4.2), s=s, g=0.3)
    l, tp, r, b = S["chart"]
    at(s, 5.6, lambda: glide(l + int((r - l) * 0.2), tp + int((b - tp) * 0.35), 0.5))
    at(s, 6.2, lambda: glide(l + int((r - l) * 0.85), tp + int((b - tp) * 0.75), 2.0))
t.voice("b12", a_b12)
t.voice("v01", lambda d: rest_edge())                        # card-warner-sheet overlaid


def a_v02(d):            # "Tools, (0.82) Approximate Drag Table, (2.51) From Measured Velocities."
    s = time.monotonic()
    menu_path("Tools", ["adt", "fromVel"], times=(0.0, 0.8, 2.4), s=s, g=0.35)
    t.main.child_window(class_name="ApproximateDrgFromVelocitiesDialog").wait("visible", timeout=10)
t.voice("v02", a_v02)


def a_v03(d):            # "The name, (0.98) the weight, (1.94) two hundred eighty-five grains, (4.05) and the diameter, (5.14) point three three eight."
    s = time.monotonic()
    put(S["v_name"], "338 Flatline 285 gr", 0.02, 0.3)
    at(s, 1.3, lambda: click_xy(*S["v_w"], 0.35))
    at(s, 2.0, lambda: type_text("285", 0.15))
    at(s, 4.1, lambda: click_xy(*S["v_d"], 0.35))
    at(s, 5.1, lambda: type_text("0.338", 0.1))
t.voice("v03", a_v03)


def a_v04(d):            # card-csv overlaid 0–8.6 s. "… each with its unit. (8.91) Load C S V. (10.53) Sixteen readings."
    s = time.monotonic()
    at(s, 0.2, rest_edge)
    at(s, 8.3, lambda: click_xy(*S["v_import"], 0.4))
    typed_path("Load Measured Velocities", DEMO / "338-flatline-velocities.csv")
    at(s, 10.6, lambda: glide(*S["v_grid"], 0.5))
    status("ApproximateDrgFromVelocitiesDialog")
t.voice("v04", a_v04)


def a_v05(d):            # "Before I save, one more thing: (2.48) the air. (3.74) The program has to know the air…"
    s = time.monotonic()
    at(s, 2.4, lambda: glide(*S["v_air"], 0.6))
t.voice("v05", a_v05)
t.voice("v06", lambda d: (time.sleep(1.0), rest()))
t.voice("v07", lambda d: (time.sleep(d - 1.2), glide(*S["v_air"], 0.8)))


def a_v08(d):            # "Set Atmosphere. (1.64) Six thousand feet, (3.10) twenty-three point nine eight inches of pressure, (5.71) seventy-eight degrees,
                         #  (8.34) and dry air: twenty-five percent. (9.87) OK."
    s = time.monotonic()
    click_xy(*S["v_air"], 0.2)
    t.main.child_window(class_name="AtmosphereDialog").wait("visible", timeout=10)
    at(s, 1.3, lambda: click_xy(*S["a_alt"], 0.3))
    at(s, 1.8, lambda: type_text("6000", 0.1))
    at(s, 3.0, lambda: click_xy(*S["a_p"], 0.3))
    at(s, 3.4, lambda: type_text("23.98", 0.1))
    at(s, 5.6, lambda: click_xy(*S["a_t"], 0.3))
    at(s, 6.0, lambda: type_text("78", 0.12))
    at(s, 8.2, lambda: click_xy(*S["a_h"], 0.3))
    at(s, 8.7, lambda: type_text("25", 0.12))
    at(s, 9.6, lambda: click_xy(*S["a_ok"], 0.3))
t.voice("v08", a_v08)


def a_v09(d):            # "Save D R G. (2.27) Close."
    click_xy(*S["v_save"], 0.25)
    typed_path("Save Custom Drag Table", VEL_DRG)
    time.sleep(0.4); click_xy(*S["v_close"], 0.3)
t.voice("v09", a_v09)


def a_v10(d):            # "I prepared a trajectory for this bullet: (2.49) three thousand seventy-nine feet per second, (5.17) a zero at one hundred yards,
                         #  (6.99) out to fifteen hundred. (8.81) It uses Warner's own G seven coefficient… (13.94) At fifteen hundred yards we hold ten point three six."
    s = time.monotonic()
    open_take("338-flatline.trajectory", s, 0.0, 0.7)
    at(s, 2.6, lambda: glide(*S["w4_vel0"], 0.6))
    at(s, 5.2, lambda: glide(*S["w4_range100"], 0.6))
    at(s, 7.0, lambda: glide(*S["w4_range1500"], 0.8))
    at(s, 9.0, rest)
    at(s, 13.8, lambda: glide(*S["w4_hold1500"], 0.8))
t.voice("v10", a_v10)


def browse_vel(secs):    # silent: Edit Parameters, Browse, the new file, OK
    menu_path("View", ["edit"], g=0.3)
    t.dlg.wait("visible", timeout=10); time.sleep(0.2)
    click_xy(*S["sd_browse"], 0.35)
    typed_path("Open Custom Drag Table", VEL_DRG)
    time.sleep(0.6); click_xy(*S["sd_ok"], 0.35); time.sleep(0.8); confirm_yes(); calc_error()
t.hold(5.0, browse_vel)
t.voice("v11", lambda d: (time.sleep(1.2), glide(*S["w4_hold1500"], 0.5)))
t.voice("x01", lambda d: (time.sleep(1.5), rest()))
t.voice("x02")
t.finish()
print("drg files written:", BC_DRG.exists(), VEL_DRG.exists())
