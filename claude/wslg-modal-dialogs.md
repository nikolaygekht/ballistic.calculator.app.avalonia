# WSLg hides modal dialogs behind their owner — closed, do not re-investigate

Written **2026-07-29**, after two rounds of failed workarounds. Kept as its own note because it is a
platform finding rather than a defect, and because the next person to see the symptom will otherwise redo
this work. User-facing version: [`docs/known-problems.md`](../docs/known-problems.md).

## Symptom (WSLg only)

Clicking the main window while any modal dialog is open — the Shot Parameters dialog, `Edit Line`, the
unsaved-changes prompt — makes the dialog appear to vanish. It returns between half a second and ten seconds
later. Every modal dialog in both applications, not one of them.

## It is stacking, not hiding

Drag the dialog **clear of its owner** and click the owner: nothing happens. The dialog is being *covered*,
because WSLg's window manager does not honour the `WM_TRANSIENT_FOR` relationship Avalonia sets up. The
variable delay is the WM eventually re-raising it.

## It is not Avalonia's fault

Avalonia does not stack windows itself: it sets `WM_TRANSIENT_FOR` plus `_NET_WM_STATE_MODAL` and the WM
decides. A native Tk program — Xlib directly, no .NET — reproduces it exactly under WSLg and behaves
correctly elsewhere:

```python
python3 - <<'EOF'
import tkinter as tk
root = tk.Tk(); root.title("parent"); root.geometry("900x600")
def open_dialog():
    d = tk.Toplevel(root); d.geometry("460x220"); d.transient(root)   # the hint Avalonia sets
    tk.Label(d, text="Click the parent while I overlap it.", padx=20, pady=20).pack()
    tk.Button(d, text="Close", command=d.destroy).pack(pady=10)
    d.grab_set(); root.wait_window(d)                                 # modal
tk.Button(root, text="Open modal dialog", command=open_dialog).pack(pady=40)
root.mainloop()
EOF
```

The same application build is correct on **real Ubuntu 24.04** and on **Windows**.

## Two workarounds were tried, both reverted

1. **Clearing Avalonia's re-activation hook.** On X11, input to a disabled owner raises
   `IWindowImpl.GotInputWhenDisabled`; `Window.OnGotInputWhenDisabled` walks to the innermost modal child
   and calls `Activate()`. Neutralising it (reflection — the member is `internal`) changed **nothing**, which
   is what ruled the path out. Reverted: no reflection into framework internals for a workaround that does
   not work.
2. **`Topmost` on the dialog**, Linux-gated, asserted on `Opened` (X11 cannot set `_NET_WM_STATE_ABOVE` on
   an unmapped window, so setting it before showing is dropped). Under WSLg this was **worse**: the dialog
   stayed in front but rendered *empty*. Reverted, with the `ShowModal` extension it needed at 22 call
   sites.

If the symptom ever shows up on a real desktop, restore (2) and test it *there* — the empty rendering was
almost certainly WSLg's compositor rather than anything wrong with `Topmost`.

## The lesson worth keeping

Ask **which platform** before treating a visual oddity as a defect. Two rounds of work went into a
WSLg-only symptom, on a machine where the same build is fine natively. Nikolay's own hand-testing on
Windows, then WSLg, then real Ubuntu is what narrowed it; each of my code-side hypotheses was wrong.
