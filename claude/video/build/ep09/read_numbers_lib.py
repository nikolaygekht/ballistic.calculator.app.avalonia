"""Shared helpers of read_numbers.py (importing starts a fresh app)."""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *
from PIL import ImageGrab

HERE = pathlib.Path(__file__).resolve().parent
DEMO = r"E:\ngeht\Documents\Ballistic Calculator demo\bc2-demo.trajectory"
t = Take(__file__)
W = lambda s: s.wrapper_object()
P = ("ParametersSubPanel",)


def table(label, keep=("0", "100", "300", "500", "600", "800", "866", "1,000")):
    tv = t.tv()
    heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
    print(f"\n### {label}\n" + " | ".join(heads[:6]))
    for r in tv.descendants(control_type="DataItem"):
        c = [x.window_text() for x in r.descendants(control_type="Text")]
        if c and c[0] in keep:
            print(" | ".join(c[:6]))
    sys.stdout.flush()


def visible_item(key):
    """A menu item by automation id (x:Name) or, for unnamed parents, by title — only one that is on screen
    now: closed menus keep their items in the UIA tree, so a plain lookup can land on a stale one."""
    for _ in range(20):
        for e in t.main.wrapper_object().descendants(control_type="MenuItem"):
            if key in (e.element_info.automation_id, e.window_text()):
                r = e.rectangle()
                if r.width() > 0 and r.height() > 0 and not e.element_info.element.CurrentIsOffscreen:
                    return e
        time.sleep(0.2)
    raise LookupError(key)


def menu(*path):
    """Click through a menu path; into a submenu go sideways along the parent's row first, then down."""
    bar = [m for m in t.main.wrapper_object().descendants(control_type="Menu") if not m.element_info.automation_id][0]
    top = [c for c in bar.children() if c.window_text() == path[0]][0]
    click(top, 0.15); time.sleep(0.5)
    prev = center(top)
    for i, key in enumerate(path[1:]):
        x, y = center(visible_item(key))
        if i == 0:      # straight down out of the bar: a diagonal crosses the next header and opens ITS menu
            glide(prev[0], y, 0.15)
        else:           # into a submenu: along the parent's row first
            glide(x, prev[1], 0.15)
        time.sleep(0.15)
        click_xy(x, y, 0.15); time.sleep(0.5)
        prev = (x, y)


def open_demo():
    menu("Trajectory", "MenuFileOpen")
    d = t.main.child_window(class_name="#32770", title="Open Trajectory"); d.wait("visible", timeout=10); time.sleep(1.0)
    click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.15); type_text(DEMO, 0.005)
    keyboard.send_keys("{ENTER}"); time.sleep(2.5)


def put(spec, text):
    e = W(spec); click(e, 0.15); keyboard.send_keys("{END}+{HOME}"); type_text(text, 0.01); time.sleep(0.25)
    v = e.get_value()
    if v.replace(",", "") != text:
        print(f"STOPPED: {text!r} read back as {v!r}"); os._exit(1)


def params(angle, max_range=None, step=None):
    menu("View", "MenuViewEditParameters")
    t.dlg.wait("visible", timeout=10); time.sleep(0.8)
    click(t.tab("Parameters"), 0.15); time.sleep(0.5)
    put(t.field(*P, "AngleControl"), angle)
    if max_range:
        put(t.field(*P, "MaxRangeControl"), max_range)
    if step:
        put(t.field(*P, "StepControl"), step)
    click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.5)
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.5):
        print("STOPPED: 'Cannot calculate this shot'; app left open."); os._exit(1)


def chart_shot(name):
    tv = t.tv()
    click(t.tv_find("TabItem", "Chart", tv=tv), 0.2); time.sleep(2.0); rest(); time.sleep(0.5)
    r = tv.rectangle()
    ImageGrab.grab(bbox=(r.left, r.top, r.right, r.bottom), all_screens=True).save(HERE / name)
    click(t.tv_find("TabItem", "Table", tv=tv), 0.2); time.sleep(1.0)


