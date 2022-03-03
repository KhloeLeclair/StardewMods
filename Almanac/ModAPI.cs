using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Leclair.Stardew.Common;

using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

using Leclair.Stardew.Almanac.Models;

namespace Leclair.Stardew.Almanac {

	public interface IAlmanacAPI {

		#region Custom Pages

		/*void RegisterPage(
			IManifest manifest,
			string id,
			// State
			Func<IClickableMenu, bool> Enabled = null,
			Func<IClickableMenu, object> saveState = null,
			Action<IClickableMenu, object> loadState = null,

			// IAlmanacPage
			bool magicTheme = false,
			bool calendar = false,

			Action<IClickableMenu> onActivate = null,
			Action<IClickableMenu> onDeactivate = null,
			Action<IClickableMenu, WorldDate, WorldDate> onDateChange = null,

			Action<IClickableMenu> onUpdateComponents = null,
			Func<IClickableMenu, ClickableComponent> getComponents = null,

			Func<IClickableMenu, Buttons, bool> onGamePadButton = null,
			Func<IClickableMenu, Keys, bool> onKeyPress = null,
			Func<IClickableMenu, int, int, int, bool> onScroll = null,
			Func<IClickableMenu, int, int, bool, bool> onLeftClick = null,
			Func<IClickableMenu, int, int, bool, bool> onRightClick = null,
			Action<IClickableMenu, int, int, Action<string>, Action<Item>> onHover = null,
			Action<IClickableMenu, SpriteBatch> onDraw = null,

			// ITab
			int tabSort = 100,
			bool? tabMagic = null,
			Func<IClickableMenu, string> tabTooltip = null,
			Func<IClickableMenu, Texture2D> tabTexture = null,
			Func<IClickableMenu, Rectangle?> tabSource = null,
			Func<IClickableMenu, float?> tabScale = null,

			// ICalendar
			Func<IClickableMenu, bool> dimPastCells = null,
			Func<IClickableMenu, bool> highlightToday = null,
			Action<IClickableMenu, SpriteBatch, WorldDate, Rectangle> onDrawUnderCell = null,
			Action<IClickableMenu, SpriteBatch, WorldDate, Rectangle> onDrawOverCell = null,

			Func<IClickableMenu, int, int, WorldDate, Rectangle, bool> onCellLeftClick = null,
			Func<IClickableMenu, int, int, WorldDate, Rectangle, bool> onCellRightClick = null,
			Action<IClickableMenu, int, int, WorldDate, Rectangle, Action<string>, Action<Item>> onCellHover = null
		);

		void UnregisterPage(IManifest manifest, string id);*/

		#endregion

		#region Crops Page

		void AddCropProvider(ICropProvider provider);

		void RemoveCropProvider(ICropProvider provider);

		void SetCropPriority(IManifest manifest, int priority);

		void SetCropCallback(IManifest manifest, Action action);

		void ClearCropCallback(IManifest manifest);

		void AddCrop(
			IManifest manifest,

			string id,

			Item item,
			string name,

			bool isTrellisCrop,
			bool isGiantCrop,
			bool isPaddyCrop,

			IList<int> phases,
			IList<Texture2D> phaseSpriteTextures,
			IList<Rectangle?> phaseSpriteSources,
			IList<Color?> phaseSpriteColors,
			IList<Texture2D> phaseSpriteOverlayTextures,
			IList<Rectangle?> phaseSpriteOverlaySources,
			IList<Color?> phaseSpriteOverlayColors,

			int regrow,

			WorldDate start,
			WorldDate end
		);

		void AddCrop(
			IManifest manifest,

			string id,

			Item item,
			string name,

			Texture2D spriteTexture,
			Rectangle? spriteSource,
			Color? spriteColor,
			Texture2D spriteOverlayTexture,
			Rectangle? spriteOverlaySource,
			Color? spriteOverlayColor,

			bool isTrellisCrop,
			bool isGiantCrop,
			bool isPaddyCrop,

			IList<int> phases,
			IList<Texture2D> phaseSpriteTextures,
			IList<Rectangle?> phaseSpriteSources,
			IList<Color?> phaseSpriteColors,
			IList<Texture2D> phaseSpriteOverlayTextures,
			IList<Rectangle?> phaseSpriteOverlaySources,
			IList<Color?> phaseSpriteOverlayColors,

			int regrow,

			WorldDate start,
			WorldDate end
		);

