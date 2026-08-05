# Windows Pinboard — handover notes

How the work has been run, what was decided, and what to be careful with.
**There is no README** — this file is the only orientation there is.

Written 5 August 2026 **from the repository and git history only — the
conversation transcript for this project is gone.** Nothing here comes from a
chat log, so the reasoning is inferred from commits and code. Anything not
established from an artifact is marked *(uncertain)*.

---

## 1. What it is

A **sidebar app for Windows** — a pinboard docked to the edge of the screen that
collapses to a handle and expands on demand.

C# / **WPF**, with an MVVM-ish layout: `Views/`, `ViewModels/`, `Models/`,
`Services/`, `Native/`, plus `Assets/` and `Tools/`. `WindowsPinboard.csproj`.

Repo `https://github.com/JNoles405/windows-pinboard.git`, branch `main`,
5 commits, 30 April – 8 May 2026.

Consistent with the owner's other Windows work (Shoebox, Speakeasy): **a native
desktop app, no browser engine.**

---

## 2. This app lives or dies on Win32 interop

`Native/NativeMethods.cs` plus a service layer that is almost entirely
OS integration:

| Service | What it does |
|---|---|
| `AppBarService` | registers the window as a Windows **AppBar** so it docks and reserves screen edge |
| `GameModeService` | detects fullscreen apps and pauses the sidebar |
| `HotkeyService` / `HotkeyFormatter` | global hotkeys |
| `MonitorService` | multi-monitor placement |
| `TrayService` | the tray icon |
| `AutostartService` | start with Windows |
| `StorageService` | persistence |

**This is the interesting and fragile part.** AppBar registration, global
hotkeys and window styles are all process-wide OS state — if the app crashes
without unregistering, Windows can be left reserving screen space for a program
that is no longer running. Anything you change here should be tested by killing
the process, not just closing it.

---

## 3. The click-through problem — two commits, and the second is the real fix

- *"Don't block scrollbars: make collapsed handle fully click-through"*
- *"Toggle **`WS_EX_TRANSPARENT`** for OS-level click-through when collapsed"*

A sidebar that sits on top of other windows steals mouse input from whatever is
underneath — most visibly the scrollbar of the window it overlaps. Doing this at
the WPF hit-test level was not enough; **the fix is toggling the
`WS_EX_TRANSPARENT` extended window style so the OS routes clicks past the
window entirely.**

The toggle matters: transparent when collapsed, not transparent when expanded.
**If you make the handle interactive, you must clear that style, or the handle
will not receive its own clicks.** That is the trap waiting in this code.

---

## 4. Game mode

*"Add automatic game mode (pauses sidebar during fullscreen apps)"* — an
always-on-top sidebar over a fullscreen game is at best distracting and at worst
causes the game to drop out of exclusive fullscreen. Detecting fullscreen and
standing down is the right behavior; keep it.

---

## 5. Assets

A single commit adds the app icon *"used for exe, shortcuts, windows, and tray"*
— one icon, four consumers. If you replace it, replace all four; a stale tray
icon is easy to miss.

---

## 6. Building

```bash
dotnet build
dotnet run
```

*(Uncertain)* what .NET version is targeted — check `WindowsPinboard.csproj`.
There is no publish script, unlike Shoebox (`build.ps1`) and Speakeasy
(`build/publish.ps1`); **adding one would match the house pattern** and produce
a self-contained exe.

`bin/` and `obj/` are present in the working directory — confirm they are
gitignored before adding anything.

---

## 7. Outstanding

- **Write a README.** This is the only project of the set with neither a README
  nor a transcript; without this file it would be unrecoverable.
- **No publish/installer path** (§6).
- *(Uncertain)* what actually goes on the pinboard — notes, links, files,
  clipboard items? The commit history describes the *window* in detail and the
  *content* not at all. Read `Models/` and `ViewModels/` before assuming.
- *(Uncertain)* whether it has been used beyond development.
- No tests, no CI.

---

## 8. Keeping this file current

Updated in the same commit as the change it describes. Anything that changes how
this is built or run, adds a trap, settles a decision, or moves an outstanding
item belongs here — **and any change to window styles, AppBar registration or
hotkeys belongs here**, because that is process-wide OS state and the failures
are non-obvious. Resolve an *(uncertain)* marker by deleting it.
