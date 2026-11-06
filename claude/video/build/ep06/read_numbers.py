"""Read episode 6's numbers from the app (no OBS): baseline (G7 0.253), the Lapua radar .drg via Browse,
then the box G1 0.505 for comparison. Never saves.   Usage: python read_numbers.py"""
import pathlib, sys, time
sys.stdout.reconfigure(encoding="utf-8")
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

DRG = r"E:\ngeht\Documents\Ballistic Calculator demo\30-lapua gb550 11,3g (175 gr) scenar-l_radar.drg"
assert pathlib.Path(DRG).exists()
t = Take(__file__)
W = lambda spec: spec.wrapper_object()
KEEP = ("0", "100", "300", "500", "600", "700", "800", "900", "1,000")
A = ("AmmoLibPanel", "AmmoSubPanel")

mw = t.main.wrapper_object()
bar = [m for m in mw.descendants(control_type="Menu") if not m.element_info.automation_id][0]
view = [c for c in bar.children() if c.window_text() == "View"][0]; vr = view.rectangle()


def table(label):
    tv = t.tv()
    heads = [h.window_text() for h in tv.descendants(control_type="HeaderItem")]
    print(f"\n### {label}\n" + " | ".join(heads[:6]))
    rows = {}
    for r in tv.descendants(control_type="DataItem"):
        c = [x.window_text() for x in r.descendants(control_type="Text")]
        if c and c[0] in KEEP:
            rows[c[0]] = c
    for k in KEEP:
        if k in rows:
            print(" | ".join(rows[k][:6]))
    sys.stdout.flush()


def edit(tab):
    click(view, 0.15); time.sleep(0.5); click_xy(vr.left + 120, vr.bottom + 28, 0.15)
    t.dlg.wait("visible", timeout=10); time.sleep(0.8)
    click(t.tab(tab), 0.15); time.sleep(0.5)


def ok():
    click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.5)
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.5):
        print("STOPPED: 'Cannot calculate this shot'; app left open."); os._exit(1)


def ammo_fields(label):
    p = W(t.by_id(*A))
    out = []
    for e in p.descendants():
        i = e.element_info
        if i.automation_id in ("NumericPart", "NameTextBox", "SourceTextBox", "CustomTableBox", "CaliberTextBox"):
            par = e.parent().element_info.automation_id
            try: v = e.get_value()
            except Exception: v = e.window_text()
            unit = [x.window_text() for x in e.parent().descendants(control_type="Text")][:1] if i.automation_id == "NumericPart" else []
            out.append(f"{par or i.automation_id}={v!r}{unit[0] if unit else ''}")
        if i.control_type == "CheckBox":
            out.append(f"{e.window_text()}={e.get_toggle_state()}")
    print(f"{label}: " + "; ".join(out), flush=True)


t.open_file("bc2-demo", 0.15)
table("baseline G7 0.253")

edit("Ammunition")
ammo_fields("before Browse")
click(t.by_id(*A, "BrowseTableButton"), 0.15)
d = t.main.child_window(class_name="#32770"); d.wait("visible", timeout=10); time.sleep(1.0)
print("file dialog:", d.window_text())
# the dialog opens in the app's data/drg: go to the demo folder first (its path has no "(" ")" for send_keys)
click(d.child_window(auto_id="1148", control_type="ComboBox"), 0.15)
type_text(str(pathlib.Path(DRG).parent), 0.01); keyboard.send_keys("{ENTER}"); time.sleep(1.2)
# send_keys eats "(" and ")" (they group modifiers), so typing the file name fails: click the file in the list
click(d.child_window(title_re=r"30-lapua gb550.*", control_type="ListItem", found_index=0), 0.15, 0.15); time.sleep(0.4)
click(d.child_window(auto_id="1", control_type="Button"), 0.15); time.sleep(1.5)
ammo_fields("after Browse")
ok()
table("radar .drg (Lapua GB550 Scenar-L)")

t.open_file("bc2-demo", 0.15)          # a second window, the untouched baseline
edit("Ammunition")
t.fill(t.field(*A, "BCControl"), "0.505", 0.01, 0.15)
click(t.by_id(*A, "BCControl", "TablePart"), 0.15); time.sleep(0.4); click(t.item("G1"), 0.15)
ammo_fields("G1 0.505")
ok()
table("box G1 0.505")
