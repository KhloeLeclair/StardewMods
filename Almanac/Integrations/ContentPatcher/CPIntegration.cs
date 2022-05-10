#nullable enable

using Leclair.Stardew.Common.Integrations;

using StardewValley;
using StardewModdingAPI;

using ContentPatcher;

namespace Leclair.Stardew.Almanac.Integrations.ContentPatcher;

public class CPIntegration : BaseAPIIntegration<IContentPatcherAPI, ModEntry> {

	public CPIntegration(ModEntry mod)
	: base(mod, "Pathoschild.ContentPatcher", "1.25") {
		if (!IsLoaded)
			return;

		API.RegisterToken(Self.ModManifest, "HasBase", () => {
			if (Context.IsWorldReady)
				return new[] {
					Self.HasAlmanac(Game1.player).ToString()
				};

			if (SaveGame.loaded?.player != null)
				return new[] {
					Self.HasAlmanac(SaveGame.loaded.player).ToString()
				};

			return null;
		});

		API.RegisterToken(Self.ModManifest, "HasMagic", () => {
			if (Context.IsWorldReady)
				return new[] {
					Self.HasMagic(Game1.player).ToString()
				};

			if (SaveGame.loaded?.player != null)
				return new[] {
					Self.HasMagic(SaveGame.loaded.player).ToString()
				};

			return null;
		});

		API.RegisterToken(Self.ModManifest, "HasIsland", () => {
			if (Context.IsWorldReady)
				return new[] {
					Self.HasIsland(Game1.player).ToString()
				};

			if (SaveGame.loaded?.player != null)
				return new[] {
					Self.HasIsland(SaveGame.loaded.player).ToString()
				};

			return null;
		});
	}
}
