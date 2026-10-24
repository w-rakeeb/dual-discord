# Public lifetime and Dualcord tray implementation plan

Goal: public releases require the manager for Dualcord; the owner's personal build retains independent mode. Provide a dedicated Dualcord tray control without patching Discord.

Architecture: one WPF codebase, public by default, compile-time PERSONAL build only for local delivery. Customization enforces the effective lifetime at its property boundary so old or edited JSON cannot enable public independent mode. Personal builds refuse public-package installation. Existing Windows Job Object implementation handles lifetime. A second manager-owned NotifyIcon follows the second profile's live state; native Discord tray icons remain unchanged.

Tech stack: WPF/.NET Framework 4.8, Windows Forms NotifyIcon, existing compiler/scripts.

- [x] Add failing public/personal flavor tests, including legacy JSON, setter and reset behavior; run to establish failure.
- [x] Add BuildProfile, enforce lifetime through Customization, disable/explain the public setting, protect personal updates, label build flavor. Keep settings paths and data unchanged.
- [x] Add SeparateTray default and live Dualcord NotifyIcon with focus/quit/open-manager controls; preserve existing hide-tray preference and dispose resources.
- [x] Run both flavor tests, existing core/integration tests, native tray/lifetime fixtures and WPF layout checks.
- [x] Produce separate public package and personal build. Preserve installed configuration and live sessions, document and publish public release, deliver local personal build safely.

Limits: suspension does not kill existing sessions; managed lifetime does not prevent manually running Discord outside this utility. Windows can place tray icons in its overflow area. Discord's native tray icon may coexist with the dedicated Dualcord control.
