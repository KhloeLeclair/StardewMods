using System;
using System.Collections.Generic;
using System.Text;

namespace Leclair.Stardew.Common.UI.FlowNode
{
	public interface INodeSlice
	{

		IFlowNode Node { get; }

		float Width { get; }
		float Height { get; }

		bool ForceWrap { get; }

		bool IsEmpty();

	}
}
