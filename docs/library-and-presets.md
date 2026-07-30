---
title: Ammunition library and presets
nav_order: 24
---

# Ammunition library and presets

**Goal of this article:** stop re-typing. Three things can be saved and reused — loads, sights and barrels
— and they work in three different ways.

| What | Where it lives | How it is edited |
|---|---|---|
| **Loads** | One `.ammox` file per load, anywhere (`data/ammo` by default) | `Load` / `Save` on the Ammunition tab |
| **Sight presets** | Entries in `user-dictionaries.xml`, beside the executable | `Tools → Edit Sights…` |
| **Barrel presets** | The same file | `Tools → Edit Barrels…` |

## The ammunition library is a folder of files

There is no library window and no database: a saved load is a **file**, and the "library" is whichever
folder you keep them in. The application defaults to `data/ammo`, where the shipped sample
cartridges live.

**Saving.** Fill in the [Ammunition tab](ammunition-tab.md#entering-a-load-by-hand-and-keeping-it) and press
**Save**. You get a `.ammox` file named after the load, holding everything on the tab: the three required
numbers, the drag table, diameter and length, a reference to a `.drg` if you attached one, and the four
descriptive fields.

**Loading.** **Load** accepts two formats:

- **`.ammox`** — what this application writes.
- **`.ammo`** — the format of the original WinForms *Ballistic Calculator .NET*, whose sample library ships
  here. That is why there is something to try before you have saved anything yourself.

Loading replaces the whole tab and nothing else; the conditions, rifle and zero stay as you had them.

Three things worth knowing about a folder-of-files library:

- **Organise it yourself.** Subfolders work — by calibre, by rifle, by purpose. The dialog just opens where
  it last was.
- **Name files so you can find them.** The suggested file name is the load's name, which is one more reason
  to fill that field in.
- **Keep your own files out of `data`.** A new release brings its own `data` folder and an overwrite can
  take your saves with it, so a folder of your own beside it — or anywhere else — is safer. See
  [Updating the application](updating.md).

## Sight presets

`Tools → Edit Sights…` edits the named sights that fill the [Rifle tab](rifle-tab.md#presets-and-where-they-come-from)'s
dropdown. A list on the left, the selected entry's fields on the right, **Add**, **Delete** and **Reset**
beneath, and **OK** / **Cancel**.

**Reset** replaces the sight list with the presets the application ships with, leaving your barrels alone.
It only changes what is on screen — nothing is written until **OK**, so **Cancel** undoes it.

| Field | What it does |
|---|---|
| **Name** | What appears in the dropdown |
| **Sight Height** | Above the bore, centre to centre |
| **H Click** / **V Click** | Your turret's click size, in whatever angular unit the turret is marked in |
| **Default Zero** | Optional — and it **writes itself into the Zero tab** when the preset is chosen |

That last field is the one to be careful with, in both directions: it is genuinely useful (most rifles have
a habitual zero), and it silently **overwrites** the zero distance you may already have typed. If you build
presets, decide deliberately whether each one carries a default zero.

Worth building one preset per rifle you actually shoot, rather than per scope model: the sight height
depends on the mount, which is part of the rifle.

## Barrel presets

`Tools → Edit Barrels…` is the same shape — including its own **Reset**, which restores the shipped
barrels and leaves your sights alone — with two fields:

| Field | What it does |
|---|---|
| **Name** | What appears in the dropdown |
| **Twist Rate** | Distance for one full turn — 7 in, 200 mm |
| **Direction** | **Left** or **Right** |

Both matter: the rate scales spin drift, the direction decides which way it goes. See
[the twist](rifle-tab.md#the-barrel-what-the-twist-actually-buys).

## One file, and an update cannot touch it

Both editors read and write the **same** file, and saving from either writes the whole thing — sights and
barrels together. That file is **`user-dictionaries.xml`, beside the executable**, not the
`data/dictionaries.xml` that ships with the application. Three consequences:

- **It is plain XML and easy to read.** `<sight name="…" sight-height="2.6in" default-zero="100m"
  horizontal-click="0.1mil" vertical-click="0.1mil" />` — you can hand-edit or version it if you prefer.
  Units are written out, and any angular unit is accepted for clicks: the shipped file mixes `0.1mil`,
  `0.25moa` and `0.5in/100yd`.
- **An update cannot overwrite it.** No release archive contains a file of that name. `data/dictionaries.xml`
  *is* replaced by every update, but the application only ever reads it: on first run to create your copy,
  and afterwards to add presets your file does not have yet.
- **New shipped presets appear; your entries are never modified.** Matching is by name. The flip side is
  that a *corrected* shipped preset will not reach an entry you already carry, and a shipped preset you
  delete reappears on the next start. **Reset** in either editor replaces that one list with the shipped
  presets — nothing is saved until OK, so Cancel undoes it.

[Updating the application](updating.md) covers this and the rest of what a release replaces.

## Which dialog opens in which units

The two dictionary editors are **standalone** — they describe hardware, not a shot — so they open in the
units of the **last trajectory you created**, remembered between sessions and imperial until you have
created one. The Ammunition tab's Load and Save, by contrast, belong to whichever trajectory window you are
editing.

That is the same rule as everywhere else in the application, and it is described under
[imperial or metric](first-trajectory.md#first-imperial-or-metric).

## Next

The manual's remaining articles — hit probability and the reference part (units, file formats, what the
model includes, troubleshooting) — are listed in [all articles](index.md#all-articles) and still to be
written.

---

[← Contents](index.md)
