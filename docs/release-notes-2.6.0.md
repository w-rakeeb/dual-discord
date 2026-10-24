Fixed tray menus and missed update checks, and added selective Vencord copying.

- Tray menus now match the app's dark/light palette and use high-contrast selection colors when needed.
- Added separate launch-time copy options for enabled Vencord plugins, plugin options, supported general preferences, theme choices and QuickCSS. Use separate Vencord storage; replaced files are backed up. Cloud authentication and recognized credential fields are excluded. Plugin binaries are not copied.
- Update checks run at startup, on reopening and every fifteen minutes. Failed requests retry after a minute; GitHub rate limits use a longer delay. Startup release/policy requests run independently, and local logs record check results.
- Signed availability checks run about every minute. In public builds, a verified suspension or required update stops managed Dualcord and blocks launches until resolved. Main Discord and files remain. Recovery/update controls stay available. Mandatory updates check for releases even with optional checks off. A new public install requires initial online verification.

107 automated checks passed (73 core, 14 edition, 7 integration, 13 native presentation), plus three actual manager startup/policy fixtures and 108 WPF render fixtures per edition. Ordinary and mandatory update notices were shown using production startup code in a private older-version fixture. Suspensions were tested using unpublished signed previews and disposable clients only.

No public suspension or minimum-version change was activated by this release. Older clients need this update to gain the new behavior. Offline or modified clients cannot be controlled instantly, and this utility does not remotely uninstall Discord or delete profiles. A recipient missing the notice can use About > Check for updates or extract the full ZIP over the existing app files after exiting the manager. Physical Windows 10/cross-PC/DPI changes were not tested in this revision.
