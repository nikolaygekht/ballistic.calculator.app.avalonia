# Plan: User documentation + embedded local host

Not scheduled with the 2026-07-25 feature work — separate track. Design only.

## Context / goal

Cross-platform user help for the Avalonia app (Windows/Linux/macOS). CHM/WinHelp are dead. Author the
guide in Markdown, build a **static HTML site**, ship it with the app, and open it in the user's default
browser. Serve it from a **tiny embedded local HTTP host** rather than `file://` (modern JS doc sites are
SPAs whose routing/search break under `file://`). The host is deliberately chosen so it can later double
as the channel a companion **mobile app** talks to.

## Decisions

- **Generator: VitePress** (Node/Vite, Markdown-first, minimal config, built-in offline search). Emits a
  fully static `dist/` (prerendered HTML per page + assets). GitHub Pages hosts the same `dist/` for the
  online copy. *(Alternatives considered: Astro Starlight — closest to working from `file://`; Docusaurus
  — heavier. MkDocs rejected: Python.)*
- **Serve locally with Kestrel** (ASP.NET Core minimal `WebApplication`), not raw `HttpListener`: for help
  alone `HttpListener` suffices, but Kestrel gives routing/JSON/WebSockets for the future mobile API for
  free, and the `Microsoft.AspNetCore.App` runtime is already installed. One host serves static help now
  and API endpoints later.
- **Open via the existing pattern** — `Process.Start(new ProcessStartInfo { FileName = url,
  UseShellExecute = true })` (same call the About dialog uses), pointed at the loopback URL.

## Architecture

### Authoring (`doc/` — new VitePress project)
- `doc/package.json`, `doc/.vitepress/config.ts` (nav/sidebar/search), `doc/index.md` + guide pages
  (Getting Started, Entering a Shot, Zeroing, Parameters/Coriolis, Reticle, Summary/Tools, File formats).
- Scripts: `npm run docs:dev` (author), `npm run docs:build` → `doc/.vitepress/dist`.
- `.gitignore`: `doc/node_modules/`, `doc/.vitepress/dist/`, `doc/.vitepress/cache/`.

### Build/bundle
- Build step runs `npm ci && npm run docs:build` and copies `doc/.vitepress/dist` → the app output under
  `Help/` (a Help content folder next to `Assets/`). Wire into `BuildRelease.bat` / `Setup/prepare.bat`
  (and/or an MSBuild `AfterBuild` target that copies to `bin/.../Help`). Portable package already ships a
  `content/` folder — include `Help/` there.

### Runtime host (`Desktop/BallisticCalculator/Services/HelpServer.cs` — new)
- Static class `HelpServer.EnsureRunning()` → builds a minimal `WebApplication` once, `UseStaticFiles`
  (or `MapStaticAssets`) rooted at `AppContext.BaseDirectory/Help`, binds to `127.0.0.1:<free port>`,
  starts it, returns `http://127.0.0.1:<port>/`. Idempotent; disposed on app exit.
- Requires `<FrameworkReference Include="Microsoft.AspNetCore.App" />` in
  `Desktop/BallisticCalculator/BallisticCalculator.csproj`. *(Fallback if we want to avoid that: ~30-line
  `System.Net.HttpListener` static server — but then no easy path to the mobile API.)*

### Menu wiring (`Views/MainWindow.axaml` + `.axaml.cs`)
- `Help → User Guide` → `Process.Start(HelpServer.EnsureRunning(), UseShellExecute)`.
- `Help → Online Docs` → open the GitHub Pages URL.

## Phasing

- **v1 (with the docs work):** loopback-only static help host + VitePress site + Help menu. No network
  exposure, safe by default.
- **v2 (only when a mobile client exists):** promote the host to an **opt-in, authenticated** LAN
  listener — separate "Enable mobile link" toggle, pairing token (QR), ideally TLS (self-signed), off by
  default. Add JSON/WebSocket endpoints. Never listen on `0.0.0.0` unauthenticated.
- Keep request/response **DTOs in `Common/`** (e.g. `Common/BallisticCalculator.Contracts`) so the desktop
  *server* and the mobile *client* share models — consistent with "shared logic in Common, platform UI on
  top." Note this is a companion/remote pattern (desktop hosts, mobile connects), distinct from a
  standalone mobile app reusing `Common` locally.

## Security notes
- Help mode binds loopback (`127.0.0.1`) only — not reachable off-machine.
- Any LAN binding is explicit, authenticated, and TLS-preferred; default off.

## Verification
1. `npm run docs:build` produces `doc/.vitepress/dist`; build step drops it into the app's `Help/`.
2. App runs; `Help → User Guide` opens the browser at `http://127.0.0.1:<port>/`, site + search work
   offline; `Help → Online Docs` opens GitHub Pages.
3. No listener on non-loopback interfaces in v1 (verify with `netstat`).

## Open decisions
- Generator confirm: VitePress vs Astro Starlight.
- Kestrel (`FrameworkReference`) vs `HttpListener` for v1 (recommend Kestrel for the mobile future).
- Whether to also produce a PDF manual (pandoc) as a secondary offline artifact.
- Where the GitHub Pages site is published from (docs branch / Actions).
