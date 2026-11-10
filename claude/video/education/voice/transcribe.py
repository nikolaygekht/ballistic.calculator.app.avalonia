"""Transcribe every take with word timestamps and score it against the script.
Usage: python transcribe.py <takes_dir> <SCRIPTS.md> <out.json> [episode, default 1]"""
import json, re, sys, difflib, glob, os
import subprocess, numpy as np
from faster_whisper import WhisperModel
FFMPEG = os.path.join(os.path.dirname(__file__), '..', 'remotion', 'node_modules', '@remotion', 'compositor-win32-x64-msvc', 'ffmpeg.exe')

def load(f):
    raw = subprocess.run([FFMPEG, '-v', 'error', '-i', f, '-f', 'wav', '-acodec', 'pcm_s16le', '-ac', '1', '-ar', '16000', '-'], capture_output=True, check=True).stdout
    i = raw.index(b'data') + 8  # skip the RIFF header
    return np.frombuffer(raw[i:], np.int16).astype(np.float32) / 32768

takes_dir, script_md, out = sys.argv[1:4]
epn = int(sys.argv[4]) if len(sys.argv) > 4 else 1
s = open(script_md, encoding='utf-8').read()
start = s.index(f'# Episode {epn}')
end = s.find(f'# Episode {epn + 1}', start)
ep = s[start:end if end > 0 else len(s)]
blocks = re.findall(r'VOICE:\n(.*?)(?=\n\n)', ep, re.S)
norm = lambda t: re.findall(r"[a-z0-9']+", t.lower().replace('-', ' '))

try:
    model = WhisperModel('medium.en', device='cuda', compute_type='float16')
except Exception as e:
    print('cuda failed:', e); model = WhisperModel('medium.en', device='cpu', compute_type='int8')
res = {}
for f in sorted(glob.glob(os.path.join(takes_dir, 's*.mp3'))):
    name = os.path.basename(f)[:-4]
    scene = int(name[1:3])
    segs, info = model.transcribe(load(f), word_timestamps=True, language='en', beam_size=5)
    words = [{'w': w.word.strip(), 's': round(w.start, 3), 'e': round(w.end, 3)} for sg in segs for w in sg.words]
    hyp = norm(' '.join(w['w'] for w in words))
    ref = norm(blocks[scene - 1])
    sm = difflib.SequenceMatcher(None, ref, hyp)
    diffs = [(op, ' '.join(ref[a:b]), ' '.join(hyp[c:d])) for op, a, b, c, d in sm.get_opcodes() if op != 'equal']
    res[name] = {'scene': scene, 'duration': info.duration, 'ratio': round(sm.ratio(), 4), 'diffs': diffs, 'words': words}
    print(name, round(info.duration, 2), round(sm.ratio(), 4), diffs)
json.dump(res, open(out, 'w', encoding='utf-8'), indent=1)
