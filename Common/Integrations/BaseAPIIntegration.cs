#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

using StardewModdingAPI;

namespace Leclair.Stardew.Common.Integrations;

public abstract class BaseAPIIntegration<T, M> : BaseIntegration<M> where M : Mod where T : class {

	protected T? API { get; }

	protected BaseAPIIntegration(M self, string modID, string? minVersion, string? maxVersion = null) : base(self, modID, minVersion, maxVersion) {
		if (!IsLoaded) {
			API = null;
			return;
		}

		API = GetAPI();

		if (API == null) {
			Log("Unable to obtain API instance. Disabling integration.", LogLevel.Warn);

			IsLoaded = false;
			return;
		}
	}

	[MemberNotNullWhen(true, nameof(API))]
	public override bool IsLoaded { get; protected set; }

	[MemberNotNull(nameof(API))]
	protected override void AssertLoaded() {
		base.AssertLoaded();
		if (API is null)
			throw new InvalidOperationException($"{ModID} integration is disabled.");
	}

	protected virtual T? GetAPI() {
		return GetAPI<T>(logError: true);
	}

	protected TArg? GetAPI<TArg>(bool logError = true) where TArg : class, T {
		try {
			return Self.Helper.ModRegistry.GetApi<TArg>(ModID);
		} catch(Exception ex) {
			if (logError)
				Log($"An error occurred calling GetApi<{typeof(TArg).Name}>(). Details: {ex}", LogLevel.Debug);
			return null;
		}
	}
}
