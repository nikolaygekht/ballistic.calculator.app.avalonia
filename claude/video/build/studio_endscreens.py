"""Set the end screens of the physics series in YouTube Studio (Chrome with --remote-debugging-port=9222).
Each episode ends on a 6 s end card (education/remotion/src/EndCard.tsx) with two dashed frames, 620 x 349 px at
(125, 424) and (1175, 424) of 1920 x 1080; this puts one element into each frame for the last 6 s.
Usage: claude/video/.venv/Scripts/python.exe claude/video/build/studio_endscreens.py phys01 [phys02 ...] [--dry]
--dry builds the elements and takes a screenshot but does not save."""
import json, pathlib, re, sys
from playwright.sync_api import sync_playwright

VIDEO = pathlib.Path(__file__).resolve().parent.parent
IDS = json.loads((VIDEO / "youtube-ids.json").read_text(encoding="utf-8"))
SHOTS = pathlib.Path(__import__("tempfile").gettempdir()) / "studio-endscreens"
SLOTS = [(125, 424), (1175, 424)]
SLOT_W, SLOT_H = 620, 349
END_S = 6
PHYS = "How Ballistic Calculator 2 Works"
APP = "Ballistic Calculator 2 — build one shot"
TITLE = {1: "Drag: where the velocity goes", 2: "How a dot flies: the point-mass model",
         3: "The wind: why 3.35 m and not 9", 4: "What a dot can't do: spin and the turning Earth"}
# (left element, right element): ("VIDEO", episode) or ("PLAYLIST", playlist title)
PLAN = {1: (("PLAYLIST", PHYS), ("VIDEO", 2)), 2: (("VIDEO", 1), ("VIDEO", 3)),
        3: (("VIDEO", 2), ("VIDEO", 4)), 4: (("VIDEO", 3), ("PLAYLIST", APP))}


def vid(name):
    return IDS[name].rsplit("/", 1)[1]


def tc_to_frames(s):
    m, sec, fr = (int(x) for x in s.split(":"))
    return (m * 60 + sec) * 30 + fr


def frames_to_tc(f, like):
    """The editor's own format: minutes as wide as in `like` (after 10 min it writes 09:57:08), seconds, frames."""
    return f"{f // 1800:0{len(like.split(':')[0])}d}:{(f // 30) % 60:02d}:{f % 30:02d}"


def add_element(pg, kind, what, slot):
    pg.locator("#add-element-menu-button").click(); pg.wait_for_timeout(800)
    pg.locator(f"tp-yt-paper-item[test-id={kind}]:visible").click(); pg.wait_for_timeout(2500)
    if kind == "VIDEO":
        pg.locator("ytve-endscreen-editor-video-element-options tp-yt-paper-radio-button[name=VIDEO_TYPE_FIXED]").click()
        pg.wait_for_timeout(2500)
    # the picker dialog: a card whose text starts with the title
    dlg = pg.locator("tp-yt-paper-dialog:visible, ytcp-dialog:visible").filter(has_text=re.compile("Choose|Select")).last
    dlg.wait_for(timeout=15000)
    card = dlg.get_by_text(TITLE[what] if kind == "VIDEO" else what, exact=False).locator("visible=true").first
    card.wait_for(timeout=15000); card.click(); pg.wait_for_timeout(2500)
    # the time: the last END_S seconds
    times = pg.locator("ytve-endscreen-editor-options-panel input:visible")
    end_tc = times.nth(1).input_value()
    start_tc = frames_to_tc(tc_to_frames(end_tc) - END_S * 30, end_tc)
    times.nth(0).click(); pg.keyboard.press("Control+A"); pg.keyboard.type(start_tc); pg.keyboard.press("Enter")
    pg.wait_for_timeout(1200)
    if times.nth(0).input_value() != start_tc:
        raise SystemExit(f"   start time reads {times.nth(0).input_value()}, wanted {start_tc}")
    # the place: drag the selected element's centre onto the frame's centre
    ov = pg.locator("ytve-endscreen-editor-preview-overlay").first.bounding_box()
    k = ov["width"] / 1920
    item = pg.locator("ytve-endscreen-editor-preview-overlay-item.selected").first
    b = item.bounding_box()
    tx, ty = ov["x"] + (slot[0] + SLOT_W / 2) * k, ov["y"] + (slot[1] + SLOT_H / 2) * k
    pg.mouse.move(b["x"] + b["width"] / 2, b["y"] + b["height"] / 2); pg.mouse.down()
    for i in range(1, 11):
        pg.mouse.move(b["x"] + b["width"] / 2 + (tx - b["x"] - b["width"] / 2) * i / 10,
                      b["y"] + b["height"] / 2 + (ty - b["y"] - b["height"] / 2) * i / 10); pg.wait_for_timeout(40)
    pg.mouse.up(); pg.wait_for_timeout(800)
    b = item.bounding_box()
    got = (round((b["x"] - ov["x"]) / k), round((b["y"] - ov["y"]) / k), round(b["width"] / k), round(b["height"] / k))
    t = [x.input_value() for x in times.all()]
    print(f"   {kind} {what}: at {got[:2]} size {got[2:]} · {t[0]}–{t[1]}", flush=True)
    if abs(got[0] - slot[0]) > 12 or abs(got[1] - slot[1]) > 12:
        raise SystemExit(f"   element landed at {got[:2]}, wanted {slot}")


def run(pg, name, dry):
    n = int(name[4:])
    pg.goto(f"https://studio.youtube.com/video/{vid(name)}/edit"); pg.wait_for_timeout(5000)
    pg.locator("#endscreen-editor-link").click(); pg.wait_for_timeout(5000)
    if pg.locator("ytve-endscreen-editor-preview-overlay-item.visible").count():
        raise SystemExit(f"{name}: already has end screen elements; remove them first")
    print(f"{name}: {TITLE[n]}", flush=True)
    for (kind, what), slot in zip(PLAN[n], SLOTS):
        add_element(pg, kind, what, slot)
    if dry:
        SHOTS.mkdir(exist_ok=True)
        pg.screenshot(path=str(SHOTS / f"{name}.png"))
        print(f"   dry run: not saved, see {SHOTS / name}.png", flush=True); return
    pg.locator("ytve-endscreen-modal ytcp-button#save-button, ytcp-button#save-button:visible").first.click()
    pg.wait_for_timeout(4000)
    # read it back: reopen the editor and count the elements
    pg.goto(f"https://studio.youtube.com/video/{vid(name)}/edit"); pg.wait_for_timeout(5000)
    pg.locator("#endscreen-editor-link").click(); pg.wait_for_timeout(5000)
    have = pg.locator("ytve-endscreen-editor-preview-overlay-item.visible").count()
    pg.locator("ytcp-button#discard-button:visible").first.click(); pg.wait_for_timeout(1500)
    if have != 2:
        raise SystemExit(f"   saved, but the editor shows {have} elements")
    print("   saved, 2 elements read back", flush=True)


with sync_playwright() as p:
    br = p.chromium.connect_over_cdp("http://127.0.0.1:9222")
    pg = [pg for c in br.contexts for pg in c.pages if "studio.youtube.com" in pg.url][0]
    pg.on("dialog", lambda d: d.accept())   # "leave the page?" on a discarded editor
    dry = "--dry" in sys.argv
    for name in [a for a in sys.argv[1:] if a.startswith("phys")]:
        run(pg, name, dry)
