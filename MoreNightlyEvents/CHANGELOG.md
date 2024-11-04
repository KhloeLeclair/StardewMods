# Changelog

## 0.4.0
Released November 4th, 2024.

*This version is compatible with Stardew Valley 1.6.9 **only**.* Please
use an earlier release for compatibility with earlier versions of
Stardew Valley.

### Fixed
* Compatibility with Stardew Valley 1.6.9.


## 0.3.0
Released June 6th, 2024.

### Added
* A `Message` type event that plays a sound and displays a message,
  like how `Placement` events work, but that doesn't actually place
  anything.

### Fixed
* Issue where some events caused crashes for farmhands in multiplayer
  due to missing constructors.
* Issue where side effects would not run for farmhands.
* Issue where an invalid item could be spawned and cause issues for
  saving the game. (Fixed in Stardew Valley 1.6.9, but this is a fix
  for previous versions of the game.)
* Remove some unnecessary log messages that were confusing users.


## 0.2.0
Released April 16th, 2024.

### Added
* The `mre_pick` command can be used to view what event would be
  selected tonight. Useful for testing conditions without actually
  going to bed to trigger an event.
* Placement events can now spawn buildings, optionally with animals.
* Placement events can now spawn crops.
* Placement event output entries can now accept a list of `SpawnAreas`,
  which are rectangles that limit the possible locations chosen to
  only be within the rectangles.
* Placement events have a new `RequireMinimumSpots` flag that will
  cause them to abort if they don't find `MinStack` valid locations.
* Events can now have a Priority, which changes how they're sorted
  in the event list. Events with a higher priority have a chance
  to happen first.
* Events can now be tagged as Exclusive. When determining which event
  should happen in a night, the first exclusive event to pass its
  conditions will be used. If no exclusive event passes, then all
  the remaining events with passing condition are selected between.
* Non-exclusive events now have a Weight on their condition, which
  can be used to adjust the likelyhood that that event is chosen
  when there are multiple matching events.

### Changed
* The placement event now displays an icon on screen when a sound is
  playing, like the vanilla SoundInTheNight event does.
* The sample content pack now has a 1% chance for any given event
  to happen, rather than 0%, so you don't *need* to trigger them
  manually. It's just as rare as a vanilla event.

### Fixed
* The farmer can now be used in `Script` events and will be visible
  and everything.


## 0.1.0
Released April 16th, 2024.

This is the initial release of More Nightly Events. Please look forward
to it, everyone! I look forward to seeing what the community comes up
with, and what we can create together.
