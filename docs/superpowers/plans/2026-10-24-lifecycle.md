# Dual Discord lifecycle and release controls

Existing WPF shell, native NotifyIcon, Windows job-object support, EventWaitHandle single-instance commands. Settings remain in customization.json; defaults: CloseAction=Tray, HideTaskbar=false, HideTray=false, KeepSecondRunning=true. Existing profile/sign-in files preserved. No statistics/reporting/server work.

- [ ] Add tests for lifecycle defaults/settings persistence, close decisions, signed policy verification and mandatory/disabled states; observe failures first.
- [ ] Add WindowLifecycle and ManagedSecond components. Explicit app exit remains reachable even when taskbar/tray icons are hidden; reopening the shortcut restores the manager. Desktop shortcut opens second directly, starting manager too in managed mode.
- [ ] Add signed GitHub launch policy, local verified cache, background periodic checks and inline block notice. Offline failures retain verified cached state and never invent a disabled instruction. Policy does not terminate existing sessions or modify/delete Discord data. About/Files/settings/update recovery remain reachable.
- [ ] Private admin signing tool in D:\Codex\Dual Discord Admin; no private keys/admin credentials in public source or packages. Default policy enabled. Publisher can set minimum version or disable launcher.
- [ ] Align caption hitboxes and footer rows. About has a lightly raised background and top separator. Common bottom baseline for About/version/creator.
- [ ] Build, all tests, native UI fixtures, isolated tray/hidden-instance/managed-child tests. Publish/install verified 2.3 update; preserve actual Discord PIDs and existing choices. Document admin use and limitations.
