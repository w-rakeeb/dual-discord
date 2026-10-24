Dualcord now requires Dual Discord to stay running in public builds. The standalone setting is locked off, including for older saved configurations. Hiding/minimizing the manager keeps Dualcord running; fully exiting closes Dualcord and may end its call. Main Discord is unaffected. Normal app updates preserve Dualcord across restart.

- Added a dedicated Dualcord tray icon with Open, Quit and Open manager actions. It follows profile state and can be disabled in Settings. Discord may still show its own native tray icon; Windows can place icons in overflow.
- Matched main Discord and Dualcord profile logos to identical visible canvas and symbol bounds.
- Added separate personal build support for the owner, retaining the standalone choice. Public packaging rejects personal binaries, and personal builds cannot install public packages over themselves.

Verified with 85 public build checks (56 core, 14 edition, 6 integration, 9 native presentation), personal behavior tests and 106 WPF render fixtures per edition. Physical Windows 10 and cross-PC behavior have not been retested for this release. No statistics service was added.
