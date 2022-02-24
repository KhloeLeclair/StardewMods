using System;

namespace StackSplitRedux {
	public interface IStackSplitAPI {
		bool TryRegisterMenu(Type menuType);
	}
}
