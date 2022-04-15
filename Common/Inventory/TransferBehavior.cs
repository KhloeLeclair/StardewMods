#nullable enable

using Leclair.Stardew.Common.Enums;

namespace Leclair.Stardew.Common.Inventory;

public struct TransferBehavior {

	public TransferMode Mode;
	public int Quantity;

	public TransferBehavior(TransferMode mode, int quantity) {
		Mode = mode;
		Quantity = quantity;
	}
}
