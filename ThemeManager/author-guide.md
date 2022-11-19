← [README](README.md)

This document is intended to help mod authors create content packs using Theme Manager.

## Contents

* [Getting Started](#getting-started)
  * [What is a Theme?](#what-is-a-theme)
  * [What About Game Themes?](#what-about-game-themes)
* [Creating a Theme](#creating-a-theme)
  * [`theme.json` Basics](#theme.json-basics)
  * [Theme Discovery](#theme-discovery)
  * [Create a Content Pack](#create-a-content-pack)
* [Game Themes](#game-themes)
  * [Basic Data](#basic-data)
  * [What is a Patch?](#what-is-a-patch)
  * [Built-in Variables](built-in-variables)
  * [Built-in Patches](#built-in-patches)
* [Other Mod Themes](#other-mod-themes)
  * [Asset Loading](#asset-loading)
  * [Code Example](#code-example)
* [Content Patcher Integration](#content-patcher-integration)
  * [Current Theme Token](#current-theme-token)
  * [Modifying Theme Data](#modifying-theme-data)
  * [Modifying Assets](#modifying-assets)
* [Miscellaneous](#miscellaneous)
  * [Color Parsing](#color-parsing)
  * [Helpful Commands](#helpful-commands)


## Getting Started

### What is a Theme?

A theme has two parts. First, there is a custom data model loaded from
the theme's `theme.json` file. This data model will change depending on
the mod, but should commonly contain color values though they are by no
means limited to only colors.

Second, themes can contain assets for mods to load. This relies upon the
mod implementing Theme Manager's API. For general asset replacement,
Content Patcher is still recommended.


### What About Game Themes?

In addition to providing a theme system for other mods to use, Theme Manager
also adds themes to the base game. Themes for the base game don't allow you
to replace assets. You should use Content Patcher for that. Instead, themes
allow you to replace colors. Almost every hard-coded color in the game can be
replaced with any other colors you want.


## Creating a Theme

### `theme.json` Basics

Every theme has a `theme.json` file (though it can, in some situations, have
a different filename). This file acts as a combination of a manifest for the
theme and a container for whatever data is supported. The specific data will
vary depending on if you're making a theme for a mod or for the base game.

The manifest properties will remain the same, however. Here's a simple example
`theme.json` that doesn't contain any extra data and just serves as a
manifest for the theme:

```json
{
    // The theme's name. A human-readable string shown to users in the
    // Theme Manager's theme selectors.
    "Name": "White Flowers",

    // Optional. The theme's name in other locales. This provides a simple
    // way to translate the theme's name, and it works no matter how the
    // theme is loaded.
    "LocalizedNames": {
        "es": "Flores Blancas"
    },

    // Optional. A list of unique IDs of mods that this theme is intended
    // to compliment. When a user's theme is set to "Automatic", the list
    // of supported mods is used to determine which theme should be used.
    "SupportedMods": [
        "SomeOtherMods.UniqueId"
    ]
}
```

There's a full list of supported manifest keys in the [Theme Manifest](#theme-manifest)
section below. You can create a valid theme with just a `Name`. But you'll
probably want to include more than that. Here's an example of a very basic
theme for the base game that would make all your text pink:

```json
{
    "Name": "Unnecessarily Pink",

    "Variables": {
        "Text": "hotpink",
        "TextShadow": "maroon",
        "TextShadowAlt": "black"
    },

    "SpriteTextColors": {
        "-1": "hotpink"
    },

    "Patches": [
        "DrawTextWithShadow"
    ]
}
```

In this example, `Variables` and `SpriteTextColors` are theme data that will
be used by Theme Manager to overwrite colors used by the game. `Patches` is
another type of data used by game themes specifically to tell Theme Manager
what patches should be applied. Check out the section on [Game Themes](#game-themes)
for more details about how it all works.


### Theme Discovery

There are three separate ways to include a theme:

1. First, themes can be included directly in a mod. When Theme Manager is
   used for a mod, it will check that mod's `assets/themes/` folder for
   valid themes. The resulting file structure will look like:
   ```
   📁 Mods/
      📁 MyCoolMod/
         🗎 MyCoolMod.dll
         🗎 manifest.json
         📁 assets/
            📁 themes/
               📁 SomeTheme/
                  🗎 theme.json
                  📁 assets/
                     🗎 example.png
   ```

2. Next, themes can be included within a content pack for a mod. When Theme
   Manager is used for a mod, it will also check the mod's content packs to
   see if any of them have valid `theme.json` files. The resulting file
   structure for such a mod would look like:
   ```
   📁 Mods/
      📁 [MCM] My Cool Theme/
         🗎 manifest.json
         🗎 theme.json
         📁 assets/
            🗎 example.png
   ```

3. Finally, themes can be included within *any* mod or content pack simply
   by including a specific key within the mod's `manifest.json`. Theme Manager
   looks for a key starting with your mod's unique ID and that then ends with
   `:theme`. For example, if your mod's ID is `YourName.YourModName`, then
   Theme Manager would look for the key `YourName.YourModName:theme` in the
   manifests of other mods, checking for themes.

   > Note: If you want to use this method for adding a theme for the base game,
   > the manifest key to use is simply: `stardew:theme`

   This key should contain a relative filepath from the root of the mod to
   the theme's `theme.json` file. It uses the folder that `theme.json` file is
   in as the root folder of the theme. For example, you might end up with a
   `manifest.json` that looks like this:
   ```json
   {
       // The Usual Suspects
       "Name": "Some Other Cool Mod",
       "Author": "A Super Cool Person",
       "Version": "5.4.3",
       "Description": "Totally rad stuff.",
       "UniqueID": "SuperCoolPerson.OtherCoolMod",
       "MinimumApiVersion": "3.7.3",
       "ContentPackFor": {
           "UniqueID": "Pathoschild.ContentPatcher"
       },
   
       // Our Theme!
       "MyName.MyCoolMod:theme": "compat/MyCoolMod/theme.json"
   }
   ```

   To go with that, you might have a file structure that would look like:

   ```
   📁 Mods/
      📁 SomeOtherCoolMod/
         🗎 manifest.json
         📁 compat/
            📁 MyCoolMod/
               🗎 theme.json
               📁 assets/
                  🗎 example.png
   ```

### Create a Content Pack

The easiest way to create a new theme is to create a content pack for it.

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
           // This should be the UniqueID of the mod your theme is for.
           // If you're making a theme for the base game, you should
           // leave this as "leclair.thememanager"
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
5. Create a `theme.json` file inside the folder with this content:
   ```json
   {
       "Name": "Your Theme's Name"
   }
   ```
6. Change the `Name` value to describe your theme. It doesn't *need* to match
   the `Name` from your `manifest.json` file but it should to
   minimize confusion.
7. Add any other [Theme Manifest](#theme-manifest) values that you want.
8. Add all your theme's `assets/` and theme data, depending on what the mod
   you're making a theme for needs.


## Game Themes

### Basic Data

### What is a Patch?

### Built-in Variables

### Built-in Patches

## Other Mod Themes

### Asset Loading

### Code Example

## Content Patcher Integration

### Current Theme Token

### Modifying Theme Data

### Modifying Assets

## Miscellaneous

### Color Parsing

### Helpful Commands

## Oops, I haven't written this yet!

Sorry. I'm still working on this. It should be finished within a few days.
Until then, please check out:

* The [ThemeManager Example C# Mod](https://github.com/KhloeLeclair/StardewMods/tree/main/ThemeManagerExample)
  * Its [assets/themes/](https://github.com/KhloeLeclair/StardewMods/tree/main/ThemeManagerExample/assets/themes) folder
* The [Theme Template](https://github.com/KhloeLeclair/StardewMods/blob/main/ThemeManager/assets/themes/template/theme.json)
  for the base game.
* The [assets/patches/](https://github.com/KhloeLeclair/StardewMods/tree/main/ThemeManager/assets/patches)
  folder with all the currently available patches to use in themes for the base game.

You can also bug me on the Stardew modding Discord server. I'm the one
and only Khloe Leclair, and I hang out in #making-mods.
