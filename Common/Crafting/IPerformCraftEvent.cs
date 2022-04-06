
using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.Common.Crafting;
public interface IPerformCraftEvent {

	Farmer Player { get; }
	Item Item { get; set; }

	IClickableMenu Menu { get; }

	void Cancel();
	void Complete();
}
