#nullable enable

using System;

using Leclair.Stardew.Common.Events;

using StardewModdingAPI;

namespace Leclair.Stardew.Almanac.Managers;

public class BaseManager : EventSubscriber<ModEntry> {

	public readonly string Name;

	public BaseManager(ModEntry mod, string? name = null) : base(mod) {
		Name = name ?? GetType().Name;
	}

	protected void Log(string message, LogLevel level = LogLevel.Debug, Exception? ex = null, LogLevel? exLevel = null) {
		Mod.Monitor.Log($"[{Name}] {message}", level: level);
		if (ex != null)
			Mod.Monitor.Log($"[{Name}] Details:\n{ex}", level: exLevel ?? level);
	}

}
