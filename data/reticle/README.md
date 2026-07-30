# Shipped reticles

The `.reticle` files here are sight pictures for the **Reticle** tab (`Load…`) and for the **Reticle
Editor**. They are XML, so you can read one in any text editor; where a reticle's drop marks were computed
for a particular load, **the load is recorded in an XML comment at the top of the file**.

Everything here is a *-style* rendering built from published subtension data — none of it is a manufacturer
file, and none of it is exact.

---

## How BDC marks work here

A reticle can carry **bullet-drop-compensator points** (`<bdc>` elements). They are positions only. The
application does not read a range off the reticle — it **labels each mark with the range at which *your*
trajectory crosses it**, from the load, zero and conditions in the Shot Parameters dialog. That is what the
*Far BDC* / *Near BDC* overlays draw.

Two consequences worth understanding before trusting any mark:

- **A mark is "400 m" only for the load the reticle was etched for.** Put a different load behind the same
  glass and the same mark is a different range — which the application will tell you, because it labels
  from your trajectory rather than from the etching.
- **A reticle with no BDC points is not broken.** It just has no marks nominated as hold-over points; the
  grid is still there to measure with.

That gives two kinds of reticle here:

| Kind | What the marks are | Example |
|---|---|---|
| **Geometric** | marks on whole grid units — 1 mrad, 2 MOA and so on. Load-independent by design: they are places on a ruler | `MILDOT`, `H58`, `MOA-GRID` |
| **Load-calibrated** | a ladder etched at the drops of one specific load, at stated ranges. Reproduced here by computing that load's trajectory | the ACOG, V-COG and Specter files |

For a calibrated ladder, the comment in the file names the projectile, ballistic coefficient and drag
model, muzzle velocity (and the barrel it implies), twist, sight height, atmosphere and zero. Those are the
inputs to reproduce it: enter them in the dialog and the labels the application draws should land on the
etched ranges.

Most of those loads are in the shipped ammunition library, so `Load` on the Ammunition tab saves the typing:

| Reticle | Library entry | |
|---|---|---|
| `ACOG-TA31` | `.223/XM855 Ammo` | exactly the ladder's load (BC 0.151 G7, 3050 ft/s, 20 in) |
| `SPECTER-5.56` | `.223/XM855 Ammo` | same bullet — change the muzzle velocity to 2800 ft/s for the 16 in barrel |
| `ACOG-TA648` | `50BMG M2` | 710 gr at 2830 ft/s against the ladder's 709 at 2810 |
| `VCOG16-300BLK` | `.300 AAC/.300 AAC 115gr` and `.300 AAC 208gr Hornady` | 2295 and 1020 ft/s against the ladder's 2330 and 1010 — close enough that the marks land where they should |
| `PSO-1` | `7.62x54/57N323S` | the LPS ball the chevrons were computed from |

---

## The files

| File | Reticle | Field of view | BDC marks | Calibration |
|---|---|---|---|---|
| `MILDOT.reticle` | Mil-Dot | 12 × 12 mrad | 6 | geometric — whole milliradians, ±1…−4 |
| `H58.reticle` | H58-style grid (high magnification) | 21 × 21 mrad | 5 | geometric — every 2 mrad to −10 |
| `MOA-GRID.reticle` | MOA grid | 32 × 32 MOA | 6 | geometric — every 2 MOA, +4 to −8 |
| `GERMAN4.reticle` | German #4 | 60 × 60 MOA | — | no marks; a hunting picture |
| `M-16 Iron 3 Inch Eye Relief.reticle` | M16 aperture and post | 350 × 350 MOA | — | no marks; an iron-sight picture, not a scope |
| `PSO-1.reticle` | PSO-1 (SVD), in Soviet thousandths | 79 × 79 ths | 3 | LPS ball, 1100–1300 m **with the drum at 1000m (see below)** |
| `ACOG-TA31.reticle` | Trijicon TA31 (ACOG 4×32), 5.56 chevron | 16 × 40 MOA | 6 | M855, 300–800 m |
| `ACOG-TA648.reticle` | Trijicon TA648, .50 BMG | 28 × 42 mrad | 9 | M2 AP, 400–2000 m |
| `VCOG16-300BLK.reticle` | Trijicon V-COG 1-6×24, 300 BLK | 172 × 172 MOA (at 6×) | 5 | two loads, supersonic + subsonic |
| `SPECTER-5.56.reticle` | Elcan Specter 1-4×, 5.56 | 140 × 190 MOA | 8 | M855, 300–1000 m |

---

## The calibrated ladders, in full

### Trijicon TA31 — ACOG 4×32, 5.56 chevron

- **Chevron tip** is the 100 m zero. **Chevron base** is the 300 m hold, and is 19 in wide — silhouette
  shoulders at 300 m, the ranging trick the pattern is designed around.
- The lines below are **400…800 m**, each drawn 19 in wide at its own range.
- Computed for **M855 62 gr, BC 0.151 G7, 3050 ft/s** (a 20 in barrel), 1:7 RH twist, **2.4 in** sight
  height, ICAO sea level, **100 m zero**.

### Trijicon TA648 — .50 BMG

