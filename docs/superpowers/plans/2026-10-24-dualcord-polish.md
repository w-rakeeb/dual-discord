# Dualcord polish and startup plan

Keep native WPF and established profile data. No statistics service.

- [x] Correct focus feedback: keyboard-only cues; transparent About item over existing footer; icon-only caption states; tonal secondary buttons consistent with expansion controls.
- [x] Distinct monochrome rounded-square vector Dualcord logo, PNG/ICO exports, profile/shortcut naming. Preserve manager branding.
- [x] Direct Dualcord shortcuts start/restore manager silently in both lifetime modes; avoid showing the manager before hiding. Existing normal manager shortcut restores it explicitly.
- [x] Reset preferences with confirmation and backup, requiring Dualcord closed; restore startup/sharing/theme/window defaults while preserving paths, accounts and signed policy. Surface existing verified relocation higher as Move Dualcord files.
- [x] Inspect bounded logs and startup timing. Optimize duplicate process queries/focus existing instance; optional reversible startup/cache repair only with evidence. Never copy account tokens or clear login data.
- [x] Separate per-window taskbar identity/icon for Dualcord where Windows supports it; verify on a signed-out isolated profile, leave main untouched.
- [x] Tests/build/fixtures and real silent-start/taskbar/reset checks; public release + verified local installation with original process/settings preservation.
