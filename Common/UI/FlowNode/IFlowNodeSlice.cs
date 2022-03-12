using System;

namespace Leclair.Stardew.Common.UI.FlowNode {
	public interface IFlowNodeSlice {

		IFlowNode Node { get; }

		float Width { get; }
		float Height { get; }

		WrapMode ForceWrap { get; }

		bool IsEmpty();

	}

	[Flags]
	public enum WrapMode {
		None,
		ForceBefore,
		ForceAfter
	}
}
