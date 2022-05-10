#nullable enable

namespace Leclair.Stardew.Almanac.Pages;

public interface ILeftFlowMargins {

	// Left Flow
	int LeftMarginTop { get; }
	int LeftMarginLeft { get; }
	int LeftMarginRight { get; }
	int LeftMarginBottom { get; }

	int LeftScrollMarginTop { get; }
	int LeftScrollMarginBottom { get; }
}
