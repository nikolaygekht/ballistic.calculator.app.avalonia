# Shipped reticles

The `.reticle` files here are sight pictures for the **Reticle** tab (`Load…`) and for the **Reticle
Editor**. They are XML, so you can read one in any text editor — but the geometry is all they hold. Where a
reticle's marks belong to a particular load, **that is recorded in the reticle's `.md` file**, not in the
`.reticle` itself.

Everything here is a *-style* rendering built from published subtension data — none of it is a
manufacturer file, and none of it is exact.

**Each reticle has a companion `.md` beside it** — the pattern element by element, the full
calibration, a mark-by-mark table, the ranging features and how the load was identified. The tables
below are the index; the detail is one click away.

---

## How BDC marks work here

A reticle can carry **bullet-drop-compensator points** (`<bdc>` elements). They are positions only. The
application does not read a range off the reticle — it **labels each mark with the range at which
*your* trajectory crosses it**, from the load, zero and conditions in the Shot Parameters dialog. That
is what the *Far BDC* / *Near BDC* overlays draw.

Two consequences worth understanding before trusting any mark:

- **A mark is "400 m" only for the load the reticle was etched for.** Put a different load behind the
  same glass and the same mark is a different range — which the application will tell you, because it
  labels from your trajectory rather than from the etching.
- **A reticle with no BDC points is not broken.** It just has no marks nominated as hold-over points;
  the grid is still there to measure with.

That gives the two kinds of reticle below: **load-calibrated** ladders etched at the drops of one
specific load, and **geometric** patterns whose marks are places on a ruler.

---

## Load-calibrated

A ladder tied to one load. The columns are the inputs to reproduce it: enter them in the Shot Parameters
dialog and the labels the application draws should land on the marks' stated ranges.

Most were **computed here** from the stated load in an **ICAO sea-level** atmosphere — those name a
sight height. Where a manufacturer **publishes its own drop table**, that table is used instead and the
atmosphere and sight height behind it are whatever the manufacturer used; those rows read "not
published". Each reticle's own `.md` says which it is.

