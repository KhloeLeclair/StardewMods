# Changelog

## 0.5.7
Released March 15th, 2025 for Stardew Valley 1.6.15+

### Added
* When the user has too many tabs to fit in one row, a second row may
  be displayed. This row can be disabled in settings.

### Fixed
* Issue where tab components weren't moved correctly when scrolling due
  to pagination.
* Issue where components weren't hidden correctly due to invisibility.
* The easter egg button being added to `OptionsPage` instances other
  than the Options tab itself.


## 0.5.6
Released March 14th, 2025 for Stardew Valley 1.6.15+

### Added
* New API method for unregistering implementations.
* New API method for quickly checking if an `IClickableMenu` is an instance
  of a Better Game Menu.
* New API method for creating an instance of Better Game Menu that's open to
  a specific page, using the built-in `GameMenu`'s tab indexes.

### Changed
* Added an optional Vector2 offset parameter to the `CreateDraw` helper method.
* Implement single-row scrolling for tabs.
* Improve the timing information available in developer tool-tips.

### Fixed
* Pages not being resized properly when the ui viewport size changes due to
  a change of the UI scale.


## 0.5.5
Released March 9th, 2025 for Stardew Valley 1.6.15+

### Added
* New API event for adding custom tab context-menu entries.

### Fixed
* Event handler priorities not being sorted correctly when
  multiple mods have registered event handlers.


## 0.5.4
Released March 9th, 2025 for Stardew Valley 1.6.15+

### Added
* Ability to open Better Game Menu's config from the context menu
  when you right-click on the Options tab.
* Easter egg at the bottom of the Options menu.

### Changed
* Set the `Allow Hot Swapping` setting to be enabled by default
  since it is so potentially useful.
* In the Menu Providers tab of the settings menu, the "Automatic"
  entries now display the name of which provider will be used
  when Automatic is selected.
* Update to using the latest Generic Mod Config Menu.


## 0.5.3
Released March 8th, 2025 for Stardew Valley 1.6.15+

### Added
* Integration with the Star Control mod.
* API method to simply instantiate a new Better Game Menu instance
  without pushing it onto `Game1.activeClickableMenu`.

### Fixed
* When the menu is set to be invisible, set the tab components to
  hidden to ensure that snapping works as expected.
* When switching tabs, make sure the upper right close button's
  visibility is as expected.


## 0.5.2
Released March 7th, 2025 for Stardew Valley 1.6.15+

### Added
* API method for getting the currently active page of a menu
  without using the `IBetterGameMenu` interface, for the purpose
  of pairing down the API surface Pintail is responsible for.

### Fixed
* Use the child menu's size for drawing the background, in case it
  is constructed with a different size than we expect.


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
