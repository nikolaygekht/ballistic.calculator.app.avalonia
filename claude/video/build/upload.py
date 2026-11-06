"""Upload the videos to YouTube (Data API v3), always as PRIVATE - the user publishes by hand in Studio.
Run with the venv:  claude/video/.venv/Scripts/python.exe claude/video/build/upload.py <command>

  auth            one-time browser consent; writes secrets/youtube-token.json
  check           which channel the token belongs to, and the playlist (read-only)
  <name> [...]    upload: ep01..ep10, ad (16:9), short (9:16)

Title, description and tags come from claude/video/youtube-metadata.md (build/make_metadata.py).
Thumbnails from claude/video/thumbs/. Episodes are appended to the series playlist. Each upload's video id is
written to claude/video/youtube-ids.json, and a name already there is never uploaded twice.
The OAuth client secret and the token live in <repo>/secrets/ (gitignored)."""
import json, pathlib, re, sys

from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
from googleapiclient.http import MediaFileUpload

VIDEO = pathlib.Path(__file__).resolve().parent.parent
REPO = VIDEO.parent.parent
SECRET = REPO / "secrets" / "client_secret.json"
TOKEN = REPO / "secrets" / "youtube-token.json"
IDS = VIDEO / "youtube-ids.json"
SCOPES = ["https://www.googleapis.com/auth/youtube"]          # covers upload, thumbnails and playlists
PLAYLIST_ID = "PLLcWDVvDdkbU"

FILES = {f"ep{n:02d}": (VIDEO / f"build/ep{n:02d}/ep{n:02d}.mp4", VIDEO / f"thumbs/ep{n:02d}.jpg", f"## Episode {n}")
         for n in range(1, 11)}
FILES["ad"] = (VIDEO / "ad30/remotion/out/ballistic-calculator-2-16x9.mp4", VIDEO / "thumbs/ad30.jpg", "## Ad 16:9")
FILES["short"] = (VIDEO / "ad30/remotion/out/ballistic-calculator-2-9x16.mp4", VIDEO / "thumbs/ad30-short.jpg", "## Short")


def creds():
    c = Credentials.from_authorized_user_file(str(TOKEN), SCOPES) if TOKEN.exists() else None
    if c and c.expired and c.refresh_token:
        c.refresh(Request())
        TOKEN.write_text(c.to_json(), encoding="utf-8")
    if not c or not c.valid:
        raise SystemExit("No valid token: run  upload.py auth  first.")
    return c


def meta(head):
    md = (VIDEO / "youtube-metadata.md").read_text(encoding="utf-8")
    sec = md[md.index(head + "\n"):]
    nxt = re.search(r"\n## ", sec[3:])
    sec = sec[: nxt.start() + 3] if nxt else sec
    title = re.search(r"\*\*Title:\*\* (.+)", sec).group(1).strip()
    desc = re.search(r"```\n(.*?)\n```", sec, re.S).group(1)
    tags = [t.strip() for t in re.search(r"\*\*Tags\*\*[^:]*: (.+)", sec).group(1).split(",")]
    return title, desc, tags


def upload(yt, name):
    ids = json.loads(IDS.read_text(encoding="utf-8")) if IDS.exists() else {}
    if name in ids:
        print(f"{name}: already uploaded as https://youtu.be/{ids[name]} - skipped"); return
    video, thumb, head = FILES[name]
    title, desc, tags = meta(head)
    body = {
        "snippet": {"title": title, "description": desc, "tags": tags, "categoryId": "28",
                    "defaultLanguage": "en", "defaultAudioLanguage": "en"},
        "status": {"privacyStatus": "private", "selfDeclaredMadeForKids": False},
    }
    print(f"{name}: uploading {video.name} ({video.stat().st_size / 1e6:.1f} MB) - {title}")
    req = yt.videos().insert(part="snippet,status", body=body,
                             media_body=MediaFileUpload(str(video), chunksize=8 * 1024 * 1024, resumable=True))
    resp = None
    while resp is None:
        status, resp = req.next_chunk()
        if status:
            print(f"   {int(status.progress() * 100)} %", flush=True)
    vid = resp["id"]
    ids[name] = vid
    IDS.write_text(json.dumps(ids, indent=1), encoding="utf-8")
    print(f"   uploaded: https://youtu.be/{vid}  (private)")
    try:
        yt.thumbnails().set(videoId=vid, media_body=MediaFileUpload(str(thumb))).execute()
        print(f"   thumbnail: {thumb.name}")
    except HttpError as e:
        print(f"   thumbnail NOT set ({e.status_code}): {e.reason}")
    if name.startswith("ep"):
        yt.playlistItems().insert(part="snippet", body={"snippet": {
            "playlistId": PLAYLIST_ID, "resourceId": {"kind": "youtube#video", "videoId": vid}}}).execute()
        print(f"   added to the playlist")


def main():
    if len(sys.argv) < 2:
        raise SystemExit(__doc__)
    cmd = sys.argv[1]
    if cmd == "auth":
        flow = InstalledAppFlow.from_client_secrets_file(str(SECRET), SCOPES)
        c = flow.run_local_server(port=0, open_browser=True, prompt="consent")
        TOKEN.write_text(c.to_json(), encoding="utf-8")
        print("token saved:", TOKEN)
        return
    yt = build("youtube", "v3", credentials=creds())
    if cmd == "check":
        ch = yt.channels().list(part="snippet", mine=True).execute()["items"]
        for c in ch:
            print("channel:", c["snippet"]["title"], c["id"])
        pl = yt.playlists().list(part="snippet,contentDetails", id=PLAYLIST_ID).execute()["items"]
        for p in pl:
            print("playlist:", p["snippet"]["title"], "-", p["contentDetails"]["itemCount"], "videos")
        return
    for name in sys.argv[1:]:
        if name not in FILES:
            raise SystemExit(f"unknown name {name}; use ep01..ep10, ad, short")
    for name in sys.argv[1:]:
        upload(yt, name)


main()
