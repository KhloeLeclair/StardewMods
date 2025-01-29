using ItemExtensions;

using Leclair.Stardew.Common.Integrations;

using Microsoft.Xna.Framework;

using StardewValley;

namespace Leclair.Stardew.CloudySkies.Integrations.ItemExtensions;

public class IEIntegration : BaseAPIIntegration<IItemExtensionsApi, ModEntry> {

	public IEIntegration(ModEntry mod) : base(mod, "mistyspring.ItemExtensions", "1.11") { }

	public bool IsClump(string itemId) {
		return IsLoaded && API.IsClump(itemId);
	}

	public bool TrySpawnClump(string itemId, Vector2 position, GameLocation location, out string? error, bool avoidOverlap = false) {
		if (!IsLoaded) {
			error = "Item Extensions is not loaded";
			return false;
		}

		return API.TrySpawnClump(itemId, position, location, out error, avoidOverlap);
	}

	public bool IsResource(string itemId, out int? health) {
		if (!IsLoaded) {
			health = null;
			return false;
		}

		return API.IsResource(itemId, out health, out _);
	}

}
