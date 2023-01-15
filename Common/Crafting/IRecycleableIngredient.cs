using System;
using System.Collections.Generic;
using System.Text;

using StardewValley;

namespace Leclair.Stardew.Common.Crafting;

public interface IRecycleableIngredient {

	/// <summary>
	/// Get an enumeration of <see cref="Item"/>s that should be returned by
	/// this ingredient when 
	/// </summary>
	/// <param name="who"></param>
	/// <param name="recycledItem"></param>
	/// <returns></returns>
	IEnumerable<Item> Recycle(Farmer who, Item? recycledItem);

}
