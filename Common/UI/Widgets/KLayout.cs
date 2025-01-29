#if COMMON_WIDGETS

using System;
using System.Collections.Generic;
using System.Text;

namespace Leclair.Stardew.Common.UI.Widgets;

public abstract class KLayout : KObject, IKLayoutItem {

	#region Life Cycle

	public KLayout(KWidget? parent = null) : base(parent) {

	}

	#endregion

	#region Identity

	protected internal override void OnParentChanged(KObject? oldParent, KObject? newParent) {
		base.OnParentChanged(oldParent, newParent);

		
		


	}

	#endregion

}

#endif
