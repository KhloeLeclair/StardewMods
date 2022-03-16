using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Models;

/// <summary>
/// Theme records group together a theme's <typeparamref name="DataT"/>
/// and <see cref="IContentPack"/> instances for convenience.
/// </summary>
/// <typeparam name="DataT">Your mod's BaseThemeData subclass</typeparam>
/// <param name="Data">The theme's theme data.</param>
/// <param name="Content">The theme's content pack.</param>
internal record Theme<DataT>(
	DataT Data,
	ThemeManifest Manifest,
	IContentPack? Content,
	string? RelativePath
);
