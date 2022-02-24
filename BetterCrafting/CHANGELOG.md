# Changelog

## 0.9.1
Released February 23rd, 2022.

### General

* Add crafted counts to recipe tooltips.

### Mod Compatibility Fixes

* Add support for mods that expand the player's inventory beyond the default 3 rows.
  Works with Bigger Backpack, and should work with other mods.
* Add support for the Cooking Skill mod. This isn't heavily tested, and might not work
  for some cases. I'm trying to duplicate the behavior of the default CraftingPage when
  the mod is present.
* Attempt to register our menu with StackSplitRedux. This doesn't do anything yet. They'll
  need to add support for Better Crafting in that mod directly, or else expand their API
  so that we can register our child inventory menu and, preferrably, trigger the pop-up
  on demand so we can integrate it into crafting.
* Add support for chests that are in other maps. This is not heavily tested.
* Add stubs to our crafting menu so that Custom Crafting Stations no longer throws an
  exception in an event handler. Not sure how to handle limiting the available recipes. I
  need to find some content packs using CCS so I can experiment.

## 0.9.0
Released February 22nd, 2022.

* Initial release.
