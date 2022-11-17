using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley.BellsAndWhistles;
using StardewModdingAPI;

using Leclair.Stardew.Common;

namespace Leclair.Stardew.ThemeManager;

public class ModAPI : IThemeManagerApi {

	internal readonly ModEntry Mod;
	internal readonly IManifest Other;

	internal ModAPI(ModEntry mod, IManifest other) {
		Mod = mod;
		Other = other;

		Mod.BaseThemeManager!.ThemeChanged += BaseThemeManager_ThemeChanged;
	}

	private void BaseThemeManager_ThemeChanged(object? sender, IThemeChangedEvent<Models.BaseTheme> e) {
		if (e is IThemeChangedEvent<IBaseTheme> bt)
			BaseThemeChanged?.Invoke(sender, bt);
	}

	#region Base Theme

	public IBaseTheme BaseTheme => Mod.BaseTheme!;

	public event EventHandler<IThemeChangedEvent<IBaseTheme>>? BaseThemeChanged;

	#endregion

	#region Custom Themes

	public ITypedThemeManager<DataT> GetOrCreateManager<DataT>(DataT? defaultTheme = null, string? embeddedThemesPath = "assets/themes", string? assetPrefix = "assets", string? assetLoaderPrefix = null, bool? forceAssetRedirection = null) where DataT : class, new() {
		ITypedThemeManager<DataT>? manager;
		lock ((Mod.Managers as ICollection).SyncRoot) {
			if (Mod.Managers.TryGetValue(Other, out var mdata)) {
				manager = mdata.Item2 as ITypedThemeManager<DataT>;
				if (manager is null || mdata.Item1 != typeof(DataT))
					throw new InvalidCastException($"Cannot convert {mdata.Item1} to {typeof(DataT)}");

				return manager;
			}
		}

		if (!Mod.Config.SelectedThemes.TryGetValue(Other.UniqueID, out string? selected))
			selected = "automatic";

		manager = new ThemeManager<DataT>(
			mod: Mod,
			other: Other,
			selectedThemeId: selected,
			defaultTheme: defaultTheme,
			embeddedThemesPath: embeddedThemesPath,
			assetPrefix: assetPrefix,
			assetLoaderPrefix: assetLoaderPrefix,
			forceAssetRedirection: forceAssetRedirection
		);

		lock ((Mod.Managers as ICollection).SyncRoot) {
			Mod.Managers[Other] = (typeof(DataT), manager);
		}

		manager.Discover();
		return manager;
	}

	public bool TryGetManager([NotNullWhen(true)] out IThemeManager? themeManager, IManifest? forMod = null) {
		lock((Mod.Managers as ICollection).SyncRoot) {
			if (!Mod.Managers.TryGetValue(forMod ?? Other, out var manager)) {
				themeManager = null;
				return false;
			}

			themeManager = manager.Item2;
			return true;
		}
	}

	public bool TryGetManager<DataT>([NotNullWhen(true)] out ITypedThemeManager<DataT>? themeManager, IManifest? forMod = null) where DataT : class, new() {
		lock ((Mod.Managers as ICollection).SyncRoot) {
			if (!Mod.Managers.TryGetValue(forMod ?? Other, out var manager) || manager.Item1 != typeof(DataT)) {
				themeManager = null;
				return false;
			}

			themeManager = manager.Item2 as ITypedThemeManager<DataT>;
			return themeManager is not null;
		}
	}

	#endregion

	#region Color Parsing

	public bool TryParseColor(string value, [NotNullWhen(true)] out Color? color) {
		color = CommonHelper.ParseColor(value);
		return color.HasValue;
	}

	#endregion

	#region Colored SpriteText

	public void DrawSpriteText(SpriteBatch batch, string text, int x, int y, Color? color, float alpha = 1f) {
		int c = color.HasValue ? CommonHelper.PackColor(color.Value) + 100 : -1;

		SpriteText.drawString(batch, text, x, y, alpha: alpha, color: c);
	}

	#endregion

}
