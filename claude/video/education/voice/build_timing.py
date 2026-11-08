"""Pick one take per scene, loudness-normalise it into remotion/public/voice/ep01/sNN.mp3 and write timing.json
(duration + word times) for the scenes. ElevenLabs takes come in at about -34 LUFS; they are brought to the
YouTube level (-14 LUFS, true peak -1.5 dBTP, same as the 30 s ad) with a two-pass loudnorm. Usage: python build_timing.py [sceneTake ...]  e.g. 04b"""
import json, re, subprocess, sys, os
base = os.path.dirname(os.path.abspath(__file__))
words = json.load(open(os.path.join(base, 'ep01-words.json'), encoding='utf-8'))
pick = {n: 'a' for n in range(1, 11)}
for a in sys.argv[1:]:
    pick[int(a[:2])] = a[2]
FF = os.path.join(base, '..', 'remotion', 'node_modules', '@remotion', 'compositor-win32-x64-msvc', 'ffmpeg.exe')
LN = 'loudnorm=I=-14:TP=-1.5:LRA=11'


def normalise(src, out):
    probe = subprocess.run([FF, '-hide_banner', '-i', src, '-af', LN + ':print_format=json', '-f', 'null', '-'],
                           capture_output=True, text=True).stderr
    m = json.loads(re.search(r'\{[^{}]*"input_i"[^{}]*\}', probe).group(0))
    af = (f"{LN}:measured_I={m['input_i']}:measured_TP={m['input_tp']}:measured_LRA={m['input_lra']}"
          f":measured_thresh={m['input_thresh']}:offset={m['target_offset']}:linear=true")
    subprocess.run([FF, '-hide_banner', '-v', 'error', '-y', '-i', src, '-af', af, '-ar', '44100',
                    '-c:a', 'libmp3lame', '-b:a', '192k', out], check=True)


dst = os.path.join(base, '..', 'remotion', 'public', 'voice', 'ep01')
scenes = []
for n in range(1, 11):
    k = f's{n:02d}{pick[n]}'
    normalise(os.path.join(dst, 'takes', k + '.mp3'), os.path.join(dst, f's{n:02d}.mp3'))
    scenes.append({'n': n, 'take': k, 'file': f'voice/ep01/s{n:02d}.mp3', 'dur': round(words[k]['duration'], 3), 'words': words[k]['words']})
json.dump({'scenes': scenes}, open(os.path.join(dst, 'timing.json'), 'w', encoding='utf-8'), indent=0)
print({s['take']: s['dur'] for s in scenes}, 'total', round(sum(s['dur'] for s in scenes), 1))
