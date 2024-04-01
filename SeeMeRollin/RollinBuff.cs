
using StardewValley;

namespace Leclair.Stardew.SeeMeRollin {
	public class RollinBuff : Buff {

		public int Speed;

		public RollinBuff(int speed) : base(I18n.Buff_Name(), 1000, null, 9) {
			which = ModEntry.BUFF;

			Speed = speed;
			buffAttributes[Buff.speed] = Speed;
		}

	}
}
