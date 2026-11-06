"""Tempo, first downbeat and loudness envelope of each music candidate (numpy only).
Usage: python beats.py  -> prints BPM, beat phase, and the 1-s RMS curve (dB) to see build/peak/ending."""
import pathlib, subprocess, numpy as np
FF = r"C:\exe\Video\ffmpeg\bin\ffmpeg.exe"; SR = 22050
for f in sorted((pathlib.Path(__file__).parent / "audio").glob("music-*.mp3")):
    raw = subprocess.run([FF, "-v", "error", "-i", str(f), "-ac", "1", "-ar", str(SR), "-f", "f32le", "-"], capture_output=True).stdout
    x = np.frombuffer(raw, np.float32)
    hop = 512; n = len(x) // hop
    e = np.array([np.sum(x[i*hop:(i+1)*hop] ** 2) for i in range(n)])
    on = np.maximum(0, np.diff(np.log1p(e * 1000)))          # onset strength
    on -= on.mean()
    ac = np.correlate(on, on, "full")[len(on) - 1:]
    fps = SR / hop
    lags = np.arange(len(ac)) / fps
    ok = (lags > 60 / 160) & (lags < 60 / 90)
    period = lags[ok][np.argmax(ac[ok])]
    # beat phase: offset maximizing onset sum on a beat grid
    P = period * fps
    ph = max(np.arange(0, P, 0.5), key=lambda p: on[np.round(np.arange(p, len(on) - 1, P)).astype(int)].sum())
    rms = [20 * np.log10(np.sqrt(np.mean(x[int(s*SR):int((s+1)*SR)] ** 2)) + 1e-9) for s in range(30)]
    print(f"{f.stem}: {60/period:.1f} BPM, beat {period:.3f} s, first beat {ph/fps:.3f} s")
    print("   dB/s:", " ".join(f"{r:.0f}" for r in rms))
