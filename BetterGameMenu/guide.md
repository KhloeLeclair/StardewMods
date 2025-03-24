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
* [Overlays](#overlays)
* [Available Events](#available-events)
  * [Menu Created](#menu-created)
  * [Tab Changed](#tab-changed)
  * [Tab Context Menu](#tab-context-menu)
  * [Page Created](#page-created)
  * [Page Overlay Creation](#page-overlay-creation)
  * [Page Ready to Close](#page-ready-to-close)


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
    if (Game1.activeClickableMenu is GameMenu)
        return true;
    return this.BetterGameMenuApi?.IsMenu(Game1.activeClickableMenu) ?? false;
}

public IClickableMenu? GetGameMenuPage(IClickableMenu menu) {
    if (Game1.activeClickableMenu is GameMenu gameMenu)
        return gameMenu.GetCurrentPage();
    return this.BetterGameMenuApi?.GetCurrentPage(menu);
}
```

These methods allow you to both check if the current menu is a game menu, and
get the current menu page. As a bonus, neither method requires fancy
interfaces so the amount of API surface you need to consume is very small if
this is all you're doing.

If you need to get more details about the game menu, or access pages that
aren't currently active, you'll need to look into the `AsMenu()` method from
the API and the `IBetterGameMenu` interface.


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

<table>
<tr>
<th>Argument</th>
<th>Description</th>
</tr>
<tr>
<td><code>id</code></td>
<td>

*string*

The unique id of the tab to register.

The built-in tabs use the keys provided in the `VanillaTabOrders` enum,
so you can use them with code like `nameof(VanillaTabOrders.Skills)`.

> Note: If another mod has already registered this tab, the existing tab
> registration will be overwritten. This behavior may change in the future,
> so you shouldn't rely on it.

</td>
</tr>
<tr>
<td><code>order</code></td>
<td>

*int*

The order of this tab relative to other tabs. See the `VanillaTabOrders`
enum to see the order values of the built-in tabs.

You can also use code like `(int) VanillaTabOrders.Skills + 5` to place
your tab relative to another tab. The built-in tabs have a degree of
separation in their orders to allow modded tabs to be inserted.

</td>
</tr>
<tr>
<td><code>getDisplayName</code></td>
<td>

*Func<string>*

A method that returns the display name of this tab, to be displayed in
a tool-tip to the user. This string is not further processed, so you
should handle any processing yourself.

</td>
</tr>
<tr>
<td><code>getIcon</code></td>
<td>

*(DrawDelegate DrawMethod, bool DrawBackground)*

A method that returns drawing instructions for this tab's icon. You
must provide both a `DrawDelegate` (which can be created using the
helper method `CreateDraw`) and a boolean indicating whether or not
you want a blank tab background to be drawn for you behind the output
of your draw delegate.

</td>
</tr>
</table>

And that's it. Everything else is associated with an implementation
rather than the generic tab itself.


### New Implementations

When registering a new tab or implementation, you can provide the
following arguments to configure the behavior of your implementation.

> Note: A given mod can only register one implementation for a given tab.

<table>
<tr>
<th>Argument</th>
<th>Description</th>
</tr>
<tr><th colspan="2">Required</th></tr>
<tr>
<td><code>priority</code></td>
<td>

*int*

The priority of this implementation. When multiple implementations for a
tab are registered, and the user hasn't explicitly chosen one, then the
implementation with the highest priority is used.

</td>
</tr>
<tr>
<td><code>getPageInstance</code></td>
<td>

*Func<IClickableMenu, IClickableMenu>*

A method that returns a new page instance for this tab. When this method
is called, the `IClickableMenu` passed to it is the Better Game Menu
instance that the page is being created for.

</td>
</tr>
<tr><th colspan="2">Optional</th></tr>
<tr>
<td><code>getDecoration</code></td>
<td>

*Func<DrawDelegate?>?*

A method that may return a decoration if one should be displayed.

A decoration is a draw delegate that's called to draw on top of a tab, so long
as the tab has not been accessed. When the user switches to a tab, its
decoration is cleared.

This feature is intended for use in drawing things on top of a tab such as
a sparkle indicating there is new/unread content available within the menu.

Please be careful when drawing outside the bounds of the tab, because tabs
may be positioned at the top of the screen, and there may be an additional
row of tabs above your tab.

</td>
</tr>
<tr>
<td><code>getTabVisible</code></td>
<td>

*Func<bool>?*

A method that returns whether or not this tab should be included in the
menu at the current time. This can be used to, for example, hide a tab until
the user encounters content that would appear in the tab.

</td>
</tr>
<tr>
<td><code>getMenuInvisible</code></td>
<td>

*Func<bool>?*

A method that returns whether or not the Better Game Menu should set its
`Invisible` flag when this is the active tab. When `Invisible` is true,
the game menu will not draw itself or its tabs, nor will the tabs be
accessible via mouse controls. The upper right close button, however,
will still be visible by default.

This is the flag used by the built-in map page to appear as it does.

</td>
</tr>
<tr>
<td><code>getWidth</code></td>
<td>

*Func<int, int>?*

A method to get the width to use for the game menu when this is the
active tab. The method is called with an argument containing the menu's
default width. If you need your menu to be 64 pixels wider, for example,
your method may look like this:

`getWidth: width => width + 64`

</td>
</tr>
<tr>
<td><code>getHeight</code></td>
<td>

*Func<int, int>?*

A method to get the height to use for the game menu when this is the
active tab. The method is called with an argument containing the menu's
default height. If you need your menu to be 64 pixels shorter, for example,
your method may look like this:

`getHeight: height => height - 64`

> Note: It's not recommended to use menus of non-standard heights.

</td>
</tr>
<tr>
<td><code>onResize</code></td>
<td>

*Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>?*

This method is called whenever the game menu is resized, either as a result
of the game window being resized or the UI scale changing. You can use this
method to either alter your existing page instance, or to return a new
page instance that should replace the existing one.

When this method is used to replace an existing page instance, Better Game Menu
still fires all applicable events, including `PageCreated` and `onClose`, and
calling `Dispose` if the menu is disposable.

</td>
</tr>
<tr>
<td><code>onClose</code></td>
<td>

*Action<IClickableMenu>?*

This method is called whenever a page instance is being closed. This can be
because the Better Game Menu was closed, or because the page instance was
replaced through `onResize` or by the user choosing a different implementation.

</td>
</tr>
</table>


## Overlays

An overlay is an encapsulation of logic that runs over top of an existing
menu page instance, allowing you to run additional behavior without relying
on imprecise events or harmony patches.

Using an overlay, you can process and override input events, prevent the
page from closing if your overlay is not ready to close, and perform your
own updates and drawing.

The interface file has a sample `IPageOverlay` class that defines every
available method you can use in an overlay. However, it's recommended that
you don't use `IPageOverlay`. Better Game Menu will detect your compatible
methods at runtime from whatever `IDisposable` you supply.

This can be used to, for example, add a new clickable component to an
existing page and respond to events targeting it.

Here's a very simple example of an overlay in action. Not a useful overlay,
but I feel it gets the idea across:
```cs
public class TestOverlay : IDisposable {
    private readonly InventoryPage Page;
    private int SinceClick = 0;

    public TestOverlay(InventoryPage page) {
        Page = page;
    }

    public void Dispose() {
        // Do nothing
    }

    public bool ReadyToClose() {
        return SinceClick <= 0;
    }

    public void Update(GameTime time, out bool suppressEvent) {
        suppressEvent = false;
        if (SinceClick > 0)
            SinceClick--;
    }

    public void ReceiveLeftClick(int x, int y, bool playSound, out bool suppressEvent) {
        suppressEvent = false;
        if (y > Game1.uiViewport.Height / 2) {
            suppressEvent = true;
            SinceClick = 300;
            if (playSound)
                Game1.playSound("Duck");
        }
    }

    public void PerformHoverAction(int x, int y, out bool suppressEvent) {
        suppressEvent = false;
        if (y > Game1.uiViewport.Height / 2) {
            suppressEvent = true;
            Page.performHoverAction(-100, -100);
        }
    }

    public void Draw(SpriteBatch batch) {
        SpriteText.drawString(batch, "Hello world!", 0, 0);
    }
}

// later
API.OnPageOverlayCreation(evt => {
    if (evt.Tab == nameof(VanillaTabOrders.Inventory) && evt.Page is InventoryPage inv)
        evt.AddOverlay(new TestOverlay(inv));
});
```

This overlay is pretty useless. It draws "Hello world!" on the screen, and
plays a duck sound if you click on the lower half of the screen. It also
prevents the actual page instance from receiving those click events, and
stops you from closing the menu for 5 seconds after it happens.


## Available Events

### Menu Created

The menu created event is fired whenever a Better Game Menu instance is
created, before the menu is assigned to `Game1.activeClickableMenu`.

The event fires after the tab UI and current page have been created.


### Tab Changed

The tab changed event is fired whenever a Better Game Menu's current tab
changes, except when the game menu is first created.


### Tab Context Menu

The tab context menu event is fired whenever a user right-clicks on a tab.
This can be used to display quick actions, such as opening your mod's
settings in Generic Mod Config Menu. As an example:
```cs
this.BetterGameMenuApi?.OnTabContextMenu(evt => {
    if (evt.Tab == nameof(VanillaTabOrder.Options))
        evt.Entries.Add(evt.CreateEntry(
            "Open Settings",
            () => this.GenericModConfigMenu.OpenModMenuAsChildMenu(this.ModManifest)
        ));
});
```


### Page Created

The page created event is fired whenever a new page instance is created
using an implementation. You can use this to apply modifications to an
instance or set up state if your mod requires doing so.


### Page Overlay Creation

The page overlay creation event is fired whenever a user is about to view
a page instance for a first time in a given menu session. This can be
because they're opening that page for the first time, or it can be
because the previous page instance was replaced for some reason.

See [Overlays](#overlays) for more information.


### Page Ready to Close

The page ready to close event is fired whenever Better Game Menu is checking
if a page is ready to be closed, either because the menu is closing, because
the page instance is going to be replaced, or because current page is about
to change.

This can be used to override the behavior of the base page's `readyToClose()`
method, so please be careful if you use it.
