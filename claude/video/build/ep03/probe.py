"""Probe for episode 3's take (no OBS): the main menu bar's View item, Edit Parameters, table headers,
the trajectory window's close button. Usage: python probe.py"""
import pathlib, sys, time
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent.parent))
from bcrec import *

t = Take(__file__)
t.open_file("bc2-demo", 0.15)
mw = t.main.wrapper_object()

bars = mw.descendants(control_type="Menu")
print("menus:", [(b.element_info.class_name, b.element_info.automation_id, b.rectangle()) for b in bars])
for b in bars:
    print(" top items:", [(c.window_text(), c.element_info.automation_id, c.rectangle()) for c in b.children()])

tv = t.tv()
print("tv rect:", tv.rectangle())
print("headers:", [h.window_text() for h in tv.descendants(control_type="HeaderItem")])
print("tv buttons:", [(b.window_text(), b.element_info.automation_id, b.rectangle()) for b in tv.descendants(control_type="Button")][:20])
# ancestors of the trajectory view up to the main window: the MDI frame with its close button
p = tv.parent()
while p is not None and p.element_info.class_name != "MainWindow":
    print("parent:", p.element_info.class_name, p.element_info.automation_id, p.window_text(), p.rectangle(),
          [(b.window_text(), b.element_info.automation_id) for b in p.children(control_type="Button")])
    p = p.parent()

# open View on the menu bar (first top-level item named View) and list its items
view = [c for b in bars for c in b.children() if c.window_text() == "View"]
print("View top items:", len(view))
if view:
    click(view[0], 0.2); time.sleep(0.6)
    items = [(m.window_text(), m.rectangle()) for m in mw.descendants(control_type="MenuItem")
             if m.is_visible() and m.window_text()]
    print("visible menu items:", items[:40])
    ep = [m for m in mw.descendants(control_type="MenuItem") if m.window_text() == "Edit Parameters"]
    print("Edit Parameters:", [(m.rectangle(), m.is_visible()) for m in ep])
    keyboard.send_keys("{ESC}")
