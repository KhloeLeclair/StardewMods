using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.Common.Crafting;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Leclair.Stardew.BCBuildings;

internal class BuildMenu : IClickableMenu {

	public readonly BluePrint Blueprint;
	public readonly Building Building;

	public readonly IPerformCraftEvent Event;

	private bool frozen;

	public ClickableTextureComponent btnCancel;

	public BuildMenu(BluePrint bp, IPerformCraftEvent evt) : base() {
		Blueprint = bp;
		Event = evt;

		Game1.displayHUD = false;
		Game1.viewportFreeze = true;
		Game1.panScreen(0, 0);


		btnCancel = new ClickableTextureComponent(
			"OK",
			new Rectangle(
				Game1.uiViewport.Width - 128,
				Game1.uiViewport.Height - 128,
				64, 64
			),
			null,
			null,
			Game1.mouseCursors,
			Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47),
			1f
		);
	}

	protected override void cleanupBeforeExit() {
		base.cleanupBeforeExit();

		Game1.displayHUD = true;
		Game1.viewportFreeze = false;
		Game1.displayFarmer = true;

	}

	public override bool shouldClampGamePadCursor() {
		return true;
	}

	public override void update(GameTime time) {
		base.update(time);

		if (frozen)
			return;

		int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
		int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;

		if (mouseX - Game1.viewport.X < 64) {
			Game1.panScreen(-8, 0);
		} else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -128) {
			Game1.panScreen(8, 0);
		}

		if (mouseY - Game1.viewport.Y < 64) {
			Game1.panScreen(0, -8);
		} else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64) {
			Game1.panScreen(0, 8);
		}

		Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
		foreach (Keys key in pressedKeys)
			receiveKeyPress(key);

		if (Game1.IsMultiplayer)
			return;

		Farm farm = Game1.getFarm();
		foreach (FarmAnimal value in farm.animals.Values)
			value.MovePosition(Game1.currentGameTime, Game1.viewport, farm);
	}

	public override void performHoverAction(int x, int y) {
		btnCancel.tryHover(x, y);
		base.performHoverAction(x, y);
	}

	public override bool overrideSnappyMenuCursorMovementBan() {
		return true;
	}

	public override void receiveKeyPress(Keys key) {
		if (frozen)
			return;

		if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose()) {
			exitThisMenu();
			return;
		}

		if (!Game1.options.SnappyMenus) {
			if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key)) {
				Game1.panScreen(0, 4);
			} else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key)) {
				Game1.panScreen(4, 0);
			} else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key)) {
				Game1.panScreen(0, -4);
			} else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key)) {
				Game1.panScreen(-4, 0);
			}
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true) {
		if (frozen)
			return;

		if (btnCancel.containsPoint(x, y)) {
			Game1.playSound("cancel");
			exitThisMenu();
			return;
		}

		if (Game1.currentLocation is not BuildableGameLocation bgl)
			return;

		Game1.player.team.buildLock.RequestLock(delegate {
			if (!bgl.buildStructure(
				Blueprint,
				new Vector2(
					(Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64,
					(Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64
				),
				Game1.player,
				true
			)) {
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
				return;
			}

			bgl.buildings.Last().daysOfConstructionLeft.Value = 0;

			frozen = true;

			DelayedAction.functionAfterDelay(delegate {
				Event.Complete();
				exitThisMenu();
			}, 2000);
		});
	}

	public override void draw(SpriteBatch b) {
		if (frozen)
			return;

		Game1.StartWorldDrawInUI(b);

		Vector2 mousePositionTile2 = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
		for (int y4 = 0; y4 < Blueprint.tilesHeight; y4++) {
			for (int x3 = 0; x3 < Blueprint.tilesWidth; x3++) {
				int sheetIndex3 = Blueprint.getTileSheetIndexForStructurePlacementTile(x3, y4);
				Vector2 currentGlobalTilePosition3 = new Vector2(mousePositionTile2.X + (float) x3, mousePositionTile2.Y + (float) y4);
				if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition3)) {
					sheetIndex3++;
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition3 * 64f), new Rectangle(194 + sheetIndex3 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
			}
		}
		foreach (Point additionalPlacementTile in Blueprint.additionalPlacementTiles) {
			int x4 = additionalPlacementTile.X;
			int y3 = additionalPlacementTile.Y;
			int sheetIndex4 = Blueprint.getTileSheetIndexForStructurePlacementTile(x4, y3);
			Vector2 currentGlobalTilePosition4 = new Vector2(mousePositionTile2.X + (float) x4, mousePositionTile2.Y + (float) y3);
			if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(currentGlobalTilePosition4)) {
				sheetIndex4++;
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition4 * 64f), new Rectangle(194 + sheetIndex4 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
		}

		Game1.EndWorldDrawInUI(b);

		btnCancel.draw(b);
		drawMouse(b);
	}

}