		void AddCrop(
			IManifest manifest,

			string id,

			Item item,
			string name,
			SpriteInfo sprite,

			bool isTrellisCrop,
			bool isGiantCrop,
			bool isPaddyCrop,

			IEnumerable<int> phases,
			IEnumerable<SpriteInfo> phaseSprites,

			int regrow,

			WorldDate start,
			WorldDate end
		);

		void RemoveCrop(IManifest manifest, string id);

		void ClearCrops(IManifest manifest);

		List<CropInfo> GetSeasonCrops(int season);

		List<CropInfo> GetSeasonCrops(string season);

		void InvalidateCrops();

		#endregion

		#region Fortunes Page

		/// <summary>
		/// Register a new hook for describing random nightly events for Almanac
		/// to list in its Fortune page. A hook function is called whenever we
		/// need to check what nightly events will happen on a given day, and
		/// it's called with the unique world ID as the first parameter and the
		/// date we want to know about as a WorldDate for the second argument.
		///
		/// The function is expected to return one or more tuples containing,
		/// in order, the following:
		///
		/// 1. A boolean that, if true, hides the vanilla generated event for
		///    that night.
		///
		/// 2. A string that is displayed to the user in the Almanac. This
		///    supports Almanac's rich text formatting.
		///
		/// 3. An optional texture, with a Texture2D and source Rectangle?
		///
		/// 4. An optional item. The item is used for compatibility with
		///    Lookup Anything. Users will be able to hover over the
		///    entry and open Lookup Anything to the item.
		///
		///    Additionally, if no sprite is provided but an item is provider,
		///    the item will be used as a sprite. To disable this behavior,
		///    return Rectangle.Empty for the source rectangle.
		/// 
		/// </summary>
		/// <param name="manifest">The manifest of the mod registering a hook</param>
		/// <param name="hook">The hook function</param>
		void SetFortuneHook(IManifest manifest, Func<int, WorldDate, IEnumerable<Tuple<bool, string, Texture2D, Rectangle?, Item>>> hook);

		/// <summary>
		/// Register a new hook. This is similar to the previous function, but
		/// rather than returning a string and texture separately, here we
		/// just return an IRichEvent using our knowledge of Almanac's interfaces.
		/// </summary>
		/// <param name="manifest">The manifest of the mod registering a hook</param>
		/// <param name="hook">The hook function</param>
		void SetFortuneHook(IManifest manifest, Func<int, WorldDate, IEnumerable<Tuple<bool, IRichEvent>>> hook);

		/// <summary>
		/// Unregister the fortunes hook for the given mod.
		/// </summary>
		/// <param name="manifest">The manifest of the mod</param>
		void ClearFortuneHook(IManifest manifest);

		#endregion

		#region Notices Page

		/// <summary>
		/// Register a new hook for describing daily events for Almanac
		/// to list in its Local Notices page. A hook function is called whenever we
		/// need to check what daily notices should be displayed, and
		/// it's called with the unique world ID as the first parameter and the
		/// date we want to know about as a WorldDate for the second argument.
		///
		/// The function is expected to return one or more tuples containing,
		/// in order, the following:
		///
		/// 1. A string that is displayed to the user in the Almanac. This
		///    supports Almanac's rich text formatting.
		///
		/// 2. An optional texture, with a Texture2D and source Rectangle?
		///
		/// 3. An optional item. The item is used for compatibility with
		///    Lookup Anything. Users will be able to hover over the
		///    entry and open Lookup Anything to the item.
		///
		///    Additionally, if no sprite is provided but an item is provider,
		///    the item will be used as a sprite. To disable this behavior,
		///    return Rectangle.Empty for the source rectangle.
		/// 
		/// </summary>
		/// <param name="manifest">The manifest of the mod registering a hook</param>
		/// <param name="hook">The hook function</param>
		void SetNoticesHook(IManifest manifest, Func<int, WorldDate, IEnumerable<Tuple<string, Texture2D, Rectangle?, Item>>> hook);

