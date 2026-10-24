# Tray, Vencord copying and update reliability

Goal: match native tray menus to the app theme, copy selected Vencord settings independently, reliably notify of updates and enforce signed public availability restrictions.

Architecture: preserve WPF/Framework and separate public/personal builds. New TrayMenu factory/renderer uses Theme tokens and system high-contrast mapping. New VencordSharing service uses explicit category allowlists, preserves cloud/authentication and recognized credential fields, rejects linked descendants/oversized data, and backs up replaced files. Engine applies selections only when Dualcord is closed and Vencord storage is separate. UpdateSchedule handles reopen checks, retries and success separately. Signed policy polling changes to one minute; valid signed restrictions stop only managed public Dualcord, retaining recovery UI. No remote deletion, no published suspension or minimum-version change in this work.

Evidence to collect: failing regression for six-hour delay; selective settings and credential-preservation fixtures; native themed menu checks/screenshots; signed restriction and scoped shutdown integration; both editions, WPF DPI/theme fixtures, actual public ZIP upgrade and unchanged live user data/processes.

- [x] Write regression tests and establish failures.
- [x] Implement themed tray factory and native state tests.
- [x] Implement category-based Vencord copying with backups and sharing controls.
- [x] Implement startup/reopen/retry update checks, cache-bypassing requests and visible status.
- [x] Enforce signed restrictions on managed public Dualcord, one-minute policy checks and recovery actions.
- [x] Verify both builds, document limitations, publish public release and install personal build preserving live settings and sessions.

Limits: no exact cause can be proved for a recipient PC without its version/logs. Reopening already-running app previously did not schedule an update; failures waited six hours. GitHub/network/old-client/offline limits remain. Source-grounded Vencord categories follow https://github.com/Vendicated/Vencord/blob/main/src/api/Settings.ts. Tray rendering follows Microsoft ToolStripProfessionalRenderer APIs.
