# Plan: two `.drg` generator dialogs (Approximate Drag Table)

> **Status 2026-07-26 — implemented.** All of §1–§4 is built and green (130 new tests, 710 total):
> `CsvTextTableReader`, `MeasurementTextParser`, `DragTableBuilder`/`DrgMetadata` in
> `BallisticCalculator.Types`; `DrgFromBcPanel` and `DrgFromVelocitiesPanel` in
> `BallisticCalculator.Panels` with `ApproximateDrgFromBcDialog` / `ApproximateDrgFromVelocitiesDialog`
> shells and the Tools → Approximate Drag Table menu. The four sample CSVs are committed to
> `Panels.Tests/TestData/` so no test depends on a path outside the repo. Remaining: interactive smoke
> pass (the app launches; the GUI itself was not driven), and the deliberately-broken sample files.
>
> Two implementation notes worth keeping:
> - **`TextChanged` and `MeasurementControl.Changed` do not fire for programmatic values in headless
>   Avalonia** (an existing comment in `MeasurementControlTests` says as much). The BC panel therefore
>   watches `TextBox.TextProperty` changes, and the velocities panel commits the detail pane into its row
>   at every point that consumes the rows (add, delete, selection change, import, build) rather than
>   trusting an event — which also fixes a real-app case, since a unit switch raises no text change.
> - The reader's ambiguity rule needed widening: under a `,` separator, *more than two fields* is
>   ambiguous, because `100,780,2` is either two values with a decimal comma or three columns. Taking the
>   first two fields silently read it as `(100, 780)`.

Design for **2026-07-26**. Implements Feature 2 of [`07-25-plan.md`](07-25-plan.md) with the decisions
confirmed below. Two *separate* editors, each a list+detail table editor with CSV import, each producing
a `DrgDragTable` saved to a `.drg` file.

## Confirmed decisions

| Question | Decision |
|---|---|
| BC knot X column | **Velocity or Mach only** — no distance→Mach solve. Rows stored canonically as Mach. |
| Table editing UI | **List + detail pane**, mirroring `SightListEditorDialog` (ListBox + `MeasurementControl` detail). |
| Output | **Save `.drg` only.** No ammo-library write-back, no "apply to active shot". The user picks the file up with the Ammunition panel's existing `Browse...` button. |

## Verified API (BallisticCalculator 1.1.11.1, by reflection)

```
DrgDragTableFactory.Build(AmmunitionLibraryEntry ammunition, DragTableId baseTable, IEnumerable<BcAtMach> bcCurve) -> DrgDragTable
    new BcAtMach(double mach, double bc)                  // knots are Mach-keyed; ascending; bc > 0
Tools.RadarDragTableFactory.Create(IEnumerable<RadarReading> readings, Measurement<WeightUnit> bulletWeight,
    Measurement<DistanceUnit> bulletDiameter, Atmosphere atmosphere = null, string name = null) -> DrgDragTable
    new RadarReading(Measurement<DistanceUnit> distance, Measurement<VelocityUnit> velocity)
DrgDragTable.Save(string fileName, Encoding encoding = null)   // also Save(Stream)
DrgDragTable.Ammunition { get; set; }  -> AmmunitionLibraryEntry (metadata carried in the file)
Atmosphere.SoundVelocity -> Measurement<VelocityUnit>          // used for velocity <-> Mach
```

Notes that shape the design:
- `Build` derives the curve from **`baseTable` + `bcCurve` only**; the `AmmunitionLibraryEntry` is
  metadata (name/caliber/weight…) written into the file. `baseTable` must be standard — `GC` throws.
- `Create` genuinely needs weight, diameter and the **atmosphere the readings were measured in**
  (density drives the Cd recovery), so that dialog has a real atmosphere input; the BC dialog does not
  (published BC-vs-velocity data is referenced to standard conditions → `new Atmosphere()`).

---

## 0. Real sample files (`D:\xep\bullets`) — what the importer must actually swallow

```
mbc1.csv                    velocity1.csv / velocity2.csv
mach;bc                     distance;velocity
1.5;0.462G7                 0yd;3078.800ft/s
1.75;0.463G7                100yd;3001.2ft/s
2;0.470G7                   ...            <- no trailing EOL on the last line
```

State as of 2026-07-26: `fps` was rewritten to `ft/s` in both velocity files and the `1400 d` typo on
line 16 of `velocity2.csv` was corrected, so all four files now parse with the plain library parsers
(5 / 8 knots, 16 / 16 readings, header line skipped). **Deliberately broken files come separately** —
the user is authoring a dedicated set for the failure paths in §0b; the importer must not assume the
happy path just because the sample files are clean now.

