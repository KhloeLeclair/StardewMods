
using Leclair.Stardew.Common.UI.FlowNode;

namespace Leclair.Stardew.BetterCrafting.DynamicRules;

public interface IExtraInfoRuleHandler {

	IFlowNode[]? GetExtraInfo(object? state);

}
