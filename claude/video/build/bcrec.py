"""Shared helpers for the episode recording scripts (build/epNN/record.py).

An episode script does:
    from bcrec import *
    take = Take(__file__)          # restarts the app on the recording monitor
    ... pre-roll (silent, not recorded) ...
    take.start()                   # OBS starts; T0
    take.voice("v01", action) ...  # timed cues
    take.finish()                  # OBS stops, timeline.json written
See claude/video/context.md, "Production pipeline", for the traps these helpers work around.
"""
import ctypes, ctypes.wintypes, json, math, os, pathlib, re, subprocess, sys, time
ctypes.windll.shcore.SetProcessDpiAwareness(2)
from pywinauto import Application, Desktop, mouse, keyboard

EXE = pathlib.Path(r"D:\develop\homeapps.projects\BallisticCalculator2\Desktop\BallisticCalculator\bin\Debug\net8.0\BallisticCalculator2.exe")
FFPROBE = r"C:\exe\Video\ffmpeg\bin\ffprobe.exe"
GAP = 0.5                       # silence after every clip
MON = (-2560, 0, 2560, 1440)    # recording monitor, physical px
user32 = ctypes.windll.user32


# ---------------------------------------------------------------- mouse / keyboard
def pos():
    p = ctypes.wintypes.POINT(); user32.GetCursorPos(ctypes.byref(p)); return p.x, p.y


ctypes.windll.winmm.timeBeginPeriod(1)   # 1 ms sleep granularity instead of 15.6 ms


def glide(x, y, secs=0.6):
    """Ease-in-out cursor move (pywinauto's click_input teleports). Clock-driven: a step-count loop of
    short sleeps ran ~1.4x longer than asked and every pointing cue drifted late."""
    x0, y0 = pos(); t0 = time.perf_counter()
    while True:
        t = min(1.0, (time.perf_counter() - t0) / secs); e = 0.5 - 0.5 * math.cos(math.pi * t)
        user32.SetCursorPos(int(x0 + (x - x0) * e), int(y0 + (y - y0) * e))
        if t >= 1.0:
            break
        time.sleep(0.008)


def center(el, dx=0.5, dy=0.5):
    r = el.rectangle(); return int(r.left + (r.right - r.left) * dx), int(r.top + (r.bottom - r.top) * dy)


def point(el, secs=0.6, dx=0.5, dy=0.5):
    glide(*center(el, dx, dy), secs)


def click_xy(x, y, secs=0.6):
    glide(x, y, secs); time.sleep(0.12); mouse.click(coords=pos()); time.sleep(0.35)


def click(el, secs=0.6, dx=0.5, dy=0.5):
    click_xy(*center(el, dx, dy), secs)


def rest():
    """Park the cursor on the right of the recording monitor, never near the bottom (auto-hide taskbar)."""
    glide(MON[0] + 2300, MON[1] + 700, 0.8)


def type_text(text, pause=0.09):
    keyboard.send_keys("^a", pause=0.05); time.sleep(0.1)
    keyboard.send_keys(text, pause=pause, with_spaces=True)


def at(start, t, fn):
    """Inside an action: run fn once t seconds have passed since start."""
    left = start + t - time.monotonic()
    if left > 0:
        time.sleep(left)
    fn()


def clip_len(audio_dir, clip):
    out = subprocess.run([FFPROBE, "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0",
                          str(audio_dir / f"{clip}.mp3")], capture_output=True, text=True).stdout
    return float(out)


