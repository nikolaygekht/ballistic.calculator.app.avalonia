# Shipping dictionary updates without eating user edits

**Status:** proposal, not started. 2026-07-30.

## The problem today

`BallisticDictionary.DefaultPath` is `data/dictionaries.xml`, and both editors write straight back to
it (`SightListEditorDialog.axaml.cs:171`, `BarrelListEditorDialog.axaml.cs:134` → `SaveDefault()`).
So a user's presets live in a shipped file, and `docs/installation.md:120` already warns what that
means: *"a fresh archive brings its own `data` folder and an overwrite can take yours with it."*
Unzipping an update over the folder destroys their presets.

## The suggestion, and the two cases it does not cover

> User works in `user-dictionaries.xml`. If it doesn't exist, create it from `dictionaries.xml`. At
> start, check `dictionaries.xml` and add any entries not in `user-dictionaries.xml` yet.

The split is right and the direction is right. But "add entries whose name is missing" is the only
rule, and it cannot express two things:

**1. It can never deliver a correction to an existing entry.** This is not hypothetical — it is the
change we made an hour ago. `ACOG TA-32@Carry Handle`, `ACOG TA-32@M4` and `Elcan Specter@M4` had
`default-zero="100yd"` where all four calibrated ladders are cut for **100 m**. The names already
exist in every user's file, so the fix reaches nobody who has run the app once. The first thing we
want to ship is precisely the thing the rule drops.

**2. Deleted entries come back every launch.** A user who deletes `Barret 82A1` because they don't own
one gets it re-added on next start, forever, with no way to stop it — "absent from the user file" and
"never seen by the user" are the same state.

Both gaps have one cause: with two files you can see *what is shipped* and *what the user has*, but
not *what changed*. You cannot tell a user's edit from a shipped update, or a deletion from a novelty.

## Recommended fix: keep a baseline, do a 3-way merge

Add a third file that is a **verbatim copy of the shipped dictionary as of the last merge**. That is
the missing "before" picture, and every case then resolves without guessing.

| File | Location | Role |
|---|---|---|
| `data/dictionaries.xml` | shipped, replaced by updates | *theirs* — what this release ships |
| `user-dictionaries.xml` | next to the exe | *mine* — the user's working set; the only file the editors write |
| `user-dictionaries.base.xml` | next to the exe | *base* — copy of the shipped file at last merge |

Merge per entry, keyed on `name`, for sights and barrels independently:

| shipped vs base | user vs base | Result |
|---|---|---|
| name is new | absent | **add** |
| unchanged | edited | keep the user's |
| unchanged | deleted | **stay deleted** — fixes gap 2 |
| **changed** | unchanged | **take the update** — fixes gap 1 |
| changed | edited | keep the user's (user wins; no prompt) |
| removed from shipped | unchanged | remove |
| removed from shipped | edited | keep the user's |

Then rewrite `user-dictionaries.base.xml` from the shipped file. First run has no user file and no
base: copy the shipped file to both and there is nothing to merge.

The baseline is a **plain copy**, so this needs no version number, no per-entry `origin` flag and no
hashes — the merge is `Load()` on three files and a comparison. That is the cheapest thing that is
actually correct, which is why I am not proposing the simpler two-file version.

## Where the files go

**Next to the executable**, like `appstate.json` (`AppStateManager.cs:16`). It matches the existing
precedent and keeps the app portable — `docs/installation.md` sells running it from "a USB stick",
which a `%LOCALAPPDATA%` path would quietly break. Not in `data/`, which updates overwrite.

The known cost, already documented for `appstate.json`: in a read-only install folder the merge cannot
persist, so the user's edits are not remembered. For window geometry that is cosmetic; for presets it
is worse. Proposed behaviour: if the user file cannot be written, fall back to reading the shipped
dictionary and let the editors report that they could not save. No new dialog.

## Code shape

- `BallisticDictionary`: rename `DefaultPath` → `ShippedPath`; add `UserPath`, `BaselinePath`.
  `SaveDefault()` → `SaveUser()` so the editors can no longer touch the shipped file.
- New `DictionaryMerge` in `Common/BallisticCalculator.Types/` — pure, UI-free, no file I/O in the
  core method:
  ```csharp
  public static MergeResult Merge(BallisticDictionary shipped,
                                  BallisticDictionary baseline,
                                  BallisticDictionary user);
  ```
  returning the merged dictionary plus counts of added / updated / removed / kept-because-edited, so a
  test can assert the table above row by row. Fits the project's pure-logic-plus-thin-shell pattern
  and is testable without Avalonia.
- Run the merge **once at startup**, not per panel: `RiflePanel.LoadDictionary()` is called on every
  construction (`RiflePanel.axaml.cs:181`) and must stay a cheap read of the user file. It already
  returns `Empty` when the file is missing, so tests that build a panel with no app around it keep
  working.
- Equality for "unchanged": compare the entry's fields, not the XML text, so reformatting or attribute
  reordering in a shipped file is not mistaken for a change.

## Decisions I need from you

1. **Entry identity is `name`.** A shipped *rename* therefore reads as delete + add. For an untouched
   user that is correct; for a user who edited the old entry it leaves a duplicate. That is fine and
   local — but it means the `ACOG TA-32` → `ACOG TA31` rename I flagged earlier should be done
   knowingly, or not at all. The alternative is a stable `id` attribute on every entry, which is more
   machinery than this deserves. **Recommend: keep names stable, no `id`.**
2. **Conflicts go to the user, silently.** If they edited an entry we later corrected, theirs survives
   and nothing is reported. The alternative is a "presets were updated" dialog at startup, which I'd
   rather not add. **Recommend: silent.**
3. **Reset to shipped defaults**: document "delete `user-dictionaries.xml`" rather than adding a
   button. **Recommend: document only.**
4. **Does the 100 m fix ship as a correction?** Under this design it reaches users who never edited
   those three sights, and does not touch users who did. Confirm that is what you want, because those
   users keep a zero that disagrees with all four calibrated ladders.

## Out of scope

The same problem exists for `data/reticle` and `data/drg` — an update overwrites the folder and takes
any user files in it. Reticles are whole files rather than entries, so the fix is different (a separate
user folder, both searched). Not addressed here; worth a line in the defects log.
