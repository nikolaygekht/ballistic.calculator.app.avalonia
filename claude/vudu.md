# EOTech Vudu reticles — dimension survey

Requested: **SR1, SR2, SR3, SR4, SR5, LE5, HC3, BD1, MD1, MD2** — ten reticles, for adding to
`data/reticle/`.

This file records, per reticle, **whether a dimensioned source exists and what it says**, before any
`.reticle` file is written. Verdict up front: **all ten are dimensioned and buildable.** Six came from
EOTech's own reticle manuals, one (BD1) from a scope manual, and **HC3, MD1 and MD2 from reticle-index
drawings supplied by Nikolay** — those three have no manual, and the index page is unreachable by our tooling.

The only feature anywhere in the ten that is *not* published is **SR-1's speed ring**, and that is
**explicitly out of scope**, so nothing is blocked and **nothing anywhere in this document is a guess** —
every value below is a printed callout from a drawing.

---

## How the sources were obtained

Worth recording, because it was the hard part and the next EOTech round will hit the same wall.

- **`eotechinc.com` cannot be fetched with `curl`.** Every path returns **HTTP 429** or a 404 carrying a
  JavaScript challenge body — a WAF. This includes their own manual URLs, e.g.
  `…/media/documents/operator_manuals/Vudu_Reticle_Manual_HC1_RevB.pdf`. `WebFetch` reaches their pages but
  **truncates** before the reticle content, so the reticle index page could not be read that way either.
- **Dealers mirror EOTech's manuals under the identical filenames**, and they do not block anything:
  - `https://farrwest.com/wp-content/uploads/2025/05/<EXACT_EOTECH_FILENAME>.pdf`
  - `https://www.opticsplanet.com/i/pdf/opplanet-eotech-<slug>-user-manual-pdf.pdf`
  - `https://media.chattanoogashooting.com/documents/product/<SKU>/EOTECH-<FILENAME>.pdf`
- **EOTech's Shopify CDN works for scope manuals**:
  `https://cdn.shopify.com/s/files/1/0698/3044/3191/files/<NAME>.pdf` — verified with
  `Vudu_1-6X24FFP_Manual.pdf` and `Vudu_1-10x28FFP_Manual.pdf`. Note the inconsistent casing between those
  two (`X24` vs `x28`), so names must be exact; guessing reticle-manual names on the CDN all returned 404.
- The web-search index was what actually surfaced the filenames. Searching for `Vudu_Reticle_Manual`
  restricted to `farrwest.com` enumerated the mirror.

**The reticle-index images are the same artwork as the manual pages.** Verified by comparing the LE-5 image
from the index (supplied by hand) against `Vudu_Reticle_Manual_SR4-5-LE_RevA.pdf` p9: identical callouts,
identical layout. So for the seven reticles that have a manual there is nothing extra to gain from the index
page — it is only worth chasing for a reticle with no manual, which is how HC3 was resolved.

### Files retrieved (in the session scratchpad, `vudu/`)

| File | Pages | Covers |
|---|---|---|
| `Vudu_Reticle_Manual_SR1_RevB.pdf` | 16 | SR-1 |
| `Vudu_Reticle_Manual_SR2-3_RevB.pdf` | 12 | SR-2, SR-3 |
| `Vudu_Reticle_Manual_SR4-5-LE_RevA.pdf` | 12 | SR-4, SR-5, LE-5 |
| `Vudu_1-8x24SFP_RevB.pdf` | 24 | the HC3 scope — **but not the HC3 reticle** |
| Vudu X 1-6x24 SFP user manual | 24 | DP1, **BD1** |
| `Vudu_Reticle_Manual_HC1_RevB.pdf`, `HC2_RevB.pdf` | 12 each | HC1, HC2 (not requested; retrieved while hunting HC3) |

---

## Coverage verdict

