"""Episode 9 take: shooting at an angle. bc2-demo (ep. 2's shot), the flat hold, the chart Over Muzzle Level
(flat, then 30 deg uphill, zoomed at the end), back to Over Line of Sight, 30 deg up and down holds; the
rifleman's rule card (r01) is overlaid after assembly (overlay_cards.py).

Pre-roll (not recorded): opens bc2-demo once to cache every menu, tab, dialog and table position, and reads
the chart geometry off screenshots (red = line of sight, blue = bullet), including the zoomed view; then a
fresh app for the take.  Usage: python record.py [--dry]
"""
import pathlib, re, sys, time
sys.stdout.reconfigure(encoding="utf-8", line_buffering=True)   # os._exit skips the flush
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *
from PIL import ImageGrab
import numpy as np

HERE = pathlib.Path(__file__).resolve().parent
DEMO = r"E:\ngeht\Documents\Ballistic Calculator demo\bc2-demo.trajectory"
P = ("ParametersSubPanel",)
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


def visible_item(key):
    """A menu item by automation id or title — only one on screen now (closed menus stay in the UIA tree)."""
    for _ in range(25):
        for e in t.main.wrapper_object().descendants(control_type="MenuItem"):
            if key in (e.element_info.automation_id, e.window_text()):
                r = e.rectangle()
                if r.width() > 0 and r.height() > 0 and not e.element_info.element.CurrentIsOffscreen:
                    return e
        time.sleep(0.2)
    raise LookupError(key)


def bar_item(name):
    bar = [m for m in t.main.wrapper_object().descendants(control_type="Menu") if not m.element_info.automation_id][0]
    return [c for c in bar.children() if c.window_text() == name][0]


def menu_path(top, items, times=None, s=None, g=0.35):
    """top + submenu items from cached centres. Out of the bar straight down; into a submenu along the
    parent's row first (a diagonal crosses the next header / the next row and switches menus)."""
    pts = [S[top]] + [S[i] for i in items]
    for k, (x, y) in enumerate(pts):
        if times:
            at(s, times[k], lambda: None)
        if k == 0:
            click_xy(x, y, g)
        else:
            px, py = pts[k - 1]
            if k == 1:
                glide(px, y, g * 0.6)
            else:
                glide(x, py, g * 0.6)
            click_xy(x, y, g * 0.4)


def check(label, spec, want):
    v = W(spec).get_value().replace(",", "")
    print(f"         {label:12s} = {v!r}", flush=True)
    if v != want:
        print(f"STOPPED: {label} read back {v!r}, wanted {want!r}"); os._exit(1)


def fc(label, xy, spec, text, pause=0.09, secs=0.35):
    click_xy(*xy, secs); keyboard.send_keys("{END}+{HOME}{DEL}"); time.sleep(0.05)   # DEL: a selected field rejects "-"
    keyboard.send_keys(text, pause=pause); time.sleep(0.25)
    check(label, spec, text)


def calc_error():
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.3):
        print("STOPPED: 'Cannot calculate this shot'"); os._exit(1)


def grab(r):
    return np.array(ImageGrab.grab(bbox=(r.left, r.top, r.right, r.bottom), all_screens=True))[:, :, :3].astype(int)


def geometry(r, r_left=0.0, r_right=1000.0, edges=False):
    """Screen positions of the two curves, by colour. Red = line of sight, blue = bullet (Drop). x of range
    rr is linear between the leftmost red pixel (r_left) and the rightmost red dot (r_right)."""
    a = grab(r)
    red = (a[:, :, 0] > 200) & (a[:, :, 1] < 80) & (a[:, :, 2] < 80)
    blue = (a[:, :, 2] > 200) & (a[:, :, 0] < 80) & (a[:, :, 1] < 80)
    xs = np.where(red.any(axis=0))[0]
    xl, xr = (xs.min(), xs.max()) if edges else (xs.min() + (0 if r_left else 4), xs.max() - 4)
    X = lambda rr: int(xl + (xr - xl) * (rr - r_left) / (r_right - r_left))

    def top(mask, col):
        for c in sorted(range(col - 6, col + 7), key=lambda c: abs(c - col)):
            ys = np.where(mask[:, c])[0]
            if len(ys):
                return int(ys.min())
        return None
    g = {"X": lambda rr: r.left + X(rr),
         "red": lambda rr: (r.left + X(rr), r.top + top(red, X(rr))),
         "blue": lambda rr: (r.left + X(rr), r.top + (top(blue, X(rr)) if top(blue, X(rr)) is not None else top(red, X(rr))))}
    return g


