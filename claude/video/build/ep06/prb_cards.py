"""Full-screen 2560x1440 cards from PrecisionRifleBlog images (prb/), each with a source line.
Source article: https://precisionrifleblog.com/2019/06/09/g1-vs-g7-vs-custom-drag-models/
Usage: python prb_cards.py"""
from PIL import Image, ImageDraw, ImageFont

SURF, INK, INK2 = "#fcfcfb", "#0b0b0b", "#52514e"
BOLD = ImageFont.truetype(r"C:\Windows\Fonts\segoeuib.ttf", 60)
REG = ImageFont.truetype(r"C:\Windows\Fonts\segoeui.ttf", 40)
CARDS = [
    ("card-prb-profiles.png", "prb/G1-vs-G7-BC.png", "The two standard bullets", "G1: short, flat base  ·  G7: long, boat tail, pointed nose"),
    ("card-prb-hornady.png", "prb/Hornady-Drag-Models-from-Doppler-Radar.png", "Every bullet has its own drag curve",
     "Four Hornady bullets measured by radar, against the G7 standard"),
    ("card-prb-radar.png", "prb/Infinition-BR-1001-Doppler-Radar.jpg", "A Doppler radar follows the bullet",
     "and measures how fast it slows down, at every speed"),
]
for out, src, title, sub in CARDS:
    card = Image.new("RGB", (2560, 1440), SURF); d = ImageDraw.Draw(card)
    d.text((150, 70), title, font=BOLD, fill=INK); d.text((150, 160), sub, font=REG, fill=INK2)
    im = Image.open(src).convert("RGB")
    box_w, box_h = 2260, 1440 - 260 - 120
    s = min(box_w / im.width, box_h / im.height)
    im = im.resize((int(im.width * s), int(im.height * s)), Image.LANCZOS)
    card.paste(im, ((2560 - im.width) // 2, 260 + (box_h - im.height) // 2))
    credit = "Image: PrecisionRifleBlog.com"
    w = d.textlength(credit, font=REG)
    d.text((2560 - 150 - w, 1440 - 90), credit, font=REG, fill=INK2)
    card.save(out); print(out, im.size)
