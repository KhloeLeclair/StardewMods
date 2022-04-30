using System;

namespace Leclair.Stardew.BreakPintail;

public interface IEventData<DataT> {

	public DataT Value { get; }

}

public interface IThing<DataT> {

	public event EventHandler<IEventData<DataT>> Changed;

}

public interface IBreakPintailApi {

	IThing<DataT> GetThing<DataT>();

}
