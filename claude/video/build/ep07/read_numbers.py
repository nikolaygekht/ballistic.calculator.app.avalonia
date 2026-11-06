"""Read episode 7's numbers from the app (no OBS, never saves). Starts from bc2-demo-radar (ep. 6) and adds one
effect at a time, printing the table after each step; then the same everything-on shot without wind (to isolate
aerodynamic jump), and Mil-Dot / Far BDC reticle screenshots for the radar baseline and for everything-on.
Usage: python read_numbers.py   -> stdout + reticle-*.png"""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *
from PIL import ImageGrab

HERE = pathlib.Path(__file__).resolve().parent
t = Take(__file__)
W = lambda s: s.wrapper_object()
FAST = dict(pause=0.01, secs=0.15)
KEEP = ("0", "100", "300", "500", "600", "800", "1,000")
mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
view = [c for c in bar.children() if c.window_text() == "View"][0]; vr = view.rectangle()


def table(label):
    tv = t.tv()
    heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
    print(f"\n### {label}\n" + " | ".join(heads[:9]))
    rows = {}
    for r in tv.descendants(control_type="DataItem"):
        c = [x.window_text() for x in r.descendants(control_type="Text")]
        if c and c[0] in KEEP:
            rows[c[0]] = c
    for k in KEEP:
        if k in rows:
            print(" | ".join(rows[k][:9]))
    sys.stdout.flush()


def edit(tab):
    click(view, 0.15); time.sleep(0.5); click_xy(vr.left + 120, vr.bottom + 28, 0.15)
    t.dlg.wait("visible", timeout=10); time.sleep(0.8)
    click(t.tab(tab), 0.15); time.sleep(0.5)


def put(spec, text):
    e = W(spec); click(e, 0.15); type_text(text, 0.01); time.sleep(0.25)
    v = e.get_value()
    if v.replace(",", "") != text:
        print(f"STOPPED: {text!r} read back as {v!r}"); os._exit(1)


def ok():
    click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.5)
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.5):
        print("STOPPED: 'Cannot calculate this shot'; app left open."); os._exit(1)


def weather(prefix, vals):
    for f, v in zip(("AltitudeControl", "PressureControl", "TemperatureControl"), vals[:3]):
        put(t.field(*prefix, f), v)
    put(t.by_id(*prefix, "HumidityTextBox"), vals[3])


def reticle_shot(name):
    tv = t.tv()
    click(t.tv_find("TabItem", "Reticle", tv=tv), 0.2); time.sleep(3.0)
    click(t.tv_find("Button", "Mil-Dot", tv=tv), 0.2); time.sleep(1.5)
    click(t.tv_find("RadioButton", "Far BDC", tv=tv), 0.2, 0.1); time.sleep(1.5)
    rest(); time.sleep(0.5)
    r = t.tv_find("Custom", auto_id="ReticleCanvas", tv=tv).rectangle()
    ImageGrab.grab(bbox=(r.left, r.top, r.right, r.bottom), all_screens=True).save(HERE / name)
    click(t.tv_find("TabItem", "Table", tv=tv), 0.2); time.sleep(1.0)


def everything(wind=True):
    edit("Rifle")
    click(t.by_id("RifleSubPanel", "RiflingDirectionCombo"), 0.15); time.sleep(0.5); click(t.item("Right"), 0.15); time.sleep(0.3)
    put(t.field("RifleSubPanel", "RiflingStepControl"), "10")
    ok(); table("+ twist 1:10 right (shape from the .drg)")
    edit("Parameters")
    click(t.by_id("ParametersSubPanel", "CoriolisCheckBox"), 0.15); time.sleep(0.3)
    put(t.field("ParametersSubPanel", "AzimuthControl"), "90")
    put(t.field("ParametersSubPanel", "LatitudeControl"), "45")
    ok(); table("+ Coriolis, east, 45 N")
    edit("Weather")
    weather(("AtmosphereSubPanel",), ("1000", "28.9", "100", "90"))
    click(t.tab("Zero"), 0.15); time.sleep(0.5)
    r = W(t.by_id("ZeroSubPanel")).rectangle(); zc = ((r.left + r.right) // 2, (r.top + r.bottom) // 2)
    mouse.scroll(coords=zc, wheel_dist=-15); time.sleep(0.5)
    Z = ("ZeroSubPanel", "ZeroAtmosphereSubPanel")
    click(t.by_id(*Z, "EnableCheckBox"), 0.15, 0.1); time.sleep(0.4)
    weather(Z + ("AtmoSubPanel",), ("1000", "28.9", "40", "40"))
    ok(); table("+ July air, February zero air")
    if wind:
        edit("Wind")
        put(t.field("WindSubPanel", "VelocityControl"), "10")
        put(t.field("WindSubPanel", "DirectionControl"), "90")
        ok(); table("+ wind 10 mph from 90 = EVERYTHING")


def reopen():
    b = [x for x in t.main.wrapper_object().descendants(control_type="Button") if x.element_info.automation_id == "PART_CloseButton"][0]
    click(b, 0.15); time.sleep(1.0)
    t.open_file("bc2-demo-radar", 0.15)


t.open_file("bc2-demo-radar", 0.15)
table("radar baseline (ep. 6 file)")
reticle_shot("reticle-radar.png")
everything()
reticle_shot("reticle-everything.png")
reopen(); everything(wind=False)                          # the same, no wind: what does the wind add to the drop?
reopen(); t.open_file("bc2-demo", 0.15); table("G7 baseline (bc2-demo)"); reticle_shot("reticle-g7.png")
