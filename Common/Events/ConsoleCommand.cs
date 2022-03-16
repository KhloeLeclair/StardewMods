#nullable enable

using System;

namespace Leclair.Stardew.Common.Events;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ConsoleCommand : Attribute {

	public string Name { get; }
	public string? Description { get; }
	public ConsoleCommand(string name, string? description = null) {
		Name = name;
		Description = description;
	}

}
