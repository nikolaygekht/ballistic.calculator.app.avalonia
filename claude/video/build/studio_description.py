"""Rewrite the description of uploaded videos from the metadata files, in YouTube Studio (Chrome with
--remote-debugging-port=9222). The text is read back after saving.
Usage: claude/video/.venv/Scripts/python.exe claude/video/build/studio_description.py phys01 [phys02 ...]"""
import json, pathlib, sys
from playwright.sync_api import sync_playwright
from studio_upload import files, norm, VIDEO

IDS = json.loads((VIDEO / "youtube-ids.json").read_text(encoding="utf-8"))

with sync_playwright() as p:
    br = p.chromium.connect_over_cdp("http://127.0.0.1:9222")
    pg = [pg for c in br.contexts for pg in c.pages if "studio.youtube.com" in pg.url][0]
    pg.on("dialog", lambda d: d.accept())
    for name in sys.argv[1:]:
        desc = files(name)[2][1]
        url = f"https://studio.youtube.com/video/{IDS[name].rsplit('/', 1)[1]}/edit"
        pg.goto(url); pg.wait_for_timeout(5000)
        box = pg.locator("#description-textarea #textbox")
        box.click(); pg.keyboard.press("Control+A"); pg.keyboard.press("Delete"); pg.keyboard.type(desc); pg.wait_for_timeout(800)
        pg.locator("ytcp-button#save").click(); pg.wait_for_timeout(4000)
        pg.goto(url); pg.wait_for_timeout(5000)
        if norm(pg.locator("#description-textarea #textbox").inner_text()) != norm(desc):
            raise SystemExit(f"{name}: the description did not stick")
        print(f"{name}: description saved and read back", flush=True)
