using Leclair.Stardew.Almanac.Menus;

using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Leclair.Stardew.Almanac.Pages {
	public class CoverPage : BasePage<BaseState> {

		private string[] words;
		private int wordHeight;

		#region Lifecycle

		public static CoverPage GetPage(AlmanacMenu menu, ModEntry mod) {
			return new(menu, mod);
		}

		public CoverPage(AlmanacMenu menu, ModEntry mod) : base(menu, mod) {
			// Cache the string.
			words = (Mod.HasIsland(Game1.player) ?
				I18n.Almanac_CoverIsland() : I18n.Almanac_Cover()
			).Split('\n');
			wordHeight = 0;
			foreach (string word in words) {
				int height = SpriteText.getHeightOfString(word);
				if (height > wordHeight)
					wordHeight = height;
			};
		}

		#endregion

		#region ITab

		public override int SortKey => int.MinValue;

		#endregion

		#region IAlmanacPage

		public override PageType Type => PageType.Cover;

		public override void Activate() {
			base.Activate();

			
		}

		public override void Draw(SpriteBatch b) {
			if (words == null)
				return;

			int center = Menu.xPositionOnScreen + (Menu.width / 2);
			int titleHeight = words.Length * wordHeight;
			int y = Menu.yPositionOnScreen + (Menu.height - (titleHeight + 60 + wordHeight)) / 2;

			foreach (string word in words) {
				SpriteText.drawStringHorizontallyCenteredAt(
					b,
					word,
					center,
					y
				);

				y += wordHeight;
			}

			SpriteText.drawStringHorizontallyCenteredAt(
				b,
				Game1.content.LoadString("Strings\\UI:Billboard_Year", Menu.Year),
				center,
				y + 60,
				color: 2
				);
		}

		#endregion
	}
}
