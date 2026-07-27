# Known defects

Found by manual testing. Newest first.

Both entries below were **fixed on 2026-07-27** and are kept as the record of what went wrong.

---

## D-002 — .drg editors: the readings/knots grid has no vertical scroll bar

- **Found:** 2026-07-27 (manual run of the desktop app)
- **Status:** FIXED 2026-07-27
- **Area:**
  - `Common/BallisticCalculator.Panels/Panels/DrgFromVelocitiesPanel.axaml` — `ReadingsGrid`
  - `Common/BallisticCalculator.Panels/Panels/DrgFromBcPanel.axaml` — `KnotsGrid`

**Steps to reproduce**

1. `Tools → Approximate .drg from velocities`.
2. Add more readings than fit in the grid's fixed `Height="170"`.

**Expected:** the grid shows a vertical scroll bar so all rows can be reached.

**Actual:** no vertical scroll bar appears; rows past the visible height are not reachable
by scrolling.

**Also affects** `Tools → Approximate .drg from BC` (`KnotsGrid`) — reported as "probably",
and the markup confirms it: both grids are declared identically, fixed `Height="170"` with
no `ScrollViewer.VerticalScrollBarVisibility` set.

**Cause and fix**

Both grids set their own `Height="170"` while sitting in a `StackPanel` inside the dialog's
`ScrollViewer`. A `StackPanel` measures its children with infinite height, so the `DataGrid`
decided it had room for every row and its `Auto` scroll bar never appeared — the giveaway in
`doc/screenshots/custom_drg.png` is that the two `*` columns span the full width, i.e. no space
was reserved for a bar.

The trajectory table (`TrajectoryTableControl.axaml`) has always scrolled correctly because it
sets **no** `Height` of its own — its container bounds it. The fix does the same here: the grid
lost its `Height`, and a `Border Height="170"` host provides it.

Regression cover: `ReadingsGrid_Height_IsOwnedByTheHostNotTheGrid` and
`KnotsGrid_Height_IsOwnedByTheHostNotTheGrid`.

**Not reproducible headlessly** — under `Avalonia.Headless` the self-constrained grid *did* get a
working scroll bar, so the fix rests on the layout reasoning above and on matching the control that
demonstrably works. Worth a glance in the running app.

---

## D-001 — Windows menu: the first entry does not activate its window

- **Found:** 2026-07-27 (manual run of the desktop app)
- **Status:** FIXED 2026-07-27
- **Area:** `Desktop/BallisticCalculator/Views/MainWindow.axaml.cs` — the dynamic `Windows` menu
  items, and now `Views/WindowActivation.cs`

**Steps to reproduce**

1. Open three (or more) child windows so the `Windows` menu lists `_1 …`, `_2 …`, `_3 …`.
2. Activate some other window (so entry 1 is not the current one).
3. Open the `Windows` menu and click the **first** entry.

**Expected:** the first window comes to the front and becomes active (as entries 2 and 3 do).

**Actual:** nothing happens — the first window is not brought forward or activated.
The second and third entries work correctly.

**Cause and fix**

Nothing positional after all, and nothing wrong with the menu: `ManagedWindow.Activate()` returns
without doing anything in two states, and the handler was a bare `w.Activate()`.

1. **Minimized** — an explicit early return. Selecting a minimized window did nothing at all.
2. **Already `IsActive`** — also an early return. A window can be active yet *buried*, because
   raising another window's z-order (maximizing another child, for instance) does not transfer the
   active state. Selecting the buried window then did nothing, which is the reported symptom.

`Views/WindowActivation.cs` now decides what a window needs — restore if minimized, then either
`Activate()` or, when it is already active, an explicit `BringToTop()` — and `MainWindow.BringToFront`
applies it. The same two dead ends were hit by **Cascade** and by **View → Compare → Add** re-using an
existing Compare window; both now go through the same helper.

Verified against the library source: activation itself is sound, and each entry activates its own
window. Cover: `WindowActivationTests` (the decision, case by case) and `WindowsMenuTests` (the menu,
end to end).
