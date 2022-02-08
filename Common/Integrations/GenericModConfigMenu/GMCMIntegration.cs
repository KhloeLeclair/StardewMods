using System;
using System.Collections.Generic;
using System.Linq;

using GenericModConfigMenu;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Leclair.Stardew.Common.Integrations.GenericModConfigMenu {

	public class GMCMIntegration<T, M> : BaseAPIIntegration<IGenericModConfigMenuApi, M> where T : new() where M : Mod {

		private bool hasRegistered = false;

		private readonly Func<T> GetConfig;
		private readonly Action ResetConfig;
		private readonly Action SaveConfig;

		private IManifest Consumer { get => Self.ModManifest; }

		public GMCMIntegration(M self, Func<T> getConfig, Action resetConfig, Action saveConfig) : base(self, "spacechase0.GenericModConfigMenu", "1.3.3") {

			GetConfig = getConfig;
			ResetConfig = resetConfig;
			SaveConfig = saveConfig;

		}

		#region Registration

		public GMCMIntegration<T, M> Register(bool? allowInGameChanges = null) {
			AssertLoaded();

			if (!hasRegistered) {
				hasRegistered = true;
				API.RegisterModConfig(Consumer, ResetConfig, SaveConfig);
			}

			if (allowInGameChanges.HasValue)
				API.SetDefaultIngameOptinValue(Consumer, optedIn: allowInGameChanges.Value);

			return this;
		}

		public GMCMIntegration<T, M> Unregister() {
			AssertLoaded();

			if (hasRegistered) {
				hasRegistered = false;
				API.UnregisterModConfig(Consumer);
			}

			return this;
		}

		#endregion

		#region Pages

		public GMCMIntegration<T, M> StartPage(string name, string displayName) {
			AssertLoaded();
			API.StartNewPage(Self.ModManifest, name);
			if (!string.IsNullOrEmpty(displayName))
				API.OverridePageDisplayName(Consumer, name, displayName);
			return this;
		}

		#endregion

		#region Formatting

		public GMCMIntegration<T, M> AddLabel(string name, string tooltip = null, string shortcut = null) {
			AssertLoaded();
			if (string.IsNullOrEmpty(shortcut))
				API.RegisterLabel(Consumer, name, tooltip);
			else
				API.RegisterPageLabel(Consumer, name, tooltip, shortcut);
			return this;
		}

		public GMCMIntegration<T, M> AddParagraph(string text) {
			AssertLoaded();
			API.RegisterParagraph(Consumer, text);
			return this;
		}

		public GMCMIntegration<T, M> AddImage(string path, Rectangle? source = null, int scale = 4) {
			AssertLoaded();
			API.RegisterImage(Consumer, path, source, scale);
			return this;
		}

		#endregion

		#region Fancy Controls

		public GMCMIntegration<T, M> AddChoice<TType>(string name, string tooltip, Func<T, TType> get, Action<T, TType> set, IEnumerable<KeyValuePair<TType, string>> choices) {
			List<TType> values = new();
			List<string> labels = new();

			foreach (KeyValuePair<TType, string> entry in choices) {
				values.Add(entry.Key);
				labels.Add(entry.Value);
			}

			return AddChoice(name, tooltip, get, set, labels, values);
		}

		public GMCMIntegration<T, M> AddChoice<TType>(string name, string tooltip, Func<T, TType> get, Action<T, TType> set, IEnumerable<KeyValuePair<string, TType>> choices) {
			List<TType> values = new();
			List<string> labels = new();

			foreach (KeyValuePair<string, TType> entry in choices) {
				values.Add(entry.Value);
				labels.Add(entry.Key);
			}

			return AddChoice(name, tooltip, get, set, labels, values);
		}

		public GMCMIntegration<T, M> AddChoice<TType>(string name, string tooltip, Func<T, TType> get, Action<T, TType> set, IList<string> labels, IList<TType> values) {
			AssertLoaded();

			API.RegisterChoiceOption(
				mod: Consumer,
				optionName: name,
				optionDesc: tooltip,
				optionGet: () => {
					TType val = get(GetConfig());
					int idx = values.IndexOf(val);
					return idx == -1 ? labels[0] : labels[idx];
				},
				optionSet: val => {
					int idx = labels.IndexOf(val);
					set(GetConfig(), idx == -1 ? values[0] : values[idx]);
				},
				choices: labels.ToArray()
			);

			return this;
		}

		public GMCMIntegration<T, M> AddCustom(string name, string tooltip, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave) {
			AssertLoaded();
			API.RegisterComplexOption(Consumer, name, tooltip, widgetUpdate, widgetDraw, onSave);
			return this;
		}

		#endregion

		#region Basic Controls

		public GMCMIntegration<T, M> Add(string name, string tooltip, Func<T, bool> get, Action<T, bool> set) {
			AssertLoaded();
			API.RegisterSimpleOption(
				mod: Consumer,
				optionName: name,
				optionDesc: tooltip,
				optionGet: () => get(GetConfig()),
				optionSet: val => set(GetConfig(), val)
			);
			return this;
		}

		public GMCMIntegration<T, M> Add(string name, string tooltip, Func<T, string> get, Action<T, string> set) {
			AssertLoaded();
			API.RegisterSimpleOption(
				mod: Consumer,
				optionName: name,
				optionDesc: tooltip,
				optionGet: () => get(GetConfig()),
				optionSet: val => set(GetConfig(), val)
			);
			return this;
		}

		public GMCMIntegration<T, M> Add(string name, string tooltip, Func<T, SButton> get, Action<T, SButton> set) {
			AssertLoaded();
			API.RegisterSimpleOption(
				mod: Consumer,
				optionName: name,
				optionDesc: tooltip,
				optionGet: () => get(GetConfig()),
				optionSet: val => set(GetConfig(), val)
			);
			return this;
		}

		public GMCMIntegration<T, M> Add(string name, string tooltip, Func<T, KeybindList> get, Action<T, KeybindList> set) {
			AssertLoaded();
			API.RegisterSimpleOption(
				mod: Consumer,
				optionName: name,
				optionDesc: tooltip,
				optionGet: () => get(GetConfig()),
				optionSet: val => set(GetConfig(), val)
			);
			return this;
		}

		#endregion

		#region Numeric Controls

		public GMCMIntegration<T, M> Add(string name, string tooltip, Func<T, int> get, Action<T, int> set, int? minValue = null, int? maxValue = null, int? interval = null) {
			AssertLoaded();

			if (minValue.HasValue || maxValue.HasValue) {
				// Here, we do stupid stuff with floats because GMCM doesn't render the current value
				// if the number isn't a float.
				API.RegisterClampedOption(
					mod: Consumer,
					optionName: name,
					optionDesc: tooltip,
					optionGet: () => get(GetConfig()),
					optionSet: val => set(GetConfig(), (int) val),
					min: minValue ?? int.MinValue,
					max: maxValue ?? int.MaxValue,
					interval: (float) (interval ?? 1)
				);

			} else
				API.RegisterSimpleOption(
					mod: Consumer,
					optionName: name,
					optionDesc: tooltip,
					optionGet: () => get(GetConfig()),
					optionSet: val => set(GetConfig(), val)
				);

			return this;
		}

		public GMCMIntegration<T, M> Add(string name, string tooltip, Func<T, float> get, Action<T, float> set, float? minValue = null, float? maxValue = null, float? interval = null) {
			AssertLoaded();

			if (minValue.HasValue || maxValue.HasValue) {
				// Here, we do stupid stuff with floats because GMCM doesn't render the current value
				// if the number isn't a float.
				API.RegisterClampedOption(
					mod: Consumer,
					optionName: name,
					optionDesc: tooltip,
					optionGet: () => get(GetConfig()),
					optionSet: val => set(GetConfig(), val),
					min: minValue ?? int.MinValue,
					max: maxValue ?? int.MaxValue,
					interval: interval ?? 1
				);

			} else
				API.RegisterSimpleOption(
					mod: Consumer,
					optionName: name,
					optionDesc: tooltip,
					optionGet: () => get(GetConfig()),
					optionSet: val => set(GetConfig(), val)
				);

			return this;
		}

		#endregion

		#region Current Menu Stuff

		public GMCMIntegration<T, M> OpenMenu() {
			AssertLoaded();
			API.OpenModMenu(Consumer);
			return this;
		}

		public bool TryGetCurrentMenu(out IManifest mod, out string page) {
			AssertLoaded();
			return TryGetCurrentMenu(out mod, out page);
		}

		#endregion

	}
}
