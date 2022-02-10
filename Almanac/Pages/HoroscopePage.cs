using System.Collections.Generic;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI.Utilities;
using StardewValley;

using Leclair.Stardew.Almanac.Menus;

namespace Leclair.Stardew.Almanac.Pages {
	public class HoroscopePage : BasePage, ICalendarPage {

		private readonly int Seed;
		private double[] Luck;

		private IEnumerable<IFlowNode> Flow;

		#region Lifecycle

		public static HoroscopePage GetPage(AlmanacMenu menu, ModEntry mod) {
			if (!mod.Config.ShowHoroscopes)
				return null;

			return new(menu, mod);
		}

		public HoroscopePage(AlmanacMenu menu, ModEntry mod) : base(menu, mod) {
			Seed = Mod.GetBaseWorldSeed();

			UpdateLuck();
		}

		#endregion

		#region Logic

		public void UpdateLuck() {
			Luck = new double[WorldDate.DaysPerMonth];
			WorldDate date = new(Menu.Date);

			for (int day = 1; day <= WorldDate.DaysPerMonth; day++) {
				date.DayOfMonth = day;
				double luck = Luck[day - 1] = LuckHelper.GetLuckForDate(Seed, date) * 100;
			}
		}

		#endregion

		#region ITab

		public override int SortKey => 10;
		public override string TabSimpleTooltip => "Horoscope";

		public override Texture2D TabTexture => Game1.mouseCursors;

		public override Rectangle? TabSource => SpriteHelper.MouseIcons.BACKPACK;

		#endregion

		#region IAlmanacPage

		public override void DateChanged(WorldDate oldDate, WorldDate newDate) {
			UpdateLuck();
		}

		#endregion

		#region ICalendarPage

		public bool ShouldDimPastCells => true;
		public bool ShouldHighlightToday => true;

		public void DrawUnderCell(SpriteBatch b, WorldDate date, Rectangle bounds) {

			if (Luck == null)
				return;

			double luck = Luck[date.DayOfMonth - 1];

			string l = $"{luck.ToString("F1")}%";

			Vector2 size = Game1.smallFont.MeasureString(l);

			b.DrawString(
				Game1.smallFont,
				l,
				new Vector2(
					bounds.Center.X - size.X / 2,
					bounds.Center.Y - size.Y / 2
				),
				Game1.textColor
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
			if (Luck == null)
				return;

			double luck = Luck[date.DayOfMonth - 1];

			Menu.HoverText = $"{luck.ToString("F1")}%";
		}

		#endregion

	}
}
