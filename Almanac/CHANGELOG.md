# Changelog

## 0.11.0
Released March 3rd, 2022.

### General

* Added settings to hide different sets of notices in the Local Notices
  page. So far, you can show or hide: Anniversaries, Gathering, and
  Festivals separately.
* Added festivals and anniversaries to the Local Notices page.
* Added support for scrolling the Almanac's tabs when there are more
  than nine tabs to display. This currently isn't important, but it
  will be in the future as other mods add their own content to
  Almanac (with any luck).
* Try to snap the cursor better when scrolling with a gamepad.
* Only display birthdays for NPCs we can socialize with on the
  Local Notices' tab, for now. There might be a better way to handle
  this in the future, but this seems to prevent spoilers from
  happening?
* When there are multiple notices on a day, and there is no NPC with
  a birthday on that day, display up to three separate notice icons
  before giving up and displaying a rotating icon.

### Bug Fixes

* Fixed an issue with the game menu overlay spamming errors when the
  menu does not have a components list.
* Fixed the Crop Previews setting not working to disable that feature.


## 0.10.1
Released March 3rd, 2022.

### General

* Make the PageUp, PageDown, Home, and End keys more useful when viewing
  long text on Almanac pages.

* Fix vertical centering of calendar labels.
* Fix a bug with the wrong cutscene flag being checked, resulting in being
  unable to open the Almanac after saving and loading.

### Translation

* Add initial Portuguese translation, thanks for Pedrowser on NexusMods!
* Add initial Russian translation, thanks to AveAcVale on GitHub!

### C# API Changes

* Nothing yet, but work is underway on an API for adding custom pages. I might
  end up waiting for SMAPI 3.14 for support for generics in API interfaces.


## 0.10.0
Released March 2nd, 2022.

### General

* Make the Almanac remember its previous state when you close and re-open it.
* Remember the scroll position when changing pages in the Almanac.
* Add hazelnut season to the Local Notices page.
* Stop recalculating page contents if the date changes but the season has not.
* Add more keyboard bindings for the Almanac menu.

### Translation

* Add support for translating events, both with normal translation keys and by
  supporting entirely different event.json files when more significant changes
  are required.

### C# API Changes

* Add methods for adding entries to the Fortune and Local Notices page.


## 0.9.0
Released March 1st, 2022.

* Initial release.
