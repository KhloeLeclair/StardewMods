using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace StarControl {

	/// <summary>
	/// Public API for mod integrations.
	/// </summary>
	public interface IStarControlApi {
		/// <summary>
		/// Forces a previously-registered page to be recreated the next time the menu is about to be shown.
		/// </summary>
		/// <remarks>
		/// <para>
		/// While a custom <see cref="IRadialMenuPage"/> can always decide when to update its contents, sometimes this is
		/// not convenient; for example, if the contents of a menu page are generated from some other data (like player
		/// inventory) and cached, or if activation of an item in one page could indirectly affect the contents of a
		/// different page. Explicit invalidation allows each page to cache its data for performance but still remain up to
		/// date when conditions change.
		/// </para>
		/// <para>
		/// The <paramref name="mod"/> and <paramref name="id"/> must already have been registered through
		/// <see cref="RegisterCustomMenuPage"/>, otherwise the request will be ignored.
		/// </para>
		/// </remarks>
		/// <param name="mod">Manifest for the mod that registered the menu.</param>
		/// <param name="id">Unique (per mod) ID for the page to invalidate.</param>
		void InvalidatePage(IManifest mod, string id);

		/// <summary>
		/// Registers a new page to be made available in the Mod Menu (default: right trigger).
		/// </summary>
		/// <param name="mod">Manifest for the mod that will own the menu.</param>
		/// <param name="id">Unique (per mod) ID for the page. Registering a page with a previously-used ID will overwrite
		/// the previous page.</param>
		/// <param name="factory">Factory for creating the page.</param>
		void RegisterCustomMenuPage(IManifest mod, string id, IRadialMenuPageFactory factory);

		/// <summary>
		/// Registers items to be available in the user's item library.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Items registered this way are not immediately associated with a menu page, and do not appear in an enforced order;
		/// instead, they can be assigned to Mod Menu pages and Quick Slots in Star Control's settings menu.
		/// </para>
		/// <para>
		/// Mods should use this registration method when they wish to simply provide access to their features without
		/// enforcing a particular order or hierarchy on the player.
		/// </para>
		/// </remarks>
		/// <param name="mod">Manifest for the mod providing the item.</param>
		/// <param name="items">The items to register.</param>
		void RegisterItems(IManifest mod, IEnumerable<IRadialMenuItem> items);
	}

	/// <summary>
	/// Factory for creating an <see cref="IRadialMenuPage"/> which adds mod-specific menu content.
	/// </summary>
	public interface IRadialMenuPageFactory {
		/// <summary>
		/// Creates the page for a given player.
		/// </summary>
		/// <remarks>
		/// This method may be invoked multiple times in case of invalidation, either implicitly due to a config change in
		/// RadialMenu or explicitly by <see cref="IStarControlApi.InvalidateCustomMenuPage"/>. If page creation is an
		/// expensive process then callers are allowed to return a cached result, but if doing so, must ensure that the
		/// result is unique per player/screen.
		/// </remarks>
		/// <param name="who">The player for whom the page will be displayed. Callers should use this whenever possible
		/// instead of <see cref="Game1.player"/> in case of co-op play.</param>
		IRadialMenuPage CreatePage(Farmer who);
	}

	/// <summary>
	/// A single page in one of the radial menus.
	/// </summary>
	/// <remarks>
	/// Pages can be navigated using left/right shoulder buttons while a menu is open. Only the items on
	/// the currently-active page are visible at any given time.
	/// </remarks>
	public interface IRadialMenuPage {
		/// <summary>
		/// The items on this page.
		/// </summary>
		/// <remarks>
		/// Any <c>null</c> entries in the list will render as a blank wedge in the menu, e.g.
		/// representing an empty inventory slot.
		/// </remarks>
		IReadOnlyList<IRadialMenuItem?> Items { get; }

		/// <summary>
		/// Index of the selected <see cref="IRadialMenuItem"/> in the <see cref="Items"/> list, or
		/// <c>-1</c> if no selection.
		/// </summary>
		int SelectedItemIndex { get; }

		/// <summary>
		/// Checks whether the page is empty, i.e. has no non-null items.
		/// </summary>
		/// <returns><c>true</c> if the page is empty, <c>false</c> if it has valid items.</returns>
		bool IsEmpty() {
			return !Items.Any(item => item is not null);
		}
	}

	/// <summary>
	/// Describes a single item on an <see cref="IRadialMenuPage"/>.
	/// </summary>
	public interface IRadialMenuItem {
		/// <summary>
		/// A unique ID for this item.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Providing a non-empty ID allows the item to be assigned to one of the player's Quick Slots.
		/// If this property is implemented, the ID <b>must</b> be stable across multiple game launches,
		/// as its value will be saved to the user's configuration.
		/// </para>
		/// <para>
		/// A typically good choice for an ID is the providing mod's unique ID, followed by the name of
		/// the feature; e.g. <c>focustense.StarControl.Settings</c> to open Star Control's settings.
		/// </para>
		/// </remarks>
		string Id => "";

		/// <summary>
		/// The item title, displayed in large text at the top of the info area when focused.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Additional description text displayed underneath the <see cref="Title"/>.
		/// </summary>
		/// <remarks>
		/// Can be used to display item stats, effect info or simply flavor text.
		/// </remarks>
		string Description { get; }

		/// <summary>
		/// Whether the item is currently enabled.
		/// </summary>
		/// <remarks>
		/// Disabled items show up in the menu as semi-transparent, and cannot be activated.
		/// </remarks>
		bool Enabled => true;

		/// <summary>
		/// The amount available.
		/// </summary>
		/// <remarks>
		/// For inventory, this is the actual <see cref="Item.Stack"/>. For other types of menu items,
		/// it can be used to indicate any "number of uses available". Any non-<c>null</c> value will
		/// render as digits at the bottom-right of the item icon/sprite in the menu.
		/// </remarks>
		int? StackSize => null;

		/// <summary>
		/// The item's quality, from 0 (base) to 3 (iridium).
		/// </summary>
		/// <remarks>
		/// For non-<c>null</c> values, the corresponding star will be drawn to the bottom-left of the
		/// item's icon determined by its <see cref="Texture"/> and <see cref="SourceRectangle"/>.
		/// </remarks>
		int? Quality => null;

		/// <summary>
		/// The texture (sprite sheet) containing the item's icon to display in the menu.
		/// </summary>
		/// <remarks>
		/// If not specified, the icon area will instead display monogram text based on the
		/// <see cref="Title"/>.
		/// </remarks>
		Texture2D? Texture => null;

		/// <summary>
		/// The area within the <see cref="Texture"/> containing this specific item's icon/sprite that
		/// should be displayed in the menu.
		/// </summary>
		/// <remarks>
		/// If not specified, the entire <see cref="Texture"/> will be used.
		/// </remarks>
		Rectangle? SourceRectangle => null;

		/// <summary>
		/// Optional separate area within the <see cref="Texture"/> providing an overlay sprite to
		/// render with <see cref="TintColor"/>.
		/// </summary>
		/// <remarks>
		/// Some "colored items" define both a base sprite and a sparser, mostly-transparent tint or
		/// overlay sprite so that the tint can be applied to only specific regions. If this is set,
		/// then any <see cref="TintColor"/> will apply only to the overlay and <em>not</em> the base
		/// sprite contained in <see cref="SourceRectangle"/>.
		/// </remarks>
		Rectangle? TintRectangle => null;

		/// <summary>
		/// Tint color, if the item icon/sprite should be drawn in a specific color.
		/// </summary>
		/// <remarks>
		/// If <see cref="TintRectangle"/> is specified, this applies to the tintable region; otherwise,
		/// it applies directly to the base sprite in <see cref="SourceRectangle"/>.
		/// </remarks>
		Color? TintColor => null;

		/// <summary>
		/// Attempts to activate the menu item, i.e. perform its associated action.
		/// </summary>
		/// <param name="who">The player who activated the item; generally,
		/// <see cref="Game1.player"/>.</param>
		/// <param name="delayedActions">The types of actions which should result in a
		/// <see cref="ItemActivationResult.Delayed"/> outcome and the actual action being
		/// skipped.</param>
		/// <param name="activationType">The type of activation requested, determined by which button
		/// the player pressed and in what context.</param>
		/// <returns>A result that describes what action, if any, was performed.</returns>
		ItemActivationResult Activate(
			Farmer who,
			DelayedActions delayedActions,
			ItemActivationType activationType = ItemActivationType.Primary
		);

		/// <summary>
		/// If implemented, performs a function for each frame after the initial <see cref="Activate"/>
		/// during which the trigger button is held.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ItemActivationType.Instant"/>; used e.g. to charge tools.
		/// </remarks>
		void ContinueActivation() { }

		/// <summary>
		/// If implemented, performs a function on the frame when the trigger button that caused the
		/// initial activation is finally released.
		/// </summary>
		/// <remarks>
		/// Only applies to <see cref="ItemActivationType.Instant"/>; e.g. to release a charged tool.
		/// </remarks>
		/// <returns>Whether the ending sequence could be performed. If this returns <c>false</c>, then
		/// the remap controller will keep calling this method on every frame (and ignore further
		/// activation attempts on this item) until it returns <c>true</c>.</returns>
		bool EndActivation() => true;

		/// <summary>
		/// Chooses the sound to play when activating an item.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Implementing this is optional; sound cues are typically determined by Star Control's mod
		/// settings. Authors only need to override it if they intend to provide a custom, per-item
		/// sound, or disable the default sound even if the user has sounds enabled.
		/// </para>
		/// <para>
		/// Custom sounds will not play if the user has all sound muted in Star Control's settings.
		/// </para>
		/// </remarks>
		/// <param name="who">The player who activated the item; generally,
		/// <see cref="Game1.player"/>.</param>
		/// <param name="activationType">The type of activation requested, determined by which button
		/// the player pressed and in what context.</param>
		/// <param name="defaultSound">The default sound that would play if not overridden; implementers
		/// should return this value if not changing the default.</param>
		/// <returns>The name of the sound cue to play on activation, or <c>null</c> or an empty string
		/// to play no sound.</returns>
		string? GetActivationSound(
			Farmer who,
			ItemActivationType activationType,
			string defaultSound
		) => defaultSound;

		/// <summary>
		/// Gets whether the item is still in the process of activating, i.e. is expecting to receive an
		/// <see cref="EndActivation"/> but has not yet received it.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The main use of this is to allow additional <see cref="Activate"/> calls to be requested on
		/// a specific item that is considered to be already in use. This has functions for certain
		/// tools such as the <see cref="StardewValley.Tools.FishingRod"/> that expect to receive
		/// multiple button presses from their trigger button (generally the game's tool-use button)
		/// during a single "use".
		/// </para>
		/// <para>
		/// Unless there is a very specific reason to override this, most API clients should not.
		/// </para>
		/// </remarks>
		/// <returns><c>true</c> if the item is currently in the middle of an activation, animation,
		/// etc. Otherwise, <c>false</c>.</returns>
		bool IsActivating() => false;
	}

	/// <summary>
	/// The types of actions that can be delayed when selecting from a controller menu.
	/// </summary>
	public enum DelayedActions {
		/// <summary>
		/// Delay for all items activated via the menu.
		/// </summary>
		All,

		/// <summary>
		/// Only delay when switching tools, which has no in-game animation.
		/// </summary>
		ToolSwitch,

		/// <summary>
		/// Never delay any menu item.
		/// </summary>
		/// <remarks>
		/// Typically only used as a transient state to indicate that the delay is done.
		/// </remarks>
		None,
	}

	/// <summary>
	/// Specifies which action associated with a given menu item should be performed.
	/// </summary>
	/// <remarks>
	/// The meanings of each type are entirely specific to the implementation of the
	/// <see cref="IRadialMenuItem"/> and the context in which it is performed. The types simply reflect
	/// how the player selected the item - that is, which button was used to activate it.
	/// </remarks>
	public enum ItemActivationType {
		/// <summary>
		/// The item's primary action.
		/// </summary>
		/// <remarks>
		/// Primary means that the item was activated from a radial menu or quick slot, via the user
		/// pressing the configured <see cref="Config.InputConfiguration.PrimaryActionButton"/>.
		/// Typical primary actions including eating a food item, warping with a totem, or selecting a
		/// tool such as the Axe or Pickaxe.
		/// </remarks>
		Primary,

		/// <summary>
		/// The item's secondary action.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Secondary means that the item was activated from a radial menu or quick slot, via the user
		/// pressing the configured <see cref="Config.InputConfiguration.SecondaryActionButton"/>.
		/// </para>
		/// <para>
		/// Secondary actions may be the same as the <see cref="Primary"/> action, or may be an
		/// alternate use of the same item, such as selecting (not using) a consumable item for the
		/// purpose of gifting or placing into a machine.
		/// </para>
		/// </remarks>
		Secondary,

		/// <summary>
		/// The item's instant (one-button) action, if it has one.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Instant actions are used in button remapping and can also be thought of as "tool use". For
		/// example, an actual <see cref="Tool"/> item would perform the tool's actual <em>function</em>
		/// as if the player had pressed the tool-use button (axe chop, sword swing, etc.).Other
		/// consumable items, such as food, might behave similarly to the <see cref="Primary"/> action,
		/// except that if their primary action involves any delay or confirmation aside from their
		/// regular animation, then that delay/confirmation should be skipped.
		/// </para>
		/// <para>
		/// Instant actions are only performed for items that are set up in the player's instant slot
		/// (button remap), and pressed while the player is free and interacting with the world, i.e.
		/// when <b>no</b> menu is open including the radial menu.
		/// </para>
		/// </remarks>
		Instant,
	}

	/// <summary>
	/// The result of activating a menu item in a controller menu.
	/// </summary>
	public enum ItemActivationResult {
		/// <summary>
		/// The activation was ignored, i.e. nothing happened.
		/// </summary>
		/// <remarks>
		/// This is normally only used internally to indicate that something went unexpectedly wrong.
		/// Actions that were understood, but had no effect, should use <see cref="Custom"/> instead.
		/// </remarks>
		Ignored = -1,

		/// <summary>
		/// An immediate action/effect was triggered, such as eating a food item, using a totem, etc.
		/// </summary>
		/// <remarks>
		/// This is the normal result when an item is activated with <see cref="InventoryAction.Use"/>,
		/// <b>and</b> the item has some useful "quick action" that's meant to be triggered from the
		/// menu directly. If no such action is possible (e.g. inedible materials or tools like axe or
		/// hoe), or the requested action was <see cref="InventoryAction.Select"/>, then one of
		/// <see cref="Delayed"/> or <see cref="Selected"/> should be used instead.
		/// </remarks>
		Used,

		/// <summary>
		/// Specifies that the real action, which will yield one of the other result types, should
		/// happen after a confirmation delay.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Confirmation delays normally are - and generally should be - used for any actions that
		/// immediately return control to the player; actions that <b>do not</b> open a menu, play an
		/// animation, fade to black, or trigger any other effect that would cause
		/// <see cref="Context.CanPlayerMove"/> to evaluate to <c>false</c>.
		/// </para>
		/// <para>
		/// Delays provide a brief window for the player to release the buttons that were used to
		/// select/activate the menu item and avoid having them become stray inputs into the game world,
		/// possibly making the player face/move in a different direction or use a held item or tool.
		/// During the delay period, the window stays open and the selected slice blinks, confirming the
		/// player's selection before performing the action.
		/// </para>
		/// <para>
		/// Items that opt into delays should check the <see cref="DelayedActions"/> in the request. If
		/// it has a value <b>other than</b> <see cref="DelayedActions.None"/>, and in particular one
		/// that is applicable to the item that was selected, then the item should return this result
		/// <em>and not perform the associated action</em>. Once the delay expires, a second activation
		/// request will be sent with <see cref="DelayedActions.None"/> to trigger the real action.
		/// </para>
		/// </remarks>
		Delayed,

		/// <summary>
		/// Indicates that an item became the selected item in its corresponding menu.
		/// </summary>
		/// <remarks>
		/// This is primarily used in the inventory menu to signal tool selection, so that the active
		/// backpack page can be updated in response. For the Mod Menu, the exact behavior depends on
		/// user settings, specifically <see cref="Config.LegacyModConfig.RememberSelection"/>.
		/// Non-inventory menus/items are <b>not</b> required to implement their own selection behavior,
		/// but if it is used, then the corresponding <see cref="IRadialMenuPage"/> must have a
		/// consistent <see cref="IRadialMenuPage.SelectedItemIndex"/> value.
		/// </remarks>
		Selected,

		/// <summary>
		/// The item is a tool, and tool use has been initiated.
		/// </summary>
		/// <remarks>
		/// This is generally only applicable to <see cref="ItemActivationType.Instant"/> actions and
		/// combines some of the semantics of both <see cref="Selected"/> and <see cref="Used"/>.
		/// Returning this has the same inventory-cycling effect as <see cref="Selected"/>, but also
		/// triggers various patches to run that trick the game into thinking the tool-use button is
		/// pressed, until the button that was actually used to activate the item is released.
		/// </remarks>
		ToolUseStarted,

		/// <summary>
		/// A special kind of action was performed that has no immediate or delayed effect on the player
		/// or game world, such as opening a menu.
		/// </summary>
		/// <remarks>
		/// The behavior for this result is generally the same as <see cref="Used"/>, but mods should
		/// distinguish between them when possible to allow for future enhancements.
		/// </remarks>
		Custom,
	}
}
