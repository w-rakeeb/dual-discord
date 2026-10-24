# Dual Discord

## Version 2.4

The second profile is now named **Dualcord**, with a distinct monochrome rounded-square vector icon. The colorful stacked mark remains the manager logo. The new composition uses the Dashboard Icons symbol; the supplied Vecteezy link is only a style reference. Artwork is embedded in the executable and extracted locally, keeping the public ZIP compatible with existing updaters.

Dualcord shortcuts now start the manager quietly in the background in both lifetime modes. Its normal shortcut or tray menu opens the manager. An availability restriction or launch error can still show an explanation.

Separate Dualcord taskbar button is on by default. A Windows per-window AppUserModelID, icon and relaunch metadata distinguish second-profile windows while the manager runs. Turning this option off or fully exiting the manager restores the original window properties. Use the new Dualcord desktop shortcut for matching pin/relaunch behavior; Windows Explorer behavior can vary.

Move Dualcord files is higher in Settings and uses the existing verified copy/switch operation. Dualcord must be closed and the destination empty. The original remains as a recovery copy.

Reset app preferences asks for confirmation and creates a backup. It resets startup, sharing, appearance and window/lifetime preferences, while retaining profile/install paths, sign-ins, files and signed publisher policy. It does not reset account/server preferences inside Discord.

Keyboard focus outlines appear after keyboard navigation; mouse input and restoration hide them. Caption controls have no hover boxes, About has no separate box, and secondary controls use subtle tonal borders.

Warm launches reuse the existing profile/window and avoid duplicate process queries. Timings in the activity log measure launcher work, not Discord readiness. Plugin startup and experiment-cache warnings were observed; plugins, login data, GPU and network settings were not changed automatically. These changes do not guarantee a cure for every Discord/Vencord loading issue.

