# Known defects

Open defects found by manual testing. Newest first. Not fixed — logged only.

---

## D-002 — .drg editors: the readings/knots grid has no vertical scroll bar

- **Found:** 2026-07-27 (manual run of the desktop app)
- **Status:** OPEN — not investigated, not fixed
- **Area:**
  - `Common/BallisticCalculator.Panels/Panels/DrgFromVelocitiesPanel.axaml:55` — `ReadingsGrid`
  - `Common/BallisticCalculator.Panels/Panels/DrgFromBcPanel.axaml:65` — `KnotsGrid`

**Steps to reproduce**

1. `Tools → Approximate .drg from velocities`.
2. Add more readings than fit in the grid's fixed `Height="170"`.

**Expected:** the grid shows a vertical scroll bar so all rows can be reached.

**Actual:** no vertical scroll bar appears; rows past the visible height are not reachable
by scrolling.

**Also affects** `Tools → Approximate .drg from BC` (`KnotsGrid`) — reported as "probably",
and the markup confirms it: both grids are declared identically, fixed `Height="170"` with
no `ScrollViewer.VerticalScrollBarVisibility` set.

**Notes**

- Both dialogs wrap the whole panel in an outer
  `ScrollViewer VerticalScrollBarVisibility="Auto"`
  (`ApproximateDrgFromVelocitiesDialog.axaml:13`, `ApproximateDrgFromBcDialog.axaml:13`),
  with the comment "the grid inside keeps its own height". The inner `DataGrid`'s own
  scroll bar is the thing that is missing/not showing — likely interaction between the
  outer `ScrollViewer` and the `DataGrid`'s internal one, or a missing explicit
  `ScrollViewer.VerticalScrollBarVisibility="Visible"/"Auto"` on the grid. Unverified.

---

## D-001 — Windows menu: the first entry does not activate its window

- **Found:** 2026-07-27 (manual run of the desktop app)
- **Status:** OPEN — not investigated, not fixed
- **Area:** `Desktop/BallisticCalculator/Views/MainWindow.axaml.cs` — `UpdateWindowsMenu()`
  (`MainWindow.axaml.cs:264`), the dynamic `Windows` menu items

**Steps to reproduce**

1. Open three (or more) child windows so the `Windows` menu lists `_1 …`, `_2 …`, `_3 …`.
2. Activate some other window (so entry 1 is not the current one).
3. Open the `Windows` menu and click the **first** entry.

**Expected:** the first window comes to the front and becomes active (as entries 2 and 3 do).

**Actual:** nothing happens — the first window is not brought forward or activated.
The second and third entries work correctly.

**Notes**

- Reported as reproducible for the first entry only, regardless of which window that is —
  so it looks positional (index 0), not tied to a particular child window.
- Each item is wired as `item.Click += (_, _) => w.Activate();`, identical for every index,
  so the difference is likely outside the click handler itself (e.g. the header access key
  `_1`, item ordering relative to `MenuWindowsSeparator`, or the activation/`Deactivated`
  bookkeeping around `MainWindow.axaml.cs:162-175`). Unverified — needs investigation.