| Reticle | Plane | Unit | Dimensioned source | Load published | Ready to build |
|---|---|---|---|---|---|
| SR-1 | FFP | mrad | SR1 manual p7 | no | **yes** — crosshair only, ring out of scope |
| SR-2 | FFP | mrad (+MOA) | SR2-3 manual p9 | **yes** | **yes** |
| SR-3 | FFP | mrad (+MOA) | SR2-3 manual p9 | **yes** | **yes** |
| SR-4 | FFP | MOA | SR4-5-LE manual p7 | no | **yes** |
| SR-5 | FFP | mrad | SR4-5-LE manual p8 | no | **yes** |
| LE-5 | FFP | mrad | SR4-5-LE manual p9 | no | **yes** |
| BD1 | SFP | MOA | Vudu X 1-6x24 manual p7 | no | **yes** |
| HC3 | SFP | MOA | reticle-index drawing (supplied) | no | **yes** |
| MD1 | FFP | mrad | reticle-index drawing (supplied) | no | **yes** |
| MD2 | FFP | MOA | reticle-index drawing (supplied) | no | **yes** |

---

## SR-1 — FFP, milliradian

Source: `Vudu_Reticle_Manual_SR1_RevB.pdf` p7. The page states *"Subtensions measured in MRADs. Image shown
is for representation only"* — so the drawing is **dimensioned but not to scale**; printed values only.

| Feature | Value |
|---|---|
| Centre dot | **Dia 0.29 mrad (1 MOA)** |
| Hashes, all four arms | **1, 2, 3, 4, 5 mrad** |
| Hash length | **0.8 mrad** |

**The speed ring is out of scope** (Nikolay, 2026-07-30). The SR-1 is a *Speed Ring* reticle, but its manual
dimensions only the inner crosshair — p7 draws the crosshair alone and p8, the "Speed Ring" section, carries
no dimensions at all. Rather than borrow a diameter from a sibling (SR-2/SR-3 rings are 2.15 mrad OD;
SR-4/SR-5 are 2.55 mrad OD) and ship an assumption, the file reproduces **the crosshair only**.

That must be stated in `SR1.md` as a deliberate omission, the way the field-stop wedges are on `LEUP-CCH` —
otherwise it reads as an incomplete reproduction rather than a scoped one.

## SR-2 — FFP, milliradian with MOA equivalents

Source: `Vudu_Reticle_Manual_SR2-3_RevB.pdf` p9, cross-checked against the reticle-index image — every value
agrees. **Calibration printed on the drawing: 7.62×51mm (M118LR), 175 grain, 2550 fps, .495 BC, 1.5 in sight
height.**

| Feature | Value |
|---|---|
| Speed ring | **OD dia 2.15 mrad**, ring **0.3 mrad** thick |
| Centre dot | **Dia 0.44 mrad (1.5 MOA)** |
| Horizontal hashes | **4 mrad (13.75 MOA)** and **6 mrad (20.63 MOA)** |
| Line thickness | **0.07 mrad (0.24 MOA)** |
| BDC bar — 400 yd | **2.5 mrad (8.59 MOA)** |
| BDC bar — 500 yd | **3.7 mrad (12.72 MOA)** |
| BDC bar — 600 yd | **5 mrad (17.19 MOA)** |

## SR-3 — FFP, milliradian with MOA equivalents

Same page. **Calibration printed: .223/5.56mm BTHP, 75 grain, 2900 fps, .395 BC, 1.5 in sight height.**

| Feature | Value |
|---|---|
| Speed ring | **ODR 2.15 mrad**, **0.3 mrad** thick, open at the bottom |
| Centre dot | **Dia 0.44 mrad (1.5 MOA)** |
| Horizontal hashes | **4 mrad (13.75 MOA)**, **6 mrad (20.63 MOA)** |
| Ring bottom opening | **2 mrad (6.88 MOA)** |
| Line thickness | **0.07 mrad (0.24 MOA)** |
| BDC bar — 400 yd | **2 mrad (6.88 MOA)** |
| BDC bar — 500 yd | **3 mrad (10.31 MOA)** |
| BDC bar — 600 yd | **4.1 mrad (14.09 MOA)** |

Note both SR-2 and SR-3 label their holdovers in **yards** (400/500/600 YDS.), unlike everything else in
`data/reticle/`, which is mostly metric.

