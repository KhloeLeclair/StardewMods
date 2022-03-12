using System.Linq;

using Leclair.Stardew.Common;

using StardewValley;

namespace Leclair.Stardew.Almanac {
	public struct CropInfo {

		// Deduplication
		public string Id { get; }

		// Main Display
		public Item Item { get; }
		public string Name { get; }
		public SpriteInfo Sprite { get; }

		public bool IsTrellisCrop { get; }
		public bool IsGiantCrop { get; }
		public SpriteInfo GiantSprite { get; }


		// Phases
		public int[] Phases { get; }
		public SpriteInfo[] PhaseSprites { get; }

		public int Days { get; }
		public int Regrow { get; }
		public bool IsPaddyCrop { get; }

		public WorldDate StartDate { get; }
		public WorldDate EndDate { get; }

		public CropInfo(string id, Item item, string name, SpriteInfo sprite, bool giantCrop, SpriteInfo giantSprite, bool trellisCrop, int[] phases, int regrow, bool paddyCrop, SpriteInfo[] phaseSprites, WorldDate startDate, WorldDate endDate) {
			Id = id;
			Item = item;
			Name = name;
			Sprite = sprite;
			IsGiantCrop = giantCrop;
			GiantSprite = giantSprite;
			IsTrellisCrop = trellisCrop;
			Phases = phases;
			Regrow = regrow;
			IsPaddyCrop = paddyCrop;
			PhaseSprites = phaseSprites;
			Days = Phases.Sum();
			StartDate = startDate;
			EndDate = endDate;
		}
	}
}
