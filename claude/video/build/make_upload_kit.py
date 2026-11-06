"""Everything for a manual upload in one folder: claude/video/upload/
  NN <name>.mp4 + NN <name>.jpg (video and its thumbnail side by side) and index.html - open it in a browser:
  per video the file names, Title / Description / Tags with Copy buttons, plus the channel and playlist texts.
Texts come from youtube-metadata.md (build/make_metadata.py). The folder is gitignored.
Usage: python make_upload_kit.py"""
import html, json, pathlib, re, shutil

VIDEO = pathlib.Path(__file__).resolve().parent.parent
OUT = VIDEO / "upload"
MD = (VIDEO / "youtube-metadata.md").read_text(encoding="utf-8")

ITEMS = [  # (file stem, metadata heading, video, thumbnail, notes)
    ("00a Ad 16x9", "## Ad 16:9", "ad30/remotion/out/ballistic-calculator-2-16x9.mp4", "thumbs/ad30.jpg",
     "The channel's first video. Not in the playlist unless you want it there."),
    ("00b Short 9x16", "## Short", "ad30/remotion/out/ballistic-calculator-2-9x16.mp4", "thumbs/ad30-short.jpg",
     "Uploads as a Short by itself (vertical, under 3 min). Related video: episode 1 or the 16:9 ad. "
     "If Studio offers no custom thumbnail, pick the frame at about 6.6 s."),
]
NAMES = ["Your first trajectory", "Table, chart, reticle, summary", "Weather and wind", "Spin drift, jump, Coriolis",
         "Dialled clicks, winter zero", "A measured drag curve", "Everything at once", "300 Blackout, one zero two loads",
         "Shooting at an angle", "Your own drag table"]
for n, name in enumerate(NAMES, 1):
    ITEMS.append((f"{n:02d} {name}", f"## Episode {n}", f"build/ep{n:02d}/ep{n:02d}.mp4", f"thumbs/ep{n:02d}.jpg",
                  "Playlist: Ballistic Calculator 2 (add in order)."))


def section(head):
    sec = MD[MD.index(head + "\n"):]
    nxt = re.search(r"\n## ", sec[3:])
    return sec[: nxt.start() + 3] if nxt else sec


def meta(head):
    sec = section(head)
    title = re.search(r"\*\*Title:\*\* (.+)", sec).group(1).strip()
    desc = re.search(r"```\n(.*?)\n```", sec, re.S).group(1)
    tags = re.search(r"\*\*Tags\*\*[^:]*: (.+)", sec).group(1).strip()
    return title, desc, tags


OUT.mkdir(exist_ok=True)
cards = []
for stem, head, video, thumb, note in ITEMS:
    v, t = VIDEO / video, VIDEO / thumb
    shutil.copy2(v, OUT / f"{stem}.mp4")
    shutil.copy2(t, OUT / f"{stem}.jpg")
    title, desc, tags = meta(head)
    cards.append((stem, title, desc, tags, note))
    print(f"{stem}: {v.stat().st_size / 1e6:.1f} MB video, title {len(title)} chars, description {len(desc)} chars")

channel = re.search(r"```\n(.*?)\n```", section("## Channel description"), re.S).group(1)
pl = section("## Playlist")
pl_title = re.search(r"\*\*Title:\*\* (.+)", pl).group(1).strip()
pl_desc = re.search(r"```\n(.*?)\n```", pl, re.S).group(1)

texts = {}
def field(key, label, value, big=False):
    texts[key] = value
    box = (f'<textarea readonly rows="{min(18, value.count(chr(10)) + 2)}">{html.escape(value)}</textarea>' if big
           else f'<div class="val">{html.escape(value)}</div>')
    return (f'<div class="field"><div class="lab">{label} <button onclick="cp(\'{key}\', this)">Copy</button>'
            f'<span class="n">{len(value)} chars</span></div>{box}</div>')

body = []
for i, (stem, title, desc, tags, note) in enumerate(cards):
    body.append(f"""<section><div class="head"><img src="{html.escape(stem)}.jpg"><div>
<h2>{html.escape(stem)}</h2><div class="files">{html.escape(stem)}.mp4 &nbsp;·&nbsp; {html.escape(stem)}.jpg</div>
<div class="note">{html.escape(note)}</div></div></div>
{field(f"t{i}", "Title", title)}{field(f"d{i}", "Description", desc, True)}{field(f"g{i}", "Tags", tags)}</section>""")
body.append(f"""<section><h2>Channel description</h2>{field("ch", "Description", channel, True)}</section>
<section><h2>Playlist</h2>{field("plt", "Title", pl_title)}{field("pld", "Description", pl_desc, True)}</section>""")

page = f"""<!doctype html><html><head><meta charset="utf-8"><title>BC2 upload kit</title><style>
body {{ font-family: "Segoe UI", sans-serif; background: #f4f5f7; color: #1b1f24; margin: 0; padding: 24px 32px; }}
h1 {{ margin: 0 0 6px; }} .intro {{ color: #555; margin-bottom: 18px; line-height: 1.5; }}
section {{ background: #fff; border-radius: 10px; padding: 18px 22px; margin: 0 0 18px; box-shadow: 0 1px 3px rgba(0,0,0,.08); }}
.head {{ display: flex; gap: 18px; align-items: center; margin-bottom: 8px; }}
.head img {{ height: 110px; border-radius: 6px; }} h2 {{ margin: 0; font-size: 22px; }}
.files {{ font-family: Consolas, monospace; color: #444; margin-top: 4px; }} .note {{ color: #8a5a00; margin-top: 6px; }}
.field {{ margin-top: 12px; }} .lab {{ font-weight: 600; display: flex; gap: 10px; align-items: center; }}
.n {{ font-weight: 400; color: #888; font-size: 13px; }}
button {{ font: inherit; font-size: 13px; padding: 3px 14px; border: 1px solid #0b6bcb; background: #0b6bcb; color: #fff;
         border-radius: 5px; cursor: pointer; }} button.ok {{ background: #1e8e3e; border-color: #1e8e3e; }}
.val {{ margin-top: 6px; padding: 8px 10px; background: #f7f8fa; border: 1px solid #dde1e6; border-radius: 6px; }}
textarea {{ width: 100%; box-sizing: border-box; margin-top: 6px; font: 14px/1.45 Consolas, monospace; padding: 8px 10px;
           border: 1px solid #dde1e6; border-radius: 6px; background: #f7f8fa; resize: vertical; }}
</style></head><body>
<h1>Ballistic Calculator 2 — upload kit</h1>
<div class="intro">Each video and its thumbnail sit side by side in this folder under the same name.
For every upload: <b>Visibility: Private</b> first, check, then publish · <b>No, it's not made for kids</b> ·
Category <b>Science &amp; Technology</b> · Language <b>English</b>. Chapters work by themselves (they are in the description).</div>
{''.join(body)}
<script>
const T = {json.dumps(texts, ensure_ascii=False)};
function cp(k, b) {{
  const done = () => {{ b.textContent = 'Copied'; b.classList.add('ok'); setTimeout(() => {{ b.textContent = 'Copy'; b.classList.remove('ok'); }}, 1500); }};
  if (navigator.clipboard) navigator.clipboard.writeText(T[k]).then(done, () => fallback(k, done)); else fallback(k, done);
}}
function fallback(k, done) {{
  const t = document.createElement('textarea'); t.value = T[k]; document.body.appendChild(t); t.select();
  document.execCommand('copy'); t.remove(); done();
}}
</script></body></html>"""
(OUT / "index.html").write_text(page, encoding="utf-8")
print(OUT / "index.html")
