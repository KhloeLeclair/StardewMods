using System.Collections.Generic;

using BmFont;

using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.ThemeManager.Models;

public readonly record struct BmFontData : IBmFontData {

	public FontFile File { get; init; }

	public Dictionary<char, FontChar> CharacterMap { get; init; }

	public List<Texture2D> FontPages { get; init; }

	public float PixelZoom { get; init; }

	public BmFontData(FontFile file, List<Texture2D> fontPages, float pixelZoom, Dictionary<char, FontChar>? characterMap = null) {
		File = file;
		FontPages = fontPages;
		PixelZoom = pixelZoom;

		if (characterMap is null) {
			CharacterMap = new Dictionary<char, FontChar>(File.Chars.Count);
			foreach (FontChar fchar in File.Chars) {
				char c = (char) fchar.ID;
				CharacterMap[c] = fchar;
			}
		} else
			CharacterMap = characterMap;
	}

}
