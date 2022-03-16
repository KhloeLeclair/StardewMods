using Microsoft.Xna.Framework.Graphics;

namespace MoreGiantCrops;

public interface IMoreGiantCropsApi {

	public Texture2D? GetTexture(int productIndex);

	public int[] RegisteredCrops();

}