		/// <summary>
		/// Register a new hook. This is similar to the previous function, but
		/// rather than returning a string and texture separately, here we
		/// just return an IRichEvent using our knowledge of Almanac's interfaces.
		/// </summary>
		/// <param name="manifest">The manifest of the mod registering a hook</param>
		/// <param name="hook">The hook function</param>
		void SetNoticesHook(IManifest manifest, Func<int, WorldDate, IEnumerable<IRichEvent>> hook);

		/// <summary>
		/// Unregister the notices hook for the given mod.
		/// </summary>
		/// <param name="manifest">The manifest of the mod</param>
		void ClearNoticesHook(IManifest manifest);

		#endregion
	}

	public class ModAPI : IAlmanacAPI {
		private readonly ModEntry Mod;

		public ModAPI(ModEntry mod) {
			Mod = mod;
		}

		#region Crop Providers

		public void AddCropProvider(ICropProvider provider) {
			Mod.Crops.AddProvider(provider);
		}

		public void RemoveCropProvider(ICropProvider provider) {
			Mod.Crops.RemoveProvider(provider);
		}

		#endregion

		#region Manual Mod Crops

		public void InvalidateCrops() {
			Mod.Crops.Invalidate();
		}

		public void SetCropPriority(IManifest manifest, int priority) {
			var provider = Mod.Crops.GetModProvider(manifest, priority != 0);
			if (provider != null && provider.Priority != priority) {
				provider.Priority = priority;
				Mod.Crops.SortProviders();
			}
		}

		public void SetCropCallback(IManifest manifest, Action action) {
			var provider = Mod.Crops.GetModProvider(manifest, action != null);
			if (provider != null)
				provider.SetCallback(action);
		}

		public void ClearCropCallback(IManifest manifest) {
			SetCropCallback(manifest, null);
		}

		public void AddCrop(
			IManifest manifest,

			string id,

			Item item,
			string name,

			bool isTrellisCrop,
			bool isGiantCrop,
			bool isPaddyCrop,

			IList<int> phases,
			IList<Texture2D> phaseSpriteTextures,
			IList<Rectangle?> phaseSpriteSources,
			IList<Color?> phaseSpriteColors,
			IList<Texture2D> phaseSpriteOverlayTextures,
			IList<Rectangle?> phaseSpriteOverlaySources,
			IList<Color?> phaseSpriteOverlayColors,

			int regrow,

			WorldDate start,
			WorldDate end
		) {
			List<SpriteInfo> phaseSprites = new();

			for(int i = 0; i < phases.Count; i++) {
				phaseSprites.Add(new(
					texture: phaseSpriteTextures[i],
					baseSource: phaseSpriteSources[i] ?? phaseSpriteTextures[i].Bounds,
					baseColor: phaseSpriteColors?[i],
					overlayTexture: phaseSpriteOverlayTextures?[i],
					overlaySource: phaseSpriteOverlaySources?[i],
					overlayColor: phaseSpriteOverlayColors?[i]
				));
			}

			AddCrop(
				manifest: manifest,
				id: id,
				item: item,
				name: name,
				sprite: item == null ? null : SpriteHelper.GetSprite(item, Mod.Helper),
				isTrellisCrop: isTrellisCrop,
				isGiantCrop: isGiantCrop,
				isPaddyCrop: isPaddyCrop,
				phases: phases,
				phaseSprites: phaseSprites,
				regrow: regrow,
				start: start,
				end: end
			);
		}

