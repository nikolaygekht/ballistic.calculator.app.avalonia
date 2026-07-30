---
title: Known problems
nav_order: 80
---

# Known problems

**Goal of this article:** if the application does something odd, find out here whether it is known, whether
it is ours, and what to do about it — before spending time on it.

Each entry says what you see, why it happens, and what you can do. Where the cause is outside the
application, that is stated plainly rather than dressed up: knowing a problem is not yours to fix is worth
as much as a workaround.

---

## Under WSL, modal dialogs hide behind the main window

**What you see.** Running the Linux build inside **WSL** (WSLg), you open any modal dialog — the Shot
Parameters dialog, an *Edit …* dialog in the reticle editor, the unsaved-changes prompt — and click the main
window behind it. The dialog appears to vanish. It comes back on its own somewhere between half a second and
ten seconds later.

**What is actually happening.** The dialog is not vanishing, it is being **covered**. Drag it clear of the
main window, so nothing overlaps, and clicking the main window does nothing at all. WSLg's window manager
does not honour the "keep this dialog above its owner" relationship (`WM_TRANSIENT_FOR`) that the
application asks for, so the main window is raised over the dialog, and the window manager re-raises the
dialog later.

**This is not an application problem, or even an Avalonia one.** The same Linux build behaves correctly on a
real desktop — verified on Ubuntu 24.04 — and on Windows. More conclusively, a twelve-line native program
using no part of this application reproduces it exactly under WSLg:

```python
python3 - <<'EOF'
import tkinter as tk
root = tk.Tk(); root.title("parent"); root.geometry("900x600")
def open_dialog():
    d = tk.Toplevel(root); d.geometry("460x220"); d.transient(root)
    tk.Label(d, text="Click the parent while I overlap it.", padx=20, pady=20).pack()
    tk.Button(d, text="Close", command=d.destroy).pack(pady=10)
    d.grab_set(); root.wait_window(d)
tk.Button(root, text="Open modal dialog", command=open_dialog).pack(pady=40)
root.mainloop()
EOF
```

**What to do.** Keep the dialog clear of the main window while you work in it, or run the application on a
real Linux desktop, or on Windows. Two workarounds were tried in the application and both were removed —
one had no effect, and forcing dialogs to stay on top made WSLg render them empty, which is worse than a
dialog that is briefly covered.

## Under Linux, the Open and Save dialogs ignore the application font size

**What you see.** You have increased the font size, but the file dialogs — `Load…`, `Save`, `Browse…` —
still use the system's.

**Why.** Those are the operating system's own file dialogs, not the application's windows. On Windows they
follow Windows' settings; on Linux they follow the desktop's (GTK) settings. The application's font size
setting cannot reach into another program's dialog, and should not.

**What to do.** Change the font size in your desktop settings if the file dialogs are hard to read. Every
window the application draws itself does follow the application font size.

## The reticle editor has no undo

**What you see.** An element you edited or deleted cannot be brought back.

**Why.** By design, for now — the editor is deliberately small.

**What to do.** The editor does protect you from losing a *document*: `File → New`, `File → Open` and
closing the window all ask before discarding unsaved changes, and the title carries an asterisk while there
are any. Within a document, use `Save As` before an experiment so there is a file to go back to. See
[The reticle editor](reticle-editor.md).

## The table stops before the maximum range you asked for

This one is **not** a problem, and it is here because it is reported as one. The solver abandons a shot once
the bullet falls below 50 ft/s or has dropped more than 10,000 ft, so a load asked for more range than it
can carry simply ends where it ran out. Rows stop; nothing errors. See
[The Parameters tab](parameters-tab.md).

---

## Something not listed here

Please open an issue at
[github.com/nikolaygekht/ballistic.calculator.app.avalonia](https://github.com/nikolaygekht/ballistic.calculator.app.avalonia/issues),
saying which platform you are on — Windows, a real Linux desktop, or WSL. As the first entry above shows,
the platform is often the whole answer.

---

[← Contents](index.md)
