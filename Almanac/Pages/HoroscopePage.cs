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

		public static readonly Rectangle CRYSTAL_BALL = new(272, 352, 16, 16);

		private readonly int Seed;
		private double[] Luck;
		private SpriteInfo[] Sprites;
		private SpriteInfo[] Extras;
		private IFlowNode[] Nodes;

		private IEnumerable<IFlowNode> Flow;

		#region Lifecycle

		public static HoroscopePage GetPage(AlmanacMenu menu, ModEntry mod) {
			if (!mod.Config.ShowHoroscopes)
				return null;

			if (!mod.HasMagic(Game1.player))
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
			Sprites = new SpriteInfo[WorldDate.DaysPerMonth];
			Extras = new SpriteInfo[WorldDate.DaysPerMonth];
			Nodes = new IFlowNode[WorldDate.DaysPerMonth];
			WorldDate date = new(Menu.Date);

			FlowBuilder builder = new();

			bool had_event = false;

			for (int day = 1; day <= WorldDate.DaysPerMonth; day++) {
				date.DayOfMonth = day;
				double luck = LuckHelper.GetLuckForDate(Seed, date);
				SpriteInfo sprite = Sprites[day - 1] = LuckHelper.GetLuckSprite(luck);
				Luck[day - 1] = luck;

				LuckHelper.IHoroscopeEvent evt = LuckHelper.GetEventForDate(Seed, date);
				if ( evt == null )
					evt = LuckHelper.GetTrashEvent(Seed, date);

				if ( evt == null )
					continue;

				had_event = true;

				SDate sdate = new(day, date.Season);

				IFlowNode node = new SpriteNode(sprite, 3, Alignment.Bottom, size: 13);
				Nodes[day - 1] = node;

				builder
					.Add(node)
					.Text($" {sdate.ToLocaleString(withYear: false)}\n", font: Game1.dialogueFont);

				builder.Text("  ");

				if (evt.Sprite != null)
					Extras[day - 1] = evt.Sprite;

				if (evt.AdvancedLabel != null)
					builder.AddRange(evt.AdvancedLabel);
				else if (!string.IsNullOrEmpty(evt.SimpleLabel))
					builder.Text(evt.SimpleLabel);

				builder.Text("\n\n");
			}

			if (!had_event)
				builder.Text(I18n.Page_Fortune_Event_None());

			Flow = builder.Build();
			if (Active)
				Menu.SetFlow(Flow, 2);
		}

		#endregion

		#region ITab

		public override int SortKey => 10;
		public override string TabSimpleTooltip => I18n.Page_Horoscope();

		public override Texture2D TabTexture => Menu.background;

		public override Rectangle? TabSource => CRYSTAL_BALL;

		#endregion

		#region IAlmanacPage

		public override bool IsMagic => true;

		public override void Activate() {
			base.Activate();
			Menu.SetFlow(Flow, 2);
		}

		public override void DateChanged(WorldDate oldDate, WorldDate newDate) {
			UpdateLuck();
		}

		#endregion

		#region ICalendarPage

		public bool ShouldDimPastCells => true;
		public bool ShouldHighlightToday => true;

		public void DrawUnderCell(SpriteBatch b, WorldDate date, Rectangle bounds) {

			if (Sprites == null)
				return;

			SpriteInfo extra = null;
			SpriteInfo sprite = Sprites[date.DayOfMonth - 1];
			if (sprite == null)
				return;

			if (Extras != null)
				extra = Extras[date.DayOfMonth - 1];

			sprite.Draw(
				b,
				new Vector2(
					bounds.Center.X - 39 / 2,
					bounds.Center.Y - 39 / 2 - (extra != null ? 16 : 0)
				),
				3,
				size: 13
			);

			extra?.Draw(
				b,
				new Vector2(
					bounds.Right - 40,
					bounds.Bottom - 40
				),
				2
			);

			/*double luck = Luck[date.DayOfMonth - 1];

			string l = $"{luck:F1}%";

			Vector2 size = Game1.smallFont.MeasureString(l);

			b.DrawString(
				Game1.smallFont,
				l,
				new Vector2(
					bounds.Center.X - size.X / 2,
					bounds.Center.Y - size.Y / 2
				),
				Color.White
			);*/
		}

		public void DrawOverCell(SpriteBatch b, WorldDate date, Rectangle bounds) {
			
		}

		public bool ReceiveCellLeftClick(int x, int y, WorldDate date, Rectangle bounds) {
			if (Nodes == null)
				return false;

			IFlowNode node = Nodes[date.DayOfMonth - 1];
			if (node != null) {
				if (Menu.ScrollFlow(node)) {
					Game1.playSound("shiny4");
					return true;
				}
			}

			return false;
		}

		public bool ReceiveCellRightClick(int x, int y, WorldDate date, Rectangle bounds) {
			return false;
		}

		public void PerformCellHover(int x, int y, WorldDate date, Rectangle bounds) {
			if (Luck == null)
				return;

			double luck = Luck[date.DayOfMonth - 1];

			string fortune = LuckHelper.GetLuckText(luck);

			Menu.HoverText = $"{fortune} ({(luck*100):F1}%)";
		}

		#endregion

	}
}
