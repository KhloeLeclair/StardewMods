using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using StardewValley;

namespace Leclair.Stardew.CloudySkies.Models;

public interface IEffect {

	ulong Id { get; }

	uint Rate { get; }

	void ReloadAssets() { }

	void Update(GameTime time);

	void Remove() { }

}
