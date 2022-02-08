
/* Unmerged change from project 'Hydrology'
Before:
using System;
using Microsoft.Xna.Framework;
After:
using System;

using Microsoft.Xna.Framework;
*/

/* Unmerged change from project 'Almanac'
Before:
using System;
using Microsoft.Xna.Framework;
After:
using System;

using Microsoft.Xna.Framework;
*/

/* Unmerged change from project 'SeeMeRollin'
Before:
using System;
using Microsoft.Xna.Framework;
After:
using System;

using Microsoft.Xna.Framework;
*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.Common.UI.SimpleLayout {
	public interface ISimpleNode {

		// Alignment
		Alignment Alignment { get; }

		// Size
		bool DeferSize { get; }
		Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize);

		// Rendering
		void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont);

	}
}
