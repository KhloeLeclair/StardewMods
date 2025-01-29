#if COMMON_BCINVENTORY

using Leclair.Stardew.Common.Enums;

namespace Leclair.Stardew.Common.Inventory;

public class TransferBehavior {

	public TransferMode Mode;
	public int Quantity;

	public TransferBehavior(TransferMode mode, int quantity) {
		Mode = mode;
		Quantity = quantity;
	}
}

#endif
