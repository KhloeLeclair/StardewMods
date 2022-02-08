using System;
using System.Collections.Generic;

using Leclair.Stardew.Almanac.Menus;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI.Utilities;

using StardewValley;

namespace Leclair.Stardew.Almanac.Pages {
	public class WeatherPage : BasePage, ICalendarPage {

		private readonly int Seed;
		private int[] Forecast;

		private IEnumerable<IFlowNode> Flow;

		#region Lifecycle

		public static WeatherPage GetPage(AlmanacMenu menu, ModEntry mod) {
			if (!mod.Config.ShowWeather || !mod.Config.EnableDeterministicWeather)
				return null;

			return new(menu, mod);
		}

		public WeatherPage(AlmanacMenu menu, ModEntry mod) : base(menu, mod) {
			Seed = Mod.GetBaseWeatherSeed();
			UpdateForecast();
		}

		#endregion

		#region Logic

		public void UpdateForecast() {
			Forecast = new int[WorldDate.DaysPerMonth];
			WorldDate date = new(Menu.Date);

			FlowBuilder builder = new();

			for (int day = 1; day <= WorldDate.DaysPerMonth; day++) {
				date.DayOfMonth = day;
				Forecast[day - 1] = WeatherHelper.GetWeatherForDate(Seed, date, GameLocation.LocationContext.Default);

				if (Utility.isFestivalDay(day, date.Season)) {
					SDate sdate = new(day, date.Season);

					var data = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + date.Season + day);
					if (!data.ContainsKey("name") || !data.ContainsKey("conditions"))
						continue;

					string name = data["name"];
					string[] conds = data["conditions"].Split('/');
					string where = conds.Length >= 1 ? conds[0] : null;

					int start = -1;
					int end = -1;

					if (conds.Length >= 2) {
						string[] bits = conds[1].Split(' ');
						if (bits.Length >= 2) {
							start = Convert.ToInt32(bits[0]);
							end = Convert.ToInt32(bits[1]);
						}
					}

					builder.Text($"{name}\n", font: Game1.dialogueFont, shadow: true);
					builder.Text($"  {I18n.Festival_Date()} ", shadow: false);
					builder.Text($"{sdate.ToLocaleString(withYear: false)}\n");

					builder.Text($"  {I18n.Festival_Where()} ", shadow: false);
					builder.Text($"{where}\n");

					if (start >= 0 && end >= 0) {
						builder.Text($"  {I18n.Festival_When()} ", shadow: false);
						builder.Translate(Mod.Helper.Translation.Get("festival.when-times"), new {
							start = TimeHelper.FormatTime(start),
							end = TimeHelper.FormatTime(end)
						}, new TextStyle(shadow: false));
						builder.Text("\n\n");
					}
				}
			}

			Flow = builder.Build();
			if (Active)
				Menu.SetFlow(Flow, 2);
		}

		#endregion

		#region ITab

		public override int SortKey => 1;

		public override string TabSimpleTooltip => I18n.Page_Weather();

		public override Texture2D TabTexture => Menu.background;

		public override Rectangle? TabSource => WeatherHelper.GetWeatherIcon(0, null);

		#endregion

		#region IAlmanacPage

		public override void Activate() {
			base.Activate();
			Menu.SetFlow(Flow, 2);
		}

		public override void DateChanged(WorldDate old, WorldDate newDate) {
			UpdateForecast();
		}

		#endregion

		#region ICalendarPage

		public bool ShouldDimPastCells => true;
		public bool ShouldHighlightToday => true;

		public void DrawUnderCell(SpriteBatch b, WorldDate date, Rectangle bounds) {
			if (Forecast == null)
				return;

			Utility.drawWithShadow(
				b,
				Menu.background,
				new Vector2(
					bounds.X + (bounds.Width - 72) / 2,
					bounds.Y + (bounds.Height - 72) / 2
				),
				WeatherHelper.GetWeatherIcon(Forecast[date.DayOfMonth - 1], date.Season),
				Color.White,
				0f,
				Vector2.Zero,
				scale: 4f,
				horizontalShadowOffset: 0
			);
		}

		public void DrawOverCell(SpriteBatch b, WorldDate date, Rectangle bounds) {

		}

		public bool ReceiveCellLeftClick(int x, int y, WorldDate date, Rectangle bounds) {
			return false;
		}

		public bool ReceiveCellRightClick(int x, int y, WorldDate date, Rectangle bounds) {
			return false;
		}

		public void PerformCellHover(int x, int y, WorldDate date, Rectangle bounds) {
			if (Forecast == null)
				return;

			int weather = Forecast[date.DayOfMonth - 1];
			Menu.HoverText = WeatherHelper.LocalizeWeather(weather);
		}

		#endregion

	}
}
