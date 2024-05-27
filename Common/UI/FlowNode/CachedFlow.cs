#if COMMON_FLOW

using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.Common.UI.FlowNode;

public struct CachedFlow {

	public IFlowNode[] Nodes { get; }
	public CachedFlowLine[] Lines { get; }

	public float Width { get; }
	public float Height { get; }

	public SpriteFont Font { get; }
	public float MaxWidth { get; }

	public CachedFlow(IFlowNode[] nodes, CachedFlowLine[] lines, float width, float height, SpriteFont font, float maxWidth) {
		Nodes = nodes;
		Lines = lines;
		Width = width;
		Height = height;

		Font = font;
		MaxWidth = maxWidth;
	}

	public bool IsCached(SpriteFont font, float maxWidth) {
		return Font == font && maxWidth == MaxWidth;
	}
}

#endif
