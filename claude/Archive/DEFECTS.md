# Known defects

Found by manual testing. Newest first.

D-003 is **open**. D-001 and D-002 were **fixed on 2026-07-27** and are kept as the record of what went wrong.

---

## D-003 — Reticle tab: the display mode is shared by all trajectory windows

- **Found:** 2026-11-05 (recording video Episode 8, two trajectory windows open)
- **Status:** OPEN
- **Area:** `Common/BallisticCalculator.Panels/Panels/ReticlePanel.axaml` — the Display radios
  `RadioNone`, `RadioFarBdc`, `RadioNearBdc`, `RadioTarget`

**Steps to reproduce**

1. Open two trajectory windows (any shots).
2. In window 1, Reticle tab: load a reticle (e.g. `VCOG16-300BLK.reticle`), select **Far BDC**. The BDC
   labels appear.
3. In window 2, Reticle tab: load a reticle, select **Far BDC**.
4. Switch back to window 1.

**Expected:** window 1 still has Far BDC selected and still shows its BDC labels.

**Actual:** none of window 1's Display radios is selected (None, Far BDC, Near BDC, Target all off),
and its reticle shows no BDC labels. Confirmed with UI Automation on the running app: window 1 all
four off, window 2 Far BDC on.

**Likely cause:** all four radios carry `GroupName="DisplayMode"`. In Avalonia a named group is
scoped to the root (the main window), not to the `UserControl`, so every `ReticlePanel` in every MDI
trajectory window joins one group: checking a radio in one window unchecks the others everywhere.
The four radios are the only ones in their `StackPanel`, so dropping `GroupName` (grouping by parent)
or making it per-instance should fix it. Check the other panels for the same pattern.

**Workaround used in the video:** click Far BDC again after switching windows.

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
and the markup confirmed it: both grids were declared identically.

**Cause and fix**

**`Width="*"` columns.** A star column sizes itself from the grid's available width — the same
budget the vertical scroll bar has to come out of — and the bar loses: the columns take the lot and
no space is left to reserve. The giveaway is visible in `docs/screenshots/custom_drg.png`:
with 16 readings in a grid showing six, the two columns still span the full width, so nothing was
ever set aside for a bar.

`TrajectoryTableControl` has always scrolled correctly because its columns are **fixed pixel
widths**, which do not compete for that budget. Both editors now match it: fixed widths (150),
`CanUserResizeColumns`, and right-aligned cells via a `DataGridCell.rightAlign` style.

**Confirmed in the running app** on 2026-07-27 — the scroll bar appeared as soon as the velocities
editor's columns became fixed. Regression cover: `ReadingsGrid_Columns_AreNotStarSized`,
`KnotsGrid_Columns_AreNotStarSized`.

**Not reproducible headlessly** — under `Avalonia.Headless` the original grid reported a perfectly
visible scroll bar with a sized thumb, so this class of defect cannot be caught by a headless test;
it needs the real window.

**A wrong turn worth remembering:** the first attempt blamed height ownership (the grid setting its
own `Height="170"` inside a `StackPanel`, which measures with infinite height) and moved the height
to a `Border` host. That changed nothing and was reverted. The lesson is the one that eventually
solved it — diff against the control in the repo that already works, and change one difference at a
time.

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
