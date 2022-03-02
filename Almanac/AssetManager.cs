using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Leclair.Stardew.Common;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Leclair.Stardew.Almanac {
	public class AssetManager : IAssetEditor {

		private readonly ModEntry Mod;

		// Events
		private readonly string EventPath = PathUtilities.NormalizeAssetName("Data/Events");

		public Dictionary<string, List<EventData>> ModEvents;
		private bool Loaded = false;
		private string Locale = null;

		public AssetManager(ModEntry mod) {
			Mod = mod;
			Mod.Helper.Content.AssetEditors.Add(this);
		}

		public void Invalidate() {
			Loaded = false;
			Mod.Helper.Content.InvalidateCache(asset => asset.AssetName.StartsWith(EventPath));
		}

		private void Load(string locale) {
			if (Loaded && Locale == locale)
				return;

			try {
				ModEvents = Mod.Helper.Content.LoadLocalized<Dictionary<string, List<EventData>>>("assets/events.json");
			} catch (Exception ex) {
				Mod.Log("Unable to load custom mod events.", ex: ex);
				ModEvents = null;
			}

			Loaded = true;
			Locale = locale;
		}

		public bool CanEdit<T>(IAssetInfo asset) {
			if (asset.AssetName.StartsWith(EventPath)) {
				Load(asset.Locale);
				if (ModEvents == null)
					return false;

				string[] bits = PathUtilities.GetSegments(asset.AssetName);
				string end = bits[bits.Length - 1];

				if (ModEvents.ContainsKey(end))
					return true;
			}

			return false;
		}

		public void Edit<T>(IAssetData asset) {
			if (!asset.AssetName.StartsWith(EventPath))
				return;

			Load(asset.Locale);
			if (ModEvents == null)
				return;

			string[] bits = PathUtilities.GetSegments(asset.AssetName);
			string end = bits[bits.Length - 1];

			if (!ModEvents.TryGetValue(end, out var events) || events == null)
				return;

			var editor = asset.AsDictionary<string, string>();
			foreach (var entry in events)
				editor.Data[entry.Key] = entry.Localize(Mod.Helper.Translation);
		}
	}

	public struct EventData {
		public static readonly Regex I18N_SPLITTER = new(@"{{(.+?)}}", RegexOptions.Compiled);

		public string Id { get; set; }
		public string[] Conditions { get; set; }
		public string[] Script { get; set; }

		public string Key => $"{Id}/{string.Join("/", Conditions)}";
		public string RealScript => string.Join("/", Script);

		public string Localize(ITranslationHelper helper) {
			string id = Id;

			return I18N_SPLITTER.Replace(RealScript, match => {
				string key = match.Groups[1].Value;
				if (key.StartsWith('.'))
					key = $"event.{id}{key}";
				return helper.Get(key).ToString();
			});
		}

	}
}
