#nullable enable

namespace Leclair.Stardew.Common.Types;

public record struct RecommendedIntegration(
	string Id,
	string Name,
	string Url,

	string[] Mods
);