| Reticle | Optic | Field of view | Load | Muzzle velocity | Sight height | Zero | Marks | Library entry |
|---|---|---|---|---|---|---|---|---|
| [ACOG-TA31](ACOG-TA31.md) | Trijicon TA31 (ACOG 4×32), 5.56 chevron | 16 × 40 MOA | M855 62 gr, 0.151 G7, 1:7 RH | 3050 ft/s (20 in) | 2.4 in | 100 m | 300–800 m, 6 | `.223/XM855 Ammo (20in)` — exactly this load |
| [ACOG-TA31-762mm](ACOG-TA31-762mm.md) | Trijicon ACOG 4×32, .308 amber crosshair (TA01NSN-308) | 40 × 46 MOA | .308/7.62 — **Trijicon publish no load** | not published | not published | **100 m** — printed on the sheet | 6 marks; **what the numerals count is not stated** | none shipped — Trijicon publish no load |
| [ACOG-TA44-556mm](ACOG-TA44-556mm.md) | Trijicon ACOG 1.5×16S, RTR .223 | 44 × 46 MOA | .223/5.56 — Trijicon publish no load; verified against XM855 | 3050 ft/s as XM855 | not published | **100 m** (not published; this is what matches) | 400–700 **m**, 4 | `.223/XM855 Ammo (20in)`, unchanged — labels 384/502/618/728 m |
| [ACOG-TA44-9mm](ACOG-TA44-9mm.md) | Trijicon ACOG 1.5×16S, RTR 9 mm PCC | 44 × 58 MOA | 9 mm carbine — **Trijicon publish no load** | not published | not published | centre dot; distance **not published and undetermined** (50 m and 100 yd both fit, depending on load) | 150–300 **yd**, 4 | none shipped for 9 mm carbine |
| [ACOG-TA648](ACOG-TA648.md) | Trijicon TA648, .50 BMG | 28 × 42 mrad | M2 AP 709 gr, 1:15 RH | 2810 ft/s | 3.5 in (M2 Browning) | **300 m** | 400–2000 m, 9 | `50BMG M2` — 710 gr at 2830 ft/s |
| [HURON-BDC-HUNTER-4](HURON-BDC-HUNTER-4.md) | Trijicon Huron 1-4×24 BDC Hunter Holds (**SFP**, at 4×) | 36 × 40 MOA | **not published** — a hunting BDC, no load stated | — | — | crosshair; distance not published | 3 holds: 2.24 / 5 / 8.32 MOA | — |
| [HURON-BDC-HUNTER-6](HURON-BDC-HUNTER-6.md) | Trijicon Huron 1-6×24 BDC Hunter Holds (**SFP**, at 6×) | 36 × 40 MOA | **not published** — a hunting BDC, no load stated | — | — | crosshair; distance not published | 3 holds: 2.22 / 5 / 8.29 MOA | — |
| [HURON-BDC-HUNTER-9](HURON-BDC-HUNTER-9.md) | Trijicon Huron 3-9×40 BDC Hunter Holds (**SFP**, at 9×) | 36 × 40 MOA | **not published** — a hunting BDC, no load stated | — | — | crosshair; distance not published | 3 holds: 2.24 / 5 / 8.32 MOA | — |
| [LEUP-CMR-W556](LEUP-CMR-W556.md) | Leupold CMR-W 5.56 Illum. FFP (to the 20 mil mark) | 42 × 22 mrad | .223/5.56 62 gr — Leupold's published ladder | 3050 ft/s as XM855 (Leupold quote 2970) | not published | **100 m** — Leupold state 50 m, but 100 m is what matches | 300–900 m, 7 | `.223/XM855 Ammo (20in)`, unchanged |
| [LEUP-CMR-W762](LEUP-CMR-W762.md) | Leupold CMR-W 7.62 Illum. FFP (to the 20 mil mark) | 42 × 28 mrad | 7.62/.308 175 gr — Leupold's published ladder | 2575 ft/s | not published | **100 m** — Leupold state 50 m, but 100 m is what matches | 300–1200 m, 10 | none shipped — **175 gr at 2575 ft/s** |
| [LEUP-CMR2](LEUP-CMR2.md) | Leupold Illum. CM-R² (**SFP**, at 6×) | 22 × 18 mrad | **not published** — Leupold dimensions the geometry only | — | — | — | 300–900, 7 | — |
| [PSO-1](PSO-1.md) | PSO-1 (SVD), in Soviet thousandths | 79 × 79 ths | 57-N-323S LPS ball 9.6 g, 0.400 G1 | 830 m/s | 65 mm | **drum at 1000 m** | 1100–1300 m, 3 | `7.62x54/57N323S` |
| [SPECTER-5.56](SPECTER-5.56.md) | Elcan Specter 1-4×, 5.56 | 140 × 190 MOA | M855 62 gr, 0.151 G7, 1:7 RH | 2800 ft/s (16 in) | 2.9 in | 100 m | 300–1000 m, 8 | `.223/XM855 Ammo (16in)` — exactly this load |
| [SPECTER-7.62](SPECTER-7.62.md) | Elcan Specter DR 1-4×, 7.62 (at 4×) | 140 × 165 MOA | M80 ball 9.5 g, 0.397 G1, 1:12 RH | 830 m/s (22 in) | 2.9 in | 100 m | 300–1000 m, 8 | `.308 Win/7.62x51 BPN` — **set 830 m/s** |
| [VCOG-16-556-55gr](VCOG-16-556-55gr.md) | Trijicon VCOG 1-6×24, 5.56 segmented BDC (at 6×) | 96 × 88 MOA | .223/5.56 55 gr — **also the published sheet for the 77 gr part** | not published | not published | **100 m** (centre crosshair) | 300–800 m, 6 | `.223/M193 Ammo` — the 55 gr designation load |
| [VCOG-16-762-175gr](VCOG-16-762-175gr.md) | Trijicon VCOG 1-6×24, 7.62 segmented BDC (at 6×) | 96 × 100 MOA | 7.62/.308 175 gr | not published | not published | **100 m** (centre crosshair) | 300–1000 m, 8 | none shipped — **175 gr** (no velocity published) |
| [VCOG16-300BLK](VCOG16-300BLK.md) | Trijicon V-COG 1-6×24, 300 BLK (at 6×) | 172 × 172 MOA | 115 gr, 0.290 G1 **and** 208 gr A-MAX, 0.648 G1, 1:8 RH | 2330 ft/s / 1010 ft/s | 2.9 in | 100 m, supersonic load | 200–600 m, 5 (+3 subsonic diamonds, unlabelled) | `.300 AAC/.300 AAC 115gr` and `.300 AAC 208gr Hornady` |

| [VUDU-BD1](VUDU-BD1.md) | EOTech Vudu X BD1 (**SFP**) | 30 × 30 MOA | **not published** — a ballistic-drop reticle with no load stated | — | — | not published | 4 circles: 2 / 4 / 6 / 8 MOA | — |
| [VUDU-HC3](VUDU-HC3.md) | EOTech Vudu HC3 (**SFP**, at 8×) | 42 × 32 MOA | **not published** | — | — | not published | 4 bars: 2 / 5 / 8.5 / 12.5 MOA | — |
| [VUDU-SR2](VUDU-SR2.md) | EOTech Vudu SR-2, 7.62 (FFP) | 14 × 12 mrad | **7.62×51 M118LR, 175 gr, 0.495 BC** — printed on the drawing | 2550 ft/s | 1.5 in | **not published** | 400–600 **yd**, 3 | none shipped — **175 gr at 2550 ft/s** |
| [VUDU-SR3](VUDU-SR3.md) | EOTech Vudu SR-3, 5.56 (FFP) | 14 × 12 mrad | **.223/5.56 BTHP, 75 gr, 0.395 BC** — printed on the drawing | 2900 ft/s | 1.5 in | **not published** | 400–600 **yd**, 3 | none shipped — **75 gr, 0.395 BC, 2900 ft/s** |

Several rows need reading before use, and their docs say why:

- **PSO-1** means nothing until the elevation drum is dialled to 1000 m.
- Both **Specter** files are cut for a barrel the library entry does not have, so the muzzle velocity must
  be changed by hand.
