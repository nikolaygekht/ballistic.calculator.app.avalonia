"""Contact sheet of review stills: python sheet.py <prefix> [cols]  -> out/sheet-<prefix>.jpg"""
import glob, sys
from PIL import Image, ImageDraw
prefix = sys.argv[1]; cols = int(sys.argv[2]) if len(sys.argv) > 2 else 3
import os, time
files = sorted(f for f in glob.glob(f'out/stills/{prefix}*.jpg') if time.time() - os.path.getmtime(f) < 900)
ims = [Image.open(f) for f in files]
w, h = ims[0].size
rows = (len(ims) + cols - 1) // cols
sheet = Image.new('RGB', (cols * w, rows * h), (0, 0, 0))
for i, (im, f) in enumerate(zip(ims, files)):
    x, y = (i % cols) * w, (i // cols) * h
    sheet.paste(im, (x, y))
    ImageDraw.Draw(sheet).text((x + 8, y + 6), f.split('stills')[-1][1:-4], fill=(255, 255, 0))
sheet.save(f'out/sheet-{prefix}.jpg', quality=80)
print(f'out/sheet-{prefix}.jpg', sheet.size)