def wheel_zoom(xy, notches=12, secs=1.4):
    """ScottPlot keeps the point under the cursor fixed. A second middle-drag rectangle in one session comes out
    with the first rectangle's X range (seen 2026-11-05), so only the first zoom is a rectangle."""
    glide(*xy, 0.3)
    for _ in range(notches):
        mouse.scroll(coords=xy, wheel_dist=1); time.sleep(secs / notches)


def end_cols(r, offsets):
    """After a wheel zoom at the 1000 yd dot: the curves' tops in columns `off` px left of that dot."""
    a = grab(r)
    red = (a[:, :, 0] > 200) & (a[:, :, 1] < 80) & (a[:, :, 2] < 80)
    blue = (a[:, :, 2] > 200) & (a[:, :, 0] < 80) & (a[:, :, 1] < 80)
    xr = np.where(red.any(axis=0))[0].max() - 4
    out = {}
    for off in offsets:
        c = xr - off
        out[f"r{off}"] = (r.left + c, r.top + int(np.where(red[:, c])[0].min()))
        out[f"b{off}"] = (r.left + c, r.top + int(np.where(blue[:, c])[0].min()))
    return out


def snap(g, keys):
    """Freeze a geometry into plain points (the lambdas read the screenshot taken in the pre-roll)."""
    return {k: g[c](rr) for k, (c, rr) in keys.items()}


# ---------------------------------------------------------------- pre-roll
minimize_strays()
# Trajectory -> Open (typed path, fast)
click(bar_item("Trajectory"), 0.15); time.sleep(0.4)
S["traj"] = center(bar_item("Trajectory")); S["open"] = center(visible_item("MenuFileOpen"))
glide(S["traj"][0], S["open"][1], 0.15); click_xy(*S["open"], 0.15)
d = t.main.child_window(class_name="#32770", title="Open Trajectory"); d.wait("visible", timeout=10); time.sleep(1.0)
f = d.child_window(title_re=r"bc2-demo(\.trajectory)?$", control_type="ListItem", found_index=0)
if not f.exists(timeout=5):
    print("STOPPED: bc2-demo is not listed in the Open dialog's folder"); os._exit(1)
S["file"] = center(W(f), 0.15); S["openbtn"] = center(W(d.child_window(auto_id="1", control_type="Button")))
click_xy(*S["file"], 0.15); time.sleep(0.3); click_xy(*S["openbtn"], 0.15); time.sleep(2.5)
tv = t.tv(); R = tv.rectangle()
heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
hold_col = next(i for i, h in enumerate(heads) if h.startswith("Hold"))
for row in tv.descendants(control_type="DataItem"):
    c = row.descendants(control_type="Text")
    if c and c[0].window_text() == "1,000":
        S["hold1000"] = center(c[hold_col]); print("   flat hold at 1000:", c[hold_col].window_text())
S["tab_table"] = center(t.tv_find("TabItem", "Table", tv=tv)); S["tab_chart"] = center(t.tv_find("TabItem", "Chart", tv=tv))
# View menu and its Drop submenu
click(bar_item("View"), 0.15); time.sleep(0.4)
S["view"] = center(bar_item("View")); S["edit"] = center(visible_item("MenuViewEditParameters"))
S["drop"] = center(visible_item("Drop"))
glide(S["view"][0], S["drop"][1], 0.15); glide(*S["drop"], 0.15); time.sleep(0.6)
S["ml"] = center(visible_item("MenuViewDropMuzzleLevel")); S["los"] = center(visible_item("MenuViewDropLineOfSight"))
glide(S["ml"][0], S["drop"][1], 0.15); time.sleep(0.1); click_xy(*S["ml"], 0.15); time.sleep(1.0)   # Over Muzzle Level
click_xy(*S["tab_chart"], 0.15); time.sleep(2.0); rest(); time.sleep(0.5)
gl = geometry(R)
if t.dry:
    ImageGrab.grab(bbox=(R.left, R.top, R.right, R.bottom), all_screens=True).save(HERE / "dry-preroll-flat.png")
