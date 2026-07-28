# Plan: user documentation on GitHub Pages

Not scheduled with the 2026-07-25 feature work — separate track. Design only.

## Context / goal

Cross-platform user help for the Avalonia app (Windows/Linux/macOS). CHM/WinHelp are dead. Author the
guide in Markdown, build a **static HTML site**, and **publish it to the project's GitHub Pages**. The app's
Help menu opens that URL in the user's default browser.

## Decisions (2026-07-27)

- **Hosting: GitHub Pages of this repo — the only copy.** Site URL
  `https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/`.
- **Online only.** Nothing is bundled into the app, so there is **no embedded HTTP host**, no `Help/`
  content folder, no `FrameworkReference`, and no `file://` constraint on the generator. Users without a
  connection get no in-app help; accepted.
- **Generator: none of our own — plain Markdown built by GitHub's own Jekyll** (revised 2026-07-27,
  superseding VitePress; see "What the Markdown decision removed"). No Node toolchain, no lockfile and
  no build script in the repository.
- **Theme: `jekyll-theme-slate`** (decided 2026-07-28, superseding `just-the-docs`). One of GitHub's
  built-in supported themes, so it goes in as `theme:` rather than `remote_theme:`. The trade is
  explicit: Slate has **no sidebar and no search**, so `index.md` carries the contents by hand and every
  article ends with a link back to it. Revisit if the manual outgrows a hand-written index.
- **Publish by folder, from `main`** — Pages set to *Deploy from a branch*, source `main` + `/docs`.
  No workflow, no `gh-pages` branch, nothing duplicated. `doc/` → `docs/` **renamed 2026-07-28**
  (briefly `usermanual/`, reverted): GitHub only offers the repository root or `/docs` as a branch
  source.
- **No pretty permalinks.** Pages resolve as `/about.html`, so relative image paths
  (`screenshots/reticle.png`) work on the site *and* in GitHub's view of the same `.md`.
  `permalink: pretty` would move pages to `/about/` and break all of them.
- **Open via the existing pattern** — the private `OpenUrl(...)` helper already in
  `Desktop/BallisticCalculator/Views/MainWindow.axaml.cs:628`
  (`Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true })`).

### What the Markdown decision removed

VitePress bought two things — a sidebar and offline search — at the price of a Node dependency tree,
`npm ci` in CI, a `base`-path trap, and a generator to keep current. Both are given up rather than
replaced (see the Slate decision above): the contents live in `index.md`, written by hand. What
actually goes away:

- `doc/package.json`, `doc/.vitepress/`, the `docs:dev` / `docs:build` scripts, three `.gitignore`
  entries, and `.github/workflows/docs.yml` entirely.
- The `base: '/ballistic.calculator.app.avalonia/'` trap. Jekyll's `baseurl` handles it, and relative
  links need no prefix at all.
- The `doc/site/` vs `doc/screenshots/` layout conflict — there is no page scanner to exclude
  anything from. `docs/screenshots/README.md` has no front matter, so Jekyll would copy it verbatim;
  `_config.yml` lists it under `exclude:` instead, keeping it a contributors' note in the repository
  and off the site.

What it costs, and it is worth stating plainly rather than discovering later:

- **Every page needs YAML front matter.** Jekyll converts only files that have it; a bare `.md` is
  copied verbatim and the browser shows plain text. So each article opens with `---` / `title:` /
  `nav_order:` / `---`. Three or four lines, and the file still reads fine in an editor.
- Kramdown, not GitHub-flavoured Markdown. Close enough that it rarely matters; footnotes and
  attribute lists are actually better, but do not assume every GFM extension works.
- No live-reload preview without installing Ruby locally. In practice the loop is "read the `.md` in
  the editor, push, look at the site" — acceptable for a manual, and GitHub's own file view renders
  the page well enough to check structure before pushing.

The gain nobody planned for: because the pages are plain Markdown with relative links, the manual is
readable **in the repository, on GitHub, offline and in any editor** — not only as a built site.

### What this decision removed

The previous design shipped the site inside the app and served it from a loopback **Kestrel** host, because
SPA routing/search break under `file://`. Serving from Pages removes that entire layer. The Kestrel idea
was also motivated by a future **mobile companion link** (desktop hosts, mobile connects) — that remains
worth doing, but it is now an **independent track with no docs dependency**, and is described under
"Deferred, unrelated" below rather than as a docs phase.

## Content plan (2026-07-27)

