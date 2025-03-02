using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Menus;

public class ErrorMenu : IClickableMenu {

	private readonly BetterGameMenuImpl Menu;
	private CachedFlow Flow;

	public ClickableComponent? btnReload;
	public ClickableComponent? btnUseVanilla;

	public ErrorMenu(ModEntry mod, BetterGameMenuImpl menu, string message, bool hasVanilla, int x, int y, int width, int height) : base(x, y, width, height, false) {
		Menu = menu;

		string labelReload = I18n.ErrorPage_TryAgain();
		string labelVanilla = "Use Standard Menu";

		btnReload = mod.Config.DeveloperMode ? new ClickableComponent(
			new Rectangle(
				0, 0,
				(int) Game1.dialogueFont.MeasureString(labelReload).X + 64, 64
			),
			"",
			labelReload
		) {
			myID = 500,
			upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
			downNeighborID = 501
		} : null;

		btnUseVanilla = hasVanilla ? new ClickableComponent(
			new Rectangle(
				0, 0,
				(int) Game1.dialogueFont.MeasureString(labelVanilla).X + 64, 64
			),
			"",
			labelVanilla
		) {
			myID = 501,
			upNeighborID = btnReload is null ? ClickableComponent.SNAP_AUTOMATIC : 500
		} : null;

		var builder = FlowHelper.Builder()
			.Sprite(new Common.SpriteInfo(
				Game1.temporaryContent.Load<Texture2D>(@"Characters\Junimo"),
				new Rectangle(112, 16, 16, 16),
				Color.DeepPink
			), 4f, Alignment.HCenter)
			.Text("\n\n", TextStyle.EMPTY)
			.Text(message, TextStyle.EMPTY)
			.Text("\n\n", TextStyle.EMPTY);

		if (btnReload != null)
			builder = builder
				.Add(new ComponentNode(btnReload, Alignment.HCenter))
				.Text("\n\n", TextStyle.EMPTY);

		if (btnUseVanilla != null)
			builder = builder
				.Add(new ComponentNode(btnUseVanilla, Alignment.HCenter));

		int w = width - IClickableMenu.borderWidth * 4;

		Flow = FlowHelper.CalculateFlow(builder.Build(), maxWidth: w, Game1.smallFont);
	}

	public override void snapToDefaultClickableComponent() {
		currentlySnappedComponent = btnReload ?? btnUseVanilla;
		snapCursorToCurrentSnappedComponent();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true) {
		base.receiveLeftClick(x, y, playSound);

		if (btnReload?.containsPoint(x, y) ?? false) {
			if (playSound)
				Game1.playSound("smallSelect");
			Menu.TryReloadPage();
		}

		if (btnUseVanilla?.containsPoint(x, y) ?? false) {
			if (playSound)
				Game1.playSound("smallSelect");
			Menu.TryReloadPage(switchProvider: true);
		}
	}

	public override void performHoverAction(int x, int y) {
		base.performHoverAction(x, y);

		if (btnReload is not null) {
			if (btnReload.containsPoint(x, y)) {
				btnReload.scale = 1f;
			} else
				btnReload.scale = 0f;
		}

		if (btnUseVanilla is not null) {
			if (btnUseVanilla.containsPoint(x, y)) {
				btnUseVanilla.scale = 1f;
			} else
				btnUseVanilla.scale = 0f;
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) {
		xPositionOnScreen = Menu.xPositionOnScreen;
		yPositionOnScreen = Menu.yPositionOnScreen;
		width = Menu.width;
		height = Menu.height;
	}

	public override void draw(SpriteBatch batch) {
		int w = width - IClickableMenu.borderWidth * 4;

		Flow = FlowHelper.RenderFlow(
			batch: batch,
			flow: Flow,
			position: new Vector2(
				xPositionOnScreen + IClickableMenu.borderWidth * 2,
				yPositionOnScreen + 128
			),
			maxWidth: w,
			defaultFont: Game1.smallFont,
			defaultColor: Game1.textColor,
			defaultShadowColor: Game1.textShadowColor,
			scale: 1f,
			scrollOffset: 0f,
			maxHeight: -1f
		);

		if (btnReload is not null)
			DrawButton(batch, btnReload);

		if (btnUseVanilla is not null)
			DrawButton(batch, btnUseVanilla);
	}

	private static void DrawButton(SpriteBatch batch, ClickableComponent btn) {
		IClickableMenu.drawTextureBox(
			batch,
			Game1.mouseCursors,
			new Rectangle(432, 439, 9, 9),
			btn.bounds.X,
			btn.bounds.Y,
			btn.bounds.Width,
			btn.bounds.Height,
			btn.scale > 0f
				? Color.Wheat
				: Color.White,
			4f
		);

		batch.DrawString(
			Game1.dialogueFont,
			btn.label,
			new Vector2(
				btn.bounds.Center.X,
				btn.bounds.Center.Y + 2
			) - Game1.dialogueFont.MeasureString(btn.label) / 2f,
			Game1.textColor
		);
	}

}
