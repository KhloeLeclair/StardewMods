
using StardewValley;

namespace Leclair.Stardew.SeeMeRollin {
	public class RollinBuff : Buff {

		public int Speed;

		public RollinBuff(int speed) : base(
			ModEntry.BUFF,
			duration: 1,
			icon_texture: Game1.buffsIcons,
			icon_sheet_index: 9,
			display_name: I18n.Buff_Name()
		) {
			effects.speed.Value = speed;
			Speed = speed;
		}
	}
}
