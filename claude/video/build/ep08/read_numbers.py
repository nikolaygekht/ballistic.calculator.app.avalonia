"""Read episode 8's numbers from the app (no OBS, never saves).

Window 1 — the episode's shot: New Metric; Ammunition = library ".300 AAC 208gr Hornady" with MV 1010 ft/s;
Rifle = Standard Optics (High, Mil) with sight height 2.9 in, rifling 1:8 in right; Zero 100 m with
"Other ammunition for zero" = library ".300 AAC 115gr" with MV 2330 ft/s; Parameters 200 m / 25 m; standard air.
Prints the table, then the Trijicon VCOG16-300BLK reticle: Far BDC and Target (19 in) at 50 / 100 / 150 m.
Window 2 — sanity check of the supersonic ladder: the 115 gr load as the shot, 600 m / 50 m, Far BDC.
Usage: python read_numbers.py   -> stdout + reticle-*.png
"""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *
from PIL import ImageGrab

HERE = pathlib.Path(__file__).resolve().parent
t = Take(__file__)
W = lambda spec: spec.wrapper_object()


def scan(spec):
    ids = {}
    for e in W(spec).descendants():
        ids.setdefault(e.element_info.automation_id, e)
    return ids


def num(e):
    return [c for c in e.descendants() if c.element_info.automation_id == "NumericPart"][0]


def unit_text(e):
    u = [c for c in e.descendants() if c.element_info.automation_id == "UnitPart"][0]
    ch = u.children()                                   # the combo's first child is the shown unit
    return ch[0].window_text() if ch else "?"


def fill(el, text):
    click(el, 0.15); keyboard.send_keys("{END}+{HOME}", pause=0.03); time.sleep(0.05)
    keyboard.send_keys(text, pause=0.02, with_spaces=True); time.sleep(0.2)
    v = el.get_value()
    if v.replace(",", "") != text:
        print(f"STOPPED: {text!r} read back as {v!r}; app left open."); os._exit(1)


def set_unit(ctrl, unit):
    u = [c for c in ctrl.descendants() if c.element_info.automation_id == "UnitPart"][0]
    click(u, 0.15); time.sleep(0.4); click(W(t.item(unit)), 0.15); time.sleep(0.3)


def load_ammo(button, title, rel):
    click(button, 0.15)
    d = t.main.child_window(class_name="#32770", title=title); d.wait("visible", timeout=10); time.sleep(0.5)
    click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.15)
    type_text(rel, 0.01); keyboard.send_keys("{ENTER}"); time.sleep(1.0)


def ammo_dump(label, ids):
    vals = {k: num(ids[k]).get_value() for k in ("WeightControl", "BCControl", "MuzzleVelocityControl") if k in ids}
    print(f"   {label}: name={ids['NameTextBox'].get_value()!r} " if "NameTextBox" in ids else f"   {label}: ", vals)


def tab(name):
    click(t.tab(name), 0.15); time.sleep(0.5)


def table(label, keep):
    tv = t.tv()
    heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
    print(f"\n### {label}\n" + " | ".join(heads[:9]))
    rows = {}
    for r in tv.descendants(control_type="DataItem"):
        c = [x.window_text() for x in r.descendants(control_type="Text")]
        if c:
            rows[c[0]] = c
    for k in keep:
        if k in rows:
            print(" | ".join(rows[k][:9]))
    sys.stdout.flush()