# ---------------------------------------------------------------- the take
class Take:
    def __init__(self, script_file):
        self.here = pathlib.Path(script_file).resolve().parent
        self.audio = self.here / "audio"
        self.dry = "--dry" in sys.argv
        self.cues, self.T0, self.video0, self.obs = [], None, 0.0, None
        self.main = self._start_app()
        self.dlg = self.main.child_window(class_name="ShotParametersDialog")

    def _start_app(self):
        subprocess.run(["taskkill", "/IM", EXE.name, "/F"], capture_output=True)
        state = EXE.parent / "appstate.json"
        s = json.loads(state.read_text())
        s.update(MainWindowX=-2400, MainWindowY=100, MainWindowIsMaximized=True)
        state.write_text(json.dumps(s, indent=2))
        proc = subprocess.Popen([str(EXE)], cwd=str(EXE.parent))
        app = Application(backend="uia").connect(process=proc.pid, timeout=20)
        main = app.window(class_name="MainWindow"); main.wait("visible", timeout=20); time.sleep(2)
        self.pid = proc.pid
        return main

    # -- recording
    def start(self, lead_in=1.0):
        """T0 = the moment a full-screen black cover over the recording monitor disappears. assemble.py
        finds that frame in the video (video0). OBS's own output_duration under-reports by ~1.0 s (measured
        2026-11-03 with a flip test), which put the voice a second ahead of the picture in eps. 1-2 v1."""
        rest(); time.sleep(1)
        cover = None
        if not self.dry:
            import obsws_python as o, tkinter as tk
            cover = tk.Tk(); cover.overrideredirect(True); cover.configure(bg="black")
            cover.geometry(f"{MON[2]}x{MON[3]}+{MON[0]}+{MON[1]}"); cover.attributes("-topmost", True)
            cover.update(); time.sleep(0.5)
            self.obs = o.ReqClient(password=os.environ["OBS_WS_PASSWORD"])
            self.obs.start_record(); time.sleep(2.0)
            cover.destroy()
        self.T0 = time.monotonic()
        self.marker = cover is not None
        if self.obs:
            self.video0 = self.obs.get_record_status().output_duration / 1000.0   # kept for reference only
        time.sleep(lead_in)

    def finish(self, tail=1.0):
        time.sleep(tail)
        rec = None
        if self.obs:
            rec = self.obs.stop_record().output_path
            # stop_record returns before OBS has written the mp4's index ("moov atom not found"):
            # wait until recording is inactive and the file size has settled.
            while self.obs.get_record_status().output_active:
                time.sleep(0.5)
            last = -1
            while (size := os.path.getsize(rec)) != last:
                last = size; time.sleep(1.0)
        name = "timeline.dry.json" if self.dry else "timeline.json"
        json.dump({"recording": rec, "video0_obs": self.video0, "marker": getattr(self, "marker", False),
                   "cues": self.cues, "dry": self.dry},
                  open(self.here / name, "w"), indent=1)
        print("recording:", rec)

    def voice(self, clip, action=None):
        """Log the clip's start, run its action (gets the clip length), wait the clip out + GAP."""
        d = clip_len(self.audio, clip); start = time.monotonic()
        self.cues.append({"clip": clip, "at": round(start - self.T0, 3), "dur": d})
        print(f"{start - self.T0:7.2f}  {clip}  ({d:.2f} s)", flush=True)
        if action:
            action(d)
        left = start + d + GAP - time.monotonic()
        if left > 0:
            time.sleep(left)
        else:
            print(f"         ! action overran {clip} by {-left:.2f} s", flush=True)

    def hold(self, secs, action=None):
        start = time.monotonic()
        if action:
            action(secs)
        left = start + secs - time.monotonic()
        if left > 0:
            time.sleep(left)

    # -- finding things
    def by_id(self, *ids, root=None):
        e = root or self.dlg
        for i in ids:
            e = e.child_window(auto_id=i)
        return e

    def field(self, *ids):
        return self.by_id(*ids).child_window(auto_id="NumericPart")

    def tab(self, name):
        return self.dlg.child_window(control_type="TabItem", title=name)

    def item(self, title, ctype="ListItem"):
        return self.main.child_window(control_type=ctype, title=title)

    def menu_item(self, title):
        return self.main.child_window(control_type="MenuItem", title=title, found_index=0)

    def tv(self):
        """The newest trajectory window."""
        return [c for c in self.main.wrapper_object().descendants()
                if c.element_info.class_name == "TrajectoryView"][-1]

    def tv_find(self, ctype, title=None, auto_id=None, tv=None):
        """Pass tv=<cached t.tv()> inside timed cues: t.tv() alone scans the whole main window."""
        for c in (tv or self.tv()).descendants(control_type=ctype):
            if (title is None or c.window_text() == title) and (auto_id is None or c.element_info.automation_id == auto_id):
                return c
        raise LookupError((ctype, title, auto_id))

    def rows(self):
        return self.tv().descendants(control_type="DataItem")

    def cell_xy(self, col):
        """Centres of one column's cells, row by row — take them BEFORE a timed cue (lookups are slow)."""
        return [center(r.descendants(control_type="Text")[col]) for r in self.rows()]

    def restart(self):
        """Fresh, empty app (after a pre-roll that only prepared files)."""
        self.main = self._start_app()
        self.dlg = self.main.child_window(class_name="ShotParametersDialog")

    # -- files (the native #32770 dialogs remember the last folder per exe, across restarts)
    def save_as(self, name, slow=True):
        g = 0.5 if slow else 0.2
        click(self.menu_item("Trajectory"), g); time.sleep(0.4)
        click(self.menu_item("Save As"), g)
        d = self.main.child_window(class_name="#32770", title="Save Trajectory"); d.wait("visible", timeout=10); time.sleep(1.0)
        click(d.child_window(auto_id="1001", control_type="Edit"), g); type_text(name, 0.09 if slow else 0.01); time.sleep(0.4)
        click(d.child_window(auto_id="1", control_type="Button"), 0.6 if slow else 0.2)
        c = self.main.child_window(title_re="Confirm Save As.*")
        if c.exists(timeout=1.5):
            c.child_window(title="Yes", control_type="Button").click_input()
        time.sleep(1.0)

    def open_file(self, name, secs=0.6):
        """Trajectory → Open, click the file in the list, Open. The dialog must already show the folder."""
        click(self.menu_item("Trajectory"), secs); time.sleep(0.4)
        click(self.menu_item("Open"), secs)
        d = self.main.child_window(class_name="#32770", title="Open Trajectory"); d.wait("visible", timeout=10); time.sleep(1.2)
        # Explorer lists the file WITH its extension ("bc2-demo-metric.trajectory")
        f = d.child_window(title_re=re.escape(name) + r"(\.trajectory)?$", control_type="ListItem", found_index=0)
        if f.exists(timeout=2):
            click(f, secs, 0.15); time.sleep(0.5)
        else:                                   # not listed (wrong folder?) — type the name instead
            click(d.child_window(auto_id="1148", control_type="ComboBox"), secs); type_text(name)
        click(d.child_window(auto_id="1", control_type="Button"), secs)
        time.sleep(1.5)

    # -- composite actions
    def fill(self, el, text, pause=0.09, secs=0.45):
        click(el, secs); type_text(text, pause); time.sleep(0.25)

    def new_trajectory(self, system, glide_secs=0.5):
        click(self.menu_item("Trajectory"), glide_secs); time.sleep(0.4)
        glide(*center(self.menu_item("New")), glide_secs); time.sleep(0.4)
        click(self.menu_item(system), glide_secs)
        self.dlg.wait("visible", timeout=10); time.sleep(0.8)

    def ok_and_accept_weather(self, secs=0.5):
        click(self.dlg.child_window(control_type="Button", title="OK"), secs)
        c = self.main.child_window(class_name="Window", title="Confirm")
        if c.exists(timeout=5):
            click(c.child_window(control_type="Button", title="Yes"), secs)
        time.sleep(1.5)

    def fill_shot(self, weight, bc, table, mv, sight_preset, zero, max_range, step, pause=0.09, secs=0.45):
        """Ammunition + Rifle (sight preset) + Zero + Parameters, then OK and default weather."""
        A = ("AmmoLibPanel", "AmmoSubPanel")
        self.fill(self.field(*A, "WeightControl"), weight, pause, secs)
        self.fill(self.field(*A, "BCControl"), bc, pause, secs)
        click(self.by_id(*A, "BCControl", "TablePart"), secs); time.sleep(0.4); click(self.item(table), secs)
        self.fill(self.field(*A, "MuzzleVelocityControl"), mv, pause, secs)
        click(self.tab("Rifle"), secs); time.sleep(0.3)
        click(self.by_id("RifleSubPanel", "SightPresetCombo"), secs); time.sleep(0.4); click(self.item(sight_preset), secs)
        click(self.tab("Zero"), secs); time.sleep(0.3)
        self.fill(self.field("ZeroSubPanel", "ZeroDistanceControl"), zero, pause, secs)
        click(self.tab("Parameters"), secs); time.sleep(0.3)
        self.fill(self.field("ParametersSubPanel", "MaxRangeControl"), max_range, pause, secs)
        self.fill(self.field("ParametersSubPanel", "StepControl"), step, pause, secs)
        self.ok_and_accept_weather(secs)
