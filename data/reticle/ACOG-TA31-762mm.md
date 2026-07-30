# ACOG-TA31-762mm — Trijicon ACOG 4×32, .308 Crosshair (TA01NSN-308)

The tritium-only 4×32 ACOG's .308 pattern: an amber crosshair with heavy duplex posts, then a ladder of
four crossbars that turns into a thick post carrying two oval holds. Also fitted to the **TA01B**, which
illuminates differently. Fixed 4×.

Unlike the other Trijicon sheets in this folder, this one **states its zero**: "ZEROED AT 100m".

## The pattern

- **Amber crosshair** — **15.7 MOA** wide, **0.4 MOA** lines. Its intersection is the **100 m zero**.
- **Duplex posts** — heavy black bars **2.6 MOA** thick, starting **1.1 MOA** outboard of the crosshair
  ends and running to the field stop (off-frame here).
- **Four crossbars**, the upper two amber, the lower two black.
- **Lower post** — one continuous **2.6 MOA** wide bar, with two **oval holds** punched through it.

## The marks

| Drop below the crosshair | Mark | Size | Numeral |
|---|---|---|---|
| 3.6 MOA | crossbar | 7.9 MOA wide | — |
| 6.6 MOA | crossbar | 5.2 MOA wide | — |
| 10.1 MOA | crossbar | 3.9 MOA wide | **4** |
| 14.0 MOA | crossbar | 3.1 MOA wide | — |
| 19.1 MOA | oval hold | 2.2 × 3.6 MOA | **6** |
| 25.5 MOA | oval hold | 2.0 × 3.1 MOA | **8** |

Every one of those numbers is printed on the sheet, positions and sizes both.

## What the numerals count is not stated

The sheet prints the geometry and the 100 m zero but never says what **4**, **6** and **8** are hundreds
*of*, nor which load the ladder was cut for. Reading them onto the marks as drawn gives 400 / 600 / 800
at the 10.1 / 19.1 / 25.5 marks, which makes the unnumbered marks 200 / 300 / 500 — and leaves **no 700
mark** between the two ovals, which is odd enough to be worth stating rather than smoothing over.

Checked against a 7.62 M80 ball at 830 m/s from a 100 m zero (the load in
[SPECTER-7.62](SPECTER-7.62.md)), neither unit reading is clean:

| Reading | Sheet vs that load |
|---|---|
| hundreds of **metres**, marks 300–800 | sheet is ~22% shallower, and inconsistently so |
| hundreds of **yards**, marks 300–800 | sheet is ~9% shallower, consistently |

The yard reading tracks better, but the sheet's own zero is metric, and a slightly faster or
higher-BC .308 than M80 would close either gap. **Load the reticle and let the application label it** —
that is the only reliable way to settle it, exactly as it settled the units for
[ACOG-TA44-556mm](ACOG-TA44-556mm.md). All six marks carry `<bdc>` anchors so every one gets a label.

## Calibration

| | |
|---|---|
| Cartridge | .308 / 7.62 — **bullet weight and velocity not published** |
| Zero | **100 m** — printed on the sheet |
| Magnification | fixed 4× |
| Sight height | not published |
| Library entry | none shipped for this ladder — Trijicon publish neither bullet weight nor velocity |

## Two reproduction notes

- **The ovals are holes punched in the post, not gaps in it.** The post is one continuous 2.6 MOA bar and
  each oval is a **white filled circle drawn after it**, so it paints over the black and reads as a hole —
  which is what the glass does. Elements are drawn in document order, so the circles must come after the
  post; swap them and the holes disappear. Their radius is the oval's *width* (1.1 and 1.0 MOA): the
  `.reticle` format has no ellipse, so the 3.6 and 3.1 MOA heights are not reproduced. The hold is the
  oval's centre either way, and that position is exact.
- **The amber elements** are what the tritium lights on this model. The sheet's own note warns that
  "different models illuminate in different ways or colors", so on a TA01B the same geometry is not amber.

## Nothing was measured

The sheet is stamped "**\*Reticles not drawn to scale**", and it demonstrably is not: measuring its drawn
marks gives 3.8 / 7.0 / 10.7 / 15.0 MOA where it prints 3.6 / 6.6 / 10.1 / 14.0 — about 5% out, growing
with depth. The drawing was used only to establish **which** printed dimension belongs to which mark.

## Wind holds

None.
