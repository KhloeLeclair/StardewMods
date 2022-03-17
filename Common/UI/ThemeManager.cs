using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using StardewModdingAPI;

namespace Leclair.Stardew.Common.UI {
	public class ThemeChangedEventArgs<DataT> : EventArgs where DataT : BaseThemeData {

		public string OldId;
		public string NewId;

		public DataT OldData;
		public DataT NewData;

		public ThemeChangedEventArgs(string oldId, DataT oldData, string newID, DataT newData) {
			OldId = oldId;
			NewId = newID;
			OldData = oldData;
			NewData = newData;
		}
	}

	internal class SimpleManifest {
		public string UniqueID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Author { get; set; }
		public string Version { get; set; }
	}

	public class ThemeManager<DataT> where DataT : BaseThemeData {

		private readonly Mod Mod;
		private readonly Dictionary<string, Tuple<DataT, IContentPack>> Themes = new();

		private DataT _DefaultTheme;

		public DataT DefaultTheme {
			get => _DefaultTheme;
			set {
				bool is_default = UsedThemeKey == "default";
				DataT oldData = Theme;
				_DefaultTheme = value;
				if (is_default)
					ThemeChanged?.Invoke(this, new ThemeChangedEventArgs<DataT>("default", oldData, "default", _DefaultTheme));
			}
		}

		private Tuple<DataT, IContentPack> BaseThemeData = null;
		public string ThemeKey { get; private set; } = null;
		public string UsedThemeKey { get; private set; } = null;

		public DataT Theme => BaseThemeData?.Item1 ?? _DefaultTheme;

		public event EventHandler<ThemeChangedEventArgs<DataT>> ThemeChanged;

		public string AssetPrefix { get; set; }

		#region Life Cycle

		public ThemeManager(Mod mod, string key, DataT defaultTheme = null, string assetPrefix = "assets/") {
			Mod = mod;
			AssetPrefix = assetPrefix;
			ThemeKey = key;
			_DefaultTheme = defaultTheme;
		}

		private void Log(string message, LogLevel level = LogLevel.Debug, Exception ex = null, LogLevel? exLevel = null) {
			Mod.Monitor.Log($"[Theme] {message}", level: level);
			if (ex != null)
				Mod.Monitor.Log($"[Theme] Details:\n{ex}", level: exLevel ?? level);
		}

		#endregion

		#region Commands

		public bool OnThemeCommand(string[] args) {
			if (args.Length > 0 && !string.IsNullOrEmpty(args[0])) {
				string key = args[0].Trim();

				if (key.Equals("reload", StringComparison.OrdinalIgnoreCase)) {
					Discover();
					Log($"Reloaded themes. You may need to reopen menus.", LogLevel.Info);
					return false;
				}

				string selected = null;
				var themes = GetThemeChoices();

				foreach(var pair in themes) {
					if (pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) { 
						selected = pair.Key;
						break;
					}
				}

				if (selected == null)
					foreach (var pair in themes) {
						if (pair.Key.Contains(key, StringComparison.OrdinalIgnoreCase)) {
							selected = pair.Key;
							break;
						}
					}

				if (selected == null)
					foreach (var pair in themes) {
						if (GetThemeName(pair.Key).Contains(key, StringComparison.OrdinalIgnoreCase)) {
							selected = pair.Key;
							break;
						}
					}

				if (selected != null) {
					SelectTheme(selected);
					return true;
				}

				Log($"Unable to match theme: {key}", LogLevel.Warn);
			}

			Log($"Available Themes:", LogLevel.Info);
			foreach (var pair in GetThemeChoices()) {
				bool selected = pair.Key == ThemeKey;
				bool used = pair.Key == UsedThemeKey;

				string selection = selected ?
					(used ? "=>" : " >") :
					(used ? "= " : "  ");

				Log($" {selection} [{pair.Key}]: {pair.Value()}", LogLevel.Info);
			}

			return false;
		}

		#endregion

		#region Theme Discovery

		private IEnumerable<IContentPack> CheckAssets(IEnumerable<IContentPack> packs) {
			string path = Path.Join(Mod.Helper.DirectoryPath, "assets", "themes");
			if (!Directory.Exists(path))
				return packs;

			List<IContentPack> result = packs.ToList();

			int count = 0;

			foreach (string dir in Directory.GetDirectories(path)) {
				string man_path = Path.Join(dir, "manifest.json");
				string t_path = Path.Join(dir, "theme.json");

				if (!File.Exists(t_path))
					continue;

				string rel_path = Path.GetRelativePath(Mod.Helper.DirectoryPath, dir);
				string folder = Path.GetRelativePath(path, dir);

				Log($"Found theme at: {dir}", LogLevel.Trace);

				SimpleManifest manifest = null;
				try {
					if (File.Exists(man_path))
						manifest = Mod.Helper.Data.ReadJsonFile<SimpleManifest>(
							Path.Join(rel_path, "manifest.json"));
					else
						manifest = Mod.Helper.Data.ReadJsonFile<SimpleManifest>(
							Path.Join(rel_path, "theme.json"));

				} catch(Exception ex) {
					Log($"Unable to read theme manifest.", LogLevel.Warn, ex);
					continue;
				}

				var cp = Mod.Helper.ContentPacks.CreateTemporary(
					directoryPath: dir,
					id: manifest.UniqueID ?? $"{Mod.ModManifest.UniqueID}.theme.{folder}",
					name: manifest.Name,
					description: manifest.Description ?? $"{Mod.ModManifest.Name} Theme: {manifest.Name}",
					author: manifest.Author ?? Mod.ModManifest.Author,
					version: new SemanticVersion(manifest.Version ?? "1.0.0")
				);

				result.Add(cp);
				count++;

				Log($"Found Theme: {cp.Manifest.Name} by {cp.Manifest.Author} ({cp.Manifest.UniqueID}", LogLevel.Trace);
			}

			Log($"Found {count} themes in assets.");

			return result;
		}