Observed and verified against the library:
- Separator **`;`**, a **header line**, **CRLF** endings, no trailing newline on the last line.
- **Values carry their units inline** — `0yd`, `3078.800fps` — so the importer is *text*-based, not
  double-based. `Measurement<DistanceUnit>.TryParse("0yd")` works; `Measurement<VelocityUnit>.TryParse`
  **rejects `fps`** (the library's names are `m/s, km/h, ft/s, mi/h, kt, in/s, cm/s, ft/min`) → an
  **alias map is mandatory**: `fps|f/s→ft/s`, `mps→m/s`, `mph→mi/h`, `kph|kmh→km/h`, `yds→yd`, `meter(s)→m`.
- The BC column carries the **drag table id**: `BallisticCoefficient.TryParse("0.462G7")` → 0.462 / G7.
  A bare `0.462` **fails** that parse and needs the base-table combo as fallback. mbc1 = G7, mbc2 = G1,
  so a clean import can *set* the base-table combo instead of asking.
- ⚠ `Measurement<VelocityUnit>.TryParse(invariant, "780,2m/s")` silently returns **7802 m/s** — a local
  decimal comma must be normalized to `.` **before** the library parse, never passed through.
- `1400 d` (the former line 16 of velocity2.csv, since corrected) is the shape of malformed row that must
  **reject the whole file** with that line quoted — see §0b.

These four files become the importer's test fixtures (embedded as string arrays plus one temp-file
round-trip, so the tests don't depend on `D:\`).

## 0b. Unparseable input — all or nothing

**Decision (2026-07-26, user):** an import either takes the whole file or takes nothing. There is no
partial import, no per-row skipping, no "imported 15 of 17". Rationale: a drag curve silently missing a
knot is worse than a refused file — the user cannot see what is absent, and the resulting table looks
plausible while being wrong.

**What is tolerated**
- **Empty / whitespace-only lines** — skipped anywhere in the file, not counted, not reported.
- **An optional header line, and only as line 1.** The real files open with `mach;bc` /
  `distance;velocity`, but the header is not required: an unparseable *first* line is taken as a header,
  and a file whose first line already parses as data is read from line 1. An unparseable line anywhere
  else is a hard error, even if it looks like a header. (Nothing else is tolerated — a `#` comment on
  line 7 rejects the file.)
- Surrounding quotes on a field (`"100"`) are stripped before parsing — a formatting artefact, not content.

**What rejects the whole file** — report `<file>: line 16 "1400 d;1643.2ft/s" — 'first' is not a valid
distance. Nothing was imported.` in red, and **leave the current list exactly as it was**:
- any non-empty line past line 1 whose two fields don't both parse;
- fewer than two fields on such a line;
- an ambiguous field: `780,2` in a `,`-separated file could be one value or two, so it is refused, never
  read as 7802;
- no data rows at all. Beyond that the minimum is **domain-specific, not the reader's business**: the BC
  dialog accepts a single knot (the library allows it — one knot means a constant BC scaling), the
  velocities dialog needs **three** readings (`RadarDragTableFactory` throws below that). The reader
  requires ≥1 row; each parser enforces its own minimum with its own message;
- I/O and access errors, and non-text input (NUL bytes in the first 4 KB → binary);
- more than 50 000 data lines — refused with a size message rather than truncated, since truncation is
  itself a silent partial import.
- Encoding: UTF-8 with BOM detection, falling back to the system default when UTF-8 decoding yields
  replacement characters. The BOM must never stay glued to the first field (it would break line 1's parse
  and get mistaken for a header).

**Column order — default assumed, header may override.**
- **No header ⇒ the documented default order**: `mach;bc` for the BC dialog, `distance;velocity` for the
  velocities dialog. This is stated in each dialog next to the Import button so it isn't folklore.
- **A header, when it names the columns, decides the order** — so `bc;mach` and `velocity;distance` import
  correctly instead of silently transposed. Roles are matched case-insensitively by keyword against the
  header's two fields: `mach`; `bc` / `coefficient`; `dist` / `range`; `vel` / `speed` / `mv`. A match on
  either field is enough to fix the order (the other column is the remaining role).
- A decorative header that matches nothing (`"data";"values"`) is dropped and the **default order**
  applies — a header we can't read is not a reason to refuse the file.
- A header where both fields resolve to the **same** role (`mach;mach`) is a hard error: the file is
  rejected rather than read under a guess.
- Consequence: no column-picker UI is needed for two-column files, and a transposed export is a
  one-line-of-header fix on the user's side rather than a re-export.

**Separator choice under an all-or-nothing rule.** Detection is still a guess, so each candidate (`;`,
tab, `,`) is tried and a candidate is accepted only if **every** data line parses under it. If none does,
the file is rejected using the failure from the candidate that got furthest, so the message points at real
content rather than at a mis-split line 1.

**Consequences for the dialogs**
- The row list is only ever replaced after a completely successful read — so a rejected file cannot
  disturb hand-typed rows, and picking a `.drg` or a trajectory export by mistake is harmless.
- Since the message names one line, it fits the status line; no tooltip overflow list is needed.

**Build level stays separate.** Rows that parse but are physically wrong (duplicate X, non-monotonic
distance, BC ≤ 0, rising velocity) are legal input and are caught by `DragTableBuilder` at *Save* time,
naming the offending value — those are editable in place, unlike a malformed file.

**Tests** (synthetic fixtures inline, plus the user's broken-file set once it exists): all four real files
accepted whole; header-only file rejected (<2 rows); one bad line at 2 / middle / last rejects the file
and imports nothing; `#` comment mid-file rejects; empty lines interleaved anywhere are ignored; `1400 d`
rejects; `0.462` where a table id is expected rejects; `780,2` in a `,` file rejects as ambiguous; a file
whose real separator is the second candidate is accepted; LF / CRLF / no-final-EOL / UTF-8 BOM accepted;
binary blob, absent and locked paths, 50 001 lines rejected; and — as its own assertion — that every
rejection leaves a previously populated list unchanged.

## 0c. `.drg` metadata — ✅ RESOLVED in BallisticCalculator 1.1.11.2 (2026-07-26)

**Shipped and verified.** The library fix is on the MyGet feed and this repo is bumped:
`1.1.11.1 → 1.1.11.2` in all five `.csproj` files (Controls, Panels, Types, Desktop app, ReticleEditor);
solution builds clean and all 580 existing tests pass.

What landed, checked by round-trip against the package:
- **`Save` writes all six header fields** — `CFM,220gr .308 test,0.0142557602,0.0078232,0.0311404,Litz BC curve`
  (was `…,0,0`), and **`Open` reads length and source back** into `BulletLength` / `Source` instead of the
  old fixed `"drg file"`.
- **The radar path is covered differently than proposed** — rather than a public setter or an entry
  overload, `RadarDragTableFactory.Create` gained two optional parameters:
  `Create(readings, bulletWeight, bulletDiameter, atmosphere = null, name = null, bulletLength = null, source = null)`.
  `DrgDragTable.Ammunition` **remains `private set`**, which no longer matters. Verified: a radar table
  created with `bulletLength: 1.215in, source: "LabRadar 2026-07-20"` saves and reopens with both intact.
- Commas in `Name`/`Source` are sanitized to spaces rather than corrupting the header (checked with
  `"name,with,commas"`), so the dialogs need no input restriction on those fields.

Both dialogs can therefore persist the full metadata set from day one, and `DragTableBuilder`'s
`DrgMetadata` maps straight onto the API with no pass-through caveat.

**Reading side, done (2026-07-26):** `AmmoPanel.OnBrowseCustomTable` now copies **bullet length** as well
as weight and diameter out of an opened `.drg`, so a table saved with metadata fills the ammunition in one
step — and spin drift (which needs diameter *and* length) works without retyping. Only **positive** values
are copied: files written before 1.1.11.2 store the unused slots as `0`, and overwriting a good field with
zero would silently disable spin drift. Two tests in `AmmoPanelTests` cover the 6-field and the legacy
`…,0,0` header (211 panel tests green).

**One wart worth a line in the next library session** (not blocking, no app impact today): when `Source`
is null, `Save` writes the literal `0` into field 6, and `Open` then reports `Source == "0"` rather than
null/"drg file". Writing an empty field — or treating `"0"` as absent on read — would round-trip cleanly.
Observed values: header `…,0,0` → `Source "0"`, `BulletLength` null (correctly); a 4-field header →
`Source "drg file"`; an empty 6th field → `"drg file"`.

Fields with **no slot in the format** — caliber, ammunition type, barrel length, muzzle velocity — remain
deliberately uncollected.

<details><summary>Original gap analysis (kept for context)</summary>

The dialogs collect name / source / weight / diameter / length, but before 1.1.11.2 only three survived:

Current state in `BallisticCalculator/Drag/DrgDragTable.cs`:
```csharp
// Save (line ~138) — fields 5 and 6 hardcoded to zero, dropping length and source:
w.WriteLine(string.Format(CultureInfo.InvariantCulture, "CFM,{0},{1:R},{2:R},0,0", name, weightKg, diameterM));

// Open / ReadHeader (line ~85) — requires >= 4 fields and never looks at 5 or 6:
if (parts.Length < 4) throw new ArgumentException("The first line of stream must have at least 4 values");
```
The format itself carries both — the repo's own fixture `BallisticCalculator.Test/resources/sierra_168_brl.drg`
opens with `BRL, 308 Sierra 168gr. (McCoy), 0.01089, 0.00782, 0.03114, Radar Data`, i.e. field 5 = bullet
length in metres (0.03114 m ≈ 1.226 in, a 168 gr MatchKing) and field 6 = source text.

Needed:
1. **`Save`** — write field 5 from `Ammunition?.Ammunition?.BulletLength` (metres, `0` when unset) and
   field 6 from `Ammunition?.Source` (comma-stripped, `0` or empty when unset) instead of `0,0`.
2. **`Open`/`ReadHeader`** — when fields 5/6 are present and non-`0`, populate `BulletLength` and
   `Source` (instead of the current fixed `Source = "drg file"`); keep the ≥4-field requirement so
   existing files still load.
3. **A way for a caller to supply metadata on the radar path.** `DrgDragTable.Ammunition` is
   `{ get; private set; }` and `RadarDragTableFactory.Create` hardcodes `Source = "radar data"` with no
   length — so today the velocities dialog *cannot* set source or length at all, and fixing `Save` alone
   would not deliver them. Either make the setter public, or add a
   `Create(..., AmmunitionLibraryEntry metadata)` overload. **Without this, item 1 only helps the BC
   dialog** (which passes its own entry to `DrgDragTableFactory.Build`).
4. Publish, then bump `BallisticCalculator` in this repo.

</details>

### Why the `fps` alias lives here and not in Gehtsoft.Measurements

`VelocityUnit.FeetPerSecond` is `[Unit("ft/s", 1)]` while its neighbours already carry aliases
(`[Unit("km/h", "kmph", 1)]`, `[Unit("mi/h", "mph", 1)]`) — the mechanism exists, `ft/s` just never got
one, and `UnitAttribute.AlternativeName` holds a single alias per unit. Adding `fps` there would be a
one-line fix. **Decision (2026-07-26): leave the library alone** — handle aliases app-side in
`MeasurementTextParser`, no cross-repo change and no publish/bump cycle. (The app pins
`Gehtsoft.Measurements 1.1.17` directly; the library repo sits at 1.1.18 unreleased.)

Unrelated non-bug found while checking: `TryParse(invariant, "780,2m/s")` → 7802 m/s because the parse
allows `NumberStyles.AllowThousands` and `,` is the invariant *group* separator. That's correct library
behaviour, and exactly why the reader normalizes the decimal mark before the library ever sees the text.
Whitespace before the unit (`2695.9 ft/s`) already parses, so the parser doesn't need to strip it.

## 1. Shared, testable pieces (`Common/BallisticCalculator.Types/`) — build first

### 1a. `CsvTextTableReader.cs` — pure CSV → two raw text columns

The counterpart of the existing `CsvExportController` (which already distinguishes *Local (for Excel)*
from *Invariant (portable)*), so an exported file round-trips and radar/chrono exports in either
convention load without the user picking a format. It splits and reports; **it does not interpret** —
unit/BC parsing belongs to the two typed parsers in §1b.

```csharp
public sealed record CsvTextRow(string First, string Second, int LineNumber);
public sealed record CsvTextTable(IReadOnlyList<CsvTextRow> Rows, char Separator,
                                  string? HeaderFirst, string? HeaderSecond);   // header nulls when absent

public static class CsvTextTableReader
{
    /// <param name="isUsableRow">
    /// Decides whether a split row's two fields both parse — supplied by the caller because unit and BC
    /// knowledge lives in the typed parsers (§1b), not here. Drives the header decision (line 1 unusable
    /// ⇒ header) and the separator choice (a candidate is accepted only if EVERY data line is usable).
    /// </param>
    /// <returns>
    /// false with a UI-ready <paramref name="error"/> naming the offending line, and no table — the
    /// caller then leaves its current data untouched. All-or-nothing: there is no partial result.
    /// </returns>
    public static bool TryRead(IEnumerable<string> lines, Func<string, string, bool> isUsableRow,
                               out CsvTextTable table, out string error);

    /// <summary>As <see cref="TryRead"/>, plus I/O, binary-content and encoding rejection. Handles BOM,
    /// CRLF/LF and a missing final EOL.</summary>
    public static bool TryReadFile(string path, Func<string, string, bool> isUsableRow,
                                   out CsvTextTable table, out string error);
}
```

Splitting responsibility this way keeps the reader free of `Measurement`/`BallisticCoefficient` knowledge
(so it stays trivially testable with a `(a, b) => double.TryParse(a, …)` predicate) while it owns the
mechanical decisions — separator, header presence, line accounting. It returns the header **text** rather
than interpreting it: mapping those names to roles (and thus the column order, §0b) needs to know what the
columns mean, so it belongs to the typed parsers in §1b, one per dialog.

Mechanical rules (auto-detection, no format prompt; acceptance/rejection per §0b):
- **Separator**: candidates `;`, tab, `,` in that order; the winner is the first under which *every* data
  line is usable. No winner ⇒ reject, reporting the failure from the candidate that got furthest.
- **Decimal separator**: per field. If the separator is not `,` and the field contains `,`, the comma is
  the decimal mark and is rewritten to `.` (`780,2` → `780.2`) before anything else sees it. Under a
  `,` separator such a field is ambiguous and rejects the file. Thousands separators are not accepted.
- Fields are trimmed and unquoted; empty/whitespace-only lines dropped silently. `#` is **not** a comment
  marker — such a line rejects the file like any other unparseable line.
- Takes the **first two fields** of each row; a row with fewer than two fields rejects the file. Wider
  exports (LabRadar &c.) therefore load only if their extra columns are trimmed first — a column mapper
  stays out of scope, and the rejection message says which line had how many fields.

### 1b. `MeasurementTextParser.cs` — tolerant unit-suffixed value parsing

```csharp
public static class MeasurementTextParser
{
    public static bool TryParseDistance(string text, DistanceUnit fallbackUnit, out Measurement<DistanceUnit> value);
    public static bool TryParseVelocity(string text, VelocityUnit fallbackUnit, out Measurement<VelocityUnit> value);
    public static bool TryParseBc(string text, DragTableId fallbackTable, out BallisticCoefficient value);
    public static bool TryParseDouble(string text, out double value);          // Mach, bare BC
}
```
- Normalizes the decimal mark, strips inner whitespace between number and unit (`2695.9 fps`), applies
  the **alias map** above, then defers to the library `TryParse`.
- A bare number (no suffix) is accepted and takes `fallbackUnit` / `fallbackTable` — that's what the
  dialogs' unit combos are for.
- `TryParseBc` prefers `BallisticCoefficient.TryParse` (keeping the file's own `G7`/`G1`), falling back
  to bare-number + `fallbackTable`.

### 1b. `DragTableBuilder.cs` — thin wrapper over both factories

```csharp
/// <summary>The metadata both dialogs collect — exactly the fields the .drg header can carry (§0c).</summary>
public sealed record DrgMetadata(string Name, string? Source,
    Measurement<WeightUnit> Weight, Measurement<DistanceUnit> Diameter,
    Measurement<DistanceUnit>? Length);

public static class DragTableBuilder
{
    public static double VelocityToMach(Measurement<VelocityUnit> velocity, Atmosphere? atmosphere = null);
    public static Measurement<VelocityUnit> MachToVelocity(double mach, VelocityUnit unit, Atmosphere? atmosphere = null);

    public static DrgDragTable FromBcCurve(DrgMetadata metadata, DragTableId baseTable, IEnumerable<BcAtMach> curve);

    public static DrgDragTable FromRadarReadings(DrgMetadata metadata, IEnumerable<RadarReading> readings,
        Atmosphere? atmosphere = null);
}
```
- Sorts knots/readings ascending and validates **before** calling the library, so the dialog shows a clean
  message instead of a library exception. Verified against the library's own rules:
  - BC curve: **≥1 knot** (one knot = a constant BC scaling — the library permits it), every BC > 0,
    Mach > 0, no duplicate Mach, `baseTable != GC`.
  - Radar: **≥3 readings** (`RadarDragTableFactory` throws below three), velocity **strictly decreasing**
    with distance, no duplicate distances, weight > 0, diameter > 0.
  Throws `ArgumentException` with UI-ready text naming the offending value.
- `FromBcCurve` assembles the `AmmunitionLibraryEntry` itself (name, source, weight, diameter, length, and
  an `Ammunition` with BC `1.0 GC` so the file documents how it must be used).
- `FromRadarReadings` passes the whole `DrgMetadata` through to `RadarDragTableFactory.Create`'s
  `name` / `bulletLength` / `source` parameters (1.1.11.2, §0c) — no post-build mutation needed.
- Saved files should land in `DataFolders.Drg` so the existing `CustomDragTableLoader` resolves them by
  bare file name from an ammunition's `CustomTableFileName`; its cache is keyed on path + last-write time,
  so re-saving the same name is picked up rather than served stale.

---

## 1d. Where the editor UI lives — panel + thin shell

There is **no `BallisticCalculator.Types.Tests` and no desktop test project**: Types-layer classes are
tested from the existing suites (`CustomDragTableLoaderTests` and `BallisticDictionaryTests` sit in
`Panels.Tests`, `ShotTrajectoryCalculatorTests` in `Controls.Tests`), and **no dialog under
`Desktop/BallisticCalculator/Views/Dialogs/` is covered by any test today.**

So, to keep TDD real without standing up a new desktop test project, each editor is built as a **panel**
in `Common/BallisticCalculator.Panels/Panels/` — `DrgFromBcPanel`, `DrgFromVelocitiesPanel` — holding all
of the UI and behaviour, wrapped by a **thin `Window` shell** in `Desktop/BallisticCalculator/Views/Dialogs/`
that only hosts the panel, supplies `IFileDialogService`, and closes. The panels are then testable headless
in `Panels.Tests` exactly like `RiflePanel` is, and the shells stay too trivial to need tests.

Layouts below are the panel contents; "dialog" in §2/§3 means panel + shell.

## 2. Editor A — `DrgFromBcPanel` in `ApproximateDrgFromBcDialog` (Tools → Approximate Drag Table → From _BC Curve…)

`Panels/DrgFromBcPanel.axaml(.cs)` with `MeasurementSystem`, `IFileDialogService` and an optional
`Ammunition` prefill as settable properties (the panel pattern), shell
`Views/Dialogs/ApproximateDrgFromBcDialog.axaml(.cs)`.

**Layout** (`DockPanel`, 700×520, mirroring `SightListEditorDialog`):

```
Name:    [ 220gr .308 Custom          ]  Source: [ Litz BC curve        ]
Bullet:  Weight [ 220 gr ]  Diameter [ 0.308 in ]  Length [ 1.226 in ]
Base table:  [ G7 ▾ ]     Knots by: [ Mach ▾ ] [ ft/s ▾ ]
┌── knots ────────────┬── selected knot ─────────────┐
│ M 1.20   BC 0.307   │  Mach:  [ 1.20   ]           │
│ M 1.65   BC 0.301   │  BC:    [ 0.307  ]           │
│ M 2.25   BC 0.318   │                              │
├─────────────────────┴──────────────────────────────┤
│ [Add] [Delete]              [Import CSV...]        │
└────────────────────────────────────────────────────┘
CSV without a header is read as mach;bc.
Status: 3 knots, Mach 1.20–2.25.
                      [ Save .drg... ]  [ Close ]
```

- **Row model** `BcKnotEditModel` (in `Models/DragTableEditModels.cs`, `INotifyPropertyChanged` like
  `SightEditModel`): stores **`Mach` + `Bc` canonically**; `Display` recomputed for the list.
- **Knots by** = `Mach` | `Velocity`+unit combo. Switching only changes *display and entry* — canonical
  Mach is preserved, so toggling back and forth is lossless. Conversion via
  `DragTableBuilder.VelocityToMach` at standard atmosphere; a gray note states that.
- Detail entry is **plain `TextBox`** (culture-parsed like `MeasurementController` does) for both
  columns — Mach and BC are dimensionless, and the velocity unit already lives in the header combo.
- Base-table combo lists G1, G2, G5, G6, G7, G8, GI, GS, RA4 (GC excluded), default **G7**.
- **Metadata** (§0c) — `Name` and `Source` `TextBox`es plus `Weight` / `Diameter` / `Length`
  `MeasurementControl`s, prefilled from the active trajectory window's ammunition where available,
  `Source` defaulting to "BC curve". These are written into the `.drg` header and **do not affect the
  curve** — the note says so, since `DrgDragTableFactory.Build` derives it from the base table and knots
  alone. `Name` is the only required field (it also seeds the save file name); weight/diameter/length are
  optional here (unlike the velocities dialog, where the factory needs them). Caliber, ammunition type,
  barrel length and muzzle velocity are deliberately absent — the format has no slot for them.
- **Import CSV** — `OpenFileAsync` (filter `csv`, `txt`) → `CsvTextTableReader.ReadFile` → per row:
  `First` = Mach or velocity per the current *Knots by* mode (`MeasurementTextParser.TryParseVelocity`
  with the combo's unit as fallback, or `TryParseDouble` for Mach), `Second` =
  `MeasurementTextParser.TryParseBc`. **Replaces** the list.
  - If every row's BC carried the **same table id** (`0.462G7` → G7, as in `mbc1.csv`/`mbc2.csv`), the
    base-table combo is set from the file and the status says so. If the ids disagree, keep the combo and
    warn — a single `.drg` has one base curve.
  - A rejected file changes nothing (§0b); the status quotes the offending line in red.
  - Status on success: `Imported 5 knots from mbc1.csv, base table G7 from file.`
- **Save .drg…** — validate → `DragTableBuilder.FromBcCurve` → `SaveFileAsync` (`DefaultExtension`
  `drg`, filter `drg`, `InitialDirectory = DataFolders.Drg`, `InitialFileName` from the table name) →
  `table.Save(path)` → status "Saved <path>". Validation/exception text goes to the status line in red;
  the dialog stays open. **Close** just closes (`Close(true)` after a successful save so the caller can
  tell, though `MainWindow` ignores it).

## 3. Dialog B — `ApproximateDrgFromVelocitiesDialog` (Tools → … → From Measured _Velocities…)

Same skeleton, ctor `(MeasurementSystem system, IFileDialogService fileDialogService,
Ammunition? ammoPrefill = null, Atmosphere? atmospherePrefill = null)`.

```
Name:    [ 6.5CM 140gr radar        ]  Source: [ LabRadar 2026-07-20  ]
Bullet:  Weight [ 140 gr ]  Diameter [ 0.264 in ]  Length [ 1.35 in ]
                            (weight and diameter required — they drive the Cd recovery)
┌── measurement conditions ──────────────────────────┐
│  <panels:AtmospherePanel>  (reused as-is)          │
└────────────────────────────────────────────────────┘
┌── readings ─────────┬── selected reading ──────────┐
│  0 m     850.0 m/s  │  Distance [ 100  ][ m   ▾ ]  │
│  100 m   780.2 m/s  │  Velocity [ 780.2][ m/s ▾ ]  │
│  200 m   714.9 m/s  │                              │
├─────────────────────┴──────────────────────────────┤
│ [Add] [Delete]   CSV units: [m ▾][m/s ▾] [Import CSV...] │
└────────────────────────────────────────────────────┘
Status: 3 readings, 0–200 m, 850.0→714.9 m/s.
                      [ Save .drg... ]  [ Close ]
```

- Row model `RadarReadingEditModel` holds real `Measurement<DistanceUnit>` / `Measurement<VelocityUnit>`;
  detail pane uses two `MeasurementControl`s (units per row as entered, as elsewhere in the app).
- **Atmosphere**: reuse the shared `AtmospherePanel` (`Common/BallisticCalculator.Panels`) rather than
  re-creating four inputs; prefilled from the active window, else `new Atmosphere()`.
- **CSV units** combos (defaulted from `MeasurementSystem`: m + m/s metric, yd + ft/s imperial) are only
  the **fallback for bare numbers** — the real files (`velocity1.csv`) carry `0yd;3078.800fps`, and the
  inline unit always wins over the combo.
- **Metadata** (§0c): `Name` + `Source` (defaulting to "radar data") `TextBox`es; `Weight` and `Diameter`
  are **required** here — unlike the BC dialog they feed the computation, not just the header — and
  `Length` is header-only. Note in the plan and in the dialog: `Source`/`Length` reach the file only once
  library item 3 lands, because `RadarDragTableFactory` hardcodes the source and
  `DrgDragTable.Ammunition` is private-set.
- A file without a header is read as `distance;velocity`; a header may reverse the columns (§0b).
- Status on success: `Imported 16 readings from velocity1.csv, 0–1500 yd, 3078.8→1994.6 ft/s.`
  A rejected file leaves the list untouched and quotes the offending line.
- Save path identical to Dialog A but through `DragTableBuilder.FromRadarReadings`, and validation adds
  the strictly-decreasing-velocity and ≥3-readings rules.

## 4. Menu wiring — `Desktop/BallisticCalculator/Views/MainWindow.axaml(.cs)`

The `_Tools` menu already exists (Edit Sights / Edit Barrels). Append:

```xml
<Separator/>
<MenuItem Header="_Approximate Drag Table">
  <MenuItem Header="From _BC Curve..."            x:Name="MenuToolsDrgFromBc" />
  <MenuItem Header="From Measured _Velocities..." x:Name="MenuToolsDrgFromVelocities" />
</MenuItem>
```
Handlers next to `MenuToolsEditSights` in `SetupMenuHandlers()`, following `ShowDictionaryEditor`:
units and prefills from `_activeChild as ITrajectoryChildWindow` when present, `_fileDialogService`
passed in. Both items are **always enabled** (standalone generators) → `UpdateMenus()` untouched.

## 5. Tests (TDD — write these first)

`Common/BallisticCalculator.Types.Tests/`
- **`CsvTextTableReaderTests`** — accepted: the four real fixtures verbatim (`mach;bc` + `1.5;0.462G7`;
  `distance;velocity` + `0yd;3078.800ft/s`), header present and absent, header naming reversed columns,
  CRLF / LF / no-final-EOL, UTF-8 BOM, `;` / `,` / tab, a file whose real separator is the second
  candidate, local `1,5;0,307` → 1.5/0.307 (**not** 15/0307), interleaved empty lines.
  Rejected whole (and `table` untouched): one bad line at position 2 / middle / last, `#` comment
  mid-file, one-column line, `780,2` under a `,` separator (ambiguous), empty input, binary blob,
  absent/locked path, 50 001 lines.
- **`MeasurementTextParserTests`** — `3078.800fps`, `2695.9 fps`, `1554ft/s`, `2333` (fallback unit),
  `0yd`, `100 yd`; `780,2m/s` → 780.2 m/s (regression against the library's silent 7802); `1400 d` →
  false; `0.462G7` → 0.462/G7, bare `0.480` → fallback table, `F1GC` → form factor.
- **`DragTableBuilderTests`** — `VelocityToMach` at standard atmosphere (≈ v/340.3 m/s) and round-trip
  with `MachToVelocity`; `FromBcCurve` returns a `GC` table carrying the metadata (name, source, weight,
  diameter, length on the entry), accepts unsorted knots and a **single** knot, and throws for zero knots,
  duplicate Mach, BC ≤ 0, `baseTable == GC`; `FromRadarReadings` returns a `GC` table for a realistic
  decay series and throws for **fewer than three** readings, non-decreasing velocity, duplicate distance,
  zero weight, zero diameter. Plus a `Save`/`Open` temp-file round-trip asserting name/weight/diameter
  survive — extended to length and source once library item 3 (§0c) lands, which is the test that proves
  the bump worked.

`Desktop/BallisticCalculator.Tests/` (per the `avalonia-ui-tests` skill; headless)
- Both dialogs construct and populate from prefills; Add/Delete keep list and detail in sync;
  Mach↔velocity mode switch preserves values; import from an injected line list fills the list and the
  status text; **a rejected import leaves an already-populated list unchanged and shows the offending
  line**; `Save` with an invalid table shows the error and does not call the file dialog (fake
  `IFileDialogService`); `Save` with a valid table writes a readable `.drg` to a temp path with the
  metadata in its header.

## 6. Verification

1. `dotnet build BallisticCalculator2.sln -c Debug` clean.
2. New + existing suites green.
3. Smoke via `App.bat`: both dialogs open from Tools; each of the four `D:\xep\bullets` files imports
   whole (base table auto-set to G7/G1 from the mbc files); the user's deliberately-broken files are each
   refused with the offending line quoted and the previously entered rows intact; a hand-typed curve also
   saves. Each saved `.drg` loads through the Ammunition panel's `Browse...` and yields a trajectory
   (BC set to `1.0 GC`).
4. After the library work in §0c is published and bumped: re-run the round-trip test and confirm the saved
   header carries length and source (`CFM,<name>,<kg>,<m>,<lengthM>,<source>`) rather than `…,0,0`.

## Out of scope (note, don't build)

Cd-vs-Mach preview chart; CSV column mapper; distance-keyed BC input; writing the generated table into
the ammo library or the active shot.