GL = snap(gl, {"b0": ("blue", 0), "r0": ("red", 0), "b150": ("blue", 150), "r300": ("red", 300),
               "b600": ("blue", 600), "r600": ("red", 600), "r1000": ("red", 1000)})
# zoom rectangle round the start of the flight: 12 yd left of the muzzle (no line there; x extrapolated) to 360 yd
GL["zl_a"] = (gl["X"](-12), GL["r0"][1] - 26); GL["zl_b"] = (gl["X"](360), GL["r300"][1] + 26)
print("   flat chart:", GL)
def middle_drag(a, b, secs=1.0):
    glide(*a, 0.4); mouse.press(button="middle", coords=a)
    n = max(1, int(secs / 0.03))
    for i in range(1, n + 1):
        e = 0.5 - 0.5 * math.cos(math.pi * i / n)
        mouse.move(coords=(int(a[0] + (b[0] - a[0]) * e), int(a[1] + (b[1] - a[1]) * e))); time.sleep(0.03)
    mouse.release(button="middle", coords=b)


def middle_click(xy):
    glide(*xy, 0.3); mouse.click(button="middle", coords=xy)


S["plot_empty"] = (GL["r600"][0], R.bottom - 260)       # inside the plot, below the flat curves, left of the legend
middle_drag(GL["zl_a"], GL["zl_b"], 0.3); time.sleep(1.0); rest(); time.sleep(0.5)
ZL = snap(geometry(R, -12, 360, edges=True), {"b0": ("blue", 0), "b150": ("blue", 150), "r300": ("red", 300)})
print("   flat zoomed:", ZL)
if t.dry:
    ImageGrab.grab(bbox=(R.left, R.top, R.right, R.bottom), all_screens=True).save(HERE / "dry-preroll-flatzoom.png")
middle_click(S["plot_empty"]); time.sleep(1.0); rest(); time.sleep(0.5)
back = snap(geometry(R), {"r1000": ("red", 1000)})
print("   after middle click (autoscale?):", back, "flat was", GL["r1000"])
# Edit Parameters: positions, then 30 deg
menu_path("view", ["edit"], g=0.15); t.dlg.wait("visible", timeout=10); time.sleep(0.8)
S["tab_params"] = center(t.tab("Parameters")); S["tab_zero"] = center(t.tab("Zero"))
S["ok"] = center(t.dlg.child_window(control_type="Button", title="OK"))
click_xy(*S["tab_params"], 0.15); time.sleep(0.5)
S["angle"] = center(W(t.field(*P, "AngleControl")))
fc("pre angle", S["angle"], t.field(*P, "AngleControl"), "30", 0.01, 0.15)
click_xy(*S["ok"], 0.15); time.sleep(2.5); calc_error()
middle_click(S["plot_empty"]); time.sleep(1.0); rest(); time.sleep(0.8)   # after a recalculation ScottPlot maps a zoom rectangle with stale limits until this
gu = geometry(R)
GU = snap(gu, {"r200": ("red", 200), "r1000": ("red", 1000), "b500": ("blue", 500), "b1000": ("blue", 1000),
               "za": ("red", 780), "zb": ("blue", 780)})
GU["zoom_a"] = (GU["za"][0], GU["r1000"][1] - 40); GU["zoom_b"] = (gu["X"](1008), GU["zb"][1] + 40)
print("   uphill chart:", GU)
wheel_zoom(GU["r1000"], secs=0.6); time.sleep(0.8); rest(); time.sleep(0.6)
GZ = end_cols(R, (30, 250))
print("   zoomed:", GZ)
if abs(GZ["r30"][1] - GZ["b30"][1]) < 15:
    ImageGrab.grab(bbox=(R.left, R.top, R.right, R.bottom), all_screens=True).save(HERE / "dry-preroll-zoom.png")
    print("STOPPED: the uphill zoom did not land on 780-1000 yd (see dry-preroll-zoom.png)"); os._exit(1)
