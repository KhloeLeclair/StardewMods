using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;

namespace Leclair.Stardew.GiantCropTweaks.Models;

public class GiantCropHarvestItemData : GenericSpawnItemData, IGiantCropHarvestItemData {

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public float Chance { get; set; } = 1f;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public bool? ForShavingEnchantment { get; set; }

}
