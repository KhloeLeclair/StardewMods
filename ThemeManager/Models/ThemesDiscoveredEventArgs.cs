using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Leclair.Stardew.ThemeManager.Models;

/// <inheritdoc />
public class ThemesDiscoveredEventArgs<DataT> : EventArgs, IThemesDiscoveredEvent<DataT> {

	/// <inheritdoc />
	public IReadOnlyDictionary<string, IThemeManifest> Manifests { get; }

	/// <inheritdoc />
	public IReadOnlyDictionary<string, DataT> Data { get; }

	internal ThemesDiscoveredEventArgs(Dictionary<string, Theme<DataT>> data) {
		Manifests = data.Select(x => new KeyValuePair<string, IThemeManifest>(x.Key, x.Value.Manifest)).ToImmutableDictionary();
		Data = data.Select(x => new KeyValuePair<string, DataT>(x.Key, x.Value.Data)).ToImmutableDictionary();
	}

}
