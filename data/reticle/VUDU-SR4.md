# VUDU-SR4 — EOTech Vudu SR-4 (FFP, MOA)

A speed ring over a deep MOA duplex ladder — forty minutes of holdover. **First focal plane.** The
best-dimensioned drawing of the EOTech set: a true engineering drawing whose callouts are in the PDF's text
layer, so nothing here was read off a picture.

## The pattern

- **Speed ring** — **R1.275 mrad** outer, **R1.075 mrad** inner (2.55 mrad OD, **0.2 mrad** thick),
  illuminated red.
- **Centre dot** — **0.25 mrad**, which is **0.86 MOA**. EOTech's prose calls it "a precision 1 MOA center
  aiming dot"; that is this value rounded, not a second measurement.
- **Horizontal line** from **±5 MOA** out to the post shoulder, the **same thickness as the hashes**
  (0.04 mrad). It stops clear of the donut — nothing touches the ring.
- **Horizontal graduation** every **1 MOA** from 5 to ±20 MOA — **0.5 mrad** long on the numbered 4 MOA marks,
  **0.39 mrad** between. Numerals **8, 12, 16, 20**.
- **Heavy posts** from **9 mrad (30.9 MOA)** to **11.5 mrad (39.5 MOA)** each side.
- **Vertical ladder** — marks every **1 MOA** from **4 to 40 MOA**. The **spine** runs from the **5 MOA mark**
  down to 40, at the same fine width as everything else: it starts at that mark, not at the donut's edge, so
  there is a 0.62 MOA gap between the ring and the top of the spine.
  **Large marks 0.555 mrad wide** on every fourth (the numbered ones); **small marks 0.255 mrad wide**
  between, three to a gap:

| | Depth | Width |
|---|---|---|
| large | 4 MOA | 0.555 mrad (1.91 MOA) |
| small | 5, 6, 7 MOA | 0.255 mrad (0.88 MOA) |
| large | 8 MOA | 0.555 mrad |
| small | 9, 10, 11 MOA | 0.255 mrad |
| large | 12 MOA | 0.555 mrad |

…continuing to 40. Ten numbered holds, 27 intermediate marks.

### The "4" numeral sits further out than the rest

Numerals stand **0.27 + 0.255 = 0.525 mrad (1.80 MOA)** from the spine — except the **4**, which is pushed out
to **4 MOA**. That is not a drafting quirk: at 4 MOA depth the speed ring still covers |x| ≤ 1.79 MOA, so a
numeral at the normal offset would be buried in the red. EOTech move that one label clear of the ring and
leave the rest tight against the ladder.

It also settles what happens to the **4 MOA mark itself**: it stays where it is, centred on the spine and
largely hidden behind the ring, with only its numeral relocated. Do not "fix" this by deleting the mark or
by lining the 4 up with the others.

**The width callouts are in milliradians, the spacings in MOA.** `0.555 mrad` and `0.255 mrad` are how *wide*
each mark is drawn; the `4 MOA`, `1 MOA`, `8 MOA` and `5 MOA` figures are vertical *spacings* between marks —
4 MOA between numbered ones, 1 MOA between the small ones, with 8 and 5 MOA spanning larger intervals. Using
a spacing value as a width was the mistake here: it made the numbered marks four times too wide and the
ladder read as irregular.

## The marks

Numerals **4, 8, 12 … 40 MOA** — ten numbered holds, each carrying a `<bdc>` anchor, with three unnumbered
1 MOA marks between consecutive numerals.

Geometric — the ladder is graduated in minutes, not tied to a load, so the application supplies the ranges.

## Watch the units

**This drawing mixes mrad and MOA within one figure.** The dot, ring, line width and hash *lengths* are
dimensioned in milliradians; the numerals, the ladder and the bar *widths* are in MOA. Read every callout's
suffix rather than assuming from the reticle's name — that is how the centre dot came to be described two
different ways.

Every line on this reticle — the horizontal, the ladder spine and the hash strokes — is drawn at the **same
fine width**; there is no thick/thin distinction except the field-stop posts.

Every callout now has a role: **0.555 / 0.255 mrad** the ladder mark widths, **0.5 / 0.39 mrad** the horizontal
hash lengths, **4 / 1 / 8 / 5 MOA** the vertical spacings, **0.27 mrad** the gap from mark to numeral,
**0.37 mrad** the numeral height, **0.04 mrad** the line width shared by every fine element.

## Which numbers came from where

`Vudu_Reticle_Manual_SR4-5-LE_RevA.pdf` p7, cross-checked against the reticle-index image. The drawing gives
dimensions as inches at the reticle plane with the angular value in brackets (`0.0545(4MOA)`), and they are
extractable as text — no measurement involved.

## Calibration

| | |
|---|---|
| Cartridge / load | **not published** |
| Zero | not published |
| Focal plane | first (FFP) |
| Library entry | — |
