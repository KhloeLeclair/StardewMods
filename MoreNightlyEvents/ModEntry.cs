using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;
using Leclair.Stardew.MoreNightlyEvents.Events;
using Leclair.Stardew.MoreNightlyEvents.Models;
using Leclair.Stardew.MoreNightlyEvents.Patches;

using Microsoft.Xna.Framework.Content;

using Netcode;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Events;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace Leclair.Stardew.MoreNightlyEvents;

public class ModEntry : ModSubscriber {

	public const string EVENTS_PATH = @"Mods/leclair.morenightlyevents/Events";

	public const string FRUIT_TREE_SEASON_DATA = @"leclair.morenightlyevents/AlwaysInSeason";

#nullable disable

	public static ModEntry Instance { get; private set; }

	public ModConfig Config { get; private set; }

	internal Harmony Harmony { get; private set; }

#nullable enable

	public string? ForcedEvent { get; internal set; }

	internal Dictionary<string, ITranslationHelper>? EventTranslators;

	internal Dictionary<string, BaseEventData>? Events;

	public override void Entry(IModHelper helper) {
		base.Entry(helper);

		Instance = this;

		// Harmony
		Harmony = new Harmony(ModManifest.UniqueID);

		Utility_Patches.Patch(this);
		FruitTree_Patches.Patch(this);

		// Read Config
		Config = Helper.ReadConfig<ModConfig>();

		// Init
		I18n.Init(Helper.Translation);

		CheckRecommendedIntegrations();
	}

	#region Events

	bool ForceEventTrigger(string[] args, TriggerActionContext context, out string? error) {
		string key = string.Join(' ', args).Trim();
		if (key.Equals("clear", StringComparison.OrdinalIgnoreCase)) {
			ForcedEvent = null;

		} else {
			LoadEvents();
			if (string.IsNullOrWhiteSpace(key) || !Events.ContainsKey(key)) {
				error = $"This is no event with the ID '{key}'.";
				return false;
			}

			ForcedEvent = key;
		}

		error = null;
		return true;
	}

	[Subscriber]
	private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {

		TriggerActionManager.RegisterAction("leclair.morenightlyevents_ForceEvent", ForceEventTrigger);

		// Commands
		Helper.ConsoleCommands.Add("mne_invalidate", "Invalidate the cached event data.", (_, _) => {
			Helper.GameContent.InvalidateCache(EVENTS_PATH);
			Log($"Invalidated cache.", LogLevel.Info);
		});

		Helper.ConsoleCommands.Add("mne_list", "List all available nightly events from MRE.", (_, _) => {
			LoadEvents();
			if (Events.Count > 0)
				Log($"Events:", LogLevel.Info);

			foreach(var evt in Events.Values) {
				Log($"- {evt.Id}: {evt.Type}", LogLevel.Info);
			}

			Log($"Total Events: {Events.Count}", LogLevel.Info);
		});

		Helper.ConsoleCommands.Add("mne_interrupt", "Interrupt the active event, immediately ending it.", (_, _) => {
			FarmEventInterrupter.Interrupt();
			Log($"Interrupted events.", LogLevel.Info);
		});

		Helper.ConsoleCommands.Add("mne_test", "Test an event by forcing it to happen the next night.", (_, args) => {
			string key = string.Join(' ', args).Trim();
			if (key.Equals("clear", StringComparison.OrdinalIgnoreCase)) {
				ForcedEvent = null;

			} else if (!string.IsNullOrWhiteSpace(key)) {
				LoadEvents();
				if (!Events.ContainsKey(key)) {
					Log($"There is no event with the ID '{key}'.", LogLevel.Warn);
					return;
				}

				ForcedEvent = key;
			}

			if (string.IsNullOrWhiteSpace(ForcedEvent))
				Log($"There is currently no event scheduled for tonight.", LogLevel.Info);
			else
				Log($"The '{ForcedEvent}' is scheduled to run tonight. Use the command 'mne_test clear' to cancel.", LogLevel.Info);
		});
	}

