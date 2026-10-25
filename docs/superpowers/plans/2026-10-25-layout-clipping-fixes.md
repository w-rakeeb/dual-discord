# Footer clipping, sidebar divider and Settings rhythm

Goal: fix the screenshot-marked cut/overlap, replace the credit text with Wrakeeb, and create balanced spaced Settings navigation without altering business behavior.

Framework: existing native WPF. The explicitly supplied daisyUI skill and tab/footer/button references were read for composition guidance; CSS/Tailwind cannot style WPF directly. Do not add a browser/UI framework to fix native controls.

Findings: the 32-DIP GitHub button uses 16 DIP total padding plus a two-DIP border for a 16-DIP path, leaving only14 DIP for content. About occupies the whole footer while version tools overlay it. Divider starts at the body row below the title bar. TabPanel measures each header by content width without gaps.

- [x] Add measured regression checks for icon fit, short credit, footer separation, tab gaps/equal widths and full-height divider; observe old-build failure.
- [x] Use 36-DIP utility hit targets and 18-DIP icons, explicit footer columns, full-height noninteractive divider, balanced native Settings tabs and consistent section spacing. Keep colors in Theme.cs.
- [x] Verify both editions and all tabs/min-wide/DPI/theme fixtures, visually inspect the marked areas and test the installed app. Preserve current settings/processes.
- [x] Publish patch, install personal build, verify actual public upgrade and retain rollback baseline.
