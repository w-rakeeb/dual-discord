# Settings tabs, alignment and release-control audit

Goal: clean three authorized desktop leftovers, display closed PID as zero, use an accessible GitHub icon link, trial top Settings tabs, align page headings/sidebar rows and check policy freshness before launches/manual update requests.

Preserve WPF, business settings/storage paths and public/personal edition rules. Move existing controls into General, Sharing, Storage, Updates and Appearance tabs; callbacks remain unchanged. Use native TabControl semantics with token styles. Baseline: Backups/Before-2.7-settings-tabs. Public distribution remains a complete portable ZIP.

- [x] Desktop cleanup: permanent deletion rejected by automatic review; safely archive exact resolved directories under Backups/Desktop-leftovers-20261025 instead.
- [x] Regression tests for PID zero and stale policy rechecks.
- [x] Icon credit, reusable SettingsSections and tab styles; heading/sidebar alignment.
- [x] Refresh stale launch policies; manual release checking also verifies availability. Publish no restriction.
- [x] Both editions, Settings categories, DPI/theme fixtures, public release/updater and installed behavior. Preserve live data/processes.
- [x] Deliver personal/public builds and document guarantees/rollback.