def shot(ammo_rel, ammo_mv, zero_rel, zero_mv, max_range, step):
    t.new_trajectory("Metric", 0.15)
    a = scan(t.by_id("AmmoLibPanel"))
    load_ammo(a["LoadButton"], "Load Ammunition", ammo_rel)
    a = scan(t.by_id("AmmoLibPanel")); ammo_dump("shot ammo as loaded", a)
    fill(num(a["MuzzleVelocityControl"]), ammo_mv)
    print("     shot MV unit:", unit_text(a["MuzzleVelocityControl"]))
    tab("Rifle")
    click(t.by_id("RifleSubPanel", "SightPresetCombo"), 0.15); time.sleep(0.4)
    click(W(t.item("Standard Optics (High, Mil)")), 0.15); time.sleep(0.4)
    r = scan(t.by_id("RifleSubPanel"))
    fill(num(r["SightHeightControl"]), "2.9")
    print("     sight height unit:", unit_text(r["SightHeightControl"]),
          "| clicks:", num(r["VerticalClickControl"]).get_value(), unit_text(r["VerticalClickControl"]))
    click(r["RiflingDirectionCombo"], 0.15); time.sleep(0.4); click(W(t.item("Right")), 0.15); time.sleep(0.3)
    if unit_text(r["RiflingStepControl"]) != "in":
        set_unit(r["RiflingStepControl"], "in")
    fill(num(r["RiflingStepControl"]), "8")
    print("     twist unit:", unit_text(r["RiflingStepControl"]))
    tab("Zero")
    z = scan(t.by_id("ZeroSubPanel"))
    if unit_text(z["ZeroDistanceControl"]) != "m":      # the sight preset sets its own default zero, 100 yd
        set_unit(z["ZeroDistanceControl"], "m")
    fill(num(z["ZeroDistanceControl"]), "100")
    print("     zero unit:", unit_text(z["ZeroDistanceControl"]))
    if zero_rel:
        za = scan(t.by_id("ZeroSubPanel", "ZeroAmmoSubPanel"))
        click(za["EnableCheckBox"], 0.15, 0.1); time.sleep(0.4)
        load_ammo(za["LoadButton"], "Load Ammunition for Zero", zero_rel)
        za = scan(t.by_id("ZeroSubPanel", "ZeroAmmoSubPanel")); ammo_dump("zero ammo as loaded", za)
        fill(num(za["MuzzleVelocityControl"]), zero_mv)
        print("     zero MV unit:", unit_text(za["MuzzleVelocityControl"]))
    tab("Parameters")
    p = scan(t.by_id("ParametersSubPanel"))
    fill(num(p["MaxRangeControl"]), max_range)
    fill(num(p["StepControl"]), step)
    t.ok_and_accept_weather(0.15)
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.5):
        print("STOPPED: 'Cannot calculate this shot'; app left open."); os._exit(1)


def reticle(prefix, targets):
    tv = t.tv()
    click(t.tv_find("TabItem", "Reticle", tv=tv), 0.2); time.sleep(3.0)
    load = [b for b in tv.descendants(control_type="Button") if b.element_info.automation_id == "LoadReticleButton"][0]
    click(load, 0.2)
    d = t.main.child_window(class_name="#32770"); d.wait("visible", timeout=10); time.sleep(0.5)
    click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.2)
    type_text("VCOG16-300BLK.reticle", 0.01); keyboard.send_keys("{ENTER}"); time.sleep(3.0)
    print("   reticle:", [e.window_text() for e in tv.descendants() if e.element_info.automation_id == "ReticleNameText"])
    canvas = t.tv_find("Custom", auto_id="ReticleCanvas", tv=tv).rectangle()
    grab = lambda name: ImageGrab.grab(bbox=(canvas.left, canvas.top, canvas.right, canvas.bottom), all_screens=True).save(HERE / name)
    click(t.tv_find("RadioButton", "Far BDC", tv=tv), 0.2, 0.1); time.sleep(2.0); rest(); time.sleep(0.5)
    grab(f"reticle-{prefix}-far.png")
    click(t.tv_find("RadioButton", "Near BDC", tv=tv), 0.2, 0.1); time.sleep(2.0); rest(); time.sleep(0.5)
    grab(f"reticle-{prefix}-near.png")
    sp = [e for e in tv.descendants() if e.element_info.automation_id == "SplitAtControl"]
    if sp:
        print("   split at:", num(sp[0]).get_value(), unit_text(sp[0]))
    if targets:
        click(t.tv_find("RadioButton", "Target", tv=tv), 0.2, 0.1); time.sleep(1.0)
        boxes = [e for e in tv.descendants() if e.element_info.automation_id == "PART_TextBox"]
        tid = [e for e in tv.descendants() if e.element_info.automation_id == "TargetDistanceControl"][0]
        for b in boxes[:2]:
            click(b, 0.15); keyboard.send_keys("{END}+{HOME}19{TAB}", pause=0.03); time.sleep(0.3)
        print("   target units:", unit_text(tid))
        for dist in targets:
            click(num(tid), 0.15); keyboard.send_keys("{END}+{HOME}" + dist + "{TAB}", pause=0.03)
            time.sleep(1.5); rest(); time.sleep(0.5)
            print("   target", dist, [e.window_text() for e in tv.descendants() if e.element_info.automation_id == "TargetAngularSizeText"])
            grab(f"reticle-{prefix}-target{dist}.png")


KEEP = ("0", "25", "50", "75", "100", "125", "150", "175", "200", "300", "400", "500", "600")
shot(r".300 AAC\.300 AAC 208gr Hornady.ammo", "1010", r".300 AAC\.300 AAC 115gr.ammo", "2330", "200", "25")
table("subsonic 208 gr, zeroed 100 m with the supersonic 115 gr", KEEP)
reticle("sub", ("50", "100", "150"))
shot(r".300 AAC\.300 AAC 115gr.ammo", "2330", None, None, "600", "50")
table("supersonic 115 gr alone, zeroed 100 m (ladder check)", KEEP)
reticle("super", ())
print("\ndone; app left open")
