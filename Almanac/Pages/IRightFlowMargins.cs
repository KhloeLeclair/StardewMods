#nullable enable

namespace Leclair.Stardew.Almanac.Pages;

public interface IRightFlowMargins {

	// Right Flow
	int RightMarginTop { get; }
	int RightMarginLeft { get; }
	int RightMarginRight { get; }
	int RightMarginBottom { get; }

	int RightScrollMarginTop { get; }
	int RightScrollMarginBottom { get; }
}
