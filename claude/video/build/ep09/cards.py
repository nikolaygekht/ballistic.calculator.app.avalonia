"""Episode 9 card (1920x1080, headless Edge): the rifleman's rule.
Usage: python cards.py  ->  card-rifleman.png"""
import math, pathlib, subprocess

HERE = pathlib.Path(__file__).resolve().parent
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

CSS = """
* { margin: 0; padding: 0; box-sizing: border-box; }
html, body { width: 1920px; height: 1080px; overflow: hidden; background: #1b2330; color: #fff;
             font-family: "Segoe UI Variable Display", "Segoe UI", sans-serif; }
.wrap { position: absolute; inset: 0; padding: 80px 110px; display: flex; flex-direction: column; }
.kicker { font-size: 32px; font-weight: 600; letter-spacing: .2em; text-transform: uppercase; color: #ffb4a8; }
h1 { font-size: 76px; font-weight: 700; line-height: 1.08; margin-top: 14px; }
.row { margin-top: 30px; display: flex; gap: 50px; align-items: flex-start; }
svg { flex: 0 0 1040px; }
.rule { flex: 1 1 auto; }
.rule { font-size: 40px; line-height: 1.35; margin-top: 30px; }
.rule .big { display: block; margin-top: 26px; font-size: 52px; font-weight: 700; }
.rule .why { display: block; margin-top: 30px; font-size: 32px; opacity: .78; }
svg text { font-family: "Segoe UI", sans-serif; fill: #fff; }
"""

# The triangle: you at O, the target 900 px up a 30 degree line of sight.
A = math.radians(30)
O = (90, 600)
L = 860
T = (O[0] + L * math.cos(A), O[1] - L * math.sin(A))
H = (T[0], O[1])
# Gravity at a point on the line of sight, and its part across the line of sight (x cos 30).
P = (O[0] + 0.55 * L * math.cos(A), O[1] - 0.55 * L * math.sin(A))
G = 170
gc = G * math.cos(A)
C = (P[0] + gc * math.sin(A), P[1] + gc * math.cos(A))

SVG = f"""
<svg width="1040" height="680" viewBox="0 0 1040 680">
  <defs>
    <marker id="ar" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto">
      <path d="M0,0 L10,5 L0,10 z" fill="#ffb4a8"/></marker>
    <marker id="ag" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto">
      <path d="M0,0 L10,5 L0,10 z" fill="#9fb3c8"/></marker>
  </defs>
  <!-- horizontal distance and height -->
  <line x1="{O[0]}" y1="{O[1]}" x2="{H[0]}" y2="{H[1]}" stroke="#ffb4a8" stroke-width="6"/>
  <line x1="{H[0]}" y1="{H[1]}" x2="{T[0]}" y2="{T[1]}" stroke="#5b6b82" stroke-width="3" stroke-dasharray="10 10"/>
  <path d="M{H[0]-26},{H[1]} L{H[0]-26},{H[1]-26} L{H[0]},{H[1]-26}" fill="none" stroke="#5b6b82" stroke-width="3"/>
  <!-- line of sight -->
  <line x1="{O[0]}" y1="{O[1]}" x2="{T[0]}" y2="{T[1]}" stroke="#fff" stroke-width="5"/>
  <circle cx="{O[0]}" cy="{O[1]}" r="11" fill="#fff"/>
  <circle cx="{T[0]}" cy="{T[1]}" r="20" fill="none" stroke="#fff" stroke-width="5"/>
  <circle cx="{T[0]}" cy="{T[1]}" r="6" fill="#fff"/>
  <!-- the angle -->
  <path d="M{O[0]+150},{O[1]} A150,150 0 0 0 {O[0]+150*math.cos(A):.1f},{O[1]-150*math.sin(A):.1f}"
        fill="none" stroke="#fff" stroke-width="3"/>
  <text x="{O[0]+165}" y="{O[1]-26}" font-size="36">30°</text>
  <!-- gravity, and the part of it across the line of sight -->
  <line x1="{P[0]:.1f}" y1="{P[1]:.1f}" x2="{P[0]:.1f}" y2="{P[1]+G:.1f}" stroke="#9fb3c8" stroke-width="4" marker-end="url(#ag)"/>
  <line x1="{P[0]:.1f}" y1="{P[1]:.1f}" x2="{C[0]:.1f}" y2="{C[1]:.1f}" stroke="#ffb4a8" stroke-width="4" marker-end="url(#ar)"/>
  <text x="{P[0]-150:.1f}" y="{P[1]+G+10:.1f}" font-size="30" style="fill:#9fb3c8">gravity</text>
  <text x="{C[0]+16:.1f}" y="{C[1]+12:.1f}" font-size="30" style="fill:#ffb4a8">× cos 30°</text>
  <!-- labels -->
  <text x="{(O[0]+T[0])/2 - 150:.1f}" y="{(O[1]+T[1])/2 - 40:.1f}" font-size="38" font-weight="700"
        transform="rotate(-30 {(O[0]+T[0])/2 - 150:.1f} {(O[1]+T[1])/2 - 40:.1f})">line of sight 1000 yd</text>
  <text x="{(O[0]+H[0])/2 - 150:.1f}" y="{O[1]+56}" font-size="38" font-weight="700" style="fill:#ffb4a8">horizontal 866 yd</text>
  <text x="{H[0]+22:.1f}" y="{(H[1]+T[1])/2:.1f}" font-size="32" style="fill:#9fb3c8">500 yd up</text>
</svg>"""

RULE = f"""<!doctype html><html><head><meta charset='utf-8'><style>{CSS}</style></head><body><div class='wrap'>
<div class='kicker'>A rule of thumb</div>
<h1>The rifleman's rule</h1>
<div class='row'>
  {SVG}
  <div class='rule'>Hold as if the target were on the flat, at the
    <b style='color:#ffb4a8'>horizontal</b> distance.
    <span class='big'>1000 yd × cos 30°<br>= 866 yd</span>
    <span class='why'>Uphill or downhill, gravity pulls straight down, so only part of it bends the bullet
    away from the line of sight.</span>
  </div>
</div>
</div></body></html>"""

for name, markup in (("card-rifleman", RULE),):
    src, png = HERE / f"{name}.html", HERE / f"{name}.png"
    src.write_text(markup, encoding="utf-8")
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    "--window-size=1920,1080", f"--screenshot={png}", src.as_uri()], check=True, capture_output=True, timeout=60)
    src.unlink()
    print(png)
