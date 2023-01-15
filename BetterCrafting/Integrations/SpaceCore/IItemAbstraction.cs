using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Integrations.SpaceCore;

public interface IItemAbstraction {

	int Quantity { get; }

	Item Create();

}
