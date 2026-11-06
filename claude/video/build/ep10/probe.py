"""Episode 10 probe (no OBS, fast, drives the mouse): runs the whole story once to read the numbers and dump
the controls of the two drag-table dialogs and the atmosphere dialog.
Writes the drg files the take will write again: scenar-l-5bc.drg, 338 flatline.drg (demo folder).
Usage: python probe.py   -> stdout + probe-ids.txt"""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8", line_buffering=True)
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

HERE = pathlib.Path(__file__).resolve().parent
DEMO = pathlib.Path(r"E:\ngeht\Documents\Ballistic Calculator demo")
import json
_st = EXE.parent / "appstate.json"
_j = json.loads(_st.read_text()); _j["LastMeasurementSystem"] = 1; _st.write_text(json.dumps(_j, indent=2))   # Imperial dialogs
t = Take(__file__)
W = lambda s: s.wrapper_object()
dump = open(HERE / "probe-ids.txt", "w", encoding="utf-8")
KNOTS = [("0.250", "2600"), ("0.240", "2200"), ("0.244", "1800"), ("0.263", "1400"), ("0.293", "1100")]


def visible_item(key):
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


def menu(*path):
    top = bar_item(path[0]); click(top, 0.15); time.sleep(0.5)
    prev = center(top)
    for i, key in enumerate(path[1:]):
        x, y = center(visible_item(key))
        glide(prev[0], y, 0.15) if i == 0 else glide(x, prev[1], 0.15)
        time.sleep(0.15); click_xy(x, y, 0.15) if i == len(path) - 2 else glide(x, y, 0.15)
        time.sleep(0.5); prev = (x, y)


def ids_of(win, label):
    out = {}
    dump.write(f"\n===== {label}\n")
    for e in W(win).descendants():
        a = e.element_info.automation_id; r = e.rectangle()
        dump.write(f"{e.element_info.control_type:12s} id={a!r:28s} text={e.window_text()[:40]!r:44s} {r}\n")
        if a:
            out.setdefault(a, e)
    dump.flush()
    return out


def numeric(e):
    return [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]


def put(e, text):
    click(e, 0.15); type_text(text, 0.01); time.sleep(0.2)


def file_dialog(title, path):
    d = t.main.child_window(class_name="#32770", title=title); d.wait("visible", timeout=10); time.sleep(0.8)
    edit = d.child_window(auto_id="1001", control_type="Edit") if title.startswith("Save") else \
        d.child_window(auto_id="1148", control_type="ComboBox")
    click(edit, 0.15); type_text(str(path), 0.005); time.sleep(0.2)
    keyboard.send_keys("{ENTER}"); time.sleep(1.0)
    c = t.main.child_window(title_re="Confirm Save As.*")
    if c.exists(timeout=1.0):
        c.child_window(title="Yes", control_type="Button").click_input(); time.sleep(0.5)


def open_traj(name):
    menu("Trajectory", "MenuFileOpen")
    file_dialog("Open Trajectory", DEMO / name); time.sleep(1.5)


def hold(*ranges):
    tv = t.tv()
    heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
    print("   heads:", heads)
    for row in tv.descendants(control_type="DataItem"):
        c = [x.window_text() for x in row.descendants(control_type="Text")]
        if c and c[0] in ranges:
            print("   ", " | ".join(c[:8]))


def browse_drg(name):
    menu("View", "MenuViewEditParameters"); t.dlg.wait("visible", timeout=10); time.sleep(0.8)
    a = ids_of(t.by_id("AmmoLibPanel"), f"ammo panel before browse {name}") if name == "scenar-l-5bc.drg" else \
        {e.element_info.automation_id: e for e in W(t.by_id("AmmoLibPanel")).descendants() if e.element_info.automation_id}
    click(a["BrowseTableButton"], 0.15)
    file_dialog("Open Custom Drag Table", DEMO / name); time.sleep(0.8)
    a = {e.element_info.automation_id: e for e in W(t.by_id("AmmoLibPanel")).descendants() if e.element_info.automation_id}
    print("   after browse: name", a["NameTextBox"].get_value() if "NameTextBox" in a else "?",
          "| bc", numeric(a["BCControl"]).get_value(), "| w", numeric(a["WeightControl"]).get_value())
    click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.0)
    c = t.main.child_window(class_name="Window", title="Confirm")
    if c.exists(timeout=2):
        click(c.child_window(control_type="Button", title="Yes"), 0.15); time.sleep(1.5)