**The mark positions are exact; the load is nominal.** Every bar's position is a printed callout in both mrad
and MOA with its yardage, so the geometry is exact. The ammunition line is EOTech's representative load, not a
lot-specific one — so when the application labels these marks from a user's own trajectory they will not
necessarily land on 400/500/600 yd, and that is correct behaviour, not an error in the file.

**The zero distance is not printed.** EOTech give bullet, weight, velocity, BC and sight height but no zero —
and a zero is needed before the labels mean anything. With the ladder starting at 400 yd, 100 yd is the
obvious candidate but it is an inference. Treat this the way the CMR-W pair was treated: build the geometry
from the printed positions, then record whichever zero actually puts the labels on 400/500/600 yd, and say in
the `.md` that EOTech do not publish it.

Two more things confirmed against the reticle-index images at full resolution:

- **SR-2's ring is closed; SR-3's is a horseshoe** with a **2 mrad (6.88 MOA)** opening at the bottom, which
  is what that horizontal dimension measures. Otherwise the two centres are identical (2.15 mrad OD,
  0.3 mrad thick, 0.44 mrad dot).
- **Dealers list SR-2 and SR-3 as "MOA" reticles**, but the drawings dimension them **milliradian-first with
  MOA in brackets**. Immaterial for building — every value is printed in both — but do not let the product
  name pick the unit for you.

## SR-4 — FFP, MOA

Source: `Vudu_Reticle_Manual_SR4-5-LE_RevA.pdf` p7. A full engineering drawing: dimensions are given as
**inches at the reticle plane with the angular value in brackets**, e.g. `0.0545(4MOA)`, and they are in the
PDF's **text layer** — no image reading needed.

| Feature | Value |
|---|---|
| Centre dot | **φ0.25 mrad** (`φ0.0117`) = **0.86 MOA** — see the note below |
| Speed ring | **R1.275 mrad** outer (`R0.0597`), **R1.075 mrad** inner (`R0.0504`) → OD 2.55 mrad, 0.2 mrad thick |
| Horizontal numerals | **8, 12, 16, 20 MOA** each side |
| Horizontal hash lengths | 0.5 / 0.39 / 0.37 / 0.11 mrad (`0.0234` / `0.0183` / `0.0173` / `0.0052`) |
| Horizontal extents | **6 mrad** (`0.2811`), **9 mrad** (`0.4217`), **11.5 mrad** (`0.5389`) |
| Vertical numerals | **4 → 40 MOA in 4 MOA steps** |
| Vertical bar widths | **8 MOA** (`0.1090`), **5 MOA** (`0.0682`), **4 MOA** (`0.0545`), **1 MOA** (`0.0136`) |
| Fine values | 0.555 / 0.27 / 0.255 / 0.055 / 0.04 mrad |
| Numerals height | **0.37 mrad** (`0.0173`) |
| Post taper | **60°** |

**The centre-dot discrepancy is resolved.** At full resolution the `φ0.0117(φ0.25MIL)` leader clearly points
at the red dot, so the dot is **0.25 mrad = 0.86 MOA**. EOTech's prose ("a precision 1 MOA center aiming
dot") is that value rounded, not a second measurement. Use 0.25 mrad. Note the oddity that an MOA reticle has
its dot, ring and hash lengths dimensioned in **milliradians** while its numerals and bar widths are in MOA —
the drawing mixes both units throughout, so read every callout's suffix rather than assuming.

## SR-5 — FFP, milliradian

Source: same manual p8. Christmas-tree.

| Feature | Value |
|---|---|
| Centre dot | **0.25 mrad** |
| Speed ring | red ring at centre (same family as SR-4) |
| Horizontal numerals | **2, 3, 4, 5, 6 mrad** each side |
| Horizontal extents | **0.4217 (9 mrad)**, **0.5389 (11.5 mrad)** |
| Vertical numerals | **2, 4, 6, 8, 10, 12 mrad** |
| Tree dot grid | **1 mrad spacing** (`0.0469(1)`) |
| Hash lengths | **0.5 mrad** and **0.11 mrad** |
| Numerals height | **0.37 mrad** |
| Post taper | **60°** |

## LE-5 — FFP, milliradian