if t.dry:
    ImageGrab.grab(bbox=(R.left, R.top, R.right, R.bottom), all_screens=True).save(HERE / "dry-preroll-zoom.png")
t.restart(); minimize_strays()


# ---------------------------------------------------------------- the take
ANGLE = t.field(*P, "AngleControl")
mid = lambda p, q: ((p[0] + q[0]) // 2, (p[1] + q[1]) // 2)

t.start()
t.voice("v00")
t.voice("i01")
t.voice("i02")


def a_a01(d):            # "Trajectory, (1.14) Open. (2.01) The same file from episode two: ..."
    s = time.monotonic()
    at(s, 0.3, lambda: click_xy(*S["traj"], 0.5))
    at(s, 1.0, lambda: (glide(S["traj"][0], S["open"][1], 0.2), click_xy(*S["open"], 0.2)))
    dlg = t.main.child_window(class_name="#32770", title="Open Trajectory"); dlg.wait("visible", timeout=10)
    # positions cached in the pre-roll: a UIA lookup of the list item once took 10 s here (2026-11-05 take)
    at(s, 3.0, lambda: click_xy(*S["file"], 0.5))
    at(s, 4.2, lambda: click_xy(*S["openbtn"], 0.5))
    time.sleep(1.5)
    at(s, d - 0.3, rest)
t.voice("a01", a_a01)


def a_a02(d):            # "On the flat, at one thousand yards, we hold ten milliradians."
    s = time.monotonic()
    at(s, 0.8, lambda: glide(*S["hold1000"], 0.7))
t.voice("a02", a_a02)


def a_a03(d):            # "Now the chart, (1.12) with the drop over muzzle level. (3.33) View, (4.1) Drop, (4.98) Over Muzzle Level, (6.32) and the Chart tab."
    s = time.monotonic()
    menu_path("view", ["drop", "ml"], times=(3.1, 3.9, 4.8), s=s)
    at(s, 6.2, lambda: click_xy(*S["tab_chart"], 0.4))
t.voice("a03", a_a03)


def a_a04(d):            # "Two lines: (1.14) the bullet, (1.97) and the line of sight. (3.69) The bullet starts below the line of sight,
                         #  (6.08) goes above it, (7.18) and comes back to it at three hundred yards, our zero.
                         #  (11.11) On the flat the line of sight is level, (13.48) so both kinds of drop are the same."
    s = time.monotonic()
    at(s, 0.5, lambda: glide(*GL["b600"], 0.5))
    at(s, 1.4, lambda: glide(*GL["r600"], 0.4))
    at(s, 2.0, lambda: middle_drag(GL["zl_a"], GL["zl_b"], 0.9))        # zoom into 0-360 yd: the classic picture
    at(s, 3.6, lambda: glide(*ZL["b0"], 0.6))
    at(s, 6.0, lambda: glide(*ZL["b150"], 0.8))
    at(s, 7.3, lambda: glide(*ZL["r300"], 1.2))
    at(s, 10.6, lambda: middle_click(S["plot_empty"]))                  # back to the whole flight
    at(s, 11.4, lambda: (glide(*GL["r0"], 0.4), glide(*GL["r1000"], 1.8)))
    at(s, d - 0.2, rest)
t.voice("a04", a_a04)


def a_u01(d):            # "Now the same target, (1.62) thirty degrees uphill. (3.45) Edit Parameters, (4.64) the Parameters tab,
                         #  (5.98) and the shot angle: (7.22) thirty."
    s = time.monotonic()
    menu_path("view", ["edit"], times=(3.0, 3.6), s=s)
    t.dlg.wait("visible", timeout=10)
    at(s, 4.6, lambda: click_xy(*S["tab_params"], 0.4))
    at(s, 6.0, lambda: glide(*S["angle"], 0.5))
    at(s, 7.0, lambda: fc("up angle", S["angle"], ANGLE, "30", 0.12, 0.2))
t.voice("u01", a_u01)


def a_u02(d):            # "Not the shot angle on the Zero tab. (2.67) That one is the angle you zeroed at, ..."
    s = time.monotonic()
    at(s, 0.5, lambda: glide(*S["tab_zero"], 0.6))
    at(s, d - 1.0, lambda: glide(*S["ok"], 0.7))
t.voice("u02", a_u02)


def a_u03(d):            # "OK. (0.89) Now both lines go up. (3.02) At one thousand yards the target is five hundred yards higher..."
    s = time.monotonic()
    click_xy(*S["ok"], 0.2); time.sleep(0.8); calc_error()
    middle_click(S["plot_empty"])                                         # see the pre-roll: stale limits otherwise
    at(s, 1.6, lambda: glide(*GU["r200"], 0.4))
    at(s, 1.9, lambda: glide(*GU["r1000"], 1.4))
t.voice("u03", a_u03)


def a_u04(d):            # "This is the drop over muzzle level: (2.64) where the bullet is in space. (4.93) At the end it is almost
                         #  fifteen hundred feet above the muzzle, and still a little below the line of sight."
    s = time.monotonic()
    at(s, 0.5, lambda: glide(*GU["b500"], 0.8))
    at(s, 5.0, lambda: glide(*GU["b1000"], 1.0))
    at(s, 8.8, lambda: glide(*GU["r1000"], 0.6))
t.voice("u04", a_u04)


def a_u05(d):            # "Let us zoom in at the end. (1.95) Now you see the gap. (3.72) How far the bullet is from the line of sight,
                         #  measured across it, (7.46) is the other kind of drop: (9.31) drop over the line of sight. This is the one you hold."
    s = time.monotonic()
    at(s, 0.2, lambda: wheel_zoom(GU["r1000"], secs=1.4))
    at(s, 2.0, lambda: glide(*mid(GZ["r30"], GZ["b30"]), 0.6))
    at(s, 3.8, lambda: glide(*GZ["r250"], 0.6))
    at(s, 4.8, lambda: glide(*GZ["b250"], 1.6))
    at(s, 9.3, lambda: glide(*mid(GZ["r30"], GZ["b30"]), 0.6))
t.voice("u05", a_u05)


def a_u06(d):            # "So back to drop over the line of sight, (2.68) and the table. (4.04) At one thousand yards the hold is eight point two..."
    s = time.monotonic()
    menu_path("view", ["drop", "los"], times=(0.1, 0.7, 1.4), s=s, g=0.3)
    at(s, 2.6, lambda: click_xy(*S["tab_table"], 0.4))
    at(s, 4.2, lambda: glide(*S["hold1000"], 0.7))
t.voice("u06", a_u06)
t.voice("u07")


def a_u08(d):            # "Why less? Gravity always pulls straight down. When the line of sight is tilted, ..." — open the dialog
    s = time.monotonic()                     # at the end, so d01 only types the angle
    at(s, 0.5, rest)
    at(s, d - 2.6, lambda: menu_path("view", ["edit"], g=0.3))
    t.dlg.wait("visible", timeout=10); time.sleep(0.2)
    click_xy(*S["tab_params"], 0.3)
t.voice("u08", a_u08)


def a_d01(d):            # "And thirty degrees downhill: (2.15) minus thirty. Eight point four."
    s = time.monotonic()
    fc("down angle", S["angle"], ANGLE, "-30", 0.08, 0.3)
    click_xy(*S["ok"], 0.3); time.sleep(0.9); calc_error()
    glide(*S["hold1000"], 0.5)
t.voice("d01", a_d01)
t.voice("d02")


def a_r01(d):            # card overlaid after assembly
    s = time.monotonic()
    at(s, 0.3, rest)
t.voice("r01", a_r01)


def a_r02(d):            # "On the flat at eight hundred sixty-six yards, (5.17) the hold is seven point three. (7.98) The full calculation says eight point two."
    s = time.monotonic()
    at(s, 7.6, lambda: glide(*S["hold1000"], 0.6))
t.voice("r02", a_r02)


def a_r03(d):
    s = time.monotonic()
    at(s, 1.0, rest)
t.voice("r03", a_r03)
t.voice("n01")
t.finish()
