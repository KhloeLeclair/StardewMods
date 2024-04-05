← [README](README.md)

This document is intended to help mod authors create content packs for Better Crafting.

## Contents

* [Getting Started](#getting-started)
  * [Create a Content Pack](#create-a-content-pack)
* [Big Craftable Actions](#big-craftable-actions)
* [Map Tile Actions](#map-tile-actions)
  * [Open Crafting Menu](#open-crafting-menu)
* [Trigger Actions](#trigger-actions)
  * [Open Crafting Menu](#open-crafting-menu)
* [Categories](#categories)
  * [Using Content Patcher](#categories-using-content-patcher)
* [Crafting Stations](#crafting-stations)
  * [Using Content Patcher](#crafting-stations-using-content-patcher)

## Getting Started

### Create a Content Pack

1. Install [SMAPI](https://smapi.io/) and [Better Crafting](https://www.nexusmods.com/stardewvalley/mods/11115/)
   if you haven't yet. (If you haven't, how did you even find this?)
2. Create an empty folder in your `Stardew Valley\Mods` folder and name it
   `[BCraft] Your Mod's Name`. Replace `Your Mod's Name` with your mod's
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
		   "UniqueID": "leclair.bettercrafting"
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
   pack to do with Better Crafting.


## Big Craftable Actions

By using Content Patcher, you can add a custom action to any big craftable that
does not already have one. These actions are [map tile actions](https://stardewvalleywiki.com/Modding:Maps#Action)
so you can do anything that you can do with one of those. For example, this
snippet would open the Prairie King arcade game when clicking a Tub o' Flowers:
```json
{
	"Format": "2.0.0",

	"Changes": [
		{
			"Action": "EditData",
			"Target": "Data/BigCraftables",
			"TargetField": [
				"108", // Tub o' Flowers
				"CustomFields"
			],
			"Entries": {
				"leclair.bettercrafting_PerformAction": "Arcade_Prairie"
			}
		}
	]
}
```

You can use this with any map tile action, but it is intended for use with
Better Crafting's custom action to open the crafting menu.

## Map Tile Actions

### Open Crafting Menu

`leclair.bettercrafting_OpenMenu [isCooking] [searchBuildings] [station-id]`

This is a custom map / big craftable action that can be used to open a
Better Crafting menu. It has three optional arguments.

1. `[isCooking]`: If true, this is cooking menu. If false, this is a crafting
   menu. This changes which set of recipes are available.
2. `[searchBuildings]`: If true, this performs like the "Magic Workbench" and
   searches for all the chests within connected buildings / the current building.
3. `[station-id]` If this is included, it must be the Id of a custom crafting
   station and it will open the menu for that station. This is how you open
   custom crafting stations.


## Trigger Actions

### Open Crafting Menu

`leclair.bettercrafting_OpenMenu [isCooking] [searchBuildings] [station-id]`

This is identical to the map tile action, just exposed to triggers as well.


## Categories

Better Crafting organizes recipes into categories to make them easier for users
to find, especially in large packs with many recipes. Many of these categories
will categorize items automatically using rules, but some categories require a
manual touch.

To make changes to categories, you'll want to make a file in your content pack
called `categories.json`. This file is split into two main groups: `Crafting`
and `Cooking`, as all crafting in Better Crafting is divided into those two
separate groups.

A `categories.json` file might look like this:
```json
{
	"Crafting": [
		{
			"Id": "decoration",
			"Recipes": [
				"--Scarecrow",
				"--Deluxe Scarecrow",
				"Mini-Obelisk"
			]
		}
	]
}
```

The top level `Crafting` and `Cooking` objects are lists, containing category
objects. Each category must have an `Id` to identify it and a list of `Recipes`
at a minimum. The following fields are available:

<table>
<tr>
<th>Field</th>
<th>Description</th>
</tr>
<tr>
<td><code>Id</code></td>
<td>

**Required.** The category's Id. This need not be unique to your content pack.
Categories with the same Id will be merged automatically.

</td>
</tr>
<tr>
<td><code>Name</code></td>
<td>

The category's display name. This does not currently support localization or
tokenized strings.

</td>
</tr>
<tr>
<td><code>Icon</code></td>
<td>

An optional CategoryIcon. A category icon must have a `Type` of either
`Item` or `Texture`. You should use `Item` in most cases. When using
`Item`, you may set a `RecipeName` or `ItemId` and the relevant item
will be used as the category's icon.

</td>
</tr>
<tr>
<td><code>Recipes</code></td>
<td>

A list of recipes for this category. Each entry should be a
recipe's name. You can start an entry with `--` to instead remove the recipe
from the category's list of recipes.

</td>
</tr>
<tr>
<td><code>UseRules</code></td>
<td>

If this is set to true, the Recipes list will be ignored and the provided
DynamicRules will be used to determine which recipes should appear within
the category.

</td>
</tr>
<tr>
<td><code>DynamicRules</code></td>
<td>

A list of DynamicRuleData. Each entry in the list should have an `Id`,
as well as any special data the specific rule requires. Most do not
require any.

</td>
</tr>
<tr>
<td><code>IncludeInMisc</code></td>
<td>

If this is set to true, recipes that appear in the category will also appear
in the Miscellaneous category.

</td>
</tr>
</table>


### Categories Using Content Patcher

In addition to using a `categories.json` file, you can edit the default
categories using Content Patcher. However, there are a few caveats.

1. Removing recipes via `--` does not work when using Content Patcher.
2. It is difficult to edit lists using Content Patcher.

However, Content Patcher does allow you to perform localization. In
some cases, you might want to use a hybrid approach where you use
Content Patcher to assign a display name, and a `categories.json` file
to set up your categories.

> When using Content Patcher, you'll need to make a separate content pack
> that targets Content Patcher rather than Better Crafting. For more on that, you'll
> want to see [Content Patcher's own documentation](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide.md).

To edit category data with Content Patcher, you'll want to use its `EditData`
action on the target `Mods/leclair.bettercrafting/Categories`.


## Crafting Stations

Similar to the mod Custom Crafting Stations, you can use Better Crafting to
set up customized crafting interfaces that only show certain recipes to users.

I implemented this feature because the Custom Crafting Stations mod is
currently on life support. Pathoschild is maintaining the mod as it's in, as
Pathos puts it, 'keeping the lights on' priority.

There are several benefits to using Better Crafting rather than Custom Crafting
Stations, as well as one drawback.

#### Benefits:
* The menu looks nicer. When using a custom crafting station with Better Crafting,
  you can display a custom icon and name to label the menu.
* You can categorize the recipes that are displayed.
* You can include any recipe in Better Crafting, and not just vanilla crafting
  recipes, which will be more of a benefit once custom recipe support is added
  for content packs via JSON.
* You can make a crafting menu that lets users craft recipes they don't
  have unlocked.
* Better Crafting supports almost all features of Custom Crafting Stations.

#### Drawbacks:
* You cannot make a crafting station with mixed crafting and cooking recipes.
* It is slightly more complicated to configure.

If this sounds good to you, then let's get started!

First though, there's a converter to help get you started if you're coming from
CCS. Check it out here: https://khloeleclair.github.io/CCSConverter/

You can define crafting stations with either Content Patcher (recommended), or
by creating a `stations.json` file in your Better Crafting content pack.

The `stations.json` file must contain a list of station objects, each object
having at minimum an `Id` and a list of `Recipes`. The following fields are
available:

<table>
<tr>
<th>Field</th>
<th>Description</th>
</tr>
<tr>
<td><code>Id</code></td>
<td>

**Required.** The station's Id. This should be unique. It should not include
spaces. You will need this Id in order to open your crafting station.

</td>
</tr>
<tr>
<td><code>DisplayName</code></td>
<td>

The crafting station's display name. This does not currently support
localization or tokenized strings. It is recommended to implement localization
by using Content Patcher.

</td>
</tr>
<tr>
<td><code>Icon</code></td>
<td>

An optional CategoryIcon. A category icon must have a `Type` of either
`Item` or `Texture`. You should use `Item` in most cases. When using
`Item`, you may set a `RecipeName` or `ItemId` and the relevant item
will be used as the crafting station's icon.

If this is not set, and your station is opened via a big craftable, it
will automatically use your big craftable's sprite as the icon.

</td>
</tr>
<tr>
<td><code>AreRecipesExclusive</code></td>
<td>

If this is set to true, the recipes in this crafting station's list will
not be displayed in standard crafting menus, forcing users to use
this crafting station (or another custom crafting station) to craft them.

</td>
</tr>
<tr>
<td><code>DisplayUnknownRecipes</code></td>
<td>

If this is set to true, recipes that a user does not have unlocked
will be made available as if the user knew them when using this
crafting station. 

</td>
</tr>
<tr>
<td><code>IsCooking</code></td>
<td>

Whether or not this station is a cooking station. You can only display
crafting recipes OR cooking recipes.

</td>
</tr>
<tr>
<td><code>Recipes</code></td>
<td>

A list of recipes for this crafting station. Each entry should be a
recipe's name.

</td>
</tr>
<tr>
<td><code>Categories</code></td>
<td>

An optional list of [categories](#categories). These are not currently
editable by end users.

If you need help with categories, you ma want to create a category in
the normal menu using Better Crafting, and examine the JSON file it
saves to see how to structure your categories.

</td>
</tr>
</table>

### Dev Commands

To see if your station has been loaded, you can use the `bc_station`
console command to list all crafting stations. You can also open your
station by using `bc_station [id]` with the station's Id.

If you're using `stations.json` files, you can use `bc_station reload`
to reload all crafting station data.

If you're using Content Patcher, reloading your patch will automatically
reload all crafting station data.


### Crafting Stations Using Content Patcher

The best way to add a custom crafting station is by using Content Patcher,
as you can localize your strings and include dynamic tokens like your
mod's Id.

> When using Content Patcher, you'll need to make a separate content pack
> that targets Content Patcher rather than Better Crafting. For more on that, you'll
> want to see [Content Patcher's own documentation](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-guide.md).

To add or edit custom crafting stations, you'll want to use Content Patcher's
`EditData` action with the target `Mods/leclair.bettercrafting/CraftingStations`.
Here's a quick example that adds a station for crafting torches:
```json
{
	"Format": "2.0.0",

	"Changes": [
		{
			"Action": "EditData",
			"Target": "Mods/leclair.bettercrafting/CraftingStations",
			"Entries": {
				"{{ModId}}_TorchBench": {
					"Id": "{{ModId}}_TorchBench",
					"DisplayName": "Torches Only",
					
					"AreRecipesExclusive": true,

					"Recipes": [
						"Torch",
						"Wooden Brazier"
					],

					"Categories": [
						{
							"Id": "torches",
							"Name": "Torches",
							"Icon": {
								"Type": "Item"
							},
							"Recipes": [
								"Torch"
							]
						},
						{
							"Id": "not-torches",
							"Name": "Kind of Torch-Like",
							"Icon": {
								"Type": "Item"
							},
							"Recipes": [
								"Wooden Brazier"
							]
						}
					]
				}
			}
		},

		{
			"Action": "EditData",
			"Target": "Data/BigCraftables",
			"TargetField": [
				"108", // Tub o' Flowers
				"CustomFields"
			],
			"Entries": {
				"leclair.bettercrafting_PerformAction": "leclair.bettercrafting_OpenMenu FALSE TRUE {{ModId}}_TorchBench"
			}
		}
	]
}
```

This file does two things.

First, it defines a custom crafting station that lets you craft a Torch and
a Wooden Brazier. It's marked as exclusive, so you won't be able to craft
those using other crafting menus. It also sets up a pair of simple categories
for the menu.

Second, it edits the data of the Tub o' Flowers big craftable to add a custom
field. That custom field tells Better Crafting to perform a map tile action
when you attempt to use the big craftable, and the action is to open a
Better Crafting menu with our custom crafting station.

