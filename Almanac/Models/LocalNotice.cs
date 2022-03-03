using Microsoft.Xna.Framework;

using Leclair.Stardew.Common.Enums;


namespace Leclair.Stardew.Almanac.Models {
	public class LocalNotice {

		// Text
		public string Description { get; set; }


		// Icon

		public GameTexture? Source { get; set; } = null;
		public string Path { get; set; }

		public Rectangle? Rect { get; set; }

	}
}
