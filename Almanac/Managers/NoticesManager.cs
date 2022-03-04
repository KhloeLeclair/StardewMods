using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.TerrainFeatures;

using SObject = StardewValley.Object;

using Leclair.Stardew.Almanac.Models;

namespace Leclair.Stardew.Almanac.Managers {

	public class NoticesManager : BaseManager {

		// This is our bush.
		// There are many like it, but this is ours.
		private Bush bush;

		// Mods
		private readonly Dictionary<IManifest, Func<int, WorldDate, IEnumerable<Tuple<string, Texture2D, Rectangle?, Item>>>> ModHooks = new();
		private readonly Dictionary<IManifest, Func<int, WorldDate, IEnumerable<IRichEvent>>> InterfaceHooks = new();

		private Dictionary<WorldDate, List<IRichEvent>> DataEvents;
		private bool Loaded = false;
		private string Locale;

		public NoticesManager(ModEntry mod) : base(mod) { }

		public void Invalidate() {
			Loaded = false;
		}

		#region Event Handlers



		#endregion

		#region Loading

		private void Load() {
			string locale = Mod.Helper.Translation.Locale;
			if (Loaded && locale == Locale)
				return;

			Dictionary<WorldDate, List<IRichEvent>> events = new();

			foreach(var cp in Mod.Helper.ContentPacks.GetOwned()) {
				if (!cp.HasLocalizedAsset("notices.json", locale))
					continue;
			}

			DataEvents = events;
			Loaded = true;
			Locale = locale;
		}


		#endregion

		#region Mod Management

		public void ClearHook(IManifest mod) {
			if (InterfaceHooks.ContainsKey(mod))
				InterfaceHooks.Remove(mod);

			if (ModHooks.ContainsKey(mod))
				ModHooks.Remove(mod);
		}

		public void RegisterHook(IManifest mod, Func<int, WorldDate, IEnumerable<Tuple<string, Texture2D, Rectangle?, Item>>> hook) {
			if (InterfaceHooks.ContainsKey(mod))
				InterfaceHooks.Remove(mod);

			if (hook == null && ModHooks.ContainsKey(mod))
				ModHooks.Remove(mod);
			else if (hook != null)
				ModHooks[mod] = hook;
		}

		public void RegisterHook(IManifest mod, Func<int, WorldDate, IEnumerable<IRichEvent>> hook) {
			if (ModHooks.ContainsKey(mod))
				ModHooks.Remove(mod);

			if (hook == null && InterfaceHooks.ContainsKey(mod))
				InterfaceHooks.Remove(mod);
			else if (hook != null)
				InterfaceHooks[mod] = hook;
		}

		#endregion

		#region Events

		public IEnumerable<IRichEvent> GetEventsForDate(int seed, WorldDate date) {

			Load();

			if (DataEvents?.ContainsKey(date) ?? false) {
				foreach(var evt in DataEvents[date])
					yield return evt;
			}

			foreach (var ihook in InterfaceHooks.Values) {
				if (ihook != null)
					foreach (var entry in ihook(seed, date)) {
						if (entry == null)
							continue;

						yield return entry;
					}
			}

			foreach (var hook in ModHooks.Values) {
				if (hook != null)
					foreach (var entry in hook(seed, date)) {
						if (entry == null || string.IsNullOrEmpty(entry.Item1))
							continue;

						SpriteInfo sprite;

						if (entry.Item3.HasValue && entry.Item3.Value == Rectangle.Empty)
							sprite = null;
						else if (entry.Item2 != null)
							sprite = new(
								entry.Item2,
								entry.Item3 ?? entry.Item2.Bounds
							);
						else if (entry.Item4 != null)
							sprite = SpriteHelper.GetSprite(
								entry.Item4,
								ModEntry.instance.Helper
							);
						else
							sprite = null;

						yield return new RichEvent(
							entry.Item1,
							null,
							sprite,
							entry.Item4
						);
					}
			}

			foreach (var evt in GetVanillaEventsForDate(seed, date)) {
				if (evt != null)
					yield return evt;
			}
		}

		#endregion

		#region Vanilla Events

