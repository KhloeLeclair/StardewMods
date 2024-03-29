using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.ThemeManagerFontStudio.Models;

public class RawSpriteFontData {

	public string? TextureName { get; set; }

	public int LineSpacing { get; set; }

	public float Spacing { get; set; }

	public char? DefaultCharacter { get; set; }

	public RawGlyphData[]? Glyphs { get; set; }

	public static RawSpriteFontData FromSpriteFont(SpriteFont font) {
		RawGlyphData[] glyphs = font.Glyphs.Select(glyph => {
			return new RawGlyphData {
				Character = glyph.Character,
				BoundsInTexture = glyph.BoundsInTexture,
				Cropping = glyph.Cropping,
				Kerning = new Vector3(glyph.LeftSideBearing, glyph.Width, glyph.RightSideBearing)
			};
		}).ToArray();

		return new RawSpriteFontData {
			LineSpacing = font.LineSpacing,
			Spacing = font.Spacing,
			DefaultCharacter = font.DefaultCharacter,
			Glyphs = glyphs
		};
	}

}

public class RawGlyphData {

	public char? Character { get; set; }

	public Rectangle? BoundsInTexture { get; set; }

	public Rectangle? Cropping { get; set; }

	public Vector3? Kerning { get; set; }

}
