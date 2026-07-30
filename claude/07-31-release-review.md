# Before the first release — review list for 2026-07-31

Written at the end of 2026-07-30, superseding [`07-30.md`](07-30.md). Everything on that shortlist is
closed except the manual's remaining stub articles; what follows is the state to start the review from and
what is genuinely left.

## State

- **BallisticCalculator 1.1.13**; clean build.
- **1006 tests pass** — Controls 322, Panels 557, ReticleEditor 89, desktop app 38.
- **`main` is pushed and level with `origin`**, head `09bb12e`. The GitHub Pages manual is live from it.
- **34 reticles**, all parsing with no errors/warnings/notes, each with a companion `.md` and a README row.
- **Every text file is CRLF**, pinned by `.gitattributes` and renormalised in `09bb12e` (212 files,
  verified content-identical).

## Closed on 2026-07-30

Both remaining blockers from `07-30.md`:

- **The Tools smoke pass** — done by hand, including signing.
- **The article that mattered most** of the five stubs — `what-the-model-includes.md`, the one the risk
  notice points at. Written against the engine source rather than from memory.

And a run of things found along the way:

- **`7z a *.*` was dropping every extension-less launcher** from all six archives, so the Linux and macOS
  builds shipped with nothing to start. Fixed, and the Unix builds now ship `.tar.gz` so the launcher
  arrives executable.
- **`pack.bat` / `prepare.bat` now fail loudly** — publish, staging, signing and packing are all checked,
  and `prepare.bat` aborts at the first failing RID instead of leaving a mixed set of archives.
- **Presets moved out of the shipped file** into `user-dictionaries.xml`, with Reset in both editors.
- **Shot Parameters blamed the wrong tab** for a missing zero distance, and a partial `<zeroing>` block
  silently blanked the Zero tab. Both fixed.
- **`data/legacy-ammo` → `data/ammo`**; ACOG/Elcan presets corrected to a 100 m zero; `ACOG TA-32`
  renamed `TA31` with its sight height aligned to that reticle's ladder.
- **macOS is documented and verified on real hardware** — it runs; the original failure was an
  `osx-arm64` archive on an Intel Mac.

## What is actually left

### Must do before cutting the release

1. **A clean `prepare.bat` run.** The archives in `Setup/` are from before the name fix and still carry
   `BallisticCaculator`. A fresh run wipes them and writes the corrected names in the right formats.
   Nothing else regenerates them.
2. **Check the live manual renders.** Every link and anchor is verified as *files*, but not as rendered
   pages. The one to look at is the new symptom table in `installation.md`'s macOS section — a table
   inside a `###` subsection between two fenced code blocks is the shape Kramdown is most likely to
   surprise us on.

### Judgement calls waiting on you

3. **`ACOG TA31@M4` sight height** — I changed it from 2.8 in to the **2.4 in** the `ACOG-TA31` ladder was
   computed for. It is a ballistic input; revert if 2.8 was deliberate.
4. **`VUDU-SR3`** is a 75 gr load at 2900 ft/s and no shipped entry fits. Its sheet now names the
   designation load rather than pointing at XM855. Worth confirming that is the treatment you want for
   the four other "none shipped" rows too.
5. **The four remaining stub articles** — the summary view, saving and exporting, the units reference,
   file formats. None is load-bearing for a release the way the model-scope article was; the manual
   already says plainly which entries are unwritten. Decide whether they block.

### Known and not blocking

6. **The reticle line-ending inconsistency is gone** — subsumed by the CRLF renormalisation.
7. **F-2 is still a guard, not validation.** A calculation failure reports cleanly rather than crashing,
   which is enough.
8. **Not notarised on macOS**, so Finder refuses the launcher by double-click; Terminal is unaffected.
   Signing would need an Apple Developer ID, a `.app` bundle and a notarisation step — deliberately not
   done, and the manual says what to do instead.

## Two things worth not relearning

- **Ask which machine before diagnosing.** An hour went into a macOS failure that was an Intel Mac
  recorded as an M4 in the IP list. `uname -m` first.
- **Verify the harness, not just the result.** Four false conclusions on 2026-07-30 came from the test
  around the thing rather than the thing: a stale `cd` scoping `git ls-files` and `find` to the wrong
  directory, `%errorlevel%` expanding before the batch file ran, `set "VAR=x" &&` not surviving WSL's
  argument translation, and a bash pipeline reporting `sed`'s exit code instead of `cmd.exe`'s. When a
  result contradicts what the code plainly says, suspect the measurement.