Source: same manual p9, cross-checked against the reticle-index image. **Geometrically identical to SR-5** —
same numerals, same 1 mrad tree grid, same 9/11.5 mrad extents, same 0.5/0.11 mrad hashes, same 0.37 mrad
numerals, same 60° taper. The only difference is the centre: **a cross-hair inside the speed ring instead of
a dot** (confirmed by Nikolay from the index images: "SR5 looks exactly like LE5").

That makes SR-5 and LE-5 a pair like the Huron variants — one geometry, one differing feature. Build them
from one generator with a centre-style switch, and say so in both `.md` files so neither looks like a
duplicate shipped by accident.

## BD1 — SFP, MOA

Source: Vudu X 1-6x24 SFP user manual p7 (also in the Vudu X 2-12x40 manual). Dimensioned drawing, values
read off the figure (the text layer holds only the turret spec).

| Feature | Value |
|---|---|
| Open centre width | **20.0 MOA** between the post tips |
| Centre dot | **0.25 MOA** |
| Horizontal hashes | **2.0** and **4.0 MOA** each side |
| Holdover circles | **2, 4, 6, 8 MOA** below centre — 4 circles |
| Circle size | **1.0 MOA** |
| Turret | 0.5 MOA/click, 30 MOA/rotation, 160 MOA travel |

Note BD1 is a **Vudu X** reticle, not classic Vudu, and it is **second focal plane** — subtensions true at
one magnification only, which needs stating in the reticle name as the CM-R2 and Huron files do.

## MD1 — FFP, milliradian

Source: reticle-index drawing, supplied by Nikolay. A plain precision crosshair — no ring, no dot, no tree.
Fine lines are drawn **red** (the illuminated section); the four field-stop posts are black.

| Feature | Value |
|---|---|
| Line width | **0.05 mrad** |
| Graduation | every **0.5 mrad**, on all four arms |
| Extent | **±5 mrad** — the `10 MIL` callout is the **full open width between the posts** |
| Long hash (whole mrad) | **0.5 mrad** |
| Short hash (half mrad) | **0.3 mrad** |
| Posts | heavy, on all four arms, beyond the graduation |

The drawing brackets `1 MIL` with a `0.5 MIL` either side of it, which is how it states that the whole-mrad
interval is split by a half-mrad mark — so the graduation is every 0.5 mrad with the whole mrads longer.

**On the extent — settled, and printed.** The `10 MIL` dimension line runs between the **inner edges of the
left and right posts**, so it is the full open width: **±5 mrad**, ten 0.5 mrad marks per arm. The hash count
in the drawing agrees.

Recorded because it was got wrong once: it was first read as ±10 mrad by reasoning from MD2 (which graduates
to ±30 MOA ≈ ±8.7 mrad) on the assumption that MD1 and MD2 must share a field. **They do not** — MD1 is
±5 mrad (±17.2 MOA) and MD2 is ±30 MOA. Same construction, different extents. Take each drawing's own
callouts and do not cross-infer between the two.

## MD2 — FFP, MOA

Source: reticle-index drawing, supplied by Nikolay. **The MOA counterpart of MD1** — same plain crosshair,
same red-fine-lines-with-black-posts construction, graduated in minutes instead of milliradians.

| Feature | Value |
|---|---|
| Line width | **0.2 MOA** |
| Graduation | every **1 MOA**, on all four arms |
| Numerals | every **10 MOA** — 10, 20, 30 — on all four arms |
| Extent | to **±30 MOA** numerals, posts just beyond |
| Long hash (numbered 10 MOA marks) | **2 MOA** |
| Plain hash | **1 MOA** |
| Posts | heavy, **2 MOA** thick, on all four arms |

**Only two hash lengths exist** — confirmed by Nikolay: **2 MOA on the 10-MOA marks, 1 MOA on every 1-MOA
mark**, and nothing intermediate. The 5-MOA positions are ordinary 1 MOA hashes. So the graduation is fully
specified: nothing has to be chosen.

That matches MD1, which likewise has exactly two lengths (0.5 mrad on whole mrads, 0.3 on halves) — the same
design rule in the other unit.

