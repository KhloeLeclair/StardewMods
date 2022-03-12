using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;

namespace Leclair.Stardew.Almanac {

	public interface IAlmanacAPI {

		int DaysPerMonth { get; }

		void SetCropPriority(IManifest manifest, int priority);

		void SetCropCallback(IManifest manifest, Action action);
		void ClearCropCallback(IManifest manifest);

		void AddCrop(
			IManifest manifest,

			string id,

			Item item,
			string name,

			int regrow,
			bool isGiantCrop,
			bool isPaddyCrop,
			bool isTrellisCrop,

			WorldDate start,
			WorldDate end,

			Tuple<Texture2D, Rectangle?, Color?, Texture2D, Rectangle?, Color?> sprite,
			Tuple<Texture2D, Rectangle?, Color?, Texture2D, Rectangle?, Color?> giantSprite,

			IReadOnlyCollection<int> phases,
			IReadOnlyCollection<Tuple<Texture2D, Rectangle?, Color?, Texture2D, Rectangle?, Color?>> phaseSprites
		);

		void RemoveCrop(IManifest manifest, string id);

		void ClearCrops(IManifest manifest);

		void InvalidateCrops();

	}
}
