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
- **Generator: VitePress** (Node/Vite, Markdown-first, built-in offline search). `base` must be set to
  `/ballistic.calculator.app.avalonia/` for Pages to resolve assets.
- **Publish from GitHub Actions on `main`** (`actions/deploy-pages`), not a hand-maintained `gh-pages`
  branch — triggered on pushes touching `doc/**` plus manual `workflow_dispatch`.
- **Open via the existing pattern** — the private `OpenUrl(...)` helper already in
  `Desktop/BallisticCalculator/Views/MainWindow.axaml.cs:628`
  (`Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true })`).

### What this decision removed

The previous design shipped the site inside the app and served it from a loopback **Kestrel** host, because
SPA routing/search break under `file://`. Serving from Pages removes that entire layer. The Kestrel idea
was also motivated by a future **mobile companion link** (desktop hosts, mobile connects) — that remains
worth doing, but it is now an **independent track with no docs dependency**, and is described under
"Deferred, unrelated" below rather than as a docs phase.

## Architecture

### Authoring (`doc/` — new VitePress project)
- `doc/package.json`, `doc/.vitepress/config.ts` (`base`, nav, sidebar, search), `doc/index.md` + guide pages:
  Getting Started, Entering a Shot, Zeroing, Parameters/Coriolis, Reticle, Summary, Tools (Approximate Drag
  Table, Hit Probability, BC converter), File formats (`.drg`, ammo library, reticle).
- Scripts: `npm run docs:dev` (author with live reload), `npm run docs:build` → `doc/.vitepress/dist`.
- `.gitignore`: `doc/node_modules/`, `doc/.vitepress/dist/`, `doc/.vitepress/cache/`.
- Screenshots under `doc/public/img/`; capture at a fixed window size so they stay consistent.

### Publishing (`.github/workflows/docs.yml` — new; no workflows exist in the repo yet)
- `on: push: branches: [main], paths: ['doc/**', '.github/workflows/docs.yml']` + `workflow_dispatch`.
- Job: `actions/checkout` → `actions/setup-node` (LTS, npm cache) → `npm ci` → `npm run docs:build` →
  `actions/configure-pages` / `upload-pages-artifact` (`doc/.vitepress/dist`) → `actions/deploy-pages`.
- Permissions `pages: write`, `id-token: write`; Pages source set to **GitHub Actions** in repo settings
  (one-time manual step).

### Menu wiring (`Views/MainWindow.axaml` + `.axaml.cs`)
- Help menu currently holds only `_About` (`MainWindow.axaml:118`). Add above it:
  - `Help → _User Guide` (F1) → `OpenUrl(HelpUrl)`.
  - separator, then the existing `_About`.
- Keep the URL as one `const string HelpUrl` next to `OpenUrl`; deep links per page can come later
  (e.g. `HelpUrl + "tools/hit-probability"`) if we want context help from dialogs.
- No build/packaging changes: `BuildRelease.bat`, `Setup/prepare.bat` and the portable `content/` folder are
  untouched.

## Verification
1. `npm run docs:build` succeeds; local `npm run docs:preview` serves the site with working search.
2. Push to `main` → workflow green → the Pages URL loads, nav/search/assets all resolve under the `base`
   path (asset 404s are the classic symptom of a wrong `base`).
3. App: `Help → User Guide` opens the default browser at the Pages URL on Windows and Linux.

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
