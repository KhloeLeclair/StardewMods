← [README](README.md)

This document is intended to help C# mod developers implement support for
Better Game Menu within their own mods.


## Contents

* [What is Better Game Menu?](#what-is-better-game-menu)
  * [Why use it?](#why-use-it)
* [Menu Access](#menu-access)
  * [Current Page](#current-page)
  * [Opening the Menu](#opening-the-menu)
* [Custom Menu Tabs](#custom-menu-tabs)
  * [New Tabs](#new-tabs)
  * [New Implementations](#new-implementations)
* [Available Events](#available-events)
  * [Menu Created](#menu-created)
  * [Tab Changed](#tab-changed)
  * [Tab Context Menu](#tab-context-menu)
  * [Page Created](#page-created)


## What is Better Game Menu?

Better Game Menu is a complete replacement of the built-in `GameMenu` class.
It does not inherit from `GameMenu`, and there are a few reasons for that.

1. Because we don't inherit from `GameMenu`, we don't have to maintain
   bad behaviors that lead to poor menu performance. Among these:
   a. `GameMenu` immediately constructs all of its page instances immediately,
      even though most of the time the player only access one or two specific
      pages when opening their menu.
   b. `GameMenu` is completely recreated every time the game window changes
      size, the UI scale changes, or one of several other things happen. This
      is a lot of wasted effort.
   c. `GameMenu` has a lot of hard coded logic for a specific set of tabs,
      making it very difficult for mods to even add new pages to it let alone
      do so in a way that doesn't conflict with other mods.

2. Many mods make assumptions about how `GameMenu` works, not without good
   reason. However, due to the optimizations in Better Game Menu, most of those
   assumptions would be incorrect.

3. Many mods *patch* `GameMenu`, and that would just make the situation even
   stranger for a subglass of `GameMenu` that doesn't use any of the same
   internal structure as `GameMenu`.

We get a lot of benefits from this, however. Often, users with a lot of mods
begin to experience significant slowdown when accessing the menu. This can be
due to the Collections page, among other things. I don't know about you, but
I use that page very infrequently compared to how often I open my menu. So,
why let it slow me down?

Better Game Menu only is focused on performance, and to that end we:

1. Only create new page instances when the user is actually going to see
   those pages (or if another mod uses our API to get a page instance).

2. Only forward window size changed events to the current page. There's no
   need to update every page instance when most of them aren't actually
   doing anything. We just keep track of this, and send the size change
   event later if the user actually tries to use a page (or, again, another
   mod uses our API to get a page instance).

3. Don't recreate the entire menu just because the window size or
   UI scale changed.

Simple. Effective.


### Why use it?

The menu opens faster. It has a robust API so other mods can work with
and modify the menu without needing to patch things and worry about
compatibility with other mods changing the menu.


## Menu Access

### Current Page
#### aka Where's my `Game1.activeClickableMenu is GameMenu`?

This is my recommended method for determining if the current menu
is a game menu, and for accessing the current page.

```cs
// somewhere at the start of your file
internal IBetterGameMenuApi? BetterGameMenuApi;

// somewhere in GameLaunched
this.BetterGameMenuApi = this.Helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");

// somewhere in your methods
public bool IsGameMenu(IClickableMenu menu) {
    return GetGameMenuPage(menu) is not null;
}

public IClickableMenu? GetGameMenuPage(IClickableMenu menu) {
    if (Game1.activeClickableMenu is GameMenu gameMenu)
        return gameMenu.GetCurrentPage();
    return this.BetterGameMenuApi?.GetCurrentPage(menu);
}
```

Basically, we're making the assumption here that if a game menu is open, then
it will have a current page. Both `GameMenu` and Better Game Menu create their
current page within their constructors, so this should always be safe, so long
as you aren't doing things to a Better Game Menu during its construction.

If you *are*, then you should be using the `AsMenu()` method from
Better Game Menu's API instead. That method also gives you a few other methods
for working with our menu instance.

Why, then, do I recommend just using `GetCurrentPage()`? Because if that's all
you're doing, then you can dramatically limit the amount of Better Game Menu's
API you're consuming, and doing that makes it all the less likely that any
future updates might break your integration.


### Opening the Menu

There are two methods you can use for opening Better Game Menu using the
API, but you probably want to do something like this:
```cs
public void OpenSkillsMenu() {
    IClickableMenu menu;
    Game1.PushUIMode();
    if (BetterGameMenuApi is not null)
        menu = BetterGameMenuApi.CreateMenu(nameof(VanillaTabOrders.Skills));
    else
        menu = new GameMenu(GameMenu.skillsTab);
    Game1.activeClickableMenu = menu;
    Game1.PopUIMode();
}
```

Here, I demonstrated a difference with Better Game Menu. Tabs are
identified with string keys, rather than the numeric IDs that `GameMenu`
makes use of. This is much better for compatibility, since mods don't have
to fight over numeric IDs and there's no concern about key stability.

To keep your code simpler, you can also use the IDs for the built-in
pages, like so:
```cs
BetterGameMenuApi.CreateMenu(GameMenu.skillsTab);
```

Just keep in mind, using these numeric IDs only supports the built-in
pages. If you do something special to add your own numeric ID to `GameMenu`,
it won't affect this.


### Other Things

Beyond this, please read the API file and see what you can find. I've done my
best to comment everything, and the file is written with `#nullable enable`
for good coding practices.


## Custom Menu Tabs

Perhaps the best feature of Better Game Menu is how easy it is to extend
the menu with additional pages, or replace existing pages. No need for
Harmony patches. Everything is quick and intended and works with other mods.

Before moving on, you should know a little of my terminology:

1. **Tab**: A tab is, well, a tab of the menu. Tabs always have the same
   display name, position in the tab list, and icon to keep things simpler
   for users. A tab can have many implementations, and may or may not have
   a page that was created using an implementation.

2. **Page**: A page is the `IClickableMenu` instance that the game menu
   renders as its child menu (not to be confused with the actual child
   menu system, since game menu doesn't use that). These are the same
   classes and instances that `GameMenu` uses. Things like `InventoryPage`,
   `SkillsPage`, `OptionsPage`, etc.

3. **Implementations**: An implementation is something that creates a
   page instance, as well as defining a bit of behavior. Implementations
   have methods for telling the menu how big it should be, whether the
   menu should be invisible (like on the Map tab) or not, and special
   methods for when the menu exits or is resized.

Now that we've covered that, let's talk about adding new stuff.


### New Tabs

When you register a new tab with Better Game Menu, you *also* need to
register an implementation. I'll quickly go over the various arguments
of `RegisterTab()` here before covering the arguments shared by
`RegisterImplementation()` below.


# TODO: THIS! Sorry

