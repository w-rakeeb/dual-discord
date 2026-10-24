# Dual Discord flexibility implementation plan

Goal: preserve the compact native WPF UI and existing sign-ins while improving branding, update visibility, optional storage and safe sharing.

Architecture: separate customization JSON, profile storage transaction helper, allowlisted preference/theme copying, native vector logo component. Existing updater integrity checks remain intact. Existing second-profile locations are retained; fresh installs default to Installation/Profiles/Second. Relocation is copy/verify/switch with source retained, closed second profile required. Never copy main login data.

- [x] Tests first: defaults, legacy root retention, relocation copy integrity and path guards, preference key selection, CSS-only theme copying, notification policy.
- [x] ProfileStorage.cs and Customization.cs: atomic settings; empty destination validation; hash-verified, non-destructive relocation; independently selected audio/graphics preferences and optional CSS theme copies.
- [x] Engine.cs/App.cs: wire storage, sharing and auto update checks. Check at manager startup and every six hours; show foreground in-app notice for newer releases, install only on click; suppress repeats per session.
- [x] LogoAssets.cs/Assets: Dashboard Icons SVG, native WPF vector images in all UI branding, per-size ICO rendering.
- [x] AppShell.cs/ProcessOverview.cs: grouped settings, storage picker, compact update notice and local started/uptime details. No account surveillance or unsupported remote clone identification.
- [x] Full test/build/UI render checks; install with manager-only restart and preserve original Discord process IDs/settings. Package, document and release the verified version.

Scope decisions: no remote admin tracking, account token access, bot setup, or arbitrary file sharing. Existing preferences preserved. All profile move source data retained. The newer Discord Clone app is outside scope.
