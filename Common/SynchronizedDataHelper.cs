namespace Leclair.Stardew.Common;

/*

public class SynchronizedDataHelper {

	#region Singleton Instantiation

	private static readonly Dictionary<Mod, SynchronizedDataHelper> Instances = new();

	public static SynchronizedDataHelper Get(Mod mod) {
		if (Instances.TryGetValue(mod, out var instance))
			return instance;

		instance = new(mod);
		Instances.Add(mod, instance);
		return instance;
	}

	#endregion

	public const string CommandPrefix = "Sync:";

	#region Declarations

	private readonly Mod Mod;

	private readonly Dictionary<string, object> LoadedData = new();
	private readonly Dictionary<string, long> DataLocks = new();
	private readonly Dictionary<string, Type> DataTypes = new();

	private readonly Dictionary<string, List<long>> SubscribedPeers = new();
	private readonly Dictionary<long, List<string>> PeerSubscriptions = new();

	private readonly Dictionary<string, List<Action<object>>> WaitingLoaders = new();
	private readonly Dictionary<string, List<Action<bool, object?>>> WaitingLocks = new();

	public bool IsReady { get; private set; }
	public bool IsSupported { get; private set; }

	private long _StoredHostId = -2;

	private long HostId {
		get {
			if (_StoredHostId != -2)
				return _StoredHostId;

			foreach (var peer in Mod.Helper.Multiplayer.GetConnectedPlayers()) {
				if (peer.IsHost) {
					_StoredHostId = peer.PlayerID;
					return _StoredHostId;
				}
			}

			_StoredHostId = -1;
			return -1;
		}
	}

	#endregion

	#region Life Cycle

	// Private constructor to enforce singleton pattern.
	private SynchronizedDataHelper(Mod mod) {

		Mod = mod;

		Mod.Helper.Events.GameLoop.Saving += GameLoop_Saving;
		Mod.Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
		Mod.Helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
		Mod.Helper.Events.Multiplayer.PeerContextReceived += Multiplayer_PeerContextReceived;
		Mod.Helper.Events.Multiplayer.PeerDisconnected += Multiplayer_PeerDisconnected;

	}

	#endregion

	private void Log(string message, LogLevel level = LogLevel.Debug, Exception? ex = null, LogLevel? exLevel = null) {
		Mod.Monitor.Log($"[SyncHelper] {message}", level: level);
		if (ex != null)
			Mod.Monitor.Log($"[SyncHelper] Details:\n{ex}", level: exLevel ?? level);
	}

	private void ResetState() {
		LoadedData.Clear();
		DataLocks.Clear();
		DataTypes.Clear();
		SubscribedPeers.Clear();
		PeerSubscriptions.Clear();
		WaitingLoaders.Clear();
		WaitingLocks.Clear();
		IsSupported = true;
		IsReady = false;
		_StoredHostId = -2;
	}

	#region Data Loading

	/// <summary>
	/// Attempt to load a data model, if it isn't already loaded, and call the
	/// provided callback when it's ready.
	/// </summary>
	/// <typeparam name="TModel">The type of model to load.</typeparam>
	/// <param name="key">The key to load it from.</param>
	/// <param name="onLoaded">An optional action to run when the model loads.</param>
	public void Load<TModel>(string key, Action<TModel>? onLoaded = null) where TModel : class, new() {
		if (DataTypes.TryGetValue(key, out Type? storedType)) {
			if (!storedType.IsAssignableTo(typeof(TModel)))
				throw new InvalidCastException($"Type for object '{key}' is '{storedType}' which cannot be assigned to {typeof(TModel)}.");
		} else {
			DataTypes.Add(key, typeof(TModel));
			storedType = typeof(TModel);
		}

		if (LoadedData.TryGetValue(key, out object? data)) {
			// Already loaded
			onLoaded?.Invoke((TModel) data);
			return;
		}

		if (!IsSupported)
			throw new NotSupportedException("Synced data is not supported in this context.");
		if (!IsReady)
			throw new NotSupportedException("Synced data is not yet ready.");
		if (storedType.AssemblyQualifiedName is null)
			throw new NotSupportedException("Unable to get fully qualified type name");

		if (Context.IsMainPlayer) {
			InternalLoad(typeof(TModel), key, onLoaded is null ? null : val => onLoaded((TModel) val));

		} else {
			SendMessage(
				"Request",
				new SimpleCommand(key, storedType.AssemblyQualifiedName, null)
			);
		}
	}

	private readonly Dictionary<Type, Func<IDataHelper, string, object>> TypedLoaders = new();

	private void InternalLoad(Type type, string key, Action<object>? onLoaded = null) {
		if (!Context.IsMainPlayer || LoadedData.ContainsKey(key))
			return;

		object? data;

		if (!TypedLoaders.TryGetValue(type, out var reader)) {
			var method = Mod.Helper.Data.GetType().GetMethod(nameof(IDataHelper.ReadSaveData), BindingFlags.Instance | BindingFlags.Public)
				?? throw new ArgumentNullException("Unable to get ReadSaveData method");
			reader = method.CreateGenericFunc<IDataHelper, string, object>(type);
			TypedLoaders[type] = reader;
		}

		try {
			data = reader(Mod.Helper.Data, key);
		} catch (Exception ex) {
			Log($"Unable to load save data model with key '{key}': {ex}", LogLevel.Error);
			data = null;
		}

		data ??= Activator.CreateInstance(type) ?? throw new ArgumentNullException($"Unable to create instance of model with key '{key}'");
		onLoaded?.Invoke(data);

		SendDataToPeers(key);
	}

	/// <summary>
	/// Check to see if a particular data asset has been loaded and
	/// synchronized.
	/// </summary>
	/// <param name="key">The data asset to check.</param>
	public bool HasLoaded(string key) {
		return LoadedData.ContainsKey(key);
	}

	/// <summary>
	/// Attempt to return a data model that has already been loaded. This will
	/// not cause a model to be loaded if it is not already loaded.
	/// </summary>
	/// <typeparam name="TModel">The type of model to get</typeparam>
	/// <param name="key">The key to get it from</param>
	/// <param name="value">The stored value, if one exists</param>
	/// <returns>Whether or not we were able to read the data model from cache</returns>
	/// <exception cref="InvalidCastException">If <typeparamref name="TModel"/>
	/// does not match the stored type.</exception>
	private bool TryGetValue<TModel>(string key, [NotNullWhen(true)] out TModel? value) where TModel : class {
		if (DataTypes.TryGetValue(key, out Type? storedType)) {
			if (!storedType.IsAssignableTo(typeof(TModel)))
				throw new InvalidCastException($"Type for object '{key}' is '{storedType}' which cannot be assigned to {typeof(TModel)}.");
		} else if (typeof(TModel) != typeof(object))
			DataTypes.Add(key, typeof(TModel));

		if (LoadedData.TryGetValue(key, out object? stored) && stored != null) {
			value = (TModel) stored;
			return true;
		}

		value = null;
		return false;
	}

	#endregion

	#region Data Editing



	#endregion

	#region Events

	private void Multiplayer_PeerContextReceived(object? sender, PeerContextReceivedEventArgs e) {
		// If we're the main player, or the peer isn't a host, we
		// don't care at all.
		if (Context.IsMainPlayer || !e.Peer.IsHost)
			return;

		// TODO: Minimum version check?
		IsSupported = e.Peer.GetMod(Mod.ModManifest.UniqueID) != null;

		// We're ready, since we are now connected to the host.
		IsReady = true;
	}

	private void Multiplayer_PeerDisconnected(object? sender, PeerDisconnectedEventArgs e) {
		if (!Context.IsMainPlayer) {
			if (e.Peer.IsHost)
				// Disconnected from the host? Better reset state.
				ResetState();

			return;
		}

		// Unlock any lock held by the disconnecting player.
		foreach (var pair in DataLocks) {
			if (pair.Value == e.Peer.PlayerID)
				DataLocks[pair.Key] = -1;
		}

		// Remove all their subscriptions.
		if (PeerSubscriptions.TryGetValue(e.Peer.PlayerID, out var subs)) {
			PeerSubscriptions.Remove(e.Peer.PlayerID);
			foreach (string sub in subs) {
				if (SubscribedPeers.TryGetValue(sub, out var subscriptions)) {
					subscriptions.Remove(e.Peer.PlayerID);
					if (subscriptions.Count == 0)
						SubscribedPeers.Remove(sub);
				}
			}
		}
	}

	private void Multiplayer_ModMessageReceived(object? sender, ModMessageReceivedEventArgs e) {
		// We only care about our own messages.
		if (e.FromModID != Mod.ModManifest.UniqueID || !e.Type.StartsWith(CommandPrefix))
			return;

		string command = e.Type[CommandPrefix.Length..];

		if (Context.IsMainPlayer) {
			// Client -> Host Messages
			switch (command) {
				case "Request":
					OnClientRequest(e);
					return;

				case "AcquireLock":
					break;

				case "ReleaseLock":
					break;

				case "UpdateData":
					OnClientUpdateData(e);
					return;
			}

		} else {
			// Host -> Client Messages
			// Verify we got this from the host.
			var peer = Mod.Helper.Multiplayer.GetConnectedPlayer(e.FromPlayerID);
			if (peer is null || !peer.IsHost)
				return;

			switch (command) {
				case "Changed":
					OnSyncChanged(e);
					return;

				case "LockChanged":
					OnLockChanged(e);
					return;
			}
		}
	}

	private void GameLoop_ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e) {
		// When returning to the main menu, clear absolutely all our state.
		ResetState();
	}

	private void GameLoop_Saving(object? sender, SavingEventArgs e) {
		if (!Context.IsMainPlayer)
			return;

		// The main player is responsible for saving all data.
		foreach (var pair in LoadedData)
			Mod.Helper.Data.WriteSaveData(pair.Key, pair.Value);
	}

	#endregion

	#region Client -> Host Message Handlers

	private void OnClientRequest(ModMessageReceivedEventArgs e) {
		var data = e.ReadAs<SimpleCommand>();
		long id = e.FromPlayerID;

		// Type validation!
		string? typeName = data.TypeName;
		Type? type = null;
		ProcessType(data.Key, ref typeName, ref type);

		if (!PeerSubscriptions.TryGetValue(id, out var subscriptions)) {
			subscriptions = [];
			PeerSubscriptions[id] = subscriptions;
		}

		// Ignore multiple requests.
		if (subscriptions.Contains(data.Key))
			return;

		// Mark this user as subscribed.
		subscriptions.Add(data.Key);

		// Update the other model.
		if (!SubscribedPeers.TryGetValue(data.Key, out var peers)) {
			peers = [];
			SubscribedPeers[data.Key] = peers;
		}

		peers.Add(id);

		// If we have the data, just return it now.
		if (LoadedData.ContainsKey(data.Key)) {
			SendDataToPeers(data.Key, [id]);
			return;
		}

		// Otherwise, load it.
		InternalLoad(type, data.Key);
	}

	private void OnClientUpdateData(ModMessageReceivedEventArgs e) {
		var data = e.ReadAs<DataUpdate>();
		long id = e.FromPlayerID;

		// Check to see if the client we got this from actually
		// held the lock. Ignore it if not.
		if (!DataLocks.TryGetValue(data.Key, out long lockValue))
			lockValue = -1;

		if (id != lockValue) {
			// Naughty!
			Log($"Received data update from client without mutex lock on model '{data.Key}'. Ignoring.", LogLevel.Trace);
			// Send them a fresh copy of the data to overwrite whatever
			// changes they tried to make.
			SendDataToPeers(data.Key, [id]);
			return;
		}

		// We got new data. Time to store it, then update any
		// clients that are subscribed to it. Except the one we
		// got it from.
		object parsed = ReadTypedData(data, GetSerializer(e));

		LoadedData[data.Key] = parsed;

		if (!SubscribedPeers.TryGetValue(data.Key, out var peers))
			return;

		SendDataToPeers(data.Key, peers.Where(peer => peer != id).ToArray());
	}

	private void SendDataToPeers(string key, long[]? peers = null) {
		if (!LoadedData.TryGetValue(key, out object? loadedData))
			return;

		// We'll always have this key, if LoadedData has it.
		Type storedType = DataTypes[key];

		if (peers is null) {
			if (!SubscribedPeers.TryGetValue(key, out var peerList))
				return;
			peers = peerList.ToArray();
		}

		SendMessage<GenericDataUpdate>(
			"Changed",
			new(
				key,
				storedType.FullName!,
				loadedData
			),
			peers
		);
	}

	#endregion

	#region Host -> Client Message Handlers

	private void OnSyncChanged(ModMessageReceivedEventArgs e) {
		var data = e.ReadAs<DataUpdate>();
		object parsed = ReadTypedData(data, GetSerializer(e));

		LoadedData[data.Key] = parsed;
		if (WaitingLoaders.TryGetValue(data.Key, out var loaders)) {
			foreach (var loader in loaders)
				loader(parsed);
			WaitingLoaders.Remove(data.Key);
		}
	}

	private void OnLockChanged(ModMessageReceivedEventArgs e) {
		var data = e.ReadAs<SimpleCommand>();
		if (!WaitingLocks.TryGetValue(data.Key, out var waiters))
			return;

		WaitingLocks.Remove(data.Key);

		bool locked = data.LockState == Game1.player.UniqueMultiplayerID;
		object? thing = LoadedData.GetValueOrDefault(data.Key);

		if (thing is not null)
			try {
				foreach (var func in waiters)
					func(locked, thing);
			} catch (Exception ex) {
				Log($"Exception in locked delegate for synced data: {ex}", LogLevel.Error);
			}

		// Always release our lock when we're done. And send back our data.
		if (locked) {
			if (thing is not null)
				SendMessage<GenericDataUpdate>("UpdateData", new(data.Key, data.TypeName!, thing));
			SendMessage<SimpleCommand>("ReleaseLock", new(data.Key, null, null));
		}
	}

	#endregion

	private void ProcessType(string key, [NotNull] ref string? typeName, [NotNull] ref Type? type) {
		DataTypes.TryGetValue(key, out var dataType);

		if (type != null && dataType != null && dataType != type)
			throw new ArgumentException($"Received incorrect type for '{key}'. Got {type} but expected {dataType}.");

		if (typeName != null && dataType != null && dataType.FullName != typeName)
			throw new ArgumentException($"Received incorrect type name for '{key}'. Got {typeName} but expected {dataType}.");

		if (dataType == null) {
			if (type != null)
				dataType = type;
			else if (typeName != null)
				dataType = Type.GetType(typeName);
			if (dataType == null)
				throw new ArgumentException($"Unable to resolve type for '{key}'. Got {typeName}");

			DataTypes[key] = dataType;
		}

		type = dataType;
		typeName = dataType.FullName;
	}

	#region Packet Stuff

	private Func<ModMessageReceivedEventArgs, JsonSerializer>? _GetSerializerFunc;

	private JsonSerializer? GetSerializer(ModMessageReceivedEventArgs e) {
		if (_GetSerializerFunc == null) {
			var field = e.GetType().GetField("JsonHelper", BindingFlags.Instance | BindingFlags.NonPublic);
			_GetSerializerFunc = field?.CreateGetter<ModMessageReceivedEventArgs, JsonSerializer>();
		}

		return _GetSerializerFunc?.Invoke(e);
	}

	private object ReadTypedData(DataUpdate data, JsonSerializer? serializer) {
		string? typeName = data.TypeName;
		Type? type = null;
		ProcessType(data.Key, ref typeName, ref type);

		object? result;

		if (serializer != null)
			result = data.Token.ToObject(type, serializer);
		else
			result = data.Token.ToObject(type);

		result ??= Activator.CreateInstance(type) ?? throw new ArgumentNullException($"Unable to create instance of data model '{type}'");
		return result;
	}

	private void SendMessage<TModel>(string command, TModel instance, long[]? peers = null) {
		Mod.Helper.Multiplayer.SendMessage(instance, CommandPrefix + command, [Mod.ModManifest.UniqueID], peers ?? [HostId]);
	}

	private record SimpleCommand(string Key, string? TypeName, long? LockState);

	private record DataUpdate(string Key, string TypeName, JToken Token);

	private record GenericDataUpdate(string Key, string TypeName, object Token);

	#endregion

}
*/
