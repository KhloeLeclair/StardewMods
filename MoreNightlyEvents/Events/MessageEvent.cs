using Leclair.Stardew.MoreNightlyEvents.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.MoreNightlyEvents.Events;

public class MessageEvent : BaseFarmEvent<MessageEventData> {

	private int timer;

	private bool playedSound;
	private bool showedMessage;
	private bool finished;

	public MessageEvent() : base() { }

	public MessageEvent(string key, MessageEventData? data = null) : base(key, data) {

	}

	#region FarmEvent

	public override bool setUp() {
		if (!LoadData())
			return true;

		Game1.freezeControls = true;
		return false;
	}

	public override void InterruptEvent() {
		finished = true;
	}

	public override bool tickUpdate(GameTime time) {
		timer += time.ElapsedGameTime.Milliseconds;
		if (timer > 1500f && !playedSound) {
			playedSound = true;
			if (!string.IsNullOrEmpty(Data?.SoundName))
				Game1.playSound(Data.SoundName);
		}

		if (timer > (Data?.MessageDelay ?? 7000) && !showedMessage) {
			showedMessage = true;
			if (Data?.Message == null)
				finished = true;
			else {
				Game1.pauseThenMessage(10, Translate(Data?.Message, Game1.player));
				Game1.afterDialogues = delegate {
					finished = true;
				};
			}
		}
		if (finished) {
			Game1.freezeControls = false;
			return true;
		}
		return false;
	}

	public override void draw(SpriteBatch b) {
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);

		if (!showedMessage)
			b.Draw(Game1.mouseCursors_1_6,
				new Vector2(12f, Game1.viewport.Height - 12 - 76),
				new Rectangle(
					256 + (int) (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 19,
					413, 19, 19),
				Color.White,
				0f,
				Vector2.Zero,
				4f,
				SpriteEffects.None,
				1f
			);

	}

	public override void makeChangesToLocation() {
		// Finally, do our stuff.
		PerformSideEffects(null, Game1.player, null);

	}

	#endregion

}
