#nullable enable

using System.Collections.Generic;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.UI.FlowNode;

using StardewValley;

namespace Leclair.Stardew.Almanac.Models;

public interface IRichEvent {
	string? SimpleLabel { get; }
	IEnumerable<IFlowNode>? AdvancedLabel { get; }
	SpriteInfo? Sprite { get; }
	Item? Item { get; }
}

public struct RichEvent : IRichEvent {
	public string? SimpleLabel { get; }
	public IEnumerable<IFlowNode>? AdvancedLabel { get; }
	public SpriteInfo? Sprite { get; }
	public Item? Item { get; }

	public RichEvent(string? simpleLabel, IEnumerable<IFlowNode>? advancedLabel = null, SpriteInfo? sprite = null, Item? item = null) {
		SimpleLabel = simpleLabel;
		AdvancedLabel = advancedLabel;
		Sprite = sprite;
		Item = item;
	}
}
