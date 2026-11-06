"""Read episode 4's numbers from the app (no OBS): baseline, + bullet shape & twist (spin drift alone),
+ wind (aerodynamic jump), then baseline + Coriolis east / west.   Usage: python read_numbers.py"""
import pathlib, sys, time
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

t = Take(__file__)
FAST = dict(pause=0.01, secs=0.15)
KEEP = ("0", "300", "600", "800", "1,000")


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
    keyboard.send_keys("{ESC}"); time.sleep(0.2)
    click(t.tv(), 0.15, 0.5, 0.9); time.sleep(0.3)
    keyboard.send_keys("^e")
    t.dlg.wait("visible", timeout=10); time.sleep(0.8)
    click(t.tab(tab), 0.15); time.sleep(0.5)


def ok():
    click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.5)


def close_and_reopen():
    b = [x for x in t.main.wrapper_object().descendants(control_type="Button")
         if x.element_info.automation_id == "PART_CloseButton"][0]
    click(b, 0.15); time.sleep(1.0)
    t.open_file("bc2-demo", 0.15)


t.open_file("bc2-demo", 0.15)
table("baseline")

edit("Ammunition")
A = ("AmmoLibPanel", "AmmoSubPanel")
t.fill(t.field(*A, "BulletDiameterControl"), "0.308", **FAST)
t.fill(t.field(*A, "BulletLengthControl"), "1.394", **FAST)
click(t.tab("Rifle"), 0.15); time.sleep(0.5)
click(t.by_id("RifleSubPanel", "RiflingDirectionCombo"), 0.15); time.sleep(0.5)
click(t.item("Right"), 0.15); time.sleep(0.3)
t.fill(t.field("RifleSubPanel", "RiflingStepControl"), "10", **FAST)
print("twist unit:", t.by_id("RifleSubPanel", "RiflingStepControl", "UnitPart").window_text())
ok()
table("+ diameter 0.308, length 1.394, twist 1:10 right (no wind)")

edit("Wind")
t.fill(t.field("WindSubPanel", "VelocityControl"), "10", **FAST)
t.fill(t.field("WindSubPanel", "DirectionControl"), "90", **FAST)
ok()
table("+ wind 10 mph from 90")

for az in ("90", "270"):
    close_and_reopen()
    edit("Parameters")
    P = "ParametersSubPanel"
    click(t.by_id(P, "CoriolisCheckBox"), 0.15); time.sleep(0.3)
    t.fill(t.field(P, "AzimuthControl"), az, **FAST)
    t.fill(t.field(P, "LatitudeControl"), "45", **FAST)
    ok()
    table(f"baseline + Coriolis azimuth {az}, latitude 45 N")