		public void AddCrop(
			IManifest manifest,

			string id,

			Item item,
			string name,

			Texture2D spriteTexture,
			Rectangle? spriteSource,
			Color? spriteColor,
			Texture2D spriteOverlayTexture,
			Rectangle? spriteOverlaySource,
			Color? spriteOverlayColor,

			bool isTrellisCrop,
			bool isGiantCrop,
			bool isPaddyCrop,

			IList<int> phases,
			IList<Texture2D> phaseSpriteTextures,
			IList<Rectangle?> phaseSpriteSources,
			IList<Color?> phaseSpriteColors,
			IList<Texture2D> phaseSpriteOverlayTextures,
			IList<Rectangle?> phaseSpriteOverlaySources,
			IList<Color?> phaseSpriteOverlayColors,

			int regrow,

			WorldDate start,
			WorldDate end
		) {
			List<SpriteInfo> phaseSprites = new();

			for (int i = 0; i < phases.Count; i++) {
				phaseSprites.Add(new(
					texture: phaseSpriteTextures[i],
					baseSource: phaseSpriteSources[i] ?? phaseSpriteTextures[i].Bounds,
					baseColor: phaseSpriteColors?[i],
					overlayTexture: phaseSpriteOverlayTextures?[i],
					overlaySource: phaseSpriteOverlaySources?[i],
					overlayColor: phaseSpriteOverlayColors?[i]
				));
			}

			AddCrop(
				manifest: manifest,
				id: id,
				item: item,
				name: name,
				sprite: new SpriteInfo(
					spriteTexture,
					spriteSource ?? spriteTexture.Bounds,
					spriteColor,
					overlayTexture: spriteOverlayTexture,
					overlaySource: spriteOverlaySource,
					overlayColor: spriteOverlayColor
				),
				isTrellisCrop: isTrellisCrop,
				isGiantCrop: isGiantCrop,
				isPaddyCrop: isPaddyCrop,
				phases: phases,
				phaseSprites: phaseSprites,
				regrow: regrow,
				start: start,
				end: end
			);
		}

		public void AddCrop(
			IManifest manifest,

			string id,

			Item item,
			string name,
			SpriteInfo sprite,

			bool isTrellisCrop,
			bool isGiantCrop,
			bool isPaddyCrop,

			IEnumerable<int> phases,
			IEnumerable<SpriteInfo> phaseSprites,

			int regrow,

			WorldDate start,
			WorldDate end
		) {
			var provider = Mod.Crops.GetModProvider(manifest);
			provider.AddCrop(
				id: id,
				item: item,
				name: name,
				sprite: sprite,
				isTrellisCrop: isTrellisCrop,
				isGiantCrop: isGiantCrop,
				isPaddyCrop: isPaddyCrop,
				phases: phases,
				phaseSprites: phaseSprites,
				regrow: regrow,
				start: start,
				end: end
			);
		}

		public void RemoveCrop(IManifest manifest, string id) {
			var provider = Mod.Crops.GetModProvider(manifest, false);
			if (provider != null)
				provider.RemoveCrop(id);
		}

		public void ClearCrops(IManifest manifest) {
			var provider = Mod.Crops.GetModProvider(manifest, false);
			if (provider != null)
				provider.ClearCrops();
		}

		#endregion

		#region Get Crops

		public List<CropInfo> GetSeasonCrops(int season) {
			return Mod.Crops.GetSeasonCrops(season);
		}

		public List<CropInfo> GetSeasonCrops(string season) {
			return Mod.Crops.GetSeasonCrops(season);
		}

		#endregion

		#region Fortune Telling

		public void SetFortuneHook(IManifest manifest, Func<int, WorldDate, IEnumerable<Tuple<bool, string, Texture2D, Rectangle?, Item>>> hook) {
			Mod.Luck.RegisterHook(manifest, hook);
		}

		public void SetFortuneHook(IManifest manifest, Func<int, WorldDate, IEnumerable<Tuple<bool, IRichEvent>>> hook) {
			Mod.Luck.RegisterHook(manifest, hook);
		}

		public void ClearFortuneHook(IManifest manifest) {
			Mod.Luck.ClearHook(manifest);
		}

		#endregion

		#region Local Notices

		public void SetNoticesHook(IManifest manifest, Func<int, WorldDate, IEnumerable<Tuple<string, Texture2D, Rectangle?, Item>>> hook) {
			Mod.Notices.RegisterHook(manifest, hook);
		}

		public void SetNoticesHook(IManifest manifest, Func<int, WorldDate, IEnumerable<IRichEvent>> hook) {
			Mod.Notices.RegisterHook(manifest, hook);
		}

		public void ClearNoticesHook(IManifest manifest) {
			Mod.Notices.ClearHook(manifest);
		}

		#endregion

	}
}
