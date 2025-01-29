#if COMMON_WIDGETS

using System;
using System.Collections.Generic;
using System.Text;

namespace Leclair.Stardew.Common.UI.Widgets;

public interface IKLayoutItem {

	/// <summary>
	/// The alignment of this item.
	/// </summary>
	//Alignment Alignment { get; }

	/// <summary>
	/// The <see cref="KLayout"/> this item belongs to. If this item is not
	/// currently within a layout, this value will be <c>null</c>
	/// </summary>
	//KLayout? Layout { get; }

}

#endif
