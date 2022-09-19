using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.DynamicTypes;

public interface IDynamicTypeHandler {

	#region Display

	string DisplayName { get; }

	string Description { get; }

	Texture2D Texture { get; }

	Rectangle Source { get; }

	IFlowNode[]? GetExtraInfo(object? state);

	bool AllowMultiple { get; }

	#endregion

	#region Editing

	bool HasEditor { get; }
	IClickableMenu? GetEditor(IDynamicType type);

	#endregion

	#region Processing

	object? ParseState(IDynamicType type);

	bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state);

	#endregion
}

public abstract class DynamicTypeHandler<T> : IDynamicTypeHandler {

	#region Display

	public abstract string DisplayName { get; }

	public abstract string Description { get; }

	public abstract Texture2D Texture { get; }

	public abstract Rectangle Source { get; }

	public abstract bool AllowMultiple { get; }

	public abstract bool HasEditor { get; }

	public abstract IFlowNode[]? GetExtraInfo(T? state);

	public abstract IClickableMenu? GetEditor(IDynamicType type);

	#endregion

	#region Processing

	public abstract T? ParseStateT(IDynamicType type);

	public object? ParseState(IDynamicType type) {
		return ParseStateT(type);
	}

	public IFlowNode[]? GetExtraInfo(object? state) {
		T? tstate = state is T ts ? ts : default;
		return GetExtraInfo(tstate);
	}

	public abstract bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, T? state);

	public bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state) {
		T? tstate = state is T ts ? ts : default;
		return DoesRecipeMatch(recipe, item, tstate);
	}

	#endregion

}
