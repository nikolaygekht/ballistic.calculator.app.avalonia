---
title: Updating the application
nav_order: 3.5
---

# Updating the application

**Goal of this article:** know exactly what a new release replaces and what it leaves alone, so an
update never costs you work you have done.

There is no updater and no version check. You update the way you installed: take the newer archive from
the [Releases page](https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/releases) and unzip
it over the folder you are already using.

## The one rule

> **Everything inside `data` belongs to the release. Everything outside it belongs to you.**

The archive contains the binaries and a complete `data` folder. Unzipping over your installation
overwrites every file the archive contains and leaves every file it does not. So:

- **A shipped file you edited is lost**, because the archive has a file of that name.
- **A file of your own is kept**, because the archive has nothing to overwrite it with — *provided you
  unzip over the folder rather than deleting it first.* Delete `data` and then unpack, and your files
  go with it.

That second point is the one people get wrong, and it is worth being precise about: an over-unzip is a
merge, not a wipe. Your `my-scope.reticle` in `data/reticle` survives. Your tweak to the shipped
`MILDOT.reticle` does not.

## What that means per kind of file

| What | Where it lives | An update |
|---|---|---|
| **Sight and barrel presets** | `user-dictionaries.xml`, **beside the executable** | never touches it — see below |
| Window layout and last-used units | `appstate.json`, beside the executable | never touches it |
| **Reticles** | `data/reticle/*.reticle` | **replaces the shipped ones**; your own files stay |
| **Drag tables** | `data/drg/*.drg` | **replaces the shipped ones**; your own files stay |
| **Sample ammunition** | `data/ammo/*.ammox`, `*.ammo` | **replaces the shipped ones**; your own files stay |
| Shipped preset source | `data/dictionaries.xml` | replaced — but the app only ever reads it |
| Your saved shots | wherever you saved them | never touched |

**The trap is that the Save dialogs start in `data`.** The reticle editor's Save opens in
`data/reticle`, and drag tables and loads default to `data/drg` and `data/ammo`. That is
convenient — it puts your work where the application looks — and it also puts your work in the folder an
update rewrites. Your files survive an over-unzip, but nothing protects them if you ever delete the
folder to start clean, and nothing warns you if you happen to name a file the way we name one.

**The safe habit:** keep your own reticles, drag tables and loads in a folder of your own, outside the
installation. Both Open dialogs will happily browse there.

### A folder we have renamed stays behind

Because an over-unzip only ever *adds and replaces*, a shipped folder that a release **renames** is left
on disk under its old name. The sample ammunition folder was `data/legacy-ammo` and is now `data/ammo`,
so updating across that change leaves you with both: the new one, and the old one still holding the
previous release's copy of the same files.

Nothing is broken — the application reads `data/ammo`, and the stale folder is only wasted space. Delete
`data/legacy-ammo` once you have checked it holds nothing of yours. If you had saved your own loads into
it, they are still there, which is the reason it is not deleted for you.

## Presets are the exception, and are handled for you

Sight and barrel presets are entries in a file rather than files, so overwriting is not a workable
answer. They are split in two:

- **`data/dictionaries.xml`** — the presets the application ships with. Replaced by every update. The
  application only ever **reads** it.
- **`user-dictionaries.xml`**, beside the executable — **your** presets. The only file the two editors
  write, and no update contains a file of that name, so nothing can overwrite it.

On first run your file is created as a copy of the shipped one. After that, each start compares the two
and **adds any shipped preset your file has no entry of that name for**. So a new release's new presets
appear, and nothing you have done is disturbed.

Two consequences of that rule are worth knowing, because both look like bugs and neither is:

- **A corrected preset does not reach you.** The comparison is by name only, and it never modifies an
  entry you already have — that is what protects your edits. If a release fixes a shipped preset whose
  name you already carry, your copy stays as it is. **Reset** in the editor takes the new one.
- **A preset you delete comes back on the next start**, because "you deleted it" and "you have never
  seen it" are the same thing to a rule that only checks whether the name is present. Delete it again
  and it returns again.

### Reset

Both `Tools → Edit Sights…` and `Tools → Edit Barrels…` have a **Reset** button. It replaces *that*
list with the shipped presets and leaves the other one alone — resetting the sights does not touch your
barrels.

Reset only changes what is on screen. Nothing is written until you press **OK**, so **Cancel** undoes
it. That is the way back from a list that has drifted, and the way to pick up corrections to presets you
already have.

If a folder is read-only, the presets still load — they simply are not remembered, exactly as with
`appstate.json`. See [Where your settings go](installation.md#where-your-settings-go).

## Downgrading, and starting clean

- **To go back to an older release**, unzip the older archive over the folder. Your `appstate.json` and
  `user-dictionaries.xml` are not in the archive and carry over; if the older version does not
  understand something in them, delete the offending file and it will be recreated with defaults.
- **To reset the presets completely**, delete `user-dictionaries.xml` — the next start recreates it from
  the shipped file. This is the sledgehammer version of the per-editor **Reset** button.
- **To reset the window layout**, delete `appstate.json`.

## Next

[Ammunition library and presets](library-and-presets.md) — where loads, sights and barrels are kept, and
how to stop re-typing them.

---

[← Contents](index.md)
