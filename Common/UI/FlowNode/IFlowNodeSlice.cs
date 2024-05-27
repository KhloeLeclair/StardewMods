#if COMMON_FLOW

using System;

namespace Leclair.Stardew.Common.UI.FlowNode;

public interface IFlowNodeSlice {

	IFlowNode Node { get; }

	float Width { get; }
	float Height { get; }

	WrapMode ForceWrap { get; }

	bool IsEmpty();
}

[Flags]
public enum WrapMode {
	None = 0,
	CannotBefore = 1,
	CannotAfter = 2,
	ForceBefore = 4,
	ForceAfter = 8
}

#endif
