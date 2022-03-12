using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Objects;

using SObject = StardewValley.Object;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;

using Leclair.Stardew.Almanac.Models;
using Leclair.Stardew.Almanac.Managers;

namespace Leclair.Stardew.Almanac.Fish {
	public class FishManager : BaseManager {

		private List<FishInfo> Fish = new();
		private bool Loaded = false;

		private readonly List<IFishProvider> Providers = new();

		#region Lifecycle

		public FishManager(ModEntry mod) : base(mod) {
			Providers.Add(new VanillaProvider(mod));
		}

		#endregion

		#region Mod Providers

		#endregion

		#region Providers

		public void AddProvider(IFishProvider provider) {
			if (Providers.Contains(provider))
				return;

			Providers.Add(provider);
			SortProviders();
		}

		public void RemoveProvider(IFishProvider provider) {
			if (!Providers.Contains(provider))
				return;

			Providers.Remove(provider);
			Invalidate();
		}

		public void SortProviders() {
			Providers.Sort((a, b) => -a.Priority.CompareTo(b.Priority));
			Invalidate();
		}

		#endregion

		#region Event Handlers

		[Subscriber]
		private void OnSaveLoaded(object sender, SaveLoadedEventArgs e) {
			Invalidate();
		}

		#endregion

		#region Data Loading

		public void Invalidate() {
			Loaded = false;
		}

		private void RefreshFish() {

			Dictionary<string, FishInfo> working = new();

			int providers = 0;

			foreach (IFishProvider provider in Providers) {
				int provided = 0;
				IEnumerable<FishInfo> fish;

				try {
					fish = provider.GetFish();
				} catch(Exception ex) {
					Log($"An error occurred getting fish from provider {provider.Name}.", LogLevel.Warn, ex);
					continue;
				}

				foreach (FishInfo info in fish) {
					if (!working.ContainsKey(info.Id)) {
						working[info.Id] = info;
						provided++;
					}
				}

				Log($"Loaded {provided} fish from {provider.Name}");
				providers++;
			}

			Fish = working.Values.ToList();
			Loaded = true;
			Log($"Loaded {Fish.Count} fish from {Providers.Count} providers.");
		}

		#endregion

		#region Queries

		public IReadOnlyCollection<FishInfo> GetFish() {
			return Fish.AsReadOnly();
		}

		public List<FishInfo> GetSeasonFish(int season) {
			if (!Loaded)
				RefreshFish();

			return Fish.Where(fish => fish.Seasons != null && fish.Seasons.Contains(season)).ToList();
		}

		public List<FishInfo> GetSeasonFish(string season) {
			WorldDate start = new(1, season, 1);
			return GetSeasonFish(start.SeasonIndex);
		}

		#endregion

	}
}