A small Windows app for running two separate Discord accounts side by side. Created by **Wrakeeb** · [GitHub](https://github.com/w-rakeeb).

## Download and start

1. Download **Dual-Discord-Windows.zip** from [Releases](https://github.com/w-rakeeb/dual-discord/releases).
2. Extract the whole ZIP into a writable folder. Keep the updater and Assets folder with the app.
3. Open **Dual Discord.exe**. It detects the normal stable Discord desktop installation or a running custom installation and prepares a separate second-profile folder.
4. Choose **Open** on Second Discord and sign in yourself. Your main login is never copied.

If Discord is not detected, open **Settings → Discord installation → Locate Discord** and select the installed **Discord.exe**. The selection is saved; later complete `app-*` versions in the same installation are detected automatically. Discord must already be installed. PTB, Canary and Microsoft Store variants are not supported in this release.

## Profiles and files

- Main uses your normal Discord data under Windows Roaming AppData.
- Fresh second profiles use `Discord installation\Profiles\Second\discord`. Existing second profiles retain their previous location, including `%LOCALAPPDATA%\Discord Duo\Second Account\discord`. Change it in Settings when the second profile is closed.
- Private Vencord data lives under the manager's `Data\Vencord` folder. It starts empty on a fresh setup.
- Files shows the actual storage locations, including directory redirects. This app does not require a D: drive or a specific Windows username.
- The main card uses the Discord symbol from [Dashboard Icons](https://dashboardicons.com/icons/discord); the second uses the stacked Dual Discord app logo.

Local audio/graphics preference sync and Vencord sharing are optional. A fresh setup defaults to separate settings. Existing sharing and startup choices are retained. These features do not copy Discord login tokens, cookies or local storage. Account/server preferences stored by Discord follow the signed-in account.

X in the second Discord hides it by default. **Focus** restores it; **Quit** in this manager closes that selected profile fully. Quitting can disconnect its call. By default X hides the manager to the tray; explicit Exit closes Dualcord in public builds. A personal build can retain independent mode. Each running profile uses its own RAM/CPU/GPU; shared installed files do not mean shared live memory.

## Updates

Open **About → Updates → Check for updates**. When a newer stable version is available, choose **Download and install**. Only the manager restarts; the updater does not close Discord.

The app uses the latest stable public release from this repository, requires GitHub's SHA-256 asset digest, validates the ZIP and replaces only allowlisted application files. It keeps an app-file backup under `Backups\App-update-*`. Settings, account data, Vencord folders, logs and local folder redirects are not replaced. A failed file transaction attempts rollback; see `Logs\last-update.txt` and the app backups if recovery is needed.

Offline requests, GitHub request limits, missing releases and invalid packages are reported as errors rather than as “up to date.” Checks can run automatically; downloads and installation require your click. No GitHub sign-in is needed to check or download public releases.

## Requirements and verification limits

Windows 10 or 11, .NET Framework 4.8, and an installed stable Discord desktop client. Extract to a folder you can write to; no administrator installer or service is required. Windows 11 rendering and local behavior are checked. Cross-PC/physical Windows 10, ARM/32-bit Windows and a real reboot are not yet verified. Future Discord client changes can affect profile isolation; please report regressions.

The app is an independent utility and is not affiliated with Discord. Discord branding belongs to Discord; see [Discord's brand guidelines](https://discord.com/branding). The code license does not grant rights to Discord trademarks.

## Share safely

Share the clean release ZIP. Do not share your working app folder: `Settings`, `Data`, `Logs`, `Backups` or profile folders may contain local paths or personal data. The published source and release package exclude those folders.

## Build and release

Run `Source\Build.ps1` in PowerShell on Windows with .NET Framework 4.8 available. Run `Tests\Run.ps1`, then `Source\Package.ps1`. Build uses native WPF and the Framework compiler; no Electron, WebView, Python or Node dependency is needed.

`Dual Discord.exe --ui-qa --qa-output=ABSOLUTE_PATH` renders isolated UI fixtures without initializing profiles. Fixture DPI/theme checks are not physical monitor/OS tests. The manual Release workflow validates the requested tag against the app version and publishes the clean binary ZIP and SHA-256 checksum.

## Removal

Turn off startup options in Settings, close the manager and keep a backup of your profile data before removing files. Deleting a manager shortcut does not uninstall Discord. No uninstall operation is performed by this app.

## Version 2.2: storage, sharing and update notices

- Logos use Dashboard Icons SVG artwork rendered as native vectors. The second profile retains the stacked Dual Discord identity.
- Settings → Second profile storage → Change folder copies the second profile to a new or empty folder, verifies file contents with SHA-256, then saves that location. Quit the second profile first. The original folder stays as a recovery copy, including its private account data; remove it only when you no longer need it. Copy failure keeps the old location selected, and may leave an incomplete destination to review.
- Fresh installations default to `Discord installation/Profiles/Second`. Existing installations keep their existing second-profile location. Private Vencord stays in the utility's own Data/Vencord folder and appears separately in Files.
- Customize sharing selects supported local audio or graphics preferences under the existing sync switch. These copy at second-profile launch, not continuously. Account/server settings remain managed by Discord.
- Optional CSS-only Vencord theme copying works with separate Vencord storage. Replaced CSS files are backed up under the private themes/Backups folder. Linked files, dependencies/assets and account/plugin settings are not copied; CSS remote imports retain their original behavior.
- Automatic update checks are enabled by default at manager startup and every six hours while it is open. A newer stable release displays an in-app foreground notice once per version per session. Later dismisses it; About always offers a manual check. Download/install still requires a click. Turn automatic checking off in Settings.
- Process details now includes the local profile's start time and running duration. It does not expose which Discord account is signed in and does not report remote usage.

Discord presence supports desktop/mobile/web categories. It does not distinguish a normal desktop session from a second profile launched by this utility. Server administration cannot reliably identify Dual Discord users through that status. No tokens, account identifiers, telemetry or remote admin dashboard are collected.

## Window, tray and lifetime controls (2.3)

X/Alt+F4 hides the manager to the tray by default. Settings can switch that to Minimize or Exit. Exit app in Settings or the tray menu always exits fully. Optional taskbar/tray hiding affects only the manager's icons. Reopen the normal manager shortcut to restore it even if both icons are hidden.

Public builds always bind Dualcord to the manager lifetime. The standalone setting is locked off, including when older settings request it. A locally compiled personal build (`Source/Build.ps1 -Personal -OutputDirectory <folder>`) retains the independent-mode option. Personal builds check public release availability but cannot install a public package over themselves. Hiding/minimizing keeps the manager running; actual exit closes the managed second profile, including on manager termination. Main Discord is never assigned to that process group. Normal app updates preserve the second profile across the manager restart. Existing calls in a managed second profile end if the manager actually exits, so choose the lifetime setting accordingly.

Use Add second Discord to desktop in Settings to create a Second Discord shortcut. It opens the second profile directly; managed mode also starts the manager in the background. The shortcut follows the installed manager path through app updates.

## Signed availability policy

The publisher can require a minimum version for critical updates or suspend new launches through this app. A signed policy is checked at startup and every 15 minutes, independently of optional update notifications. It does not send profile/account statistics. Restrictions show an explanation and retain access to Files, settings, Quit and update recovery. Existing Discord processes and files are not removed by a policy change. Offline/network errors use the last verified policy; a first-time offline install is allowed. Older pre-2.3 builds and modified clients are outside these controls; standalone Discord remains usable. This is an open-source launcher, not DRM or a remote uninstall service.

The signing/private administration tools are intentionally not distributed with the app.

### Dualcord tray control and matched branding

Separate Dualcord tray icon is enabled by default. While Dualcord runs, the manager adds a monochrome Dualcord icon with Open Dualcord, Quit Dualcord and Open Dual Discord actions. It disappears when the profile stops and respects Hide the tray icons. Windows may put it in tray overflow. This is an additional manager-owned control: Discord can still show its native tray icon; the utility does not patch or impersonate it. Both profile cards use identical visible icon canvas and symbol bounds.

Only public builds can be packaged by Source/Package.ps1. The personal build is not included in public releases. Signed availability and minimum-version rules apply to both editions; edition selection does not bypass these rules. Separate build editions are a distribution choice, not DRM against modified open-source clients.
