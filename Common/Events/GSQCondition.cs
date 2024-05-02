#nullable enable

using System;

namespace Leclair.Stardew.Common.Events;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class GSQCondition : Attribute {

	public string? Name { get; }

	public bool SkipPrefix { get; }

	public GSQCondition(string? name = null, bool skipPrefix = false) {
		Name = name;
		SkipPrefix = skipPrefix;
	}

}
