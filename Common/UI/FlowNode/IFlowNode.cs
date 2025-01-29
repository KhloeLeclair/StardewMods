#if COMMON_FLOW

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley.Menus;

namespace Leclair.Stardew.Common.UI.FlowNode;

public interface IFlowNode {

	bool IsEmpty();

	Alignment Alignment { get; }

	string? UniqueId { get; }

	object? Extra { get; }

	IFlowNodeSlice? Slice(IFlowNodeSlice? last, SpriteFont font, float maxWidth, float remaining, IFlowNodeSlice? nextSlice);

	void Draw(IFlowNodeSlice slice, SpriteBatch batch, Vector2 position, float scale, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor, CachedFlowLine line, CachedFlow flow);

	// Interaction

	ClickableComponent? UseComponent(IFlowNodeSlice slice);
	bool? WantComponent(IFlowNodeSlice slice);

	Func<IFlowNodeSlice, int, int, bool>? OnHover { get; }
	Func<IFlowNodeSlice, int, int, bool>? OnClick { get; }
	Func<IFlowNodeSlice, int, int, bool>? OnRightClick { get; }
}

#endif
