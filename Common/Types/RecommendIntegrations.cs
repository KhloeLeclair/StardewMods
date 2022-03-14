using System;
using System.Collections.Generic;
using System.Text;

namespace Leclair.Stardew.Common.Types
{
	public record struct RecommendedIntegration(
		string Id,
		string Name,
		string Url,

		string[] Mods
	);
}