# ---------------------------------------------------------------- part 1
open_traj("bc2-demo.trajectory"); print("bc2-demo, one BC:"); hold("800", "1,000")

menu("Tools", "Approximate Drag Table", "MenuToolsDrgFromBc")
bcd = t.main.child_window(class_name="ApproximateDrgFromBcDialog"); bcd.wait("visible", timeout=10); time.sleep(1.0)
B = ids_of(bcd, "From BC Curve")
put(B["NameBox"], "Scenar-L 175 gr, five G7 BCs")
put(numeric(B["WeightControl"]), "175"); put(numeric(B["DiameterControl"]), "0.308")
print("   units: weight", [c.window_text() for c in B["WeightControl"].descendants() if c.element_info.automation_id == "UnitPart"],
      "base", B["BaseTableCombo"].window_text())
tp = [c for c in B["BcControl"].descendants() if c.element_info.automation_id == "TablePart"][0]
click(tp, 0.15); time.sleep(0.5); click(t.item("G7"), 0.15); time.sleep(0.3)
mach = [e for e in W(bcd).descendants() if e.element_info.automation_id == "PART_TextBox"][0]
for bc, v in KNOTS:
    put(numeric(B["BcControl"]), bc); put(numeric(B["VelocityControl"]), v); time.sleep(0.3)
    print(f"   knot {bc} @ {v}: mach field {mach.get_value()!r}, table {tp.window_text()!r}")
    click(B["AddButton"], 0.15); time.sleep(0.3)
print("   status:", B["StatusText"].window_text())
for r in B["KnotsGrid"].descendants(control_type="DataItem"):
    print("   grid:", [x.window_text() for x in r.descendants(control_type="Text")])
click(B["SaveButton"], 0.15)
file_dialog("Save Custom Drag Table", DEMO / "scenar-l-5bc.drg"); time.sleep(0.5)
print("   status after save:", B["StatusText"].window_text())
click(B["CloseButton"], 0.15); time.sleep(0.8)

browse_drg("scenar-l-5bc.drg"); print("bc2-demo on five BCs:"); hold("800", "1,000")
open_traj("bc2-demo-radar.trajectory"); print("radar:"); hold("800", "1,000")

# ---------------------------------------------------------------- part 2
menu("Tools", "Approximate Drag Table", "MenuToolsDrgFromVelocities")
vd = t.main.child_window(class_name="ApproximateDrgFromVelocitiesDialog"); vd.wait("visible", timeout=10); time.sleep(1.0)
V = ids_of(vd, "From Measured Velocities")
put(V["NameBox"], "338 Flatline 285 gr"); put(V["SourceBox"], "Warner Tool radar sheet")
put(numeric(V["WeightControl"]), "285"); put(numeric(V["DiameterControl"]), "0.338")
click(V["ImportButton"], 0.15)
file_dialog("Load Measured Velocities", DEMO / "338-flatline-velocities.csv"); time.sleep(0.8)
print("   status after csv:", V["StatusText"].window_text())
click(V["AtmosphereButton"], 0.15)
ad = t.main.child_window(class_name="AtmosphereDialog"); ad.wait("visible", timeout=10); time.sleep(0.8)
A = ids_of(ad, "Atmosphere")
put(numeric(A["AltitudeControl"]), "6000"); put(numeric(A["PressureControl"]), "23.98")
put(numeric(A["TemperatureControl"]), "78"); put(A["HumidityTextBox"], "25")
for k in ("AltitudeControl", "PressureControl", "TemperatureControl"):
    print(f"   {k}:", numeric(A[k]).get_value(), [c.window_text() for c in A[k].descendants() if c.element_info.automation_id == "UnitPart"])
click(ad.child_window(control_type="Button", title="OK"), 0.15); time.sleep(0.8)
print("   status after air:", V["StatusText"].window_text())
click(V["SaveButton"], 0.15)
file_dialog("Save Custom Drag Table", DEMO / "338 flatline.drg"); time.sleep(0.5)
print("   status after save:", V["StatusText"].window_text())
click(V["CloseButton"], 0.15); time.sleep(0.8)

open_traj("338-flatline.trajectory"); print("338, G7 0.484:"); hold("1,000", "1,500")
browse_drg("338 flatline.drg"); print("338 on our curve:"); hold("1,000", "1,500")
print("\ndone; app left open")