MD1 and MD2 share a **construction** — fine red crosshair, exactly two hash lengths, four heavy black posts,
no ring or dot — but **not a field**: MD1 is ±5 mrad (±17.2 MOA), MD2 is ±30 MOA. So one generator can still
build both, but the extent, unit, graduation step and hash lengths are all parameters; only the shape is
shared. Each `.md` should point at the other and state the differing extents, so neither looks like a
mis-scaled copy of the other.

---

## HC3 — SFP, MOA, dimensioned at 8×

**Source: the reticle-index drawing, supplied by Nikolay** (2026-07-30). This is the only one of the eight
with no reticle manual, and the index page cannot be reached by our tooling — so the drawing came in by hand.
Units are MOA and the sheet is stamped **8×**, which is the magnification the subtensions hold at (SFP).

| Feature | Value |
|---|---|
| Centre dot | **DIA 0.5 MOA**, illuminated red |
| Horizontal line | **0.25 MOA** thick, plain — **no horizontal graduation at all** |
| Horizontal ends | heavy posts, left and right |
| Upper vertical | **0.5 MOA** thick |
| Lower vertical stem | **0.25 MOA** thick, heavy post stub at the bottom |
| Holdover bar 1 | **2 MOA** below centre |
| Holdover bar 2 | **5 MOA** |
| Holdover bar 3 | **8.5 MOA** |
| Holdover bar 4 | **12.5 MOA** |
| Bar thickness | **0.25 MOA** |
| Bar width | **2 MOA** (dimensioned on the lowest bar; the four are drawn equal) |

The four bar positions match EOTech's product copy exactly ("2-, 5-, 8.5- and 12.5-MOA subtensions on the
vertical axis at 8X"), which is what confirms the drawing is HC3 and not one of the HC1 variants whose tabs
are visible behind it.

Two notes:

- **The vertical stem changes thickness across the centre** — 0.5 MOA above, 0.25 MOA below. That is what the
  drawing dimensions; it is not a duplex taper.
- **Nothing is dimensioned horizontally except the line thickness**, so where the heavy posts begin is a
  framing choice for us, as it was for the ACOGs.

### What was tried before the drawing arrived

Recorded so nobody repeats it: the **1-8x24 SFP scope manual** was obtained (24 pp) and mentions "HC3"
exactly **once**, in the turret table on p5 — no reticle drawing at all. A dedicated HC3 reticle manual was
probed on `farrwest` and the Shopify CDN across `_RevA/_RevB/_RevC`, bare names and combined names
(`HC1-3`, `HC3-BD1`) over several upload months: all 404, even though HC1 and HC2 manuals both exist. CDN
image-name guessing (`HC3.png`, `HC3_Reticle.png`, …) also 404.

Turret data, from `Vudu_1-8x24SFP_RevB.pdf` p5: 0.25 MOA/click, 20 MOA/rotation, 100 MOA elevation travel,
80 MOA windage travel. Field of view at 100 yd: 105.8 ft at 1×, 13.2 ft at 8×.

---

## Notes for the build

- **Units:** SR-1/2/3/5 and LE-5 are milliradian; SR-4 and BD1 are MOA. SR-2/SR-3 print both. Use `mrad`,
  never `mil` (military mil), per the reticle-designer skill.
- **Focal plane:** SR-1…SR-5 and LE-5 are **FFP**; **BD1 and HC3 are SFP** and need the magnification in
  the reticle name.
- **Not to scale:** the SR-1 sheet says so explicitly; the SR-4/5/LE-5 sheets are true engineering drawings.
  Either way, use printed values only — do not measure these.
- **SR-2/SR-3 are load-calibrated with the load published**, so they go in the README's load-calibrated
  table with real entries. SR-1, SR-4, SR-5, LE-5 and BD1 have no load and behave like CM-R2: geometry
  published, ladder unattributed.
- **SR-5/LE-5 share a geometry**, so one generator with a centre-style switch produces both.
- **The SR-4 centre dot discrepancy** (φ0.25 mrad drawn vs "1 MOA" written) should be resolved before
  building — 0.86 MOA vs 1 MOA is small but it is the aiming point.
