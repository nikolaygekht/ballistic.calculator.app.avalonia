"""Read episode 3's numbers from the app itself: baseline, + February weather, + 10 mph wind from the right.

Usage: python numbers.py      (drives the app on the recording monitor, no OBS; prints the tables)
"""
import pathlib, sys, time
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

t = Take(__file__)
FAST = dict(pause=0.01, secs=0.15)


def table(label):
    tv = t.tv()
    heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
    print(f"\n### {label}\n" + " | ".join(heads))
    for r in tv.descendants(control_type="DataItem"):
        print(" | ".join(c.window_text() for c in r.descendants(control_type="Text")))
    sys.stdout.flush()


def edit():
    keyboard.send_keys("{ESC}"); time.sleep(0.2)
    click(t.tv(), 0.15, 0.5, 0.9); time.sleep(0.3)          # focus the trajectory window
    # "View" is not unique in the UIA tree (the trajectory window has View-named items too): a menu
    # lookup once opened Tools → Edit Sights. Ctrl+E is View → Edit Parameters.
    keyboard.send_keys("^e")
    t.dlg.wait("visible", timeout=10); time.sleep(0.8)


def unit(*ids):
    return t.by_id(*ids, "UnitPart").window_text()


t.open_file("bc2-demo", 0.15)
table("baseline")

# ---- February weather
edit()
click(t.tab("Weather"), 0.15); time.sleep(0.5)
W = "AtmosphereSubPanel"
t.fill(t.field(W, "AltitudeControl"), "1000", **FAST)
t.fill(t.field(W, "PressureControl"), "28.9", **FAST)
t.fill(t.field(W, "TemperatureControl"), "40", **FAST)
t.fill(t.by_id(W, "HumidityTextBox"), "60", **FAST)
for f in ("AltitudeControl", "PressureControl", "TemperatureControl"):
    print(f, t.field(W, f).window_text(), "|", t.by_id(W, f).wrapper_object().descendants(control_type="ComboBox")[0].window_text()
          if t.by_id(W, f).wrapper_object().descendants(control_type="ComboBox") else "")
click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.0)
table("+ weather 1000 ft, 28.9 inHg, 40 F, 60 %")

# ---- wind
edit()
click(t.tab("Wind"), 0.15); time.sleep(0.5)
wp = t.by_id("WindSubPanel").wrapper_object()
if not [e for e in wp.descendants() if e.element_info.automation_id == "VelocityControl"]:
    click(t.by_id("WindSubPanel", "AddButton"), 0.15); time.sleep(0.5)
print("wind tab:", [(e.element_info.automation_id, e.element_info.control_type, e.window_text())
                    for e in wp.descendants() if e.element_info.automation_id][:40])
t.fill(t.field("WindSubPanel", "VelocityControl"), "10", **FAST)
t.fill(t.field("WindSubPanel", "DirectionControl"), "90", **FAST)
click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.0)
table("+ wind 10 mph from 90 deg")
