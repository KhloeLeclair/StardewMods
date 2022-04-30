using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BreakPintail;

namespace Leclair.Stardew.BreakPintailHost;

internal class Thing<DataT> : IThing<DataT> {

	public event EventHandler<IEventData<DataT>>? Changed;

}

public class ModAPI : IBreakPintailApi {

	public ModAPI() { }

	public IThing<DataT> GetThing<DataT>() {
		return new Thing<DataT>();
	}
}
