using Microsoft.Xna.Framework;

using Leclair.Stardew.Common.Enums;


namespace Leclair.Stardew.Almanac.Models {

	public enum NoticeIconType {
		Item,
		Texture
	}

	public enum NoticePeriod {
		Week,
		Season,
		Year,
		Total
	};

	public enum NoticeSeason {
		Spring = 0,
		Summer = 1,
		Fall = 2,
		Winter = 3
	};

	public record struct DateRange(int Start, int End, int[] Valid = null);

	public class LocalNotice {

		// When
		public NoticePeriod Period { get; set; }
		public DateRange[] Ranges { get; set; }

		public int FirstYear { get; set; } = 1;
		public int LastYear { get; set; } = int.MaxValue;
		public int[] ValidYears { get; set; } = null;

		public NoticeSeason[] ValidSeasons { get; set; } = new NoticeSeason[] {
			NoticeSeason.Spring,
			NoticeSeason.Summer,
			NoticeSeason.Fall,
			NoticeSeason.Winter
		};


		// Text
		public bool ShowEveryDay { get; set; } = false;

		public string Description { get; set; }

		// Icon
		public NoticeIconType IconType { get; set; }

		// Item
		public string Item { get; set; }

		// Texture
		public GameTexture? IconSource { get; set; } = null;
		public string IconPath { get; set; }
		public Rectangle? IconSourceRect { get; set; }
	}
}