- Both **CMR-W** files line up at a **100 m** zero although Leupold publish 50 m — the same offset on two
  different cartridges, so it is systematic rather than a quirk of one load.
- **ACOG-TA44-9mm**'s zero is genuinely undetermined: Trijicon publish neither the load nor the zero, and
  50 m and 100 yd both fit depending on which 9 mm load you assume.
- **ACOG-TA31-762mm** is graduated in hundreds of *something* Trijicon never states; load it and let the
  labels tell you.
- **LEUP-CMR2** and the three **Huron** files are **second focal plane**: their subtensions are true only at
  the magnification in the *Optic* column.

## Geometric

Marks on whole grid units. Load-independent by design — they are places on a ruler, and the
application supplies the ranges.

| Reticle | Pattern | Field of view | Marks |
|---|---|---|---|
| [GERMAN4](GERMAN4.md) | German #4 hunting picture | 60 × 60 MOA | none — a hunting picture |
| [H58](H58.md) | H58-style grid with a christmas tree | 21 × 21 mrad | every 2 mrad to −10 (5) |
| [LEUP-CCH](LEUP-CCH.md) | Leupold FFP CCH (Mark 5HD) — mrad grid with a 20-row christmas tree | 15 × 25 mrad | every 1 mrad to −20 (20) |
| [LEUP-CMR-MIL](LEUP-CMR-MIL.md) | Leupold CMR-MIL Illum. FFP (Mark 4HD 2-10×) — mrad tree, 5 rows | 28 × 24 mrad | 2 / 4 / 6 / 8 / 10 mrad (5) |
| [M-16 Iron 3 Inch Eye Relief](M-16%20Iron%203%20Inch%20Eye%20Relief.md) | M16 aperture and post | 350 × 350 MOA | none — iron sights, not a scope |
| [MILDOT](MILDOT.md) | Mil-Dot, in true milliradians | 12 × 12 mrad | whole mrad, +2 to −4 (6) |
| [MOA-GRID](MOA-GRID.md) | MOA crosshair, 1 MOA hashes to ±14 — **MILDOT's field of view** | 12 × 12 mrad (41.25 MOA) | every 2 MOA, +4 to −14 (9) |
| [VUDU-LE5](VUDU-LE5.md) | EOTech Vudu LE-5 (FFP) — mrad christmas tree, cross-hair in the speed ring | 26 × 20 mrad | every 1 mrad to −12 (12) |
| [VUDU-MD1](VUDU-MD1.md) | EOTech Vudu MD1 (FFP) — plain mrad crosshair, 0.5 mrad graduation | 14 × 14 mrad | whole mrad, 1–5 (5) |
| [VUDU-MD2](VUDU-MD2.md) | EOTech Vudu MD2 (FFP) — plain MOA crosshair, 1 MOA graduation to ±30 | 80 × 80 MOA | 10 / 20 / 30 MOA (3) |
| [VUDU-SR1](VUDU-SR1.md) | EOTech Vudu SR-1 (FFP) — mrad crosshair; **speed ring out of scope** | 12 × 12 mrad | whole mrad, 1–5 (5) |
| [VUDU-SR4](VUDU-SR4.md) | EOTech Vudu SR-4 (FFP) — MOA duplex ladder to 40, with speed ring | 84 × 58 MOA | every 4 MOA to −40 (10) |
| [VUDU-SR5](VUDU-SR5.md) | EOTech Vudu SR-5 (FFP) — mrad christmas tree, dot in the speed ring | 26 × 20 mrad | every 1 mrad to −12 (12) |

---

## Adding your own

Drop any `.reticle` file in this folder and it appears in `Load…`. Two ways to make one:

- **The Reticle Editor** application — every element typed as an angular position and size.
- **The `reticle-designer` skill** for Claude Code or Codex, in the
  [BallisticCalculator repository](https://github.com/gehtsoft-usa/BallisticCalculator1/tree/main/SKILL/SKILLS/reticle-designer)
  — describe the reticle in words and it writes the file, an SVG preview and a subtension table.

Four conventions worth keeping:

1. **Write a `<NAME>.md` next to it** and add a row to the table above. This is where a reticle's
   provenance lives: what the pattern is, the calibration as a table (projectile, BC and drag model,
   muzzle velocity, twist, sight height, atmosphere, zero), the marks with their ranges, the ranging
   features, and how the load was identified if it was fitted rather than published. Copy the shape of an
   existing one.
2. **Don't put that in an XML comment in the `.reticle` file.** Comments do not survive a round-trip
   through the Reticle Editor, so provenance kept there quietly disappears the first time the file is
   re-saved. The `.md` is the record. (A `description` and zero fields are planned for the format itself,
   which will make this moot.)
3. **Keep both tables sorted alphabetically by file name** — always. They are indexes, and an index is
   only usable if you can find a name in it without reading every row. Insert the new row in place
   rather than appending it.
4. **Say what the ranging features measure**, if the reticle has any — a stadia pair is useless without
   the target size it assumes.

The file format ignores attributes it does not understand, so a misspelling is a silently missing
element rather than an error. Check the preview after editing by hand.
