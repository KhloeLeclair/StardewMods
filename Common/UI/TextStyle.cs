using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.Common.UI {
	public struct TextStyle {

		public readonly static TextStyle EMPTY = new();

		public bool? Fancy { get; }
		public bool? Title { get; }
		public bool? Shadow { get; }
		public Color? ShadowColor { get; }
		public bool? Bold { get; }
		public Color? Color { get; }
		public bool? Prismatic { get; }
		public SpriteFont Font { get; }
		public bool? Strikethrough { get; }
		public bool? Underline { get; }
		public float? Scale { get; }

		public TextStyle(Color? color = null, bool? prismatic = null, SpriteFont font = null, bool? fancy = null, bool? title = null, bool? shadow = null, Color? shadowColor = null, bool? bold = null, bool? strikethrough = null, bool? underline = null, float? scale = null) {
			Title = title;
			Fancy = fancy;
			Bold = bold;
			Shadow = shadow;
			ShadowColor = shadowColor;
			Color = color;
			Prismatic = prismatic;
			Font = font;
			Scale = scale;
			Strikethrough = strikethrough;
			Underline = underline;
		}

		public bool HasShadow() {
			return Shadow ?? true;
		}

		public bool IsFancy() {
			return Fancy ?? false;
		}

		public bool IsBold() {
			return Bold ?? false;
		}

		public bool IsPrismatic() {
			return Prismatic ?? false;
		}

		public bool IsStrikethrough() {
			return Strikethrough ?? false;
		}

		public bool IsUnderline() {
			return Underline ?? false;
		}

		public bool IsEmpty() {
			return Equals(EMPTY);
		}

		public override bool Equals(object obj) {
			if (!(obj is TextStyle))
				return false;

			TextStyle other = (TextStyle) obj;
			return
				Title.Equals(other.Title) &&
				Fancy.Equals(other.Fancy) &&
				Shadow.Equals(other.Shadow) &&
				ShadowColor.Equals(other.ShadowColor) &&
				Bold.Equals(other.Bold) &&
				Color.Equals(other.Color) &&
				Prismatic.Equals(other.Prismatic) &&
				Font.Equals(other.Font) &&
				Scale.Equals(other.Scale) &&
				Strikethrough.Equals(other.Strikethrough) &&
				Underline.Equals(other.Underline);
		}

		public override int GetHashCode() {
			int hashCode = -412244955;
			hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Fancy);
			hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Title);
			hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Shadow);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(ShadowColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Bold);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(Color);
			hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Prismatic);
			hashCode = hashCode * -1521134295 + EqualityComparer<SpriteFont>.Default.GetHashCode(Font);
			hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Strikethrough);
			hashCode = hashCode * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Underline);
			hashCode = hashCode * -1521134295 + EqualityComparer<float?>.Default.GetHashCode(Scale);
			return hashCode;
		}
	}
}
