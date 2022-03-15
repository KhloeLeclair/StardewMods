using System;
using Newtonsoft.Json;

using Microsoft.Xna.Framework;

using Leclair.Stardew.Common.Serialization.Converters;
using Leclair.Stardew.Common.UI;

namespace Leclair.Stardew.Almanac.Models {
	public class Theme : BaseThemeData {

		public int CoverTextColor { get; set; } = -1;
		public int CoverYearColor { get; set; } = -1;


		public Style Standard { get; set; } = new Style {
			SeasonTextColor = -1
		};

		public Style Magic { get; set; } = new Style {
			SeasonTextColor = 4
		};

	}

	public class Style {

		public int SeasonTextColor { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? CalendarLabelColor { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? CalendarDayColor { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? CalendarDimColor { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? CalendarHighlightColor { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? TextColor { get; set; }

		[JsonConverter(typeof(ColorConverter))]
		public Color? TextShadowColor { get; set; }

	}
}
