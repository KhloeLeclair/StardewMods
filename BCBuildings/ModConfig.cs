namespace Leclair.Stardew.BCBuildings;

public class ModConfig {

	public string? CostAdditional { get; set; } = null;

	public int CostMaterial { get; set; } = 100;

	public int CostCurrency { get; set; } = 100;

	public int RefundMaterial { get; set; } = 0;

	public int RefundCurrency { get; set; } = 0;

	public bool AllowMovingUnfinishedGreenhouse { get; set; } = false;

	public bool AllowHouseUpgrades { get; set; } = false;

	public bool AllowHouseRenovation { get; set; } = false;

}