		public IEnumerable<IRichEvent> GetVanillaEventsForDate(int seed, WorldDate date) {

			// Berry Season
			bool gathering = ModEntry.instance.Config.NoticesShowGathering;

			if (gathering && bush == null)
				bush = new();

			if (gathering && bush.inBloom(date.Season, date.DayOfMonth)) {
				SObject berry = null;
				if (date.SeasonIndex == 0)
					berry = new(296, 1); // Salmonberry

				else if (date.SeasonIndex == 2)
					berry = new(410, 1); // Blackberry

				if (berry != null) {
					bool first_day = date.DayOfMonth == 1 || !bush.inBloom(date.Season, date.DayOfMonth - 1);
					int last = date.DayOfMonth;

					// If it's the first day, then we also need the last day
					// so we can display a nice string to the user.
					if (first_day)
						for (int d = date.DayOfMonth + 1; d <= ModEntry.DaysPerMonth; d++) {
							if (bush.inBloom(date.Season, d))
								last = d;
							else
								break;
						}

					yield return new RichEvent(
						null,
						first_day ?
							FlowHelper.Translate(
								ModEntry.instance.Helper.Translation.Get("page.notices.season"),
								new {
									item = berry.DisplayName,
									start = new SDate(date.DayOfMonth, date.Season).ToLocaleString(withYear: false),
									end = new SDate(last, date.Season).ToLocaleString(withYear: false)
								},
								alignment: Alignment.Middle
							) : null,
						SpriteHelper.GetSprite(
							berry,
							ModEntry.instance.Helper
						),
						berry
					);
				}
			}

			// Festivals
			if (ModEntry.instance.Config.NoticesShowFestivals && Utility.isFestivalDay(date.DayOfMonth, date.Season)) {
				var data = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + date.Season + date.DayOfMonth);
				if (data.ContainsKey("name") && data.ContainsKey("conditions")) {
					string name = data["name"];
					string[] conds = data["conditions"].Split('/');
					string where = conds.Length >= 1 ? conds[0] : null;

					int start = -1;
					int end = -1;

					if (conds.Length >= 2) {
						string[] bits = conds[1].Split(' ');
						if (bits.Length >= 2) {
							start = Convert.ToInt32(bits[0]);
							end = Convert.ToInt32(bits[1]);
						}
					}

					foreach (GameLocation loc in Game1.locations) {
						if (loc?.Name == where) {
							where = Mod.GetLocationName(loc);
							break;
						}
					}

					yield return new RichEvent(
						null,
						FlowHelper.Translate(
							ModEntry.instance.Helper.Translation.Get("page.notices.festival"),
							new {
								name = name,
								where = where,
								start = TimeHelper.FormatTime(start),
								end = TimeHelper.FormatTime(end)
							},
							alignment: Alignment.Middle
						),
						new SpriteInfo(
							Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Billboard"),
							new Rectangle(
								1, 398,
								84, 12
							),
							baseFrames: 6
						)
					);
				}
			}

			// Weddings / Anniversaries / Children
			foreach (var who in Game1.getAllFarmers()) {
				// Children
				// TODO: This.

				if (!ModEntry.instance.Config.NoticesShowAnniversaries)
					continue;

				// Player Weddings and Anniversaries
				// TODO: This.

				// NPC Weddings and Anniversaries
				if ((who.isEngaged() || who.isMarried()) && who.friendshipData != null) {
					foreach (var entry in who.friendshipData.Pairs) {
						if (entry.Value == null || entry.Value.WeddingDate == null)
							continue;

						WorldDate wedding = entry.Value.WeddingDate;
						if (wedding == null || wedding.SeasonIndex != date.SeasonIndex || wedding.DayOfMonth != date.DayOfMonth)
							continue;

						NPC spouse = Game1.getCharacterFromName(entry.Key);
						if (spouse == null)
							continue;

						char last = spouse.displayName.Last<char>();

						bool no_s = last == 's' ||
							LocalizedContentManager.CurrentLanguageCode ==
							LocalizedContentManager.LanguageCode.de &&
								(last == 'x' || last == 'ß' || last == 'z');

						var pendant = new SObject(460, 1);
						var sprite = SpriteHelper.GetSprite(
							pendant,
							ModEntry.instance.Helper
						);

						// Wedding?
						if (date.Year == wedding.Year) {
							yield return new RichEvent(
								null,
								FlowHelper.Translate(
									ModEntry.instance.Helper.Translation.Get(
										no_s ?
											"page.notices.wedding.no-s"
											: "page.notices.wedding.s"
									),
									new {
										name = who.displayName,
										spouse = spouse.displayName
									},
									alignment: Alignment.Middle
								),
								sprite
							);
						} else {
							yield return new RichEvent(
								null,
								FlowHelper.Translate(
									ModEntry.instance.Helper.Translation.Get(
										no_s ?
											"page.notices.anniversary.no-s"
											: "page.notices.anniversary.s"
									),
									new {
										name = who.displayName,
										spouse = spouse.displayName
									},
									alignment: Alignment.Middle
								),
								sprite
							);
						}
					}
				}
			}

			// Spring
			if (date.SeasonIndex == 0) {

			}

			// Summer
			else if (date.SeasonIndex == 1) {

				// Extra Foragables
				if (gathering && date.DayOfMonth >= 12 && date.DayOfMonth <= 14) {
					yield return new RichEvent(
						date.DayOfMonth == 12 ?
							I18n.Page_Notices_Summer() : null,
						null,
						SpriteHelper.GetSprite(
							new SObject(394, 1), // Rainbow Shell
							ModEntry.instance.Helper
						)
					);
				}
			}

			// Fall
			else if (date.SeasonIndex == 2) {

				if (gathering && date.DayOfMonth >= 15 && date.DayOfMonth <= 28) {
					SObject nut = new SObject(408, 1);

					yield return new RichEvent(
						null,
						date.DayOfMonth == 15 ?
						FlowHelper.Translate(
							ModEntry.instance.Helper.Translation.Get("page.notices.season"),
							new {
								item = nut.DisplayName,
								start = new SDate(15, date.Season).ToLocaleString(withYear: false),
								end = new SDate(28, date.Season).ToLocaleString(withYear: false),
							},
							alignment: Alignment.Middle
						) : null,
						SpriteHelper.GetSprite(
							nut,
							ModEntry.instance.Helper
						),
						nut
					);
				}

			}

			// Winter
			else if (date.SeasonIndex == 3) {

				// Night Market
				if (date.DayOfMonth >= 15 && date.DayOfMonth <= 17) {
					yield return new RichEvent(
						date.DayOfMonth == 15 ?
							I18n.Page_Notices_Market() : null,
						null,
						new SpriteInfo(
							Game1.mouseCursors,
							new Rectangle(346, 392, 8, 8)
						)
					);
				}

			}

		}

		#endregion

	}
}
