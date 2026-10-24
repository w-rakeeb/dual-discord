Fixed the marked footer clipping, sidebar divider and cramped Settings navigation.

- Credit now shows Wrakeeb and an unclipped GitHub mark. GitHub and update controls use matching 36-DIP hit targets and 18-DIP icons.
- About and version/update controls have separate footer columns and no overlapping hit areas.
- Sidebar divider runs through the title bar and footer. Compact mode hides the title text with sidebar labels, retaining the window's accessible app name.
- Settings tabs use balanced full-row widths, visible gaps, consistent heights and a right edge aligned with the cards. Labels fit at the minimum normal-text width and can wrap at larger text sizes. Redundant section labels and excess first-section gaps were removed.

124 automated checks passed (76 core, 14 edition, 7 integration, 27 native presentation), plus 204 WPF fixtures per edition. Existing business settings, process management, update controls and storage remain unchanged. Physical Windows10/cross-PC/system text-scale changes have not been tested in this revision.