		public void Discover(IEnumerable<IContentPack> packs = null, bool checkAssets = true) {
			lock ((Themes as ICollection).SyncRoot) {
				Themes.Clear();

				if (packs == null)
					packs = Mod.Helper.ContentPacks.GetOwned();

				if (checkAssets)
					packs = CheckAssets(packs);

				foreach (var cp in packs) {
					if (!cp.HasFile("theme.json"))
						continue;

					DataT data;
					try {
						data = cp.ReadJsonFile<DataT>("theme.json");
						if (data is null)
							throw new ArgumentNullException("theme.json");
					} catch (Exception ex) {
						Log($"The content pack {cp.Manifest.Name} has an invalid theme.json file.", LogLevel.Warn, ex);
						continue;
					}

					Themes[cp.Manifest.UniqueID] = new(data, cp);
				}
			}

			string oldKey = ThemeKey;

			BaseThemeData = null;
			ThemeKey = null;
			UsedThemeKey = null;

			SelectTheme(oldKey);
		}

		public string GetThemeName(string key) {
			if (key.Equals("default", StringComparison.OrdinalIgnoreCase))
				return Mod.Helper.Translation.Get("theme.default").ToString();

			Tuple<DataT, IContentPack> theme;
			lock ((Themes as ICollection).SyncRoot) {
				if (!Themes.TryGetValue(key, out theme))
					return key;
			}

			// TODO: Check if we actually have this translation key.
			if (theme.Item2.Translation.ContainsKey("theme.name"))
				return theme.Item2.Translation.Get("theme.name").ToString();

			return theme.Item2.Manifest.Name;
		}

		public IEnumerable<KeyValuePair<string, Func<string>>> GetThemeChoices() {
			Dictionary<string, Func<string>> result = new();

			result.Add("automatic", () => Mod.Helper.Translation.Get("theme.automatic"));
			result.Add("default", () => Mod.Helper.Translation.Get("theme.default"));

			foreach(string theme in Themes.Keys)
				result.Add(theme, () => GetThemeName(theme));

			return result;
		}

		public IEnumerable<KeyValuePair<string, DataT>> GetThemes() {
			return Themes.Select(x =>
				new KeyValuePair<string, DataT>(x.Key, x.Value.Item1));
		}

		#endregion

		#region Select Theme

		public void SelectTheme(string key) {
			if (string.IsNullOrEmpty(key))
				key = "automatic";

			string old_key = ThemeKey;
			var old_data = BaseThemeData;
			string actual;

			// Deal with the default theme quickly.
			if (key.Equals("default", StringComparison.OrdinalIgnoreCase)) {
				BaseThemeData = null;
				ThemeKey = "default";
				actual = "default";
			}

			// Does this string match something?
			else if (!key.Equals("automatic", StringComparison.OrdinalIgnoreCase) && Themes.TryGetValue(key, out var theme)) {
				BaseThemeData = theme;
				ThemeKey = key;
				actual = key;
			}

			// Determine the best theme
			else {
				actual = "default";
				BaseThemeData = null;

				foreach (var td in Themes) {
					if (td.Value.Item1?.HasMatchingMod(Mod.Helper.ModRegistry) ?? false) { 
						BaseThemeData = td.Value;
						actual = td.Key;
						break;
					}
				}

				ThemeKey = "automatic";
			}

			// Now emit our event.
			UsedThemeKey = actual;
			Log($"Selected Theme: {ThemeKey} => {GetThemeName(actual)} ({actual})");
			ThemeChanged?.Invoke(this, new(old_key, old_data?.Item1, ThemeKey, BaseThemeData?.Item1));
		}

		#endregion

		#region Resource Loading

		public T Load<T>(string path) {
			if (!string.IsNullOrEmpty(AssetPrefix))
				path = $"{AssetPrefix}{path}";

			if (BaseThemeData != null && BaseThemeData.Item2.HasFile(path)) {
				try {
					return BaseThemeData.Item2.LoadAsset<T>(path);
				} catch (Exception ex) {
					Log($"Failed to load asset \"{path}\" from content pack {BaseThemeData.Item2.Manifest.Name}.", LogLevel.Warn, ex);
				}
			}

			return Mod.Helper.Content.Load<T>(path);
		}

		public T LoadLocalized<T>(string path, string locale) {
			if (!string.IsNullOrEmpty(AssetPrefix))
				path = $"{AssetPrefix}{path}";

			if (BaseThemeData != null && BaseThemeData.Item2.HasLocalizedAsset(path, locale)) {
				try {
					return BaseThemeData.Item2.LoadLocalizedAsset<T>(path, locale);
				} catch (Exception ex) {
					Log($"Failed to load asset \"{path}\" (locale:{locale}) from content pack {BaseThemeData.Item2.Manifest.Name}.", LogLevel.Warn, ex);
				}
			}

			return Mod.Helper.Content.LoadLocalized<T>(path, locale);
		}

		#endregion

	}
}