	[Subscriber]
	private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e) {
		foreach (var name in e.Names) {
			if (name.IsEquivalentTo(EVENTS_PATH))
				Events = null;
		}
	}

	[Subscriber]
	private void OnAssetRequested(object? sender, AssetRequestedEventArgs e) {
		if (e.Name.IsEquivalentTo(EVENTS_PATH))
			e.LoadFrom(LoadAssetEvents, AssetLoadPriority.Exclusive);
	}

	[Subscriber]
	private void OnDayStarted(object? sender, DayStartedEventArgs e) {
		ForcedEvent = null;
	}

	#endregion

	#region Event Data

	[return: NotNullIfNotNull(nameof(input))]
	public string? TokenizeFromEvent(string eventId, string? input, Farmer? who = null, Random? rnd = null) {
		if (string.IsNullOrWhiteSpace(input) || EventTranslators is null || ! EventTranslators.TryGetValue(eventId, out var translator))
			return input;

		bool ParseToken(string[] query, out string? replacement, Random? random, Farmer? player) {
			if (!ArgUtility.TryGet(query, 0, out string? cmd, out string? error))
				return TokenParser.LogTokenError(query, error, out replacement);

			if (cmd is null || ! cmd.Equals("LocalizedText")) {
				replacement = null;
				return false;
			}

			if (!ArgUtility.TryGet(query, 1, out string? key, out error))
				return TokenParser.LogTokenError(query, error, out replacement);

			var tl = translator.Get(key);
			if (!tl.HasValue()) { 
				replacement = null;
				return false;
			}

			Dictionary<int, string> replacements;
			if (query.Length > 2) {
				replacements = new();
				for (int i = 2; i < query.Length; i++) {
					replacements[i - 2] = query[i];
				}

			} else
				replacements = [];

			replacement = tl.Tokens(replacements).ToString();
			return true;
		}

		return TokenParser.ParseText(input, rnd, ParseToken, who);
	}


	public bool TryGetEvent<T>(string key, out T? value) where T : BaseEventData {
		LoadEvents();
		if (Events.TryGetValue(key, out var val) && val is T tval) {
			value = tval;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Load 
	/// </summary>
	[MemberNotNull(nameof(Events))]
	private void LoadEvents() {
		Events ??= Helper.GameContent.Load<Dictionary<string, BaseEventData>>(EVENTS_PATH);
	}

	/// <summary>
	/// Load all the events defined in assets / content packs.
	/// </summary>
	private Dictionary<string, BaseEventData> LoadAssetEvents() {
		var result = new Dictionary<string, BaseEventData>();

		Dictionary<string, BaseEventData>? data = null;
		EventTranslators = new();

		try {
			data = Helper.ModContent.Load<Dictionary<string, BaseEventData>>("assets/events.json");
		} catch (Exception ex) {
			try {
				var dlist = Helper.ModContent.Load<List<BaseEventData>>("assets/events.json");
				if (dlist is not null) {
					data = new();
					foreach (var entry in dlist) {
						if (!string.IsNullOrEmpty(entry.Id) && !data.ContainsKey(entry.Id))
							data.Add(entry.Id, entry);
					}
				}
			} catch (Exception) {
				/* no op */
			}

			if (data is null)
				Log($"The event.json asset file is invalid.", LogLevel.Error, ex);
		}

		if (data is not null)
			foreach (var entry in data) {
				entry.Value.Id = entry.Key;
				if (result.TryAdd(entry.Key, entry.Value))
					EventTranslators.TryAdd(entry.Key, Helper.Translation);
			}

		foreach (var cp in Helper.ContentPacks.GetOwned()) {
			if (!cp.HasFile("events.json"))
				continue;

			data = null;

			try {
				data = cp.ReadJsonFile<Dictionary<string, BaseEventData>>("events.json");
			} catch(Exception ex) {
				try {
					var dlist = cp.ReadJsonFile<List<BaseEventData>>("events.json");
					if (dlist is not null) {
						data = new();
						foreach(var entry in dlist) {
							if (!string.IsNullOrEmpty(entry.Id) && !data.ContainsKey(entry.Id))
								data.Add(entry.Id, entry);
						}
					}
				} catch(Exception) {
					/* no op */
				}

				if (data is null)
					Log($"The event.json file of {cp.Manifest.Name} is invalid.", LogLevel.Error, ex);
			}

			if (data is not null)
				foreach(var entry in data) {
					entry.Value.Id = entry.Key;
					if (result.TryAdd(entry.Key, entry.Value))
						EventTranslators.TryAdd(entry.Key, cp.Translation);
				}
		}

		return result;
	}

	#endregion

	#region Farm Event Picker

	/// <summary>
	/// Determine whether or not we can replace an existing <see cref="FarmEvent"/>
	/// with a new instance. This logic is used to avoid overriding important events
	/// from the base game, such as the earthquake event. It also preserves the
	/// vanilla behavior or not having a night event the night before a wedding.
	/// </summary>
	/// <param name="evt">The event we want to replace.</param>
	/// <returns>Whether or not the event can be replaced.</returns>
	public bool CanReplaceEvent(FarmEvent? evt) {
		if (evt is null) {
			if (Game1.weddingToday)
				return false;

			foreach(Farmer who in Game1.getOnlineFarmers()) {
				Friendship spouse = who.GetSpouseFriendship();
				if (spouse is not null && spouse.IsMarried() && spouse.WeddingDate == Game1.Date)
					return false;
			}

			return true;
		}

		if (evt is FairyEvent)
			return true;
		if (evt is WitchEvent)
			return true;
		if (evt is SoundInTheNightEvent) {
			var behavior = Helper.Reflection.GetField<NetInt>(evt, "behavior", false)?.GetValue();
			if (behavior is not null && behavior.Value != 4 && behavior.Value != 5)
				return true;
		}

		return false;
	}

	public BaseEventData? SelectEvent() {
		LoadEvents();
		if (ForcedEvent is not null)
			return Events.GetValueOrDefault(ForcedEvent);

		Random rnd = Utility.CreateDaySaveRandom();

		Log($"Total Events: {Events.Count}", LogLevel.Debug);

		foreach (var pair in Events) {
			var evt = pair.Value;
			if (evt?.Conditions is null || evt.Conditions.Count == 0)
				continue;

			double? val = null;
			bool matched = false;

			foreach (var cond in evt.Conditions) {
				if (cond.Chance <= 0 || string.IsNullOrWhiteSpace(cond.Condition))
					continue;

				if (GameStateQuery.CheckConditions(cond.Condition, random: rnd)) {
					if (!val.HasValue)
						val = rnd.NextDouble();

					Log($"Condition Matched for \"{evt.Id}\": {cond.Condition}\n  Chance: {cond.Chance} -- Rnd: {val.Value}", LogLevel.Trace);

					if (cond.Chance >= 1 || cond.Chance >= val.Value) {
						matched = true;
						break;
					}
				}
			}

			if (matched)
				return evt;
		}

		return null;
	}

	public FarmEvent? PickEvent(FarmEvent? existing) {
		Log($"PickEvent: {existing}", LogLevel.Debug);

		if (!CanReplaceEvent(existing))
			return existing;

		BaseEventData? evt = SelectEvent();

		if (evt is null) { 
			Log($"No matching event.", LogLevel.Debug);
			return existing;
		}

		Log($"Using Event: {evt.Id}", LogLevel.Debug);

		if (evt is PlacementEventData ped)
			return new PlacementEvent(evt.Id, ped);

		if (evt is ScriptEventData sed)
			return new ScriptEvent(evt.Id, sed);

		if (evt is GrowthEventData ged)
			return new GrowthEvent(evt.Id, ged);

		Log($"No matching event type. Ignoring event: {evt.Id}", LogLevel.Warn);
		return existing;
	}

	#endregion

}
