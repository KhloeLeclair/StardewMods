using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.BreakPintail;

public interface IThingTwo<TValue> {
	TValue? Value { get; }
}

public interface IThingOne<TValue> : IReadOnlyDictionary<string, TValue> {

	IReadOnlyDictionary<string, TValue> CalculatedValues { get; }

}

public interface IBreakPintailApi {

	IThingOne<IThingTwo<Texture2D>> GetThingGetter();

}