### Who we are writing for

One audience with two entry points, and the split matters because it decides how much ballistics we
teach:

- **The shooter who wants a firing solution.** Knows what a zero and a scope click are; does not
  necessarily know what a ballistic coefficient *is*, only that a box quotes one. Needs task
  guidance, in the order the work happens.
- **The shooter who already runs another solver.** Wants to know what this model includes, where its
  numbers come from, and how to feed it a measured drag curve. Needs reference and honesty about
  limits.

**Decided (2026-07-27): no ballistics primer.** We teach the *application*, and explain a concept only
where not explaining it makes the UI unusable — form factor, BC vs drag table, 1σ group size, station
vs sea-level pressure. Everything else is handled by **pointing at the literature**: the reference
links the README already carries, plus recommended books, gathered in article 21 and cross-linked from
the articles that touch each topic. A primer inside the manual would be a different and much larger
book, and it would compete with better ones.

**Decided (2026-07-27): task-based orientation** — see the principles below.

### Principles for this manual

- **One goal per article, stated in its first paragraph.** If an article needs two goals, it is two
  articles.
- **Task order, not menu order.** A page per dialog tab is the failure mode to avoid: it produces
  field lists nobody reads and duplicates the tooltips. Fields are documented where a task needs
  them; the exhaustive per-field table lives once, in the reference part.
- **State the caveat where the number is produced**, not in a disclaimer nobody reaches. The
  approximation tools and hit probability are the places this matters most.
- **Every article names its screenshots.** Existing captures are catalogued in
  [`../docs/screenshots/README.md`](../docs/screenshots/README.md); the inventory below marks what is
  already shot and what still needs capturing.

### Article inventory

Phase numbers are priority, not chapter order. **Phase 1 is the minimum publishable manual** — an
honest guide with nothing important missing. Phases 2 and 3 add depth.

#### Part 1 — Getting started

| # | Article | Goal — what the reader can do afterwards | Phase |
|---|---|---|---|
| 0 | **What Ballistic Calculator 2 is** (`about.md`) — **written 2026-07-28** | Decide whether the application answers the reader's question before they invest an evening in it: what it computes, the four goals, what it deliberately does *not* do (no 4DOF angular motion, no primer, no load data, no online lookup), and the risk notice in full. Added to the inventory when the first pages were written — the README's job, restated for a reader who arrived at the manual first | 1 |
| 1 | **Installation and first run** (`installation.md`) — **written 2026-07-28** | Get from the Releases archive to a running app on Windows or Linux: the .NET 8 runtime requirement, unzip anywhere writable, `chmod +x` on Linux, what the `data` folder holds and why it must stay beside the executable, where `appstate.json` lives, and the imperial/metric choice made at `Trajectory → New`. Ends by pointing at article 2 | 1 |
| 2 | **Your first trajectory** (`first-trajectory.md`) — **written 2026-07-28** | The map, not a walkthrough: the imperial/metric choice and how little it binds, one section per tab of the Shot Parameters dialog in workflow order (each linking to its own detailed article), what OK validates — the three messages, in order — the shortest possible first run on defaults, the four views of the answer, and `Ctrl+E` as the way to iterate | 1 |

*Screenshots: `params_1_ammo.png`, `ballistic_table.png` (both shot). Needs: the empty main window at
first run. `about.md` reuses the README's five (`ballistic_table`, `reticle`, `compare_charts`,
`hit_probability`, `custom_drg`); `installation.md` carries none yet.*

#### Part 2 — Building and running a shot

**Restructured 2026-07-28: one detailed article per dialog tab**, hung off `first-trajectory.md` as the
hub, in tab order (Ammunition, Weather, Wind, Rifle, Zero, Parameters). This is not the "page per dialog
tab" failure mode the principles above warn about, and the distinction is the point: each tab *is* a step
of the workflow, and each article is organised by **what the reader is trying to do** — enter a load by
hand, load a saved one, drive the shot from a measured curve, decide whether a field matters — not by
walking fields top to bottom. The exhaustive per-field table still belongs once, in the reference part.

