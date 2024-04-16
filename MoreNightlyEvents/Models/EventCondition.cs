using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.Common;

using Newtonsoft.Json;

namespace Leclair.Stardew.MoreNightlyEvents.Models;

public class EventCondition {

	public string? Condition { get; set; }

	public float Weight { get; set; } = 1f;

	public float Chance { get; set; } = 1f;

	public bool IsExclusive { get; set; } = false;

}
