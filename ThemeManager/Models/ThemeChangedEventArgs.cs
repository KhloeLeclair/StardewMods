using System;

namespace Leclair.Stardew.ThemeManager.Models;

public class ThemeChangedEventArgs<DataT> : EventArgs, IThemeChangedEvent<DataT> {
	public string OldId { get; }

	public string NewId { get; }

	public DataT? OldData { get; }

	public DataT NewData { get; }

	public ThemeChangedEventArgs(string oldId, DataT? oldData, string newID, DataT newData) {
		OldId = oldId;
		NewId = newID;
		OldData = oldData;
		NewData = newData;
	}
}