| # | Article | Goal | Phase |
|---|---|---|---|
| 3 | **The Ammunition tab** (`ammunition-tab.md`) — **written 2026-07-28**, absorbing "Describing the load" | Four scenarios, in the order readers meet them: enter a load by hand and `Save` it as `.ammox`; `Load` a saved or legacy `.ammo` one; drive the shot from a `.drg` curve through `Browse…` (what the five automatic changes are, and that muzzle velocity is deliberately not one of them); and when diameter and length are actually needed — spin drift, aerodynamic jump, and the hard stop that a form factor without a diameter cannot be computed at all. Ends by stating that caliber / bullet type / barrel length / source never reach the solver | 1 |
| 4 | **Zeroing** | Understand that the app computes the zero mathematically rather than being told a sight angle, then use zero distance, impact offset at zero, and — the feature no other free solver has — zeroing with a *different* cartridge, atmosphere or wind than the shot. Worked case: zero supersonic, shoot subsonic | 1 |
| 5 | **Atmosphere and wind** | Enter conditions without the two classic errors: station pressure vs sea-level pressure, and wind direction convention. Covers multiple wind zones along the flight path and when they are worth the trouble | 1 |
| 6 | **Range, step, angle and the Coriolis effect** | Know what each run-time knob changes: max range and step (and the cost of a fine step), uphill/downhill shots, already-dialled clicks, and the azimuth + latitude that Coriolis needs. Says when Coriolis is noise and when it is not | 2 |
| 7 | **Reading the table** | Interpret every column and the two conventions behind them — drop over line of sight vs over muzzle level, hold vs drop, clicks in the reader's chosen angular unit, windage adjustment, energy, optimal game weight | 1 |
| 8 | **Chart, reticle and summary** | Use the other three views: chart variables (velocity, Mach, drop, windage, energy) and Y-axis zoom; the sight picture with far/near BDC and target overlays, a loaded custom reticle, and the moving-target aim-off box; the summary's point-blank range, dead zone, near and far zero, and the distance where the bullet goes subsonic | 1 |
| 9 | **Comparing loads, saving and exporting** | Put several trajectories on one chart, save and reopen a shot, and export CSV in the local (Excel) or invariant (portable) format — including why that choice exists | 2 |

*Screenshots: `params_2_weather.png`, `params_3_wind.png`, `params_4_rifle.png`, `params_5_zero.png` +
`params_5_zero_1.png`, `params_6_shot.png`, `ballistic_table.png`, `chart.png`, `reticle.png`,
`compare_charts.png` (all shot). Needs: the summary view, a BDC overlay, a moving-target lead box.*

#### Part 3 — Drag models: where accuracy comes from

This part is the manual's argument, and the reason the application exists. It should read as one
sequence.

| # | Article | Goal | Phase |
|---|---|---|---|
| 10 | **Choosing a drag model** | Decide between a standard curve and a measured one, knowing what a BC actually is (a ratio to a reference projectile, not a property of the bullet), when G1 misleads and G7 does not, and what the form-factor switch means | 1 |
| 11 | **Custom drag tables (`.drg`)** | Load a projectile's own measured Cd curve, understand the form-factor-of-1 convention that comes with it, and know what a `.drg` does *not* carry — the muzzle velocity is the reader's to enter | 1 |
| 12 | **Approximating a drag table you do not have** | Produce a usable curve from what a data sheet or a chronograph actually gives: a multi-BC curve, or measured downrange velocities. Both tools in one article because the choice between them is the reader's real question. Must state the accuracy caveat next to the Save button, not below the fold | 1 |
| 13 | **Converting a ballistic coefficient between tables** | Answer the everyday G1 ↔ G7 question at a chosen reference velocity, and understand why the answer is velocity-dependent — i.e. why a single converted number is a compromise | 2 |

*Screenshots: `params_1_ammo_gc.png`, `custom_drg.png` (both shot). Needs: From BC Curve with knots
loaded, the BC converter.*

#### Part 4 — Analysis

