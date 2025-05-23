#nullable enable

using Leclair.Stardew.Common.Serialization.Converters;

using Microsoft.Xna.Framework;

using Newtonsoft.Json;

namespace Leclair.Stardew.BetterCrafting.Models;

public class Theme {

	[JsonConverter(typeof(ColorConverter))]
	public Color? SearchHighlightColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? QuantityCriticalTextColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? QuantityCriticalShadowColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? QuantityWarningTextColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? QuantityWarningShadowColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? ShopLabelColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? ShopHoverColor { get; set; }

	public bool CustomTooltip { get; set; }
	public bool CustomScroll { get; set; }
	public bool CustomMouse { get; set; }


	[JsonConverter(typeof(ColorConverter))]
	public Color? TextColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? TextShadowColor { get; set; }


	[JsonConverter(typeof(ColorConverter))]
	public Color? TooltipTextColor { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? TooltipTextShadowColor { get; set; }

}
