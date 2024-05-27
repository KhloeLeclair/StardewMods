#if COMMON_FLOW

namespace Leclair.Stardew.Common.UI.FlowNode;

public struct CachedFlowLine {

	public IFlowNodeSlice[] Slices;

	public float Width { get; }
	public float Height { get; }

	public CachedFlowLine(IFlowNodeSlice[] slices, float width, float height) {
		Slices = slices;
		Width = width;
		Height = height;
	}
}

#endif
