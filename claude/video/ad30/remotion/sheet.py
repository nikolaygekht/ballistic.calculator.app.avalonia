"""Contact sheet of the review stills: python sheet.py Ad16x9|Ad9x16 [cols] -> out/sheet-<comp>.jpg"""
import sys, pathlib
from PIL import Image, ImageDraw, ImageFont
comp = sys.argv[1]; cols = int(sys.argv[2]) if len(sys.argv) > 2 else (2 if comp == "Ad16x9" else 4)
files = sorted(pathlib.Path("out/stills").glob(f"{comp}-*.jpg"))
ims = [Image.open(f) for f in files]
sc = 0.5 if comp == "Ad16x9" else 0.4
w, h = int(ims[0].width * sc), int(ims[0].height * sc)
rows = (len(ims) + cols - 1) // cols
sheet = Image.new("RGB", (cols * w + (cols + 1) * 8, rows * h + (rows + 1) * 8), "#444")
font = ImageFont.truetype("consola.ttf", 22)
for i, (f, im) in enumerate(zip(files, ims)):
    x, y = 8 + (i % cols) * (w + 8), 8 + (i // cols) * (h + 8)
    sheet.paste(im.resize((w, h), Image.LANCZOS), (x, y))
    fr = int(f.stem.split("-")[1]); ImageDraw.Draw(sheet).text((x + 8, y + 6), f"f{fr} {fr/30:.2f}s", fill="#ff0", font=font)
sheet.save(f"out/sheet-{comp}.jpg", quality=85); print(f"out/sheet-{comp}.jpg", sheet.size)