| # | Article | Goal | Phase |
|---|---|---|---|
| 14 | **Hit probability** | Build an honest error budget — group size as a **1σ per-axis** figure (about a quarter of a large group's extreme spread), shooting position, range and wind estimation error, muzzle-velocity deviation — then read the three outputs: single-shot probability, shots for a first hit at 50–98 %, and the impact scatter. Must be explicit that the estimate assumes a correct come-up and wind hold, and a circular vital zone | 1 |

*Screenshot: `hit_probability.png` (shot).*

#### Part 5 — Libraries and editors

| # | Article | Goal | Phase |
|---|---|---|---|
| 15 | **Ammunition library, sight and barrel presets** | Stop re-typing: save loads, build sight presets with click values, and barrel presets with twist | 2 |
| 16 | **The reticle editor** | Build or edit a reticle: the moa/mil coordinate space and where zero sits in it, the element list (lines, paths, circles, rectangles, text, BDC marks), and how the result appears in the sight picture | 2 |

*Screenshot: `reticleeditor.png` (shot). Needs: the ammunition library window, a sight preset editor.*

#### Part 6 — Reference

| # | Article | Goal | Phase |
|---|---|---|---|
| 17 | **Units and measurement systems** | Look up every unit the app accepts, switch systems without losing precision, and choose an angular unit (MOA, mil, thousandths, mrad, in/100 yd, cm/100 m) | 2 |
| 18 | **File formats** | Read or produce the files by hand: `.drg`, the ammunition library, reticle files, and the saved shot | 3 |
| 19 | **What the model does and does not include** | Judge the numbers: 3DOF point-mass integration plus spin drift, aerodynamic jump and Coriolis; what a 4DOF model adds (angular motion, not a better drag model); and the risk notice in full rather than as fine print | 1 |
| 20 | **Troubleshooting and FAQ** | Resolve the recurring stumbles — "why is my drop wrong at 1,000 yd", "why is spin drift zero", "why does the table stop early", "why do two solvers disagree" — each answer pointing at the article that explains it properly | 3 |
| 21 | **Recommended reading** (`recommended-reading.md`) — **written 2026-07-28**, titled as the README calls it rather than "Further reading" | Learn the ballistics this manual deliberately does not teach. Carries the README's reference links plus the standard books, each with one line on *why* it is worth the reader's time and which article sends them there. This page is what makes "no primer" an honest position rather than a gap | 1 |

Candidate books for article 21, mapped to the articles that should link to them:

| Work | Why | Sends readers from |
|---|---|---|
| Litz, *Applied Ballistics for Long-Range Shooting* | The practical standard: BC, drag models, wind, spin drift and jump explained for shooters rather than engineers | 3, 5, 10 |
| Litz, *Accuracy and Precision for Long Range Shooting* | Hit probability done properly — the WEZ analysis this app's Monte-Carlo tool is a small cousin of, including why group size dominates at some ranges and wind call at others | 14 |
| Litz, *Ballistic Performance of Rifle Bullets* | Measured G1/G7 BCs and form factors for real projectiles — where a reader gets trustworthy numbers to type in | 3, 10, 13 |
| McCoy, *Modern Exterior Ballistics* | The technical reference behind the engine: point-mass and 6DOF formulations, drag coefficients, the standard drag families | 19 |
| Vaughn, *Rifle Accuracy Facts* | Where dispersion actually comes from — the input the hit-probability tool asks for and cannot compute for you | 14 |
| The README's Wikipedia links (external ballistics, projectile motion, ballistic coefficient, reticle) | Free, adequate, and enough to follow the manual | 1, 10 |

### Phase 1 in one line

Articles **1, 2, 3, 4, 5, 7, 8, 10, 11, 12, 14, 19, 21** — thirteen pages. That is a manual a stranger
can use without asking us anything, and it is written entirely from features that already exist.
Article 21 is in Phase 1 on purpose: "no primer" only works if the reader is told where to go instead.

### Open questions on content

- **One page per approximation tool, or one page for both?** Planned as one (article 12) because the
  reader's question is "which of these do I use"; splitting is easy if it grows.
- **Screenshot policy for the manual** — the README set is restrained by design, but a manual can
  carry many more. Worth agreeing a per-article budget before capturing, since every image is upkeep.

## Architecture

### Authoring (`docs/` — plain Markdown, no project to install)

Done 2026-07-28: `doc/` → `docs/`, the five image paths in the root `README.md`, and the references in
this plan, `claude/DEFECTS.md` and `docs/screenshots/README.md`.

```
docs/
  _config.yml          theme: jekyll-theme-slate, title, description, url, baseurl, exclude
  index.md             the manual's front page — the contents by hand, and where to start
  <article>.md         one file per article in the inventory, front matter for title + nav order
  screenshots/         already committed; images shared with the root README
```

- **Front matter is mandatory** — Jekyll only converts files that have it. Minimum per page:
  `title:` plus `nav_order:`. `nav_order` builds nothing under Slate; it is kept as the record of
  reading order, and as what a nav-bearing theme would need if we ever switch back.
- **Slate has no sidebar**, so `index.md` is the only table of contents and every article ends with a
  `[← Contents](index.md)` link. Adding an article means adding it to `index.md` by hand.
- **One image store.** `docs/screenshots/` serves both the root README and the manual; no second copy
  under an assets folder. Relative links (`screenshots/reticle.png`) resolve in the built site and in
  GitHub's file view alike — which is what forbids `permalink: pretty`.
- `docs/screenshots/README.md` is listed under `exclude:` in `_config.yml` — a contributors' note, kept
  in the repository and off the site.
- Capture at a fixed window size and 100 % display scaling so shots stay consistent; the capture
  notes in `claude/SCREENSHOTS.md` still apply.

### Publishing (no workflow, no `gh-pages` branch)
- Repo Settings → Pages → **Deploy from a branch**, branch `main`, folder `/docs`. One-time click.
- GitHub runs Jekyll on every push that touches `docs/`. Nothing else in the repository is published.
- `jekyll-theme-slate` is one of GitHub's built-in supported themes, so it goes in as `theme:` — no
  `remote_theme`, no `Gemfile`.
- `baseurl: /ballistic.calculator.app.avalonia` in `_config.yml` so theme assets resolve under the
  project-pages path.

### Menu wiring (`Views/MainWindow.axaml` + `.axaml.cs`) — **done 2026-07-28**
- `Help → _User Guide` (F1), a separator, then the existing `_About` (which keeps `Ctrl+F1`). Bare F1
  is handled at the top of `OnKeyDown`, before its `if (!ctrl) return` — `InputGesture` only draws the
  accelerator, as the existing `Ctrl+…` shortcuts already show.
- The URL is one `internal const string HelpUrl` next to `OpenUrl`; deep links per page can come later
  (e.g. `HelpUrl + "installation.html"`) if we want context help from dialogs.
- `internal Action<string> UrlOpener { get; init; } = OpenUrl` is the test seam. Without it a test that
  clicks the item launches a real browser on the machine running the suite.
- Cover: `HelpMenuTests` — the click, bare F1, F1 with a modifier doing nothing, the menu order, and
  that `HelpUrl` is the published address. The one thing tests cannot reach is `Process.Start` itself,
  so *press F1 once in the real app* after touching this code.
- No build/packaging changes: `BuildRelease.bat`, `Setup/prepare.bat` and the portable `content/` folder are
  untouched.

**A trap this uncovered, worth knowing before writing any other `MainWindow` test:** only the *first*
`MainWindow` constructed in a headless process works — `WindowsPanel` resolves its control theme once,
and the next window to lay out throws `ArgumentNullException: PART_Windows`. Merely constructing a
second one (no `Show()`) is enough to break a *later* test. `HeadlessMainWindow` now owns the single
instance, and `WindowsMenuTests` and `HelpMenuTests` share it through one xUnit collection.

## Verification

Done 2026-07-28 for the first three articles (`10d5e5c`):

1. ✅ Pages build green for the pushed commit, site root 200. A page rendering as **plain text is the
   missing-front-matter symptom**; theme CSS 404s are the wrong-`baseurl` symptom. Checked with
   `gh api repos/<owner>/<repo>/pages/builds/latest` and `curl`.
2. ✅ `index.md` lists every article that exists (no sidebar to check — Slate has none, and no search).
3. ✅ Screenshots load on the site **and** in GitHub's own view of the same `.md` file — the point of
   relative paths. `assets/css/style.css` resolves under the `baseurl`, and `screenshots/README.md`
   is 404 by way of `exclude:`.
4. ⏳ App: `Help → User Guide` opens the default browser at the Pages URL — wired and unit-tested, but
   `Process.Start` itself is only provable by pressing F1 in the real app on each platform.

## Open decisions
- Whether to also produce a PDF manual (pandoc) as a secondary artifact — currently **no**.
- Whether dialogs get context-sensitive deep links (F1 per window) or one entry point only.
- Docs versioning: single "latest" site vs per-release copies — start with latest only.

## Deferred, unrelated: mobile companion link
Kept here only so the reasoning is not lost. If a mobile client is ever built, the desktop app can host an
**opt-in, authenticated** LAN listener (Kestrel minimal `WebApplication`, pairing token/QR, TLS preferred,
default off, never unauthenticated on `0.0.0.0`) with JSON/WebSocket endpoints, sharing DTOs from
`Common/` (e.g. `Common/BallisticCalculator.Contracts`) between the desktop *server* and the mobile
*client*. This has nothing to do with documentation and should not gate it.
