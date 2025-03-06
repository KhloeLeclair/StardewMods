# Changelog

## 0.5.1
Released March 6th, 2025 for Stardew Valley 1.6.15+

### Added
* Context menu when right-clicking a game tab, allowing a user to
  immediately switch the provider for a given tab or to reload the
  tab page. Requires developer options to be enabled.
* Setting to temporarily disable Better Game Menu from replacing
  the built-in Game Menu, for testing purposes.
* Ability to disable tabs entirely by setting them to Disabled
  within the settings menu.

### Changed
* When there are more than 11 tabs, start creating a second row
  of tabs. This still isn't production ready, but it should be
  by the first public release.
* Change a few log messages from debug to trace severity.

### Fixed
* Make sure to call `emergencyExit()` on the different vanilla
  menu pages when relevant to prevent held items being deleted.
* Clear the tool-tip when creating or destroying a page instance
  to ensure that developer tool-tips stay up to date.


## 0.5.0
Released March 2nd, 2025 for Stardew Valley 1.6.15+

This is the initial preview release of Better Game Menu, intended for
other developers to allow them to both discover issues and implement
support for Better Game Menu in their own mods.
