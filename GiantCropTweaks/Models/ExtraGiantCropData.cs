using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Leclair.Stardew.GiantCropTweaks.Models;

public class ExtraGiantCropData {
	/// <summary>Must match the ID of an existing <see cref="GiantCrops"/> instance in <c>Data\GiantCrops</c></summary>
	public string ID { get; set; } = string.Empty;
	/// <summary>An optional texture for an overlay. If this is set, a second layer will be drawn with this texture.</summary>
	public string? OverlayTexture { get; set; }
	/// <summary>An optional color to apply to the overlay texture. Ignored if <c>OptionalColorChoices</c> has a value.</summary>
	public Color? OverlayColor { get; set; }
	/// <summary>An optional list of colors to apply to the overlay texture. One color from this list will be selected at random for a given giant crop instance.</summary>
	public Color[]? OverlayColorChoices { get; set; }
	/// <summary>Whether or not the overlay should be drawn as prismatic. If this is true, the overlay color will not be used.</summary>
	public bool OverlayPrismatic { get; set; } = false;
	/// <summary>The top-left pixel position of the overlay sprite within the texture. If this is null, <see cref="GiantCrops.Corner"/> will be used instead.</summary>
	public Point? OverlayCorner { get; set; }
}