- Nine marks, **400…2000 m in 200 m steps** (the etched numerals are hundreds of metres).
- Computed for **M2 AP 709 gr at 2810 ft/s**, 1:15 RH (the M2 barrel), **300 m zero**.

### Trijicon V-COG 1-6×24 — 300 BLK

One of the busiest reticles, because 300 BLK is two cartridges in one chamber. Framed as Trijicon's reticle
sheet draws it: the **6× field of view, 25 mils (84.5 MOA) in radius**.

- **Centre crosshair** — 19 in at 100 m, and the 100 m supersonic zero. Its **bottom end is the 200 m**
  supersonic hold.
- **Stadia below** — 300, 400, 500 and 600 m, each 19 in wide at its own range.
- **Three diamonds** — the *subsonic* holds: 25/50 m, 100 m, 150 m, centred on the hold. The 25/50 m
  diamond sits on the 50 m hold; 25 m is 0.54 MOA lower.
- **Ring** — 25 MOA gaps on both axes; 66.4 MOA across the outer corners, which is 19 in at 25 m.
- Computed for **115 gr, BC 0.290 G1 at 2330 ft/s** (supersonic) and **208 gr A-MAX, BC 0.648 G1 at
  1010 ft/s** (subsonic), **2.9 in** sight height (the V-COG's tall integral mount), 1:8 RH, ICAO sea
  level, zeroed with the **supersonic** load at 100 m.
- Both loads were identified from the sheet's own geometry, fitting to **0.17 and 0.18 MOA RMS** across
  every mark it draws — so the ladder matches the published pattern, not merely a plausible 300 BLK.
- The horizontal stadia are marked every 5 mils (16.9 MOA). The sheet's "16.9 MOA × 12" says the etched
  pattern carries twelve of them, one mark further out than this field of view shows.

### Elcan Specter 1-4× — 5.56

- Eight marks, **300…1000 m** (numerals are hundreds of metres).
- Computed for **M855 62 gr, BC 0.151 G7, 2800 ft/s** (a 16 in barrel), 1:7 RH twist, **2.9 in** sight
  height, ICAO sea level, **100 m zero**.

### PSO-1 — SVD, and the one reticle here that needs the turret dialled

The three chevrons below the main one are **not** hold-overs from the zero. They only mean anything with the
**elevation drum set to 10** — dialled for 1000 m. With that on the turret:

- the **main chevron** is the 1000 m hold;
- the three below are **1100, 1200 and 1300 m**.

**Dialling to 1000 m is 30 vertical clicks** — the drum and the reticle are both graduated in Soviet
thousandths (1 ths = 1/6000 of a circle = 1.0472 mrad = 3.6 MOA), and one click is 0.5 ths, so 30 clicks is
15 ths up from the 100 m setting.

Calibrated for **57-N-323S LPS ball, 9.6 g, BC 0.400 G1, 830 m/s**, 65 mm sight height, ICAO sea level.
Recomputing that load puts the three holds below a 1000 m hold at **3.166 / 6.719 / 10.654 ths** against the
etched **3.162 / 6.714 / 10.647** — agreement to five thousandths of a thousandth, which is what identifies
the load and the drum setting together. Read at the drum's other settings the spacing does not fit.

**Two ways to see that in the app**, since the *Far BDC* overlay labels marks from your *current* solution:

1. **Set the zero to 1000 m** — simplest, and the labels land on 1100 / 1200 / 1300.
2. **Keep the 100 m zero and dial it**: put `0.5 ths` in the sight's vertical click on the Rifle tab, then
   enter **30** V-Clicks on the Parameters tab.

With an ordinary 100 m zero and *nothing* dialled, the overlay will label these three marks with whatever
ranges they happen to correspond to from the crosshair — correct arithmetic, but not what the etching means.

One honest discrepancy: the real drum was cut to the issue firing tables, and this model puts the 1000 m
come-up at **14.0 ths (28 clicks)** rather than 15. It moves where the main chevron lands by about 1 ths; it
does not touch the spacing of the three chevrons below it, which is what they are read by.

The PSO-1's own **stadiametric rangefinder** is at lower left and has nothing to do with drop: put the
target's feet on the horizontal base line and its head on the curve, and read the range off the numbers —
hundreds of metres, 2 nearest the centre out to 10 at the far left. It assumes the true **1.7 m** target
height.

---

## Adding your own

Drop any `.reticle` file in this folder and it appears in `Load…`. Two ways to make one:

- **The Reticle Editor** application — every element typed as an angular position and size.
- **The `reticle-designer` skill** for Claude Code or Codex, in the
  [BallisticCalculator repository](https://github.com/gehtsoft-usa/BallisticCalculator1/tree/main/SKILL/SKILLS/reticle-designer)
  — describe the reticle in words and it writes the file, an SVG preview and a subtension table.

Two conventions worth keeping if you add a calibrated reticle:

1. **Record the calibration in a comment at the top of the file** — projectile, BC and drag model, muzzle
   velocity, twist, sight height, atmosphere, zero, and which ranges the marks are. Without it the marks are
   just positions, and nobody can check them later.
2. **Say what the ranging features measure**, if the reticle has any — a stadia pair is useless without the
   target size it assumes.

The file format ignores attributes it does not understand, so a misspelling is a silently missing element
rather than an error. Check the preview after editing by hand.
