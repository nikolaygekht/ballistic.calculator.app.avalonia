"""Watch the user's own pass through a YouTube Studio upload (read-only) to learn the dialog's pages and fields.
Connects to the Edge window started with --remote-debugging-port=9222; every 1.5 s, if the upload dialog changed,
saves a screenshot and the dialog's interactive elements (tag, id, name, aria-label, text) to <out>/NNN.*
Usage: python yt_watch.py <out dir> [minutes]"""
import hashlib, json, pathlib, sys, time
from playwright.sync_api import sync_playwright

OUT = pathlib.Path(sys.argv[1]); OUT.mkdir(parents=True, exist_ok=True)
END = time.time() + 60 * float(sys.argv[2] if len(sys.argv) > 2 else 40)
JS = """() => {
  const root = document.querySelector('ytcp-uploads-dialog') || document.body;
  const out = [];
  for (const e of root.querySelectorAll('input, textarea, button, [contenteditable="true"], tp-yt-paper-radio-button, tp-yt-paper-checkbox, ytcp-button, ytcp-checkbox-lit, ytcp-form-select, ytcp-text-dropdown-trigger, tp-yt-paper-item, ytcp-ve, #textbox')) {
    const r = e.getBoundingClientRect();
    if (r.width === 0 && r.height === 0 && e.type !== 'file') continue;
    out.push({tag: e.tagName.toLowerCase(), id: e.id, name: e.getAttribute('name'), type: e.getAttribute('type'),
              aria: e.getAttribute('aria-label'), checked: e.getAttribute('aria-checked'),
              cls: (e.className && e.className.baseVal === undefined ? e.className : '').toString().slice(0, 80),
              text: (e.innerText || e.value || '').trim().slice(0, 80)});
  }
  return {url: location.href, dialog: !!document.querySelector('ytcp-uploads-dialog'),
          title: (document.querySelector('ytcp-uploads-dialog #dialog-title') || {}).innerText || '', els: out};
}"""

with sync_playwright() as p:
    br = p.chromium.connect_over_cdp("http://127.0.0.1:9222")
    last, n = None, 0
    while time.time() < END:
        for ctx in br.contexts:
            for pg in ctx.pages:
                if "studio.youtube.com" not in pg.url:
                    continue
                try:
                    snap = pg.evaluate(JS)
                except Exception:
                    continue
                h = hashlib.md5(json.dumps([snap["title"], [(e["tag"], e["id"], e["name"], e["aria"], e["checked"])
                                                            for e in snap["els"]]]).encode()).hexdigest()
                if h != last:
                    last, n = h, n + 1
                    (OUT / f"{n:03d}.json").write_text(json.dumps(snap, ensure_ascii=False, indent=1), encoding="utf-8")
                    try:
                        pg.screenshot(path=str(OUT / f"{n:03d}.jpg"), type="jpeg", quality=60)
                    except Exception:
                        pass
                    print(f"{n:03d} {snap['title'][:60]!r} {len(snap['els'])} elements {snap['url'][:80]}", flush=True)
        time.sleep(1.5)
