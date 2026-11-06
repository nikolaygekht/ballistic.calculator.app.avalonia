"""Read episode 5's numbers from the app (no OBS): baseline, + V-Clicks for the 800 yd hold; then on a reopened
bc2-demo: July air (zero inherits it), + Zero tab's own February air; the same pair with a 600 yd zero.
Usage: python read_numbers.py"""
import pathlib, sys, time
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

t = Take(__file__)
FAST = dict(pause=0.01, secs=0.15)
KEEP = ("0", "100", "200", "300", "400", "500", "600", "700", "800", "900", "1,000")
JULY = ("1000", "28.9", "100", "90")
FEB = {h: ("1000", "28.9", "40", h) for h in ("60", "40")}


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
    return rows


def edit(tab):
    """View -> Edit Parameters by mouse (Ctrl+E needs keyboard focus, which the user's own work can take)."""
    bar = [m for m in t.main.wrapper_object().descendants(control_type="Menu") if not m.element_info.automation_id][0]
    view = [c for c in bar.children() if c.window_text() == "View"][0]
    vr = view.rectangle()
    click(view, 0.15); time.sleep(0.5)
    click_xy(vr.left + 120, vr.bottom + 28, 0.15)
    t.dlg.wait("visible", timeout=10); time.sleep(0.8)
    click(t.tab(tab), 0.15); time.sleep(0.5)


def ok():
    click(t.dlg.child_window(control_type="Button", title="OK"), 0.15); time.sleep(2.5)
    if t.main.child_window(title="Cannot calculate this shot").exists(timeout=0.5):
        print("STOPPED: 'Cannot calculate this shot'; app left open."); os._exit(1)
    if t.dlg.exists(timeout=0.5):
        print("!! dialog still open:", [e.window_text() for e in t.main.wrapper_object().descendants(control_type="Text")
                                       if "click" in e.window_text().lower() or "not" in e.window_text().lower()][:5])


def close_and_reopen():
    b = [x for x in t.main.wrapper_object().descendants(control_type="Button")
         if x.element_info.automation_id == "PART_CloseButton"][0]
    click(b, 0.15); time.sleep(1.0)
    t.open_file("bc2-demo", 0.15)


def put(spec, text):
    """Type into a field and read it back. On a mismatch stop with everything left open for inspection."""
    e = W(spec)
    before = e.get_value()
    click(e, 0.15); type_text(text, 0.01); time.sleep(0.3)
    after = e.get_value()
    ok_ = after.replace(",", "") .rstrip("0").rstrip(".") == text.rstrip("0").rstrip(".") if "." in after or "." in text           else after.replace(",", "") == text
    print(f"   {e.parent().element_info.automation_id or e.element_info.automation_id:22} was {before!r:10} typed {text!r:7} now {after!r}"
          + ("" if ok_ else "   <-- MISMATCH"))
    if not ok_:
        print("STOPPED: field value is wrong; app left open."); os._exit(1)


def weather(prefix, vals):
    alt, prs, tmp, hum = vals
    put(t.field(*prefix, "AltitudeControl"), alt)
    put(t.field(*prefix, "PressureControl"), prs)
    put(t.field(*prefix, "TemperatureControl"), tmp)
    put(t.by_id(*prefix, "HumidityTextBox"), hum)


def scroll_zero_down():
    r = W(t.by_id("ZeroSubPanel")).rectangle()
    mouse.scroll(coords=((r.left + r.right) // 2, (r.top + r.bottom) // 2), wheel_dist=-15); time.sleep(0.5)


W = lambda spec: spec.wrapper_object()

# ---- part 1: dialled clicks
t.open_file("bc2-demo", 0.15)
base = table("baseline")
hold800 = float(base["800"][4].replace("−", "-"))
clicks = round(-hold800 / 0.1)
print("hold 800:", hold800, "-> V-Clicks", clicks)
edit("Parameters")
vbox = [e for e in W(t.by_id("ParametersSubPanel")).descendants()
        if e.element_info.automation_id == "PART_TextBox"][0]     # NumericUpDown has no id of its own; V first
click(vbox, 0.15); keyboard.send_keys("^a"); type_text(str(clicks), 0.01); time.sleep(0.3)
ok()
table(f"+ V-Clicks {clicks}")

# ---- part 2: July air, then the zero's own February air (300 yd zero, then 600 yd zero)
for zero, fh in (("300", "60"), ("300", "40"), ("600", "60"), ("600", "40")):
    close_and_reopen()
    edit("Weather")
    weather(("AtmosphereSubPanel",), JULY)
    if zero != "300":
        click(t.tab("Zero"), 0.15); time.sleep(0.5)
        put(t.field("ZeroSubPanel", "ZeroDistanceControl"), zero)
    ok()
    table(f"zero {zero}: July air (1000 ft, 28.9 inHg, 100 F, 90 %), zero inherits it")
    edit("Zero")
    scroll_zero_down()
    Z = ("ZeroSubPanel", "ZeroAtmosphereSubPanel")
    click(t.by_id(*Z, "EnableCheckBox"), 0.15, 0.1); time.sleep(0.4)
    scroll_zero_down()
    weather(Z + ("AtmoSubPanel",), FEB[fh])
    ok()
    table(f"zero {zero}: + Zero tab other atmosphere = February (1000 ft, 28.9 inHg, 40 F, {fh} %)")
