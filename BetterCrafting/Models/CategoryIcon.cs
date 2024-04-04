#nullable enable

using Leclair.Stardew.Common.Enums;

using Microsoft.Xna.Framework;

namespace Leclair.Stardew.BetterCrafting.Models;

public class CategoryIcon {

	public enum IconType {
		Item,
		Texture
	}

	public IconType Type { get; set; }

	// Item
	public string? RecipeName { get; set; }

	public string? ItemId { get; set; }

	// Texture
	public GameTexture? Source { get; set; } = null;
	public string? Path { get; set; }

	public Rectangle? Rect { get; set; }
	public float Scale { get; set; } = 1;
}
