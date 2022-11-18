← [README](README.md)

This document is intended to help mod authors create content packs using Theme Manager.

## Contents

* [Getting Started](#getting-started)
  * [Create a Content Pack](#create-a-content-pack)

## Getting Started

### Create a Content Pack

1. Install [SMAPI](https://smapi.io/) and [Theme Manager](https://www.nexusmods.com/stardewvalley/mods/14525)
   if you haven't yet. (If you haven't, how did you even find this?)
2. Create an empty folder in your `Stardew Valley\Mods` folder and name it
   `[TM] Your Mod's Name`. Replace `Your Mod's Name` with your mod's
   unique name, of course.
3. Create a `manifest.json` file inside the folder with this content:
   ```json
   {
	   "Name": "Your Mod's Name",
	   "Author": "Your Name",
	   "Version": "1.0.0",
	   "Description": "Something short about your mod.",
	   "UniqueId": "YourName.YourModName",
	   "ContentPackFor": {
		   // Do not change this when changing UniqueID
		   "UniqueID": "leclair.thememanager"
	   },
	   "UpdateKeys": [
		   // When you get ready to release your mod, you will populate
		   // this section as according to the instructions at:
		   // https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Update_checks
	   ]
   }
   ```
4. Change the `Name`, `Author`, `Description`, and `UniqueID` values to
   describe your mod. Later, don't forget to set your UpdateKeys before
   uploading your mod.
5. Create other files as described below, based on what you want your content
   pack to do with Theme Manager.


## Oops, I haven't written this yet!

Sorry. I'm still working on this. It should be out within a few days.
Until then, please check out:

* The [ThemeManager Example C# Mod](https://github.com/KhloeLeclair/StardewMods/tree/main/ThemeManagerExample)
  * Its [assets/themes/](https://github.com/KhloeLeclair/StardewMods/tree/main/ThemeManagerExample/assets/themes) folder
* The [Theme Template](https://github.com/KhloeLeclair/StardewMods/blob/main/ThemeManager/assets/themes/template/theme.json)
  for the base game.
* The [assets/patches/](https://github.com/KhloeLeclair/StardewMods/tree/main/ThemeManager/assets/patches)
  folder with all the currently available patches to use in themes for the base game.
* The [standalone Theme Manager's README](https://github.com/KhloeLeclair/Stardew-ThemeManager/blob/main-4/README.md)
  as the theme discovery information within and description of how it works is still
  relevant to this mod.
