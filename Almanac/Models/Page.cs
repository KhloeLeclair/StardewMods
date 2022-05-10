#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json;

using Leclair.Stardew.Common.Serialization.Converters;
using Leclair.Stardew.Common.Types;

namespace Leclair.Stardew.Almanac.Models;

internal class Page {

	#region Basic Properties

	/// <summary>
	/// 
	/// </summary>
	public NamespaceId? Type { get; set; }

	#endregion

	#region Visibility

	/// <summary>
	/// A GameStateQuery that, if set, must evaluate to true for the page
	/// to be displayed in the entry. Unlike books, chapters, and entries
	/// there is no concept of showing locked pages to players.
	/// </summary>
	public string? Condition { get; set; }

	#endregion

	#region Content

	[JsonExtensionData]
	public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

	#endregion

}
