# Screenshots for README.md — what to capture

Written **2026-07-27**. The README has no images yet; these are the ones worth producing, in priority order.
Six essential, two optional — every screenshot is upkeep, so six good ones beat twelve stale ones.

Target folder: `screenshots/` at the repository root. Once the files exist, the README gets the image links at
the placements below.

## The shots

| # | File | What is on screen | Where it goes in README.md | Why this one |
|---|---|---|---|---|
| 1 | `01-sight-picture.png` | Trajectory window on the **reticle** view: the best-looking reticle, BDC marks resolved, target box visible | Top, directly under the tagline | The hero. Instantly reads "ballistic calculator" and shows the feature commercial apps charge for. Nothing else in the app is as distinctive |
| 2 | `02-table.png` | **Table** view — drop / windage / velocity / energy / time columns, filled out to a long range | Under "Results as a table, a chart, and a sight picture" | Proves it is a serious solver rather than a toy. Dense numbers do that at a glance |
| 3 | `03-chart-compare.png` | **Chart** with two or three trajectories compared, not a single curve | Same bullet list | One curve is unremarkable; the comparison is the differentiator |
| 4 | `04-hit-probability.png` | Hit Probability **after pressing Estimate** — probability, the shots-to-hit row, and the impact scatter with the vital-zone circle | Under the Hit probability bullet | The newest feature, and the scatter is the second most visual thing in the app |
| 5 | `05-linux.png` | The **same** trajectory window running on Linux, with a window frame that is obviously not Windows | Beside the "Truly cross-platform" goal | The one that does real work. "Truly cross-platform" backed only by Windows screenshots is a claim; this makes it evidence |
| 6 | `06-drag-table.png` | Approximate Drag Table → From BC Curve, **with knots loaded** (5–6 rows spanning roughly Mach 1.2–2.5) | Under the Drag models bullets | The `.drg` tooling is the technical heart of the accuracy argument, and it is invisible without a picture |
| 7 *(optional)* | `07-reticle-editor.png` | Reticle editor with a reticle part-built, element tree visible | Under "Libraries and editors" | Sells "build your own reticle", but it is a second application — fine to omit from a first pass |
| 8 *(optional)* | `08-inputs.png` | Shot parameters dialog on the **Wind** or **Zero** tab, showing multiple wind zones or zeroing with another cartridge | Under the Trajectory bullets | Substantiates the two subtlest features, though input forms photograph poorly |

## Capture notes

These are the things that cause a redo:

- **Use one shot across all of them** — same cartridge, same range, same measurement system. If #2 shows
  2700 fps and #4 shows 2850, it reads as carelessness whether or not anyone checks properly.
- **Capture at 100% display scaling.** At 125% or 150% the text resamples and looks soft at README width.
  Native pixels, and no resizing afterwards.
- **Fixed window size**, around 1200×800, consistent across shots so images do not jump around as the page is
  scrolled.
- **Populate the window before capturing.** The drag table editors and the hit probability window open empty by
  design, and an empty one shows nothing. Hit Probability in particular needs **Estimate** pressed.
- **Check the title bars** for local file paths, and the Hit Probability title, which names the shot.
- PNG, under about 300 KB each — GitHub serves them on every page view.

## Follow-up

- Add the image links to `README.md` at the placements in the table, sized so they do not dominate a wide
  monitor.
- Screenshots go stale: when a panel's layout changes materially, the matching image needs retaking. Worth a
  glance whenever a Tools window is reworked.
