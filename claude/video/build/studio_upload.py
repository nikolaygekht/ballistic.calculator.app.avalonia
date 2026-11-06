"""Upload episodes through YouTube Studio in the Chrome window started with --remote-debugging-port=9222
(profile secrets/chrome-youtube, already signed in) - used because the API login is blocked.
Each video: file, title, description, thumbnail, series playlist, not made for kids, tags, PRIVATE, Save.
Every field is read back after it is set; a mismatch stops the run. Video links go to youtube-ids.json.
Usage: claude/video/.venv/Scripts/python.exe claude/video/build/studio_upload.py ep05 ep06 ..."""
import json, pathlib, re, sys
from playwright.sync_api import sync_playwright

VIDEO = pathlib.Path(__file__).resolve().parent.parent
KIT = VIDEO / "upload"
IDS = VIDEO / "youtube-ids.json"
CHANNEL = "UC2IsMmLsSDYhi1RaGmhXbBQ"
PLAYLIST = "Ballistic Calculator 2"
MD = (VIDEO / "youtube-metadata.md").read_text(encoding="utf-8")


def meta(n):
    sec = MD[MD.index(f"## Episode {n}\n"):]
    nxt = re.search(r"\n## ", sec[3:]); sec = sec[: nxt.start() + 3] if nxt else sec
    return (re.search(r"\*\*Title:\*\* (.+)", sec).group(1).strip(), re.search(r"```\n(.*?)\n```", sec, re.S).group(1),
            re.search(r"\*\*Tags\*\*[^:]*: (.+)", sec).group(1).strip())


def norm(s):
    return re.sub(r"\s+", " ", s).strip()


def upload(pg, name):
    n = int(name[2:])
    stem = next(p.stem for p in KIT.glob(f"{n:02d} *.mp4"))
    title, desc, tags = meta(n)
    print(f"{name}: {stem}", flush=True)
    pg.goto(f"https://studio.youtube.com/channel/{CHANNEL}/videos/upload?d=ud")
    d = pg.locator("ytcp-uploads-dialog")
    d.locator("input[name=Filedata]").wait_for(state="attached", timeout=30000)
    d.locator("input[name=Filedata]").set_input_files(str(KIT / f"{stem}.mp4"))
    d.locator("#title-textarea #textbox").wait_for(timeout=90000)
    pg.wait_for_timeout(2500)
    for sel, text in (("#title-textarea #textbox", title), ("#description-textarea #textbox", desc)):
        box = d.locator(sel)
        box.click(); pg.keyboard.press("Control+A"); pg.keyboard.press("Delete")
        pg.keyboard.type(text); pg.wait_for_timeout(600)
        if norm(box.inner_text()) != norm(text):
            raise SystemExit(f"   {sel}: text did not stick")
    print("   title, description ok", flush=True)
    d.locator("input#file-loader").set_input_files(str(KIT / f"{stem}.jpg"))
    pg.wait_for_timeout(3000)
    d.locator("ytcp-video-metadata-playlists ytcp-text-dropdown-trigger").click()
    dlg = pg.locator("ytcp-playlist-dialog")
    li = dlg.locator("li").filter(has_text=PLAYLIST).first
    li.wait_for(timeout=15000)
    if li.locator("[role=checkbox]").first.get_attribute("aria-checked") != "true":
        li.locator("ytcp-checkbox-lit").first.click(); pg.wait_for_timeout(600)
    dlg.locator("ytcp-button.done-button").click(); pg.wait_for_timeout(1200)
    if PLAYLIST not in d.locator("ytcp-video-metadata-playlists").inner_text():
        raise SystemExit("   playlist not set")
    kids = d.locator("tp-yt-paper-radio-button[name=VIDEO_MADE_FOR_KIDS_NOT_MFK]")
    if kids.get_attribute("aria-checked") != "true":
        kids.click(); pg.wait_for_timeout(400)
    print("   thumbnail, playlist, not-for-kids ok", flush=True)
    if "Show more" in d.locator("#toggle-button").inner_text():
        d.locator("#toggle-button").click(); pg.wait_for_timeout(1200)
    d.locator("input[aria-label=Tags]").click(); pg.keyboard.type(tags + ","); pg.wait_for_timeout(800)
    chips = d.locator("ytcp-free-text-chip-bar ytcp-chip, #tags-container ytcp-chip").count()
    if chips != len(tags.split(",")):
        raise SystemExit(f"   tags: {chips} chips for {len(tags.split(','))} tags")
    print(f"   {chips} tags ok", flush=True)
    link = d.locator("a[href*='youtu.be/']").first.get_attribute("href") if d.locator("a[href*='youtu.be/']").count() else None
    for want in ("VIDEO_ELEMENTS", "CHECKS", "REVIEW"):
        d.locator("ytcp-button#next-button").click()
        for _ in range(20):
            pg.wait_for_timeout(500)
            if d.get_attribute("workflow-step") == want:
                break
        else:
            raise SystemExit(f"   stuck before {want}")
    d.locator("tp-yt-paper-radio-button[name=PRIVATE]").click(); pg.wait_for_timeout(600)
    if d.locator("tp-yt-paper-radio-button[name=PRIVATE]").get_attribute("aria-checked") != "true":
        raise SystemExit("   private not selected")
    d.locator("ytcp-button#done-button").click()
    pg.wait_for_timeout(5000)
    ids = json.loads(IDS.read_text(encoding="utf-8")) if IDS.exists() else {}
    ids[name] = link
    IDS.write_text(json.dumps(ids, indent=1), encoding="utf-8")
    print(f"   saved PRIVATE: {link}", flush=True)


with sync_playwright() as p:
    br = p.chromium.connect_over_cdp("http://127.0.0.1:9222")
    pg = [pg for c in br.contexts for pg in c.pages if "studio.youtube.com" in pg.url][0]
    for name in sys.argv[1:]:
        upload(pg, name)
