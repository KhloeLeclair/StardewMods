using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using StardewValley;
using SObject = StardewValley.Object;

using Leclair.Stardew.Almanac.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Leclair.Stardew.Almanac.Pages {
	public class FishingPage : BasePage<BaseState> {

		#region Life Cycle

		public static FishingPage GetPage(AlmanacMenu menu, ModEntry mod) {
			return new(menu, mod);
		}

		public FishingPage(AlmanacMenu menu, ModEntry mod) : base(menu, mod) {

		}

		#endregion

		#region ITab

		public override int SortKey => 40;
		public override string TabSimpleTooltip => "Fishing";
		public override Texture2D TabTexture => Game1.mouseCursors;
		public override Rectangle? TabSource => new(20, 428, 10, 10);

		#endregion

		#region IAlmanacPage

		public override void Update() {
			base.Update();

			object[] stuff = new object[] {
				"The following growth times and dates assume you are an ",
				new Tuple<Texture2D, Rectangle>(Game1.mouseCursors, new(80, 624, 16, 16)),
				" @B@C{forestgreen}Agriculturist@C@b. They also assume crops that benefit from it are @Bgrown near water@b.",
				"\n\n",
				new Tuple<Item, float>(new SObject(454, 1), 3f),
				" @T{dialog}Ancient Fruit\n",
				"Grows in 25 days. Regrows every 7 days. Expects 5 harvests this year. Plant no later than Fall 3.",
				"\n\n",
				new Tuple<Item, float>(new SObject(454, 1), 3f),
				" @T{dialog}Ancient Fruit\n",
				"Grows in 25 days. Regrows every 7 days. Expects 5 harvests this year. Plant no later than Fall 3.",
				"\n\n",
				new Tuple<Item, float>(new SObject(454, 1), 3f),
				" @T{dialog}Ancient Fruit\n",
				"Grows in 25 days. Regrows every 7 days. Expects 5 harvests this year. Plant no later than Fall 3.",
				"\n\n",
				new Tuple<Item, float>(new SObject(454, 1), 3f),
				" @T{dialog}Ancient Fruit\n",
				"Grows in 25 days. Regrows every 7 days. Expects 5 harvests this year. Plant no later than Fall 3."
			};

			List<IFlowNode> nodes = FlowHelper.GetNodes(stuff, Alignment.Middle, true, Mod.Helper);

			SetFlow(nodes);
		}


		#endregion

	}
}
