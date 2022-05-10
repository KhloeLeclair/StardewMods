#nullable enable

using Leclair.Stardew.Common.UI.SimpleLayout;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.Almanac.Pages;

public interface ITab {

	int SortKey { get; }

	bool TabVisible { get; }
	string? TabSimpleTooltip { get; }
	ISimpleNode? TabAdvancedTooltip { get; }

	bool TabMagic { get; }

	Texture2D? TabTexture { get; }
	Rectangle? TabSource { get; }
	float? TabScale { get; }

}
