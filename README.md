# Dual Discord

A small Windows app for running two separate Discord accounts side by side. Created by **Wrakeeb** · [GitHub](https://github.com/w-rakeeb).

## Download and start

1. Download **Dual-Discord-Windows.zip** from [Releases](https://github.com/w-rakeeb/dual-discord/releases).
2. Extract the whole ZIP into a writable folder. Keep the updater and Assets folder with the app.
3. Open **Dual Discord.exe**. It detects the normal stable Discord desktop installation or a running custom installation and prepares a separate second-profile folder.
4. Choose **Open** on Second Discord and sign in yourself. Your main login is never copied.

If Discord is not detected, open **Settings → Discord installation → Locate Discord** and select the installed **Discord.exe**. The selection is saved; later complete `app-*` versions in the same installation are detected automatically. Discord must already be installed. PTB, Canary and Microsoft Store variants are not supported in this release.

## Profiles and files

- Main uses your normal Discord data under Windows Roaming AppData.
- Second uses `%LOCALAPPDATA%\Discord Duo\Second Account\discord`. The older folder name is retained for compatibility with existing profiles.
- Private Vencord data lives under the manager's `Data\Vencord` folder. It starts empty on a fresh setup.
- Files shows the actual storage locations, including directory redirects. This app does not require a D: drive or a specific Windows username.
- The main card uses Discord's official symbol; the second uses the stacked Dual Discord app logo.

Local audio/graphics preference sync and Vencord sharing are optional. A fresh setup defaults to separate settings. Existing sharing and startup choices are retained. These features do not copy Discord login tokens, cookies or local storage. Account/server preferences stored by Discord follow the signed-in account.

X in the second Discord hides it by default. **Focus** restores it; **Quit** in this manager closes that selected profile fully. Quitting can disconnect its call. Closing the manager leaves Discord running. Each running profile uses its own RAM/CPU/GPU; shared installed files do not mean shared live memory.

## Updates

Open **About → Updates → Check for updates**. When a newer stable version is available, choose **Download and install**. Only the manager restarts; the updater does not close Discord.

The app uses the latest stable public release from this repository, requires GitHub's SHA-256 asset digest, validates the ZIP and replaces only allowlisted application files. It keeps an app-file backup under `Backups\App-update-*`. Settings, account data, Vencord folders, logs and local folder redirects are not replaced. A failed file transaction attempts rollback; see `Logs\last-update.txt` and the app backups if recovery is needed.

Offline requests, GitHub request limits, missing releases and invalid packages are reported as errors rather than as “up to date.” Updates are user initiated. No GitHub sign-in is needed to check or download public releases.

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
